# Phase 5: Session Tracking - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Users can log workouts in real time with type-appropriate inputs, previous performance context, and full resilience against connection loss. Starting from a scheduled workout, the user sees pre-filled targets, logs sets/endurance data, marks exercises complete/partial/skipped, rates the session with RPE and notes, and can resume after any interruption.

</domain>

<decisions>
## Implementation Decisions

### Strength logging interaction
- **D-01:** Pre-filled editable set rows — each set shows planned weight/reps pre-filled from template snapshot. Tap the number to edit, tap checkmark to complete the set.
- **D-02:** Set type defaults to Working. Tap a small type label to cycle through: Working → Warm-up → Failure → Drop. Minimal UI footprint.
- **D-03:** Users can add extra sets beyond the template target via a "+" button after the last planned set. New sets pre-fill with the same weight as the previous set.
- **D-04:** Previous performance shown as expandable drawer — tap "▶ Previous" to expand last 3 sessions with sets/weight/reps per session. Hidden by default to keep logging view clean.

### Endurance logging interaction
- **D-05:** No built-in timer/stopwatch. Just input fields for duration, distance, and pace after the activity. Users use their phone clock or watch for timing.
- **D-06:** Distance + duration are manual inputs, pace is auto-calculated from those two values. HR zone is optional (1-5 select or skip).
- **D-07:** Previous endurance performance uses the same expandable drawer pattern as strength — last 3 sessions with distance/duration/pace.

### Session flow & navigation
- **D-08:** Hybrid navigation: exercise list always visible as compact rows. Tap one to expand its set entry inline. Only one exercise expanded at a time. Completed exercises show summary (e.g. "3/3 sets"), pending ones show "pending".
- **D-09:** Two entry points for starting a session: (1) "Start Session" button in the calendar's WorkoutDetailDialog, (2) a dedicated /session page listing today's scheduled workouts.
- **D-10:** Exercise completion status via manual toggle — explicit Complete / Partial / Skip buttons per exercise. User decides the status regardless of set completion state.
- **D-11:** End-of-session summary screen after completing the last exercise. Shows total volume, duration, and asks for RPE (1-10 slider) and free-text notes. "Finish Session" button to close out.
- **D-12:** Session page shows a progress bar at top: "3/5 exercises" with filled/empty segments.

### Resilience & persistence
- **D-13:** Save on every set completion — each time the user taps ✓ on a set or saves endurance data, it's immediately persisted to the database. Most resilient approach.
- **D-14:** No rest timer. Users manage their own rest between sets.
- **D-15:** Auto-navigate to incomplete session — if an incomplete WorkoutLog exists (StartedAt set, CompletedAt null), automatically redirect to the session page with it loaded. No prompt needed.
- **D-16:** Abandon session marks as partial — keeps whatever sets were logged, sets CompletedAt, marks ScheduledWorkout as Completed with partial data. No data is discarded.

### Claude's Discretion
- Session page URL structure and routing
- Set row component design details (spacing, input sizing)
- How the progress bar segments are styled
- Summary screen layout and volume calculation logic
- How the exercise expand/collapse animation works
- Exact previous performance query strategy (eager load vs on-demand)

</decisions>

<canonical_refs>
## Canonical References

No external specs — requirements are fully captured in decisions above and the following planning artifacts:

### Data model (already built in Phase 1)
- `Data/Entities/WorkoutLog.cs` — WorkoutLog, SetLog, EnduranceLog entities with planned-vs-actual columns
- `Data/Entities/ScheduledWorkout.cs` — ScheduledWorkout entity with WorkoutLog navigation property
- `Data/Entities/WorkoutTemplate.cs` — WorkoutTemplate, TemplateItem with strength/endurance targets
- `Data/Configurations/LogConfiguration.cs` — EF Core configurations for log entities
- `Data/Enums/SetType.cs` — WarmUp, Working, Failure, Drop
- `Data/Enums/WorkoutStatus.cs` — Planned, Completed, Skipped

### Prior phase decisions
- `.planning/phases/01-data-foundation/01-CONTEXT.md` §D-14,D-15,D-16 — Snapshot strategy (deep copy targets into log rows)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Components/Shared/Dialog.razor` — Base dialog component for the summary/abandon confirmation dialogs
- `Components/Shared/Toast.razor` — Toast notifications for save confirmations
- `Components/Shared/WorkoutDetailDialog.razor` — Already shows workout details; "Start Session" button should be added here
- `Services/SchedulingService.cs` — Has `GetWorkoutsForWeekAsync` and `DetermineWorkoutType` for loading today's workouts

### Established Patterns
- `IDbContextFactory<AppDbContext>` — All services use factory pattern, session service should follow
- `::deep` in parent `.razor.css` for styling InputText/InputSelect (per project feedback)
- Scoped CSS with `.razor.css` files for all components
- `@inject` for service injection, `@rendermode InteractiveServer` for interactive pages

### Integration Points
- `ScheduledWorkout.WorkoutLog` navigation property — already configured as one-to-one
- `WorkoutDetailDialog` — needs "Start Session" button added
- Calendar page — scheduled workouts with Status=Completed should show differently
- `WorkoutTemplate.Items` → `TemplateItem` with targets → snapshot into `SetLog`/`EnduranceLog` planned columns

</code_context>

<deferred>
## Deferred Ideas

- Built-in rest timer — decided to skip for now (D-14), could add in QoL phase
- Superset/EMOM group-aware logging (log exercises in group order) — future enhancement
- Live timer/stopwatch for endurance — users use external timing for now (D-05)

</deferred>

---

*Phase: 05-session-tracking*
*Context gathered: 2026-03-21*
