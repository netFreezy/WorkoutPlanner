---
phase: 01-data-foundation
plan: 01
subsystem: database
tags: [ef-core, sqlite, tph-inheritance, blazor-server, entity-framework, migrations]

# Dependency graph
requires:
  - phase: none
    provides: blank Blazor Server scaffold
provides:
  - EF Core data model with TPH Exercise hierarchy (Strength/Endurance)
  - WorkoutTemplate with ordered TemplateItems and grouping constructs (superset/EMOM)
  - ScheduledWorkout with RecurrenceRule (frequency, interval, day-of-week flags)
  - WorkoutLog with planned-vs-actual SetLog and EnduranceLog
  - AppDbContext with 11 DbSets registered via IDbContextFactory
  - InitialCreate migration applied to SQLite
affects: [02-exercise-library, 03-templates, 04-calendar, 05-sessions, 06-analytics]

# Tech tracking
tech-stack:
  added: [Microsoft.EntityFrameworkCore.Sqlite 10.0.5, Microsoft.EntityFrameworkCore.Design 10.0.5]
  patterns: [IDbContextFactory for Blazor Server, TPH inheritance with string discriminator, fluent API configurations via IEntityTypeConfiguration, flags enum for DaysOfWeek]

key-files:
  created:
    - Data/AppDbContext.cs
    - Data/Entities/Exercise.cs
    - Data/Entities/WorkoutTemplate.cs
    - Data/Entities/ScheduledWorkout.cs
    - Data/Entities/WorkoutLog.cs
    - Data/Enums/MuscleGroup.cs
    - Data/Enums/Equipment.cs
    - Data/Enums/ActivityType.cs
    - Data/Enums/FrequencyType.cs
    - Data/Enums/DaysOfWeek.cs
    - Data/Enums/WorkoutStatus.cs
    - Data/Enums/SetType.cs
    - Data/Enums/GroupType.cs
    - Data/Enums/SectionType.cs
    - Data/Configurations/ExerciseConfiguration.cs
    - Data/Configurations/TemplateConfiguration.cs
    - Data/Configurations/ScheduleConfiguration.cs
    - Data/Configurations/LogConfiguration.cs
    - Migrations/20260321151236_InitialCreate.cs
  modified:
    - BlazorApp2.csproj
    - Program.cs
    - appsettings.json

key-decisions:
  - "Used IDbContextFactory (not scoped AddDbContext) for Blazor Server thread safety per DATA-01"
  - "TPH with string discriminator ('Strength'/'Endurance') for Exercise hierarchy per D-01"
  - "DaysOfWeek stored as integer flags via [Flags] enum -- no value converter needed per D-10"
  - "double for weight/distance/pace (not decimal) to avoid SQLite TEXT storage issues"
  - "int for duration in seconds (not TimeSpan) to enable server-side querying"
  - "DeleteBehavior.Restrict on Exercise FKs to prevent cascading exercise deletion"
  - "DeleteBehavior.SetNull on TemplateGroup FK and RecurrenceRule FK for graceful orphaning"

patterns-established:
  - "IEntityTypeConfiguration<T> classes in Data/Configurations/ for fluent API setup"
  - "Entity classes in Data/Entities/ with navigation properties using null-forgiving operator (null!)"
  - "Enum files in Data/Enums/ with file-scoped namespaces"
  - "ApplyConfigurationsFromAssembly for auto-discovery of configuration classes"

requirements-completed: [DATA-01, DATA-02, DATA-03, DATA-04, DATA-05, DATA-06, DATA-07, DATA-08, DATA-09, DATA-10]

# Metrics
duration: 4min
completed: 2026-03-21
---

# Phase 01 Plan 01: Data Foundation Summary

**Complete EF Core data model with TPH Exercise hierarchy, WorkoutTemplate with grouping/sections, RecurrenceRule scheduling, and planned-vs-actual workout logging -- 10 tables in SQLite via InitialCreate migration**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-21T15:10:07Z
- **Completed:** 2026-03-21T15:13:53Z
- **Tasks:** 2
- **Files modified:** 25 (9 enums, 4 entities, 1 DbContext, 4 configurations, 3 migrations, 1 csproj, 1 Program.cs, 1 appsettings.json, 1 .gitignore)

## Accomplishments
- Installed EF Core 10.0.5 with SQLite provider and Design-time tools
- Created complete domain model: 9 enums, 4 entity files (12 classes total) covering exercises, templates, scheduling, and logging
- Configured TPH inheritance with string discriminator for Exercise hierarchy (StrengthExercise/EnduranceExercise)
- Built fluent API configurations with proper FK relationships, cascade behaviors, and performance indexes
- Registered IDbContextFactory in Program.cs with SQLite connection string
- Applied InitialCreate migration creating all 10 tables with correct column types and constraints

