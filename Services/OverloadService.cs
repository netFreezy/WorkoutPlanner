using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public record OverloadSuggestion(
    int ExerciseId,
    string ExerciseName,
    double CurrentWeight,
    double SuggestedWeight,
    double Increment,
    int ConsecutiveSessions,
    int PlannedSets,
    int PlannedReps);

public class OverloadService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public static double GetWeightIncrement(MuscleGroup muscleGroup) => muscleGroup switch
    {
        MuscleGroup.Chest => 2.5,
        MuscleGroup.Back => 2.5,
        MuscleGroup.Shoulders => 2.5,
        MuscleGroup.Legs => 5.0,
        MuscleGroup.FullBody => 5.0,
        MuscleGroup.Arms => 1.0,
        MuscleGroup.Core => 1.0,
        _ => 2.5
    };

    public async Task<List<OverloadSuggestion>> GetSuggestionsAsync(int workoutLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        // Load the current workout log with set logs and exercises
        var workoutLog = await context.WorkoutLogs
            .Include(wl => wl.SetLogs)
                .ThenInclude(sl => sl.Exercise)
            .FirstOrDefaultAsync(wl => wl.Id == workoutLogId);

        if (workoutLog == null)
            return new List<OverloadSuggestion>();

        // Get distinct strength exercise IDs from this session
        var strengthExerciseIds = workoutLog.SetLogs
            .Where(sl => sl.Exercise is StrengthExercise)
            .Select(sl => sl.ExerciseId)
            .Distinct()
            .ToList();

        var suggestions = new List<OverloadSuggestion>();

        foreach (var exerciseId in strengthExerciseIds)
        {
            // Query last 2 completed sessions for this exercise (excluding current)
            var recentSessions = await context.SetLogs
                .Where(sl => sl.ExerciseId == exerciseId
                    && sl.IsCompleted
                    && sl.SetType == SetType.Working
                    && sl.WorkoutLog.CompletedAt != null
                    && sl.WorkoutLogId != workoutLogId)
                .Select(sl => new
                {
                    sl.WorkoutLogId,
                    sl.WorkoutLog.CompletedAt,
                    sl.PlannedReps,
                    sl.PlannedWeight,
                    sl.ActualReps,
                    sl.ActualWeight,
                    sl.SetNumber
                })
                .ToListAsync();

            // Group by session, order by most recent
            var sessionGroups = recentSessions
                .GroupBy(sl => new { sl.WorkoutLogId, sl.CompletedAt })
                .OrderByDescending(g => g.Key.CompletedAt)
                .Take(2)
                .ToList();

            if (sessionGroups.Count < 2)
                continue;

            // Check if both sessions qualify: ALL working sets met or exceeded targets
            bool allQualify = true;
            foreach (var session in sessionGroups)
            {
                var sets = session.ToList();
                foreach (var set in sets)
                {
                    if (set.ActualReps == null || set.PlannedReps == null
                        || set.ActualWeight == null || set.PlannedWeight == null
                        || set.ActualReps < set.PlannedReps
                        || set.ActualWeight < set.PlannedWeight)
                    {
                        allQualify = false;
                        break;
                    }
                }
                if (!allQualify) break;
            }

            if (!allQualify)
                continue;

            // Get exercise details for muscle group and name
            var exercise = await context.Exercises.FindAsync(exerciseId);
            if (exercise is not StrengthExercise strengthExercise)
                continue;

            var increment = GetWeightIncrement(strengthExercise.MuscleGroup);

            // Use the most recent session for current weight, planned sets/reps
            var mostRecentSession = sessionGroups[0].ToList();
            var currentWeight = mostRecentSession
                .Where(s => s.PlannedWeight.HasValue)
                .Select(s => s.PlannedWeight!.Value)
                .FirstOrDefault();

            var plannedSets = mostRecentSession.Count;
            var plannedReps = mostRecentSession
                .OrderBy(s => s.SetNumber)
                .Select(s => s.PlannedReps ?? 0)
                .FirstOrDefault();

            suggestions.Add(new OverloadSuggestion(
                exerciseId,
                strengthExercise.Name,
                currentWeight,
                currentWeight + increment,
                increment,
                sessionGroups.Count,
                plannedSets,
                plannedReps));
        }

        return suggestions;
    }
}
