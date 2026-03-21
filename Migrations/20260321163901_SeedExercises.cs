using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BlazorApp2.Migrations
{
    /// <inheritdoc />
    public partial class SeedExercises : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "CreatedDate", "Description", "Equipment", "ExerciseType", "MuscleGroup", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hang from bar with overhand grip shoulder-width apart. Pull until chin clears bar. Control the descent.", 2, "Strength", 1, "Pull-Up" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hang from bar with underhand grip shoulder-width apart. Pull until chin clears bar. Emphasizes biceps more than pull-up.", 2, "Strength", 1, "Chin-Up" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Perform pull-up with added weight via belt or vest. Maintain strict form without kipping.", 2, "Strength", 1, "Weighted Pull-Up" },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hang beneath a low bar with feet on ground. Pull chest to bar keeping body rigid. Scale by adjusting body angle.", 2, "Strength", 1, "Inverted Row" },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "One hand and knee on bench, row dumbbell to hip. Keep elbow close to body. Squeeze shoulder blade at top.", 1, "Strength", 1, "Dumbbell Row" },
                    { 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lie across bench with single dumbbell held overhead. Lower behind head with slight elbow bend. Engage lats to return.", 1, "Strength", 1, "Dumbbell Pullover" },
                    { 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hands shoulder-width apart, body in plank. Lower chest to floor keeping elbows at 45 degrees. Push back up fully.", 2, "Strength", 0, "Push-Up" },
                    { 8, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "On parallel bars, lean forward slightly for chest emphasis. Lower until shoulders are below elbows. Add weight via belt.", 2, "Strength", 0, "Weighted Dip" },
                    { 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hands together forming diamond shape under chest. Lower and push up. Emphasizes inner chest and triceps.", 2, "Strength", 0, "Diamond Push-Up" },
                    { 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lie flat on bench, press dumbbells from chest level to full extension. Allows greater range of motion than barbell.", 1, "Strength", 0, "Dumbbell Bench Press" },
                    { 11, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lie flat on bench with dumbbells extended above chest. Lower with slight elbow bend in arc motion. Squeeze chest to return.", 1, "Strength", 0, "Dumbbell Fly" },
                    { 12, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Place feet on elevated surface, hands on floor. Perform push-up targeting upper chest. Higher surface increases difficulty.", 2, "Strength", 0, "Incline Push-Up (Feet Elevated)" },
                    { 13, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Start in downward dog position with hips high. Bend elbows to lower head toward floor. Press back up. Mimics overhead press.", 2, "Strength", 2, "Pike Push-Up" },
                    { 14, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand or sit with dumbbells at shoulder height. Press overhead to full lockout. Control the descent back to shoulders.", 1, "Strength", 2, "Dumbbell Overhead Press" },
                    { 15, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand with dumbbells at sides. Raise arms laterally to shoulder height with slight elbow bend. Lower with control.", 1, "Strength", 2, "Lateral Raise" },
                    { 16, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand with dumbbells in front of thighs. Raise one or both arms forward to shoulder height. Lower slowly.", 1, "Strength", 2, "Dumbbell Front Raise" },
                    { 17, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Anchor band at face height. Pull toward face with elbows high, externally rotating at end. Targets rear delts and rotator cuff.", 5, "Strength", 2, "Face Pull (Band)" },
                    { 18, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand shoulder-width apart. Sit back and down keeping chest up and knees tracking over toes. Descend to parallel or below.", 2, "Strength", 3, "Bodyweight Squat" },
                    { 19, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Rear foot elevated on bench. Lower front knee to 90 degrees keeping torso upright. Drive through front heel to stand.", 2, "Strength", 3, "Bulgarian Split Squat" },
                    { 20, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand on one leg, extend other leg forward. Squat down to full depth on single leg. Requires balance and mobility.", 2, "Strength", 3, "Pistol Squat" },
                    { 21, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hold kettlebell at chest level with both hands. Squat to parallel or below keeping elbows inside knees. Stand tall at top.", 6, "Strength", 3, "Goblet Squat" },
                    { 22, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hold dumbbells in front of thighs. Hinge at hips pushing them back, slight knee bend. Lower until hamstring stretch, drive hips forward.", 1, "Strength", 3, "Romanian Deadlift (Dumbbell)" },
                    { 23, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Step forward into split stance. Lower back knee toward floor. Push through front heel to return. Alternate legs.", 2, "Strength", 3, "Lunge" },
                    { 24, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand on edge of step with heels hanging off. Rise onto toes as high as possible. Lower below platform level for full stretch.", 2, "Strength", 3, "Calf Raise" },
                    { 25, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Step onto a sturdy elevated surface leading with one foot. Drive through heel to stand fully. Step down with control.", 2, "Strength", 3, "Step-Up" },
                    { 26, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand with dumbbells at sides, palms forward. Curl to shoulders keeping elbows pinned to sides. Lower with control.", 1, "Strength", 4, "Dumbbell Curl" },
                    { 27, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Stand with dumbbells at sides, palms facing each other. Curl to shoulders maintaining neutral grip. Targets brachialis.", 1, "Strength", 4, "Hammer Curl" },
                    { 28, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "On parallel bars or bench edge, lower body by bending elbows to 90 degrees. Keep torso upright for tricep emphasis. Press up.", 2, "Strength", 4, "Tricep Dip" },
                    { 29, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hold single dumbbell overhead with both hands. Lower behind head by bending elbows. Extend back to lockout. Keep elbows close.", 1, "Strength", 4, "Overhead Tricep Extension" },
                    { 30, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sit on bench, brace elbow against inner thigh. Curl dumbbell to shoulder. Isolates the bicep with strict form.", 1, "Strength", 4, "Concentration Curl" },
                    { 31, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Forearms and toes on floor, body in straight line. Brace core and hold position. Avoid sagging hips or piking up.", 2, "Strength", 5, "Plank" },
                    { 32, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hang from pull-up bar with straight arms. Raise legs to parallel or higher. Lower with control, avoid swinging.", 2, "Strength", 5, "Hanging Leg Raise" },
                    { 33, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kneel with ab wheel in front. Roll forward extending body as far as possible. Use core to pull back to start position.", 7, "Strength", 5, "Ab Wheel Rollout" },
                    { 34, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sit with knees bent and feet off floor. Lean back slightly and rotate torso side to side. Add weight for more resistance.", 2, "Strength", 5, "Russian Twist" },
                    { 35, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "From standing, drop to push-up position, perform push-up, jump feet to hands, explode upward. Full body conditioning movement.", 2, "Strength", 6, "Burpee" },
                    { 36, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lie on floor holding kettlebell overhead. Stand up through a series of positions while keeping weight locked out. Reverse to return.", 6, "Strength", 6, "Turkish Get-Up" },
                    { 37, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hinge at hips with kettlebell between legs. Drive hips forward explosively to swing weight to chest height. Control the backswing.", 6, "Strength", 6, "Kettlebell Swing" }
                });

            migrationBuilder.InsertData(
                table: "Exercises",
                columns: new[] { "Id", "ActivityType", "CreatedDate", "Description", "ExerciseType", "Name" },
                values: new object[,]
                {
                    { 101, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Conversational pace. You should be able to hold a full conversation. Keep heart rate in Zone 2.", "Endurance", "Easy Run" },
                    { 102, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Comfortably hard pace you could sustain for about an hour. Breathing is labored but controlled. Zone 3-4.", "Endurance", "Tempo Run" },
                    { 103, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Alternate between hard efforts and recovery jogs. Work intervals at Zone 4-5. Recovery intervals at easy pace.", "Endurance", "Interval Run" },
                    { 104, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Extended duration at easy to moderate pace. Build aerobic base and mental endurance. Longest run of the week.", "Endurance", "Long Run" },
                    { 105, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Unstructured speed play. Vary pace throughout the run based on feel. Mix fast bursts with easy recovery periods.", "Endurance", "Fartlek Run" },
                    { 106, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Short all-out sprints up a steep hill. Walk or jog back down for recovery. Builds power and running economy.", "Endurance", "Hill Sprints" },
                    { 107, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Very easy pace, shorter duration. Promotes blood flow for recovery. Should feel effortless. Zone 1-2.", "Endurance", "Recovery Run" },
                    { 108, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Low intensity cycling at conversational pace. Steady cadence of 80-90 RPM. Zone 2 effort for aerobic base building.", "Endurance", "Easy Ride" },
                    { 109, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sustained effort at threshold pace. Breathing is heavy but sustainable. Maintain steady power output in Zone 3-4.", "Endurance", "Tempo Ride" },
                    { 110, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "High-intensity intervals with recovery periods. Work intervals at Zone 4-5 power. Recovery at easy spinning.", "Endurance", "Interval Ride" },
                    { 111, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Extended duration ride at moderate pace. Build endurance and fat adaptation. Longest ride of the week.", "Endurance", "Long Ride" },
                    { 112, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Focus on sustained climbing efforts. Lower cadence, higher resistance. Builds leg strength and climbing fitness.", "Endurance", "Hill Climb Ride" },
                    { 113, 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Purposeful walking at an elevated pace. Arm swing engaged, stride slightly longer than normal. Low-impact active recovery.", "Endurance", "Brisk Walk" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 106);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 107);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 108);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 109);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 110);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 111);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 112);

            migrationBuilder.DeleteData(
                table: "Exercises",
                keyColumn: "Id",
                keyValue: 113);
        }
    }
}
