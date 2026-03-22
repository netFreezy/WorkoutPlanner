---
phase: 7
slug: quality-of-life
status: draft
nyquist_compliant: false
wave_0_complete: false
created: 2026-03-22
---

# Phase 7 — Validation Strategy

> Per-phase validation contract for feedback sampling during execution.

---

## Test Infrastructure

| Property | Value |
|----------|-------|
| **Framework** | xUnit 2.9.3 |
| **Config file** | `BlazorApp2.Tests/BlazorApp2.Tests.csproj` |
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~OverloadTests" --no-build -q` |
| **Full suite command** | `dotnet test BlazorApp2.Tests -q` |
| **Estimated runtime** | ~15 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~{TestClass}" -q`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests -q`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 07-01-01 | 01 | 0 | QOL-03 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~OverloadTests" -q` | ❌ W0 | ⬜ pending |
| 07-01-02 | 01 | 0 | QOL-04,QOL-05 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~ExportTests" -q` | ❌ W0 | ⬜ pending |
| 07-01-03 | 01 | 0 | QOL-06 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~HistoryTests" -q` | ❌ W0 | ⬜ pending |
| 07-01-04 | 01 | 0 | QOL-01,QOL-02 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~HomeTests" -q` | ❌ W0 | ⬜ pending |

*Status: ⬜ pending · ✅ green · ❌ red · ⚠️ flaky*

---

## Wave 0 Requirements

- [ ] `BlazorApp2.Tests/OverloadTests.cs` — stubs for QOL-03 (overload detection, increment mapping)
- [ ] `BlazorApp2.Tests/ExportTests.cs` — stubs for QOL-04, QOL-05 (CSV columns, PDF byte generation)
- [ ] `BlazorApp2.Tests/HistoryTests.cs` — stubs for QOL-06 (history queries, filtering)
- [ ] `BlazorApp2.Tests/HomeTests.cs` — stubs for QOL-01, QOL-02 (today's workout, last workout)
- [ ] NuGet packages: `dotnet add BlazorApp2 package QuestPDF --version 2026.2.4` and `dotnet add BlazorApp2 package CsvHelper --version 33.1.0`

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Home screen layout renders today's workout correctly | QOL-01 | Visual layout verification | Navigate to `/`, verify workout card shows exercise list, targets, "Start Session" button |
| Overload suggestion card displays inline | QOL-03 | UI component rendering | Start a session with qualifying exercises, verify suggestion card appears with Apply/Dismiss |
| PDF renders with correct formatting | QOL-05 | Visual document layout | Export PDF, open file, verify headers, tables, page numbers, period overview section |
| History card expand/collapse interaction | QOL-06 | Interactive UI behavior | Navigate to `/history`, click a card, verify detail expands; click again, verify collapse |

---

## Validation Sign-Off

- [ ] All tasks have `<automated>` verify or Wave 0 dependencies
- [ ] Sampling continuity: no 3 consecutive tasks without automated verify
- [ ] Wave 0 covers all MISSING references
- [ ] No watch-mode flags
- [ ] Feedback latency < 15s
- [ ] `nyquist_compliant: true` set in frontmatter

**Approval:** pending
