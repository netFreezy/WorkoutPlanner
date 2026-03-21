# Architecture Research

**Domain:** Unified Workout Planner (Strength + Endurance) -- Blazor Server
**Researched:** 2026-03-21
**Confidence:** HIGH

## System Overview

```
┌─────────────────────────────────────────────────────────────────────┐
│                      Presentation Layer                             │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ │
│  │ Exercise │ │ Template │ │ Calendar │ │ Session  │ │Analytics │ │
│  │ Library  │ │ Builder  │ │Scheduler │ │ Tracker  │ │Dashboard │ │
│  │  Page    │ │  Page    │ │  Page    │ │  Page    │ │  Page    │ │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘ │
│       │            │            │            │            │        │
│  ┌────┴────────────┴────────────┴────────────┴────────────┴─────┐  │
│  │                   Shared UI Components                       │  │
│  │  ExercisePicker, SetEditor, TimerControl, WeekView, etc.     │  │
│  └──────────────────────────┬────────────────────────────────────┘  │
├─────────────────────────────┼───────────────────────────────────────┤
│                      Service Layer                                  │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ │
│  │Exercise  │ │Template  │ │Schedule  │ │Session   │ │Analytics │ │
│  │Service   │ │Service   │ │Service   │ │Service   │ │Service   │ │
│  └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────┘ │
│       │            │            │            │            │        │
├───────┴────────────┴────────────┴────────────┴────────────┴────────┤
│                      Data Access Layer                              │
│  ┌─────────────────────────────────────────────────────────────┐    │
│  │                 AppDbContext (EF Core)                       │    │
│  │    DbSet<Exercise>  DbSet<WorkoutTemplate>                  │    │
│  │    DbSet<ScheduledWorkout>  DbSet<WorkoutLog>               │    │
│  └─────────────────────────────┬───────────────────────────────┘    │
├────────────────────────────────┼────────────────────────────────────┤
│                      Storage                                        │
│  ┌─────────────────────────────┴───────────────────────────────┐    │
│  │                    SQLite Database                           │    │
│  │   Exercises | Templates | TemplateItems | Schedules | Logs  │    │
│  └─────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility | Typical Implementation |
|-----------|----------------|------------------------|
| Page Components | Route handling, page layout, orchestrating child components | `.razor` files with `@page` directive, inject services |
| Shared UI Components | Reusable input/display elements (ExercisePicker, SetEditor) | `.razor` files with `[Parameter]` and `[EventCallback]` |
| Service Layer | Business logic, validation, data transformation | C# classes registered as scoped DI services |
| AppDbContext | EF Core context with TPH mapping, queries, persistence | Single `DbContext` subclass with `DbSet<>` properties |
| Domain Entities | Data shape, type hierarchy, validation attributes | C# classes/records with EF Core mapping configuration |

## Domain Model Design

### Entity Hierarchy (TPH -- Table Per Hierarchy)

Use TPH for the exercise type system. This is the correct strategy because:
- Queries will frequently mix strength and endurance exercises in the same workout template
- Polymorphic queries against the base `Exercise` type are the primary access pattern
- SQLite does not support sequences, making TPC impractical (no identity seed/increment support)
- TPH avoids JOINs for the most common query: "get all exercises in a template"

```csharp
// ── Exercise Library (TPH) ────────────────────────────────

public abstract class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }          // Discriminator
    public bool IsCustom { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum ExerciseType { Strength, Endurance }

public class StrengthExercise : Exercise
{
    public MuscleGroup PrimaryMuscleGroup { get; set; }
    public MuscleGroup? SecondaryMuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
}

public class EnduranceExercise : Exercise
{
    public ActivityType ActivityType { get; set; }   // Run, Cycle, Row, Swim, etc.
    public bool SupportsDistance { get; set; } = true;
    public bool SupportsHeartRate { get; set; } = true;
}

public enum MuscleGroup
{
    Chest, Back, Shoulders, Biceps, Triceps, Forearms,
    Quads, Hamstrings, Glutes, Calves, Core, FullBody
}

public enum Equipment
{
    Barbell, Dumbbell, Kettlebell, Cable, Machine,
    Bodyweight, Band, Other, None
}

public enum ActivityType
{
    Run, Cycle, Row, Swim, Walk, Hike, Ski, Other
}
```

### Template System (The Blueprint)

Templates are reusable workout blueprints. A template contains ordered items, each referencing an exercise with type-specific targets.

```csharp
// ── Workout Templates ─────────────────────────────────────

public class WorkoutTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<TemplateItem> Items { get; set; } = [];
}

