# Phase 6: Analytics - Research

**Researched:** 2026-03-22
**Domain:** Blazor charting, analytics data aggregation, PR detection algorithms
**Confidence:** HIGH

## Summary

Phase 6 adds a unified analytics dashboard with four tabs (Overview, Strength, Endurance, PRs) using BlazorApexCharts for interactive charting. The data model is already complete from Phase 5 -- SetLog, EnduranceLog, WorkoutLog, and ScheduledWorkout contain all fields needed for volume trends, PR detection, adherence metrics, and endurance analytics. The main implementation areas are: (1) an AnalyticsService that queries and aggregates data into weekly buckets, (2) a PRDetectionService that runs inline on session finish, (3) a set of Blazor components for the tabbed dashboard with KPI cards and charts, and (4) integration of BlazorApexCharts 6.1.0 which has explicit .NET 10 support.

The Epley formula for estimated 1RM is straightforward: `weight * (1 + reps / 30.0)`. PR detection compares session results against historical bests per exercise. The key architectural decision is keeping all aggregation in C# (LINQ over fetched data) rather than complex SQL, following the established pattern from Phase 5 where GetPreviousStrengthPerformanceAsync uses client-side GroupBy after fetch.

**Primary recommendation:** Use BlazorApexCharts 6.1.0 for all charting with dark mode configuration. Build AnalyticsService and PRDetectionService as separate scoped services following the existing IDbContextFactory per-method pattern.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Use a Blazor charting library (e.g. BlazorApexCharts or Radzen Charts) -- JS interop under the hood is acceptable as long as the API is C#/Razor
- **D-02:** Charts must be interactive with hover tooltips showing exact values
- **D-03:** Chart type per metric is Claude's discretion -- pick what fits (line for trends, bar for volume, etc.)
- **D-04:** Theming: good-enough match to the dark premium theme -- no need for pixel-perfect custom SVG
- **D-05:** Tabbed layout -- segmented sections (e.g. Overview / Strength / Endurance / PRs), not a single scrolling page
- **D-06:** Summary KPI cards at the top of the Overview tab (total sessions this week, current streak, latest PR, etc.)
- **D-07:** Designed for both quick-glance ("how's my week") and weekly review depth
- **D-08:** Per-exercise drill-down -- users can select a specific exercise and see its history with weekly-aggregated data points
- **D-09:** PRs detected inline on session finish (not background scan) -- immediate feedback
- **D-10:** Strength PRs: heaviest weight, most reps at a given weight, and estimated 1RM (Epley formula: weight x (1 + reps/30))
- **D-11:** Endurance PRs: fastest pace and longest distance, tracked per activity type (running separate from cycling, etc.)
- **D-12:** Dedicated PR section/tab showing PRs across all exercises the user has actually done -- if they only run, they only see running PRs
- **D-13:** PR timeline showing when each record was set
- **D-14:** Default time window: last 4 weeks
- **D-15:** User-selectable range: 4W / 8W / 12W / All
- **D-16:** Aggregation granularity: weekly buckets for all charts
- **D-17:** Empty/skipped weeks show as gaps (zero bars) in charts -- don't compress the timeline

### Claude's Discretion
- Specific charting library choice (BlazorApexCharts vs Radzen vs other) -- **Decided: BlazorApexCharts 6.1.0**
- Exact tab names and ordering
- KPI card selection and layout
- Chart colors beyond the existing strength (blue) / endurance (green) tokens
- Per-exercise drill-down UX (separate page vs. modal vs. inline expand)
- 1RM formula details and edge case handling

