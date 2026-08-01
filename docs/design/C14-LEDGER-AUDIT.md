# C14 audit — LEDGER surface vs. the design system, 1:1

**Date:** 2026-08-01 · **HEAD:** `11fabaa` · **Standard:** C14 hardened — 1:1 is the bar, deviations only
where physically impossible, each DD-signed before build.

**Method.** Two read-only sub-agent sweeps on separate dimensions — structure/geometry/tokens, and
copy/voice/states/product-truth — against `main-2/docs/design/design-system/`. Findings measured from
pixels and read from source, not inferred. Captures: `20260801-190744-463-06-ledger-flat` (via tray)
and `20260801-190747-933-08-old-slips-flat` (via desktop icon).

**26 gaps: 9 fix-now, 3 needs-window, 14 needs-DD.**

---

## Correction to the audit's own premise

I dispatched this believing the kit did not specify a LEDGER screen, because there is no
`components/ledger/` directory. **That was wrong**, and the structure sweep pushed back with evidence
rather than accepting it:

- `ui_kits/surething/screens.jsx:132-146` — `LedgerScreen()`, with a 44px `--ground-2` header bar
- `ui_kits/surething/app.jsx:94-97` — a dedicated `PassiveMargin title="Record"` branch for LEDGER
- `components/records/LedgerEntry.jsx` — the per-ticket record component

I checked a directory name and stopped. The finding is therefore the inverse of what I expected and
considerably more serious: **this screen has drifted from a specification that exists**, and much of
that drift is structural rather than cosmetic.

## Both entry points are confirmed identical — not a gap

`06-ledger` and `08-old-slips` differ by 2,034 of 720,896 pixels (0.28%), in exactly two clusters: a
toast over the rail, and `BANK $315 → $290`. The capture sequence is `06 → 07-rewards → 08`, so a
Rewards visit and a purchase account for both. Board and margin content are byte-identical, and both
routes call the same `OldSlipsApp.Render()`. **S16's one-screen-two-routes holds.**

---

## fix-now (9)

Unambiguous, small, and decidable without an editor. **Sequencing matters: F3 must land before F1
and F2**, which cannot be done correctly until the strong rule token is reachable.

| # | Gap | Kit says | Build does | Where |
|---|---|---|---|---|
| **F1** | Five chrome-band boundary rules missing outright | `OsRail`/`SectionTabs`/`Masthead` each carry `border-bottom: … solid var(--rule)` | Flat colour steps, no rule at y=34, 72, 670 | `LaptopOs.cs` `BuildRail`/`BuildTray`; verified by column scan |
| **F2** | No vertical rule dividing sheet from margin | `screens.jsx:4` — `sheet.borderRight: "2px solid var(--rule)"`, applies to every screen | No divider at x=700 | all 18 `MakeRule` calls are horizontal |
| **F3** | `MakeRule()` can only ever draw `--rule-soft`; `--rule` is dead code | Two distinct tokens: `--rule #3C3C2C`, `--rule-soft #2C2C20` | Every rule in the runtime is `#2C2C20`; `LaptopOs.Rule` has zero references | `LaptopOs.cs:616-618` |
| **F4** | Tabs meta hardcoded | `app.jsx:121` — `tray === "LEDGER" ? "READ ONLY" : "SHEET 1 OF 1"` | Always `SHEET 1 OF 1` | `SportsbookApp.cs:1358` |
| **F5** | `LOST` word filled oxide | `LedgerEntry.jsx:32` — `color: won ? var(--wax) : var(--toner-3)`; only `textDecorationColor` is `--stamp` | Word glyphs are `MoneyBad` | `SportsbookApp.cs:1387,1403` |
| **F6** | Lost ticket's `$0` in brightest toner | `LedgerEntry.jsx:26` — `--toner-3` | `LaptopOs.White` (`--toner`) | `SportsbookApp.cs:1395` |
| **F7** | Ledger legs use raw engine `DisplayLabel` | `CompactLegLabel` exists precisely to stop the engine repeating the picked team | `1. DULUTH PLUMBERS ML — DULUTH PLUMBERS V TULSA LOOPHOLES` | `SportsbookApp.cs:1438` |
| **F8** | Rail/tray padding off-token | `space.css:20` — `--st-rail-pad-x: 11px` | 14/14 and 12/14 | `LaptopOs.cs:777,783,801,810` |
| **F9** | `TicketIdentity` 15px | `LedgerEntry.jsx:14` — `--st-size-leg` = 16px | 15px | `SportsbookApp.cs:1399` |

**F5 and F6 are my own S15 implementation, wrong.** The ruling read "LOST struck in oxide … the
returned figure in toner". I applied oxide to the word and `--toner` to the figure; the kit resolves
both more precisely — only the *strike* is oxide, the word and the figure are `--toner-3`. Kit is
spec-of-record, so it wins, but the wording difference is worth a DD glance in case the ruling meant
what I built.

**F7 is the sharpest of these**: the correct helper already exists and is already called twice in the
same file (`BuildSlip:451`, `BuildStagedReceipt:627`). The ledger just doesn't call it.

