using BlazorApp2.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp2.Data.Configurations;

public class PersonalRecordConfiguration : IEntityTypeConfiguration<PersonalRecord>
{
    public void Configure(EntityTypeBuilder<PersonalRecord> builder)
    {
        builder.HasOne(pr => pr.Exercise)
            .WithMany()
            .HasForeignKey(pr => pr.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.WorkoutLog)
            .WithMany()
            .HasForeignKey(pr => pr.WorkoutLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pr => pr.ExerciseId);
        builder.HasIndex(pr => pr.AchievedAt);
    }
}
