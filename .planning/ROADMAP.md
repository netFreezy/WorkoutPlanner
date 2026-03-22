# Roadmap: Unified Workout Planner

## Overview

Build a personal workout planner that unifies strength and endurance training in a single Blazor Server app. The build follows a strict dependency chain: data model first, then exercise catalog, then templates, then calendar scheduling, then session logging, then analytics, and finally quality-of-life polish. Each phase delivers a complete, verifiable capability that the next phase builds on.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [ ] **Phase 1: Data Foundation** - EF Core data model with full entity hierarchy, migrations, and SQLite persistence
- [ ] **Phase 2: Exercise Library** - Searchable exercise catalog with custom exercise creation
- [ ] **Phase 3: Workout Templates** - Template builder with strength/endurance targets, superset/EMOM grouping, and warm-up/cool-down sections
- [ ] **Phase 4: Calendar & Scheduling** - Weekly/monthly calendar views with recurrence rules and conflict detection
- [ ] **Phase 5: Session Tracking** - Real-time workout logging with incremental persistence and circuit-death resilience
- [ ] **Phase 6: Analytics** - Unified strength and endurance analytics with PR tracking and adherence metrics
- [ ] **Phase 7: Quality of Life** - Quick-start, progressive overload suggestions, export, and workout history

## Phase Details

### Phase 1: Data Foundation
**Goal**: The complete data model and persistence layer exists so all subsequent features can store and retrieve data correctly
**Depends on**: Nothing (first phase)
**Requirements**: DATA-01, DATA-02, DATA-03, DATA-04, DATA-05, DATA-06, DATA-07, DATA-08, DATA-09, DATA-10
**Success Criteria** (what must be TRUE):
  1. AppDbContext uses IDbContextFactory pattern and SQLite with WAL mode -- a component can create, use, and dispose a DbContext without threading violations
  2. Exercise entity hierarchy (StrengthExercise, EnduranceExercise) persists and queries correctly via TPH inheritance with type-specific metadata
  3. WorkoutTemplate with ordered TemplateItems, grouping constructs (superset/EMOM), and section types (warm-up/working/cool-down) round-trips through the database
  4. ScheduledWorkout with RecurrenceRule and WorkoutLog with planned-vs-actual separation (strength SetLog, endurance EnduranceLog) persist and retrieve correctly
  5. EF Core migrations apply cleanly to a fresh SQLite database
**Plans:** 2/2 plans executed

Plans:
- [x] 01-01-PLAN.md — Entity model, enums, DbContext, fluent configurations, and initial migration
- [x] 01-02-PLAN.md — xunit test project with integration tests verifying all DATA requirements

### Phase 2: Exercise Library
**Goal**: Users can browse, search, and filter a catalog of exercises and add their own custom exercises
**Depends on**: Phase 1
**Requirements**: EXER-01, EXER-02, EXER-03
**Success Criteria** (what must be TRUE):
  1. User can browse a pre-loaded catalog of ~50 exercises spanning both strength and endurance types
  2. User can search exercises by name and filter by type (strength/endurance), muscle group, and equipment
  3. User can create a custom exercise with name, type, muscle group, equipment, and optional notes, and it appears in the catalog immediately
**Plans:** 3/4 plans executed

Plans:
- [x] 02-01-PLAN.md — Seed ~50 exercises into database via EF Core HasData migration + data-layer tests
- [x] 02-02-PLAN.md — Exercise library page with card grid, search/filter, detail dialog, and navigation bar
- [x] 02-03-PLAN.md — Custom exercise creation dialog with polymorphic form, FAB trigger, and success toast
- [x] 02-04-PLAN.md — Premium dark-mode design system: CSS tokens, Inter font, glassmorphism, card animations, tag glow, frosted dialogs

### Phase 3: Workout Templates
**Goal**: Users can build reusable workout blueprints with ordered exercises, type-appropriate targets, grouping constructs, and warm-up/cool-down sections
**Depends on**: Phase 2
**Requirements**: TMPL-01, TMPL-02, TMPL-03, TMPL-04, TMPL-05, TMPL-06, TMPL-07
**Success Criteria** (what must be TRUE):
  1. User can create a named workout template, add exercises from the library in a specific order, and reorder them
  2. User can set strength targets (sets/reps/weight) on strength exercises and endurance targets (distance/duration/pace/HR zone) on endurance exercises within a template
  3. User can group 2+ exercises into a superset with a visual connector, and define EMOM blocks with exercise count, minute window, and rounds
  4. User can designate exercises as warm-up or cool-down sections that are visually separated from working sets
  5. User can view a list of saved templates and open any template for editing
**Plans:** 3/5 plans executed

Plans:
- [x] 03-01-PLAN.md — Tags property, JSON converter, migration, TemplateBuilderState model, unit tests
- [x] 03-02-PLAN.md — Template list page with search/tag filter, cards, detail dialog, delete, duplicate
- [x] 03-03-PLAN.md — Template builder with exercise picker, inline targets, save/load, NavigationLock
- [x] 03-04-PLAN.md — Section management (warm-up/working/cool-down) and superset/EMOM grouping
- [x] 03-05-PLAN.md — Drag-and-drop reordering via SortableJS interop and keyboard undo/redo