### Deferred Ideas (OUT OF SCOPE)
- Progressive overload suggestions based on PR trends -- Phase 7
- Export analytics data as CSV/PDF -- Phase 7
- Comparison views (this month vs. last month) -- backlog
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| ANLY-01 | Volume trends -- total sets and total weight lifted per week over time | AnalyticsService queries SetLog with weekly grouping; bar chart (total volume) and line chart (weight trend) via BlazorApexCharts |
| ANLY-02 | PR tracking with automatic detection (weight PR, rep PR, estimated 1RM, pace PR, distance PR) | PRDetectionService with Epley formula; compares session results against historical bests; hooks into FinishSessionAsync |
| ANLY-03 | Streak and consistency metrics -- X workouts completed out of Y planned per week/month | ScheduledWorkout status query with Planned vs Completed counts; bar chart with completion rate overlay |
| ANLY-04 | Endurance trends -- pace and distance per week over time | AnalyticsService queries EnduranceLog with weekly grouping; line chart for pace, bar chart for distance |
| ANLY-05 | Planned vs. actual adherence -- deviation display per session and over time | Compare PlannedWeight/ActualWeight and PlannedReps/ActualReps from SetLog; PlannedDistance/ActualDistance from EnduranceLog |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor-ApexCharts | 6.1.0 | Interactive charting (bar, line, area) | MIT licensed, 2.3M downloads, .NET 10 support (added Jan 2026), wraps ApexCharts.js with C#/Razor API, built-in dark mode, interactive tooltips |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.5 | Already installed | All data queries for analytics |
| xunit | 2.9.3 | Already installed | Testing PR detection logic and analytics aggregation |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| BlazorApexCharts | Radzen.Blazor Charts | Radzen is a full component suite (100+ components); overkill when only charts needed. BlazorApexCharts is chart-focused, lighter, and has better chart-specific features (more chart types, better tooltip customization) |
| BlazorApexCharts | Blazorise.Charts (Chart.js) | Chart.js wrapper; less interactive by default, fewer chart type options, less active Blazor-specific development |

**Installation:**
```bash
dotnet add package Blazor-ApexCharts --version 6.1.0
```

**Version verification:** Blazor-ApexCharts 6.1.0 confirmed via `dotnet package search` on 2026-03-22. Targets net8.0, net9.0, net10.0. Released 2026-01-17.

## Architecture Patterns

### Recommended Project Structure
```
Services/
    AnalyticsService.cs       # Volume/endurance/adherence data queries
    PRDetectionService.cs     # PR detection and storage logic
Components/
    Pages/
        Analytics.razor       # Main analytics page with tabs
        Analytics.razor.cs    # Code-behind for analytics page
        Analytics.razor.css   # Scoped styles
    Shared/
        KpiCard.razor         # Reusable KPI summary card component
        KpiCard.razor.css
        ExerciseDrillDown.razor    # Per-exercise detail modal/view
        ExerciseDrillDown.razor.css
Data/
    Entities/
        PersonalRecord.cs    # PR entity for persistence
```

### Pattern 1: AnalyticsService with Weekly Bucket Aggregation
**What:** A service that fetches raw log data for a time range, then aggregates into weekly buckets in C# (not SQL)
**When to use:** All analytics queries -- volume, endurance, adherence
**Why:** The project already uses client-side GroupBy after fetch (SessionService.GetPreviousStrengthPerformanceAsync). EF Core/SQLite complex GROUP BY with date math causes translation issues. Fetch the range, group in memory.
**Example:**
```csharp
// Source: Established project pattern from SessionService
public class AnalyticsService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<List<WeeklyVolume>> GetWeeklyVolumeAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var setLogs = await context.SetLogs
            .Where(sl => sl.IsCompleted
                && sl.SetType == SetType.Working
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= rangeStart
                && sl.WorkoutLog.CompletedAt < rangeEnd)
            .Select(sl => new
            {
                sl.WorkoutLog.CompletedAt,
                sl.ActualWeight,
                sl.ActualReps
            })
            .ToListAsync();

        // Aggregate into ISO week buckets in C#
        return setLogs
            .GroupBy(sl => GetWeekStart(sl.CompletedAt!.Value))
            .Select(g => new WeeklyVolume(
                g.Key,
                g.Count(),
                g.Sum(sl => (sl.ActualWeight ?? 0) * (sl.ActualReps ?? 0))
            ))
            .OrderBy(wv => wv.WeekStart)
            .ToList();
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }
}

public record WeeklyVolume(DateTime WeekStart, int TotalSets, double TotalVolume);
```

