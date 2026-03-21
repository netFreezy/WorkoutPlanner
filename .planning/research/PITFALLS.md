# Pitfalls Research

**Domain:** Personal workout planner (strength + endurance) on Blazor Server / EF Core / SQLite
**Researched:** 2026-03-21
**Confidence:** HIGH (most findings verified against Microsoft official docs and EF Core GitHub issues)

## Critical Pitfalls

### Pitfall 1: Blazor Server Circuit Death During Mid-Workout Logging

**What goes wrong:**
The user is mid-workout, logging sets between rest periods on their phone. They switch to Spotify to change a song, check a text message, or their phone locks the screen. Mobile browsers aggressively suspend background tabs, killing the SignalR WebSocket. When they return to the Blazor tab, the circuit is dead, the component state is gone, and their partially-logged session data (sets completed, weights entered, timer progress) vanishes. The reconnection modal appears, and if reconnection fails, a page reload wipes all in-memory state.

**Why it happens:**
Blazor Server holds all component state in server memory tied to a specific circuit. Mobile browsers (iOS Safari especially) kill background WebSocket connections within seconds of a tab switch. The default circuit timeout (3 minutes) and reconnection logic cannot recover component-level state -- only the circuit connection itself. There is no built-in persistence of form/component data across circuit death.

**How to avoid:**
1. Persist in-progress session state to the database on every meaningful user action (each set logged, each exercise marked). Do not accumulate state only in component memory.
2. Design the session tracker as a "resume-friendly" flow: on component initialization, check for an open (incomplete) session in the database and restore it.
3. Implement a custom reconnection handler in `App.razor` that attempts immediate reconnection and falls back to `location.reload()`, which then hits the resume logic.
4. Configure extended timeouts for the workout logging pages:
   ```csharp
   options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
   options.KeepAliveInterval = TimeSpan.FromSeconds(15);
   ```
5. Save critical state (current exercise index, timer value) to browser `localStorage` via JS interop as a secondary safety net.

**Warning signs:**
- Testing only on desktop where tab switching does not kill connections.
- Session tracker component holds a `List<LoggedSet>` in memory without database writes until a "Save Session" button is pressed.
- No `OnInitializedAsync` logic that checks for an in-progress session.

**Phase to address:**
Session Tracker phase. This must be a first-class architectural concern when building the logging flow, not bolted on after.

---

### Pitfall 2: DbContext Threading Violations in Blazor Server Components

**What goes wrong:**
EF Core's `DbContext` is not thread-safe. In Blazor Server, a scoped `DbContext` is shared across all components within a user's circuit. If two components (or two event handlers on the same component) trigger database operations concurrently -- for example, a timer callback querying the database while the user simultaneously clicks a button -- EF Core throws `InvalidOperationException: A second operation was started on this context instance before a previous operation was completed`. In the worst case with SQLite, you get `SQLiteException: database is locked`.

**Why it happens:**
Developers register `DbContext` with `AddDbContext` (scoped lifetime) out of habit from MVC/API projects. In those contexts, a scope = one HTTP request = one thread = safe. In Blazor Server, a scope = one circuit = potentially long-lived with multiple concurrent UI interactions = unsafe. Microsoft's own documentation explicitly warns against this pattern for Blazor Server.

**How to avoid:**
1. Use `AddDbContextFactory<AppDbContext>` instead of `AddDbContext<AppDbContext>` in `Program.cs`.
2. Inject `IDbContextFactory<AppDbContext>` into components.
3. For short operations, create and dispose per-method:
   ```csharp
   private async Task LoadExercises()
   {
       using var context = DbFactory.CreateDbContext();
       Exercises = await context.Exercises.ToListAsync();
   }
   ```
4. For component-lifetime contexts (change tracking needed), implement `IDisposable`:
   ```csharp
   @implements IDisposable
   private AppDbContext? Context { get; set; }
   protected override void OnInitialized() => Context = DbFactory.CreateDbContext();
   public void Dispose() => Context?.Dispose();
   ```
