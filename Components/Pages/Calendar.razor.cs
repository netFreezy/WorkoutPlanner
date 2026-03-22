using BlazorApp2.Components.Shared;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp2.Components.Pages;

public partial class Calendar : IDisposable
{
    [Inject] private SchedulingService SchedulingService { get; set; } = null!;
    [Inject] private MaterializationService MaterializationService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    [SupplyParameterFromQuery] private string? Toast { get; set; }

    private DateTime currentWeekStart;
    private List<ScheduledWorkout> weekWorkouts = new();
    private Dictionary<DateTime, List<ScheduledWorkout>> workoutsByDay = new();
    private bool isLoading = true;
    private bool showMonthView = false;
    private string weekAnimationClass = "";

    // Dialog state
    private bool showScheduleDialog = false;
    private DateTime? scheduleDialogDate = null;
    private ScheduledWorkout? selectedWorkout = null;
    private bool showDetailDialog = false;
    private ScheduledWorkout? editWorkout = null;
    private Toast? toast;
    private DateTime currentDisplayMonth;

    // Drag-to-reschedule
    private DotNetObjectReference<Calendar>? dotNetRef;

    private static readonly string[] DayNames = ["MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"];

    protected override async Task OnInitializedAsync()
    {
        currentWeekStart = MaterializationService.GetMondayOfWeek(DateTime.UtcNow);
        currentDisplayMonth = new DateTime(currentWeekStart.Year, currentWeekStart.Month, 1);
        await MaterializationService.MaterializeAllAsync();
        await LoadWeekData();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !string.IsNullOrEmpty(Toast) && toast != null)
        {
            await toast.ShowAsync(Uri.UnescapeDataString(Toast));
        }

        if (!isLoading && !showMonthView)
        {
            dotNetRef ??= DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("calendarDrag.init", dotNetRef);
        }
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
        editWorkout = null;
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
        editWorkout = null;
        scheduleDialogDate = DateTime.UtcNow.Date;
        showScheduleDialog = true;
    }

    private async Task HandleScheduled()
    {
        showScheduleDialog = false;
        editWorkout = null;
        await LoadWeekData();
        if (toast != null)
            await toast.ShowAsync("Workout scheduled");
    }

    private async Task HandleWorkoutChanged()
    {
        showDetailDialog = false;
        await LoadWeekData();
    }

    private async Task HandleMonthDateClicked(DateTime date)
    {
        showMonthView = false;
        currentWeekStart = MaterializationService.GetMondayOfWeek(date);
        await LoadWeekData();
    }

    private void OpenEditFromDetail(ScheduledWorkout workout)
    {
        showDetailDialog = false;
        editWorkout = workout;
        scheduleDialogDate = workout.ScheduledDate;
        showScheduleDialog = true;
    }

    private async Task HandleDragReschedule(int workoutId, DateTime newDate)
    {
        await SchedulingService.RescheduleWorkoutAsync(workoutId, newDate);
        await LoadWeekData();
        var dayName = newDate.ToString("dddd");
        if (toast != null)
            await toast.ShowAsync($"Workout moved to {dayName}");
    }

    [JSInvokable]
    public async Task OnWorkoutDropped(int workoutId, string newDateStr)
    {
        if (DateTime.TryParse(newDateStr, out var newDate))
        {
            await HandleDragReschedule(workoutId, newDate);
            StateHasChanged();
        }
    }

    public void Dispose()
    {
        dotNetRef?.Dispose();
    }
}
