# Register entries — batch 38

**Design Director** · 2026-08-12 · docket: TV's tabular-cannot finding (`dd-import/tv-tabular-cannot-2026-08-12.md`, tree `cb84278`)

**Destination tables** (C43's standing practice — every row names where it lands):

| row | table |
|---|---|
| `T82` | **TV — match theater** (new) |
| `T80-am2` | **TV — match theater** (amends T80) |
| `T75-am3` | **TV — match theater** (amends T75) |
| `T74-am` | **TV — match theater** (amends T74) |
| `G1-am2` | **TV — match theater** (amends G1) |
| `C45` | **Cross-surface** (new law) |

---

## The measurement is accepted in full, and it corroborates harder than the docket claimed

Three instruments, two of which share no machinery: an in-engine digit probe, `hmtx` advances read
through `cmap` with no editor involved, and a GSUB feature enumeration. The finding survives its own
editor crash *because* of the second one. That is the discipline to copy, and it is named here as
such.

**Checked independently at this seat, and the agreement is exact.** The docket reported the
file-versus-engine gap loosely — *"≈87 px against the 93 px measured, the difference being the
instance"* — and left it as a hand-wave. It is not a hand-wave. Predicting each measured spread from
`hmtx` alone and taking the ratio:

| face · size | predicted from `hmtx` | measured in engine | ratio |
|---|---|---|---|
| Regular 36pt | 87.12 px | 93.25 px | **1.0704** |
| Regular 28pt | 67.76 px | 72.53 px | **1.0704** |
| Condensed 29pt | 70.76 px | 70.62 px | **0.998** |
| Condensed 19pt | 46.36 px | 46.27 px | **0.998** |

**The ratio is constant across sizes within each face, to four figures.** A constant per-face ratio
is the signature of a uniform instance scale — which is exactly the cause the docket named. So the
two instruments do not roughly agree; they agree *exactly*, up to one scale factor per face, and
that scale factor **is** the instance. Condensed renders its file default (1.000). Regular renders an
instance about 7% wider in advance than the file's default.

The proportionality is therefore over-determined, not merely corroborated. It is not in doubt.

**C44-clean.** The two instruments are reported as independent corroboration with the residual named,
never as a bound judging a reading. This is the correct use of the clause ruled one batch ago.

---

## T82 — the mandate stands; the route is the atlas's own tabular glyphs

### Classification first (C16), because the headline and the body disagree

The docket's title says the mandatory tabular figures **cannot be reached**. Its body says something
much narrower and correct: *"unreachable by any in-scope choice"*, *"no face assignment available
inside Phase T"*, and — decisively — *"closing it requires a decision about how the glyphs are
obtained rather than a line of code."*

That last sentence **is C16's classification, stated by the lane**, and it is adopted as this
ruling's own reasoning.

- **`tnum` at runtime is IMPOSSIBLE.** TextMeshPro's `OTL_FeatureTag` declares `kern`, `liga`, `mark`
  and `mkmk`. There is no `tnum` to enable and no rich-text tag or component property exposing one.
  That is the platform, and platforms are what make things impossible.
- **Tabular figures on this surface are POSSIBLE.** The docket's own option (a) reaches them. The
  glyphs are drawn, present, and addressable at build time.
- Therefore, under C16, this is the **expensive** kind, not the impossible kind. **No signed
  deviation is issued and none is needed.** A signed deviation here would buy a permanent exception
  to a live mandate in exchange for work the lane has already scoped.

The headline overshoots the body. Recorded as a classification correction and nothing more — the
lane surfaced the refuting route itself and recommended it, which is the opposite of a finding that
hides its own escape.

### The mandate is NOT amended

`tv-design.md` §4 stands unaltered: *"Tabular numerals are mandatory."*

- The twitch is not marginal. Ten digits spread **46–93 px** at the sizes this surface renders
  numbers. A score, a clock and a money figure change on the tick.
- T11 selected Encode Sans **on this criterion**, with Saira disqualified for lacking `tnum`.
  Amending the mandate now would retroactively decide that the criterion the face was chosen on does
  not matter — and would leave T11's comparison resting on a property the studio had abandoned.
- A faithful route exists. C16 does not let convenience reclassify itself as impossibility.

### Option (b), `<mspace>` — REFUSED

Forcing one advance for every character imposes a metric **the family does not contain**. That is
§4's *no synthesised styling on this surface* in the spacing channel: a synthesised bold is a smear,
a synthesised italic is a shear, and a synthesised monospace is a third of the same thing. It also
applies to letters in any mixed string — `CASH OUT $183` is the money control, the element that cost
four batches at T63/T68/T68-am/T71. Monospacing its label to fix its figures is C10's shape exactly:
a wrong mechanism tuned toward looking acceptable.

### Option (c), amend the mandate — REFUSED, and held as the named fallback

Refused for the reasons above. **Its condition is pre-committed now, before the build attempt**, so
the disposition is not decided after seeing the result:

> If option (a) proves unbuildable — the substitution cannot be resolved at generation time, or the
> tabular glyphs are not addressable in the generated set despite their presence in GSUB — then the
> classification changes to **genuinely impossible**, and option (c) returns as a **C16 signed
> deviation carrying a named cost and an expiry**. Not before, and not on difficulty.

### Option (a) — GRANTED as the route

The atlas ships **the font's own tabular digits** as its default set: the substitution is resolved at
build time and U+0030–0039 map to the tabular glyph indices. These are the figures the type designer
drew. It is not invented spacing, which is what separates it from (b).

**The ruling is the outcome, not the mechanism.** Two implementation routes reach it — the Unity
generator resolving the substitution, or a pre-processed static instance with the feature frozen,
committed as the atlas source. The second keeps the transformation in a re-runnable script with named
constants. **The lane picks**; this seat is not choosing a build method.

Four conditions on it:

1. **The unit is the ASSET, not the slot.** T75-am established this and the docket confirms it one
   level down: the property lives on the font asset, and a static TMP asset bakes one set. So —
   **every generated asset that any figure slot renders on takes the tabular set, and the inventory
   names its members** (C18 §4.1). Not "the atlas". The condensed Bold 700 asset carries `CashOut`,
   the surface's money figure; an inventory that names two assets and misses a third ships a control
   that still twitches.
2. **Digits alone satisfy the mandate.** Digits are the characters that change; `$`, `:` and
   separators are constant and need nothing. The mandate's content is **equal advance among digits**,
   not constant string width — a score going 9 to 10 adds a character and widens, and that is layout
   (T75-am2's right-anchor), never a tabular failure.
3. **One variable.** `lnum` is also present in GSUB. Figure *style* does not change in this commit;
   tabular only.
4. **§3.5 binds the same commit.** A width change obliges the layout depending on it to be re-derived
   in the same commit. Where a zone cannot hold the widened figure, that routes to T74 — it is never
   tuned inside the tabular commit.

**Licensing: no new question.** Same OFL 1.1 files, already subsetted into SDF atlases; applying a
feature at build time is the same class of operation on the same files. Informational, since T11
recorded a licence basis.

### A free by-product, routed not ruled

The constant per-face ratio above is a **width-axis fingerprint of the rendered instance**, obtained
without frames. T78-am owes exactly this kind of desk measurement. It is routed, with two bounds
stated so nobody over-reads it:

- **Advance alone cannot separate weight from width.** A heavier instance at one width is wider too.
  T78-am asked for stem width *and* width axis for this reason; this supplies one channel.
- **Which asset `cb84278` loaded decides what the number means.** At T-5 the canon faces may already
  be wired, in which case the ratio describes the migrated instance and not the legacy base T78
  asked about.

Lands in the deferred sizing pass with the rest of T78-am, not in Phase T.

---

## T80-am2 — the freeze list gains a sixth item, and this seat created it again

Option (a) changes rendered digit widths on slots the pair measures — `Score`, `Clock`, `CashOut`,
`RiskPays`, `LegRowPrice`, and every count. That is a change to what the after-set is measuring.

**Ruled: T82's tabular change is FROZEN from the before-set to the after-set on T80's terms.** Lands
in between, the pair is void and re-shoots.

**Costs nothing** — TV sequenced (a) after the pair itself, correctly, and the after-set is shooting
now. It is stated because the window is not theoretical: this set is already a re-shoot after a
crash, and a second failure re-opens exactly the interval in which an eager fix could land.

**This is the second instance of one catch, one batch apart.** TV's submission is a **measurement**;
measurements move no pixels and the freeze was respected by construction. It becomes a frozen
variable **at the moment this seat rules it**. That is precisely what happened at T80-am with room's
T65 cast measurement — *"true of a MEASUREMENT; it stops being true the moment the measurement becomes
a RULING"*. Two lanes, two dockets, same shape. Promoted at C45.

---

## T75-am3 — the `Clock` carve-out, and a defective pre-commitment recorded as this seat's (§1.5)

T75-am2 pre-committed both branches of the `Clock` test:

> left edge invariant within a group → carve-out discharges;
> left edge moves → **regular is wrong for this slot regardless of how the default was reached**.

**Both halves are defective, and the docket is what shows it.**

- **Branch 1's condition is unreachable.** No face available on this surface produces an invariant
  left edge — both are proportional at ~0.24 em. The instrument could not have shown the success it
  was looking for. **C37's shape, applied to the positive branch**: a test whose pass condition is
  unreachable certifies nothing by failing.
- **Branch 2's conclusion does not follow.** The left edge moves under regular *and* under condensed.
  The observation cannot discriminate the face assignment, which is the only thing branch 2 concludes
  about. Moving `Clock` to condensed would have bought a different proportional face and called it a
  fix.

Recorded under §1.5 as this seat's error — the ruling is the defect, not the lane, which routed the
question rather than quietly answering it.

**Corrected disposition:** the left edge moves → **the mandate is unmet by the STACK, not by the
slot's face assignment.** `Clock` stays **Regular**, on §4's own reasoning that regular is the face
the mandate wants. The tabular property is delivered at the asset (T82) and never by a face swap.

**The carve-out's question is answered here, not on the after-set.** Three measurements settle
whether `Clock` renders tabular: it does not, and cannot on the shipped stack. The after-set frames
become **corroboration**, not the deciding instrument.

**The owed harness clock-string emission is NOT retired — it is re-pointed.** Batch 34 owed it so the
carve-out could be tested by subtraction instead of by glyph segmentation (which bloom confounded).
It now becomes **the acceptance test for T82's fix**: within a set of equal-character-count strings in
a right-anchored slot, the left ink edge is invariant iff the digits are tabular. Same instrument,
same subtraction, now verifying the remedy rather than deciding the carve-out.

**`BigAmount`:** T75-am's shared-asset invariant does the work a second time — the fix lands on the
asset, so any slot on that asset inherits it with no per-slot work. Moot in practice until T79
resolves, since the element renders nothing today.

---

## G1-am2 — the fit certification faces a SECOND geometry change

G1-am voided the batch-27 fit grant under C18 §4.1 and set it to re-certify on the after-set. The
after-set is shot with **proportional** digits. T82 then **widens** digits.

Several authored forms carry figures — Total Goals, Total Corners, Total Cards, and the price and
money forms. Those strings face a second geometry change *after* the re-certification that was meant
to close them.

**Ruled:** G1's after-set re-certification is **final for figure-free strings** and **provisional for
any string containing a digit**; the latter re-certifies once more after T82's assets land. Same
clause as before — a gate certifies the geometry it ran against.

**Remedy unchanged and still outside Phase T:** an overrun is answered by the span or the size and
routes to T74. **Never the copy** (§4 / T24-am). The authored deck stands.

Stated so nobody reads the after-set grant as closing the whole deck. This is the third geometry
event on one certification.

---

## T74-am — the deferred sizing pass now has a named input list

T74 ruled the size-authority conflict is not settled inside Phase T. It has accumulated inputs
without ever naming them, which is C18 §4.1 on a work item rather than a gate. **The list, closed as
of today:**

1. **Phase T's rendered-extent deltas** — point size held, extent moves; T81 ruled that is the
   measurement, and `TvTypeParityProbe.cs` is the instrument that reports it, not a knob.
2. **T78-am's base-instance question** — plus the width-axis fingerprint routed from T82 above.
3. **T82's digit-width deltas** — every figure slot on every rebuilt asset.
4. **Any G1 overrun** surfaced by G1-am2, carried as a span-or-size question.

Anything that joins later joins by amendment, named.

---

## C45 — a freeze is checked against the seat's own OUTPUT, not only the lane's input

**Law, register-level.**

Where a freeze protects a verification pair, a routed **measurement** respects it by construction —
measurements move no pixels, and a lane reporting one is correctly freeze-clean. **The ruling that
measurement produces may not be.** The freeze check therefore runs at the moment of ruling, on what
the ruling causes to change, and **it is this seat's to run, because the lane cannot see it**: from
inside the lane the submission is frozen-clean and stays that way right up until someone rules on it.

**Two founding cases, one batch apart, two different lanes:**

- **T80-am** — room's T65 settlement-cast measurement. Room stated the freeze was respected; that was
  true of a measurement and stopped being true when the measurement became a ruling that re-authored
  a value inside the pair's own frames.
- **T82 / T80-am2** — TV's tabular measurement. Identical shape: freeze-clean as a finding, a frozen
  variable the moment option (a) is granted.

**Standing practice:** any ruling issued while a verification pair is open states, in the ruling,
whether it creates a frozen variable.

**Not proposed for the constitution** — register-level, on C39/C42/C43's standing. Two independent
founding cases is what makes it a law at all; a third argues for promotion to §4, beside the other
instrument clauses.

---

## Falsification, pre-committed before the evidence was read

The Phase T after-set landed at `dd-import/tv-phase-t-after-2026-08-12/` while this batch was being
transcribed, and it **carries `clock-strings.tsv`** — the harness instrument owed since batch 34, now
built. This ruling makes a checkable prediction about that file, so the prediction is stated **before
it is read**. At the moment of writing, this seat has **counted the set's files and opened none of
them.**

- **Predicted:** within a group of equal-character-count clock strings in the right-anchored `Clock`
  slot, the **left ink edge MOVES.** That is what proportional digits mean, and it is what T82
  asserts. Observing it corroborates T82 and T75-am3 and closes the `Clock` carve-out on rendered
  evidence as well as on the files.
- **Falsifier:** if the left ink edge is **invariant** within an equal-character-count group, T82's
  premise is wrong on the rendered surface and **the whole ruling re-opens** — the atlas grant, the
  owning doc's delivery clause, and T75-am3's corrected disposition together — because the shipped
  stack would be reaching a tabular advance by a path none of the three measurements found.

This is not a caveat. It is a stated way for this ruling to be wrong, against evidence already on
disk, written down before anyone looked.

---

## Sequencing summary, for the orchestrator

- **Nothing here blocks the after-set.** The freeze is unchanged in practice; the pair proceeds.
- **T82's build lands AFTER the after-set is shot.** Not in Phase T — T81's variable is a closed list
  of three and C43-am(3) forbids anything joining it afterwards.
- **The gate repair (V6) and this fix are both freeze-compatible work** — V6 because an instrument
  moves no pixels, T82 because it is sequenced after the pair.
- **No Allen stop.** The mandate is upheld, not changed; no scope, licence or irreversible call. The
  font-selection basis he has seen (T11) is *preserved* by this ruling, which is worth one line in
  his next status but is not a decision waiting on him.
