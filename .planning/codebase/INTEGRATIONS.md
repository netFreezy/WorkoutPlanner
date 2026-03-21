# External Integrations

**Analysis Date:** 2026-03-21

## APIs & External Services

**Not detected**

No external APIs or third-party services are currently integrated. The application is a standalone ASP.NET Core Blazor application with no dependencies on external services.

## Data Storage

**Databases:**
- Not detected - No database provider configured

**File Storage:**
- Local filesystem only - Static assets served from `wwwroot/` directory

**Caching:**
- Not configured - No explicit caching middleware detected in `Program.cs`

## Authentication & Identity

**Auth Provider:**
- Not configured - No authentication middleware in `Program.cs`
- Custom: Application currently has no built-in authentication or authorization

**Implementation:**
- None detected - Routes are public with no access control

## Monitoring & Observability

**Error Tracking:**
- None - No error tracking service integrated

**Logs:**
- ASP.NET Core built-in logging only
- Configured via `appsettings.json` and `appsettings.Development.json`
- Default log level: Information
- Microsoft.AspNetCore log level: Warning
- No external logging provider (e.g., Serilog, Application Insights)

## CI/CD & Deployment

**Hosting:**
- Not configured - No hosting platform specified in configuration
- Application is configured to run on:
  - Development: localhost with ports 5235 (HTTP) and 7189 (HTTPS)
  - Production deployment method: Not specified

**CI Pipeline:**
- Not detected - No CI/CD configuration files (GitHub Actions, Azure DevOps, etc.)

## Environment Configuration

**Required env vars:**
- ASPNETCORE_ENVIRONMENT - Set to "Development" in launch profiles (only required variable)

**Secrets location:**
- No secrets configured
- appsettings files contain no sensitive data
- .env or secrets management: Not implemented

## Webhooks & Callbacks

**Incoming:**
- Not applicable - No webhook endpoints configured

**Outgoing:**
- Not applicable - No external service callbacks

## HTTP Configuration

**Pipeline:**
- Status code pages with re-execute to `/not-found` (error handling)
- HTTPS redirection enabled (except in development)
- HSTS enabled in production (30-day default)
- Antiforgery protection enabled

**Content Security:**
- No explicit CORS configuration
- No rate limiting
- No request logging middleware

---

*Integration audit: 2026-03-21*
