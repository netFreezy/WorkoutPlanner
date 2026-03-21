<!-- GSD:project-start source:PROJECT.md -->
## Project

**Unified Workout Planner**

A personal workout planning and tracking app built in Blazor that treats strength and endurance training as first-class citizens in the same system. Define workouts from a shared exercise library, schedule them on a calendar with recurrence, log sessions as you go, and track progress over time. Unlike apps like Strong or Runna, your entire training week — pull-ups and running sessions — lives in one place, and analytics understand both.

**Core Value:** A single system where you plan, log, and analyze both strength and endurance training side by side — your whole training week in one view.

### Constraints

- **Tech stack**: Blazor Server with .NET 10, C#, EF Core with SQLite — no JavaScript frameworks
- **Platform**: Server-side rendered with interactive server components via WebSocket
- **Data**: Local SQLite database, single-user, no cloud dependencies
- **UI**: Blazor components only, responsive web design for desktop and mobile browser use
<!-- GSD:project-end -->

<!-- GSD:stack-start source:codebase/STACK.md -->
## Technology Stack

## Languages
- C# - Used for server-side logic and Razor components in `Program.cs`, `Components/*.razor`
- HTML/CSS - Client-side markup and styling in `Components/*.razor` and `wwwroot/app.css`
- JavaScript - Embedded through Blazor framework runtime (`blazor.web.js`)
## Runtime
- .NET 10.0 (net10.0)
- ASP.NET Core 10.0.3 framework
- NuGet (Microsoft .NET package manager)
- Lockfile: project.assets.json present at `obj/project.assets.json`
## Frameworks
- ASP.NET Core 10.0.3 - Web framework and runtime
- Blazor Web (Interactive Server Components) - Full-stack C# web framework for building interactive web UI
- Microsoft.NET.Sdk.Web - Web SDK for ASP.NET Core projects
- dotnet CLI - Command-line interface (version 10.0.103 installed)
## Key Dependencies
- Microsoft.AspNetCore.App.Internal.Assets 10.0.3 - Framework runtime assets including Blazor JavaScript runtime
- Razor Components - Component-based UI framework
- Antiforgery middleware - CSRF protection configured in `Program.cs`
- Status code middleware - Error page handling
## Configuration
- Development environment configured via `ASPNETCORE_ENVIRONMENT` in `Properties/launchSettings.json`
- Separate appsettings files:
- Project file: `BlazorApp2.csproj`
- Launch profiles: `Properties/launchSettings.json`
- Features configured:
## Platform Requirements
- .NET 10.0 SDK or runtime required
- Visual Studio, Visual Studio Code, or JetBrains Rider recommended
- Windows/Linux/macOS compatible
- .NET 10.0 runtime
- Any platform supporting ASP.NET Core 10.0 (Windows, Linux, macOS, containers)
- HTTP/HTTPS capable server
<!-- GSD:stack-end -->

<!-- GSD:conventions-start source:CONVENTIONS.md -->
## Conventions