5. Use a `Loading` flag to prevent concurrent operations from the same component:
   ```csharp
   if (Loading) return;
   try { Loading = true; /* db work */ }
   finally { Loading = false; }
   ```

**Warning signs:**
- `AddDbContext` in `Program.cs` instead of `AddDbContextFactory`.
- `@inject AppDbContext Db` in any `.razor` file.
- No `IDisposable` implementation on components that hold a `DbContext`.
- Intermittent "second operation started" exceptions during testing.

**Phase to address:**
Data layer / foundation phase. This must be the pattern from day one. Retrofitting `IDbContextFactory` after building 20 components with injected `DbContext` is painful.

---

### Pitfall 3: Rigid Exercise Type Model That Cannot Evolve

**What goes wrong:**
The data model hardcodes exactly two exercise types (strength and endurance) with fixed property sets. Strength gets `Sets/Reps/Weight`. Endurance gets `Distance/Duration/Pace`. Then real-world training surfaces exercises that don't fit: timed holds (planks -- duration but no distance), bodyweight exercises (pull-ups -- reps but no weight, or weighted pull-ups where weight is optional), EMOM protocols (time-based but with reps inside), flexibility work (neither sets/reps nor distance/duration), rowing (has distance, duration, pace AND stroke rate), or swimming (laps, not distance in km). Each new type forces schema migrations and component rewrites.

**Why it happens:**
Developers design the data model around a clean strength/endurance dichotomy because that is how the requirements read. The real world of training is messier. The PROJECT.md itself mentions "weighted pull-ups + dips EMOM" -- an exercise that blends strength properties (weight, reps) with a time-based protocol (EMOM intervals). A strict TPH discriminator with non-nullable type-specific columns breaks down.

**How to avoid:**
1. Use a composable metrics approach: define a set of possible metric types (`Reps`, `Weight`, `Duration`, `Distance`, `Pace`, `HeartRateZone`, `RPE`) and let each exercise declare which metrics apply to it.
2. Keep the `ExerciseType` discriminator (`Strength`, `Endurance`, `Hybrid`) for UI presentation and grouping, but do not use it to determine which database columns exist.
3. Make logging columns nullable -- a strength exercise logs `Sets`, `Reps`, `Weight?` (weight is nullable for bodyweight). An endurance exercise logs `Duration`, `Distance?`, `Pace?`.
4. Use EF Core TPH (Table-Per-Hierarchy) for the exercise entity with a discriminator column. TPH is the right choice here: it avoids JOINs for polymorphic queries (listing all exercises in a workout regardless of type), and the null columns cost is negligible for a personal app with hundreds, not millions, of rows.
5. Define a `MetricDefinition` on the exercise that specifies which fields the UI should render when logging that exercise.

**Warning signs:**
- `StrengthExercise` and `EnduranceExercise` as separate C# classes with no shared logging interface.
- Non-nullable `Weight` column on the exercise log (bodyweight exercises cannot be logged).
- Adding a new exercise type requires a database migration rather than just a data entry.
- Hard-coded `if (type == Strength)` / `else` branches scattered through UI components.

**Phase to address:**
Data model design phase. This is the single most impactful data model decision. Getting it wrong means migration pain throughout every subsequent phase.

---

### Pitfall 4: Planned-vs-Actual Coupling That Prevents Template Evolution

**What goes wrong:**
The workout template and the logged session share the same database rows or are tightly coupled via foreign keys. When the user edits a template (adds an exercise, changes set/rep targets), the change either retroactively alters historical logs (making past data meaningless) or is blocked because historical sessions reference the old template structure.

**Why it happens:**
It feels natural to have `WorkoutSession.TemplateId -> WorkoutTemplate.Id` and query the template at display time to show what was planned. But templates evolve: the user adds a new exercise, removes one, changes target reps. If the session only stores "I did template X," you cannot reconstruct what was planned that day versus what the template looks like now.

