using BlazorApp2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp2.Data.Configurations;

public class WorkoutLogConfiguration : IEntityTypeConfiguration<WorkoutLog>
{
    public void Configure(EntityTypeBuilder<WorkoutLog> builder)
    {
        builder.HasOne(wl => wl.ScheduledWorkout)
            .WithOne(sw => sw.WorkoutLog)
            .HasForeignKey<WorkoutLog>(wl => wl.ScheduledWorkoutId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SetLogConfiguration : IEntityTypeConfiguration<SetLog>
{
    public void Configure(EntityTypeBuilder<SetLog> builder)
    {
        builder.HasOne(sl => sl.WorkoutLog)
            .WithMany(wl => wl.SetLogs)
            .HasForeignKey(sl => sl.WorkoutLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sl => sl.Exercise)
            .WithMany()
            .HasForeignKey(sl => sl.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EnduranceLogConfiguration : IEntityTypeConfiguration<EnduranceLog>
{
    public void Configure(EntityTypeBuilder<EnduranceLog> builder)
    {
        builder.HasOne(el => el.WorkoutLog)
            .WithMany(wl => wl.EnduranceLogs)
            .HasForeignKey(el => el.WorkoutLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(el => el.Exercise)
            .WithMany()
            .HasForeignKey(el => el.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
