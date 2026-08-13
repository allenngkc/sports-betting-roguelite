# 00 — Reference list: verification + proposal

**Lane:** research (compulsion-loop references) · **Lead:** Claude (Opus 5) · **Date:** 2026-08-12
**Status:** **RULED 2026-08-12 (Allen, via orchestrator).** List APPROVED; **Buckshot Roulette ADDED** to
Tier 1 (RF-1 granted). Allen has played **Balatro** and **CloverPit** — those two autopsies weighted
accordingly, questions on his sheet at `07-questions-for-allen-from-play.md`. **RF-2 (Schüll / near-miss /
LDW literature) NOT RULED — parked on Allen's list**; the review corpus + achievement funnel are confirmed
as the substitute for play access. Deep dives ran: `02`–`05` (autopsies), `06` (mapping).
**Routes:** lead → orchestrator → Allen. Never to the DD seat (`docs/handoffs/research.md` §3).

---

## 1. Verdict in one line

All three named titles are real and correctly named — but **all three resolve in seconds, and SBR's
signature moment is a resolution that takes a minute.** The named list has no reference for the one
thing the game is built around. I propose adding one, and I propose three cheap probes the list is
also missing: a failure case, an ancestor, and the direct competitor.

---

## 2. Title verification (task 1 of the first action)

Pulled first-hand from Steam's public `appdetails` + `appreviews` APIs, **2026-08-12**. Review counts
are all-language; scores are Steam's own band.

| Named as | Actual title | Dev / Pub | Released | Price (now) | Reviews | Verdict |
|---|---|---|---|---|---|---|
| Balatro | **Balatro** | LocalThunk / Playstack | 2024-02-20 | $11.99 | 196,738 · Overwhelmingly Positive | ✅ exact |
| CloverPit | **CloverPit** | Panik Arcade / Future Friends Games | 2025-09-26 | $5.99 (from $9.99) | 25,496 · Very Positive | ✅ exact |
| Raccoin | **RACCOIN: Coin Pusher Roguelike** | Doraccoon / **Playstack** | 2026-03-31 | $9.59 (from $11.99) | 4,495 · Very Positive | ✅ real, **title is longer than canon records** |

Three corrections to the record, all small, all worth carrying:

1. **Raccoin's full title is `RACCOIN: Coin Pusher Roguelike`** and it is stylised in caps. `design/00-vision.md`
   and `design/07-business-and-roadmap.md` both use the short form. Harmless in prose, wrong in a citation.
2. **RACCOIN is published by Playstack — Balatro's publisher.** Not recorded anywhere in canon. This makes
   RACCOIN the best available read on *what the genre's most successful publisher currently believes ships*,
   which is a different and more useful question than "was it fun".
3. `00-vision.md` names a fourth and fifth reference — **Scritchy Scratchy** and **Parlay** — that the lane
   charter does not. Both verified below. Parlay is the load-bearing one.

### The two canon-named titles the charter omitted

| Title | Dev / Pub | Released | Price | Reviews |
|---|---|---|---|---|
| **Scritchy Scratchy** | Lunch Money Games / Funday Games | 2026-03-18 | $5.59 (from $6.99) | 14,629 · Very Positive |
| **Parlay** | Urple / Urple (self-pub) | **`Coming soon` — still unreleased at 2026-08-12** | — | 0 · no reviews, **no demo** |

