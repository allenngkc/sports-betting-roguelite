# Register entries — 2026-08-09, batch 27

**Seat:** Design Director (`main-2` terminal) · **Source:** `dd-import/tv-batch22-window/`
(`B22-MEASUREMENTS.txt`, frames 01–07; `04-accept-flood-PEAK` read at review distance by this seat),
source verified at `04f7739` and `41d5cbe`.

**Batch 22's three needs-frames verdicts are all decided.** One grant, one withdrawal of my own flag,
and one refusal that goes further than the lane's finding.

---

## T68-am + T71 — REFUSED on the frames. And the reason outranks the finding

### What the measurements establish

The lane's time series is exactly the instrument C35/V8 asked for, and it answers cleanly:

| | frame 0 | frame 20 (peak) |
|---|---|---|
| accept — ground | 0.6881 | 0.6899 |
| accept — ink | 0.0640 | **0.3840** |
| **CR** | **6.47** | **1.70** |
| win tally — CR | 6.58 | **1.86** |

**The ground is static (0.688–0.690); the ink is not.** The step to 0.586 at frame 24 is
`PunchThenSettle` releasing at `hdrPunchDuration` — **§6.1's punch→L3 settle works exactly as ruled.**
The ink rises 0.064 → 0.384, tracking the flood's own 0.063 → 0.507. **The punched-out dark type is
being painted over.**

**Batch 22's refusal to rule on green suites and a computed 9.68:1 is vindicated in both directions:**
the computation was right about the *field* and blind to what draws over the ink, and the rendered
alpha-0 value is **6.47:1, not 9.68 — a 33% gap** in the quantity the ruling turned on.

### The finding above the finding: this is a FULL-FIELD wash, and T40 deleted it

The lane diagnosed z-order — the flood is created after the slot, so it renders on top. **True, and it
understates the case.** Verified in source at `04f7739`:

```
_wonFlood   = MakeStretchImage(root, "WonFlood",  …)   :3541
_goldFlood  = MakeStretchImage(root, "GoldFlood", …)   :3548   + MakeHdrMaterial()
```

`root` is the **screen root** — the same root passed to `BuildTicketColumn`, `BuildScoreBug`,
`BuildEventStrip`, `BuildCashOutZone`, `BuildChromeStrip`. **The flood is not a zone element that
happens to sit above the slot. It is a full-screen HDR wash created after every zone on the surface.**

And the frame says so plainly. Read at review distance on `04-accept-flood-PEAK`: **the entire screen
is gold** — the scoreline, the pitch, `OVER 2.5 GOALS`, `BTTS NO`, `PAVEMENT ANYTIME`, `RISK $87 PAYS
$1,818`, the event strip, the chrome. Not the cash-out zone. Everything.

**T40 (batch 5, 2026-08-01) ruled this deleted:** *"the LEG-WON gold wash and the dead-leg oxide wash
are deleted, not dimmed (C10) — a full-field wash spends the whole gold ration in one frame and is a
celebration; the win is carried where it is already carried."* `_wonFlood` is that ruling's first named
subject, by name, still in the tree.

**The frame makes T40's case better than T40 did.** Gold is money (C4). At flood peak **every fact on
the surface is gold**, so for 0.6 s the money signal means nothing — and it lands on the exact beat
three batches of ladder work (T63, T66, T68) existed to protect. It is T65's defect one layer up: the
room was stopped from flooding gold on a leg win and routed through a single settlement painting point;
**the screen still floods.**

### Ruled

1. **T40 governs. The full-field flood is STRUCK** — deleted, not z-ordered, not dimmed (C10).
2. **T68-am's "the flood stays" is corrected as this seat's error (§1.5).** It was a passing clause in a
   ruling about where money reads, written without reference to T40, and **a passing clause cannot
   ratify an effect an earlier ruling deleted.** The lane built it faithfully; the register contained
   both and the seat did not reconcile them.
3. **The z-order remedy is moot, and so is the lane's concern about it.** It asked whether moving the
   flood below the slot "changes what it washes generally". **Nothing else should be washed either** —
   there is no general washing to preserve. Do not reorder a wash that should not exist.
4. **The beat keeps its punctuation.** §6.1's brief L4 punch then settle is measured working
   (0.688 → 0.586 at frame 24). The flood was redundant with the punch, not carrying it.
5. **The fix's outcome is already photographed.** Frame 0 *is* the flood-at-alpha-0 state and it
   measures **6.47:1 accept / 6.58:1 win tally**. With the flood gone that is the shipping value — a
   pre-verified fix, needing confirmation rather than discovery.
6. **T71 rides with it, one commit, unchanged reasoning:** divergence between siblings is what produced
   T68, and both siblings are in the same measurement above.

