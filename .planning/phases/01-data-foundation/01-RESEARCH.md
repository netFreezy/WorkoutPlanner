# Phase 1: Data Foundation - Research

**Researched:** 2026-03-21
**Domain:** EF Core with SQLite on .NET 10, TPH inheritance, workout domain data modeling
**Confidence:** HIGH

## Summary

Phase 1 establishes the complete data model and persistence layer for a workout planning app using EF Core 10 with SQLite. The project is a blank Blazor Server scaffold (.NET 10.0.103 SDK) with no existing data layer. All entities, DbContext, migrations, and NuGet packages must be created from scratch.

The data model involves a TPH inheritance hierarchy for Exercise (StrengthExercise/EnduranceExercise), a WorkoutTemplate with ordered TemplateItems and grouping constructs (superset/EMOM), scheduling with RecurrenceRule, and workout logging with planned-vs-actual separation. EF Core 10.0.5 with SQLite is the target, using IDbContextFactory for Blazor Server thread safety.

**Primary recommendation:** Use EF Core 10.0.5 with SQLite, IDbContextFactory pattern, TPH inheritance with string discriminator, [Flags] enum for DaysOfWeek, and separate flat entity tables (no JSON columns) to avoid the SQLite JSON complexity concern flagged in STATE.md.

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** TPH inheritance -- base `Exercise` with `StrengthExercise` and `EnduranceExercise` subtypes
- **D-02:** Single muscle group per exercise as an enum (e.g. Chest, Back, Shoulders, Legs, Arms, Core, FullBody)
- **D-03:** Single equipment per exercise as an enum (e.g. Barbell, Dumbbell, Bodyweight, Cable, Machine, Band, Kettlebell)
- **D-04:** Endurance exercises use an activity type enum (Run, Cycle, Swim, Row, etc.) -- no deeper subtype hierarchy
- **D-05:** Base Exercise fields: Name, Description, CreatedDate (plus EF-managed Id and discriminator)
- **D-06:** Separate `TemplateGroup` entity (not a nullable GroupId on TemplateItem)
- **D-07:** TemplateGroup carries GroupType (Superset/EMOM) and EMOM-specific fields (Rounds, MinuteWindow) directly
- **D-08:** Flat grouping only -- no nesting of groups within groups
- **D-09:** Single global sort order across the template -- TemplateItems have a Position int, groups are contiguous runs in that ordering
- **D-10:** Structured columns on RecurrenceRule: FrequencyType enum (Daily/Weekly/Custom), Interval (every N), DaysOfWeek (flags or comma-separated)
- **D-11:** Recurrence runs forever until manually deleted -- no end date or occurrence count
- **D-12:** Fixed 4-week materialization window for generating concrete ScheduledWorkout rows
- **D-13:** Editing a recurrence rule regenerates all future materialized rows -- no "edit just this one" support
- **D-14:** Deep copy into log tables -- when a session starts, each exercise's targets are copied into log rows with both planned and actual columns
- **D-15:** Strength snapshot fields: planned sets, planned reps, planned weight (actual columns filled during logging)
- **D-16:** Endurance snapshot fields: planned distance, planned duration, planned pace, planned HR zone, plus activity type (actual columns filled during logging)

### Claude's Discretion
- Exact enum values for muscle groups, equipment, and activity types
- Previous performance lookup strategy (query at render time vs. caching)
- Value converter approach for any SQLite type limitations
- Migration naming and organization
- DbContext configuration details (fluent API vs. data annotations)
- Seed data structure (Phase 2 seeds the exercises, Phase 1 just ensures the model supports it)

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope.

</user_constraints>

