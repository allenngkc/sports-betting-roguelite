# Register entries — batch 192 (2026-08-25)

**Time-critical: the pending-window build is running NOW, and my own copy spec has a gap it would
fall into. The window names a leg from the SAME source batch 191 just ruled defective.**

**One row.** **Destination table:** TV (`T143-am5`).

**Amends `docs/design/spec-pending-window-copy-2026-08-25.md` §2/§5. Nothing measured.**

---

## The row

| T143-am5 | The pending window's leg name is built TO THE RULING, not to the build state — and for three kinds it inherits `T156` wholesale | **RULED — DD 2026-08-25 batch 192, amending this seat's own spec while the build that consumes it is in flight. **Two gaps, both mine, both cheap to close before they are compiled.*** **GAP 1 — THE SPEC ASSUMED A FIX THAT IS NOT BUILT. `spec-pending-window-copy` §2 gives `N LET AUDITORS +1.5 DIE` and says the club token comes through `SweatFlavor.Short` per `T168-am`. **`T168-am` IS RULED AND UNBUILT** — TV verified its absence at `b60d2bd` and it is still absent at HEAD. A lane naming the leg the obvious way, `LegStatement(leg)`, gets the `default:` arm → `SheetName` → `MarketSheet` → `fields.Line` → for a handicap `{hteam} ±1.5` with the **FULL** club name. **The build would render `N LET DULUTH AUDITORS +1.5 DIE` while the spec, and its measurement instruction, describe the short form.** This is `C58-am2`'s defect appearing in the document that CITES `C58-am2` — recorded plainly rather than quietly fixed.** **RULED: **NEW COPY IS BUILT TO THE RULING, NOT TO THE CURRENT BUILD STATE.** The pending window applies `Short` at its OWN call site. It does not wait for `T168-am`'s retrofit of existing sites, and it does not inherit `LegStatement`'s full-name behaviour by calling into it. **A new surface built to today's defect would have to be fixed twice**, and `T168-am` already ruled the render is where the club token is shortened.** **GAP 2, AND IT IS THE LARGER ONE — THE NAME'S SOURCE CARRIES `T156` INTO A SECOND SURFACE. The window must name the dying leg; for the three team totals that name comes from the same `fields.Line` batch 191 measured. **So a dying team total would be named inside the decline row by the very string ruled defective** — `N LET RENO FERRETS OVER DIE` once truncation lands, a sentence naming a direction and no market. **`Short` does NOT rescue this**: batch 185's 449.5 against 261.0 is already the short-club form.** **SO THE SCOPE IS SPLIT AND THE BUILD IS NOT BLOCKED: **for every kind with authored copy the one-leg form ships exactly as specced.** For `TeamTotalGoals`/`Corners`/`Cards` the name is defective at the source and no wording in this window can repair it — **the pending window inherits Allen's scope call rather than creating a second one.** Build the window; do not treat those three as a copy question inside it.** **AND §5's MEASUREMENT TARGET IS CORRECTED: measure THE STRING THE BUILD ACTUALLY PRODUCES. §5 asked for "the longest club short-form", which presumes `T168-am`; with `Short` applied at this call site per the ruling above that instruction becomes true, **but it must be true because the build does it, not because the spec assumed it** | batch 192 |

---

## For the orchestrator

- **Straight to TV, ahead of the rest of this batch:** apply `Short` at the pending window's own
  call site. Do not name the leg by calling `LegStatement` and taking what comes.
- **The build is NOT blocked.** Every kind with authored copy ships as specced; the three team
  totals are defective at the source and ride Allen's scope call.
- **This corrects my own spec**, two batches after I ruled the law it broke.
- **Backlog is 192.**

## Limits

- **Nothing measured.** The claim that `LegStatement` yields the full club name for a handicap is a
  code read (`MatchModel`'s handicap arm, `MarketSheet.NameOf`), not a rendered string.
- **I have not seen TV's build.** This is written from the spec it consumes and HEAD's source; if
  the lane already applies `Short`, this row costs nothing and confirms it.
- **The ≥3-dead-legs hole (batch 189 `S85-am2`) is untouched** and still owed.