**`_dimOverlay` is the same construction** (`MakeStretchImage(root, …)`) and is **not** struck — a dim
is not a wash and T40 does not reach it. Named so the removal is scoped to the two floods and does not
quietly take a third element with it.

## C35-am — The contrast pair is what the EYE receives, not what the layers author

**Amended** · DD 2026-08-09. Fourth correction to this instrument family.

C35 gave V8 the clause *"an inverting element reports whether its own ground is STATIC across the
beat."* **On this frame that gate reads GREEN and the element is illegible at 1.70:1.** Stable ground,
inverted ink, and a third thing drawn between the ink and the camera.

**Amended: an inverting element reports the composited luminance at its own pixels against the
composited luminance beside it — and V8 additionally reports whether anything draws OVER the element
between it and the camera.** A gate that asks only what is *behind* an element cannot see what is *in
front of* it, and legibility lives in what reaches the eye, not in what the layers were authored as.

The class is live independent of this instance — any full-screen overlay, dim or vignette created after
the zones does the same thing, and one such element remains in the tree by design.

## G1 — GRANTED · CLOSED

Four markets on one ticket, longest names the slate offers, **read at review distance on frame 04**:
`MUSKRATS TO WIN`, `OVER 2.5 GOALS`, `BTTS NO`, `PAVEMENT ANYTIME` — **one line each, no wrap, no
truncation, no ellipsis.** T69's founding defect does not reproduce.

**The fallback shipped and was photographed working.** BTTS-No's live NEED renders `ONE TEAM BLANKED`,
not `ONE TEAM SCORELESS` — the authored fallback G1 pre-committed, taken because the primary missed its
measured column, **predicted by the EditMode measurement and confirmed in the render.** Third
pre-commitment to fire in three days.

**TotalCorners and TotalCards — granted on the measurement, not the frame, and here is why that is
allowed.** They could not be shot: a ticket may not carry two legs on one fixture and the slate ran
out. Normally that is a gap. **But the EditMode column measurement has just been validated against a
render** — it predicted `ONE TEAM SCORELESS` would miss its 249px column, and the frame took the
fallback exactly as predicted. An instrument confirmed against a rendered frame may stand in for one
(C11 is satisfied by the *validation*, not by each use). **Their forms go through `FitToColumn` like
every other, and `FitToColumn` is now frame-proven.**

*Scope (C25):* longest names **this seed** offers, not the longest the generator can produce. A worse
case beyond this slate is not photographed and is not claimed.

## T70-am — my BTTS flag is WITHDRAWN. It was my error

Batch 23 flagged `BOTH HAVE SCORED` as a candidate under the corrected information test, reading it as
the resolved state of `BOTH TEAMS SCORE`. **Verified at `41d5cbe`, it is not:**

```
DescribeBttsYes → NEED "BOTH TEAMS SCORE"    live "{n}/2 TEAMS SCORED"
DescribeBttsNo  → NEED "ONE TEAM SCORELESS"  live "BOTH HAVE SCORED" / "CLEAN-SHEET PATH LIVE"
                       (fallback "ONE TEAM BLANKED")
```

**`BOTH HAVE SCORED` sits under `ONE TEAM BLANKED`, where it shares no term with its requirement and
carries the break.** It passes. Both BTTS pairs pass:

- `BOTH TEAMS SCORE` / `{n}/2 TEAMS SCORED` — shares vocabulary, carries a count. Ruled passing in
  batch 23.
- `ONE TEAM BLANKED` / `BOTH HAVE SCORED` — no shared term, carries the break. Passes.

**I read two adjacent lines out of a diff fragment and took them for one pair. They are two methods.**
Recorded as the seat's error (§1.5) — third source-reading error in this docket family, and the same
shape as `PRICES FINAL`: a conclusion drawn from a fragment that the surrounding source would have
corrected.

**No seed hunt is needed, and the lane should not spend one.** The gap it honestly filed — the flagged
branch never rendered — was a gap in a flag that should not have existed.

**The three moneyline pairs and the BTTS pair are granted as composed** — `MUSKRATS TO WIN` over
`TRAILING 0-1` read directly on frame 04. `LEVEL 0-0`, `TRAILING 0-1`, `LEADING 1-0` and
`CLEAN-SHEET PATH LIVE` all carry match state the requirement cannot. **T70-am is satisfied on frames.**

---

## Recorded, on the window

The BTTS pair not composing on the first run was **found by looking at the frames, not by the run
failing** — a green run that photographed the wrong thing, caught by eye. That is C18 §4.2's spirit
without needing the clause quoted at it.

And the z-order finding was **staged as a finding rather than fixed**, on the correct judgement that
changing what a full-screen element draws over is composition and therefore this seat's. **It was more
this seat's than the lane knew** — the element should not be there at all, and only a frame plus the
register together could show that. The suites were green, the computation was clean, and a ruled-deleted
effect has been shipping over the surface's money for some time.
