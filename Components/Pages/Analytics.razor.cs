using ApexCharts;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorApp2.Components.Pages;

public partial class Analytics : ComponentBase
{
    [Inject] private AnalyticsService AnalyticsService { get; set; } = null!;
    [Inject] private PRDetectionService PRDetectionService { get; set; } = null!;
    [Inject] private ExportService ExportService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

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

    // PRs data
    private List<IGrouping<string, PersonalRecord>> prGroups = new();
    private List<PersonalRecord> prTimeline = new();
    private List<PersonalRecord> prTimelineStrength => prTimeline.Where(pr => pr.StrengthType.HasValue).ToList();
    private List<PersonalRecord> prTimelineEndurance => prTimeline.Where(pr => pr.EnduranceType.HasValue).ToList();

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
    private ApexChartOptions<PersonalRecord> prTimelineChartOptions = new();

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
        ConfigureChartOptions(prTimelineChartOptions);
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
            case "prs": await LoadPRsDataAsync(); break;
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

    private async Task LoadPRsDataAsync()
    {
        prGroups = await PRDetectionService.GetAllPRsAsync();
        prTimeline = await PRDetectionService.GetPRTimelineAsync(RangeStart, RangeEnd);
    }

    private bool HasPRData => prGroups.Any();

    private static string GetPRTypeLabel(PersonalRecord pr)
    {
        if (pr.StrengthType.HasValue)
        {
            return pr.StrengthType.Value switch
            {
                StrengthPRType.Weight => "Weight",
                StrengthPRType.Reps => "Reps",
                StrengthPRType.EstimatedOneRepMax => "Est. 1RM",
                _ => "PR"
            };
        }
        if (pr.EnduranceType.HasValue)
        {
            return pr.EnduranceType.Value switch
            {
                EndurancePRType.Pace => "Pace",
                EndurancePRType.Distance => "Distance",
                _ => "PR"
            };
        }
        return "PR";
    }

    private string TimeRangeLabel => selectedWeeks switch
    {
        4 => "4 weeks",
        8 => "8 weeks",
        12 => "12 weeks",
        _ => "all time"
    };

    // Export state
    private bool isExportingCsv = false;
    private bool isExportingPdf = false;

    private async Task ExportCsv()
    {
        isExportingCsv = true;
        StateHasChanged();
        try
        {
            // Per D-09, always attempt both strength and endurance downloads
            var strengthBytes = await ExportService.GenerateStrengthCsvAsync(RangeStart, RangeEnd);
            var enduranceBytes = await ExportService.GenerateEnduranceCsvAsync(RangeStart, RangeEnd);

            // Download strength CSV
            var strengthFileName = $"strength-data-{DateTime.UtcNow:yyyy-MM-dd}.csv";
            using var stream = new MemoryStream(strengthBytes);
            using var streamRef = new DotNetStreamReference(stream);
            await JS.InvokeVoidAsync("downloadFileFromStream", strengthFileName, streamRef);

            // Download endurance CSV only if it contains data rows (not just header)
            if (enduranceBytes.Length > 50)
            {
                var enduranceFileName = $"endurance-data-{DateTime.UtcNow:yyyy-MM-dd}.csv";
                using var endStream = new MemoryStream(enduranceBytes);
                using var endStreamRef = new DotNetStreamReference(endStream);
                await JS.InvokeVoidAsync("downloadFileFromStream", enduranceFileName, endStreamRef);
            }
        }
        catch (Exception)
        {
            // Silent catch for now; toast notification can be added later
        }
        finally
        {
            isExportingCsv = false;
            StateHasChanged();
        }
    }

    private async Task ExportPdf()
    {
        isExportingPdf = true;
        StateHasChanged();
        try
        {
            var pdfBytes = await ExportService.GenerateTrainingSummaryPdfAsync(RangeStart, RangeEnd);
            var fileName = $"training-summary-{DateTime.UtcNow:yyyy-MM-dd}.pdf";
            using var stream = new MemoryStream(pdfBytes);
            using var streamRef = new DotNetStreamReference(stream);
            await JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
        }
        catch (Exception)
        {
            // Silent catch for now; toast notification can be added later
        }
        finally
        {
            isExportingPdf = false;
            StateHasChanged();
        }
    }
}