<phase_requirements>

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| DATA-01 | EF Core with SQLite using `IDbContextFactory` pattern (not scoped DbContext) | IDbContextFactory is the official Microsoft pattern for Blazor Server. Register with `AddDbContextFactory`, use `CreateDbContext()` with `using` statements. EF Core 10.0.5 + SQLite confirmed. |
| DATA-02 | Exercise entity with TPH inheritance -- StrengthExercise and EnduranceExercise subtypes with type-specific metadata | TPH is EF Core's default strategy. Use `HasDiscriminator<string>` with `HasValue<T>`. Subtype columns are automatically nullable in the database. |
| DATA-03 | WorkoutTemplate with ordered TemplateItems supporting strength and endurance targets | Standard parent-child relationship with `Position` int for ordering. TemplateItem has FK to Exercise and nullable target fields for both types. |
| DATA-04 | Superset and EMOM grouping constructs within templates | Separate `TemplateGroup` entity with GroupType enum. TemplateItems reference their group via FK. Groups are contiguous runs in Position ordering. |
| DATA-05 | Warm-up and cool-down block sections in templates | `SectionType` enum (WarmUp/Working/CoolDown) on TemplateItem. Excluded from volume stats in analytics (Phase 6 concern, but column must exist now). |
| DATA-06 | ScheduledWorkout entity with date, status, and template snapshot | Entity with Date, Status enum (Planned/Completed/Skipped), FK to WorkoutTemplate. Template snapshot is achieved via deep copy at session start (DATA-08). |
| DATA-07 | RecurrenceRule support -- every X days, specific weekdays, every other day | Structured columns: FrequencyType enum, Interval int, DaysOfWeek as [Flags] enum stored as integer. No end date per D-11. |
| DATA-08 | WorkoutLog with planned-vs-actual separation -- snapshot planned targets at session creation | WorkoutLog has FK to ScheduledWorkout. LogEntry rows (SetLog/EnduranceLog) carry both planned and actual columns. Deep copy at session start. |
| DATA-09 | Strength log entries: actual sets with reps and weight per set, set type | SetLog entity with PlannedReps, PlannedWeight, ActualReps, ActualWeight, SetType enum (WarmUp/Working/Failure/Drop). FK to WorkoutLog + Exercise. |
| DATA-10 | Endurance log entries: actual distance, duration, pace, optional HR data | EnduranceLog entity with planned + actual columns for Distance, Duration, Pace, HeartRateZone, ActivityType. FK to WorkoutLog + Exercise. |

</phase_requirements>

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.5 | SQLite database provider for EF Core | Official Microsoft provider, matches .NET 10 target framework |
| Microsoft.EntityFrameworkCore.Design | 10.0.5 | Design-time services for migrations CLI | Required for `dotnet ef migrations add/update` commands |
| Microsoft.EntityFrameworkCore.Tools | 10.0.5 | PMC tools (optional, CLI preferred) | Only needed if using Visual Studio Package Manager Console |

### Supporting (Test Project)

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| xunit | 2.9.3 | Test framework | Unit/integration tests for data model round-trips |
| xunit.runner.visualstudio | 2.9.3 | Test runner for VS/CLI | Discovering and running tests |
| Microsoft.NET.Test.Sdk | 18.3.0 | Test platform infrastructure | Required for `dotnet test` |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.5 | SQLite in-memory for tests | In-memory SQLite via `DataSource=:memory:` for fast, isolated tests |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| SQLite | SQL Server LocalDB | Heavier, requires SQL Server install. SQLite is correct per project constraints. |
| xunit | NUnit/MSTest | All viable. xunit is most common in .NET ecosystem for new projects. |
| Fluent API | Data Annotations | Data annotations are simpler for basic config but insufficient for TPH discriminator, value converters, and complex relationships. Use fluent API exclusively for consistency. |

**Installation:**
```bash
# Main project
dotnet add BlazorApp2.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.5
dotnet add BlazorApp2.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.5

# Test project (create first)
dotnet new xunit -n BlazorApp2.Tests --framework net10.0
dotnet add BlazorApp2.Tests/BlazorApp2.Tests.csproj package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.5
dotnet add BlazorApp2.Tests/BlazorApp2.Tests.csproj reference BlazorApp2.csproj
dotnet sln BlazorApp2.slnx add BlazorApp2.Tests/BlazorApp2.Tests.csproj
```

**Version verification:** All versions verified against NuGet registry on 2026-03-21. EF Core 10.0.5 is the latest stable release. xunit 2.9.3 is the latest stable. Microsoft.NET.Test.Sdk 18.3.0 is the latest stable.

## Architecture Patterns

### Recommended Project Structure

