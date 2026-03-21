using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class LogTests : DataTestBase
{
    private (WorkoutTemplate template, ScheduledWorkout scheduled, StrengthExercise strengthExercise, EnduranceExercise enduranceExercise) CreateTestSetup()
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
        var template = new WorkoutTemplate { Name = "Test Workout" };
        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            Status = WorkoutStatus.Planned,
            WorkoutTemplate = template
        };

        Context.StrengthExercises.Add(strengthExercise);
        Context.EnduranceExercises.Add(enduranceExercise);
        Context.ScheduledWorkouts.Add(scheduled);

        return (template, scheduled, strengthExercise, enduranceExercise);
    }

    [Fact]
    public async Task WorkoutLog_WithRpeAndNotes_PersistsCorrectly()
    {
        var (_, scheduled, _, _) = CreateTestSetup();
        var startedAt = new DateTime(2026, 3, 25, 8, 0, 0, DateTimeKind.Utc);
        var completedAt = new DateTime(2026, 3, 25, 9, 0, 0, DateTimeKind.Utc);

        var log = new WorkoutLog
        {
            ScheduledWorkout = scheduled,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            Rpe = 7,
            Notes = "Great session"
        };
        Context.WorkoutLogs.Add(log);
        await Context.SaveChangesAsync();

        var loaded = await Context.WorkoutLogs
            .Include(l => l.SetLogs)
            .Include(l => l.EnduranceLogs)
            .FirstAsync();

        Assert.Equal(startedAt, loaded.StartedAt);
        Assert.Equal(completedAt, loaded.CompletedAt);
        Assert.Equal(7, loaded.Rpe);
        Assert.Equal("Great session", loaded.Notes);
    }

    [Fact]
    public async Task SetLog_WithPlannedAndActualValues_PersistsCorrectly()
    {
        var (_, scheduled, strengthExercise, _) = CreateTestSetup();

        var log = new WorkoutLog
        {
            ScheduledWorkout = scheduled,
            StartedAt = DateTime.UtcNow
        };
        log.SetLogs.Add(new SetLog
        {
            Exercise = strengthExercise,
            SetNumber = 1,
            SetType = SetType.Working,
            PlannedReps = 10,
            PlannedWeight = 80.0,
            ActualReps = 8,
            ActualWeight = 80.0,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(log);
        await Context.SaveChangesAsync();

        var loadedSet = await Context.SetLogs.FirstAsync();

        Assert.Equal(1, loadedSet.SetNumber);
        Assert.Equal(SetType.Working, loadedSet.SetType);
        Assert.Equal(10, loadedSet.PlannedReps);
        Assert.Equal(80.0, loadedSet.PlannedWeight);
        Assert.Equal(8, loadedSet.ActualReps);
        Assert.Equal(80.0, loadedSet.ActualWeight);
        Assert.True(loadedSet.IsCompleted);
    }

    [Fact]
    public async Task SetLog_DifferentSetTypes_WarmUpAndDrop_PersistCorrectly()
    {
        var (_, scheduled, strengthExercise, _) = CreateTestSetup();

        var log = new WorkoutLog
        {
            ScheduledWorkout = scheduled,
            StartedAt = DateTime.UtcNow
        };
        log.SetLogs.Add(new SetLog
        {
            Exercise = strengthExercise,
            SetNumber = 1,
            SetType = SetType.WarmUp,
            PlannedReps = 10,
            PlannedWeight = 40.0,
            IsCompleted = true
        });
        log.SetLogs.Add(new SetLog
        {
            Exercise = strengthExercise,
            SetNumber = 2,
            SetType = SetType.Drop,
            PlannedReps = 12,
            PlannedWeight = 30.0,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(log);
        await Context.SaveChangesAsync();

        var sets = await Context.SetLogs.OrderBy(s => s.SetNumber).ToListAsync();

        Assert.Equal(2, sets.Count);
        Assert.Equal(SetType.WarmUp, sets[0].SetType);
        Assert.Equal(SetType.Drop, sets[1].SetType);
    }

    [Fact]
    public async Task EnduranceLog_WithPlannedAndActualValues_PersistsCorrectly()
    {
        var (_, scheduled, _, enduranceExercise) = CreateTestSetup();

        var log = new WorkoutLog
        {
            ScheduledWorkout = scheduled,
            StartedAt = DateTime.UtcNow
        };
        log.EnduranceLogs.Add(new EnduranceLog
        {
            Exercise = enduranceExercise,
            ActivityType = ActivityType.Run,
            PlannedDistance = 5.0,
            PlannedDurationSeconds = 1500,
            PlannedPace = 5.0,
            PlannedHeartRateZone = 3,
            ActualDistance = 5.2,
            ActualDurationSeconds = 1470,
            ActualPace = 4.71,
            ActualHeartRateZone = 4,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(log);
        await Context.SaveChangesAsync();

        var loadedEndurance = await Context.EnduranceLogs.FirstAsync();

        Assert.Equal(ActivityType.Run, loadedEndurance.ActivityType);
        Assert.Equal(5.0, loadedEndurance.PlannedDistance);
        Assert.Equal(1500, loadedEndurance.PlannedDurationSeconds);
        Assert.Equal(5.0, loadedEndurance.PlannedPace);
        Assert.Equal(3, loadedEndurance.PlannedHeartRateZone);
        Assert.Equal(5.2, loadedEndurance.ActualDistance);
        Assert.Equal(1470, loadedEndurance.ActualDurationSeconds);
        Assert.Equal(4.71, loadedEndurance.ActualPace);
        Assert.Equal(4, loadedEndurance.ActualHeartRateZone);
        Assert.True(loadedEndurance.IsCompleted);
    }
}
