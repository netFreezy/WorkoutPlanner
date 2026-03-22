# Phase 7: Quality of Life - Research

**Researched:** 2026-03-22
**Domain:** Blazor Server dashboard, progressive overload logic, CSV/PDF export, workout history browser
**Confidence:** HIGH

## Summary

Phase 7 transforms the app from a functional tool into a polished daily driver. The four feature areas are largely independent: (1) a home screen dashboard that replaces the empty Home.razor placeholder, (2) progressive overload detection using existing session history data, (3) CSV and PDF export using CsvHelper and QuestPDF respectively, and (4) a workout history browser with search/filter.

The existing codebase provides strong foundations: `SessionService.GetPreviousStrengthPerformanceAsync()` already queries past performance per exercise (reusable for overload detection), `SchedulingService.GetWorkoutsForWeekAsync()` and `SessionService.GetTodaysWorkoutsAsync()` provide today's scheduled data, and `AnalyticsService` has all the aggregate query patterns needed for PDF summaries. The main new technical surface is PDF generation (QuestPDF) and file download via JS interop (Blazor's `DotNetStreamReference` pattern).

**Primary recommendation:** Use QuestPDF 2026.2.4 for PDF generation (community license, free for personal use) and CsvHelper 33.1.0 for CSV export. Deliver file downloads via the standard Blazor `DotNetStreamReference` + JS interop pattern from Microsoft's official docs. Build a new `OverloadService` for progressive overload logic, and a new `ExportService` for data export. History browser gets its own page at `/history`.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Full workout preview -- show today's scheduled workout with exercise list, targets (sets/reps/weight or distance/duration), and a prominent "Start Session" button
- **D-02:** "Up next" line below the workout showing tomorrow's scheduled workout for context
- **D-03:** When no workout is scheduled: show last completed workout with actual logged weights (not template targets), plus a "Browse Templates" option to pick a different workout
- **D-04:** No summary stats on home screen -- keep it purely action-focused. Analytics page handles stats
- **D-05:** Trigger after 2 consecutive sessions where all working sets hit target reps at the same weight for that exercise
- **D-06:** Show as inline suggestion card at session start -- appears per exercise with "Apply" (updates the target weight for this session) and "Dismiss" actions
- **D-07:** Weight increments based on exercise muscle group: +2.5kg for upper body compounds (bench, rows, OHP, pull-ups), +5kg for lower body compounds (squat, deadlift, leg press), +1kg for isolation exercises (curls, laterals, triceps)
- **D-08:** Strength exercises only -- no progressive overload suggestions for endurance exercises
- **D-09:** CSV export = one row per set/entry with full detail. Strength columns: Date, Workout, Exercise, SetNum, PlannedWeight, PlannedReps, ActualWeight, ActualReps, SetType, RPE, Notes. Endurance columns: Date, Workout, Exercise, PlannedDistance, PlannedDuration, ActualDistance, ActualDuration, ActualPace, HRZone, RPE, Notes
- **D-10:** Export buttons on the analytics page, reusing the existing time range selector (4W/8W/12W/All) for date filtering
- **D-11:** PDF = training summary report with period overview (sessions completed, adherence %, PRs hit, total volume) + chronological per-session breakdown
- **D-12:** Rich summary cards: date, workout name, type icon (strength/endurance), exercise count, duration, total volume (strength) or distance (endurance), RPE badge
- **D-13:** Four filter options: text search (workout/exercise name), date range picker, workout type filter (strength/endurance/mixed), exercise filter (sessions containing a specific exercise)
- **D-14:** Expandable inline detail -- tap a card to reveal full exercise breakdown (sets with reps/weight, endurance entries) and session notes. Collapse to return to list

