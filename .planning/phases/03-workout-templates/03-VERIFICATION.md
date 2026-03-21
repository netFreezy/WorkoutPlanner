---
phase: 03-workout-templates
verified: 2026-03-21T00:00:00Z
status: passed
score: 15/15 must-haves verified
gaps: []
human_verification:
  - test: "Drag-and-drop reordering — cross-section drag"
    expected: "Dragging an exercise row from Working to Warm-Up updates its SectionType visually and after save"
    why_human: "SortableJS interop cannot be verified programmatically; requires a running browser"
  - test: "NavigationLock discard confirmation"
    expected: "Navigating away with unsaved changes shows a discard dialog; choosing Keep Editing stays on the page"
    why_human: "NavigationLock behaviour is a browser-level interaction that cannot be verified by static analysis"
  - test: "Touch-device drag-and-drop"
    expected: "Drag handles work on a mobile browser (touch events forwarded through SortableJS)"
    why_human: "Touch interaction requires a real device or emulator"
  - test: "Toast messages appear on duplicate and delete"
    expected: "Duplicating shows 'Template duplicated as X (copy)', deleting shows 'Template deleted'"
    why_human: "Toast component is UI-level; requires a running browser to observe"
---

# Phase 3: Workout Templates Verification Report

**Phase Goal:** Users can build reusable workout blueprints with ordered exercises, type-appropriate targets, grouping constructs, and warm-up/cool-down sections
**Verified:** 2026-03-21
**Status:** PASSED
**Re-verification:** No — initial verification

---

## Goal Achievement

### Observable Truths

| # | Truth | Status | Evidence |
|---|-------|--------|----------|
| 1 | User can create a named workout template, add exercises, and reorder them | VERIFIED | `TemplateBuilder.razor` at `/templates/new`, `AddSelectedExercises`, `OnItemReordered` with SortableJS wired |
| 2 | User can set strength targets (sets/reps/weight) on strength exercises inline | VERIFIED | `ExerciseRow.razor` renders `strength-targets` inputs bound to `TargetSets`, `TargetReps`, `TargetWeight` |
| 3 | User can set endurance targets (distance/duration/pace/HR zone) inline | VERIFIED | `ExerciseRow.razor` renders `endurance-targets` inputs for all four endurance fields |
| 4 | User can group exercises as superset with visual connector | VERIFIED | `GroupAsSuperset` in TemplateBuilder.razor.cs; `GroupBracket.razor` renders `SUPERSET` label and bracket line |
| 5 | User can define EMOM blocks with rounds and minute window | VERIFIED | `GroupAsEmom` creates `BuilderGroup { GroupType.EMOM, Rounds=5 }`; `GroupBracket.razor` renders editable round/window inputs |
| 6 | User can designate exercises as warm-up or cool-down sections | VERIFIED | `AddWarmUpSection`/`AddCoolDownSection` in TemplateBuilder.razor.cs; `SectionHeader.razor` renders per section |
| 7 | User can view a list of saved templates and open any for editing | VERIFIED | `Templates.razor` at `/templates` with card grid; `TemplateDetailDialog` opens detail; Edit button navigates to `/templates/{id}/edit` |
| 8 | Template can be duplicated with deep copy of items, groups, tags | VERIFIED | `DuplicateTemplate` in Templates.razor.cs performs group-mapped deep copy with `(copy)` suffix |
| 9 | Template can be deleted with confirmation | VERIFIED | `DeleteConfirmationDialog.razor` + `DeleteTemplate` method in Templates.razor.cs |
| 10 | Tags can be added/removed via tag input component | VERIFIED | `TagInput.razor` handles Enter to add, Backspace to remove, fires `OnTagsChanged` |
| 11 | Tags persist through database round-trip | VERIFIED | `WorkoutTemplateConfiguration.cs` has `HasConversion` using `JsonSerializer`; 3 tag tests pass |
| 12 | Navigating away with unsaved changes shows discard confirmation | VERIFIED | `NavigationLock` bound to `OnBeforeInternalNavigation` in `TemplateBuilder.razor` |
| 13 | Drag-and-drop exercise reordering with SortableJS | VERIFIED (partial — human needed for browser) | `template-builder.js` exports `initSortable` wired to `.NET OnItemReordered`; `App.razor` references `sortable.min.js` |
| 14 | Ctrl+Z / Ctrl+Shift+Z keyboard undo/redo | VERIFIED | `HandleKeyDown` in TemplateBuilder.razor.cs handles both key combinations |
| 15 | Undo/redo stack (50-deep) persists positions, groups, section types | VERIFIED | `TemplateBuilderState.PushUndo/Undo/Redo` with `MaxUndoDepth = 50`; snapshot serializes all fields except `IsSelected` |