### Pattern 2: PR Detection on Session Finish
**What:** PRDetectionService checks session results against historical bests immediately when a session is finished
**When to use:** Hook into the session finish flow (D-09: inline detection, not background scan)
**Why:** Provides immediate "New PR!" feedback via toast notification
**Example:**
```csharp
public class PRDetectionService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<List<PersonalRecord>> DetectPRsAsync(int workoutLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var newPRs = new List<PersonalRecord>();

        var workoutLog = await context.WorkoutLogs
            .Include(wl => wl.SetLogs.Where(sl => sl.IsCompleted))
                .ThenInclude(sl => sl.Exercise)
            .Include(wl => wl.EnduranceLogs.Where(el => el.IsCompleted))
                .ThenInclude(el => el.Exercise)
            .FirstOrDefaultAsync(wl => wl.Id == workoutLogId);

        if (workoutLog == null) return newPRs;

        // Check strength PRs per exercise
        foreach (var exerciseGroup in workoutLog.SetLogs.GroupBy(sl => sl.ExerciseId))
        {
            // ... compare against historical bests
        }

        return newPRs;
    }

    /// Epley formula: 1RM = weight * (1 + reps / 30.0)
    /// Edge cases: reps == 1 returns weight itself, reps > 30 becomes less accurate
    public static double EstimateE1RM(double weight, int reps)
    {
        if (reps <= 0 || weight <= 0) return 0;
        if (reps == 1) return weight;
        return weight * (1.0 + reps / 30.0);
    }
}
```

### Pattern 3: BlazorApexCharts Dark Mode Configuration
**What:** Configure ApexCharts globally for dark mode with custom colors matching the app theme
**When to use:** Service registration in Program.cs
**Example:**
```csharp
// Program.cs
builder.Services.AddApexCharts(e =>
{
    e.GlobalOptions = new ApexChartBaseOptions
    {
        Theme = new Theme { Mode = "dark", Palette = "palette6" },
        Chart = new Chart { Background = "transparent" },
        Colors = new List<string> { "#60A5FA", "#34D399", "#7C5CFC", "#FBBF24", "#EF4444" }
    };
});
```

### Pattern 4: Tab Navigation within Analytics Page
**What:** Use a local string state variable for active tab, render conditionally
**When to use:** The tabbed layout (D-05)
**Example:**
```razor
<div class="analytics-tabs">
    <button class="tab @(activeTab == "overview" ? "tab--active" : "")"
            @onclick='() => activeTab = "overview"'>Overview</button>
    <button class="tab @(activeTab == "strength" ? "tab--active" : "")"
            @onclick='() => activeTab = "strength"'>Strength</button>
    <button class="tab @(activeTab == "endurance" ? "tab--active" : "")"
            @onclick='() => activeTab = "endurance"'>Endurance</button>
    <button class="tab @(activeTab == "prs" ? "tab--active" : "")"
            @onclick='() => activeTab = "prs"'>PRs</button>
</div>

@if (activeTab == "overview") { /* Overview content */ }
@if (activeTab == "strength") { /* Strength content */ }
```

### Pattern 5: Time Range Selector
**What:** A segmented button group to select 4W / 8W / 12W / All, computing rangeStart from today
**When to use:** All chart views (D-14, D-15)
**Example:**
```csharp
private int selectedWeeks = 4; // Default: 4W (D-14)

private DateTime GetRangeStart()
{
    if (selectedWeeks == 0) return DateTime.MinValue; // "All"
    return DateTime.UtcNow.Date.AddDays(-(selectedWeeks * 7));
}
```

### Pattern 6: Weekly Bucket Gap Filling (D-17)
**What:** Ensure empty weeks appear as zero values in charts, not compressed out
**When to use:** All chart data preparation
**Example:**
```csharp
private List<WeeklyVolume> FillGaps(List<WeeklyVolume> data, DateTime rangeStart, DateTime rangeEnd)
{
    var allWeeks = new List<WeeklyVolume>();
    var dataDict = data.ToDictionary(d => d.WeekStart);
    var current = GetWeekStart(rangeStart);

    while (current < rangeEnd)
    {
        allWeeks.Add(dataDict.TryGetValue(current, out var existing)
            ? existing
            : new WeeklyVolume(current, 0, 0));
        current = current.AddDays(7);
    }
    return allWeeks;
}
```

### Pattern 7: BlazorApexCharts Bar Chart with Tooltip
**What:** Render a bar chart with hover tooltips showing exact values
**When to use:** Volume per week, adherence bars
**Example:**
```razor
@using ApexCharts

<ApexChart TItem="WeeklyVolume"
           Title="Weekly Volume"
           Options="volumeChartOptions">
    <ApexPointSeries TItem="WeeklyVolume"
                     Items="weeklyVolumeData"
                     Name="Total Volume (kg)"
                     SeriesType="SeriesType.Bar"
                     XValue="e => e.WeekStart.ToString("MMM dd")"
                     YValue="e => (decimal)e.TotalVolume" />
</ApexChart>

@code {
    private ApexChartOptions<WeeklyVolume> volumeChartOptions = new();

    protected override void OnInitialized()
    {
        volumeChartOptions.Chart = new Chart { Background = "transparent" };
        volumeChartOptions.Theme = new Theme { Mode = "dark" };
        volumeChartOptions.Tooltip = new Tooltip { Enabled = true };
    }
}
```

