---
phase: 02-exercise-library
plan: 02
subsystem: ui
tags: [blazor, css-grid, dark-theme, responsive, dialog, filtering, exercise-library]

# Dependency graph
requires:
  - phase: 02-exercise-library
    plan: 01
    provides: 50 seed exercises (37 strength, 13 endurance) via EF Core HasData
  - phase: 01-data-foundation
    provides: Exercise TPH hierarchy, AppDbContext, IDbContextFactory
provides:
  - Exercise library page at /exercises with card grid, search, and filter
  - Reusable Dialog, ExerciseCard, FilterChip, ExerciseDetailDialog shared components
  - Top navigation bar in MainLayout with Home and Exercises links
  - CSS design token system (dark premium theme) in app.css
  - Google Fonts Inter loaded in App.razor
affects: [02-exercise-library, 03-workout-templates, 04-calendar-scheduler]

# Tech tracking
tech-stack:
  added: [Inter font via Google Fonts CDN]
  patterns: [css-design-tokens, dark-theme-tokens, scoped-css-components, idbcontextfactory-per-operation, client-side-filtering, pure-css-dialog]

key-files:
  created:
    - Components/Shared/Dialog.razor
    - Components/Shared/Dialog.razor.css
    - Components/Shared/ExerciseCard.razor
    - Components/Shared/ExerciseCard.razor.css
    - Components/Shared/ExerciseDetailDialog.razor
    - Components/Shared/ExerciseDetailDialog.razor.css
    - Components/Shared/FilterChip.razor
    - Components/Shared/FilterChip.razor.css
    - Components/Pages/Exercises.razor
    - Components/Pages/Exercises.razor.cs
    - Components/Pages/Exercises.razor.css
  modified:
    - Components/_Imports.razor
    - Components/App.razor
    - Components/Layout/MainLayout.razor
    - Components/Layout/MainLayout.razor.css
    - wwwroot/app.css

key-decisions:
  - "Dark premium theme using CSS custom properties as design tokens in :root, per UI-SPEC contract"
  - "Client-side filtering: load all exercises once, filter in-memory via LINQ (50 items, no DB round-trips on filter change)"
  - "Pure CSS dialog component with backdrop-filter blur, no JavaScript interop"
  - "SVG inline icons for close/search/chevron, no icon library dependency"
  - "Staggered card entrance animation with CSS --card-index variable and prefers-reduced-motion support"

patterns-established:
  - "Design token pattern: all visual values as CSS custom properties on :root in app.css"
  - "Shared component pattern: Components/Shared/ directory with scoped CSS co-located files"
  - "Dialog pattern: reusable Dialog wrapper with IsOpen/Title/ChildContent/OnClose parameters"
  - "IDbContextFactory per-operation: using var context = DbFactory.CreateDbContext() for each query"
  - "Enum display helper: Regex.Replace for PascalCase to space-separated words"
  - "Reduced motion: all @keyframes and multi-property transitions wrapped in @media (prefers-reduced-motion: no-preference)"

requirements-completed: [EXER-01]

# Metrics
duration: 5min
completed: 2026-03-21
---

# Phase 02 Plan 02: Exercise Library Browse Experience Summary

**Dark-themed exercise library page with CSS Grid card grid, instant search/filter with AND logic, removable filter chips, detail dialog, and responsive top navigation bar using CSS design tokens**

## Performance

- **Duration:** 5 min
- **Started:** 2026-03-21T16:45:01Z
- **Completed:** 2026-03-21T16:49:56Z
- **Tasks:** 2
- **Files modified:** 16

## Accomplishments
- Built complete exercise browse experience with card grid showing all 50 seeded exercises
- Implemented instant search (by name) and filter (type, muscle group, equipment) with AND logic and removable filter chips
- Created reusable Dialog, ExerciseCard, FilterChip, and ExerciseDetailDialog shared components
- Established dark premium theme with CSS design tokens (90+ custom properties) per UI-SPEC
- Added top navigation bar to MainLayout with active link indicator
- All components support keyboard navigation, ARIA labels, and reduced-motion preferences

## Task Commits

Each task was committed atomically:

1. **Task 1: Create shared components and update imports** - `11db64f` (feat)
2. **Task 2: Create Exercises page with search/filter/grid and update MainLayout navigation** - `c448f82` (feat)

