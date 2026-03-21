# Phase 3: Workout Templates - Research

**Researched:** 2026-03-21
**Domain:** Blazor Server interactive builder UI with drag-and-drop, undo/redo, grouping constructs, and EF Core CRUD
**Confidence:** HIGH

## Summary

Phase 3 builds a full template management system: a list page with tag filtering, a read-only detail dialog, and a full-page template builder with exercise picker, inline target editing, superset/EMOM grouping, warm-up/cool-down sections, drag-and-drop reordering, and undo/redo. The data model (WorkoutTemplate, TemplateItem, TemplateGroup) is already complete from Phase 1, along with all enums (SectionType, GroupType). The primary engineering challenges are: (1) drag-and-drop via JS interop with SortableJS, (2) undo/redo state management with a command/memento pattern, (3) tag storage requiring a schema addition (WorkoutTemplate currently has no Tags property), and (4) managing complex in-memory builder state that only persists on explicit Save.

The UI-SPEC (03-UI-SPEC.md) provides detailed visual contracts for all 12 components. Phase 2 established reusable patterns (Dialog, FilterChip, Toast, ExerciseCard, FAB, filter bar, card grid) that Phase 3 extends. All CSS design tokens exist in app.css; Phase 3 adds section/group/drag color tokens and new keyframe animations per the UI-SPEC.

**Primary recommendation:** Build the template list page first (reusing Phase 2 patterns), then the builder page incrementally -- exercise rows with targets, then sections, then grouping, then drag-and-drop, then undo/redo. Use raw SortableJS via custom JS interop module (no NuGet wrapper) and a memento-based undo/redo stack in C#.

<user_constraints>
## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Summary cards showing template name, first 3-4 exercise names as preview, total exercise count, and estimated duration
- **D-02:** Freeform tags on templates -- user types any tag when creating/editing (not a predefined set)
- **D-03:** Template list filterable by tag -- click a tag chip to filter, consistent with exercise library filter pattern
- **D-04:** FAB button to create new template (consistent with exercise library pattern)
- **D-05:** Clicking a template card opens a read-only detail view showing the full workout structure (ordered exercises with targets, grouped exercises with connectors, section headers), with an "Edit" button to enter the builder
- **D-06:** Duplicate template action available ("Copy as new") to create variations without rebuilding
- **D-07:** Full-page editor -- navigate away from the template list into a dedicated builder page
- **D-08:** Add exercises via a browse dialog -- opens exercise library in a picker dialog with search + filter, supports selecting multiple exercises at once (checkboxes, then "Add selected")
- **D-09:** Inline target editing -- strength exercises show sets/reps/weight fields directly on the row; endurance exercises show distance/duration/pace/HR zone fields on the row
- **D-10:** Explicit "Save" button with "Discard changes" option -- no auto-save
- **D-11:** Undo/redo capability in the builder
- **D-12:** Drag-and-drop reordering of exercises (JS interop acceptable for this)
- **D-13:** Select-then-group: user selects 2+ exercises already in the template, then clicks "Group as superset" or "Group as EMOM"
- **D-14:** EMOM follows same select-then-group flow, but after grouping prompts for rounds and minute window inline on the group header
- **D-15:** Grouped exercises connected by a vertical bracket/connector on the left side with a label ("Superset" or "EMOM 5x2min")
- **D-16:** Ungroup via remove-from-group (detach individual exercises) or dissolve group (ungroup all back to standalone)
- **D-17:** Visual section headers dividing the exercise list into warm-up / working / cool-down zones
- **D-18:** Warm-up and cool-down sections are optional -- builder starts with just "Working"; toolbar buttons appear when those sections don't exist
- **D-19:** New exercises default to the "Working" section
- **D-20:** Drag exercises between sections to reassign
- **D-21:** Empty sections auto-remove -- if all exercises are removed from warm-up or cool-down, the section header disappears