### Pattern 8: BlazorApexCharts Line Chart for Trends
**What:** Render a line chart for pace/distance trends over time
**When to use:** Endurance metrics, 1RM trend
**Example:**
```razor
<ApexChart TItem="WeeklyEndurance"
           Title="Pace Trend"
           Options="paceChartOptions">
    <ApexPointSeries TItem="WeeklyEndurance"
                     Items="weeklyPaceData"
                     Name="Avg Pace (min/km)"
                     SeriesType="SeriesType.Line"
                     XValue="e => e.WeekStart.ToString("MMM dd")"
                     YValue="e => (decimal)(e.AvgPace ?? 0)"
                     OrderBy="e => e.X" />
</ApexChart>
```

### Anti-Patterns to Avoid
- **Shared ApexChartOptions:** Each chart instance MUST have its own `ApexChartOptions<T>` instance. Sharing options between charts causes rendering conflicts.
- **Complex SQL aggregation via EF Core:** Don't try to push weekly GROUP BY with date math into EF Core queries for SQLite. SQLite date functions don't translate well. Fetch the range, aggregate in C#.
- **Background PR scanning:** Don't scan all historical data periodically. Detect PRs inline on session finish only (D-09).
- **Decimal type in YValue:** BlazorApexCharts YValue expects `decimal` or `decimal?`. The project uses `double` for weights -- cast to `(decimal)` in the lambda.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Interactive charts | Custom SVG/Canvas rendering | BlazorApexCharts 6.1.0 | Tooltip handling, responsiveness, animation, axis formatting -- deceptively complex |
| Dark mode chart theming | Manual CSS for chart elements | ApexCharts Theme.Mode = "dark" + Chart.Background = "transparent" | ApexCharts handles text color, grid lines, tooltip styling |
| Chart tooltip formatting | Custom overlay divs | ApexCharts built-in Tooltip with Blazor RenderFragment support | Positioning, hover detection, mobile touch all handled |
| Week number calculation | Custom ISO week logic | Simple DayOfWeek-based Monday start | ISO week numbers are complex; just use Monday-start week grouping |

**Key insight:** The charting library handles all visual complexity (axes, legends, tooltips, animations, responsive sizing). The implementation work is in data aggregation services and PR detection logic, not in rendering.

## Common Pitfalls

### Pitfall 1: ApexChartOptions Instance Sharing
**What goes wrong:** Multiple charts share the same options object, causing one chart to overwrite another's settings
**Why it happens:** Developers create one options instance and pass it to multiple `<ApexChart>` components
**How to avoid:** Create a separate `new ApexChartOptions<T>()` for each chart component
**Warning signs:** Charts displaying wrong colors, axes, or titles; charts failing to render

### Pitfall 2: EF Core SQLite Date Grouping Translation Failure
**What goes wrong:** EF Core throws "could not be translated" exception when using .Date, DateOnly, or date math in GroupBy
**Why it happens:** SQLite has limited date function support in EF Core's SQL translator
**How to avoid:** Fetch raw data with Where filter on date range, then GroupBy in C# after ToListAsync()
**Warning signs:** InvalidOperationException at runtime mentioning LINQ translation

### Pitfall 3: Empty Week Gaps Compressed in Charts
**What goes wrong:** Charts show continuous data without gaps for weeks with no activity
**Why it happens:** LINQ GroupBy only produces groups for existing data, not missing weeks
**How to avoid:** Generate the full set of week buckets between rangeStart and rangeEnd, then left-join/fill with zeros
**Warning signs:** Chart X-axis labels skip weeks, timeline appears compressed

### Pitfall 4: Epley Formula Edge Cases
**What goes wrong:** Nonsensical 1RM estimates (NaN, negative, or extremely high values)
**Why it happens:** Division by zero when reps=0, or meaningless results for reps>30
**How to avoid:** Guard: if reps <= 0 or weight <= 0 return 0; if reps == 1 return weight; cap at reps <= 30 for accuracy
**Warning signs:** 1RM showing as "NaN" or unreasonably high numbers in PR tab

