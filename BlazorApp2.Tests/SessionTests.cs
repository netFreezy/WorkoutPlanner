using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class SessionTests : DataTestBase
{
    private (WorkoutTemplate template, ScheduledWorkout scheduled, StrengthExercise strengthExercise, EnduranceExercise enduranceExercise) CreateSessionTestSetup()
    {
        var strengthExercise = new StrengthExercise
        {
            Name = "Bench Press",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        };
        var enduranceExercise = new EnduranceExercise
        {
            Name = "5K Run",
            ActivityType = ActivityType.Run
        };

        Context.StrengthExercises.Add(strengthExercise);
        Context.EnduranceExercises.Add(enduranceExercise);
        Context.SaveChanges();

        var template = new WorkoutTemplate { Name = "Test Session Workout" };
        template.Items.Add(new TemplateItem
        {
            ExerciseId = strengthExercise.Id,
            Position = 1,
            TargetSets = 3,
            TargetReps = 10,
            TargetWeight = 60.0
        });
        template.Items.Add(new TemplateItem
        {
            ExerciseId = enduranceExercise.Id,
            Position = 2,
            TargetDistance = 5.0,
            TargetDurationSeconds = 1500,
            TargetPace = 5.0,
            TargetHeartRateZone = 3
        });
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            Status = WorkoutStatus.Planned,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        return (template, scheduled, strengthExercise, enduranceExercise);
    }

    [Fact]
    public async Task StartSessionAsync_CreatesWorkoutLogWithSnapshotSetLogs()
    {
        var (template, scheduled, strengthEx, enduranceEx) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);

        Assert.NotNull(workoutLog);
        Assert.True(workoutLog.StartedAt > DateTime.MinValue);
        Assert.Null(workoutLog.CompletedAt);

        // 3 sets for strength exercise
        Assert.Equal(3, workoutLog.SetLogs.Count);
        foreach (var setLog in workoutLog.SetLogs)
        {
            Assert.Equal(10, setLog.PlannedReps);
            Assert.Equal(60.0, setLog.PlannedWeight);
            Assert.Equal(10, setLog.ActualReps);
            Assert.Equal(60.0, setLog.ActualWeight);
            Assert.False(setLog.IsCompleted);
        }

        // 1 endurance log
        Assert.Single(workoutLog.EnduranceLogs);
        var enduranceLog = workoutLog.EnduranceLogs.First();
        Assert.Equal(5.0, enduranceLog.PlannedDistance);
        Assert.Equal(1500, enduranceLog.PlannedDurationSeconds);
        Assert.False(enduranceLog.IsCompleted);
    }

    [Fact]
    public async Task StartSessionAsync_ResumeExistingSession_ReturnsSameWorkoutLog()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var first = await service.StartSessionAsync(scheduled.Id);
        var second = await service.StartSessionAsync(scheduled.Id);

        Assert.Equal(first.Id, second.Id);
    }

    [Fact]
    public async Task CompleteSetAsync_PersistsActualValuesAndMarksCompleted()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        var setLogId = workoutLog.SetLogs.First().Id;

        await service.CompleteSetAsync(setLogId, 65.0, 8, SetType.Working);

        // Reload from DB via fresh context
        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.SetLogs.FindAsync(setLogId);
        Assert.NotNull(loaded);
        Assert.Equal(65.0, loaded!.ActualWeight);
        Assert.Equal(8, loaded.ActualReps);
        Assert.True(loaded.IsCompleted);
    }

    [Fact]
    public async Task UncompleteSetAsync_SetsIsCompletedFalse()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        var setLogId = workoutLog.SetLogs.First().Id;

        await service.CompleteSetAsync(setLogId, 60.0, 10, SetType.Working);
        await service.UncompleteSetAsync(setLogId);

        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.SetLogs.FindAsync(setLogId);
        Assert.False(loaded!.IsCompleted);
    }

    [Fact]
    public async Task CompleteSetAsync_WithDifferentSetType_PersistsType()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        var setLogId = workoutLog.SetLogs.First().Id;

        await service.CompleteSetAsync(setLogId, 60.0, 6, SetType.Failure);

        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.SetLogs.FindAsync(setLogId);
        Assert.Equal(SetType.Failure, loaded!.SetType);
    }

    [Fact]
    public async Task SaveEnduranceLogAsync_PersistsValuesAndCalculatesPace()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        var enduranceLogId = workoutLog.EnduranceLogs.First().Id;

        await service.SaveEnduranceLogAsync(enduranceLogId, 5.2, 1560, 4);

        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.EnduranceLogs.FindAsync(enduranceLogId);
        Assert.NotNull(loaded);
        Assert.Equal(5.2, loaded!.ActualDistance);
        Assert.Equal(1560, loaded.ActualDurationSeconds);
        Assert.Equal(4, loaded.ActualHeartRateZone);
        Assert.True(loaded.IsCompleted);

        // Pace = (1560/60.0) / 5.2 = 26.0 / 5.2 = 5.0
        Assert.NotNull(loaded.ActualPace);
        Assert.Equal(5.0, loaded.ActualPace!.Value, 2);
    }

    [Fact]
    public async Task AddSetAsync_CreatesSetWithIncrementedNumber()
    {
        var (_, scheduled, strengthEx, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);

        var newSet = await service.AddSetAsync(workoutLog.Id, strengthEx.Id, 60.0);

        Assert.Equal(4, newSet.SetNumber);
        Assert.Equal(60.0, newSet.ActualWeight);
        Assert.False(newSet.IsCompleted);

        // Verify total count
        await using var verifyContext = factory.CreateDbContext();
        var totalSets = await verifyContext.SetLogs
            .Where(sl => sl.WorkoutLogId == workoutLog.Id && sl.ExerciseId == strengthEx.Id)
            .CountAsync();
        Assert.Equal(4, totalSets);
    }

    [Fact]
    public async Task UpdateSetTypeAsync_PersistsNewType()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        var setLogId = workoutLog.SetLogs.First().Id;

        await service.UpdateSetTypeAsync(setLogId, SetType.WarmUp);

        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.SetLogs.FindAsync(setLogId);
        Assert.Equal(SetType.WarmUp, loaded!.SetType);
    }

    // --- Task 2: previous performance, finish, abandon, resume, today's workouts ---

    /// <summary>
    /// Helper to create a completed session manually via Context for previous performance tests.
    /// </summary>
    private WorkoutLog CreateCompletedSession(StrengthExercise strengthEx, EnduranceExercise enduranceEx, DateTime date, double weight, int reps, double distance, int durationSec)
    {
        var template = new WorkoutTemplate { Name = $"Completed Workout {date:yyyy-MM-dd}" };
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
            ExerciseId = strengthEx.Id,
            SetNumber = 1,
            SetType = SetType.Working,
            PlannedReps = reps,
            PlannedWeight = weight,
            ActualReps = reps,
            ActualWeight = weight,
            IsCompleted = true
        });
        log.SetLogs.Add(new SetLog
        {
            ExerciseId = strengthEx.Id,
            SetNumber = 2,
            SetType = SetType.Working,
            PlannedReps = reps,
            PlannedWeight = weight,
            ActualReps = reps,
            ActualWeight = weight,
            IsCompleted = true
        });
        log.EnduranceLogs.Add(new EnduranceLog
        {
            ExerciseId = enduranceEx.Id,
            ActivityType = ActivityType.Run,
            PlannedDistance = distance,
            PlannedDurationSeconds = durationSec,
            ActualDistance = distance,
            ActualDurationSeconds = durationSec,
            ActualPace = distance > 0 ? (durationSec / 60.0) / distance : null,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log;
    }

    [Fact]
    public async Task GetPreviousStrengthPerformanceAsync_ReturnsLast3Sessions()
    {
        var (_, _, strengthEx, enduranceEx) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        // Create 4 completed sessions with different dates and weights
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), 50.0, 10, 5.0, 1500);
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), 55.0, 10, 5.0, 1450);
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc), 60.0, 8, 5.0, 1400);
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc), 62.5, 8, 5.0, 1380);

        var results = await service.GetPreviousStrengthPerformanceAsync(strengthEx.Id, limit: 3);

        Assert.Equal(3, results.Count);
        // Most recent first
        Assert.Equal(62.5, results[0].Sets[0].Weight);
        Assert.Equal(60.0, results[1].Sets[0].Weight);
        Assert.Equal(55.0, results[2].Sets[0].Weight);
        // Each session should have 2 sets
        Assert.Equal(2, results[0].Sets.Count);
    }

    [Fact]
    public async Task GetPreviousStrengthPerformanceAsync_ExcludesCurrentWorkoutLog()
    {
        var (_, scheduled, strengthEx, enduranceEx) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        // Create 2 completed sessions
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), 50.0, 10, 5.0, 1500);
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), 55.0, 10, 5.0, 1450);

        // Start a 3rd session (incomplete -- will not have CompletedAt)
        var currentSession = await service.StartSessionAsync(scheduled.Id);

        var results = await service.GetPreviousStrengthPerformanceAsync(strengthEx.Id, excludeWorkoutLogId: currentSession.Id, limit: 3);

        // Only 2 completed sessions should be returned (current excluded by CompletedAt null AND excludeWorkoutLogId)
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task GetPreviousEndurancePerformanceAsync_ReturnsLast3Sessions()
    {
        var (_, _, strengthEx, enduranceEx) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        // Create 2 completed endurance sessions
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc), 50.0, 10, 5.0, 1500);
        CreateCompletedSession(strengthEx, enduranceEx, new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc), 55.0, 10, 5.2, 1560);

        var results = await service.GetPreviousEndurancePerformanceAsync(enduranceEx.Id, limit: 3);

        Assert.Equal(2, results.Count);
        // Most recent first
        Assert.Equal(5.2, results[0].Distance);
        Assert.Equal(1560, results[0].DurationSeconds);
        Assert.Equal(5.0, results[1].Distance);
        Assert.Equal(1500, results[1].DurationSeconds);
    }

    [Fact]
    public async Task FinishSessionAsync_SetsCompletedAtRpeNotes()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        await service.FinishSessionAsync(workoutLog.Id, 7, "Good session");

        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.WorkoutLogs
            .Include(wl => wl.ScheduledWorkout)
            .FirstAsync(wl => wl.Id == workoutLog.Id);
        Assert.NotNull(loaded.CompletedAt);
        Assert.Equal(7, loaded.Rpe);
        Assert.Equal("Good session", loaded.Notes);
        Assert.Equal(WorkoutStatus.Completed, loaded.ScheduledWorkout.Status);
    }

    [Fact]
    public async Task AbandonSessionAsync_SetsCompletedAtKeepsLoggedData()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        var firstSetId = workoutLog.SetLogs.First().Id;

        // Complete 1 set, then abandon
        await service.CompleteSetAsync(firstSetId, 65.0, 8, SetType.Working);
        await service.AbandonSessionAsync(workoutLog.Id);

        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.WorkoutLogs
            .Include(wl => wl.SetLogs)
            .Include(wl => wl.ScheduledWorkout)
            .FirstAsync(wl => wl.Id == workoutLog.Id);

        Assert.NotNull(loaded.CompletedAt);
        // The completed set should still be present
        var completedSet = loaded.SetLogs.First(sl => sl.Id == firstSetId);
        Assert.True(completedSet.IsCompleted);
        Assert.Equal(65.0, completedSet.ActualWeight);
        // Status per D-16
        Assert.Equal(WorkoutStatus.Completed, loaded.ScheduledWorkout.Status);
    }

    [Fact]
    public async Task GetIncompleteSessionIdAsync_ReturnsScheduledWorkoutId_WhenIncompleteExists()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        await service.StartSessionAsync(scheduled.Id);

        var result = await service.GetIncompleteSessionIdAsync();

        Assert.NotNull(result);
        Assert.Equal(scheduled.Id, result!.Value);
    }

    [Fact]
    public async Task GetIncompleteSessionIdAsync_ReturnsNull_WhenNoIncomplete()
    {
        var (_, scheduled, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var workoutLog = await service.StartSessionAsync(scheduled.Id);
        await service.FinishSessionAsync(workoutLog.Id, 6, null);

        var result = await service.GetIncompleteSessionIdAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task GetTodaysWorkoutsAsync_ReturnsOnlyTodaysPlannedWorkouts()
    {
        var (template, _, _, _) = CreateSessionTestSetup();
        var factory = new TestDbContextFactory(Connection);
        var service = new SessionService(factory);

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);

        // Workout for today (Planned)
        var todayPlanned = new ScheduledWorkout
        {
            ScheduledDate = today,
            Status = WorkoutStatus.Planned,
            WorkoutTemplateId = template.Id
        };
        // Workout for yesterday (Planned)
        var yesterdayPlanned = new ScheduledWorkout
        {
            ScheduledDate = yesterday,
            Status = WorkoutStatus.Planned,
            WorkoutTemplateId = template.Id
        };
        // Workout for today (Completed)
        var todayCompleted = new ScheduledWorkout
        {
            ScheduledDate = today,
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };

        Context.ScheduledWorkouts.AddRange(todayPlanned, yesterdayPlanned, todayCompleted);
        await Context.SaveChangesAsync();

        var results = await service.GetTodaysWorkoutsAsync();

        Assert.Single(results);
        Assert.Equal(todayPlanned.Id, results[0].Id);
    }
}
