# SPEC — the theater for count markets (Phase 2, FINAL)

**Written:** Design Director seat, 2026-08-17 · **Authority:** Allen's calls relayed 2026-08-17,
landed as `T109-cl` and `T115` · **Evidence:** `count-sweat-read-2026-08-16.md` ·
`grammar-count-markets-2026-08-17.md` · `T113` (the calm-beat probe) · **Surface:** TV — match theater

**Two clauses, and they are deliberately separable.** §2 is the reveal; §3 is the grammar. **The
grammar does not depend on the reveal** (`T113` §9), so a change to §2 changes nothing below it.

---

## 1. WHAT WAS WRONG, MEASURED ON A MATCHED PAIR

Same seed, same fixture, same pacing, same predicates, same stake, **identical final scoreline**, one
variable — `OVER 8.5 CORNERS` against `OVER 1.5 GOALS`.

- **The corners arm had MORE events and FEWER dead stretches** (7 against 3; 9 against 11) **and it is
  the one that watches flat. Event scarcity is not the cause.**
- **The two arms received beat-for-beat IDENTICAL drama, probabilities within thousandths** (`T113`).
  **The stream is innocent. The flatness was entirely presentation routing.**
- **Up to six of eight beats were calm beats the count branch spent on corners carrying no tension.**
- Against the 8.5 line, distance ran **7 → 5 → 3 → 1 → crossed → decided → decided**, and **all seven
  corners got one treatment.**
- **The corners player was shown `0 — 0` for 86% of a match that finished 5–1**, then handed the
  result in two steps at the death.

## 2. CLAUSE ONE — THE REVEAL (`T109-cl`) · ⚠ SEPARABLE, AND FLAGGED