**Score:** 15/15 truths verified (4 items flagged for human browser verification)

---

## Required Artifacts

### Plan 03-01 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Data/Entities/WorkoutTemplate.cs` | Tags property on WorkoutTemplate | VERIFIED | `public List<string> Tags { get; set; } = new();` present |
| `Data/Configurations/TemplateConfiguration.cs` | JSON value converter for Tags | VERIFIED | `HasConversion` with `JsonSerializer.Serialize/Deserialize` and `ValueComparer` |
| `Models/TemplateBuilderState.cs` | In-memory builder state with undo/redo | VERIFIED | `class TemplateBuilderState` with `MaxUndoDepth = 50`, `PushUndo`, `Undo`, `Redo`, `EstimateDurationMinutes` |
| `Models/TemplateFormModel.cs` | Template name validation model | VERIFIED | `[Required]` and `[MaxLength(200)]` on `Name` |
| `BlazorApp2.Tests/TemplateTagTests.cs` | Tags round-trip tests | VERIFIED | 3 tests: empty list, multi-value, special characters — all pass |
| `BlazorApp2.Tests/TemplateDuplicateTests.cs` | Deep copy tests | VERIFIED | 2 tests: full data preservation, EMOM rounds/window — all pass |
| `BlazorApp2.Tests/TemplateDurationTests.cs` | Duration estimate tests | VERIFIED | 6 tests: strength, endurance, mixed, EMOM, defaults, empty — all pass |
| EF Core migration `AddTemplateTags` | Migration file for Tags column | VERIFIED | `Migrations/20260321185044_AddTemplateTags.cs` exists |

### Plan 03-02 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Components/Pages/Templates.razor` | Template list page at `/templates` | VERIFIED | `@page "/templates"`, `@rendermode InteractiveServer`, renders `<TemplateCard>`, `<TemplateDetailDialog>`, `<DeleteConfirmationDialog>`, `<Toast>` |
| `Components/Pages/Templates.razor.cs` | Code-behind with load, filter, CRUD | VERIFIED | `IDbContextFactory<AppDbContext>`, `DuplicateTemplate`, `DeleteTemplate`, `FilteredTemplates`, `AllTags` |
| `Components/Shared/TemplateCard.razor` | Summary card component | VERIFIED | `[Parameter] public WorkoutTemplate Template`, `OnClick`, `Index` — renders name, exercise preview, tags, duration |
| `Components/Shared/TemplateDetailDialog.razor` | Read-only template detail dialog | VERIFIED | Renders sections by `SectionType`, groups by `GroupType.Superset`/`EMOM`, targets, edit/duplicate/delete actions |
| `Components/Shared/DeleteConfirmationDialog.razor` | Delete confirmation dialog | VERIFIED | `[Parameter] public string TemplateName`, `OnConfirm`, `OnCancel`; `.destructive-btn` in CSS |
| `Components/Layout/MainLayout.razor` | Templates nav link | VERIFIED | `<NavLink class="nav-link" href="templates">Templates</NavLink>` present |
| `wwwroot/app.css` | Design tokens for sections/groups | VERIFIED | `--color-warmup-text`, `--color-cooldown-text`, `--color-group-superset`, `--color-selection-bg`; `@keyframes rowSlideIn` and `bracketDraw` |

### Plan 03-03 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Components/Pages/TemplateBuilder.razor` | Full-page template editor | VERIFIED | `@page "/templates/new"` and `@page "/templates/{Id:int}/edit"`, renders `<BuilderToolbar>`, `<SectionHeader>`, `<GroupBracket>`, `<ExerciseRow>`, `<ExercisePickerDialog>`, `<NavigationLock>` |
| `Components/Shared/BuilderToolbar.razor` | Sticky toolbar with actions | VERIFIED | Add exercises, warm-up/cool-down buttons, superset/EMOM/ungroup buttons, undo/redo icons, save button |
| `Components/Shared/ExerciseRow.razor` | Exercise row with inline targets | VERIFIED | Checkbox selection, drag handle, strength targets (sets/reps/weight), endurance targets (distance/duration/pace/HR zone) |
| `Components/Shared/ExercisePickerDialog.razor` | Multi-select exercise browser | VERIFIED | Search, type filter, toggleable selection, `OnExercisesSelected` callback, loads from `IDbContextFactory` |
| `Components/Shared/TagInput.razor` | Freeform tag input | VERIFIED | Enter to add, Backspace to remove last, fires `OnTagsChanged` |