```
BlazorApp2/
  Data/
    AppDbContext.cs              # DbContext with DbSet declarations and OnModelCreating
    Entities/
      Exercise.cs               # Base class + StrengthExercise + EnduranceExercise
      WorkoutTemplate.cs        # Template + TemplateItem + TemplateGroup
      ScheduledWorkout.cs       # ScheduledWorkout + RecurrenceRule
      WorkoutLog.cs             # WorkoutLog + SetLog + EnduranceLog
    Enums/
      MuscleGroup.cs            # Enum: Chest, Back, Shoulders, etc.
      Equipment.cs              # Enum: Barbell, Dumbbell, Bodyweight, etc.
      ActivityType.cs           # Enum: Run, Cycle, Swim, Row, etc.
      FrequencyType.cs          # Enum: Daily, Weekly, Custom
      DaysOfWeek.cs             # [Flags] enum for weekday selection
      WorkoutStatus.cs          # Enum: Planned, Completed, Skipped
      SetType.cs                # Enum: WarmUp, Working, Failure, Drop
      GroupType.cs              # Enum: Superset, EMOM
      SectionType.cs            # Enum: WarmUp, Working, CoolDown
    Configurations/
      ExerciseConfiguration.cs  # IEntityTypeConfiguration<Exercise> with TPH setup
      TemplateConfiguration.cs  # Template + items + groups fluent config
      ScheduleConfiguration.cs  # ScheduledWorkout + RecurrenceRule config
      LogConfiguration.cs       # WorkoutLog + SetLog + EnduranceLog config
  Migrations/                   # Auto-generated by EF Core CLI
```

### Pattern 1: IDbContextFactory Registration and Usage

**What:** Register DbContextFactory in DI, create short-lived DbContext instances per operation.
**When to use:** All database access in Blazor Server components.
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0

// Program.cs registration
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// appsettings.json
// "ConnectionStrings": { "DefaultConnection": "Data Source=workoutplanner.db" }

// Component usage -- short-lived per operation (PREFERRED)
@inject IDbContextFactory<AppDbContext> DbFactory

private async Task LoadExercises()
{
    using var context = DbFactory.CreateDbContext();
    exercises = await context.Exercises.ToListAsync();
}

// Component usage -- lifetime-scoped (for change tracking)
@implements IDisposable
@inject IDbContextFactory<AppDbContext> DbFactory

@code {
    private AppDbContext? Context;

    protected override void OnInitialized()
    {
        Context = DbFactory.CreateDbContext();
    }

    public void Dispose() => Context?.Dispose();
}
```

### Pattern 2: TPH Inheritance with Explicit Discriminator

**What:** Map Exercise hierarchy to a single table with string discriminator column.
**When to use:** Exercise entity hierarchy.
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/ef/core/modeling/inheritance

// Entity classes
public abstract class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public class StrengthExercise : Exercise
{
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
}

public class EnduranceExercise : Exercise
{
    public ActivityType ActivityType { get; set; }
}

// Fluent API configuration
modelBuilder.Entity<Exercise>()
    .HasDiscriminator<string>("ExerciseType")
    .HasValue<StrengthExercise>("Strength")
    .HasValue<EnduranceExercise>("Endurance");

// Querying derived types
using var context = DbFactory.CreateDbContext();
var strengthExercises = await context.Set<StrengthExercise>()
    .Where(e => e.MuscleGroup == MuscleGroup.Chest)
    .ToListAsync();
```

### Pattern 3: IEntityTypeConfiguration for Clean Model Building

**What:** Separate fluent API configuration into dedicated classes.
**When to use:** All entity configurations -- keeps OnModelCreating clean.
**Example:**
```csharp
// Source: EF Core standard pattern

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasDiscriminator<string>("ExerciseType")
            .HasValue<StrengthExercise>("Strength")
            .HasValue<EnduranceExercise>("Endurance");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Name);
    }
}

// In AppDbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

### Pattern 4: [Flags] Enum for DaysOfWeek

**What:** Store multiple weekday selections as a single integer using bitwise flags.
**When to use:** RecurrenceRule.DaysOfWeek property.
**Example:**
```csharp
// Source: https://medium.com/@josiahmahachi/using-powers-of-2-to-store-flags-in-entity-framework-23f7315c36d5

[Flags]
public enum DaysOfWeek
{
    None      = 0,
    Monday    = 1,
    Tuesday   = 2,
    Wednesday = 4,
    Thursday  = 8,
    Friday    = 16,
    Saturday  = 32,
    Sunday    = 64,
    Weekdays  = Monday | Tuesday | Wednesday | Thursday | Friday,
    Weekend   = Saturday | Sunday,
    EveryDay  = Weekdays | Weekend
}

