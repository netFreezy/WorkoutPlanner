# Architecture

**Analysis Date:** 2026-03-21

## Pattern Overview

**Overall:** Component-based server-side rendered web application using ASP.NET Core Blazor

**Key Characteristics:**
- Server-side rendering (SSR) with interactive server components
- Single Page Application (SPA) architecture with client-side routing
- Razor component-based UI structure
- Built on ASP.NET Core 10.0 web framework
- Default layout system with route-based page composition

## Layers

**Presentation Layer:**
- Purpose: Render user interface and handle user interactions
- Location: `Components/` directory
- Contains: Razor components (.razor files), layout components, page components
- Depends on: ASP.NET Core components, routing services
- Used by: Browser requests via HTTP

**Routing Layer:**
- Purpose: Map URLs to components and manage navigation
- Location: `Components/Routes.razor` (Router configuration)
- Contains: Route definitions, NotFound handler, layout association
- Depends on: ASP.NET Core Router component
- Used by: App.razor (root component)

**Root Component Layer:**
- Purpose: Bootstrap the application and define HTML structure
- Location: `Components/App.razor`
- Contains: HTML document structure, asset references, script injection
- Depends on: Routes, ReconnectModal components
- Used by: ASP.NET Core startup pipeline

**Layout Layer:**
- Purpose: Define shared visual structure and error handling
- Location: `Components/Layout/`
- Contains: MainLayout.razor (primary layout), ReconnectModal.razor (connection recovery UI)
- Depends on: ASP.NET Core LayoutComponentBase
- Used by: All routed pages via Router component

**Page Layer:**
- Purpose: Implement routable pages with specific content
- Location: `Components/Pages/`
- Contains: Home.razor (primary page), Error.razor (error display), NotFound.razor (404 page)
- Depends on: Layout components, routing attributes
- Used by: Router component based on URL matching

**Service Layer:**
- Purpose: Not currently present; available for future business logic
- Location: Would be in root or `Services/` directory
- Contains: Would contain API clients, data access, business rules
- Depends on: Would use external APIs or databases
- Used by: Component code-behind sections

## Data Flow

**Page Request Flow:**

1. Browser makes HTTP request to `/` or other route
2. ASP.NET Core receives request and matches route via Router component
3. Routes.razor identifies matching page component (e.g., Home.razor)
4. MainLayout.razor applies layout wrapper
5. Page component renders with layout
6. App.razor injects required assets and framework scripts
7. ASP.NET Core returns complete HTML to browser
8. Browser renders page and establishes Blazor server connection

**Component Re-render Flow:**

1. User interaction triggers event in component (button click, form submission, etc.)
2. Event handler executes, updates component state
3. Component re-renders with updated state
4. Browser receives update via WebSocket connection
5. Browser applies DOM updates

**Navigation Flow:**

1. User clicks navigation link or uses NavigationManager
2. Router component detects route change
3. Old page component is disposed
4. New page component is instantiated
5. FocusOnNavigate component shifts focus to heading (h1)
6. Page renders with MainLayout

**Error Handling Flow:**

1. Unhandled exception occurs during request processing
2. Program.cs error handler middleware catches exception
3. Request re-executed to `/Error` page
4. Error.razor component displays error information
5. Displays Request ID from Activity or HttpContext
6. In Development environment, shows debug information

## Key Abstractions

**Router:**
- Purpose: Maps HTTP requests to Razor components based on routing rules
- Examples: `Components/Routes.razor`
- Pattern: Declares `@page` directives in individual page components; Router scans assembly for these declarations

**Layout Component Base:**
- Purpose: Provides shared wrapper structure for pages
- Examples: `Components/Layout/MainLayout.razor`
- Pattern: Inherits from LayoutComponentBase, renders `@Body` directive to embed page content

**Razor Component:**
- Purpose: Encapsulates UI markup and interaction logic
- Examples: `Components/Pages/Home.razor`, `Components/Layout/ReconnectModal.razor`
- Pattern: Mix of HTML markup and C# code in `@code` blocks; supports cascading parameters and event binding

**Page Component:**
- Purpose: Routable component representing a page
- Examples: `Components/Pages/Home.razor`, `Components/Pages/Error.razor`
- Pattern: Uses `@page` directive to define route; optionally uses `@layout` to specify layout wrapper

**Modal Component:**
- Purpose: Provide UI feedback during server reconnection
- Examples: `Components/Layout/ReconnectModal.razor`
- Pattern: HTML dialog element with JavaScript interop for showing/hiding based on connection state

## Entry Points

**Program.cs:**
- Location: `/Program.cs`
- Triggers: Application startup (dotnet run or production deployment)
- Responsibilities:
  - Configures service dependencies via builder.Services
  - Registers Razor components and interactive server rendering
  - Sets up middleware pipeline (exception handling, HTTPS, antiforgery, static assets)
  - Maps Razor components to route handler
  - Starts HTTP server

**App.razor:**
- Location: `Components/App.razor`
- Triggers: Initial page load in browser
- Responsibilities:
  - Defines HTML document structure
  - Loads stylesheets and framework resources
  - Renders Routes component (router)
  - Renders ReconnectModal component
  - Injects Blazor framework JavaScript

**Routes.razor:**
- Location: `Components/Routes.razor`
- Triggers: Every navigation event
- Responsibilities:
  - Declares Router component with application assembly
  - Handles successful route matches (Found context)
  - Applies default layout to matched pages
  - Triggers focus management on navigation
  - Handles not-found routes (404)

## Error Handling

**Strategy:** Multi-layered with environment-aware responses

**Patterns:**

**Middleware-level (Program.cs):**
- Unhandled exceptions caught by UseExceptionHandler middleware
- Re-executes request to `/Error` page for consistent error UI
- Displays Request ID (from Activity or HttpContext trace identifier)
- In Development mode, shows detailed exception information

**Client-side (App.razor):**
- Error UI div with id "blazor-error-ui" displays unhandled client-side errors
- Provides reload button to recover connection
- Automatic reconnection attempts via ReconnectModal

**Route-level:**
- NotFound handler displays 404 page when no route matches
- UseStatusCodePagesWithReExecute maps status code responses to `/not-found` page

**Component-level (Error.razor):**
- Accesses HttpContext via CascadingParameter
- Displays Activity.Current?.Id for correlation
- Shows development-specific debug information when appropriate

## Cross-Cutting Concerns

**Logging:**
- Default ASP.NET Core logging configured in `appsettings.json`
- Information level for general application, Warning level for ASP.NET Core framework
- Configured per environment in `appsettings.Development.json`

**Validation:**
- No client-side validation framework detected
- Available through Microsoft.AspNetCore.Components.Forms imports in `_Imports.razor`
- Can be implemented in form components using EditForm and DataAnnotations

**Authentication:**
- Not currently implemented
- ASP.NET Core authentication services available through dependency injection
- Can be added via builder.Services.AddAuthentication() in Program.cs

**CORS & Antiforgery:**
- Antiforgery middleware enabled in pipeline (app.UseAntiforgery())
- Prevents cross-site request forgery attacks on state-changing operations
- CORS not explicitly configured (same-origin requests only by default)

**HTTPS & Security:**
- HTTPS redirection enforced in non-Development environments (app.UseHttpsRedirection())
- HSTS (HTTP Strict-Transport-Security) enabled with 30-day default in non-Development
- Static assets mapped before authentication for public content access

---

*Architecture analysis: 2026-03-21*
