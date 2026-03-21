# Phase 5: Session Tracking - Research

**Researched:** 2026-03-22
**Domain:** Real-time workout logging with Blazor Server, EF Core incremental persistence, type-aware UI
**Confidence:** HIGH

## Summary

Phase 5 implements the core session tracking experience: starting a workout from a scheduled entry, logging sets (strength) and distance/duration (endurance) with type-appropriate inputs, viewing previous performance inline, marking exercise completion status, rating the session with RPE and notes, and resuming after any interruption. The data model is fully built (Phase 1), the scheduling infrastructure exists (Phase 4), and the UI design system is mature (Phase 2/3/4). This phase is primarily a service layer + UI build on top of solid existing foundations.

The primary technical challenges are: (1) the snapshot strategy -- deep-copying template targets into SetLog/EnduranceLog rows at session creation time, (2) incremental persistence -- saving individual set completions to the database immediately for circuit-death resilience, (3) elapsed time counter using System.Threading.Timer with InvokeAsync(StateHasChanged) threading model, and (4) a complex but well-specified expand/collapse exercise interaction pattern. All of these are well-understood Blazor Server patterns with no exotic dependencies.

**Primary recommendation:** Create a `SessionService` following the same `IDbContextFactory<AppDbContext>` pattern as `SchedulingService`. The service handles session creation (with snapshot), incremental set/endurance persistence, previous performance queries, and session finalization. The UI consists of a Session page with inline components (no separate component files except `SessionExerciseItem.razor` and `SessionSummary.razor` per the UI spec).

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Pre-filled editable set rows -- each set shows planned weight/reps pre-filled from template snapshot. Tap the number to edit, tap checkmark to complete the set.
- **D-02:** Set type defaults to Working. Tap a small type label to cycle through: Working -> Warm-up -> Failure -> Drop. Minimal UI footprint.
- **D-03:** Users can add extra sets beyond the template target via a "+" button after the last planned set. New sets pre-fill with the same weight as the previous set.
- **D-04:** Previous performance shown as expandable drawer -- tap "Previous" to expand last 3 sessions with sets/weight/reps per session. Hidden by default to keep logging view clean.
- **D-05:** No built-in timer/stopwatch. Just input fields for duration, distance, and pace after the activity. Users use their phone clock or watch for timing.
- **D-06:** Distance + duration are manual inputs, pace is auto-calculated from those two values. HR zone is optional (1-5 select or skip).
- **D-07:** Previous endurance performance uses the same expandable drawer pattern as strength -- last 3 sessions with distance/duration/pace.
- **D-08:** Hybrid navigation: exercise list always visible as compact rows. Tap one to expand its set entry inline. Only one exercise expanded at a time. Completed exercises show summary (e.g. "3/3 sets"), pending ones show "pending".
- **D-09:** Two entry points for starting a session: (1) "Start Session" button in the calendar's WorkoutDetailDialog, (2) a dedicated /session page listing today's scheduled workouts.
- **D-10:** Exercise completion status via manual toggle -- explicit Complete / Partial / Skip buttons per exercise. User decides the status regardless of set completion state.
- **D-11:** End-of-session summary screen after completing the last exercise. Shows total volume, duration, and asks for RPE (1-10 slider) and free-text notes. "Finish Session" button to close out.
- **D-12:** Session page shows a progress bar at top: "3/5 exercises" with filled/empty segments.
- **D-13:** Save on every set completion -- each time the user taps checkmark on a set or saves endurance data, it's immediately persisted to the database. Most resilient approach.
- **D-14:** No rest timer. Users manage their own rest between sets.
- **D-15:** Auto-navigate to incomplete session -- if an incomplete WorkoutLog exists (StartedAt set, CompletedAt null), automatically redirect to the session page with it loaded. No prompt needed.
- **D-16:** Abandon session marks as partial -- keeps whatever sets were logged, sets CompletedAt, marks ScheduledWorkout as Completed with partial data. No data is discarded.