public class TemplateItem
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public int ExerciseId { get; set; }
    public int SortOrder { get; set; }

    // Grouping constructs
    public int? GroupId { get; set; }               // null = standalone
    public GroupType? GroupType { get; set; }        // Superset, EMOM, Circuit
    public int? GroupSortOrder { get; set; }         // Order within group

    // Section
    public TemplateSection Section { get; set; }    // WarmUp, Working, CoolDown

    // Strength targets (nullable -- only relevant for strength exercises)
    public int? TargetSets { get; set; }
    public int? TargetReps { get; set; }
    public decimal? TargetWeight { get; set; }
    public string? TargetRpe { get; set; }           // e.g., "7-8"

    // Endurance targets (nullable -- only relevant for endurance exercises)
    public decimal? TargetDistanceKm { get; set; }
    public TimeSpan? TargetDuration { get; set; }
    public string? TargetPace { get; set; }          // e.g., "5:30/km"
    public int? TargetHeartRateZone { get; set; }    // 1-5

    // EMOM-specific
    public TimeSpan? IntervalDuration { get; set; }  // e.g., "every 90 seconds"

    // Navigation
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}

public enum GroupType { Superset, EMOM, Circuit }
public enum TemplateSection { WarmUp, Working, CoolDown }
```

### Schedule System (Planning)

```csharp
// ── Scheduling ────────────────────────────────────────────

public class ScheduledWorkout
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public DateOnly ScheduledDate { get; set; }

    // Recurrence
    public int? RecurrenceRuleId { get; set; }

    // Status
    public ScheduleStatus Status { get; set; }       // Planned, Completed, Skipped, Partial

    // Navigation
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public RecurrenceRule? RecurrenceRule { get; set; }
    public WorkoutLog? WorkoutLog { get; set; }       // 1:1 when completed
}

public class RecurrenceRule
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public RecurrenceType Type { get; set; }          // Weekly, EveryNDays, SpecificDays
    public string DaysOfWeek { get; set; } = "";      // "Mon,Wed,Fri" or empty
    public int? EveryNDays { get; set; }              // For "every other day" patterns
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }            // null = indefinite

    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public List<ScheduledWorkout> ScheduledWorkouts { get; set; } = [];
}

public enum RecurrenceType { Weekly, EveryNDays, SpecificDays }
public enum ScheduleStatus { Planned, Completed, Partial, Skipped }
```

### Logging System (What Actually Happened)

The deliberate planned-vs-actual separation is the core design decision. A `WorkoutLog` mirrors a `ScheduledWorkout` but records what actually happened.

```csharp
// ── Workout Logging ───────────────────────────────────────

public class WorkoutLog
{
    public int Id { get; set; }
    public int? ScheduledWorkoutId { get; set; }     // null for ad-hoc sessions
    public int WorkoutTemplateId { get; set; }
    public DateOnly Date { get; set; }
    public TimeSpan? Duration { get; set; }

    // Session metadata
    public int? Rpe { get; set; }                    // 1-10
    public string? Notes { get; set; }

    // Navigation
    public ScheduledWorkout? ScheduledWorkout { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public List<ExerciseLog> ExerciseLogs { get; set; } = [];
}

public class ExerciseLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public int ExerciseId { get; set; }
    public int SortOrder { get; set; }
    public ExerciseCompletionStatus Status { get; set; }

    // Navigation
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
    public List<SetLog> SetLogs { get; set; } = [];           // Strength
    public EnduranceLog? EnduranceLog { get; set; }           // Endurance (1:1)
}

