using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class ExerciseFilterTests : DataTestBase
{
    [Fact]
    public async Task FilterByName_CaseInsensitive_ReturnsMatching()
    {
        var all = await Context.Exercises.ToListAsync();

        var results = all.Where(e =>
            e.Name.Contains("pull", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.NotEmpty(results);
        Assert.Contains(results, e => e.Name == "Pull-Up");
        Assert.Contains(results, e => e.Name == "Weighted Pull-Up");
        Assert.Contains(results, e => e.Name == "Dumbbell Pullover");
    }

    [Fact]
    public async Task FilterByMuscleGroup_ReturnsOnlyMatching()
    {
        var all = await Context.Exercises.ToListAsync();

        var backExercises = all
            .OfType<StrengthExercise>()
            .Where(e => e.MuscleGroup == MuscleGroup.Back)
            .ToList();

        Assert.NotEmpty(backExercises);
        Assert.All(backExercises, e => Assert.Equal(MuscleGroup.Back, e.MuscleGroup));
        Assert.Contains(backExercises, e => e.Name == "Pull-Up");
    }

    [Fact]
    public async Task FilterByEquipment_ReturnsOnlyMatching()
    {
        var all = await Context.Exercises.ToListAsync();

        var bodyweightExercises = all
            .OfType<StrengthExercise>()
            .Where(e => e.Equipment == Equipment.Bodyweight)
            .ToList();

        Assert.NotEmpty(bodyweightExercises);
        Assert.All(bodyweightExercises, e => Assert.Equal(Equipment.Bodyweight, e.Equipment));
    }

    [Fact]
    public async Task FilterByType_StrengthOnly_ReturnsOnlyStrength()
    {
        var all = await Context.Exercises.ToListAsync();

        var strengthOnly = all.Where(e => e is StrengthExercise).ToList();

        Assert.Equal(37, strengthOnly.Count);
        Assert.All(strengthOnly, e => Assert.IsType<StrengthExercise>(e));
    }

    [Fact]
    public async Task FilterByType_EnduranceOnly_ReturnsOnlyEndurance()
    {
        var all = await Context.Exercises.ToListAsync();

        var enduranceOnly = all.Where(e => e is EnduranceExercise).ToList();

        Assert.Equal(13, enduranceOnly.Count);
        Assert.All(enduranceOnly, e => Assert.IsType<EnduranceExercise>(e));
    }

    [Fact]
    public async Task FilterCombined_AND_MuscleGroupAndEquipment()
    {
        var all = await Context.Exercises.ToListAsync();

        var results = all
            .OfType<StrengthExercise>()
            .Where(e => e.MuscleGroup == MuscleGroup.Back && e.Equipment == Equipment.Bodyweight)
            .ToList();

        Assert.NotEmpty(results);
        Assert.All(results, e =>
        {
            Assert.Equal(MuscleGroup.Back, e.MuscleGroup);
            Assert.Equal(Equipment.Bodyweight, e.Equipment);
        });
        Assert.Contains(results, e => e.Name == "Pull-Up");
    }

    [Fact]
    public async Task FilterNoMatch_ReturnsEmpty()
    {
        var all = await Context.Exercises.ToListAsync();

        var results = all.Where(e =>
            e.Name.Contains("zzzzzzz", StringComparison.OrdinalIgnoreCase)).ToList();

        Assert.Empty(results);
    }
}
