using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp2.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExerciseType = table.Column<string>(type: "TEXT", maxLength: 13, nullable: false),
                    ActivityType = table.Column<int>(type: "INTEGER", nullable: true),
                    MuscleGroup = table.Column<int>(type: "INTEGER", nullable: true),
                    Equipment = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecurrenceRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    FrequencyType = table.Column<int>(type: "INTEGER", nullable: false),
                    Interval = table.Column<int>(type: "INTEGER", nullable: false),
                    DaysOfWeek = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurrenceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurrenceRules_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    GroupType = table.Column<int>(type: "INTEGER", nullable: false),
                    Rounds = table.Column<int>(type: "INTEGER", nullable: true),
                    MinuteWindow = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateGroups_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledWorkouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduledDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkoutTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecurrenceRuleId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledWorkouts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledWorkouts_RecurrenceRules_RecurrenceRuleId",
                        column: x => x.RecurrenceRuleId,
                        principalTable: "RecurrenceRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ScheduledWorkouts_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplateItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    Position = table.Column<int>(type: "INTEGER", nullable: false),
                    SectionType = table.Column<int>(type: "INTEGER", nullable: false),
                    TemplateGroupId = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetSets = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetReps = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetWeight = table.Column<double>(type: "REAL", nullable: true),
                    TargetDistance = table.Column<double>(type: "REAL", nullable: true),
                    TargetDurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    TargetPace = table.Column<double>(type: "REAL", nullable: true),
                    TargetHeartRateZone = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateItems_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplateItems_TemplateGroups_TemplateGroupId",
                        column: x => x.TemplateGroupId,
                        principalTable: "TemplateGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TemplateItems_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ScheduledWorkoutId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Rpe = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_ScheduledWorkouts_ScheduledWorkoutId",
                        column: x => x.ScheduledWorkoutId,
                        principalTable: "ScheduledWorkouts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnduranceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutLogId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    ActivityType = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedDistance = table.Column<double>(type: "REAL", nullable: true),
                    PlannedDurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    PlannedPace = table.Column<double>(type: "REAL", nullable: true),
                    PlannedHeartRateZone = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualDistance = table.Column<double>(type: "REAL", nullable: true),
                    ActualDurationSeconds = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualPace = table.Column<double>(type: "REAL", nullable: true),
                    ActualHeartRateZone = table.Column<int>(type: "INTEGER", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnduranceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnduranceLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EnduranceLogs_WorkoutLogs_WorkoutLogId",
                        column: x => x.WorkoutLogId,
                        principalTable: "WorkoutLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WorkoutLogId = table.Column<int>(type: "INTEGER", nullable: false),
                    ExerciseId = table.Column<int>(type: "INTEGER", nullable: false),
                    SetNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    SetType = table.Column<int>(type: "INTEGER", nullable: false),
                    PlannedReps = table.Column<int>(type: "INTEGER", nullable: true),
                    PlannedWeight = table.Column<double>(type: "REAL", nullable: true),
                    ActualReps = table.Column<int>(type: "INTEGER", nullable: true),
                    ActualWeight = table.Column<double>(type: "REAL", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetLogs_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SetLogs_WorkoutLogs_WorkoutLogId",
                        column: x => x.WorkoutLogId,
                        principalTable: "WorkoutLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnduranceLogs_ExerciseId",
                table: "EnduranceLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_EnduranceLogs_WorkoutLogId",
                table: "EnduranceLogs",
                column: "WorkoutLogId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RecurrenceRules_WorkoutTemplateId",
                table: "RecurrenceRules",
                column: "WorkoutTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledWorkouts_RecurrenceRuleId",
                table: "ScheduledWorkouts",
                column: "RecurrenceRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledWorkouts_ScheduledDate",
                table: "ScheduledWorkouts",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledWorkouts_WorkoutTemplateId",
                table: "ScheduledWorkouts",
                column: "WorkoutTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SetLogs_ExerciseId",
                table: "SetLogs",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_SetLogs_WorkoutLogId",
                table: "SetLogs",
                column: "WorkoutLogId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateGroups_WorkoutTemplateId",
                table: "TemplateGroups",
                column: "WorkoutTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_ExerciseId",
                table: "TemplateItems",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_TemplateGroupId",
                table: "TemplateItems",
                column: "TemplateGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateItems_WorkoutTemplateId_Position",
                table: "TemplateItems",
                columns: new[] { "WorkoutTemplateId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_ScheduledWorkoutId",
                table: "WorkoutLogs",
                column: "ScheduledWorkoutId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnduranceLogs");

            migrationBuilder.DropTable(
                name: "SetLogs");

            migrationBuilder.DropTable(
                name: "TemplateItems");

            migrationBuilder.DropTable(
                name: "WorkoutLogs");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "TemplateGroups");

            migrationBuilder.DropTable(
                name: "ScheduledWorkouts");

            migrationBuilder.DropTable(
                name: "RecurrenceRules");

            migrationBuilder.DropTable(
                name: "WorkoutTemplates");
        }
    }
}
