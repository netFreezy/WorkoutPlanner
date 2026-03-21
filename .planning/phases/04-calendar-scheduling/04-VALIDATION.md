---
phase: 4
slug: calendar-scheduling
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-21
---

# Phase 4 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 |
| **Config file** | BlazorApp2.Tests/BlazorApp2.Tests.csproj |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Schedule OR FullyQualifiedName~Materialization" -v q` |
| **Full suite command** | `dotnet test BlazorApp2.Tests -v q` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Schedule OR FullyQualifiedName~Materialization" -v q`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests -v q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 04-01-01 | 01 | 1 | SCHED-04 | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~MaterializationTests" -v q` | Wave 0 | ⬜ pending |
| 04-01-02 | 01 | 1 | SCHED-06 | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~MaterializationTests" -v q` | Wave 0 | ⬜ pending |
| 04-01-03 | 01 | 1 | SCHED-03 | unit (data) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ScheduleTests" -v q` | Partial | ⬜ pending |
| 04-02-01 | 02 | 2 | SCHED-01 | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~SchedulingServiceTests" -v q` | Wave 0 | ⬜ pending |
| 04-02-02 | 02 | 2 | SCHED-02 | unit (service) | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~MonthlyAggregation" -v q` | Wave 0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `BlazorApp2.Tests/MaterializationTests.cs` — stubs for SCHED-04, SCHED-06 (date generation, deduplication, 4-week window)
- [ ] `BlazorApp2.Tests/ScheduleTests.cs` — extend for ad-hoc workouts (SCHED-03 ad-hoc path)
- [ ] `BlazorApp2.Tests/SchedulingServiceTests.cs` — stubs for SCHED-01 date range queries, SCHED-02 monthly aggregation

*Existing infrastructure covers test framework — no new install needed.*

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Weekly calendar visual type indicators | SCHED-01 | CSS color/styling verification | Load weekly view, verify strength=blue, endurance=green, mixed=gradient chips |
| Monthly color-coded dots | SCHED-02 | Visual rendering verification | Load monthly view, verify dot colors match workout types |
| Conflict warning on consecutive muscle groups | SCHED-05 (partial) | UI notification rendering | Schedule same muscle group on consecutive days, verify warning toast appears |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
