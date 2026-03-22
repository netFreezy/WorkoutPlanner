using BlazorApp2.Components.Shared;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace BlazorApp2.Components.Pages;

public partial class Session : IDisposable
{
    [Parameter] public int ScheduledWorkoutId { get; set; }

    private WorkoutLog? workoutLog;
    private Dictionary<int, ExerciseCompletionStatus> exerciseStatuses = new();
    private int? expandedExerciseId;
    private List<int> exerciseOrder = new();
    private Timer? _elapsedTimer;
    private TimeSpan _elapsed;
    private bool isResumed = false;
    private bool isLoading = true;
    private List<ScheduledWorkout>? todaysWorkouts;
    private Toast? _toast;
    private bool showSummary = false;
    private bool showAbandonDialog = false;

    protected override async Task OnInitializedAsync()
    {
        if (ScheduledWorkoutId > 0)
        {
            await LoadActiveSession();
        }
        else
        {
            // Landing state: check for incomplete session first
            var incompleteId = await SessionService.GetIncompleteSessionIdAsync();
            if (incompleteId.HasValue)
            {
                NavigationManager.NavigateTo($"/session/{incompleteId.Value}", replace: true);
                return;
            }

            todaysWorkouts = await SessionService.GetTodaysWorkoutsAsync();
            isLoading = false;
        }
    }

    private async Task LoadActiveSession()
    {
        try
        {
            workoutLog = await SessionService.StartSessionAsync(ScheduledWorkoutId);

            // Check if this is a resumed session (has completed sets already)
            isResumed = workoutLog.SetLogs.Any(sl => sl.IsCompleted) ||
                        workoutLog.EnduranceLogs.Any(el => el.IsCompleted);

            exerciseOrder = GetExerciseOrder();

            // Set first pending exercise as expanded
            expandedExerciseId = exerciseOrder.FirstOrDefault(id => !exerciseStatuses.ContainsKey(id));
            if (expandedExerciseId == 0 && exerciseOrder.Count > 0)
                expandedExerciseId = exerciseOrder[0];

            // Start elapsed timer
            StartTimer();

            isLoading = false;

            if (isResumed && _toast != null)
            {
                await _toast.ShowAsync("Session resumed -- picking up where you left off");
            }
        }
        catch
        {
            NavigationManager.NavigateTo("/calendar");
        }
    }

    private void StartTimer()
    {
        if (workoutLog == null) return;
        _elapsed = DateTime.UtcNow - workoutLog.StartedAt;
        _elapsedTimer = new Timer(_ =>
        {
            _elapsed = DateTime.UtcNow - workoutLog.StartedAt;
            InvokeAsync(StateHasChanged);
        }, null, 0, 1000);
    }

    private string FormatElapsed()
    {
        if (_elapsed.TotalHours >= 1)
            return _elapsed.ToString(@"h\:mm\:ss");
        return _elapsed.ToString(@"mm\:ss");
    }

    private List<int> GetExerciseOrder()
    {
        if (workoutLog == null) return new();

        var ordered = new List<int>();

        // Get exercise IDs from SetLogs ordered by SetNumber
        foreach (var setLog in workoutLog.SetLogs.OrderBy(sl => sl.Id))
        {
            if (!ordered.Contains(setLog.ExerciseId))
                ordered.Add(setLog.ExerciseId);
        }

        // Get exercise IDs from EnduranceLogs
        foreach (var endLog in workoutLog.EnduranceLogs.OrderBy(el => el.Id))
        {
            if (!ordered.Contains(endLog.ExerciseId))
                ordered.Add(endLog.ExerciseId);
        }

        return ordered;
    }

    private string GetWorkoutName()
    {
        return workoutLog?.ScheduledWorkout?.DisplayName ?? "Workout";
    }

    private Exercise GetExercise(int exerciseId)
    {
        var setLog = workoutLog?.SetLogs.FirstOrDefault(sl => sl.ExerciseId == exerciseId);
        if (setLog != null) return setLog.Exercise;

        var endLog = workoutLog?.EnduranceLogs.FirstOrDefault(el => el.ExerciseId == exerciseId);
        return endLog?.Exercise!;
    }

    private List<SetLog> GetSetLogsForExercise(int exerciseId)
    {
        return workoutLog?.SetLogs
            .Where(sl => sl.ExerciseId == exerciseId)
            .OrderBy(sl => sl.SetNumber)
            .ToList() ?? new();
    }

    private EnduranceLog? GetEnduranceLogForExercise(int exerciseId)
    {
        return workoutLog?.EnduranceLogs.FirstOrDefault(el => el.ExerciseId == exerciseId);
    }

    private string GetSegmentClass(ExerciseCompletionStatus? status)
    {
        return status switch
        {
            ExerciseCompletionStatus.Complete => "progress-bar__segment--complete",
            ExerciseCompletionStatus.Partial => "progress-bar__segment--partial",
            ExerciseCompletionStatus.Skipped => "progress-bar__segment--skipped",
            _ => "progress-bar__segment--pending"
        };
    }