## Files Created/Modified
- `Components/Shared/Dialog.razor` - Reusable modal overlay wrapper with frosted backdrop blur
- `Components/Shared/Dialog.razor.css` - Dialog overlay, panel, header, and close button dark theme styles
- `Components/Shared/ExerciseCard.razor` - Exercise card with name and type tag, keyboard accessible
- `Components/Shared/ExerciseCard.razor.css` - Card hover effects, type tag glow, focus-visible ring
- `Components/Shared/ExerciseDetailDialog.razor` - Read-only exercise detail view with polymorphic fields
- `Components/Shared/ExerciseDetailDialog.razor.css` - Detail field layout and type tag styles (scoped)
- `Components/Shared/FilterChip.razor` - Removable filter chip with SVG close icon
- `Components/Shared/FilterChip.razor.css` - Accent-colored chip with entrance animation
- `Components/Pages/Exercises.razor` - Exercise library page with search, filter bar, card grid, and states
- `Components/Pages/Exercises.razor.cs` - Code-behind with IDbContextFactory, LINQ filtering, state management
- `Components/Pages/Exercises.razor.css` - Page layout, filter bar, grid, responsive breakpoints
- `Components/_Imports.razor` - Added Shared, Entities, Enums, EntityFrameworkCore namespaces
- `Components/App.razor` - Added Google Fonts Inter preconnect and stylesheet links
- `Components/Layout/MainLayout.razor` - Added sticky top nav bar with NavLink components
- `Components/Layout/MainLayout.razor.css` - Nav bar glassmorphism styling with active link accent border
- `wwwroot/app.css` - Added 90+ CSS design tokens for dark premium theme, body styling, scrollbar styling

## Decisions Made
- Used dark premium theme from UI-SPEC instead of light-colored values in plan's inline code snippets (UI-SPEC is authoritative design contract)
- Added CSS design tokens and Google Fonts as part of Task 1 (Rule 2: missing critical functionality -- all components depend on tokens)
- Created ExerciseDetailDialog.razor.css (not in original plan) to properly scope type-tag styles inside the dialog (scoped CSS from ExerciseCard would not apply inside a different component)
- Used `se2` variable name for second StrengthExercise pattern match in FilteredExercises to avoid C# compiler error with duplicate `se` variable in separate LINQ Where clauses
- Set nav height to 60px per UI-SPEC (plan spec said 48px, but UI-SPEC is authoritative)
- Used 280px card minmax per UI-SPEC (plan said 260px)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 2 - Missing Critical] Added CSS design tokens and Google Fonts to app.css and App.razor**
- **Found during:** Task 1 (shared component creation)
- **Issue:** UI-SPEC specifies a dark premium theme with CSS custom properties as design tokens. These tokens did not exist in app.css yet, and the Google Fonts Inter link was not in App.razor. All shared components reference these tokens.
- **Fix:** Added 90+ CSS custom properties to :root in app.css, added body styling for dark background, added Inter font links to App.razor head
- **Files modified:** wwwroot/app.css, Components/App.razor
- **Verification:** dotnet build succeeds, all components render with correct token values
- **Committed in:** 11db64f (Task 1 commit)

**2. [Rule 3 - Blocking] Created ExerciseDetailDialog.razor.css for scoped type-tag styles**
- **Found during:** Task 1 (ExerciseDetailDialog creation)
- **Issue:** Type tag CSS classes (.type-tag, .type-tag--strength, .type-tag--endurance) are defined in ExerciseCard.razor.css as scoped CSS. They would not apply inside ExerciseDetailDialog since Blazor scoped CSS isolates styles per component.
- **Fix:** Created ExerciseDetailDialog.razor.css with the type-tag classes and detail-specific layout styles
- **Files modified:** Components/Shared/ExerciseDetailDialog.razor.css (new file)
- **Verification:** dotnet build succeeds
- **Committed in:** 11db64f (Task 1 commit)

**3. [Rule 1 - Bug] Fixed duplicate variable name in LINQ Where chain**
- **Found during:** Task 2 (Exercises.razor.cs code-behind)
- **Issue:** Plan's FilteredExercises code reuses `se` variable name in two separate `.Where()` lambda clauses for pattern matching `e is StrengthExercise se`. C# requires unique variable names across chained LINQ expressions.
- **Fix:** Used `se2` for the equipment filter clause: `e is StrengthExercise se2 && se2.Equipment == selectedEquipment`
- **Files modified:** Components/Pages/Exercises.razor.cs
- **Verification:** dotnet build succeeds with 0 errors
- **Committed in:** c448f82 (Task 2 commit)

---

**Total deviations:** 3 auto-fixed (1 missing critical, 1 blocking, 1 bug)
**Impact on plan:** All auto-fixes necessary for correct dark theme rendering and compilation. No scope creep.

## Issues Encountered
None - both tasks compiled successfully on first build attempt.

## Known Stubs
None - all exercises load from seeded database, all filter logic is fully wired, detail dialog displays complete exercise data.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Exercise library browse experience is complete with search, filter, and detail view
- Shared Dialog component ready for reuse in Plan 03 (exercise creation form) and Plan 04 (exercise edit)
- FilterChip component reusable for future filter UIs
- CSS design token system established for all future UI phases
- Navigation bar ready for additional links (Workouts, Calendar, etc.) in future phases

## Self-Check: PASSED

All 16 files verified on disk. Both task commit hashes (11db64f, c448f82) found in git log.

---
*Phase: 02-exercise-library*
*Completed: 2026-03-21*