**How to avoid:**
1. **Snapshot the plan at session creation.** When the user starts a workout, copy the template's exercises and targets into `PlannedExercise` rows linked to the `WorkoutSession`, not to the template. The session now has an immutable record of what was planned that day.
2. Keep `WorkoutSession.OriginalTemplateId` as a soft reference for grouping/filtering ("show all sessions from my Push Day template"), but never use the live template to reconstruct what was planned.
3. `ActualExerciseLog` rows reference `PlannedExercise` rows within the same session, enabling clean planned-vs-actual comparison.
4. This mirrors how calendar apps handle recurring events: each occurrence is a snapshot, not a live pointer to the rule.

**Warning signs:**
- Historical session views change when the user edits a template.
- "What was my target weight on March 5?" requires querying the current template.
- Deleting a template orphans or cascades deletion of historical sessions.
- Analytics show sudden jumps because a template edit retroactively changed planned volume for past sessions.

**Phase to address:**
Data model design phase. The template-to-session relationship is foundational. Build it wrong and adherence tracking (a core feature) is unreliable.

---

### Pitfall 5: Calendar Recurrence Implementation Bloat

**What goes wrong:**
Building a full iCalendar RRULE engine for workout scheduling. The developer implements `FREQ=WEEKLY;BYDAY=MO,WE,FR`, exception dates, timezone-aware recurrence expansion, and DST handling. The implementation is complex, buggy around DST transitions (events shift by an hour), and the user only needed "repeat every Monday" and "repeat every other day."

**Why it happens:**
Recurrence feels simple until you start implementing it. Developers reach for the iCal spec or a full RRULE library and inherit massive complexity. RRULE implementations are notorious for DST bugs: a workout scheduled at 6:00 AM shifts to 5:00 AM after a daylight saving transition. Querying "all workouts between date X and date Y" with recurrence rules requires expanding occurrences for every recurring workout in that window, which is a non-trivial algorithm.

**How to avoid:**
1. **Start with materialized instances, not recurrence rules.** When the user says "repeat every Monday for 8 weeks," generate 8 concrete `ScheduledWorkout` rows with specific dates. Store the recurrence intent as metadata for the UI, but always have materialized rows for querying.
2. Support only a small set of recurrence patterns the user actually needs:
   - Specific days of the week (every Mon/Wed/Fri)
   - Every N days (every other day)
   - Weekly on the same day
3. When the user modifies a recurring series, offer "this instance only" or "this and all future" -- standard calendar semantics, implemented by modifying materialized rows, not by patching an RRULE.
4. Skip timezone complexity entirely. This is a personal, single-timezone app. Store dates as `DateOnly`, not `DateTimeOffset`. Workouts happen on days, not at precise UTC timestamps.
5. If a recurrence library is later needed, use `Ical.Net` (the mature .NET library), but defer this until the simple approach proves insufficient.

**Warning signs:**
- Importing an RRULE library in the first phase.
- Storing recurrence rules as strings and parsing them at query time.
- The calendar query "what's scheduled this week?" requires expanding rules for all recurring workouts rather than a simple date-range query.
- Tests failing around March/November (DST transitions).

**Phase to address:**
Calendar/Scheduler phase. The temptation to over-engineer recurrence is highest here. Start with materialized instances and add complexity only if the user's actual patterns demand it.

---

### Pitfall 6: SQLite Lock Contention From Long-Lived DbContext

**What goes wrong:**
During a workout logging session, the user has a component with a long-lived `DbContext` tracking their in-progress session. Meanwhile, a background timer or another component queries the database for analytics or the exercise library. SQLite allows only one writer at a time. The write from the logging component and the read from the analytics component collide, producing `SQLiteException: database is locked` or slow responses as one operation waits for the other.

**Why it happens:**
SQLite uses file-level locking. Even with WAL (Write-Ahead Logging) mode -- which EF Core enables by default -- writers still block other writers. If a `DbContext` holds an open transaction (even implicitly via change tracking), it can hold a write lock longer than expected. Combined with the Blazor Server pattern of long-lived components, this creates a window for lock contention even in a single-user app, because multiple components within the same circuit can issue database calls concurrently.

