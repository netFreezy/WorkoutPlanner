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

        var result = await Context.Exercises.FirstAsync(e => e.Name == "Bench Press");

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

        var result = await Context.Exercises.FirstAsync(e => e.Name == "5K Run");

        var endurance = Assert.IsType<EnduranceExercise>(result);
        Assert.Equal("5K Run", endurance.Name);
        Assert.Equal(ActivityType.Run, endurance.ActivityType);
    }

    [Fact]
    public async Task BothTypes_QueryFromExercisesBase_ReturnsCorrectDiscriminators()
    {
        // Seed data already provides both types; verify discriminators work
        var strengthFromBase = await Context.Exercises
            .Where(e => e.Name == "Pull-Up")
            .FirstAsync();
        var enduranceFromBase = await Context.Exercises
            .Where(e => e.Name == "Easy Run")
            .FirstAsync();

        Assert.IsType<StrengthExercise>(strengthFromBase);
        Assert.IsType<EnduranceExercise>(enduranceFromBase);
    }

    [Fact]
    public async Task StrengthExercisesDbSet_ReturnsOnlyStrengthExercises()
    {
        var strengthOnly = await Context.StrengthExercises.ToListAsync();
        var enduranceOnly = await Context.EnduranceExercises.ToListAsync();

        Assert.NotEmpty(strengthOnly);
        Assert.All(strengthOnly, e => Assert.IsType<StrengthExercise>(e));

        // Verify no overlap -- strength names should not appear in endurance
        var enduranceNames = enduranceOnly.Select(e => e.Name).ToHashSet();
        Assert.All(strengthOnly, e => Assert.DoesNotContain(e.Name, enduranceNames));
    }

    [Fact]
    public async Task EnduranceExercisesDbSet_ReturnsOnlyEnduranceExercises()
    {
        var enduranceOnly = await Context.EnduranceExercises.ToListAsync();
        var strengthOnly = await Context.StrengthExercises.ToListAsync();

        Assert.NotEmpty(enduranceOnly);
        Assert.All(enduranceOnly, e => Assert.IsType<EnduranceExercise>(e));

        // Verify no overlap -- endurance names should not appear in strength
        var strengthNames = strengthOnly.Select(e => e.Name).ToHashSet();
        Assert.All(enduranceOnly, e => Assert.DoesNotContain(e.Name, strengthNames));
    }
}
