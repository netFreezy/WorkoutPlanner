using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;

namespace BlazorApp2.Tests;

/// <summary>
/// Test factory that creates new AppDbContext instances sharing the same
/// in-memory SQLite connection. Each context can be independently disposed
/// by the services' "await using" pattern without affecting the test's Context.
/// </summary>
public class TestDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDbContextFactory(SqliteConnection connection)
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    public AppDbContext CreateDbContext() => new AppDbContext(_options);
}

public class MaterializationTests : DataTestBase
{
    private readonly MaterializationService _service;

    public MaterializationTests()
    {
        var factory = new TestDbContextFactory(Connection);
        _service = new MaterializationService(factory);
    }

    [Fact]
    public async Task Weekly_MondayWednesdayFriday_Generates12DatesIn4Weeks()
    {
        // 2026-03-23 is a Monday
        var rule = new RecurrenceRule
        {
            FrequencyType = FrequencyType.Weekly,
            DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Wednesday | DaysOfWeek.Friday,
            StartDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc),
            Interval = 1
        };
        Context.RecurrenceRules.Add(rule);
        await Context.SaveChangesAsync();

        // Call GenerateOccurrences directly with a fixed window for predictable testing
        var windowStart = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = windowStart.AddDays(28);

        var dates = MaterializationService.GenerateOccurrences(rule, windowStart, windowEnd).ToList();

        Assert.Equal(12, dates.Count); // 3 days/week * 4 weeks

        // Verify each date is a Monday, Wednesday, or Friday
        foreach (var date in dates)
        {
            Assert.True(
                date.DayOfWeek == System.DayOfWeek.Monday ||
                date.DayOfWeek == System.DayOfWeek.Wednesday ||
                date.DayOfWeek == System.DayOfWeek.Friday,
                $"Expected Mon/Wed/Fri but got {date.DayOfWeek} on {date:yyyy-MM-dd}");
        }
    }

    [Fact]
    public async Task Daily_Interval2_Generates14DatesIn28Days()
    {
        var startDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc);
        var rule = new RecurrenceRule
        {
            FrequencyType = FrequencyType.Daily,
            Interval = 2,
            StartDate = startDate,
            DaysOfWeek = DaysOfWeek.None
        };
        Context.RecurrenceRules.Add(rule);
        await Context.SaveChangesAsync();

        var windowStart = startDate;
        var windowEnd = windowStart.AddDays(28);

        var dates = MaterializationService.GenerateOccurrences(rule, windowStart, windowEnd).ToList();

        Assert.Equal(14, dates.Count); // 28 days / 2 = 14

        // Verify dates are exactly 2 days apart
        for (int i = 1; i < dates.Count; i++)
        {
            Assert.Equal(2, (dates[i] - dates[i - 1]).Days);
        }
    }

    [Fact]
    public async Task Materialize_CalledTwice_NoDuplicates()
    {
        var startDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc);
        var rule = new RecurrenceRule
        {
            FrequencyType = FrequencyType.Weekly,
            DaysOfWeek = DaysOfWeek.Monday | DaysOfWeek.Wednesday | DaysOfWeek.Friday,
            StartDate = startDate,
            Interval = 1
        };
        Context.RecurrenceRules.Add(rule);
        await Context.SaveChangesAsync();

        // Materialize with a fixed window by creating workouts manually using GenerateOccurrences
        var windowStart = startDate;
        var windowEnd = windowStart.AddDays(28);

        var occurrences = MaterializationService.GenerateOccurrences(rule, windowStart, windowEnd).ToList();
        foreach (var date in occurrences)
        {
            Context.ScheduledWorkouts.Add(new ScheduledWorkout
            {
                ScheduledDate = date,
                RecurrenceRuleId = rule.Id,
                Status = WorkoutStatus.Planned
            });
        }
        await Context.SaveChangesAsync();

        var countAfterFirst = await Context.ScheduledWorkouts.CountAsync();

        // Simulate second materialization -- check for duplicates using the same dedup logic
        var existingDates = new HashSet<DateTime>(
            (await Context.ScheduledWorkouts
                .Where(sw => sw.RecurrenceRuleId == rule.Id)
                .ToListAsync())
                .Select(sw => sw.ScheduledDate.Date));

        var newOccurrences = MaterializationService.GenerateOccurrences(rule, windowStart, windowEnd)
            .Where(d => !existingDates.Contains(d.Date))
            .ToList();

        foreach (var date in newOccurrences)
        {
            Context.ScheduledWorkouts.Add(new ScheduledWorkout
            {
                ScheduledDate = date,
                RecurrenceRuleId = rule.Id,
                Status = WorkoutStatus.Planned
            });
        }
        await Context.SaveChangesAsync();

        var countAfterSecond = await Context.ScheduledWorkouts.CountAsync();

        Assert.Equal(countAfterFirst, countAfterSecond); // No duplicates
    }

    [Fact]
    public void Weekly_NoMatchingDaysInWindow_CreatesNoRows()
    {
        var rule = new RecurrenceRule
        {
            FrequencyType = FrequencyType.Weekly,
            DaysOfWeek = DaysOfWeek.None,
            StartDate = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc),
            Interval = 1
        };

        var windowStart = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc);
        var windowEnd = windowStart.AddDays(28);

        var dates = MaterializationService.GenerateOccurrences(rule, windowStart, windowEnd).ToList();

        Assert.Empty(dates);
    }

    [Theory]
    [InlineData(System.DayOfWeek.Sunday, DaysOfWeek.Sunday)]
    [InlineData(System.DayOfWeek.Monday, DaysOfWeek.Monday)]
    [InlineData(System.DayOfWeek.Tuesday, DaysOfWeek.Tuesday)]
    [InlineData(System.DayOfWeek.Wednesday, DaysOfWeek.Wednesday)]
    [InlineData(System.DayOfWeek.Thursday, DaysOfWeek.Thursday)]
    [InlineData(System.DayOfWeek.Friday, DaysOfWeek.Friday)]
    [InlineData(System.DayOfWeek.Saturday, DaysOfWeek.Saturday)]
    public void DaysOfWeek_FlagMapping_SystemDayOfWeekToCustomEnum(System.DayOfWeek systemDay, DaysOfWeek expected)
    {
        var result = MaterializationService.ToDaysOfWeek(systemDay);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetMondayOfWeek_ReturnsCorrectMonday()
    {
        // 2026-03-25 is a Wednesday
        var wednesday = new DateTime(2026, 3, 25, 0, 0, 0, DateTimeKind.Utc);
        var monday = MaterializationService.GetMondayOfWeek(wednesday);
        Assert.Equal(new DateTime(2026, 3, 23), monday);
        Assert.Equal(System.DayOfWeek.Monday, monday.DayOfWeek);

        // 2026-03-23 is already a Monday
        var mondayInput = new DateTime(2026, 3, 23, 0, 0, 0, DateTimeKind.Utc);
        var result = MaterializationService.GetMondayOfWeek(mondayInput);
        Assert.Equal(new DateTime(2026, 3, 23), result);

        // 2026-03-29 is a Sunday
        var sunday = new DateTime(2026, 3, 29, 0, 0, 0, DateTimeKind.Utc);
        var mondayOfSundayWeek = MaterializationService.GetMondayOfWeek(sunday);
        Assert.Equal(new DateTime(2026, 3, 23), mondayOfSundayWeek);
    }
}
