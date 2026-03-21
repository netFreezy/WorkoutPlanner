# Phase 2: Exercise Library - Research

**Researched:** 2026-03-21
**Domain:** Blazor Server interactive components, EF Core data seeding (TPH), responsive UI without component libraries
**Confidence:** HIGH

## Summary

Phase 2 is the first UI phase in the project. The data model is already complete from Phase 1 -- Exercise base class with StrengthExercise and EnduranceExercise TPH subtypes, all enums (MuscleGroup, Equipment, ActivityType), and EF Core configurations including the TPH discriminator. The project uses `IDbContextFactory<AppDbContext>` for Blazor Server thread safety, with no Bootstrap or third-party component libraries -- all styling is plain CSS.

This phase requires: (1) seeding ~50 exercises into the database via EF Core migration, (2) building a browsable card grid with search and filter, and (3) a custom exercise creation dialog. The main technical concerns are the correct HasData pattern for TPH derived types in EF Core (must call HasData on the derived entity type, not the base), the IDbContextFactory usage pattern for interactive components, and building reusable dialog/modal components without JavaScript.

**Primary recommendation:** Use EF Core HasData on derived types (StrengthExercise/EnduranceExercise separately, with explicit IDs) for seed data. Build the exercise library page as a single interactive server component with CSS Grid card layout, input-driven filtering (no apply button), and a pure-CSS dialog component for detail/create views. Keep the architecture simple -- access DbContext directly from components via IDbContextFactory, no service layer needed yet.

<user_constraints>

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** Card grid layout for displaying exercises
- **D-02:** Cards show exercise name and a subtle type tag (strength/endurance) -- minimal info, not cluttered
- **D-03:** Clicking a card opens a detail dialog showing full info (name, description, type, muscle group, equipment/activity type)
- **D-04:** Strength and endurance exercises appear in a unified list, distinguished by subtle type tags (not separate tabs or sections)
- **D-05:** Search bar and filter controls live in a top bar above the card grid
- **D-06:** Instant filtering -- results update as user types or selects filter values, no apply button
- **D-07:** Filters use AND across categories -- selecting "Chest" + "Barbell" requires both to match; unset categories are ignored
- **D-08:** Active filters shown as chips with X to remove individually, plus a "clear all" button
- **D-09:** "Add exercise" triggered by a floating action button (FAB)
- **D-10:** Creation form opens in a dialog (consistent with detail view pattern)
- **D-11:** Toggle/radio at top of form to pick strength vs endurance type -- form fields adapt below (muscle group + equipment for strength, activity type for endurance)
- **D-12:** After save: brief success message, dialog closes, catalog scrolls to the new exercise
- **D-13:** ~35-40 strength exercises, ~10-15 endurance exercises
- **D-14:** Calisthenics-focused strength selection -- weighted pull-ups, dips, bodyweight movements for home workouts
- **D-15:** Equipment bias toward what's available at home: bodyweight, dumbbell, dip bars, weighted vest, kettlebell -- not gym-machine heavy
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

### Deferred Ideas (OUT OF SCOPE)
None -- discussion stayed within phase scope.

</user_constraints>

<phase_requirements>

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| EXER-01 | Searchable, filterable exercise catalog by name, type, muscle group, and equipment | Card grid with CSS Grid, instant filter/search via component state, IDbContextFactory for data loading, TPH query support via OfType<T> |
| EXER-02 | Custom exercise creation with name, type, muscle group, equipment, optional notes | EditForm + DataAnnotationsValidator, polymorphic form with conditional fields, IDbContextFactory for save operations |
| EXER-03 | Seed database with ~50 common exercises across strength and endurance | EF Core HasData on StrengthExercise/EnduranceExercise derived types with explicit IDs, new migration |

