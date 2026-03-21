# Testing Patterns

**Analysis Date:** 2026-03-21

## Test Framework

**Status:** Not Implemented

**No test framework is currently configured or present in this project.**

- Runner: None configured
- Test projects: None found
- Assertion library: None
- Test dependencies: None in `BlazorApp2.csproj`

**Project Configuration:**
- `BlazorApp2.csproj` contains only web SDK dependencies
- No `<PackageReference>` entries for testing libraries (xUnit, NUnit, MSTest)
- No separate test project (`.Tests` or `.Test` project file)
- No test files found in codebase (no `*.test.cs` or `*.spec.cs` files)

## Test File Organization

**Current State:** Not applicable - no tests present

**Recommended Location for Future Tests:**
- Co-located pattern: `Component.razor.cs` for component code-behind
- Test suffix: Could use `Component.razor.tests.cs` following project naming conventions
- Or: Separate test project `BlazorApp2.Tests/` at solution level

**Naming Convention:**
- Follow `PascalCase.razor.Tests.cs` pattern to match component naming

## Test Structure

**Recommended Pattern for Blazor Components:**
```csharp
// Not yet implemented in codebase
// Example structure:
[TestClass]
public class HomeComponentTests
{
    [TestMethod]
    public void ComponentRendersWithCorrectTitle()
    {
        // Arrange
        // Act
        // Assert
    }
}
```

**Current Component Code-Behind Pattern in `Error.razor`:**
```csharp
@code{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId { get; set; }
    private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    protected override void OnInitialized() =>
        RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}
```

This code pattern would require:
- Component testing library (e.g., `bUnit` for Blazor)
- Cascading parameter injection in tests
- Lifecycle method testing (`OnInitialized`)
- Property assertions for computed values

## Mocking

**Framework:** Not configured

**Current Practice:**
- `HttpContext` injected via `[CascadingParameter]` in `Components/Pages/Error.razor`
- Could be mocked in tests using Blazor testing utilities

**Recommended Approach for Future Tests:**
- Use `bUnit` (Blazor Unit Testing Library) for component testing
- Mock `HttpContext` cascading parameter
- Mock `Activity.Current` for request ID testing
- Mock external services if added to project

## Fixtures and Factories

**Test Data:** Not applicable - no tests present

**Recommended Location for Future Use:**
- Could create `Tests/Fixtures/` or `Tests/Builders/` directory
- Test data builders for complex scenarios
- Shared test component templates

## Coverage

**Requirements:** Not enforced

**No code coverage tools configured:**
- No coverage badge in repository
- No coverage thresholds in CI/CD (no CI/CD configured)
- No coverage reports generated

**To Add Coverage:**
- Configure test framework (xUnit, NUnit, or MSTest)
- Add code coverage tool (OpenCover, Coverlet)
- Set coverage targets in CI/CD pipeline

## Test Types

**Unit Tests:** Not implemented

**Integration Tests:** Not implemented

**E2E Tests:** Not implemented

**Current Manual Testing Approach:**
- Application runs via `dotnet run` with profiles in `Properties/launchSettings.json`
- Profile `http`: Launches on `http://localhost:5235`
- Profile `https`: Launches on `https://localhost:7189;http://localhost:5235`
- Manual browser testing only

## Common Patterns

**Async Testing:** Not applicable

**Code-Behind Lifecycle Pattern (from `Error.razor`):**
- `@code { }` block for C# logic in components
- `protected override void OnInitialized()` for initialization
- `[CascadingParameter]` for dependency injection from parent components
- Property-based state management

**Property Null-Safety Pattern Observed:**
```csharp
private string? RequestId { get; set; }
private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
```

This pattern demonstrates nullable reference type usage and computed properties that could be tested.

## Recommended Next Steps

**Phase 1 - Foundation:**
1. Choose testing framework:
   - `bUnit` for component testing (Blazor-specific)
   - `xUnit` or `NUnit` for service/utility testing
2. Create test project: `BlazorApp2.Tests/` or `BlazorApp2.UnitTests/`
3. Add test dependencies to new project

**Phase 2 - Coverage:**
1. Add `Coverlet` for coverage reporting
2. Test critical components (Error handling, routing, state)
3. Set coverage threshold (recommended: 70%+)

**Phase 3 - CI/CD:**
1. Add GitHub Actions workflow for test execution
2. Enforce test pass requirements in pull requests
3. Publish coverage reports

**Components Recommended for Initial Testing:**
- `Error.razor`: Test `OnInitialized()` and `RequestId` computation
- `Routes.razor`: Test router configuration
- `Home.razor`: Test basic rendering
- Error handling middleware in `Program.cs`

---

*Testing analysis: 2026-03-21*
