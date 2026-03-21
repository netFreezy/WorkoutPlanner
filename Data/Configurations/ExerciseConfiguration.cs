using BlazorApp2.Data.Entities;
using BlazorApp2.Data.SeedData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorApp2.Data.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.HasDiscriminator<string>("ExerciseType")
            .HasValue<StrengthExercise>("Strength")
            .HasValue<EnduranceExercise>("Endurance");

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Name);
    }
}

public class StrengthExerciseConfiguration : IEntityTypeConfiguration<StrengthExercise>
{
    public void Configure(EntityTypeBuilder<StrengthExercise> builder)
    {
        builder.HasData(ExerciseSeedData.GetStrengthExercises());
    }
}

public class EnduranceExerciseConfiguration : IEntityTypeConfiguration<EnduranceExercise>
{
    public void Configure(EntityTypeBuilder<EnduranceExercise> builder)
    {
        builder.HasData(ExerciseSeedData.GetEnduranceExercises());
    }
}