// Usage: store "Monday, Wednesday, Friday" as integer 21 (1 + 4 + 16)
var rule = new RecurrenceRule
{
    FrequencyType = FrequencyType.Weekly,
    Interval = 1,
    DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Wednesday | DaysOfWeek.Friday
};

// EF Core stores this as integer 21 automatically -- no value converter needed
// Querying with HasFlag:
var rules = await context.RecurrenceRules
    .Where(r => r.DaysOfWeek.HasFlag(DaysOfWeek.Monday))
    .ToListAsync();
```

### Pattern 5: Ordered Items with Position Column

**What:** Use an integer Position column for explicit ordering within a parent.
**When to use:** TemplateItems within a WorkoutTemplate.
**Example:**
```csharp
public class TemplateItem
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Position { get; set; }  // Global sort order within template
    public SectionType SectionType { get; set; } = SectionType.Working;

    // Grouping (optional)
    public int? TemplateGroupId { get; set; }
    public TemplateGroup? TemplateGroup { get; set; }

    // Strength targets (nullable -- only for strength exercises)
    public int? TargetSets { get; set; }
    public int? TargetReps { get; set; }
    public decimal? TargetWeight { get; set; }

    // Endurance targets (nullable -- only for endurance exercises)
    public decimal? TargetDistance { get; set; }
    public TimeSpan? TargetDuration { get; set; }
    public decimal? TargetPace { get; set; }
    public int? TargetHeartRateZone { get; set; }
}

// Query ordered
var items = await context.TemplateItems
    .Where(ti => ti.WorkoutTemplateId == templateId)
    .OrderBy(ti => ti.Position)
    .Include(ti => ti.Exercise)
    .Include(ti => ti.TemplateGroup)
    .ToListAsync();
