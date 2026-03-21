using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;

namespace BlazorApp2.Tests;

public class DbContextFactoryTests : DataTestBase
{
    [Fact]
    public async Task ExercisesDbSet_IsQueryable_ReturnsSeedData()
    {
        var exercises = await Context.Exercises.ToListAsync();

        Assert.NotNull(exercises);
        Assert.NotEmpty(exercises);
        Assert.Equal(50, exercises.Count);
    }

    [Fact]
    public async Task TwoContextInstances_SharedConnection_SeesSameData()
    {
        // Use a second context sharing the same connection (via same options)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Context.Database.GetDbConnection())
            .Options;

        using var context2 = new AppDbContext(options);

        var seedCount = await Context.Exercises.CountAsync();

        Context.StrengthExercises.Add(new StrengthExercise
        {
            Name = "Test Exercise",
            MuscleGroup = Data.Enums.MuscleGroup.Chest,
            Equipment = Data.Enums.Equipment.Barbell
        });
        await Context.SaveChangesAsync();

        var exercises = await context2.Exercises.ToListAsync();
        Assert.Equal(seedCount + 1, exercises.Count);
        Assert.Contains(exercises, e => e.Name == "Test Exercise");
    }
}
