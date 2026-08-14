# Register entries — 2026-08-11, batch 37

**Author:** Design Director · **Docket:** V6's gate-space arc (room `6e61c9d`, merged `9ab2cfe`),
concluded and routed with a recommendation room explicitly declined to rule · **Canon at authoring:**
266 rows, batch 36.

**Destination tables named per C43's standing practice:**

| row | destination table |
|---|---|
| V6 | **TV — match theater** (new) |
| C44 | **Cross-surface** (new) |
| T65-am2 | **TV — match theater** (folds into T65's row) |
| Row repairs — S23, S37, C8 | **in place**: SureThing ×2, Cross-surface ×1 |

---

## V6 — the gate reads a different quantity than its bounds describe

**Destination: TV — match theater.** New row; V6 has been referenced from T65 since batch 13 and
has never had one.

**Room's recommendation is ADOPTED: V6 measures in CIELAB via the room's existing shared
`linear_to_lab`. The bounds stay exactly as ruled — 85–92°.** The provenance work is accepted in
full; it is traced three independent ways and its conclusion is reproducible from source.

### Why convert the reading rather than re-derive the bounds

The bounds **are** the studio's palette figures — laptop lid 85.1–85.3° and the warm key tube ~92°,
both entering the record at batch 13 as this lane's own CIELAB measurements. Re-deriving them in HSV
would keep V6's code and orphan the numbers T65 clause 4 cites, while leaving every room palette
figure in the studio permanently incomparable with V6's output. Under C33-am3 — *the three ladders
are never compared* — a gate whose output cannot be compared with the palette it polices is a
standing defect, not a local one. **The reading is the half that moves.**

**Naming the space is necessary and not sufficient.** Room is exactly right: a label makes the
mismatch visible without making the bounds correct. Do both — convert, *and* name the space on the
print line. The asymmetry inside the file is the tell: line 91 already names its luma unit (*"C33's
unit: Rec.709 luma on display-encoded values"*) and the hue line names nothing. **That asymmetry is
where the error entered**, and leaving it after a conversion invites the next one.

### Every historical V6 verdict is VOID — both directions

TV's *"every in-band verdict is suspect"* is right in direction and understates the consequence.
Room's self-test settles it: **feed the band the two colours it was derived from, and both fail it.**
A gate whose own reference points fall outside its own band is not mis-calibrated — it is reading a
different quantity.

And the regions are **disjoint, not shifted**: HSV 85–92° is CIELAB ≈124–128°, which is green. So:

- **In-band verdicts certify nothing.** They passed values selected from a disjoint region — the
  vacuous-green family again, now in its sharpest form.
- **Out-of-band verdicts are equally void.** Near-inversion means a rejected value may have been a
  correct warm one. Nothing was learned in either direction.

**This recovers work as often as it discards it** — C37's shape exactly, and the reason the ruling is
"void" rather than "failed".

**TV owes an inventory of V6 verdicts already acted on, both directions** (C18 §4.1 — an inventory
names its members). Room's scope note correctly disclaims this: it is TV's record, not room's. A bare
"we'll re-run it" is not the deliverable; the members are.

### Ownership, and the one thing that must not happen

`v6_room_region.py` is **TV's** file (added at `97350ae`); `linear_to_lab` is **room's**, in
`tools/room_gate_check.py`, and it is already shared by the emission instrument and the R23 cast.
Cross-lane, so ruled here with both lanes present (C20).

**The converter is shared, never forked.** A second implementation of `linear_to_lab` is precisely
how a two-space defect regrows, and it would be undetectable by the same self-test that caught this
one. Import it.

### The amplitude window is not independent evidence

`[0.78, 1.06]` and the *"130° at zero falling to ~45.5°"* trajectory sit in the same source comment
as the HSV 88.0° and are functions of the band. **Both are VOID as evidence and re-derive against the
converted gate.** They bound nothing today, and anyone citing them is citing an HSV derivation of a
CIELAB constraint. Room named this and declined to rule it; ruled here.

### The repair is NOT frozen — and that is the useful sequencing

T80-am froze T65's re-authored **value** because it moves rendered pixels inside the Phase T pair.
**V6 is an instrument: converting its reading changes no rendered frame, so no freeze attaches.**

That gives the orchestrator a free ordering: **repair the gate now, during the freeze, so that when
the freeze lifts T65's re-authored value is judged by an instrument that works.** The alternative —
lifting the freeze first and measuring against the broken gate — spends the after-set on a verdict
that would be void on arrival.

---

## T65-am2 — correcting batch 36's target (§1.5, this seat's)

**Destination: TV — match theater.** **The verdict is unchanged; the remedy was wrong.**

Batch 36 restated T65's band from *"~85–92°"* to **83–85°**, reasoning that its upper half bracketed
no measured member. **That was wrong, and V6's provenance is what shows it.** The band has two traced
anchors: **laptop lid 85.1–85.3° at the bottom and the warm key tube ~92° at the top**, both rendered
CIELAB, both from batch-13 records. **The key is the upper anchor.** The band was correctly drawn.
The restatement to 83–85° is **withdrawn**; the ruled band **85–92°** stands as written.

Cause, recorded: batch 36 read the family off room's T65 record, which lists the **screens** (lid,
phone, laptop) and not the key. The key entered the record one docket later, in V6's trace.

**Second error, same class as the one this seat warned room about in the same batch.** The band is a
**rendered** band; `roomSettlementWarm` is an **authored** constant. Batch 36 told the lane to author
to 83–85° — comparing an authored value against a rendered bound, which is the two-instruments error.
Corrected:

- **The authored constant is the knob. The ruled band 85–92° is checked on the RENDERED cast**, on
  TV's own box under R23 with `TvLight` disabled — the capture already owed, now carrying the check.
- **Nobody authors to the band's number and assumes it renders there.** The authored↔rendered gap is
  real and not uniform: 1.9° on the lid, 10.7° on the key.

**Unchanged and still in force:** the violation itself (125.7° sits 33.7° past the band's top, and is
outside the room's warm family in *both* spaces); the retirement of the provisional (88.0°, 0.9);
hue-only with chroma not re-opened; the firing half granted; and T80-am's freeze.

**What the pair of errors actually argues.** Batch 36's finding that the *verdict* is independent of
V6 was sound and stands. Room's hedge was aimed at the verdict, and on that it was over-cautious. The
coupling that mattered ran to the **remedy** — and neither seat named it. That is the lesson worth
keeping: *a coupled docket can be independent for the verdict and dependent for the fix.*

---

## C44 — a bound and the reading it judges must come from one instrument

**Destination: Cross-surface.** New row. Register-level law, C43's standing — not a constitutional
amendment.

**A bound and the reading compared against it must be produced by the same instrument, in the same
stated space. A gate demonstrates this by printing both.**

*Founding case:* V6 — bound in CIELAB, reading in HSV, the two regions **disjoint**, and both of the
gate's own founding colours failing its own band. Every verdict it ever issued was void in both
directions, and nothing in the pipeline was positioned to notice.

*Second and third catches, inside one fortnight:* this seat's own batch-36 T65 target (an authored
value ruled against a rendered band — T65-am2), and the room lane's twice-paid region-comparison
error, which room names in its own T65 record.

**Relation to C33-am3.** C33-am3 requires every measurement to state its space, and it governed
review prose. **V6 stated nothing, and nothing required it to** — the clause never reached a gate's
own output, which is where the comparison actually happens. C44 puts it there.

**The cheap test, promoted from room's own method:** *feed a bound the values it was derived from. If
they fail, the gate is reading a different quantity than its bounds describe.* It costs one run, it
needs no frames, and it is the only check in this family that catches a **disjoint** band rather than
a loose one — a shifted band still passes its own founding values, and a disjoint one cannot.

**Standing practice:** any gate carrying a numeric bound runs its founding values through itself once,
and records the result beside the bound.

---

## Row repairs — three rows render broken on the canon page

**Bookkeeping. No ruling text altered, no IDs changed.** All three pre-date batch 35; none came from
batches 35–36. Found by a cell-count check, which is worth running at each seating beside the row
count — a row with the wrong number of cells renders wrong, and C22 makes the rendered table the
canon a lead reads.

| row | defect | repair |
|---|---|---|
| **S23** (SureThing) | 12 cells — the ruling quotes an enum, `` `PENDING \| RIDING \| …` ``, whose literal pipes split the row. **The ruling text visibly truncates at "amended to `PENDING"** and the rest scatters into phantom columns. The worst of the three. | pipes escaped `\|`; **zero words changed** |
| **S37** (SureThing) | 7 cells — a stray `\|` splits the Batch cell in two, mid-amendment | separator replaced with the register's own `·`; **zero words changed** |
| **C8** (Cross-surface) | 5 cells — a missing `\|` merges Item and State into one cell | split restored at the natural boundary: Item *"Bloom floor — risk/pays in the protected set"*, State *"**amended** · DD 2026-07-31 …"*. **The only one needing an editorial call**, recorded as such |

---

## Row count after batch 37

266 + **V6** + **C44** = **268.** T65-am2 folds into an existing row and adds none.

Sections after transcription: SureThing 79 · TV 88 · Room 48 · Cross-surface 45 · Phone 8 = **268.**
