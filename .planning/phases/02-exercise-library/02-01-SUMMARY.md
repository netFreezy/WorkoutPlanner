---
phase: 02-exercise-library
plan: 01
subsystem: database
tags: [ef-core, seed-data, sqlite, tph, xunit]

# Dependency graph
requires:
  - phase: 01-data-foundation
    provides: Exercise TPH hierarchy, AppDbContext, DataTestBase
provides:
  - 50 seed exercises (37 strength, 13 endurance) via EF Core HasData
  - ExerciseSeedData static class for seed arrays
  - StrengthExerciseConfiguration and EnduranceExerciseConfiguration with HasData
  - SeedExercises migration
  - 17 data-layer tests covering seed verification, filtering, and creation
affects: [02-exercise-library, 03-workout-templates]

# Tech tracking
tech-stack:
  added: []
  patterns: [seed-data-via-hasdata, derived-type-configuration, fixed-datetime-for-seeds]

key-files:
  created:
    - Data/SeedData/ExerciseSeedData.cs
    - BlazorApp2.Tests/ExerciseSeedTests.cs
    - BlazorApp2.Tests/ExerciseFilterTests.cs
    - BlazorApp2.Tests/ExerciseCreateTests.cs
    - Migrations/20260321163901_SeedExercises.cs
  modified:
    - Data/Configurations/ExerciseConfiguration.cs
    - BlazorApp2.Tests/ExerciseHierarchyTests.cs
    - BlazorApp2.Tests/DbContextFactoryTests.cs

key-decisions:
  - "Seed data uses fixed DateTime(2026,1,1) to prevent migration diffs from UtcNow"
  - "Strength IDs 1-37, Endurance IDs 101-113 to leave expansion room and separate from auto-increment"
  - "HasData applied via separate derived type configurations (StrengthExerciseConfiguration, EnduranceExerciseConfiguration) per EF Core TPH requirements"
  - "Reduced Core exercises from 6 to 4 to fit 3 FullBody exercises within 37 total"

patterns-established:
  - "Seed data pattern: static class with typed arrays, fixed CreatedDate, explicit IDs with range separation"
  - "Derived type HasData: separate IEntityTypeConfiguration per derived type, not on base builder"

requirements-completed: [EXER-03, EXER-01, EXER-02]

# Metrics
duration: 6min
completed: 2026-03-21
---

# Phase 02 Plan 01: Seed Data and Tests Summary

**50 exercises seeded via EF Core HasData (37 calisthenics-focused strength, 13 running/cycling endurance) with 17 data-layer tests covering seed counts, LINQ filtering, and CRUD persistence**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-21T16:36:25Z
- **Completed:** 2026-03-21T16:42:37Z
- **Tasks:** 2
- **Files modified:** 8

## Accomplishments
- Seeded 50 exercises into database with full descriptions and form cues for every exercise
- Created comprehensive test suite covering seed data verification (EXER-03), filtering logic (EXER-01), and exercise creation (EXER-02)
- All 39 tests in the full suite pass (17 new + 22 existing)

## Task Commits

Each task was committed atomically:

1. **Task 1: Create seed data and generate migration** - `7a55181` (feat)
2. **Task 2: Create data-layer tests for seed, filter, and create operations** - `0094553` (test)

## Files Created/Modified
- `Data/SeedData/ExerciseSeedData.cs` - Static seed arrays for 37 StrengthExercise and 13 EnduranceExercise with form cue descriptions
- `Data/Configurations/ExerciseConfiguration.cs` - Added StrengthExerciseConfiguration and EnduranceExerciseConfiguration with HasData calls
- `Migrations/20260321163901_SeedExercises.cs` - Migration inserting all 50 exercises
- `BlazorApp2.Tests/ExerciseSeedTests.cs` - 6 tests verifying seed data counts, descriptions, muscle group coverage, running variants
- `BlazorApp2.Tests/ExerciseFilterTests.cs` - 7 tests verifying name search, muscle group/equipment/type filtering, AND combinations
- `BlazorApp2.Tests/ExerciseCreateTests.cs` - 4 tests verifying strength/endurance creation, base DbSet queryability, auto-ID
- `BlazorApp2.Tests/ExerciseHierarchyTests.cs` - Updated to account for seed data presence
- `BlazorApp2.Tests/DbContextFactoryTests.cs` - Updated to account for seed data presence

## Decisions Made
- Used fixed `DateTime(2026, 1, 1)` for all seed CreatedDate to prevent migration diffs from `DateTime.UtcNow`
- Strength exercise IDs 1-37, Endurance IDs 101-113 with gap for future expansion
- HasData applied via separate derived type configurations per EF Core TPH pattern (cannot call HasData on base type builder for derived types)
- Reduced Core category from 6 to 4 exercises to accommodate 3 FullBody exercises (Burpee, Turkish Get-Up, Kettlebell Swing) within the 37 total

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed Phase 1 tests broken by seed data introduction**
- **Found during:** Task 2 (test creation and verification)
- **Issue:** Pre-existing ExerciseHierarchyTests and DbContextFactoryTests assumed empty database. EnsureCreated() now loads seed data, breaking Assert.Single(), Assert.Empty(), and FirstAsync() assumptions.
- **Fix:** Updated 7 test methods to query by name instead of positional access, use seed-aware count assertions, and verify type discrimination with seed data present
- **Files modified:** BlazorApp2.Tests/ExerciseHierarchyTests.cs, BlazorApp2.Tests/DbContextFactoryTests.cs
- **Verification:** Full test suite (39 tests) passes with 0 failures
- **Committed in:** 0094553 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 bug fix)
**Impact on plan:** Necessary correction to pre-existing tests that assumed empty database. No scope creep.

## Issues Encountered
None - seed data applied cleanly and all tests pass.

## Known Stubs
None - all seed data is fully populated with descriptions and form cues.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Exercise catalog data is seeded and verified, ready for UI display in Plan 02 (exercise list page)
- Filtering logic patterns established in tests match what the UI component will use
- All three EXER requirements have data-layer test coverage

## Self-Check: PASSED

All 6 created files verified on disk. Both task commit hashes (7a55181, 0094553) found in git log.

---
*Phase: 02-exercise-library*
*Completed: 2026-03-21*
