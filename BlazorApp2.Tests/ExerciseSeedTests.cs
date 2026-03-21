using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class ExerciseSeedTests : DataTestBase
{
    [Fact]
    public async Task SeedData_LoadsCorrectTotalCount()
    {
        var total = await Context.Exercises.CountAsync();
        Assert.Equal(50, total);
    }

    [Fact]
    public async Task SeedData_ContainsExpectedStrengthCount()
    {
        var strengthCount = await Context.StrengthExercises.CountAsync();
        Assert.InRange(strengthCount, 35, 40);
    }

    [Fact]
    public async Task SeedData_ContainsExpectedEnduranceCount()
    {
        var enduranceCount = await Context.EnduranceExercises.CountAsync();
        Assert.InRange(enduranceCount, 10, 15);
    }

    [Fact]
    public async Task SeedData_AllExercisesHaveDescriptions()
    {
        var exercises = await Context.Exercises.ToListAsync();
        Assert.All(exercises, e =>
        {
            Assert.False(string.IsNullOrEmpty(e.Description),
                $"Exercise '{e.Name}' (ID {e.Id}) has null or empty Description.");
        });
    }

    [Fact]
    public async Task SeedData_CoversMuscleGroups()
    {
        var muscleGroups = await Context.StrengthExercises
            .Select(s => s.MuscleGroup)
            .Distinct()
            .ToListAsync();

        Assert.True(muscleGroups.Count >= 5,
            $"Expected at least 5 distinct MuscleGroup values but found {muscleGroups.Count}: {string.Join(", ", muscleGroups)}");
    }

    [Fact]
    public async Task SeedData_HasRunningVariants()
    {
        var runExercises = await Context.EnduranceExercises
            .Where(e => e.ActivityType == ActivityType.Run)
            .ToListAsync();

        Assert.True(runExercises.Count >= 5,
            $"Expected at least 5 running exercises but found {runExercises.Count}.");
    }
}
