using BlazorApp2.Data.Entities;
using BlazorApp2.Data.Enums;

namespace BlazorApp2.Data.SeedData;

public static class ExerciseSeedData
{
    private static readonly DateTime SeedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public static StrengthExercise[] GetStrengthExercises() => new[]
    {
        // Back (6)
        new StrengthExercise
        {
            Id = 1, Name = "Pull-Up", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight,
            Description = "Hang from bar with overhand grip shoulder-width apart. Pull until chin clears bar. Control the descent.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 2, Name = "Chin-Up", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight,
            Description = "Hang from bar with underhand grip shoulder-width apart. Pull until chin clears bar. Emphasizes biceps more than pull-up.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 3, Name = "Weighted Pull-Up", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight,
            Description = "Perform pull-up with added weight via belt or vest. Maintain strict form without kipping.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 4, Name = "Inverted Row", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight,
            Description = "Hang beneath a low bar with feet on ground. Pull chest to bar keeping body rigid. Scale by adjusting body angle.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 5, Name = "Dumbbell Row", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Dumbbell,
            Description = "One hand and knee on bench, row dumbbell to hip. Keep elbow close to body. Squeeze shoulder blade at top.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 6, Name = "Dumbbell Pullover", MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Dumbbell,
            Description = "Lie across bench with single dumbbell held overhead. Lower behind head with slight elbow bend. Engage lats to return.",
            CreatedDate = SeedDate
        },

        // Chest (6)
        new StrengthExercise
        {
            Id = 7, Name = "Push-Up", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Bodyweight,
            Description = "Hands shoulder-width apart, body in plank. Lower chest to floor keeping elbows at 45 degrees. Push back up fully.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 8, Name = "Weighted Dip", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Bodyweight,
            Description = "On parallel bars, lean forward slightly for chest emphasis. Lower until shoulders are below elbows. Add weight via belt.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 9, Name = "Diamond Push-Up", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Bodyweight,
            Description = "Hands together forming diamond shape under chest. Lower and push up. Emphasizes inner chest and triceps.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 10, Name = "Dumbbell Bench Press", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Dumbbell,
            Description = "Lie flat on bench, press dumbbells from chest level to full extension. Allows greater range of motion than barbell.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 11, Name = "Dumbbell Fly", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Dumbbell,
            Description = "Lie flat on bench with dumbbells extended above chest. Lower with slight elbow bend in arc motion. Squeeze chest to return.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 12, Name = "Incline Push-Up (Feet Elevated)", MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Bodyweight,
            Description = "Place feet on elevated surface, hands on floor. Perform push-up targeting upper chest. Higher surface increases difficulty.",
            CreatedDate = SeedDate
        },

        // Shoulders (5)
        new StrengthExercise
        {
            Id = 13, Name = "Pike Push-Up", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Bodyweight,
            Description = "Start in downward dog position with hips high. Bend elbows to lower head toward floor. Press back up. Mimics overhead press.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 14, Name = "Dumbbell Overhead Press", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Dumbbell,
            Description = "Stand or sit with dumbbells at shoulder height. Press overhead to full lockout. Control the descent back to shoulders.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 15, Name = "Lateral Raise", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Dumbbell,
            Description = "Stand with dumbbells at sides. Raise arms laterally to shoulder height with slight elbow bend. Lower with control.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 16, Name = "Dumbbell Front Raise", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Dumbbell,
            Description = "Stand with dumbbells in front of thighs. Raise one or both arms forward to shoulder height. Lower slowly.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 17, Name = "Face Pull (Band)", MuscleGroup = MuscleGroup.Shoulders, Equipment = Equipment.Band,
            Description = "Anchor band at face height. Pull toward face with elbows high, externally rotating at end. Targets rear delts and rotator cuff.",
            CreatedDate = SeedDate
        },

        // Legs (8)
        new StrengthExercise
        {
            Id = 18, Name = "Bodyweight Squat", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Bodyweight,
            Description = "Stand shoulder-width apart. Sit back and down keeping chest up and knees tracking over toes. Descend to parallel or below.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 19, Name = "Bulgarian Split Squat", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Bodyweight,
            Description = "Rear foot elevated on bench. Lower front knee to 90 degrees keeping torso upright. Drive through front heel to stand.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 20, Name = "Pistol Squat", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Bodyweight,
            Description = "Stand on one leg, extend other leg forward. Squat down to full depth on single leg. Requires balance and mobility.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 21, Name = "Goblet Squat", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Kettlebell,
            Description = "Hold kettlebell at chest level with both hands. Squat to parallel or below keeping elbows inside knees. Stand tall at top.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 22, Name = "Romanian Deadlift (Dumbbell)", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Dumbbell,
            Description = "Hold dumbbells in front of thighs. Hinge at hips pushing them back, slight knee bend. Lower until hamstring stretch, drive hips forward.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 23, Name = "Lunge", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Bodyweight,
            Description = "Step forward into split stance. Lower back knee toward floor. Push through front heel to return. Alternate legs.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 24, Name = "Calf Raise", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Bodyweight,
            Description = "Stand on edge of step with heels hanging off. Rise onto toes as high as possible. Lower below platform level for full stretch.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 25, Name = "Step-Up", MuscleGroup = MuscleGroup.Legs, Equipment = Equipment.Bodyweight,
            Description = "Step onto a sturdy elevated surface leading with one foot. Drive through heel to stand fully. Step down with control.",
            CreatedDate = SeedDate
        },

        // Arms (5)
        new StrengthExercise
        {
            Id = 26, Name = "Dumbbell Curl", MuscleGroup = MuscleGroup.Arms, Equipment = Equipment.Dumbbell,
            Description = "Stand with dumbbells at sides, palms forward. Curl to shoulders keeping elbows pinned to sides. Lower with control.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 27, Name = "Hammer Curl", MuscleGroup = MuscleGroup.Arms, Equipment = Equipment.Dumbbell,
            Description = "Stand with dumbbells at sides, palms facing each other. Curl to shoulders maintaining neutral grip. Targets brachialis.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 28, Name = "Tricep Dip", MuscleGroup = MuscleGroup.Arms, Equipment = Equipment.Bodyweight,
            Description = "On parallel bars or bench edge, lower body by bending elbows to 90 degrees. Keep torso upright for tricep emphasis. Press up.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 29, Name = "Overhead Tricep Extension", MuscleGroup = MuscleGroup.Arms, Equipment = Equipment.Dumbbell,
            Description = "Hold single dumbbell overhead with both hands. Lower behind head by bending elbows. Extend back to lockout. Keep elbows close.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 30, Name = "Concentration Curl", MuscleGroup = MuscleGroup.Arms, Equipment = Equipment.Dumbbell,
            Description = "Sit on bench, brace elbow against inner thigh. Curl dumbbell to shoulder. Isolates the bicep with strict form.",
            CreatedDate = SeedDate
        },

        // Core (4)
        new StrengthExercise
        {
            Id = 31, Name = "Plank", MuscleGroup = MuscleGroup.Core, Equipment = Equipment.Bodyweight,
            Description = "Forearms and toes on floor, body in straight line. Brace core and hold position. Avoid sagging hips or piking up.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 32, Name = "Hanging Leg Raise", MuscleGroup = MuscleGroup.Core, Equipment = Equipment.Bodyweight,
            Description = "Hang from pull-up bar with straight arms. Raise legs to parallel or higher. Lower with control, avoid swinging.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 33, Name = "Ab Wheel Rollout", MuscleGroup = MuscleGroup.Core, Equipment = Equipment.Other,
            Description = "Kneel with ab wheel in front. Roll forward extending body as far as possible. Use core to pull back to start position.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 34, Name = "Russian Twist", MuscleGroup = MuscleGroup.Core, Equipment = Equipment.Bodyweight,
            Description = "Sit with knees bent and feet off floor. Lean back slightly and rotate torso side to side. Add weight for more resistance.",
            CreatedDate = SeedDate
        },

        // FullBody (3)
        new StrengthExercise
        {
            Id = 35, Name = "Burpee", MuscleGroup = MuscleGroup.FullBody, Equipment = Equipment.Bodyweight,
            Description = "From standing, drop to push-up position, perform push-up, jump feet to hands, explode upward. Full body conditioning movement.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 36, Name = "Turkish Get-Up", MuscleGroup = MuscleGroup.FullBody, Equipment = Equipment.Kettlebell,
            Description = "Lie on floor holding kettlebell overhead. Stand up through a series of positions while keeping weight locked out. Reverse to return.",
            CreatedDate = SeedDate
        },
        new StrengthExercise
        {
            Id = 37, Name = "Kettlebell Swing", MuscleGroup = MuscleGroup.FullBody, Equipment = Equipment.Kettlebell,
            Description = "Hinge at hips with kettlebell between legs. Drive hips forward explosively to swing weight to chest height. Control the backswing.",
            CreatedDate = SeedDate
        },
    };