### Plan 03-04 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `Components/Shared/SectionHeader.razor` | Section divider | VERIFIED | `[Parameter] public SectionType Section`, renders warmup/working/cooldown labels with exercise count |
| `Components/Shared/GroupBracket.razor` | Visual bracket connector | VERIFIED | `[Parameter] public BuilderGroup Group`, renders SUPERSET label or EMOM label with editable rounds/window inputs |

### Plan 03-05 Artifacts

| Artifact | Expected | Status | Details |
|----------|----------|--------|---------|
| `wwwroot/js/sortable.min.js` | SortableJS library | VERIFIED | File exists at path |
| `wwwroot/js/template-builder.js` | JS interop module | VERIFIED | Exports `initSortable`, `refreshSortable`, `destroySortable`; calls `invokeMethodAsync('OnItemReordered', ...)` |
| `Components/App.razor` | SortableJS script reference | VERIFIED | `<script src="js/sortable.min.js"></script>` on line 21 |

---

## Key Link Verification

| From | To | Via | Status | Details |
|------|----|-----|--------|---------|
| `TemplateConfiguration.cs` | `WorkoutTemplate.cs` | `HasConversion` JSON serializer for Tags | VERIFIED | `JsonSerializer.Serialize/Deserialize` present in configure method |
| `TemplateBuilderState.cs` | `Data/Enums/SectionType.cs` | `BuilderItem.SectionType` uses `SectionType` enum | VERIFIED | `public SectionType SectionType { get; set; } = SectionType.Working;` |
| `Templates.razor.cs` | `AppDbContext` | `IDbContextFactory` loads templates | VERIFIED | `DbFactory.CreateDbContext()` called in `LoadTemplatesAsync`, `DuplicateTemplate`, `DeleteTemplate` |
| `Templates.razor` | `TemplateCard.razor` | Renders `<TemplateCard>` for each template | VERIFIED | `<TemplateCard Template="@t" OnClick="@ShowDetail" ... />` in template grid |
| `Templates.razor` | `TemplateDetailDialog.razor` | Opens detail dialog | VERIFIED | `<TemplateDetailDialog IsOpen="@(selectedTemplate is not null)" ... />` |
| `TemplateBuilder.razor.cs` | `TemplateBuilderState.cs` | Maintains state as editing state | VERIFIED | `private TemplateBuilderState State = new();` — all edits go through `State` |
| `TemplateBuilder.razor.cs` | `AppDbContext` | `IDbContextFactory` for load and save | VERIFIED | `DbFactory.CreateDbContext()` in `LoadTemplateAsync`, `SaveTemplate` |
| `ExercisePickerDialog.razor` | `AppDbContext` | Loads exercises for picker | VERIFIED | `IDbContextFactory<AppDbContext>` injected; `DbFactory.CreateDbContext()` on line 72 |
| `template-builder.js` | `TemplateBuilder.razor.cs` | `DotNetObjectReference` callback | VERIFIED | `dotNetRef.invokeMethodAsync('OnItemReordered', ...)` matches `[JSInvokable] public void OnItemReordered(...)` |
| `TemplateBuilder.razor.cs` | `template-builder.js` | `IJSRuntime` import and `initSortable` call | VERIFIED | `JS.InvokeAsync<IJSObjectReference>("import", "./js/template-builder.js")` then `InvokeVoidAsync("initSortable", ...)` |
| `TemplateBuilder.razor` | `SectionHeader.razor` | Renders section headers between exercise groups | VERIFIED | `<SectionHeader Section="@sectionType" ExerciseCount="..." />` in `GetVisibleSections()` loop |
| `TemplateBuilder.razor` | `GroupBracket.razor` | Renders brackets for grouped exercises | VERIFIED | `<GroupBracket Group="@entry.Group" ItemCount="..." OnParamsChanged="..." />` in `GetSectionEntries()` output |
| `TemplateBuilder.razor.cs` | `TemplateBuilderState.cs` | Manages group/section state | VERIFIED | `State.Groups`, `State.Items`, `BuilderGroup` — all grouping methods on `State` |