**How to avoid:**
1. Keep database operations short. Use per-method `DbContext` instances (`using var context = ...`) for reads. Only use component-lifetime contexts when change tracking is genuinely needed.
2. Ensure WAL mode is enabled (EF Core does this by default, but verify):
   ```csharp
   optionsBuilder.UseSqlite("Data Source=workouts.db;Cache=Shared");
   // WAL is enabled by default in EF Core-created databases
   ```
3. Set a busy timeout so SQLite waits briefly instead of immediately throwing:
   ```csharp
   "Data Source=workouts.db;Cache=Shared"
   // Then execute: PRAGMA busy_timeout = 5000;
   ```
4. Never call `SaveChanges` inside a loop. Batch changes and save once.
5. Avoid fire-and-forget async database calls that might overlap.

**Warning signs:**
- Intermittent "database is locked" errors, especially during active logging.
- Slow UI responses when multiple components are visible (e.g., session tracker + exercise search).
- `SaveChanges` called inside `foreach` loops.

**Phase to address:**
Data layer / foundation phase. Configure SQLite connection string and establish the short-lived `DbContext` pattern before any feature development.

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Storing exercise metadata as JSON blobs in SQLite | Flexible schema, no migrations for new fields | Cannot query/filter by JSON properties in SQLite efficiently; EF Core LINQ-to-SQL does not translate JSON path queries for SQLite | Never for queryable data (muscle group, equipment). Acceptable for truly unstructured notes or user preferences |
| Using `string` discriminator with magic values instead of enums | Quick to add new types | Typos cause silent bugs, no compiler safety, refactoring is grep-and-replace | Never. Use a C# enum mapped to the discriminator column |
| Skipping the planned-workout snapshot (referencing live template) | Faster to build, fewer tables | Retroactive data corruption when templates change, adherence tracking is unreliable | Never. The snapshot is the core value proposition of planned-vs-actual |
| Hard-coding set/rep/weight UI for all exercises | Faster initial development | Endurance exercises get a nonsensical "Weight" input field; bodyweight exercises require entering "0 kg" | Never. The metric composition approach costs minimal extra effort upfront |
| Accumulating session state in component memory, saving only on "Finish Workout" | Simpler code, fewer DB writes | Total data loss on circuit death. User loses 30 minutes of logging | Never for a Blazor Server app used on mobile. Save incrementally |

## Performance Traps

Patterns that work at small scale but degrade as data grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Loading full exercise history for PR detection on every session view | Slow page load, visible spinner on the session tracker | Query only the max for the specific exercise and metric, not the full history. Create an index on `(ExerciseId, MetricType, Value)` | After ~500 logged sessions (a few months of consistent training) |
| Querying all `ScheduledWorkout` rows to render the calendar month view | Calendar page loads slowly, especially if recurrence generated many rows | Date-range query with index on `ScheduledDate`. Paginate by visible date range only | After ~1 year of scheduling (365+ rows per recurring workout) |
| Eager-loading entire workout template hierarchy (template -> exercises -> sets -> metrics) | Over-fetching on list views; slow template browser | Use projection (`Select`) for list views; eager-load only on the detail/edit view | After ~50 templates with 10+ exercises each |
| No index on `WorkoutSession.Date` or `ExerciseLog.ExerciseId` | Full table scans on analytics queries | Add indexes during data model phase: `Date`, `ExerciseId`, `SessionId`, `ScheduledDate` | After ~200 sessions |
| Recomputing analytics (weekly volume, streaks) from raw logs on every dashboard load | Dashboard becomes the slowest page in the app | Precompute weekly/monthly aggregates into a summary table, update on session completion | After ~6 months of data |

## UX Pitfalls

