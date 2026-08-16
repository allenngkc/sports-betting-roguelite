# Market surfaces — research pass, and the design read off it

**Written:** Design Director seat, 2026-08-16 · **Mandate:** `docs/5-orchestration/dd-mandate-2026-08-16.md`,
Phase 1 · **Status:** research complete; **no spec is issued here and no material choice is taken.**

This is step one of Phase 1: study how real sportsbooks organise dozens of markets per event, and
report what transfers to our kit and what does not. The board and ENTRY spec follows Allen's calls
on §5.

---

## 1. Evidence quality, stated first (C18 §4.2)

The research was run by a subagent under a read-only brief; it wrote nothing and committed nothing,
verified. What it returned separates cleanly into three tiers, and the tiers matter:

**Verified by direct query of live public pages.** DraftKings' taxonomy, read out of its URL
contract (`?category=…&subcategory=…&nav_1=…`). This is the only first-hand structural evidence in
the set, and it is good: a genuine three-level tree, confirmed live.

**Practitioner teardowns.** One production sportsbook design team's own component inventory is the
single best source in the corpus — it gives component states, clamp rules, contrast floors and a
CLS budget, all specific.

**Reviews and how-tos.** Density counts and interaction counts. Useful, softer.

**The gaps, which are real.** **bet365's event pages returned 403 on every attempt, so its actual UI
tab names are UNVERIFIED** — what we have is its *rules* index, which may not match the interface.
DraftKings' JSON API also 403'd, so the subcategory lists are **lower bounds**: absence is not proof
of absence. And live browser inspection was available but **deliberately not used** — driving
Allen's authenticated browser into gambling sites was not a call for a subagent to make. That was
the right refusal, and it leaves the biggest gap open: **no first-hand screenshots of bet365 or
FanDuel event pages.** If Allen wants that closed it has to run from his own seat.

Two findings are explicitly labelled guesses by the researcher and are carried here as guesses: the
*cause* of DraftKings' within-group ordering, and a Pinnacle market-count figure that appeared in a
search snippet and could not be confirmed in the page.

---

## 2. What the books actually do — the five findings that bear on us

**The taxonomy is a hybrid, and every book converged on the hybrid.** Groups are named by
STATISTIC where the thing is physical and countable (Goals, Corners, Cards), and by BET TYPE or
SUBJECT where it is abstract (Player Props, Halves, Handicaps, Futures). Nobody picks one axis and
holds it. Two books at opposite ends of the market disagree on almost everything else and still
both land here.

**Popularity leads the group rail; a fixed config order governs inside a group.** Verified on
DraftKings: the top rail is popularity-led, while the subcategory order is stable config (Player
Tackles leads Player Props, and tackles is not a popular market).

**Empty groups are not rendered at all.** Verified: on one league DK renders Corners, Player Props,
Team Props and Halves; on another, only Matches and Futures. The group rail is generated per-event
from what is actually priced. **No book prints a heading over nothing.** See §4.1 — this is the
finding we should deliberately invert.

**The row is a component with six states**, from a production book's own inventory: default, hover,
selected, suspended, price-up, price-down. Market names are **clamped at two lines with
tap-to-expand** because "market names can be essays." Price movement gets **colour AND icons**;
suspension greys out, ignores clicks, **and is announced**. Layout is held to CLS < 0.07 — "price
moves blink politely, then settle."

**The long tail is not solved, it is managed.** A deep soccer fixture carries 200–300+ markets.
Search indexes **events, not markets** — every book, without exception. What actually keeps market
#97 findable is popularity-led grouping plus collapse-by-default, and reviewers still complain:
"deep menu trees, tiny tap targets, sidebars that require pinching" is named the single biggest
design failure in mobile sportsbooks. A usability study found experienced users coped while less
experienced ones were overwhelmed.

---

## 3. What transfers

1. **The hybrid taxonomy.** Our vocabulary maps onto it almost exactly — countables take statistic
   names (**Goals, Corners, Cards**), abstractions take bet-type names (**Result, Handicaps,
   Correct Score, Team Totals, Odd/Even**). Convergent evidence, not laziness. Do not purify it.
2. **Popularity-first at group level, stable printed order within a group.** The right split for
   us: the first group should be the one the player wants, but inside a group **a printed order
   that never changes between readings** is what a form guide is. This also satisfies §2's
   derived-once, cannot-change-under-the-player standard that the COUNTS panel was held to.
3. **Two-line clamp with expand.** A typographic rule, and the one specific thing standing between
   us and ragged rows at the 13px floor.
4. **Suspended = greyed, non-clickable, and stated.** A production book independently arrived at
   our own law: they announce suspensions rather than relying on grey. **Our "status is never
   carried by colour alone" is not a handicap here — it is what the competent end of the industry
   already does.** Worth recording, because that law has been argued as a cost before.
5. **No layout wobble; blink-then-settle.** On a fixed world-space canvas we have no excuse for
   reflow at all, and a paper register that twitches destroys the diegesis instantly.
6. **Collapsible group headers that keep context** — compatible with the position rail.

## 4. What does not transfer, and what it costs us

- **Hover.** It does real work in their component set — it previews affordance before commitment.
  We have no reliable cursor hover on a diegetic in-world laptop. **The cost is real:** we must buy
  that signal back with a persistent always-visible affordance, and that costs ink on a ruled
  ground. The failure mode to avoid is a row indistinguishable from printed text.
