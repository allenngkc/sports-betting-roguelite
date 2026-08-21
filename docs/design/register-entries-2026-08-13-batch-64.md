# Register entries — 2026-08-13, batch 64

**THE BEFORE-SHIP LIST CLOSES.** Ruled at the DD seat on `dd-import/tv-t95-transitions-2026-08-13/`
(159 frames, two entry points, tree `8ecdc53`), read at review distance at this seat.

**Destination tables: TV — match theater** (`T95-am`, `G1-am9`) · **Cross-surface** (`C50`, `C51`).

**Rows shipped:** `T95-am` · `G1-am9` · `C50` · `C51`.

---

## T95-am — CONFIRMED FIXED on frames. The scoreline reads as one string.

**The call T95 reserved to this seat — whether the scoreline reads as one string at review distance —
is made here: it does.**

| beat | frame | reads |
|---|---|---|
| **leg-resolution** | `t70am-live-pair` frame000 — `ZAMBONIS 0 — 1 REGULATORS`, 90'+1, event line `CINDER FINDS THE NET` | **one clean string**, clock clear |
| **goal** (punch overlay active) | `goal` seed `42108675` — `ZAMBONIS 1 — 0 STARTUPS`, 60' | **one clean string** |
| **goal** (punch overlay active) | `goal` seed `30941771` — `PLUMBERS 1 — 0 REGULATORS`, 14' | **one clean string** |

**The leg-resolution frame is a TRUE match to the one the defect was named on** — same seed, same
moment, same score, same clock, same event line. Not the filename: **the beat.** And the two goal
frames carry the punch overlay visibly active — the very element that was doubling — rendering
superimposed rather than offset.

**The cause is accepted as TV states it, and this seat's hypothesis is corrected in the direction that
matters.** T95 offered a stale rect as the first thing to check and named T91-am as the likely cause;
the measurement shows **`Matchup` centre 92.7 vs `Score` centre 133.7 — a 41.0px delta that is exactly
this lane's own `scoreCentreShift`**, so the ruling moved a box and **the mirror was not re-derived
with it.** **§3.5's fifth instance** (*a bound added in one place obliges the layout depending on it to
be re-derived in the same commit*), and the first where the dependent layout was **another copy of the
same string.**

**Fixed by construction and PINNED** — one position, one size, both layers, hoisted into shared locals,
with `T95_the_punch_overlay_and_the_scoreline_share_one_rect` asserting width, height, position and
alignment. **The remedy is better than the fix**, and it is promoted at **C51**.

### One correction to the submission's own basis (and it is the reason this seat re-read rather than accepted)

The README states that both named frames are in the set *"under identical filenames — so the comparison
is the same reading of the same frames on a third tree — identical, not analogous."*

**True of one frame. Not true of the other.** `t68am-accept-slot` frame008 here renders
`SPREADSHEETS 0 — 0 MUSKRATS` at **11'** with no score change; the frame the doubling was read on was
**35' on a lead change.** Same index, **different beat** — so that frame is clean and **shows nothing
about the fix.**

**This is T83's own mechanism turned around: the seed pins the DEAL, not the timeline**, which is
precisely why 21 frames were excluded from the Phase T pair. **Frame identity is not beat identity**,
and a confirmation resting on it would have been a false confirmation — the exact mirror of the trap
this seat named at T95 in the other direction (*absence of doubling in a drifted frame is not evidence
of absence*).

**The conclusion is unaffected** — the live-pair frame is a genuine matched beat and the two goal
frames are independent — **but the basis is corrected, because the next claim of this shape may not be
lucky.** Promoted at **C50**.

### Also read on these frames

- **T91-am2 — LANDED.** `Matchup` starts 2.0px right of the ticket column's edge and the widest
  scoreline clears the clock; on the frames the scoreline no longer crowds the leg row's state word.
  **Both sides of the edge now hold.**
- **`MARKET SUSPENDED` still overruns**, visible in the accept-slot frames. **On T74's table by name,
  never part of any gate**, and unchanged by anything here.

---

## G1-am9 — rung 2 is RENDERED, and RATIFIED at review distance. The honest note at G1-am8 closes.

G1-am8 recorded that **rung 2 of neither arm had been seen rendered** and owed its read at the next
capture carrying a long club. **That capture is this one, and it carries one.**

**Seed `30941771` renders `REGULATORS WIN`** — `REGULATORS TO WIN` measured 264.1 against the 261.0
box, one of the five clubs that overrun, **so rung 2 fired exactly as the ladder specifies.** And the
same set renders `MUSKRATS TO WIN` and `STARTUPS TO WIN` at rung 1. **Both rungs observed in one set:
the selector picks by measurement, on frames.**