### Pitfall 5: Scoped CSS Not Applying to ApexChart Elements
**What goes wrong:** CSS styles defined in .razor.css don't affect chart container or inner elements
**Why it happens:** ApexCharts renders its own DOM via JS interop, outside Blazor's CSS isolation scope
**How to avoid:** Use `::deep` for any selectors targeting ApexChart-rendered elements, or apply styles via ApexChartOptions (preferred)
**Warning signs:** Chart appears with wrong background, font, or colors despite CSS rules

### Pitfall 6: PR Detection Querying All Historical Data
**What goes wrong:** Slow session finish when user has many logged sessions
**Why it happens:** PR detection query fetches entire history without bounds
**How to avoid:** Query only the current best (MAX/MIN aggregates) per exercise, not all rows. Use indexed queries.
**Warning signs:** Session finish takes several seconds

### Pitfall 7: BlazorApexCharts YValue Type Mismatch
**What goes wrong:** Chart renders but shows wrong values or fails silently
**Why it happens:** YValue lambda must return `decimal` or `decimal?`, but entity properties are `double`
**How to avoid:** Cast in the lambda: `YValue="e => (decimal)e.TotalVolume"`
**Warning signs:** Charts showing all zeros or not rendering data series

### Pitfall 8: Forgotten AddApexCharts() Service Registration
**What goes wrong:** Runtime exception when navigating to analytics page
**Why it happens:** ApexCharts components require the ApexChartService in DI
**How to avoid:** Add `builder.Services.AddApexCharts(...)` in Program.cs
**Warning signs:** "Cannot provide a value for internal property 'ApexChartService'" exception

## Code Examples

### BlazorApexCharts Setup in Program.cs
```csharp
// Source: BlazorApexCharts official docs + project color tokens
builder.Services.AddApexCharts(e =>
{
    e.GlobalOptions = new ApexChartBaseOptions
    {
        Theme = new Theme { Mode = "dark" },
        Chart = new Chart { Background = "transparent" }
    };
});
```

### KPI Card Component Pattern
```razor
<!-- Follows existing glassmorphism card pattern from exercise library -->
<div class="kpi-card">
    <span class="kpi-card__label">@Label</span>
    <span class="kpi-card__value">@Value</span>
    @if (!string.IsNullOrEmpty(Subtitle))
    {
        <span class="kpi-card__subtitle">@Subtitle</span>
    }
</div>

@code {
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string Value { get; set; } = "";
    [Parameter] public string? Subtitle { get; set; }
}
```

### PR Detection: Epley Formula
```csharp
// Source: Wikipedia One-repetition maximum article
// Epley formula: 1RM = weight * (1 + reps / 30.0)
// Per CONTEXT.md D-10
public static double EstimateE1RM(double weight, int reps)
{
    if (reps <= 0 || weight <= 0) return 0;
    if (reps == 1) return weight; // Actual 1RM
    return Math.Round(weight * (1.0 + reps / 30.0), 1);
}
```

### Strength PR Types
```csharp
// Per D-10: three types of strength PRs
public enum StrengthPRType { Weight, Reps, EstimatedOneRepMax }

// Per D-11: two types of endurance PRs, tracked per ActivityType
public enum EndurancePRType { Pace, Distance }
```

### PersonalRecord Entity
```csharp
public class PersonalRecord
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public DateTime AchievedAt { get; set; }

    // Strength PRs
    public StrengthPRType? StrengthType { get; set; }
    public double? Value { get; set; }  // The PR value (weight, reps, or e1RM)

    // Endurance PRs
    public EndurancePRType? EnduranceType { get; set; }
    public ActivityType? ActivityType { get; set; }  // Per D-11: per activity type

    public string DisplayValue => FormatValue();

    private string FormatValue()
    {
        if (StrengthType.HasValue)
        {
            return StrengthType.Value switch
            {
                StrengthPRType.Weight => $"{Value:F1} kg",
                StrengthPRType.Reps => $"{Value:F0} reps",
                StrengthPRType.EstimatedOneRepMax => $"{Value:F1} kg (e1RM)",
                _ => $"{Value}"
            };
        }
        if (EnduranceType.HasValue)
        {
            return EnduranceType.Value switch
            {
                EndurancePRType.Pace => $"{Value:F2} min/km",
                EndurancePRType.Distance => $"{Value:F2} km",
                _ => $"{Value}"
            };
        }
        return $"{Value}";
    }
}
```

