using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data;
using BlazorApp2.Data.Entities;

namespace BlazorApp2.Tests;

public class DbContextFactoryTests : DataTestBase
{
    [Fact]
    public async Task ExercisesDbSet_IsQueryable_ReturnsEmptyList()
    {
        var exercises = await Context.Exercises.ToListAsync();

        Assert.NotNull(exercises);
        Assert.Empty(exercises);
    }

    [Fact]
    public async Task TwoContextInstances_SharedConnection_SeesSameData()
    {
        // Use a second context sharing the same connection (via same options)
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(Context.Database.GetDbConnection())
            .Options;

        using var context2 = new AppDbContext(options);

        Context.StrengthExercises.Add(new StrengthExercise
        {
            Name = "Test Exercise",
            MuscleGroup = Data.Enums.MuscleGroup.Chest,
            Equipment = Data.Enums.Equipment.Barbell
        });
        await Context.SaveChangesAsync();

        var exercises = await context2.Exercises.ToListAsync();
        Assert.Single(exercises);
        Assert.Equal("Test Exercise", exercises[0].Name);
    }
}
