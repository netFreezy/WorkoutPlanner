using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class SchedulingService(IDbContextFactory<AppDbContext> contextFactory, MaterializationService materializationService)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
    private readonly MaterializationService _materializationService = materializationService;

    /// <summary>
    /// Returns scheduled workouts for a 7-day period starting from weekStart,
    /// with template, items, exercises, and recurrence rule included.
    /// </summary>
    public async Task<List<ScheduledWorkout>> GetWorkoutsForWeekAsync(DateTime weekStart)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.ScheduledWorkouts
            .Include(sw => sw.WorkoutTemplate!)
                .ThenInclude(t => t.Items)
                    .ThenInclude(i => i.Exercise)
            .Include(sw => sw.RecurrenceRule)
            .Where(sw => sw.ScheduledDate >= weekStart && sw.ScheduledDate < weekStart.AddDays(7))
            .OrderBy(sw => sw.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Returns scheduled workouts for a calendar month starting from monthStart.
    /// </summary>
    public async Task<List<ScheduledWorkout>> GetWorkoutsForMonthAsync(DateTime monthStart)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.ScheduledWorkouts
            .Include(sw => sw.WorkoutTemplate!)
                .ThenInclude(t => t.Items)
                    .ThenInclude(i => i.Exercise)
            .Include(sw => sw.RecurrenceRule)
            .Where(sw => sw.ScheduledDate >= monthStart && sw.ScheduledDate < monthStart.AddMonths(1))
            .OrderBy(sw => sw.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Schedules a workout (template-based or ad-hoc), optionally with a recurrence rule.
    /// If recurrence is provided, materializes future occurrences.
    /// </summary>
    public async Task<ScheduledWorkout> ScheduleWorkoutAsync(int? templateId, string? adHocName, DateTime date, RecurrenceRule? recurrence)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var workout = new ScheduledWorkout
        {
            ScheduledDate = date,
            WorkoutTemplateId = templateId,
            AdHocName = adHocName,
            Status = WorkoutStatus.Planned
        };

        if (recurrence != null)
        {
            recurrence.WorkoutTemplateId = templateId;
            recurrence.AdHocName = adHocName;
            context.RecurrenceRules.Add(recurrence);
            await context.SaveChangesAsync();

            workout.RecurrenceRuleId = recurrence.Id;
            context.ScheduledWorkouts.Add(workout);
            await context.SaveChangesAsync();

            await _materializationService.MaterializeAsync(recurrence.Id);
        }
        else
        {
            context.ScheduledWorkouts.Add(workout);
            await context.SaveChangesAsync();
        }

        return workout;
    }

    /// <summary>
    /// Removes a single scheduled workout. If it was the last occurrence of a recurrence rule,
    /// deletes the rule as well.
    /// </summary>
    public async Task RemoveWorkoutAsync(int workoutId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var workout = await context.ScheduledWorkouts.FindAsync(workoutId);
        if (workout == null) return;

        var recurrenceRuleId = workout.RecurrenceRuleId;
        context.ScheduledWorkouts.Remove(workout);
        await context.SaveChangesAsync();

        // If this was the last occurrence, clean up the rule
        if (recurrenceRuleId.HasValue)
        {
            var remaining = await context.ScheduledWorkouts
                .AnyAsync(sw => sw.RecurrenceRuleId == recurrenceRuleId.Value);

            if (!remaining)
            {
                var rule = await context.RecurrenceRules.FindAsync(recurrenceRuleId.Value);
                if (rule != null)
                {
                    context.RecurrenceRules.Remove(rule);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    /// <summary>
    /// Removes a recurrence rule and all its future Planned occurrences.
    /// Completed and Skipped occurrences are preserved.
    /// </summary>
    public async Task RemoveRecurringAsync(int recurrenceRuleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var rule = await context.RecurrenceRules
            .Include(r => r.ScheduledWorkouts)
            .FirstOrDefaultAsync(r => r.Id == recurrenceRuleId);

        if (rule == null) return;

        var futureToRemove = rule.ScheduledWorkouts
            .Where(sw => sw.Status == WorkoutStatus.Planned)
            .ToList();

        context.ScheduledWorkouts.RemoveRange(futureToRemove);
        context.RecurrenceRules.Remove(rule);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Marks a scheduled workout as Skipped.
    /// </summary>
    public async Task SkipWorkoutAsync(int workoutId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var workout = await context.ScheduledWorkouts.FindAsync(workoutId);
        if (workout == null) return;

        workout.Status = WorkoutStatus.Skipped;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Reschedules a workout to a new date. If recurring, detaches from the recurrence rule.
    /// </summary>
    public async Task RescheduleWorkoutAsync(int workoutId, DateTime newDate)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var workout = await context.ScheduledWorkouts.FindAsync(workoutId);
        if (workout == null) return;

        workout.ScheduledDate = newDate;
        if (workout.RecurrenceRuleId.HasValue)
        {
            workout.RecurrenceRuleId = null;
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Determines the workout type based on exercises in the template.
    /// Returns AdHoc if no template, Strength/Endurance if all items are one type, Mixed if both.
    /// </summary>
    public static WorkoutType DetermineWorkoutType(ScheduledWorkout sw)
    {
        if (sw.WorkoutTemplate == null)
            return WorkoutType.AdHoc;

        var items = sw.WorkoutTemplate.Items;
        if (items == null || !items.Any())
            return WorkoutType.AdHoc;

        bool hasStrength = items.Any(i => i.Exercise is StrengthExercise);
        bool hasEndurance = items.Any(i => i.Exercise is EnduranceExercise);

        if (hasStrength && hasEndurance)
            return WorkoutType.Mixed;
        if (hasStrength)
            return WorkoutType.Strength;
        if (hasEndurance)
            return WorkoutType.Endurance;

        return WorkoutType.AdHoc;
    }
}
