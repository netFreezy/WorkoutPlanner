# Phase 1: Data Foundation - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

EF Core data model with full entity hierarchy, migrations, and SQLite persistence. No UI components, no service layer beyond what's needed to verify the model round-trips correctly. All subsequent phases depend on this foundation.

</domain>

<decisions>
## Implementation Decisions

### Exercise entity shape
- **D-01:** TPH inheritance — base `Exercise` with `StrengthExercise` and `EnduranceExercise` subtypes
- **D-02:** Single muscle group per exercise as an enum (e.g. Chest, Back, Shoulders, Legs, Arms, Core, FullBody)
- **D-03:** Single equipment per exercise as an enum (e.g. Barbell, Dumbbell, Bodyweight, Cable, Machine, Band, Kettlebell)
- **D-04:** Endurance exercises use an activity type enum (Run, Cycle, Swim, Row, etc.) — no deeper subtype hierarchy
- **D-05:** Base Exercise fields: Name, Description, CreatedDate (plus EF-managed Id and discriminator)

### Grouping construct modeling
- **D-06:** Separate `TemplateGroup` entity (not a nullable GroupId on TemplateItem)
- **D-07:** TemplateGroup carries GroupType (Superset/EMOM) and EMOM-specific fields (Rounds, MinuteWindow) directly
- **D-08:** Flat grouping only — no nesting of groups within groups
- **D-09:** Single global sort order across the template — TemplateItems have a Position int, groups are contiguous runs in that ordering

### Recurrence rule storage
- **D-10:** Structured columns on RecurrenceRule: FrequencyType enum (Daily/Weekly/Custom), Interval (every N), DaysOfWeek (flags or comma-separated)
- **D-11:** Recurrence runs forever until manually deleted — no end date or occurrence count
- **D-12:** Fixed 4-week materialization window for generating concrete ScheduledWorkout rows
- **D-13:** Editing a recurrence rule regenerates all future materialized rows — no "edit just this one" support

### Planned-vs-actual snapshot strategy
- **D-14:** Deep copy into log tables — when a session starts, each exercise's targets are copied into log rows with both planned and actual columns
- **D-15:** Strength snapshot fields: planned sets, planned reps, planned weight (actual columns filled during logging)
- **D-16:** Endurance snapshot fields: planned distance, planned duration, planned pace, planned HR zone, plus activity type (actual columns filled during logging)

### Claude's Discretion
- Exact enum values for muscle groups, equipment, and activity types
- Previous performance lookup strategy (query at render time vs. caching)
- Value converter approach for any SQLite type limitations
- Migration naming and organization
- DbContext configuration details (fluent API vs. data annotations)
- Seed data structure (Phase 2 seeds the exercises, Phase 1 just ensures the model supports it)

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches.

</specifics>

<canonical_refs>
## Canonical References

No external specs — requirements are fully captured in decisions above and the following planning artifacts:

### Planning artifacts
- `.planning/PROJECT.md` — Core value, constraints, key decisions (SQLite, TPH, planned-vs-actual separation, superset/EMOM)
- `.planning/REQUIREMENTS.md` — DATA-01 through DATA-10 define all Phase 1 deliverables
- `.planning/ROADMAP.md` §Phase 1 — Success criteria (5 verification points)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- None — the project is a blank Blazor scaffold with no data layer

### Established Patterns
- Nullable reference types enabled — model properties should use nullable annotations
- Implicit usings enabled — EF Core namespaces will be globally available once packages are added
- Interactive Server Components — all data access runs server-side

### Integration Points
- `Program.cs` — DbContextFactory registration goes here
- `appsettings.json` — SQLite connection string goes here
- `.csproj` — Needs Microsoft.EntityFrameworkCore.Sqlite and .Design NuGet packages

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 01-data-foundation*
*Context gathered: 2026-03-21*
