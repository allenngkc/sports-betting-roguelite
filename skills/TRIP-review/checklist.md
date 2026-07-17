# Code Review Checklist

This file is the **single source of truth** for code-review criteria. Both human-driven reviews via `.claude/skills/TRIP-review` and Codex-driven reviews via `.claude/skills/codex-code-review` apply the criteria below — referenced, not copied — so the two review surfaces cannot drift.

## Systematic Review Checklist

### 1. Functional Requirements

- [ ] Implementation logic matches requirements correctly
- [ ] Interface/API matches documented specifications
- [ ] Error scenarios handled with proper feedback
- [ ] Edge cases and boundary conditions validated

### 2. Code Quality

- [ ] Proper typing (no unjustified dynamic types)
- [ ] DRY principle - no code duplication
- [ ] KISS principle - not unnecessarily complex
- [ ] Consistent, descriptive naming conventions
- [ ] Complex logic has explanatory comments
- [ ] Files/modules not excessively large
- [ ] Imports/includes organized, unused ones removed

### 3. Architectural Compliance

- [ ] Code follows established patterns from ARCHI.md
- [ ] Proper separation of concerns
- [ ] Appropriate abstractions used
- [ ] Consistent with existing codebase style

### 4. Determinism & RNG Discipline

- [ ] All new randomness flows through `RngHub` named streams or `Derive()` (no `new Random()`, no shared-stream theft)
- [ ] Player-timed actions use `Derive()` so timing never perturbs the fixed universe
- [ ] Draw order unchanged, OR golden-seed pin changes are explicit and justified in the plan/CR
- [ ] Presentation code consumes no engine RNG (outcomes stay baked at lock)
- [ ] Engine stays headless: no Unity types in `engine/`, netstandard2.1-compatible APIs only

### 5. Economy & Effect Pipeline Compliance

- [ ] Payout multipliers go through `Ticket.SetFactor`/`RemoveFactor` (the one product slot), never ad-hoc multiplication
- [ ] One modifier per ticket (FreeBet xor DoubleOrNothing) respected
- [ ] Effect hooks used in the fixed order; `OnLegResolved` fires exactly once per leg
- [ ] Locked contracts never mutate: base is stored, effective is computed (getters/factors)
- [ ] Stateful passives reset on sale; comps arithmetic stays integer deci-comps
- [ ] Tuning changes re-ran the sim gates and synced the moved test pins; holdout freeze/burn protocol honored

### 6. Error Handling

- [ ] Errors are properly caught and handled
- [ ] Error messages are clear and actionable
- [ ] Failure modes are graceful
- [ ] Engine guard clauses reject illegal verbs atomically (no partial state on rejection)

### 7. Security (if applicable)

- [ ] Input validation implemented
- [ ] No sensitive data exposed
- [ ] Authentication/authorization respected
- [ ] No obvious vulnerabilities

### 8. Performance

- [ ] No obvious performance issues
- [ ] Resource cleanup implemented (no leaks)
- [ ] Appropriate data structures used
- [ ] No unnecessary operations in hot paths

---

## Issue Severity Classification

**Critical (Block Deployment)**:

- Security vulnerabilities
- Data corruption risks
- Breaking API/interface changes
- Authentication bypasses

**Major (Require Immediate Fix)**:

- Incorrect business logic
- Significant performance degradation
- Missing error handling
- Compilation/build errors

**Minor (Should Fix)**:

- Code style inconsistencies
- Missing documentation
- Code duplication
- Missing edge case handling

**Suggestions (Nice to Have)**:

- Performance optimizations
- Readability improvements
- Additional test coverage

---

## Review Completion Criteria (Approval Gate)

Minimum for approval:

- [ ] All functional requirements implemented
- [ ] No critical or major issues remaining
- [ ] `dotnet build SBR.slnx` successful
- [ ] Affected `dotnet test engine.tests` tests pass (per the TRIP-2 testing gate); Unity-touching changes: EditMode/PlayMode green (batch exit 5 with XML written = teardown crash, XML authoritative)
- [ ] Economy-touching changes: `dotnet run --project sim -- --gates` exits 0 (gates + item flags clean)
- [ ] New logic has test coverage (or a coverage-debt ledger entry per the hard-to-cover policy)
- [ ] Documentation updated per project standards (ARCHI.md if architecture moved; DECISIONS.md for decisions of record)