### Claude's Discretion
- Session page URL structure and routing
- Set row component design details (spacing, input sizing)
- How the progress bar segments are styled
- Summary screen layout and volume calculation logic
- How the exercise expand/collapse animation works
- Exact previous performance query strategy (eager load vs on-demand)

### Deferred Ideas (OUT OF SCOPE)
- Built-in rest timer -- decided to skip for now (D-14), could add in QoL phase
- Superset/EMOM group-aware logging (log exercises in group order) -- future enhancement
- Live timer/stopwatch for endurance -- users use external timing for now (D-05)
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SESS-01 | Start logging from a scheduled workout -- open template with targets pre-filled | SessionService.StartSessionAsync creates WorkoutLog, snapshots template targets into SetLog/EnduranceLog rows with Planned values pre-filled |
| SESS-02 | Strength logging: tap through sets, enter weight and reps, checkmark to complete | SetRow inline component with pre-filled inputs, checkmark button persists via SessionService.CompleteSetAsync |
| SESS-03 | Endurance logging: timer/stopwatch with distance and pace entry | Per D-05: no timer/stopwatch. EnduranceInputGroup with manual distance/duration inputs, auto-calc pace. Persisted via SessionService.SaveEnduranceLogAsync |
| SESS-04 | Previous performance displayed inline for each exercise | PreviousPerformanceDrawer loads last 3 sessions on-demand via SessionService.GetPreviousPerformanceAsync |
| SESS-05 | Mark exercises as completed, partially completed, or skipped | ExerciseStatusButtons with Complete/Partial/Skip toggles, tracked in component state and persisted on session finish |
| SESS-06 | Rest timer -- auto-start on set completion, adjustable duration | Per D-14: explicitly deferred. No rest timer implementation in this phase |
| SESS-07 | RPE rating (1-10) per session | SessionSummary component with range slider (1-10), persisted to WorkoutLog.Rpe on finish |
| SESS-08 | Free-text session notes | SessionSummary textarea, persisted to WorkoutLog.Notes on finish |
| SESS-09 | Incremental persistence -- save progress to DB during logging | SessionService.CompleteSetAsync and SaveEnduranceLogAsync persist on every completion action per D-13 |
| SESS-10 | Resume incomplete session after connection loss | On page load, check for WorkoutLog where CompletedAt is null. Auto-redirect per D-15. Pre-fill actual values from saved data |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| ASP.NET Core Blazor Server | 10.0 | Interactive server-side rendering over SignalR | Already configured in project |
| EF Core with SQLite | 10.0.5 | Data access with IDbContextFactory pattern | Already configured, all services use this pattern |
| System.Threading.Timer | .NET BCL | Elapsed time counter on session page | No external dependency needed; standard pattern for periodic UI updates in Blazor Server |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.AspNetCore.Components.Forms | 10.0 | InputNumber, InputSelect, InputTextArea for form inputs | Already in _Imports.razor |
| Microsoft.EntityFrameworkCore | 10.0.5 | Include/ThenInclude for eager loading, LINQ queries | Already referenced |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| System.Threading.Timer | PeriodicTimer (.NET 6+) | PeriodicTimer is newer and async-native, but System.Threading.Timer is simpler for this use case and already the established pattern in Blazor Server documentation |
| Manual number inputs | Blazor InputNumber<T> | InputNumber provides validation binding but styling requires ::deep; raw `<input type="number">` gives more CSS control. Either works -- recommend InputNumber for consistency with Phase 2/3 patterns |

**Installation:**
```bash
# No new packages needed -- all dependencies already in project
```

## Architecture Patterns