</phase_requirements>

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.5 | Database access, TPH queries, seed data | Already installed from Phase 1; used for all data operations |
| ASP.NET Core Blazor Server | 10.0.3 | Interactive server components | Project framework; all UI runs server-side via WebSocket |
| CSS Grid + Flexbox | N/A (browser standard) | Card grid layout, responsive design | No Bootstrap in project; CSS Grid is the standard approach for card layouts |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| bUnit | 2.6.2 | Component rendering tests | If adding Blazor component tests (optional for this phase) |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Plain CSS dialog | Blazored.Modal (NuGet library) | Adds dependency; project constraint says no JS frameworks; plain CSS/HTML dialog element is sufficient and simpler |
| Plain CSS Grid | QuickGrid (built-in .NET 8+) | QuickGrid is tabular, not card-based; wrong visual pattern for D-01's card grid requirement |
| Service/repository layer | Direct IDbContextFactory in components | For ~50 exercises with simple CRUD, a service layer adds indirection without benefit; revisit in Phase 3 if needed |

**Installation:**
No new packages needed. All dependencies are already installed from Phase 1.

## Architecture Patterns

### Recommended Project Structure
```
Components/
  Pages/
    Exercises.razor              # Main exercise library page (@page "/exercises")
    Exercises.razor.cs           # Code-behind for the page
    Exercises.razor.css          # Scoped CSS for card grid, search bar, filters
  Shared/
    ExerciseCard.razor           # Reusable card component
    ExerciseCard.razor.css       # Card styling
    ExerciseDetailDialog.razor   # Detail view dialog (read-only)
    ExerciseFormDialog.razor     # Create/edit form dialog
    ExerciseFormDialog.razor.css # Dialog form styling
    Dialog.razor                 # Generic reusable dialog wrapper component
    Dialog.razor.css             # Dialog overlay + panel styling
    FilterChip.razor             # Filter chip with X remove button
    FilterChip.razor.css         # Chip styling
Data/
  Configurations/
    ExerciseConfiguration.cs     # Updated with HasData seed (already exists)
  SeedData/
    ExerciseSeedData.cs          # Static class with seed exercise arrays
Migrations/
  {timestamp}_SeedExercises.cs   # New migration for seed data
```

### Pattern 1: IDbContextFactory Per-Operation Pattern
**What:** Create and dispose a DbContext for each discrete database operation, not per component lifetime.
**When to use:** For read-only queries and simple save operations where change tracking is not needed across multiple interactions.
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core
@inject IDbContextFactory<AppDbContext> DbFactory

private List<Exercise> exercises = new();

protected override async Task OnInitializedAsync()
{
    await LoadExercisesAsync();
}

private async Task LoadExercisesAsync()
{
    using var context = DbFactory.CreateDbContext();
    exercises = await context.Exercises
        .OrderBy(e => e.Name)
        .ToListAsync();
}
```

### Pattern 2: Client-Side Filtering (No Re-Query)
**What:** Load all exercises once, filter in-memory using LINQ on the component's list.
**When to use:** When the full dataset is small enough to hold in memory (~50-100 items). Avoids database round-trips on every keystroke.
**Example:**
```csharp
private List<Exercise> allExercises = new();
private string searchText = "";
private MuscleGroup? selectedMuscleGroup;
private Equipment? selectedEquipment;

