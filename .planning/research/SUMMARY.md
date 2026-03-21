# Project Research Summary

**Project:** Unified Workout Planner
**Domain:** Personal fitness tracking — strength + endurance (Blazor Server / .NET 10 / EF Core / SQLite)
**Researched:** 2026-03-21
**Confidence:** HIGH

## Executive Summary

This is a personal single-user workout planner that unifies strength training and endurance tracking — a gap that is well-documented in the competitive landscape. Hevy and Strong are excellent strength loggers but have no calendar, no real endurance support, and no planned-vs-actual comparison. TrainingPeaks covers both but is coach-oriented, subscription-based, and complex. No consumer-grade personal tool combines strength and endurance with EMOM/superset grouping, a real calendar scheduler, and planned-vs-actual deviation tracking. The recommended approach is to build on the already-decided Blazor Server / .NET 10 / EF Core / SQLite foundation, layer MudBlazor 9.2.0 for UI, and follow a clear dependency chain: data model -> exercise library -> templates -> calendar -> session tracker -> analytics.

The most important architectural decisions are settled by research: use EF Core TPH for the exercise type hierarchy, use a DbContext factory (not direct injection), persist session state to the database incrementally on every set logged (not only on "Finish Workout"), and snapshot planned targets at session creation rather than referencing the live template. These four decisions either cannot be changed cheaply after the fact or directly enable the app's core differentiator (planned-vs-actual tracking). Get them right in Phase 1.

The principal risk is Blazor Server circuit death during active workout logging. Mobile browsers (iOS Safari especially) kill background WebSocket connections within seconds of a tab switch, wiping all in-memory component state. The mitigation is non-negotiable: persist each set to the database as it is logged, and implement a session resume flow on component initialization. This must be a first-class architectural concern in the session tracker phase, not an afterthought. Secondary risks are manageable: SQLite lock contention is solved with `IDbContextFactory` and WAL mode; recurrence over-engineering is solved by materializing concrete `ScheduledWorkout` rows rather than storing RRULE strings; rigid exercise types are solved by making type-specific columns nullable rather than using separate tables per type.

## Key Findings

### Recommended Stack

The base stack (Blazor Server, .NET 10, EF Core 10 LTS, SQLite) is fixed. Research identified the supplementary packages needed: MudBlazor 9.2.0 as the sole UI component library (Material Design, .NET 10-verified, includes DropZone for drag-and-drop reorder), Heron.MudCalendar 4.0.0 for the weekly/monthly calendar views (with a custom MudBlazor grid as a fallback if it falls short), Blazor-ApexCharts 6.1.0 for analytics charting (line/bar/area with zoom and real-time updates), Ical.Net 5.2.1 for recurrence rule parsing if needed, QuestPDF 2026.2.3 for PDF export, and CsvHelper 33.1.0 for CSV export. All packages were verified on NuGet against .NET 10 compatibility as of 2026-03-21.

**Core technologies:**
- MudBlazor 9.2.0 — primary UI framework — Material Design, full .NET 10 support, DropZone covers drag-and-drop exercise reorder
- Heron.MudCalendar 4.0.0 — calendar views — MudBlazor-native, lighter than Radzen Scheduler for a personal app
- Blazor-ApexCharts 6.1.0 — charting — best free Blazor chart library, interactive zoom for trend analysis
- EF Core 10 + SQLite — data persistence — LTS, TPH inheritance support, JSON complex types, DateOnly support
- Ical.Net 5.2.1 — recurrence expansion — RFC 5545 standard, 27M+ downloads, handles DST edge cases
- QuestPDF 2026.2.3 — PDF export — fluent C# API, free for personal use
- CsvHelper 33.1.0 — CSV export — industry standard, 100M+ downloads

### Expected Features

Research analyzed Hevy, Strong, Fitbod, TrainingPeaks, Runna, HYBRD, and others to establish what belongs at each tier.

