---
phase: 1
slug: data-foundation
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-21
---

# Phase 1 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xunit 2.9.3 |
| **Config file** | None — Wave 0 will create test project |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "Category=Data" --no-build` |
| **Full suite command** | `dotnet test BlazorApp2.Tests` |
| **Estimated runtime** | ~5 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --no-build -v quiet`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 10 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 01-01-01 | 01 | 1 | DATA-01 | integration | `dotnet test --filter "FullyQualifiedName~DbContextFactoryTests"` | ❌ W0 | ⬜ pending |
| 01-01-02 | 01 | 1 | DATA-02 | integration | `dotnet test --filter "FullyQualifiedName~ExerciseHierarchyTests"` | ❌ W0 | ⬜ pending |
| 01-01-03 | 01 | 1 | DATA-03 | integration | `dotnet test --filter "FullyQualifiedName~TemplateTests"` | ❌ W0 | ⬜ pending |
| 01-01-04 | 01 | 1 | DATA-04 | integration | `dotnet test --filter "FullyQualifiedName~TemplateTests"` | ❌ W0 | ⬜ pending |
| 01-01-05 | 01 | 1 | DATA-05 | integration | `dotnet test --filter "FullyQualifiedName~TemplateTests"` | ❌ W0 | ⬜ pending |
| 01-01-06 | 01 | 1 | DATA-06 | integration | `dotnet test --filter "FullyQualifiedName~ScheduleTests"` | ❌ W0 | ⬜ pending |
| 01-01-07 | 01 | 1 | DATA-07 | integration | `dotnet test --filter "FullyQualifiedName~ScheduleTests"` | ❌ W0 | ⬜ pending |
| 01-01-08 | 01 | 1 | DATA-08 | integration | `dotnet test --filter "FullyQualifiedName~LogTests"` | ❌ W0 | ⬜ pending |
| 01-01-09 | 01 | 1 | DATA-09 | integration | `dotnet test --filter "FullyQualifiedName~LogTests"` | ❌ W0 | ⬜ pending |
| 01-01-10 | 01 | 1 | DATA-10 | integration | `dotnet test --filter "FullyQualifiedName~LogTests"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `BlazorApp2.Tests/BlazorApp2.Tests.csproj` — xunit test project with EF Core SQLite references
- [ ] `BlazorApp2.Tests/DataTestBase.cs` — shared SQLite in-memory test fixture
- [ ] `BlazorApp2.Tests/DbContextFactoryTests.cs` — covers DATA-01
- [ ] `BlazorApp2.Tests/ExerciseHierarchyTests.cs` — covers DATA-02
- [ ] `BlazorApp2.Tests/TemplateTests.cs` — covers DATA-03, DATA-04, DATA-05
- [ ] `BlazorApp2.Tests/ScheduleTests.cs` — covers DATA-06, DATA-07
- [ ] `BlazorApp2.Tests/LogTests.cs` — covers DATA-08, DATA-09, DATA-10
- [ ] Solution file update: `dotnet sln add BlazorApp2.Tests`

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| WAL mode active on SQLite | DATA-01 | Requires runtime inspection | Run app, check `PRAGMA journal_mode;` returns `wal` |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 10s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
