# Register entries — 2026-08-14, batch 68

**THE STRIP SLOT AND THE TICKET ROW — the three ruled together**, as batch 66 said they must be.
Ruled at the DD seat against `dd-import/tv-goalless-draw-2026-08-14/` and the build at `tv-sweat`.

**Destination table: TV — match theater.** **Rows shipped:** `T96` (ruled AND fixed) · `T97` ·
`T87-am` (owed item DISCHARGED) · `T70-am`. **Recorded:** `S76` approved by Allen.

---

## 0. Why the three are one ruling — the frames say so literally

**`scene002` frames 000–030 are the same 31 frames three times over.** They are the window where the
match's ending should be stated, they are the window a goal line occupies instead, and they are the
window that ends when `LEG 1 — DEAD` displaces it. **T97's defect and T87-am's owed statement are not
adjacent — they are the same slot in the same 31 frames**, and T96 is the ticket row that same
settlement writes.

**Scene002 is also the proof the statement is genuinely absent rather than played before the
capture.** `scene001` shows its leg grade already up at frame 000, which alone proves nothing — but
**scene002 gives 31 frames of the strip between the whistle and its leg grade, and no full-time
statement appears in them.** The window exists, it is long enough, and it is occupied by a falsehood.

---

## T96 — RULED, AND FIXED IN THIS BATCH. The deck was the defect, and the deck is mine.

**The diagnosis in batch 66 was right about the string and wrong about the blame.** The build is
**faithful**. `LegStatement()`'s Moneyline branch is a two-way `pickedHome ? Home : Away` **because
that is exactly what the copy deck told it to build**:

> `| Moneyline | {CLUB} TO WIN | {CLUB} ML |` — one row, two-way, no draw case.

**`tv-g1-authored-leg-statements-2026-08-08.md` was written 2026-08-08. S74 authored the draw's forms
2026-08-12. The deck was never amended.** It contained **zero occurrences of the word "draw"** until
this batch. **§8's claim that the draw's forms *"are authored and live with the rest"* was false
against the file §8 itself points builders at.**

**This is a DD-owned file, so this batch fixes it rather than filing it.** Amended:

| MarketKind | NEED (live) | compact |
|---|---|---|
| Moneyline · Home/Away | `{CLUB} TO WIN` | `{CLUB} ML` |
| **Moneyline · Draw** | **`LEVEL AT FULL TIME`** | **`DRAW`** |