    private int GetCompletedCount()
    {
        return exerciseStatuses.Count;
    }

    private bool AllExercisesHaveStatus()
    {
        return exerciseOrder.Count > 0 && exerciseStatuses.Count == exerciseOrder.Count;
    }

    private int GetTemplateExerciseCount(ScheduledWorkout workout)
    {
        return workout.WorkoutTemplate?.Items.Count ?? 0;
    }

    private void HandleExerciseClicked(int exerciseId)
    {
        expandedExerciseId = expandedExerciseId == exerciseId ? null : exerciseId;
    }

    private async Task HandleSetCompleted((int setLogId, double? weight, int? reps, SetType type) args)
    {
        await SessionService.CompleteSetAsync(args.setLogId, args.weight, args.reps, args.type);

        // Update local state
        var setLog = workoutLog?.SetLogs.FirstOrDefault(sl => sl.Id == args.setLogId);
        if (setLog != null)
        {
            setLog.ActualWeight = args.weight;
            setLog.ActualReps = args.reps;
            setLog.SetType = args.type;
            setLog.IsCompleted = true;
        }
    }

    private async Task HandleSetUncompleted(int setLogId)
    {
        await SessionService.UncompleteSetAsync(setLogId);

        var setLog = workoutLog?.SetLogs.FirstOrDefault(sl => sl.Id == setLogId);
        if (setLog != null)
        {
            setLog.IsCompleted = false;
        }
    }

    private async Task HandleEnduranceSaved((int enduranceLogId, double? distance, int? duration, int? hrZone) args)
    {
        await SessionService.SaveEnduranceLogAsync(args.enduranceLogId, args.distance, args.duration, args.hrZone);

        var endLog = workoutLog?.EnduranceLogs.FirstOrDefault(el => el.Id == args.enduranceLogId);
        if (endLog != null)
        {
            endLog.ActualDistance = args.distance;
            endLog.ActualDurationSeconds = args.duration;
            endLog.ActualHeartRateZone = args.hrZone;
            endLog.IsCompleted = true;
        }
    }

    private async Task HandleSetAdded((int exerciseId, double? prefillWeight) args)
    {
        if (workoutLog == null) return;

        var newSet = await SessionService.AddSetAsync(workoutLog.Id, args.exerciseId, args.prefillWeight);

        // Reload to get navigation properties
        workoutLog = await SessionService.LoadSessionAsync(workoutLog.Id);
        exerciseOrder = GetExerciseOrder();
    }

    private async Task HandleStatusChanged((int exerciseId, ExerciseCompletionStatus status) args)
    {
        exerciseStatuses[args.exerciseId] = args.status;
        ExpandNextPending();
        await Task.CompletedTask;
    }

    private async Task HandleSetTypeChanged((int setLogId, SetType type) args)
    {
        await SessionService.UpdateSetTypeAsync(args.setLogId, args.type);

        var setLog = workoutLog?.SetLogs.FirstOrDefault(sl => sl.Id == args.setLogId);
        if (setLog != null)
        {
            setLog.SetType = args.type;
        }
    }

    private void ExpandNextPending()
    {
        var nextPending = exerciseOrder.FirstOrDefault(id => !exerciseStatuses.ContainsKey(id));
        expandedExerciseId = nextPending == 0 ? null : nextPending;
    }

    private async Task StartSession(int scheduledWorkoutId)
    {
        NavigationManager.NavigateTo($"/session/{scheduledWorkoutId}");
        await Task.CompletedTask;
    }

    private void HandleFinish()
    {
        showSummary = true;
    }

    private async Task HandleFinishSession((int? rpe, string? notes) args)
    {
        if (workoutLog == null) return;
        await SessionService.FinishSessionAsync(workoutLog.Id, args.rpe, args.notes);
        _elapsedTimer?.Dispose();
        NavigationManager.NavigateTo("/calendar?toast=Session+complete");
    }

    private void HandleBackToSession()
    {
        showSummary = false;
    }

    private void ShowAbandonDialog()
    {
        showAbandonDialog = true;
    }

    private async Task HandleAbandonConfirm()
    {
        if (workoutLog == null) return;
        showAbandonDialog = false;
        await SessionService.AbandonSessionAsync(workoutLog.Id);
        _elapsedTimer?.Dispose();
        NavigationManager.NavigateTo("/calendar?toast=Session+abandoned");
    }

    private void OnBeforeNavigation(LocationChangingContext context)
    {
        if (workoutLog != null && workoutLog.CompletedAt == null && !context.TargetLocation.Contains("/session"))
        {
            context.PreventNavigation();
            showAbandonDialog = true;
            InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        _elapsedTimer?.Dispose();
    }
}
