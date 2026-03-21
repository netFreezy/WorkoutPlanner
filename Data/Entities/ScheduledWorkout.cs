using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.Entities;

public class ScheduledWorkout
{
    public int Id { get; set; }
    public DateTime ScheduledDate { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Planned;
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;

    // Recurrence tracking
    public int? RecurrenceRuleId { get; set; }
    public RecurrenceRule? RecurrenceRule { get; set; }

    public WorkoutLog? WorkoutLog { get; set; }
}

public class RecurrenceRule
{
    public int Id { get; set; }
    public int WorkoutTemplateId { get; set; }
    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;

    public FrequencyType FrequencyType { get; set; }
    public int Interval { get; set; } = 1;
    public DaysOfWeek DaysOfWeek { get; set; } = DaysOfWeek.None;

    public ICollection<ScheduledWorkout> ScheduledWorkouts { get; set; } = new List<ScheduledWorkout>();
}
