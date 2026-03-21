# Phase 2: Exercise Library - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Searchable exercise catalog with custom exercise creation. Users can browse, search, and filter a pre-loaded catalog of ~50 exercises spanning strength and endurance types, and add their own custom exercises. This is the first UI phase — no prior visual components exist.

</domain>

<decisions>
## Implementation Decisions

### Catalog browse layout
- **D-01:** Card grid layout for displaying exercises
- **D-02:** Cards show exercise name and a subtle type tag (strength/endurance) — minimal info, not cluttered
- **D-03:** Clicking a card opens a detail dialog showing full info (name, description, type, muscle group, equipment/activity type)
- **D-04:** Strength and endurance exercises appear in a unified list, distinguished by subtle type tags (not separate tabs or sections)

### Search and filter interaction
- **D-05:** Search bar and filter controls live in a top bar above the card grid
- **D-06:** Instant filtering — results update as user types or selects filter values, no apply button
- **D-07:** Filters use AND across categories — selecting "Chest" + "Barbell" requires both to match; unset categories are ignored
- **D-08:** Active filters shown as chips with X to remove individually, plus a "clear all" button

### Exercise creation flow
- **D-09:** "Add exercise" triggered by a floating action button (FAB)
- **D-10:** Creation form opens in a dialog (consistent with detail view pattern)
- **D-11:** Toggle/radio at top of form to pick strength vs endurance type — form fields adapt below (muscle group + equipment for strength, activity type for endurance)
- **D-12:** After save: brief success message, dialog closes, catalog scrolls to the new exercise

### Seed data composition
- **D-13:** ~35-40 strength exercises, ~10-15 endurance exercises
- **D-14:** Calisthenics-focused strength selection — weighted pull-ups, dips, bodyweight movements for home workouts
- **D-15:** Equipment bias toward what's available at home: bodyweight, dumbbell, dip bars, weighted vest, kettlebell — not gym-machine heavy
- **D-16:** Endurance exercises are mostly running variants (easy run, tempo run, interval run, long run) and cycling variants
- **D-17:** All seed exercises include brief descriptions with form cues

### Claude's Discretion
- Card sizing, spacing, and responsive breakpoints
- Exact color/icon for strength vs endurance type tags
- Filter dropdown/chip selector implementation details
- Dialog sizing and animation
- FAB positioning and styling
- Success message display pattern (toast, inline, etc.)
- Exact seed exercise list within the composition guidelines above
- Whether to add a service/repository layer or access DbContext directly from components

</decisions>

<specifics>
## Specific Ideas

- User trains at home with calisthenics focus — dip bars, weights, weighted vest — not a full gym setup
- User does mostly running and cycling for cardio, not swimming/rowing/etc.
- This shapes the seed data: exercises should feel relevant to a home calisthenics + running/cycling athlete, not a commercial gym user

</specifics>

<canonical_refs>
## Canonical References

### Planning artifacts
- `.planning/PROJECT.md` — Core value, constraints (Blazor Server, no JS frameworks, SQLite)
- `.planning/REQUIREMENTS.md` — EXER-01, EXER-02, EXER-03 define all Phase 2 deliverables
- `.planning/ROADMAP.md` §Phase 2 — Success criteria (3 verification points)

### Phase 1 context (data model decisions)
- `.planning/phases/01-data-foundation/01-CONTEXT.md` — D-01 through D-05 define Exercise entity shape (TPH, enums, base fields)

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Data/Enums/MuscleGroup.cs` — 7 values, use as filter dropdown source
- `Data/Enums/Equipment.cs` — 8 values, use as filter dropdown source
- `Data/Enums/ActivityType.cs` — 9 values, use as filter dropdown for endurance exercises
- `Data/Entities/Exercise.cs` — Abstract base + StrengthExercise/EnduranceExercise subtypes, TPH inheritance ready
- `Data/AppDbContext.cs` — DbSets for Exercises, StrengthExercises, EnduranceExercises with TPH query support

### Established Patterns
- `IDbContextFactory<AppDbContext>` registered in Program.cs — components create/dispose contexts per operation
- Scoped CSS via `.razor.css` files — use for component-level styling
- EditForm + DataAnnotations imported in `_Imports.razor` — available for form validation
- Validation CSS classes already defined in `wwwroot/app.css`

### Integration Points
- `Components/Pages/` — New exercise library page goes here with `@page` directive
- `Components/Layout/MainLayout.razor` — Navigation to exercise library needs to be added
- `Program.cs` — If adding a service layer, register services here
- `Data/AppDbContext.cs` — Seed data via `HasData()` in configuration or migration

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 02-exercise-library*
*Context gathered: 2026-03-21*