### Claude's Discretion
- History page placement in nav (new nav link vs sub-route of analytics)
- PDF generation library choice (server-side rendering approach)
- CSV file naming convention and encoding
- Overload suggestion dismissal persistence (per-session vs remembered)
- History pagination vs infinite scroll
- Empty states for home screen (no workouts ever created)

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| QOL-01 | Quick-start -- home screen shows today's scheduled workout, one action to start logging | `SessionService.GetTodaysWorkoutsAsync()` already exists; `SchedulingService.GetWorkoutsForWeekAsync()` can get tomorrow's workout; `WorkoutDetailDialog.razor` has reusable exercise list rendering pattern |
| QOL-02 | "Repeat last workout" option when no workout scheduled | Need new query: get most recent completed WorkoutLog with template + actual logged values; use existing `SchedulingService.ScheduleWorkoutAsync()` to create ad-hoc scheduled workout |
| QOL-03 | Progressive overload suggestions -- nudge to increase weight when target hit consistently | `SessionService.GetPreviousStrengthPerformanceAsync()` provides history data; new `OverloadService` needed; `MuscleGroup` enum exists on `StrengthExercise` for increment categorization |
| QOL-04 | CSV export of all training data | CsvHelper 33.1.0 for CSV generation; Blazor `DotNetStreamReference` + JS interop for file download |
| QOL-05 | PDF export of workout templates and training summaries | QuestPDF 2026.2.4 for PDF generation; reuse `AnalyticsService` aggregation methods for summary stats |
| QOL-06 | Workout history -- chronological list of completed sessions with search/filter | New `HistoryService` querying `WorkoutLogs` with includes; client-side filtering pattern (proven in Exercises page) |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| QuestPDF | 2026.2.4 | PDF document generation | The standard .NET PDF library: fluent C# API, no external dependencies, free community license for personal projects, 18M+ downloads |
| CsvHelper | 33.1.0 | CSV file generation | De facto .NET CSV library with 552M+ downloads, handles encoding/escaping edge cases, strongly-typed mapping |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Blazor-ApexCharts | 6.1.0 (already installed) | Charts on analytics page | Already in use -- no changes needed |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.5 (already installed) | Data access | Already in use -- no changes needed |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| QuestPDF | iTextSharp | iText is AGPL (copyleft license), QuestPDF is simpler API and free for personal use |
| QuestPDF | PdfSharp | PdfSharp is lower-level, requires manual layout math; QuestPDF's fluent API is far more productive |
| CsvHelper | Manual StringBuilder | CsvHelper handles all quoting/escaping edge cases (commas in notes, newlines, Unicode BOM) |

**Installation:**
```bash
dotnet add package QuestPDF --version 2026.2.4
dotnet add package CsvHelper --version 33.1.0
```

**Version verification:**
- QuestPDF 2026.2.4 -- verified via `dotnet package search QuestPDF` on 2026-03-22
- CsvHelper 33.1.0 -- verified via `dotnet package search CsvHelper` on 2026-03-22

## Architecture Patterns

### Recommended Project Structure
```
Services/
    OverloadService.cs       # Progressive overload detection + suggestion logic
    ExportService.cs         # CSV and PDF generation (byte array output)
    HistoryService.cs        # Workout history queries with filtering
Components/
    Pages/
        Home.razor           # REPLACE empty placeholder with dashboard (existing file)
        Home.razor.cs        # Code-behind for dashboard logic
        Home.razor.css       # Scoped styles for dashboard
        History.razor         # NEW workout history browser page
        History.razor.cs
        History.razor.css
    Shared/
        OverloadSuggestion.razor     # Suggestion card component (per exercise)
        OverloadSuggestion.razor.css
        HistoryCard.razor            # Summary card for completed session
        HistoryCard.razor.css
        HistoryDetail.razor          # Expandable detail panel for a session
        HistoryDetail.razor.css
wwwroot/
    js/
        file-download.js     # downloadFileFromStream JS interop function
```

### Pattern 1: File Download via DotNetStreamReference (Blazor Official Pattern)
**What:** Generate files server-side as byte arrays, stream to client via JS interop
**When to use:** For CSV and PDF export from interactive server components
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads
// JS function (file-download.js):
// window.downloadFileFromStream = async (fileName, contentStreamReference) => {
//     const arrayBuffer = await contentStreamReference.arrayBuffer();
//     const blob = new Blob([arrayBuffer]);
//     const url = URL.createObjectURL(blob);
//     const anchorElement = document.createElement('a');
//     anchorElement.href = url;
//     anchorElement.download = fileName ?? '';
//     anchorElement.click();
//     anchorElement.remove();
//     URL.revokeObjectURL(url);
// }

// C# component code:
@inject IJSRuntime JS

