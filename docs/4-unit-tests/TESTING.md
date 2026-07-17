# Testing Guidelines

## Test Framework

- **Engine**: xUnit 2.9.3 on net10.0 (`engine.tests/`), with `Microsoft.NET.Test.Sdk` 17.14.1 and coverlet for coverage. 144 tests as of the charm expansion.
- **Unity**: Unity Test Framework (Unity 6000.5.3f1) — EditMode (32) + PlayMode (8) under `unity/SBR/Assets/Tests/`.
- **The economy**: the sim gate campaign (`sim/`) is a statistical regression suite, not a unit suite — G1–G6 gates plus per-item audit flags with Bonferroni-corrected CIs.

## Running Tests

```bash
# All engine tests
dotnet test engine.tests

# One class / one test
dotnet test engine.tests --filter "FullyQualifiedName~CharmExpansionTests"
dotnet test engine.tests --filter "FullyQualifiedName~RunTests.SomeTestName"

# Coverage
dotnet test engine.tests --collect:"XPlat Code Coverage"

# Economy regression (economy-touching changes only; exit 0 = gates + flags clean)
dotnet run --project sim -- --gates --runs 50000 --seed-prefix TUNE --report sim-report.md

# Unity: run EditMode/PlayMode through the Test Runner (editor or batch mode)
# Batch quirk: exit code 5 with a written results XML = teardown crash; the XML is authoritative.
```

## Test Organization

- `engine.tests/` — one file per domain area: `RunTests`, `RunSweatTests`, `SweatSessionTests`, `ItemTests`, `CharmExpansionTests`, `ShopAndGiftTests`, `PaymentAndTotemTests`, `OddsMathTests`, `SlateGeneratorTests`, `DramaGeneratorTests`, `RngTests`, `GoldenSeedTests`
- `unity/SBR/Assets/Tests/EditMode/` — model logic (odds formatting, betslip, bookie feed triggers)
- `unity/SBR/Assets/Tests/PlayMode/` — room wiring and screen flows

## Writing Tests

- Naming: `Behavior_Condition_Expectation`; worked-number pins show their arithmetic (e.g. the House Key 402.5/350 pin).
- **Pins are policy**: tests pin tuned values (multipliers, prices, offer counts, payment schedule) so a tuning change is also a test change — deliberate friction, keep it.
- **Determinism is tested directly**: golden-seed tests pin full-run outcomes; `Derive()` tests assert that a player acting vs not acting leaves the universe identical.
- Item tests cover the full behavior matrix: every hook the item implements, wind conditions, reset-on-sale, interaction with modifiers.
- Engine guard clauses get negative tests (illegal verb → throw, no partial state).

## Coverage Requirements

Not defined numerically. The working standard: new engine behavior ships with tests in the same change; uncovered risky paths go in `docs/4-unit-tests/COVERAGE-DEBT.md` (`path | why hard | escape plan`) per the TRIP-test seam ladder. Economy changes are additionally "covered" only by a green sim campaign.
