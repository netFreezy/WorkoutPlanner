# Technology Stack

**Project:** Unified Workout Planner
**Researched:** 2026-03-21

## Recommended Stack

The base stack is decided (Blazor Server, .NET 10, EF Core, SQLite). This document focuses on the libraries, packages, and patterns that should be layered on top.

### UI Component Library

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| MudBlazor | 9.2.0 | Primary UI framework | Material Design component library with full .NET 10 support. Near-zero JavaScript. Rich component set (forms, tables, dialogs, navigation, icons, dropzones) covers 90% of UI needs. 5000+ GitHub stars, excellent docs with live examples. The DropZone component with `AllowReorder` supports drag-and-drop exercise reordering in templates. |

**Confidence:** HIGH -- Verified via NuGet (9.2.0 released 2026-03-18, targets net8.0/net9.0/net10.0).

### Calendar / Scheduler

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Heron.MudCalendar | 4.0.0 | Weekly/monthly calendar views | MudBlazor-native calendar component. Requires MudBlazor >= 9.0.0, targets .NET 8/9/10. Provides monthly and weekly views out of the box, integrates visually with MudBlazor theming. Lighter and more controllable than Radzen Scheduler for a personal app. |

**Confidence:** MEDIUM -- Package is maintained (released 2026-02-23) but is a community project with smaller user base than Radzen Scheduler. If it falls short on weekly day-slot interaction, fall back to building a custom weekly grid with MudBlazor primitives (MudSimpleTable + CSS Grid). The workout planner's "weekly view" is more of a planning grid than a full calendar scheduler, so a custom solution may end up being better UX anyway.

**Alternative considered:** Radzen.Blazor Scheduler (v8.0.4) -- more mature scheduler, but pulling in all of Radzen just for the scheduler adds a second component library with conflicting theming. Not worth the complexity for a single-user app.

### Charting / Analytics

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Blazor-ApexCharts | 6.1.0 | Volume trends, PR tracking, pace graphs | Best free charting library for Blazor. Wraps ApexCharts.js which provides line, bar, area, mixed charts with smooth animations and responsive sizing. Targets .NET 8+ (compatible with .NET 10). Supports real-time data updates, tooltips, and zoom -- all needed for training analytics. |

**Confidence:** HIGH -- Verified on NuGet (6.1.0, released 2026-01-17, targets net8.0+). ApexCharts.js is the underlying engine -- battle-tested in production across ecosystems.

### Recurrence / Scheduling Logic

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Ical.Net | 5.2.1 | RRULE recurrence rule parsing and occurrence generation | iCalendar RFC 5545 compliant library. Handles "every Monday", "every other day", "3x/week on Mon/Wed/Fri" recurrence patterns. Store the RRULE string in SQLite, use Ical.Net to expand occurrences on read. 27.8M+ NuGet downloads -- this is the .NET standard for recurrence. Targets .NET Standard 2.0 + .NET 6+ (runs on .NET 10). |

**Confidence:** HIGH -- Verified on NuGet (5.2.1, released 2026-02-09). RFC 5545 is the right standard for recurrence rules rather than inventing a custom format.

### PDF Export

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| QuestPDF | 2026.2.3 | Training data PDF export | Fluent C# API for PDF generation -- no HTML-to-PDF conversion needed. Build pixel-perfect training summaries, workout logs, and progress reports programmatically. High performance (thousands of pages/second). Free for individuals and orgs under $1M revenue. |

**Confidence:** HIGH -- Verified on NuGet (2026.2.3, actively maintained). The fluent C# API is a natural fit for a C#-only Blazor stack.

### CSV Export

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| CsvHelper | 33.1.0 | Training data CSV export | Industry-standard .NET CSV library. Fast, handles edge cases (quoting, escaping, encoding), supports strongly-typed mapping from domain objects. .NET Standard 2.0 (runs everywhere). |

**Confidence:** HIGH -- Verified on NuGet (33.1.0, released 2025-06). Battle-tested, 100M+ downloads.

### Database / ORM