private IEnumerable<Exercise> FilteredExercises => allExercises
    .Where(e => string.IsNullOrEmpty(searchText)
        || e.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
    .Where(e => selectedMuscleGroup == null
        || (e is StrengthExercise se && se.MuscleGroup == selectedMuscleGroup))
    .Where(e => selectedEquipment == null
        || (e is StrengthExercise se && se.Equipment == selectedEquipment));
```

### Pattern 3: Pure CSS Dialog Component
**What:** A reusable dialog component using CSS overlay + the HTML `<dialog>` element or a CSS-controlled div, toggled by a bool parameter.
**When to use:** For detail views and forms that appear as modal overlays. No JavaScript needed.
**Example:**
```csharp
// Dialog.razor
@if (IsOpen)
{
    <div class="dialog-overlay" @onclick="Close">
        <div class="dialog-panel" @onclick:stopPropagation="true">
            <div class="dialog-header">
                <h2>@Title</h2>
                <button class="dialog-close" @onclick="Close">&times;</button>
            </div>
            <div class="dialog-body">
                @ChildContent
            </div>
        </div>
    </div>
}

@code {
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task Close() => await OnClose.InvokeAsync();
}
```

### Pattern 4: EditForm with Polymorphic Model
**What:** An EditForm whose visible fields change based on the exercise type selection (strength vs endurance).
**When to use:** For the exercise creation dialog (D-11).
**Example:**
```csharp
// Use a view model that can represent either type
public class ExerciseFormModel
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsStrength { get; set; } = true;

    // Strength-specific
    public MuscleGroup MuscleGroup { get; set; }
    public Equipment Equipment { get; set; }

    // Endurance-specific
    public ActivityType ActivityType { get; set; }
}
```

### Pattern 5: EF Core HasData for TPH Derived Types
**What:** Seed data for TPH hierarchies must be called on the derived type's entity configuration, not the base type.
**When to use:** For EXER-03 seed data.
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
// MUST use modelBuilder.Entity<DerivedType>().HasData(), NOT modelBuilder.Entity<BaseType>().HasData()
// MUST specify explicit Id values (auto-generation not supported with HasData)

modelBuilder.Entity<StrengthExercise>().HasData(
    new StrengthExercise { Id = 1, Name = "Pull-Up", Description = "Hang from bar, pull chin over...",
        MuscleGroup = MuscleGroup.Back, Equipment = Equipment.Bodyweight,
        CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
    new StrengthExercise { Id = 2, Name = "Weighted Dip", Description = "Dip bars, add weight...",
        MuscleGroup = MuscleGroup.Chest, Equipment = Equipment.Bodyweight,
        CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
);

modelBuilder.Entity<EnduranceExercise>().HasData(
    new EnduranceExercise { Id = 101, Name = "Easy Run", Description = "Conversational pace...",
        ActivityType = ActivityType.Run,
        CreatedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
);
```

### Anti-Patterns to Avoid
- **Scoped DbContext injection:** Never use `@inject AppDbContext` in Blazor Server -- circuits keep it alive too long, causing stale data and concurrency issues. Always use IDbContextFactory.
- **Re-querying on every filter change:** With only ~50 exercises, hitting the database on each keystroke or filter toggle is wasteful. Load once, filter in-memory.
- **HasData on base type for TPH:** `modelBuilder.Entity<Exercise>().HasData(new StrengthExercise{...})` throws `InvalidOperationException`. Must call HasData on each derived type separately.
- **JavaScript interop for dialogs:** The project constraint is "no JavaScript frameworks." A CSS-only dialog component is sufficient and stays within the Blazor-only constraint.
- **Over-componentizing:** For a page with ~3 interactive areas (search bar, card grid, dialog), splitting into too many tiny components adds complexity. A page + 3-4 child components is the right granularity.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Form validation | Custom validation logic | EditForm + DataAnnotationsValidator | Built into Blazor, handles field-level and summary messages, CSS classes already defined in app.css |
| Responsive grid | Manual media queries for each breakpoint | CSS Grid `auto-fill` / `minmax()` | One CSS rule handles all breakpoints: `grid-template-columns: repeat(auto-fill, minmax(280px, 1fr))` |
| Enum to dropdown | Manual option rendering | Enum.GetValues + select binding | Standard C# pattern, no hand-rolled mapping needed |
| TPH type checking | String-based discriminator checks | C# `is` pattern matching (`e is StrengthExercise se`) | Type-safe, compiler-checked, works with EF Core materialized entities |
| Seed data management | Manual SQL inserts | EF Core HasData in configuration | Tracked by migrations, repeatable, type-safe |

**Key insight:** This phase has no technically novel problems. Every piece -- card grids, dialogs, forms, filtering, seeding -- has well-established patterns in Blazor + EF Core. The risk is in getting the integration right (TPH seeding, IDbContextFactory lifecycle), not in any individual piece.

## Common Pitfalls

### Pitfall 1: HasData on Base Type with TPH
**What goes wrong:** Calling `modelBuilder.Entity<Exercise>().HasData(new StrengthExercise{...})` throws InvalidOperationException at migration generation time.
**Why it happens:** EF Core requires seed data for derived types to be added via the derived entity type builder, not the base type.
**How to avoid:** Always use `modelBuilder.Entity<StrengthExercise>().HasData(...)` and `modelBuilder.Entity<EnduranceExercise>().HasData(...)` separately.
**Warning signs:** Migration generation fails with "The seed entity for entity type 'Exercise' cannot be added because the value provided is of a derived type."

