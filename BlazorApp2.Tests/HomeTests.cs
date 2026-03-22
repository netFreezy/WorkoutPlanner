using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class HomeTests : DataTestBase
{
    private readonly HistoryService _historyService;
    private readonly TestDbContextFactory _factory;
    private readonly StrengthExercise _benchPress;
    private readonly EnduranceExercise _run5k;

    public HomeTests()
    {
        _factory = new TestDbContextFactory(Connection);
        _historyService = new HistoryService(_factory);

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
    }

    /// <summary>
    /// Creates a ScheduledWorkout at the given date with the given status.
    /// Returns the ScheduledWorkout.
    /// </summary>
    private ScheduledWorkout CreateScheduledWorkout(DateTime date, WorkoutStatus status, string templateName)
    {
        var template = new WorkoutTemplate { Name = templateName };
        template.Items.Add(new TemplateItem
        {
            ExerciseId = _benchPress.Id,
            Position = 1,
            TargetSets = 3,
            TargetReps = 8,
            TargetWeight = 60.0
        });
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = date,
            Status = status,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        return scheduled;
    }

    /// <summary>
    /// Creates a completed session with set logs.
    /// Returns the WorkoutLog.Id.
    /// </summary>
    private int CreateCompletedSession(DateTime completedAt, int? templateId, string workoutName)
    {
        WorkoutTemplate? template = null;
        if (templateId.HasValue)
        {
            template = Context.WorkoutTemplates.Find(templateId.Value);
        }
        else
        {
            template = new WorkoutTemplate { Name = workoutName };
            Context.WorkoutTemplates.Add(template);
            Context.SaveChanges();
        }

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = completedAt,
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template!.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = completedAt.AddHours(8),
            CompletedAt = completedAt.AddHours(9)
        };
        log.SetLogs.Add(new SetLog
        {
            ExerciseId = _benchPress.Id,
            SetNumber = 1,
            SetType = SetType.Working,
            PlannedReps = 8,
            PlannedWeight = 60.0,
            ActualReps = 8,
            ActualWeight = 60.0,
            IsCompleted = true
        });
        log.SetLogs.Add(new SetLog
        {
            ExerciseId = _benchPress.Id,
            SetNumber = 2,
            SetType = SetType.Working,
            PlannedReps = 8,
            PlannedWeight = 60.0,
            ActualReps = 8,
            ActualWeight = 65.0,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    // --- GetTodaysScheduledWorkoutAsync tests ---

    [Fact]
    public async Task TodaysWorkout_ReturnsPlannedWorkout()
    {
        var today = DateTime.UtcNow.Date;
        var sw = CreateScheduledWorkout(today, WorkoutStatus.Planned, "Push Day");

        var result = await _historyService.GetTodaysScheduledWorkoutAsync();

        Assert.NotNull(result);
        Assert.Equal(sw.Id, result!.Id);
        Assert.Equal("Push Day", result.WorkoutTemplate!.Name);
    }

    [Fact]
    public async Task TodaysWorkout_ReturnsNull_WhenNothingScheduled()
    {
        // No scheduled workouts at all
        var result = await _historyService.GetTodaysScheduledWorkoutAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task TodaysWorkout_ReturnsNull_WhenStatusNotPlanned()
    {
        var today = DateTime.UtcNow.Date;
        CreateScheduledWorkout(today, WorkoutStatus.Completed, "Completed Workout");

        var result = await _historyService.GetTodaysScheduledWorkoutAsync();

        Assert.Null(result);
    }

    // --- GetLastCompletedWorkoutAsync tests ---

    [Fact]
    public async Task LastCompleted_ReturnsMostRecent()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        CreateCompletedSession(date1, null, "Session 1");
        CreateCompletedSession(date2, null, "Session 2");

        var result = await _historyService.GetLastCompletedWorkoutAsync();

        Assert.NotNull(result);
        // Most recent session should be March 15
        Assert.Equal(new DateTime(2026, 3, 15, 9, 0, 0, DateTimeKind.Utc), result!.CompletedAt);
        // TotalVolume: 2 sets: (60*8) + (65*8) = 480 + 520 = 1000
        Assert.Equal(1000.0, result.TotalVolume);
    }

    [Fact]
    public async Task LastCompleted_ReturnsNull_WhenNoCompletedSessions()
    {
        var result = await _historyService.GetLastCompletedWorkoutAsync();

        Assert.Null(result);
    }

    [Fact]
    public async Task LastCompleted_IncludesTemplateId()
    {
        // Create a template first, then a completed session from it
        var template = new WorkoutTemplate { Name = "Repeat Template" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var date = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);
        CreateCompletedSession(date, template.Id, "From Template");

        var result = await _historyService.GetLastCompletedWorkoutAsync();

        Assert.NotNull(result);
        Assert.NotNull(result!.TemplateId);
        Assert.Equal(template.Id, result.TemplateId);
    }

    // --- GetTomorrowsScheduledWorkoutAsync tests ---

    [Fact]
    public async Task TomorrowsWorkout_ReturnsPlannedWorkout()
    {
        var tomorrow = DateTime.UtcNow.Date.AddDays(1);
        var sw = CreateScheduledWorkout(tomorrow, WorkoutStatus.Planned, "Leg Day Tomorrow");

        var result = await _historyService.GetTomorrowsScheduledWorkoutAsync();

        Assert.NotNull(result);
        Assert.Equal(sw.Id, result!.Id);
        Assert.Equal("Leg Day Tomorrow", result.WorkoutTemplate!.Name);
    }

    [Fact]
    public async Task TomorrowsWorkout_ReturnsNull_WhenNothingScheduled()
    {
        var result = await _historyService.GetTomorrowsScheduledWorkoutAsync();

        Assert.Null(result);
    }
}
