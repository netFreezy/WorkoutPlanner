# Phase 4: Calendar & Scheduling - Research

**Researched:** 2026-03-21
**Domain:** Calendar UI, scheduling workflow, recurrence materialization
**Confidence:** HIGH

## Summary

Phase 4 builds a calendar-centric scheduling UI on top of the existing ScheduledWorkout/RecurrenceRule data model from Phase 1. The core challenge is a pure CSS/HTML calendar grid (no third-party calendar libraries per project constraint "no JavaScript frameworks"), a materialization service that generates concrete ScheduledWorkout rows from recurrence rules, and a scheduling dialog that ties into the template system from Phase 3.

The data model is almost ready but needs one schema change: ScheduledWorkout.WorkoutTemplateId is currently a required int, but CONTEXT D-06 specifies ad-hoc workouts as name-only placeholders with no template. This requires making WorkoutTemplateId nullable and adding an AdHocName string field, plus a migration.

The project has no Services directory yet -- the materialization service will be the first registered service. Established patterns (IDbContextFactory, Dialog/Toast components, dark theme tokens, SortableJS interop) provide solid foundations. The SortableJS drag pattern from template-builder.js can be extended for drag-to-reschedule between day cells.

**Primary recommendation:** Build the calendar as a pure CSS Grid (7 columns for weekly, 7x5/6 for monthly), introduce a MaterializationService for recurrence logic, and add an entity migration for ad-hoc workout support before any UI work.

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Day columns for weekly view -- 7 columns (Mon-Sun) with workouts stacked vertically in each day. No time grid since ScheduledWorkout has date only, no time.
- **D-02:** Mini calendar grid for monthly overview -- traditional 7x5 grid with small date cells, each showing 1-3 colored dots for workout types (blue=strength, green=endurance, purple=mixed). Click a day to jump to that week.
- **D-03:** Mobile layout collapses to a day list -- vertical list of days with workouts under each, instead of 7 cramped columns.
- **D-04:** Default to current week with today highlighted. Weekly is primary view, monthly is toggle/secondary panel. Navigation via prev/next arrows + "Today" jump button.
- **D-05:** Click empty day cell (or "+" in cell) opens schedule dialog with template picker and date pre-filled. FAB also available as alternative entry point.
- **D-06:** Ad-hoc workouts are name-only placeholders -- just a label like "Rest day yoga" with no structured exercises. Not a mini builder.
- **D-07:** Click scheduled workout opens detail view showing template name, exercises preview, date, recurrence info. Actions: edit date, remove, skip occurrence, edit recurrence.
- **D-08:** Drag-to-reschedule if feasible -- drag a workout chip to another day to change its date. Falls back to date edit in dialog if too complex.
- **D-09:** Compact chips in day cells -- template name + type indicator (colored left border). Multiple workouts stack vertically.
- **D-10:** Repeat toggle with inline options in the schedule dialog. Reveals: "Every week on [day chips]" (Weekly), "Every X days" (Daily with interval), matching FrequencyType enum. Day chips are clickable toggles (Mon-Sun) matching DaysOfWeek flags.
- **D-11:** Small repeat icon on recurring workout chips to distinguish from one-off workouts.
- **D-12:** Materialization triggers on both save (immediate feedback) and page load (rolling window catch-up). 4-week window per Phase 1 D-12.
- **D-13:** Recurring workout actions: "Skip this one" (mark occurrence as Skipped), "Remove all" (delete rule + future rows), "Edit schedule" (change recurrence, regenerates future rows per Phase 1 D-13).
- **D-14:** Skipped entirely. No muscle group overlap warnings, no rest day flagging. SCHED-05 is out of scope for this phase.

### Claude's Discretion
- Calendar page layout details (sidebar vs. top toggle for weekly/monthly)
- Chip color scheme and type indicator styling
- Template picker dialog design (dropdown vs. searchable list)
- Monthly dot colors and sizing
- Materialization service implementation (inline vs. background service)
- Mobile day list interaction details
- Animation for adding/moving workout chips

### Deferred Ideas (OUT OF SCOPE)
- Conflict detection / rest day warnings (SCHED-05) -- intentionally skipped, user manages their own training logic.

</user_constraints>