### Adherence Calculation Pattern
```csharp
// Per D-05/ANLY-03: planned vs completed vs skipped
public async Task<List<WeeklyAdherence>> GetWeeklyAdherenceAsync(DateTime rangeStart, DateTime rangeEnd)
{
    await using var context = await _contextFactory.CreateDbContextAsync();

    var scheduled = await context.ScheduledWorkouts
        .Where(sw => sw.ScheduledDate >= rangeStart && sw.ScheduledDate < rangeEnd)
        .Select(sw => new { sw.ScheduledDate, sw.Status })
        .ToListAsync();

    return scheduled
        .GroupBy(sw => GetWeekStart(sw.ScheduledDate))
        .Select(g => new WeeklyAdherence(
            g.Key,
            g.Count(),                                           // Total planned
            g.Count(sw => sw.Status == WorkoutStatus.Completed), // Completed
            g.Count(sw => sw.Status == WorkoutStatus.Skipped)    // Skipped
        ))
        .OrderBy(wa => wa.WeekStart)
        .ToList();
}

public record WeeklyAdherence(DateTime WeekStart, int Planned, int Completed, int Skipped);
```

### Per-Exercise Drill-Down Query
```csharp
// Per D-08: weekly-aggregated data points for a specific exercise
public async Task<List<ExerciseWeeklyData>> GetExerciseHistoryAsync(int exerciseId, DateTime rangeStart, DateTime rangeEnd)
{
    await using var context = await _contextFactory.CreateDbContextAsync();

    var setLogs = await context.SetLogs
        .Where(sl => sl.ExerciseId == exerciseId
            && sl.IsCompleted
            && sl.WorkoutLog.CompletedAt != null
            && sl.WorkoutLog.CompletedAt >= rangeStart
            && sl.WorkoutLog.CompletedAt < rangeEnd)
        .Select(sl => new
        {
            sl.WorkoutLog.CompletedAt,
            sl.ActualWeight,
            sl.ActualReps
        })
        .ToListAsync();

    return setLogs
        .GroupBy(sl => GetWeekStart(sl.CompletedAt!.Value))
        .Select(g => new ExerciseWeeklyData(
            g.Key,
            g.Max(sl => sl.ActualWeight ?? 0),                    // Best weight that week
            g.Max(sl => EstimateE1RM(sl.ActualWeight ?? 0, sl.ActualReps ?? 0)), // Best e1RM
            g.Count()                                              // Total sets
        ))
        .OrderBy(e => e.WeekStart)
        .ToList();
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Chart.js wrappers (Blazorise.Charts) | BlazorApexCharts with native C# API | 2024+ | Better interactive features, dark mode, tooltips |
| Server-side SQL aggregation | Client-side LINQ aggregation after fetch | Project convention | Avoids EF Core/SQLite translation issues |
| Separate PR check pages | Inline PR detection on session finish | D-09 decision | Immediate user feedback |

**Deprecated/outdated:**
- BlazorApexCharts < 6.0: No .NET 10 support. Must use 6.1.0+.

## Open Questions

1. **PR Persistence Strategy**
   - What we know: PRs should be detected inline on session finish and displayed in the PRs tab with a timeline
   - What's unclear: Whether to persist PRs as a separate entity (PersonalRecord table) or recompute from SetLog/EnduranceLog on each page load
   - Recommendation: **Persist as PersonalRecord entity.** Re-computation is expensive for PR timeline (D-13) and "when each record was set" requires knowing the specific session. A PersonalRecord row with ExerciseId, WorkoutLogId, AchievedAt, Value, and PRType is lightweight and enables fast queries. This also makes the "New PR!" toast simple -- the detection service returns the new PR records.

2. **Per-Exercise Drill-Down UX**
   - What we know: Users need to select a specific exercise and see its history (D-08)
   - What's unclear: Whether to use a dropdown selector, a separate page, or a dialog
   - Recommendation: **Use a dropdown/select within each tab** (Strength tab and Endurance tab). Default shows aggregated view across all exercises; selecting a specific exercise filters to that exercise's weekly data. This avoids navigation away from the analytics page and fits the tabbed layout.

3. **Chart Type Decisions (Claude's Discretion per D-03)**
   - Volume (total weight lifted per week): **Bar chart** -- discrete weekly buckets, natural for volume
   - Total sets per week: **Bar chart** -- discrete count data
   - Endurance pace trend: **Line chart** -- continuous trend over time
   - Endurance distance per week: **Bar chart** -- discrete weekly totals
   - Adherence (planned vs completed): **Stacked/grouped bar chart** -- comparison of two discrete values
   - e1RM trend per exercise: **Line chart** -- progress over time
   - PR timeline: **Timeline/scatter** or simple table with dates -- showing when each PR was set

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 |
| Config file | BlazorApp2.Tests/BlazorApp2.Tests.csproj |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "ClassName~Analytics" --no-build -q` |
| Full suite command | `dotnet test BlazorApp2.Tests --no-build` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| ANLY-01 | Weekly volume aggregation returns correct sums and week buckets | unit | `dotnet test BlazorApp2.Tests --filter "ClassName~AnalyticsTests" -q` | Wave 0 |
| ANLY-02 | PR detection finds weight PR, rep PR, e1RM PR, pace PR, distance PR | unit | `dotnet test BlazorApp2.Tests --filter "ClassName~PRDetectionTests" -q` | Wave 0 |
| ANLY-03 | Adherence calculation: planned vs completed counts per week | unit | `dotnet test BlazorApp2.Tests --filter "ClassName~AnalyticsTests" -q` | Wave 0 |
| ANLY-04 | Weekly endurance aggregation returns pace averages and distance sums | unit | `dotnet test BlazorApp2.Tests --filter "ClassName~AnalyticsTests" -q` | Wave 0 |
| ANLY-05 | Planned vs actual deviation computes correctly for sets and endurance | unit | `dotnet test BlazorApp2.Tests --filter "ClassName~AnalyticsTests" -q` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test BlazorApp2.Tests --filter "ClassName~Analytics|ClassName~PRDetection" --no-build -q`
- **Per wave merge:** `dotnet test BlazorApp2.Tests --no-build`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `BlazorApp2.Tests/AnalyticsTests.cs` -- covers ANLY-01, ANLY-03, ANLY-04, ANLY-05 (weekly aggregation, adherence, endurance trends, deviation)
- [ ] `BlazorApp2.Tests/PRDetectionTests.cs` -- covers ANLY-02 (PR detection logic, Epley formula, edge cases)
- [ ] PersonalRecord entity + EF migration -- required before PR persistence tests

## Sources

### Primary (HIGH confidence)
- [NuGet Blazor-ApexCharts 6.1.0](https://www.nuget.org/packages/Blazor-ApexCharts) - Version, .NET 10 support, release date confirmed via `dotnet package search`
- [GitHub Blazor-ApexCharts README](https://github.com/apexcharts/Blazor-ApexCharts) - Setup instructions, API patterns, chart types
- [ApexCharts.js Theme Docs](https://apexcharts.com/docs/options/theme/) - Dark mode configuration, palette options
- [DeepWiki Chart Types Reference](https://deepwiki.com/apexcharts/Blazor-ApexCharts/6-chart-types-reference) - SeriesType examples, tooltip customization
- Project codebase (SessionService.cs, entities, test patterns) - Established patterns for IDbContextFactory, client-side GroupBy, TestDbContextFactory

### Secondary (MEDIUM confidence)
- [Wikipedia One-repetition maximum](https://en.wikipedia.org/wiki/One-repetition_maximum) - Epley formula verification: weight * (1 + reps/30)

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Blazor-ApexCharts 6.1.0 verified on NuGet with .NET 10 target, 2.3M downloads, MIT license
- Architecture: HIGH - Patterns follow existing project conventions (IDbContextFactory, client-side GroupBy, scoped CSS, code-behind separation)
- Pitfalls: HIGH - Identified from library docs (options sharing), project history (CSS isolation, EF Core translation), and formula math (Epley edge cases)
- PR Detection: MEDIUM - PersonalRecord entity design is a recommendation; the session finish hook point is well understood

**Research date:** 2026-03-22
**Valid until:** 2026-04-22 (30 days -- Blazor-ApexCharts is stable, no breaking changes expected)