private async Task DownloadCsv()
{
    var csvBytes = await ExportService.GenerateCsvAsync(RangeStart, RangeEnd);
    using var stream = new MemoryStream(csvBytes);
    using var streamRef = new DotNetStreamReference(stream);
    await JS.InvokeVoidAsync("downloadFileFromStream",
        $"training-data-{DateTime.UtcNow:yyyy-MM-dd}.csv", streamRef);
}
```

### Pattern 2: QuestPDF Document Composition
**What:** Fluent C# API to build PDF layout with tables, headers, pagination
**When to use:** For the training summary PDF report
**Example:**
```csharp
// Source: https://www.questpdf.com/quick-start.html
QuestPDF.Settings.License = LicenseType.Community;

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(2, Unit.Centimetre);
        page.DefaultTextStyle(x => x.FontSize(11));

        page.Header().Text("Training Summary").FontSize(20).Bold();

        page.Content().Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Exercise
                    columns.RelativeColumn(1); // Sets
                    columns.RelativeColumn(1); // Best Weight
                });
                table.Header(header =>
                {
                    header.Cell().Text("Exercise").Bold();
                    header.Cell().Text("Sets").Bold();
                    header.Cell().Text("Best Weight").Bold();
                });
                // Add data rows in foreach
            });
        });

        page.Footer().AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    });
}).GeneratePdf(); // Returns byte[]
```

### Pattern 3: Progressive Overload Detection Algorithm
**What:** Query last N sessions for an exercise, check if target reps were consistently hit
**When to use:** At session start, per strength exercise
**Example:**
```csharp
// For each strength exercise in the session:
// 1. Get last 2 completed sessions for this exercise
// 2. For each session, check if ALL working sets hit planned reps at planned weight
// 3. If both sessions qualify -> suggest weight increase
// 4. Increment based on MuscleGroup:
//    - Chest, Back, Shoulders (compounds): +2.5kg
//    - Legs, FullBody (compounds): +5kg
//    - Arms (isolation): +1kg

public record OverloadSuggestion(
    int ExerciseId,
    string ExerciseName,
    double CurrentWeight,
    double SuggestedWeight,
    double Increment,
    int ConsecutiveSessions);
```

### Pattern 4: IDbContextFactory Per-Method (Established Project Pattern)
**What:** Each service method creates its own DbContext from the factory
**When to use:** All new service methods in OverloadService, ExportService, HistoryService
**Example:**
```csharp
public async Task<List<OverloadSuggestion>> GetSuggestionsAsync(int workoutLogId)
{
    await using var context = await _contextFactory.CreateDbContextAsync();
    // ... query logic
}
```

### Anti-Patterns to Avoid
- **Scoped DbContext injection in services:** This project uses `IDbContextFactory` exclusively -- never inject `AppDbContext` directly
- **Server-side file storage for exports:** Generate bytes in memory, stream to client immediately. No temp files on server
- **Eager loading all history at once:** The history page should paginate or limit initial load. Use Skip/Take pattern for DB queries
- **Calculating overload suggestions on every set completion:** Only compute at session start (per D-06). Cache per session lifecycle

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| CSV generation | Manual string concatenation | CsvHelper 33.1.0 | Handles commas in notes, newlines, Unicode BOM, proper quoting |
| PDF layout | HTML-to-PDF conversion | QuestPDF fluent API | Purpose-built for document generation; proper pagination, tables, fonts |
| File download | Custom WebSocket binary transfer | Blazor's `DotNetStreamReference` + JS interop | Official Microsoft pattern, handles cleanup, memory management |
| Date range filtering | Custom date parsing | Reuse analytics page time range selector (4W/8W/12W/All) pattern | Already proven in Analytics.razor, per D-10 |
| Workout type detection | Duplicate type logic | Reuse `SchedulingService.DetermineWorkoutType()` static method | Already handles all edge cases (strength/endurance/mixed/ad-hoc) |

**Key insight:** The existing codebase already has 80% of the query infrastructure needed for this phase. The new services compose existing queries rather than writing novel data access patterns.

## Common Pitfalls

### Pitfall 1: QuestPDF License Configuration Missing
**What goes wrong:** QuestPDF throws an exception at runtime if `QuestPDF.Settings.License` is not set before generating any document
**Why it happens:** Starting from 2024.3.0, QuestPDF requires explicit license selection
**How to avoid:** Set `QuestPDF.Settings.License = LicenseType.Community;` in `Program.cs` at startup, before any PDF generation occurs
**Warning signs:** Runtime exception "QuestPDF license has not been configured"

### Pitfall 2: JS Interop File Not Loaded for File Download
**What goes wrong:** `JSException: Could not find 'downloadFileFromStream' ('downloadFileFromStream' was undefined)`
**Why it happens:** The JS function must be loaded before it can be called from C#
**How to avoid:** Add the `file-download.js` script reference in `App.razor` after the Blazor script, or use a co-located `.razor.js` module
**Warning signs:** Export buttons work in development but fail with JS errors

### Pitfall 3: Memory Pressure from Large PDF/CSV Generation
**What goes wrong:** Out-of-memory or slow response for users with years of training data
**Why it happens:** Loading all WorkoutLog + SetLog + EnduranceLog entities into memory at once
**How to avoid:** Use projection queries (Select only needed columns), stream CSV rows instead of building full list, limit PDF to reasonable date range
**Warning signs:** Export taking > 5 seconds or browser timeout

### Pitfall 4: DbContext Threading Issues in Blazor Server
**What goes wrong:** `InvalidOperationException: A second operation was started on this context instance before a previous operation was completed`
**Why it happens:** Multiple async calls on the same DbContext instance
**How to avoid:** Use `IDbContextFactory` per-method pattern (already established in this project). Never share a context across methods
**Warning signs:** Intermittent exceptions during rapid UI interactions

### Pitfall 5: CSS Isolation with InputText/InputSelect in History Filters
**What goes wrong:** Blazor scoped CSS (`::deep`) required for styling form elements nested inside child components
**Why it happens:** Blazor CSS isolation adds `b-{hash}` attributes only to the owning component's elements, not child component elements
**How to avoid:** Use `::deep` selector in parent `.razor.css` when styling `InputText`, `InputSelect` or any child-rendered elements
**Warning signs:** Filter inputs appear unstyled despite CSS being written

### Pitfall 6: Overload Detection False Positives with Warm-up Sets
**What goes wrong:** Warm-up sets included in "all working sets hit target" check, giving incorrect suggestions
**Why it happens:** Querying all SetLogs without filtering by SetType
**How to avoid:** Filter to `SetType.Working` only (exclude WarmUp, Failure, Drop sets) -- exactly as PRDetectionService already does
**Warning signs:** Overload suggestions appearing after only warm-up completion

### Pitfall 7: Date/Time Comparison Mismatch (UTC vs Local)
**What goes wrong:** "Today's workout" shows wrong workout because UTC date differs from user's local date
**Why it happens:** `DateTime.UtcNow.Date` may be different from user's local date near midnight
**How to avoid:** The existing `GetTodaysWorkoutsAsync()` uses `DateTime.UtcNow.Date` consistently -- maintain this pattern. For history date range filters, be explicit about comparing `.Date` properties
**Warning signs:** Workouts appearing on wrong day near midnight

## Code Examples

### CsvHelper: Generating CSV from Entity Data
```csharp
// Source: https://joshclose.github.io/CsvHelper/getting-started/
using CsvHelper;
using System.Globalization;
using System.Text;