    public static EnduranceExercise[] GetEnduranceExercises() => new[]
    {
        // Run (7)
        new EnduranceExercise
        {
            Id = 101, Name = "Easy Run", ActivityType = ActivityType.Run,
            Description = "Conversational pace. You should be able to hold a full conversation. Keep heart rate in Zone 2.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 102, Name = "Tempo Run", ActivityType = ActivityType.Run,
            Description = "Comfortably hard pace you could sustain for about an hour. Breathing is labored but controlled. Zone 3-4.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 103, Name = "Interval Run", ActivityType = ActivityType.Run,
            Description = "Alternate between hard efforts and recovery jogs. Work intervals at Zone 4-5. Recovery intervals at easy pace.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 104, Name = "Long Run", ActivityType = ActivityType.Run,
            Description = "Extended duration at easy to moderate pace. Build aerobic base and mental endurance. Longest run of the week.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 105, Name = "Fartlek Run", ActivityType = ActivityType.Run,
            Description = "Unstructured speed play. Vary pace throughout the run based on feel. Mix fast bursts with easy recovery periods.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 106, Name = "Hill Sprints", ActivityType = ActivityType.Run,
            Description = "Short all-out sprints up a steep hill. Walk or jog back down for recovery. Builds power and running economy.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 107, Name = "Recovery Run", ActivityType = ActivityType.Run,
            Description = "Very easy pace, shorter duration. Promotes blood flow for recovery. Should feel effortless. Zone 1-2.",
            CreatedDate = SeedDate
        },

        // Cycle (5)
        new EnduranceExercise
        {
            Id = 108, Name = "Easy Ride", ActivityType = ActivityType.Cycle,
            Description = "Low intensity cycling at conversational pace. Steady cadence of 80-90 RPM. Zone 2 effort for aerobic base building.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 109, Name = "Tempo Ride", ActivityType = ActivityType.Cycle,
            Description = "Sustained effort at threshold pace. Breathing is heavy but sustainable. Maintain steady power output in Zone 3-4.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 110, Name = "Interval Ride", ActivityType = ActivityType.Cycle,
            Description = "High-intensity intervals with recovery periods. Work intervals at Zone 4-5 power. Recovery at easy spinning.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 111, Name = "Long Ride", ActivityType = ActivityType.Cycle,
            Description = "Extended duration ride at moderate pace. Build endurance and fat adaptation. Longest ride of the week.",
            CreatedDate = SeedDate
        },
        new EnduranceExercise
        {
            Id = 112, Name = "Hill Climb Ride", ActivityType = ActivityType.Cycle,
            Description = "Focus on sustained climbing efforts. Lower cadence, higher resistance. Builds leg strength and climbing fitness.",
            CreatedDate = SeedDate
        },

        // Walk (1)
        new EnduranceExercise
        {
            Id = 113, Name = "Brisk Walk", ActivityType = ActivityType.Walk,
            Description = "Purposeful walking at an elevated pace. Arm swing engaged, stride slightly longer than normal. Low-impact active recovery.",
            CreatedDate = SeedDate
        },
    };
}
