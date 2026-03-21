using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class ExerciseHierarchyTests : DataTestBase
{
    [Fact]
    public async Task StrengthExercise_PersistsAndQueriesBack_WithCorrectProperties()
    {
        Context.StrengthExercises.Add(new StrengthExercise
        {
            Name = "Bench Press",
            Description = "Barbell bench press",
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        });
        await Context.SaveChangesAsync();

        var result = await Context.Exercises.FirstAsync();

        var strength = Assert.IsType<StrengthExercise>(result);
        Assert.Equal("Bench Press", strength.Name);
        Assert.Equal("Barbell bench press", strength.Description);
        Assert.Equal(MuscleGroup.Chest, strength.MuscleGroup);
        Assert.Equal(Equipment.Barbell, strength.Equipment);
    }

    [Fact]
    public async Task EnduranceExercise_PersistsAndQueriesBack_WithCorrectActivityType()
    {
        Context.EnduranceExercises.Add(new EnduranceExercise
        {
            Name = "5K Run",
            ActivityType = ActivityType.Run
        });
        await Context.SaveChangesAsync();

        var result = await Context.Exercises.FirstAsync();

        var endurance = Assert.IsType<EnduranceExercise>(result);
        Assert.Equal("5K Run", endurance.Name);
        Assert.Equal(ActivityType.Run, endurance.ActivityType);
    }

    [Fact]
    public async Task BothTypes_QueryFromExercisesBase_ReturnsCorrectDiscriminators()
    {
        Context.StrengthExercises.Add(new StrengthExercise
        {
            Name = "Pull-Up",
            MuscleGroup = MuscleGroup.Back,
            Equipment = Equipment.Bodyweight
        });
        Context.EnduranceExercises.Add(new EnduranceExercise
        {
            Name = "Cycling",
            ActivityType = ActivityType.Cycle
        });
        await Context.SaveChangesAsync();

        var exercises = await Context.Exercises.OrderBy(e => e.Name).ToListAsync();

        Assert.Equal(2, exercises.Count);
        Assert.IsType<EnduranceExercise>(exercises[0]); // Cycling
        Assert.IsType<StrengthExercise>(exercises[1]);  // Pull-Up
    }

    [Fact]
    public async Task StrengthExercisesDbSet_ReturnsOnlyStrengthExercises()
    {
        Context.StrengthExercises.Add(new StrengthExercise
        {
            Name = "Squat",
            MuscleGroup = MuscleGroup.Legs,
            Equipment = Equipment.Barbell
        });
        Context.EnduranceExercises.Add(new EnduranceExercise
        {
            Name = "Swimming",
            ActivityType = ActivityType.Swim
        });
        await Context.SaveChangesAsync();

        var strengthOnly = await Context.StrengthExercises.ToListAsync();

        Assert.Single(strengthOnly);
        Assert.Equal("Squat", strengthOnly[0].Name);
    }

    [Fact]
    public async Task EnduranceExercisesDbSet_ReturnsOnlyEnduranceExercises()
    {
        Context.StrengthExercises.Add(new StrengthExercise
        {
            Name = "Deadlift",
            MuscleGroup = MuscleGroup.Back,
            Equipment = Equipment.Barbell
        });
        Context.EnduranceExercises.Add(new EnduranceExercise
        {
            Name = "Rowing",
            ActivityType = ActivityType.Row
        });
        await Context.SaveChangesAsync();

        var enduranceOnly = await Context.EnduranceExercises.ToListAsync();

        Assert.Single(enduranceOnly);
        Assert.Equal("Rowing", enduranceOnly[0].Name);
    }
}
