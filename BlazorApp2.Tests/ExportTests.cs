using System.Text;
using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class ExportTests : DataTestBase
{
    private readonly ExportService _exportService;
    private readonly TestDbContextFactory _factory;
    private readonly StrengthExercise _benchPress;
    private readonly EnduranceExercise _run5k;

    public ExportTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        _factory = new TestDbContextFactory(Connection);
        var analyticsService = new AnalyticsService(_factory);
        _exportService = new ExportService(_factory, analyticsService);

        _benchPress = new StrengthExercise
        {
            Name = "Bench Press",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        };
        _run5k = new EnduranceExercise
        {
            Name = "5K Run",
            ActivityType = ActivityType.Run
        };

        Context.StrengthExercises.Add(_benchPress);
        Context.EnduranceExercises.Add(_run5k);
        Context.SaveChanges();

        // Create 1 completed session with 3 SetLogs and 1 EnduranceLog
        var template = new WorkoutTemplate { Name = "Test Workout" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = new DateTime(2026, 3, 15, 8, 0, 0, DateTimeKind.Utc),
            CompletedAt = new DateTime(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc)
        };

        for (int i = 1; i <= 3; i++)
        {
            log.SetLogs.Add(new SetLog
            {
                ExerciseId = _benchPress.Id,
                SetNumber = i,
                SetType = SetType.Working,
                PlannedReps = 8,
                PlannedWeight = 80.0,
                ActualReps = 8,
                ActualWeight = 80.0,
                IsCompleted = true
            });
        }

        log.EnduranceLogs.Add(new EnduranceLog
        {
            ExerciseId = _run5k.Id,
            ActivityType = ActivityType.Run,
            PlannedDistance = 5.0,
            PlannedDurationSeconds = 1500,
            ActualDistance = 5.0,
            ActualDurationSeconds = 1500,
            ActualPace = 5.0,
            IsCompleted = true
        });

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
    }

    // --- Strength CSV tests ---

    [Fact]
    public async Task GenerateStrengthCsv_ReturnsNonEmptyBytes()
    {
        var bytes = await _exportService.GenerateStrengthCsvAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task GenerateStrengthCsv_ContainsCorrectHeaderColumns()
    {
        var bytes = await _exportService.GenerateStrengthCsvAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Header should contain key column names
        var header = lines[0];
        Assert.Contains("Date", header);
        Assert.Contains("ActualWeight", header);
        Assert.Contains("ActualReps", header);
        Assert.Contains("SetType", header);
    }

    [Fact]
    public async Task GenerateStrengthCsv_DataRowCountMatchesSetLogs()
    {
        var bytes = await _exportService.GenerateStrengthCsvAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // 1 header + 3 data rows (3 SetLogs)
        Assert.Equal(4, lines.Length);
    }

    [Fact]
    public async Task GenerateStrengthCsv_IncludesUtf8Bom()
    {
        var bytes = await _exportService.GenerateStrengthCsvAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        // UTF-8 BOM: 0xEF 0xBB 0xBF
        Assert.True(bytes.Length >= 3);
        Assert.Equal(0xEF, bytes[0]);
        Assert.Equal(0xBB, bytes[1]);
        Assert.Equal(0xBF, bytes[2]);
    }

    [Fact]
    public async Task GenerateStrengthCsv_EmptyRange_ReturnsHeaderOnly()
    {
        var bytes = await _exportService.GenerateStrengthCsvAsync(
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc));

        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Header line only, no data
        Assert.Single(lines);
        Assert.Contains("Date", lines[0]);
    }

    // --- Endurance CSV tests ---

    [Fact]
    public async Task GenerateEnduranceCsv_ReturnsNonEmptyBytes()
    {
        var bytes = await _exportService.GenerateEnduranceCsvAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public async Task GenerateEnduranceCsv_ContainsEnduranceColumns()
    {
        var bytes = await _exportService.GenerateEnduranceCsvAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        var csv = Encoding.UTF8.GetString(bytes);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var header = lines[0];
        Assert.Contains("ActualDistance", header);
        Assert.Contains("ActualPace", header);

        // 1 header + 1 data row (1 EnduranceLog)
        Assert.Equal(2, lines.Length);
    }

    // --- PDF tests ---

    [Fact]
    public async Task GenerateTrainingSummaryPdf_ReturnsNonEmptyBytes()
    {
        var bytes = await _exportService.GenerateTrainingSummaryPdfAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 100);
    }

    [Fact]
    public async Task GenerateTrainingSummaryPdf_StartsWith_PdfMagicBytes()
    {
        var bytes = await _exportService.GenerateTrainingSummaryPdfAsync(
            new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc));

        // %PDF magic bytes: 0x25 0x50 0x44 0x46
        Assert.True(bytes.Length >= 4);
        Assert.Equal(0x25, bytes[0]); // %
        Assert.Equal(0x50, bytes[1]); // P
        Assert.Equal(0x44, bytes[2]); // D
        Assert.Equal(0x46, bytes[3]); // F
    }
}
