---
phase: 02-exercise-library
plan: 04
subsystem: ui
tags: [css-custom-properties, design-tokens, dark-mode, glassmorphism, animations, inter-font]

# Dependency graph
requires:
  - phase: 02-exercise-library (02-02)
    provides: Exercise page UI with filter bar, card grid, FAB, dialogs, toast
  - phase: 02-exercise-library (02-03)
    provides: Form dialog with polymorphic create flow
provides:
  - Global CSS custom properties (design tokens) for all future phases
  - Dark-mode premium design system applied to entire exercise library
  - Inter font via Google Fonts CDN
  - Global keyframe animations (fadeSlideIn, dialogSlideIn, chipIn, fabIn, toastIn, toastOut, fadeIn, shimmer)
  - Staggered card entry animation with --card-index CSS variable
  - Card hover lift with type tag glow effects
  - Glassmorphism on nav bar and filter bar
  - Frosted dialog overlay with spring slide-in animation
  - Gradient FAB with glow hover and spin-in entrance
  - Accent-purple filter chips with entrance animation
  - Gradient submit button with glow hover
  - Glass toast with slide-in animation
affects: [03-workout-templates, 04-calendar, 05-session-tracker, 06-analytics]

# Tech tracking
tech-stack:
  added: [Google Fonts Inter 400/600]
  patterns: [CSS custom properties as design tokens, prefers-reduced-motion wrapping, scoped component CSS referencing global tokens]

key-files:
  created: []
  modified:
    - wwwroot/app.css
    - Components/Layout/MainLayout.razor.css
    - Components/Pages/Exercises.razor
    - Components/Pages/Exercises.razor.css
    - Components/Shared/ExerciseCard.razor
    - Components/Shared/ExerciseCard.razor.css
    - Components/Shared/Dialog.razor.css
    - Components/Shared/ExerciseFormDialog.razor.css
    - Components/Shared/FilterChip.razor.css
    - Components/Shared/Toast.razor.css

key-decisions:
  - "Global keyframes in app.css inside prefers-reduced-motion media query rather than duplicated in component CSS files"
  - "Card stagger animation via --card-index CSS variable set inline on ExerciseCard, capped at 10 slots"

patterns-established:
  - "Design tokens: All visual values reference CSS custom properties (var(--*)) on :root, never hardcoded hex values in component CSS"
  - "Animation pattern: All @keyframes and multi-property transitions wrapped in @media (prefers-reduced-motion: no-preference)"
  - "Glassmorphism pattern: backdrop-filter: blur(Npx) with semi-transparent dark background for elevated surfaces"

requirements-completed: [EXER-01, EXER-02]

# Metrics
duration: 6min
completed: 2026-03-21
---

# Phase 02 Plan 04: Dark-Mode Design System Summary

**Premium dark-mode design system with CSS custom properties, Inter font, glassmorphism nav/filter bar, card hover lift with type-tag glow, staggered entry animations, frosted dialog overlay, gradient FAB/submit, and accent-purple filter chips**

## Performance

- **Duration:** 6 min
- **Started:** 2026-03-21T17:02:04Z
- **Completed:** 2026-03-21T17:08:24Z
- **Tasks:** 2
- **Files modified:** 10

## Accomplishments
- Global design tokens in app.css with 50+ CSS custom properties matching UI-SPEC exactly, body dark-mode styling, antialiased Inter font rendering, thin dark scrollbars
- All 8 global keyframe animations defined inside prefers-reduced-motion media query (fadeSlideIn, dialogSlideIn, chipIn, fabIn, toastIn, toastOut, fadeIn, shimmer)
- Validation CSS updated from hardcoded hex to design token references
- All component CSS files refactored to use design tokens with premium visual effects: card hover lift with translateY(-2px) and type-tag glow, staggered fadeSlideIn entry, glassmorphism on nav/filter bar, frosted dialog overlay with spring slide-in, gradient FAB with glow and spin entrance, accent-purple filter chips with chipIn animation, gradient submit button with glow hover, glass toast with slide-in

## Task Commits

Each task was committed atomically:

1. **Task 1: Set up global design system** - `14544cb` (feat)
2. **Task 2: Refactor all component CSS to dark-mode with premium effects** - `809ea5b` (feat)

## Files Created/Modified
- `wwwroot/app.css` - Global design tokens, body styling, scrollbar styling, global keyframe animations, validation token references
- `Components/Layout/MainLayout.razor.css` - Dark glassmorphism nav bar with accent active indicator
- `Components/Pages/Exercises.razor` - search-input-wrapper class, Index parameter on ExerciseCard, removed wrapping div
- `Components/Pages/Exercises.razor.css` - Sticky filter bar with glassmorphism, gradient FAB with glow, staggered card grid
- `Components/Shared/ExerciseCard.razor` - Index parameter, --card-index CSS variable for stagger animation
- `Components/Shared/ExerciseCard.razor.css` - fadeSlideIn staggered animation, card hover lift, type-tag glow on hover
- `Components/Shared/Dialog.razor.css` - Frosted overlay with fadeIn, spring slide-in dialogSlideIn, z-index tokens
- `Components/Shared/ExerciseFormDialog.razor.css` - Placeholder styling, fadeIn for conditional fields, gradient submit
- `Components/Shared/FilterChip.razor.css` - Accent-purple chips with chipIn animation, remove button styling
- `Components/Shared/Toast.razor.css` - Glass toast with prefers-reduced-motion wrapped toastIn

## Decisions Made
- Global keyframes defined in app.css rather than duplicated in individual component CSS files, since global @keyframes are accessible from scoped component CSS via Blazor's CSS isolation
- Card stagger achieved via --card-index CSS variable set inline on ExerciseCard component, modulo 10 to cap at 500ms max stagger delay
- Kept #FFFFFF for FAB icon color (white on gradient accent) as this is intentional per UI-SPEC, not a light-theme remnant

## Deviations from Plan
None - plan executed exactly as written. The existing component CSS was already partially using design tokens from plans 02-02 and 02-03; this plan completed the refactoring by adding missing effects (staggered animation, placeholder styling, fadeIn for form groups) and ensuring exact alignment with UI-SPEC values.

## Issues Encountered
- Build errors (MSB3021/MSB3027) during verification due to running BlazorApp2 process locking the output binary -- these are file lock errors only, not compilation errors. All code compiles cleanly (no CS errors).

## Known Stubs
None - all CSS is fully wired to design tokens with no placeholder values.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 02 (exercise-library) is now complete with all 4 plans executed
- Full dark-mode design system ready for Phase 03 (workout-templates) to build upon
- CSS custom properties established as the pattern for all future component styling

## Self-Check: PASSED

All 10 files verified present. Both task commits (14544cb, 809ea5b) verified in git log.

---
*Phase: 02-exercise-library*
*Completed: 2026-03-21*