// Strength: multiple sets per exercise
public class SetLog
{
    public int Id { get; set; }
    public int ExerciseLogId { get; set; }
    public int SetNumber { get; set; }
    public int? Reps { get; set; }
    public decimal? WeightKg { get; set; }
    public int? Rpe { get; set; }
    public bool IsWarmUp { get; set; }

    public ExerciseLog ExerciseLog { get; set; } = null!;
}

// Endurance: single record per exercise
public class EnduranceLog
{
    public int Id { get; set; }
    public int ExerciseLogId { get; set; }
    public decimal? DistanceKm { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? AvgPace { get; set; }             // Calculated: "5:30/km"
    public int? AvgHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }

    public ExerciseLog ExerciseLog { get; set; } = null!;
}

public enum ExerciseCompletionStatus { Completed, Partial, Skipped }
```

### Entity Relationship Diagram

```
Exercise (TPH)
    |-- StrengthExercise
    |-- EnduranceExercise
    |
    +<-- TemplateItem -->+ WorkoutTemplate
    |                         |
    |                         +<-- ScheduledWorkout --+ RecurrenceRule
    |                         |        |
    |                         |        +-- WorkoutLog (1:1 or standalone)
    |                         |               |
    +<-- ExerciseLog ---------+               |
              |                               |
              +-- SetLog (1:N, strength)      |
              +-- EnduranceLog (1:1, endurance)|