```

### Anti-Patterns to Avoid

- **Scoped DbContext in Blazor Server:** Never use `AddDbContext` (scoped lifetime). Blazor circuits outlive HTTP request scopes. Always use `AddDbContextFactory`.
- **Concurrent DbContext access:** DbContext is not thread-safe. Never share a DbContext instance across multiple async operations simultaneously. Use `using var context = DbFactory.CreateDbContext()` per operation.
- **JSON columns for structured data on SQLite:** While EF Core 8+ supports JSON on SQLite, mixing JSON with TPH inheritance creates complexity. Use flat relational columns instead -- they're simpler to query, index, and migrate.
- **Shared `Cache=Shared` with WAL:** Mixing shared-cache mode and WAL is discouraged. Omit `Cache=Shared` when using WAL mode.
- **`decimal` on SQLite without converter:** SQLite stores decimal as TEXT. Use `double` for numeric precision that doesn't require exact decimal arithmetic, or add a value converter to `double` for decimal properties.
- **Forgetting `HasBaseType` registration:** If you don't expose a DbSet for derived types, you must register them explicitly in OnModelCreating or they won't be included in the model.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Database migrations | Manual SQL scripts | `dotnet ef migrations add/update` | EF Core generates migration code from model diffs, handles SQLite rebuild pattern |
| Connection pooling | Custom pool manager | `AddDbContextFactory` + EF Core's pooling | Factory manages instance creation; SQLite connection pooling is handled by Microsoft.Data.Sqlite |
| Discriminator mapping | Manual type column checks | `HasDiscriminator<T>().HasValue<T>()` | EF Core auto-filters by discriminator in queries, handles materialization |
| Enum storage | String parsing/serialization | EF Core default int mapping (or `HasConversion<string>()`) | Built-in converter handles enum-to-int/string round-trip |
| Flags enum bitmask | Custom bitfield logic | `[Flags]` attribute + integer storage | .NET runtime handles bitwise operations; EF stores as integer natively |
| Ordered collections | Manual sort indices | `Position` int column + `OrderBy` | Simple, explicit, no framework magic needed |
| WAL mode | Manual PRAGMA commands | EF Core SQLite provider sets WAL by default on new databases | WAL is the default journal mode when EF Core creates the database |

**Key insight:** EF Core 10 with SQLite handles most concerns out of the box. The main areas requiring manual attention are: (1) IEntityTypeConfiguration classes for clean fluent API setup, (2) the `decimal` to `double` value converter for SQLite compatibility, and (3) ensuring all derived types are registered in the model.

## Common Pitfalls

### Pitfall 1: Blazor Server DbContext Threading Violations
**What goes wrong:** Using `AddDbContext` (scoped) instead of `AddDbContextFactory` causes shared DbContext across concurrent render cycles in the same circuit. Results in `InvalidOperationException` or data corruption.
**Why it happens:** Blazor Server circuits have a long-lived scope that persists across multiple user interactions. Scoped services are shared within that scope.
**How to avoid:** Always use `AddDbContextFactory<AppDbContext>`. Create and dispose DbContext instances per operation using `using var context = DbFactory.CreateDbContext()`.
**Warning signs:** Random `InvalidOperationException: A second operation was started on this context instance before a previous operation completed` errors.

### Pitfall 2: TPH Nullable Columns Surprise
**What goes wrong:** Expecting subtype-specific columns (like MuscleGroup on StrengthExercise) to be non-nullable in the database. EF Core forces all TPH subtype columns to be nullable because the table stores all types.
**Why it happens:** A StrengthExercise row won't have ActivityType, and an EnduranceExercise row won't have MuscleGroup -- so both must be nullable in the single table.
**How to avoid:** Accept nullable database columns. Enforce non-null at the C# model level (required properties on derived types). The discriminator ensures correct type materialization.
**Warning signs:** Migrations generate nullable columns even when the C# property is required. This is correct behavior.

### Pitfall 3: SQLite Decimal Type Limitation
**What goes wrong:** Using `decimal` properties causes client-side evaluation of comparisons and ordering in queries. SQLite stores decimal as TEXT.
**Why it happens:** SQLite has limited numeric type support. Decimal is not a native SQLite type.
**How to avoid:** Use `double` for weight, distance, pace values. The precision difference is irrelevant for workout data. If decimal is needed, add `.HasConversion<double>()` in fluent configuration.
**Warning signs:** EF Core warnings about client-side evaluation. LINQ queries with `.OrderBy()` on decimal columns being slow.

### Pitfall 4: SQLite TimeSpan Limitations
**What goes wrong:** TimeSpan comparison/ordering requires client evaluation on SQLite.
**Why it happens:** SQLite does not have a native time data type.
**How to avoid:** Store durations as `int` (total seconds) or `double` (minutes) instead of `TimeSpan`. Add a computed C# property for convenience. Alternatively, use TimeSpan but accept that ordering queries will evaluate client-side.
**Warning signs:** Warnings about client-side evaluation for queries filtering/sorting by duration.

### Pitfall 5: Forgetting to Register Derived Types
**What goes wrong:** Derived entity types silently excluded from the model if not registered via DbSet or fluent API.
**Why it happens:** EF Core does not auto-scan for derived types by convention.
**How to avoid:** Either expose `DbSet<StrengthExercise>` and `DbSet<EnduranceExercise>` on the DbContext, or configure them in `HasDiscriminator().HasValue<T>()`. The discriminator configuration implicitly registers them.
**Warning signs:** Queries for derived types return empty results. Migrations don't include subtype columns.

### Pitfall 6: SQLite Migration Lock Table
**What goes wrong:** A failed migration leaves the `__EFMigrationsLock` table in a locked state, preventing all subsequent migrations.
**Why it happens:** EF Core 9+ added concurrent migration protection. If migration fails non-recoverably, the lock row persists.
**How to avoid:** Be aware of this mechanism. If migrations hang, manually delete the `__EFMigrationsLock` table from the SQLite database using a tool like DB Browser for SQLite.
**Warning signs:** `dotnet ef database update` hangs indefinitely.

### Pitfall 7: DateTimeOffset on SQLite
**What goes wrong:** Using `DateTimeOffset` properties causes issues with comparisons and ordering.
**Why it happens:** SQLite does not natively support DateTimeOffset.
**How to avoid:** Use `DateTime` (UTC) throughout. Store all dates as UTC. Convert to local time only in the UI layer.
**Warning signs:** Query comparison/ordering warnings from EF Core.

## Code Examples

### AppDbContext Setup
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0

using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<StrengthExercise> StrengthExercises => Set<StrengthExercise>();
    public DbSet<EnduranceExercise> EnduranceExercises => Set<EnduranceExercise>();
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<TemplateItem> TemplateItems => Set<TemplateItem>();
    public DbSet<TemplateGroup> TemplateGroups => Set<TemplateGroup>();
    public DbSet<ScheduledWorkout> ScheduledWorkouts => Set<ScheduledWorkout>();
    public DbSet<RecurrenceRule> RecurrenceRules => Set<RecurrenceRule>();
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<SetLog> SetLogs => Set<SetLog>();
    public DbSet<EnduranceLog> EnduranceLogs => Set<EnduranceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

### Program.cs Registration
```csharp
// Add after existing services, before var app = builder.Build();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### appsettings.json Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=workoutplanner.db"
  }
}
```

### Entity Hierarchy -- Exercise
```csharp
public abstract class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public class StrengthExercise : Exercise
{
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
}