## needs-window (3)

| # | Gap | Why it needs an editor |
|---|---|---|
| **W1** | No overflow guard on the settled-ticket list — 3 tickets × 6 legs ≈ 576px against a 530px board, and the tray draws over any overrun | Missing guard is certain from source; whether real play triggers it needs a populated capture. Same defect class already fixed in Rewards |
| **W2** | `OsRail` identity mark, sticker and battery are collapsed into plain glyph text — no swatch, no bordered chip, no battery bar, and no low-battery state is representable | Kit is clear on intent; real elements at 34px need visual verification before sizes are locked |
| **W3** | `OsTray` missing the per-slot active/inactive dot and the badge pill | Same — small elements at small scale |
| **W4** | Possible `PENDING` leg inside an already-terminal ticket | Not traceable by static reading; needs a populated state |

## needs-DD (14)

Grouped, because several are one decision.

**The screen's structural shape — one decision, four gaps.** The kit specifies a persistent
4-tab strip on every surface (`SectionTabs.prompt.md`: *"present on every surface and never rebuilds"*),
a masthead carrying `RunFigure`s that *"must survive the 50% thumbnail check"*, a 44px `--ground-2`
board header, and a `LedgerEntry` row whose terminal word sits **rightmost**. The build has a single
fake `LEDGER` tab with no strip, no run figures at all, no header bar, and an information hierarchy
**inverted** — the dollar payout is the final scan point and `WON`/`LOST` is buried mid-row. Whether a
read-only historical screen should carry live run figures is a genuine product call; the inverted
hierarchy probably is not, and I'd expect that one to come back as "fix it".

**The margin — one decision, two gaps.** Kit: `PassiveMargin` = a biro-ruled `MarginHeader` (*"The
margin is his. Its header is biro-ruled."*) plus exactly **3** `MarginRow`s and one note. Build: a
toner header with a soft rule, no biro anywhere, and **7** content blocks. Also mixes the type voices
against `MarginRow`'s convention.

**Ruled-paper texture absent** — `margin.jsx:7-10` specifies a 26px `repeating-linear-gradient`. Not
physically impossible (the toner-grain tile proves the technique), but a real cost call.

**Voice and behaviour** — `SETTLED TICKETS EXPOSED BY RUN.TICKETS ONLY` reads as a leaked property
path (my lean: genuine defect, not machine-flavour); the cross-app toast bleeds onto a read-only
screen; `CASHED OUT` renders toner-2 where the kit pairs it with `WON` as wax — though the payout
figure legitimately *can't* go wax, because the engine stores no cash-out amount; leg rows carry no
per-outcome colour, and here the two kit sources disagree with each other.

**Restatement** — scope is restated 38px below the masthead, and the round number appears three times.
The code argues in its own comments that these are structurally distinct. Plausible, and exactly the
pattern the "say it once, well" ruling was meant to catch.

### One cross-cutting interaction neither sweep could see

**F4 and the restatement gap collide.** Setting the tabs meta to `READ ONLY` on this screen — correct
per `app.jsx:121` — makes the masthead's existing `READ ONLY` a *second* instance, reintroducing the
redundancy that S9 defect 7 removed. Fix F4 and drop the masthead's copy in the same change, or the
fix regresses a closed ruling.

---

## Swept and clean

Recorded so the sweep's coverage is legible, not just its hits.

**Structure.** Band heights 34/38/68/530/34 = 704, verified by source arithmetic *and* independent
pixel boundary detection. The 700/324 sheet-margin split, no gap or overlap. `--ground`/`--ground-2`/
`--ground-3` byte-identical to the palette, measured within 1-4/255 of nominal — consistent with the
documented 5% grain dither. `--st-pad-x` 14px and `--st-mast-pad-x` 16px correctly honoured. Existing
internal rules all at correct 1-2px weights. Rail and tray confirmed genuine shared implementation,
not a near-copy.

**Copy and truth.** S16 naming clean at every player-visible site. `READ ONLY` appears exactly once.
Strike sized from the word's own bounds, not a fixed sprite box. S9 defect 8 column-head ordering
confirmed fixed. Fact floor holds — every product-fact string ≥13px, the only 12px text being shared
OS chrome. No biro on this read-only surface beyond the S8-signed sticker. Oxide never on a price;
wax only for money. Voice carries no imperative or comfort phrasing.

**Product truth, verified against the engine rather than assumed:** cash-out non-retention, cross-run
non-persistence, open-ticket exclusion, `KNOWN WIN PAYOUTS` correctly excluding cashed-out tickets.

## Not verifiable from current evidence

Every capture shows the **empty** ledger. The populated-state findings (W1, W4, and the column maths
behind the hierarchy gap) are read from source and deterministic in UGUI, but no capture of a
populated ledger exists against this build. **A populated-state capture should precede any rebuild of
the record row** — the same blind spot that let a BUY-in-biro violation survive for weeks because no
capture ever showed an affordable offer.
