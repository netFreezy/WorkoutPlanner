using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Components.Pages;

public partial class Home : ComponentBase
{
    [Inject] private HistoryService HistoryService { get; set; } = null!;
    [Inject] private SchedulingService SchedulingService { get; set; } = null!;
    [Inject] private SessionService SessionService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IDbContextFactory<AppDbContext> ContextFactory { get; set; } = null!;

    private ScheduledWorkout? todaysWorkout;
    private ScheduledWorkout? tomorrowsWorkout;
    private HistorySession? lastCompleted;
    private bool isLoading = true;
    private bool showAllExercises = false;
    private bool isRepeating = false;
    private bool hasAnyTemplates = false;

    protected override async Task OnInitializedAsync()
    {
        todaysWorkout = await HistoryService.GetTodaysScheduledWorkoutAsync();
        tomorrowsWorkout = await HistoryService.GetTomorrowsScheduledWorkoutAsync();

        if (todaysWorkout == null)
        {
            lastCompleted = await HistoryService.GetLastCompletedWorkoutAsync();
        }

        if (todaysWorkout == null && lastCompleted == null)
        {
            await using var context = await ContextFactory.CreateDbContextAsync();
            hasAnyTemplates = await context.WorkoutTemplates.AnyAsync();
        }

        isLoading = false;
    }

    private string GetWorkoutTypeClass(ScheduledWorkout sw)
    {
        var type = SchedulingService.DetermineWorkoutType(sw);
        return type switch
        {
            WorkoutType.Strength => "strength",
            WorkoutType.Endurance => "endurance",
            WorkoutType.Mixed => "mixed",
            WorkoutType.AdHoc => "adhoc",
            _ => ""
        };
    }

    private string GetWorkoutTypeLabel(ScheduledWorkout sw)
    {
        var type = SchedulingService.DetermineWorkoutType(sw);
        return type switch
        {
            WorkoutType.Strength => "Strength",
            WorkoutType.Endurance => "Endurance",
            WorkoutType.Mixed => "Mixed",
            WorkoutType.AdHoc => "Ad Hoc",
            _ => ""
        };
    }

    private string GetWorkoutTypeDotColor(ScheduledWorkout sw)
    {
        var label = GetWorkoutTypeLabel(sw);
        return label switch
        {
            "Strength" => "var(--color-strength-text)",
            "Endurance" => "var(--color-endurance-text)",
            "Mixed" => "var(--color-mixed-text)",
            _ => "var(--color-text-tertiary)"
        };
    }

    private string GetHistoryTypeDotColor(string workoutType)
    {
        return workoutType switch
        {
            "Strength" => "var(--color-strength-text)",
            "Endurance" => "var(--color-endurance-text)",
            "Mixed" => "var(--color-mixed-text)",
            _ => "var(--color-text-tertiary)"
        };
    }

    private string FormatTargets(TemplateItem item)
    {
        if (item.Exercise is StrengthExercise)
        {
            var sets = item.TargetSets?.ToString() ?? "?";
            var reps = item.TargetReps?.ToString() ?? "?";
            var weight = item.TargetWeight.HasValue ? $" @ {item.TargetWeight}kg" : "";
            return $"{sets}x{reps}{weight}";
        }

        if (item.Exercise is EnduranceExercise)
        {
            var parts = new List<string>();
            if (item.TargetDistance.HasValue)
                parts.Add($"{item.TargetDistance}km");
            if (item.TargetDurationSeconds.HasValue)
                parts.Add(FormatDuration(item.TargetDurationSeconds.Value));
            return parts.Count > 0 ? string.Join(" / ", parts) : "";
        }

        return "";
    }

    private static string FormatDuration(int totalSeconds)
    {
        var minutes = totalSeconds / 60;
        var seconds = totalSeconds % 60;
        return seconds > 0 ? $"{minutes}:{seconds:D2}" : $"{minutes}:00";
    }

    private string FormatHistoryTargets(HistoryExerciseDetail ex)
    {
        if (ex.IsStrength && ex.Sets.Count > 0)
        {
            var workingSets = ex.Sets.Where(s => s.SetType == "Working").ToList();
            if (workingSets.Count == 0) workingSets = ex.Sets;

            var firstSet = workingSets.First();
            var reps = firstSet.Reps?.ToString() ?? "?";
            var weight = firstSet.Weight.HasValue ? $" @ {firstSet.Weight}kg" : "";
            return $"{workingSets.Count}x{reps}{weight}";
        }

        if (!ex.IsStrength && ex.Endurance != null)
        {
            var parts = new List<string>();
            if (ex.Endurance.Distance.HasValue)
                parts.Add($"{ex.Endurance.Distance:F1}km");
            if (ex.Endurance.DurationSeconds.HasValue)
                parts.Add(FormatDuration(ex.Endurance.DurationSeconds.Value));
            return parts.Count > 0 ? string.Join(" / ", parts) : "";
        }

        return "";
    }

    private int GetRemainingExerciseCount(int totalCount)
    {
        return Math.Max(0, totalCount - 5);
    }

    private void StartSession()
    {
        if (todaysWorkout != null)
        {
            Navigation.NavigateTo($"/session/{todaysWorkout.Id}");
        }
    }

    private async Task RepeatWorkout()
    {
        if (lastCompleted == null) return;

        isRepeating = true;

        try
        {
            ScheduledWorkout newWorkout;
            if (lastCompleted.TemplateId != null)
            {
                newWorkout = await SchedulingService.ScheduleWorkoutAsync(
                    lastCompleted.TemplateId, null, DateTime.UtcNow.Date, null);
            }
            else
            {
                newWorkout = await SchedulingService.ScheduleWorkoutAsync(
                    null, lastCompleted.WorkoutName, DateTime.UtcNow.Date, null);
            }

            Navigation.NavigateTo($"/session/{newWorkout.Id}");
        }
        finally
        {
            isRepeating = false;
        }
    }

    private void ToggleExerciseList()
    {
        showAllExercises = !showAllExercises;
    }

    private void NavigateToTemplates()
    {
        Navigation.NavigateTo("/templates");
    }

    private void NavigateToCalendar()
    {
        Navigation.NavigateTo("/calendar");
    }

    private void NavigateToExercises()
    {
        Navigation.NavigateTo("/exercises");
    }

    private void NavigateToNewTemplate()
    {
        Navigation.NavigateTo("/templates/new");
    }
}
