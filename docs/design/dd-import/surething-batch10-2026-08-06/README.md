# Batch 10 — built. Five frames, one answer, one audit

**SureThing UI lead · 2026-08-06 · HEAD `bdbd82b`**
**Suites: EditMode 76/76, PlayMode 56/56** — every invocation through
`tools/run-unity-tests.ps1`, executed counts reported (C29).

## C29 first, studio-wide, before any verdict run

`tools/run-unity-tests.ps1` wraps every Unity suite invocation. It always prints the executed case
count, and exits non-zero on zero cases, on failures, and on a missing results file.

**Proven against the defect the law names rather than asserted:**

```
./tools/run-unity-tests.ps1 -Platform EditMode -TestFilter 48151623
[EditMode] executed 0 of 0 discovered · passed 0 · failed 0 · Passed
C29 VIOLATION: this run executed ZERO test cases and Unity reported it green.
exit 3
```

Unity's own verdict on that run was `testcasecount=0, result=Passed`. It also earned its keep twice
more during this batch: once on a Unity boot crash that wrote no results, and once on a filtered
diagnostic run.

## S53-am + S55 + S57 — `13-verdict-run-won`, `14-verdict-run-lost`

One re-shoot, three items, because they are one frame.

- **Ground is `--ground`.** Measured on these frames: **21.5, 21.5, 12.7** against the token's
  22, 22, 15, and against the desktop's own 22, 22, 13 — the verdict shares the surface's ground
  rather than having one. Green is no longer zero anywhere; it was zero on 2449 of 2449 samples.
- **The machine stays.** Rail and tray, `Running.Sportsbook`, the same call `BuildTaskbar` makes.
  The work area is the remainder between them and the content dropped into it with its own anchors
  unchanged. The player can still reach the ledger or the desktop.
- **S57 — the answer is capture data.** The verdict *does* derive from the bank, but from
  bank-versus-payment, and **the engine does not deduct a payment the bank cannot meet**, so a
  forced loss kept its whole $350 while the forced win had already paid $60 of its own. Nothing is
  wrong with the product. The figures were arbitrary because both states are forced rather than
  played — so they are now chosen to read: the win ends holding **$290**, the bust holds **$40**
  against a **$155** payment, a real figure from the shipped schedule.

## S56 — `11-desktop`

The chip is **removed for both states**, not firmed up. A firmer invisible thing is still invisible,
and keeping it leaves an element that draws and cannot be seen. `NOT INSTALLED` prints under the
caption of an app that does not launch — the machine's own register, the same terse system fact as
the tray's `DISK 61% FULL`, and it states what is **true** rather than what is planned, which is the
whole difference from the `(soon)` S47 deleted.

Measured, because the complaint was a measurement:

| | step above ground |
|---|---|
| `NOT INSTALLED`, peak 107.1 on ground 22.2 | **85** |
| the chip it replaces | **3** |

Glyph tone remains the other channel at 215 vs 108. Two channels, both legible.

Icon pitch went 105 → 126 to make room; the column start (S52) is untouched and the last state line
closes at y 549 against the tray's 670.

## S58 — `05-my-bets-green-dead`

The tally states the run now: `TICKETS THIS ROUND`, `AT RISK · N RIDING`, `IF EVERYTHING LANDS`.
None of those are on the sheet, because the sheet is per-ticket and these are sums across it. Derived
only from the TV-owned mirror — the header promises READ ONLY.

`AT RISK` counts only tickets still riding; a dead ticket's stake is gone, not at risk. **The riding
count sits in the label so that `$0` explains itself.**

**Capture caveat:** the only MY BETS state in the set is a fully-dead ticket, so the row is
photographed reading `1 / $0 / $0`. Correct, but it does not show the row doing its job. A
riding-state capture would; it does not exist and is not built here.

## New: `15-ledger-across-rounds` — the retention capture

The one claim in the granted set that was proven by construction and by nothing photographic. The
masthead reads **ROUND 2 OF 8** and the board carries **TICKET 1.0 beside TICKET 2.0** — a ticket
from a finished round, on screen, in the next one. Every previous ledger frame was round 1, and a
single-round ledger looks identical whether the screen reads retained history or the current round.

The assertion is the point: **the board renders more rows than `run.Tickets` holds.** Only possible
if it reads retention, and it fails the moment anyone points it back.

Only the bank is rigged (5000), not the schedule — an earlier cut used `{1,1,1,…}` and printed
`TARGET $1`, which is visibly a test rig and exactly what S57 rules against. The masthead reads
`TARGET $70` in round 2. The bank does read high at $2,698; that is the honest consequence of a
survivable rig and the alternative is a run that ends before it can prove anything.

## S54 — the audit

Full text at `docs/design/S54-COLOR-AUDIT.md` in the surething-ui tree. 27 instances; none can
silently change a rendered colour, and **two cannot be answered from source at all**:
`LaptopScreen.idleEmission` and `attentionEmission` are serialized fields, so the scene ships and
the source default is a fallback. Checked rather than assumed — the scene agrees for the laptop, and
the other `idleEmission` in `Room.unity` belongs to `PhoneScreen`, resolved by script guid.

Two things worth your eye: **`LaptopUi.FromRgb` is dead** (no call sites; the live `FromRgb` calls
are `TheaterStage`'s) and recommended for deletion; and **`attentionEmission` is (0.28, 0.10, 0.55)**,
a saturated violet on the laptop lid, in a project that retired purple. Flagged, not touched — it is
room lighting rather than the document.

**What the audit cannot see (C25):** it reads source and scene, not frames. The emissions are HDR
through a `MaterialPropertyBlock`, a different path to the screen than every other colour here, and
**no capture in the set shows the lid glow at all.** Closing that needs a room-camera state with the
glow active, which does not exist.
