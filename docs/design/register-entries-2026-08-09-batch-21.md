# Register entries — 2026-08-09, batch 21

**Seat:** Design Director (`main-2` terminal) · **One item.** Batch 20's Owed·1, verified at HEAD as
promised rather than left as an observation. Filed as its own batch because batch 20 has shipped
(`45c678b`) and a shipped batch is not amended (C22).

---

## S37-cl — `PRICES FINAL` leaves the masthead subline

**Ruled — conformance; S37 governs, no new ID** (C22.1). Verified on **two lines of evidence**, source
and render, which is what §2.5 asks for and what a single grep would not have been:

- **Source, at HEAD:** `SportsbookApp.cs:98` — the masthead's `Run` element prints
  `ROUND {run.Round} OF {run.Config.Rounds}  ·  PRICES FINAL`.
- **Render:** Allen's 2026-08-09 playtest frame reads `ROUND 1 OF 8 · PRICES FINAL` beneath the
  brand. (Unaffected by C38 — a string's presence does not depend on its sharpness.)

### It is not an unexecuted ruling, and I said it might be — correcting that first

Batch 20 flagged this as "an unexecuted ruling already granted twice." **That was wrong, and the
source says so.** S50 §1's deletion **was executed** — on a *different* instance. The comments at
`SportsbookApp.cs:809-813` and `:836-839` record exactly what was deleted and why: the working
margin's house status line, `PRICES FINAL. NOTHING YOU DO MOVES THEM.`, worth 18 of the 44 px, with
an explicit note that re-adding it would resurrect the restatement S37 forbids.

So the lead executed S50 faithfully and read the *masthead* instance as the sanctioned one. **That
reading is documented in the source, and it is reasoned.** This is a scope divergence between a
ruling and a build, not a ruling ignored — and the register would have recorded it as the latter if
nobody had opened the file. **Recorded as this seat's error (§1.5), second instance today:** batch
20's Owed·1 inferred an unexecuted ruling from a rendered string without reading the source that
would have corrected it, one section after ruling that a source read is not a measurement. The
inverse error is equally available and I made it.

### The ruling

**`· PRICES FINAL` is deleted from the masthead subline. The subline is `ROUND n OF 8`.**

S37's wording settles it (*"the subline is `ROUND 1 OF 8`"*), but a ruling that rests only on its own
wording is a ruling nobody can apply to the next case. The reason underneath it:

**A line that is identical on every screen in every round is not scope — it is a standing rule.** The
masthead's job is to tell the player *where he is in the run*; `ROUND 3 OF 8` does that and changes
while doing it. A constant cannot, and printing it in the slot that answers "where am I" costs the
slot half its bandwidth to say nothing new. This is the general form of S37 and it is the test to
apply to any future masthead candidate: **does it change?**

**Where the fact goes: nowhere, for now.** `PRICES FINAL` is a *real* product fact — prices do not
drift, and that is worth knowing before you stake. But it is learned once, in the first round, and
never needs restating. Its editorialized half was already deleted for good reason. **I am not opening
a slot for it elsewhere**: nothing in the record shows it has been missed, and inventing a home for a
line nobody asked for is how a surface accretes. If a playtest shows a player surprised that prices
are fixed, that is a new item with evidence behind it.

### Cost and sequencing

- **One string, no layout consequence.** The text is deleted from inside an existing 340×20 box;
  no band moves, no origin re-derives. **§3.5 does not apply** — stated so nobody re-derives a grid
  that did not change.
- Returns ~18 px of visual quiet to the masthead block. Not a headroom grant, and not spendable.
- **Rides with the next SureThing commit.** It does not earn one of its own.

### Recorded, on the source

The comment blocks at `:809-813` and `:836-839` explain what was deleted, why, what would resurrect
it, and which ruling each clause serves. **They are why this took one file read instead of a capture
cycle**, and they are the reason the divergence is legible as a reading rather than a lapse. Comments
that name the ruling they execute are how a build argues back — worth copying.