### Recommended Project Structure
```
Services/
  SessionService.cs          # New -- all session CRUD and query logic

Components/
  Pages/
    Session.razor             # New -- session page (landing + active)
    Session.razor.cs          # New -- code-behind with service injection, timer, state
    Session.razor.css         # New -- scoped styles
  Shared/
    SessionExerciseItem.razor      # New -- expand/collapse exercise logging
    SessionExerciseItem.razor.css  # New -- scoped styles
    SessionSummary.razor           # New -- end-of-session overlay
    SessionSummary.razor.css       # New -- scoped styles
    WorkoutDetailDialog.razor      # Modified -- add "Start Session" button

  Layout/
    MainLayout.razor          # Modified -- add "Session" NavLink

Data/
  Enums/
    ExerciseCompletionStatus.cs  # New -- Complete, Partial, Skipped enum

wwwroot/
  app.css                     # Modified -- new color tokens, animations, z-index
```

### Pattern 1: SessionService with IDbContextFactory
**What:** A scoped service registered in DI that handles all session operations. Each method creates its own DbContext via the factory, matching the established pattern in SchedulingService and MaterializationService.
**When to use:** All database operations for session tracking.
**Example:**
```csharp
// Source: Follows SchedulingService pattern in Services/SchedulingService.cs
public class SessionService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<WorkoutLog> StartSessionAsync(int scheduledWorkoutId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        // Load scheduled workout with template + items + exercises
        // Create WorkoutLog with StartedAt = DateTime.UtcNow
        // Snapshot template targets into SetLog/EnduranceLog rows
        // Return the created WorkoutLog with navigation properties
    }

    public async Task CompleteSetAsync(int setLogId, double? actualWeight, int? actualReps, SetType setType)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var setLog = await context.SetLogs.FindAsync(setLogId);
        if (setLog == null) return;
        setLog.ActualWeight = actualWeight;
        setLog.ActualReps = actualReps;
        setLog.SetType = setType;
        setLog.IsCompleted = true;
        await context.SaveChangesAsync();
    }
}
```

### Pattern 2: Template Snapshot at Session Creation
**What:** When a session starts, deep-copy the TemplateItem targets into SetLog/EnduranceLog Planned columns. This decouples the log from template changes (per Phase 1 decisions D-14, D-15, D-16).
**When to use:** StartSessionAsync only.
**Example:**
```csharp
// For each TemplateItem in the template:
foreach (var item in template.Items.OrderBy(i => i.Position))
{
    if (item.Exercise is StrengthExercise)
    {
        // Create N SetLog rows (one per TargetSets)
        for (int s = 1; s <= (item.TargetSets ?? 1); s++)
        {
            workoutLog.SetLogs.Add(new SetLog
            {
                ExerciseId = item.ExerciseId,
                SetNumber = s,
                SetType = SetType.Working,
                PlannedReps = item.TargetReps,
                PlannedWeight = item.TargetWeight,
                // Pre-fill actual with planned per UI spec
                ActualReps = item.TargetReps,
                ActualWeight = item.TargetWeight,
                IsCompleted = false
            });
        }
    }
    else if (item.Exercise is EnduranceExercise endEx)
    {
        workoutLog.EnduranceLogs.Add(new EnduranceLog
        {
            ExerciseId = item.ExerciseId,
            ActivityType = endEx.ActivityType,
            PlannedDistance = item.TargetDistance,
            PlannedDurationSeconds = item.TargetDurationSeconds,
            PlannedPace = item.TargetPace,
            PlannedHeartRateZone = item.TargetHeartRateZone,
            // Pre-fill actual with planned
            ActualDistance = item.TargetDistance,
            ActualDurationSeconds = item.TargetDurationSeconds,
            IsCompleted = false
        });
    }
}
```