**Parlay is the standing action item in `07-business-and-roadmap.md` ("check its launch performance when
it ships"). It has not shipped.** Thirteen months after canon logged it as "unreleased as of Jul 2026" it is
still `coming_soon: true`, self-published, with no demo and no announced date. Two consequences: the free
market research canon is waiting on has not arrived, and **the competitor cannot be autopsied by play** —
only from its public materials. See §5.

---

## 3. The hole in the named list — the finding that drives this proposal

`design/00-vision.md` pillar 1: *"The sweat is sacred. Leg-by-leg resolution with a live cash-out offer is
the signature moment. Nothing may make resolution instant or skippable by default."*

Observed: every one of the three named references makes resolution **instant by default**.

| Reference | What "resolution" is | Roughly how long | Player agency during it |
|---|---|---|---|
| Balatro | hand scores, chips tick up | ~1–3 s | none |
| CloverPit | reels stop, payout resolves | ~2–4 s | none once spun |
| RACCOIN | coins fall, cascade settles | ~3–8 s | none once pushed |
| **SBR (canon)** | **N legs resolve one at a time with live cash-out offers** | **a phase, not a beat** | **the whole point** |

Inference (falsifiable): a reference set whose members all resolve in under ten seconds can teach SBR its
**item economy, target curve, and shop grammar** — and can teach it **nothing about its signature moment.**
What would falsify this: if the autopsies find that one of the three sustains tension across a multi-second
resolution the player merely watches, that mechanism transfers and the hole closes. I do not expect it.
CloverPit's dread is between spins (the debt counter), not inside them.

**Proposal RF-1: add one reference chosen specifically for resolution tension, and autopsy it against the
sweat.** My pick is **Buckshot Roulette** (Mike Klubnika, 2024-04-04, $2.99, 123,710 reviews ·
Overwhelmingly Positive). It is the cheapest, most-reviewed study available of a game whose *entire* design
is one binary random outcome the player has to sit inside. It is not a roguelite and does not need to be —
we are stealing a cadence, not a structure. `design/04-the-sweat.md` is an outcome-first drama generator
with explicit pacing dials and no reference game behind those dials. This is that reference.

---

## 4. Proposed reference list

Tiered by cost, because a full autopsy is expensive and a probe is not.

### Tier 1 — full autopsy (4)

| # | Title | Why it earns a full autopsy |
|---|---|---|
| 1 | **CloverPit** | Named. **Scoped: the felt half only** — see the non-duplication clause below. |
| 2 | **Balatro** | Named. The genre's grammar, and canon already borrows its meta model (`01-core-loop.md`, "unlocks rather than power creep — Balatro model") without ever having written down why that model works. |
| 3 | **RACCOIN: Coin Pusher Roguelike** | Named. The current juice-and-price bar, and the Playstack read (§2.2). Four months old — the freshest full data in the set. |
| 4 | **Buckshot Roulette** | Added by me (RF-1). The only resolution-tension reference available. Serves pillar 1 and `04-the-sweat.md`'s pacing dials. |

**Non-duplication clause (CloverPit).** `design/09-cloverpit-math-comparison.md` (2026-07-12) already did
CloverPit's math — payout formula, symbol weights, luck/pity schedule, requirement curve, charm taxonomy —
and `design/11` already shipped 17 items translated from it. **This lane does not re-open any of that.**
The CloverPit autopsy covers only the four charter dimensions (result cadence, compulsion levers, session
shape, meta hooks), and opens with a delta section naming what design/09 already settled. If the autopsy
contradicts design/09 on a number, that is a finding routed to Allen, not a quiet correction.

### Tier 2 — targeted probe (4)

A probe answers **one** named question with evidence and stops. No template, ~1 page each.

| # | Title | The one question |
|---|---|---|
| 5 | **Gambonanza** (2026-05-01, $14.99, 1,601 reviews · **Mostly Positive**) | *Why did this one land soft?* The studio has four success references and zero failure references. This is the only recent gambling roguelite in the set that underperformed on reception — and it is the most expensive of them. The negative case is worth more per page than a fifth success. |
| 6 | **Luck be a Landlord** (2023-01-06, $9.99, 11,517 reviews) | *How does a rent-escalation curve fail after three years of tuning?* It is the ancestor CloverPit's debt framing descends from, and `design/10 A` just moved SBR onto debt **payments**. Known failure modes of the ancestor are cheap insurance on a structural decision taken five weeks ago. |
| 7 | **Dungeons & Degenerate Gamblers** (2024-08-08, $7.49, 3,500 reviews) | *How does a real gambling game's vocabulary become a roguelite without a tutorial wall?* It converts actual blackjack. That is pillar 2's exact problem (`00-vision`: "jargon is the mastery layer, not the entry fee") and no reference in the named list solves it — Balatro's poker is cosmetic, CloverPit's slots need no vocabulary at all. |
| 8 | **Scritchy Scratchy** (canon-named, 2026-03-18, $5.59) | *Does the $8–13 band in `07-business-and-roadmap.md` still hold?* It sits below the band with 14,629 reviews. Also the clean study of maximum juice with near-zero decision — a ceiling worth knowing. |

### Tier 3 — watchlist, no spend now

- **Parlay** — **standing watch, not an autopsy.** No build exists to autopsy. Proposed instead: a one-page
  teardown from public materials now (store copy, trailer, devlogs), then a re-check on a cadence Allen sets.
  See §5 — I found something in the store copy that should reach him before the deep dives.
- **Slots & Daggers** ($7.99, 7,439 reviews) · **Bingle Bingle** (2026-06-22, $11.99, 682) ·
  **Dungeon Clawler** (2026-04-30, $14.99, 3,930) · **Ballionaire** (2024-12-10, $12.34, 2,580) — genre
  breadth. Pull into Tier 2 only if a Tier-1 autopsy raises a question one of them answers.
- **Sports Betting Simulator** (2021-12-06, $0.99, **3 reviews**) — one line in the mapping doc as a negative
  control: the literal genre name, shipped, and nobody came. Worth exactly one line.

### Tier 2b — one non-game source, flagged because it is a scope call

**Proposal RF-2: one probe on the real slot-machine literature** — Schüll's *Addiction by Design*, and the
published near-miss / losses-disguised-as-wins research. Reason: this lane's job is to inventory compulsion
levers, and the industry that invented them documented them. It is also the honest read on pillar 4 (*satire,
not glorification*) — you cannot satirise a technique you have not named, and you cannot decide which levers
we refuse to ship until they are on one page. **Flagged, not assumed**: it is not a game, and it may be
scope Allen does not want. His call.

---

## 5. Flagged for Allen before the deep dives — Parlay's structural convergence

I pulled Parlay's full store description first-hand (Steam `appdetails`, 2026-08-12). Verbatim:

> "Build high-stakes parlays across **8 grueling rounds** of fictional football, basketball, baseball, and
> hockey matchups. Each round, make **3 escalating sets of picks** — from safe doubles to outrageous
> **6-leg** gambles... **You owe money. A lot of it.** Win enough to survive each round's **crushing debt**…
> or lose it all and disappear into **the bookie's ledger** forever." · "**Limited Buff Slots.**" ·
> "**Fixed odds system that worsens over time.**"

Against SBR canon:

| Parlay (marketing copy) | SBR (canon) |
|---|---|
| 8 rounds | a season of N rounds (`01-core-loop`) |
| 3 sets of picks per round | 3 concurrent tickets, upgradable (`01-core-loop`, DECIDED 2026-07-07) |
| doubles → 6-leg | singles → 6-leg parlays (`01-core-loop`) |
| procedural fictional teams, "Atlanta Yams" | same — canon cites the Yams by name (`00-vision`) |
| debt to a bookie; the bookie's ledger is the fail state | debt model; *"you were never playing against the sports — you were playing against your bookie"* (`01-core-loop`, DECIDED 2026-07-09) |
| limited buff slots | 15 passives + 7 consumables, dealt-hand shop (`design/11`) |
| fixed odds worsen over time | vig creep — canon knows, and differentiates by worsening **reactively** |

**Observation:** six of seven structural elements match. **Inference:** `00-vision` states our
differentiation as *"the sweat + cash-out, the information axis, and real betting-edge concepts as
mechanics."* Two of SBR's most recent structural decisions — debt-as-HP → debt payments (2026-07-12) and
bookie-as-antagonist (2026-07-09) — moved *toward* the competitor's advertised design, not away. The
differentiation is now carried entirely by the sweat, the information axis, and getting-limited.

**What I am not claiming:** that anything was copied in either direction. Both games model real sports
betting, where a bookie and a debt are the obvious fiction; convergence is the expected outcome, and canon
logged Parlay as a known competitor from the start. Marketing copy is also not a build — Parlay may not
play like its store page.

**Proposal RF-3:** the differentiation has to be legible in the first thirty seconds of a store page, not
only in the design docs — because on a store page the two products currently describe themselves the same
way. This is a claim about `00-vision` §"Reference games" and `07-business`, and I raise it as a proposal
under the standing mandate, not as a finding requiring action now.

---

## 6. What this lane can and cannot measure — read before ruling

I cannot play any of these games. Docs-only, and no build access. **Every autopsy is therefore
second-hand unless Allen supplies first-hand play.** Stating the instrument scope up front is this
studio's own law (`C25` — instrument scope is part of a measurement), so the autopsies will each declare
their evidence basis and a confidence ceiling in the header.

Two public instruments are stronger than "watch a video", and I verified both work today:

1. **The review corpus.** Steam's `appreviews` API returns full review text plus `playtime_at_review`,
   timestamp, and vote direction, per review, unlimited paging. CloverPit alone has 13,027 English reviews.
   This turns "what makes it compulsive" into a counted claim: lever language, quit language, and a real
   playtime distribution rather than a guess at session length. Sampling method and n get recorded so any
   number is re-derivable (`C34` — evidence that cannot be reproduced is not a set).
2. **The achievement funnel.** Steam's global achievement percentages are public per title (verified:
   CloverPit returns 30). "What fraction of owners ever finished a run" is a measurement, not an opinion —
   and it is the only honest read on session shape and drop-off available without telemetry.

