---
phase: 06-analytics
plan: 03
subsystem: ui
tags: [blazor, apex-charts, analytics, dashboard, kpi, charts, razor-components]

# Dependency graph
requires:
  - phase: 06-analytics
    provides: AnalyticsService with weekly volume/endurance/adherence/deviation queries, PRDetectionService, BlazorApexCharts 6.1.0 with dark theme
  - phase: 05-session-tracking
    provides: WorkoutLog and SetLog/EnduranceLog entities populated by session logging
provides:
  - Analytics dashboard page at /analytics with 4 tabs (Overview, Strength, Endurance, PRs stub)
  - KpiCard reusable component with glassmorphism styling
  - Time range selector (4W/8W/12W/All) updating all charts
  - Exercise drill-down dropdowns for Strength and Endurance tabs
  - Empty states with contextual messaging per tab
  - Analytics NavLink in main navigation bar
affects: [06-analytics plan 04 (PRs tab implementation)]

# Tech tracking
tech-stack:
  added: []
  patterns: [ApexChart XValue single-quote attribute wrapping for Razor quote escaping, per-chart ApexChartOptions instances (never shared), @() expression syntax for lambda attributes containing string literals]

key-files:
  created:
    - Components/Pages/Analytics.razor
    - Components/Pages/Analytics.razor.cs
    - Components/Pages/Analytics.razor.css
    - Components/Shared/KpiCard.razor
    - Components/Shared/KpiCard.razor.css
  modified:
    - Components/Layout/MainLayout.razor

key-decisions:
  - "Single-quote attribute delimiters for XValue lambdas to avoid Razor double-quote escaping issues"
  - "HasAdherenceData helper added beyond plan spec to control empty state logic for adherence chart independently"
  - "Reused existing stat-card pattern from SessionSummary for KpiCard glassmorphism styling"

patterns-established:
  - "ApexChart XValue lambda pattern: XValue='@(e => e.WeekStart.ToString(\"MMM dd\"))' for Razor compatibility"
  - "Tab state management via string activeTab field with SwitchTab async method pattern"
  - "KpiCard component with --card-index CSS variable for stagger animation"

requirements-completed: [ANLY-01, ANLY-03, ANLY-04, ANLY-05]

# Metrics
duration: 3min
completed: 2026-03-22
---

# Phase 06 Plan 03: Analytics Dashboard UI Summary

**Analytics dashboard with tabbed layout (Overview/Strength/Endurance/PRs), KPI cards, interactive ApexCharts (volume, adherence, max weight, e1RM, distance, pace), time range selector, and exercise drill-down dropdowns**

## Performance

- **Duration:** 3 min
- **Started:** 2026-03-22T11:07:47Z
- **Completed:** 2026-03-22T11:11:00Z
- **Tasks:** 1
- **Files modified:** 6

## Accomplishments
- Full analytics dashboard page at /analytics with 4 tabbed sections and interactive chart rendering via BlazorApexCharts
- KpiCard reusable component showing sessions this week, current streak, total volume, and latest PR with glassmorphism card styling and stagger animation
- Overview tab with weekly volume bar chart (blue) and adherence grouped bar chart (green completed, ghost planned)
- Strength tab with exercise drill-down dropdown, volume/max weight/estimated 1RM charts for per-exercise analysis
- Endurance tab with activity drill-down dropdown, distance and pace trend charts
- Time range selector (4W/8W/12W/All) that reloads all chart data on change
- Responsive layout: 4-column KPI grid on desktop, 2-column on mobile; scrollable tab bar on mobile
- Empty states with contextual copy per tab type (no data, no range data, no strength data, no endurance data)
- Analytics NavLink added to main navigation bar

## Task Commits

Each task was committed atomically:

1. **Task 1: KpiCard component, NavLink, and Analytics page with Overview tab (KPI cards + volume + adherence charts)** - `aabde46` (feat)

## Files Created/Modified
- `Components/Shared/KpiCard.razor` - Reusable KPI card with label, value, subtitle parameters and stagger animation via --card-index
- `Components/Shared/KpiCard.razor.css` - Glassmorphism card styling matching stat-card pattern from SessionSummary
- `Components/Pages/Analytics.razor` - Main analytics page with 4 tabs, 12 ApexChart instances, time range selector, exercise dropdowns, empty states
- `Components/Pages/Analytics.razor.cs` - Code-behind with data loading, tab/time range state, chart options initialization, exercise drill-down handlers
- `Components/Pages/Analytics.razor.css` - Scoped styles for page layout, tabs, time range, chart containers, exercise select, empty states, responsive breakpoints
- `Components/Layout/MainLayout.razor` - Added Analytics NavLink after Session link

## Decisions Made
- Used single-quote attribute delimiters (`XValue='@(...)'`) for ApexChart XValue lambdas to resolve Razor double-quote escaping issue where `ToString("MMM dd")` inner quotes terminated the attribute
- Added `HasAdherenceData` helper property beyond plan spec to independently control adherence chart vs empty state display
- Used `Mode.Dark` enum value (from Plan 01 decision) consistently for ApexCharts theme configuration

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed Razor quote escaping in ApexChart XValue attributes**
- **Found during:** Task 1 (Analytics page implementation)
- **Issue:** XValue lambdas like `XValue="e => e.WeekStart.ToString("MMM dd")"` caused build errors because inner double quotes terminated the outer attribute delimiter
- **Fix:** Wrapped all XValue lambdas with `@()` expression syntax; linter further converted to single-quote delimiters: `XValue='@(e => e.WeekStart.ToString("MMM dd"))'`
- **Files modified:** Components/Pages/Analytics.razor
- **Verification:** dotnet build succeeds with 0 errors
- **Committed in:** aabde46 (Task 1 commit)

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Standard Razor syntax fix necessary for compilation. No scope creep.

## Issues Encountered
None beyond the Razor quote escaping issue documented above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- PRs tab is stubbed and ready for Plan 04 implementation
- All chart infrastructure and patterns established for reuse
- All 99 existing tests pass (no regressions)

## Known Stubs

| File | Line | Stub | Reason |
|------|------|------|--------|
| Components/Pages/Analytics.razor | ~289 | PRs tab placeholder text "coming in next plan" | Plan 04 implements full PRs tab with PR record book and timeline chart |

## Self-Check: PASSED

---
*Phase: 06-analytics*
*Completed: 2026-03-22*
