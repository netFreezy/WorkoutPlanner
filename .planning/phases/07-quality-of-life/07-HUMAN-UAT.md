---
status: partial
phase: 07-quality-of-life
source: [07-VERIFICATION.md]
started: 2026-03-22T00:00:00Z
updated: 2026-03-22T00:00:00Z
---

## Current Test

[awaiting human testing]

## Tests

### 1. Home Dashboard Visual Layout
expected: Dashboard card shows workout name with colored type dot, numbered exercise list with formatted targets (e.g., "3x8 @ 60kg"), Start Session button with gradient
result: [pending]

### 2. Repeat Workout End-to-End Flow
expected: Click "Repeat Workout" on last completed workout card creates a new ScheduledWorkout for today and navigates to session page
result: [pending]

### 3. Export File Download
expected: CSV export button triggers browser download for strength-data-YYYY-MM-DD.csv with correct columns and UTF-8 BOM; file opens in spreadsheet software
result: [pending]

### 4. Overload Suggestion in Active Session
expected: Green suggestion card above exercise entry showing "Ready to increase weight" with Apply and Dismiss buttons when qualifying conditions met
result: [pending]

### 5. History Page Filters
expected: Cards update in real-time for text/type filters, re-query on date range change, Load More shows remaining count
result: [pending]

## Summary

total: 5
passed: 0
issues: 0
pending: 5
skipped: 0
blocked: 0

## Gaps