Plus the progress pair `LEVEL` / `NOT LEVEL` (S74, unchanged), a fallback row for
`LEVEL AT FULL TIME` (18 chars, at the NEED budget — same class as `ONE TEAM SCORELESS`; authored
shorter line **`LEVEL AT FT`**, `FT` being **this surface's own clock token** rather than jargon), and
an explicit note that **`DRAW` needs no fallback** at 4 chars against a ~19-char budget.

**The lesson, recorded because it is the reusable half:** a ruling that authors copy is not landed
until **the artifact the builders read** carries it. **S74 was ruled, folded into the owning doc, and
still shipped a defect — because the deck sat between the doc and the build and nobody amended it.**
**A copy ruling lands in the deck or it has not landed.**

---

## T97 — RULED. The words are licensed by the RESOLVED SCENE, never by the beat's type — and the remedy already exists.

**The mechanism, named exactly.** `RenderEvent` stashes `_pendingFlavor = flavor`; `RevealBeatChrome`
lands it into `_tFlavor` — **the event strip, confirmed at `AtTier(ink, TierL2)`, T66's own tier.**
The line comes from `SweatFlavor.Line` → `Base(e.Type, …)`, selected by **the beat's TYPE**, and
`"{other} on the board; the slip flinches."` is a member of **`EventText.ScoreDown` — the
opponent-scored family.** So a beat typed `Score` prints a goal sentence **whether or not the resolved
scene contains a goal**, and on this match none ever did.

### The remedy is not new copy. It is an existing guard, applied to one family and never to the other.

**`SweatFlavor.NeutralLine` already exists and its doc comment is this exact defect, one market family
over:**

> Ordinary-play line for a count-market beat whose resolved scene carries no count event (a zero
> batch fell through) — **corner/booking words would be a lie there** (Sol, F_0.4.0 P3 r2). Plain
> possession language, direction from the beat.

**The orchestrator already overrides with `NeutralLine` when the resolved scene carries no count
event.** **The goal families never got the same override.** The class was found, ruled and fixed for
corners and cards; **goals were left on the beat's type alone**, and that is the whole of T97.

### RULED — the law, stated generally because this is its second instance

**A beat's WORDS are licensed by what the resolved scene actually contains, never by the beat's type
label alone.** Where the scene does not contain the event the words assert, the authored neutral line
stands in. **This is Sol's F_0.4.0 P3 r2 finding generalised from counts to goals**, and it is the
same sentence with one noun changed.

**Scope: every family that ASSERTS A GOAL** — `ScoreUp`, `ScoreDown`, and the parts of `BigUp` /
`BigDown` that finish (`walk it in`, `break the line and score`, `tear away and finish`). **The lead
sweeps those four arrays string by string and reports which assert a goal and which assert only a
dangerous move** — this seat is not claiming to have audited every line, and the ones that assert
only danger are correct as they are. **The `NearMiss` overrides are already right** and are the model:
they assert no goal and are used where none occurred.

**Not ruled here: whether this instance was a stale carry or a fresh mis-selection.** The lead
diagnoses and reports. **Both are the same defect under the law above and neither changes the
remedy** — a stale line is also a line the resolved scene does not license.

---

## T87-am — THE OWED L2 STATEMENT, AUTHORED. This discharges batch 66's owed item.

**`THE MATCH ENDS LEVEL`**

Fires at the whistle of a **drawn** match, into the event strip at L2 (T66), and holds until the
leg's own grade displaces it — **the sequencing that already exists; the 31-frame window is not
shortened to make room.**

### Why this line and not the obvious one

**`FULL TIME — LEVEL` was the obvious form and it is REFUSED.** The scorebug prints `FT` in the clock
slot directly above the strip. **Putting `FULL TIME` in the strip states the same fact twice, one slot
apart** — §8 forbids the strip duplicating the score, and duplicating the *clock* is the same error
with a different neighbour. **The strip's job is to say what the score and clock cannot.**

**It takes the shape of the surface's own beat statements** — `THE BOARD IS SET`, `THE TOTEM BURNS` —
uppercase, `THE {subject} {verb}`. **Uppercase is not a choice here: every authored line in
`_tFlavor` is caps** (`VAR — NO GOAL`, `THE SLIP COMES OUT — LEG VOIDED, THE TICKET LIVES`,
`LEG {k} — WON`). The sentence-case lines in that slot come from the `EventText` path alone.

**It reports and does not editorialise** (T87 §6.8's own words). No hype, no exclamation, no
superlative, no second person, one casing, no dash to misuse. **`LEVEL` is this surface's word for a
tied scoreline** (T62, S74) — nothing invented, and the same word the draw-backer's progress line
uses, which is the one-name-per-thing convention working as intended.

### It is the DRAWN match's line, not the goalless one — and that is the point

**Nothing here is 0–0-specific.** `THE MATCH ENDS LEVEL` is true at 0–0 and at 2–2, and **authoring a
goalless-only line would be exactly the narrowing T87 §6.8 forbids** — *nothing here may be narrowed
to exclude a goalless match* cuts both ways, and a line that only exists at 0–0 makes 0–0 a special
case again.

**Why only a DRAWN match needs an authored ending line, stated because it is the real mechanism:**
**a decided match ends ON a goal, so its final beat's line IS its ending and the strip is already
correct.** **A drawn match ends on nothing**, so the last beat's line is stale by construction — there
is no closing event to speak. **The strip's silence at a draw is structural, not incidental**, which
is why T87 assigned it a statement and why no other result needs one.

---

## T70-am — "NO TERM REPEATED" GOVERNS THE SUBJECT, NOT THE PREDICATE

**Raised because amending the deck forced it.** S74's pair is NEED `LEVEL AT FULL TIME` over progress
`LEVEL` — **`LEVEL` appears in both**, and T70 reads *no term repeated across the two.*

**RULED: no breach. T70 governs redundant IDENTIFICATION.** Its own example is `LANYARD TO SCORE` over
`WAITING FOR LANYARD` — **a name printed twice** — and T70 calls it *"T69's defect turned vertical"*,
where T69 is **the backed team printed twice.** Both are the subject.

**A binary state answering its own requirement in the requirement's word is not redundant
identification — it is the progress line doing its only job.** Forcing a different word below would
put a second name on one thing and break the one-name-per-thing convention T62 established. **The
cure would be the worse defect.**

---

## S76 — VOID row: APPROVED (Allen, relayed 2026-08-14)

The candidate treatment ruled in batch 67 is **approved**: the entry **rubbed out**, **`VOID`** printed
as a word (S22), the stake printed as a **known sum**, **never the oxide strike**, and the row **not**
drained to `DEAD`'s .55. **State: Approved → the screen lane builds it. Design-verified still needs
frames** (C11) — approval is of the spec, and the read is checked when a voided ledger row exists.

---

**Build order, because two of these land in one slot:** the strip takes **T97's guard and
`THE MATCH ENDS LEVEL` in one change**; the ticket row takes **`DRAW` from the amended deck**. **Three
rulings, two touches, one capture to verify all of them** — a goalless draw to full time with both
tickets, which the harness can already produce.