| Technology | Version | Purpose | Why |
|------------|---------|---------|-----|
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.x | SQLite database provider | Ships with .NET 10. EF Core 10 is LTS (supported until 2028). SQLite JSON column support (since EF Core 8) enables complex types mapped to JSON -- use for exercise metadata and set log details. |
| Microsoft.EntityFrameworkCore.Design | 10.0.x | Migrations tooling | Required for `dotnet ef` CLI commands. Dev dependency only. |
| Microsoft.EntityFrameworkCore.Tools | 10.0.x | Package Manager Console migrations | Only if using Visual Studio PMC. Otherwise `dotnet ef` CLI suffices. |

**Confidence:** HIGH -- Official Microsoft packages, LTS release.

### Supporting Libraries

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Timers.Timer | Built-in | Session timer/stopwatch | Use for the endurance session timer. Wrap in a Blazor component with `InvokeAsync(StateHasChanged)` for thread-safe UI updates. Dispose on component teardown to prevent memory leaks. |
| System.Text.Json | Built-in | JSON serialization | Use for any manual JSON handling (e.g., RRULE metadata, API payloads). Already included in .NET 10 runtime. |
| Microsoft.Extensions.Logging | Built-in | Structured logging | Already configured in the scaffold. Use `ILogger<T>` throughout services. |

## Key Stack Patterns

### EF Core: Use TPH for the Exercise Hierarchy

The Exercise entity has a type discriminator (strength vs. endurance). Use **Table Per Hierarchy (TPH)** -- EF Core's default inheritance mapping.

**Why TPH over TPT:**
- No JOINs for polymorphic queries (listing all exercises regardless of type)
- Better query performance for the exercise library browse/search/filter use case
- Simpler migrations -- one table, one discriminator column
- SQLite has no complex JOIN optimization, making TPH's single-table approach even more advantageous

```csharp
// EF Core TPH -- just define the hierarchy, EF handles the rest
public abstract class Exercise { /* shared properties */ }
public class StrengthExercise : Exercise { /* muscle group, equipment */ }
public class EnduranceExercise : Exercise { /* activity type */ }

// DbContext
modelBuilder.Entity<Exercise>()
    .HasDiscriminator<string>("ExerciseType")
    .HasValue<StrengthExercise>("Strength")
    .HasValue<EnduranceExercise>("Endurance");
```

**Confidence:** HIGH -- TPH is EF Core's default, recommended by Microsoft for polymorphic workloads, and avoids SQLite JOIN overhead.

### EF Core: Use Complex Types with JSON for Polymorphic Log Entries

Workout log entries have different shapes (strength: sets/reps/weight vs. endurance: distance/duration/pace/HR). Use **EF Core complex types mapped to JSON columns** rather than wide nullable columns or separate tables.

**Why JSON columns:**
- EF Core 8+ supports JSON columns on SQLite via `json_extract()`
- EF Core 10 adds complex type JSON mapping with `ToJson()` and `ExecuteUpdateAsync` support
- Avoids table explosion: one LogEntry table with a JSON details column
- Type-safe in C# via complex type mapping
- Queryable -- EF Core translates LINQ into `json_extract()` for filtering/sorting

```csharp
public class LogEntry
{
    public int Id { get; set; }
    public required LogDetails Details { get; set; }
}

// Complex type -- no identity, value semantics
public class StrengthLogDetails : LogDetails { /* sets, reps, weight */ }
public class EnduranceLogDetails : LogDetails { /* distance, duration, pace, hr */ }

// In OnModelCreating:
modelBuilder.Entity<LogEntry>()
    .ComplexProperty(e => e.Details, d => d.ToJson());
```

**Confidence:** MEDIUM -- Complex types with JSON on SQLite work in EF Core 10, but polymorphic complex types (discriminated JSON) may need a value converter workaround if EF Core does not support complex type inheritance directly. Validate during implementation. Fallback: use a `string` column with `System.Text.Json` value converter and manual deserialization.

### Recurrence: Store RRULE Strings, Expand on Read

