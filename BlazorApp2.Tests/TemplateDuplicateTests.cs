using Microsoft.EntityFrameworkCore;
using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Tests;

public class TemplateDuplicateTests : DataTestBase
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

    /// <summary>
    /// Deep copy helper that simulates the duplicate template logic.
    /// Creates a new WorkoutTemplate with copied items, groups, and tags.
    /// </summary>
    private static WorkoutTemplate DuplicateTemplate(WorkoutTemplate original)
    {
        var copy = new WorkoutTemplate
        {
            Name = $"{original.Name} (copy)",
            Description = original.Description,
            Tags = new List<string>(original.Tags)
        };

        // Map original group IDs to new groups
        var groupMap = new Dictionary<int, TemplateGroup>();
        foreach (var group in original.Groups)
        {
            var newGroup = new TemplateGroup
            {
                WorkoutTemplate = copy,
                GroupType = group.GroupType,
                Rounds = group.Rounds,
                MinuteWindow = group.MinuteWindow
            };
            copy.Groups.Add(newGroup);
            groupMap[group.Id] = newGroup;
        }

        // Deep copy items with group mapping
        foreach (var item in original.Items)
        {
            var newItem = new TemplateItem
            {
                WorkoutTemplate = copy,
                ExerciseId = item.ExerciseId,
                Position = item.Position,
                SectionType = item.SectionType,
                TargetSets = item.TargetSets,
                TargetReps = item.TargetReps,
                TargetWeight = item.TargetWeight,
                TargetDistance = item.TargetDistance,
                TargetDurationSeconds = item.TargetDurationSeconds,
                TargetPace = item.TargetPace,
                TargetHeartRateZone = item.TargetHeartRateZone
            };

            if (item.TemplateGroupId.HasValue && groupMap.ContainsKey(item.TemplateGroupId.Value))
            {
                newItem.TemplateGroup = groupMap[item.TemplateGroupId.Value];
            }

            copy.Items.Add(newItem);
        }

        return copy;
    }

    [Fact]
    public async Task DeepCopy_Template_PreservesAllData()
    {
        var exercise1 = CreateStrengthExercise("Bench Press");
        var exercise2 = CreateStrengthExercise("Incline Press");
        var exercise3 = CreateStrengthExercise("Tricep Extension");

        var template = new WorkoutTemplate
        {
            Name = "Push Day",
            Description = "Chest and triceps",
            Tags = new List<string> { "Push" }
        };

        var group = new TemplateGroup
        {
            WorkoutTemplate = template,
            GroupType = GroupType.Superset
        };

        template.Groups.Add(group);

        template.Items.Add(new TemplateItem
        {
            Exercise = exercise1,
            Position = 0,
            SectionType = SectionType.Working,
            TemplateGroup = group,
            WorkoutTemplate = template,
            TargetSets = 3,
            TargetReps = 10
        });
        template.Items.Add(new TemplateItem
        {
            Exercise = exercise2,
            Position = 1,
            SectionType = SectionType.Working,
            TemplateGroup = group,
            WorkoutTemplate = template,
            TargetSets = 3,
            TargetReps = 10
        });
        template.Items.Add(new TemplateItem
        {
            Exercise = exercise3,
            Position = 2,
            SectionType = SectionType.Working,
            WorkoutTemplate = template,
            TargetSets = 3,
            TargetReps = 12
        });

        Context.WorkoutTemplates.Add(template);
        await Context.SaveChangesAsync();

        // Reload to get IDs
        var original = await Context.WorkoutTemplates
            .Include(t => t.Items)
            .Include(t => t.Groups)
            .FirstAsync();

        var copy = DuplicateTemplate(original);
        Context.WorkoutTemplates.Add(copy);
        await Context.SaveChangesAsync();

        // Reload the copy
        var loadedCopy = await Context.WorkoutTemplates
            .Include(t => t.Items)
            .Include(t => t.Groups).ThenInclude(g => g.Items)
            .Where(t => t.Name == "Push Day (copy)")
            .FirstAsync();

        Assert.Equal("Push Day (copy)", loadedCopy.Name);
        Assert.Contains("Push", loadedCopy.Tags);
        Assert.Equal(3, loadedCopy.Items.Count);
        var singleGroup = Assert.Single(loadedCopy.Groups);
        Assert.Equal(GroupType.Superset, singleGroup.GroupType);
        Assert.Equal(2, singleGroup.Items.Count);

        var orderedItems = loadedCopy.Items.OrderBy(i => i.Position).ToList();
        Assert.Equal(0, orderedItems[0].Position);
        Assert.Equal(1, orderedItems[1].Position);
        Assert.Equal(2, orderedItems[2].Position);
    }

    [Fact]
    public async Task DeepCopy_EmomGroup_PreservesRoundsAndWindow()
    {
        var exercise = CreateStrengthExercise("Kettlebell Swing");

        var template = new WorkoutTemplate
        {
            Name = "EMOM Workout",
            Tags = new List<string> { "EMOM", "Conditioning" }
        };

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

        var original = await Context.WorkoutTemplates
            .Include(t => t.Items)
            .Include(t => t.Groups)
            .FirstAsync();

        var copy = DuplicateTemplate(original);
        Context.WorkoutTemplates.Add(copy);
        await Context.SaveChangesAsync();

        var loadedCopy = await Context.WorkoutTemplates
            .Include(t => t.Groups)
            .Where(t => t.Name == "EMOM Workout (copy)")
            .FirstAsync();

        var copyGroup = loadedCopy.Groups.First();
        Assert.Equal(GroupType.EMOM, copyGroup.GroupType);
        Assert.Equal(5, copyGroup.Rounds);
        Assert.Equal(2, copyGroup.MinuteWindow);
        Assert.Equal(2, loadedCopy.Tags.Count);
        Assert.Contains("EMOM", loadedCopy.Tags);
        Assert.Contains("Conditioning", loadedCopy.Tags);
    }
}