<phase_requirements>

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SCHED-01 | Weekly calendar view (primary) showing scheduled workouts with type indicators | CSS Grid 7-column layout, workout chips with colored left border, date range querying |
| SCHED-02 | Monthly calendar overview with color-coded dots for workout types | CSS Grid 7x5/6 mini-calendar, aggregate dot rendering, click-to-navigate-week |
| SCHED-03 | Schedule workouts from templates or ad-hoc on specific dates | Schedule dialog with template picker, ad-hoc name field, entity schema change for nullable template |
| SCHED-04 | Recurrence rules: every Monday, every other day, 3x/week on specific days | Recurrence UI with frequency selector and DaysOfWeek toggle chips, maps to existing FrequencyType/DaysOfWeek enums |
| SCHED-05 | Rest day awareness -- flag conflicts when scheduling same muscle group on consecutive days | OUT OF SCOPE per D-14 |
| SCHED-06 | Materialize scheduled workout rows from recurrence rules (rolling window) | MaterializationService with 4-week window, triggered on save and page load |

</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| ASP.NET Core Blazor Server | 10.0.3 | Interactive server components | Project constraint -- already configured |
| EF Core + SQLite | 10.0.5 | Data access, scheduling queries | Already configured with IDbContextFactory |
| CSS Grid | N/A | Calendar layout (weekly 7-col, monthly 7x5) | No JS calendar libraries allowed per project constraint |
| SortableJS | (already loaded) | Drag-to-reschedule between day cells | Already used for template builder drag-and-drop |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Dialog.razor | existing | Schedule dialog, workout detail dialog | All modal interactions |
| Toast.razor | existing | Success/error notifications for scheduling actions | After schedule/delete/skip actions |
| FilterChip.razor | existing | Potential reuse for day-of-week toggle chips in recurrence UI | Recurrence day selection |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| CSS Grid calendar | MudBlazor/Radzen calendar | Project constraint forbids JS frameworks; MudBlazor adds massive bundle |
| SortableJS drag | HTML5 native DnD | SortableJS already loaded, better mobile support, proven interop pattern |
| Custom materialization | Hangfire/Quartz | Overkill for single-user app, page-load trigger is sufficient |

## Architecture Patterns

### Recommended Project Structure
```
Components/
  Pages/
    Calendar.razor              # Main calendar page with weekly/monthly views
    Calendar.razor.css          # Scoped styles for calendar
    Calendar.razor.cs           # Code-behind (if needed for size)
  Shared/
    ScheduleDialog.razor        # Schedule/edit workout dialog
    ScheduleDialog.razor.css
    WorkoutDetailDialog.razor   # View scheduled workout details + actions
    WorkoutDetailDialog.razor.css
    WorkoutChip.razor           # Compact workout chip for day cells
    WorkoutChip.razor.css
    MonthlyMiniCalendar.razor   # Monthly overview sub-component
    MonthlyMiniCalendar.razor.css
    TemplatePicker.razor        # Searchable template list for schedule dialog
    TemplatePicker.razor.css
    DayOfWeekToggle.razor       # Clickable day chips for recurrence (Mon-Sun)
    DayOfWeekToggle.razor.css
Services/
  MaterializationService.cs     # Recurrence -> ScheduledWorkout row generation
  SchedulingService.cs          # CRUD operations for scheduled workouts
Data/
  Entities/
    ScheduledWorkout.cs         # Modified: nullable WorkoutTemplateId + AdHocName
wwwroot/
  js/
    calendar-drag.js            # SortableJS interop for drag-to-reschedule
```

### Pattern 1: Entity Schema Change for Ad-Hoc Workouts
**What:** Make WorkoutTemplateId nullable and add AdHocName to support both template-based and ad-hoc scheduling.
**When to use:** Required before any scheduling UI work.
**Example:**
```csharp
// Modified ScheduledWorkout entity
public class ScheduledWorkout
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Planned;

    // Template-based workout (nullable for ad-hoc)
    public int? WorkoutTemplateId { get; set; }
    public WorkoutTemplate? WorkoutTemplate { get; set; }

    // Ad-hoc workout name (used when WorkoutTemplateId is null)
    public string? AdHocName { get; set; }

    // Recurrence tracking
    public int? RecurrenceRuleId { get; set; }
    public RecurrenceRule? RecurrenceRule { get; set; }

    public WorkoutLog? WorkoutLog { get; set; }

    // Computed display name
    public string DisplayName => WorkoutTemplate?.Name ?? AdHocName ?? "Untitled";
}
```
**Impact:** Requires EF migration. ScheduleConfiguration needs update (DeleteBehavior.Restrict -> SetNull for WorkoutTemplate FK). Existing ScheduleTests need review (they assume required template).

