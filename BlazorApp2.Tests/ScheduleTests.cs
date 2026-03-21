using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

public class ScheduleTests : DataTestBase
{
    private WorkoutTemplate CreateTemplate(string name = "Test Workout")
    {
        var template = new WorkoutTemplate { Name = name };
        Context.WorkoutTemplates.Add(template);
        return template;
    }

    [Fact]
    public async Task ScheduledWorkout_WithDateAndStatus_PersistsCorrectly()
    {
        var template = CreateTemplate();
        var scheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc);

        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = scheduledDate,
            Status = WorkoutStatus.Planned,
            WorkoutTemplate = template
        });
        await Context.SaveChangesAsync();

        var loaded = await Context.ScheduledWorkouts.FirstAsync();

        Assert.Equal(scheduledDate, loaded.ScheduledDate);
        Assert.Equal(WorkoutStatus.Planned, loaded.Status);
    }

    [Fact]
    public async Task ScheduledWorkout_UpdateStatus_FromPlannedToCompleted_PersistsCorrectly()
    {
        var template = CreateTemplate();
        var workout = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            Status = WorkoutStatus.Planned,
            WorkoutTemplate = template
        };
        Context.ScheduledWorkouts.Add(workout);
        await Context.SaveChangesAsync();

        workout.Status = WorkoutStatus.Completed;
        await Context.SaveChangesAsync();

        var loaded = await Context.ScheduledWorkouts.FirstAsync();
        Assert.Equal(WorkoutStatus.Completed, loaded.Status);
    }

    [Fact]
    public async Task RecurrenceRule_WeeklyWithDaysOfWeek_PersistsCorrectly()
    {
        var template = CreateTemplate();
        Context.RecurrenceRules.Add(new RecurrenceRule
        {
            WorkoutTemplate = template,
            FrequencyType = FrequencyType.Weekly,
            Interval = 1,
            DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Wednesday | DaysOfWeek.Friday,
            StartDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc)
        });
        await Context.SaveChangesAsync();

        var loaded = await Context.RecurrenceRules.FirstAsync();

        Assert.Equal(FrequencyType.Weekly, loaded.FrequencyType);
        Assert.Equal(1, loaded.Interval);
        Assert.Equal(DaysOfWeek.Monday | DaysOfWeek.Wednesday | DaysOfWeek.Friday, loaded.DaysOfWeek);
    }

    [Fact]
    public async Task RecurrenceRule_DailyWithInterval_PersistsCorrectly()
    {
        var template = CreateTemplate();
        Context.RecurrenceRules.Add(new RecurrenceRule
        {
            WorkoutTemplate = template,
            FrequencyType = FrequencyType.Daily,
            Interval = 2,
            DaysOfWeek = DaysOfWeek.None,
            StartDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc)
        });
        await Context.SaveChangesAsync();

        var loaded = await Context.RecurrenceRules.FirstAsync();

        Assert.Equal(FrequencyType.Daily, loaded.FrequencyType);
        Assert.Equal(2, loaded.Interval);
        Assert.Equal(DaysOfWeek.None, loaded.DaysOfWeek);
    }

    [Fact]
    public async Task DaysOfWeek_FlagsEnum_HasFlagVerification_WorksCorrectly()
    {
        var template = CreateTemplate();
        Context.RecurrenceRules.Add(new RecurrenceRule
        {
            WorkoutTemplate = template,
            FrequencyType = FrequencyType.Weekly,
            Interval = 1,
            DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Friday,  // value = 17
            StartDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc)
        });
        await Context.SaveChangesAsync();

        var loaded = await Context.RecurrenceRules.FirstAsync();

        Assert.True(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Monday));
        Assert.True(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Friday));
        Assert.False(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Tuesday));
        Assert.False(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Wednesday));
        Assert.False(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Thursday));
    }

    // --- Ad-hoc workout tests ---

    [Fact]
    public async Task AdHocWorkout_NullTemplate_PersistsWithAdHocName()
    {
        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplateId = null,
            AdHocName = "Rest day yoga",
            Status = WorkoutStatus.Planned
        });
        await Context.SaveChangesAsync();

        var loaded = await Context.ScheduledWorkouts.FirstAsync();

        Assert.Equal("Rest day yoga", loaded.AdHocName);
        Assert.Null(loaded.WorkoutTemplateId);
    }

    [Fact]
    public async Task DisplayName_ReturnsTemplateName_WhenTemplateExists()
    {
        var template = CreateTemplate("Upper Body Pull");
        var workout = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplate = template,
            Status = WorkoutStatus.Planned
        };
        Context.ScheduledWorkouts.Add(workout);
        await Context.SaveChangesAsync();

        var loaded = await Context.ScheduledWorkouts
            .Include(sw => sw.WorkoutTemplate)
            .FirstAsync();

        Assert.Equal("Upper Body Pull", loaded.DisplayName);
    }

    [Fact]
    public async Task DisplayName_ReturnsAdHocName_WhenNoTemplate()
    {
        var workout = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplateId = null,
            AdHocName = "Rest day yoga",
            Status = WorkoutStatus.Planned
        };
        Context.ScheduledWorkouts.Add(workout);
        await Context.SaveChangesAsync();

        var loaded = await Context.ScheduledWorkouts.FirstAsync();

        Assert.Equal("Rest day yoga", loaded.DisplayName);
    }

    [Fact]
    public async Task DisplayName_ReturnsUntitled_WhenBothNull()
    {
        var workout = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplateId = null,
            AdHocName = null,
            Status = WorkoutStatus.Planned
        };
        Context.ScheduledWorkouts.Add(workout);
        await Context.SaveChangesAsync();

        var loaded = await Context.ScheduledWorkouts.FirstAsync();

        Assert.Equal("Untitled", loaded.DisplayName);
    }

    // --- DetermineWorkoutType tests ---

    [Fact]
    public void DetermineWorkoutType_AllStrength_ReturnsStrength()
    {
        var template = new WorkoutTemplate
        {
            Name = "Strength Day",
            Items = new List<TemplateItem>
            {
                new() { Exercise = new StrengthExercise { Name = "Bench Press", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Barbell } },
                new() { Exercise = new StrengthExercise { Name = "Rows", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Barbell } }
            }
        };

        var workout = new ScheduledWorkout
        {
            ScheduledDate = DateTime.UtcNow,
            WorkoutTemplate = template
        };

        Assert.Equal(WorkoutType.Strength, SchedulingService.DetermineWorkoutType(workout));
    }

    [Fact]
    public void DetermineWorkoutType_AllEndurance_ReturnsEndurance()
    {
        var template = new WorkoutTemplate
        {
            Name = "Cardio Day",
            Items = new List<TemplateItem>
            {
                new() { Exercise = new EnduranceExercise { Name = "5K Run", ActivityType = ActivityType.Run } },
                new() { Exercise = new EnduranceExercise { Name = "Cycling", ActivityType = ActivityType.Cycle } }
            }
        };

        var workout = new ScheduledWorkout
        {
            ScheduledDate = DateTime.UtcNow,
            WorkoutTemplate = template
        };

        Assert.Equal(WorkoutType.Endurance, SchedulingService.DetermineWorkoutType(workout));
    }

    [Fact]
    public void DetermineWorkoutType_Mixed_ReturnsMixed()
    {
        var template = new WorkoutTemplate
        {
            Name = "Mixed Session",
            Items = new List<TemplateItem>
            {
                new() { Exercise = new StrengthExercise { Name = "Pull-ups", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight } },
                new() { Exercise = new EnduranceExercise { Name = "Zone 2 Run", ActivityType = ActivityType.Run } }
            }
        };

        var workout = new ScheduledWorkout
        {
            ScheduledDate = DateTime.UtcNow,
            WorkoutTemplate = template
        };

        Assert.Equal(WorkoutType.Mixed, SchedulingService.DetermineWorkoutType(workout));
    }

    [Fact]
    public void DetermineWorkoutType_NoTemplate_ReturnsAdHoc()
    {
        var workout = new ScheduledWorkout
        {
            ScheduledDate = DateTime.UtcNow,
            WorkoutTemplateId = null,
            AdHocName = "Just stretching"
        };

        Assert.Equal(WorkoutType.AdHoc, SchedulingService.DetermineWorkoutType(workout));
    }

    // --- SchedulingService operation tests ---

    [Fact]
    public async Task SkipWorkoutAsync_SetsStatusToSkipped()
    {
        var factory = new TestDbContextFactory(Connection);
        var matService = new MaterializationService(factory);
        var service = new SchedulingService(factory, matService);

        var template = CreateTemplate();
        var workout = new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplate = template,
            Status = WorkoutStatus.Planned
        };
        Context.ScheduledWorkouts.Add(workout);
        await Context.SaveChangesAsync();

        await service.SkipWorkoutAsync(workout.Id);

        // Use a fresh context to verify (service used a separate context)
        await using var verifyContext = factory.CreateDbContext();
        var loaded = await verifyContext.ScheduledWorkouts.FindAsync(workout.Id);
        Assert.Equal(WorkoutStatus.Skipped, loaded!.Status);
    }

    [Fact]
    public async Task RemoveRecurringAsync_DeletesRuleAndFuturePlanned_KeepsCompleted()
    {
        var factory = new TestDbContextFactory(Connection);
        var matService = new MaterializationService(factory);
        var service = new SchedulingService(factory, matService);

        var rule = new RecurrenceRule
        {
            FrequencyType = FrequencyType.Weekly,
            DaysOfWeek = DaysOfWeek.Monday,
            StartDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc),
            Interval = 1
        };
        Context.RecurrenceRules.Add(rule);
        await Context.SaveChangesAsync();

        // Add completed and planned workouts for this rule
        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc),
            RecurrenceRuleId = rule.Id,
            Status = WorkoutStatus.Completed
        });
        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc),
            RecurrenceRuleId = rule.Id,
            Status = WorkoutStatus.Planned
        });
        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 4, 6, 0, 0, 0, DateTimeKind.Utc),
            RecurrenceRuleId = rule.Id,
            Status = WorkoutStatus.Planned
        });
        await Context.SaveChangesAsync();

        await service.RemoveRecurringAsync(rule.Id);

        // Use a fresh context to verify (service used a separate context)
        await using var verifyContext = factory.CreateDbContext();

        // Rule should be deleted
        Assert.Null(await verifyContext.RecurrenceRules.FindAsync(rule.Id));

        // Only the Completed workout should remain
        var remaining = await verifyContext.ScheduledWorkouts.ToListAsync();
        Assert.Single(remaining);
        Assert.Equal(WorkoutStatus.Completed, remaining[0].Status);
    }

    [Fact]
    public async Task GetWorkoutsForWeekAsync_ReturnsOnlyWorkoutsInRange()
    {
        var factory = new TestDbContextFactory(Connection);
        var matService = new MaterializationService(factory);
        var service = new SchedulingService(factory, matService);

        var template = CreateTemplate("Week Test");

        // Workout inside the week range
        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplate = template,
            Status = WorkoutStatus.Planned
        });
        // Workout outside the week range (next week)
        Context.ScheduledWorkouts.Add(new ScheduledWorkout
        {
            ScheduledDate = new DateTime(2026, 3, 31, 0, 0, 0, DateTimeKind.Utc),
            WorkoutTemplate = template,
            Status = WorkoutStatus.Planned
        });
        await Context.SaveChangesAsync();

        var weekStart = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc);
        var result = await service.GetWorkoutsForWeekAsync(weekStart);

        Assert.Single(result);
        Assert.Equal(new DateTime(2026, 3, 24, 0, 0, 0, DateTimeKind.Utc), result[0].ScheduledDate);
    }
}
