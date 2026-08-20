# SPEC — the console's betting surface (Phase 1, for Allen)

**Written:** Design Director seat, 2026-08-19 · **Batch 121**
**Mandate:** `docs/5-orchestration/dd-mandate-2026-08-18.md` Phase 1
**Fork:** `docs/design/mandate-scope-2026-08-19.md` §3 — **Allen ruled Reading A.**
**Evidence:** `docs/design/dd-import/console-read-2026-08-19/` — four transcripts, shot at this
seat off a build made from HEAD, plus `MEASUREMENTS.md`, `GEOMETRY.txt` and `SPECIMEN.txt`.

**Status: FOR ALLEN.** Nothing here binds until he approves it. The markets-pregame lane builds
from the approved version.

---

## 0. WHAT READING A MEANS, AND THE ONE PLACE THE SCOPE PASS WAS TOO KIND

Reading A is *"a presentation pass on `game-console`'s existing betting surface, bringing the
second surface up to the vocabulary the laptop now carries."* That is what this spec does.

**But the scope pass's central measurement is wrong in a way that makes the phase better, not
worse, and it has to be corrected before anything is built on it.** It read the six market kinds
named in `BettingScreen.cs` and concluded the console has `S86`'s defect — *nine market kinds with
no reachable home.* **It does not.**

**`ShowDetail` is kind-agnostic.** It iterates `matchup.Markets` and labels every offer through
`MatchModel.Fields`, so it prints **every kind the engine prices**. Counted on the frame:

> **84 offers on one matchup.** The laptop's own measurement over 18,000 matchups puts a matchup at
> **79–90, mean 84.78**. Reconciled destination by destination against
> `spec-market-surfaces-2026-08-17.md` §3, the console's 84 land as **13 · 18 · 10 · 10 · 14 · 19** —
> the table exactly.

**The six kinds are the PARSER's, not the renderer's.** `ParseOne` accepts `H`/`A`, `Y`/`N`, `S#`,
`GO`/`GU`, `CO`/`CU`, `KO`/`KU` — six of fifteen.

**So `C19` does not fail here the way it failed on the laptop. It fails one step later and harder:
the console prints the whole slate and will accept a bet on two fifths of it.** That is not `S86`'s
shape. It is `S85`'s — *a price cell is both a fact and an offer; an offer that cannot be taken is
not an offer* — running across nine market kinds permanently instead of fourteen cells
situationally.

Recorded under constitution §1.5 as a correction to a document at this seat, not to the relay: the
scope pass measured the parser and reported it as the renderer, and it is a **source count where a
frame was available.** The frame took four minutes to shoot.

---

## 1. THE SUBJECT

`game-console/BettingScreen.cs` — the surface the player lists, confirms and reads back bets on,
plus the primitives in `Ui.cs` it draws with.

**In scope:** the slate, the `M n` market sheet, the `picks>` grammar, the `TICKET PLACED`
confirmation, the `TICKETS` block, and the sweat's naming of a ticket's legs.

**Out of scope and named so nothing is tidied:** the shop, relics and consumables; `EventText`'s
beat words; `SweatRenderer`'s composition beyond leg naming; `Ui.Title()`; the round header block
except where the page width forces it.

---

## 2. THE PROBLEM, MEASURED

Everything in this section is read off `dd-import/console-read-2026-08-19/`.

| | |
|---|---|
| offers printed on one matchup | **84** |
| lines written after `Ui.Clear()` | **92** |
| destinations, groups, counts, position fact | **0** |
| market kinds printed | **15** |
| market kinds the grammar accepts | **6** |
| widths the surface prints at | **62 · 79 · 166** |

**The four defects, in the order they hurt:**

1. **Nine kinds are unbettable, including the draw.** `MONEYLINE DRAW +259` prints on the sheet;
   `1D` returns *"Bad market in '1D'. Use GO/GU, CO/CU, KO/KU, Y/N, or S#."* `Matchup.DrawOdds` is
   a public property. **The game's headline market went three-way and this surface can bet two of
   the three.**
2. **The sheet has no structure and does not fit a screen.** Eighty-four rows in one run, ordered
   by `MarketKind`'s **declaration order** — which splits GOALS across positions 2 and 12 and
   CORNERS across 4 and 10. The surface calls `Ui.Clear()` and then writes 92 lines.