Where those two cannot reach — moment-to-moment feel, exact resolution timings, the texture of a near-miss —
the autopsy will say so in the field rather than fill it in. **The cheapest fix is Allen: if he has played
any of these, his raw impressions are the only first-hand channel this lane has.** Which ones he has played
is the question with the largest effect on output quality.

---

## 7. Need Allen — ruling requested

1. **Approve the list?** Tier 1 = CloverPit (scoped) + Balatro + RACCOIN + **Buckshot Roulette (new)**.
   Tier 2 = Gambonanza, Luck be a Landlord, Dungeons & Degenerate Gamblers, Scritchy Scratchy.
   *Recommend: yes.* The added Tier-1 title is the one that serves pillar 1; the probes are cheap.
2. **Does the CloverPit autopsy exclude the math** already settled in `design/09`? *Recommend: yes* —
   otherwise we pay twice for July's work.
3. **RF-2 — is the real slot-machine literature in scope?** Not a game. *Recommend: yes, one probe* —
   it is where the lever vocabulary actually comes from, and where pillar 4's line gets drawn.
4. **Which of these have you played?** Name them and I will mark those autopsies first-hand and lift their
   confidence ceiling. This is the single highest-leverage input you can give this lane.
5. **Parlay cadence** — how often should I re-check for its launch? It is your standing action item and it
   is still unfired. *Recommend: monthly, one line, until it ships; a full teardown the week it does.*