**Must have (table stakes):**
- Exercise library with search/filter by type, muscle group, equipment
- Custom exercise creation
- Workout templates (reusable blueprints) with ordered exercises and targets
- Superset grouping with vertical connector visual and smart scrolling
- Session logging: strength (sets/reps/weight with checkmark completion) and endurance (distance/duration/pace)
- Previous performance display inline during logging — critical for progressive overload
- Rest timer — auto-start on set completion, adjustable, visual countdown
- Personal record tracking — auto-detect weight/rep/volume/1RM PRs for strength, pace/distance PRs for endurance
- Weekly calendar view as the primary planning interface
- RPE logging per set (strength) or per session (endurance)
- Session notes per session and per exercise
- Workout history — chronological log with search/filter
- CSV export for data ownership

**Should have (differentiators that justify building this app over existing tools):**
- EMOM grouping construct — no consumer app supports this natively; essential for the user's actual training pattern (weighted pull-ups + dips EMOM)
- Planned-vs-actual deviation tracking — store planned targets separately from actuals; show deviation inline
- Unified strength + endurance analytics — single dashboard, both modalities, the core thesis
- Warm-up/cool-down blocks — excluded from volume/PR calculations
- Quick-start "today's workout" on the home screen — single action to begin logging
- Recurrence rules for scheduling — "every Monday", "every other day", "3x/week Mon/Wed/Fri"
- Progressive overload suggestions — rule-based threshold nudges, not AI
- Monthly calendar overview — training density at a glance
- PDF export — formatted workout plan/summary

**Defer to v2+:**
- Wearable/Garmin integration (OAuth, API work, data sync)
- Real-time GPS tracking (Strava handles this better)
- Progressive overload suggestions (needs historical data to be useful; add after v1 data accumulates)
- Monthly calendar overview (weekly view covers the core need for v1)
- AI-generated workouts (out of scope — let users build their own)
- Social features, video demos, nutrition tracking, gamification, body measurements

### Architecture Approach

The architecture follows a standard four-layer Blazor Server pattern: Page Components (route handling, orchestration) -> Shared UI Components (reusable inputs using Parameter-Down/EventCallback-Up) -> Service Layer (all business logic, all EF Core access) -> AppDbContext/SQLite. The domain model has four aggregate roots: Exercise (TPH hierarchy with StrengthExercise and EnduranceExercise), WorkoutTemplate (with TemplateItem join table supporting GroupType for superset/EMOM and TemplateSection for warm-up/cool-down), ScheduledWorkout (with RecurrenceRule and DateOnly dates), and WorkoutLog (with ExerciseLog, SetLog for strength, and EnduranceLog for endurance). The deliberate template-to-session separation — templates are blueprints, sessions are snapshots — is the foundational design decision for planned-vs-actual tracking.

**Major components:**
1. Exercise Library page + ExercisePicker shared component — catalog, search, filter, custom creation
2. Template Builder page + ExerciseGroupEditor component — EMOM/superset grouping, section assignment, target setting
3. Calendar page + WeekView/MonthView components — scheduling with recurrence, planned-vs-completed overview
4. Session Tracker page + SetEditor/EnduranceEditor/TimerControl components — the core logging flow with incremental persistence
5. Analytics page + StatCard components + ApexCharts — volume trends, PRs, adherence, unified view
6. Service layer: ExerciseService, TemplateService, ScheduleService, SessionService, AnalyticsService

### Critical Pitfalls

1. **Circuit death during mid-workout logging** — Persist each set to the database immediately on user action; implement session resume on component init; extend SignalR timeouts for the tracker page; save current exercise index to localStorage as a fallback. Never accumulate state only in memory.

2. **DbContext threading violations** — Use `AddDbContextFactory<AppDbContext>` in Program.cs from day one, never `AddDbContext`. Inject `IDbContextFactory<AppDbContext>` into components. Create and dispose per-method contexts for reads. Zero `@inject AppDbContext` in any `.razor` file — ever.

3. **Template-session coupling without snapshot** — When the user starts a session, copy the template's current exercise targets into the WorkoutLog as immutable planned values. Store `OriginalTemplateId` as a soft reference only. Never reconstruct planned values by querying the live template. Recovery cost if this is done wrong: HIGH.

4. **Rigid exercise type model** — Keep type-specific columns nullable. A weighted pull-up has reps + optional weight. A plank has duration but no reps or weight. The `ExerciseType` discriminator drives UI rendering, not schema existence. Use a composable metrics approach. Recovery cost if done wrong: HIGH (schema migration + component rewrites).

