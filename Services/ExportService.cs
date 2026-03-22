using System.Globalization;
using System.Text;
using BlazorApp2.Data;
using BlazorApp2.Data.Enums;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BlazorApp2.Services;

public class ExportService(IDbContextFactory<AppDbContext> contextFactory, AnalyticsService analyticsService)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
    private readonly AnalyticsService _analyticsService = analyticsService;

    public async Task<byte[]> GenerateStrengthCsvAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var data = await context.SetLogs
            .Where(sl => sl.IsCompleted
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= rangeStart
                && sl.WorkoutLog.CompletedAt < rangeEnd)
            .OrderBy(sl => sl.WorkoutLog.CompletedAt)
            .ThenBy(sl => sl.ExerciseId)
            .ThenBy(sl => sl.SetNumber)
            .Select(sl => new
            {
                Date = sl.WorkoutLog.CompletedAt!.Value.ToString("yyyy-MM-dd"),
                Workout = sl.WorkoutLog.ScheduledWorkout.WorkoutTemplate != null
                    ? sl.WorkoutLog.ScheduledWorkout.WorkoutTemplate.Name
                    : sl.WorkoutLog.ScheduledWorkout.AdHocName ?? "Untitled",
                Exercise = sl.Exercise.Name,
                SetNum = sl.SetNumber,
                sl.PlannedWeight,
                sl.PlannedReps,
                sl.ActualWeight,
                sl.ActualReps,
                SetType = sl.SetType.ToString(),
                RPE = sl.WorkoutLog.Rpe,
                Notes = sl.WorkoutLog.Notes
            })
            .ToListAsync();

        await using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, new UTF8Encoding(true));
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(data);
        await writer.FlushAsync();
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateEnduranceCsvAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var rawData = await context.EnduranceLogs
            .Where(el => el.IsCompleted
                && el.WorkoutLog.CompletedAt != null
                && el.WorkoutLog.CompletedAt >= rangeStart
                && el.WorkoutLog.CompletedAt < rangeEnd)
            .OrderBy(el => el.WorkoutLog.CompletedAt)
            .ThenBy(el => el.ExerciseId)
            .Select(el => new
            {
                CompletedAt = el.WorkoutLog.CompletedAt!.Value,
                Workout = el.WorkoutLog.ScheduledWorkout.WorkoutTemplate != null
                    ? el.WorkoutLog.ScheduledWorkout.WorkoutTemplate.Name
                    : el.WorkoutLog.ScheduledWorkout.AdHocName ?? "Untitled",
                Exercise = el.Exercise.Name,
                el.PlannedDistance,
                PlannedDurationSeconds = el.PlannedDurationSeconds,
                el.ActualDistance,
                ActualDurationSeconds = el.ActualDurationSeconds,
                el.ActualPace,
                HRZone = el.ActualHeartRateZone,
                RPE = el.WorkoutLog.Rpe,
                Notes = el.WorkoutLog.Notes
            })
            .ToListAsync();

        var data = rawData.Select(el => new
        {
            Date = el.CompletedAt.ToString("yyyy-MM-dd"),
            el.Workout,
            el.Exercise,
            el.PlannedDistance,
            PlannedDuration = FormatDuration(el.PlannedDurationSeconds),
            el.ActualDistance,
            ActualDuration = FormatDuration(el.ActualDurationSeconds),
            el.ActualPace,
            el.HRZone,
            el.RPE,
            el.Notes
        });

        await using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, new UTF8Encoding(true));
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteRecords(data);
        await writer.FlushAsync();
        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateTrainingSummaryPdfAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Gather KPI data
        var kpi = await _analyticsService.GetKpiSummaryAsync();
        var adherence = await _analyticsService.GetWeeklyAdherenceAsync(rangeStart, rangeEnd);
        var totalCompleted = adherence.Sum(a => a.Completed);
        var totalPlanned = adherence.Sum(a => a.Planned);
        var adherencePercent = totalPlanned > 0 ? (double)totalCompleted / totalPlanned * 100 : 0;

        // PR count in range
        var prCount = await context.PersonalRecords
            .Where(pr => pr.AchievedAt >= rangeStart && pr.AchievedAt < rangeEnd)
            .CountAsync();

        // Total volume in range
        var volumeData = await context.SetLogs
            .Where(sl => sl.IsCompleted && sl.SetType == SetType.Working
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= rangeStart
                && sl.WorkoutLog.CompletedAt < rangeEnd)
            .Select(sl => new { sl.ActualWeight, sl.ActualReps })
            .ToListAsync();
        var totalVolume = volumeData.Sum(sl => (sl.ActualWeight ?? 0) * (sl.ActualReps ?? 0));

        // Session breakdown
        var sessions = await context.WorkoutLogs
            .Where(wl => wl.CompletedAt != null
                && wl.CompletedAt >= rangeStart
                && wl.CompletedAt < rangeEnd)
            .Include(wl => wl.ScheduledWorkout)
                .ThenInclude(sw => sw.WorkoutTemplate)
            .Include(wl => wl.SetLogs.OrderBy(sl => sl.SetNumber))
                .ThenInclude(sl => sl.Exercise)
            .Include(wl => wl.EnduranceLogs)
                .ThenInclude(el => el.Exercise)
            .OrderBy(wl => wl.CompletedAt)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                page.Header().Column(col =>
                {
                    col.Item().Text("Training Summary Report")
                        .FontSize(18).Bold().FontColor(Colors.Grey.Darken4);
                    col.Item().Text($"{rangeStart:MMM dd, yyyy} - {rangeEnd:MMM dd, yyyy}")
                        .FontSize(12).FontColor(Colors.Grey.Medium);
                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(10).Column(content =>
                {
                    // Period Overview KPI row
                    content.Item().PaddingBottom(15).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Sessions").FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{totalCompleted}").FontSize(16).Bold();
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Adherence").FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{adherencePercent:F0}%").FontSize(16).Bold();
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("PRs").FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{prCount}").FontSize(16).Bold();
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Total Volume").FontSize(9).FontColor(Colors.Grey.Medium);
                            c.Item().Text($"{totalVolume:N0} kg").FontSize(16).Bold();
                        });
                    });

                    content.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                    content.Item().PaddingTop(10);

                    // Per-session breakdown
                    foreach (var session in sessions)
                    {
                        var workoutName = session.ScheduledWorkout.WorkoutTemplate?.Name
                            ?? session.ScheduledWorkout.AdHocName ?? "Untitled";

                        content.Item().PaddingBottom(10).Column(sessionCol =>
                        {
                            sessionCol.Item().Text($"{session.CompletedAt:ddd, MMM dd yyyy} - {workoutName}")
                                .FontSize(11).Bold();

                            // Strength sets
                            if (session.SetLogs.Any())
                            {
                                sessionCol.Item().PaddingTop(4).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(3); // Exercise
                                        cols.RelativeColumn(2); // Planned
                                        cols.RelativeColumn(2); // Actual
                                        cols.RelativeColumn(2); // Set Details
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Exercise").FontSize(9).Bold();
                                        header.Cell().Text("Planned").FontSize(9).Bold();
                                        header.Cell().Text("Actual").FontSize(9).Bold();
                                        header.Cell().Text("Set Details").FontSize(9).Bold();
                                    });

                                    foreach (var setLog in session.SetLogs)
                                    {
                                        table.Cell().Text(setLog.Exercise.Name).FontSize(9);
                                        table.Cell().Text($"{setLog.PlannedWeight ?? 0}kg x {setLog.PlannedReps ?? 0}").FontSize(9);
                                        table.Cell().Text($"{setLog.ActualWeight ?? 0}kg x {setLog.ActualReps ?? 0}").FontSize(9);
                                        table.Cell().Text($"Set {setLog.SetNumber} ({setLog.SetType})").FontSize(9);
                                    }
                                });
                            }

                            // Endurance logs
                            if (session.EnduranceLogs.Any())
                            {
                                sessionCol.Item().PaddingTop(4).Table(table =>
                                {
                                    table.ColumnsDefinition(cols =>
                                    {
                                        cols.RelativeColumn(3); // Exercise
                                        cols.RelativeColumn(2); // Planned
                                        cols.RelativeColumn(2); // Actual
                                        cols.RelativeColumn(2); // Pace
                                    });

                                    table.Header(header =>
                                    {
                                        header.Cell().Text("Exercise").FontSize(9).Bold();
                                        header.Cell().Text("Planned").FontSize(9).Bold();
                                        header.Cell().Text("Actual").FontSize(9).Bold();
                                        header.Cell().Text("Pace").FontSize(9).Bold();
                                    });

                                    foreach (var el in session.EnduranceLogs)
                                    {
                                        table.Cell().Text(el.Exercise.Name).FontSize(9);
                                        table.Cell().Text($"{el.PlannedDistance ?? 0}km / {FormatDuration(el.PlannedDurationSeconds)}").FontSize(9);
                                        table.Cell().Text($"{el.ActualDistance ?? 0}km / {FormatDuration(el.ActualDurationSeconds)}").FontSize(9);
                                        table.Cell().Text(el.ActualPace.HasValue ? $"{el.ActualPace:F2} min/km" : "-").FontSize(9);
                                    }
                                });
                            }

                            if (session.Rpe.HasValue)
                                sessionCol.Item().PaddingTop(2).Text($"RPE: {session.Rpe}/10").FontSize(9).FontColor(Colors.Grey.Medium);

                            if (!string.IsNullOrWhiteSpace(session.Notes))
                                sessionCol.Item().PaddingTop(2).Text(session.Notes).FontSize(9).Italic().FontColor(Colors.Grey.Medium);
                        });
                    }
                });

                page.Footer().AlignCenter().Row(footer =>
                {
                    footer.RelativeItem().AlignLeft().Text(text =>
                    {
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" of ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                    footer.RelativeItem().AlignRight().Text($"Generated on {DateTime.UtcNow:yyyy-MM-dd}")
                        .FontSize(8).FontColor(Colors.Grey.Medium);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static string FormatDuration(int? totalSeconds)
    {
        if (totalSeconds == null) return "-";
        var minutes = totalSeconds.Value / 60;
        var seconds = totalSeconds.Value % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }
}
