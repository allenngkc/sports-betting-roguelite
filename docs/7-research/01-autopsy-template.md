# 01 — The fun autopsy: template + rules

**Lane:** research · **Lead:** Claude (Opus 5) · **Date:** 2026-08-12
**Status:** PROPOSAL — awaiting Allen. One autopsy per Tier-1 title, copied from §B verbatim.

---

## A. The five rules the template enforces

These are why the template has the shape it has. They are the studio's own measurement culture applied
to research claims (`docs/handoffs/research.md` §4).

1. **Observation and inference never share a paragraph.** Every section is split: what the evidence shows,
   then what I think it means. An observation cites its source and date. An inference does not get to
   borrow the observation's confidence.
2. **Every inference states its falsifier.** "What would have to be true for this to be wrong, and could we
   see it?" An inference with no writable falsifier is labelled a **hunch** and marked as one — that is a
   legal outcome, not a failure. Unmarked confident prose is the failure.
3. **The instrument is part of the measurement** (`C25`). The header declares evidence basis and confidence
   ceiling before any claim. An autopsy built from footage cannot make felt-experience claims at the same
   confidence as one built from play, and must not write as if it can.
4. **Unanswerable is an answer — write it.** A field that the available instruments cannot reach gets
   `UNREACHED — <why, and what would reach it>`. A blank field and a guessed field are both defects. Coverage
   is reported, never inferred (`C28`).
5. **Numbers are re-derivable** (`C34`). Any count, distribution, or percentage records its source endpoint,
   its n, its sampling method, and its pull date. A number nobody can re-pull is an anecdote.

---

## B. The template — copy from here down

---

# NN — Fun autopsy: <TITLE>

**Autopsied:** <date> · **By:** research lane (Claude, Opus 5) · **Status:** DRAFT / ROUTED / RULED

## 0. Identity and instrument

| | |
|---|---|
| **Full title** (as the store stylises it) | |
| Developer / Publisher | |
| Released / Price at pull / Reviews + band | |
| Store + wiki URLs | |

**Evidence basis** — tick every channel actually used, with volume:
`played (hrs, by whom)` · `long-form footage (hrs, links)` · `wiki/mechanics guides` ·
`review corpus (n, sampling method, pull date)` · `achievement funnel (pull date)` ·
`dev commentary / postmortems` · `store materials only`

**Confidence ceiling:** HIGH (played) / MEDIUM (footage + corpus) / LOW (materials only).
One sentence on what this basis structurally cannot see.

**Prior canon on this title:** name any existing SBR doc that already covers it and what it settled, so
this autopsy does not re-open it. (For CloverPit: `design/09-cloverpit-math-comparison.md`, the math —
excluded by the reference-list ruling. A contradiction with it is a routed finding, never a quiet fix.)

---

## 1. Result cadence — the game's clock

> The charter dimension SBR cares about most, because pillar 1 lives here.

**Observed**

| Measure | Value | How measured |
|---|---|---|
| Commit → first information | s | |
| Commit → resolution complete | s | |
| Player decisions per resolution | n | |
| Agency *during* resolution | none / named | |
| Resolution skippable or speed-uppable? Default state? | | |
| Granularity — one result, or a stream of sub-results? | | |
| Who paces it — player input, or the game's clock? | | |
| Dead time per loop (no decision, no information) | s | |

Timings are min/median/max over ≥5 sampled loops, with the sampling source named. A single stopwatch
reading is not a cadence.

**Inferred** — what the cadence is *for*, each line with its falsifier.

**Against SBR:** SBR's resolution is a phase (N legs, live cash-out), theirs is a beat. Say plainly whether
anything here survives that translation, or whether the section is structurally inapplicable — and if it is,
say that instead of stretching it.

---

## 2. Compulsion levers — what produces "one more"

**Observed — lever inventory.** One row per lever actually present. Standard vocabulary so the
cross-game table works; add rows for anything the vocabulary misses and name it.

| Lever | Present? | Where it fires | Frequency / magnitude | Evidence |
|---|---|---|---|---|
| Near-miss (visible almost-win) | | | | |
| Loss disguised as win (payout < stake, celebrated) | | | | |
| Variable-ratio reward | | | | |
| Escalating target / ratchet on the requirement | | | | |
| Debt, interest, or sunk cost | | | | |
| Permanent in-run growth (ratchets) | | | | |
| Pity / deterministic mercy | | | | |
| Jackpot or ceiling made visible before it is reachable | | | | |
| Forced-choice regret (the road not taken stays visible) | | | | |
| Collection / completion pressure | | | | |
| Unlock drip across runs | | | | |
| Restart friction (seconds to next run) | | | | |
| <other, named> | | | | |

