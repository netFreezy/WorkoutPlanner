using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class HistoryTests : DataTestBase
{
    private readonly HistoryService _historyService;
    private readonly TestDbContextFactory _factory;
    private readonly StrengthExercise _benchPress;
    private readonly StrengthExercise _squat;
    private readonly EnduranceExercise _run5k;

    public HistoryTests()
    {
        _factory = new TestDbContextFactory(Connection);
        _historyService = new HistoryService(_factory);

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

        // Create 3 completed sessions with different dates
        CreateCompletedSession(
            new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc),
            _benchPress.Id, 3, 60.0, 8, "Session A");
        CreateCompletedSession(
            new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            _squat.Id, 3, 100.0, 5, "Session B");
        CreateCompletedSession(
            new DateTime(2026, 3, 20, 0, 0, 0, DateTimeKind.Utc),
            _benchPress.Id, 3, 65.0, 8, "Session C");
    }

    /// <summary>
    /// Creates a completed session with strength set logs and an endurance log.
    /// </summary>
    private int CreateCompletedSession(DateTime completedAt, int exerciseId, int sets, double weight, int reps, string name)
    {
        var template = new WorkoutTemplate { Name = name };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = completedAt,
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = completedAt.AddHours(8),
            CompletedAt = completedAt.AddHours(9)
        };

        for (int i = 1; i <= sets; i++)
        {
            log.SetLogs.Add(new SetLog
            {
                ExerciseId = exerciseId,
                SetNumber = i,
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

    // --- GetCompletedSessionsAsync tests ---

    [Fact]
    public async Task GetCompletedSessions_ReturnsOrderedByCompletedAtDescending()
    {
        var results = await _historyService.GetCompletedSessionsAsync(null, null, null, 0, 10);

        Assert.Equal(3, results.Count);
        // Most recent first
        Assert.True(results[0].CompletedAt > results[1].CompletedAt);
        Assert.True(results[1].CompletedAt > results[2].CompletedAt);
        Assert.Equal("Session C", results[0].WorkoutName);
        Assert.Equal("Session B", results[1].WorkoutName);
        Assert.Equal("Session A", results[2].WorkoutName);
    }

    [Fact]
    public async Task GetCompletedSessions_RespectsSkipTakePagination()
    {
        // Skip 1, take 1 -- should return the second session (Session B)
        var results = await _historyService.GetCompletedSessionsAsync(null, null, null, 1, 1);

        Assert.Single(results);
        Assert.Equal("Session B", results[0].WorkoutName);
    }

    [Fact]
    public async Task GetCompletedSessions_FiltersByDateRange()
    {
        var dateStart = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc);
        var dateEnd = new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc);

        var results = await _historyService.GetCompletedSessionsAsync(dateStart, dateEnd, null, 0, 10);

        // Only Session B (March 15) falls within [March 12, March 18)
        Assert.Single(results);
        Assert.Equal("Session B", results[0].WorkoutName);
    }

    [Fact]
    public async Task GetCompletedSessions_FiltersByExerciseId()
    {
        // Filter by bench press -- Sessions A and C contain bench press
        var results = await _historyService.GetCompletedSessionsAsync(null, null, _benchPress.Id, 0, 10);

        Assert.Equal(2, results.Count);
        Assert.Equal("Session C", results[0].WorkoutName);
        Assert.Equal("Session A", results[1].WorkoutName);
    }

    [Fact]
    public async Task GetTotalCount_ReturnsCorrectCountWithFilters()
    {
        // Total count with no filters
        var totalAll = await _historyService.GetTotalCountAsync(null, null, null);
        Assert.Equal(3, totalAll);

        // Count filtered by exercise
        var totalBench = await _historyService.GetTotalCountAsync(null, null, _benchPress.Id);
        Assert.Equal(2, totalBench);

        // Count filtered by date range
        var dateStart = new DateTime(2026, 3, 12, 0, 0, 0, DateTimeKind.Utc);
        var dateEnd = new DateTime(2026, 3, 18, 0, 0, 0, DateTimeKind.Utc);
        var totalRange = await _historyService.GetTotalCountAsync(dateStart, dateEnd, null);
        Assert.Equal(1, totalRange);
    }

    [Fact]
    public async Task GetLoggedExercises_ReturnsDistinctExercises()
    {
        var exercises = await _historyService.GetLoggedExercisesAsync();

        // Should return bench press and squat (both used in completed sessions)
        Assert.Equal(2, exercises.Count);
        Assert.Contains(exercises, e => e.Name == "Bench Press");
        Assert.Contains(exercises, e => e.Name == "Squat");
    }
}