3. **The page is unauthored.** `Ui.Rule()` says 62 columns; the slate prints at 79; a four-leg
   `TICKETS` row prints at 166. **Three page widths, none of them chosen.**
4. **Refusals arrive after the act, never before.** A fifth leg is refused *after the stake is
   typed*. Nine kinds are refused *after the offer is read off the sheet*.

**None of this is a regression.** It is HEAD behaving as written — the vocabulary shipped with
surfaces owed, the laptop's was paid at batch 119, and this is the other half.

---

## 3. THE PAGE — 80 × 24, and it is DERIVED

**This surface has never had a page.** `Ui.Rule()`'s 62 columns is the title box's 60 plus two — a
number that came from a decoration, not from content. `C46` is exactly this: a fixed box carrying an
unstated claim about what fits in it.

**Measured against the enumerated pool, per `S84` and `S96-am` — the pool, never the seed's
champion.** `SlateGenerator`: 16 cities × 20 nouns = **320 constructible clubs**, widest 26
characters; 12 × 12 = **144 constructible players**, widest 15.

Composed through the laptop's own row-name rule (§6), the widest constructible row name is

> **`SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS` — 44 characters.**
>
> **The same champion `S96-am2` found on the laptop**, arriving independently on a second surface in
> a different medium. That is the strongest available evidence that the transfer is exact.

Against a row whose fixed chrome — line number, price, probability — is 19 columns:

