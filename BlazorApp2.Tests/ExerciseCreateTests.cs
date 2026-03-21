using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class ExerciseCreateTests : DataTestBase
{
    [Fact]
    public async Task CreateStrengthExercise_PersistsCorrectly()
    {
        var exercise = new StrengthExercise
        {
            Name = "Barbell Squat",
            Description = "Place barbell on upper back. Squat to parallel. Drive through heels to stand.",
            MuscleGroup = MuscleGroup.Legs,
            Equipment = Equipment.Barbell
        };

        Context.StrengthExercises.Add(exercise);
        await Context.SaveChangesAsync();

        var result = await Context.StrengthExercises
            .FirstAsync(e => e.Name == "Barbell Squat");

        Assert.Equal("Barbell Squat", result.Name);
        Assert.Equal("Place barbell on upper back. Squat to parallel. Drive through heels to stand.", result.Description);
        Assert.Equal(MuscleGroup.Legs, result.MuscleGroup);
        Assert.Equal(Equipment.Barbell, result.Equipment);
    }

    [Fact]
    public async Task CreateEnduranceExercise_PersistsCorrectly()
    {
        var exercise = new EnduranceExercise
        {
            Name = "Open Water Swim",
            Description = "Swimming in natural open water. Focus on sighting and bilateral breathing.",
            ActivityType = ActivityType.Swim
        };

        Context.EnduranceExercises.Add(exercise);
        await Context.SaveChangesAsync();

        var result = await Context.EnduranceExercises
            .FirstAsync(e => e.Name == "Open Water Swim");

        Assert.Equal("Open Water Swim", result.Name);
        Assert.Equal("Swimming in natural open water. Focus on sighting and bilateral breathing.", result.Description);
        Assert.Equal(ActivityType.Swim, result.ActivityType);
    }

    [Fact]
    public async Task CreateExercise_AppearsInMainExercisesDbSet()
    {
        var exercise = new StrengthExercise
        {
            Name = "Custom Press",
            Description = "A custom pressing movement.",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Dumbbell
        };

        Context.StrengthExercises.Add(exercise);
        await Context.SaveChangesAsync();

        // Should be queryable from the base Exercises DbSet
        var fromBase = await Context.Exercises
            .FirstOrDefaultAsync(e => e.Name == "Custom Press");

        Assert.NotNull(fromBase);
        Assert.IsType<StrengthExercise>(fromBase);
    }

    [Fact]
    public async Task CreateExercise_AutoGeneratesId()
    {
        var exercise = new EnduranceExercise
        {
            Name = "Trail Run",
            Description = "Running on unpaved trails with varied terrain.",
            ActivityType = ActivityType.Run
        };

        Context.EnduranceExercises.Add(exercise);
        await Context.SaveChangesAsync();

        Assert.True(exercise.Id > 0, $"Expected auto-generated Id > 0 but got {exercise.Id}");
    }
}