```

### TPH EF Core Configuration

```csharp
// In AppDbContext.OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Exercise TPH hierarchy
    modelBuilder.Entity<Exercise>(entity =>
    {
        entity.UseTphMappingStrategy();

        entity.HasDiscriminator(e => e.Type)
            .HasValue<StrengthExercise>(ExerciseType.Strength)
            .HasValue<EnduranceExercise>(ExerciseType.Endurance);

        entity.HasIndex(e => e.Type);     // Index the discriminator for filter perf
        entity.HasIndex(e => e.Name);
    });

    // TemplateItem ordering
    modelBuilder.Entity<TemplateItem>(entity =>
    {
        entity.HasIndex(e => new { e.WorkoutTemplateId, e.SortOrder });
    });

    // WorkoutLog <-> ScheduledWorkout 1:1
    modelBuilder.Entity<WorkoutLog>(entity =>
    {
        entity.HasOne(l => l.ScheduledWorkout)
            .WithOne(s => s.WorkoutLog)
            .HasForeignKey<WorkoutLog>(l => l.ScheduledWorkoutId)
            .IsRequired(false);   // Supports ad-hoc (unscheduled) sessions
    });

    // EnduranceLog <-> ExerciseLog 1:1
    modelBuilder.Entity<EnduranceLog>(entity =>
    {
        entity.HasOne(e => e.ExerciseLog)
            .WithOne(e => e.EnduranceLog)
            .HasForeignKey<EnduranceLog>(e => e.ExerciseLogId);
    });
}
```

**Why TPH over TPC for this project:** SQLite does not support sequences, and TPC requires a shared sequence or GUID keys for unique IDs across concrete tables. TPH keeps everything in one table with a discriminator column, which is simpler, faster for polymorphic queries, and fully supported by SQLite. The nullable columns for type-specific fields (e.g., `PrimaryMuscleGroup` is null for endurance exercises) are an acceptable trade-off for a single-user app with a modest exercise library (hundreds, not millions of rows).

## Recommended Project Structure

```
BlazorApp2/
├── Components/
│   ├── App.razor                    # Root component (existing)
│   ├── Routes.razor                 # Router (existing)
│   ├── _Imports.razor               # Global usings (existing)
│   ├── Layout/
│   │   ├── MainLayout.razor         # App layout (existing, extend with nav)
│   │   └── ReconnectModal.razor     # Connection recovery (existing)
│   ├── Pages/
│   │   ├── Home.razor               # Landing / quick-start (existing, repurpose)
│   │   ├── ExerciseLibrary.razor    # Exercise catalog page
│   │   ├── TemplateBuilder.razor    # Create/edit workout templates
│   │   ├── TemplateList.razor       # Browse saved templates
│   │   ├── Calendar.razor           # Weekly/monthly schedule view
│   │   ├── SessionTracker.razor     # Active workout logging
│   │   ├── WorkoutHistory.razor     # Past workout logs
│   │   ├── Analytics.razor          # Charts and trends
│   │   ├── Error.razor              # Error page (existing)
│   │   └── NotFound.razor           # 404 (existing)
│   └── Shared/
│       ├── ExercisePicker.razor     # Search/filter exercise selector
│       ├── ExerciseCard.razor       # Display an exercise with metadata
│       ├── SetEditor.razor          # Inline set editing (reps/weight)
│       ├── EnduranceEditor.razor    # Distance/duration/pace input
│       ├── ExerciseGroupEditor.razor # Superset/EMOM grouping UI
│       ├── TimerControl.razor       # Stopwatch / countdown timer
│       ├── WeekView.razor           # 7-day calendar grid
│       ├── MonthView.razor          # Monthly overview grid
│       ├── ConfirmDialog.razor      # Reusable confirmation modal
│       ├── EmptyState.razor         # "No items yet" placeholder
│       └── StatCard.razor           # Analytics summary card
├── Data/
│   ├── AppDbContext.cs              # EF Core DbContext with TPH config
│   └── Migrations/                  # EF Core migrations
├── Models/
│   ├── Exercise.cs                  # Exercise base + StrengthExercise + EnduranceExercise
│   ├── WorkoutTemplate.cs           # WorkoutTemplate + TemplateItem
│   ├── Schedule.cs                  # ScheduledWorkout + RecurrenceRule
│   ├── WorkoutLog.cs                # WorkoutLog + ExerciseLog + SetLog + EnduranceLog
│   └── Enums.cs                     # All enums in one file
├── Services/
│   ├── ExerciseService.cs           # CRUD + search/filter for exercises
│   ├── TemplateService.cs           # CRUD for workout templates
│   ├── ScheduleService.cs           # Scheduling, recurrence generation, conflict detection
│   ├── SessionService.cs            # Active workout session state + logging
│   └── AnalyticsService.cs          # Aggregations, PR detection, trends
├── Program.cs                       # Startup (existing, extend with DI registrations)
├── appsettings.json                 # Config (existing, add connection string)
└── wwwroot/                         # Static assets (existing)
```

### Structure Rationale

- **Models/:** Flat folder with one file per aggregate root. No sub-folders needed for this domain size. Keeps the entity hierarchy easy to navigate.
- **Services/:** One service per domain boundary. Services are the only layer that touches `AppDbContext`. Components never call EF directly.
- **Data/:** Isolated EF Core concern. Only the `AppDbContext` and migrations live here.
- **Components/Shared/:** Reusable components that appear on multiple pages. These use the "Parameter-Down, EventCallback-Up" pattern exclusively.
- **Components/Pages/:** One page per major feature area. Pages orchestrate shared components and inject services.

## Architectural Patterns

### Pattern 1: Parameter-Down, EventCallback-Up

**What:** Parent components pass data to children via `[Parameter]`, children notify parents of changes via `EventCallback<T>`. This is the standard Blazor component communication model.

**When to use:** Every parent-child component relationship. This is the default -- deviate only with strong justification.

**Trade-offs:** Simple and explicit data flow. Can get verbose with deeply nested components, but the workout planner is max 3-4 levels deep, which is manageable.

**Example:**
```csharp
// SetEditor.razor - Child component
@code {
    [Parameter] public SetLog Set { get; set; } = null!;
    [Parameter] public SetLog? PreviousSet { get; set; }     // Show last session's data
    [Parameter] public EventCallback<SetLog> OnSetUpdated { get; set; }
    [Parameter] public EventCallback OnSetDeleted { get; set; }

    private async Task UpdateReps(int reps)
    {
        Set.Reps = reps;
        await OnSetUpdated.InvokeAsync(Set);
    }
}

// SessionTracker.razor - Parent page (simplified)
<SetEditor Set="@currentSet"
           PreviousSet="@previousSet"
           OnSetUpdated="HandleSetUpdated"
           OnSetDeleted="HandleSetDeleted" />