public async Task<byte[]> GenerateStrengthCsvAsync(DateTime rangeStart, DateTime rangeEnd)
{
    await using var context = await _contextFactory.CreateDbContextAsync();

    var data = await context.SetLogs
        .Where(sl => sl.IsCompleted
            && sl.WorkoutLog.CompletedAt != null
            && sl.WorkoutLog.CompletedAt >= rangeStart
            && sl.WorkoutLog.CompletedAt < rangeEnd)
        .Select(sl => new
        {
            Date = sl.WorkoutLog.CompletedAt,
            Workout = sl.WorkoutLog.ScheduledWorkout.WorkoutTemplate!.Name
                ?? sl.WorkoutLog.ScheduledWorkout.AdHocName ?? "Untitled",
            Exercise = sl.Exercise.Name,
            SetNum = sl.SetNumber,
            PlannedWeight = sl.PlannedWeight,
            PlannedReps = sl.PlannedReps,
            ActualWeight = sl.ActualWeight,
            ActualReps = sl.ActualReps,
            SetType = sl.SetType.ToString(),
            RPE = sl.WorkoutLog.Rpe,
            Notes = sl.WorkoutLog.Notes
        })
        .OrderBy(r => r.Date)
        .ThenBy(r => r.Exercise)
        .ThenBy(r => r.SetNum)
        .ToListAsync();

    using var memoryStream = new MemoryStream();
    // UTF-8 with BOM for Excel compatibility
    using var writer = new StreamWriter(memoryStream, new UTF8Encoding(true));
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.WriteRecords(data);
    await writer.FlushAsync();
    return memoryStream.ToArray();
}
```

### QuestPDF: Training Summary Report
```csharp
// Source: https://www.questpdf.com/api-reference/table/basics.html
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public byte[] GenerateTrainingSummaryPdf(TrainingSummaryData data)
{
    return Document.Create(container =>
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

            page.Header().Column(col =>
            {
                col.Item().Text("Training Summary Report").FontSize(18).Bold();
                col.Item().Text($"{data.RangeStart:MMM d} - {data.RangeEnd:MMM d, yyyy}")
                    .FontSize(12).FontColor(Colors.Grey.Medium);
            });

            page.Content().Column(col =>
            {
                // Period overview KPIs
                col.Item().PaddingVertical(10).Row(row =>
                {
                    row.RelativeItem().Text($"Sessions: {data.SessionCount}");
                    row.RelativeItem().Text($"Adherence: {data.AdherencePercent:F0}%");
                    row.RelativeItem().Text($"PRs: {data.PrCount}");
                    row.RelativeItem().Text($"Volume: {data.TotalVolume:N0} kg");
                });

                // Per-session breakdown
                foreach (var session in data.Sessions)
                {
                    col.Item().PaddingTop(10).BorderTop(1).BorderColor(Colors.Grey.Lighten2);
                    col.Item().Text($"{session.Date:ddd MMM d} - {session.WorkoutName}")
                        .FontSize(11).Bold();
                    // ... exercise details table
                }
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Page "); x.CurrentPageNumber();
                x.Span(" of "); x.TotalPages();
            });
        });
    }).GeneratePdf();
}
```

### Blazor File Download JS Interop
```javascript
// Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads
// wwwroot/js/file-download.js
window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
```

### Progressive Overload: Muscle Group to Increment Mapping
```csharp
// Per D-07: Weight increments based on muscle group
public static double GetWeightIncrement(MuscleGroup muscleGroup) => muscleGroup switch
{
    // Upper body compounds: +2.5kg
    MuscleGroup.Chest => 2.5,
    MuscleGroup.Back => 2.5,
    MuscleGroup.Shoulders => 2.5,
    // Lower body compounds: +5kg
    MuscleGroup.Legs => 5.0,
    MuscleGroup.FullBody => 5.0,
    // Isolation: +1kg
    MuscleGroup.Arms => 1.0,
    MuscleGroup.Core => 1.0,
    _ => 2.5 // safe default
};
```

## Discretion Recommendations

Based on research, here are recommendations for the areas left to Claude's discretion:

### History Page Placement: New Nav Link
**Recommendation:** Add "History" as a new nav link in MainLayout.razor (7th link), positioned between "Session" and "Analytics". This keeps it discoverable -- burying it as a sub-route of analytics would hide a primary feature. The nav already has 6 links; 7 is still manageable.

### CSV File Naming Convention
**Recommendation:** Use pattern `workout-data-YYYY-MM-DD.csv` for the combined export file. UTF-8 with BOM for Excel compatibility. Use `CultureInfo.InvariantCulture` for consistent number formatting across locales.

### Overload Suggestion Dismissal: Per-Session (Not Persisted)
**Recommendation:** Store dismissed suggestions in the session component's state (a `HashSet<int>` of exercise IDs). Do not persist to database. Rationale: if the user dismisses a suggestion and comes back next session, the same suggestion should reappear if they still meet the criteria -- they may have changed their mind. Avoids schema changes.

### History: Pagination Over Infinite Scroll
**Recommendation:** Use cursor-based pagination (Load More button) rather than infinite scroll. Load 20 sessions initially, "Load More" fetches next 20. Simpler implementation than virtual scrolling, works reliably in Blazor Server without complex JS interop for scroll detection. Uses EF Core `Skip()`/`Take()` pattern.

### Empty State for Home Screen (No Workouts Ever)
**Recommendation:** Show a welcoming empty state card: "No workouts scheduled yet -- Create your first workout template to get started" with links to /templates and /exercises. Reuse the empty-state styling pattern from Analytics.razor.

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| iTextSharp for .NET PDF | QuestPDF fluent API | 2022+ | Free for personal use, C#-native layout, no HTML intermediary |
| Manual CSV with StringBuilder | CsvHelper with type mapping | Mature (2012+) | Handles all edge cases, RFC 4180 compliant |
| Blazor file download via base64 data URI | DotNetStreamReference streaming | .NET 6+ (2021) | Efficient binary streaming, no base64 overhead, officially supported |

**Deprecated/outdated:**
- `@bind-Value` pattern for form inputs: still valid but note that `::deep` is required in parent scoped CSS for styling
- `Blazor.FileReader` NuGet package: obsoleted by built-in `DotNetStreamReference` in .NET 6+

## Open Questions

1. **MuscleGroup Classification for Mixed Exercises**
   - What we know: D-07 specifies increments by muscle group, and `MuscleGroup` enum has: Chest, Back, Shoulders, Legs, Arms, Core, FullBody
   - What's unclear: Some exercises could be classified differently (e.g., pull-ups as Back +2.5kg or compound FullBody +5kg)
   - Recommendation: Use the muscle group as defined on the exercise entity. The user controls exercise classification when creating exercises. If FullBody, use +5kg per D-07 mapping

2. **QuestPDF Native Dependencies on Deployment**
   - What we know: QuestPDF uses SkiaSharp internally, which has native dependencies
   - What's unclear: Whether this works out-of-box on the deployment target
   - Recommendation: Test PDF generation in a quick spike. For Windows development/deployment (this project's environment), it works without extra configuration. LOW risk for single-user local app

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 |
| Config file | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~OverloadTests" --no-build -q` |
| Full suite command | `dotnet test BlazorApp2.Tests -q` |

