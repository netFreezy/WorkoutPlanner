using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public record HistorySession(
    int WorkoutLogId,
    DateTime CompletedAt,
    string WorkoutName,
    string WorkoutType,
    int ExerciseCount,
    int DurationMinutes,
    double TotalVolume,
    double? TotalDistance,
    int? Rpe,
    string? Notes,
    int? TemplateId,
    List<HistoryExerciseDetail> Exercises);

public record HistoryExerciseDetail(
    string ExerciseName,
    bool IsStrength,
    List<HistorySetDetail> Sets,
    HistoryEnduranceDetail? Endurance);

public record HistorySetDetail(int SetNumber, double? Weight, int? Reps, string SetType);
public record HistoryEnduranceDetail(double? Distance, int? DurationSeconds, double? Pace);

public class HistoryService(IDbContextFactory<AppDbContext> contextFactory)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;

    public async Task<List<HistorySession>> GetCompletedSessionsAsync(
        DateTime? dateStart, DateTime? dateEnd, int? exerciseId, int skip, int take)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.WorkoutLogs
            .Where(wl => wl.CompletedAt != null);

        if (dateStart.HasValue)
            query = query.Where(wl => wl.CompletedAt >= dateStart.Value);
        if (dateEnd.HasValue)
            query = query.Where(wl => wl.CompletedAt < dateEnd.Value);
        if (exerciseId.HasValue)
            query = query.Where(wl =>
                wl.SetLogs.Any(sl => sl.ExerciseId == exerciseId.Value) ||
                wl.EnduranceLogs.Any(el => el.ExerciseId == exerciseId.Value));

        var workoutLogs = await query
            .Include(wl => wl.ScheduledWorkout)
                .ThenInclude(sw => sw.WorkoutTemplate)
            .Include(wl => wl.SetLogs.OrderBy(sl => sl.SetNumber))
                .ThenInclude(sl => sl.Exercise)
            .Include(wl => wl.EnduranceLogs)
                .ThenInclude(el => el.Exercise)
            .OrderByDescending(wl => wl.CompletedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return workoutLogs.Select(MapToHistorySession).ToList();
    }

    public async Task<int> GetTotalCountAsync(DateTime? dateStart, DateTime? dateEnd, int? exerciseId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.WorkoutLogs
            .Where(wl => wl.CompletedAt != null);

        if (dateStart.HasValue)
            query = query.Where(wl => wl.CompletedAt >= dateStart.Value);
        if (dateEnd.HasValue)
            query = query.Where(wl => wl.CompletedAt < dateEnd.Value);
        if (exerciseId.HasValue)
            query = query.Where(wl =>
                wl.SetLogs.Any(sl => sl.ExerciseId == exerciseId.Value) ||
                wl.EnduranceLogs.Any(el => el.ExerciseId == exerciseId.Value));

        return await query.CountAsync();
    }

    public async Task<List<(int Id, string Name)>> GetLoggedExercisesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var strengthExercises = await context.SetLogs
            .Where(sl => sl.IsCompleted && sl.WorkoutLog.CompletedAt != null)
            .Select(sl => new { sl.ExerciseId, sl.Exercise.Name })
            .Distinct()
            .ToListAsync();

        var enduranceExercises = await context.EnduranceLogs
            .Where(el => el.IsCompleted && el.WorkoutLog.CompletedAt != null)
            .Select(el => new { el.ExerciseId, el.Exercise.Name })
            .Distinct()
            .ToListAsync();

        return strengthExercises
            .Concat(enduranceExercises)
            .DistinctBy(e => e.ExerciseId)
            .OrderBy(e => e.Name)
            .Select(e => (e.ExerciseId, e.Name))
            .ToList();
    }

    public async Task<HistorySession?> GetLastCompletedWorkoutAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var workoutLog = await context.WorkoutLogs
            .Where(wl => wl.CompletedAt != null)
            .Include(wl => wl.ScheduledWorkout)
                .ThenInclude(sw => sw.WorkoutTemplate)
            .Include(wl => wl.SetLogs.OrderBy(sl => sl.SetNumber))
                .ThenInclude(sl => sl.Exercise)
            .Include(wl => wl.EnduranceLogs)
                .ThenInclude(el => el.Exercise)
            .OrderByDescending(wl => wl.CompletedAt)
            .FirstOrDefaultAsync();

        return workoutLog == null ? null : MapToHistorySession(workoutLog);
    }

    public async Task<ScheduledWorkout?> GetTodaysScheduledWorkoutAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var today = DateTime.UtcNow.Date;

        return await context.ScheduledWorkouts
            .Where(sw => sw.ScheduledDate.Date == today && sw.Status == WorkoutStatus.Planned)
            .Include(sw => sw.WorkoutTemplate!)
                .ThenInclude(t => t.Items.OrderBy(i => i.Position))
                    .ThenInclude(i => i.Exercise)
            .OrderBy(sw => sw.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<ScheduledWorkout?> GetTomorrowsScheduledWorkoutAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var tomorrow = DateTime.UtcNow.Date.AddDays(1);

        return await context.ScheduledWorkouts
            .Where(sw => sw.ScheduledDate.Date == tomorrow && sw.Status == WorkoutStatus.Planned)
            .Include(sw => sw.WorkoutTemplate!)
                .ThenInclude(t => t.Items.OrderBy(i => i.Position))
                    .ThenInclude(i => i.Exercise)
            .OrderBy(sw => sw.Id)
            .FirstOrDefaultAsync();
    }

    private static HistorySession MapToHistorySession(WorkoutLog wl)
    {
        var workoutName = wl.ScheduledWorkout.WorkoutTemplate?.Name
            ?? wl.ScheduledWorkout.AdHocName ?? "Untitled";

        var hasStrength = wl.SetLogs.Any();
        var hasEndurance = wl.EnduranceLogs.Any();
        var workoutType = (hasStrength, hasEndurance) switch
        {
            (true, true) => "Mixed",
            (true, false) => "Strength",
            (false, true) => "Endurance",
            _ => "Unknown"
        };

        var durationMinutes = wl.CompletedAt.HasValue
            ? (int)Math.Round((wl.CompletedAt.Value - wl.StartedAt).TotalMinutes)
            : 0;

        var totalVolume = wl.SetLogs
            .Where(sl => sl.IsCompleted && sl.SetType == SetType.Working)
            .Sum(sl => (sl.ActualWeight ?? 0) * (sl.ActualReps ?? 0));

        var totalDistance = wl.EnduranceLogs
            .Where(el => el.IsCompleted && el.ActualDistance.HasValue)
            .Sum(el => el.ActualDistance!.Value);

        var exerciseCount = wl.SetLogs.Select(sl => sl.ExerciseId)
            .Concat(wl.EnduranceLogs.Select(el => el.ExerciseId))
            .Distinct()
            .Count();

        // Build exercise details
        var exercises = new List<HistoryExerciseDetail>();

        // Strength exercises grouped by exercise
        var strengthGroups = wl.SetLogs
            .GroupBy(sl => new { sl.ExerciseId, sl.Exercise.Name })
            .OrderBy(g => g.Min(sl => sl.SetNumber));

        foreach (var group in strengthGroups)
        {
            exercises.Add(new HistoryExerciseDetail(
                group.Key.Name,
                true,
                group.OrderBy(sl => sl.SetNumber)
                     .Select(sl => new HistorySetDetail(sl.SetNumber, sl.ActualWeight, sl.ActualReps, sl.SetType.ToString()))
                     .ToList(),
                null));
        }

        // Endurance exercises
        foreach (var el in wl.EnduranceLogs)
        {
            exercises.Add(new HistoryExerciseDetail(
                el.Exercise.Name,
                false,
                new List<HistorySetDetail>(),
                new HistoryEnduranceDetail(el.ActualDistance, el.ActualDurationSeconds, el.ActualPace)));
        }

        return new HistorySession(
            wl.Id,
            wl.CompletedAt!.Value,
            workoutName,
            workoutType,
            exerciseCount,
            durationMinutes,
            totalVolume,
            totalDistance > 0 ? totalDistance : null,
            wl.Rpe,
            wl.Notes,
            wl.ScheduledWorkout.WorkoutTemplateId,
            exercises);
    }
}