public class EnduranceExercise : Exercise
{
    public ActivityType ActivityType { get; set; }
}
```

### RecurrenceRule Entity
```csharp
public class RecurrenceRule
{
    public int Id { get; set; }
    public int ScheduledWorkoutId { get; set; }
    public ScheduledWorkout ScheduledWorkout { get; set; } = null!;

    public FrequencyType FrequencyType { get; set; }
    public int Interval { get; set; } = 1;  // Every N (days/weeks)
    public DaysOfWeek DaysOfWeek { get; set; } = DaysOfWeek.None;
}
```

### WorkoutLog with Planned-vs-Actual
```csharp
public class SetLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public SetType SetType { get; set; } = SetType.Working;

    // Planned (snapshot at session creation)
    public int? PlannedReps { get; set; }
    public double? PlannedWeight { get; set; }

    // Actual (filled during logging)
    public int? ActualReps { get; set; }
    public double? ActualWeight { get; set; }
    public bool IsCompleted { get; set; }
}

public class EnduranceLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public ActivityType ActivityType { get; set; }

    // Planned
    public double? PlannedDistance { get; set; }    // km
    public int? PlannedDurationSeconds { get; set; }
    public double? PlannedPace { get; set; }        // min/km
    public int? PlannedHeartRateZone { get; set; }

    // Actual
    public double? ActualDistance { get; set; }
    public int? ActualDurationSeconds { get; set; }
    public double? ActualPace { get; set; }
    public int? ActualHeartRateZone { get; set; }
    public bool IsCompleted { get; set; }
}
```

### SQLite In-Memory Test Pattern
```csharp
// Source: https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class DataTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly AppDbContext Context;

    public DataTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `AddDbContext` (scoped) in Blazor | `AddDbContextFactory` | .NET 5 / EF Core 5 | Required for Blazor Server thread safety |
| InMemory provider for testing | SQLite in-memory (`:memory:`) | EF Core 7+ recommendation | InMemory doesn't enforce relational constraints; SQLite provides realistic testing |
| JSON columns only on SQL Server | JSON columns on SQLite too | EF Core 8 | SQLite now supports `ToJson()` but flat columns remain simpler for this project |
| Manual WAL mode via PRAGMA | EF Core SQLite sets WAL by default | EF Core (all recent versions) | No manual PRAGMA needed -- WAL is set automatically when EF creates the database |
| Sequences for TPC key generation | Not available on SQLite | Always | SQLite does not support sequences -- TPH (single table) avoids this issue entirely |

**Deprecated/outdated:**
- `UseInMemoryDatabase()` for testing: Still functional but officially discouraged in favor of SQLite in-memory for relational model testing
- `DbContext.Database.Migrate()` at startup: Viable for development but EF Core team recommends CLI-based migrations for production reliability

## Enum Value Recommendations (Claude's Discretion)

Based on common workout applications and the domain:

### MuscleGroup
```csharp
public enum MuscleGroup
{
    Chest,
    Back,
    Shoulders,
    Legs,
    Arms,
    Core,
    FullBody
}
```

### Equipment
```csharp
public enum Equipment
{
    Barbell,
    Dumbbell,
    Bodyweight,
    Cable,
    Machine,
    Band,
    Kettlebell,
    Other
}
```

### ActivityType
```csharp
public enum ActivityType
{
    Run,
    Cycle,
    Swim,
    Row,
    Walk,
    Hike,
    Elliptical,
    StairClimber,
    Other
}
```

### Recommendation on `decimal` vs `double`

Use `double` for all numeric values (weight, distance, pace). Rationale:
- SQLite does not natively support `decimal`; EF Core stores it as TEXT and requires client evaluation for ordering/comparison
- Workout data does not require exact decimal arithmetic (financial precision)
- `double` maps natively to SQLite REAL, enabling server-side query evaluation
- No value converter needed

### Recommendation on Duration Storage

