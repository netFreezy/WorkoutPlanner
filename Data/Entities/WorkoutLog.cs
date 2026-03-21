using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.Entities;

public class WorkoutLog
{
    public int Id { get; set; }
    public int ScheduledWorkoutId { get; set; }
    public ScheduledWorkout ScheduledWorkout { get; set; } = null!;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int? Rpe { get; set; }  // 1-10 rating of perceived exertion
    public string? Notes { get; set; }

    public ICollection<SetLog> SetLogs { get; set; } = new List<SetLog>();
    public ICollection<EnduranceLog> EnduranceLogs { get; set; } = new List<EnduranceLog>();
}

public class SetLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int SetNumber { get; set; }
    public SetType SetType { get; set; } = SetType.Working;

    // Planned (snapshot at session creation) -- per D-15
    public int? PlannedReps { get; set; }
    public double? PlannedWeight { get; set; }

    // Actual (filled during logging)
    public int? ActualReps { get; set; }
    public double? ActualWeight { get; set; }
    public bool IsCompleted { get; set; }
}

public class EnduranceLog
{
    public int Id { get; set; }
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public ActivityType ActivityType { get; set; }

    // Planned -- per D-16
    public double? PlannedDistance { get; set; }
    public int? PlannedDurationSeconds { get; set; }
    public double? PlannedPace { get; set; }
    public int? PlannedHeartRateZone { get; set; }

    // Actual
    public double? ActualDistance { get; set; }
    public int? ActualDurationSeconds { get; set; }
    public double? ActualPace { get; set; }
    public int? ActualHeartRateZone { get; set; }
    public bool IsCompleted { get; set; }
}
