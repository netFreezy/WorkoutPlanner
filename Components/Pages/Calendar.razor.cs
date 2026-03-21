using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorApp2.Components.Pages;

public partial class Calendar : IDisposable
{
    [Inject] private SchedulingService SchedulingService { get; set; } = null!;
    [Inject] private MaterializationService MaterializationService { get; set; } = null!;

    private DateTime currentWeekStart;
    private List<ScheduledWorkout> weekWorkouts = new();
    private Dictionary<DateTime, List<ScheduledWorkout>> workoutsByDay = new();
    private bool isLoading = true;
    private bool showMonthView = false;
    private string weekAnimationClass = "";

    // Dialog state (wired in Plan 03)
    private bool showScheduleDialog = false;
    private DateTime? scheduleDialogDate = null;
    private ScheduledWorkout? selectedWorkout = null;
    private bool showDetailDialog = false;

    private static readonly string[] DayNames = ["MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"];

    protected override async Task OnInitializedAsync()
    {
        currentWeekStart = MaterializationService.GetMondayOfWeek(DateTime.UtcNow);
        await MaterializationService.MaterializeAllAsync();
        await LoadWeekData();
    }

    private async Task LoadWeekData()
    {
        isLoading = true;
        StateHasChanged();
        weekWorkouts = await SchedulingService.GetWorkoutsForWeekAsync(currentWeekStart);
        workoutsByDay = weekWorkouts
            .GroupBy(sw => sw.ScheduledDate.Date)
            .ToDictionary(g => g.Key, g => g.ToList());
        isLoading = false;
    }

    private async Task NavigateWeek(int direction)
    {
        weekAnimationClass = direction > 0 ? "week-animate-left" : "week-animate-right";
        currentWeekStart = currentWeekStart.AddDays(7 * direction);
        await LoadWeekData();
    }

    private async Task GoToToday()
    {
        weekAnimationClass = "";
        currentWeekStart = MaterializationService.GetMondayOfWeek(DateTime.UtcNow);
        await LoadWeekData();
    }

    private string GetWeekLabel()
    {
        var end = currentWeekStart.AddDays(6);
        if (currentWeekStart.Month == end.Month)
            return $"{currentWeekStart:MMM d} \u2013 {end:d}, {end:yyyy}";
        if (currentWeekStart.Year == end.Year)
            return $"{currentWeekStart:MMM d} \u2013 {end:MMM d}, {end:yyyy}";
        return $"{currentWeekStart:MMM d, yyyy} \u2013 {end:MMM d, yyyy}";
    }

    private List<ScheduledWorkout> GetWorkoutsForDay(DateTime date) =>
        workoutsByDay.TryGetValue(date.Date, out var list) ? list : new();

    private bool IsToday(DateTime date) => date.Date == DateTime.UtcNow.Date;

    private string GetMobileDayLabel(DateTime date) => $"{date:dddd, MMM d}";

    private void OpenScheduleDialog(DateTime date)
    {
        scheduleDialogDate = date;
        showScheduleDialog = true;
    }

    private void OpenDetailDialog(ScheduledWorkout workout)
    {
        selectedWorkout = workout;
        showDetailDialog = true;
    }

    private void OpenFabScheduleDialog()
    {
        scheduleDialogDate = DateTime.UtcNow.Date;
        showScheduleDialog = true;
    }

    public void Dispose() { }
}