### Pattern 2: MaterializationService
**What:** Service that generates concrete ScheduledWorkout rows from RecurrenceRules within a rolling 4-week window.
**When to use:** Called on page load (catch-up) and after saving a recurrence rule (immediate feedback).
**Example:**
```csharp
public class MaterializationService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public MaterializationService(IDbContextFactory<AppDbContext> factory)
    {
        _factory = factory;
    }

    public async Task MaterializeAsync(int recurrenceRuleId)
    {
        using var context = await _factory.CreateDbContextAsync();
        var rule = await context.RecurrenceRules
            .Include(r => r.ScheduledWorkouts)
            .FirstOrDefaultAsync(r => r.Id == recurrenceRuleId);
        if (rule is null) return;

        var windowStart = DateTime.UtcNow.Date;
        var windowEnd = windowStart.AddDays(28); // 4-week window

        var existingDates = rule.ScheduledWorkouts
            .Select(sw => sw.ScheduledDate.Date)
            .ToHashSet();

        foreach (var date in GenerateDates(rule, windowStart, windowEnd))
        {
            if (!existingDates.Contains(date))
            {
                context.ScheduledWorkouts.Add(new ScheduledWorkout
                {
                    ScheduledDate = date,
                    WorkoutTemplateId = rule.WorkoutTemplateId,
                    RecurrenceRuleId = rule.Id,
                    Status = WorkoutStatus.Planned
                });
            }
        }
        await context.SaveChangesAsync();
    }

    public async Task MaterializeAllAsync()
    {
        using var context = await _factory.CreateDbContextAsync();
        var ruleIds = await context.RecurrenceRules.Select(r => r.Id).ToListAsync();
        foreach (var id in ruleIds)
        {
            await MaterializeAsync(id);
        }
    }

    private static IEnumerable<DateTime> GenerateDates(
        RecurrenceRule rule, DateTime start, DateTime end)
    {
        // Implementation depends on FrequencyType
        // Daily: every N days from start
        // Weekly: specific DaysOfWeek each week
        // Custom: handled via DaysOfWeek flags with interval
    }
}
```

### Pattern 3: Weekly Calendar CSS Grid
**What:** 7-column CSS Grid for Mon-Sun with stacked workout chips per day.
**When to use:** Primary calendar view.
**Example:**
```css
.week-grid {
    display: grid;
    grid-template-columns: repeat(7, 1fr);
    gap: 1px;
    background: var(--color-border-subtle);
    border: 1px solid var(--color-border-subtle);
    border-radius: var(--radius-md);
    overflow: hidden;
}

.day-cell {
    background: var(--color-bg-secondary);
    min-height: 120px;
    padding: var(--space-sm);
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
}

.day-cell--today {
    background: var(--color-accent-muted);
}

.day-header {
    font-size: var(--font-size-label);
    color: var(--color-text-secondary);
    letter-spacing: var(--letter-spacing-label);
    text-transform: uppercase;
    text-align: center;
    padding-bottom: var(--space-xs);
    border-bottom: 1px solid var(--color-border-subtle);
}

.day-date {
    font-size: var(--font-size-label);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text-primary);
    text-align: center;
}

.day-date--today {
    background: var(--color-accent);
    color: var(--color-text-inverse);
    border-radius: var(--radius-full);
    width: 24px;
    height: 24px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto;
}

/* Mobile: collapse to day list */
@media (max-width: 767px) {
    .week-grid {
        grid-template-columns: 1fr;
        gap: 0;
    }
    .day-cell {
        min-height: auto;
        border-bottom: 1px solid var(--color-border-subtle);
    }
}
```

