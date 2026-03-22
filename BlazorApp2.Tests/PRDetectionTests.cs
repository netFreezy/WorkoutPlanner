using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class PRDetectionTests : DataTestBase
{
    private readonly PRDetectionService _prService;
    private readonly StrengthExercise _benchPress;
    private readonly StrengthExercise _squat;
    private readonly EnduranceExercise _run5k;
    private readonly TestDbContextFactory _factory;

    public PRDetectionTests()
    {
        _factory = new TestDbContextFactory(Connection);
        _prService = new PRDetectionService(_factory);

        _benchPress = new StrengthExercise
        {
            Name = "Bench Press",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        };
        _squat = new StrengthExercise
        {
            Name = "Squat",
            MuscleGroup = MuscleGroup.Legs,
            Equipment = Equipment.Barbell
        };
        _run5k = new EnduranceExercise
        {
            Name = "5K Run",
            ActivityType = ActivityType.Run
        };

        Context.StrengthExercises.Add(_benchPress);
        Context.StrengthExercises.Add(_squat);
        Context.EnduranceExercises.Add(_run5k);
        Context.SaveChanges();
    }

    /// <summary>
    /// Creates a completed strength session with given sets.
    /// Returns the WorkoutLog.Id.
    /// </summary>
    private int CreateCompletedStrengthSession(int exerciseId, DateTime scheduledDate, List<(double weight, int reps)> sets)
    {
        var template = new WorkoutTemplate { Name = $"Strength {scheduledDate:yyyy-MM-dd}" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = scheduledDate,
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = scheduledDate.AddHours(8),
            CompletedAt = scheduledDate.AddHours(9)
        };

        int setNum = 1;
        foreach (var (weight, reps) in sets)
        {
            log.SetLogs.Add(new SetLog
            {
                ExerciseId = exerciseId,
                SetNumber = setNum++,
                SetType = SetType.Working,
                PlannedReps = reps,
                PlannedWeight = weight,
                ActualReps = reps,
                ActualWeight = weight,
                IsCompleted = true
            });
        }

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    /// <summary>
    /// Creates a completed endurance session.
    /// Returns the WorkoutLog.Id.
    /// </summary>
    private int CreateCompletedEnduranceSession(int exerciseId, DateTime scheduledDate, double distance, double pace, ActivityType activityType)
    {
        var template = new WorkoutTemplate { Name = $"Endurance {scheduledDate:yyyy-MM-dd}" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = scheduledDate,
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = scheduledDate.AddHours(8),
            CompletedAt = scheduledDate.AddHours(9)
        };

        log.EnduranceLogs.Add(new EnduranceLog
        {
            ExerciseId = exerciseId,
            ActivityType = activityType,
            PlannedDistance = distance,
            ActualDistance = distance,
            ActualPace = pace,
            ActualDurationSeconds = (int)(pace * distance * 60),
            IsCompleted = true
        });

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    // --- First session always creates PRs ---

    [Fact]
    public async Task FirstSession_AlwaysCreatesPRs()
    {
        var date = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var logId = CreateCompletedStrengthSession(_benchPress.Id, date, new() { (80, 8) });

        var prs = await _prService.DetectAndSavePRsAsync(logId);

        // First session should create 3 PRs: Weight=80, Reps=8, e1RM
        Assert.Equal(3, prs.Count);
        Assert.Contains(prs, pr => pr.StrengthType == StrengthPRType.Weight && pr.Value == 80);
        Assert.Contains(prs, pr => pr.StrengthType == StrengthPRType.Reps && pr.Value == 8);
        var expectedE1RM = AnalyticsService.EstimateE1RM(80, 8);
        Assert.Contains(prs, pr => pr.StrengthType == StrengthPRType.EstimatedOneRepMax && pr.Value == expectedE1RM);
    }

    // --- Weight PR detection ---

    [Fact]
    public async Task WeightPR_DetectedWhenHigher()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        var logId1 = CreateCompletedStrengthSession(_benchPress.Id, date1, new() { (80, 8) });
        await _prService.DetectAndSavePRsAsync(logId1); // seed first session PRs

        var logId2 = CreateCompletedStrengthSession(_benchPress.Id, date2, new() { (85, 8) });
        var prs = await _prService.DetectAndSavePRsAsync(logId2);

        Assert.Contains(prs, pr => pr.StrengthType == StrengthPRType.Weight && pr.Value == 85);
    }

    [Fact]
    public async Task WeightPR_NotDetectedWhenEqual()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        var logId1 = CreateCompletedStrengthSession(_benchPress.Id, date1, new() { (80, 8) });
        await _prService.DetectAndSavePRsAsync(logId1);

        var logId2 = CreateCompletedStrengthSession(_benchPress.Id, date2, new() { (80, 8) });
        var prs = await _prService.DetectAndSavePRsAsync(logId2);

        // No weight PR since 80 == previous best 80
        Assert.DoesNotContain(prs, pr => pr.StrengthType == StrengthPRType.Weight);
    }

    // --- Rep PR detection ---

    [Fact]
    public async Task RepPR_DetectedWhenHigher()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        var logId1 = CreateCompletedStrengthSession(_benchPress.Id, date1, new() { (80, 10) });
        await _prService.DetectAndSavePRsAsync(logId1);

        var logId2 = CreateCompletedStrengthSession(_benchPress.Id, date2, new() { (80, 12) });
        var prs = await _prService.DetectAndSavePRsAsync(logId2);

        Assert.Contains(prs, pr => pr.StrengthType == StrengthPRType.Reps && pr.Value == 12);
    }

    // --- e1RM PR detection ---

    [Fact]
    public async Task E1RM_PR_Detected()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        // Session 1: 80kg x 8 reps -> e1RM = 80*(1+8/30) = 101.3
        var logId1 = CreateCompletedStrengthSession(_benchPress.Id, date1, new() { (80, 8) });
        await _prService.DetectAndSavePRsAsync(logId1);

        // Session 2: 75kg x 12 reps -> e1RM = 75*(1+12/30) = 105.0
        var logId2 = CreateCompletedStrengthSession(_benchPress.Id, date2, new() { (75, 12) });
        var prs = await _prService.DetectAndSavePRsAsync(logId2);

        var expectedE1RM = AnalyticsService.EstimateE1RM(75, 12);
        Assert.Contains(prs, pr => pr.StrengthType == StrengthPRType.EstimatedOneRepMax && pr.Value == expectedE1RM);
    }

    // --- Endurance pace PR ---

    [Fact]
    public async Task EndurancePacePR_DetectedWhenFaster()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        var logId1 = CreateCompletedEnduranceSession(_run5k.Id, date1, 5.0, 5.5, ActivityType.Run);
        await _prService.DetectAndSavePRsAsync(logId1);

        var logId2 = CreateCompletedEnduranceSession(_run5k.Id, date2, 5.0, 5.0, ActivityType.Run);
        var prs = await _prService.DetectAndSavePRsAsync(logId2);

        Assert.Contains(prs, pr => pr.EnduranceType == EndurancePRType.Pace && pr.Value == 5.0);
    }

    // --- Endurance distance PR ---

    [Fact]
    public async Task EnduranceDistancePR_DetectedWhenFarther()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        var logId1 = CreateCompletedEnduranceSession(_run5k.Id, date1, 5.0, 5.5, ActivityType.Run);
        await _prService.DetectAndSavePRsAsync(logId1);

        var logId2 = CreateCompletedEnduranceSession(_run5k.Id, date2, 7.5, 5.5, ActivityType.Run);
        var prs = await _prService.DetectAndSavePRsAsync(logId2);

        Assert.Contains(prs, pr => pr.EnduranceType == EndurancePRType.Distance && pr.Value == 7.5);
    }

    // --- Persistence verification ---

    [Fact]
    public async Task PRs_PersistedToDatabase()
    {
        var date = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var logId = CreateCompletedStrengthSession(_benchPress.Id, date, new() { (80, 8) });

        await _prService.DetectAndSavePRsAsync(logId);

        // Verify via fresh context
        await using var verifyContext = _factory.CreateDbContext();
        var savedPRs = await verifyContext.PersonalRecords
            .Where(pr => pr.WorkoutLogId == logId)
            .ToListAsync();

        Assert.Equal(3, savedPRs.Count);
        Assert.Contains(savedPRs, pr => pr.ExerciseId == _benchPress.Id && pr.StrengthType == StrengthPRType.Weight);
        Assert.Contains(savedPRs, pr => pr.ExerciseId == _benchPress.Id && pr.StrengthType == StrengthPRType.Reps);
        Assert.Contains(savedPRs, pr => pr.ExerciseId == _benchPress.Id && pr.StrengthType == StrengthPRType.EstimatedOneRepMax);
        // Verify AchievedAt is set
        Assert.All(savedPRs, pr => Assert.True(pr.AchievedAt > DateTime.MinValue));
    }

    // --- Independent PRs per exercise ---

    [Fact]
    public async Task DifferentExercises_IndependentPRs()
    {
        var date = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        // Create a session with both exercises in the same workout log
        var template = new WorkoutTemplate { Name = "Multi Exercise" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = date,
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = date.AddHours(8),
            CompletedAt = date.AddHours(9)
        };
        log.SetLogs.Add(new SetLog
        {
            ExerciseId = _benchPress.Id,
            SetNumber = 1,
            SetType = SetType.Working,
            PlannedReps = 8, PlannedWeight = 80,
            ActualReps = 8, ActualWeight = 80,
            IsCompleted = true
        });
        log.SetLogs.Add(new SetLog
        {
            ExerciseId = _squat.Id,
            SetNumber = 1,
            SetType = SetType.Working,
            PlannedReps = 5, PlannedWeight = 60,
            ActualReps = 5, ActualWeight = 60,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();

        var prs = await _prService.DetectAndSavePRsAsync(log.Id);

        // Both exercises should have PRs independently (3 each = 6 total)
        Assert.Equal(6, prs.Count);
        Assert.Equal(3, prs.Count(pr => pr.ExerciseId == _benchPress.Id));
        Assert.Equal(3, prs.Count(pr => pr.ExerciseId == _squat.Id));
    }

    // --- GetAllPRsAsync grouped by exercise ---

    [Fact]
    public async Task GetAllPRs_GroupedByExercise()
    {
        var date1 = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 9, 0, 0, 0, DateTimeKind.Utc);

        // Create PRs for bench press
        var logId1 = CreateCompletedStrengthSession(_benchPress.Id, date1, new() { (80, 8) });
        await _prService.DetectAndSavePRsAsync(logId1);

        // Create PRs for squat
        var logId2 = CreateCompletedStrengthSession(_squat.Id, date2, new() { (100, 5) });
        await _prService.DetectAndSavePRsAsync(logId2);

        var grouped = await _prService.GetAllPRsAsync();

        Assert.Equal(2, grouped.Count);
        Assert.Contains(grouped, g => g.Key == "Bench Press");
        Assert.Contains(grouped, g => g.Key == "Squat");
        // Each exercise should have 3 PRs (Weight, Reps, e1RM)
        Assert.All(grouped, g => Assert.Equal(3, g.Count()));
    }
}
