using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlazorApp2.Migrations
{
    /// <inheritdoc />
    public partial class AddAdHocWorkoutSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurrenceRules_WorkoutTemplates_WorkoutTemplateId",
                table: "RecurrenceRules");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledWorkouts_WorkoutTemplates_WorkoutTemplateId",
                table: "ScheduledWorkouts");

            migrationBuilder.AlterColumn<int>(
                name: "WorkoutTemplateId",
                table: "ScheduledWorkouts",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "AdHocName",
                table: "ScheduledWorkouts",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WorkoutTemplateId",
                table: "RecurrenceRules",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "AdHocName",
                table: "RecurrenceRules",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "RecurrenceRules",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_RecurrenceRules_WorkoutTemplates_WorkoutTemplateId",
                table: "RecurrenceRules",
                column: "WorkoutTemplateId",
                principalTable: "WorkoutTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledWorkouts_WorkoutTemplates_WorkoutTemplateId",
                table: "ScheduledWorkouts",
                column: "WorkoutTemplateId",
                principalTable: "WorkoutTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecurrenceRules_WorkoutTemplates_WorkoutTemplateId",
                table: "RecurrenceRules");

            migrationBuilder.DropForeignKey(
                name: "FK_ScheduledWorkouts_WorkoutTemplates_WorkoutTemplateId",
                table: "ScheduledWorkouts");

            migrationBuilder.DropColumn(
                name: "AdHocName",
                table: "ScheduledWorkouts");

            migrationBuilder.DropColumn(
                name: "AdHocName",
                table: "RecurrenceRules");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "RecurrenceRules");

            migrationBuilder.AlterColumn<int>(
                name: "WorkoutTemplateId",
                table: "ScheduledWorkouts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WorkoutTemplateId",
                table: "RecurrenceRules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RecurrenceRules_WorkoutTemplates_WorkoutTemplateId",
                table: "RecurrenceRules",
                column: "WorkoutTemplateId",
                principalTable: "WorkoutTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ScheduledWorkouts_WorkoutTemplates_WorkoutTemplateId",
                table: "ScheduledWorkouts",
                column: "WorkoutTemplateId",
                principalTable: "WorkoutTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