**THE VOICE CALL, made against the rendered string rather than a description of it (T88's standard):
`REGULATORS WIN` reads as a REQUIREMENT, not a result. RATIFIED.**

The disambiguation is exactly what G1-am7 argued it would be, and it is stronger on the frame than in
the argument: the line beneath reads **`TRAILING 0–1`** and the scoreline above reads
**`PLUMBERS 1 — 0 REGULATORS`**. **A result-reading is not merely unlikely, it is contradicted twice on
the same screen** — you cannot read *Regulators win* as an outcome while the surface says they are
trailing and losing 1–0. **The pair carries the tense, which is what the pair is for.**

**Still owed and unchanged: `{SURNAME} SCORES` measured across the twelve surnames**, and its rendered
read at a capture carrying a long surname. **Rung 1 of the scorer arm is visible here and complete**
(`MUFFIN ANYTIME`, `RACKET ANYTIME`, `PAVEMENT ANYTIME` in compact); rung 2 is not.

---

## C50 — a pinned run pins the DEAL, not the TIMELINE. Frame identity is not beat identity.

**Law, cross-surface** · DD 2026-08-13 batch 64. **Third catch, and T83 deferred it to exactly this.**

**A claim that two frames show the same moment is made against the moment's own observable markers —
the score, the clock, the event line, the state — never against the filename, the frame index or the
seed.** Pinning a run fixes what is dealt; it does not fix when anything happens, so **frame N of two
runs is not the same beat**, and a comparison resting on the index is comparing two different moments
while believing it is comparing one.

**The three catches, on three lanes and in both directions:**

| | case | direction of the error |
|---|---|---|
| 1 | R43, one lane over — the seed pins the deal, not the timeline | founding mechanism |
| 2 | T83 — 21 paired frames matched on moment, seed, scene and index, **differing on grammar**; the README's *"nothing was substituted"* was true and insufficient | a **false pairing** |
| 3 | T95-am — a clean frame at the same index offered as proof of a fix, on a run that had drifted to a different beat | a **false confirmation** |

**T83 wrote that this promotes on a third catch. This is the third**, and it is the first to arrive as a
*confirmation* rather than a *comparison*, which is what completes the shape: **the error is symmetric,
and both halves are silent.** A drifted frame that looks broken invents a defect; a drifted frame that
looks clean retires a real one.

**Practical form: a set claiming to show a beat states the beat's markers in its own README**, so the
reader can check the claim against the artifact rather than against the naming convention.

---

## C51 — a cross-element invariant is an ASSERTION or it does not exist.

**Law, cross-surface** · DD 2026-08-13 batch 64, promoted from T95. **§3.5's enabling instrument.**

**Where two elements are required to hold a relationship — the same rect, the same face, the same
value, the same state — that requirement is expressed as a machine-checked assertion, or it is not a
requirement at all.** A relationship recorded in prose, in a comment beside the code, or in a
reviewer's memory **is unenforced**, and the first ruling that moves one element breaks it **silently
and everywhere.**

**Founding case:** `Score` is the punch overlay and **its own build comment stated the invariant
verbatim** — *"Same text, SAME RECT, same face as `_tMatchup` … so superimposing it."* Both centre in
their own box. T91-am re-bounded one of them; **the comment could not stop the other from staying
where it was**, and the scoreline rendered doubled and illegible on every score change until frames
caught it. TV's own line is the clause's short form: **a shared local is a convention; an assertion is
a contract.**

**Why this is a law and not a note — it is what makes §3.5 compliable.** §3.5 obliges the dependent
layout to be re-derived in the same commit as the bound that moved. **Nobody can re-derive a dependency
they cannot see**, and this one was visible only to a reader of one comment in one file. **A rule that
requires perfect recall is a rule that will be broken; C51 converts it into one an instrument keeps.**

**Second catch of the same shape, named because it is the reason this promotes now rather than later:**
C46-am2 found that **the code knew a worst case and the sweep did not use it** — *a worst case
documented in a comment beside the generator is not a worst case the generator uses.* Same defect
class, different instrument: **knowledge recorded where nothing can act on it.**

**Both instances are on the TV surface**, so this ships as a register-level law binding all four
surfaces by its reasoning; **it promotes to the constitution on a catch on a second surface.** This
seat does not promote to the constitution on one surface's evidence.

**Scope, stated so it is not read wider than it is: this governs INVARIANTS BETWEEN ELEMENTS**, not
every design decision. A ruled value lives in the owning document; **a required RELATIONSHIP between
two elements lives in a test.**