5. **Recurrence over-engineering** — Materialize concrete `ScheduledWorkout` rows for a rolling 4-week window. Do not store RRULE strings and parse at query time for the calendar view. "What's scheduled this week?" must be a simple date-range query, not a recurrence expansion. Use Ical.Net only if materialized instances prove insufficient.

## Implications for Roadmap

Based on the dependency chain in FEATURES.md, the build order in ARCHITECTURE.md, and the phase-to-pitfall mapping in PITFALLS.md, the following phase structure is recommended.

### Phase 1: Data Foundation
**Rationale:** Every feature references exercises. Every phase builds on the data model. TPH inheritance, DbContext factory pattern, and SQLite connection string configuration must be established before any feature work. Getting the data model wrong (rigid types, template-session coupling) has HIGH recovery cost. Do it once, do it right.
**Delivers:** AppDbContext with full TPH Exercise hierarchy, WorkoutTemplate + TemplateItem, ScheduledWorkout + RecurrenceRule, WorkoutLog + ExerciseLog + SetLog + EnduranceLog, all enums, EF Core migrations, seed data for ~50 exercises.
**Addresses:** Foundation for all features
**Avoids:** DbContext threading violations (IDbContextFactory from day one), rigid exercise type model (nullable columns, composable metrics from day one), template-session coupling (WorkoutLog planned-snapshot structure from day one), SQLite lock contention (WAL mode + busy timeout in connection string)

### Phase 2: Exercise Library
**Rationale:** The exercise library is referenced by every other feature. Templates cannot be built without exercises. The session tracker cannot log without exercises. Build the catalog and custom exercise creation first.
**Delivers:** ExerciseLibrary page, ExercisePicker shared component, ExerciseService (search/filter by name/type/muscle group/equipment), custom exercise creation flow.
**Uses:** MudBlazor data table, search inputs, ExerciseType/MuscleGroup/Equipment filter chips
**Implements:** Pattern 1 (Parameter-Down/EventCallback-Up), Pattern 3 (Service Layer Encapsulation)

### Phase 3: Workout Templates
**Rationale:** Templates are the planning layer. They must exist before anything can be scheduled or logged. This phase includes the EMOM/superset grouping constructs (a core differentiator) and warm-up/cool-down section support.
**Delivers:** TemplateBuilder page, TemplateList page, TemplateService (CRUD), ExerciseGroupEditor component (superset/EMOM visual grouping), drag-and-drop exercise reorder (MudBlazor DropZone), section assignment (WarmUp/Working/CoolDown), strength and endurance target entry per exercise.
**Uses:** MudBlazor DropZone (AllowReorder), ExercisePicker, StrengthTargetEditor + EnduranceTargetEditor sub-components (polymorphic rendering — no runtime type checks in parent)
**Implements:** GroupType enum (Superset, EMOM, Circuit), TemplateSection enum, the template-is-immutable-after-session-creation contract

### Phase 4: Calendar Scheduler
**Rationale:** Once templates exist, they need to be scheduled. The weekly calendar is the primary navigation ("what do I do today?"). Recurrence must be implemented correctly here — materialized rows, not RRULE expansion.
**Delivers:** Calendar page, WeekView and MonthView shared components, ScheduleService (create/edit/delete scheduled workouts, recurrence materialization for rolling 4-week window), "this instance only" vs "this and future" edit semantics, visual planned/completed/missed/skipped indicators.
**Uses:** Heron.MudCalendar 4.0.0 (with custom MudBlazor grid fallback if needed), DateOnly throughout
**Avoids:** Recurrence over-engineering (concrete rows only), DST bugs (DateOnly, not DateTimeOffset)

### Phase 5: Session Tracker (Logging)
**Rationale:** The session tracker is the highest-complexity feature with the most critical pitfall (circuit death). Building it after the calendar enables the Quick-Start flow. This phase must implement incremental persistence from the start.
**Delivers:** SessionTracker page, SessionService (scoped circuit state, StartSession snapshots planned targets, SaveSession persists to DB, resume-on-init for in-progress sessions), SetEditor component (reps/weight with increment buttons, previous performance inline, set type selector), EnduranceEditor component (distance/duration/pace with auto-calculation), TimerControl component (rest timer + stopwatch, service-level state for navigation survival), PR detection live during session, RPE and notes per set/session, partial completion support (individual exercise skip).
**Uses:** System.Timers.Timer wrapped in service, JS interop for localStorage session backup, extended SignalR timeout configuration
**Avoids:** Circuit death data loss (incremental DB writes + localStorage backup + resume flow), poor logging UX (pre-fill previous values, single-tap confirm, no keyboard for routine entries)