### Pattern 4: Workout Chip with Type Indicator
**What:** Compact chip showing template name + colored left border for workout type.
**When to use:** Inside day cells for both weekly and detail views.
**Example:**
```css
.workout-chip {
    background: var(--color-bg-glass);
    border: 1px solid var(--color-border-subtle);
    border-left: 3px solid var(--color-strength-text); /* default strength */
    border-radius: var(--radius-sm);
    padding: 4px 8px;
    font-size: var(--font-size-label);
    color: var(--color-text-primary);
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: var(--space-xs);
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    transition: background-color var(--transition-fast);
}

.workout-chip--endurance {
    border-left-color: var(--color-endurance-text);
}

.workout-chip--mixed {
    border-left-color: #A78BFA; /* purple for mixed */
}

.workout-chip--adhoc {
    border-left-color: var(--color-text-tertiary);
    font-style: italic;
}

.workout-chip:hover {
    background: var(--color-bg-glass-hover);
}

.workout-chip__recurrence-icon {
    flex-shrink: 0;
    color: var(--color-text-tertiary);
    width: 12px;
    height: 12px;
}
```

### Pattern 5: Date Range Query for Calendar Data
**What:** Efficiently load scheduled workouts for the visible date range.
**When to use:** On every view navigation (prev/next week, month click).
**Example:**
```csharp
// In Calendar.razor code
private async Task LoadWeekData(DateTime weekStart)
{
    var weekEnd = weekStart.AddDays(7);
    using var context = await DbFactory.CreateDbContextAsync();

    scheduledWorkouts = await context.ScheduledWorkouts
        .Include(sw => sw.WorkoutTemplate)
            .ThenInclude(t => t!.Items)
                .ThenInclude(i => i.Exercise)
        .Include(sw => sw.RecurrenceRule)
        .Where(sw => sw.ScheduledDate >= weekStart && sw.ScheduledDate < weekEnd)
        .OrderBy(sw => sw.ScheduledDate)
        .ToListAsync();

    // Group by date for rendering
    workoutsByDay = scheduledWorkouts
        .GroupBy(sw => sw.ScheduledDate.Date)
        .ToDictionary(g => g.Key, g => g.ToList());
}
```

### Anti-Patterns to Avoid
- **Loading all workouts at once:** Always filter by date range. As data grows, loading everything would be slow.
- **Materializing on every render:** Materialize on page load once and on save -- not inside render loops.
- **Using JS calendar libraries:** Project constraint is "no JavaScript frameworks." SortableJS is acceptable as a utility, but full calendar widgets (FullCalendar, etc.) are not.
- **Storing computed workout type on entity:** Determine strength/endurance/mixed at render time from template items, not as a stored field.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Date arithmetic | Manual day/week calculations | `DateTime.AddDays()`, `DayOfWeek` enum, `CultureInfo.FirstDayOfWeek` | Edge cases around month boundaries, leap years |
| Drag and drop | HTML5 native DnD API | SortableJS (already loaded) | Mobile support, ghost rendering, proven Blazor interop pattern |
| Dialog overlay | Custom positioned div | Existing Dialog.razor component | Consistent behavior, backdrop blur, animations already polished |
| Toast notifications | Custom notification system | Existing Toast.razor component | CancellationTokenSource pattern for overlapping toasts |
| Recurrence date generation | Naive loop | Well-tested method with DaysOfWeek flags HasFlag check | Flags enum bit manipulation is error-prone |

**Key insight:** The calendar layout itself is simple enough for CSS Grid (no times, just dates with chips), but recurrence materialization logic needs careful testing because DaysOfWeek flags and interval math have subtle edge cases.

## Common Pitfalls

### Pitfall 1: DaysOfWeek Flags Enum Mapping to System.DayOfWeek
**What goes wrong:** The custom `DaysOfWeek` flags enum uses bit values (Monday=1, Tuesday=2, etc.) while `System.DayOfWeek` uses ordinal values (Sunday=0, Monday=1, etc.). Direct casting between them will produce wrong results.
**Why it happens:** Similar names suggest interoperability, but the values and Sunday positioning differ.
**How to avoid:** Create an explicit mapping method:
```csharp
private static DaysOfWeek ToDaysOfWeek(System.DayOfWeek day) => day switch
{
    System.DayOfWeek.Monday => DaysOfWeek.Monday,
    System.DayOfWeek.Tuesday => DaysOfWeek.Tuesday,
    System.DayOfWeek.Wednesday => DaysOfWeek.Wednesday,
    System.DayOfWeek.Thursday => DaysOfWeek.Thursday,
    System.DayOfWeek.Friday => DaysOfWeek.Friday,
    System.DayOfWeek.Saturday => DaysOfWeek.Saturday,
    System.DayOfWeek.Sunday => DaysOfWeek.Sunday,
    _ => DaysOfWeek.None
};
```
**Warning signs:** Workouts appearing on wrong days, off-by-one in weekly patterns.

