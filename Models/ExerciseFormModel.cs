using System.ComponentModel.DataAnnotations;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Models;

public class ExerciseFormModel
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public bool IsStrength { get; set; } = true;

    // Strength-specific fields
    public MuscleGroup MuscleGroup { get; set; } = MuscleGroup.Chest;
    public Equipment Equipment { get; set; } = Equipment.Bodyweight;

    // Endurance-specific fields
    public ActivityType ActivityType { get; set; } = ActivityType.Run;
}
