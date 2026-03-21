using BlazorApp2.Data.Enums;
using BlazorApp2.Models;

namespace BlazorApp2.Tests;

public class TemplateDurationTests
{
    private static BuilderItem MakeStrength(int? sets = null)
    {
        return new BuilderItem
        {
            IsStrength = true,
            TargetSets = sets
        };
    }

    private static BuilderItem MakeEndurance(int? durationSeconds = null)
    {
        return new BuilderItem
        {
            IsStrength = false,
            TargetDurationSeconds = durationSeconds
        };
    }

    [Fact]
    public void StrengthOnly_3Sets()
    {
        // 3 sets * 1.5 = 4.5, rounded to nearest 5 = 5
        var items = new List<BuilderItem> { MakeStrength(3) };
        var groups = new List<BuilderGroup>();

        var result = TemplateBuilderState.EstimateDurationMinutes(items, groups);

        Assert.Equal(5, result);
    }

    [Fact]
    public void EnduranceOnly_30Min()
    {
        // 1800 seconds / 60 = 30, rounded to nearest 5 = 30
        var items = new List<BuilderItem> { MakeEndurance(1800) };
        var groups = new List<BuilderGroup>();

        var result = TemplateBuilderState.EstimateDurationMinutes(items, groups);

        Assert.Equal(30, result);
    }

    [Fact]
    public void Mixed_StrengthAndEndurance()
    {
        // 2 strength items * 3 sets * 1.5 = 9 + 1 endurance 1200s/60 = 20 => total 29, rounded to 30
        var items = new List<BuilderItem>
        {
            MakeStrength(3),
            MakeStrength(3),
            MakeEndurance(1200)
        };
        var groups = new List<BuilderGroup>();

        var result = TemplateBuilderState.EstimateDurationMinutes(items, groups);

        Assert.Equal(30, result);
    }

    [Fact]
    public void EmomGroup_OverridesPerExercise()
    {
        // EMOM group: 5 rounds * 2 min = 10 min
        var groupId = Guid.NewGuid().ToString();
        var group = new BuilderGroup
        {
            LocalId = groupId,
            GroupType = GroupType.EMOM,
            Rounds = 5,
            MinuteWindow = 2
        };
        var items = new List<BuilderItem>
        {
            new BuilderItem { IsStrength = true, GroupLocalId = groupId, TargetSets = 10 },
            new BuilderItem { IsStrength = true, GroupLocalId = groupId, TargetSets = 10 }
        };
        var groups = new List<BuilderGroup> { group };

        var result = TemplateBuilderState.EstimateDurationMinutes(items, groups);

        Assert.Equal(10, result);
    }

    [Fact]
    public void NoTargets_UsesDefaults()
    {
        // Strength with no sets: default 2 * 1.5 = 3
        // Endurance with no duration: default 10
        // Total = 13, rounded to nearest 5 = 15
        var items = new List<BuilderItem>
        {
            MakeStrength(null),
            MakeEndurance(null)
        };
        var groups = new List<BuilderGroup>();

        var result = TemplateBuilderState.EstimateDurationMinutes(items, groups);

        Assert.Equal(15, result);
    }

    [Fact]
    public void Empty_ReturnsMinimum5()
    {
        var items = new List<BuilderItem>();
        var groups = new List<BuilderGroup>();

        var result = TemplateBuilderState.EstimateDurationMinutes(items, groups);

        Assert.Equal(5, result);
    }
}