**Observed — the review corpus.** The lever language players themselves use, counted. n, sampling
method, and pull date recorded. Quote 3–5 representative reviews verbatim with playtime-at-review attached.

**Inferred — which levers carry the weight.** Ranked, each with a falsifier. The honest form is usually
"the corpus mentions X in k of n reviews and never mentions Y, which is weak evidence X is load-bearing."

**Pillar-4 read (`00-vision`, *satire, not glorification*).** Which levers here are the real gambling
industry's, whether the game endorses or satirises them, and — separately — whether SBR should ship them
at all. This section is allowed to recommend refusing a lever that works.

---

## 3. Session shape — the arc, and where players stop

**Observed**

| Measure | Value | Source |
|---|---|---|
| Rounds per run (design) | | |
| Run length — min / median / max | | |
| Where run tension peaks (which round) | | |
| Loss rate / how a run ends | | |
| Seconds from run-over to next run | | |
| Playtime distribution from the review corpus (median, IQR, n) | | |
| Share under 2h (refund window) / over 20h / over 50h | | |
| **Achievement funnel** — % of owners reaching each milestone | | Steam global achievement % , pull date |
| **Rarity floor** — the rarest achievement in the game, and the count | | same pull |

The achievement funnel is this lane's best public read on drop-off: the beat-round-1 percentage against
the finish-a-run percentage says how many owners ever saw the design's back half. Record the raw rows,
not just the conclusion.

**The rarity floor** (added 2026-08-12, from the Scritchy Scratchy probe) is the rarest achievement a title
has. It measures how much of a game is reachable by almost everybody, and it separates *engagement* from
*completion space*. Observed range across the first eight titles: 0.1%–1.5%, except Scritchy Scratchy at
**12.1%** — nothing in that game is hidden from the median player, and its tail is 3.6% over fifty hours
despite an 11.7h median. **A high median with a low tail is the signature of a game everyone finishes.**

**Inferred** — the shape of the arc, the real quit point, and whether the design's intended session
matches the observed one. Falsifier per line.

**Against SBR:** `01-core-loop.md` still has "round count and session length target" open (Balatro ≈ 30–60
min). This section is where that open question gets evidence rather than a straw man.

---

## 4. Meta hooks — what pulls a player back tomorrow

**Observed**

| Measure | Value | Source |
|---|---|---|
| What unlocks between runs (content / power / difficulty / cosmetic) | | |
| Drip rate — unlocks per run, early vs late | | |
| Difficulty ladder (ascension analog) — depth, and % of owners who climb it | | |
| Completion surface (collection, seeds, dailies, leaderboards) | | |
| Does meta gate *content* or gate *difficulty*? | | |
| Is any meta progression power creep? | | |

**Inferred** — what the meta is actually doing, with falsifiers.

**Against SBR:** `01-core-loop.md` commits to "unlocks rather than power creep — Balatro model" and leaves
"how much meta is too much for scope?" open. Say whether this title's evidence supports or undercuts that
commitment. Undercutting it is a proposal, and proposals are this lane's job.

---

## 5. The thing a summary would miss

One paragraph. The single most distinctive decision in this game that none of §1–§4 captured — the reason
this title is remembered rather than merely played. If there isn't one, say that; for some titles the
absence is the finding.

## 6. Transfer to SBR

Every line names the canon doc and section it touches, and the mechanics axis it lands on
(`03-mechanics-catalog.md`: 1 Information · 2 Odds · 3 Capital · 4 Resolution · 5 Economy/Meta). This is
what makes the mapping doc assembly rather than re-analysis — so a line with no canon reference is not done.

**STEAL** — transfers roughly as-is.

| What | Axis | Canon it lands in | Cost to try | Why it survives translation |
|---|---|---|---|---|

**CONFLICT** — argues against something canon has already decided. Numbered `RF#`, written as a proposal
with the canon clause quoted, the research argument, and what Allen is being asked to rule. Fearless is the
instruction; sourced is the condition (`docs/handoffs/research.md` §3).

| RF# | Canon clause (quoted) | What the research argues | Ruling requested |
|---|---|---|---|

**REJECT** — attractive and wrong for us. Reason required. The usual reason: their resolution is
instantaneous and unwatched, ours is a live multi-leg phase, so the mechanism has nowhere to attach.

| What | Why it fails for SBR |
|---|---|

## 7. The comparison row

One row per autopsy, fixed schema, so the mapping doc's cross-game table is assembled and never re-derived.
Copy this row filled; do not vary the columns.

