using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class OverloadTests : DataTestBase
{
    private readonly OverloadService _overloadService;
    private readonly TestDbContextFactory _factory;
    private readonly StrengthExercise _benchPress;
    private readonly StrengthExercise _squat;
    private readonly EnduranceExercise _run5k;

    public OverloadTests()
    {
        _factory = new TestDbContextFactory(Connection);
        _overloadService = new OverloadService(_factory);

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
    /// Creates a completed session with the given sets for a strength exercise.
    /// Returns the WorkoutLog.Id.
    /// </summary>
    private int CreateCompletedSession(int exerciseId, int sets, double weight, int reps, DateTime completedAt,
        SetType setType = SetType.Working, int? actualReps = null)
    {
        var template = new WorkoutTemplate { Name = $"Session {completedAt:yyyy-MM-dd}" };
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
                SetType = setType,
                PlannedReps = reps,
                PlannedWeight = weight,
                ActualReps = actualReps ?? reps,
                ActualWeight = weight,
                IsCompleted = true
            });
        }

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    /// <summary>
    /// Creates an active (incomplete) session for the given exercise.
    /// Returns the WorkoutLog.Id.
    /// </summary>
    private int CreateActiveSession(int exerciseId, int sets, double weight, int reps)
    {
        var template = new WorkoutTemplate { Name = "Active Session" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = DateTime.UtcNow.Date,
            Status = WorkoutStatus.Planned,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();

        var log = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = DateTime.UtcNow,
            CompletedAt = null // active session -- not completed
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
                IsCompleted = false
            });
        }

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    // --- GetWeightIncrement tests ---

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForChest()
    {
        Assert.Equal(2.5, OverloadService.GetWeightIncrement(MuscleGroup.Chest));
    }

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForBack()
    {
        Assert.Equal(2.5, OverloadService.GetWeightIncrement(MuscleGroup.Back));
    }

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForShoulders()
    {
        Assert.Equal(2.5, OverloadService.GetWeightIncrement(MuscleGroup.Shoulders));
    }

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForLegs()
    {
        Assert.Equal(5.0, OverloadService.GetWeightIncrement(MuscleGroup.Legs));
    }

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForFullBody()
    {
        Assert.Equal(5.0, OverloadService.GetWeightIncrement(MuscleGroup.FullBody));
    }

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForArms()
    {
        Assert.Equal(1.0, OverloadService.GetWeightIncrement(MuscleGroup.Arms));
    }

    [Fact]
    public void GetWeightIncrement_ReturnsCorrectForCore()
    {
        Assert.Equal(1.0, OverloadService.GetWeightIncrement(MuscleGroup.Core));
    }

    // --- GetSuggestionsAsync tests ---

    [Fact]
    public async Task GetSuggestions_ReturnsSuggestion_When2ConsecutiveQualifyingSessions()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        // Two completed sessions with 3 sets of 8 reps at 60kg -- all sets hit targets
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date1);
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date2);

        // Active session for the same exercise
        var activeLogId = CreateActiveSession(_benchPress.Id, 3, 60.0, 8);

        var suggestions = await _overloadService.GetSuggestionsAsync(activeLogId);

        Assert.Single(suggestions);
        var suggestion = suggestions[0];
        Assert.Equal(_benchPress.Id, suggestion.ExerciseId);
        Assert.Equal("Bench Press", suggestion.ExerciseName);
        Assert.Equal(60.0, suggestion.CurrentWeight);
        Assert.Equal(62.5, suggestion.SuggestedWeight);
        Assert.Equal(2.5, suggestion.Increment);
        Assert.Equal(3, suggestion.PlannedSets);
        Assert.Equal(8, suggestion.PlannedReps);
    }

    [Fact]
    public async Task GetSuggestions_ReturnsEmpty_WhenOnly1QualifyingSession()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);

        // Only one completed session
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date1);

        var activeLogId = CreateActiveSession(_benchPress.Id, 3, 60.0, 8);

        var suggestions = await _overloadService.GetSuggestionsAsync(activeLogId);

        Assert.Empty(suggestions);
    }

    [Fact]
    public async Task GetSuggestions_ReturnsEmpty_WhenSessionMissedReps()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        // First session hits all targets
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date1);

        // Second session: planned 8 reps but only achieved 6
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date2, actualReps: 6);

        var activeLogId = CreateActiveSession(_benchPress.Id, 3, 60.0, 8);

        var suggestions = await _overloadService.GetSuggestionsAsync(activeLogId);

        Assert.Empty(suggestions);
    }

    [Fact]
    public async Task GetSuggestions_ExcludesWarmUpSets()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        // Create completed sessions with working sets that hit targets
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date1);
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date2);

        // Also add warmup sets to the second session (these should not affect qualification)
        var template = new WorkoutTemplate { Name = "Extra WarmUp" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();
        var scheduled = new ScheduledWorkout
        {
            ScheduledDate = date2.AddHours(1),
            Status = WorkoutStatus.Completed,
            WorkoutTemplateId = template.Id
        };
        Context.ScheduledWorkouts.Add(scheduled);
        Context.SaveChanges();
        var warmupLog = new WorkoutLog
        {
            ScheduledWorkoutId = scheduled.Id,
            StartedAt = date2.AddHours(7),
            CompletedAt = date2.AddHours(8)
        };
        warmupLog.SetLogs.Add(new SetLog
        {
            ExerciseId = _benchPress.Id,
            SetNumber = 1,
            SetType = SetType.WarmUp,
            PlannedReps = 8,
            PlannedWeight = 30.0,
            ActualReps = 8,
            ActualWeight = 30.0,
            IsCompleted = true
        });
        Context.WorkoutLogs.Add(warmupLog);
        Context.SaveChanges();

        var activeLogId = CreateActiveSession(_benchPress.Id, 3, 60.0, 8);

        // Should still suggest overload -- warm-up sets are excluded from the query (Working only)
        var suggestions = await _overloadService.GetSuggestionsAsync(activeLogId);

        Assert.Single(suggestions);
        Assert.Equal(62.5, suggestions[0].SuggestedWeight);
    }

    [Fact]
    public async Task GetSuggestions_ReturnsNoSuggestions_ForEnduranceExercises()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        // Create completed endurance sessions (using EnduranceLogs, not SetLogs)
        var template1 = new WorkoutTemplate { Name = "Run 1" };
        Context.WorkoutTemplates.Add(template1);
        Context.SaveChanges();
        var sw1 = new ScheduledWorkout { ScheduledDate = date1, Status = WorkoutStatus.Completed, WorkoutTemplateId = template1.Id };
        Context.ScheduledWorkouts.Add(sw1);
        Context.SaveChanges();
        var log1 = new WorkoutLog { ScheduledWorkoutId = sw1.Id, StartedAt = date1.AddHours(8), CompletedAt = date1.AddHours(9) };
        log1.EnduranceLogs.Add(new EnduranceLog
        {
            ExerciseId = _run5k.Id, ActivityType = ActivityType.Run,
            PlannedDistance = 5.0, ActualDistance = 5.0, ActualDurationSeconds = 1500, IsCompleted = true
        });
        Context.WorkoutLogs.Add(log1);
        Context.SaveChanges();

        // Create an active session that has this endurance exercise as SetLogs won't exist
        var template2 = new WorkoutTemplate { Name = "Active Run" };
        Context.WorkoutTemplates.Add(template2);
        Context.SaveChanges();
        var sw2 = new ScheduledWorkout { ScheduledDate = DateTime.UtcNow.Date, Status = WorkoutStatus.Planned, WorkoutTemplateId = template2.Id };
        Context.ScheduledWorkouts.Add(sw2);
        Context.SaveChanges();
        var activeLog = new WorkoutLog { ScheduledWorkoutId = sw2.Id, StartedAt = DateTime.UtcNow, CompletedAt = null };
        activeLog.EnduranceLogs.Add(new EnduranceLog
        {
            ExerciseId = _run5k.Id, ActivityType = ActivityType.Run,
            PlannedDistance = 5.0, IsCompleted = false
        });
        Context.WorkoutLogs.Add(activeLog);
        Context.SaveChanges();

        var suggestions = await _overloadService.GetSuggestionsAsync(activeLog.Id);

        // No suggestions because endurance exercises don't have SetLogs -> no strength exercise IDs
        Assert.Empty(suggestions);
    }

    [Fact]
    public async Task GetSuggestions_UsesCorrectIncrement_ForLegsExercise()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        // Two qualifying sessions for squat (Legs)
        CreateCompletedSession(_squat.Id, 3, 100.0, 5, date1);
        CreateCompletedSession(_squat.Id, 3, 100.0, 5, date2);

        var activeLogId = CreateActiveSession(_squat.Id, 3, 100.0, 5);

        var suggestions = await _overloadService.GetSuggestionsAsync(activeLogId);

        Assert.Single(suggestions);
        Assert.Equal(5.0, suggestions[0].Increment);
        Assert.Equal(105.0, suggestions[0].SuggestedWeight);
    }

    [Fact]
    public async Task GetSuggestions_ConsecutiveSessions_IsAtLeast2()
    {
        var date1 = new DateTime(2026, 3, 10, 0, 0, 0, DateTimeKind.Utc);
        var date2 = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc);

        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date1);
        CreateCompletedSession(_benchPress.Id, 3, 60.0, 8, date2);

        var activeLogId = CreateActiveSession(_benchPress.Id, 3, 60.0, 8);

        var suggestions = await _overloadService.GetSuggestionsAsync(activeLogId);

        Assert.Single(suggestions);
        Assert.Equal(2, suggestions[0].ConsecutiveSessions);
    }
}
