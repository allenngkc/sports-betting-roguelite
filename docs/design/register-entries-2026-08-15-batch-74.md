# Register entries — 2026-08-15, batch 74

**S80's DONOR FAILS, AND S50 ALREADY RULED WHY** — written at a fresh DD seat against the live
source at `main` (`2d83f50`), the invariant at `SureThingEntryTests.cs`, and TV's `5724aa1`.

**Destination table: SureThing — the laptop.** **Rows shipped:** `S80-am` (the donor withdrawn, the
bill restated) · `S74-am2` (the DRAW row's pre-committed read + its C46 obligation).

**FIRST, THE PROCESS FACT, because it changes what anyone should do next:** **batch 73 exists.** It
is on disk at `register-entries-2026-08-15-batch-73.md`, written 03:22 today by the seat that died
in the second Orca restart, together with its `REGISTER.md` row and its `surething-design.md` §2
fold — **all three untracked.** The docket seated this seat to spec a move that was already specced
eleven hours earlier. **The restart recovery verified TV's and sgp's disks and did not verify the
DD's.** Nothing was lost; ~11h of sequencing was. **This batch amends batch 73 rather than replacing
it — both land together, and the withdrawn half stays legible.**

---

## S80-am — THE MASTHEAD CANNOT PAY. Not "takes Allen's word" — it is arithmetically impossible, and barred twice over besides.

Batch 73 §4 proposed **masthead 68 → 32, work area 530 → 566**, and routed the cost to Allen as a
scope call because the masthead is shared chrome. **The routing was right and the proposal is not
available to route.**

### 1. The arithmetic — the masthead's 68 IS its content, measured

`SportsbookApp.cs:87` builds `FormMasthead` at `1024 × 68`. Its four children, in masthead-local px:

| child | rect | face | occupies |
|---|---|---|---|
| `Brand` "SURETHING" | `(16, −8)` `300 × 28` | 26px cond | **−8 … −36** |
| `Run` "ROUND n OF m" | `(17, −38)` `340 × 20` | 13px roman | **−38 … −58** |
| `Figures` BANK/TARGET/TICKETS | `(−16, −10)` `610 × 48` | 21px cond | **−10 … −58** |
| `MastheadRule` | bottom-anchored `1024 × 2` | — | **−66 … −68** |

**The two stacked text lines alone occupy 48px** (28 + 2 step + 20 + the 2px they start below the
top pad's edge). **68 = 8 top pad + 48 content + 8 clear + 2 rule + 2.** **The band is not slack
wrapped around content; it is the content, plus 16px of pad.**

**So 32 is not a tight number, it is an impossible one** — a 32px box cannot hold a 48px stack under
any yield. What each option actually gives:

| move | yield | what it deletes |
|---|---|---|
| spacing only (top pad 8→6, subline box 20→18, bottom clear 8→4) | **~8px** | nothing |
| delete the `ROUND n OF m` subline | ~22px | **the round number** |
| one-line masthead (brand + figures abreast, subline gone) | ~26px | **the round number** |
| — | **36px is not reachable** | — |

**Spacing is the only yield S50 permits before deletion, and spacing yields eight pixels.**

### 2. S50 already refused this exact trade, on this exact budget, twelve days ago

S50 is *markets' 44px deficit* — **the same margin, the same flow, a deficit 8px larger than this
one.** It was ruled:

> **Panel growth REFUSED; the 34px is the OS tray** — `--st-band-tray` is the closing term of S2's
> locked arithmetic (**34+38+68+530+34=704**) … **there is no unused screen on this surface.**

**`68` is a term in the arithmetic S50 cited as the reason there is none.** The tray was refused by
naming the whole sum, not by a property peculiar to trays. **And S50's granted remedy is the
precedent that matters: the 44px came out of the FLOW** — an unexecuted deletion (18px) plus S39's
one-baseline collapse applied to the margin leg (26px) — **and it founded the standing yield order:
spacing, then repetition, then nothing.**

**Batch 73 §4 inverts S50 on S50's own screen.** That is the ruling, and it did not need Allen.

### 3. R30 is a bar, not a door

R30: **a locked band is not headroom** — *no band is spare space regardless of what it currently
draws; a lead proposing to grow into one cites the band by name.* **Citation is the minimum to be
heard, not the argument that wins.** Batch 73 read the naming requirement as a permission and
concluded that the one nameable band was therefore the donor. **Every band on this surface is
locked; naming one does not unlock it.**

### 4. And Allen has already ruled on this specific band

`SportsbookApp.cs:173`, in the masthead's own source, recording his word of **2026-08-08**:

> a conformance gap closing rather than a redesign, **which is the only way a Design-verified
> masthead changes**.

**A 36px height cut is a redesign.** It is barred by a standing Allen ruling, so it is not a scope
call to put back to him in the form batch 73 gave it.

### 5. What it would have deleted, had it been possible

`ROUND n OF m` is not decoration. **S37: the live round number appears exactly once on the surface,
and this is that once** — deleting it deletes the fact from the product, not from a screen. The run
figures are **S31**'s BANK / TARGET / TICKETS, written once here and consumed by `LEDGER` through
the same call. **S50's yield order terminates in *nothing*: nothing that states a product fact is
deleted to make a layout fit.** Four facts is not a shortfall, it is the list.

### RULED

**Batch 73 §4 is WITHDRAWN. There is no citable donor band on this surface.** §1, §2, §3 and §6 of
batch 73 stand — verified independently at this seat and listed in §7 below. **The donor question
does not go to Allen as "which band"; it goes to him as §6.**

---

## 6. THE BILL IS NOT ~36px, AND ~34px OF IT IS NOT THE STATEMENT'S — now measured, not reconstructed

Batch 73 §5 asserted a latent overrun **"as a reconstruction, not a fact"** and asked for it to be
measured. **It is a fact, and the gate's own blind-spot list is the proof.**

**The cursor chain, re-derived at this seat from the source, sums to 374** — header 44 (`:901`
`float y = -44f`), 4 × `LegRowPitch` 35 = 140, `:1096` 4, `:1127` 28, `:1180` 34, `:1186` 34,
`:1191` 32, `:1200` 18, `:1228` 40.

**It reconciles to a tenth of a pixel against the shipped pin, which is why the rest can be
trusted.** The payout figure's 36px box bottom lands flush on −370; the wax band's kit position puts
its bottom at 36.10 below that box's top; `structuralOverrunPx = 0.10f` at `SureThingEntryTests.cs:1399`
is exactly that difference. **The reconstruction and the instrument agree.**

**Now the part that is not a hypothesis.** `SportsbookApp.cs:1129-1139`:

```
bool freeHeld = run.OwnsConsumable("free_bet");
bool donHeld  = run.OwnsConsumable("double_or_nothing");
if (freeHeld || donHeld) { … y -= 34f; }
```

**Pure run state.** Independent of leg count, of slip contents, of same-match status. **It composes
freely with four legs and with the statement.**

| state | flow | vs the 370 budget |
|---|---|---|
| 4 legs (**the only state any gate measures**) | 370.1 | **+0.1** |
| + a held consumable | 404.1 | **+34.1** |
| + a relation statement | 406.1 | **+36.1** |
| **+ both** | **440.1** | **+70.1** |

**A player holding one consumable and marking four legs overruns T47's reservation by 34px today,
with no relation statement anywhere near the screen.**

### Why no gate has ever seen it — and this is a T53 defect in the gate

`Working_margin_contains_its_content_at_the_legal_maximum_leg_count` builds **one** state: four legs
**across distinct matchups**, on top of one staged receipt. **It never renders a statement** — its
own source says so at `:963-965`, *"the margin invariant fills MaxLegs across DIFFERENT matchups and
so never renders a statement at all"* — **and it never grants a consumable.**

**The statement's absence is declared. The consumables axis is not.** `:1445-1447` lists what the
gate cannot see — leg counts other than MaxLegs, multiple staged receipts, the board-frozen state —
and **does not mention `OwnsConsumable` at all.** **T53 requires every gate to state what it cannot
see; the one unstated blind spot is the one hiding a live 34px overrun.** That is not a coincidence,
it is the mechanism: **an axis nobody wrote down is an axis nobody sweeps.**

### RULED — three things, and only the third waits on measurement

1. **The invariant's T53 list takes the consumables axis immediately**, whatever the sweep later
   says. **A gate that under-states its blind spots is a worse instrument than a gate with fewer
   checks**, because its silence is read as coverage — S77-am's standard, one lane over.
2. **The sweep batch 73 owed is now the gate on EVERYTHING, not on the constant.** Legs {1..4} ×
   modifiers {none, one, both} × statement {absent, present, longest sentence}. **C46's discipline —
   sweep the population, not the suspects.** Nine to thirty-six measurements.
3. **THE REPORT TO ALLEN IS RESTATED, and this is the material half.** He ruled *pay the pixels* on
   a stated cost of ~36px. **The measured cost is up to ~70px, roughly half of it a defect that
   predates the statement.** His ruling is not disturbed — **the statement's own ~36px is what he
   priced and it stands** — but **the surface cannot fund it from chrome, and the other ~34px was
   never his to price because nobody knew it was there.** The two must not be paid for as one bill.

**No band is proposed here.** With chrome refused, **S50's order is the whole of the available
answer: spacing across the flow chain (~20px is visible in the advances today, against boxes of
30/32/32/16/36), then repetition, then the deficit returns to Allen as a scope call with the
statement and the modifiers priced SEPARATELY.** **This seat does not pick between them ahead of the
sweep** — that is the error batch 73 made in citing a band before the number existed.

---

## 7. What of batch 73 stands, independently re-derived here

| § | claim | verified at this seat |
|---|---|---|
| §1 | `RelationStatementHeight 30 + 6 = 36`, and the 30 is a two-line box | **STANDS.** `:818`, `:1089`. The statement's box is `headerRight` = **296px** (`:879`, *"324 − 14 − 14, the content width every row below uses"*) — batch 73's assumed ~296 is exact, not approximate |
| §2 | the 6px pad survives; S51's trade gets S51's answer | **STANDS.** `ActionBandReservedHeight = PlaceBandY + PlaceBandH + 6f` |
| §3 | the action band is not the donor, by arithmetic | **STANDS.** 8/34, 52/52, 110/44 confirmed at `:784-789`; 130 of ruled height + 24 gaps + 6 pad = 160, and absorbing 36 puts `SkipBandY` below the floor |
| §6 | the invariant's shape: `flowBottom ≥ −MarginFlowBudget`, slack ≤ one leg row, blind spots stated | **STANDS, and §6 above adds the axis it must name** |
| §4 | the masthead as donor | **WITHDRAWN — §1–5 above** |
| §5 | the ~70px warning | **UPGRADED from reconstruction to measured, §6 above** |

---

## S74-am2 — THE DRAW ROW: read pre-committed, so the frame closes it in one pass. And one consequence the ruling never priced.

The lobby frame has **not** landed in `dd-import/` at the time of writing. **This is not a wait:**
the build is readable at `5724aa1` and the read's criteria are pre-committed here, so the frame
either passes them or does not, with no second reading pass. **Batch 71's discipline, applied
before the evidence rather than after it.**

### The build, as it stands

```
DrawOdds  (462, −43)  112 × 32  19px  _fontCond  LaptopTrack.Names
label:    $"DRAW  {OddsFormat.American(matchup.DrawOdds)}"     (two spaces)
gated:    if (matchup.DrawOdds > 1.0)
pitch:    MatchupCardPitch 78 → 116
```

**Every clause of S74/batch 65 is present in the source**: the price cell carries the outcome
(`AWAY`/`HOME` were already outcomes, not clubs, so nothing is invented); the matchup column has no
`TeamLine` call, which is the ruling rather than an omission; the middle position is literal at −43
between −8 and −81; no dot, crest or hue. **The `drawSelected` wash and ring were swept rather than
fixed per site, which is T43's lesson applied without being asked.** **The composition is not in
doubt; what wants the frame is the READ.**

### PRE-COMMITTED — the frame passes if and only if all five hold

1. **The DRAW line reads as an OUTCOME, not as a third team.** S74's whole basis is that the engine
   refused the draw as a `Side` and *the surface must not undo it in presentation*. **The empty
   subject column is what carries this**, and an empty column reads as absence-of-subject or as a
   missing team depending entirely on the frame. **This is the one clause only a frame can settle.**
2. **The middle position reads as between rather than as third-in-a-list** — the two teams' lines
   above and below it, attached to neither.
3. **`DRAW  {price}` sits inside its 112px cell with the same clearance its two siblings have.**
4. **No team treatment leaks** — no dot, crest or hue on the draw line (T2's muted blue/pink are the
   two sides; a draw has no side).
5. **The three price cells read as one column of three offers** at one pitch, not two-plus-one.

### THE C46 OBLIGATION, named exactly — and it is cheap, because the cell was sized against a sibling

TV flagged fit as not asserted, and it is right to. **But the population is narrow and the source
says why.** `SportsbookApp.cs:281-284`, the ring's own comment:

> The price cell IS the odds button (112x32) — it is already wider than the 96x30 cell ASSETS.md
> assumed, **because the `AWAY  -341` label needs the room.**

**The 112 was derived from `AWAY` + two spaces + a four-character price.** `DRAW  {odds}` is
**the same shape, the same face, the same size, the same tracking, the same separator.** So the
sweep has exactly two questions:

- **does `DRAW` measure wider than `AWAY`/`HOME` at 19px Archivo Narrow with `LaptopTrack.Names`?**
- **does the draw's odds population produce a longer numeral than the moneyline population does?**
  `DrawOdds` is set at slate generation (`SlateGenerator.cs:91`), so the population is enumerable.

**Measure both against 112px and report as widths, not as a verdict** — this seat asserts no fit.
**If `DRAW  {odds}` clears with its siblings' clearance, C46 is discharged for this string and no
face-wide re-sweep is owed**, because nothing about the face changed; **one string joined a box that
was already sized against its own shape.**

### THE CONSEQUENCE NOBODY PRICED — the board no longer shows the whole slate

TV's re-derivation is correct and its arithmetic is honest: **one more line is one more 38px pitch,
so `MatchupCardPitch` 78 → 116, and the 504px list area holds 4.34 blocks where it held 6.46.**

**`MatchupsPerSlate = 6`** (`engine/RunConfig.cs:40`). So:

| | cards | list area | visible |
|---|---|---|---|
| before | 6 × 78 = **468** | 504 | **all six** |
| after | 6 × 116 = **696** | 504 | **four** |

**The FORM board stopped being a board he can take in at once.** It scrolls, so nothing overflows
and no gate fires — **that is precisely why it needs a ruling rather than a test.** S74 ruled the
draw's composition; **it did not rule that the slate becomes a scrolling list, and that is a change
to what the screen IS.** It is the T20/T47 shape a third time: **landing a cap is not landing the
layout the cap implies.**

**NOT RULED HERE, and deliberately.** Three readings are open — *(a)* six-of-six was never load-
bearing and four is fine on a list that already scrolled for receipts; *(b)* the slate's
comparability is the FORM board's job and a scroll breaks it; *(c)* the pitch is re-derivable
downward if the draw line can share space rather than take a full 38. **Each is a different screen,
and the frame that is being docked shows which one he is actually looking at.** **Docketed as the
first question of the read, ahead of the five checks above** — a composition that reads perfectly on
a board he can no longer see whole has still cost something.

---

**Routing.** **S80-am → surething-ui**: the invariant's blind-spot line now, the state-space sweep
next, and **no geometry moves until the sweep lands.** **S74-am2 → the frame**, then TV.
**To the orchestrator: batch 73 and this batch land together, and `5724aa1` must merge main before
it pushes** — it branched at `755deb8` and does not contain su's P5 work (`main` is 16 commits
ahead, three of them in this same file). TV measured overlap against the surething-ui-2 lane's
unmerged commits, not against main; the hunks still do not collide, but the baseline it checked was
the wrong one.

**To Allen, in one line:** *the pixels you ruled paid for cannot come out of the chrome — the
masthead's 68 is its own two lines of type, not slack — and the sweep that finds them has also found
a 34px overrun that has nothing to do with the sentence and is on the screen today.*