- **Colour-coded price movement.** Banned twice over — oxide red is the house's mark, and status
  may not be colour-only. **We lose at-a-glance drift across a sheet.** The honest replacement is
  the bookmaker's-board gesture: the old price struck through, the new one written beside it. It is
  slower to read and takes more width. **Budget for it rather than discovering it late.**
- **Infinite scroll and virtualisation.** They virtualise because 200–639 markets will not fit a
  DOM budget. At ~80 offers it buys nothing and actively **breaks the position rail's honesty** — a
  rail reading "46 of 80" must be backed by 80 real rows, or the number is a lie inside a game
  about being lied to.
- **Responsive reflow** — and the second-order consequence matters more than the obvious one:
  **their "what mobile cuts" guidance is driven by WIDTH.** We are desktop-wide and roughly
  mobile-short. **Nothing in that literature tells us what to cut from a short-wide surface. That
  guidance does not exist and we will have to derive it.**
- **Browser `Ctrl+F`.** Desktop books quietly free-ride on it — which is a large part of why none
  of them ships market-level search. **We have no OS text-find, so an in-world find affordance is
  not a nicety; it replaces a missing platform capability.** But a search *field* is a web
  register, foreign to ruled paper. §4.2 below is the alternative.
- **The double-tiered rail.** DraftKings ships it and is rated down for it by every comparison
  found. At our scale one level of grouping is sufficient. **Do not build tier two.**
- **Horizontally-scrolling chip rails.** Off-screen chips are invisible, unsearchable and
  unprintable. **A paper register cannot have headings that run off the edge of the page — the
  medium forbids it, and the medium is right.**

---

## 5. THREE MATERIAL CHOICES — Allen's, not this seat's

### 5.1 Print the empty groups, against the whole industry

Every book silently drops a group with nothing priced. Our law says the slate must not be
misrepresented. **Proposal: print the empty group with a ruled zero — `CORNERS ….. no prices
offered` — and print a count beside every group that has offers — `CORNERS ….. 11`.**

Costs perhaps six lines. Buys three things: the player can *see* the house is not offering corners
today, which is information the real books destroy; the contents block becomes a fixed-length
object rather than a shape-shifter; and **C19's reachability stops being an invisible engineering
promise and becomes a legible feature of the fiction.** A racecard prints the race even when it is
abandoned.

**Recommend: yes.** It is cheap, it is on-register, and it converts a law into something the player
can read.

### 5.2 The position rail as a printed folio, and a contents block instead of search

The most durable complaint in the corpus is orientation — "buried", "endless", novices overwhelmed.
Every book answers with a chip rail and a scrollbar, and reviewers still complain. **A form guide
answers with a printed contents list and a page number, and nobody has ever complained that a
racecard is hard to navigate.**

So: the position rail is **not** a scrollbar analogue, it is a folio. `46–66 of 80` is a *fact*; a
scrollbar thumb is a proprioceptive hint. And a contents block — group name, printed line range,
count — replaces the search field entirely at this scale, in a register native to the medium.

**At ~80 offers with a contents block, worst-case navigation is 3 interactions (contents → group →
row) against DraftKings' ~7.** That is not parity with the books; it is a win, and it is available
only because we are small and made of paper.

**Recommend: yes**, and it subsumes the "we have no Ctrl+F" problem in §4 without importing a
search field.

### 5.3 Invert the price-first hierarchy — the one that changes how the sheet looks

Every book leads with the price: the pill is the largest, boldest, highest-contrast element, and
all their density bragging is counted in *prices visible*, never in *market names visible*.

**A form guide leads with the runner.** Since our register is literally "The Annotated Form Guide":
**lead with the market name in the typeset layer, and let the price sit in the annotation column in
amber/wax.**

Three payoffs. It is the diegetically correct hierarchy. It separates **LINE** (printed, fixed,
part of the form) from **PRICE** (annotated, the house's offer, mutable) along **the exact seam our
colour law already draws**. And it means the only element that must hold strict numeric precision
at 13px is the price, so the typography budget goes where it matters.

**This is the material one.** It will make our sheet look *less* like a sportsbook at a glance —
deliberately. **Recommend: yes, and it should be a deliberate ruling rather than a drift**, because
once the board is built price-first it will not be cheaply reversed.

### 5.4 Scope question, not a design one — the cross-event pivot

The one thing users actually ask for and no book ships: markets **pivoted across events** ("every
corners market on the slate") rather than siloed per match. Nearly free for us, since the engine
prices a known market set per matchup. **Genuinely unmet need in the real category.**

**Not recommended for this phase** — it is a new surface, not a presentation pass on an existing
one. Recorded so it is not lost, and so it is not smuggled into the board spec by accident.

---

## 6. What this seat must measure before any spec

The researcher offered density arithmetic for our canvas — roughly 21 rows per screen at 13px in
26px rows, putting ~80 offers under four screens, and concluded we are "not density-constrained."

**That is arithmetic, not a measurement, and it is about OUR surface rather than theirs.** Under
C11 and the constitution's *measure the rendered thing, not the source*, it is not a spec input
until this seat measures it on a rendered board against the real kit — row pitch, the actual chrome
and slip-strip reservations, and the 13px floor as it renders rather than as it is authored.

**It is a promising direction and it is explicitly not yet evidence.** Batch 95 is the standing
lesson: a column's widest string is a measurement, never something readable off type sizes — and
that correction cost this seat two wrong predictions in one week.

---

## 7. Owed next

1. Allen's calls on §5.1, §5.2, §5.3 (and confirmation that §5.4 stays out of scope).
2. This seat measures §6 on a rendered board.
3. Then the board and ENTRY spec, against the approved calls — after which the markets-pregame lane
   revives to build.
