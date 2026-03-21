---
phase: 03-workout-templates
plan: 01
subsystem: database, models
tags: [ef-core, json-converter, undo-redo, value-comparer, sqlite, tdd]

# Dependency graph
requires:
  - phase: 01-data-foundation
    provides: WorkoutTemplate entity, TemplateItem, TemplateGroup, AppDbContext, SectionType/GroupType enums
provides:
  - Tags property on WorkoutTemplate with JSON value converter
  - TemplateBuilderState in-memory editing model with undo/redo
  - TemplateFormModel for template name validation
  - EF Core migration AddTemplateTags
  - Duration estimation algorithm for mixed strength/endurance templates
affects: [03-workout-templates]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - JSON value converter with ValueComparer for List<string> in EF Core
    - In-memory builder state decoupled from EF Core entities (TemplateBuilderState)
    - Snapshot-based undo/redo with JSON serialization and depth limit

key-files:
  created:
    - Models/TemplateBuilderState.cs
    - Models/TemplateFormModel.cs
    - Migrations/20260321185044_AddTemplateTags.cs
    - BlazorApp2.Tests/TemplateTagTests.cs
    - BlazorApp2.Tests/TemplateDuplicateTests.cs
    - BlazorApp2.Tests/TemplateDurationTests.cs
  modified:
    - Data/Entities/WorkoutTemplate.cs
    - Data/Configurations/TemplateConfiguration.cs

key-decisions:
  - "ValueComparer added for List<string> Tags to ensure EF Core change tracking works correctly with collection value converters"
  - "TemplateBuilderState uses JSON snapshot serialization for undo/redo (excludes UI-only IsSelected property)"
  - "Duration estimation rounds to nearest 5 with minimum 5 minutes, EMOM groups override per-exercise calculation"

patterns-established:
  - "JSON value converter + ValueComparer pattern for collection properties in EF Core on SQLite"
  - "Builder state decoupled from EF entities with snapshot-based undo/redo"

requirements-completed: [TMPL-01, TMPL-03, TMPL-04, TMPL-05, TMPL-06, TMPL-07]

# Metrics
duration: 5min
completed: 2026-03-21
---

# Phase 3 Plan 1: Template Data Layer Summary

**Tags JSON converter on WorkoutTemplate with ValueComparer, TemplateBuilderState with snapshot undo/redo (50-deep), duration estimation, and 11 unit tests**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-21T18:49:26Z
- **Completed:** 2026-03-21T18:54:16Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Added Tags property to WorkoutTemplate entity with JSON value converter and ValueComparer for correct EF Core change tracking
- Created TemplateBuilderState providing in-memory editing with undo/redo stacks capped at 50, plus static duration estimation for mixed strength/endurance templates
- Created TemplateFormModel with Required/MaxLength validation matching ExerciseFormModel pattern
- 11 new tests covering tag round-trips, template deep copy, and duration estimation edge cases -- all passing alongside existing 39 tests (50 total)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add Tags, JSON converter, migration, TemplateBuilderState, TemplateFormModel** - `6a41fd8` (feat)
2. **Task 2: Write unit tests for Tags, Duplicate, and Duration** - `88c1b78` (test)

## Files Created/Modified
- `Data/Entities/WorkoutTemplate.cs` - Added List<string> Tags property
- `Data/Configurations/TemplateConfiguration.cs` - Added JSON value converter with ValueComparer for Tags
- `Models/TemplateBuilderState.cs` - BuilderItem, BuilderGroup, and TemplateBuilderState with undo/redo and EstimateDurationMinutes
- `Models/TemplateFormModel.cs` - Template name validation model (Required, MaxLength 200)
- `Migrations/20260321185044_AddTemplateTags.cs` - EF Core migration adding Tags column with DEFAULT '[]'
- `BlazorApp2.Tests/TemplateTagTests.cs` - 3 tests: empty, multi-value, special character tag round-trips
- `BlazorApp2.Tests/TemplateDuplicateTests.cs` - 2 tests: deep copy preserving items/groups/tags, EMOM params
- `BlazorApp2.Tests/TemplateDurationTests.cs` - 6 tests: strength, endurance, mixed, EMOM, defaults, empty minimum

## Decisions Made
- Added ValueComparer for List<string> Tags to ensure EF Core correctly detects collection mutations (auto-fix, Rule 2 -- EF warning during migration)
- TemplateBuilderState uses JSON snapshot serialization excluding UI-only IsSelected property
- Duration estimation algorithm: EMOM groups use rounds*minuteWindow, strength uses sets*1.5, endurance uses durationSeconds/60 or default 10, result rounded to nearest 5 with minimum 5

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added ValueComparer for Tags collection property**
- **Found during:** Task 1 (EF Core migration creation)
- **Issue:** EF Core warned that List<string> Tags has a value converter but no value comparer, meaning change tracking would not detect mutations to the collection
- **Fix:** Added ValueComparer<List<string>> with SequenceEqual comparison, HashCode.Combine aggregation, and ToList snapshot
- **Files modified:** Data/Configurations/TemplateConfiguration.cs
- **Verification:** Migration applies cleanly, build succeeds with 0 warnings
- **Committed in:** 6a41fd8 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 missing critical)
**Impact on plan:** Essential for correct EF Core change tracking. No scope creep.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Data layer foundation complete for Phase 3 template UI plans
- TemplateBuilderState ready for binding to Blazor components in subsequent plans
- TemplateFormModel ready for EditForm integration
- All tests green, migration applied

## Self-Check: PASSED

All 9 files verified present. Both task commits (6a41fd8, 88c1b78) verified in git log.

---
*Phase: 03-workout-templates*
*Completed: 2026-03-21*
