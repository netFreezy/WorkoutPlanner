using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.Entities;

public class WorkoutTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public ICollection<TemplateItem> Items { get; set; } = new List<TemplateItem>();
    public ICollection<TemplateGroup> Groups { get; set; } = new List<TemplateGroup>();
}

public class TemplateItem
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int Position { get; set; }
    public SectionType SectionType { get; set; } = SectionType.Working;

    // Grouping (optional) -- per D-06
    public int? TemplateGroupId { get; set; }
    public TemplateGroup? TemplateGroup { get; set; }

    // Strength targets (nullable -- only for strength exercises) -- per D-03
    public int? TargetSets { get; set; }
    public int? TargetReps { get; set; }
    public double? TargetWeight { get; set; }

    // Endurance targets (nullable -- only for endurance exercises) -- per D-03
    public double? TargetDistance { get; set; }
    public int? TargetDurationSeconds { get; set; }
    public double? TargetPace { get; set; }
    public int? TargetHeartRateZone { get; set; }
}

public class TemplateGroup
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;
    public GroupType GroupType { get; set; }

    // EMOM-specific fields -- per D-07
    public int? Rounds { get; set; }
    public int? MinuteWindow { get; set; }

    public ICollection<TemplateItem> Items { get; set; } = new List<TemplateItem>();
}
