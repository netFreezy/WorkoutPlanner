using BlazorApp2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp2.Data.Configurations;

public class ScheduledWorkoutConfiguration : IEntityTypeConfiguration<ScheduledWorkout>
{
    public void Configure(EntityTypeBuilder<ScheduledWorkout> builder)
    {
        builder.HasOne(sw => sw.WorkoutTemplate)
            .WithMany()
            .HasForeignKey(sw => sw.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sw => sw.RecurrenceRule)
            .WithMany(rr => rr.ScheduledWorkouts)
            .HasForeignKey(sw => sw.RecurrenceRuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(sw => sw.ScheduledDate);

        builder.Property(sw => sw.AdHocName).HasMaxLength(200);
    }
}

public class RecurrenceRuleConfiguration : IEntityTypeConfiguration<RecurrenceRule>
{
    public void Configure(EntityTypeBuilder<RecurrenceRule> builder)
    {
        builder.HasOne(rr => rr.WorkoutTemplate)
            .WithMany()
            .HasForeignKey(rr => rr.WorkoutTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(rr => rr.AdHocName).HasMaxLength(200);
    }
}
