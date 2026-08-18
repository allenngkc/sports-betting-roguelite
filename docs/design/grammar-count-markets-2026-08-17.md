# The count-market grammar — direction

**Written:** Design Director seat, 2026-08-17 · **Mandate:** `dd-mandate-2026-08-16.md` Phase 2, the
grammar direction named as develop-meanwhile · **Status:** EXPLORATION. No spec, no ruling, no
material choice taken.

**Reveal-agnostic by construction.** Everything below holds under any answer to the reveal question
(`T109`). §9 names the one place it forks.

**Supersedes the mechanism paragraph of `count-sweat-read-2026-08-16.md` §2.** The read's conclusion —
*the corners arm has no resting state* — stands and is confirmed. Its implied mechanism (a token that
persists) is wrong, and the true one is both simpler and more actionable.

---

## 1. THE MECHANISM, found at source

`count-sweat-read` §2 measured `CornerFor` across fourteen consecutive windows and inferred the token
was latching. It is not. **`CalmPossession` is never consulted.**

`TheaterChoreographer` takes the count branch **first**, before the base scene table:

- On a `TotalCorners`/`TotalCards` leg, any non-`LegFinal` beat calls `countLedger.StageBeat()`.
- If the staged batch has **`TotalDelta > 0`**, it returns a `CornerFor`/`CornerAgainst` scene and
  **returns** — short-circuiting the table below.
- `CalmPossession` is produced **only** in that table, on a `Momentum` beat tagged `Calm`.

**So calm does not lose a competition on a corners ticket. It is unreachable on any beat that stages
a count at all.** The goals arm reaches the table freely because the count branch does not apply to
`TotalGoals` — which is exactly why the control has a contour and the corners arm does not. **The pair
isolated a real difference and this is what it is.**

**And the gate is `TotalDelta > 0` — arrival, with no test of significance.** Corner #1 and corner #4
take the identical path and produce the identical template.

## 2. THE NUMBERS — the treatment is flat across a distance that is not

From the capture's own log, against the line of **8.5** (counts are `HOME-AWAY`; the total is the sum):

| # | clock | total | distance to the line | treatment |
|---|---|---|---|---|
| 1 | 8' | 2 | 7 away | `CornerFor` |
| 2 | 20' | 4 | 5 away | `CornerFor` |
| 3 | 30' | 6 | 3 away | `CornerFor` |
| 4 | **43'** | **8** | **one short — THE APPROACH** | `CornerFor` |
| 5 | **53'** | **10** | **CROSSED — the leg is won** | `CornerFor` |
| 6 | 65' | 11 | decided | `CornerFor` |
| 7 | 76' | 12 | decided | `CornerFor` |

**One treatment across a distance running 7 → 5 → 3 → 1 → crossed → decided → decided.** The
theater had no ramp where the market's whole tension is a ramp.

**And the copy makes it worse in the two places it could least afford to.** From the same log:

- **#4, the approach**, printed `corner kick won. another little number for the ledger. (2 in the
  spell)` — **verbatim the line from #1**, the least consequential event of the match.
- **#5, the crossing — the moment the bet was won** — printed `the flag goes up; pressure becomes a
  corner.` — **verbatim the line from #2.**

**The two decisive events of the watch were narrated with recycled openers from the two that mattered
least.** `count-sweat-read` §3 recorded the repetition; this is *which* events took it, and it is the
worst possible assignment.

*Checked, and NOT a defect:* `(2 in the spell)` is `spec.Count.Value.TotalDelta` — truthful. Five of
the seven events carried a delta of **2**, so the watch delivered **twelve corners in seven events**.
Recorded because it is opaque copy that reads like a running total, and because a step of 2 can
**jump the line without ever landing on it** — the count went 8 → 10 and never sat at 9.

## 3. THE DIRECTION — buildup is SPENT, not automatic

**An event earns its treatment from its distance to the line, not from having arrived.**

This is `count-sweat-read` §2's finding and the exploration's §2 rate line arriving on the **scene
grammar** rather than on a line of text — and it is the cricket steal in its proper place. A required
run rate is a **continuous quantity**; so is this. **A ramp, not a switch.**

**What makes it cheap: the answer already exists elsewhere on the same surface.** The ticket column
printed `8 CORNERS • NEED 1` at 48', derived in `SweatActiveLegModel` as `threshold - total` from
revealed values. **The theater can ask the question the column already answers.** Nothing is invented,
nothing is computed twice, and — per `T108`'s own standard — it reads the **revealed** count, never
the locked target.

**The resting state is not authored. It is what remains.** Stop spending buildup on corners that
carry none, and the quiet stretches appear by themselves — out of scenes that already exist and
already play.

## 4. WHAT ONE RULE BUYS — three findings, one change

1. **The missing resting state** (`count-sweat-read` §2). Low-distance corners stop pre-empting the
   base table, and calm becomes reachable.
2. **Undifferentiated events** (§2 above). The approach and the turn become the only two moments that
   get weight, which is what makes them read as moments.
3. **The corpse stretch.** `CornerFor` held for four windows after the leg was won at 53', and the
   token did not flip until `scene016` — **roughly 20 of the sweat's 44 seconds elapse after the
   ticket can no longer lose.** A resolved leg's remaining corners have **no distance to any line**,
   so they earn nothing under the same rule. **The corpse-narration stops as a consequence rather
   than needing its own fix**, and exploration §9's second owed item — *confirm on frames what a
   decided leg does for the rest of the match* — is now **discharged on the token stream**.

## 5. THE BINDING — a quiet corner MUST STILL COUNT

**Named first because the obvious implementation ships a counting bug.**

`StageBeat()` **advances its cursor unconditionally** — it consumes the batch on the beat it is
called. `CompleteCount` is called from `OnCountPlayed`, **the scene's payoff callback**. So a beat
that takes a batch and then falls through to a calm scene **consumes the count without committing
it**: the column stops tracking, and the match ends short of its own total.

**RULE, at design level: no beat may consume a count batch without committing it.** A corner that
earns no scene is still a corner — **the count is a fact and only the drama is discretionary.** How
that is arranged is the lane's call; that it must hold is not.

**This inverts the cost of the change.** On its face it is one gate; in truth it is one gate plus a
commit path that does not currently exist off the scene payoff. Stated now so it is budgeted rather
than discovered.

## 6. ALREADY BUILT — do not respecify

- **Valence off the ticket is DONE.** Exploration §4 proposed that no count beat take its register
  from the event. `TheaterChoreographer` already sets `countHelps` from
  `leg.Selection.Choice == MarketChoice.Over`, and `ScoreLedgerTests` asserts it in terms —
  *`CornerFor`/`CornerAgainst` is the bettor's MOOD, not team*, and mood must never drive routing.
  **The ruling exists, is built, and is gated. §4 is withdrawn as a proposal.**
- **Calm scenes exist and play.** `CalmPossession` is a live template with its own pacing
  (`SweatPacer.calmSeconds`) and is explicitly excluded from buildup. Nothing needs authoring.
- **Zero batches already fall through** — *"a zero batch stages NO count event; the beat falls
  through to ordinary play"*. **The fall-through path this direction widens is already there and
  already correct.**
- **The UNDER's win by absence** is `T97-am`'s: the strip's words are licensed by the **resolved
  scene**, never the beat's own moment. Exploration §6 stands; do not re-derive it.

## 7. CARDS REMAIN THE OPPOSITE PROBLEM

Unchanged from exploration §3 and **untouched by any of the above**: a booking arrives carrying its
own significance and needs **catching**, not ramping. Distance-to-line is the wrong instrument for a
market whose line is 4.5 and whose events are worth one each with real drama attached to every one.

**No cards arm has been shot.** Everything here is derived from a corners set, and §2's delta-of-2
batching may not hold for cards at all. **Booking drama is not addressed by this direction and its
absence here is not evidence about it.**

## 8. WHAT MUST BE CHECKED BEFORE THIS BECOMES A SPEC

**One item, and it is the `T108` lesson applied to my own proposal.** I have read the decision path;
I have **not** verified that `Momentum` beats tagged `Calm` are actually scheduled during a corners
sweat. If the drama stream for a corners leg never emits them, widening the gate yields `Territory`
or `Fallback` — **not calm** — and this direction would deliver a different watch than it promises.

**Naming a reachable branch is not checking that anything reaches it.** That is exactly the error
`T108-am` recorded three days running, and it is unproven here. **Settle it before speccing.**
> **DISCHARGED — 2026-08-17, by measurement (`T113`, batch 105).** The calm-beat probe answers
> this section and refutes both fears.
>
> - **Calm beats ARE scheduled: up to SIX OF EIGHT** beats in this sweat are calm beats already
>   being spent. **And the stream is innocent** — the goals and corners arms are beat-for-beat
>   identical, probabilities within thousandths. The flatness is **entirely presentation routing.**
> - **`Territory` can never occur on a momentum beat** — `Swing` needs Δp ≥ 0.10, `Momentum` caps
>   under 0.07: **mutually exclusive by arithmetic**, so it holds for every seed. The **stronger**
>   of the two refutations.
> - **The fallback arm is unreachable today** — true of *today's* config, so the **weaker** one; it
>   would not survive a config change unexamined.
>
> **One wording correction, and it changes the fix:** the count branch **returns before the base
> table runs**, so `CalmPossession` is never constructed. Nothing is *overwritten* — **the calm
> branch is never reached.** The beats are tagged `Calm` by the stream and rendered `CornerFor` by
> the routing. *Overwritten* invites computing both and preferring calm; the actual change is
> **gating the count branch's entry.**
>
> **Still not established, and §5 is untouched:** the probe measured **scheduling only**, one seed,
> one line, with `ResolveBeat`'s interception left alone — which is exactly what this direction
> changes. It proves the calm beats exist and are spent; **it does not prove that reclaiming them
> yields a good watch.** That is a `C11` frame claim awaiting a capture.


## 9. WHERE THIS FORKS ON ALLEN'S REVEAL ANSWER

**It does not change, but its weight does.**

- Under **A/B** (the scoreline is shown), goals become departures too, and the corners watch gains
  `calm → goal → calm` contours **on top of** what this direction supplies. The two compose; neither
  substitutes for the other.
- Under **C** (no change), **this is the only available source of contour** on a non-goal ticket, and
  it becomes correspondingly more important.

**So it is worth developing under every answer, and it is most needed under the answer I did not
recommend.**

## 10. NOT CLAIMED

- **No frame has been read for this document.** §1 and §5 are source reads; §2 is the capture's own
  log. Under `C11` nothing here is a claim about how anything looks.
- **One seed, one line, one side.** An UNDER's distance profile is the mirror of this one and is not
  in evidence; a leg that lands *near* its line, or loses, is a different watch again.
- **No claim that the gate is one line of work.** §5 says it is not.
