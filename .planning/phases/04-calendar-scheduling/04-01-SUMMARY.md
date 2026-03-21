---
phase: 04-calendar-scheduling
plan: 01
subsystem: database
tags: [ef-core, sqlite, scheduling, recurrence, materialization, ad-hoc-workouts]

# Dependency graph
requires:
  - phase: 01-data-foundation
    provides: ScheduledWorkout, RecurrenceRule, WorkoutTemplate entities and AppDbContext
provides:
  - "Nullable WorkoutTemplateId + AdHocName on ScheduledWorkout for ad-hoc workouts"
  - "MaterializationService with GenerateOccurrences and MaterializeAsync for recurrence"
  - "SchedulingService with CRUD, date range queries, and DetermineWorkoutType"
  - "WorkoutType enum (Strength, Endurance, Mixed, AdHoc)"
  - "TestDbContextFactory for service-level testing with shared SQLite connection"
affects: [04-02, 04-03, 04-04, 05-session-logging]

# Tech tracking
tech-stack:
  added: []
  patterns: [IDbContextFactory service pattern, materialization with dedup, TestDbContextFactory for service testing]

key-files:
  created:
    - Services/MaterializationService.cs
    - Services/SchedulingService.cs
    - Data/Enums/WorkoutType.cs
    - BlazorApp2.Tests/MaterializationTests.cs
    - Migrations/20260321201353_AddAdHocWorkoutSupport.cs
  modified:
    - Data/Entities/ScheduledWorkout.cs
    - Data/Configurations/ScheduleConfiguration.cs
    - Program.cs
    - BlazorApp2.Tests/ScheduleTests.cs
    - BlazorApp2.Tests/DataTestBase.cs

key-decisions:
  - "TestDbContextFactory creates new contexts per call sharing same SQLite connection, avoiding disposed-context issues with service await-using pattern"
  - "GenerateOccurrences is a static pure method for testability without DB dependency"

patterns-established:
  - "Service testing pattern: TestDbContextFactory with shared SqliteConnection for service-level tests using IDbContextFactory"
  - "Fresh context verification: use factory.CreateDbContext() for assertions after service operations modify data"

requirements-completed: [SCHED-03, SCHED-04, SCHED-06]

# Metrics
duration: 7min
completed: 2026-03-21
---

# Phase 04 Plan 01: Scheduling Data Layer Summary

**Ad-hoc workout support with nullable template FK, materialization service for recurrence date generation, and scheduling CRUD service with 73 passing tests**

## Performance

- **Duration:** 7 min
- **Started:** 2026-03-21T20:11:58Z
- **Completed:** 2026-03-21T20:19:56Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments
- Schema supports ad-hoc workouts (nullable WorkoutTemplateId + AdHocName + DisplayName computed property)
- MaterializationService generates correct dates for Weekly (DaysOfWeek flags) and Daily (interval) recurrence, idempotent with dedup
- SchedulingService provides full CRUD: schedule, remove, skip, reschedule, plus date range queries with eager-loaded includes
- 23 new tests covering materialization logic, ad-hoc persistence, DisplayName fallback, DetermineWorkoutType, skip, and remove-recurring

## Task Commits

Each task was committed atomically:

1. **Task 1: Schema change + migration + services** - `c49b4fc` (feat)
2. **Task 2: Materialization and scheduling tests** - `f421d21` (test)

## Files Created/Modified
- `Data/Entities/ScheduledWorkout.cs` - Nullable WorkoutTemplateId, AdHocName, DisplayName on ScheduledWorkout; StartDate and nullable fields on RecurrenceRule
- `Data/Configurations/ScheduleConfiguration.cs` - SetNull delete behavior for both FKs, AdHocName max length constraints
- `Data/Enums/WorkoutType.cs` - New enum: Strength, Endurance, Mixed, AdHoc
- `Services/MaterializationService.cs` - GenerateOccurrences (static), MaterializeAsync, MaterializeAllAsync, ToDaysOfWeek, GetMondayOfWeek
- `Services/SchedulingService.cs` - GetWorkoutsForWeek/MonthAsync, ScheduleWorkoutAsync, RemoveWorkoutAsync, RemoveRecurringAsync, SkipWorkoutAsync, RescheduleWorkoutAsync, DetermineWorkoutType
- `Program.cs` - DI registration for MaterializationService and SchedulingService
- `Migrations/20260321201353_AddAdHocWorkoutSupport.cs` - EF migration for schema changes
- `BlazorApp2.Tests/MaterializationTests.cs` - 5+ test methods for recurrence generation and dedup
- `BlazorApp2.Tests/ScheduleTests.cs` - Extended with ad-hoc, DisplayName, DetermineWorkoutType, skip, and remove-recurring tests
- `BlazorApp2.Tests/DataTestBase.cs` - Exposed Connection property for TestDbContextFactory

## Decisions Made
- TestDbContextFactory creates new AppDbContext instances per call sharing the same in-memory SQLite connection, avoiding ObjectDisposedException when services use "await using" pattern
- GenerateOccurrences implemented as static pure method on MaterializationService for direct unit testing without DB dependency
- Fresh context pattern for test assertions: after service operations, use factory.CreateDbContext() to query, since the test's shared Context has stale cached entities

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed ActivityType enum values in tests**
- **Found during:** Task 2 (test creation)
- **Issue:** Plan examples used ActivityType.Running/Cycling but actual enum uses Run/Cycle
- **Fix:** Changed to ActivityType.Run and ActivityType.Cycle in test assertions
- **Files modified:** BlazorApp2.Tests/ScheduleTests.cs
- **Verification:** Tests compile and pass
- **Committed in:** f421d21 (Task 2 commit)

**2. [Rule 3 - Blocking] Fixed ObjectDisposedException in service tests**
- **Found during:** Task 2 (test execution)
- **Issue:** TestDbContextFactory returned shared Context which got disposed by services' "await using" pattern
- **Fix:** Redesigned factory to create new contexts from shared SqliteConnection; exposed Connection property on DataTestBase; used fresh contexts for assertions
- **Files modified:** BlazorApp2.Tests/MaterializationTests.cs, BlazorApp2.Tests/DataTestBase.cs, BlazorApp2.Tests/ScheduleTests.cs
- **Verification:** All 73 tests pass
- **Committed in:** f421d21 (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (1 bug, 1 blocking)
**Impact on plan:** Both fixes necessary for test correctness. No scope creep.

## Issues Encountered
None beyond the auto-fixed deviations.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- MaterializationService and SchedulingService are registered in DI and ready for UI consumption
- Calendar UI components (Plan 04-02) can use GetWorkoutsForWeekAsync/GetWorkoutsForMonthAsync for data
- Ad-hoc workout creation (Plan 04-03) has full schema and service support
- DetermineWorkoutType enables visual differentiation of workout types in calendar view

## Self-Check: PASSED

- All 5 created files verified on disk
- Both task commits (c49b4fc, f421d21) found in git log
- SUMMARY.md created successfully
- 73/73 tests passing

---
*Phase: 04-calendar-scheduling*
*Completed: 2026-03-21*
