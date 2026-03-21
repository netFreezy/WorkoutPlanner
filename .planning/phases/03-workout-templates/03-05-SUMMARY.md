---
phase: 03-workout-templates
plan: 05
subsystem: ui
tags: [blazor, sortablejs, drag-and-drop, js-interop, keyboard-shortcuts, cross-section-drag]

# Dependency graph
requires:
  - phase: 03-workout-templates/plan-03
    provides: TemplateBuilder page, ExerciseRow with drag handle, BuilderToolbar with undo/redo
  - phase: 03-workout-templates/plan-04
    provides: SectionHeader component, section rendering with data-section attributes, GroupBracket component
provides:
  - SortableJS v1.15.7 library for drag-and-drop reordering
  - JS interop module with initSortable/refreshSortable/destroySortable exports
  - Cross-section drag support (D-20) that updates SectionType on drop
  - DOM revert pattern preventing Blazor/SortableJS DOM ownership conflict
  - Keyboard undo/redo via Ctrl+Z and Ctrl+Shift+Z
  - IAsyncDisposable cleanup for SortableJS instance on page leave
affects: [04-calendar-scheduler]

# Tech tracking
tech-stack:
  added: [SortableJS 1.15.7]
  patterns:
    - "SortableJS DOM revert pattern: revert DOM change on drop, let Blazor re-render from state"
    - "Section-aware drop detection via data-section attribute scanning backwards from drop position"
    - "Exercise-only index computation excluding section header DOM elements"
    - "IJSObjectReference module import pattern for ES module interop"

key-files:
  created:
    - wwwroot/js/sortable.min.js
    - wwwroot/js/template-builder.js
  modified:
    - Components/App.razor
    - Components/Pages/TemplateBuilder.razor
    - Components/Pages/TemplateBuilder.razor.cs
    - Components/Pages/TemplateBuilder.razor.css

key-decisions:
  - "SortableJS loaded as global script (not ES module) because the interop module references Sortable as a global constructor"
  - "DOM revert pattern: SortableJS moves DOM elements, but we undo that and let Blazor re-render from C# state to avoid DOM ownership conflicts"
  - "Cross-section detection via backward scan from drop position for nearest data-section attribute on section header wrapper divs"

patterns-established:
  - "DOM revert pattern: on SortableJS onEnd, remove item from new position, insertBefore at old position, then notify .NET to update state"
  - "Section header wrapper: wrap SectionHeader component in a div with data-section and class=section-header for JS detection and filter"
  - "IJSObjectReference lifecycle: import on firstRender, re-init on item count change, destroy on DisposeAsync with JSDisconnectedException catch"

requirements-completed: [TMPL-02]

# Metrics
duration: 4min
completed: 2026-03-21
---

# Phase 03 Plan 05: Drag-and-Drop Reordering Summary

**SortableJS drag-and-drop with cross-section exercise reassignment (D-20), DOM revert pattern for Blazor compatibility, and Ctrl+Z/Ctrl+Shift+Z keyboard shortcuts**

## Performance

- **Duration:** 4 min
- **Started:** 2026-03-21T19:13:03Z
- **Completed:** 2026-03-21T19:16:48Z
- **Tasks:** 2
- **Files modified:** 6

## Accomplishments
- Downloaded SortableJS v1.15.7 (45KB) and created JS interop module with initSortable, refreshSortable, destroySortable exports
- Implemented cross-section drag (D-20): dragging an exercise between sections updates its SectionType based on nearest section header detection
- DOM revert pattern prevents Blazor/SortableJS DOM ownership conflict -- SortableJS moves are undone, Blazor re-renders from C# state
- Keyboard shortcuts: Ctrl+Z for undo, Ctrl+Shift+Z for redo, via @onkeydown handler on builder-page div
- SortableJS lifecycle: initializes on first render, re-initializes when item count changes, properly disposed via IAsyncDisposable
- Section headers wrapped in data-section divs with .section-header class so SortableJS filter prevents them from being dragged

## Task Commits

Each task was committed atomically:

1. **Task 1: Download SortableJS, create JS interop module, add script to App.razor** - `a5da96d` (feat)
2. **Task 2: Wire SortableJS into TemplateBuilder with section-aware OnItemReordered and keyboard shortcuts** - `f120903` (feat)

## Files Created/Modified
- `wwwroot/js/sortable.min.js` - SortableJS v1.15.7 minified library (45KB)
- `wwwroot/js/template-builder.js` - Custom JS interop module with initSortable, refreshSortable, destroySortable
- `Components/App.razor` - Added sortable.min.js script reference before blazor.web.js
- `Components/Pages/TemplateBuilder.razor` - Added exercise-list-sortable class, data-section wrapper divs, @key on ExerciseRows, @onkeydown handler
- `Components/Pages/TemplateBuilder.razor.cs` - IJSRuntime injection, OnAfterRenderAsync lifecycle, OnItemReordered with D-20 cross-section support, HandleKeyDown, IAsyncDisposable
- `Components/Pages/TemplateBuilder.razor.css` - drag-placeholder, drag-chosen, drag-active CSS styles using design tokens

## Decisions Made
- SortableJS loaded as a global script (not ES module) because the template-builder.js interop module references Sortable as a global constructor -- this avoids module import complexity for a library that expects global scope
- DOM revert pattern chosen to prevent Blazor/SortableJS DOM ownership conflicts: SortableJS's DOM mutations are undone in onEnd, and .NET state update triggers Blazor to re-render correctly
- Section detection uses backward scan from drop position in the flat children list, finding the nearest element with data-section attribute -- this works because section headers are interleaved with exercise rows in the same container

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## Known Stubs
None - all functionality is fully wired.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 03 (workout-templates) is now complete with all 5 plans executed
- Template builder has full CRUD, sections, grouping, drag-and-drop, and undo/redo
- Ready for Phase 04 (calendar-scheduler) which will use templates for scheduling workouts

## Self-Check: PASSED

- All 6 created/modified files verified present on disk
- Both task commits (a5da96d, f120903) verified in git log
- Build: 0 errors, 0 warnings
- Tests: 50/50 passed

---
*Phase: 03-workout-templates*
*Completed: 2026-03-21*