Store recurrence rules as RFC 5545 RRULE strings in the database. Use Ical.Net to expand them into concrete dates at query time.

```csharp
public class ScheduledWorkout
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public string? RecurrenceRule { get; set; } // "FREQ=WEEKLY;BYDAY=MO,WE,FR"
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// Expand occurrences
var rule = new RecurrencePattern(scheduledWorkout.RecurrenceRule);
var occurrences = rule.GetOccurrences(rangeStart, rangeEnd);
```

**Why this over materialized rows:**
- No need to pre-generate hundreds of future rows
- Changing a recurrence pattern is a single string update
- Ical.Net handles complex rules (every other week, specific days, count limits)
- RRULE is a standard -- interoperable if you ever export to calendar apps

**Confidence:** HIGH -- This is the standard pattern for calendar recurrence in any ecosystem.

### Timer: Client-Side JavaScript Interop for Stopwatch Accuracy

Blazor Server timers using `System.Timers.Timer` fire on the server and push updates over SignalR. For a workout stopwatch updating every second, this works but has latency. For sub-second display accuracy:

- Use a small JavaScript interop for the visual countdown/stopwatch display
- Use the server timer for session state (elapsed time, rest period tracking)
- Record start/stop timestamps on the server for the actual logged duration

This is the one place where a small JS interop is justified. MudBlazor's near-zero-JS philosophy still holds for everything else.

**Confidence:** MEDIUM -- Training data suggests this is the standard pattern. Timer accuracy over SignalR at 1s intervals is generally acceptable for a workout app. JS interop is a refinement, not a requirement.

## SQLite-Specific Considerations

| Concern | Impact | Mitigation |
|---------|--------|------------|
| No `DateTimeOffset` support | EF Core reads/writes DateTime but comparison/ordering may evaluate client-side | Store all times as UTC `DateTime`. Convert to local time in the UI layer only. |
| No automatic concurrency tokens | No `rowversion` equivalent | Use application-managed concurrency via a `DateTime LastModified` column with `[ConcurrencyCheck]`. For a single-user app, concurrency conflicts are extremely unlikely -- this is defensive. |
| No `decimal` ordering (pre-EF10) | EF Core 10 added `MAX`/`MIN`/`ORDER BY` for decimal on SQLite | Use EF Core 10 (which we are). No workaround needed. |
| 2GB database size limit | Theoretical limit for SQLite file | A single user's workout data will never approach this. Not a concern. |
| No migrations `ALTER COLUMN` | SQLite cannot alter column types | EF Core handles this with table rebuild migrations. Be aware that migrations may be slower but this is a dev-time concern only. |

## Alternatives Considered

| Category | Recommended | Alternative | Why Not |
|----------|-------------|-------------|---------|
| UI Library | MudBlazor 9.2.0 | Radzen.Blazor 8.0.4 | Radzen is excellent but MudBlazor has better DX, more Material Design polish, and the DropZone component covers our drag-drop needs. Mixing two libraries creates theming conflicts. |
| UI Library | MudBlazor 9.2.0 | Fluent UI Blazor | Microsoft's library but less mature component set, fewer community examples, steeper learning curve for non-Microsoft-design-system apps. |
| Charting | Blazor-ApexCharts 6.1.0 | Radzen Charts | Radzen charts are simpler but less feature-rich. ApexCharts provides better animation, more chart types, and interactive zoom for trend analysis. |
| Calendar | Heron.MudCalendar 4.0.0 | Radzen Scheduler | Radzen Scheduler is more feature-complete but pulls in the entire Radzen library. Overkill for a workout planner's weekly planning view. |
| Calendar | Heron.MudCalendar 4.0.0 | Custom MudBlazor grid | More work but more control. Keep as fallback if MudCalendar does not meet UX needs. |
| PDF Export | QuestPDF 2026.2.3 | iTextSharp / Puppeteer | iTextSharp has AGPL licensing issues. Puppeteer requires a headless browser -- too heavy. QuestPDF is pure C# with a fluent API. |
| Recurrence | Ical.Net 5.2.1 | Custom recurrence logic | Reinventing RFC 5545 is a waste of time. Ical.Net is proven and handles edge cases (leap years, DST transitions, end-of-month). |
| Inheritance | TPH | TPT | TPT adds JOINs for every query. SQLite has no JOIN optimizer. TPH is simpler and faster. |
| Inheritance | TPH | TPC (Table Per Concrete type) | TPC duplicates shared columns and complicates polymorphic queries. No benefit for this use case. |

