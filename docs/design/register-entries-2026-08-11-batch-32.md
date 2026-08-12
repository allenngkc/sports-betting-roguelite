# Register entries — 2026-08-11, batch 32

**Seat:** Design Director (`main-2` terminal) · **Subject:** TV Phase T — the seven type-migration
gaps. One of them was an inference that existing frames already answer, and it answers *against* the
inference, which is why nothing else in this batch is held for it.

**Docket:** `docs/design/dd-import/tv-phase-t-gaps-2026-08-11.md`, keyed T72–T78 by the TV lane.
Source inventory `docs/tv-sweat-refinement/phase-t-inventory.md` on `slice/tv-sweat-refinement`.

**Evidence read at this seat, not taken on report:**

- `unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans.ttf` — md5 `908166ba…`, byte-identical in
  `main-2` and `tv-sweat`. Inspected with the lane's own `tools/ttf_faces.py`.
- `EncodeSansCondensed.ttf` — md5 `4d598f9d…`, same in both worktrees.
- `docs/design/dd-import/tv-batch22-window/` — `06-crops-three-claims.png`,
  `07-crops-composed-pairs.png`, `B22-MEASUREMENTS.txt`. Build **`7ab60b8`**, seated in-room render
  (SeatedEye, FOV 17° at 2.18 m), seed **48151623**, `boost1.4`, frame-locked. C34-compliant: seed
  pinned and asserted, Editor released, DLL cmp-verified identical to HEAD. Shot 2026-08-09 — eight
  days after the face landed at `ccc6f56`.

**The key is clean.** Highest existing T-row is T71; T72–T78 and C43 appear nowhere in
`docs/design/` in text. The lane re-keyed off `G1` before staging rather than after transcription.
Recorded because the register carries three legacy renumber scars (T22–T27, T53–T56, C35-issued-as-C34)
and this is the first time the collision was caught in front of the transcription instead of behind it.

---

## T78 — the face the surface is actually rendering. **The file facts hold. The inference does not.**

### The file half — confirmed independently at this seat

Every measured claim in the docket reproduces exactly:

| claim | reproduced |
|---|---|
| axis defaults | `wght` DEFAULT **100** (100–900) · `wdth` DEFAULT **75** (75–125) |
| `OS/2 usWeightClass` | **100** · `usWidthClass` 3 |
| nameID 1 | `'Encode Sans Condensed Thin'` · nameID 17 `'Condensed Thin'` |
| postscript | `'EncodeSans-CondensedThin'` |
| default instance | **[0] Condensed Thin**, at the axis default on both axes |

`EncodeSansCondensed.ttf` is confirmed static, `usWeightClass` **400**, Regular only.

The default instance of the variable file is wrong on both axes. That is a fact about the file and it
is not in dispute.

### The inference half — refused as stated

The docket's inference is that Unity's legacy `Font` renders the default instance, and therefore the
TV has been rendering its regular voice in Condensed Thin 100/75 since `ccc6f56`, with every
`FontStyle.Bold` site synthesising bold from a Thin base.

**Existing frames contradict it.** Seven frames across five states in `07-crops-composed-pairs.png`,
plus the four-market crop in `06-crops-three-claims.png`, at build `7ab60b8`.

The argument is an **in-frame invariant**, not a judgement of absolute weight — C42's shape, this
seat's own law from yesterday, applied to the axis it was written for:

- The bold-styled slots (compact leg statement, NEED, risk/pays, cash-out figure) and the non-bold
  slots (progress line, price, `NEXT` label) sit **in the same frame**, through the same bloom,
  chromatic aberration and grade. The pipeline is common to both and cannot explain a difference
  between them.
- The **non-bold** slots — `TRAILING 0–1`, `LEVEL 0–0`, `LEADING 1–0`, `CLEAN-SHEET PATH LIVE`,
  `−227`, `−176`, `NEXT` — render as solid, clean letterforms at a regular-class weight.
- Weight 100 at 19–28 px is a hairline. Bloom spreads light around a stroke; it does not convert a
  hairline into a solid letterform, and it cannot do so for the non-bold slots while leaving the
  bold-to-non-bold differential as modest as these frames show.

**Therefore the base face is not weight 100.** The weight half of the inference is falsified on
rendered evidence that already existed.

### Scope — what this read does *not* establish (C25, C32)

- It does **not** identify which instance is rendering. Absolute weight is unreadable through
  bloom + CA + grade at this ramp, and I make no claim on it.
