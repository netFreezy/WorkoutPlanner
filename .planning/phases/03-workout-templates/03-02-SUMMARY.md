---
phase: 03-workout-templates
plan: 02
subsystem: ui
tags: [blazor, razor, css, templates, dialog, cards, filtering]

# Dependency graph
requires:
  - phase: 03-workout-templates/01
    provides: "WorkoutTemplate, TemplateItem, TemplateGroup entities and TemplateBuilderState"
provides:
  - "Templates list page at /templates with search, tag filtering, and card grid"
  - "TemplateCard component with exercise preview and duration badge"
  - "TemplateDetailDialog with section/group visualization and action buttons"
  - "DeleteConfirmationDialog reusable component"
  - "Templates NavLink in main navigation"
  - "CSS design tokens for sections, groups, drag, selection, builder z-index"
  - "New keyframe animations: rowSlideIn, sectionExpand, bracketDraw, checkIn"
affects: [03-workout-templates/03, 03-workout-templates/04, 03-workout-templates/05]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Tag filter chips with toggle behavior (click to activate, click again to deactivate)"
    - "Template deep copy with group mapping for duplicate functionality"
    - "Section-based exercise grouping visualization with colored left borders"
    - "Duration estimation by converting entities to builder models"

key-files:
  created:
    - "Components/Pages/Templates.razor"
    - "Components/Pages/Templates.razor.cs"
    - "Components/Pages/Templates.razor.css"
    - "Components/Shared/TemplateCard.razor"
    - "Components/Shared/TemplateCard.razor.css"
    - "Components/Shared/TemplateDetailDialog.razor"
    - "Components/Shared/TemplateDetailDialog.razor.css"
    - "Components/Shared/DeleteConfirmationDialog.razor"
    - "Components/Shared/DeleteConfirmationDialog.razor.css"
  modified:
    - "wwwroot/app.css"
    - "Components/Layout/MainLayout.razor"

key-decisions:
  - "Tag filter chips as toggle buttons instead of dropdown for better discoverability"
  - "Deep copy with group mapping dictionary for correct template duplication"
  - "Section-based visualization with colored left borders matching UI-SPEC section colors"

patterns-established:
  - "Tag filter toggle: active chip has accent border/background, inactive has glass background"
  - "Template card pattern: name, exercise preview (3 items), duration badge, footer with count and tags"
  - "Detail dialog wider panel via ::deep .dialog-panel override in scoped CSS"

requirements-completed: [TMPL-01, TMPL-03, TMPL-04, TMPL-05, TMPL-06, TMPL-07]

# Metrics
duration: 5min
completed: 2026-03-21
---

# Phase 03 Plan 02: Template List Page Summary

**Templates list page with search/tag filtering, exercise preview cards, read-only detail dialog with section/group visualization, duplicate, and delete functionality**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-21T18:57:11Z
- **Completed:** 2026-03-21T19:02:11Z
- **Tasks:** 2
- **Files modified:** 11

## Accomplishments
- Templates list page at /templates with search input, tag filter chips, card grid, FAB, and empty states
- TemplateCard component showing name, exercise preview (first 3), duration badge, exercise count, and tag pills
- TemplateDetailDialog displaying full workout structure with warm-up/working/cool-down sections, superset/EMOM groups, and formatted targets
- DeleteConfirmationDialog with destructive button styling and keep/delete action buttons
- Template duplicate with deep copy (including group mapping) and toast notification
- CSS design tokens for sections, groups, drag, selection, and builder z-index layers
- Four new keyframe animations: rowSlideIn, sectionExpand, bracketDraw, checkIn

## Task Commits

Each task was committed atomically:

1. **Task 1: Add new CSS tokens, nav link, Templates page with search/filter, TemplateCard, and empty states** - `42011ee` (feat)
2. **Task 2: TemplateDetailDialog and DeleteConfirmationDialog** - `d2bc39f` (feat)

## Files Created/Modified
- `wwwroot/app.css` - Added CSS design tokens for sections, groups, drag, selection, builder z-index, and 4 keyframe animations
- `Components/Layout/MainLayout.razor` - Added Templates NavLink after Exercises
- `Components/Pages/Templates.razor` - Template list page with search, tag filter, card grid, FAB, empty states
- `Components/Pages/Templates.razor.cs` - Code-behind with DB loading, filtering, duplicate, delete
- `Components/Pages/Templates.razor.css` - Page styling following Exercises.razor.css pattern
- `Components/Shared/TemplateCard.razor` - Summary card with name, exercise preview, duration, tags
- `Components/Shared/TemplateCard.razor.css` - Card styling with hover effects and stagger animation
- `Components/Shared/TemplateDetailDialog.razor` - Full workout structure dialog with sections, groups, targets
- `Components/Shared/TemplateDetailDialog.razor.css` - Dialog styling with section colors, group indicators
- `Components/Shared/DeleteConfirmationDialog.razor` - Reusable delete confirmation with keep/delete buttons
- `Components/Shared/DeleteConfirmationDialog.razor.css` - Destructive button styling per UI-SPEC

## Decisions Made
- Tag filter chips as toggle buttons: clicking a tag toggles it as the active filter, clicking again clears. More discoverable than a dropdown since users can see all available tags at a glance.
- Deep copy with group mapping dictionary: when duplicating a template, groups are copied first and old-to-new IDs mapped, then items reference the new group via the map. Ensures correct group associations in the copy.
- Section-based visualization with colored left borders: WarmUp uses warm yellow (--color-warmup-text), Working uses text-secondary, CoolDown uses indigo (--color-cooldown-text), matching the UI-SPEC color contract.
- Razor `@section` keyword conflict: used `@(section.ToString().ToLower())` to prevent Razor from interpreting the variable name as a directive.

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Created stub dialogs for Task 1 build compatibility**
- **Found during:** Task 1
- **Issue:** Templates.razor references TemplateDetailDialog and DeleteConfirmationDialog which are Task 2 deliverables, causing build failure in Task 1
- **Fix:** Created minimal stub components with correct parameter signatures to allow Task 1 to build independently
- **Files modified:** Components/Shared/TemplateDetailDialog.razor, Components/Shared/DeleteConfirmationDialog.razor
- **Verification:** Build succeeded with stubs, stubs were replaced with full implementations in Task 2
- **Committed in:** 42011ee (Task 1 commit)

**2. [Rule 1 - Bug] Fixed Razor @section directive conflict**
- **Found during:** Task 2
- **Issue:** `@section.ToString().ToLower()` in CSS class interpolation was parsed as a Razor `@section` directive, causing build errors RZ9979, RZ2005, RZ1011
- **Fix:** Changed to explicit expression syntax `@(section.ToString().ToLower())` to disambiguate
- **Files modified:** Components/Shared/TemplateDetailDialog.razor
- **Verification:** Build succeeded with 0 errors, 0 warnings
- **Committed in:** d2bc39f (Task 2 commit)

---

**Total deviations:** 2 auto-fixed (1 blocking, 1 bug)
**Impact on plan:** Both auto-fixes necessary for correct compilation. No scope creep.

## Issues Encountered
None beyond the auto-fixed items above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Template list page complete and functional, ready for Plans 03-05 (builder, exercise picker, etc.)
- All CSS design tokens for template builder UI are in place
- TemplateDetailDialog and DeleteConfirmationDialog are reusable components available to other pages

## Self-Check: PASSED

All 9 created files verified present. Both task commit hashes (42011ee, d2bc39f) confirmed in git log.

---
*Phase: 03-workout-templates*
*Completed: 2026-03-21*
