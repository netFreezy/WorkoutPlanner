using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class TemplateTagTests : DataTestBase
{
    private StrengthExercise CreateStrengthExercise(string name = "Bench Press")
    {
        var exercise = new StrengthExercise
        {
            Name = name,
            MuscleGroup = MuscleGroup.Chest,
            Equipment = Equipment.Barbell
        };
        Context.StrengthExercises.Add(exercise);
        return exercise;
    }

    [Fact]
    public async Task Tags_EmptyList_PersistsAsEmptyArray()
    {
        var exercise = CreateStrengthExercise();
        var template = new WorkoutTemplate
        {
            Name = "Empty Tags Test",
            Tags = new List<string>()
        };
        template.Items.Add(new TemplateItem { Exercise = exercise, Position = 0 });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var loaded = await Context.WorkoutTemplates.FirstAsync();

        Assert.NotNull(loaded.Tags);
        Assert.Empty(loaded.Tags);
    }

    [Fact]
    public async Task Tags_MultipleValues_RoundTrip()
    {
        var exercise = CreateStrengthExercise();
        var template = new WorkoutTemplate
        {
            Name = "Multi Tag Test",
            Tags = new List<string> { "Push", "Upper Body", "Monday" }
        };
        template.Items.Add(new TemplateItem { Exercise = exercise, Position = 0 });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var loaded = await Context.WorkoutTemplates.FirstAsync();

        Assert.Equal(3, loaded.Tags.Count);
        Assert.Contains("Push", loaded.Tags);
        Assert.Contains("Upper Body", loaded.Tags);
        Assert.Contains("Monday", loaded.Tags);
    }

    [Fact]
    public async Task Tags_SpecialCharacters_RoundTrip()
    {
        var exercise = CreateStrengthExercise();
        var template = new WorkoutTemplate
        {
            Name = "Special Chars Test",
            Tags = new List<string> { "Push & Pull", "Leg Day" }
        };
        template.Items.Add(new TemplateItem { Exercise = exercise, Position = 0 });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var loaded = await Context.WorkoutTemplates.FirstAsync();

        Assert.Equal(2, loaded.Tags.Count);
        Assert.Contains("Push & Pull", loaded.Tags);
        Assert.Contains("Leg Day", loaded.Tags);
    }
}
