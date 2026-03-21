using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.Entities;

public abstract class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}

public class StrengthExercise : Exercise
{
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }
}

public class EnduranceExercise : Exercise
{
    public ActivityType ActivityType { get; set; }
}
