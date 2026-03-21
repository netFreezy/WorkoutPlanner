# Phase 4: Calendar & Scheduling - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Calendar UI with weekly and monthly views, scheduling workouts from templates or ad-hoc, recurrence rule management, and materialization of scheduled workout rows. Users see their training week at a glance and can plan ahead. No session logging, no analytics — those are separate phases. Conflict detection (SCHED-05) is explicitly out of scope — user manages their own training logic.

</domain>

<decisions>
## Implementation Decisions

### Calendar layout
- **D-01:** Day columns for weekly view — 7 columns (Mon–Sun) with workouts stacked vertically in each day. No time grid since ScheduledWorkout has date only, no time.
- **D-02:** Mini calendar grid for monthly overview — traditional 7x5 grid with small date cells, each showing 1-3 colored dots for workout types (blue=strength, green=endurance, purple=mixed). Click a day to jump to that week.
- **D-03:** Mobile layout collapses to a day list — vertical list of days with workouts under each, instead of 7 cramped columns.
- **D-04:** Default to current week with today highlighted. Weekly is primary view, monthly is toggle/secondary panel. Navigation via prev/next arrows + "Today" jump button.

### Scheduling interaction
- **D-05:** Click empty day cell (or "+" in cell) opens schedule dialog with template picker and date pre-filled. FAB also available as alternative entry point.
- **D-06:** Ad-hoc workouts are name-only placeholders — just a label like "Rest day yoga" with no structured exercises. Not a mini builder.
- **D-07:** Click scheduled workout opens detail view showing template name, exercises preview, date, recurrence info. Actions: edit date, remove, skip occurrence, edit recurrence.
- **D-08:** Drag-to-reschedule if feasible — drag a workout chip to another day to change its date. Falls back to date edit in dialog if too complex.
- **D-09:** Compact chips in day cells — template name + type indicator (colored left border). Multiple workouts stack vertically.

### Recurrence UI & materialization
- **D-10:** Repeat toggle with inline options in the schedule dialog. Reveals: "Every week on [day chips]" (Weekly), "Every X days" (Daily with interval), matching FrequencyType enum. Day chips are clickable toggles (Mon–Sun) matching DaysOfWeek flags.
- **D-11:** Small ↻ repeat icon on recurring workout chips to distinguish from one-off workouts.
- **D-12:** Materialization triggers on both save (immediate feedback) and page load (rolling window catch-up). 4-week window per Phase 1 D-12.
- **D-13:** Recurring workout actions: "Skip this one" (mark occurrence as Skipped), "Remove all" (delete rule + future rows), "Edit schedule" (change recurrence, regenerates future rows per Phase 1 D-13).

### Conflict detection
- **D-14:** Skipped entirely. No muscle group overlap warnings, no rest day flagging. User manages their own training logic. SCHED-05 is out of scope for this phase.

### Claude's Discretion
- Calendar page layout details (sidebar vs. top toggle for weekly/monthly)
- Chip color scheme and type indicator styling
- Template picker dialog design (dropdown vs. searchable list)
- Monthly dot colors and sizing
- Materialization service implementation (inline vs. background service)
- Mobile day list interaction details
- Animation for adding/moving workout chips

</decisions>

<specifics>
## Specific Ideas

- Weekly day columns should feel clean and scannable — your whole training week visible at a glance is the core value
- FAB provides an always-available scheduling entry point, consistent with Exercises and Templates pages
- Compact chips keep the calendar uncluttered even with daily workouts
- Ad-hoc name-only scheduling is intentionally lightweight — for things that don't need a full template

</specifics>

<canonical_refs>
## Canonical References

### Planning artifacts
- `.planning/PROJECT.md` — Core value ("whole training week in one view"), constraints (Blazor Server, no JS frameworks, SQLite)
- `.planning/REQUIREMENTS.md` — SCHED-01 through SCHED-06 define Phase 4 deliverables (SCHED-05 deferred)
- `.planning/ROADMAP.md` §Phase 4 — Success criteria (5 verification points)

### Phase 1 context (data model decisions)
- `.planning/phases/01-data-foundation/01-CONTEXT.md` — D-10 through D-13 define recurrence storage (FrequencyType, DaysOfWeek flags, 4-week window, edit-regenerates-all)
- `Data/Entities/ScheduledWorkout.cs` — ScheduledWorkout and RecurrenceRule entities
- `Data/Enums/FrequencyType.cs` — Daily, Weekly, Custom
- `Data/Enums/DaysOfWeek.cs` — Flags enum with Mon–Sun, Weekdays, Weekend, EveryDay
- `Data/Enums/WorkoutStatus.cs` — Planned, Completed, Skipped

### Phase 3 context (template system)
- `.planning/phases/03-workout-templates/03-CONTEXT.md` — Template list, builder, and detail patterns
- `Data/Entities/WorkoutTemplate.cs` — Template entity with Items, Groups, Tags

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Components/Shared/Dialog.razor` — Base modal for schedule dialog and workout detail view
- `Components/Shared/Toast.razor` — Notifications for schedule/delete/skip actions
- `Components/Shared/FilterChip.razor` — Could reuse for day-of-week toggle chips in recurrence UI
- `Components/Pages/Templates.razor` — Pattern for list page with FAB, search, DB loading
- `Components/Shared/TemplateDetailDialog.razor` — Pattern for showing workout structure in a detail view
- `wwwroot/js/template-builder.js` — SortableJS interop pattern, could extend for drag-to-reschedule

### Established Patterns
- `IDbContextFactory<AppDbContext>` injection with `using var context` per operation
- Dark theme design tokens in `wwwroot/app.css` — all spacing, colors, radii, shadows
- FAB: fixed bottom-right, 56px circle, gradient background
- Dialog: fixed overlay with backdrop blur, max-width panel
- Card/chip visual patterns from exercise and template pages

### Integration Points
- `Components/Layout/MainLayout.razor` — Add "Calendar" NavLink
- `Components/Pages/` — New Calendar.razor (calendar views) and ScheduleDialog
- `Data/AppDbContext.cs` — Already has `DbSet<ScheduledWorkout>` and `DbSet<RecurrenceRule>`
- `Data/Configurations/ScheduleConfiguration.cs` — Already configured relationships
- Materialization logic needs a service or page-load hook to generate ScheduledWorkout rows from RecurrenceRules

</code_context>

<deferred>
## Deferred Ideas

- Conflict detection / rest day warnings (SCHED-05) — intentionally skipped, user manages their own training logic. Could revisit as optional toggle in a future phase.

</deferred>

---

*Phase: 04-calendar-scheduling*
*Context gathered: 2026-03-21*
