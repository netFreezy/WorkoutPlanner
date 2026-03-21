---
phase: 3
slug: workout-templates
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-21
---

# Phase 3 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.x (existing from Phase 1) |
| **Config file** | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "Category=Templates"` |
| **Full suite command** | `dotnet test BlazorApp2.Tests` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "Category=Templates"`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| TBD | TBD | TBD | TMPL-01 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |
| TBD | TBD | TBD | TMPL-02 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |
| TBD | TBD | TBD | TMPL-03 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |
| TBD | TBD | TBD | TMPL-04 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |
| TBD | TBD | TBD | TMPL-05 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |
| TBD | TBD | TBD | TMPL-06 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |
| TBD | TBD | TBD | TMPL-07 | integration | `dotnet test --filter "Category=Templates"` | TBD | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- Existing test infrastructure covers Phase 3 requirements (xUnit + DataTestBase from Phase 1)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Drag-and-drop reorder | TMPL-02 | Browser interaction (SortableJS) | Drag exercise row up/down, verify new order persists after save |
| Superset visual bracket | TMPL-05 | Visual rendering | Select 2+ exercises, group as superset, verify bracket connector visible |
| EMOM visual bracket | TMPL-06 | Visual rendering | Create EMOM block, verify rounds/minute display on group header |
| Section headers | TMPL-07 | Visual rendering | Add warm-up section, verify header appears and exercises are visually separated |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
