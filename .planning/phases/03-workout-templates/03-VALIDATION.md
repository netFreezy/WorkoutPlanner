---
phase: 3
slug: workout-templates
status: draft
nyquist_compliant: true
wave_0_complete: true
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
| **Quick run command** | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Template"` |
| **Full suite command** | `dotnet test BlazorApp2.Tests` |
| **Estimated runtime** | ~10 seconds |

---

## Sampling Rate

- **After every task commit:** Run `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~Template"`
- **After every plan wave:** Run `dotnet test BlazorApp2.Tests`
- **Before `/gsd:verify-work`:** Full suite must be green
- **Max feedback latency:** 15 seconds

---

## Per-Task Verification Map

| Task ID | Plan | Wave | Requirement | Test Type | Automated Command | File Exists | Status |
|---------|------|------|-------------|-----------|-------------------|-------------|--------|
| 03-01-T1 | 01 | 1 | TMPL-01, TMPL-03, TMPL-04, TMPL-05, TMPL-06, TMPL-07 | build | `dotnet build BlazorApp2.csproj` | Models/TemplateBuilderState.cs | pending |
| 03-01-T2 | 01 | 1 | TMPL-01, TMPL-03, TMPL-04, TMPL-05, TMPL-06, TMPL-07 | unit | `dotnet test BlazorApp2.Tests --filter "FullyQualifiedName~TemplateTag\|FullyQualifiedName~TemplateDuplicate\|FullyQualifiedName~TemplateDuration"` | BlazorApp2.Tests/TemplateTagTests.cs, BlazorApp2.Tests/TemplateDuplicateTests.cs, BlazorApp2.Tests/TemplateDurationTests.cs | pending |
| 03-02-T1 | 02 | 2 | TMPL-01, TMPL-05, TMPL-06, TMPL-07 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Pages/Templates.razor | pending |
| 03-02-T2 | 02 | 2 | TMPL-01, TMPL-05, TMPL-06, TMPL-07 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Shared/TemplateDetailDialog.razor | pending |
| 03-03-T1 | 03 | 2 | TMPL-01, TMPL-02, TMPL-03, TMPL-04 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Pages/TemplateBuilder.razor | pending |
| 03-03-T2 | 03 | 2 | TMPL-03, TMPL-04 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Shared/ExerciseRow.razor | pending |
| 03-04-T1 | 04 | 3 | TMPL-05, TMPL-06, TMPL-07 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Shared/SectionHeader.razor | pending |
| 03-04-T2 | 04 | 3 | TMPL-05, TMPL-06, TMPL-07 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Pages/TemplateBuilder.razor | pending |
| 03-05-T1 | 05 | 4 | TMPL-02 | build | `dotnet build BlazorApp2.csproj` | wwwroot/js/template-builder.js | pending |
| 03-05-T2 | 05 | 4 | TMPL-02 | build+test | `dotnet build BlazorApp2.csproj && dotnet test BlazorApp2.Tests` | Components/Pages/TemplateBuilder.razor.cs | pending |

*Status: pending -- ✅ green -- ❌ red -- ⚠️ flaky*

---

## Wave 0 Requirements

- Existing test infrastructure covers Phase 3 requirements (xUnit + DataTestBase from Phase 1)
- Plan 01 Task 2 creates all Wave 1 unit tests (TemplateTagTests, TemplateDuplicateTests, TemplateDurationTests)

---

## Manual-Only Verifications

| Behavior | Requirement | Why Manual | Test Instructions |
|----------|-------------|------------|-------------------|
| Drag-and-drop reorder | TMPL-02 | Browser interaction (SortableJS) | Drag exercise row up/down, verify new order persists after save |
| Cross-section drag | TMPL-02 (D-20) | Browser interaction (SortableJS + section boundaries) | Drag exercise from Working to Warm-Up, verify SectionType updates and section header appears/disappears |
| Inline target editing (strength) | TMPL-03 | UI input fields | Add strength exercise, set sets/reps/weight inline, save, reopen, verify values persist |
| Inline target editing (endurance) | TMPL-04 | UI input fields | Add endurance exercise, set distance/duration/pace/HR zone inline, save, reopen, verify values persist |
| Superset visual bracket | TMPL-05 | Visual rendering | Select 2+ exercises, group as superset, verify bracket connector visible |
| EMOM visual bracket | TMPL-06 | Visual rendering | Create EMOM block, verify rounds/minute display on group header |
| Section headers | TMPL-07 | Visual rendering | Add warm-up section, verify header appears and exercises are visually separated |

---

## Validation Sign-Off

- [x] All tasks have `<automated>` verify or Wave 0 dependencies
- [x] Sampling continuity: no 3 consecutive tasks without automated verify
- [x] Wave 0 covers all MISSING references
- [x] No watch-mode flags
- [x] Feedback latency < 15s
- [x] `nyquist_compliant: true` set in frontmatter

**Approval:** pending execution