- It does **not** settle the **width** axis. `wdth` 75 vs 100 is not resolvable at the acceptance
  view and I decline it explicitly. **The width half of T78 stands open.**
- It is a relative-weight read, qualitative, at the acceptance view — deliberately, because that is
  the only view any claim about this surface is made at (C11).

Per C41's discipline I state a **direction of travel and not a number**: whatever is rendering, it is
heavier than 100.

### What this protects

**T50 and T20 / T24-am are NOT destabilised, and no re-derivation is authorised on the inference.**
This is C17 in its exact form — no rebuild verdict on a state no capture shows — with the frames
running the other way. Had the inference stood unchallenged, the lane would have re-derived a px
scale, and T50's in-situ confirmation would have gone back into question, on a premise that a
capture taken two days earlier already refutes.

### The before-set still runs, and its question changes

Not *"confirm we have been shipping Thin."* Instead: **name the instance that is actually rendering,
given it is demonstrably not the axis default.** Better posed and cheaper to answer.

**Both dispositions pre-committed now, before the frames land:**

1. Before-set names an instance at **`wght` ≥ 300** → the inference is dead, T50 and T20/T24-am
   stand as they are, and T78 closes as a **documentation defect** (below), not a surface defect.
2. Before-set names **`wght` 100** → this seat's frame read was wrong, recorded under §1.5, and
   T50 + T20/T24-am re-open for re-derivation against the true face.

The width axis is settled by the same set and carries no pre-commitment, because this seat has
offered no read on it.

### The documentation defect — where the inference came from, and why it must not survive

Two artefacts assert the render behaviour as fact:

- **`FONTS.md` line 16** — *"Unity's legacy `Font` renders its default instance."* A claim about
  engine behaviour, never measured on this surface, now contradicted by frames. It corrects or it
  carries a hedge. It must stop licensing inferences. R39's precedent: a dead assertion sitting in
  source licensed shipped code.
- **`tools/ttf_faces.py`'s docstring** — *"Unity's legacy `Font` renders the default instance, so
  that is what the TV surface has been rendering."* The docket hedged this correctly; the committed
  tool does not. **C40 applies exactly**: the tool prints a measurement and asserts an inference in
  the same breath, which is how the inference launders into fact for the next reader. Label the
  measured half as measurement and the render claim as inference, **in the tool**.

The tool otherwise stays and is the reason any of this was catchable. Keep it.

### Recorded to the lane's credit

The docket labelled the inference **as** an inference, recommended ruling on frames rather than on
it, and stated a direction of travel rather than a number — C41 applied unprompted, three days after
the clause existed. That discipline is the only reason this seat could check it in an afternoon
instead of a lane re-deriving a type scale. Named as the standard.

---

## T72 — team names and score figures share one component. **Split it. Option (a).**

Canon puts team names on condensed and SCORE figures on regular. `_tMatchup` renders both as one
string in one `Text`, so the split is unsatisfiable as written. The lane offered three routes.

**Ruled: (a) — split into name / score / name spans.** The shape already exists in this codebase;
TV-14 used exactly three spans for the compact leg row.

Why not the cheaper two:

- **(c) whole line condensed** puts the SCORE on the condensed face. Two objections, either fatal.
  The score is the surface's first law — *the largest element at all times, nothing outgrows it* —
  and it is not resolved by moving it off its ruled face for a component's convenience. And §4 makes
  **tabular figures mandatory** for exactly this slot: the score changes in place on every goal.
  Whether the *condensed* face's figures are tabular is a measurement nobody has made — T11 measured
  the family, and S29 is the standing lesson that a family-level `tnum` claim does not survive to a
  specific face. Option (c) would put the surface's most-changing figures on an unverified figure
  set.
- **(b) whole line regular** puts team names on regular while team names elsewhere stay condensed —
  one object, two renderings, decided by where it appears. That is S60's defect at the laptop
  (`RECORD` header biro on one screen, toner on the other), and it is a violation for the same
  reason here.

**Cost, named (C16):** (a) is the expensive route — a component change threaded through the
scorebug. Expensive is not impossible. Only the platform makes a thing impossible, and expense buys
no deviation on the surface's first law.

**§3.5 fires — a bound is not a layout.** A three-span scoreline re-derives the scoreline's own
composition **in the same commit**: span origins, the em-dash's spacing, and the centring of the
assembly. Splitting one string into three changes how advance widths accumulate. Once, at design
time — §6 forbids a zone sized to content at runtime. Fifth instance of this shape after T20, T47,
T51 and S59.