## Installation

```bash
# Core UI
dotnet add package MudBlazor --version 9.2.0

# Calendar
dotnet add package Heron.MudCalendar --version 4.0.0

# Charting
dotnet add package Blazor-ApexCharts --version 6.1.0

# Database
dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 10.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# Recurrence
dotnet add package Ical.Net --version 5.2.1

# Export
dotnet add package QuestPDF --version 2026.2.3
dotnet add package CsvHelper --version 33.1.0
```

## What NOT to Use

| Technology | Why Avoid |
|------------|-----------|
| Radzen.Blazor alongside MudBlazor | Two component libraries = conflicting CSS, double bundle size, inconsistent UX. Pick one. |
| Syncfusion / Telerik / DevExpress | Commercial licenses are overkill for a personal app. The free tier restrictions add friction. |
| JavaScript SPA frameworks (React, etc.) | Project constraint: Blazor components only. JS interop only for timer accuracy. |
| Entity Framework Owned Entities for JSON | EF Core 10 recommends complex types over owned entities for JSON mapping. Owned entities have identity/reference semantics issues (cannot assign same instance to two properties, no ExecuteUpdateAsync support). |
| TPT inheritance for Exercise model | JOIN overhead on every query. SQLite has no advanced query planner. TPH is strictly better here. |
| Blazor WebAssembly | Project is Blazor Server. No reason to switch -- single user, local SQLite, no offline requirement. |
| Custom recurrence engine | Ical.Net exists and handles RFC 5545. Do not reinvent this wheel. |

## Sources

- [MudBlazor NuGet (v9.2.0)](https://www.nuget.org/packages/MudBlazor) -- verified 2026-03-21
- [MudBlazor .NET 10 support discussion](https://github.com/MudBlazor/MudBlazor/discussions/12122)
- [MudBlazor .NET 10 target issue](https://github.com/MudBlazor/MudBlazor/issues/12049)
- [Heron.MudCalendar NuGet (v4.0.0)](https://www.nuget.org/packages/Heron.MudCalendar) -- verified 2026-03-21
- [Blazor-ApexCharts NuGet (v6.1.0)](https://www.nuget.org/packages/Blazor-ApexCharts) -- verified 2026-03-21
- [Blazor-ApexCharts GitHub](https://github.com/apexcharts/Blazor-ApexCharts)
- [Ical.Net NuGet (v5.2.1)](https://www.nuget.org/packages/Ical.Net) -- verified 2026-03-21
- [Ical.Net GitHub](https://github.com/ical-org/ical.net)
- [QuestPDF NuGet (v2026.2.3)](https://www.nuget.org/packages/QuestPDF) -- verified 2026-03-21
- [CsvHelper NuGet (v33.1.0)](https://www.nuget.org/packages/CsvHelper) -- verified 2026-03-21
- [EF Core 10 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-10.0/whatsnew) -- Microsoft official docs
- [EF Core Inheritance (TPH/TPT/TPC)](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) -- Microsoft official docs
- [EF Core SQLite Limitations](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations) -- Microsoft official docs
- [EF Core Value Conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) -- Microsoft official docs
- [Radzen.Blazor NuGet (v8.0.4)](https://packages.nuget.org/packages/Radzen.Blazor) -- considered, not recommended
- [Fluent UI vs MudBlazor vs Radzen comparison](https://medium.com/net-code-chronicles/fluentui-vs-mudblazor-vs-radzen-ae86beb3e97b)

---

*Stack research: 2026-03-21*