### Phase 6: Analytics Dashboard
**Rationale:** Analytics depend on accumulated log data from Phase 5. Build it after the logging flow is stable so there is real data to display. This phase delivers the unified strength + endurance view that is the app's core thesis.
**Delivers:** Analytics page, AnalyticsService (volume trends, PR history, streaks, endurance pace/distance aggregates, planned-vs-actual adherence percentage), StatCard components, ApexCharts line/bar/area charts, PR timeline, training density calendar overlay, weekly adherence score.
**Uses:** Blazor-ApexCharts 6.1.0, EF Core aggregation queries with proper indexes on Date/ExerciseId
**Avoids:** Analytics query performance traps (indexed date range queries, projected list views, precomputed weekly summaries for dashboard)

### Phase 7: Export and Polish
**Rationale:** Export is table stakes but low urgency — it depends on having data. Progressive overload suggestions need historical data to analyze. These features are independent of each other and can be built in any order.
**Delivers:** CSV export (CsvHelper, date range filtering), PDF export (QuestPDF, workout templates and weekly summaries), progressive overload suggestions (rule-based: "you hit 3x8@20kg twice, ready for 22.5kg?"), "today's workout" quick-start on Home page.
**Uses:** QuestPDF 2026.2.3, CsvHelper 33.1.0
**Defers:** Wearable integration, AI suggestions

### Phase Ordering Rationale

- Phases 1-5 are strict sequential dependencies enforced by the domain model: exercises must exist before templates, templates before schedules, schedules before sessions.
- Phase 6 (analytics) can begin once Phase 5 produces any log data, but is practically useful only after several sessions.
- Phase 7 items are independent of each other and of Phase 6; they can begin as soon as Phase 5 is stable.
- The Home page quick-start feature (Phase 7) is built last because it requires templates, schedules, and session tracking to all be working.
- Pitfall prevention is front-loaded: the four HIGH-recovery-cost decisions (DbContext factory, TPH model, nullable columns, planned snapshot) all land in Phase 1.

### Research Flags

Phases likely needing deeper research during planning:
- **Phase 3 (Templates):** EMOM grouping is a novel UX pattern with no well-documented Blazor reference implementation. The ExerciseGroupEditor component design (how EMOM intervals interact with the TemplateItem model) needs deliberate design work before coding.
- **Phase 5 (Session Tracker):** The incremental persistence + circuit-death resume flow has no canonical Blazor Server recipe. The localStorage backup + SessionService.Resume pattern needs prototype validation early in the phase.
- **Phase 6 (Analytics):** Planned-vs-actual adherence calculation requires careful query design across WorkoutLog + TemplateItem snapshot data. The analytics query schema (what indexes, what precomputed summaries) should be designed upfront.

Phases with standard patterns (can skip deeper research):
- **Phase 1 (Data Foundation):** EF Core TPH, DbContext factory, SQLite migrations — all well-documented in Microsoft official docs. ARCHITECTURE.md provides the full entity model and EF Core configuration verbatim.
- **Phase 2 (Exercise Library):** Standard CRUD with search/filter. MudBlazor data table + text field patterns are fully documented.
- **Phase 4 (Calendar):** Heron.MudCalendar has docs; materialized-instance recurrence is a simple pattern. If MudCalendar falls short, the fallback (custom MudSimpleTable grid) is also standard.
- **Phase 7 (Export):** QuestPDF and CsvHelper are well-documented libraries with many examples.

## Confidence Assessment

| Area       | Confidence | Notes                                                                                                           |
|------------|------------|-----------------------------------------------------------------------------------------------------------------|
| Stack      | HIGH       | All packages verified on NuGet against .NET 10 compatibility as of 2026-03-21. Official Microsoft docs for EF Core patterns. |
| Features   | MEDIUM-HIGH | Based on analysis of 10+ real apps (Hevy, Strong, TrainingPeaks, Runna, Fitbod, HYBRD, etc.). Feature table is grounded in observed competitor behavior, not speculation. |
| Architecture | HIGH     | Domain model and EF Core configuration provided in full detail. Microsoft official docs for Blazor Server state management and EF Core inheritance confirm all patterns. |
| Pitfalls   | HIGH       | Most pitfalls verified against GitHub issues in the official dotnet/aspnetcore and dotnet/efcore repos. Circuit death and DbContext threading are documented, known issues. |

