---
phase: 2
slug: exercise-library
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-21
---

# Phase 2 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.x with EF Core in-memory SQLite |
| **Config file** | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "Category=Exercise"` |
| **Full suite command** | `dotnet test BlazorApp2.Tests` |
| **Estimated runtime** | ~5 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "Category=Exercise"`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 5 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| TBD | 01 | 1 | EXER-03 | integration | `dotnet test --filter "Category=Exercise"` | ❌ W0 | ⬜ pending |
| TBD | 01 | 1 | EXER-01 | integration | `dotnet test --filter "Category=Exercise"` | ❌ W0 | ⬜ pending |
| TBD | 01 | 1 | EXER-02 | integration | `dotnet test --filter "Category=Exercise"` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `BlazorApp2.Tests/ExerciseLibraryTests.cs` — stubs for EXER-01, EXER-02, EXER-03
- [ ] Existing `DataTestBase.cs` covers shared fixtures

*Existing test infrastructure from Phase 1 covers framework setup.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Card grid renders correctly | EXER-01 | Visual layout | Browse /exercises, verify card grid with type tags |
| Dialog opens on card click | EXER-01 | UI interaction | Click any exercise card, verify detail dialog appears |
| Instant search filtering | EXER-01 | UI responsiveness | Type in search bar, verify cards filter in real-time |
| FAB triggers create dialog | EXER-02 | UI interaction | Click FAB, verify create dialog opens with type toggle |
| Scroll to new exercise | EXER-02 | UI behavior | Create exercise, verify catalog scrolls to it |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 5s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
