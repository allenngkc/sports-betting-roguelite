# Register entries — 2026-08-15, batch 80

**THE SEAMS — a hole in the C spec, closed before the build reaches it.** Written at the DD seat on
the screen lane starting the scroll.

**Destination table: SureThing — the laptop.** **Row shipped:** `S83-am2`.

**This corrects this seat's own spec.** Batch 78 ruled three zones and their contents and **never
ruled what happens WHERE THEY MEET.** With zone 2 scrolling between two fixed neighbours there are
now two seams that did not exist before, and **content sliding under an opaque block with nothing
between them reads as a collision, not as a scroll.** Raised now rather than found on frames,
because the lane is building against the spec today.

---

## S83-am2 — THE TWO SEAMS. Both derived, neither invented — and they are NOT the same treatment.

### The commit seam (zone 2 → zone 3) takes **6px**, and T47 is the derivation

**T47 exists for exactly this problem, one boundary down.** Its whole basis is that **the flow region
and the anchored band must never meet**, and the answer it landed on is the `+ 6f` inside
`ActionBandReservedHeight` — **the pad S51 refused to spend and batch 73 §2 ruled non-negotiable.**

**Under C that pad no longer sits at the boundary it was ruled for.** Zone 3 is now
`[stake · chips · nudges · payout] + [PLACE · LOCK · SKIP]`, so **T47's 6px has become an INTERNAL
gap inside zone 3, between the payout and PLACE.** **The seam it was ruled to protect has moved up,
and nothing moved with it.**

> **RULED: the zone 2 → zone 3 seam takes 6px of ground, on T47's own reasoning. The flow and the
> fixed block below it must never meet, and that is as true when the flow scrolls as when it does
> not — more so, because the content is moving.**

**And the seam is only meaningful if the content stops at it.** **Zone 2's content CLIPS at its
viewport** (`RectMask2D`, as `BuildScrollingBody` already does) **and never draws into the 6px or
under zone 3.** A pad behind an opaque block is not a pad; it is a hidden overlap.

### The head seam (zone 1 → zone 2) takes **NOTHING**, and that is the board's own precedent

**The symmetric worry is wrong, and the board settles it.** `BoardTitle` is a fixed column head with
`titleStripHeight = 26f` for a 26px title box, and **`BoardBody` clips immediately beneath it with no
separating pad at all** — the arrangement Allen ruled permanent this morning and which reads
correctly on the frame this seat measured.

**A column head and a dense control block are not the same kind of neighbour.** One line of muted
text at the fact floor does not need defending from the rows beneath it; **a stack of five controls
carrying the money does.**

**So the 4px A left between the header content and the first leg is not a seam that needs
re-deriving** — it is more clearance than the board gets, and **A's `8 → 4` harvest is not disturbed.**
*(This seat expected the opposite and checked before writing it: a moving boundary looked like it
needed more separation than a static one, and the board's own arrangement says the distinction that
matters is what sits BELOW the seam, not whether the content moves.)*

### THE VIEWPORT, CORRECTED — 168, not 174

```
margin                             530
  zone 1  HEAD, fixed             − 40     (as A left it; no seam pad, per the board)
  zone 3  COMMIT, reserved        −316
  the commit seam                 −  6     ← batch 78 omitted this
  ─────────────────────────────────────
  zone 2  VIEWPORT                 168
```

**Batch 79's `S83-am` said 174. It is 168, and this row supersedes it.**

### The consequence is good, and it is worth stating because it looks like bad news

| state | zone 2 content | vs the 168 viewport |
|---|---|---|
| 4 legs alone | 140 + 28 | **168 — exactly flush, no scroll** |
| + a held consumable | 202 | scrolls **34** |
| + the relation statement | 204 | scrolls **36** |
| + both | 238 | scrolls **70** |

**The ordinary composition still does not scroll** — batch 78's sequencing condition holds and C is
still clear to build. **And the three scroll distances are 34, 36 and 70: exactly the two bills and
their sum.** **That is the arithmetic closing on itself** — the scroll is the overrun, neither more
nor less, which is what tells us the zone split neither created nor absorbed a pixel.

**Flush is tight and is named as tight.** 168 into 168 has no clearance, so **batch 78's 1px
dead-band is what stands between the ordinary slip and a scrollbar that means nothing.** It is
load-bearing rather than defensive, and **the gate must exercise the 4-legs-alone state explicitly**
— a state that sits exactly on its boundary is the one worth pinning.

---

**Routing.** **→ surething-ui, now, ahead of the build reaching the boundary.** **The viewport is 168
and is still derived from the factored measurement rather than from this arithmetic** — the figure
above is the shape, the measurement governs. **Nothing else in batch 78 changes.**

**To Allen, in one line:** *my own spec said what goes in each zone and not what happens where they
meet — the seam below the slip takes the same six pixels T47 already ruled for the same problem one
boundary down, the seam above it takes none because the board settles that, and the ordinary slip
still doesn't scroll.*