---

## Requirements Coverage

| Requirement | Source Plans | Description | Status | Evidence |
|-------------|-------------|-------------|--------|----------|
| TMPL-01 | 03-01, 03-02, 03-03 | Template builder — create named templates with ordered exercise list | SATISFIED | TemplateBuilder at `/templates/new`, Templates list at `/templates`, exercise ordering via Position |
| TMPL-02 | 03-05 | Reorderable exercises (drag-and-drop or move up/down) | SATISFIED | SortableJS interop wired in `template-builder.js` + `OnItemReordered`; keyboard undo via Ctrl+Z |
| TMPL-03 | 03-01, 03-03 | Strength targets per exercise: sets, reps, weight | SATISFIED | `ExerciseRow.razor` strength-targets inputs; `BuilderItem.TargetSets/TargetReps/TargetWeight` |
| TMPL-04 | 03-01, 03-03 | Endurance targets per exercise: distance, duration, pace, HR zone | SATISFIED | `ExerciseRow.razor` endurance-targets inputs; all four endurance fields on `BuilderItem` |
| TMPL-05 | 03-04 | Superset grouping — visually group 2+ exercises with connector | SATISFIED | `GroupAsSuperset()`, `GroupBracket.razor` with SUPERSET label and bracket-line CSS |
| TMPL-06 | 03-04 | EMOM grouping — N exercises, M-minute window per round, R rounds | SATISFIED | `GroupAsEmom()` with defaults, `GroupBracket.razor` with editable number inputs |
| TMPL-07 | 03-01, 03-04 | Warm-up and cool-down sections separate from working sets | SATISFIED | `SectionType` enum; `AddWarmUpSection`/`AddCoolDownSection`; `SectionHeader.razor`; `GetVisibleSections()` |

**All 7 TMPL requirements satisfied.** No orphaned requirements found. The REQUIREMENTS.md traceability table marks all TMPL-01 through TMPL-07 as Phase 3 / Complete.

---

## Anti-Patterns Found

No anti-patterns detected. Scanned:
- `Components/Pages/TemplateBuilder.razor.cs` — no TODOs, no stubs, no empty return implementations
- `Components/Shared/ExerciseRow.razor` — `placeholder` attributes are HTML form hints, not code stubs; all target fields are bound to real model properties
- `Components/Shared/GroupBracket.razor` — EMOM inputs are fully wired to `Group.Rounds`/`Group.MinuteWindow` with `OnParamsChanged` callback
- `Components/Shared/SectionHeader.razor` — no stubs
- `wwwroot/js/template-builder.js` — `initSortable`, `refreshSortable`, `destroySortable` all have real implementations

---

## Human Verification Required

### 1. Drag-and-drop exercise reordering

**Test:** Open `/templates/new`, add 3+ exercises, drag one row to a new position using the grip handle
**Expected:** Exercise list reorders immediately; saving persists the new order
**Why human:** SortableJS interop runs in a browser; DOM manipulation and callback cannot be verified by static analysis

### 2. Cross-section drag (D-20)

**Test:** Add one exercise to Warm-Up and one to Working; drag the Working exercise above the Warm-Up header
**Expected:** The dragged exercise's section badge changes to Warm-Up and it persists correctly on save
**Why human:** Requires observing the JS section-detection algorithm execute in a live browser

### 3. NavigationLock discard confirmation

**Test:** Open `/templates/new`, type a name, then click the "Exercises" nav link without saving
**Expected:** A discard dialog appears; "Keep Editing" returns to the builder with state intact; "Discard Changes" navigates away
**Why human:** NavigationLock is a Blazor browser-level intercept that requires a running WebSocket connection

### 4. Toast messages

**Test:** Duplicate a template from the detail dialog; then delete a template
**Expected:** "Template duplicated as X (copy)" toast appears after duplicate; "Template deleted" toast appears after delete
**Why human:** Toast show/hide lifecycle requires a live Blazor circuit to observe

---

## Gaps Summary

No gaps. All 15 observable truths are verified at all three levels (exists, substantive, wired). All 7 TMPL requirements are covered. All 50 tests in the full suite pass (11 new phase-3 tests + 39 prior). The build produces 0 errors and 0 warnings.

The 4 human-verification items are interaction-level checks that cannot be confirmed by static code analysis alone. The automated wiring for all four is complete and correct.

---

_Verified: 2026-03-21_
_Verifier: Claude (gsd-verifier)_