### Claude's Discretion
- Full-page builder layout details (toolbar placement, spacing)
- Exercise picker dialog sizing and multi-select UX
- Undo/redo implementation strategy (command pattern, state snapshots, etc.)
- Drag-and-drop JS interop library choice or custom implementation
- Bracket/connector visual styling (color, width, label positioning)
- Estimated duration calculation heuristic
- Tag input component styling and behavior
- Responsive breakpoints for the builder on mobile

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope.
</user_constraints>

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| TMPL-01 | Template builder -- create named templates with ordered exercise list | Builder page with name input, exercise picker dialog, Position-based ordering. Data model fully supports this via WorkoutTemplate + TemplateItem entities. |
| TMPL-02 | Reorderable exercises within templates (drag-and-drop or move up/down) | SortableJS via custom JS interop module for drag-and-drop. Position field on TemplateItem for ordering. Mobile fallback with move up/down buttons. |
| TMPL-03 | Strength targets per exercise: target sets, reps, weight | TemplateItem already has TargetSets, TargetReps, TargetWeight nullable fields. Inline compact number inputs per UI-SPEC. |
| TMPL-04 | Endurance targets per exercise: target distance, duration, pace, HR zone | TemplateItem already has TargetDistance, TargetDurationSeconds, TargetPace, TargetHeartRateZone nullable fields. Inline compact inputs per UI-SPEC. |
| TMPL-05 | Superset grouping -- visually group 2+ exercises with connector | TemplateGroup entity with GroupType.Superset. Visual bracket connector with CSS absolute positioning. Select-then-group interaction. |
| TMPL-06 | EMOM grouping -- N exercises, M-minute window per round, R rounds | TemplateGroup with GroupType.EMOM, Rounds, MinuteWindow fields. Same select-then-group flow plus inline EMOM parameter editing. |
| TMPL-07 | Warm-up and cool-down sections separate from working sets | SectionType enum (WarmUp, Working, CoolDown) on TemplateItem. Visual section headers with colored left borders. Auto-remove empty optional sections. |
</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor Server (InteractiveServer) | .NET 10.0 | Component framework | Already in use; all Phase 3 pages use `@rendermode InteractiveServer` |
| EF Core with SQLite | 10.0.5 | Data persistence | Already configured with IDbContextFactory pattern |
| SortableJS | 1.15.7 | Drag-and-drop reordering | De facto standard for sortable lists; endorsed by Microsoft's Blazor team for Blazor interop; no jQuery dependency; touch support built-in |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| IJSRuntime (built-in) | .NET 10.0 | JS interop for SortableJS | Initializing SortableJS on exercise list container after render |
| NavigationLock (built-in) | .NET 10.0 | Prevent navigation with unsaved changes | Template builder page to warn before discarding edits |
| DataAnnotationsValidator (built-in) | .NET 10.0 | Form validation | Template name required validation |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Raw SortableJS interop | BlazorSortable NuGet | NuGet wrapper adds dependency and limits customization; raw interop is simple for this use case (single list with section boundaries) and keeps the project's "no JS frameworks" constraint satisfied since SortableJS is a standalone library, not a framework |
| Memento-based undo/redo | Fluxor/Redux state store | Overkill for a single-page builder; memento pattern with a stack of state snapshots is simpler and contained within the builder component |
| JSON column for tags | Separate Tag entity with join table | JSON string is simpler for freeform tags on SQLite; no need for tag management UI or tag reuse across templates; querying by tag is done client-side (load all templates, filter in memory -- same pattern as exercises) |

**Installation:**
No NuGet packages needed beyond what is already installed. SortableJS is loaded via CDN script tag or local copy in wwwroot.

**SortableJS delivery:** Download `Sortable.min.js` v1.15.7 to `wwwroot/js/sortable.min.js` (local copy avoids CDN dependency, consistent with the "no cloud dependencies" constraint). Reference it in App.razor before the Blazor script tag.