### Pitfall 2: DateTime.UtcNow in HasData Seed
**What goes wrong:** Using `DateTime.UtcNow` in HasData causes a new migration to be generated every time `dotnet ef migrations add` runs, because the value changes.
**Why it happens:** HasData values must be deterministic -- EF Core compares them across migrations.
**How to avoid:** Use a fixed date: `new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)` for all seed data CreatedDate values.
**Warning signs:** Unexpected migration diff showing only timestamp changes.

### Pitfall 3: Missing Id in HasData
**What goes wrong:** Omitting the Id property in HasData objects causes EF Core to fail, because auto-generated keys are not supported in model-managed data.
**Why it happens:** HasData requires explicit primary keys since it can't rely on database-generated values during migration script generation.
**How to avoid:** Always set explicit Id values. Use ID ranges to separate types: 1-100 for strength, 101-200 for endurance (or similar).
**Warning signs:** "The seed entity for entity type requires a value for the 'Id' property."

### Pitfall 4: DbContext Lifetime in Interactive Components
**What goes wrong:** Injecting a DbContext directly or keeping one alive for the component lifetime leads to stale data after other components modify the database, or ObjectDisposedException if the circuit outlives the scope.
**Why it happens:** Blazor Server circuits are long-lived; scoped DbContext was designed for short HTTP request lifetimes.
**How to avoid:** Use `IDbContextFactory<AppDbContext>`, create per-operation with `using var context = DbFactory.CreateDbContext()`.
**Warning signs:** Stale data after creating an exercise, or exceptions mentioning disposed objects.

### Pitfall 5: Filter State Not Resetting After Exercise Creation
**What goes wrong:** User creates an exercise while filters are active, dialog closes, but the new exercise is hidden by filters. User thinks it was not saved.
**Why it happens:** Filters remain applied after creation. The new exercise may not match current filter criteria.
**How to avoid:** After successful creation, either clear all filters or temporarily highlight the new exercise regardless of filter state (per D-12: "catalog scrolls to the new exercise").
**Warning signs:** User creates exercise, sees success message, but cannot find it in the grid.

### Pitfall 6: Enum Display Names
**What goes wrong:** Enum values like `FullBody` display as "FullBody" instead of "Full Body" in dropdowns and cards.
**Why it happens:** C# enum names cannot contain spaces; default ToString() returns the identifier.
**How to avoid:** Use a display helper method or `[Display(Name = "Full Body")]` attribute on enum values, plus a helper to extract the display name.
**Warning signs:** Awkward enum value display in UI (e.g., "StairClimber" instead of "Stair Climber").

## Code Examples

### Loading Exercises with TPH in Blazor Component
```csharp
// Source: EF Core TPH documentation + Blazor IDbContextFactory pattern
@inject IDbContextFactory<AppDbContext> DbFactory

@code {
    private List<Exercise> allExercises = new();

    protected override async Task OnInitializedAsync()
    {
        using var context = DbFactory.CreateDbContext();
        // EF Core automatically materializes as StrengthExercise or EnduranceExercise
        allExercises = await context.Exercises
            .OrderBy(e => e.Name)
            .ToListAsync();
    }
}
```

### Enum to Select Dropdown
```csharp
// Render a select element from any enum type
<select @bind="selectedMuscleGroup">
    <option value="">All Muscle Groups</option>
    @foreach (var value in Enum.GetValues<MuscleGroup>())
    {
        <option value="@value">@FormatEnumName(value)</option>
    }
</select>

@code {
    private MuscleGroup? selectedMuscleGroup;

    private string FormatEnumName<T>(T value) where T : Enum
    {
        // Insert spaces before capital letters: "FullBody" -> "Full Body"
        return System.Text.RegularExpressions.Regex.Replace(
            value.ToString(), "(?<!^)([A-Z])", " $1");
    }
}
```

