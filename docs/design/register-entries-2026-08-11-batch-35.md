# Register entries — 2026-08-11, batch 35

**Author:** Design Director (seated at the batch-34 boundary) · **Docket:** TV's C43 collision on T-3
(HELD on this ruling) · **Canon at authoring:** 265 rows, through batch 34.

**Destination tables named per C43's standing practice** — every row below states where it lands.

| row | destination table |
|---|---|
| T81 | **TV — match theater** |
| T78-am | **TV — match theater** (folds into T78's row) |
| G1-am | **TV — match theater** (folds into G1's row, after the move below) |
| C43-am | **Cross-surface** (folds into C43's row) |
| Placement fix — G1, C25, C26 | **moved out of Phone (P)**: G1 → TV, C25 + C26 → Cross-surface |

---

## T81 — Phase T's pair moves the renderer and the face together

**Destination: TV — match theater.**

**Ruled — the migration lands the canon face. The two are ONE variable on this surface, and
option 1 was never the conservative arm.**

TV routed the collision rather than deciding it, correctly, and its lean was right. The reasoning
below re-founds that answer so it is C43 applied, not C43 bent.

### The renderer swap has no null arm

TMP renders from a **baked named instance**. There is no way to perform the renderer swap without
selecting one of the variable file's 45 instances. "Renderer only" is not an available arm of this
experiment: every arm ships a face. The only question is *which*, and canon already answers it.

### Canon already ruled the face, and already gave this pair a face-deciding job

`tv-design.md` §4 (owning document, Allen-approved, amended batch 32) rules **every slot's face, all
23, none defaults**. Three sentences there are dispositive:

1. **"No synthesised styling on this surface" (T73, T77)** — legacy's synthetic bold is not a face
   the family contains, and it is forbidden.
2. **"Retiring them is what the TMP migration is *for* (C15)."** Option 1 is a migration that
   preserves the synthesis this migration is defined as retiring.
3. **`CashOutStatus`'s "face is shown on the Phase T pair with the disposition pre-committed"**
   (§4, and T75/T75-am2's both-branches pre-commitment).

Point 3 settles it on its own. **The owning document already assigns the Phase T pair a face
question**, with both branches pre-committed before the evidence. Option 1 does not merely coarsen
that pair — it *voids a pre-commitment canon has already recorded*, because a set pinned to the
legacy face applies no face split for the control to be read against. Renderer-only contradicts the
owning document directly, not just in spirit. Under §1.2 the owning document governs the face.

### Option 1 moves the face too — to a third value, unevenly

The premise "pin TMP to the instance legacy actually used" assumes we know what legacy uses. **T78
refused exactly that inference.** And even granting the file facts, legacy's bold slots are
*synthesised* from the base; TMP's faux-bold is a different mechanism with a different amount.
Option 1 therefore reproduces the before-half on the non-bold slots and **misses it on the bold
ones** — a set that isolates the renderer on some slots and not others, **with no in-frame marker of
which is which.**

That is the decisive comparison, and it is C18 §4.2's: *a check's blind spot is part of its result.*

- Option 1's pair has a blind spot that is **differential and unstatable** — it cannot say which
  slots it isolated.
- Option 2's pair has a blind spot that is **uniform and statable in one line.**

Between an instrument whose blind spot can be named and one whose cannot, §4.2 selects the
nameable one. Option 1 buys no isolation and cannot report how much it failed to buy.

Reproducing the legacy face faithfully would additionally require *tuning a synthesis parameter to
match a defect* — C10's shape (a mechanism failure tuned toward invisibility) — and would need a
C14/C16 signed deviation, with a named cost and expiry, **to depart from canon on 23 slots at once
for the purpose of preserving a defect.** This seat declines to sign that.

### The cost TV did not name: option 1 doubles T80's freeze

Option 1 is two pairs, and **T80's freeze must span both** — C2, T9, T10 and T61 move the ground
under every value, so they stay frozen until the *face* pair closes, not the renderer pair. Three of
those four are parked against a phase **T41-cl unblocked on 2026-08-04**. Option 1 freezes an
unblocked Phase 3 for two full pair cycles to buy an isolation it cannot deliver.

Per T80, the constraint is this seat's and the sequencing is the orchestrator's: **ruling option 2
holds the freeze to one window.** That is a consequence of the ruling, not a reason for it.

### What the pair certifies, stated at its true resolution

The after-set certifies: **the type stack moved from legacy + defaulted instance to TMP + the §4
canon faces, and here is the rendered delta.** It does **not** certify that the renderer is neutral,
and **no later item may cite the Phase T pair as renderer-isolation evidence.** That sentence is the
pair's blind spot, stated, per C18 §4.2.

### The variable is CLOSED and named — not a bucket

"The type stack" admits exactly three things, and nothing joins later:

1. the renderer swap (`UI.Text` → TMP);
2. the §4 canon face per slot, all 23;
3. the retirement of synthesised bold and italic that follows necessarily from (1) and (2)
   (T73, T77).

**Still sequenced out under C43, unchanged:** T74 / the size reconciliation (§4.1: *"a sizing pass
with its own frames… not a font-stack swap and does not ride inside one"*), **T79** (`BigAmount`),
and any per-slot face reopening. T76 moves nothing (the matrix rule stays).

### Point size is held; rendered EXTENT will move — that is the measurement, not a breach

T74 ruled *"the migration preserves rendered size."* A heavier, wider face changes **advance widths
and string extent** while point size is held. These are different quantities and the ruling governs
the first.

**`TvTypeParityProbe.cs` is an instrument, not a knob.** It reports whether rendered size changed. A
reported extent change is an **outcome of the one variable**, never a licence to re-tune sizes inside
Phase T; any size response routes to the deferred sizing pass. This is the guard that keeps C43's
founding case intact through a deliberately coarsened pair.

Per C41, the expectation is stated as **direction of travel — heavier and wider — and not a number.**
The lane already framed it this way, correctly.

---

## G1-am — the deck's copy holds; its FIT certification is void and re-certifies

**Destination: TV — match theater** (after the placement fix below).

TV named "G1-copy re-authoring" as a known consequence of option 2. **Scope corrected downward — the
copy does not move, and canon already says so.**

§4 / T24-am: *"Where a weight or face change makes a string overrun, the remedy is the size or the
span — **never the copy**; §8's authored forms exist so truncation is never reached."*

- **Stands, unchanged** (face-independent copy decisions): both strings per leg (NEED 249px@28px /
  compact 143px@15px), the club-word and surname conventions, the authored fallbacks, and
  AnytimeScorer's `NOT YET`/`SCORED` pair-defect fix.
- **Void** (C18 §4.1 — *a gate certifies the geometry it ran against, and any change to that geometry
  voids the certification*): G1's batch-27 **fit grant**, which was granted on frames in the old
  face. R22's shape exactly.
- **Re-certifies** on the after-set's own frames. This is a **re-measurement, not a re-authoring.**
- **If a string overruns:** the remedy is the span or the size, and **both routes sit outside Phase
  T.** Record the overrun on the after-set, route it to the deferred sizing pass, do not re-author
  copy and do not re-tune inside the pair.

Authoring reopens **only** if a string overruns and the span route cannot carry it — which is a
finding the after-set produces, not an assumption to build on now.

---

## T78-am — the refusal's argument is softer than the refusal, recorded as this seat's (§1.5)

**Destination: TV — match theater.** **No verdict moves today.**

T78 refused the inference that the TV renders Thin, citing batch-22 frames where bold and non-bold
slots separate cleanly in one frame. **That observation does not refute the inference it was cited
against.** The inference was *"the regular voice renders Condensed Thin **and every `FontStyle.Bold`
call site synthesises bold from a Thin base"*** — which **predicts** clean in-frame separation,
because synthesised bold from Thin is heavier than Thin. The frames are consistent with both
readings; they discriminate neither. Recorded under §1.5 as this seat's, not the lane's: the lane
stated its inference as an inference and asked for frames.

**The file facts are untouched and were never in dispute. T81 does not depend on this** — option 1
fails whether the refusal stands (no known target to pin to) or falls (the target is a canon-forbidden
defect). The collision is ruled either way; this is why T-3 is not held on it.

**Desk work, costs no capture window:** the before-set is already shot and in hand. Measure the
rendered base against the 45 named instances on those frames (stem width and width-axis, in-frame,
per C42's invariant shape). Frames exist — check the crops before scheduling anything.

**Where the answer lands, and it is not Phase T:** if the base was Condensed Thin, then T50's
"confirmed in situ" and T20/T24-am's px re-derivation were taken against a face that is not the ruled
one. That is an **input to the deferred sizing reconciliation (T74)**, which is where it belongs and
where it can wait.

---

## C43-am — one variable means one SEPARABLE variable

**Destination: Cross-surface** (folds into C43's row, C22.1 — the ruling is extended, not replaced).

C43's first live collision produced the clause it was missing. Its named examples — a size
re-derivation, a weight fix — are changes that **could have been sequenced out**, and T79 was, on
exactly that ground. This adds the test for when they cannot be.

**Two changes are ONE variable when neither can be performed without the other.** The test is
**separability, not size**: if arm A cannot be executed without also landing some value of B, then B
is not a fold — it is a parameter of A, and the pair is honest at the resolution of both.

Three corollaries, all exercised here:

1. **Check for a null arm first.** Where a migration's target medium *requires* a value that the
   source medium supplied implicitly, there is no "leave it alone" arm — only a choice of value.
   Preserving the legacy value is itself a change, and usually to a third value that matches
   neither.
2. **A pair whose blind spot cannot be named loses to one whose can** (C18 §4.2). A change that
   isolates unevenly across the surface, with no in-frame marker of where it isolated, is a worse
   instrument than a uniformly coarser pair that states its resolution in one line.
3. **Coarsening is bounded at authoring.** A pair ruled coarse names a **closed list** of what its
   variable contains; everything else stays sequenced out, and nothing joins the list afterwards.
   "The type stack" is a name for three enumerated changes, never a bucket.

**The precedence C43 does not override:** where a surface's owning document has already ruled the
value a migration must land — or has already assigned the migration's pair a question — C43 does not
license shipping a non-canon set to protect the pair. §1.2 puts the owning document above the
register's ruling for the item on that surface's content.

**Not promoted to the constitution.** C43 is still register-level; this is its second catch and its
first application. It promotes when it earns it, and this seat does not promote its own law inside
the batch that first applies it.

---

## Placement fix — three foreign rows in the Phone table

**Bookkeeping, not an amendment. No ruling changes, no ID changes (C22.1).** Found while siting
G1-am under C43's standing practice.

The Phone (P) section's real membership is **P1–P8**. Three rows sit below them that are not phone
items:

| row | what it is | destination table |
|---|---|---|
| `G1` | leg-statement short forms — *"Scope: TV ticket column only"* | **TV — match theater** |
| `C25` | instrument scope is part of a measurement — a cross-surface law | **Cross-surface** |
| `C26` | owning documents owed — cross-surface | **Cross-surface** |

**This is C43's founding defect a second time, three rows over**, and it is the argument for the
standing practice rather than against it: C43 was moved on 2026-08-11 for the same reason — a
cross-surface law parked in a surface's table is invisible to anyone reading the laws end-to-end,
and `C25` is an instrument law that the instrument-law family (C33/C36/C37/C41) does not currently
sit beside.

**Section counts move: TV 85 → 86, Cross-surface 42 → 44, Phone 11 → 8. Total unchanged at 265**
(before batch 35's own new row). A section count that disagrees with the last recorded one is
explained by this move, not by a lost row.

---

## Row count after batch 35

265 + **T81** = **266.** T78-am, G1-am and C43-am fold into existing rows and add none.

Sections after transcription: SureThing 79 · TV 87 · Room 48 · Cross-surface 44 · Phone 8 = **266.**