## Naming Patterns
- Razor components: PascalCase with `.razor` extension (e.g., `App.razor`, `MainLayout.razor`, `Home.razor`)
- Code-behind files: PascalCase with `.razor.cs` suffix (e.g., `Error.razor` contains inline code)
- Layout files: Placed in `Components/Layout/` directory with PascalCase names
- Page components: Placed in `Components/Pages/` with PascalCase names
- Supporting files: `.js`, `.css` files co-located with component (e.g., `ReconnectModal.razor.js`, `ReconnectModal.razor.css`)
- Global imports: `_Imports.razor` prefix (underscore convention for global files)
- PascalCase for class names (e.g., `HttpContext`)
- PascalCase for public properties (e.g., `RequestId`, `ShowRequestId`)
- Arrow functions used for simple expression-bodied members (e.g., `RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier;`)
- camelCase for local variables (e.g., `httpContext`, `traceIdentifier`)
- PascalCase for properties and auto-properties
- Nullable reference types enabled via `<Nullable>enable</Nullable>` in csproj
- Kebab-case for URL paths (e.g., `/not-found`, `/Error`)
- Page routing via `@page` directive at component top
## Code Style
- No explicit formatter configured in project
- Standard C# formatting conventions appear to be followed
- Indentation: 4 spaces
- No `.editorconfig`, `.eslintrc`, or StyleCop configuration detected
- Project uses implicit usings via `<ImplicitUsings>enable</ImplicitUsings>` in csproj
- `<Nullable>enable</Nullable>`: Enforces nullable reference types
- `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>`: Custom Blazor setting
- Target framework: `net10.0`
## Import Organization
- Enabled at project level, reducing explicit `using` statements needed
- Global usings for common namespaces automatically available
## Error Handling
- Conditional rendering based on state checks (e.g., `@if (ShowRequestId)`)
- Try-with-nullability checks using null-coalescing operator (`??`)
- Global exception handler via middleware in `Program.cs`: `app.UseExceptionHandler("/Error", createScopeForErrors: true)`
- Status code pages via `app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true)`
- Dedicated error pages: `Error.razor` and `NotFound.razor`
- Error boundary display via `<div id="blazor-error-ui">` in `App.razor`
- Receives `HttpContext` via `[CascadingParameter]`
- Displays `RequestId` using `Activity.Current?.Id ?? HttpContext?.TraceIdentifier`
- Environment-aware messages (Development vs Production)
- Shows request ID in `<code>` tags for debugging
## Logging
- No custom logging calls visible in source code
- Framework diagnostics captured via `Activity.Current` in error handling
## Comments
- No explicit comment guidelines observed
- Code is minimal and self-documenting
- No XML documentation comments (`///`) present in sample code
- Not applicable to this project
## Function Design
- C# cascade parameter pattern: `[CascadingParameter] private HttpContext? HttpContext { get; set; }`
- Method signatures use standard C# conventions
- Expression-bodied members for computed properties: `private bool ShowRequestId => !string.IsNullOrEmpty(RequestId);`
- Methods follow standard return conventions
## Module Design
- Razor components: Default export via component file structure
- No explicit export statements (Blazor component convention)
- `_Imports.razor` acts as namespace aggregator for all components
- `Components/_Imports.razor`: Global imports for entire component tree
- Layout components in `Components/Layout/`
- Page components in `Components/Pages/`
- Root component `App.razor` at `Components/` level
- Router configuration in `Routes.razor`
<!-- GSD:conventions-end -->

<!-- GSD:architecture-start source:ARCHITECTURE.md -->
## Architecture

