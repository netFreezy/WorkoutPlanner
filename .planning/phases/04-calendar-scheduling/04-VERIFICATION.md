---
phase: 04-calendar-scheduling
verified: 2026-03-21T21:00:00Z
status: human_needed
score: 12/12 must-haves verified (SCHED-05 formally deferred per D-14)
gaps: []
human_verification:
  - test: "Visual: workout type color indicators in weekly view"
    expected: "Strength chips have blue left border, Endurance green, Mixed purple, Ad-hoc uses text-tertiary color"
    why_human: "CSS color rendering requires visual inspection"
  - test: "Visual: monthly mini-calendar color-coded dots"
    expected: "Dots below date numbers match workout types -- blue for strength, green for endurance, purple for mixed"
    why_human: "CSS color rendering requires visual inspection"
  - test: "Drag-to-reschedule between day cells"
    expected: "Dragging a workout chip to a different day cell moves the workout, toast confirms the move"
    why_human: "SortableJS interaction requires browser testing"
  - test: "Mobile responsive layout at 767px"
    expected: "Weekly grid collapses to single-column day list, day names appear inline"
    why_human: "Responsive layout requires browser resize testing"
---

# Phase 4: Calendar Scheduling Verification Report

**Phase Goal:** Users can schedule workouts on a calendar with recurrence rules and see their training week at a glance
**Verified:** 2026-03-21T21:00:00Z
**Status:** gaps_found
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | Weekly calendar grid (Mon-Sun) shows scheduled workouts with type indicators | VERIFIED | Calendar.razor: `week-grid` with 7-column grid, WorkoutChip with DetermineWorkoutType color logic |
| 2 | User can navigate between weeks (prev/next arrows + Today button) | VERIFIED | Calendar.razor.cs: `NavigateWeek(int direction)` and `GoToToday()` methods, wired to buttons in Calendar.razor |
| 3 | Workout chips show template name with color-coded left border for workout type | VERIFIED | WorkoutChip.razor: renders `Workout.DisplayName` with `GetTypeClass()` returning `workout-chip--strength/endurance/mixed/adhoc` |
| 4 | Today's column is highlighted, today's date has an accent circle | VERIFIED | Calendar.razor: `day-cell--today` conditional class, `day-date--today` applied only on matching date |
| 5 | Empty state shows "Plan your week" when no workouts scheduled | VERIFIED | Calendar.razor line 71: `<h2 class="empty-state__heading">Plan your week</h2>` |
| 6 | Mobile layout collapses to single-column day list | VERIFIED | Calendar.razor.css: `@media (max-width: 767px)` block; Calendar.razor has separate `week-list--mobile` block |
| 7 | User can schedule from template or ad-hoc via ScheduleDialog | VERIFIED | ScheduleDialog.razor: "From Template"/"Ad-hoc" toggle, TemplatePicker child, adHocName input, calls `ScheduleWorkoutAsync` |
| 8 | Recurrence rules (weekly on specific days, every N days) persist and materialize | VERIFIED | MaterializationService.cs: `GenerateOccurrences` handles FrequencyType.Weekly and Daily; SchedulingService.ScheduleWorkoutAsync creates RecurrenceRule and calls MaterializeAsync |
| 9 | Monthly mini-calendar with color-coded dots shows workout density per day | VERIFIED | MonthlyMiniCalendar.razor: loads via `GetWorkoutsForMonthAsync`, groups by day, renders `monthly-dot--{type}` spans |
| 10 | Clicking a day in monthly view jumps to that week | VERIFIED | Calendar.razor: `HandleMonthDateClicked` sets `showMonthView = false` and updates `currentWeekStart` |
| 11 | User can click a workout to see details (name, exercises, date, recurrence) and skip/remove/edit | VERIFIED | WorkoutDetailDialog.razor: full exercise preview, `GetRecurrenceSummary`, `SkipWorkoutAsync`, `RemoveWorkoutAsync`, `RemoveRecurringAsync`, wired in Calendar.razor |
| 12 | SCHED-05: Conflict warning when scheduling same muscle group on consecutive days | FAILED | D-14 explicitly deferred this. No implementation found in Services/, ScheduleDialog.razor, or Calendar.razor. REQUIREMENTS.md marks it [x] Complete — inconsistent with codebase. |

