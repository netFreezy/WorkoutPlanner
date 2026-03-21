---
phase: 5
slug: session-tracking
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-22
---

# Phase 5 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 with Microsoft.NET.Test.Sdk 17.14.1 |
| **Config file** | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests" --no-build -q` |
| **Full suite command** | `dotnet test BlazorApp2.Tests` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests" --no-build -q`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 05-01-01 | 01 | 1 | SESS-01 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.StartSession"` | ❌ W0 | ⬜ pending |
| 05-01-02 | 01 | 1 | SESS-02 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.CompleteSet"` | ❌ W0 | ⬜ pending |
| 05-01-03 | 01 | 1 | SESS-03 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.EnduranceLog"` | ❌ W0 | ⬜ pending |
| 05-01-04 | 01 | 1 | SESS-04 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.PreviousPerformance"` | ❌ W0 | ⬜ pending |
| 05-01-05 | 01 | 1 | SESS-05 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.ExerciseStatus"` | ❌ W0 | ⬜ pending |
| 05-01-06 | 01 | 1 | SESS-07 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.Rpe"` | ✅ partial | ⬜ pending |
| 05-01-07 | 01 | 1 | SESS-08 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.Notes"` | ✅ partial | ⬜ pending |
| 05-01-08 | 01 | 1 | SESS-09 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.IncrementalSave"` | ❌ W0 | ⬜ pending |
| 05-01-09 | 01 | 1 | SESS-10 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SessionTests.Resume"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `BlazorApp2.Tests/SessionTests.cs` — stubs for SESS-01 through SESS-05, SESS-09, SESS-10
- [ ] `Data/Enums/ExerciseCompletionStatus.cs` — new enum (Complete, Partial, Skipped)
- [ ] `builder.Services.AddScoped<SessionService>()` registration in Program.cs

*Existing LogTests.cs provides entity-level persistence validation for WorkoutLog, SetLog, and EnduranceLog; SessionTests will focus on service-level operations.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Rest timer | SESS-06 | Deferred per D-14 — no rest timer in this phase | N/A — requirement deferred |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