### Phase Requirements to Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| QOL-01 | Today's workout retrieved for home screen | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~HomeTests" -q` | No -- Wave 0 |
| QOL-02 | Last completed workout retrieved when none scheduled | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~HomeTests" -q` | No -- Wave 0 |
| QOL-03 | Overload triggers after 2 consecutive qualifying sessions | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~OverloadTests" -q` | No -- Wave 0 |
| QOL-03 | Correct increment by muscle group | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~OverloadTests" -q` | No -- Wave 0 |
| QOL-04 | CSV export generates valid output with correct columns | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExportTests" -q` | No -- Wave 0 |
| QOL-05 | PDF export generates non-empty byte array | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExportTests" -q` | No -- Wave 0 |
| QOL-06 | History query returns completed sessions with correct filters | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~HistoryTests" -q` | No -- Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~{TestClass}" -q`
- **Per wave merge:** `dotnet test BlazorApp2.Tests -q`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `BlazorApp2.Tests/OverloadTests.cs` -- covers QOL-03 (overload detection logic, increment mapping, edge cases)
- [ ] `BlazorApp2.Tests/ExportTests.cs` -- covers QOL-04, QOL-05 (CSV column structure, PDF byte generation)
- [ ] `BlazorApp2.Tests/HistoryTests.cs` -- covers QOL-06 (history queries, filtering)
- [ ] `BlazorApp2.Tests/HomeTests.cs` -- covers QOL-01, QOL-02 (today's workout, last workout query)
- [ ] NuGet packages: `dotnet add BlazorApp2 package QuestPDF --version 2026.2.4` and `dotnet add BlazorApp2 package CsvHelper --version 33.1.0`

## Sources

### Primary (HIGH confidence)
- [Microsoft Blazor File Downloads docs](https://learn.microsoft.com/en-us/aspnet/core/blazor/file-downloads?view=aspnetcore-10.0) -- DotNetStreamReference pattern, JS interop function
- [QuestPDF NuGet](https://www.nuget.org/packages/QuestPDF) -- Version 2026.2.4 verified
- [QuestPDF License Configuration](https://www.questpdf.com/license/configuration.html) -- Community license setup
- [CsvHelper NuGet](https://www.nuget.org/packages/CsvHelper) -- Version 33.1.0 verified
- Existing codebase: `SessionService.cs`, `AnalyticsService.cs`, `PRDetectionService.cs`, `SchedulingService.cs` -- query patterns, data model

### Secondary (MEDIUM confidence)
- [QuestPDF Table API docs](https://www.questpdf.com/api-reference/table/basics.html) -- Table layout patterns
- [CsvHelper Getting Started](https://joshclose.github.io/CsvHelper/getting-started/) -- Writing patterns

### Tertiary (LOW confidence)
- QuestPDF SkiaSharp native dependency compatibility on all deployment targets -- verified for Windows only

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- QuestPDF and CsvHelper are well-established, versions verified against NuGet
- Architecture: HIGH -- Follows established project patterns (IDbContextFactory, scoped services, co-located Razor components)
- Pitfalls: HIGH -- Identified from official docs, existing project decisions, and known Blazor Server patterns
- Progressive overload logic: HIGH -- Algorithm is straightforward, data model supports it, D-05/D-07 fully specify behavior

**Research date:** 2026-03-22
**Valid until:** 2026-04-22 (stable domain, no fast-moving APIs)
