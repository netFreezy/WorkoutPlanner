using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class AnalyticsServiceTests : DataTestBase
{
    private readonly AnalyticsService _service;
    private readonly StrengthExercise _strengthExercise;
    private readonly StrengthExercise _strengthExercise2;
    private readonly EnduranceExercise _enduranceExercise;

    public AnalyticsServiceTests()
    {
        var factory = new TestDbContextFactory(Connection);
        _service = new AnalyticsService(factory);

        _strengthExercise = new StrengthExercise
        {
            Name = "Bench Press",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        };
        _strengthExercise2 = new StrengthExercise
        {
            Name = "Squat",
            MuscleGroup = MuscleGroup.Legs,
            Equipment = Equipment.Barbell
        };
        _enduranceExercise = new EnduranceExercise
        {
            Name = "5K Run",
            ActivityType = ActivityType.Run
        };

        Context.StrengthExercises.Add(_strengthExercise);
        Context.StrengthExercises.Add(_strengthExercise2);
        Context.EnduranceExercises.Add(_enduranceExercise);
        Context.SaveChanges();
    }

    /// <summary>
    /// Creates a completed session with strength set logs.
    /// Returns the WorkoutLog.Id.
    /// </summary>
    private int CreateCompletedSession(DateTime date, List<(int exerciseId, double weight, int reps, SetType setType, bool isCompleted)>? sets = null,
        List<(int exerciseId, double distance, double pace)>? enduranceLogs = null)
    {
        var template = new WorkoutTemplate { Name = $"Test Workout {date:yyyy-MM-dd}" };
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

        if (sets != null)
        {
            int setNum = 1;
            foreach (var (exerciseId, weight, reps, setType, isCompleted) in sets)
            {
                log.SetLogs.Add(new SetLog
                {
                    ExerciseId = exerciseId,
                    SetNumber = setNum++,
                    SetType = setType,
                    PlannedReps = reps,
                    PlannedWeight = weight,
                    ActualReps = reps,
                    ActualWeight = weight,
                    IsCompleted = isCompleted
                });
            }
        }

        if (enduranceLogs != null)
        {
            foreach (var (exerciseId, distance, pace) in enduranceLogs)
            {
                log.EnduranceLogs.Add(new EnduranceLog
                {
                    ExerciseId = exerciseId,
                    ActivityType = ActivityType.Run,
                    PlannedDistance = distance,
                    ActualDistance = distance,
                    ActualPace = pace,
                    ActualDurationSeconds = (int)(pace * distance * 60),
                    IsCompleted = true
                });
            }
        }

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    /// <summary>
    /// Creates a ScheduledWorkout with a given status (for adherence tests).
    /// </summary>
    private void CreateScheduledWorkout(DateTime date, WorkoutStatus status)
    {
        var template = new WorkoutTemplate { Name = $"Scheduled {date:yyyy-MM-dd}" };
        Context.WorkoutTemplates.Add(template);
        Context.SaveChanges();

        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = date,
            Status = status,
            WorkoutTemplateId = template.Id
        });
        Context.SaveChanges();
    }

    /// <summary>
    /// Creates a completed session with planned and actual values (for deviation tests).
    /// </summary>
    private int CreateDeviationSession(DateTime date,
        double plannedWeight, double actualWeight,
        int plannedReps, int actualReps,
        double? plannedDistance = null, double? actualDistance = null)
    {
        var template = new WorkoutTemplate { Name = $"Deviation Workout {date:yyyy-MM-dd}" };
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
            ExerciseId = _strengthExercise.Id,
            SetNumber = 1,
            SetType = SetType.Working,
            PlannedReps = plannedReps,
            PlannedWeight = plannedWeight,
            ActualReps = actualReps,
            ActualWeight = actualWeight,
            IsCompleted = true
        });

        if (plannedDistance.HasValue && actualDistance.HasValue)
        {
            log.EnduranceLogs.Add(new EnduranceLog
            {
                ExerciseId = _enduranceExercise.Id,
                ActivityType = ActivityType.Run,
                PlannedDistance = plannedDistance,
                ActualDistance = actualDistance,
                IsCompleted = true
            });
        }

        Context.WorkoutLogs.Add(log);
        Context.SaveChanges();
        return log.Id;
    }

    // --- Volume Tests (ANLY-01) ---

    [Fact]
    public async Task GetWeeklyVolume_ReturnsCorrectAggregation()
    {
        // 2026-03-02 is a Monday
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var wednesday = monday.AddDays(2);

        // Session 1: 3 working sets at 60kg x 10 reps
        CreateCompletedSession(monday, sets: new()
        {
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
        });

        // Session 2: 3 working sets at 60kg x 10 reps
        CreateCompletedSession(wednesday, sets: new()
        {
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
        });

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetWeeklyVolumeAsync(rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(monday, result[0].WeekStart);
        Assert.Equal(6, result[0].TotalSets);
        Assert.Equal(3600.0, result[0].TotalVolume); // 60 * 10 * 6
    }

    [Fact]
    public async Task GetWeeklyVolume_ExcludesNonWorkingSets()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        CreateCompletedSession(monday, sets: new()
        {
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
            (_strengthExercise.Id, 30.0, 10, SetType.WarmUp, true),
        });

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetWeeklyVolumeAsync(rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(1, result[0].TotalSets);       // Only working set
        Assert.Equal(600.0, result[0].TotalVolume);  // 60 * 10
    }

    [Fact]
    public async Task GetWeeklyVolume_ExcludesIncompleteSets()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        CreateCompletedSession(monday, sets: new()
        {
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
            (_strengthExercise.Id, 60.0, 10, SetType.Working, false), // incomplete
        });

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetWeeklyVolumeAsync(rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(1, result[0].TotalSets);
        Assert.Equal(600.0, result[0].TotalVolume);
    }

    // --- Endurance Tests (ANLY-04) ---

    [Fact]
    public async Task GetWeeklyEndurance_ReturnsDistanceAndPace()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var wednesday = monday.AddDays(2);

        CreateCompletedSession(monday, enduranceLogs: new()
        {
            (_enduranceExercise.Id, 5.0, 5.5),
        });

        CreateCompletedSession(wednesday, enduranceLogs: new()
        {
            (_enduranceExercise.Id, 10.0, 4.5),
        });

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetWeeklyEnduranceAsync(rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(15.0, result[0].TotalDistance);
        Assert.NotNull(result[0].AvgPace);
        Assert.Equal(5.0, result[0].AvgPace!.Value, 1);
    }

    // --- Adherence Tests (ANLY-03) ---

    [Fact]
    public async Task GetWeeklyAdherence_CountsStatusCorrectly()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        CreateScheduledWorkout(monday, WorkoutStatus.Completed);
        CreateScheduledWorkout(monday.AddDays(1), WorkoutStatus.Skipped);
        CreateScheduledWorkout(monday.AddDays(2), WorkoutStatus.Planned);

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetWeeklyAdherenceAsync(rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(3, result[0].Planned);    // Total count
        Assert.Equal(1, result[0].Completed);
        Assert.Equal(1, result[0].Skipped);
    }

    // --- Deviation Tests (ANLY-05) ---

    [Fact]
    public async Task GetWeeklyDeviation_ComputesPercentage()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        // PlannedWeight=50, ActualWeight=55 (10% over)
        // PlannedReps=10, ActualReps=8 (-20%)
        CreateDeviationSession(monday, plannedWeight: 50, actualWeight: 55, plannedReps: 10, actualReps: 8);

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetWeeklyDeviationAsync(rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(10.0, result[0].AvgWeightDeviation, 1);
        Assert.Equal(-20.0, result[0].AvgRepsDeviation, 1);
    }

    // --- Gap-filling Tests (D-17) ---

    [Fact]
    public async Task GapFilling_InsertsZeroWeeks()
    {
        // Create data in week 1 and week 3 of a 4-week range
        var week1Monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var week3Monday = week1Monday.AddDays(14);

        CreateCompletedSession(week1Monday, sets: new()
        {
            (_strengthExercise.Id, 60.0, 10, SetType.Working, true),
        });
        CreateCompletedSession(week3Monday, sets: new()
        {
            (_strengthExercise.Id, 70.0, 8, SetType.Working, true),
        });

        var rangeStart = week1Monday;
        var rangeEnd = week1Monday.AddDays(28);

        var result = await _service.GetWeeklyVolumeAsync(rangeStart, rangeEnd);

        Assert.Equal(4, result.Count);
        // Week 1 has data
        Assert.Equal(1, result[0].TotalSets);
        Assert.Equal(600.0, result[0].TotalVolume);
        // Week 2 is gap-filled with zeros
        Assert.Equal(0, result[1].TotalSets);
        Assert.Equal(0.0, result[1].TotalVolume);
        // Week 3 has data
        Assert.Equal(1, result[2].TotalSets);
        Assert.Equal(560.0, result[2].TotalVolume); // 70 * 8
        // Week 4 is gap-filled with zeros
        Assert.Equal(0, result[3].TotalSets);
        Assert.Equal(0.0, result[3].TotalVolume);
    }

    [Fact]
    public async Task EmptyDatabase_ReturnsGapFilledZeros()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);
        var rangeEnd = monday.AddDays(28);

        var result = await _service.GetWeeklyVolumeAsync(monday, rangeEnd);

        Assert.Equal(4, result.Count);
        foreach (var week in result)
        {
            Assert.Equal(0, week.TotalSets);
            Assert.Equal(0.0, week.TotalVolume);
        }
    }

    // --- Epley Formula Tests ---

    [Fact]
    public void EstimateE1RM_EdgeCases()
    {
        // weight=0 returns 0
        Assert.Equal(0, AnalyticsService.EstimateE1RM(0, 10));

        // reps=0 returns 0
        Assert.Equal(0, AnalyticsService.EstimateE1RM(100, 0));

        // reps=1 returns weight
        Assert.Equal(100, AnalyticsService.EstimateE1RM(100, 1));

        // Normal case: weight * (1 + reps/30.0)
        var expected = Math.Round(100 * (1 + 10 / 30.0), 1);
        Assert.Equal(expected, AnalyticsService.EstimateE1RM(100, 10));
    }

    // --- Per-exercise drill-down Tests ---

    [Fact]
    public async Task GetExerciseHistory_ReturnsPerExerciseData()
    {
        var monday = new DateTime(2026, 3, 2, 0, 0, 0, DateTimeKind.Utc);

        // Session with both exercises
        CreateCompletedSession(monday, sets: new()
        {
            (_strengthExercise.Id, 80.0, 8, SetType.Working, true),
            (_strengthExercise.Id, 85.0, 6, SetType.Working, true),
            (_strengthExercise2.Id, 100.0, 5, SetType.Working, true),
        });

        var rangeStart = monday;
        var rangeEnd = monday.AddDays(7);

        var result = await _service.GetExerciseHistoryAsync(_strengthExercise.Id, rangeStart, rangeEnd);

        Assert.Single(result);
        Assert.Equal(85.0, result[0].MaxWeight);   // Max of 80 and 85
        // Best e1RM: max(EstimateE1RM(80,8), EstimateE1RM(85,6))
        var e1rm1 = AnalyticsService.EstimateE1RM(80.0, 8);
        var e1rm2 = AnalyticsService.EstimateE1RM(85.0, 6);
        Assert.Equal(Math.Max(e1rm1, e1rm2), result[0].BestE1RM);
        Assert.Equal(2, result[0].TotalSets);  // Only bench press sets
    }
}
