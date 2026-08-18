# Register entries — batch 102 (2026-08-17)

**Written at the DD seat.** Two rows, answering tv-theater's merged unit 1 (handoff 0-U1, `main` at
`d671fee`). **Destination tables:** TV — match theater (`T108-am2`) · SureThing (`S88`).

---

## `T108-am2` — the extension CONFIRMED, and it was never a generalisation

**CONFIRMED AS BUILT.**

**`GOALS 0 MORE` is not an *analogous* defect — it is the same clamp at a second call site.**
`Math.Max(0, threshold - total)` on identical arithmetic. And `T108`'s governing argument was `G1`'s,
which is **market-agnostic**: *NEED is the requirement while live, compact is identity elsewhere* says
nothing about corners. **So extending is not widening the ruling. It is completing it** — and
stopping at corners would have knowingly shipped the twin.

**Clause 4 does not bite, and the lane read it correctly.** Its own preamble says *"named because a
fix of this shape invites tidying"*. It forbids taking the occasion to improve the column; it does
not forbid applying a ruled form to a sibling market carrying the identical defect. **Stated for
reuse: clause 4 bars changes the ruling did not reach. It does not bar the ruling reaching everywhere
its own reason applies.**

**The lane's two-part rule is ratified — and it is better stated than the spec stated it:**

> The **outcome** is derived wherever revealed values decide the leg; the **string** changes only
> where the old string named a requirement that no longer exists.

**The broad half is required** — a narrow outcome would break `TicketCannotLose`, which needs every
leg. **The narrow half is what stops it becoming a rewrite.** Checked against all seven arms and it
discriminates correctly: goals over/under and corners/cards change string; **BTTS and scorer take an
outcome and keep their copy** (`2/2 TEAMS SCORED`, `BOTH HAVE SCORED`, `SCORED` name no requirement
left to void); moneyline including the draw stays `Undecided`, because a goal can flip it up to the
whistle. **A rule that gives the right answer on cases it was not written for.**

**`T108-am` §4 generalises with it:** the statement line does not change on a decided leg in any arm.
`BOTH TEAMS SCORE` above `2/2 TEAMS SCORED` is identity answered by its own progress line, exactly as
`OVER 8.5 CORNERS` is.

**Routed to `T111`, not ruled here (`C17` — source read, no frame):** every count arm formats
`{total} {NOUN}` unguarded, so **`1 GOALS • 1 MORE` appears constructible** at one revealed goal.
Corners may never reach it — this seed's deltas were 2 — but goals certainly can. **The docked
`goals-control-2026-08-16` set already contains the window** (revealed `1 — 0` holds from 30' to
`90'+1`), so the sweep settles it on frames already on disk and **no capture is owed.**

## `S88` — MY BETS reads the stale mirror

**FLAGGED under `C17`, NOT RULED — source read, no frame; capture owed.**

Reported by tv-theater at handoff and **verified at source by this seat**: `RevealedView.ResolveLeg`
has exactly one call site (`FinalSlam`), so on a multi-leg ticket a leg resolved through `ResolveBeat`
never leaves `RevealedLegState.Live` in the mirror. **The TV does not read that mirror** —
`BuildTicketLegOutcomes` deliberately builds from the same sources its own rows render from, and says
so in terms. **The laptop's MY BETS does.**

**The disposition is confirmed — route, do not fix cross-surface.** A TV lane editing a laptop
surface crosses a worktree boundary, and **what MY BETS should say about a leg decided on a ticket
still riding is a laptop copy question that no TV evidence answers.**

**Corrected in one respect: routing to the orchestrator's board is not enough.** `C22` — *a ruling
exists when it is a row in `REGISTER.md`* — so it takes a row now, or the laptop lane will not find
it. **That is exactly how `T101`'s residual came to be named twice as owed and unclaimed.**

**The governing principle travels with it** when it is picked up: `T108`'s — *no word may name a
jeopardy or a requirement that no longer exists* — and **MY BETS is arguably the worse surface for
it, being a record rather than a live watch.**

**Owed:** a MY BETS capture on a multi-leg ticket with one leg resolved and the ticket still riding.
Until it lands this is a source read and no verdict is taken; `S86` sat here before its capture and
the discipline is the same.
