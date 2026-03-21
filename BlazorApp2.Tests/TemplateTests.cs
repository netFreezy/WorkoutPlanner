using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class TemplateTests : DataTestBase
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

    private EnduranceExercise CreateEnduranceExercise(string name = "5K Run")
    {
        var exercise = new EnduranceExercise
        {
            Name = name,
            ActivityType = ActivityType.Run
        };
        Context.EnduranceExercises.Add(exercise);
        return exercise;
    }

    [Fact]
    public async Task WorkoutTemplate_WithOrderedItems_PreservesPositionOrder()
    {
        var exercise = CreateStrengthExercise();
        var template = new WorkoutTemplate
        {
            Name = "Push Day",
            Description = "Chest and triceps"
        };
        template.Items.Add(new TemplateItem { Exercise = exercise, Position = 0, SectionType = SectionType.Working });
        template.Items.Add(new TemplateItem { Exercise = exercise, Position = 1, SectionType = SectionType.Working });
        template.Items.Add(new TemplateItem { Exercise = exercise, Position = 2, SectionType = SectionType.Working });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var loaded = await Context.WorkoutTemplates
            .Include(t => t.Items)
            .FirstAsync();

        Assert.Equal("Push Day", loaded.Name);
        Assert.Equal("Chest and triceps", loaded.Description);
        Assert.Equal(3, loaded.Items.Count);

        var orderedItems = loaded.Items.OrderBy(i => i.Position).ToList();
        Assert.Equal(0, orderedItems[0].Position);
        Assert.Equal(1, orderedItems[1].Position);
        Assert.Equal(2, orderedItems[2].Position);
    }

    [Fact]
    public async Task TemplateItem_WithStrengthTargets_PersistsCorrectly()
    {
        var exercise = CreateStrengthExercise();
        var template = new WorkoutTemplate { Name = "Strength Test" };
        template.Items.Add(new TemplateItem
        {
            Exercise = exercise,
            Position = 0,
            SectionType = SectionType.Working,
            TargetSets = 3,
            TargetReps = 10,
            TargetWeight = 60.0
        });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var item = await Context.TemplateItems.FirstAsync();

        Assert.Equal(3, item.TargetSets);
        Assert.Equal(10, item.TargetReps);
        Assert.Equal(60.0, item.TargetWeight);
    }

    [Fact]
    public async Task TemplateItem_WithEnduranceTargets_PersistsCorrectly()
    {
        var exercise = CreateEnduranceExercise();
        var template = new WorkoutTemplate { Name = "Endurance Test" };
        template.Items.Add(new TemplateItem
        {
            Exercise = exercise,
            Position = 0,
            SectionType = SectionType.Working,
            TargetDistance = 5.0,
            TargetDurationSeconds = 1500,
            TargetPace = 5.0,
            TargetHeartRateZone = 3
        });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var item = await Context.TemplateItems.FirstAsync();

        Assert.Equal(5.0, item.TargetDistance);
        Assert.Equal(1500, item.TargetDurationSeconds);
        Assert.Equal(5.0, item.TargetPace);
        Assert.Equal(3, item.TargetHeartRateZone);
    }

    [Fact]
    public async Task TemplateGroup_SupersetType_WithItems_PersistsCorrectly()
    {
        var exercise1 = CreateStrengthExercise("Bicep Curl");
        var exercise2 = CreateStrengthExercise("Tricep Extension");
        var template = new WorkoutTemplate { Name = "Superset Test" };

        var group = new TemplateGroup
        {
            WorkoutTemplate = template,
            GroupType = GroupType.Superset
        };

        var item1 = new TemplateItem
        {
            Exercise = exercise1,
            Position = 0,
            SectionType = SectionType.Working,
            TemplateGroup = group,
            WorkoutTemplate = template
        };
        var item2 = new TemplateItem
        {
            Exercise = exercise2,
            Position = 1,
            SectionType = SectionType.Working,
            TemplateGroup = group,
            WorkoutTemplate = template
        };

        template.Groups.Add(group);
        template.Items.Add(item1);
        template.Items.Add(item2);

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var loadedGroup = await Context.TemplateGroups
            .Include(g => g.Items)
            .FirstAsync();

        Assert.Equal(GroupType.Superset, loadedGroup.GroupType);
        Assert.Equal(2, loadedGroup.Items.Count);
    }

    [Fact]
    public async Task TemplateGroup_EmomType_WithRoundsAndMinuteWindow_PersistsCorrectly()
    {
        var exercise = CreateStrengthExercise("Kettlebell Swing");
        var template = new WorkoutTemplate { Name = "EMOM Test" };

        var group = new TemplateGroup
        {
            WorkoutTemplate = template,
            GroupType = GroupType.EMOM,
            Rounds = 5,
            MinuteWindow = 2
        };

        template.Groups.Add(group);
        template.Items.Add(new TemplateItem
        {
            Exercise = exercise,
            Position = 0,
            SectionType = SectionType.Working,
            TemplateGroup = group,
            WorkoutTemplate = template
        });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var loadedGroup = await Context.TemplateGroups.FirstAsync();

        Assert.Equal(GroupType.EMOM, loadedGroup.GroupType);
        Assert.Equal(5, loadedGroup.Rounds);
        Assert.Equal(2, loadedGroup.MinuteWindow);
    }

    [Fact]
    public async Task TemplateItem_SectionType_WarmUpAndCoolDown_PersistsCorrectly()
    {
        var exercise = CreateStrengthExercise("Arm Circle");
        var template = new WorkoutTemplate { Name = "Section Type Test" };

        template.Items.Add(new TemplateItem
        {
            Exercise = exercise,
            Position = 0,
            SectionType = SectionType.WarmUp
        });
        template.Items.Add(new TemplateItem
        {
            Exercise = exercise,
            Position = 1,
            SectionType = SectionType.CoolDown
        });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        var items = await Context.TemplateItems.OrderBy(i => i.Position).ToListAsync();

        Assert.Equal(SectionType.WarmUp, items[0].SectionType);
        Assert.Equal(SectionType.CoolDown, items[1].SectionType);
    }
}
