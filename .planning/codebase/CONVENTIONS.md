# Coding Conventions

**Analysis Date:** 2026-03-21

## Naming Patterns

**Files:**
- Razor components: PascalCase with `.razor` extension (e.g., `App.razor`, `MainLayout.razor`, `Home.razor`)
- Code-behind files: PascalCase with `.razor.cs` suffix (e.g., `Error.razor` contains inline code)
- Layout files: Placed in `Components/Layout/` directory with PascalCase names
- Page components: Placed in `Components/Pages/` with PascalCase names
- Supporting files: `.js`, `.css` files co-located with component (e.g., `ReconnectModal.razor.js`, `ReconnectModal.razor.css`)
- Global imports: `_Imports.razor` prefix (underscore convention for global files)

**C# Classes and Methods:**
- PascalCase for class names (e.g., `HttpContext`)
- PascalCase for public properties (e.g., `RequestId`, `ShowRequestId`)
- Arrow functions used for simple expression-bodied members (e.g., `RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;`)

**Variables:**
- camelCase for local variables (e.g., `httpContext`, `traceIdentifier`)
- PascalCase for properties and auto-properties
- Nullable reference types enabled via `<Nullable>enable</Nullable>` in csproj

**Routes:**
- Kebab-case for URL paths (e.g., `/not-found`, `/Error`)
- Page routing via `@page` directive at component top

## Code Style

**Formatting:**
- No explicit formatter configured in project
- Standard C# formatting conventions appear to be followed
- Indentation: 4 spaces

**Linting:**
- No `.editorconfig`, `.eslintrc`, or StyleCop configuration detected
- Project uses implicit usings via `<ImplicitUsings>enable</ImplicitUsings>` in csproj

**Project Configuration:**
- `<Nullable>enable</Nullable>`: Enforces nullable reference types
- `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>`: Custom Blazor setting
- Target framework: `net10.0`

## Import Organization

**Order in Razor Files (`Components/_Imports.razor`):**
1. System namespaces (`System.Net.Http`, `System.Net.Http.Json`)
2. ASP.NET Core namespaces (`Microsoft.AspNetCore.Components*`)
3. Framework helpers (`static Microsoft.AspNetCore.Components.Web.RenderMode`)
4. JavaScript interop (`Microsoft.JSInterop`)
5. Application namespaces (`BlazorApp2`, `BlazorApp2.Components`)

**Order in C# Code (`Program.cs`):**
1. Application namespace imports (one `using BlazorApp2.Components`)
2. Followed by builder initialization

**Implicit Usings:**
- Enabled at project level, reducing explicit `using` statements needed
- Global usings for common namespaces automatically available

## Error Handling

**Patterns:**
- Conditional rendering based on state checks (e.g., `@if (ShowRequestId)`)
- Try-with-nullability checks using null-coalescing operator (`??`)
- Global exception handler via middleware in `Program.cs`: `app.UseExceptionHandler("/Error", createScopeForErrors: true)`
- Status code pages via `app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true)`
- Dedicated error pages: `Error.razor` and `NotFound.razor`
- Error boundary display via `<div id="blazor-error-ui">` in `App.razor`

**Error Page Pattern in `Components/Pages/Error.razor`:**
- Receives `HttpContext` via `[CascadingParameter]`
- Displays `RequestId` using `Activity.Current?.Id ?? HttpContext?.TraceIdentifier`
- Environment-aware messages (Development vs Production)
- Shows request ID in `<code>` tags for debugging

## Logging

**Framework:** Not configured. Application uses default ASP.NET Core logging via configuration in `appsettings.json`.

**Configuration:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Patterns:**
- No custom logging calls visible in source code
- Framework diagnostics captured via `Activity.Current` in error handling

## Comments

**When to Comment:**
- No explicit comment guidelines observed
- Code is minimal and self-documenting
- No XML documentation comments (`///`) present in sample code

**JSDoc/TSDoc:**
- Not applicable to this project

## Function Design

**Size:** Single-line arrow functions preferred for simple expressions.

**Parameters:**
- C# cascade parameter pattern: `[CascadingParameter] private HttpContext? HttpContext { get; set; }`
- Method signatures use standard C# conventions

**Return Values:**
- Expression-bodied members for computed properties: `private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);`
- Methods follow standard return conventions

## Module Design

**Exports:**
- Razor components: Default export via component file structure
- No explicit export statements (Blazor component convention)

**Barrel Files:**
- `_Imports.razor` acts as namespace aggregator for all components
- `Components/_Imports.razor`: Global imports for entire component tree

**Component Organization:**
- Layout components in `Components/Layout/`
- Page components in `Components/Pages/`
- Root component `App.razor` at `Components/` level
- Router configuration in `Routes.razor`

---

*Convention analysis: 2026-03-21*
