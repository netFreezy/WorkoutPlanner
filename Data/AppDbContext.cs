using BlazorApp2.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp2.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Exercise hierarchy (TPH)
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<StrengthExercise> StrengthExercises => Set<StrengthExercise>();
    public DbSet<EnduranceExercise> EnduranceExercises => Set<EnduranceExercise>();

    // Templates
    public DbSet<WorkoutTemplate> WorkoutTemplates => Set<WorkoutTemplate>();
    public DbSet<TemplateItem> TemplateItems => Set<TemplateItem>();
    public DbSet<TemplateGroup> TemplateGroups => Set<TemplateGroup>();

    // Scheduling
    public DbSet<ScheduledWorkout> ScheduledWorkouts => Set<ScheduledWorkout>();
    public DbSet<RecurrenceRule> RecurrenceRules => Set<RecurrenceRule>();

    // Logging
    public DbSet<WorkoutLog> WorkoutLogs => Set<WorkoutLog>();
    public DbSet<SetLog> SetLogs => Set<SetLog>();
    public DbSet<EnduranceLog> EnduranceLogs => Set<EnduranceLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