## Architecture Patterns

### Recommended Project Structure
```
Components/
  Pages/
    Templates.razor            # Template list page (@page "/templates")
    Templates.razor.cs         # Code-behind for list page
    Templates.razor.css        # Scoped styles for list page
    TemplateBuilder.razor      # Builder page (@page "/templates/new", "/templates/{Id:int}/edit")
    TemplateBuilder.razor.cs   # Code-behind for builder page
    TemplateBuilder.razor.css  # Scoped styles for builder page
  Shared/
    TemplateCard.razor         # Summary card for template list
    TemplateCard.razor.css
    TemplateDetailDialog.razor # Read-only detail view in dialog
    TemplateDetailDialog.razor.css
    ExercisePickerDialog.razor # Multi-select exercise browser
    ExercisePickerDialog.razor.css
    ExerciseRow.razor          # Single exercise row in builder
    ExerciseRow.razor.css
    BuilderToolbar.razor       # Sticky toolbar for builder actions
    BuilderToolbar.razor.css
    SectionHeader.razor        # Section divider (warm-up/working/cool-down)
    SectionHeader.razor.css
    GroupBracket.razor         # Visual bracket connector for groups
    GroupBracket.razor.css
    TagInput.razor             # Freeform tag input component
    TagInput.razor.css
    DeleteConfirmationDialog.razor  # Reusable delete confirmation
    DeleteConfirmationDialog.razor.css
Models/
  TemplateBuilderState.cs      # In-memory builder state (items, groups, undo stack)
  TemplateFormModel.cs         # View model for template name/description validation
wwwroot/
  js/
    sortable.min.js            # SortableJS v1.15.7 (local copy)
    template-builder.js        # Custom JS interop module for drag-and-drop
```

