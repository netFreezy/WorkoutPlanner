using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.Entities;

public class ScheduledWorkout
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Planned;

    // Template reference (nullable for ad-hoc workouts)
    public int? WorkoutTemplateId { get; set; }
    public WorkoutTemplate? WorkoutTemplate { get; set; }

    // Ad-hoc workout name (used when no template)
    public string? AdHocName { get; set; }

    // Computed display name: template name > ad-hoc name > fallback
    public string DisplayName => WorkoutTemplate?.Name ?? AdHocName ?? "Untitled";

    // Recurrence tracking
    public int? RecurrenceRuleId { get; set; }
    public RecurrenceRule? RecurrenceRule { get; set; }

    public WorkoutLog? WorkoutLog { get; set; }
}

public class RecurrenceRule
{
    public int Id { get; set; }

    // Template reference (nullable for ad-hoc recurring workouts)
    public int? WorkoutTemplateId { get; set; }
    public WorkoutTemplate? WorkoutTemplate { get; set; }

    // Ad-hoc recurring workout name
    public string? AdHocName { get; set; }

    // Anchor date for Daily interval counting
    public DateTime StartDate { get; set; }

    public FrequencyType FrequencyType { get; set; }
    public int Interval { get; set; } = 1;
    public DaysOfWeek DaysOfWeek { get; set; } = DaysOfWeek.None;

    public ICollection<ScheduledWorkout> ScheduledWorkouts { get; set; } = new List<ScheduledWorkout>();
}