**T32 is untouched.** It ruled team names `--tv-fact` with hues confined to pitch dots. That is
colour; this is face. The lane read that boundary correctly.

**Not held for T78.** This is an assignment ruling and it is correct whichever instance resolves.

---

## T73 — bold on the condensed slots. **Real Condensed Bold 700, all four sites.**

Confirmed at this seat: the variable `EncodeSans.ttf` carries **Condensed Bold at instance [6],
`wght=700 wdth=75`**, and the static condensed file is Regular 400 only. The lane's correction to
its own premise holds — **there is no asset decision and no licence decision here.** The weight has
been in the repo since `ccc6f56`.

Four call sites pass `FontStyle.Bold` today: compact leg statement, NEED, risk/pays, cash-out
figure. Two of them are the loudest facts on the surface. All four get the real instance.

Why:

1. A synthesised bold is a smear, not a face. C14 is 1:1 with the intended design, and a platform
   approximation is not the design.
2. **This is what C15 is for.** S20 ruled variable weight unaddressable in UGUI a *constraint*; the
   migration exists to make it reachable. Retiring synthesis is the migration's purpose, not a
   bonus.
3. Zero cost, real fidelity gain. Under C16 a thing that is neither impossible nor expensive is
   simply done.

**§3.5 fires again, and harder.** Real Bold 700 has different advance widths than a synthetic
emboldening of the base face. NEED and the compact leg statement are the two slots whose fit was
authored against a **measured column** — G1's deck, granted on rendered fit at batch 27, where
*FitToColumn is the authority over character counts, two at-budget forms measured not assumed.*

**Landing Bold 700 obliges the G1 deck's fit to be re-verified against the measured column in the
same commit.** A weight change is precisely the stale-measurement class T24-am was written for.

**Named precondition, because the cheapest wrong fix is right here:** if Condensed Bold 700 overruns
the column, the remedy is the size or the span — **never the copy.** Authored strings do not bend to
measurements (T24, §6), and G1 authored fallbacks specifically so truncation is never reached. A
trimmed string would spend the deck's whole argument to save a re-derivation.

---

## T74 — which size authority governs. **Neither. The migration preserves rendered size.**

The docket's arithmetic is correct; I recomputed all ten rows and both columns. No single base
satisfies the ratio table — the implied score-size runs 36 → 68 — and every shipped size sits above
its canon ratio, consistently.

The owning document has already spoken, in a place the docket did not cite. **§10 item 5,
quarantined:** *"the two type tables disagree — relative ratios versus reference px cannot both hold.
Ratios are the law; the px table is one provisional instantiation."* That is canon, Allen-approved
2026-08-07.

But taken literally as a migration instruction it is wrong, and the way it is wrong is the finding.
Re-deriving every size from the ratios against a 36 px score gives team 19.8, clock 18, need 18,
risk 14.4, event 13, leg 12.2 and **label 7.9** — a ticket column shrunk past legibility, T20/T24-am
destroyed, and G1's measured fit and T69's no-wrap grant both void.

So the two tables are not two answers to one question. **They are two instruments answering
different questions, and neither is a size authority:**

- **The ratio table encodes ORDER.** Its load-bearing sentence is §4.1's first: *the score is the
  largest element on the surface at all times, nothing outgrows it, cash-out included.* That is a
  **ranking**, and the numbers are its rough shape.
- **The px table encodes FIT.** It is what a narrow column and a legibility floor produce.

Trying to make one yield the other is a category error this studio has already ruled on one register
across: **C33(b)** — *a per-element value check cannot see a ranking; emphasis is not one scalar.*
The ratio table is a ranking instrument, the px table a per-element fit instrument, and C33's
correction applies unchanged when the axis is size instead of brightness.

**Ruled, therefore:**