```

### Pattern 2: Scoped Service as Session State

**What:** Register services as `Scoped` in DI. In Blazor Server, scoped = circuit-scoped = per-user-session. Use a scoped service to hold active workout session state that multiple components on the same page need.

**When to use:** The session tracker page, where multiple components (timer, set editors, exercise list) all need access to the current workout-in-progress.

**Trade-offs:** Simpler than Fluxor/Redux for this scale. The service outlives individual components but dies with the circuit. No cross-user leak risk since this is single-user.

**Example:**
```csharp
// Services/SessionService.cs
public class SessionService
{
    private WorkoutLog? _activeSession;

    public WorkoutLog? ActiveSession => _activeSession;
    public bool HasActiveSession => _activeSession is not null;

    public event Action? OnSessionChanged;

    public void StartSession(WorkoutTemplate template, ScheduledWorkout? scheduled)
    {
        _activeSession = new WorkoutLog
        {
            WorkoutTemplateId = template.Id,
            ScheduledWorkoutId = scheduled?.Id,
            Date = DateOnly.FromDateTime(DateTime.Today),
            ExerciseLogs = template.Items
                .Where(i => i.Section == TemplateSection.Working)
                .Select((item, idx) => new ExerciseLog
                {
                    ExerciseId = item.ExerciseId,
                    SortOrder = idx,
                    Status = ExerciseCompletionStatus.Skipped  // Default until logged
                }).ToList()
        };
        OnSessionChanged?.Invoke();
    }

    public async Task SaveSession(AppDbContext db)
    {
        if (_activeSession is null) return;
        db.WorkoutLogs.Add(_activeSession);
        await db.SaveChangesAsync();
        _activeSession = null;
        OnSessionChanged?.Invoke();
    }
}

// Program.cs registration
builder.Services.AddScoped<SessionService>();
```

### Pattern 3: Service Layer Encapsulation

**What:** Components never call `AppDbContext` directly. All data access goes through service classes that encapsulate queries, validation, and business rules.

**When to use:** Always. Every database interaction.

**Trade-offs:** Adds a layer of indirection but prevents query logic from scattering across `.razor` files. Makes unit testing possible (mock the service, not EF). Critical for progressive overload suggestions and PR detection, which involve non-trivial query logic.

**Example:**
```csharp
// Services/ExerciseService.cs
public class ExerciseService(AppDbContext db)
{
    public async Task<List<Exercise>> SearchAsync(
        string? query = null,
        ExerciseType? type = null,
        MuscleGroup? muscleGroup = null)
    {
        var q = db.Exercises.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
            q = q.Where(e => e.Name.Contains(query));

        if (type.HasValue)
            q = q.Where(e => e.Type == type.Value);

        if (muscleGroup.HasValue)
            q = q.OfType<StrengthExercise>()
                 .Where(e => e.PrimaryMuscleGroup == muscleGroup.Value);

        return await q.OrderBy(e => e.Name).ToListAsync();
    }
}
```

## Data Flow

### Template Creation Flow

```
[User builds template in TemplateBuilder page]
         |
         v
[TemplateBuilder.razor]
    |-- injects TemplateService
    |-- uses ExercisePicker to select exercises
    |-- uses ExerciseGroupEditor for superset/EMOM grouping
    |-- collects targets (sets/reps/weight OR distance/duration/pace)
         |
         v
[TemplateService.CreateAsync(template)]
    |-- validates: at least one exercise, no duplicate sort orders
    |-- saves WorkoutTemplate + TemplateItems to DB
         |
         v
[AppDbContext] --> [SQLite]
```

### Template to Schedule to Log Flow (Core Data Lifecycle)

```
WorkoutTemplate (blueprint)
         |
         | "Schedule this template"
         v
ScheduledWorkout (planned instance on a date)
    |-- linked to RecurrenceRule for repeating schedules
    |-- Status: Planned
         |
         | "Start workout" (user opens session tracker)
         v
WorkoutLog (actual session record)
    |-- mirrors template structure
    |-- records actual reps/weight or distance/duration
    |-- Status on ScheduledWorkout flips to Completed/Partial
         |
         | Per exercise:
         v
    ExerciseLog
        |-- SetLog (strength: N sets with reps + weight)
        |-- EnduranceLog (endurance: distance + duration + pace + HR)