### CSS Grid Card Layout (Responsive)
```css
/* Source: CSS Grid standard pattern for responsive card grids */
.exercise-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
    gap: 1rem;
    padding: 1rem;
}

.exercise-card {
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    padding: 1rem;
    cursor: pointer;
    transition: box-shadow 0.2s ease;
}

.exercise-card:hover {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
}

.type-tag {
    display: inline-block;
    font-size: 0.75rem;
    padding: 0.125rem 0.5rem;
    border-radius: 12px;
    font-weight: 500;
}

.type-tag--strength {
    background: #e3f2fd;
    color: #1565c0;
}

.type-tag--endurance {
    background: #e8f5e9;
    color: #2e7d32;
}
```

### CSS Dialog Overlay
```css
/* Source: Standard CSS modal overlay pattern */
.dialog-overlay {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 100;
}

.dialog-panel {
    background: white;
    border-radius: 12px;
    max-width: 500px;
    width: 90%;
    max-height: 85vh;
    overflow-y: auto;
    padding: 1.5rem;
}

.dialog-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1rem;
}

.dialog-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
    color: #666;
}
```

### Saving a New Exercise (Polymorphic)
```csharp
// Source: EF Core TPH + Blazor form pattern
private async Task SaveExerciseAsync(ExerciseFormModel model)
{
    using var context = DbFactory.CreateDbContext();

    Exercise exercise;
    if (model.IsStrength)
    {
        exercise = new StrengthExercise
        {
            Name = model.Name,
            Description = model.Description,
            MuscleGroup = model.MuscleGroup,
            Equipment = model.Equipment,
        };
    }
    else
    {
        exercise = new EnduranceExercise
        {
            Name = model.Name,
            Description = model.Description,
            ActivityType = model.ActivityType,
        };
    }

    context.Exercises.Add(exercise);
    await context.SaveChangesAsync();

    // Reload the full list to include the new exercise
    await LoadExercisesAsync();
}
```

### Filter Chip Component
```csharp
// FilterChip.razor
<span class="filter-chip">
    @Label
    <button class="filter-chip-remove" @onclick="OnRemove">&times;</button>
</span>

@code {
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public EventCallback OnRemove { get; set; }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| HasData (model managed data) for seeding | UseSeeding / UseAsyncSeeding (EF Core 9+) | EF Core 9 (Nov 2024) | UseSeeding is now the recommended approach for general seeding, but HasData is still appropriate for small, fixed, deterministic datasets like exercise catalogs |
| AddDbContext (scoped) | AddDbContextFactory (transient factory) | EF Core 5+ / Blazor Server pattern | Required for Blazor Server to avoid stale context issues |
| Bootstrap CSS grid | CSS Grid native | Browser standard | No external dependency; `auto-fill` + `minmax()` handles responsiveness in one line |

**Note on HasData vs UseSeeding:** Microsoft now recommends UseSeeding for general-purpose data seeding (EF Core 9+). However, HasData remains the correct choice for this use case -- small, fixed, deterministic data (~50 exercises) that should be tracked by migrations. UseSeeding runs at application startup (not migration time), which is better for data that changes or depends on database state.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit 2.9.3 + Microsoft.NET.Test.Sdk 17.14.1 |
| Config file | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| Quick run command | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExerciseLibrary" --no-build` |
| Full suite command | `dotnet test BlazorApp2.Tests` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| EXER-01 | Query exercises with TPH, filter by type/muscle/equipment | unit (data) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExerciseFilterTests" -x` | No -- Wave 0 |
| EXER-02 | Create StrengthExercise and EnduranceExercise via DbContext | unit (data) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExerciseCreateTests" -x` | No -- Wave 0 |
| EXER-03 | Seed data loads ~50 exercises via migration / EnsureCreated | unit (data) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExerciseSeedTests" -x` | No -- Wave 0 |
| EXER-01 | Search filters results by name substring | unit (data) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExerciseFilterTests" -x` | No -- Wave 0 |

### Sampling Rate
- **Per task commit:** `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Exercise" -x`
- **Per wave merge:** `dotnet test BlazorApp2.Tests`
- **Phase gate:** Full suite green before `/gsd:verify-work`

### Wave 0 Gaps
- [ ] `BlazorApp2.Tests/ExerciseSeedTests.cs` -- verifies seed data count and composition (strength/endurance split)
- [ ] `BlazorApp2.Tests/ExerciseFilterTests.cs` -- verifies in-memory filtering logic (search, type, muscle group, equipment, AND composition)
- [ ] `BlazorApp2.Tests/ExerciseCreateTests.cs` -- verifies creating both StrengthExercise and EnduranceExercise via DbContext

