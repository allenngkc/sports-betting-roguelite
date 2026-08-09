# Markets → studio · captured strings are not a production formula (a new flake signature)

> ## ⚠ CORRECTED 2026-08-06 — the diagnosis below is WRONG, and the real cause is duller
>
> **It was never font-atlas state. It was a stale hardcoded width.** The fixture fitted the
> staged-receipt label to `receiptTextWidth = 280f` — the width the receipt had while it lived
> in the 324px working margin — and **E-07 moved receipts to the 700px sheet without that
> constant following.** The test therefore fitted to a narrower box than the render used and
> expected an ellipsis the surface had not drawn.
>
> That explains the intermittency exactly, and better than atlas warmth did: whether the two
> widths produce a *different string* depends on the label's length for that seed, so short
> labels fitted both boxes and passed while long ones failed. Nothing was warming up; the
> instrument was measuring a box that no longer existed.
>
> Fixed by measuring the width from the rendered header instead of naming it
> (`SureThingEntryTests`), so it cannot go stale again. PlayMode 47/47.
>
> **What I got right and what I got wrong.** The prescription — *assert against the production
> formula, not a captured literal* — was correct and is what fixed it. The mechanism I gave was
> invented from a plausible story about dynamic fonts that I never measured, in the same session
> I filed a note about not doing exactly that. The fixture's own comment already promised "not a
> duplicated literal" for the formula; the width beside it was the literal nobody checked.
>
> The signature line below still holds as a *recognition* aid — single test, difference at the
> ellipsis, passes on re-run — but the cause it names is retracted.

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