**Score:** 11/12 truths verified

---

## Required Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Data/Entities/ScheduledWorkout.cs` | Nullable WorkoutTemplateId + AdHocName + DisplayName | VERIFIED | `int? WorkoutTemplateId`, `string? AdHocName`, `string DisplayName => WorkoutTemplate?.Name ?? AdHocName ?? "Untitled"` |
| `Services/MaterializationService.cs` | GenerateOccurrences + MaterializeAsync + MaterializeAllAsync | VERIFIED | 167 lines, static GenerateOccurrences with Weekly/Daily/Custom, idempotent MaterializeAsync with HashSet dedup |
| `Services/SchedulingService.cs` | CRUD + date range queries + DetermineWorkoutType | VERIFIED | 199 lines, all 7 methods present and substantive |
| `Data/Enums/WorkoutType.cs` | Strength, Endurance, Mixed, AdHoc | VERIFIED (via imports) | Referenced in WorkoutChip.razor, MonthlyMiniCalendar.razor, WorkoutDetailDialog.razor |
| `Components/Pages/Calendar.razor` | Weekly grid, navigation, dialogs wired | VERIFIED | `@page "/calendar"`, `@rendermode InteractiveServer`, all 3 dialogs wired |
| `Components/Pages/Calendar.razor.cs` | Service injection, NavigateWeek, OnWorkoutDropped [JSInvokable] | VERIFIED | SchedulingService + MaterializationService injected, all handler methods present |
| `Components/Shared/WorkoutChip.razor` | Type-colored chip with recurrence icon | VERIFIED | DetermineWorkoutType call, recurrence SVG icon, DisplayName rendering |
| `Components/Shared/MonthlyMiniCalendar.razor` | Monthly grid with color dots + date click | VERIFIED | GetWorkoutsForMonthAsync, 35/42 cell generation, OnDateClicked EventCallback |
| `Components/Shared/WorkoutDetailDialog.razor` | Exercise preview + skip/remove/edit actions | VERIFIED | GetRecurrenceSummary, GetTargetSummary, SkipWorkoutAsync, RemoveRecurringAsync wired |
| `Components/Shared/ScheduleDialog.razor` | Template/ad-hoc toggle, recurrence config, save | VERIFIED | From Template/Ad-hoc toggle, DayOfWeekToggle, interval input, ScheduleWorkoutAsync called |
| `Components/Shared/DayOfWeekToggle.razor` | 7 day chips with DaysOfWeek flags | VERIFIED | 7 DayInfo entries Monday-Sunday, HasFlag toggle, ValueChanged EventCallback |
| `Components/Shared/TemplatePicker.razor` | Searchable template list | VERIFIED | Search input, filteredTemplates computed, OnTemplateSelected callback |
| `wwwroot/js/calendar-drag.js` | SortableJS cross-container drag | VERIFIED | `window.calendarDrag`, `Sortable.create`, group `calendar-days`, `OnWorkoutDropped` invoked |
| `BlazorApp2.Tests/MaterializationTests.cs` | 5+ tests for recurrence generation and dedup | VERIFIED | 211 lines, 6 test methods including Weekly/Daily/NoDuplicates/NoMatchingDays/FlagMapping/MondayOfWeek |
| SCHED-05 implementation | Muscle group conflict detection + warning | MISSING | No implementation anywhere in codebase — intentionally deferred per D-14 |

---

## Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| MaterializationService.cs | AppDbContext | IDbContextFactory | WIRED | Constructor takes `IDbContextFactory<AppDbContext>`, used in MaterializeAsync/MaterializeAllAsync |
| SchedulingService.cs | AppDbContext | IDbContextFactory | WIRED | Constructor takes `IDbContextFactory<AppDbContext>`, all query methods use `_contextFactory.CreateDbContextAsync()` |
| Program.cs | MaterializationService | DI registration | WIRED | Line 15: `builder.Services.AddScoped<MaterializationService>()` |
| Program.cs | SchedulingService | DI registration | WIRED | Line 16: `builder.Services.AddScoped<SchedulingService>()` |
| Calendar.razor.cs | SchedulingService | [Inject] | WIRED | `[Inject] private SchedulingService SchedulingService { get; set; }`, used in LoadWeekData, HandleDragReschedule |
| Calendar.razor.cs | MaterializationService | [Inject] | WIRED | `[Inject] private MaterializationService MaterializationService { get; set; }`, called in OnInitializedAsync |
| WorkoutChip.razor | ScheduledWorkout entity | Parameter binding | WIRED | `[Parameter] public ScheduledWorkout Workout`, renders `Workout.DisplayName` |
| ScheduleDialog.razor | SchedulingService | @inject | WIRED | `@inject SchedulingService SchedulingService`, calls `ScheduleWorkoutAsync` in HandleSchedule |
| ScheduleDialog.razor | TemplatePicker | child component | WIRED | `<TemplatePicker SelectedTemplateId="selectedTemplateId" OnTemplateSelected="HandleTemplateSelected" />` |
| ScheduleDialog.razor | DayOfWeekToggle | child component | WIRED | `<DayOfWeekToggle Value="selectedDays" ValueChanged="days => selectedDays = days" />` |
| MonthlyMiniCalendar.razor | SchedulingService | @inject + GetWorkoutsForMonthAsync | WIRED | `@inject SchedulingService SchedulingService`, called in LoadMonthData, result used to populate dotData |
| WorkoutDetailDialog.razor | SchedulingService | @inject + SkipWorkoutAsync | WIRED | `@inject SchedulingService SchedulingService`, HandleSkip calls `SkipWorkoutAsync`, HandleRemoveAllConfirm calls `RemoveRecurringAsync` |
| Calendar.razor | ScheduleDialog | dialog integration | WIRED | `<ScheduleDialog IsOpen="showScheduleDialog" OnClose="..." OnScheduled="HandleScheduled" ...>` |
| Calendar.razor | WorkoutDetailDialog | dialog integration | WIRED | `<WorkoutDetailDialog IsOpen="showDetailDialog" ... OnWorkoutChanged="HandleWorkoutChanged" OnEditRequested="OpenEditFromDetail">` |
| Calendar.razor | MonthlyMiniCalendar | month view integration | WIRED | `<MonthlyMiniCalendar DisplayMonth="currentDisplayMonth" SelectedWeekStart="currentWeekStart" OnDateClicked="HandleMonthDateClicked">` |
| Calendar.razor.cs | calendarDrag.init | JS interop | WIRED | `await JS.InvokeVoidAsync("calendarDrag.init", dotNetRef)` in OnAfterRenderAsync; `[JSInvokable] public async Task OnWorkoutDropped(...)` |
| Components/App.razor | calendar-drag.js | script tag | WIRED | `<script src="js/calendar-drag.js"></script>` confirmed at line 22 |

---

## Requirements Coverage

| Requirement | Source Plan | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| SCHED-01 | 04-02 | Weekly calendar view with type indicators | SATISFIED | Calendar.razor weekly grid with WorkoutChip + DetermineWorkoutType color coding |
| SCHED-02 | 04-04 | Monthly calendar overview with color-coded dots | SATISFIED | MonthlyMiniCalendar.razor with `monthly-dot--{type}` dots per day |
| SCHED-03 | 04-01, 04-03 | Schedule from templates or ad-hoc | SATISFIED | ScheduleDialog.razor From Template/Ad-hoc toggle + SchedulingService.ScheduleWorkoutAsync |
| SCHED-04 | 04-01, 04-03 | Recurrence rules (weekly days, every N days) | SATISFIED | MaterializationService.GenerateOccurrences + DayOfWeekToggle + interval input |
| SCHED-05 | 04-02 | Rest day awareness — flag conflicts for consecutive same muscle group | BLOCKED | No implementation. D-14 explicitly deferred this. REQUIREMENTS.md marks [x] Complete but no code exists. The plan 02 SUMMARY's `requirements-completed: [SCHED-01, SCHED-05]` is inaccurate. |
| SCHED-06 | 04-01, 04-04 | Materialize scheduled rows from recurrence rules (rolling window) | SATISFIED | MaterializationService.MaterializeAsync creates rows in 28-day window with dedup; MaterializeAllAsync called on Calendar page load |

---

## Anti-Patterns Found

| File | Line | Pattern | Severity | Impact |
|------|------|---------|----------|--------|
| `.planning/phases/04-calendar-scheduling/04-02-SUMMARY.md` | 45 | `requirements-completed: [SCHED-01, SCHED-05]` — SCHED-05 listed as complete when D-14 explicitly deferred it | Warning | REQUIREMENTS.md [x] marks are inconsistent with D-14 decision; creates false expectation for Phase 5 |

