# Feature Landscape

**Domain:** Unified workout planner (strength + endurance)
**Researched:** 2026-03-21
**Confidence:** MEDIUM-HIGH (based on analysis of Hevy, Strong, Fitbod, TrainingPeaks, Runna, HYBRD, Edge, Strava, StrengthLog, and others)

---

## Table Stakes

Features users expect. Missing = product feels incomplete.

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Exercise library with search/filter | Every competitor has one; users need to find exercises fast by name, muscle group, equipment, or type (strength/endurance) | Medium | Hevy has 400+, TrainingPeaks 1000+. For a personal app, start with ~50 curated exercises + custom exercise creation. Filter by muscle group, equipment, and exercise type |
| Custom exercise creation | Users always have exercises not in the library | Low | Name, type (strength/endurance), muscle group, equipment, optional notes. Must integrate seamlessly with all tracking features |
| Workout templates (routines) | Core of every planning app; Hevy, Strong, TrainingPeaks all center on reusable templates | Medium | Ordered list of exercises with target parameters. Must support both strength targets (sets/reps/weight) and endurance targets (distance/duration/pace) |
| Superset grouping | Hevy, Strong, TrainingPeaks all support supersets. Users doing compound training expect this | Medium | Visual grouping with a vertical connector line (Hevy/Strong pattern). Two or more exercises grouped. Smart scrolling between superset exercises on set completion (Hevy's standout UX pattern) |
| Session logging (strength) | Core value prop of every strength tracker | High | Log sets with weight/reps, tap checkmark to complete set, show previous performance inline (Hevy/Strong pattern). Support warm-up, working, failure, and drop set types |
| Session logging (endurance) | Required for a unified app; without this you are just another strength tracker | Medium | Log distance, duration, pace (auto-calculated from distance+duration), optional HR data. Timer/stopwatch for duration-based sessions |
| Previous performance display | Hevy, Strong, StrengthLog all show "what you did last time" inline during logging | Medium | Critical for progressive overload. Show previous weight/reps next to each set entry. For endurance: show previous pace/distance |
| Rest timer | Hevy, Strong, Fitbod, Stronglifts all have auto-starting rest timers | Low-Medium | Auto-start on set completion. Adjustable per exercise or globally. Visual countdown with audio/vibration alert. +15/-15 second adjustment buttons |
| Personal record tracking | Hevy shows live PR notifications; Strong, RepCount, FitProgress all auto-detect PRs | Medium | Auto-detect PRs: weight PR, rep PR, volume PR, estimated 1RM PR for strength. Pace PR, distance PR for endurance. Celebrate with visual indicator during session |
| Weekly calendar view | TrainingPeaks, Runna, and scheduling-focused apps all use weekly as the primary view | Medium | Show scheduled workouts per day. Visual indicator for completed vs planned vs missed. Primary navigation for "what do I do today?" |
| RPE logging | Hevy, Strong, COROS, TrainingPeaks all support RPE per set or per session | Low | 1-10 scale (or 6-10 Borg scale). Optional per set for strength, per session for endurance. Displayed in history for trend analysis |
| Session notes | Nearly universal feature | Low | Free-text notes per session and per exercise. Visible during next session for context ("left shoulder felt tight", "ran on hilly route") |
| Workout history | Every app has a chronological log of completed sessions | Low-Medium | List of past sessions with date, exercises, volume summary. Tap to expand full detail. Searchable/filterable |
| Data export (CSV) | Strong, Hevy, FitNotes, TrainingPeaks all offer CSV export | Low | Export workout history to CSV. Essential for data ownership in a personal app |

## Differentiators

Features that set this product apart. Not expected by default, but highly valued.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| EMOM grouping construct | Most apps (Hevy, Strong, Fitbod) do NOT support EMOM natively. Users must use separate EMOM timer apps (PushPress, Box Timer). TrainingPeaks has no EMOM block. This is a genuine gap in the market | Medium-High | Define EMOM as a grouping construct in templates: N exercises, M-minute window per round, R rounds. During logging: countdown timer per minute, auto-advance through exercises. The user's actual training pattern (weighted pull-ups + dips EMOM) requires this |
| True planned-vs-actual tracking | Most apps are either planners OR loggers. Hevy shows "previous performance" but does not track deviation from a plan. TrainingPeaks has color-coded compliance but it is coach-focused. No consumer app does this well | Medium | Store planned targets separately from actual logs. Show deviation inline: planned 3x8@20kg, actual 3x8@22.5kg (+2.5kg). Adherence percentage per session and per week. This enables "did I do what I intended?" analysis |
| Unified strength + endurance analytics | The core thesis. Hevy is strength-only analytics. Runna is running-only. TrainingPeaks has both but is coach-focused and expensive. HYBRD is the closest but is mobile-only and AI-driven. No simple personal tool does this | High | Single dashboard showing: weekly volume (sets, weight lifted) alongside weekly distance/duration. Training load across both modalities. Streak/consistency metrics spanning both types. Muscle group heatmap + endurance trends side by side |
| Warm-up / cool-down blocks | TrainingPeaks has dedicated warm-up/cool-down blocks. ABC Trainerize does NOT have dedicated support. Most consumer apps (Hevy, Strong) have no concept of this -- warm-up sets exist but not warm-up exercises as a separate block | Low-Medium | Separate template sections marked as warm-up or cool-down. These exercises are tracked but excluded from working volume statistics. Prevents warm-up jog or mobility work from inflating analytics |
| Progressive overload suggestions | Fitbod does this with AI. Stronglifts auto-increments weight. Most apps only show data -- the user decides. A simple rule-based nudge is a sweet spot for a personal app | Medium | If target hit consistently (e.g., 3x8@20kg completed for 2+ sessions), suggest increase: "Ready for 22.5kg?" Not AI-driven -- simple threshold rules the user can configure. Show suggestion inline when starting the next session |
| Quick-start "today's workout" | Hevy and Strong require navigating to routines. Apple Watch apps surface the next workout. Runna opens directly to today's run. Reducing friction to start logging is a major UX win | Low | Home screen shows today's scheduled workout (from calendar). One action to start logging. If no workout scheduled, show "repeat last [type] workout" option |
| Monthly calendar overview | Complements the weekly view. Visual "did I train?" history. Color-coded dots (Hevy-style monthly report) | Low-Medium | Monthly grid showing training days with color coding: strength (one color), endurance (another), rest (empty). Tap a day to see session summary. Training density visible at a glance |
| Recurrence rules for scheduling | Runna adapts plans dynamically. TrainingPeaks has structured multi-week plans. For a personal planner: simple recurrence rules are sufficient and avoid over-engineering | Medium | "Every Monday", "Every other day", "3x/week on Mon/Wed/Fri". Rest day awareness: warn if scheduling on a day after a heavy session. Conflict detection: already have a workout scheduled |
| PDF export | Less common than CSV. Useful for printing a workout plan to bring to the gym as backup, or sharing a training summary | Low-Medium | Generate a formatted PDF of a workout template or a weekly/monthly training summary |

## Anti-Features

Features to explicitly NOT build. These are traps that add complexity without value for a personal single-user app.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| AI-generated workouts | Fitbod's core value prop, requires massive exercise data and algorithm tuning. Overkill for a personal app where the user knows what they want to train | Let the user build their own templates. Progressive overload suggestions (rule-based) provide the "smart" nudge without AI complexity |
| Social features / sharing | Hevy and Strava are social platforms. Social features require multi-user infrastructure, feeds, privacy controls, moderation. Out of scope for personal app | Single user. No feed, no followers, no sharing. Export covers "show someone my data" |
| Video exercise demos | Hevy, TrainingPeaks, Fitbod all embed exercise demo videos. Requires hosting, licensing, or recording hundreds of videos | Text descriptions and optional user-added notes per exercise. The user already knows how to do their exercises |
| Real-time GPS tracking | Strava, Runna, MapMyRun use phone GPS for live route/pace tracking. Requires background location services, battery optimization, map rendering | Manual entry of distance/duration/pace after an endurance session. GPS tracking is better served by dedicated running apps (Strava, Garmin). Future: import from Garmin (v2 scope) |
| Wearable integration (v1) | Garmin, Apple Watch, COROS integration requires API work, OAuth flows, data sync logic. Explicitly deferred to v2 in PROJECT.md | Manual data entry for v1. Clean data model that can accept imported data later |
| Heart rate streaming | Requires Bluetooth LE connection to chest strap or watch. Complex real-time data handling | Manual HR entry (average, max) as optional fields on endurance sessions |
| Nutrition / calorie tracking | MyFitnessPal territory. Completely different domain. Would dilute the core value prop | Out of scope entirely. Not even planned for v2 |
| Pre-built training programs | Runna sells 16-week marathon plans. TrainingPeaks has a marketplace. Content creation is a business, not a feature | User creates their own templates and schedules them. The app is a tool, not a content platform |
| Gamification (XP, badges, levels) | RepXP gamifies workouts. Fun but adds complexity and can feel patronizing for experienced athletes | PR celebrations and streak tracking provide natural motivation without artificial game mechanics |
| Body measurement tracking | Hevy includes weight/measurements/photos. Different tracking concern, adds UI surface area | Out of scope. Use a dedicated app or spreadsheet for body metrics |

## Feature Dependencies

```
Exercise Library ──────> Workout Templates ──────> Calendar Scheduler
       |                        |                        |
       |                        v                        v
       |                 Superset/EMOM            Recurrence Rules
       |                  Grouping                       |
       |                        |                        v
       |                        v                  Quick-Start
       |                 Session Tracker           "Today's Workout"
       |                   (Logging)
       |                   /        \
       |                  v          v
       |         Strength Log    Endurance Log
       |              |               |
       |              v               v
       |        Previous Perf    Previous Perf
       |         Display          Display
       |              |               |
       v              v               v
  Custom Exercises   PR Detection   RPE + Notes
                          |
                          v
                   Analytics Dashboard
                    /        |        \
                   v         v         v
             Volume     PR History   Adherence
             Trends    & Records    (Planned vs
                                     Actual)
                          |
                          v
                   Export (CSV/PDF)
```

**Key dependency chains:**

1. **Exercise Library** is the foundation -- everything references exercises
2. **Workout Templates** depend on the exercise library and define the grouping constructs (superset, EMOM, warm-up/cool-down)
3. **Session Tracker** depends on templates (to know what to log) and the exercise library (for metadata)
4. **Analytics** depend on logged session data accumulating over time
5. **Calendar/Scheduler** depends on templates (what to schedule) and enables Quick-Start
6. **Planned vs Actual** requires both the template targets AND the logged actuals to exist

## MVP Recommendation

Prioritize in this order:

1. **Exercise Library** (table stakes, foundation for everything)
   - Searchable catalog with type discriminator, muscle group, equipment
   - Custom exercise creation
   - Seed with the user's actual exercises (~30-50)

2. **Workout Templates** (table stakes, core planning)
   - Ordered exercise list with strength/endurance targets
   - Superset grouping (table stakes)
   - EMOM grouping (differentiator, but needed for user's actual training)
   - Warm-up/cool-down blocks (differentiator)

3. **Session Tracker / Logging** (table stakes, core value)
   - Strength: sets/reps/weight with checkmark completion
   - Endurance: distance/duration/pace entry
   - Previous performance inline
   - Rest timer
   - RPE and notes
   - Set types (warm-up, working, failure, drop)

4. **Calendar Scheduler** (table stakes, planning)
   - Weekly view (primary)
   - Schedule workouts with recurrence
   - Quick-start today's workout

5. **Analytics Dashboard** (table stakes, but depends on logged data)
   - Volume trends, PR tracking, streaks
   - Unified strength + endurance view (differentiator)
   - Planned vs actual adherence (differentiator)

6. **Export** (table stakes, but low urgency)
   - CSV export of all training data
   - PDF export of templates/summaries

**Defer to later iterations:**
- Progressive overload suggestions (needs session data to analyze)
- Monthly calendar overview (nice-to-have, weekly view covers the core need)

## Competitive Landscape Summary

| App | Strength | Endurance | Supersets | EMOM | Calendar | Planned vs Actual | Analytics |
|-----|----------|-----------|-----------|------|----------|-------------------|-----------|
| Hevy | Excellent | Basic (recent addition) | Yes (smart scrolling) | No | No calendar | No (shows previous only) | Strength-only |
| Strong | Excellent | Duration exercises only | Yes | No | No calendar | No | Strength-only |
| Fitbod | AI-driven | Conditioning only | Limited | No | No calendar | No | Strength-only |
| TrainingPeaks | Good (new) | Excellent | Yes (blocks) | No | Yes (full calendar) | Yes (compliance scores) | Both (coach-focused) |
| Runna | No | Excellent | N/A | N/A | Yes (plan view) | Basic | Running-only |
| HYBRD | Good | Good | Unknown | Unknown | Yes | Unknown | Both (AI-driven) |
| Edge | Good | Good | Unknown | Unknown | Yes | Unknown | Both (coach-led) |
| **This App** | **Target** | **Target** | **Yes** | **Yes** | **Yes** | **Yes** | **Both (personal)** |

**The gap this app fills:** No consumer-grade personal tool combines strength and endurance with superset/EMOM grouping, a real calendar scheduler, and planned-vs-actual deviation tracking. TrainingPeaks comes closest but is coach-oriented, subscription-based, and complex. Hevy and Strong are excellent strength loggers but weak on endurance and have no calendar/planning. Runna is running-only. The unified personal planner does not exist in the market.

## Sources

- [Hevy Features](https://www.hevyapp.com/features/) - Feature list, superset support, RPE, rest timer, analytics
- [Hevy Superset Scrolling](https://www.hevyapp.com/features/what-are-supersets/) - Smart superset scrolling UX pattern
- [Strong Supersets](https://help.strongapp.io/article/98-supersets-and-circuits) - Superset/circuit grouping in Strong
- [TrainingPeaks Strength Builder](https://help.trainingpeaks.com/hc/en-us/articles/21397126893581-Using-the-Strength-Workout-Builder) - Block structure: warm-up, superset, cool-down
- [TrainingPeaks Strength Announcement](https://www.prnewswire.com/news-releases/trainingpeaks-muscles-up-with-new-strength-feature-302206105.html) - Strength + endurance in one platform
- [Runna Features](https://www.runna.com) - Dynamic training plans, schedule flexibility
- [Best Hybrid Fitness Apps 2025](https://www.findyouredge.app/news/best-hybrid-fitness-apps-2025) - Hybrid training gap analysis
- [HYBRD App](https://www.hybrd.app/) - Combined strength + cardio tracking
- [Hevy RPE](https://www.hevyapp.com/features/how-to-calculate-rpe/) - RPE logging implementation
- [Strong RPE](https://help.strongapp.io/article/230-about-rpe) - RPE in Strong app
- [Hevy Rest Timer](https://www.hevyapp.com/features/workout-rest-timer/) - Auto rest timer UX
- [Strong Export](https://help.strongapp.io/article/235-export-workout-data) - CSV export pattern
- [ABC Trainerize Warm-up](https://help.trainerize.com/hc/en-us/articles/34824850269460-How-to-Build-Warm-Ups-Cool-Downs-and-Drop-Sets-into-Workouts) - Warm-up/cool-down limitations
- [PushPress EMOM Timer](https://www.pushpress.com/workout-timer/emom-timer) - EMOM timer features
- [Fitness App UX Failures](https://www.consagous.co/blog/from-download-to-delete-the-real-reasons-fitness-apps-fail-users) - Common UX complaints
- [Fitbod Algorithm](https://fitbod.zendesk.com/hc/en-us/articles/360004429814-How-Fitbod-Creates-Your-Workout) - AI workout generation approach
- [Stronglifts Progressive Overload](https://stronglifts.com/app/) - Auto weight increment pattern
