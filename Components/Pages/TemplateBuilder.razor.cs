using BlazorApp2.Components.Shared;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Components.Pages;

public partial class TemplateBuilder
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public int? Id { get; set; }

    private TemplateBuilderState State = new();
    private bool isEditing;
    private int? templateId;
    private bool showExercisePicker;
    private bool showDiscardDialog;
    private bool showDescription;
    private string? pendingNavigationUri;
    private Toast toast = null!;
    private string _lastSavedName = "";

    private int SelectedCount => State.Items.Count(i => i.IsSelected);
    private bool HasSelection => SelectedCount > 0;

    protected override async Task OnInitializedAsync()
    {
        if (Id.HasValue)
        {
            await LoadTemplateAsync(Id.Value);
        }
    }

    private async Task LoadTemplateAsync(int id)
    {
        using var context = DbFactory.CreateDbContext();
        var template = await context.WorkoutTemplates
            .Include(t => t.Items)
                .ThenInclude(ti => ti.Exercise)
            .Include(t => t.Groups)
                .ThenInclude(g => g.Items)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template is null)
        {
            NavigationManager.NavigateTo("/templates");
            return;
        }

        isEditing = true;
        templateId = id;

        State.Name = template.Name;
        State.Description = template.Description;
        State.Tags = new List<string>(template.Tags);

        // Map groups first so items can reference them
        var groupMap = new Dictionary<int, string>(); // entity ID -> local ID
        foreach (var g in template.Groups)
        {
            var bg = new BuilderGroup
            {
                GroupType = g.GroupType,
                Rounds = g.Rounds,
                MinuteWindow = g.MinuteWindow
            };
            State.Groups.Add(bg);
            groupMap[g.Id] = bg.LocalId;
        }

        foreach (var item in template.Items.OrderBy(i => i.Position))
        {
            State.Items.Add(new BuilderItem
            {
                ExerciseId = item.ExerciseId,
                ExerciseName = item.Exercise.Name,
                IsStrength = item.Exercise is StrengthExercise,
                Position = item.Position,
                SectionType = item.SectionType,
                GroupLocalId = item.TemplateGroupId.HasValue && groupMap.ContainsKey(item.TemplateGroupId.Value)
                    ? groupMap[item.TemplateGroupId.Value]
                    : null,
                TargetSets = item.TargetSets,
                TargetReps = item.TargetReps,
                TargetWeight = item.TargetWeight,
                TargetDistance = item.TargetDistance,
                TargetDurationSeconds = item.TargetDurationSeconds,
                TargetPace = item.TargetPace,
                TargetHeartRateZone = item.TargetHeartRateZone
            });
        }

        showDescription = !string.IsNullOrEmpty(State.Description);
        _lastSavedName = State.Name;
        State.ResetChangeTracking();
    }

    private void OnNameChanged()
    {
        if (State.Name != _lastSavedName)
        {
            State.PushUndo();
            _lastSavedName = State.Name;
        }
    }

    private void OnTagsChanged(List<string> tags)
    {
        State.PushUndo();
        State.Tags = tags;
    }

    private void OnTargetChanged(BuilderItem item)
    {
        State.PushUndo();
    }

    private void OnSelectionChanged()
    {
        StateHasChanged();
    }

    private void AddSelectedExercises(List<Exercise> exercises)
    {
        State.PushUndo();
        int nextPosition = State.Items.Count > 0 ? State.Items.Max(i => i.Position) + 1 : 0;
        foreach (var ex in exercises)
        {
            State.Items.Add(new BuilderItem
            {
                ExerciseId = ex.Id,
                ExerciseName = ex.Name,
                IsStrength = ex is StrengthExercise,
                Position = nextPosition++,
                SectionType = SectionType.Working
            });
        }
        showExercisePicker = false;
    }

    private void RemoveExercise(BuilderItem item)
    {
        State.PushUndo();
        State.Items.Remove(item);
        // Recompact positions
        var ordered = State.Items.OrderBy(i => i.Position).ToList();
        for (int i = 0; i < ordered.Count; i++)
            ordered[i].Position = i;
    }

    private async Task SaveTemplate()
    {
        if (string.IsNullOrWhiteSpace(State.Name))
        {
            await toast.ShowAsync("Template name is required");
            return;
        }

        using var context = DbFactory.CreateDbContext();

        WorkoutTemplate template;
        if (isEditing && templateId.HasValue)
        {
            template = await context.WorkoutTemplates
                .Include(t => t.Items)
                .Include(t => t.Groups)
                .FirstAsync(t => t.Id == templateId.Value);

            // Remove old items and groups
            context.TemplateItems.RemoveRange(template.Items);
            context.TemplateGroups.RemoveRange(template.Groups);

            template.Name = State.Name;
            template.Description = State.Description;
            template.Tags = new List<string>(State.Tags);
        }
        else
        {
            template = new WorkoutTemplate
            {
                Name = State.Name,
                Description = State.Description,
                Tags = new List<string>(State.Tags),
                CreatedDate = DateTime.UtcNow
            };
            context.WorkoutTemplates.Add(template);
        }

        // Rebuild groups with mapping from local ID to entity
        var groupMap = new Dictionary<string, TemplateGroup>();
        foreach (var bg in State.Groups)
        {
            var tg = new TemplateGroup
            {
                GroupType = bg.GroupType,
                Rounds = bg.Rounds,
                MinuteWindow = bg.MinuteWindow
            };
            template.Groups.Add(tg);
            groupMap[bg.LocalId] = tg;
        }

        // Rebuild items
        foreach (var bi in State.Items.OrderBy(i => i.Position))
        {
            var ti = new TemplateItem
            {
                ExerciseId = bi.ExerciseId,
                Position = bi.Position,
                SectionType = bi.SectionType,
                TargetSets = bi.TargetSets,
                TargetReps = bi.TargetReps,
                TargetWeight = bi.TargetWeight,
                TargetDistance = bi.TargetDistance,
                TargetDurationSeconds = bi.TargetDurationSeconds,
                TargetPace = bi.TargetPace,
                TargetHeartRateZone = bi.TargetHeartRateZone
            };

            if (bi.GroupLocalId is not null && groupMap.TryGetValue(bi.GroupLocalId, out var group))
            {
                ti.TemplateGroup = group;
            }

            template.Items.Add(ti);
        }

        await context.SaveChangesAsync();

        State.ResetChangeTracking();
        _lastSavedName = State.Name;

        if (!isEditing)
        {
            isEditing = true;
            templateId = template.Id;
            NavigationManager.NavigateTo($"/templates/{template.Id}/edit", replace: true);
        }

        await toast.ShowAsync("Template saved");
    }

    private void Discard()
    {
        if (State.HasUnsavedChanges)
        {
            showDiscardDialog = true;
        }
        else
        {
            NavigationManager.NavigateTo("/templates");
        }
    }

    private void OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (State.HasUnsavedChanges)
        {
            context.PreventNavigation();
            pendingNavigationUri = context.TargetLocation;
            showDiscardDialog = true;
        }
    }

    private void ConfirmDiscard()
    {
        showDiscardDialog = false;
        State.ResetChangeTracking();
        NavigationManager.NavigateTo(pendingNavigationUri ?? "/templates");
    }

    private void CancelDiscard()
    {
        showDiscardDialog = false;
        pendingNavigationUri = null;
    }

    private void Undo()
    {
        State.Undo();
        StateHasChanged();
    }

    private void Redo()
    {
        State.Redo();
        StateHasChanged();
    }

    private void OpenExercisePicker()
    {
        showExercisePicker = true;
    }

    private void CloseExercisePicker()
    {
        showExercisePicker = false;
    }

    // Stub callbacks for Plan 04/05 features (grouping, sections)
    private void NoOp() { }
}