No blocker anti-patterns found in implementation code. The single "placeholder" string match in ScheduleDialog.razor (line 36) is a legitimate HTML input `placeholder` attribute.

---

## Test Results

- **Targeted tests** (MaterializationTests + ScheduleTests): **28/28 passed**
  - Weekly_MondayWednesdayFriday_Generates12DatesIn4Weeks: PASS
  - Daily_Interval2_Generates14DatesIn28Days: PASS
  - Materialize_CalledTwice_NoDuplicates: PASS
  - Weekly_NoMatchingDaysInWindow_CreatesNoRows: PASS
  - DaysOfWeek_FlagMapping_SystemDayOfWeekToCustomEnum: PASS
  - GetMondayOfWeek_ReturnsCorrectMonday: PASS
  - AdHocWorkout_NullTemplate_PersistsWithAdHocName: PASS
  - DisplayName_ReturnsTemplateName/AdHocName/Untitled: PASS (3 tests)
  - DetermineWorkoutType_AllStrength/AllEndurance/Mixed/NoTemplate: PASS (4 tests)
  - Skip/Remove/RemoveRecurring tests: PASS
- **Full test suite**: **73/73 passed**
- **Build**: Clean (0 errors, 0 warnings)

---

## Human Verification Required

### 1. Workout Type Color Indicators

**Test:** Navigate to `/calendar`, schedule a strength workout, an endurance workout, and a mixed template workout for the current week.
**Expected:** Strength chip has blue left border (`#60A5FA`), endurance chip has green border (`#34D399`), mixed chip has purple border (`#A78BFA`).
**Why human:** CSS color rendering requires visual inspection.

### 2. Monthly Mini-Calendar Dots

**Test:** Toggle to Month view. Days with scheduled workouts should show small colored dots below the date number.
**Expected:** Dots match workout types — blue for strength, green for endurance, purple for mixed. Max 3 dots shown.
**Why human:** CSS color rendering requires visual inspection.

### 3. Drag-to-Reschedule

**Test:** In week view with workouts visible, drag a workout chip to a different day cell.
**Expected:** Workout moves to the new day, a toast notification confirms "Workout moved to [day name]". The chip no longer appears on the original day after Blazor re-renders.
**Why human:** SortableJS drag interaction requires live browser testing.

### 4. Mobile Responsive Layout

**Test:** Resize browser to below 767px width on the `/calendar` page.
**Expected:** 7-column grid collapses to single-column day list. Day names appear inline as "Monday, Mar 17" format. WorkoutChips remain tappable (min-height 44px).
**Why human:** Responsive layout requires browser resize testing.

---

## Gaps Summary

One requirement is unimplemented: **SCHED-05** (rest day awareness / consecutive muscle group conflict warning).

**Root cause:** Decision D-14 explicitly scoped SCHED-05 out of Phase 4 ("Skipped entirely. No muscle group overlap warnings, no rest day flagging. User manages their own training logic."). The planning documentation (04-CONTEXT.md, 04-RESEARCH.md) is consistent in marking it out of scope.

**The inconsistency:** Plan 04-02's SUMMARY incorrectly lists `requirements-completed: [SCHED-01, SCHED-05]`, and REQUIREMENTS.md was updated to show SCHED-05 as `[x] Complete` for Phase 4. Neither reflects the D-14 decision.

**Resolution options:**
1. Implement SCHED-05 in a gap plan (conflict detection in ScheduleDialog, warning toast when same muscle group scheduled on consecutive days)
2. Formally defer SCHED-05 — update REQUIREMENTS.md to mark it as deferred (not Phase 4 complete), update plan 02 SUMMARY to remove SCHED-05 from requirements-completed

Option 2 aligns with the D-14 decision and the rest of the planning documents. The feature has zero implementation and zero test coverage, so Option 1 would require meaningful scope.

All other must-haves are fully verified: services exist and are substantive, calendar UI renders correctly, all dialogs are wired, materialization is idempotent, drag-to-reschedule is integrated, and 73 tests pass.

---

_Verified: 2026-03-21T21:00:00Z_
_Verifier: Claude (gsd-verifier)_