Note: UI component tests with bUnit are optional for this phase. The core logic (filtering, CRUD, seeding) can be tested at the data layer using the existing DataTestBase pattern without adding bUnit as a dependency.

## Open Questions

1. **Navigation entry point for exercise library**
   - What we know: MainLayout.razor currently renders only `@Body` with error UI -- no navigation sidebar or top nav exists yet
   - What's unclear: Should Phase 2 add navigation infrastructure (sidebar/top nav with links) or just create the page at `/exercises` and defer navigation to a later phase?
   - Recommendation: Add minimal navigation (a simple nav bar with Home + Exercises links) since this is the first real page. Keep it simple -- a top nav bar with two links.

2. **Custom exercise ID collision with seed data**
   - What we know: HasData requires explicit IDs (e.g., 1-50). User-created exercises get auto-generated IDs from SQLite.
   - What's unclear: Will SQLite's autoincrement start after the seeded max ID?
   - Recommendation: SQLite AUTOINCREMENT (or ROWID) uses the max existing value + 1, so seeded IDs 1-50 will cause new inserts to start at 51+. No collision risk, but use contiguous ranges and document the reserved ID range.

3. **Dip bars as equipment value**
   - What we know: Equipment enum has: Barbell, Dumbbell, Bodyweight, Cable, Machine, Band, Kettlebell, Other. User trains with dip bars but there is no explicit "DipBars" enum value.
   - What's unclear: Should dip-bar exercises be categorized as "Bodyweight" (since dip bars are just an assist) or should a new enum value be added?
   - Recommendation: Use "Bodyweight" for dip bar exercises (dip bars are a bodyweight station, not separate equipment). Adding enum values requires a migration and affects Phase 1's schema. "Other" is the fallback if Bodyweight feels wrong.

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn: Blazor with EF Core](https://learn.microsoft.com/en-us/aspnet/core/blazor/blazor-ef-core?view=aspnetcore-10.0) -- IDbContextFactory pattern, per-operation and per-component-lifetime approaches
- [Microsoft Learn: EF Core Data Seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) -- HasData limitations, UseSeeding alternative, explicit ID requirement, TPH derived type requirement
- [Microsoft Learn: EF Core Inheritance](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance) -- TPH discriminator configuration
- [Microsoft Learn: Blazor Forms Validation](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/validation?view=aspnetcore-10.0) -- EditForm + DataAnnotationsValidator pattern
- [Microsoft Learn: Blazor Virtualization](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/virtualization?view=aspnetcore-10.0) -- Virtualize component (not needed for ~50 items but documented for future reference)

### Secondary (MEDIUM confidence)
- [Telerik: JavaScript-Free Blazor Modal](https://www.telerik.com/blogs/creating-a-reusable-javascript-free-blazor-modal) -- Pure CSS modal pattern verified against Blazor component model
- [GitHub: EF Core Issue #12841](https://github.com/dotnet/efcore/issues/12841) -- Confirms HasData on base type throws for derived types in TPH
- [bUnit.dev](https://bunit.dev/) -- bUnit 2.6.2 supports .NET 10, xUnit integration confirmed
- [NuGet: bUnit 2.6.2](https://www.nuget.org/packages/bunit/) -- Latest version confirmed via NuGet search

### Tertiary (LOW confidence)
- CSS Grid responsive patterns -- based on widely documented web standard, no single authoritative source needed

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all technologies already in use from Phase 1, no new dependencies needed
- Architecture: HIGH -- standard Blazor Server patterns documented by Microsoft, verified against current .NET 10 docs
- Pitfalls: HIGH -- HasData TPH limitation confirmed via EF Core GitHub issue and official docs; IDbContextFactory pattern well-documented
- Seed data composition: MEDIUM -- exact exercise list is Claude's discretion per CONTEXT.md, but composition guidelines (D-13 through D-17) are locked

**Research date:** 2026-03-21
**Valid until:** 2026-04-21 (stable technologies, no fast-moving dependencies)