| page | name/leader field | worst name 44 | |
|---|---|---|---|
| **62** (today's rule) | 43 | **FAILS by 1 character** | |
| 72 | 53 | fits | 7 leader dots at worst |
| **80** | **61** | fits | **15 leader dots at worst** |

**RULED: the console's page is 80 columns × 24 rows.**

- **80 is derived twice over.** The enumerated pool refuses 62 by one character, and the surface is
  *already printing at 79* — its slate row. One of the two authored widths is wrong, and it is the
  one that came from a title box. `S94-cl`'s lesson, on a second surface: *the rail fit because its
  packing is derived from measured labels rather than authored constants.*
- **24 is the medium's floor**, not a preference — the ANSI/VT100 standard and the Windows console
  default's usable height.
- **The page does not reflow.** `Console.WindowHeight` throws on a redirected stream, so a floor has
  to exist regardless; a layout that reflows cannot be captured or gated (`C11`, `C18`); and this
  studio's whole direction is that we are made of paper. **A larger window holds the page; it does
  not change it.**
- **It is a STATED constant.** Constitution §3.5 — a bound is not a layout — so the page is written
  down once and every layout derives from it. Nothing may re-author 62, 79 or 80 locally.

**Blind spot, stated per constitution §4.2: a piped transcript has no viewport.** These captures
show what the surface printed, never what the player saw. **That blind spot is the reason §3 is a
ruling at all** — a surface that clears the screen and writes 92 lines has made the viewport the
deciding element of its design, and no instrument in this studio is on it.

---

## 4. THE DESTINATIONS — the laptop's six, transferred whole

**RULED: `RESULT · GOALS · CORNERS · CARDS · CORRECT SCORE · PLAYERS`, in that order (`S95`), with
the same contents (`spec-market-surfaces-2026-08-17.md` §3).**

**The reason is not coherence. It is that the destinations are what make the sheet fit the page.**

| destination | offers | + 4 lines of chrome | against a 24-row page |
|---|---|---|---|
| RESULT | 13 | 17 | fits |
| GOALS | 18 | 22 | fits |
| CORNERS | 10 | 14 | fits |
| CARDS | 10 | 14 | fits |
| CORRECT SCORE | 11–16 | 15–20 | fits |
| PLAYERS | 17–24 | 21–**28** | **overflows above 20 offers** |

**Eighty-four lines do not fit a screen. Thirteen, eighteen, ten, ten, fourteen and nineteen all
do.** The laptop derived six destinations from a 700px rail; the console derives the same six from
a 24-row page, and they agree. That is the same argument arriving in a different unit.

**And PLAYERS is the one that overflows on BOTH surfaces** — it scrolls at 1.88× on the laptop
(`S87`) and passes 24 rows here. Same destination, same medium-independent cause: it is the only
group whose size follows the roster.

**Do not re-derive the taxonomy.** Three separate DD findings (`T44`, `T97`, `T98`) have crossed
between these two surfaces in the last fortnight — `T98` on Allen's own direction. **Two taxonomies
for one vocabulary means every future ruling is authored twice**, which is the failure `S98`'s
general form exists to prevent.

**`S94-cl`'s seventh-destination clause binds here too, for the console's own reason.** The
contents page (§5) lands at **exactly 24 lines of a 24-row page**. There is no seventh slot on
either surface. **A seventh destination requires a DD ruling before it is proposed** — and it now
has two independent measurements behind it.

**`S102` is what makes the console's contents page fit.** Suppressing `CORRECT SCORE`'s duplicate
child saves the one line that would otherwise put the page at 25. A laptop ruling paying for itself
on a surface it was not written for.

### 4.1 Empty groups still print (`S89`)

`CORNERS ......... no prices offered`, on the contents page and as the destination page's body.
**A racecard prints the race even when it is abandoned**, and it means the destination set is a
constant — it never varies by matchup, so the page is authored once and never reflows.

---

## 5. THE CONTENTS BLOCK, THE FOLIO, AND THE ADDRESS

**RULED: `S90` transfers whole. `M n` opens the CONTENTS page, not the sheet.**

The contents is `spec-market-surfaces-2026-08-17.md` §5.2's block, in monospace, with `S98`'s long
form (`TEAM TOTAL GOALS · TEAM TOTAL CORNERS · TEAM TOTAL CARDS`) and `S102`'s suppression. It is
rendered in full at `dd-import/console-read-2026-08-19/SPECIMEN.txt`, Specimen 1.

**RULED: every destination page carries the folio at its foot — `GOALS   14–31 of 84`.**
Derived from the rendered list, never authored (`S74-am3`). **A folio that lies is worse than no
folio**, because its whole value is that it is true inside a game about being lied to.

**Where the destination overflows the page, the folio paginates and says so:**
`PLAYERS   66–83 of 84   [N]ext`. The folio already carries the range; a next page is the folio's
own next page and needs no new vocabulary. `S25-am` transfers as: *every destination that overflows
the page paginates with the folio.*

### 5.1 The printed line number is also the ADDRESS — and this is the whole design

**RULED: every offer row carries its printed line number, 1–84, matchup-global.**

The laptop's rows do not carry numbers because its folio is a scroll position. **On the console the
number does two jobs — it is the folio's referent AND the pick address — and that is why the console
gets numbers and the laptop does not.**

Worst-case navigation: **two interactions** (`M 1` for the map, `M 1 GOALS` to read the group)
against the laptop's three and DraftKings' ~7. **And zero to bet**, because the address works from
the top level.

---

## 6. THE ROW

```
 nn  NAME ·································   PRICE   p NN%
 |   |                                        |       |
 |   name/leader field, 61 cols               |       true probability (00-vision §3)
 |                                            American odds, right-aligned, 5
 printed line number = the pick address
```

Chrome is 19 columns; the field is 61; the worst constructible name is 44. Rendered at
`SPECIMEN.txt` 2 and 3.

### 6.1 One offer per row, name first — RATIFIED AS BUILT (`S91`, `S92`)

The console already does both. **Ratifying costs nothing and buys the thing that matters:** the
laptop's version arrived by migration drift and could have drifted back until `S91` and `S92` pinned
it. The console's is unpinned today.

### 6.2 The price stays in toner — RATIFIED AS BUILT (`S97`)

Offer rows are `ConsoleColor.Gray` and prices carry no colour of their own. **That is `S97`
delivered**, arrived at independently. Do not add colour to prices. `S97`'s named-not-ruled residual
— *amber's real claim is the SELECTED price* — is not opened here.

### 6.3 The true-probability column STAYS — and it is not a dev readout

`p 47%` is `design/00-vision.md` §3: *"Every mechanic is mathematically legible. The baseline bet is
the four-number model — true probability, offered odds, stake, payout."*

**The console is the only surface in the studio that prints all four.** It is not an artefact of
being a prototype; it is the vision clause made visible, and it is the clearest answer available to
*what is this surface for* without inventing the in-fiction identity Reading B would have needed.
**Recorded so nobody removes it as a leak.**

### 6.4 Leader dots (`S89`, §4.3)

The gap between a name and its price is the **annotation gap**, and a gap doing work should look
like it. Monospace makes this exact rather than measured.

**`S100`'s minimum-run guard transfers as law and never fires at this geometry** — the worst
constructible row still prints 15 dots, where the laptop's prints none. Stated so nobody removes the
guard, and so nobody thinks it is doing work.

### 6.5 Row names uppercase at the presentation layer (`S96`)

The console reproduces `S96`'s founding finding verbatim: `Sheboygan Refunds MONEYLINE` sits beside
`MONEYLINE DRAW`, and `LANCE STAPLER 2+` uppercases a player's name in the same column where
`Sheboygan Refunds OVER 0.5 GOALS` does not uppercase a club's. **Two proper nouns, two treatments,
one column** — `S96`'s own words, on the console's own frame.

`A2` is not overridden: **the words are the engine's, the case is the surface's.**

### 6.6 The composer — ONE rule for both surfaces

**The two surfaces print different names for the same market today**, and `MarketLabel`'s own doc
comment says it exists so that *"the console and the laptop UI can never print two different names
for the same market."* The laptop moved to `MarketSheet.NameOf` in the surfaces build; the console
did not.

| kind | laptop `NameOf` | console `MarketLabel` |
|---|---|---|
| Moneyline (team) | `SHEBOYGAN REFUNDS` | `SHEBOYGAN REFUNDS MONEYLINE` |
| Moneyline (draw) | `DRAW` | `MONEYLINE DRAW` |
| Total goals | `OVER 2.5 GOALS` | `TOTAL GOALS OVER 2.5 GOALS` |
| BTTS | `BTTS — YES` | `BTTS — YES BOTH TEAMS TO SCORE` |
| Correct score | `1-1` | `CORRECT SCORE 1-1` |
| Odd/even | `ODD` | `TOTAL GOALS ODD` |
| Handicap, team totals, scorers | *identical* | *identical* |

**Neither is wrong today, and that is the finding.** The laptop's short names are legible **because
the destination and the contents name the market**; the console's longer ones are legible **because
nothing else does**. `MarketLabel` drops the market name for Handicap and the three team totals —
correct on the laptop, and on the console it leaves 12 of 84 rows with no market kind at all,
because it assumes a container this surface does not have.

**RULED: the console adopts `MarketSheet.NameOf` verbatim, and it becomes correct the moment §4 and
§5 give it the container.** One composer, two surfaces, `S22` honoured as written, `MarketLabel`'s
stated purpose restored. **The divergence is not drift — it is one surface having a structure the
other lacks — and building the structure is the fix.**

### 6.7 The role prints as a WORD (`S22`, already ruled — this is enforcement)

Fourteen player rows print `[FW]` `[MF]` `[DF]`. **`S22` struck the bracketed tag on 2026-07-31**,
and `MatchModel.RoleWord()` exists, returns `FORWARD` / `MIDFIELDER` / `DEFENDER`, and is bypassed
by `BettingScreen.cs:171`. No new ruling — a standing one that never reached this surface.

---

## 7. THE PICK GRAMMAR — the folio is the address

**RULED: the pick token is `{matchup}#{line}` — `1#38`. The six existing mnemonics stay as
aliases.**

**This is not a preference; the mnemonic grammar cannot reach the vocabulary.** It is a closed set
that needs one authored token per kind, taught in a prompt line that already reads
`(e.g. 1H 3GO2.5 5CO9.5 2Y 1S3)` — five examples for six kinds, at the edge of what one line can
teach. **Team totals alone need three fields** (team, over/under, line) in a space-delimited token
that has room for one. The grammar does not extend; it is a measured capacity failure, the same
shape as a rail overflow.

**And it invents nothing. `1S3` — matchup 1, scorer 3 — is already index addressing**, adopted for
the one market with too many rows to mnemonic. **§7 generalises the surface's own answer to the
exact problem that produced it.** `S85`'s treatment was `S69`'s rule already ruled; this is the same
move.

**Three properties follow, and the third is the one that matters:**

1. Every printed offer has an address, and every address is a printed offer.
2. It reaches all fifteen kinds, and the sixteenth, with no new vocabulary.
3. **`C19` becomes structural rather than maintained.** A kind cannot be printed-but-unbettable,
   because printing it is what gives it an address. The defect this phase exists to fix cannot recur.

The prompt becomes `picks> (e.g. 1#16 3#22 5H)` — the general form plus one alias.

---

## 8. REFUSALS MOVE BEFORE THE ACT (`S85`)

**`S85`'s law, verbatim: where a refusal is knowable BEFORE the act, the surface shows it before and
the act never happens; where it is knowable only AFTER, the statement gives cause and remedy. A dead
click is what happens when a knowable refusal is left to be discovered.**

Three refusals on this surface, all knowable before, all discovered after:

| | today | ruled |
|---|---|---|
| a fifth leg | `Tickets take 1 to 4 legs, got 5` — **after the stake is typed** | refused at the **fifth token**, naming the cap and the count already held |
| a kind with no token | `Bad market in '1D'. Use GO/GU, …` — after the offer was read off the sheet | **cannot occur**: §7 gives every printed offer an address |
| an out-of-range address | *(new)* | refused at the picks prompt, naming the matchup's range: `matchup 1 lists 1–84` |

**And one refusal names the wrong fault.** `1CS1-1` returns *"Bad line in '1CS1-1'"* — but `CS` is
not a market prefix at all. The line is parsed at `BettingScreen.cs:322` before the over/under check
at `:324`, so the player is told the *line* is bad on a market that does not exist. **RULED: a
refusal names the first thing that is actually wrong.** Under §7 this path becomes unreachable;
fixing the order costs one swap and stops the class.

**What does NOT change:** engine validation messages print verbatim. `S85` moves *when* a refusal
happens, never who authors it.

---

## 9. THE READ-BACK

Three defects, all measured on `A3` and `A4`.

**9.1 The same string prints twice.** `TICKET PLACED: {DescribeLegs}` and then the `TICKETS` row
print identical text one screen apart. `T69`/`T70` — do not reprint the subject. **RULED: the
confirmation states the ACT and its arity; the ledger holds the legs.** This is `S77`'s form: *the
stamp states the act and its arity while the legs are marked in the flow.*

> `TICKET PLACED — 4 LEGS · $100 → $2,680` , and the `TICKETS` block below carries the legs.

**9.2 A ticket is not a line.** A four-leg `TICKETS` row measures **166 columns** on a mid-length
seed and **256** at the widest constructible, against an 80-column page. **RULED: `S92` applies to
the ledger — one leg per row.** A ticket is a small block, not a comma-joined run-on:

```
 1   $100 → $2,680                                                    4 LEGS
     WATERLOO GRAVEDIGGERS ...................................  -121
     WATERLOO SPREADSHEETS ...................................  +103
     ATLANTA BRICKLAYERS .....................................  +225
     ATLANTA OVERHEADS .......................................  +122
```

**9.3 The sweat names a leg by its ordinal alone.** On `A4` a two-leg ticket prints `LEG 1: ✘ DEAD`,
the second leg is never named, and **neither leg's market appears anywhere in the sweat.** The
player must remember what LEG 1 was. **RULED: a leg is named when its state changes** — the same
name the ledger prints, `T69`/`T70` again.

**`S88` does NOT reach this surface, and the check is worth stating because a reader comparing the
two will assume it does.** `S88`'s subject is `RevealedView`, a Unity-side class in
`TvSweatScreen.cs` whose `ResolveLeg` has one call site. **The console reads a different mirror** —
`SweatSession.RevealedLegState` in the engine, written at every `LegFinal` beat
(`SweatSession.cs:164`, `:170`, `:227`). Two mirrors, two write paths; the console's is not the
stale one.

---

## 10. THE SLATE

**10.1 The draw is priced and is not on the slate.** `Matchup.DrawOdds` is public
(`Domain.cs:369`); `RenderSlate` prints `HomeOdds` and `AwayOdds`; the header says
`(away @ home · moneyline)` and the moneyline is 1X2.

**`S74` already ruled the form and it transfers without amendment: three offers, the draw in the
MIDDLE** — its position is meaning, not convention, because the draw is the outcome where neither
wins; **named `DRAW`, never as a team and never with a team's treatment**; and `1X2` never reaches
the player.

**10.2 The slate row is 79 columns and its pads are `C46` boxes.** The widest constructible side is
`San Francisco Spreadsheets (8-0)` = 32 characters against a 28-char pad — **over by 4 on each
side, giving an 87-column row** that passes 80 as well as 62. **RULED: the slate derives its column
widths from the pool and fits the §3 page.** Whether the moneyline stays one row with three prices
or takes `S74`'s three-row form is the lane's layout to propose against the 80-column page, subject
to `S74`'s two invariants above.

---

## 11. AUTHORITY — what this surface answers to

Constitution §1.1 lists four owning documents; **the console has none.** The phone precedent
(`C26-am3`) says a surface in scope needs an authority to be judged against.

**RULED: no owning document is commissioned. The console inherits.**

- **Its words, taxonomy, order and row grammar are the laptop's** — `spec-market-surfaces-2026-08-17.md`
  plus `S89`–`S102`. It is not a fifth in-fiction surface and Reading B is off the table, so it needs
  no identity of its own.
- **Its evidence and authority rules are the constitution's**, unchanged.
- **This spec is the authority for what the medium forces it to differ on** — the page, the address,
  the pagination, the refusal points. That list is short by construction.

**If it ever grows past that list, it has stopped inheriting and the question returns to Allen.**
Commissioning a document for four differences would be the `08-art-direction.md` mistake in
miniature.

---

## 12. RAISED, NOT RULED — the console's ink

The surface uses seven `ConsoleColor`s with no stated system, and **`Green` and `Magenta` appear in
no palette in this studio.**

**I am not ruling it, and the reason is `C11`:** my evidence is plain-text transcripts with every
colour stripped. **I cannot see the thing I would be ruling on.** Ruling a colour off a source read
is precisely what `C11` forbids, and this seat's record this week is four desk reads corrected by
frames against one confirmed.

**What settles it:** one colour capture — a terminal screenshot or an ANSI-preserving transcript —
of the betting screen, a placed ticket, an error, and the sweat. **It costs one shot and it can ride
with §13's set.** Until then the build changes no colour it does not have to.

---

## 12a. FOUND WHILE CITING IT — `A2` GOVERNS AND HAS NO ROW

This spec leans on `A2` at §6.5 and §15: **the words are the engine's, the case is the surface's.**
Checking the citation before making it, `A2` **is defined nowhere in the design canon.**

Searched: `REGISTER.md`'s tables (cited six times, defined in none), every
`register-entries-*.md`, `constitution.md`, all four owning documents, `design/`, `PRODUCT.md` and
the `direction-concepts` package. **No definition.**

**What DOES exist is four shipped-source sites treating it as binding law** — `MarketSheet.cs:385`
(*"A2 already ruled it on the shipped surface"*), `SportsbookApp.cs:113`, `:589` (*"A2 deleted that
title"*), `:1402` — plus `surfaces-build-findings-2026-08-17.md` §159 (*"`A2` fixes the row label as
the engine's own DD-verbatim string"*), and **`S96` and `S96-am2`, which turn on it.** `S96`'s whole
ruling is stated as *"`A2` IS NOT OVERRIDDEN and the distinction is the whole ruling."*

**`C22`: a ruling exists when it is a row in `REGISTER.md`.** By that law either `A2` is not a
ruling — in which case `S96` rests on nothing and a lane declined to normalise casing to protect a
phantom — or it is one and the register is missing it. **`S88` was corrected on exactly this
point** three days ago: *"routing to the orchestrator's board is not enough; it takes a row now, or
the lane will not find it."*

**RAISED, NOT RULED.** I do not know `A2`'s provenance and will not reconstruct a law from four
source comments — that is how a citation becomes a law nobody authored. **It costs whoever knows
where `A2` came from about one minute, and it does not block this spec:** everything §6.5 needs is
ruled directly and independently by `S96`.

## 13. THE GATE — what must be ASSERTED

**A real advantage of this medium: every geometric gate here is a string-length assertion, which is
exact rather than measured.** Where the laptop needed an in-engine 493.69px measurement, the console
needs `line.Length <= 80`. `C46` is cheap on this surface and should be gated everywhere.

1. **The page.** No rendered line exceeds **80 columns**, on every screen this spec touches. Swept
   over the whole population of rendered lines, not sampled — `C46`'s *sweep the population, not the
   suspects*.
2. **No screen exceeds 24 rows** between one `Ui.Clear()` and the next prompt, or it paginates and
   its folio says so.
3. **`C46` against the POOL, not the seed** (`S84`, `S96-am`): the widest-name assertion is
   constructed from `Cities × Nouns` and `PlayerFirst × PlayerLast`, in-code, not from whatever the
   run deals.
4. **Reachability, both directions** (`C19`): every offer in `matchup.Markets` has an address the
   parser accepts, **and** every address the parser accepts names a printed offer. **This gate is
   what makes §7 structural.** It states its blind spot per §4.2.
5. **The folio is derived** (`S74-am3`): its numerator, range and denominator are read off the
   rendered list. A test that hard-codes 84 is testing nothing.
6. **Composer parity** (§6.6): for every kind, the console's row name equals
   `MarketSheet.NameOf`'s. One assertion, fifteen kinds — it is what keeps the two surfaces from
   drifting apart again.
7. **No two rows on a page share a name** — the laptop's `MarketSheetTests` gate, which is what
   caught the BTTS arm. It transfers unchanged.
8. **Executed-case count reported, non-zero** (`C29`).

---

## 14. EVIDENCE OWED BEFORE DESIGN-VERIFIED

Transcripts, shot the way §2's were — cheap, deterministic, and this surface's frames.

| | what it must show | pin |
|---|---|---|
| `B1` | the contents page | all six destinations, all line ranges, `CORRECT SCORE`'s child suppressed (`S102`) |
| `B2` | a destination page at its maximum | GOALS at 18 offers, folio, leaders on every row |
| `B3` | **the worst constructible row** | a **forced** matchup carrying `SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS`. **`S99`'s pin and `C55`: the subject must be IN FRAME** — a capture that happens to deal `Denver Plumbers` proves nothing |
| `B4` | PLAYERS paginating | the folio reading `66–83 of 84` and its next page |
| `B5` | an empty destination | `no prices offered` (`S89`), forced |
| `B6` | the three refusals | fifth leg at the fifth token, an out-of-range address, and a legal `1#nn` pick placed |
| `B7` | a four-leg ticket read back | the ledger block, and no line over 80 |
| `B8` | the sweat naming a leg | the dead leg named by its market, not `LEG 1` |
| `B9` | **colour** (§12) | ANSI-preserving or a screenshot — the one capture this spec cannot read |

**`B3` is the one to hold the phase on.** It is the only frame that tests the geometry §3 is ruled
on, and `S101` held the laptop's phase for exactly its twin.

---

## 15. WHAT THIS SPEC DOES NOT DO

- **It does not build a new surface.** Reading B is off the table; nothing here invents an
  in-fiction apparatus, and §11 declines to commission an owning document for one.
- **It does not re-derive the taxonomy, the order, the row grammar or the vocabulary.** All six
  destinations, `S95`'s order, `S98`'s long form, `S102`'s suppression and `MarketSheet.NameOf` are
  taken from the laptop unchanged, deliberately.
- **It rules no colour** (§12), and it changes nothing in the shop, the relics, `EventText` or
  `SweatRenderer`'s composition.
- **It does not touch the engine's words.** `A2` stands: the fields are the engine's, the case and
  the composition are the surface's.

---

## 16. NOT CLAIMED

- **No viewport claim.** These captures cannot see a terminal height. What is measured is 92 lines
  after a `Ui.Clear()`, with no position fact — which is why §3 authors a page rather than asserting
  a read.
- **No claim about how the specimens READ.** `SPECIMEN.txt` is a spec artefact showing the geometry
  is arithmetically sound. **It is not evidence.** Every read claim waits for §14.
- **No claim that the console should or should not ship.** Allen's, and the spec is right either
  way: a surface that misrepresents the slate should stop doing so whoever is looking at it.
- **`S93` and `S88` remain open on the laptop** and are untouched here.
