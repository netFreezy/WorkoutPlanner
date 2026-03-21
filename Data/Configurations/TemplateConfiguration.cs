using BlazorApp2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp2.Data.Configurations;

public class WorkoutTemplateConfiguration : IEntityTypeConfiguration<WorkoutTemplate>
{
    public void Configure(EntityTypeBuilder<WorkoutTemplate> builder)
    {
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}

public class TemplateItemConfiguration : IEntityTypeConfiguration<TemplateItem>
{
    public void Configure(EntityTypeBuilder<TemplateItem> builder)
    {
        builder.HasOne(ti => ti.WorkoutTemplate)
            .WithMany(wt => wt.Items)
            .HasForeignKey(ti => ti.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ti => ti.Exercise)
            .WithMany()
            .HasForeignKey(ti => ti.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ti => ti.TemplateGroup)
            .WithMany(tg => tg.Items)
            .HasForeignKey(ti => ti.TemplateGroupId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(ti => new { ti.WorkoutTemplateId, ti.Position });
    }
}

public class TemplateGroupConfiguration : IEntityTypeConfiguration<TemplateGroup>
{
    public void Configure(EntityTypeBuilder<TemplateGroup> builder)
    {
        builder.HasOne(tg => tg.WorkoutTemplate)
            .WithMany(wt => wt.Groups)
            .HasForeignKey(tg => tg.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