### Pattern 3: Blazor Server Timer for Elapsed Time
**What:** Use System.Threading.Timer to update the elapsed time display every second, dispatching to the render thread via InvokeAsync.
**When to use:** Active session page only. Dispose timer on component disposal.
**Example:**
```csharp
// Source: Microsoft Blazor synchronization context docs
// https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context
private Timer? _elapsedTimer;
private TimeSpan _elapsed;

protected override void OnInitialized()
{
    _elapsedTimer = new Timer(_ =>
    {
        _elapsed = DateTime.UtcNow - workoutLog.StartedAt;
        InvokeAsync(StateHasChanged);
    }, null, 0, 1000);
}

public void Dispose()
{
    _elapsedTimer?.Dispose();
}

private string FormatElapsed()
{
    if (_elapsed.TotalHours >= 1)
        return _elapsed.ToString(@"h\:mm\:ss");
    return _elapsed.ToString(@"mm\:ss");
}
```

### Pattern 4: On-Demand Previous Performance Loading
**What:** Load previous performance data lazily when the user expands the "Previous" drawer, not when the page loads. This avoids loading history for all exercises upfront.
**When to use:** PreviousPerformanceDrawer toggle.
**Example:**
```csharp
public async Task<List<PreviousSession>> GetPreviousPerformanceAsync(int exerciseId, int limit = 3)
{
    await using var context = await _contextFactory.CreateDbContextAsync();

    // For strength: get last N completed WorkoutLogs that have SetLogs for this exercise
    var previousSets = await context.SetLogs
        .Where(sl => sl.ExerciseId == exerciseId && sl.IsCompleted)
        .Include(sl => sl.WorkoutLog)
        .Where(sl => sl.WorkoutLog.CompletedAt != null)
        .OrderByDescending(sl => sl.WorkoutLog.StartedAt)
        .GroupBy(sl => sl.WorkoutLogId)
        .Take(limit)
        .ToListAsync();

    // For endurance: similar query on EnduranceLogs
}
```

### Pattern 5: Exercise Completion State Tracking
**What:** Track per-exercise completion status in component state (a Dictionary<int, ExerciseCompletionStatus>). This is an in-memory UI concern during the session, persisted only on session finish/abandon. The exercise exercises don't have a "status" column in the database -- status is determined by whether all sets are completed, partially completed, or the user explicitly sets it.
**When to use:** Session page component state.
**Example:**
```csharp
// In Session.razor.cs
private Dictionary<int, ExerciseCompletionStatus> exerciseStatuses = new();
private int? expandedExerciseId = null;

private void SetExerciseStatus(int exerciseId, ExerciseCompletionStatus status)
{
    exerciseStatuses[exerciseId] = status;
    // Auto-expand next pending exercise after 200ms delay
    // Update progress bar
}
```

### Anti-Patterns to Avoid
- **Saving entire WorkoutLog on every set change:** Only save the individual SetLog/EnduranceLog row that changed. Loading and saving the full WorkoutLog graph on every set completion is wasteful and risks concurrency issues.
- **Eager-loading previous performance for all exercises:** Load on-demand per D-04/D-07. The session page could have 5-10 exercises; loading 3 previous sessions for each upfront wastes DB calls.
- **Using SignalR hub for timer instead of System.Threading.Timer:** Overkill for a single-user app. The timer runs on the server thread and pushes UI updates via the existing SignalR connection.
- **Storing exercise completion status in the database per-exercise:** The data model doesn't have a per-exercise status column. Track in component state and derive from set completion data on session resume.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Pace calculation | Custom pace formula | `distance > 0 && durationSeconds > 0 ? (durationSeconds / 60.0) / distance : null` | Simple arithmetic, but handle division by zero and null inputs |
| Volume calculation | Complex aggregation | LINQ `.Where(s => s.IsCompleted).Sum(s => (s.ActualWeight ?? 0) * (s.ActualReps ?? 0))` | One-liner, no custom logic needed |
| Timer disposal | Manual timer tracking | `IDisposable` on component, dispose timer in `Dispose()` | Blazor lifecycle handles this; missing disposal causes memory leaks |
| Elapsed time formatting | Custom string building | `TimeSpan.ToString(@"h\:mm\:ss")` or `@"mm\:ss"` | .NET format strings handle this correctly |

