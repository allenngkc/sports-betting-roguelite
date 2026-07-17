---
name: TRIP-test
description: Write/run tests following project standards (deep test authoring)
disable-model-invocation: true
argument-hint: "component or feature to test"
---

# Testing Mode

You are now in **testing mode** for **Sports Betting Roguelite (SBR)**.

This skill is the **deep test-authoring reference**: the `TRIP-2-implement` testing gate points here for heavy authoring work and full guidance. Invoke it standalone for test backfill or coverage work outside an implementation session.

## Prerequisites - Read First

Before testing, you MUST read:

1. @docs/ARCHI.md - Understand system architecture
2. @docs/4-unit-tests/TESTING.md - Testing guidelines

## Your Task

Test: $ARGUMENTS

---

## Testing Guidelines

### Scope

- Only run tests for relevant files that changed (not the whole project)
- Focus on the new feature/fix/refactor

### Commands

```bash
# Run all engine tests (xUnit, 144 as of the charm expansion)
dotnet test engine.tests

# Run a specific test class or method
dotnet test engine.tests --filter "FullyQualifiedName~CharmExpansionTests"
dotnet test engine.tests --filter "FullyQualifiedName~RunTests.PlaceTicket_RejectsOverStake"

# With coverage (coverlet)
dotnet test engine.tests --collect:"XPlat Code Coverage"

# Unity tests: EditMode + PlayMode via the Unity Test Runner (editor or batch mode).
# Batch quirk: exit code 5 with the results XML written = teardown crash; the XML is authoritative.

# The economy's statistical regression suite (economy-touching changes only):
dotnet run --project sim -- --gates --runs 50000 --seed-prefix TUNE --report sim-report.md
```

### Test Structure

- `engine.tests/*.cs` — xUnit, one file per domain area: `RunTests`, `RunSweatTests`, `SweatSessionTests`, `ItemTests`, `CharmExpansionTests`, `ShopAndGiftTests`, `PaymentAndTotemTests`, `OddsMathTests`, `SlateGeneratorTests`, `DramaGeneratorTests`, `RngTests`, `GoldenSeedTests`
- `unity/SBR/Assets/Tests/EditMode/` and `PlayMode/` — Unity Test Framework asmdefs (`SBR.Tests.EditMode`/`SBR.Tests.PlayMode`); EditMode covers models (odds format, betslip, bookie feed), PlayMode covers room wiring
- Naming: `MethodOrBehavior_Condition_Expectation` style; worked-number pins carry the arithmetic in the test name or a comment

### Testing Priorities

**Unit Tests (engine)**:

- Per-item behavior matrix: every hook the item implements, wind/reset semantics, worked-number payout pins
- Run state machine legality: verbs rejected outside their phase, atomic validation (no partial state)
- Odds/EV/cash-out math against hand-computed anchors
- Determinism: golden-seed full-run pins; `Derive()` stream independence (acting vs not acting leaves the universe identical)
- Catalog invariants (counts, prices, one-modifier law, factor-map exclusivity)

**Integration Tests**:

- Unity PlayMode room wiring (screens react to engine state)
- Console client smoke (a scripted run end to end)

**Statistical (the sim)**:

- Economy changes are regression-tested by the G1–G6 gate campaign plus per-item audit flags — a balance change without a green `--gates` run is untested by definition

**What to Test**:

- Tuned values via pins — a tuning change is a test change, deliberately
- Boundary states: bank exactly $0, the final round (no totem), empty consumable slots, single-leg tickets in the pending window
- Reset semantics: sell/rebuy never resumes ratchet state
- Draw-order stability: any new RNG consumption acknowledged in golden-seed pins

---

## Hard-to-Test Code

Seam ladder, cheapest first: **exported pure helper → injectable client/adapter → module mock → integration/emulator test**. Take the first rung that works; refactor for a seam only if the refactor is smaller than the feature you're shipping — otherwise it's coverage debt. Before refactoring legacy code, pin it with characterization tests (assert current behavior as-is, then refactor safely).

Uncovered risky paths: one line each in `docs/4-unit-tests/COVERAGE-DEBT.md` (`path | why hard | escape plan`). Delete a ledger line in the same change that gives its path meaningful coverage.

---

## Post-Testing Summary

After completing tests, create a summary file:

**File**: `docs/4-unit-tests/wa_vx.y.z_test.md`
(a = project week, x.y.z = version)

**Content**:

```markdown
# Test Summary - Week a, V. x.y.z

## What Was Tested

[List of tested components/functions]

## Test Results

- Total tests: X
- Passed: X
- Failed: X
- Coverage: X%

## Key Findings

[Any issues discovered, edge cases found, etc.]

## Notes

[Additional context or recommendations]
```
