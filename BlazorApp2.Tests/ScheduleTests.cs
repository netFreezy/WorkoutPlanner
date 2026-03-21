using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

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
            DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Wednesday | DaysOfWeek.Friday
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
            DaysOfWeek = DaysOfWeek.None
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
            DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Friday  // value = 17
        });
        await Context.SaveChangesAsync();

        var loaded = await Context.RecurrenceRules.FirstAsync();

        Assert.True(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Monday));
        Assert.True(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Friday));
        Assert.False(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Tuesday));
        Assert.False(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Wednesday));
        Assert.False(loaded.DaysOfWeek.HasFlag(DaysOfWeek.Thursday));
    }
}