**Overall confidence:** HIGH

### Gaps to Address

- **EF Core complex types with JSON on SQLite (polymorphic):** STACK.md flags that polymorphic complex types (discriminated JSON for LogDetails) may need a value converter workaround in EF Core 10 if direct complex type inheritance is not supported. Validate during Phase 1 implementation. Fallback is a `string` column with `System.Text.Json` value converter — this is acceptable and well-understood.

- **Heron.MudCalendar weekly interaction model:** The calendar library is community-maintained with a smaller user base. Validate that day-slot interaction (click a day to schedule a workout, drag to move) meets UX requirements in Phase 4. The custom MudSimpleTable + CSS Grid fallback is prepared.

- **Timer accuracy over SignalR:** For the rest timer and EMOM countdown, `System.Timers.Timer` firing server-side and pushing updates over SignalR at 1-second intervals is generally acceptable. If visual lag is noticeable on mobile, the JS interop stopwatch display is the upgrade path. This is a refinement concern, not a blocking gap.

- **Progressive overload suggestion thresholds:** The rule-based suggestion logic ("hit target N times -> suggest increment") needs user-configurable thresholds. The increment amounts (2.5kg for barbell, 1kg for dumbbell, etc.) are domain knowledge — these defaults need a first iteration and a settings UI. Defer to Phase 7 with a note to prototype with the actual user.

## Sources

### Primary (HIGH confidence)
- [MudBlazor NuGet v9.2.0](https://www.nuget.org/packages/MudBlazor) — verified .NET 10 compatibility 2026-03-21
- [Blazor-ApexCharts NuGet v6.1.0](https://www.nuget.org/packages/Blazor-ApexCharts) — verified 2026-03-21
- [Ical.Net NuGet v5.2.1](https://www.nuget.org/packages/Ical.Net) — verified 2026-03-21
- [QuestPDF NuGet v2026.2.3](https://www.nuget.org/packages/QuestPDF) — verified 2026-03-21
- [EF Core 10 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew) — Microsoft official docs
- [EF Core Inheritance (TPH/TPT/TPC)](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) — Microsoft official docs
- [EF Core SQLite Limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations) — Microsoft official docs
- [ASP.NET Core Blazor with EF Core](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0) — Official DbContext lifetime guidance for Blazor Server
- [ASP.NET Core Blazor SignalR guidance](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/signalr?view=aspnetcore-10.0) — Circuit lifecycle, mobile disconnections
- [Blazor Server State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-10.0) — Scoped service patterns

### Secondary (MEDIUM confidence)
- [Heron.MudCalendar NuGet v4.0.0](https://www.nuget.org/packages/Heron.MudCalendar) — community package, verified 2026-03-21
- [Blazor DbContext threading issue #18340](https://github.com/dotnet/aspnetcore/issues/18340) — confirms threading violation pattern
- [SQLite "database is locked" EF Core issue #29514](https://github.com/dotnet/efcore/issues/29514) — WAL mode and busy timeout mitigations
- [Mobile browser kills SignalR issue #40721](https://github.com/dotnet/aspnetcore/issues/40721) — circuit death on tab switch confirmed
- [Best Hybrid Fitness Apps 2025](https://www.findyouredge.app/news/best-hybrid-fitness-apps-2025) — competitive gap analysis
- [Hevy](https://www.hevyapp.com/features/), [Strong](https://help.strongapp.io/), [TrainingPeaks](https://help.trainingpeaks.com/) — feature comparison baseline

### Tertiary (LOW confidence / needs validation)
- [Heron.MudCalendar](https://github.com/hawkerm/mudcalendar) — weekly slot interaction quality, validate in Phase 4
- EF Core 10 complex type polymorphism on SQLite — needs prototype validation in Phase 1

---
*Research completed: 2026-03-21*
*Ready for roadmap: yes*
