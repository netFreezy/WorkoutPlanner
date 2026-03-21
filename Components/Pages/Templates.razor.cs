using BlazorApp2.Components.Shared;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Components.Pages;

public partial class Templates
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private List<WorkoutTemplate> allTemplates = new();
    private bool isLoading = true;
    private string searchText = "";
    private string? activeTagFilter;
    private WorkoutTemplate? selectedTemplate;
    private bool showDeleteConfirm;
    private Toast toast = null!;

    protected override async Task OnInitializedAsync()
    {
        await LoadTemplatesAsync();
    }

    private async Task LoadTemplatesAsync()
    {
        isLoading = true;
        using var context = DbFactory.CreateDbContext();
        allTemplates = await context.WorkoutTemplates
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Exercise)
            .Include(t => t.Groups)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();
        isLoading = false;
    }

    private IEnumerable<WorkoutTemplate> FilteredTemplates => allTemplates
        .Where(t => string.IsNullOrEmpty(searchText)
            || t.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
        .Where(t => string.IsNullOrEmpty(activeTagFilter)
            || t.Tags.Contains(activeTagFilter));

    private List<string> AllTags => [.. allTemplates
        .SelectMany(t => t.Tags)
        .Distinct()
        .OrderBy(t => t)];

    private bool HasActiveFilters =>
        !string.IsNullOrEmpty(searchText) ||
        !string.IsNullOrEmpty(activeTagFilter);

    private void ClearAllFilters()
    {
        searchText = "";
        activeTagFilter = null;
    }

    private void ShowDetail(WorkoutTemplate template)
    {
        selectedTemplate = template;
    }

    private void CloseDetail()
    {
        selectedTemplate = null;
    }

    private void NavigateToNew()
    {
        NavigationManager.NavigateTo("/templates/new");
    }

    private void NavigateToEdit(int id)
    {
        NavigationManager.NavigateTo($"/templates/{id}/edit");
    }

    private async Task DuplicateTemplate(WorkoutTemplate original)
    {
        // Close detail dialog first
        selectedTemplate = null;

        using var context = DbFactory.CreateDbContext();

        // Reload the original with includes
        var source = await context.WorkoutTemplates
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Exercise)
            .Include(t => t.Groups)
                .ThenInclude(g => g.Items)
            .FirstOrDefaultAsync(t => t.Id == original.Id);

        if (source is null) return;

        // Deep copy template
        var copy = new WorkoutTemplate
        {
            Name = $"{source.Name} (copy)",
            Description = source.Description,
            Tags = [.. source.Tags],
            CreatedDate = DateTime.UtcNow
        };

        // Deep copy groups with mapping from old to new
        var groupMap = new Dictionary<int, TemplateGroup>();
        foreach (var group in source.Groups)
        {
            var newGroup = new TemplateGroup
            {
                GroupType = group.GroupType,
                Rounds = group.Rounds,
                MinuteWindow = group.MinuteWindow
            };
            copy.Groups.Add(newGroup);
            groupMap[group.Id] = newGroup;
        }

        // Deep copy items with group mapping
        foreach (var item in source.Items.OrderBy(i => i.Position))
        {
            var newItem = new TemplateItem
            {
                ExerciseId = item.ExerciseId,
                Position = item.Position,
                SectionType = item.SectionType,
                TargetSets = item.TargetSets,
                TargetReps = item.TargetReps,
                TargetWeight = item.TargetWeight,
                TargetDistance = item.TargetDistance,
                TargetDurationSeconds = item.TargetDurationSeconds,
                TargetPace = item.TargetPace,
                TargetHeartRateZone = item.TargetHeartRateZone
            };

            // Map to new group if item was grouped
            if (item.TemplateGroupId.HasValue && groupMap.TryGetValue(item.TemplateGroupId.Value, out var newGroup))
            {
                newItem.TemplateGroup = newGroup;
            }

            copy.Items.Add(newItem);
        }

        context.WorkoutTemplates.Add(copy);
        await context.SaveChangesAsync();

        // Reload all templates
        await LoadTemplatesAsync();

        await toast.ShowAsync($"Template duplicated as \"{copy.Name}\"");
    }

    private void ConfirmDelete()
    {
        showDeleteConfirm = true;
    }

    private void CancelDelete()
    {
        showDeleteConfirm = false;
    }

    private async Task DeleteTemplate()
    {
        if (selectedTemplate is null) return;

        using var context = DbFactory.CreateDbContext();
        var template = await context.WorkoutTemplates
            .Include(t => t.Items)
            .Include(t => t.Groups)
            .FirstOrDefaultAsync(t => t.Id == selectedTemplate.Id);

        if (template is not null)
        {
            context.WorkoutTemplates.Remove(template);
            await context.SaveChangesAsync();
        }

        // Close dialogs
        showDeleteConfirm = false;
        selectedTemplate = null;

        // Reload
        await LoadTemplatesAsync();

        await toast.ShowAsync("Template deleted");
    }

    private string GetEstimatedDuration(WorkoutTemplate t)
    {
        if (!t.Items.Any()) return "";

        // Convert template items/groups to builder models for estimation
        var builderGroups = t.Groups.Select(g => new BuilderGroup
        {
            LocalId = g.Id.ToString(),
            GroupType = g.GroupType,
            Rounds = g.Rounds,
            MinuteWindow = g.MinuteWindow
        }).ToList();

        var builderItems = t.Items.Select(i => new BuilderItem
        {
            LocalId = i.Id.ToString(),
            ExerciseId = i.ExerciseId,
            ExerciseName = i.Exercise.Name,
            IsStrength = i.Exercise is StrengthExercise,
            Position = i.Position,
            SectionType = i.SectionType,
            GroupLocalId = i.TemplateGroupId?.ToString(),
            TargetSets = i.TargetSets,
            TargetReps = i.TargetReps,
            TargetWeight = i.TargetWeight,
            TargetDistance = i.TargetDistance,
            TargetDurationSeconds = i.TargetDurationSeconds,
            TargetPace = i.TargetPace,
            TargetHeartRateZone = i.TargetHeartRateZone
        }).ToList();

        var result = TemplateBuilderState.EstimateDurationMinutes(builderItems, builderGroups);
        return $"~{result} min";
    }
}