**Key insight:** This phase has no exotic requirements. The data model is built, the patterns are established, and the UI spec is exhaustive. The implementation is mostly connecting existing pieces with a new service layer and building the UI components per spec.

## Common Pitfalls

### Pitfall 1: Thread Safety with Timer and StateHasChanged
**What goes wrong:** Calling StateHasChanged directly from a Timer callback throws an exception because the callback runs on a ThreadPool thread, not the Blazor synchronization context.
**Why it happens:** System.Threading.Timer fires on a ThreadPool thread, but Blazor's renderer is single-threaded per circuit.
**How to avoid:** Always wrap StateHasChanged in `InvokeAsync(() => StateHasChanged())` inside timer callbacks.
**Warning signs:** "The current thread is not associated with the Dispatcher" exception.

### Pitfall 2: DbContext Lifetime with IDbContextFactory
**What goes wrong:** Holding a DbContext open across multiple user interactions causes stale data and tracking issues.
**Why it happens:** EF Core change tracker accumulates state. In Blazor Server, the component lives for the duration of the circuit (potentially hours).
**How to avoid:** Create a new DbContext per service method call using `await _contextFactory.CreateDbContextAsync()`. Dispose it with `await using`. Never store a DbContext as a component field.
**Warning signs:** Stale data after save, "entity is already being tracked" exceptions.

### Pitfall 3: Snapshot Not Capturing Exercise Type
**What goes wrong:** When creating SetLog/EnduranceLog rows, the code checks `item.Exercise is StrengthExercise` but if Exercise navigation property isn't loaded, it's always null.
**Why it happens:** Lazy loading isn't enabled. TPH requires the discriminator to be loaded via Include.
**How to avoid:** When loading the template for snapshot, always Include Items then ThenInclude Exercise: `.Include(t => t.Items).ThenInclude(i => i.Exercise)`.
**Warning signs:** All exercises treated as one type, or NullReferenceException on Exercise.

### Pitfall 4: Scoped CSS Not Applying to Form Inputs
**What goes wrong:** Styles defined in `.razor.css` don't apply to InputNumber, InputSelect, or textarea elements rendered by child Blazor components.
**Why it happens:** Blazor's CSS isolation adds a unique attribute to elements rendered by the component, but child component elements don't get the parent's attribute.
**How to avoid:** Use `::deep` selector in parent `.razor.css` files when targeting Blazor form component elements. Alternatively, use raw HTML `<input>` elements with direct styling.
**Warning signs:** Inputs appear with default browser styling (white background) instead of the dark theme.

### Pitfall 5: Navigation During Active Session
**What goes wrong:** User navigates away from the session page (clicks "Exercises" or "Templates" in nav) without the session being properly abandoned or completed.
**Why it happens:** No navigation guard prevents leaving the session page.
**How to avoid:** Per the UI spec, the back arrow triggers the Abandon dialog. For nav links, consider adding a `NavigationLock` component (Blazor built-in) or simply rely on the fact that incomplete sessions auto-resume per D-15.
**Warning signs:** Orphaned WorkoutLog records with StartedAt set but CompletedAt null and no way to get back.

### Pitfall 6: Resuming Session with Missing Data
**What goes wrong:** After resuming an incomplete session, actual values that were pre-filled at creation appear as if they were user-entered, even though the user hasn't confirmed them.
**Why it happens:** Pre-filling ActualWeight/ActualReps from Planned values at creation means IsCompleted=false rows already have actual values.
**How to avoid:** Only treat sets as "user-confirmed" when IsCompleted=true. On resume, display actual values in inputs but don't count them toward volume/progress unless IsCompleted is true.
**Warning signs:** Volume calculations include unconfirmed sets.

## Code Examples

