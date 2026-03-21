using BlazorApp2.Components.Shared;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Components.Pages;

public partial class Exercises
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;

    private List<Exercise> allExercises = new();
    private bool isLoading = true;
    private string searchText = "";
    private string selectedType = "";
    private MuscleGroup? selectedMuscleGroup;
    private Equipment? selectedEquipment;
    private Exercise? selectedExercise;

    protected override async Task OnInitializedAsync()
    {
        await LoadExercisesAsync();
    }

    private async Task LoadExercisesAsync()
    {
        isLoading = true;
        using var context = DbFactory.CreateDbContext();
        allExercises = await context.Exercises
            .OrderBy(e => e.Name)
            .ToListAsync();
        isLoading = false;
    }

    private IEnumerable<Exercise> FilteredExercises => allExercises
        .Where(e => string.IsNullOrEmpty(searchText)
            || e.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        .Where(e => string.IsNullOrEmpty(selectedType)
            || (selectedType == "Strength" && e is StrengthExercise)
            || (selectedType == "Endurance" && e is EnduranceExercise))
        .Where(e => selectedMuscleGroup == null
            || (e is StrengthExercise se && se.MuscleGroup == selectedMuscleGroup))
        .Where(e => selectedEquipment == null
            || (e is StrengthExercise se2 && se2.Equipment == selectedEquipment));

    private bool HasActiveFilters =>
        !string.IsNullOrEmpty(searchText) ||
        !string.IsNullOrEmpty(selectedType) ||
        selectedMuscleGroup.HasValue ||
        selectedEquipment.HasValue;

    private void ClearAllFilters()
    {
        searchText = "";
        selectedType = "";
        selectedMuscleGroup = null;
        selectedEquipment = null;
    }

    private void ShowDetail(Exercise exercise)
    {
        selectedExercise = exercise;
    }

    private void CloseDetail()
    {
        selectedExercise = null;
    }

    private bool showCreateDialog;
    private Toast toast = null!;

    private void OpenCreateDialog()
    {
        showCreateDialog = true;
    }

    private void CloseCreateDialog()
    {
        showCreateDialog = false;
    }

    private async Task HandleExerciseCreated(Exercise exercise)
    {
        // Save to database
        using var context = DbFactory.CreateDbContext();
        context.Exercises.Add(exercise);
        await context.SaveChangesAsync();

        // Close dialog
        showCreateDialog = false;

        // Reload exercises to include the new one
        await LoadExercisesAsync();

        // Clear filters so the new exercise is visible (per RESEARCH.md Pitfall 5)
        ClearAllFilters();

        // Show success toast
        await toast.ShowAsync($"{exercise.Name} added to library");
    }

    private static string FormatEnumName<T>(T value) where T : Enum
        => System.Text.RegularExpressions.Regex.Replace(value.ToString(), "(?<!^)([A-Z])", " $1");
}
