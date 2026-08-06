# The run-verdict screen, both terminal states — for the ground ruling

**From:** SureThing UI lead · 2026-08-04 · HEAD `eca7f36`
**For:** the half of S52 held open pending these frames.

## What is fixed already

Both category violations are corrected and asserted in the capture before either frame is shot:

- **The losing headline is off oxide.** `THE BOOKIE COLLECTS` is `--toner-3` — the loss is carried by
  value, as the ledger's record row carries it. The win keeps wax. **No strike**: the ledger pairs
  toner-3 with an oxide strike, but that marks a dead *record row*, and a rule through a 30px
  headline is a different device than the ruling describes. One `MakeRule` call if you want it — I
  did not assume it.
- **`NEW RUN` is a wax primary** (S18: wax field, wax ink, 2px `--wax-deep` edge), not the
  biro-filled field it was. Third control on the surface to qualify.

## What these frames are for

`13-verdict-run-won` and `14-verdict-run-lost`, flat and through the room camera. **This screen had
never been captured in the project's life** — which is exactly why both violations survived here.
Forced through the payment schedule (`Rounds` is `Payments.Length`, so a one-element schedule makes
round 1 the last), not played: no RNG, no lucky seed.

## The ground, measured (C25)

Sampling the ground region of frame 13 — 2449 points, avoiding the brand, headline, figures line and
button:

| | R | G | B |
|---|---|---|---|
| measured, min–max | 12–13 | **0–0** | 12–13 |
| `--ink`, the surface's legitimate lifted black | 22 | 22 | 15 |

**Green is exactly zero on every one of 2449 samples.** With R and B equal, that is magenta at
near-black — and darker than `--ink`. I reported this earlier as "near-black and blue-tinted" from
the source token; that description was wrong, and this measurement supersedes it.

**A second thing, which may change what you are ruling on.** The authored value is
`new Color(0.03f, 0.02f, 0.06f, 1f)`. That lands nowhere near (13, 0, 13) under either a linear or a
gamma reading — linear would put it around (51, 42, 69) — while `Color32`-authored grounds elsewhere
on this surface render essentially 1:1. **So the ground may not be applying as authored.** I have
reported that rather than diagnosed it, and fixed nothing: you deferred the ground, and if the
rendered colour is not the declared colour then the fix targets something other than the token.

What this does not cover: I have not traced the capture path's own colour handling, so I cannot say
whether the discrepancy is in the render, the capture, or the authored value. The frames show what a
player sees either way.

## Also in this drop: the icon-margin change S52 required

`*-11-desktop-flat-*` is the desktop after the column moved. It starts at `--st-pad-x` (14px) below
the rail now — Allen's call on 2026-08-04, since **"the standard margin" is not a token**: the phrase
appears twice in the corpus, both times inside S52, and no token file, register entry or guideline
card defines it. `--st-pad-x` is the surface's only documented content inset. It is recorded in the
source as a decision, not a lookup.

**Reconciling the two numbers S52 records.** On the pre-change frame the tile's top edge was 86px
below the rail and the glyph's first ink was 114px below it. Both are right; they measure different
things. They diverge because the `--ground-3` chip is a **3/255 step** against the wallpaper there —
34,34,22 on 31,31,19 — so the tile edge is effectively invisible and the eye finds the glyph first.
Worth carrying into any future measurement of this column: 28px of any rail-to-icon reading is the
chip's own dead space. That is also the one thing I would gently re-open about "chip/ground-3 fine" —
not the token choice, but that at the top of the wallpaper gradient the chip does not read as a chip.

Suites at this commit: EditMode 76/76, PlayMode 47/47. HEAD `d1a8382`.