**RULED (on the reading of Allen's *"stay personalized"* as option A): the revealed scoreline is never
withheld. Goals reveal on the true clock whether or not the ticket rides on them.**

**Everything else stays ticket-keyed** — the stats panel's rows, player detail, and the flavour
strip's subject continue to follow the ticket. **Personalization is preserved as the governing
principle; the score is carved out of it because a scoreline is the match's primary fact and the
surface asserts it rather than withholding it.**

**What it fixes:** the false `0 — 0`, the two-step result at `90'+1`/`90'+2`, and the fact that the
arm with a resting state was also the only arm that got to see a goal.

**What it costs, stated plainly:** the ledger's rule stops being *"reveal what the ticket rides on"*
and becomes *"the score is always true, the rest follows the ticket."* **A rule changing shape, not a
tweak** — every reader of the revealed ledger inherits it.

> **⚠ THE READING MAY BE WRONG, AND THE SPEC IS BUILT SO THAT BEING WRONG IS CHEAP.**
> *"Stay personalized"* discriminates cleanly against option **B** (show the whole match) but **not
> between A and C**, and **"stay" is a preservation word** — which reads toward **C, no change at
> all.** A and C are opposite answers on whether a measured falsehood gets fixed.
>
> **This clause is therefore self-contained. If Allen meant C, delete §2 and build §3 unchanged.**
> Nothing below reads from it. **Do not let §3 acquire a dependency on §2 during the build** — that
> is the one thing that would make the flag expensive.

**And it compounds with §3 rather than overlapping it:** a goal the corners player does not need is
exactly the **departure from calm** his watch is missing. §2 supplies contour from outside the count
grammar; §3 stops spending contour inside it. **Neither substitutes for the other.**

## 3. CLAUSE TWO — THE GRAMMAR (`T115`)

### 3.1 The rule

**An event earns its treatment from its DISTANCE TO THE LINE, not from having arrived.**

**A ramp, not a switch.** The market's whole tension is a continuous quantity — this is the cricket
required-run-rate steal landing on the **scene grammar** rather than on a line of text.

### 3.2 The mechanism, found at source

`TheaterChoreographer` takes the count branch **first**: on a corners/cards leg, any non-`LegFinal`
beat that stages **`TotalDelta > 0`** returns a `CornerFor`/`CornerAgainst` scene and **returns**,
short-circuiting the only table that can produce `CalmPossession`.

**So calm does not lose a competition — it is never reached.** The beats were **tagged `Calm` by the
stream and rendered `CornerFor` by the routing.**

**The change is to GATE THE COUNT BRANCH'S ENTRY**, not to compute both and prefer calm. Stated
because the probe's own wording (*"overwritten"*) invites the more expensive edit.

### 3.3 Where the significance comes from — nothing is invented

**The ticket column already computes it.** `SweatActiveLegModel` derives `threshold − total` from
**revealed** values and printed `8 CORNERS • NEED 1` at 48'. **The theater asks the question the
column already answers.**

**It reads the REVEALED count, never the locked target** — `T108`'s standard, and the no-leak law in
that file already enforces the provenance.

### 3.4 What the rule buys — three findings, one change

1. **The resting state returns.** It is **not authored** — it is what remains when buildup stops being
   spent, out of scenes that already exist and already play.
2. **The approach and the turn become the only weighted moments**, which is what makes them read as
   moments at all.
3. **The corpse stretch ends as a consequence.** A resolved leg's corners have **no distance to any
   line**, so the ~20 seconds of post-win narration stops without its own fix.

### 3.5 The strings — the two decisive beats may not take a recycled line

**Measured, and it is the worst possible assignment:** of seven count events, **the approach (43')
printed the line from corner #1 — the least consequential event of the match — verbatim**, and **the
crossing (53'), the moment the bet was won, printed the line from corner #2.**

**RULED: the approach and the turn draw from an authored pool that ordinary count events cannot
reach.** A ramp in treatment with a flat string pool would still narrate the win with a line the
player has already read twice.

**The words themselves are COPY and `C11` authors copy on a frame — they are not written here.** What
is ruled is that the pool is **disjoint**, so recycling onto a decisive beat is unconstructible rather
than unlikely (`T108` clause 1's standard).

## 4. THE BINDING — A QUIET CORNER MUST STILL COUNT

**This is the gating condition, not a footnote, and it is what turns one gate into real work.**

`StageBeat()` **advances its cursor unconditionally** — it consumes the batch on the beat it is
called. `CompleteCount` fires from `OnCountPlayed`, **the scene's payoff callback.** So a beat that
takes a batch and falls through to calm **consumes the count without committing it: the column stops
tracking and the match ends short of its own total.**

**RULE: no beat may consume a count batch without committing it.** A corner that earns no scene is
still a corner — **the count is a fact; only the drama is discretionary.** The arrangement is the
lane's call; that it must hold is not.

**Budgeted here rather than discovered: this is one gate PLUS a commit path that does not exist
today.**

## 5. ALREADY BUILT — DO NOT RESPECIFY

- **Valence off the ticket is DONE.** `countHelps` is set from `leg.Selection.Choice`, and
  `ScoreLedgerTests` asserts it: *`CornerFor`/`CornerAgainst` is the bettor's MOOD, not team*, and
  mood must never drive routing. **The earlier proposal is withdrawn — it exists and is gated.**
- **Calm scenes exist and play**, with their own pacing, excluded from buildup. Nothing to author.
- **Zero batches already fall through.** The path this spec widens is already there and already
  correct.
- **The UNDER's win by absence is `T97-am`'s**: the strip's words are licensed by the **resolved
  scene**, never the beat's own moment. Do not re-derive it.

## 6. OUT OF SCOPE — named so absence is not read as coverage

- **CARDS.** The opposite problem — a booking arrives carrying its own significance and needs
  **catching**, not ramping. Distance-to-line is the wrong instrument for it. **No cards arm has ever
  been shot** and nothing here is evidence about booking drama.
- **The UNDER case.** The mirror distance profile, not in evidence.
- **The flavour strip's overrun** — `T110-am`, its own ruling, its own measurement.
- **The resolved column's strings** — `T108` and its amendments.
- **The cashed-out footer** — `T114`.

## 7. THE GATE

1. **Assert a count event below the significance threshold does NOT produce a count scene** — and that
   the beat reaches the base table.
2. **Assert the count is committed on every staged batch, scene or no scene.** §4's binding, and it is
   the assertion that matters most: **a fixture running a full sweat must finish with the column's
   total equal to the match's own.**
3. **Assert significance is computed from REVEALED values** — no path from the locked target.
4. **Assert the decisive-beat string pool is disjoint** from the ordinary pool (§3.5).
5. If §2 is built: **assert the scoreline reveals independently of the ticket's market**, on a
   corners fixture whose match scores.

**Blind to:** whether the watch is better. **That is the whole point of the phase and no gate can
speak to it** — §8.

## 8. EVIDENCE OWED BEFORE DESIGN-VERIFIED

1. **A corners sweat re-shot on the same seed and line as `corners-sweat-2026-08-16`.** The
   before-state exists, so the after can be read against it directly — **same seed, same fixture, one
   variable.** That pairing is the instrument and it is already half-built.
2. **The scoreline's behaviour on that arm**, for §2.
3. **A near-line watch** — a leg that lands close to its line, or loses. Every frame we hold is a
   comfortable winner, and the ramp's whole value is in the case we have never seen.

## 9. NOT CLAIMED

- **The probe measured SCHEDULING**, one seed, one line, with `ResolveBeat`'s interception untouched —
  **which is exactly what §3 changes.** It proves the calm beats exist and are spent. **It does not
  prove that reclaiming them yields a good watch**; that is a `C11` frame claim awaiting item 1 above.
- **No frame has been read for this spec.** §1's numbers are the capture's own log and the probe's.
- **`Territory` is excluded by arithmetic** (`Swing` Δp ≥ 0.10 against `Momentum` under 0.07) and holds
  for every seed; **the fallback arm's unreachability is a property of TODAY'S config** and would not
  survive a config change unexamined.