### Pitfall 2: Week Start Day (Monday vs. Sunday)
**What goes wrong:** `System.DayOfWeek` starts weeks on Sunday (DayOfWeek.Sunday = 0), but D-01 specifies Mon-Sun columns. Calendar navigation math that uses `DayOfWeek` directly will misalign.
**Why it happens:** US convention starts Sunday, European/ISO convention starts Monday. .NET defaults to US.
**How to avoid:** Always calculate week start as: `date.AddDays(-(((int)date.DayOfWeek + 6) % 7))` to get Monday. Or use `ISOWeek.GetWeekOfYear()` for ISO week numbers.
**Warning signs:** Sunday workouts appearing in the wrong week, weekly navigation jumping incorrectly.

### Pitfall 3: Blazor Scoped CSS and Child Components
**What goes wrong:** Styles defined in `Calendar.razor.css` won't apply to child component HTML (WorkoutChip, MonthlyMiniCalendar) without `::deep` selector.
**Why it happens:** Blazor CSS isolation adds a unique attribute to the parent component's elements but not to child component render trees.
**How to avoid:** Either put all child component styles in their own `.razor.css` files (preferred) or use `::deep` from parent (per project memory: `feedback_blazor_css_deep.md`).
**Warning signs:** Components rendering with no styles, white/unstyled appearance.

### Pitfall 4: Materialization Duplicating Rows
**What goes wrong:** If materialization runs without checking existing rows, it creates duplicate ScheduledWorkout entries for the same date/rule combination.
**Why it happens:** Multiple page loads or save operations trigger materialization without deduplication.
**How to avoid:** Always load existing scheduled workout dates for a rule before generating new ones. Use a Set to track which dates already have rows.
**Warning signs:** Multiple identical workout chips on the same day.

### Pitfall 5: ScheduledWorkout FK Change Breaking Existing Tests
**What goes wrong:** Making WorkoutTemplateId nullable changes the entity schema. ScheduleConfiguration's DeleteBehavior.Restrict must become SetNull. Existing ScheduleTests assume a required WorkoutTemplate.
**Why it happens:** D-06 ad-hoc workouts weren't anticipated in Phase 1 entity design.
**How to avoid:** Update ScheduleConfiguration, add migration, update test helper to support both template-based and ad-hoc workout creation.
**Warning signs:** Migration failures, test compilation errors.

### Pitfall 6: SortableJS Container Re-render in Blazor
**What goes wrong:** When Blazor re-renders the calendar grid (e.g., navigating weeks), SortableJS loses its reference to the DOM container.
**Why it happens:** Blazor replaces DOM elements on re-render, orphaning the SortableJS instance.
**How to avoid:** Use the same pattern as template-builder.js: destroy and re-initialize SortableJS after Blazor re-render via `OnAfterRenderAsync`. Use `@key` directives on day cells to help Blazor's diff algorithm.
**Warning signs:** Drag stops working after navigation, console errors about destroyed elements.

## Code Examples

### Week Start Calculation (Monday-based)
```csharp
// Source: standard .NET pattern for ISO week start
private static DateTime GetMondayOfWeek(DateTime date)
{
    var diff = (7 + (int)date.DayOfWeek - (int)System.DayOfWeek.Monday) % 7;
    return date.Date.AddDays(-diff);
}

private static DateTime GetMondayOfWeek(int year, int month, int day)
    => GetMondayOfWeek(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc));
```