6. **RF-3** — do you want the differentiation-legibility question opened as a real proposal against
   `00-vision`, or noted and parked? *Recommend: parked until the mapping doc, which is where it belongs.*
7. **Prefix for this lane's items.** I have used `RF#` (research finding) above. `R#` is Room's in the
   design register — do not let these collide. Confirm `RF#`, or name another.

---

## 8. Sources

All Steam figures pulled first-hand 2026-08-12 from the public endpoints
`store.steampowered.com/api/appdetails`, `store.steampowered.com/appreviews`, and
`api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2` — store data is live and
will drift; re-pull before quoting any number in a launch decision.

- Balatro — https://store.steampowered.com/app/2379780/Balatro/
- CloverPit — https://store.steampowered.com/app/3314790/CloverPit/ · https://en.wikipedia.org/wiki/CloverPit
- RACCOIN: Coin Pusher Roguelike — https://store.steampowered.com/app/3784030/RACCOIN_Coin_Pusher_Roguelike/ · https://en.wikipedia.org/wiki/Raccoin:_Coin_Pusher_Roguelike
- Parlay — https://store.steampowered.com/app/3592780/Parlay/ (store copy quoted in §5 retrieved 2026-08-12)
- Scritchy Scratchy — https://store.steampowered.com/app/3948120/Scritchy_Scratchy/
- Buckshot Roulette — https://store.steampowered.com/app/2835570/
- Gambonanza — https://store.steampowered.com/app/3509230/ · Luck be a Landlord — https://store.steampowered.com/app/1404850/
- Dungeons & Degenerate Gamblers — https://store.steampowered.com/app/2400510/ · https://en.wikipedia.org/wiki/Dungeons_%26_Degenerate_Gamblers
- Slots & Daggers — app 3631290 · Bingle Bingle — app 2789810 · Dungeon Clawler — app 2356780 · Ballionaire — app 2667120 · Sports Betting Simulator — app 1740550

Canon read for this proposal: `design/00-vision.md`, `01-core-loop.md`, `03-mechanics-catalog.md`,
`04-the-sweat.md`, `07-business-and-roadmap.md`, `09-cloverpit-math-comparison.md`, `10-economy-rework.md`,
`11-charm-expansion-prototype.md`.
