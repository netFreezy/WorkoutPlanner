# Requirements: Unified Workout Planner

**Defined:** 2026-03-21
**Core Value:** A single system where you plan, log, and analyze both strength and endurance training side by side

## v1 Requirements

### Data Foundation

- [ ] **DATA-01**: EF Core with SQLite using `IDbContextFactory` pattern (not scoped DbContext)
- [ ] **DATA-02**: Exercise entity with TPH inheritance — StrengthExercise and EnduranceExercise subtypes with type-specific metadata (muscle group, equipment, activity type)
- [ ] **DATA-03**: WorkoutTemplate entity with ordered TemplateItems supporting both strength targets (sets/reps/weight) and endurance targets (distance/duration/pace/HR zone)
- [ ] **DATA-04**: Superset and EMOM grouping constructs within templates
- [ ] **DATA-05**: Warm-up and cool-down block sections in templates (excluded from working volume stats)
- [ ] **DATA-06**: ScheduledWorkout entity with date, status (planned/completed/skipped), and template snapshot
- [ ] **DATA-07**: RecurrenceRule support — every X days, specific weekdays, every other day
- [ ] **DATA-08**: WorkoutLog entity with planned-vs-actual separation — snapshot planned targets at session creation
- [ ] **DATA-09**: Strength log entries: actual sets with reps and weight per set, set type (warm-up/working/failure/drop)
- [ ] **DATA-10**: Endurance log entries: actual distance, duration, pace, optional HR data

### Exercise Library

- [ ] **EXER-01**: Searchable, filterable exercise catalog by name, type, muscle group, and equipment
- [ ] **EXER-02**: Custom exercise creation with name, type, muscle group, equipment, optional notes
- [ ] **EXER-03**: Seed database with ~50 common exercises across strength and endurance

### Workout Templates

- [ ] **TMPL-01**: Template builder — create named templates with ordered exercise list
- [ ] **TMPL-02**: Reorderable exercises within templates (drag-and-drop or move up/down)
- [ ] **TMPL-03**: Strength targets per exercise: target sets, reps, weight
- [ ] **TMPL-04**: Endurance targets per exercise: target distance, duration, pace, HR zone
- [ ] **TMPL-05**: Superset grouping — visually group 2+ exercises with connector
- [ ] **TMPL-06**: EMOM grouping — N exercises, M-minute window per round, R rounds
- [ ] **TMPL-07**: Warm-up and cool-down sections separate from working sets

### Calendar & Scheduling

- [ ] **SCHED-01**: Weekly calendar view (primary) showing scheduled workouts with type indicators
- [ ] **SCHED-02**: Monthly calendar overview with color-coded dots for workout types
- [ ] **SCHED-03**: Schedule workouts from templates or ad-hoc on specific dates
- [ ] **SCHED-04**: Recurrence rules: every Monday, every other day, 3x/week on specific days
- [ ] **SCHED-05**: Rest day awareness — flag conflicts when scheduling same muscle group on consecutive days
- [ ] **SCHED-06**: Materialize scheduled workout rows from recurrence rules (rolling window)

### Session Tracking

- [ ] **SESS-01**: Start logging from a scheduled workout — open template with targets pre-filled
- [ ] **SESS-02**: Strength logging: tap through sets, enter weight and reps, checkmark to complete
- [ ] **SESS-03**: Endurance logging: timer/stopwatch with distance and pace entry
- [ ] **SESS-04**: Previous performance displayed inline for each exercise
- [ ] **SESS-05**: Mark exercises as completed, partially completed, or skipped
- [ ] **SESS-06**: Rest timer — auto-start on set completion, adjustable duration
- [ ] **SESS-07**: RPE rating (1-10) per session
- [ ] **SESS-08**: Free-text session notes
- [ ] **SESS-09**: Incremental persistence — save progress to DB during logging (circuit death resilience)
- [ ] **SESS-10**: Resume incomplete session after connection loss

### Analytics

- [ ] **ANLY-01**: Volume trends — total sets and total weight lifted per week over time
- [ ] **ANLY-02**: PR tracking with automatic detection (weight PR, rep PR, estimated 1RM, pace PR, distance PR)
- [ ] **ANLY-03**: Streak and consistency metrics — X workouts completed out of Y planned per week/month
- [ ] **ANLY-04**: Endurance trends — pace and distance per week over time
- [ ] **ANLY-05**: Planned vs. actual adherence — deviation display per session and over time

