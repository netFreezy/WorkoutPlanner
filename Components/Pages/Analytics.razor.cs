using ApexCharts;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorApp2.Components.Pages;

public partial class Analytics : ComponentBase
{
    [Inject] private AnalyticsService AnalyticsService { get; set; } = null!;
    [Inject] private PRDetectionService PRDetectionService { get; set; } = null!;

    private string activeTab = "overview";
    private int selectedWeeks = 4; // Default per D-14

    private DateTime RangeStart => selectedWeeks == 0 ? DateTime.MinValue : DateTime.UtcNow.Date.AddDays(-(selectedWeeks * 7));
    private DateTime RangeEnd => DateTime.UtcNow.Date.AddDays(1);

    // Overview data
    private KpiSummary? kpiSummary;
    private List<WeeklyVolume> weeklyVolume = new();
    private List<WeeklyAdherence> weeklyAdherence = new();

    // Strength data
    private List<WeeklyVolume> strengthVolume = new();
    private List<ExerciseWeeklyData> exerciseHistory = new();
    private List<(int Id, string Name)> strengthExercises = new();
    private int? selectedStrengthExerciseId;

    // Endurance data
    private List<WeeklyEndurance> weeklyEndurance = new();
    private List<EnduranceExerciseWeeklyData> enduranceExerciseHistory = new();
    private List<(int Id, string Name)> enduranceExercises = new();
    private int? selectedEnduranceExerciseId;

    // Chart options -- EACH chart gets its OWN instance
    private ApexChartOptions<WeeklyVolume> volumeChartOptions = new();
    private ApexChartOptions<WeeklyAdherence> adherenceChartOptions = new();
    private ApexChartOptions<WeeklyVolume> strengthVolumeChartOptions = new();
    private ApexChartOptions<ExerciseWeeklyData> maxWeightChartOptions = new();
    private ApexChartOptions<ExerciseWeeklyData> e1rmChartOptions = new();
    private ApexChartOptions<WeeklyEndurance> distanceChartOptions = new();
    private ApexChartOptions<WeeklyEndurance> paceChartOptions = new();
    private ApexChartOptions<EnduranceExerciseWeeklyData> endExDistanceChartOptions = new();
    private ApexChartOptions<EnduranceExerciseWeeklyData> endExPaceChartOptions = new();

    protected override async Task OnInitializedAsync()
    {
        InitChartOptions();
        await LoadOverviewDataAsync();
    }

    private void InitChartOptions()
    {
        // Configure each chart's options with dark theme
        // (each is its own instance -- never shared)
        ConfigureChartOptions(volumeChartOptions);
        ConfigureChartOptions(adherenceChartOptions);
        ConfigureChartOptions(strengthVolumeChartOptions);
        ConfigureChartOptions(maxWeightChartOptions);
        ConfigureChartOptions(e1rmChartOptions);
        ConfigureChartOptions(distanceChartOptions);
        ConfigureChartOptions(paceChartOptions);
        ConfigureChartOptions(endExDistanceChartOptions);
        ConfigureChartOptions(endExPaceChartOptions);
    }

    private static void ConfigureChartOptions<T>(ApexChartOptions<T> options) where T : class
    {
        options.Theme = new Theme { Mode = Mode.Dark };
        options.Chart = new Chart { Background = "transparent" };
        options.Tooltip = new Tooltip { Enabled = true };
    }

    private async Task SwitchTab(string tab) { activeTab = tab; await LoadTabDataAsync(); }
    private async Task ChangeTimeRange(int weeks) { selectedWeeks = weeks; await LoadTabDataAsync(); }

    private async Task LoadTabDataAsync()
    {
        switch (activeTab)
        {
            case "overview": await LoadOverviewDataAsync(); break;
            case "strength": await LoadStrengthDataAsync(); break;
            case "endurance": await LoadEnduranceDataAsync(); break;
            // "prs" loaded in Plan 04
        }
    }

    private async Task LoadOverviewDataAsync()
    {
        kpiSummary = await AnalyticsService.GetKpiSummaryAsync();
        weeklyVolume = await AnalyticsService.GetWeeklyVolumeAsync(RangeStart, RangeEnd);
        weeklyAdherence = await AnalyticsService.GetWeeklyAdherenceAsync(RangeStart, RangeEnd);
    }

    private async Task LoadStrengthDataAsync()
    {
        strengthExercises = await AnalyticsService.GetLoggedStrengthExercisesAsync();
        if (selectedStrengthExerciseId.HasValue)
        {
            exerciseHistory = await AnalyticsService.GetExerciseHistoryAsync(selectedStrengthExerciseId.Value, RangeStart, RangeEnd);
        }
        else
        {
            strengthVolume = await AnalyticsService.GetWeeklyVolumeAsync(RangeStart, RangeEnd);
        }
    }

    private async Task LoadEnduranceDataAsync()
    {
        enduranceExercises = await AnalyticsService.GetLoggedEnduranceExercisesAsync();
        if (selectedEnduranceExerciseId.HasValue)
        {
            enduranceExerciseHistory = await AnalyticsService.GetEnduranceExerciseHistoryAsync(selectedEnduranceExerciseId.Value, RangeStart, RangeEnd);
        }
        else
        {
            weeklyEndurance = await AnalyticsService.GetWeeklyEnduranceAsync(RangeStart, RangeEnd);
        }
    }

    private async Task ChangeStrengthExercise(ChangeEventArgs e)
    {
        selectedStrengthExerciseId = int.TryParse(e.Value?.ToString(), out var id) ? id : null;
        await LoadStrengthDataAsync();
    }

    private async Task ChangeEnduranceExercise(ChangeEventArgs e)
    {
        selectedEnduranceExerciseId = int.TryParse(e.Value?.ToString(), out var id) ? id : null;
        await LoadEnduranceDataAsync();
    }

    // Helper to check if volume data has any non-zero entries
    private bool HasVolumeData => weeklyVolume.Any(w => w.TotalSets > 0 || w.TotalVolume > 0);
    private bool HasAdherenceData => weeklyAdherence.Any(w => w.Planned > 0 || w.Completed > 0);
    private bool HasStrengthData => selectedStrengthExerciseId.HasValue
        ? exerciseHistory.Any(e => e.TotalSets > 0)
        : strengthVolume.Any(w => w.TotalSets > 0);
    private bool HasEnduranceData => selectedEnduranceExerciseId.HasValue
        ? enduranceExerciseHistory.Any(e => e.TotalDistance > 0)
        : weeklyEndurance.Any(w => w.TotalDistance > 0);

    private string TimeRangeLabel => selectedWeeks switch
    {
        4 => "4 weeks",
        8 => "8 weeks",
        12 => "12 weeks",
        _ => "all time"
    };
}
