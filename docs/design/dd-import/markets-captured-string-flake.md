# Markets → studio · captured strings are not a production formula (a new flake signature)

**From:** markets/sim lead (`markets-2`) · **2026-08-05** · **Not blocking B1**
**Observed at:** `cf64199`, PlayMode, 1 failure in 3 consecutive full runs (46/46 → 45/46 → 46/46).

`SureThingEntryTests.Working_margin_renders_every_staged_receipt_and_lock_tracks_only_current_marks`
fails intermittently on a **one-character difference in where a leg label's ellipsis lands** —
expected 44 characters, measured 45, differing at index 37
(`"1. OVERHEADS MONEYLINE — v GRAVEDIGGE…  -301"`). The product is correct in both outcomes: the
label fits its column either way and the protected price suffix survives, which is exactly what
`FitLabelKeepingSuffix` guarantees. What varies is *which* character the ellipsis replaces.

**Mechanism.** The label is fitted by measuring glyph advances at runtime — `LaptopUi.MeasureWidth`
requests glyphs into a dynamic font atlas, and Unity's dynamic font metrics are not guaranteed
identical between a cold atlas and a warm one. A label sitting within a glyph-width of its fit
boundary can therefore land either side of it depending on what rendered before it in the same
session. The earlier type-conformance pass took this label from 13px to 16px (`--st-size-leg`,
S28-load-bearing), which tightened the fit and plausibly moved it onto such a boundary. The 16px is
correct and is not the defect.

**The finding is about the instrument, not the label.** The test asserts against a **string captured
earlier in the same run** and re-compared later, so it is not measuring "does the margin render the
right leg?" — it is measuring "did two separate measurements of the same text agree?", which is a
question about font-atlas state. `CompactLegLabel` is `internal` precisely so a fixture can assert
against the *production formula* rather than a hand-kept or captured duplicate — its own doc comment
says so — and the persistence snapshot is the one place that still compares a captured literal. That
is the fix: assert the rendered node equals `SportsbookApp.CompactLegLabel(matchup, selection)`
evaluated at assert time, so both sides are subject to the same atlas state and the comparison tests
the thing it claims to.

**Recorded as a signature**, alongside the documented cash-out flake, so the next person to see a
red suite on this test recognises it in seconds instead of diagnosing it fresh: *single test, single
character, index near the ellipsis, passes on re-run.* Under C18/C25 an undocumented intermittent is
a gate that does not state what it covers.

**Scope of this report:** three full PlayMode runs on one machine, one graphics device, warm Library.
I have not established the failure rate, whether it depends on test execution order, or whether other
fitted labels (the staged receipt header, the ledger rows) share the boundary — all three are
plausible and none are measured.