### Session Service: Start Session with Snapshot
```csharp
// Source: Follows SchedulingService pattern + Phase 1 snapshot decision D-14/D-15/D-16
public async Task<WorkoutLog> StartSessionAsync(int scheduledWorkoutId)
{
    await using var context = await _contextFactory.CreateDbContextAsync();

    var scheduled = await context.ScheduledWorkouts
        .Include(sw => sw.WorkoutTemplate!)
            .ThenInclude(t => t.Items.OrderBy(i => i.Position))
                .ThenInclude(i => i.Exercise)
        .Include(sw => sw.WorkoutLog)
        .FirstOrDefaultAsync(sw => sw.Id == scheduledWorkoutId);

    if (scheduled == null)
        throw new InvalidOperationException("Scheduled workout not found");

    // If session already exists, return it (resume case)
    if (scheduled.WorkoutLog != null)
        return await LoadSessionAsync(scheduled.WorkoutLog.Id);

    var workoutLog = new WorkoutLog
    {
        ScheduledWorkoutId = scheduledWorkoutId,
        StartedAt = DateTime.UtcNow
    };

    if (scheduled.WorkoutTemplate != null)
    {
        foreach (var item in scheduled.WorkoutTemplate.Items.OrderBy(i => i.Position))
        {
            if (item.Exercise is StrengthExercise)
            {
                var setCount = item.TargetSets ?? 1;
                for (int s = 1; s <= setCount; s++)
                {
                    workoutLog.SetLogs.Add(new SetLog
                    {
                        ExerciseId = item.ExerciseId,
                        SetNumber = s,
                        SetType = SetType.Working,
                        PlannedReps = item.TargetReps,
                        PlannedWeight = item.TargetWeight,
                        ActualReps = item.TargetReps,
                        ActualWeight = item.TargetWeight,
                        IsCompleted = false
                    });
                }
            }
            else if (item.Exercise is EnduranceExercise endEx)
            {
                workoutLog.EnduranceLogs.Add(new EnduranceLog
                {
                    ExerciseId = item.ExerciseId,
                    ActivityType = endEx.ActivityType,
                    PlannedDistance = item.TargetDistance,
                    PlannedDurationSeconds = item.TargetDurationSeconds,
                    PlannedPace = item.TargetPace,
                    PlannedHeartRateZone = item.TargetHeartRateZone,
                    ActualDistance = item.TargetDistance,
                    ActualDurationSeconds = item.TargetDurationSeconds,
                    IsCompleted = false
                });
            }
        }
    }

    context.WorkoutLogs.Add(workoutLog);
    scheduled.Status = WorkoutStatus.Completed;
    await context.SaveChangesAsync();

    return workoutLog;
}
```

### Session Service: Complete Set (Incremental Persistence)
```csharp
// Source: D-13 -- save on every set completion
public async Task CompleteSetAsync(int setLogId, double? actualWeight, int? actualReps, SetType setType)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    var setLog = await context.SetLogs.FindAsync(setLogId);
    if (setLog == null) return;

    setLog.ActualWeight = actualWeight;
    setLog.ActualReps = actualReps;
    setLog.SetType = setType;
    setLog.IsCompleted = true;
    await context.SaveChangesAsync();
}

public async Task UncompleteSetAsync(int setLogId)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    var setLog = await context.SetLogs.FindAsync(setLogId);
    if (setLog == null) return;

    setLog.IsCompleted = false;
    await context.SaveChangesAsync();
}
```

### Session Service: Check for Incomplete Session (D-15)
```csharp
public async Task<int?> GetIncompleteSessionIdAsync()
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    var incomplete = await context.WorkoutLogs
        .Where(wl => wl.CompletedAt == null)
        .Select(wl => (int?)wl.ScheduledWorkoutId)
        .FirstOrDefaultAsync();
    return incomplete;
}
```

