---
phase: 03-workout-templates
plan: 03
subsystem: ui
tags: [blazor, template-builder, exercise-picker, tag-input, navigation-lock, inline-editing]

# Dependency graph
requires:
  - phase: 03-workout-templates/plan-01
    provides: TemplateBuilderState, BuilderItem, BuilderGroup, TemplateFormModel, WorkoutTemplate entity
provides:
  - TemplateBuilder page with new/edit routes
  - BuilderToolbar with sticky positioning and action callbacks
  - ExerciseRow with inline strength/endurance target editing
  - ExercisePickerDialog with search, filter, and multi-select
  - TagInput component with pill display and keyboard interaction
  - Save/load mapping between BuilderState and WorkoutTemplate entities
  - NavigationLock with discard confirmation for unsaved changes
affects: [03-workout-templates/plan-04, 03-workout-templates/plan-05, 04-calendar-scheduler]

# Tech tracking
tech-stack:
  added: []
  patterns: [inline-target-editing, multi-select-picker-dialog, tag-input-pills, navigation-lock-with-discard, builder-state-to-entity-mapping]

key-files:
  created:
    - Components/Pages/TemplateBuilder.razor
    - Components/Pages/TemplateBuilder.razor.cs
    - Components/Pages/TemplateBuilder.razor.css
    - Components/Shared/BuilderToolbar.razor
    - Components/Shared/BuilderToolbar.razor.css
    - Components/Shared/ExerciseRow.razor
    - Components/Shared/ExerciseRow.razor.css
    - Components/Shared/ExercisePickerDialog.razor
    - Components/Shared/ExercisePickerDialog.razor.css
    - Components/Shared/TagInput.razor
    - Components/Shared/TagInput.razor.css
  modified:
    - Components/_Imports.razor

key-decisions:
  - "Added BlazorApp2.Data and BlazorApp2.Models to _Imports.razor for global access in inline code blocks"
  - "ExercisePickerDialog uses wasOpen tracking to detect open transitions and reload exercises fresh each time"
  - "ExerciseRow uses direct value/onchange binding pattern for nullable numeric inputs (not @bind) to support null clearing"
  - "Builder stubs NoOp callbacks for Plan 04/05 features (grouping, sections) to keep toolbar wired"

patterns-established:
  - "Inline target editing: compact-input with ParseNullableInt/ParseNullableDouble helpers for nullable numeric fields"
  - "Multi-select picker: HashSet<int> toggle pattern with filtered exercise list and bulk add"
  - "Tag input: enter-to-add, backspace-to-remove, duplicate-prevention with case-insensitive comparison"
  - "Builder save: clear-and-rebuild pattern for items/groups on existing templates"

requirements-completed: [TMPL-01, TMPL-02, TMPL-03, TMPL-04]

# Metrics
duration: 6min
completed: 2026-03-21
---

# Phase 3 Plan 03: Template Builder Page Summary

**Full-page template builder with exercise picker dialog, inline strength/endurance target editing, tag input, save/load, and NavigationLock for unsaved changes**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-21T18:57:39Z
- **Completed:** 2026-03-21T19:04:10Z
- **Tasks:** 2
- **Files modified:** 12

## Accomplishments
- Built TemplateBuilder page with /templates/new and /templates/{id}/edit routes, including name input, description toggle, tag input, and empty state
- Created ExerciseRow with checkbox selection, drag handle, position numbers, type tags, and inline strength (sets x reps @ weight) and endurance (km/sec/pace/zone) target fields
- Created ExercisePickerDialog with search, type filter, multi-select checkboxes, selected count, and add/close actions
- Implemented save/load logic mapping BuilderState to/from WorkoutTemplate entities with clear-and-rebuild for edits
- Added NavigationLock with discard confirmation dialog preventing accidental data loss

## Task Commits

Each task was committed atomically:

1. **Task 1: TemplateBuilder page with name/description/tag input, exercise list rendering, save/load, and NavigationLock** - `747b586` (feat)
2. **Task 2: ExerciseRow with inline targets and ExercisePickerDialog with multi-select** - `326f428` (feat)

## Files Created/Modified
- `Components/Pages/TemplateBuilder.razor` - Full-page template editor with new/edit routes
- `Components/Pages/TemplateBuilder.razor.cs` - Code-behind with save/load, undo/redo, NavigationLock handlers
- `Components/Pages/TemplateBuilder.razor.css` - Builder page styling with name input, description, empty state
- `Components/Shared/BuilderToolbar.razor` - Sticky toolbar with add exercises, undo/redo, save, discard actions
- `Components/Shared/BuilderToolbar.razor.css` - Toolbar styling with ghost buttons, icon buttons, save gradient
- `Components/Shared/ExerciseRow.razor` - Exercise row with checkbox, drag handle, type tag, inline targets
- `Components/Shared/ExerciseRow.razor.css` - Row styling with selection state, compact inputs, type tags
- `Components/Shared/ExercisePickerDialog.razor` - Multi-select exercise browser with search and filter
- `Components/Shared/ExercisePickerDialog.razor.css` - Picker styling with scrollable list, checkboxes, footer
- `Components/Shared/TagInput.razor` - Freeform tag input with pill display and keyboard interaction
- `Components/Shared/TagInput.razor.css` - Tag input styling with pills, focus ring, inline text input
- `Components/_Imports.razor` - Added BlazorApp2.Data and BlazorApp2.Models namespaces

## Decisions Made
- Added BlazorApp2.Data namespace to _Imports.razor globally since ExercisePickerDialog uses inline @code block needing DbContext access
- Used wasOpen boolean pattern in ExercisePickerDialog to detect dialog open transitions and reload exercises fresh from DB each time
- ExerciseRow uses value/onchange binding (not @bind) for nullable numeric inputs to properly support clearing values to null
- Stubbed NoOp callbacks in TemplateBuilder for Plan 04/05 grouping and section features to keep toolbar fully wired

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Added BlazorApp2.Data namespace to _Imports.razor**
- **Found during:** Task 2 (ExercisePickerDialog)
- **Issue:** ExercisePickerDialog uses inline @code block with IDbContextFactory<AppDbContext>, but AppDbContext namespace was not globally imported
- **Fix:** Added @using BlazorApp2.Data to _Imports.razor
- **Files modified:** Components/_Imports.razor
- **Verification:** Build succeeds with 0 errors
- **Committed in:** 326f428 (Task 2 commit)

---

**Total deviations:** 1 auto-fixed (1 blocking)
**Impact on plan:** Essential for compilation. No scope creep.

## Issues Encountered
None

## Known Stubs
- BuilderToolbar: OnAddWarmUp, OnAddCoolDown, OnGroupSuperset, OnGroupEmom, OnUngroup callbacks are wired to NoOp -- implemented in Plan 04 (grouping) and Plan 05 (sections)
- HasWarmUp and HasCoolDown are hardcoded false -- implemented in Plan 05 (sections)

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Template builder core is complete and ready for Plan 04 (drag-and-drop reordering) and Plan 05 (sections and grouping)
- ExerciseRow, BuilderToolbar, and TemplateBuilder.razor.cs are designed to receive grouping and section features via the stubbed callbacks
- All existing tests pass (50/50)

## Self-Check: PASSED

- All 11 created files verified present on disk
- Both task commits (747b586, 326f428) verified in git log
- Build: 0 errors, 0 warnings
- Tests: 50/50 passed

---
*Phase: 03-workout-templates*
*Completed: 2026-03-21*
