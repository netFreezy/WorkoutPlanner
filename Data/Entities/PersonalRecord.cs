using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.Entities;

public class PersonalRecord
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; } = null!;
    public int WorkoutLogId { get; set; }
    public WorkoutLog WorkoutLog { get; set; } = null!;
    public DateTime AchievedAt { get; set; }

    // Strength PRs (per D-10)
    public StrengthPRType? StrengthType { get; set; }
    public double? Value { get; set; }

    // Endurance PRs (per D-11: tracked per activity type)
    public EndurancePRType? EnduranceType { get; set; }
    public ActivityType? ActivityType { get; set; }

    public string DisplayValue => FormatValue();

    private string FormatValue()
    {
        if (StrengthType.HasValue)
        {
            return StrengthType.Value switch
            {
                StrengthPRType.Weight => $"{Value:F1} kg",
                StrengthPRType.Reps => $"{Value:F0} reps",
                StrengthPRType.EstimatedOneRepMax => $"{Value:F1} kg (e1RM)",
                _ => $"{Value}"
            };
        }
        if (EnduranceType.HasValue)
        {
            return EnduranceType.Value switch
            {
                EndurancePRType.Pace => $"{Value:F2} min/km",
                EndurancePRType.Distance => $"{Value:F2} km",
                _ => $"{Value}"
            };
        }
        return $"{Value}";
    }
}