### Session Service: Finish Session
```csharp
public async Task FinishSessionAsync(int workoutLogId, int? rpe, string? notes)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    var log = await context.WorkoutLogs
        .Include(wl => wl.ScheduledWorkout)
        .FirstOrDefaultAsync(wl => wl.Id == workoutLogId);
    if (log == null) return;

    log.CompletedAt = DateTime.UtcNow;
    log.Rpe = rpe;
    log.Notes = notes;
    log.ScheduledWorkout.Status = WorkoutStatus.Completed;
    await context.SaveChangesAsync();
}
```

### Session Service: Add Extra Set (D-03)
```csharp
public async Task<SetLog> AddSetAsync(int workoutLogId, int exerciseId, double? prefillWeight)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    var maxSetNumber = await context.SetLogs
        .Where(sl => sl.WorkoutLogId == workoutLogId && sl.ExerciseId == exerciseId)
        .MaxAsync(sl => (int?)sl.SetNumber) ?? 0;

    var newSet = new SetLog
    {
        WorkoutLogId = workoutLogId,
        ExerciseId = exerciseId,
        SetNumber = maxSetNumber + 1,
        SetType = SetType.Working,
        ActualWeight = prefillWeight,
        IsCompleted = false
    };

    context.SetLogs.Add(newSet);
    await context.SaveChangesAsync();
    return newSet;
}
```

### Pace Auto-Calculation (D-06)
```csharp
// In component code -- calculate pace from distance and duration
private double? CalculatePace(double? distanceKm, int? durationSeconds)
{
    if (distanceKm == null || distanceKm <= 0 || durationSeconds == null || durationSeconds <= 0)
        return null;
    return (durationSeconds.Value / 60.0) / distanceKm.Value; // min/km
}

private string FormatPace(double? paceMinPerKm)
{
    if (paceMinPerKm == null) return "--:--";
    var totalSeconds = (int)(paceMinPerKm.Value * 60);
    var minutes = totalSeconds / 60;
    var seconds = totalSeconds % 60;
    return $"{minutes}:{seconds:D2}";
}
```

### Volume Calculation for Summary (D-11)
```csharp
private double CalculateTotalVolume(IEnumerable<SetLog> completedSets)
{
    return completedSets
        .Where(s => s.IsCompleted)
        .Sum(s => (s.ActualWeight ?? 0) * (s.ActualReps ?? 0));
}

private double CalculateTotalDistance(IEnumerable<EnduranceLog> completedEndurance)
{
    return completedEndurance
        .Where(e => e.IsCompleted)
        .Sum(e => e.ActualDistance ?? 0);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| PeriodicTimer (async) | System.Threading.Timer with InvokeAsync | Both valid since .NET 6 | Either works; Timer is simpler for fire-and-forget UI updates |
| Scoped DbContext | IDbContextFactory | .NET 5+ / Blazor Server best practice | Project already uses factory pattern; critical for Blazor Server thread safety |
| JavaScript interop timers | C# System.Threading.Timer | Always available | No JS needed for periodic updates in Blazor Server |

**Deprecated/outdated:**
- None relevant. All patterns used are current for .NET 10.

## Open Questions

1. **Exercise ordering in session matches template ordering**
   - What we know: TemplateItem has a Position property used for ordering.
   - What's unclear: Should warm-up and cool-down section exercises be displayed in their section groups, or flat-listed by position?
   - Recommendation: Flat-list by position. The session view doesn't need section headers -- just exercise items in order. The set type (warm-up, working) on individual sets conveys the intent. This matches the simplified logging experience.

2. **Ad-hoc workouts without templates**
   - What we know: ScheduledWorkout can have WorkoutTemplateId=null with AdHocName set.
   - What's unclear: How to start a session for an ad-hoc workout with no template items to snapshot.
   - Recommendation: Per UI spec, "Start Session" button only shows when workout has a template. Ad-hoc workout session support can be deferred. The landing page can show ad-hoc workouts but without the "Start Session" action.

3. **Exercise completion status persistence strategy**
   - What we know: WorkoutLog has no per-exercise status columns. D-10 says users manually set Complete/Partial/Skip.
   - What's unclear: Where is the per-exercise status stored between set completions?
   - Recommendation: Track in component state (dictionary). On session finish or abandon, the status is implicitly encoded: if all sets for an exercise have IsCompleted=true, it's "Complete"; if some do, "Partial"; if none and user explicitly skipped, "Skipped." The endurance equivalent is the IsCompleted flag. No schema change needed -- derive status from set completion data on resume.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Session" --no-build -q` |