## Pattern Overview
- Server-side rendering (SSR) with interactive server components
- Single Page Application (SPA) architecture with client-side routing
- Razor component-based UI structure
- Built on ASP.NET Core 10.0 web framework
- Default layout system with route-based page composition
## Layers
- Purpose: Render user interface and handle user interactions
- Location: `Components/` directory
- Contains: Razor components (.razor files), layout components, page components
- Depends on: ASP.NET Core components, routing services
- Used by: Browser requests via HTTP
- Purpose: Map URLs to components and manage navigation
- Location: `Components/Routes.razor` (Router configuration)
- Contains: Route definitions, NotFound handler, layout association
- Depends on: ASP.NET Core Router component
- Used by: App.razor (root component)
- Purpose: Bootstrap the application and define HTML structure
- Location: `Components/App.razor`
- Contains: HTML document structure, asset references, script injection
- Depends on: Routes, ReconnectModal components
- Used by: ASP.NET Core startup pipeline
- Purpose: Define shared visual structure and error handling
- Location: `Components/Layout/`
- Contains: MainLayout.razor (primary layout), ReconnectModal.razor (connection recovery UI)
- Depends on: ASP.NET Core LayoutComponentBase
- Used by: All routed pages via Router component
- Purpose: Implement routable pages with specific content
- Location: `Components/Pages/`
- Contains: Home.razor (primary page), Error.razor (error display), NotFound.razor (404 page)
- Depends on: Layout components, routing attributes
- Used by: Router component based on URL matching
- Purpose: Not currently present; available for future business logic
- Location: Would be in root or `Services/` directory
- Contains: Would contain API clients, data access, business rules
- Depends on: Would use external APIs or databases
- Used by: Component code-behind sections
## Data Flow
## Key Abstractions
- Purpose: Maps HTTP requests to Razor components based on routing rules
- Examples: `Components/Routes.razor`
- Pattern: Declares `@page` directives in individual page components; Router scans assembly for these declarations
- Purpose: Provides shared wrapper structure for pages
- Examples: `Components/Layout/MainLayout.razor`
- Pattern: Inherits from LayoutComponentBase, renders `@Body` directive to embed page content
- Purpose: Encapsulates UI markup and interaction logic
- Examples: `Components/Pages/Home.razor`, `Components/Layout/ReconnectModal.razor`
- Pattern: Mix of HTML markup and C# code in `@code` blocks; supports cascading parameters and event binding
- Purpose: Routable component representing a page
- Examples: `Components/Pages/Home.razor`, `Components/Pages/Error.razor`
- Pattern: Uses `@page` directive to define route; optionally uses `@layout` to specify layout wrapper
- Purpose: Provide UI feedback during server reconnection
- Examples: `Components/Layout/ReconnectModal.razor`
- Pattern: HTML dialog element with JavaScript interop for showing/hiding based on connection state
## Entry Points
- Location: `/Program.cs`
- Triggers: Application startup (dotnet run or production deployment)
- Responsibilities:
- Location: `Components/App.razor`
- Triggers: Initial page load in browser
- Responsibilities:
- Location: `Components/Routes.razor`
- Triggers: Every navigation event
- Responsibilities:
## Error Handling
- Unhandled exceptions caught by UseExceptionHandler middleware
- Re-executes request to `/Error` page for consistent error UI
- Displays Request ID (from Activity or HttpContext trace identifier)
- In Development mode, shows detailed exception information
- Error UI div with id "blazor-error-ui" displays unhandled client-side errors
- Provides reload button to recover connection
- Automatic reconnection attempts via ReconnectModal
- NotFound handler displays 404 page when no route matches
- UseStatusCodePagesWithReExecute maps status code responses to `/not-found` page
- Accesses HttpContext via CascadingParameter
- Displays Activity.Current?.Id for correlation
- Shows development-specific debug information when appropriate
## Cross-Cutting Concerns
- Default ASP.NET Core logging configured in `appsettings.json`
- Information level for general application, Warning level for ASP.NET Core framework
- Configured per environment in `appsettings.Development.json`
- No client-side validation framework detected
- Available through Microsoft.AspNetCore.Components.Forms imports in `_Imports.razor`
- Can be implemented in form components using EditForm and DataAnnotations
- Not currently implemented
- ASP.NET Core authentication services available through dependency injection
- Can be added via builder.Services.AddAuthentication() in Program.cs
- Antiforgery middleware enabled in pipeline (app.UseAntiforgery())
- Prevents cross-site request forgery attacks on state-changing operations
- CORS not explicitly configured (same-origin requests only by default)
- HTTPS redirection enforced in non-Development environments (app.UseHttpsRedirection())
- HSTS (HTTP Strict-Transport-Security) enabled with 30-day default in non-Development
- Static assets mapped before authentication for public content access
<!-- GSD:architecture-end -->

<!-- GSD:workflow-start source:GSD defaults -->
## GSD Workflow Enforcement

Before using Edit, Write, or other file-changing tools, start work through a GSD command so planning artifacts and execution context stay in sync.

Use these entry points:
- `/gsd:quick` for small fixes, doc updates, and ad-hoc tasks
- `/gsd:debug` for investigation and bug fixing
- `/gsd:execute-phase` for planned phase work

Do not make direct repo edits outside a GSD workflow unless the user explicitly asks to bypass it.
<!-- GSD:workflow-end -->



<!-- GSD:profile-start -->
## Developer Profile

> Profile not yet configured. Run `/gsd:profile-user` to generate your developer profile.
> This section is managed by `generate-claude-profile` -- do not edit manually.
<!-- GSD:profile-end -->