## Task Commits

Each task was committed atomically:

1. **Task 1: Install NuGet packages and create all enum and entity classes** - `7164bf5` (feat)
2. **Task 2: Create AppDbContext, fluent configs, registration, and initial migration** - `8b483d5` (feat)
3. **Deviation: Add .gitignore for generated files** - `c298365` (chore)

## Files Created/Modified
- `BlazorApp2.csproj` - Added EF Core Sqlite and Design package references
- `Data/Enums/*.cs` (9 files) - MuscleGroup, Equipment, ActivityType, FrequencyType, DaysOfWeek (flags), WorkoutStatus, SetType, GroupType, SectionType
- `Data/Entities/Exercise.cs` - Abstract Exercise base, StrengthExercise (MuscleGroup, Equipment), EnduranceExercise (ActivityType)
- `Data/Entities/WorkoutTemplate.cs` - WorkoutTemplate, TemplateItem (ordered, sectioned, grouped), TemplateGroup (Superset/EMOM)
- `Data/Entities/ScheduledWorkout.cs` - ScheduledWorkout (date, status, template FK), RecurrenceRule (frequency, interval, day flags)
- `Data/Entities/WorkoutLog.cs` - WorkoutLog (RPE, notes, timestamps), SetLog (planned/actual reps/weight), EnduranceLog (planned/actual distance/duration/pace/HR)
- `Data/AppDbContext.cs` - DbContext with 11 DbSets and ApplyConfigurationsFromAssembly
- `Data/Configurations/ExerciseConfiguration.cs` - TPH discriminator, Name index and max length
- `Data/Configurations/TemplateConfiguration.cs` - Template/Item/Group FK relationships, composite index on (TemplateId, Position)
- `Data/Configurations/ScheduleConfiguration.cs` - ScheduledWorkout FKs, ScheduledDate index
- `Data/Configurations/LogConfiguration.cs` - 1:1 WorkoutLog-ScheduledWorkout, cascade log deletes
- `Program.cs` - AddDbContextFactory<AppDbContext> with UseSqlite registration
- `appsettings.json` - DefaultConnection string for workoutplanner.db
- `Migrations/20260321151236_InitialCreate.cs` - Initial migration creating all tables
- `.gitignore` - Ignore runtime database and build artifacts

## Decisions Made
- Used `IDbContextFactory` (not scoped `AddDbContext`) for Blazor Server thread safety per DATA-01
- TPH with string discriminator ("Strength"/"Endurance") for Exercise hierarchy -- EF Core default strategy, nullable subtype columns in DB
- `[Flags]` DaysOfWeek enum stored as integer automatically by EF Core -- no value converter needed
- `double` for weight/distance/pace instead of `decimal` to avoid SQLite storing as TEXT
- `int` for duration in seconds instead of `TimeSpan` to enable direct SQL comparisons
- `DeleteBehavior.Restrict` on Exercise FKs (TemplateItem, SetLog, EnduranceLog) to prevent accidental exercise deletion
- `DeleteBehavior.SetNull` on optional FKs (TemplateGroupId, RecurrenceRuleId) for graceful orphaning
- `DeleteBehavior.Cascade` on parent-child relationships (Template->Items, Template->Groups, WorkoutLog->SetLogs, WorkoutLog->EnduranceLogs)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added .gitignore for generated runtime database**
- **Found during:** Task 2 (post-migration verification)
- **Issue:** workoutplanner.db generated by `dotnet ef database update` was untracked, would be accidentally committed
- **Fix:** Created .gitignore with entries for database files, build outputs, and IDE artifacts
- **Files modified:** .gitignore (created)
- **Verification:** `git status` no longer shows workoutplanner.db
- **Committed in:** c298365

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Necessary housekeeping for generated files. No scope creep.

## Issues Encountered
None - both tasks executed cleanly on first attempt.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- All entity classes and DbContext are ready for Phase 2 (Exercise Library) to build CRUD services and UI on top
- IDbContextFactory is registered and available for injection into Blazor components
- Migration is applied -- database schema matches the entity model exactly
- All 10 requirements (DATA-01 through DATA-10) are fulfilled

## Self-Check: PASSED

- All 19 key files verified present on disk
- All 3 task commits verified in git history (7164bf5, 8b483d5, c298365)
- `dotnet build` succeeds with 0 errors, 0 warnings
- `dotnet ef database update` confirms database is up to date

---
*Phase: 01-data-foundation*
*Completed: 2026-03-21*