| Full suite command | `dotnet test BlazorApp2.Tests` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| SESS-01 | Start session creates WorkoutLog with snapshot | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.StartSession" -x` | No -- Wave 0 |
| SESS-02 | Complete set persists actual values | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.CompleteSet" -x` | No -- Wave 0 |
| SESS-03 | Endurance log saves distance/duration/pace | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.EnduranceLog" -x` | No -- Wave 0 |
| SESS-04 | Previous performance returns last 3 sessions | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.PreviousPerformance" -x` | No -- Wave 0 |
| SESS-05 | Exercise status derives from set completion | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.ExerciseStatus" -x` | No -- Wave 0 |
| SESS-06 | Rest timer | manual-only | N/A -- deferred per D-14 | N/A |
| SESS-07 | RPE persists to WorkoutLog | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.Rpe" -x` | Partial -- LogTests covers entity-level persistence |
| SESS-08 | Notes persists to WorkoutLog | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.Notes" -x` | Partial -- LogTests covers entity-level persistence |
| SESS-09 | Incremental save on set completion | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.IncrementalSave" -x` | No -- Wave 0 |
| SESS-10 | Resume loads incomplete session | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.Resume" -x` | No -- Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests" --no-build -q`
- **Per wave merge:** `dotnet test BlazorApp2.Tests`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `BlazorApp2.Tests/SessionTests.cs` -- covers SESS-01 through SESS-05, SESS-09, SESS-10
- [ ] Service registration: `builder.Services.AddScoped<SessionService>()` in Program.cs
- [ ] `Data/Enums/ExerciseCompletionStatus.cs` -- new enum needed

*(Existing LogTests.cs provides entity-level persistence validation for WorkoutLog, SetLog, and EnduranceLog; SessionTests will focus on service-level operations)*

## Sources

### Primary (HIGH confidence)
- Project source code: `Data/Entities/WorkoutLog.cs`, `ScheduledWorkout.cs`, `WorkoutTemplate.cs`, `Exercise.cs` -- verified data model
- Project source code: `Services/SchedulingService.cs`, `MaterializationService.cs` -- verified service patterns
- Project source code: `BlazorApp2.Tests/` -- verified test infrastructure and patterns
- Project source code: `wwwroot/app.css` -- verified design token system
- `.planning/phases/05-session-tracking/05-CONTEXT.md` -- locked user decisions
- `.planning/phases/05-session-tracking/05-UI-SPEC.md` -- complete visual/interaction specification
- [Microsoft Blazor synchronization context docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-10.0) -- Timer + InvokeAsync pattern

### Secondary (MEDIUM confidence)
- [Blazor University - InvokeAsync thread safety](https://blazor-university.com/components/multi-threaded-rendering/invokeasync/) -- timer threading pattern

### Tertiary (LOW confidence)
- None. All findings verified against project source code or official documentation.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- no new dependencies, all patterns already established in project
- Architecture: HIGH -- service pattern matches existing SchedulingService exactly; data model built in Phase 1
- Pitfalls: HIGH -- common Blazor Server threading and EF Core patterns well-documented
- UI patterns: HIGH -- exhaustive UI-SPEC provides pixel-level specifications

**Research date:** 2026-03-22
**Valid until:** 2026-04-22 (stable -- no fast-moving dependencies)
