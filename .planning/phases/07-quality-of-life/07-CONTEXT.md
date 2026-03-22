# Phase 7: Quality of Life - Context

**Gathered:** 2026-03-22
**Status:** Ready for planning

<domain>
## Phase Boundary

Streamline the daily workout workflow: transform the empty home page into a quick-start dashboard, add progressive overload suggestions for strength exercises, enable CSV/PDF data export, and provide a searchable workout history browser. This phase makes the app feel complete for daily use.

</domain>

<decisions>
## Implementation Decisions

### Home Screen Dashboard
- **D-01:** Full workout preview — show today's scheduled workout with exercise list, targets (sets/reps/weight or distance/duration), and a prominent "Start Session" button
- **D-02:** "Up next" line below the workout showing tomorrow's scheduled workout for context
- **D-03:** When no workout is scheduled: show last completed workout with actual logged weights (not template targets), plus a "Browse Templates" option to pick a different workout
- **D-04:** No summary stats on home screen — keep it purely action-focused. Analytics page handles stats

### Progressive Overload Suggestions
- **D-05:** Trigger after 2 consecutive sessions where all working sets hit target reps at the same weight for that exercise
- **D-06:** Show as inline suggestion card at session start — appears per exercise with "Apply" (updates the target weight for this session) and "Dismiss" actions
- **D-07:** Weight increments based on exercise muscle group: +2.5kg for upper body compounds (bench, rows, OHP, pull-ups), +5kg for lower body compounds (squat, deadlift, leg press), +1kg for isolation exercises (curls, laterals, triceps)
- **D-08:** Strength exercises only — no progressive overload suggestions for endurance exercises

### Export (CSV + PDF)
- **D-09:** CSV export = one row per set/entry with full detail. Strength columns: Date, Workout, Exercise, SetNum, PlannedWeight, PlannedReps, ActualWeight, ActualReps, SetType, RPE, Notes. Endurance columns: Date, Workout, Exercise, PlannedDistance, PlannedDuration, ActualDistance, ActualDuration, ActualPace, HRZone, RPE, Notes
- **D-10:** Export buttons on the analytics page, reusing the existing time range selector (4W/8W/12W/All) for date filtering
- **D-11:** PDF = training summary report with period overview (sessions completed, adherence %, PRs hit, total volume) + chronological per-session breakdown

### Workout History Browser
- **D-12:** Rich summary cards: date, workout name, type icon (strength/endurance), exercise count, duration, total volume (strength) or distance (endurance), RPE badge
- **D-13:** Four filter options: text search (workout/exercise name), date range picker, workout type filter (strength/endurance/mixed), exercise filter (sessions containing a specific exercise)
- **D-14:** Expandable inline detail — tap a card to reveal full exercise breakdown (sets with reps/weight, endurance entries) and session notes. Collapse to return to list

### Claude's Discretion
- History page placement in nav (new nav link vs sub-route of analytics)
- PDF generation library choice (server-side rendering approach)
- CSV file naming convention and encoding
- Overload suggestion dismissal persistence (per-session vs remembered)
- History pagination vs infinite scroll
- Empty states for home screen (no workouts ever created)

</decisions>

<canonical_refs>
## Canonical References

No external specs — requirements are fully captured in decisions above and REQUIREMENTS.md (QOL-01 through QOL-06).

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Components/Shared/Toast.razor` — Toast notification pattern (3s auto-hide, blur backdrop) — reuse for overload suggestion dismissal feedback
- `Components/Shared/WorkoutDetailDialog.razor` — Shows workout preview with exercise list + "Start Session" button — home screen can reuse this rendering pattern
- `Components/Shared/KpiCard.razor` — Glassmorphism card component — reuse for history summary cards
- `Services/AnalyticsService.cs` — Has `GetWeeklyVolumeAsync`, `GetAdherenceAsync`, `GetAllPRsAsync` — reuse for PDF summary stats

### Established Patterns
- `[Inject] IDbContextFactory<AppDbContext>` — All services use factory pattern, not scoped DbContext
- `SessionService.GetPreviousStrengthPerformanceAsync()` — Already queries past performance per exercise — base for overload detection
- Tab UI pattern in `Analytics.razor` — Reuse for history filters or sub-views
- Time range selector in `Analytics.razor` — Reuse for export date range and history date filter
- `::deep` required in parent `.razor.css` for InputText/InputSelect styling

### Integration Points
- `Home.razor` — Currently empty placeholder, becomes the dashboard
- `MainLayout.razor` — Nav links (currently 6) — may need "History" added
- `SessionService.StartSessionAsync()` — Entry point for quick-start flow from home screen
- `SchedulingService` — Query today's scheduled workouts for home screen
- `Analytics.razor` — Add export buttons to existing page

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 07-quality-of-life*
*Context gathered: 2026-03-22*