Common user experience mistakes in the workout planner domain.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| Requiring too many taps to log a single set | Each set takes 30-45 seconds instead of 5-10. Over a workout, this adds 10+ minutes of phone fumbling. User stops logging | Pre-fill with previous session values. Single tap to confirm "same as last time." Number inputs with increment/decrement buttons, not keyboard entry |
| Showing a generic exercise form regardless of type | Endurance exercise shows "Weight" and "Reps" fields. Strength exercise shows "Distance" and "Pace." Confusing and cluttered | Render only the metrics that apply to the specific exercise. Use the exercise's metric definition to drive UI |
| No inline display of previous performance | User cannot remember what they lifted last time and must navigate away from the logging screen to check | Show "Last time: 3x8 @ 20kg" directly below each exercise during logging. This is table-stakes for any workout tracker |
| Calendar shows only workout names, not content preview | User must open each day to see what exercises are planned. Cannot quickly assess their week | Show an abbreviated preview (e.g., "Push: Bench, OHP, Dips") on hover or in a compact day-cell format |
| Forcing the user to create a template before logging | User wants to do an ad-hoc workout (a spontaneous run or gym session). The app requires template creation first | Support "empty session" creation where the user picks exercises on the fly. Template creation should be optional, not mandatory |
| Complex onboarding before first workout | User must set up profile, goals, body measurements, and a full program before they can log anything | Let the user log a workout within 60 seconds of first launch. Profile and program setup can come later |
| Timer/stopwatch resets on page navigation | User starts a rest timer, navigates to check an exercise video, returns, and the timer is gone | Persist timer state outside the component (service-level or `localStorage`). Timer should survive navigation within the app |
| EMOM/superset grouping is visually indistinguishable from sequential exercises | User cannot tell which exercises are grouped together for a superset | Use visual grouping (cards, borders, color coding) and explicit labels ("Superset A", "EMOM Block") |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Exercise library:** Often missing search/filter -- verify exercises can be filtered by muscle group, equipment, and type simultaneously
- [ ] **Workout template:** Often missing reorder capability -- verify exercises can be drag-reordered or moved up/down
- [ ] **Session logging:** Often missing partial completion -- verify individual exercises can be marked "skipped" without failing the entire session
- [ ] **Session logging:** Often missing mid-session resume -- verify closing the browser and reopening recovers the in-progress session
- [ ] **Calendar view:** Often missing "this instance only" edit -- verify editing one occurrence of a recurring workout does not modify all occurrences
- [ ] **Analytics/PRs:** Often missing context -- verify PRs show the date achieved and the exercise parameters, not just the number
- [ ] **Progressive overload:** Often missing the "opt out" -- verify the user can dismiss a suggestion without it reappearing every session
- [ ] **Warm-up blocks:** Often missing volume exclusion -- verify warm-up sets are excluded from weekly volume and PR calculations
- [ ] **Data model:** Often missing soft delete -- verify deleting a template does not cascade-delete historical sessions that used it
- [ ] **Export:** Often missing date range filtering -- verify export allows selecting a date range, not just "export everything"

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Circuit death loses session data | MEDIUM | Add `localStorage` backup of current session state. Write migration to add `IsComplete` flag to sessions so incomplete ones can be resumed. Requires JS interop work |
| DbContext threading violations | LOW | Replace `AddDbContext` with `AddDbContextFactory` in `Program.cs`. Update component injections from `@inject AppDbContext` to `@inject IDbContextFactory<AppDbContext>`. Mechanical refactor, pattern is well-documented |
| Rigid exercise type model | HIGH | Requires schema migration to make type-specific columns nullable. Must update all components that assume non-null weight/reps. Must backfill existing data. Most painful if many sessions are already logged |
| Template-session coupling (no snapshot) | HIGH | Must add `PlannedExercise` table, write migration to snapshot existing template state for historical sessions (best-effort, may lose accuracy for sessions where template was modified between then and now). Adherence data for pre-migration sessions is unreliable |
| Over-engineered recurrence | MEDIUM | Replace RRULE storage with materialized instances. Write a one-time migration script to expand existing rules into concrete rows. Simpler code going forward, but the migration itself requires careful testing |
| SQLite lock contention | LOW | Switch from component-lifetime `DbContext` to per-method instances. Add busy timeout to connection string. Low risk, mostly mechanical changes |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Circuit death data loss | Session Tracker | Test: switch phone tabs during active logging, return after 2 minutes, verify session data is intact |
| DbContext threading | Data Layer / Foundation | Verify: `AddDbContextFactory` in `Program.cs`, no `@inject AppDbContext` in any razor file, grep confirms zero `AddDbContext` calls |
| Rigid exercise types | Data Model Design | Verify: can create a "Plank" exercise (duration only, no reps/weight) and a "Weighted Pull-up" (reps + optional weight) without schema changes |
| Template-session coupling | Data Model Design | Verify: edit a template, then view a historical session that used the old template -- planned values should reflect the original template, not the edited one |
| Recurrence over-engineering | Calendar / Scheduler | Verify: "repeat every Monday" creates concrete `ScheduledWorkout` rows. Calendar week view is a simple date-range query, not a recurrence expansion |
| SQLite lock contention | Data Layer / Foundation | Verify: open session tracker and exercise library simultaneously, perform writes from tracker while browsing exercises -- no "database is locked" errors |
| Poor logging UX (too many taps) | Session Tracker UI | Verify: log a complete set (confirm weight + reps) in under 3 taps. Previous performance is visible without navigation |
| No inline previous performance | Session Tracker UI | Verify: when logging bench press, the display shows "Last: 3x8 @ 60kg" without the user navigating away |