Store durations as `int` (total seconds) rather than `TimeSpan`. Rationale:
- SQLite does not support TimeSpan natively
- Integer seconds enable full server-side comparison and ordering
- Add convenience C# properties: `public TimeSpan Duration => TimeSpan.FromSeconds(DurationSeconds);`

### Recommendation on Configuration Approach

Use fluent API exclusively (via `IEntityTypeConfiguration<T>` classes). Rationale:
- Data annotations cannot configure TPH discriminators
- Data annotations cannot configure value converters
- Mixing approaches creates inconsistency
- Separate configuration files keep entity classes clean

### STATE.md Concern Resolution: JSON on SQLite

The STATE.md flags: "EF Core complex types with JSON on SQLite (polymorphic) may need value converter workaround."

**Resolution:** This concern is avoided entirely. The data model uses flat relational columns, not JSON columns. All properties are stored as standard columns:
- Exercise subtype properties (MuscleGroup, Equipment, ActivityType) are nullable columns in the TPH table
- RecurrenceRule uses structured columns (FrequencyType, Interval, DaysOfWeek) not JSON
- Log entries use explicit planned/actual column pairs

No JSON columns, no complex types, no value converter workarounds needed for polymorphic storage. The concern from STATE.md can be marked as resolved.

## Open Questions

1. **RecurrenceRule relationship direction**
   - What we know: RecurrenceRule needs to relate to something that generates ScheduledWorkout rows. A WorkoutTemplate can have a recurrence attached.
   - What's unclear: Should RecurrenceRule be a child of WorkoutTemplate (one template can have one recurrence) or should ScheduledWorkout reference a RecurrenceRule (decoupling template from schedule)?
   - Recommendation: Make RecurrenceRule a child of a "ScheduleSeries" or directly on a parent ScheduledWorkout that acts as a series anchor. The simplest approach: RecurrenceRule is a separate entity with FK to WorkoutTemplate, and materialized ScheduledWorkout rows reference both the template and the recurrence rule they were generated from.

2. **TemplateGroup Position semantics**
   - What we know: Groups are contiguous runs of TemplateItems in Position ordering (D-09). TemplateGroup has GroupType, Rounds, MinuteWindow.
   - What's unclear: Does TemplateGroup itself need a Position, or is its position derived from its first member's Position?
   - Recommendation: No Position on TemplateGroup. A group's position is implicitly defined by the minimum Position of its member TemplateItems. This avoids dual ordering that can get out of sync.

3. **WorkoutLog session lifecycle**
   - What we know: Deep copy at session start (D-14). Planned columns are snapshots.
   - What's unclear: Does WorkoutLog need a StartedAt/CompletedAt timestamp? Status tracking (InProgress/Completed)?
   - Recommendation: Add `StartedAt` (DateTime, set on creation), `CompletedAt` (DateTime?, set on finish), and `Notes` (string?) to WorkoutLog. This supports SESS-07 (RPE), SESS-08 (notes), and SESS-09 (incremental persistence).

## Validation Architecture

### Test Framework