### Recurrence Date Generation
```csharp
// Source: project-specific implementation matching FrequencyType/DaysOfWeek enums
private static IEnumerable<DateTime> GenerateOccurrences(
    RecurrenceRule rule, DateTime windowStart, DateTime windowEnd)
{
    switch (rule.FrequencyType)
    {
        case FrequencyType.Daily:
            // Every N days starting from windowStart
            for (var d = windowStart; d < windowEnd; d = d.AddDays(rule.Interval))
                yield return d;
            break;

        case FrequencyType.Weekly:
            // Each week, check DaysOfWeek flags
            var weekStart = GetMondayOfWeek(windowStart);
            for (var week = weekStart; week < windowEnd; week = week.AddDays(7))
            {
                for (int i = 0; i < 7; i++)
                {
                    var day = week.AddDays(i);
                    if (day >= windowStart && day < windowEnd)
                    {
                        var flag = ToDaysOfWeek(day.DayOfWeek);
                        if (rule.DaysOfWeek.HasFlag(flag))
                            yield return day;
                    }
                }
            }
            break;

        case FrequencyType.Custom:
            // DaysOfWeek flags with interval (every N weeks)
            var start = GetMondayOfWeek(windowStart);
            for (var week = start; week < windowEnd; week = week.AddDays(7 * rule.Interval))
            {
                for (int i = 0; i < 7; i++)
                {
                    var day = week.AddDays(i);
                    if (day >= windowStart && day < windowEnd)
                    {
                        var flag = ToDaysOfWeek(day.DayOfWeek);
                        if (rule.DaysOfWeek.HasFlag(flag))
                            yield return day;
                    }
                }
            }
            break;
    }
}
```

### Workout Type Determination from Template Items
```csharp
// Source: project-specific, derived from exercise TPH hierarchy
public enum WorkoutType { Strength, Endurance, Mixed, AdHoc }

public static WorkoutType DetermineWorkoutType(ScheduledWorkout sw)
{
    if (sw.WorkoutTemplate is null) return WorkoutType.AdHoc;

    var hasStrength = sw.WorkoutTemplate.Items.Any(i => i.Exercise is StrengthExercise);
    var hasEndurance = sw.WorkoutTemplate.Items.Any(i => i.Exercise is EnduranceExercise);

    return (hasStrength, hasEndurance) switch
    {
        (true, true) => WorkoutType.Mixed,
        (true, false) => WorkoutType.Strength,
        (false, true) => WorkoutType.Endurance,
        (false, false) => WorkoutType.AdHoc // empty template
    };
}
```

### Monthly Overview Dot Aggregation
```csharp
// Source: project-specific, for monthly mini-calendar rendering
private async Task<Dictionary<DateTime, List<WorkoutType>>> LoadMonthDots(DateTime month)
{
    var start = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    var end = start.AddMonths(1);

    using var context = await DbFactory.CreateDbContextAsync();
    var workouts = await context.ScheduledWorkouts
        .Include(sw => sw.WorkoutTemplate)
            .ThenInclude(t => t!.Items)
                .ThenInclude(i => i.Exercise)
        .Where(sw => sw.ScheduledDate >= start && sw.ScheduledDate < end)
        .ToListAsync();

    return workouts
        .GroupBy(sw => sw.ScheduledDate.Date)
        .ToDictionary(
            g => g.Key,
            g => g.Select(DetermineWorkoutType).Distinct().ToList()
        );
}
```