`title | released | price | reviews(n, band) | commit→resolve (s) | decisions/loop | agency during resolve | rounds/run | run length (median) | escalation curve | failure model | item composition (add/mult/both) | pity? | unlock model | % owners finishing a run | evidence basis | confidence`

## 8. Sources

Per source: URL, access date, and **what it could and could not establish**. A wiki settles mechanics and
not feel; a review corpus settles language and not intent; a store page settles nothing but marketing.

---

## B2. The frozen lexicon — use exactly this, or the cells do not compare

Added 2026-08-12 after the Tier-2 probes found drift in my own method. The Tier-1 and Tier-2 pulls used
slightly different patterns, which made two families non-comparable across the table (`C44` — a bound and
the reading it judges must come from one instrument). **Do not edit these patterns. If a family must
change, re-run every prior title against the new pattern before publishing a comparison.**

```
addiction      addict|hooked|dopamine|\bcrack\b|can'?t stop|couldn'?t stop|cannot stop
boredom_quit   repetitive|got (boring|old|stale)|gets (boring|old|stale)|grindy|\bgrind\b|
               burn(ed|t) out|stopped playing|same every|samey|monoton
luck_vs_skill  \bluck\b|\brng\b|random|skill|strateg|synerg|combo|build
dread_tension  tense|tension|anxiet|anxious|stress|heart (was )?(racing|pounding)|palms|sweat|
               dread|terrif|scary|nerve|adrenaline
gambling_real  gambling|casino|slot machine|real money|problem gambl|degenerate
price_value    not worth|overpriced|wait for (a )?sale|\bprice\b|\$\d|too expensive|worth (the|every)
too_hard       too hard|unfair|brutal|punishing|impossible|\bwall\b|bullshit
too_easy       too easy|no challenge|easy once|breeze
onboard        tutorial|learn|confusing|didn'?t know|hard to (understand|follow)|
               easy to (pick up|learn)|explain|teaches|rules
depth_thin     shallow|not enough content|lacks (depth|content)|too short|thin|no depth|surface level
one_more       \bone more\b|\bjust one more\b|\blast (run|spin|game|hand)\b
cash_out       cash ?out|cash(ing)? out|double or nothing|walk away|quit while
```

**Two rules that come with it.**

1. **Contamination check, every title, before publishing a number.** If a pattern term appears in the game's
   own title or is its core verb, that family is unusable for that title and must be struck, not footnoted.
   Two live examples: `scratch` inside *Scritchy Scratchy*, `degenerate` inside *Dungeons & Degenerate
   Gamblers* — both inflate `gambling_real` mechanically.
2. **Two families are known-dead and are not to be reported as findings.** `near_miss` returned 1 hit in
   4,000 reviews and `cash_out` 12 in 4,000 — Steam reviewers do not narrate moments. Run them if you like;
   a null from either is an instrument failure, never evidence of absence (`C37`).

Sampling is fixed too: `appreviews`, `language=english`, `purchase_type=all`, `filter=recent`,
cursor-paged to n=1,000 (or the full pool if smaller — record which). Recent-filter skews toward the current
build and sale cohorts; say so in §8 every time.

## C. Notes on running the template

- **Cheapest order:** identity → achievement funnel → review corpus → footage → the felt sections. The two
  API instruments are minutes of work and they anchor every later judgement, so pulling them last wastes
  the anchor.
- **Length target:** 3–5 pages per Tier-1 autopsy. A probe (Tier 2) uses §0, the one question, and §6 —
  nothing else, ~1 page.
- **When the instruments disagree** — corpus says one thing, footage another — record both and say which
  you believe and why. Do not average them.
- **A finding that contradicts SBR canon goes in §6 CONFLICT with an `RF#`, and nowhere else.** This lane
  never edits `docs/design/**` or `design/**`. Proposals route lead → orchestrator → Allen.

## D. The per-game card (Allen, 2026-08-13) — the reading format

The autopsy above is the *working* form. The **card** is the reading form, and every studied title carries
one in `19-per-game-cards.md`: **addicting elements · fun core · quoted mechanic · our application**, then
honest gaps. Any new title gets a card in `19` in the same commit as its autopsy or probe.

- **Addicting and fun are separate fields and must not be merged.** Keeping them apart is what makes the
  set legible — see `19` §4, and RF-8/RF-14 for the finding it encodes.
- **Quoted mechanic means verbatim** — store copy, a review, or an achievement name, attributed. Where no
  verbatim names the mechanic, say the mechanic is *measured, not quoted*, and give the funnel numbers.
- **Sentiment carries its provenance in the card**: † for an all-language store rating, unmarked for a
  recent-English corpus. Never rank across the two (`C44`; the `14` → `15` correction is the live example).
