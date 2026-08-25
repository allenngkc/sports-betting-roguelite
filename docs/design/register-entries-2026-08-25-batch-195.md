# Register entries — batch 195 (2026-08-25)

**The read. §2 is right, the ≥3 escalation is WITHDRAWN by my own pre-committed reading — and the
window's NAME is wrong for two kinds and ambiguous for a third, which TV routed as a ≥2 problem
and is live at N=1 today.**

**Two rows.** **Destination table:** TV (`T143-am8`, `T143-am9`).

**Build:** `d66710c`; handoff `6ede668`. **Five source reads. Nothing measured.**

---

## The rows

| T143-am8 | The pending-window build ACCEPTED, and the ≥3 escalation WITHDRAWN IN FULL by the pre-committed reading — with a gate owed on a 3.4px margin | **RULED — DD 2026-08-25 batch 195, on `d66710c`.** **§2 IS BUILT AS SPEC'D: the decline row absorbs the name, three rows at 82.5, and the decline never becomes the widest row — `HOLD R` stays at 523.8. **Batch 192 is honoured in BOTH halves**: `Short` is applied at THIS call site and never via `LegStatement`, and the three team totals take no special case and no repaired name. **The seat asked for a build to the ruling rather than to the build state and got exactly that.*** **READING (A) FIRED: two bare names plus the separator measure **631.6 against 635.0**. Batch 193 pre-committed this: **there is NO hole at any reachable N** — `MaxLegs` caps it at four — **`T143`'s *names every leg that died* stands UNAMENDED, and batch 189's escalation is WITHDRAWN IN FULL.** It is withdrawn by the number, not by my judgement of the number, which is the whole point of having written both readings down first.** **BUT 3.4px IS 0.5%, AND TV SAID IT PLAINLY: *any longer club noun entering the pool eats it*. **The withdrawal STANDS — re-opening a ruling because its margin is slender is exactly the goalpost-moving a pre-commitment exists to prevent.** What a slender margin needs is a GATE, not a second opinion: **when §3 lands it lands with a check that FAILS if the two-name row exceeds the zone.** A ruling resting on a measured margin owes a test that the margin still holds; `C29`'s shape, applied to a width.** **THE ≥2 BRANCH RENDERS A SUPERSEDED STRING, and it is owed rather than faulted: it prints `N LET IT DIE`, which `S85-am3` replaced with **`N GO ON`** twenty-one minutes before this commit. **§3 is an explicit TODO with no silent fallthrough and no part-built composition — the right way to leave it** — but the placeholder is now known-wrong and it does render, so `N GO ON` lands WITH §3 and not later.** **AND THE `T88` PIN RE-BASE IS ENDORSED: pinning the ruled-invariant `N LET` … `DIE` rather than a club name that varies with the seed is correct. **A pin that asserts a string the copy ruling changed is a pin testing the fixture, not the invariant** | batch 195 |
| T143-am9 | THE WINDOW'S NAME IS THE CLUB ALONE — which MISNAMES the scorer markets outright and is AMBIGUOUS on a same-match ticket TODAY, not at §3 | **RULED — DD 2026-08-25 batch 195, widening the item TV routed.** **WHAT IS BUILT: `PendingLegName` returns `MatchModel.AnchorSide`'s club, `Short`-ened — **and a qualifier for `Handicap` ONLY.** Every other kind renders the bare club.** **DEFECT 1, AND IT IS A MISNAMING RATHER THAN A COLLISION: `AnchorSide` answers for `AnytimeScorer` and `PlayerMultiScorer` through `leg.Matchup.PlayerSide(...)`. **So a bet on a PLAYER is named by his CLUB.** The ticket column prints `LANYARD ANYTIME`; the window would say `N LET AUDITORS DIE`. **That is not the bet, and the surface already has the right word** — the surname convention `LegStatement` and `SweatActiveLegModel.Surname` both use.** **AND IT IS `T96`'s SHAPE EXACTLY — *a row must never borrow another market's identity*. **The irony is worth recording rather than scoring: this seat chose `AnchorSide` OVER `PickedHomeForPresentation` precisely to avoid `T96`'s defect** (the null→HOME collapse naming a club the ticket never backed) — and reached a different `T96`-shaped defect by the safer route. The lane's reasoning was right; the rule it landed on is too coarse.** **DEFECT 2, AND IT BITES NOW: with the club alone, a `Moneyline`, a team total and a scorer leg **on the same club render the SAME STRING**. TV routed this as *"it does not bite while the composition is a TODO"* — **it bites at N=1.** On a same-match ticket, which `T142` ships, two legs sit on one fixture and resolve at one whistle; if one dies the window says `N LET AUDITORS DIE` and **the player cannot tell WHICH of his two AUDITORS bets it was.** `T143` exists so he does not have to work that out.** **AND THE COLUMN DOES NOT RESCUE IT, which is the distinction from batch 193: there the window states a COUNT and does not purport to name, so the column carrying identities is a loss of adjacency only. **Here the window PURPORTS to name and names ambiguously. A name that does not distinguish is worse than a count that never claimed to.*** **RULED: **THE WINDOW NAMES THE LEG BY THE SAME IDENTITY THE TICKET COLUMN PRINTS.** One bet, one name, across two zones of one surface — `S96`'s one-composer principle applied INSIDE the TV, and the same fault frame B carried when one club appeared in two conventions on one screen. Take `LegStatement`'s AUTHORED arms; **do NOT fall through to its `default:`**, which is batch 192's warning and stands.** **WHAT IS KEPT: the `Handicap` form as built — `{CLUB} ±1.5` IS `G1-am11` rung 3 and reusing `MatchModel`'s own `+0.0;-0.0` format is right. **What changes is the RULE for everything else, not that arm.*** **WHAT IS UNCHANGED: the unauthored kinds still have no name and still ride Allen's scope call, exactly as batch 192 left them. **This row moves no market into or out of that set** | batch 195 |

---

## For the orchestrator — the incoming TV seat needs all four

1. **`PendingLegName` takes the column's authored identity**, not a club derived from the anchor
   side. The `Handicap` arm stays as built.
2. **`N GO ON` lands with §3**, replacing the superseded `N LET IT DIE` in the ≥2 branch.
3. **§3 lands with a width gate** that fails if the two-name row exceeds the zone — 3.4px of spare
   is not something to leave untested.
4. **The ≥3 escalation is withdrawn**; nothing is owed on it.

## Limits

- **Nothing measured here.** TV's `334.8` with 300.2 spare, and `477.3` un-shortened, suggest the
  authored identities have ample room, but **they are measurements of a different string** — the
  column's forms have not been measured in this zone.
- **I did not run the build.** The misnaming is read from `PendingLegName` and `AnchorSide`'s arms,
  not seen on a frame.
- **`DoubleChance` has an `AnchorSide` arm** and is out of the offered set; it is not part of this.