| Property | Value |
|----------|-------|
| Framework | xunit 2.9.3 |
| Config file | None -- Wave 0 will create test project |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "Category=Data" --no-build` |
| Full suite command | `dotnet test BlazorApp2.Tests` |

### Phase Requirements to Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| DATA-01 | DbContextFactory creates and disposes contexts without error | integration | `dotnet test --filter "FullyQualifiedName~DbContextFactoryTests"` | -- Wave 0 |
| DATA-02 | TPH Exercise hierarchy round-trips (insert Strength, query as Exercise) | integration | `dotnet test --filter "FullyQualifiedName~ExerciseHierarchyTests"` | -- Wave 0 |
| DATA-03 | WorkoutTemplate with ordered TemplateItems persists and loads in order | integration | `dotnet test --filter "FullyQualifiedName~TemplateItemOrderTests"` | -- Wave 0 |
| DATA-04 | TemplateGroup with superset and EMOM types persists with members | integration | `dotnet test --filter "FullyQualifiedName~TemplateGroupTests"` | -- Wave 0 |
| DATA-05 | TemplateItem with SectionType (WarmUp/Working/CoolDown) persists | integration | `dotnet test --filter "FullyQualifiedName~SectionTypeTests"` | -- Wave 0 |
| DATA-06 | ScheduledWorkout with status enum round-trips | integration | `dotnet test --filter "FullyQualifiedName~ScheduledWorkoutTests"` | -- Wave 0 |
| DATA-07 | RecurrenceRule with FrequencyType, Interval, DaysOfWeek flags persists | integration | `dotnet test --filter "FullyQualifiedName~RecurrenceRuleTests"` | -- Wave 0 |
| DATA-08 | WorkoutLog with planned-vs-actual columns round-trips | integration | `dotnet test --filter "FullyQualifiedName~WorkoutLogTests"` | -- Wave 0 |
| DATA-09 | SetLog with planned/actual reps/weight and SetType persists | integration | `dotnet test --filter "FullyQualifiedName~SetLogTests"` | -- Wave 0 |
| DATA-10 | EnduranceLog with planned/actual distance/duration/pace/HR persists | integration | `dotnet test --filter "FullyQualifiedName~EnduranceLogTests"` | -- Wave 0 |

### Sampling Rate

- **Per task commit:** `dotnet test BlazorApp2.Tests --no-build -v quiet`
- **Per wave merge:** `dotnet test BlazorApp2.Tests`
- **Phase gate:** Full suite green + `dotnet ef database update` on fresh database succeeds

### Wave 0 Gaps

- [ ] `BlazorApp2.Tests/BlazorApp2.Tests.csproj` -- test project does not exist yet
- [ ] `BlazorApp2.Tests/DataTestBase.cs` -- shared SQLite in-memory test fixture
- [ ] `BlazorApp2.Tests/ExerciseHierarchyTests.cs` -- covers DATA-02
- [ ] `BlazorApp2.Tests/TemplateTests.cs` -- covers DATA-03, DATA-04, DATA-05
- [ ] `BlazorApp2.Tests/ScheduleTests.cs` -- covers DATA-06, DATA-07
- [ ] `BlazorApp2.Tests/LogTests.cs` -- covers DATA-08, DATA-09, DATA-10
- [ ] `BlazorApp2.Tests/DbContextFactoryTests.cs` -- covers DATA-01
- [ ] Test infrastructure: `dotnet new xunit` + NuGet references to EF Core SQLite
- [ ] Solution file update: `dotnet sln add BlazorApp2.Tests`

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: Blazor with EF Core (ASP.NET Core 10.0)](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0) -- IDbContextFactory pattern, component usage, SQLite configuration
- [Microsoft Learn: EF Core Inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) -- TPH discriminator configuration, nullable columns, shared columns
- [Microsoft Learn: SQLite Provider Limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations) -- Migration support table, decimal/TimeSpan/DateTimeOffset limitations, workarounds
- [Microsoft Learn: EF Core Value Conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) -- Enum converters, custom converters
- [NuGet: Microsoft.EntityFrameworkCore.Sqlite 10.0.5](https://www.nuget.org/packages/microsoft.entityframeworkcore.sqlite) -- Version verified 2026-03-21
- [NuGet: Microsoft.EntityFrameworkCore.Design 10.0.5](https://www.nuget.org/packages/microsoft.entityframeworkcore.design/) -- Version verified 2026-03-21
- [GitHub Issue #36513: SQLite default journal mode](https://github.com/dotnet/efcore/issues/36513) -- Confirmed WAL is default when EF Core creates SQLite databases

### Secondary (MEDIUM confidence)
- [EF Core TPH Explained (learnentityframeworkcore.com)](https://www.learnentityframeworkcore.com/inheritance/table-per-hierarchy) -- TPH nullable columns explanation
- [Using Powers of 2 for Flags in EF (Medium)](https://medium.com/@josiahmahachi/using-powers-of-2-to-store-flags-in-entity-framework-23f7315c36d5) -- [Flags] enum bitmask storage pattern
- [Microsoft Learn: Testing without production database](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database) -- SQLite in-memory test patterns

### Tertiary (LOW confidence)
- None -- all findings verified with official sources.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all packages verified on NuGet, versions confirmed current, official Microsoft docs consulted
- Architecture: HIGH -- IDbContextFactory pattern from official Blazor EF Core docs, TPH from EF Core inheritance docs, all patterns verified
- Pitfalls: HIGH -- SQLite limitations from official provider docs, threading from official Blazor docs, WAL default confirmed via GitHub issue

**Research date:** 2026-03-21
**Valid until:** 2026-04-21 (stable -- EF Core 10 is current LTS-compatible release)