```

### Session Tracking Flow (Real-Time Interaction)

```
[User opens SessionTracker page]
         |
         v
[SessionService.StartSession(template, scheduled)]
    |-- creates in-memory WorkoutLog from template
    |-- populates ExerciseLogs from TemplateItems
         |
         v
[SessionTracker.razor renders exercise list]
    |-- for each StrengthExercise: renders SetEditor components
    |       |-- shows previous session data inline (from AnalyticsService)
    |       |-- user taps through sets, entering reps + weight
    |       |-- EventCallback updates ExerciseLog in SessionService
    |
    |-- for each EnduranceExercise: renders EnduranceEditor + TimerControl
    |       |-- user starts timer, enters distance after
    |       |-- EventCallback updates ExerciseLog in SessionService
         |
         v
[User taps "Finish Workout"]
    |-- SessionService.SaveSession(db) persists the WorkoutLog
    |-- ScheduledWorkout.Status updated to Completed/Partial
    |-- navigates to summary view
```

### Analytics Query Flow

```
[Analytics page loads]
         |
         v
[AnalyticsService queries historical data]
    |-- Volume: SUM(SetLog.Reps * SetLog.WeightKg) GROUP BY week
    |-- PRs: MAX(SetLog.WeightKg) WHERE SetLog.Reps >= N, per exercise
    |-- Streaks: COUNT consecutive weeks with >= 1 WorkoutLog
    |-- Endurance: AVG pace, total distance per week
    |-- Adherence: COUNT(Completed) / COUNT(Planned) per period
         |
         v
