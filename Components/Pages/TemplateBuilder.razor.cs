using BlazorApp2.Components.Shared;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;
using BlazorApp2.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;

namespace BlazorApp2.Components.Pages;

public partial class TemplateBuilder : IAsyncDisposable
{
    [Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;

    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<TemplateBuilder>? _dotNetRef;
    private bool _sortableInitialized;
    private int _lastItemCount;

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
    private bool HasWarmUp => State.Items.Any(i => i.SectionType == SectionType.WarmUp);
    private bool HasCoolDown => State.Items.Any(i => i.SectionType == SectionType.CoolDown);
    private bool CanUngroup => State.Items.Any(i => i.IsSelected && i.GroupLocalId != null);

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

    // --- Section and grouping rendering ---

    private record SectionEntry(BuilderGroup? Group, List<BuilderItem> Items);

    private List<SectionEntry> GetSectionEntries(SectionType sectionType)
    {
        var entries = new List<SectionEntry>();
        var sectionItems = State.Items
            .Where(i => i.SectionType == sectionType)
            .OrderBy(i => i.Position)
            .ToList();
        var renderedGroupIds = new HashSet<string>();

        foreach (var item in sectionItems)
        {
            if (item.GroupLocalId != null && !renderedGroupIds.Contains(item.GroupLocalId))
            {
                renderedGroupIds.Add(item.GroupLocalId);
                var group = State.Groups.FirstOrDefault(g => g.LocalId == item.GroupLocalId);
                var groupItems = sectionItems.Where(i => i.GroupLocalId == item.GroupLocalId).ToList();
                if (group != null)
                {
                    entries.Add(new SectionEntry(group, groupItems));
                }
            }
            else if (item.GroupLocalId == null)
            {
                entries.Add(new SectionEntry(null, new List<BuilderItem> { item }));
            }
        }

        return entries;
    }

    // --- Section management ---

    private List<SectionType> GetVisibleSections()
    {
        var sections = new List<SectionType>();
        if (State.Items.Any(i => i.SectionType == SectionType.WarmUp))
            sections.Add(SectionType.WarmUp);
        sections.Add(SectionType.Working);
        if (State.Items.Any(i => i.SectionType == SectionType.CoolDown))
            sections.Add(SectionType.CoolDown);
        return sections;
    }

    private async Task AddWarmUpSection()
    {
        var selected = State.Items.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0)
        {
            await toast.ShowAsync("Select exercises first, then click Add warm-up to move them.");
            return;
        }
        State.PushUndo();
        foreach (var item in selected)
        {
            item.SectionType = SectionType.WarmUp;
            item.IsSelected = false;
        }
    }

