using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class PRDetectionService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    /// <summary>
    /// Detect and persist PRs for a completed session (per D-09: inline on session finish).
    /// Returns list of newly created PersonalRecord entries.
    /// </summary>
    public async Task<List<PersonalRecord>> DetectAndSavePRsAsync(int workoutLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var newPRs = new List<PersonalRecord>();

        var workoutLog = await context.WorkoutLogs
            .Include(wl => wl.SetLogs.Where(sl => sl.IsCompleted && sl.SetType == SetType.Working))
                .ThenInclude(sl => sl.Exercise)
            .Include(wl => wl.EnduranceLogs.Where(el => el.IsCompleted))
                .ThenInclude(el => el.Exercise)
            .FirstOrDefaultAsync(wl => wl.Id == workoutLogId);

        if (workoutLog == null) return newPRs;
        var achievedAt = workoutLog.CompletedAt ?? DateTime.UtcNow;

        // --- Strength PRs (per D-10) ---
        foreach (var exerciseGroup in workoutLog.SetLogs.GroupBy(sl => sl.ExerciseId))
        {
            var exerciseId = exerciseGroup.Key;

            // Get historical bests for this exercise
            var historicalSets = await context.SetLogs
                .Where(sl => sl.ExerciseId == exerciseId
                    && sl.IsCompleted
                    && sl.SetType == SetType.Working
                    && sl.WorkoutLogId != workoutLogId
                    && sl.WorkoutLog.CompletedAt != null)
                .Select(sl => new { sl.ActualWeight, sl.ActualReps })
                .ToListAsync();

            double prevMaxWeight = historicalSets.Any() ? historicalSets.Max(s => s.ActualWeight ?? 0) : 0;
            // Rep PR: most reps at any given weight -- track as max reps across all weights
            int prevMaxReps = historicalSets.Any() ? historicalSets.Max(s => s.ActualReps ?? 0) : 0;
            double prevMaxE1RM = historicalSets.Any()
                ? historicalSets.Max(s => AnalyticsService.EstimateE1RM(s.ActualWeight ?? 0, s.ActualReps ?? 0))
                : 0;

            var sessionSets = exerciseGroup.ToList();
            double sessionMaxWeight = sessionSets.Max(s => s.ActualWeight ?? 0);
            int sessionMaxReps = sessionSets.Max(s => s.ActualReps ?? 0);
            double sessionMaxE1RM = sessionSets.Max(s => AnalyticsService.EstimateE1RM(s.ActualWeight ?? 0, s.ActualReps ?? 0));

            // Weight PR
            if (sessionMaxWeight > prevMaxWeight && sessionMaxWeight > 0)
            {
                var pr = new PersonalRecord
                {
                    ExerciseId = exerciseId,
                    WorkoutLogId = workoutLogId,
                    AchievedAt = achievedAt,
                    StrengthType = StrengthPRType.Weight,
                    Value = sessionMaxWeight
                };
                context.PersonalRecords.Add(pr);
                newPRs.Add(pr);
            }

            // Rep PR
            if (sessionMaxReps > prevMaxReps && sessionMaxReps > 0)
            {
                var pr = new PersonalRecord
                {
                    ExerciseId = exerciseId,
                    WorkoutLogId = workoutLogId,
                    AchievedAt = achievedAt,
                    StrengthType = StrengthPRType.Reps,
                    Value = sessionMaxReps
                };
                context.PersonalRecords.Add(pr);
                newPRs.Add(pr);
            }

            // Estimated 1RM PR
            if (sessionMaxE1RM > prevMaxE1RM && sessionMaxE1RM > 0)
            {
                var pr = new PersonalRecord
                {
                    ExerciseId = exerciseId,
                    WorkoutLogId = workoutLogId,
                    AchievedAt = achievedAt,
                    StrengthType = StrengthPRType.EstimatedOneRepMax,
                    Value = sessionMaxE1RM
                };
                context.PersonalRecords.Add(pr);
                newPRs.Add(pr);
            }
        }

        // --- Endurance PRs (per D-11: per activity type) ---
        foreach (var endLog in workoutLog.EnduranceLogs)
        {
            var exerciseId = endLog.ExerciseId;

            var historicalEnd = await context.EnduranceLogs
                .Where(el => el.ExerciseId == exerciseId
                    && el.IsCompleted
                    && el.WorkoutLogId != workoutLogId
                    && el.WorkoutLog.CompletedAt != null)
                .Select(el => new { el.ActualPace, el.ActualDistance })
                .ToListAsync();

            // Pace PR (lower is better)
            if (endLog.ActualPace.HasValue && endLog.ActualPace > 0)
            {
                double? prevBestPace = historicalEnd
                    .Where(e => e.ActualPace.HasValue && e.ActualPace > 0)
                    .Select(e => e.ActualPace!.Value)
                    .DefaultIfEmpty(double.MaxValue)
                    .Min();

                if (endLog.ActualPace < prevBestPace || prevBestPace == double.MaxValue)
                {
                    var pr = new PersonalRecord
                    {
                        ExerciseId = exerciseId,
                        WorkoutLogId = workoutLogId,
                        AchievedAt = achievedAt,
                        EnduranceType = EndurancePRType.Pace,
                        ActivityType = endLog.ActivityType,
                        Value = endLog.ActualPace.Value
                    };
                    context.PersonalRecords.Add(pr);
                    newPRs.Add(pr);
                }
            }

            // Distance PR (higher is better)
            if (endLog.ActualDistance.HasValue && endLog.ActualDistance > 0)
            {
                double prevMaxDistance = historicalEnd
                    .Where(e => e.ActualDistance.HasValue && e.ActualDistance > 0)
                    .Select(e => e.ActualDistance!.Value)
                    .DefaultIfEmpty(0)
                    .Max();

                if (endLog.ActualDistance > prevMaxDistance)
                {
                    var pr = new PersonalRecord
                    {
                        ExerciseId = exerciseId,
                        WorkoutLogId = workoutLogId,
                        AchievedAt = achievedAt,
                        EnduranceType = EndurancePRType.Distance,
                        ActivityType = endLog.ActivityType,
                        Value = endLog.ActualDistance.Value
                    };
                    context.PersonalRecords.Add(pr);
                    newPRs.Add(pr);
                }
            }
        }

        if (newPRs.Any())
            await context.SaveChangesAsync();

        // Load exercise names for display
        foreach (var pr in newPRs)
        {
            if (pr.Exercise == null)
            {
                pr.Exercise = await context.Exercises.FindAsync(pr.ExerciseId) ?? new StrengthExercise { Name = "Unknown" };
            }
        }

        return newPRs;
    }

    /// <summary>
    /// Get all PRs grouped by exercise for the PRs tab (per D-12).
    /// Only returns exercises the user has actually set PRs for.
    /// </summary>
    public async Task<List<IGrouping<string, PersonalRecord>>> GetAllPRsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var prs = await context.PersonalRecords
            .Include(pr => pr.Exercise)
            .OrderByDescending(pr => pr.AchievedAt)
            .ToListAsync();

        return prs.GroupBy(pr => pr.Exercise.Name).ToList();
    }

    /// <summary>
    /// Get PR timeline data for the PR timeline chart (per D-13).
    /// </summary>
    public async Task<List<PersonalRecord>> GetPRTimelineAsync(DateTime rangeStart, DateTime rangeEnd)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.PersonalRecords
            .Include(pr => pr.Exercise)
            .Where(pr => pr.AchievedAt >= rangeStart && pr.AchievedAt < rangeEnd)
            .OrderBy(pr => pr.AchievedAt)
            .ToListAsync();
    }
}
