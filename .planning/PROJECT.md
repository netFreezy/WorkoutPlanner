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
- ✓ Exercise library — Validated in Phase 02: exercise-library
- ✓ Workout templates — Validated in Phase 03: workout-templates
- ✓ Calendar scheduler — weekly and monthly views, scheduling with recurrence, materialization — Validated in Phase 04: calendar-scheduling (SCHED-05 conflict detection deferred)
- ✓ Session tracker — real-time workout logging with incremental persistence, previous performance, RPE/notes, resume on reconnect — Validated in Phase 05: session-tracking (SESS-06 rest timer deferred per D-14)
- ✓ Workout logging — planned-vs-actual with strength set logging and endurance data entry — Validated in Phase 05: session-tracking
- ✓ RPE and session notes — RPE 1-10 slider and free-text notes on session finish — Validated in Phase 05: session-tracking
- ✓ Analytics dashboard — volume trends, PR tracking with automatic detection, streak/consistency metrics, endurance pace/distance trends, planned-vs-actual deviation — Validated in Phase 06: analytics
- ✓ Quick-start home dashboard — today's workout with Start Session CTA, Repeat Workout for last completed, Up Next preview — Validated in Phase 07: quality-of-life
- ✓ Progressive overload suggestions — inline suggestion cards when target hit consistently across 2 sessions, per-muscle-group increments — Validated in Phase 07: quality-of-life
- ✓ Export — CSV (CsvHelper) and PDF (QuestPDF) export from analytics page with time range selector — Validated in Phase 07: quality-of-life
- ✓ Workout history browser — searchable, filterable history page with paginated session cards and expandable detail — Validated in Phase 07: quality-of-life

### Active
- [ ] Warm-up / cool-down blocks — separate template sections that don't count toward working volume stats

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
*Last updated: 2026-03-22 after Phase 07 completion — Quality of life features: home dashboard, progressive overload suggestions, CSV/PDF export, workout history browser*