    private async Task AddCoolDownSection()
    {
        var selected = State.Items.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0)
        {
            await toast.ShowAsync("Select exercises first, then click Add cool-down to move them.");
            return;
        }
        State.PushUndo();
        foreach (var item in selected)
        {
            item.SectionType = SectionType.CoolDown;
            item.IsSelected = false;
        }
    }

    // --- Grouping ---

    private void GroupAsSuperset()
    {
        var selected = State.Items.Where(i => i.IsSelected).ToList();
        if (selected.Count < 2) return;
        State.PushUndo();
        var group = new BuilderGroup { GroupType = GroupType.Superset };
        State.Groups.Add(group);
        foreach (var item in selected)
        {
            item.GroupLocalId = group.LocalId;
            item.IsSelected = false;
        }
    }

    private void GroupAsEmom()
    {
        var selected = State.Items.Where(i => i.IsSelected).ToList();
        if (selected.Count < 2) return;
        State.PushUndo();
        var group = new BuilderGroup
        {
            GroupType = GroupType.EMOM,
            Rounds = 5,
            MinuteWindow = 1
        };
        State.Groups.Add(group);
        foreach (var item in selected)
        {
            item.GroupLocalId = group.LocalId;
            item.IsSelected = false;
        }
    }

    private void UngroupSelected()
    {
        var selected = State.Items.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0) return;
        State.PushUndo();
        var groupIdsToCheck = selected
            .Select(i => i.GroupLocalId)
            .Where(g => g != null)
            .Distinct()
            .ToList();
        foreach (var item in selected)
        {
            item.GroupLocalId = null;
            item.IsSelected = false;
        }
        // Remove groups that have no items left
        foreach (var groupId in groupIdsToCheck)
        {
            if (!State.Items.Any(i => i.GroupLocalId == groupId))
                State.Groups.RemoveAll(g => g.LocalId == groupId);
        }
    }

    private void OnGroupParamsChanged()
    {
        State.PushUndo();
    }

    // --- SortableJS lifecycle ---

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _jsModule = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/template-builder.js");
            _dotNetRef = DotNetObjectReference.Create(this);
        }

        // Initialize or refresh sortable when items exist
        if (State.Items.Count > 0 && _jsModule != null && _dotNetRef != null)
        {
            if (!_sortableInitialized || _lastItemCount != State.Items.Count)
            {
                await _jsModule.InvokeVoidAsync("initSortable", _dotNetRef, ".exercise-list-sortable");
                _sortableInitialized = true;
                _lastItemCount = State.Items.Count;
            }
        }
    }

    [JSInvokable]
    public void OnItemReordered(int oldIndex, int newIndex, string newSectionName)
    {
        if (oldIndex == newIndex && string.IsNullOrEmpty(newSectionName)) return;

        State.PushUndo();

        // Get the flat list of items in current display order (section-by-section,
        // matching the DOM order that Plan 04 renders: WarmUp items, Working items, CoolDown items)
        var orderedItems = State.Items
            .OrderBy(i => i.SectionType) // WarmUp=0, Working=1, CoolDown=2
            .ThenBy(i => i.Position)
            .ToList();

        if (oldIndex < 0 || oldIndex >= orderedItems.Count || newIndex < 0 || newIndex >= orderedItems.Count)
            return;

        // Move item in the flat list
        var item = orderedItems[oldIndex];
        orderedItems.RemoveAt(oldIndex);
        orderedItems.Insert(newIndex, item);

        // D-20: Update SectionType if the item was dragged across a section boundary.
        // The JS interop passes the section name determined by scanning backwards from
        // the drop position for the nearest section header's data-section attribute.
        if (!string.IsNullOrEmpty(newSectionName))
        {
            var newSection = newSectionName switch
            {
                "WarmUp" => SectionType.WarmUp,
                "CoolDown" => SectionType.CoolDown,
                _ => SectionType.Working
            };
            item.SectionType = newSection;
        }

        // Recompact positions per section (each section starts at 0)
        var sectionPositions = new Dictionary<SectionType, int>
        {
            { SectionType.WarmUp, 0 },
            { SectionType.Working, 0 },
            { SectionType.CoolDown, 0 }
        };
        foreach (var orderedItem in orderedItems)
        {
            orderedItem.Position = sectionPositions[orderedItem.SectionType]++;
        }

        // D-21: If cross-section drag emptied a WarmUp or CoolDown section,
        // it will automatically disappear because GetVisibleSections() only
        // shows sections that have items.

        StateHasChanged();
    }

    // --- Keyboard shortcuts ---

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.CtrlKey && e.Key == "z" && !e.ShiftKey)
        {
            Undo();
        }
        else if (e.CtrlKey && e.Key == "z" && e.ShiftKey)
        {
            Redo();
        }
        else if (e.CtrlKey && e.Key == "Z") // Shift+Z sends uppercase Z
        {
            Redo();
        }
    }

    // --- Dispose ---

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            try
            {
                await _jsModule.InvokeVoidAsync("destroySortable");
                await _jsModule.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
        }
        _dotNetRef?.Dispose();
    }
}
