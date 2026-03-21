using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class MaterializationService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    /// <summary>
    /// Maps System.DayOfWeek to the custom DaysOfWeek flags enum.
    /// Explicit switch because ordinal values differ (System.DayOfWeek: Sunday=0, Monday=1, etc.).
    /// </summary>
    public static DaysOfWeek ToDaysOfWeek(System.DayOfWeek dayOfWeek) => dayOfWeek switch
    {
        System.DayOfWeek.Sunday => DaysOfWeek.Sunday,
        System.DayOfWeek.Monday => DaysOfWeek.Monday,
        System.DayOfWeek.Tuesday => DaysOfWeek.Tuesday,
        System.DayOfWeek.Wednesday => DaysOfWeek.Wednesday,
        System.DayOfWeek.Thursday => DaysOfWeek.Thursday,
        System.DayOfWeek.Friday => DaysOfWeek.Friday,
        System.DayOfWeek.Saturday => DaysOfWeek.Saturday,
        _ => DaysOfWeek.None
    };

    /// <summary>
    /// Returns the Monday of the week containing the given date (ISO 8601 weeks).
    /// </summary>
    public static DateTime GetMondayOfWeek(DateTime date)
    {
        return date.Date.AddDays(-((7 + (int)date.DayOfWeek - (int)System.DayOfWeek.Monday) % 7));
    }

    /// <summary>
    /// Generates occurrence dates for a recurrence rule within the specified window [windowStart, windowEnd).
    /// </summary>
    public static IEnumerable<DateTime> GenerateOccurrences(RecurrenceRule rule, DateTime windowStart, DateTime windowEnd)
    {
        switch (rule.FrequencyType)
        {
            case FrequencyType.Daily:
            {
                var anchor = rule.StartDate.Date;
                var intervalDays = Math.Max(rule.Interval, 1);

                // Find the first occurrence on or after windowStart
                var current = anchor;
                if (current < windowStart)
                {
                    var daysDiff = (windowStart.Date - anchor).Days;
                    var periodsToSkip = daysDiff / intervalDays;
                    current = anchor.AddDays(periodsToSkip * intervalDays);
                    if (current < windowStart.Date)
                        current = current.AddDays(intervalDays);
                }

                while (current < windowEnd)
                {
                    if (current >= windowStart)
                        yield return current;
                    current = current.AddDays(intervalDays);
                }
                break;
            }

            case FrequencyType.Weekly:
            {
                var weekStart = GetMondayOfWeek(windowStart);
                while (weekStart < windowEnd)
                {
                    for (int d = 0; d < 7; d++)
                    {
                        var day = weekStart.AddDays(d);
                        if (day >= windowStart && day < windowEnd)
                        {
                            var dayFlag = ToDaysOfWeek(day.DayOfWeek);
                            if (rule.DaysOfWeek.HasFlag(dayFlag) && dayFlag != DaysOfWeek.None)
                            {
                                yield return day;
                            }
                        }
                    }
                    weekStart = weekStart.AddDays(7);
                }
                break;
            }

            case FrequencyType.Custom:
            {
                var intervalWeeks = Math.Max(rule.Interval, 1);
                var weekStart = GetMondayOfWeek(windowStart);
                while (weekStart < windowEnd)
                {
                    for (int d = 0; d < 7; d++)
                    {
                        var day = weekStart.AddDays(d);
                        if (day >= windowStart && day < windowEnd)
                        {
                            var dayFlag = ToDaysOfWeek(day.DayOfWeek);
                            if (rule.DaysOfWeek.HasFlag(dayFlag) && dayFlag != DaysOfWeek.None)
                            {
                                yield return day;
                            }
                        }
                    }
                    weekStart = weekStart.AddDays(7 * intervalWeeks);
                }
                break;
            }
        }
    }

    /// <summary>
    /// Materializes scheduled workout rows for a recurrence rule within a 4-week rolling window.
    /// Skips dates that already have ScheduledWorkout rows (idempotent).
    /// </summary>
    public async Task MaterializeAsync(int recurrenceRuleId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var rule = await context.RecurrenceRules
            .Include(r => r.ScheduledWorkouts)
            .FirstOrDefaultAsync(r => r.Id == recurrenceRuleId);

        if (rule == null) return;

        var windowStart = DateTime.UtcNow.Date;
        var windowEnd = windowStart.AddDays(28);

        var existingDates = new HashSet<DateTime>(
            rule.ScheduledWorkouts.Select(sw => sw.ScheduledDate.Date));

        foreach (var date in GenerateOccurrences(rule, windowStart, windowEnd))
        {
            if (!existingDates.Contains(date.Date))
            {
                context.ScheduledWorkouts.Add(new ScheduledWorkout
                {
                    ScheduledDate = date,
                    WorkoutTemplateId = rule.WorkoutTemplateId,
                    AdHocName = rule.AdHocName,
                    RecurrenceRuleId = rule.Id,
                    Status = WorkoutStatus.Planned
                });
            }
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Materializes all active recurrence rules.
    /// </summary>
    public async Task MaterializeAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var ruleIds = await context.RecurrenceRules.Select(r => r.Id).ToListAsync();

        foreach (var ruleId in ruleIds)
        {
            await MaterializeAsync(ruleId);
        }
    }
}
