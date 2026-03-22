using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Services;

public class SessionService(IDbContextFactory<AppDbContext> contextFactory, PRDetectionService prDetectionService)
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory = contextFactory;
    private readonly PRDetectionService _prDetectionService = prDetectionService;

    /// <summary>
    /// Starts a new session for a scheduled workout by creating a WorkoutLog
    /// and snapshotting template targets into SetLog/EnduranceLog rows.
    /// If a session already exists (resume case), loads and returns it.
    /// </summary>
    public async Task<WorkoutLog> StartSessionAsync(int scheduledWorkoutId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var scheduled = await context.ScheduledWorkouts
            .Include(sw => sw.WorkoutTemplate!)
                .ThenInclude(t => t.Items.OrderBy(i => i.Position))
                    .ThenInclude(i => i.Exercise)
            .Include(sw => sw.WorkoutLog)
            .FirstOrDefaultAsync(sw => sw.Id == scheduledWorkoutId)
            ?? throw new InvalidOperationException("Scheduled workout not found");

        // If session already exists, return it (resume case per D-15)
        if (scheduled.WorkoutLog != null)
            return await LoadSessionAsync(scheduled.WorkoutLog.Id);

        var workoutLog = new WorkoutLog
        {
            ScheduledWorkoutId = scheduledWorkoutId,
            StartedAt = DateTime.UtcNow
        };

        if (scheduled.WorkoutTemplate != null)
        {
            foreach (var item in scheduled.WorkoutTemplate.Items.OrderBy(i => i.Position))
            {
                if (item.Exercise is StrengthExercise)
                {
                    var setCount = item.TargetSets ?? 1;
                    for (int s = 1; s <= setCount; s++)
                    {
                        workoutLog.SetLogs.Add(new SetLog
                        {
                            ExerciseId = item.ExerciseId,
                            SetNumber = s,
                            SetType = SetType.Working,
                            PlannedReps = item.TargetReps,
                            PlannedWeight = item.TargetWeight,
                            // Pre-fill actual with planned per D-01
                            ActualReps = item.TargetReps,
                            ActualWeight = item.TargetWeight,
                            IsCompleted = false
                        });
                    }
                }
                else if (item.Exercise is EnduranceExercise endEx)
                {
                    workoutLog.EnduranceLogs.Add(new EnduranceLog
                    {
                        ExerciseId = item.ExerciseId,
                        ActivityType = endEx.ActivityType,
                        PlannedDistance = item.TargetDistance,
                        PlannedDurationSeconds = item.TargetDurationSeconds,
                        PlannedPace = item.TargetPace,
                        PlannedHeartRateZone = item.TargetHeartRateZone,
                        // Pre-fill actual with planned
                        ActualDistance = item.TargetDistance,
                        ActualDurationSeconds = item.TargetDurationSeconds,
                        IsCompleted = false
                    });
                }
            }
        }

        context.WorkoutLogs.Add(workoutLog);
        scheduled.Status = WorkoutStatus.Completed;
        await context.SaveChangesAsync();

        // Reload with full navigation properties
        return await LoadSessionAsync(workoutLog.Id);
    }

    /// <summary>
    /// Loads a WorkoutLog with all navigation properties for the session UI.
    /// </summary>
    public async Task<WorkoutLog> LoadSessionAsync(int workoutLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WorkoutLogs
            .Include(wl => wl.SetLogs.OrderBy(sl => sl.SetNumber))
                .ThenInclude(sl => sl.Exercise)
            .Include(wl => wl.EnduranceLogs)
                .ThenInclude(el => el.Exercise)
            .Include(wl => wl.ScheduledWorkout)
                .ThenInclude(sw => sw.WorkoutTemplate)
            .FirstOrDefaultAsync(wl => wl.Id == workoutLogId)
            ?? throw new InvalidOperationException("Workout log not found");
    }

    /// <summary>
    /// Completes a set with actual values. Persists immediately per D-13.
    /// </summary>
    public async Task CompleteSetAsync(int setLogId, double? actualWeight, int? actualReps, SetType setType)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var setLog = await context.SetLogs.FindAsync(setLogId);
        if (setLog == null) return;

        setLog.ActualWeight = actualWeight;
        setLog.ActualReps = actualReps;
        setLog.SetType = setType;
        setLog.IsCompleted = true;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Uncompletes a previously completed set.
    /// </summary>
    public async Task UncompleteSetAsync(int setLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var setLog = await context.SetLogs.FindAsync(setLogId);
        if (setLog == null) return;

        setLog.IsCompleted = false;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Saves endurance log data with auto-calculated pace. Persists immediately per D-13.
    /// </summary>
    public async Task SaveEnduranceLogAsync(int enduranceLogId, double? actualDistance, int? actualDurationSeconds, int? actualHeartRateZone)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.EnduranceLogs.FindAsync(enduranceLogId);
        if (log == null) return;

        log.ActualDistance = actualDistance;
        log.ActualDurationSeconds = actualDurationSeconds;
        log.ActualHeartRateZone = actualHeartRateZone;

        // Auto-calculate pace (min/km) per D-06
        log.ActualPace = actualDistance > 0 && actualDurationSeconds > 0
            ? (actualDurationSeconds.Value / 60.0) / actualDistance.Value
            : null;

        log.IsCompleted = true;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Adds an extra set beyond the template target per D-03.
    /// </summary>
    public async Task<SetLog> AddSetAsync(int workoutLogId, int exerciseId, double? prefillWeight)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var maxSetNumber = await context.SetLogs
            .Where(sl => sl.WorkoutLogId == workoutLogId && sl.ExerciseId == exerciseId)
            .MaxAsync(sl => (int?)sl.SetNumber) ?? 0;

        var newSet = new SetLog
        {
            WorkoutLogId = workoutLogId,
            ExerciseId = exerciseId,
            SetNumber = maxSetNumber + 1,
            SetType = SetType.Working,
            ActualWeight = prefillWeight,
            IsCompleted = false
        };

        context.SetLogs.Add(newSet);
        await context.SaveChangesAsync();
        return newSet;
    }

    /// <summary>
    /// Gets previous strength performance for an exercise (last N completed sessions).
    /// </summary>
    public async Task<List<PreviousStrengthSession>> GetPreviousStrengthPerformanceAsync(int exerciseId, int? excludeWorkoutLogId = null, int limit = 3)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.SetLogs
            .Where(sl => sl.ExerciseId == exerciseId && sl.IsCompleted)
            .Where(sl => sl.WorkoutLog.CompletedAt != null);

        if (excludeWorkoutLogId.HasValue)
            query = query.Where(sl => sl.WorkoutLogId != excludeWorkoutLogId.Value);

        var grouped = await query
            .OrderByDescending(sl => sl.WorkoutLog.StartedAt)
            .Select(sl => new
            {
                sl.WorkoutLogId,
                sl.WorkoutLog.StartedAt,
                sl.SetNumber,
                sl.ActualWeight,
                sl.ActualReps
            })
            .ToListAsync();

        return grouped
            .GroupBy(sl => new { sl.WorkoutLogId, sl.StartedAt })
            .OrderByDescending(g => g.Key.StartedAt)
            .Take(limit)
            .Select(g => new PreviousStrengthSession(
                g.Key.StartedAt,
                g.OrderBy(s => s.SetNumber)
                 .Select(s => new PreviousSet(s.SetNumber, s.ActualWeight, s.ActualReps))
                 .ToList()
            ))
            .ToList();
    }

    /// <summary>
    /// Gets previous endurance performance for an exercise (last N completed sessions).
    /// </summary>
    public async Task<List<PreviousEnduranceSession>> GetPreviousEndurancePerformanceAsync(int exerciseId, int? excludeWorkoutLogId = null, int limit = 3)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var query = context.EnduranceLogs
            .Where(el => el.ExerciseId == exerciseId && el.IsCompleted)
            .Where(el => el.WorkoutLog.CompletedAt != null);

        if (excludeWorkoutLogId.HasValue)
            query = query.Where(el => el.WorkoutLogId != excludeWorkoutLogId.Value);

        return await query
            .OrderByDescending(el => el.WorkoutLog.StartedAt)
            .Take(limit)
            .Select(el => new PreviousEnduranceSession(
                el.WorkoutLog.StartedAt,
                el.ActualDistance,
                el.ActualDurationSeconds,
                el.ActualPace
            ))
            .ToListAsync();
    }

    /// <summary>
    /// Finishes a session with RPE and notes per D-11.
    /// Detects PRs inline per D-09 and returns any new personal records.
    /// </summary>
    public async Task<List<PersonalRecord>> FinishSessionAsync(int workoutLogId, int? rpe, string? notes)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.WorkoutLogs
            .Include(wl => wl.ScheduledWorkout)
            .FirstOrDefaultAsync(wl => wl.Id == workoutLogId);
        if (log == null) return new List<PersonalRecord>();

        log.CompletedAt = DateTime.UtcNow;
        log.Rpe = rpe;
        log.Notes = notes;
        log.ScheduledWorkout.Status = WorkoutStatus.Completed;
        await context.SaveChangesAsync();

        // Detect PRs inline per D-09
        var newPRs = await _prDetectionService.DetectAndSavePRsAsync(workoutLogId);
        return newPRs;
    }

    /// <summary>
    /// Abandons a session, keeping partial data per D-16.
    /// </summary>
    public async Task AbandonSessionAsync(int workoutLogId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var log = await context.WorkoutLogs
            .Include(wl => wl.ScheduledWorkout)
            .FirstOrDefaultAsync(wl => wl.Id == workoutLogId);
        if (log == null) return;

        log.CompletedAt = DateTime.UtcNow;
        log.ScheduledWorkout.Status = WorkoutStatus.Completed;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks for an incomplete session (StartedAt set, CompletedAt null) per D-15.
    /// Returns the ScheduledWorkoutId if found.
    /// </summary>
    public async Task<int?> GetIncompleteSessionIdAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        return await context.WorkoutLogs
            .Where(wl => wl.CompletedAt == null)
            .Select(wl => (int?)wl.ScheduledWorkoutId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets today's scheduled workouts for the session landing page per D-09.
    /// </summary>
    public async Task<List<ScheduledWorkout>> GetTodaysWorkoutsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var today = DateTime.UtcNow.Date;

        return await context.ScheduledWorkouts
            .Where(sw => sw.ScheduledDate.Date == today && sw.Status == WorkoutStatus.Planned)
            .Include(sw => sw.WorkoutTemplate)
            .Include(sw => sw.WorkoutLog)
            .OrderBy(sw => sw.ScheduledDate)
            .ToListAsync();
    }

    /// <summary>
    /// Updates the set type for a set (cycle through Working, WarmUp, Failure, Drop) per D-02.
    /// </summary>
    public async Task UpdateSetTypeAsync(int setLogId, SetType setType)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var setLog = await context.SetLogs.FindAsync(setLogId);
        if (setLog == null) return;

        setLog.SetType = setType;
        await context.SaveChangesAsync();
    }
}

// DTOs for previous performance queries
public record PreviousStrengthSession(DateTime Date, List<PreviousSet> Sets);
public record PreviousSet(int SetNumber, double? Weight, int? Reps);
public record PreviousEnduranceSession(DateTime Date, double? Distance, int? DurationSeconds, double? Pace);
