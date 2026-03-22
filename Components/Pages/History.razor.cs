using BlazorApp2.Services;
using Microsoft.AspNetCore.Components;

namespace BlazorApp2.Components.Pages;

public partial class History : IDisposable
{
    [Inject] private HistoryService HistoryService { get; set; } = null!;

    private List<HistorySession> sessions = new();
    private List<(int Id, string Name)> loggedExercises = new();
    private HashSet<int> expandedCards = new();
    private string searchText = "";
    private bool filterStrength = false;
    private bool filterEndurance = false;
    private bool filterMixed = false;
    private DateTime? dateStart;
    private DateTime? dateEnd;
    private int? selectedExerciseId;
    private int totalCount = 0;
    private int loadedCount = 0;
    private bool isLoading = true;
    private bool isLoadingMore = false;
    private const int PageSize = 20;
    private System.Timers.Timer? _debounceTimer;

    protected override async Task OnInitializedAsync()
    {
        sessions = await HistoryService.GetCompletedSessionsAsync(null, null, null, 0, PageSize);
        loggedExercises = await HistoryService.GetLoggedExercisesAsync();
        totalCount = await HistoryService.GetTotalCountAsync(null, null, null);
        loadedCount = sessions.Count;
        isLoading = false;
    }

    private async Task LoadMore()
    {
        isLoadingMore = true;
        var more = await HistoryService.GetCompletedSessionsAsync(dateStart, dateEnd, selectedExerciseId, loadedCount, PageSize);
        sessions.AddRange(more);
        loadedCount = sessions.Count;
        totalCount = await HistoryService.GetTotalCountAsync(dateStart, dateEnd, selectedExerciseId);
        isLoadingMore = false;
    }

    private async Task ApplyFilters()
    {
        isLoading = true;
        sessions = await HistoryService.GetCompletedSessionsAsync(dateStart, dateEnd, selectedExerciseId, 0, PageSize);
        totalCount = await HistoryService.GetTotalCountAsync(dateStart, dateEnd, selectedExerciseId);
        loadedCount = sessions.Count;
        isLoading = false;
    }

    private void OnSearchInput(ChangeEventArgs e)
    {
        searchText = e.Value?.ToString() ?? "";
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
        _debounceTimer = new System.Timers.Timer(300);
        _debounceTimer.AutoReset = false;
        _debounceTimer.Elapsed += (_, _) =>
        {
            InvokeAsync(StateHasChanged);
        };
        _debounceTimer.Start();
    }

    private void ToggleStrength()
    {
        filterStrength = !filterStrength;
    }

    private void ToggleEndurance()
    {
        filterEndurance = !filterEndurance;
    }

    private void ToggleMixed()
    {
        filterMixed = !filterMixed;
    }

    private async Task OnDateRangeChanged()
    {
        await ApplyFilters();
    }

    private async Task OnExerciseFilterChanged(ChangeEventArgs e)
    {
        var val = e.Value?.ToString();
        selectedExerciseId = string.IsNullOrEmpty(val) ? null : int.Parse(val);
        await ApplyFilters();
    }

    private void ToggleCard(int workoutLogId)
    {
        if (!expandedCards.Remove(workoutLogId))
        {
            expandedCards.Add(workoutLogId);
        }
    }

    private bool IsExpanded(int workoutLogId) => expandedCards.Contains(workoutLogId);

    private List<HistorySession> FilteredSessionsList => GetFilteredSessions().ToList();

    private bool HasAnyFilter =>
        searchText.Length > 0 || filterStrength || filterEndurance || filterMixed ||
        dateStart.HasValue || dateEnd.HasValue || selectedExerciseId.HasValue;

    private IEnumerable<HistorySession> GetFilteredSessions()
    {
        var filtered = sessions.AsEnumerable();

        // Client-side text search
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered.Where(s =>
                s.WorkoutName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                s.Exercises.Any(ex => ex.ExerciseName.Contains(searchText, StringComparison.OrdinalIgnoreCase)));
        }

        // Client-side type chip filtering
        var anyTypeActive = filterStrength || filterEndurance || filterMixed;
        if (anyTypeActive)
        {
            filtered = filtered.Where(s =>
                (filterStrength && s.WorkoutType == "Strength") ||
                (filterEndurance && s.WorkoutType == "Endurance") ||
                (filterMixed && s.WorkoutType == "Mixed"));
        }

        return filtered;
    }

    private int GetRemainingCount() => Math.Max(0, totalCount - loadedCount);

    private async Task ClearFilters()
    {
        searchText = "";
        filterStrength = false;
        filterEndurance = false;
        filterMixed = false;
        dateStart = null;
        dateEnd = null;
        selectedExerciseId = null;

        sessions = await HistoryService.GetCompletedSessionsAsync(null, null, null, 0, PageSize);
        totalCount = await HistoryService.GetTotalCountAsync(null, null, null);
        loadedCount = sessions.Count;
    }

    public void Dispose()
    {
        _debounceTimer?.Stop();
        _debounceTimer?.Dispose();
    }
}
