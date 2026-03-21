using System.Text.Json;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Models;

public class BuilderItem
{
    public string LocalId { get; set; } = Guid.NewGuid().ToString();
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public bool IsStrength { get; set; }
    public int Position { get; set; }
    public SectionType SectionType { get; set; } = SectionType.Working;
    public string? GroupLocalId { get; set; }

    // Strength targets
    public int? TargetSets { get; set; }
    public int? TargetReps { get; set; }
    public double? TargetWeight { get; set; }

    // Endurance targets
    public double? TargetDistance { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public double? TargetPace { get; set; }
    public int? TargetHeartRateZone { get; set; }

    // UI-only state (not serialized in snapshots)
    public bool IsSelected { get; set; }
}

public class BuilderGroup
{
    public string LocalId { get; set; } = Guid.NewGuid().ToString();
    public GroupType GroupType { get; set; }
    public int? Rounds { get; set; }
    public int? MinuteWindow { get; set; }
}

public class TemplateBuilderState
{
    private const int MaxUndoDepth = 50;
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<BuilderItem> Items { get; set; } = new();
    public List<BuilderGroup> Groups { get; set; } = new();
    public bool HasUnsavedChanges { get; set; }
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public void PushUndo()
    {
        var snapshot = SerializeSnapshot();
        _undoStack.Push(snapshot);
        _redoStack.Clear();
        HasUnsavedChanges = true;

        // Trim oldest entries if stack exceeds max depth
        if (_undoStack.Count > MaxUndoDepth)
        {
            var items = _undoStack.ToArray();
            _undoStack.Clear();
            // Keep only the newest MaxUndoDepth entries (they come out newest-first from ToArray)
            for (int i = MaxUndoDepth - 1; i >= 0; i--)
            {
                _undoStack.Push(items[i]);
            }
        }
    }

    public void Undo()
    {
        if (!CanUndo) return;

        var currentSnapshot = SerializeSnapshot();
        _redoStack.Push(currentSnapshot);

        var previousSnapshot = _undoStack.Pop();
        RestoreSnapshot(previousSnapshot);
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var currentSnapshot = SerializeSnapshot();
        _undoStack.Push(currentSnapshot);

        var nextSnapshot = _redoStack.Pop();
        RestoreSnapshot(nextSnapshot);
    }

    public void ResetChangeTracking()
    {
        HasUnsavedChanges = false;
        _undoStack.Clear();
        _redoStack.Clear();
    }

    public static int EstimateDurationMinutes(List<BuilderItem> items, List<BuilderGroup> groups)
    {
        if (items.Count == 0)
            return 5;

        double total = 0;
        var emomHandledItemIds = new HashSet<string>();

        // Handle EMOM groups first
        foreach (var group in groups.Where(g => g.GroupType == GroupType.EMOM))
        {
            var rounds = group.Rounds ?? 1;
            var minuteWindow = group.MinuteWindow ?? 1;
            total += rounds * minuteWindow;

            // Track items in this EMOM group so we don't double-count
            foreach (var item in items.Where(i => i.GroupLocalId == group.LocalId))
            {
                emomHandledItemIds.Add(item.LocalId);
            }
        }

        // Handle remaining items (not in EMOM groups)
        foreach (var item in items.Where(i => !emomHandledItemIds.Contains(i.LocalId)))
        {
            if (item.IsStrength)
            {
                var sets = item.TargetSets ?? 2;
                total += sets * 1.5;
            }
            else
            {
                total += item.TargetDurationSeconds.HasValue
                    ? item.TargetDurationSeconds.Value / 60.0
                    : 10;
            }
        }

        // Round to nearest 5
        var rounded = (int)(Math.Round(total / 5.0) * 5);

        // Minimum 5 minutes
        return Math.Max(rounded, 5);
    }

    private string SerializeSnapshot()
    {
        var snapshot = new
        {
            Name,
            Description,
            Tags,
            Items = Items.Select(i => new
            {
                i.LocalId,
                i.ExerciseId,
                i.ExerciseName,
                i.IsStrength,
                i.Position,
                i.SectionType,
                i.GroupLocalId,
                i.TargetSets,
                i.TargetReps,
                i.TargetWeight,
                i.TargetDistance,
                i.TargetDurationSeconds,
                i.TargetPace,
                i.TargetHeartRateZone
            }).ToList(),
            Groups
        };
        return JsonSerializer.Serialize(snapshot);
    }

    private void RestoreSnapshot(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Name = root.GetProperty("Name").GetString() ?? string.Empty;
        Description = root.GetProperty("Description").ValueKind == JsonValueKind.Null
            ? null
            : root.GetProperty("Description").GetString();

        Tags = new List<string>();
        foreach (var tag in root.GetProperty("Tags").EnumerateArray())
        {
            Tags.Add(tag.GetString() ?? string.Empty);
        }

        Items = new List<BuilderItem>();
        foreach (var itemEl in root.GetProperty("Items").EnumerateArray())
        {
            Items.Add(new BuilderItem
            {
                LocalId = itemEl.GetProperty("LocalId").GetString() ?? Guid.NewGuid().ToString(),
                ExerciseId = itemEl.GetProperty("ExerciseId").GetInt32(),
                ExerciseName = itemEl.GetProperty("ExerciseName").GetString() ?? string.Empty,
                IsStrength = itemEl.GetProperty("IsStrength").GetBoolean(),
                Position = itemEl.GetProperty("Position").GetInt32(),
                SectionType = (SectionType)itemEl.GetProperty("SectionType").GetInt32(),
                GroupLocalId = itemEl.GetProperty("GroupLocalId").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("GroupLocalId").GetString(),
                TargetSets = itemEl.GetProperty("TargetSets").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetSets").GetInt32(),
                TargetReps = itemEl.GetProperty("TargetReps").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetReps").GetInt32(),
                TargetWeight = itemEl.GetProperty("TargetWeight").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetWeight").GetDouble(),
                TargetDistance = itemEl.GetProperty("TargetDistance").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetDistance").GetDouble(),
                TargetDurationSeconds = itemEl.GetProperty("TargetDurationSeconds").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetDurationSeconds").GetInt32(),
                TargetPace = itemEl.GetProperty("TargetPace").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetPace").GetDouble(),
                TargetHeartRateZone = itemEl.GetProperty("TargetHeartRateZone").ValueKind == JsonValueKind.Null
                    ? null
                    : itemEl.GetProperty("TargetHeartRateZone").GetInt32()
            });
        }

        Groups = new List<BuilderGroup>();
        foreach (var groupEl in root.GetProperty("Groups").EnumerateArray())
        {
            Groups.Add(new BuilderGroup
            {
                LocalId = groupEl.GetProperty("LocalId").GetString() ?? Guid.NewGuid().ToString(),
                GroupType = (GroupType)groupEl.GetProperty("GroupType").GetInt32(),
                Rounds = groupEl.GetProperty("Rounds").ValueKind == JsonValueKind.Null
                    ? null
                    : groupEl.GetProperty("Rounds").GetInt32(),
                MinuteWindow = groupEl.GetProperty("MinuteWindow").ValueKind == JsonValueKind.Null
                    ? null
                    : groupEl.GetProperty("MinuteWindow").GetInt32()
            });
        }
    }
}
