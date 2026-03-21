# Unified Workout Planner

## What This Is

A personal workout planning and tracking app built in Blazor that treats strength and endurance training as first-class citizens in the same system. Define workouts from a shared exercise library, schedule them on a calendar with recurrence, log sessions as you go, and track progress over time. Unlike apps like Strong or Runna, your entire training week — pull-ups and running sessions — lives in one place, and analytics understand both.

## Core Value

A single system where you plan, log, and analyze both strength and endurance training side by side — your whole training week in one view.

## Requirements

### Validated

- ✓ Blazor Server app scaffold with .NET 10 — existing
- ✓ Interactive server rendering configured — existing
- ✓ Layout system with navigation — existing
- ✓ SQLite database with EF Core — Validated in Phase 01: data-foundation
- ✓ Exercise type hierarchy (strength/endurance TPH) — Validated in Phase 01: data-foundation
- ✓ Workout template model with grouping/ordering — Validated in Phase 01: data-foundation
- ✓ Scheduling with recurrence rules — Validated in Phase 01: data-foundation
- ✓ Session logging with planned-vs-actual tracking — Validated in Phase 01: data-foundation

### Active

- [ ] Exercise library — searchable, filterable catalog of exercises with type discriminator (strength/endurance), metadata (muscle group, equipment, activity type), and ability to add custom exercises
- [ ] Workout templates — reusable blueprints with ordered exercise list, support for both strength targets (sets/reps/weight) and endurance targets (distance/duration/pace/HR zone), plus superset and EMOM grouping constructs
- [ ] Calendar scheduler — weekly view (primary) and monthly overview, schedule workouts with recurrence rules (every Monday, every other day, 3x/week on specific days), rest day awareness and conflict detection
- [ ] Session tracker — open today's workout, log as you go; strength: tap through sets with weight/reps, previous performance shown inline; endurance: timer/stopwatch with distance/pace entry after; mark exercises completed/partial/skipped
- [ ] Workout logging — separation between planned and actual; strength log entries: actual sets with reps and weight per set; endurance log entries: actual distance, duration, pace, optional HR data; tracks adherence and deviation
- [ ] Analytics dashboard — volume trends over time (total sets, weight lifted per week), PR tracking with automatic detection, streak and consistency metrics (X/Y planned workouts completed), endurance pace trends and distance per week
- [ ] RPE and session notes — rate perceived effort (1–10) and free-text notes per session for recovery/trend analysis
- [ ] Progressive overload suggestions — if target hit consistently (e.g. 3x8 at 20kg for two sessions), nudge to increase weight
- [ ] Warm-up / cool-down blocks — separate template sections that don't count toward working volume stats
- [ ] Quick-start repeat last workout — open app, today's recurring workout is right there, one tap to start logging
- [ ] Export — CSV/PDF export of training data and summaries
- [ ] SQLite database with EF Core — local data storage, zero setup

### Out of Scope

- Garmin/wearable integration — deferred to v2, will explore importing workout data from Garmin
- Authentication/multi-user — personal app, single user
- Mobile native app — web-first, responsive design covers mobile use
- Real-time heart rate streaming — manual HR entry sufficient for v1
- Social/sharing features — personal use only
- Cloud sync — local SQLite sufficient, backup via file copy

## Context

- Building on an existing Blazor Server (.NET 10) scaffold that has routing, layout, and error handling in place
- No service layer, data layer, or business logic exists yet — the app is a blank canvas with infrastructure
- The user's training style includes mixed sessions (e.g. weighted pull-ups + dips EMOM on Monday, Zone 2 runs on other days) — the data model must handle both in a single workout template
- Superset and EMOM grouping constructs are important patterns that most apps handle poorly
- The planned vs. logged separation is a deliberate design choice to track adherence and deviation

## Constraints

- **Tech stack**: Blazor Server with .NET 10, C#, EF Core with SQLite — no JavaScript frameworks
- **Platform**: Server-side rendered with interactive server components via WebSocket
- **Data**: Local SQLite database, single-user, no cloud dependencies
- **UI**: Blazor components only, responsive web design for desktop and mobile browser use

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| SQLite with EF Core for data | Zero setup, easy backup, sufficient for single-user personal app | ✓ Phase 01 |
| Strength/endurance type discriminator on Exercise entity | Allows shared library while preserving type-specific metadata and logging shapes | ✓ Phase 01 |
| Planned vs. logged separation in data model | Enables adherence tracking and deviation analysis — see what you intended vs. what happened | ✓ Phase 01 |
| Superset/EMOM as grouping constructs in templates | User's actual training patterns require this; most apps handle it poorly | ✓ Phase 01 |
| Full vision as v1 scope | Personal app, user wants all features before switching from current workflow | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd:transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd:complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-03-21 after Phase 01 completion*