1. **Phase T is a face migration, not a size migration.** Every slot lands at its **current rendered
   size**. The migration's success criterion is that the TMP build is size-identical to the UGUI
   build at every product-fact slot — the identical bar Phase L was granted on ("every product-fact
   slot identical ink at identical scanlines through the full stack replacement").
2. **The ratio law stays the law, for order.** The ranking score > cash-out > team ≈ clock ≈ need >
   risk > event > progress ≈ leg > label holds on the built surface and is asserted **against the
   composition**, per C33(b) — never as ten per-element size checks.
3. **The px table stays provisional.** §10 item 5 is unchanged and this ruling does not promote it.
   It is carried forward because carrying it forward is what *migrate the face, not the layout*
   means.
4. **The reconciliation is deferred and named as owed.** Re-authoring the surface's type scale is a
   sizing pass with its own frames. It is not a font-stack swap and must not ride inside one.

**Why the deferral is the ruling and not an evasion:** a migration verified by a before/after pair
must move one variable. If the stack and the sizes move together, no frame in the pair can attribute
a difference to either, and the pair — the only instrument the migration has — stops measuring.
That is §2.6's shape applied before the measurement is taken, and it is promoted below as **C43**.

**Sequencing with T78, and they support each other:** preserving size is exactly what makes the pair
diagnostic for the face question. Swap only the stack, and the pair shows the face change cleanly.
If T78's answer later says the face was wrong, the sizing pass that follows re-derives against the
correct face, once, with frames. **T78 answers → then the sizing pass. Never both inside the
migration.**

---

## T75 — the 12 unowned slots. **Granted: ruled regular, not defaulted. Three carve-outs.**

Canon names 7 condensed roles and 4 regular; the surface has 23 slot types. Twelve render regular
today by defaulting rather than by ruling. The lane asked for the default to be confirmed so the
inventory records a decision. **Granted** — and the finding above the item is that **half the
surface's type had no owner**, which is C18's own subject: the doc's split named 7 + 4 of 23, and an
inventory that does not name its members is a claim that does not say what it covers.

Three of the twelve are not harmless defaults and are ruled explicitly:

- **`Clock`** and **`BigAmount`** are named by §4's tabular mandate — *scores, clocks, money and
  counts all change in place.* Regular is almost certainly right for both (it is the face canon
  gives SCORE figures, the tabular case par excellence), so the default **confirms** rather than
  conflicts. But they are **verified tabular on the built face, per slot, on frames.** A figure slot
  ruled by default and never checked for `tnum` is S29's defect exactly, and S29 is why nobody gets
  to assume this twice.
- **`CashOutStatus`** is the one member sitting inside a ruled control (§6.1, the money control,
  six states). Its neighbour — the cash-out figure — is a named condensed role and takes Bold 700
  under T73, so the default would put two faces inside one control. That may be ordinary
  label-and-figure practice or it may be two voices; it is not rulable from the record.
  **Shown on the before/after pair, disposition pre-committed:** reads as two voices inside one
  control → it moves to condensed; reads as label-and-figure → the default stands.

`MomentumLabel` is not in question — it cites canon directly and is correctly excluded.

**Owning-doc consequence:** §4.1 carries **all 23 slots** with their faces, so the next reader sees
a decision rather than an absence.

---

## T76 — strikethrough. **The matrix rule stays. The lane's reasoning is adopted as the ruling.**

Phase T removes the *technical* premise — TMP has native strikethrough where UI.Text did not — and
the lane's own sentence is the answer: the migration makes the alternative **available** without
making it **correct**.

Three reasons, in ascending order of weight:

1. §6 forbids geometry computed from content. Native strikethrough is content-derived by
   construction — its length *is* the text advance. Adopting it would put the surface's first
   content-sized geometry on screen, in the VOID state.
2. A fixed-width rule is the only one whose length does not move when the face changes. Given T78,
   that is a virtue nobody planned for.
3. **Decisive:** canon does not ask for strikethrough. It says *"STRUCK THROUGH on the matrix"* — a
   **matrix rule**, and that is a different object. A strikethrough is a property of a string. A
   matrix rule is a mark the board makes across a cell: the institution striking a line through a
   row. That is this surface's entire register, and the laptop rules the analogous mark the same way
   — the oxide strike is *the house's mark* (S3, S15-am), drawn, not a text decoration.

Routing it rather than assuming it was correct: §6 is this seat's, and a lead who reads a rule
correctly and still escalates it is doing the contract.

**Consequence:** the rule's width was set against the column, so under T74's preserve-size ruling it
does not move. If a later sizing pass changes the column, the rule re-derives with it, once, at
design time (§3.5).

---

## T77 — no italic face. **Drop the slot to regular. The synthesised italic goes.**

Confirmed at this seat: 45 named instances, axes `wght` and `wdth` only, **no italic anywhere**, and
no italic file committed. `_tConsolation`'s italic is a shear.

All three options considered; the third is the one worth arguing:

- **Keep the synthesis** — refused. It ships a letterform the family does not contain, on the one
  slot with no real face behind it, in the batch that retires synthesis for bold. Ruling synthesis
  out at T73 and in at T77 would be incoherent within a single batch.
- **Rule an italic in** — refused. Encode Sans has none, so this means a *second family* for one
  consolation line, breaking §4's *one hand, different jobs*, and spending Allen's attention and a
  licence decision on it. Not proportionate to one slot.
- **Drop to regular** — ruled. And this is the cleanest case the batch contains: **no canon line
  assigns italic to the consolation slot at all.** The italic is not a design being approximated; it
  is an unruled style that arrived in code. There is nothing to be faithful to.

If the consolation line needs to feel apart from its neighbours, that is carried by the channels
this surface already owns — size (§4.1's ladder), value (§2's brightness ladder) or position — never
by a letterform the family does not have.

**Standing line for the surface, from T73 and T77 together:**
**Phase T retires synthesised styling on the TV. Weight comes from a real instance; slant is not
used.** That sentence is what the migration is for.

---

## C43 — A migration moves one variable, because its verification pair is the instrument

**Law · register-level, DD 2026-08-11 batch 32. Deliberately NOT proposed for the constitution —
one founding case, following C39's and C42's precedent. Promotes if it catches a second.**

**Where a change is verified by a before/after pair, that change moves exactly one variable. Anything
else the change would like to alter is a separate pass with its own pair. A pair spanning two
variables cannot attribute a difference to either, and stops being an instrument.**

A migration is the standing temptation, because it touches every slot and therefore looks like the
cheap moment to fix everything those slots were owed. It is the opposite: it is the one moment when
the surface has an instrument pointed at it, and loading a second variable into the frame is how
that instrument is spent.

*Founding case:* Phase T's T74. The ratio and px tables have disagreed since before the column
narrowed, and the font-stack swap touches every size in the process of touching every slot. Ruling
the sizes inside the migration would have produced a pair in which no measured difference could be
assigned to the stack or to the re-sizing — while the surface's own §10 already quarantines the px
table, so nothing forced the bundling but convenience. Phase L is the counter-example that shows the
discipline working: it moved the stack and nothing else, which is precisely why *identical ink at
identical scanlines* could be asserted and granted.

Relation to §2.6: that clause returns a confounded measurement unadjudicated **after** it is taken.
This one refuses to *build* the confound, before the frames exist. Same discipline, one step
earlier — the position C41 occupies for predictions, C43 occupies for changes.

---

## Routed, not ruled

- **To the TV lane —** `FONTS.md` line 16 and `tools/ttf_faces.py`'s docstring both state the
  default-instance render behaviour as fact. Correct both to state it as an inference, or drop the
  claim; the tool's *measured* output is untouched and stays as it is. Small edit, no re-shoot
  implied, and it is what stops the next reader inheriting the claim as settled (C40).
- **To the TV lane —** the docket's corroborating line, that `FONTS.md`'s *"wider than canon intends
  for the condensed slots"* is consistent with a condensed render mistaken for a regular one, does
  not carry. A render at `wdth` 75 would sit **at** the condensed width, not wider than it. The
  observation is neutral-to-contrary, so the inference rested on a single unmeasured assertion rather
  than on two agreeing signals. Recorded because knowing an argument had one leg and not two is what
  made ruling it on frames obviously right.

## Carried

- **The re-key is the model.** Caught in front of the transcription, verified free before claiming,
  key stated in the docket header, and the reason written down. Three prior renumbers in this
  register were all caught behind the transcription. This is the first one that cost nothing.
- **The premise that changed mid-window (T73)** was reported as a correction to the lane's own draft,
  in the direction that made its own ask smaller — a new font file turning out to be no font file at
  all. A lane that re-reads its premise while the docket is open, and reports the finding that
  shrinks its own request, is the standard S64 and R19b-am2 named.

## Owed, and by whom

- **This seat:** nothing outstanding in this batch. T78's width half and the two pre-committed
  dispositions (T78 weight, T75 `CashOutStatus`) are answered by the before-set and are already
  written down, which is the point of pre-committing them.
- **The orchestrator:** transcribe T72–T78 and C43 into the tables and report the ID list back
  (C22); amend `tv-design.md` per T74 (§10 item 5 cites T74; §4.1 carries the order-vs-fit split),
  T75 (§4.1 carries all 23 slots), T73/T77 (§4 gains the no-synthesised-styling line) and T74's gate
  (§9 gains a ratio-**order** line asserted against the composition, stating that the order holding
  says nothing about whether any individual size is right). This seat will author those amendments
  on request; they follow transcription rather than preceding it.
- **The TV lane:** the seven rulings; the before-set's changed question at T78; the §3.5
  re-derivations attached to T72 and T73, each **in the same commit** as the change that obliges it.