### Quality of Life

- [ ] **QOL-01**: Quick-start — home screen shows today's scheduled workout, one action to start logging
- [ ] **QOL-02**: "Repeat last workout" option when no workout scheduled
- [ ] **QOL-03**: Progressive overload suggestions — nudge to increase weight when target hit consistently
- [ ] **QOL-04**: CSV export of all training data
- [ ] **QOL-05**: PDF export of workout templates and training summaries
- [ ] **QOL-06**: Workout history — chronological list of completed sessions with search/filter

## v2 Requirements

### Wearable Integration

- **WEAR-01**: Garmin Connect API integration to import workout data
- **WEAR-02**: Auto-populate endurance logs from Garmin activity data

## Out of Scope

| Feature | Reason |
|---------|--------|
| AI-generated workouts | Personal app — user knows what to train |
| Social features / sharing | Single user, no multi-user infrastructure |
| Video exercise demos | User already knows exercises |
| Real-time GPS tracking | Use Strava/Garmin for that, import later |
| Heart rate streaming (BLE) | Manual HR entry sufficient |
| Nutrition / calorie tracking | Different domain entirely |
| Pre-built training programs | User builds own templates |
| Gamification (XP, badges) | PR celebrations + streaks provide natural motivation |
| Body measurement tracking | Out of scope, use dedicated app |
| Authentication / multi-user | Personal app |
| Cloud sync | Local SQLite, backup via file copy |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| DATA-01 | Phase 1 | Pending |
| DATA-02 | Phase 1 | Pending |
| DATA-03 | Phase 1 | Pending |
| DATA-04 | Phase 1 | Pending |
| DATA-05 | Phase 1 | Pending |
| DATA-06 | Phase 1 | Pending |
| DATA-07 | Phase 1 | Pending |
| DATA-08 | Phase 1 | Pending |
| DATA-09 | Phase 1 | Pending |
| DATA-10 | Phase 1 | Pending |
| EXER-01 | Phase 2 | Pending |
| EXER-02 | Phase 2 | Pending |
| EXER-03 | Phase 2 | Pending |
| TMPL-01 | Phase 3 | Pending |
| TMPL-02 | Phase 3 | Pending |
| TMPL-03 | Phase 3 | Pending |
| TMPL-04 | Phase 3 | Pending |
| TMPL-05 | Phase 3 | Pending |
| TMPL-06 | Phase 3 | Pending |
| TMPL-07 | Phase 3 | Pending |
| SCHED-01 | Phase 4 | Pending |
| SCHED-02 | Phase 4 | Pending |
| SCHED-03 | Phase 4 | Pending |
| SCHED-04 | Phase 4 | Pending |
| SCHED-05 | Phase 4 | Pending |
| SCHED-06 | Phase 4 | Pending |
| SESS-01 | Phase 5 | Pending |
| SESS-02 | Phase 5 | Pending |
| SESS-03 | Phase 5 | Pending |
| SESS-04 | Phase 5 | Pending |
| SESS-05 | Phase 5 | Pending |
| SESS-06 | Phase 5 | Pending |
| SESS-07 | Phase 5 | Pending |
| SESS-08 | Phase 5 | Pending |
| SESS-09 | Phase 5 | Pending |
| SESS-10 | Phase 5 | Pending |
| ANLY-01 | Phase 6 | Pending |
| ANLY-02 | Phase 6 | Pending |
| ANLY-03 | Phase 6 | Pending |
| ANLY-04 | Phase 6 | Pending |
| ANLY-05 | Phase 6 | Pending |
| QOL-01 | Phase 7 | Pending |
| QOL-02 | Phase 7 | Pending |
| QOL-03 | Phase 7 | Pending |
| QOL-04 | Phase 7 | Pending |
| QOL-05 | Phase 7 | Pending |
| QOL-06 | Phase 7 | Pending |

**Coverage:**
- v1 requirements: 47 total
- Mapped to phases: 47
- Unmapped: 0

---
*Requirements defined: 2026-03-21*
*Last updated: 2026-03-21 after roadmap creation*
