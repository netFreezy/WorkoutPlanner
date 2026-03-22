# Phase 6: Analytics - Context

**Gathered:** 2026-03-22
**Status:** Ready for planning

<domain>
## Phase Boundary

Unified analytics dashboard for strength and endurance training. Users can view volume trends, automatically detected personal records, adherence/consistency metrics, and endurance pace/distance trends. Per-exercise drill-down provides individual exercise history. This phase does NOT include progressive overload suggestions, export, or quick-start (Phase 7).

</domain>

<decisions>
## Implementation Decisions

### Chart rendering
- **D-01:** Use a Blazor charting library (e.g. BlazorApexCharts or Radzen Charts) — JS interop under the hood is acceptable as long as the API is C#/Razor
- **D-02:** Charts must be interactive with hover tooltips showing exact values
- **D-03:** Chart type per metric is Claude's discretion — pick what fits (line for trends, bar for volume, etc.)
- **D-04:** Theming: good-enough match to the dark premium theme — no need for pixel-perfect custom SVG

### Dashboard layout
- **D-05:** Tabbed layout — segmented sections (e.g. Overview / Strength / Endurance / PRs), not a single scrolling page
- **D-06:** Summary KPI cards at the top of the Overview tab (total sessions this week, current streak, latest PR, etc.)
- **D-07:** Designed for both quick-glance ("how's my week") and weekly review depth
- **D-08:** Per-exercise drill-down — users can select a specific exercise and see its history with weekly-aggregated data points

### PR detection & display
- **D-09:** PRs detected inline on session finish (not background scan) — immediate feedback
- **D-10:** Strength PRs: heaviest weight, most reps at a given weight, and estimated 1RM (Epley formula: weight x (1 + reps/30))
- **D-11:** Endurance PRs: fastest pace and longest distance, tracked per activity type (running separate from cycling, etc.)
- **D-12:** Dedicated PR section/tab showing PRs across all exercises the user has actually done — if they only run, they only see running PRs
- **D-13:** PR timeline showing when each record was set

### Time range & aggregation
- **D-14:** Default time window: last 4 weeks
- **D-15:** User-selectable range: 4W / 8W / 12W / All
- **D-16:** Aggregation granularity: weekly buckets for all charts
- **D-17:** Empty/skipped weeks show as gaps (zero bars) in charts — don't compress the timeline

### Claude's Discretion
- Specific charting library choice (BlazorApexCharts vs Radzen vs other)
- Exact tab names and ordering
- KPI card selection and layout
- Chart colors beyond the existing strength (blue) / endurance (green) tokens
- Per-exercise drill-down UX (separate page vs. modal vs. inline expand)
- 1RM formula details and edge case handling

</decisions>

<specifics>
## Specific Ideas

- Summary cards should give a quick pulse — "how's my week going" at a glance before diving into charts
- PR section should feel like a record book — show all exercises you've done, their PRs, and when they were set
- The tab structure keeps things organized without overwhelming — strength people see strength data, runners see running data

</specifics>

<canonical_refs>
## Canonical References

No external specs — requirements are fully captured in decisions above and ROADMAP.md success criteria.

### Relevant codebase references
- `Services/SessionService.cs` — GetPreviousStrengthPerformanceAsync, GetPreviousEndurancePerformanceAsync (existing historical query patterns)
- `Data/Entities/SetLog.cs` — PlannedWeight/ActualWeight, PlannedReps/ActualReps, SetType, IsCompleted
- `Data/Entities/EnduranceLog.cs` — Distance, Duration, Pace, ActivityType (per-activity-type PR separation)
- `Data/Entities/WorkoutLog.cs` — StartedAt, CompletedAt, Rpe, Notes
- `Data/Entities/ScheduledWorkout.cs` — ScheduledDate, Status (Planned/Completed/Skipped for adherence)
- `wwwroot/app.css` — Design tokens including --color-strength-*, --color-endurance-*, --color-rpe-*

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- **Color tokens**: --color-strength-bg/text/border (blue), --color-endurance-bg/text/border (green) — direct mapping to tab/chart coloring
- **RPE color scale**: --color-rpe-low/mid/high — can be reused for intensity overlays
- **Card pattern**: Glassmorphism cards used across exercise library and templates — reuse for KPI summary cards
- **Toast system**: Existing toast notifications — can surface "New PR!" on session finish

### Established Patterns
- **IDbContextFactory**: All services use factory pattern for thread-safe DB access in Blazor Server
- **Scoped CSS**: Every component has co-located .razor.css with ::deep where needed
- **SchedulingService queries**: GetWorkoutsForWeekAsync/GetWorkoutsForMonthAsync — pattern for time-range queries
- **DetermineWorkoutType()**: Static method classifying workouts as Strength/Endurance/Mixed — reuse for analytics categorization

### Integration Points
- Session finish flow (SessionService.FinishSessionAsync) — hook for inline PR detection
- MainLayout.razor nav — add Analytics link
- Existing tab/segmented control patterns from prior phases

</code_context>

<deferred>
## Deferred Ideas

- Progressive overload suggestions based on PR trends — Phase 7
- Export analytics data as CSV/PDF — Phase 7
- Comparison views (this month vs. last month) — backlog

</deferred>

---

*Phase: 06-analytics*
*Context gathered: 2026-03-22*
