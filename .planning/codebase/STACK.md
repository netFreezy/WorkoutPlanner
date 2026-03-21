# Technology Stack

**Analysis Date:** 2026-03-21

## Languages

**Primary:**
- C# - Used for server-side logic and Razor components in `Program.cs`, `Components/*.razor`
- HTML/CSS - Client-side markup and styling in `Components/*.razor` and `wwwroot/app.css`

**Secondary:**
- JavaScript - Embedded through Blazor framework runtime (`blazor.web.js`)

## Runtime

**Environment:**
- .NET 10.0 (net10.0)
- ASP.NET Core 10.0.3 framework

**Package Manager:**
- NuGet (Microsoft .NET package manager)
- Lockfile: project.assets.json present at `obj/project.assets.json`

## Frameworks

**Core:**
- ASP.NET Core 10.0.3 - Web framework and runtime
- Blazor Web (Interactive Server Components) - Full-stack C# web framework for building interactive web UI
  - Rendering mode: Interactive Server with Razor Components
  - Configured in `Program.cs` via `.AddInteractiveServerComponents()` and `.AddRazorComponents()`

**Build/Dev:**
- Microsoft.NET.Sdk.Web - Web SDK for ASP.NET Core projects
- dotnet CLI - Command-line interface (version 10.0.103 installed)

## Key Dependencies

**Critical:**
- Microsoft.AspNetCore.App.Internal.Assets 10.0.3 - Framework runtime assets including Blazor JavaScript runtime
  - Provides `_framework/blazor.web.js` referenced in `Components/App.razor`
  - Provides `_framework/blazor.server.js` for server-side interactivity
  - Framework reference implicit via SDK

**Built-in (via ASP.NET Core SDK):**
- Razor Components - Component-based UI framework
- Antiforgery middleware - CSRF protection configured in `Program.cs`
- Status code middleware - Error page handling

## Configuration

**Environment:**
- Development environment configured via `ASPNETCORE_ENVIRONMENT` in `Properties/launchSettings.json`
- Separate appsettings files:
  - `appsettings.json` - Production configuration
  - `appsettings.Development.json` - Development-specific logging settings

**Build:**
- Project file: `BlazorApp2.csproj`
- Launch profiles: `Properties/launchSettings.json`
  - HTTP profile: `http://localhost:5235`
  - HTTPS profile: `https://localhost:7189` and `http://localhost:5235`
- Features configured:
  - `<Nullable>enable</Nullable>` - Null safety enabled
  - `<ImplicitUsings>enable</ImplicitUsings>` - Global using statements
  - `<BlazorDisableThrowNavigationException>true</BlazorDisableThrowNavigationException>` - Navigation exception handling disabled

## Platform Requirements

**Development:**
- .NET 10.0 SDK or runtime required
- Visual Studio, Visual Studio Code, or JetBrains Rider recommended
- Windows/Linux/macOS compatible

**Production:**
- .NET 10.0 runtime
- Any platform supporting ASP.NET Core 10.0 (Windows, Linux, macOS, containers)
- HTTP/HTTPS capable server

---

*Stack analysis: 2026-03-21*