## Sources

- [ASP.NET Core Blazor with Entity Framework Core (EF Core) - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0) -- Official DbContext lifetime guidance for Blazor Server
- [ASP.NET Core Blazor SignalR guidance - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-10.0) -- Circuit lifecycle, reconnection, mobile disconnections
- [Blazor is NOT losing SignalR connection. Browser is! - GitHub Issue #40721](https://github.com/dotnet/aspnetcore/issues/40721) -- Mobile browser tab-switching kills WebSocket connections
- [Concurrent Write on SQLite with single connection - EF Core Issue #22664](https://github.com/dotnet/efcore/issues/22664) -- SQLite does not support concurrent write transactions
- [SQLite Error 5: 'database is locked' - EF Core Issue #29514](https://github.com/dotnet/efcore/issues/29514) -- WAL mode and busy timeout as mitigations
- [Blazor concurrency problem using Entity Framework Core - Issue #18340](https://github.com/dotnet/aspnetcore/issues/18340) -- The original issue documenting DbContext threading in Blazor Server
- [Inheritance Mapping in EF Core - Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) -- TPH vs TPT vs TPC tradeoffs
- [The Deceptively Complex World of Calendar Events and RRULEs - Nylas](https://www.nylas.com/blog/calendar-events-rrules/) -- RRULE complexity and DST edge cases
- [Recurrence series changes time of day after Daylight Savings - rrule Issue #550](https://github.com/jkbrzt/rrule/issues/550) -- DST bugs in recurrence implementations
- [Designing a lightweight workout log - George Wang](https://georgewang89.medium.com/designing-a-lightweight-workout-log-bd430039762f) -- UX research on workout logging speed
- [Strong Workout App Redesign UX Case Study](https://medium.com/@hwaijunyap/ui-ux-case-study-strong-workout-app-redesign-fc22afbada65) -- Common UX failures in workout trackers
- [Finally! Improved Blazor Server reconnection UX - Jon Hilton](https://jonhilton.net/blazor-server-reconnects/) -- Custom reconnection handlers in .NET 9+
- [How to design a scalable data model for a workout tracking app - Dittofi](https://www.dittofi.com/learn/how-to-design-a-data-model-for-a-workout-tracking-app) -- Template vs instance separation in workout data models

---
*Pitfalls research for: Unified Workout Planner (Blazor Server / EF Core / SQLite)*
*Researched: 2026-03-21*
