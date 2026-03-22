using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

// DTO records for analytics data
public record WeeklyVolume(DateTime WeekStart, int TotalSets, double TotalVolume);
public record WeeklyEndurance(DateTime WeekStart, double TotalDistance, double? AvgPace, int SessionCount);
public record WeeklyAdherence(DateTime WeekStart, int Planned, int Completed, int Skipped);
public record WeeklyDeviation(DateTime WeekStart, double AvgWeightDeviation, double AvgRepsDeviation, double AvgDistanceDeviation, int SampleCount);
public record ExerciseWeeklyData(DateTime WeekStart, double MaxWeight, double BestE1RM, int TotalSets);
public record EnduranceExerciseWeeklyData(DateTime WeekStart, double TotalDistance, double? BestPace, int SessionCount);
public record KpiSummary(int SessionsThisWeek, int PlannedThisWeek, int CurrentStreak, int LongestStreak, double TotalVolume, string? LatestPRExercise, string? LatestPRValue, int? LatestPRDaysAgo);

public class AnalyticsService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    // ANLY-01: Weekly volume (total sets and total weight lifted, working sets only)
    public async Task<List<WeeklyVolume>> GetWeeklyVolumeAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var setLogs = await context.SetLogs
            .Where(sl => sl.IsCompleted
                && sl.SetType == SetType.Working
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= rangeStart
                && sl.WorkoutLog.CompletedAt < rangeEnd)
            .Select(sl => new { sl.WorkoutLog.CompletedAt, sl.ActualWeight, sl.ActualReps })
            .ToListAsync();

        var grouped = setLogs
            .GroupBy(sl => GetWeekStart(sl.CompletedAt!.Value))
            .Select(g => new WeeklyVolume(
                g.Key,
                g.Count(),
                g.Sum(sl => (sl.ActualWeight ?? 0) * (sl.ActualReps ?? 0))))
            .OrderBy(wv => wv.WeekStart)
            .ToList();

        return FillWeeklyGaps(grouped, rangeStart, rangeEnd,
            ws => new WeeklyVolume(ws, 0, 0));
    }

    // ANLY-04: Weekly endurance (pace avg and distance sum)
    public async Task<List<WeeklyEndurance>> GetWeeklyEnduranceAsync(DateTime rangeStart, DateTime rangeEnd, int? exerciseId = null)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.EnduranceLogs
            .Where(el => el.IsCompleted
                && el.WorkoutLog.CompletedAt != null
                && el.WorkoutLog.CompletedAt >= rangeStart
                && el.WorkoutLog.CompletedAt < rangeEnd);

        if (exerciseId.HasValue)
            query = query.Where(el => el.ExerciseId == exerciseId.Value);

        var logs = await query
            .Select(el => new { el.WorkoutLog.CompletedAt, el.ActualDistance, el.ActualPace })
            .ToListAsync();

        var grouped = logs
            .GroupBy(el => GetWeekStart(el.CompletedAt!.Value))
            .Select(g => new WeeklyEndurance(
                g.Key,
                g.Sum(el => el.ActualDistance ?? 0),
                g.Where(el => el.ActualPace.HasValue && el.ActualPace > 0).Any()
                    ? g.Where(el => el.ActualPace.HasValue && el.ActualPace > 0).Average(el => el.ActualPace!.Value)
                    : null,
                g.Select(el => el.CompletedAt).Distinct().Count()))
            .OrderBy(we => we.WeekStart)
            .ToList();

        return FillWeeklyGaps(grouped, rangeStart, rangeEnd,
            ws => new WeeklyEndurance(ws, 0, null, 0));
    }

    // ANLY-03: Weekly adherence (planned vs completed vs skipped)
    public async Task<List<WeeklyAdherence>> GetWeeklyAdherenceAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var scheduled = await context.ScheduledWorkouts
            .Where(sw => sw.ScheduledDate >= rangeStart && sw.ScheduledDate < rangeEnd)
            .Select(sw => new { sw.ScheduledDate, sw.Status })
            .ToListAsync();

        var grouped = scheduled
            .GroupBy(sw => GetWeekStart(sw.ScheduledDate))
            .Select(g => new WeeklyAdherence(
                g.Key,
                g.Count(),
                g.Count(sw => sw.Status == WorkoutStatus.Completed),
                g.Count(sw => sw.Status == WorkoutStatus.Skipped)))
            .OrderBy(wa => wa.WeekStart)
            .ToList();

        return FillWeeklyGaps(grouped, rangeStart, rangeEnd,
            ws => new WeeklyAdherence(ws, 0, 0, 0));
    }

    // ANLY-05: Weekly deviation (planned vs actual for strength and endurance)
    public async Task<List<WeeklyDeviation>> GetWeeklyDeviationAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Strength deviation
        var setLogs = await context.SetLogs
            .Where(sl => sl.IsCompleted
                && sl.SetType == SetType.Working
                && sl.PlannedWeight.HasValue && sl.ActualWeight.HasValue
                && sl.PlannedReps.HasValue && sl.ActualReps.HasValue
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= rangeStart
                && sl.WorkoutLog.CompletedAt < rangeEnd)
            .Select(sl => new
            {
                sl.WorkoutLog.CompletedAt,
                WeightDev = sl.PlannedWeight!.Value != 0
                    ? ((sl.ActualWeight!.Value - sl.PlannedWeight!.Value) / sl.PlannedWeight!.Value) * 100.0
                    : 0,
                RepsDev = sl.PlannedReps!.Value != 0
                    ? ((double)(sl.ActualReps!.Value - sl.PlannedReps!.Value) / sl.PlannedReps!.Value) * 100.0
                    : 0
            })
            .ToListAsync();

        // Endurance deviation
        var endLogs = await context.EnduranceLogs
            .Where(el => el.IsCompleted
                && el.PlannedDistance.HasValue && el.ActualDistance.HasValue
                && el.WorkoutLog.CompletedAt != null
                && el.WorkoutLog.CompletedAt >= rangeStart
                && el.WorkoutLog.CompletedAt < rangeEnd)
            .Select(el => new
            {
                el.WorkoutLog.CompletedAt,
                DistDev = el.PlannedDistance!.Value != 0
                    ? ((el.ActualDistance!.Value - el.PlannedDistance!.Value) / el.PlannedDistance!.Value) * 100.0
                    : 0
            })
            .ToListAsync();

        // Combine by week
        var allWeeks = setLogs.Select(s => GetWeekStart(s.CompletedAt!.Value))
            .Concat(endLogs.Select(e => GetWeekStart(e.CompletedAt!.Value)))
            .Distinct()
            .OrderBy(w => w);

        var result = allWeeks.Select(week =>
        {
            var weekSets = setLogs.Where(s => GetWeekStart(s.CompletedAt!.Value) == week).ToList();
            var weekEnd = endLogs.Where(e => GetWeekStart(e.CompletedAt!.Value) == week).ToList();
            return new WeeklyDeviation(
                week,
                weekSets.Any() ? weekSets.Average(s => s.WeightDev) : 0,
                weekSets.Any() ? weekSets.Average(s => s.RepsDev) : 0,
                weekEnd.Any() ? weekEnd.Average(e => e.DistDev) : 0,
                weekSets.Count + weekEnd.Count);
        }).ToList();

        return FillWeeklyGaps(result, rangeStart, rangeEnd,
            ws => new WeeklyDeviation(ws, 0, 0, 0, 0));
    }

    // D-08: Per-exercise drill-down (strength)
    public async Task<List<ExerciseWeeklyData>> GetExerciseHistoryAsync(int exerciseId, DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var setLogs = await context.SetLogs
            .Where(sl => sl.ExerciseId == exerciseId
                && sl.IsCompleted
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= rangeStart
                && sl.WorkoutLog.CompletedAt < rangeEnd)
            .Select(sl => new { sl.WorkoutLog.CompletedAt, sl.ActualWeight, sl.ActualReps })
            .ToListAsync();

        var grouped = setLogs
            .GroupBy(sl => GetWeekStart(sl.CompletedAt!.Value))
            .Select(g => new ExerciseWeeklyData(
                g.Key,
                g.Max(sl => sl.ActualWeight ?? 0),
                g.Max(sl => EstimateE1RM(sl.ActualWeight ?? 0, sl.ActualReps ?? 0)),
                g.Count()))
            .OrderBy(e => e.WeekStart)
            .ToList();

        return FillWeeklyGaps(grouped, rangeStart, rangeEnd,
            ws => new ExerciseWeeklyData(ws, 0, 0, 0));
    }

    // D-08: Per-exercise drill-down (endurance)
    public async Task<List<EnduranceExerciseWeeklyData>> GetEnduranceExerciseHistoryAsync(int exerciseId, DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var logs = await context.EnduranceLogs
            .Where(el => el.ExerciseId == exerciseId
                && el.IsCompleted
                && el.WorkoutLog.CompletedAt != null
                && el.WorkoutLog.CompletedAt >= rangeStart
                && el.WorkoutLog.CompletedAt < rangeEnd)
            .Select(el => new { el.WorkoutLog.CompletedAt, el.ActualDistance, el.ActualPace })
            .ToListAsync();

        var grouped = logs
            .GroupBy(el => GetWeekStart(el.CompletedAt!.Value))
            .Select(g => new EnduranceExerciseWeeklyData(
                g.Key,
                g.Sum(el => el.ActualDistance ?? 0),
                g.Where(el => el.ActualPace.HasValue && el.ActualPace > 0).Any()
                    ? g.Min(el => el.ActualPace!.Value) : null,
                g.Count()))
            .OrderBy(e => e.WeekStart)
            .ToList();

        return FillWeeklyGaps(grouped, rangeStart, rangeEnd,
            ws => new EnduranceExerciseWeeklyData(ws, 0, null, 0));
    }

    // KPI summary for Overview tab (per D-06)
    public async Task<KpiSummary> GetKpiSummaryAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var now = DateTime.UtcNow;
        var thisWeekStart = GetWeekStart(now);
        var thisWeekEnd = thisWeekStart.AddDays(7);

        // Sessions this week
        var thisWeekScheduled = await context.ScheduledWorkouts
            .Where(sw => sw.ScheduledDate >= thisWeekStart && sw.ScheduledDate < thisWeekEnd)
            .Select(sw => new { sw.Status })
            .ToListAsync();
        var sessionsThisWeek = thisWeekScheduled.Count(sw => sw.Status == WorkoutStatus.Completed);
        var plannedThisWeek = thisWeekScheduled.Count;

        // Current streak (consecutive days with completed workouts going backward from today)
        var completedDates = await context.ScheduledWorkouts
            .Where(sw => sw.Status == WorkoutStatus.Completed)
            .Select(sw => sw.ScheduledDate.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync();

        int currentStreak = 0;
        int longestStreak = 0;
        if (completedDates.Any())
        {
            // Current streak: start from today or most recent completed date
            var checkDate = now.Date;
            // Allow starting from today or yesterday
            if (completedDates.Contains(checkDate) || completedDates.Contains(checkDate.AddDays(-1)))
            {
                var startDate = completedDates.Contains(checkDate) ? checkDate : checkDate.AddDays(-1);
                var streakDate = startDate;
                foreach (var d in completedDates)
                {
                    if (d == streakDate)
                    {
                        currentStreak++;
                        streakDate = streakDate.AddDays(-1);
                    }
                    else if (d < streakDate) break;
                }
            }

            // Longest streak
            int tempStreak = 1;
            var sorted = completedDates.OrderBy(d => d).ToList();
            for (int i = 1; i < sorted.Count; i++)
            {
                if ((sorted[i] - sorted[i - 1]).Days == 1)
                    tempStreak++;
                else
                    tempStreak = 1;
                longestStreak = Math.Max(longestStreak, tempStreak);
            }
            longestStreak = Math.Max(longestStreak, tempStreak);
        }

        // Total volume this period (last 4 weeks default)
        var fourWeeksAgo = thisWeekStart.AddDays(-28);
        var volumeData = await context.SetLogs
            .Where(sl => sl.IsCompleted && sl.SetType == SetType.Working
                && sl.WorkoutLog.CompletedAt != null
                && sl.WorkoutLog.CompletedAt >= fourWeeksAgo)
            .Select(sl => new { sl.ActualWeight, sl.ActualReps })
            .ToListAsync();
        var totalVolume = volumeData.Sum(sl => (sl.ActualWeight ?? 0) * (sl.ActualReps ?? 0));

        // Latest PR
        var latestPR = await context.PersonalRecords
            .Include(pr => pr.Exercise)
            .OrderByDescending(pr => pr.AchievedAt)
            .FirstOrDefaultAsync();

        return new KpiSummary(
            sessionsThisWeek,
            plannedThisWeek,
            currentStreak,
            longestStreak,
            totalVolume,
            latestPR?.Exercise.Name,
            latestPR?.DisplayValue,
            latestPR != null ? (int)(now - latestPR.AchievedAt).TotalDays : null);
    }

    // Get list of strength exercises the user has actually logged (for drill-down dropdown)
    public async Task<List<(int Id, string Name)>> GetLoggedStrengthExercisesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.SetLogs
            .Where(sl => sl.IsCompleted)
            .Select(sl => new { sl.ExerciseId, sl.Exercise.Name })
            .Distinct()
            .OrderBy(e => e.Name)
            .Select(e => ValueTuple.Create(e.ExerciseId, e.Name))
            .ToListAsync();
    }

    // Get list of endurance exercises the user has actually logged (for drill-down dropdown)
    public async Task<List<(int Id, string Name)>> GetLoggedEnduranceExercisesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EnduranceLogs
            .Where(el => el.IsCompleted)
            .Select(el => new { el.ExerciseId, el.Exercise.Name })
            .Distinct()
            .OrderBy(e => e.Name)
            .Select(e => ValueTuple.Create(e.ExerciseId, e.Name))
            .ToListAsync();
    }

    // Epley formula: weight * (1 + reps / 30.0), per D-10
    public static double EstimateE1RM(double weight, int reps)
    {
        if (reps <= 0 || weight <= 0) return 0;
        if (reps == 1) return weight;
        return Math.Round(weight * (1.0 + reps / 30.0), 1);
    }

    // Monday-start week bucketing
    public static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    // D-17: Fill gaps so empty/skipped weeks show as zero bars
    private static List<T> FillWeeklyGaps<T>(List<T> data, DateTime rangeStart, DateTime rangeEnd, Func<DateTime, T> createEmpty) where T : class
    {
        var weekStartProp = typeof(T).GetProperty("WeekStart") ?? throw new InvalidOperationException("T must have WeekStart property");
        var dataDict = data.ToDictionary(d => (DateTime)weekStartProp.GetValue(d)!);
        var result = new List<T>();
        var current = GetWeekStart(rangeStart);

        while (current < rangeEnd)
        {
            result.Add(dataDict.TryGetValue(current, out var existing) ? existing : createEmpty(current));
            current = current.AddDays(7);
        }
        return result;
    }
}