### Pattern 1: In-Memory Builder State with Explicit Save
**What:** The template builder maintains all state in memory (C# objects). Changes to exercise order, targets, groups, and sections are NOT persisted until the user clicks "Save". This decouples the UI editing experience from database round-trips.
**When to use:** Any time the builder page modifies template structure.
**Example:**
```csharp
// TemplateBuilderState holds the working copy
public class TemplateBuilderState
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<BuilderItem> Items { get; set; } = new();
    public List<BuilderGroup> Groups { get; set; } = new();

    // Undo/redo stacks
    private Stack<TemplateSnapshot> _undoStack = new();
    private Stack<TemplateSnapshot> _redoStack = new();

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public bool HasUnsavedChanges { get; private set; }

    public void PushUndo()
    {
        _undoStack.Push(TakeSnapshot());
        _redoStack.Clear(); // New action invalidates redo stack
        HasUnsavedChanges = true;
    }

    public void Undo() { /* restore from _undoStack, push current to _redoStack */ }
    public void Redo() { /* restore from _redoStack, push current to _undoStack */ }
}
```

### Pattern 2: SortableJS Interop Module
**What:** A standalone JS module in wwwroot/js/template-builder.js that initializes SortableJS on the exercise list container and calls back into .NET via DotNetObjectReference when items are reordered or moved between sections.
**When to use:** After the builder component renders its exercise list.
**Example:**
```javascript
// wwwroot/js/template-builder.js
export function initSortable(dotNetRef, containerSelector) {
    const el = document.querySelector(containerSelector);
    if (!el) return;

    const sortable = new Sortable(el, {
        handle: '.drag-handle',
        animation: 150,
        ghostClass: 'drag-placeholder',
        onEnd: function (evt) {
            // Cancel the DOM move -- let Blazor re-render
            evt.item.remove();
            evt.from.insertBefore(evt.item, evt.from.children[evt.oldIndex]);
            // Notify .NET of the reorder
            dotNetRef.invokeMethodAsync('OnItemReordered', evt.oldIndex, evt.newIndex);
        }
    });

    return sortable;
}

export function destroySortable(sortableInstance) {
    if (sortableInstance) sortableInstance.destroy();
}
```

```csharp
// In TemplateBuilder.razor.cs
[Inject] private IJSRuntime JS { get; set; } = null!;
private IJSObjectReference? _jsModule;
private DotNetObjectReference<TemplateBuilder>? _dotNetRef;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        _jsModule = await JS.InvokeAsync<IJSObjectReference>(
            "import", "./js/template-builder.js");
        _dotNetRef = DotNetObjectReference.Create(this);
        await _jsModule.InvokeVoidAsync("initSortable", _dotNetRef, ".exercise-list");
    }
}

[JSInvokable]
public void OnItemReordered(int oldIndex, int newIndex)
{
    State.PushUndo();
    // Move item in the in-memory list
    var item = State.Items[oldIndex];
    State.Items.RemoveAt(oldIndex);
    State.Items.Insert(newIndex, item);
    // Recalculate positions
    for (int i = 0; i < State.Items.Count; i++)
        State.Items[i].Position = i;
    StateHasChanged();
}
```

### Pattern 3: Tag Storage via JSON String Column
**What:** Store tags as a JSON array string on WorkoutTemplate (e.g., `["Push","Upper Body"]`). Use a value converter to map between `List<string>` in C# and a JSON string in SQLite.
**When to use:** WorkoutTemplate entity needs a Tags property added, plus EF Core configuration for the value converter.
**Example:**
```csharp
// Add to WorkoutTemplate entity
public List<string> Tags { get; set; } = new();

// Add to WorkoutTemplateConfiguration
builder.Property(t => t.Tags)
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
    .HasColumnType("TEXT");
```

### Pattern 4: NavigationLock for Unsaved Changes
**What:** Use Blazor's built-in NavigationLock component to prevent navigation away from the builder when there are unsaved changes.
**When to use:** Template builder page.
**Example:**
```razor
<NavigationLock OnBeforeInternalNavigation="OnBeforeInternalNavigation"
                ConfirmExternalNavigation="@State.HasUnsavedChanges" />

@code {
    private async Task OnBeforeInternalNavigation(LocationChangingContext context)
    {
        if (State.HasUnsavedChanges)
        {
            showDiscardDialog = true;
            StateHasChanged();
            context.PreventNavigation();
        }
    }
}
```

### Anti-Patterns to Avoid
- **Auto-saving on every change:** Decision D-10 explicitly requires explicit Save. Do not persist on blur or change events.
- **Direct EF entity mutation in the builder:** Use a separate in-memory model (BuilderItem/BuilderGroup) that gets mapped to/from entities on load/save. Mutating tracked entities leads to partial saves and state corruption.
- **DOM-based reordering:** SortableJS naturally moves DOM elements, but Blazor will fight this. Always cancel the SortableJS DOM move and let Blazor re-render from the C# state.
- **Scoped CSS without ::deep for child components:** Blazor's InputText/InputSelect render their own elements outside the parent's CSS isolation scope. Always use `::deep` when targeting these from parent .razor.css (per project memory note).
- **Large undo stack:** Cap the undo stack at ~50 entries to avoid memory bloat during long editing sessions.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Drag-and-drop reordering | Custom HTML5 drag API interop | SortableJS 1.15.7 | Touch support, cross-browser compat, animation, ghost elements -- dozens of edge cases solved |
| Preventing navigation on unsaved changes | Custom beforeunload JS interop | Blazor NavigationLock component | Built-in, handles both internal Blazor navigation and external browser navigation |
| Form validation | Manual if-checks | EditForm + DataAnnotationsValidator | Established pattern from Phase 2; consistent error display |
| JSON serialization for tags | Manual string parsing | System.Text.Json with EF Core value converter | Handles edge cases (escaping, null, empty arrays) correctly |

**Key insight:** The complexity in this phase is state management (undo/redo, unsaved change tracking, section/group membership), not infrastructure. The libraries and framework features handle the infrastructure; the custom code manages the builder's domain logic.

## Common Pitfalls

### Pitfall 1: SortableJS DOM Conflict with Blazor Re-render
**What goes wrong:** SortableJS moves DOM elements on drag, but Blazor also owns the DOM. After a drag, Blazor's diff sees unexpected DOM state and either duplicates or loses elements.
**Why it happens:** Two systems (SortableJS and Blazor) both trying to manage the same DOM subtree.
**How to avoid:** In the SortableJS `onEnd` callback, immediately revert the DOM move (put the element back where it was), then call into .NET to update the C# list, which triggers Blazor to re-render correctly.
**Warning signs:** Duplicated exercise rows, missing rows, or rows appearing in wrong positions after drag.

### Pitfall 2: Blazor Scoped CSS and ::deep
**What goes wrong:** Styles applied to Blazor input components (InputText, InputSelect, InputNumber) from parent scoped CSS silently fail.
**Why it happens:** Blazor's CSS isolation adds a scope attribute to the parent component's elements, but child Blazor components render their own HTML without that attribute.
**How to avoid:** Always use `::deep` selector prefix when targeting classes on Blazor built-in form components from a parent .razor.css file.
**Warning signs:** Input fields appearing with default browser styling (white backgrounds, wrong fonts) despite correct CSS classes.

### Pitfall 3: Lost SortableJS Instance After Re-render
**What goes wrong:** After adding/removing exercises, the SortableJS instance becomes detached from the DOM because Blazor replaced the container element.
**Why it happens:** Blazor may replace the entire container div during re-render if the element identity changes.
**How to avoid:** Use `@key` directives on the container to maintain element identity, or re-initialize SortableJS in OnAfterRenderAsync when the item list changes. Consider using a stable container element with `@ref`.
**Warning signs:** Drag-and-drop stops working after adding or removing exercises.

### Pitfall 4: EF Core Tracked Entity State Corruption
**What goes wrong:** Loading a template for editing creates tracked entities. Modifying them in memory (reordering, adding, removing) without proper change tracking leads to unexpected SaveChanges behavior -- orphaned items, wrong foreign keys, or duplicate inserts.
**Why it happens:** EF Core tracks all loaded entities. Mixing in-memory editing with tracked entities creates a complex state graph.
**How to avoid:** Use the IDbContextFactory pattern (already established). Load template data into plain C# objects (BuilderItem, BuilderGroup) for editing. On Save, create a fresh DbContext, load the original template, and apply changes explicitly (add new items, remove deleted items, update changed items).
**Warning signs:** Duplicate TemplateItems in the database, orphaned TemplateGroups, wrong Position values after save.

### Pitfall 5: Undo/Redo Invalidating Group References
**What goes wrong:** Undo restores a previous state but group object references are stale. Items think they belong to a group that was dissolved, or groups reference items that were removed.
**Why it happens:** Memento snapshots must deep-copy the entire state graph (items + groups + their relationships), not just item positions.
**How to avoid:** Snapshot the full builder state as a serializable structure. On restore, rebuild all relationships from the snapshot.
**Warning signs:** "Ungroup" button appearing when no group exists, bracket connector rendering for non-existent groups.

### Pitfall 6: Position Gaps After Delete
**What goes wrong:** Deleting an exercise from position 3 in a 5-item list leaves positions [0,1,3,4]. Database queries with ORDER BY Position still work, but the builder UI and grouping logic may assume contiguous positions.
**Why it happens:** Removing an item without recompacting positions.
**How to avoid:** After any removal, reindex all items: `for (int i = 0; i < items.Count; i++) items[i].Position = i;`
**Warning signs:** Incorrect exercise numbering in the builder, grouping logic failing to find contiguous items.

### Pitfall 7: Tag Migration on Existing Database
**What goes wrong:** Adding a Tags column to WorkoutTemplate requires a migration. If not handled, the app crashes on startup with a "no such column" SQLite error.
**Why it happens:** Schema change without migration.
**How to avoid:** Create an EF Core migration after adding the Tags property. The migration should add the column with a default value of `"[]"` (empty JSON array).
**Warning signs:** SQLite exception on first query to WorkoutTemplates table.

## Code Examples

### Template List Page -- Load and Filter (following Exercises.razor pattern)
```csharp
// Templates.razor.cs
[Inject] private IDbContextFactory<AppDbContext> DbFactory { get; set; } = null!;

private List<WorkoutTemplate> allTemplates = new();
private string searchText = "";
private string? activeTagFilter;

protected override async Task OnInitializedAsync()
{
    using var context = DbFactory.CreateDbContext();
    allTemplates = await context.WorkoutTemplates
        .Include(t => t.Items)
            .ThenInclude(ti => ti.Exercise)
        .Include(t => t.Groups)
        .OrderByDescending(t => t.CreatedDate)
        .ToListAsync();
}

private IEnumerable<WorkoutTemplate> FilteredTemplates => allTemplates
    .Where(t => string.IsNullOrEmpty(searchText)
        || t.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
    .Where(t => string.IsNullOrEmpty(activeTagFilter)
        || t.Tags.Contains(activeTagFilter));
```

### Duplicate Template (D-06)
```csharp
private async Task DuplicateTemplate(WorkoutTemplate original)
{
    using var context = DbFactory.CreateDbContext();

    var copy = new WorkoutTemplate
    {
        Name = $"{original.Name} (copy)",
        Description = original.Description,
        Tags = new List<string>(original.Tags),
        CreatedDate = DateTime.UtcNow
    };

    // Deep copy groups first so items can reference them
    var groupMap = new Dictionary<int, TemplateGroup>();
    foreach (var og in original.Groups)
    {
        var ng = new TemplateGroup
        {
            GroupType = og.GroupType,
            Rounds = og.Rounds,
            MinuteWindow = og.MinuteWindow,
            WorkoutTemplate = copy
        };
        copy.Groups.Add(ng);
        groupMap[og.Id] = ng;
    }

    // Deep copy items
    foreach (var oi in original.Items.OrderBy(i => i.Position))
    {
        var ni = new TemplateItem
        {
            ExerciseId = oi.ExerciseId,
            Position = oi.Position,
            SectionType = oi.SectionType,
            TargetSets = oi.TargetSets,
            TargetReps = oi.TargetReps,
            TargetWeight = oi.TargetWeight,
            TargetDistance = oi.TargetDistance,
            TargetDurationSeconds = oi.TargetDurationSeconds,
            TargetPace = oi.TargetPace,
            TargetHeartRateZone = oi.TargetHeartRateZone,
            WorkoutTemplate = copy
        };
        if (oi.TemplateGroupId.HasValue && groupMap.TryGetValue(oi.TemplateGroupId.Value, out var mappedGroup))
            ni.TemplateGroup = mappedGroup;
        copy.Items.Add(ni);
    }

    context.WorkoutTemplates.Add(copy);
    await context.SaveChangesAsync();
}
```

### Estimated Duration Calculation (per UI-SPEC heuristic)
```csharp
public static int EstimateDurationMinutes(WorkoutTemplate template)
{
    double totalMinutes = 0;
    var emomGroupIds = new HashSet<int>();

    // Calculate EMOM groups first
    foreach (var group in template.Groups.Where(g => g.GroupType == GroupType.EMOM))
    {
        totalMinutes += (group.Rounds ?? 1) * (group.MinuteWindow ?? 1);
        foreach (var item in group.Items)
            emomGroupIds.Add(item.Id); // Track items handled by EMOM calc
    }

    // Calculate remaining items
    foreach (var item in template.Items.Where(i => !emomGroupIds.Contains(i.Id)))
    {
        if (item.Exercise is StrengthExercise)
            totalMinutes += (item.TargetSets ?? 2) * 1.5; // default 3 min
        else if (item.Exercise is EnduranceExercise)
            totalMinutes += item.TargetDurationSeconds.HasValue
                ? item.TargetDurationSeconds.Value / 60.0
                : 10; // default 10 min
    }

    // Round to nearest 5
    return (int)(Math.Round(totalMinutes / 5.0) * 5);
}
```

### Save Template from Builder State
```csharp
private async Task SaveTemplate()
{
    using var context = DbFactory.CreateDbContext();

    WorkoutTemplate template;
    if (isEditing)
    {
        template = await context.WorkoutTemplates
            .Include(t => t.Items)
            .Include(t => t.Groups)
            .FirstAsync(t => t.Id == templateId);

        // Clear existing items and groups
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

    // Rebuild groups
    var groupMap = new Dictionary<string, TemplateGroup>(); // local ID -> entity
    foreach (var bg in State.Groups)
    {
        var tg = new TemplateGroup
        {
            GroupType = bg.GroupType,
            Rounds = bg.Rounds,
            MinuteWindow = bg.MinuteWindow,
            WorkoutTemplate = template
        };
        template.Groups.Add(tg);
        groupMap[bg.LocalId] = tg;
    }

    // Rebuild items
    foreach (var bi in State.Items)
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
            TargetHeartRateZone = bi.TargetHeartRateZone,
            WorkoutTemplate = template
        };
        if (bi.GroupLocalId != null && groupMap.TryGetValue(bi.GroupLocalId, out var group))
            ti.TemplateGroup = group;
        template.Items.Add(ti);
    }

    await context.SaveChangesAsync();
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| HTML5 Drag API directly in Blazor | SortableJS via JS interop module | 2023+ | Reliable touch support, better animations, cross-browser consistency |
| Custom beforeunload JS for unsaved changes | Blazor NavigationLock component | .NET 7 (2022) | Built-in, handles internal + external navigation, no custom JS needed |
| Scoped AddDbContext | IDbContextFactory | .NET 6+ | Required for Blazor Server thread safety; already used in this project |
| Separate Tag table with join entity | JSON column with value converter | EF Core 7+ | Simpler for freeform tags when no tag management/reuse is needed |

**Deprecated/outdated:**
- HTML5 native drag events in Blazor: Unreliable across browsers and impossible on touch devices without polyfills. SortableJS handles all of this.
- LocationChangingRegistration (manual): Replaced by NavigationLock component which is declarative and easier to use.

## Open Questions

1. **SortableJS and section boundaries**
   - What we know: SortableJS supports multiple connected lists (one per section). Dragging between lists is supported via the `group` option.
   - What's unclear: Whether using one SortableJS instance per section (warm-up, working, cool-down) or a single instance with drop zone detection is cleaner for this UI.
   - Recommendation: Start with a single SortableJS instance on the entire exercise list. Use the item's position relative to section headers to determine section assignment on drop. This is simpler and avoids synchronizing multiple SortableJS instances.

2. **Undo stack depth and performance**
   - What we know: Each undo snapshot is a deep copy of the builder state (items + groups + tags). For typical templates (5-20 exercises), this is small.
   - What's unclear: The ideal max stack depth before memory becomes a concern.
   - Recommendation: Cap at 50 snapshots. A template with 20 exercises and 5 groups serializes to roughly 2-4 KB per snapshot, so 50 snapshots is ~200 KB maximum.

3. **Migration strategy for Tags column**
   - What we know: WorkoutTemplate entity needs a new `Tags` property. This requires an EF Core migration.
   - What's unclear: Whether to add the migration in Phase 3 or whether it should have been anticipated in Phase 1.
   - Recommendation: Add the Tags property and create a new migration in Phase 3. The column has a safe default (`"[]"`), so existing data is unaffected.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xunit 2.9.3 + Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Template" -x` |
| Full suite command | `dotnet test BlazorApp2.Tests` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| TMPL-01 | Create template with name, items in order | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~TemplateTests" -x` | Exists (basic tests in TemplateTests.cs) |
| TMPL-02 | Reorder items updates Position values | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~TemplateReorder" -x` | Wave 0 |
| TMPL-03 | Strength targets persist correctly | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~StrengthTargets" -x` | Exists |
| TMPL-04 | Endurance targets persist correctly | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~EnduranceTargets" -x` | Exists |
| TMPL-05 | Superset group with items persists | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Superset" -x` | Exists |
| TMPL-06 | EMOM group with rounds/minutes persists | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Emom" -x` | Exists |
| TMPL-07 | WarmUp/CoolDown section types persist | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SectionType" -x` | Exists |
| TMPL-01 | Tags stored/loaded via JSON converter | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~TemplateTags" -x` | Wave 0 |
| TMPL-06 | Duplicate template deep-copies all fields | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Duplicate" -x` | Wave 0 |
| TMPL-01 | Estimated duration calculation | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Duration" -x` | Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Template" -x`
- **Per wave merge:** `dotnet test BlazorApp2.Tests`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `BlazorApp2.Tests/TemplateTagTests.cs` -- covers Tags JSON value converter round-trip
- [ ] `BlazorApp2.Tests/TemplateDuplicateTests.cs` -- covers deep copy of template with groups, items, tags
- [ ] `BlazorApp2.Tests/TemplateReorderTests.cs` -- covers position recompaction after add/remove/reorder
- [ ] `BlazorApp2.Tests/TemplateDurationTests.cs` -- covers estimated duration calculation heuristic
- [ ] EF Core migration for Tags column on WorkoutTemplate

## Sources

### Primary (HIGH confidence)
- Existing codebase -- WorkoutTemplate.cs, TemplateConfiguration.cs, TemplateTests.cs, Exercises.razor, Dialog.razor, ExerciseCard.razor, app.css, ExerciseFormDialog.razor patterns
- 03-UI-SPEC.md -- Complete visual and interaction contracts for all 12 Phase 3 components
- 03-CONTEXT.md -- 21 locked decisions (D-01 through D-21) and discretion areas
- [ASP.NET Core Blazor navigation - NavigationLock](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/navigation?view=aspnetcore-10.0)
- [ASP.NET Core Blazor JavaScript interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-10.0)
- [EF Core many-to-many relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many)

### Secondary (MEDIUM confidence)
- [Blazor Sortable - Microsoft .NET Blog](https://devblogs.microsoft.com/dotnet/introducing-blazor-sortable/) -- SortableJS interop pattern endorsed by Microsoft
- [SortableJS GitHub - v1.15.7](https://github.com/SortableJS/Sortable) -- Latest stable release, feature reference
- [SortableJS on jsDelivr](https://www.jsdelivr.com/package/npm/sortablejs) -- Version verification

### Tertiary (LOW confidence)
- Undo/redo memento pattern implementation details derived from general software engineering patterns (not Blazor-specific documentation). The pattern is well-established but specific performance characteristics in Blazor Server need validation during implementation.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all libraries are already in the project or well-documented Microsoft recommendations; SortableJS version verified
- Architecture: HIGH -- data model fully exists, UI patterns established in Phase 2, code examples based on actual codebase patterns
- Pitfalls: HIGH -- DOM conflict pitfall verified via Microsoft's own Blazor Sortable blog post; ::deep pitfall confirmed by project memory; EF tracked entity pitfall is well-documented EF Core behavior
- Undo/redo: MEDIUM -- pattern is sound but implementation details (snapshot size, stack depth, serialization approach) will need tuning during development

**Research date:** 2026-03-21
**Valid until:** 2026-04-21 (stable -- no fast-moving dependencies)