### DI Registration Pattern
```csharp
// In Program.cs -- register services
builder.Services.AddScoped<MaterializationService>();
builder.Services.AddScoped<SchedulingService>();
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Scoped DbContext | IDbContextFactory | .NET 5+ / Blazor Server | Project already uses factory pattern correctly |
| JS Calendar widgets (FullCalendar) | CSS Grid native calendars | CSS Grid widespread support | No dependency, full control, matches project constraint |
| iCal/RFC 5545 recurrence | Simple enum-based recurrence | Project decision (D-10) | Much simpler than full iCal; sufficient for workout scheduling |

**Deprecated/outdated:**
- Heron.MudCalendar was previously considered (STATE.md blocker note) -- dropped in favor of custom CSS Grid per project constraints.

## Open Questions

1. **Materialization start date for Daily recurrence**
   - What we know: Weekly uses DaysOfWeek flags so start date is implicit. Daily with interval needs an anchor date to count intervals from.
   - What's unclear: RecurrenceRule has no StartDate field. When does "every 2 days" start counting?
   - Recommendation: Use the first ScheduledWorkout date linked to the rule as the anchor, or add a StartDate to RecurrenceRule via migration. Adding StartDate is cleaner -- the materialization service needs a reference point.

2. **Drag-to-reschedule feasibility with SortableJS**
   - What we know: SortableJS works for reordering within a single container. Calendar days are separate cells.
   - What's unclear: Cross-container drag (day A to day B) requires SortableJS `group` configuration, which is more complex than the template builder pattern.
   - Recommendation: Implement as a stretch goal. Start with click-to-edit-date in the detail dialog (D-07). If time permits, add SortableJS `group` setup across day cells. The fallback (dialog date edit) is fully functional.

3. **Monthly overview performance**
   - What we know: Loading all workouts for a month with full template/item/exercise includes could be heavy.
   - What's unclear: For a personal app with ~4-6 workouts/week, is this actually a concern?
   - Recommendation: Not a concern for single-user. ~24 workouts/month with includes is fine. Optimize later if needed.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 |
| Config file | BlazorApp2.Tests/BlazorApp2.Tests.csproj |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Schedule" -v q` |
| Full suite command | `dotnet test BlazorApp2.Tests -v q` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| SCHED-01 | Weekly view loads workouts for 7-day range | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SchedulingServiceTests" -x` | Wave 0 |
| SCHED-02 | Monthly overview aggregates workout types per day | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~MonthlyAggregation" -x` | Wave 0 |
| SCHED-03 | Schedule from template and ad-hoc persist correctly | unit (data) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ScheduleTests" -x` | Partial (template-based exists, ad-hoc needs new) |
| SCHED-04 | Recurrence rules generate correct dates | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~MaterializationTests" -x` | Wave 0 |
| SCHED-05 | Rest day awareness (OUT OF SCOPE) | N/A | N/A | N/A |
| SCHED-06 | Materialization creates rows in 4-week window | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~MaterializationTests" -x` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Schedule OR FullyQualifiedName~Materialization" -v q`
- **Per wave merge:** `dotnet test BlazorApp2.Tests -v q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `BlazorApp2.Tests/MaterializationTests.cs` -- covers SCHED-04, SCHED-06 (date generation, deduplication, 4-week window)
- [ ] `BlazorApp2.Tests/ScheduleTests.cs` -- extend for ad-hoc workouts (SCHED-03 ad-hoc path)
- [ ] `BlazorApp2.Tests/SchedulingServiceTests.cs` -- covers SCHED-01 date range queries, SCHED-02 monthly aggregation

## Sources

### Primary (HIGH confidence)
- Codebase inspection: `Data/Entities/ScheduledWorkout.cs`, `Data/Enums/FrequencyType.cs`, `Data/Enums/DaysOfWeek.cs`, `Data/Enums/WorkoutStatus.cs` -- existing entity model
- Codebase inspection: `Data/Configurations/ScheduleConfiguration.cs` -- FK relationships, index on ScheduledDate
- Codebase inspection: `Components/Shared/Dialog.razor`, `Components/Shared/Toast.razor` -- reusable UI components
- Codebase inspection: `wwwroot/app.css` -- design tokens, color scheme (strength blue, endurance green)
- Codebase inspection: `wwwroot/js/template-builder.js` -- SortableJS interop pattern with DOM revert
- Codebase inspection: `BlazorApp2.Tests/ScheduleTests.cs` -- existing test patterns for scheduling entities
- Phase 1 CONTEXT: `.planning/phases/01-data-foundation/01-CONTEXT.md` D-10 through D-13 -- recurrence decisions
- Phase 4 CONTEXT: `.planning/phases/04-calendar-scheduling/04-CONTEXT.md` -- all locked decisions

### Secondary (MEDIUM confidence)
- CSS Grid calendar layout patterns -- standard web development practice, no specific external source needed
- .NET `System.DayOfWeek` enum behavior -- standard .NET documentation

### Tertiary (LOW confidence)
- SortableJS cross-container drag (`group` option) for drag-to-reschedule -- not yet validated with Blazor Server interop in this project. Template builder only uses single-container drag. May need experimentation.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all technologies already in use in the project, no new dependencies
- Architecture: HIGH -- follows established project patterns (Pages, Shared components, scoped CSS, DbContextFactory)
- Pitfalls: HIGH -- identified from direct codebase analysis (entity schema, CSS isolation, SortableJS patterns)
- Materialization logic: MEDIUM -- recurrence date generation needs careful testing, especially Daily with interval and Custom frequency

**Research date:** 2026-03-21
**Valid until:** 2026-04-21 (stable -- no external dependency changes expected)