### Phase 4: Calendar & Scheduling
**Goal**: Users can schedule workouts on a calendar with recurrence rules and see their training week at a glance
**Depends on**: Phase 3
**Requirements**: SCHED-01, SCHED-02, SCHED-03, SCHED-04, SCHED-05, SCHED-06
**Success Criteria** (what must be TRUE):
  1. User can view a weekly calendar showing scheduled workouts with visual type indicators (strength/endurance/mixed)
  2. User can view a monthly overview with color-coded dots showing workout density and types
  3. User can schedule a workout from a template (or ad-hoc) on a specific date, and set recurrence rules (every Monday, every other day, 3x/week on specific days)
  4. Recurring workouts materialize as concrete scheduled rows for a rolling window -- "what's scheduled this week?" is a simple date query
  5. User sees a conflict warning when scheduling the same muscle group on consecutive days
**Plans:** 4 plans

Plans:
- [x] 04-01-PLAN.md — Schema change for ad-hoc workouts, MaterializationService, SchedulingService, and tests
- [x] 04-02-PLAN.md — Calendar page with weekly grid, WorkoutChip component, navigation, and nav link
- [x] 04-03-PLAN.md — ScheduleDialog with template picker, ad-hoc input, and recurrence configuration
- [x] 04-04-PLAN.md — MonthlyMiniCalendar, WorkoutDetailDialog, dialog wiring, and drag-to-reschedule

### Phase 5: Session Tracking
**Goal**: Users can log workouts in real time with type-appropriate inputs, previous performance context, and full resilience against connection loss
**Depends on**: Phase 4
**Requirements**: SESS-01, SESS-02, SESS-03, SESS-04, SESS-05, SESS-06, SESS-07, SESS-08, SESS-09, SESS-10
**Success Criteria** (what must be TRUE):
  1. User can start a session from a scheduled workout and see pre-filled targets from the template; strength exercises show set-by-set entry (weight/reps) and endurance exercises show timer/stopwatch with distance/pace entry
  2. User can see their previous performance for each exercise inline while logging, and a rest timer auto-starts after completing a set
  3. User can mark individual exercises as completed, partially completed, or skipped, and rate the session with RPE (1-10) and free-text notes
  4. Progress is saved to the database incrementally on every set completion -- if the browser tab dies or the connection drops, the user can reopen the app and resume the session exactly where they left off
**Plans:** 5 plans

Plans:
- [x] 05-01-PLAN.md — SessionService, ExerciseCompletionStatus enum, CSS tokens/animations, DI registration
- [x] 05-02-PLAN.md — SessionTests: 16 integration tests covering all service operations
- [x] 05-03-PLAN.md — Session page (landing + active) and SessionExerciseItem component
- [x] 05-04-PLAN.md — WorkoutDetailDialog "Start Session" button and MainLayout nav link
- [x] 05-05-PLAN.md — SessionSummary overlay, AbandonSessionDialog, NavigationLock wiring

### Phase 6: Analytics
**Goal**: Users can view unified training progress across strength and endurance with automatic PR detection and adherence tracking
**Depends on**: Phase 5
**Requirements**: ANLY-01, ANLY-02, ANLY-03, ANLY-04, ANLY-05
**Success Criteria** (what must be TRUE):
  1. User can view volume trends over time (total sets, total weight lifted per week) and endurance trends (pace and distance per week) on interactive charts
  2. User can see automatically detected personal records (weight PR, rep PR, estimated 1RM for strength; pace PR, distance PR for endurance) with a timeline of when they were set
  3. User can see streak and consistency metrics showing how many planned workouts were completed out of how many were scheduled, per week and month
  4. User can see planned-vs-actual deviation per session and aggregated over time -- how closely actual performance matched planned targets
**Plans**: TBD

Plans:
- [ ] 06-01: TBD
- [ ] 06-02: TBD

### Phase 7: Quality of Life
**Goal**: Daily workflow is streamlined with quick-start, smart suggestions, data export, and workout history browsing
**Depends on**: Phase 5 (can begin once session tracking is stable; independent of Phase 6)
**Requirements**: QOL-01, QOL-02, QOL-03, QOL-04, QOL-05, QOL-06
**Success Criteria** (what must be TRUE):
  1. User opens the app and sees today's scheduled workout on the home screen with one action to start logging; if no workout is scheduled, a "repeat last workout" option is available
  2. User receives progressive overload suggestions when they have consistently hit a target (e.g., "you hit 3x8 at 20kg twice -- ready for 22.5kg?")
  3. User can export all training data as CSV with date-range filtering, and export workout templates and training summaries as formatted PDF
  4. User can browse a chronological list of completed workout sessions with search and filter capabilities
**Plans**: TBD

Plans:
- [ ] 07-01: TBD
- [ ] 07-02: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Data Foundation | 2/2 | Complete |  |
| 2. Exercise Library | 3/4 | In Progress|  |
| 3. Workout Templates | 3/5 | In Progress|  |
| 4. Calendar & Scheduling | 0/4 | Not started | - |
| 5. Session Tracking | 0/5 | Not started | - |
| 6. Analytics | 0/? | Not started | - |
| 7. Quality of Life | 0/? | Not started | - |
