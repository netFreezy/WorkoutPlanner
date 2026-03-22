---
phase: 06
slug: analytics
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-22
---

# Phase 06 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit with EF Core InMemory/SQLite |
| **Config file** | BlazorApp2.Tests/BlazorApp2.Tests.csproj |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "Category=Analytics"` |
| **Full suite command** | `dotnet test BlazorApp2.Tests` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "Category=Analytics"`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 06-01-01 | 01 | 1 | ANLY-01 | unit | `dotnet test --filter "AnalyticsService"` | ❌ W0 | ⬜ pending |
| 06-01-02 | 01 | 1 | ANLY-02 | unit | `dotnet test --filter "PRDetection"` | ❌ W0 | ⬜ pending |
| 06-02-01 | 02 | 1 | ANLY-03 | unit | `dotnet test --filter "Adherence"` | ❌ W0 | ⬜ pending |
| 06-02-02 | 02 | 1 | ANLY-04 | unit | `dotnet test --filter "Deviation"` | ❌ W0 | ⬜ pending |
| 06-03-01 | 03 | 2 | ANLY-05 | integration | `dotnet test --filter "AnalyticsDashboard"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `BlazorApp2.Tests/Services/AnalyticsServiceTests.cs` — stubs for ANLY-01, ANLY-03, ANLY-04
- [ ] `BlazorApp2.Tests/Services/PRDetectionServiceTests.cs` — stubs for ANLY-02
- [ ] Existing test infrastructure covers framework needs

*If none: "Existing infrastructure covers all phase requirements."*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Chart renders with tooltips | ANLY-01 | Visual rendering via JS interop | Load analytics page, hover chart data points, verify tooltip shows exact values |
| Dark theme integration | ANLY-05 | Visual CSS verification | Verify charts match dark premium theme tokens |
| Tab navigation UX | ANLY-05 | Interactive navigation flow | Click each tab, verify content switches correctly |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