[Analytics.razor renders charts via StatCard components]
```

### Key Data Flows

1. **Template-to-Session:** Template defines the plan; SessionService hydrates it into a live WorkoutLog with blank ExerciseLogs. The user fills in actuals. This is a one-directional copy -- editing the template later does not retroactively change logged sessions.

2. **Previous Performance Inline:** When starting a session, AnalyticsService queries the most recent WorkoutLog for the same template to show "last time you did 3x8 @ 20kg" next to each set input. This is the key UX differentiator for progressive overload.

3. **Recurrence Expansion:** RecurrenceRule defines patterns ("every Monday"). ScheduleService materializes these into concrete ScheduledWorkout rows for a rolling window (e.g., 4 weeks ahead). This avoids infinite row generation while keeping the calendar populated.

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| Single user (this app) | SQLite is sufficient. All data fits in memory. No caching needed. Scoped services are effectively singletons. |
| 1-5 years of data | Add indexes on date columns (ScheduledWorkout.ScheduledDate, WorkoutLog.Date). Consider yearly archiving of old logs if queries slow down. |
| Multi-user (future v2) | Replace SQLite with PostgreSQL. Add User FK to all root entities. Scoped services already isolate per-circuit. Add auth via ASP.NET Core Identity. |

### Scaling Priorities

1. **First bottleneck:** Analytics queries over large date ranges. Fix by adding computed/materialized views or caching weekly aggregates in a separate table.
2. **Second bottleneck:** Calendar page loading all scheduled workouts. Fix by only querying the visible date range (which the architecture already supports via date filtering).

## Anti-Patterns

### Anti-Pattern 1: Injecting DbContext into Components

**What people do:** `@inject AppDbContext Db` in `.razor` files, writing LINQ queries directly in `@code` blocks.
**Why it's wrong:** Scatters query logic across the UI layer. Makes testing impossible. Makes refactoring data access a nightmare. Violates separation of concerns.
**Do this instead:** Inject services. Services own all `DbContext` interactions. Components call service methods with clear semantic names like `SearchExercisesAsync()`.

### Anti-Pattern 2: Fat TemplateItem with Runtime Type Checking

**What people do:** Put all possible fields on `TemplateItem` and use `if (exercise is StrengthExercise)` checks everywhere in components to decide which fields to show.
**Why it's wrong:** Spreads type-awareness into every component. Adding a third exercise type (e.g., flexibility) requires touching every component.
**Do this instead:** Use polymorphic rendering. Create `StrengthTargetEditor` and `EnduranceTargetEditor` components. The parent checks the type once and renders the correct child. `TemplateItem` is allowed to have nullable fields for both types because it is a join entity, not a domain object -- the type semantics live on the `Exercise` hierarchy.

### Anti-Pattern 3: Storing Recurrence as Separate Rows Upfront

**What people do:** Generate a year of `ScheduledWorkout` rows when the user creates a recurrence rule.
**Why it's wrong:** Creates thousands of unused rows. Editing the recurrence means deleting and regenerating. Cancelling one instance is awkward.
**Do this instead:** Store the `RecurrenceRule` as a pattern. Materialize `ScheduledWorkout` rows only for a rolling window (4 weeks ahead). Regenerate on calendar load if the window has advanced. Individual instance overrides (skip, reschedule) are stored as explicit rows.

### Anti-Pattern 4: Cascading State for Everything

**What people do:** Use `CascadingParameter` to pass workout session state through the entire component tree.
**Why it's wrong:** Every child re-renders when the cascading value changes, even if they do not use the changed property. Performance degrades with many components on the session tracker page.
**Do this instead:** Use scoped `SessionService` with targeted event subscriptions. Components subscribe to the specific events they care about and call `StateHasChanged()` explicitly.

## Internal Boundaries

| Boundary | Communication | Notes |
|----------|---------------|-------|
| Page <-> Shared Components | Parameters + EventCallbacks | Strict unidirectional data flow |
| Page <-> Services | Injected via DI, async method calls | Services return domain objects, never IQueryable |
| Services <-> DbContext | Constructor-injected, async queries | Services own all query construction |
| Session state <-> Components | Event subscription on scoped service | Components call StateHasChanged on event |

## Build Order (Dependency Chain)

The components have clear dependency ordering. Build bottom-up:

| Phase | What to Build | Depends On | Unlocks |
|-------|---------------|------------|---------|
| 1 | Models + Enums + AppDbContext + Migrations | Nothing | Everything else |
| 2 | ExerciseService + ExerciseLibrary page | Phase 1 | Exercise catalog exists to reference |
| 3 | TemplateService + TemplateBuilder page + ExercisePicker | Phase 1, 2 | Workout blueprints exist |
| 4 | ScheduleService + Calendar page + WeekView/MonthView | Phase 1, 3 | Workouts can be planned on dates |
| 5 | SessionService + SessionTracker page + SetEditor + EnduranceEditor + TimerControl | Phase 1, 2, 3 | Workouts can be logged |
| 6 | AnalyticsService + Analytics page + StatCard | Phase 1, 5 | Progress tracking works |
| 7 | Progressive overload suggestions, RPE/notes, export | Phase 5, 6 | Polish features |

**Critical path:** Phases 1-5 are sequential dependencies. Phase 6 can start once Phase 5 produces log data. Phase 7 items are independent and can be built in any order.

**The Home page** should be built last (or evolved continuously) because "quick-start repeat last workout" depends on having templates, schedules, and session tracking all working.

## Sources

- [EF Core Inheritance - Microsoft Learn](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) -- Official TPH/TPT/TPC documentation (HIGH confidence)
- [Blazor Server State Management - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-10.0) -- Circuit-scoped services (HIGH confidence)
- [Blazor State Management Overview - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/?view=aspnetcore-10.0) -- State management patterns (HIGH confidence)
- [Fitness Tracking Database Schema - Back4App](https://www.back4app.com/tutorials/how-to-build-a-database-schema-for-a-fitness-tracking-application) -- Exercise/Workout entity patterns (MEDIUM confidence)
- [Blazor DI Best Practices - Telerik](https://www.telerik.com/blogs/blazor-basics-dependency-injection-best-practices-use-cases) -- Scoped service patterns (MEDIUM confidence)
- [Discriminator Column Guide - TheCodeMan](https://thecodeman.net/posts/discriminator-column-efcore-quick-guide) -- TPH discriminator configuration (MEDIUM confidence)

---
*Architecture research for: Unified Workout Planner (Blazor Server)*
*Researched: 2026-03-21*
