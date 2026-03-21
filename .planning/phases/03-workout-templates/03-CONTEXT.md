# Phase 3: Workout Templates - Context

**Gathered:** 2026-03-21
**Status:** Ready for planning

<domain>
## Phase Boundary

Template builder with strength/endurance targets, superset/EMOM grouping, and warm-up/cool-down sections. Users can create, browse, edit, and duplicate reusable workout blueprints with ordered exercises and type-appropriate targets. No scheduling, no session logging — those are separate phases.

</domain>

<decisions>
## Implementation Decisions

### Template list view
- **D-01:** Summary cards showing template name, first 3-4 exercise names as preview, total exercise count, and estimated duration
- **D-02:** Freeform tags on templates — user types any tag when creating/editing (not a predefined set)
- **D-03:** Template list filterable by tag — click a tag chip to filter, consistent with exercise library filter pattern
- **D-04:** FAB button to create new template (consistent with exercise library pattern)
- **D-05:** Clicking a template card opens a read-only detail view showing the full workout structure (ordered exercises with targets, grouped exercises with connectors, section headers), with an "Edit" button to enter the builder
- **D-06:** Duplicate template action available ("Copy as new") to create variations without rebuilding

### Template builder layout
- **D-07:** Full-page editor — navigate away from the template list into a dedicated builder page
- **D-08:** Add exercises via a browse dialog — opens exercise library in a picker dialog with search + filter, supports selecting multiple exercises at once (checkboxes, then "Add selected")
- **D-09:** Inline target editing — strength exercises show sets/reps/weight fields directly on the row; endurance exercises show distance/duration/pace/HR zone fields on the row
- **D-10:** Explicit "Save" button with "Discard changes" option — no auto-save
- **D-11:** Undo/redo capability in the builder
- **D-12:** Drag-and-drop reordering of exercises (JS interop acceptable for this)

### Grouping interaction
- **D-13:** Select-then-group: user selects 2+ exercises already in the template, then clicks "Group as superset" or "Group as EMOM"
- **D-14:** EMOM follows same select-then-group flow, but after grouping prompts for rounds and minute window inline on the group header
- **D-15:** Grouped exercises connected by a vertical bracket/connector on the left side with a label ("Superset" or "EMOM 5x2min")
- **D-16:** Ungroup via remove-from-group (detach individual exercises) or dissolve group (ungroup all back to standalone)

### Section management
- **D-17:** Visual section headers dividing the exercise list into warm-up / working / cool-down zones
- **D-18:** Warm-up and cool-down sections are optional — builder starts with just "Working"; toolbar buttons ("Add warm-up section" / "Add cool-down section") appear when those sections don't exist
- **D-19:** New exercises default to the "Working" section
- **D-20:** Drag exercises between sections to reassign
- **D-21:** Empty sections auto-remove — if all exercises are removed from warm-up or cool-down, the section header disappears

### Claude's Discretion
- Full-page builder layout details (toolbar placement, spacing)
- Exercise picker dialog sizing and multi-select UX
- Undo/redo implementation strategy (command pattern, state snapshots, etc.)
- Drag-and-drop JS interop library choice or custom implementation
- Bracket/connector visual styling (color, width, label positioning)
- Estimated duration calculation heuristic
- Tag input component styling and behavior
- Responsive breakpoints for the builder on mobile

</decisions>

<specifics>
## Specific Ideas

- Detail view should show the entire workout structure: ordered exercises with targets, grouped exercises with bracket connectors, and section headers — a true read-only preview of what you'd see in the builder
- Duplicate ("Copy as new") enables quick creation of workout variations (e.g. "Pull Day A" → "Pull Day B" with tweaks)
- Freeform tags should feel lightweight — type and press enter, not a heavy management UI

</specifics>

<canonical_refs>
## Canonical References

### Planning artifacts
- `.planning/PROJECT.md` — Core value, constraints (Blazor Server, no JS frameworks, SQLite)
- `.planning/REQUIREMENTS.md` — TMPL-01 through TMPL-07 define all Phase 3 deliverables
- `.planning/ROADMAP.md` §Phase 3 — Success criteria (5 verification points)

### Phase 1 context (data model decisions)
- `.planning/phases/01-data-foundation/01-CONTEXT.md` — D-06 through D-09 define grouping model (TemplateGroup entity, flat grouping, single Position ordering); D-01 through D-05 define Exercise entity shape

### Phase 2 context (UI patterns)
- `.planning/phases/02-exercise-library/02-CONTEXT.md` — D-01 through D-04 establish card grid layout pattern; D-05 through D-08 establish filter interaction pattern; D-09 through D-12 establish FAB + dialog + form patterns

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `Components/Shared/Dialog.razor` — Base modal component with header, body, close button — reuse for exercise picker dialog and detail view
- `Components/Shared/ExerciseCard.razor` — Card pattern with type tags — adapt for template cards
- `Components/Shared/FilterChip.razor` — Removable filter chip — reuse for tag filtering
- `Components/Shared/Toast.razor` — Auto-dismissing notification — reuse for save/duplicate confirmations
- `Components/Shared/ExerciseFormDialog.razor` — EditForm + DataAnnotationsValidator pattern — reference for template name/tag form
- `Data/Entities/WorkoutTemplate.cs` — Template, TemplateItem, TemplateGroup entities fully defined
- `Data/Configurations/TemplateConfiguration.cs` — EF Core fluent configuration with relationships and delete behaviors
- `Data/Enums/` — GroupType, SectionType, MuscleGroup, Equipment, ActivityType all defined

### Established Patterns
- `IDbContextFactory<AppDbContext>` injection with `using var context = DbFactory.CreateDbContext()` per operation
- Scoped CSS via `.razor.css` files with `::deep` for child component styling
- Dark theme design tokens in `wwwroot/app.css` — spacing, colors, radii, shadows, transitions, z-index scale
- Card grid: `repeat(auto-fill, minmax(280px, 1fr))` with staggered `fadeSlideIn` animation
- FAB: fixed bottom-right, 56px circle, gradient background, spring animation
- Dialog: fixed overlay with backdrop blur, max-width panel, slide-in animation

### Integration Points
- `Components/Layout/MainLayout.razor` — Add "Templates" NavLink to navigation
- `Components/Pages/` — New Templates.razor (list page) and TemplateBuilder.razor (editor page)
- `Components/Shared/` — New shared components (TemplateCard, ExercisePicker, etc.)
- `wwwroot/app.css` — May need new animations/keyframes for builder interactions
- JS interop file needed for drag-and-drop functionality

</code_context>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 03-workout-templates*
*Context gathered: 2026-03-21*
