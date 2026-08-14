# 16 — Fun autopsy: Nubby's Number Factory

**Autopsied:** 2026-08-12 · research lane (Claude, Opus 5) · **Status:** DRAFT → routed to Allen
**Why it earned a full autopsy:** it is the business case that matches `00-vision`'s actual constraint —
*"solo developer + AI collaboration… effectively $0 cash budget"* — shipped, and successful.

## 0. Identity and instrument

| | |
|---|---|
| Full title | Nubby's Number Factory |
| Dev / Pub | **MogDogBlog Productions — solo, self-published** |
| Released / Price | 2025-03-07 · **$4.99** — well below the $8–13 band |
| Reviews | **19,198 all-language, Overwhelmingly Positive** (18,694 pos / 504 neg = **97.4%**) |
| Store | https://store.steampowered.com/app/3191030/ |

**Evidence basis:** achievement funnel (13 rows, full) · review corpus (n=600 recent English) · store copy.
Pulled 2026-08-12. Not played. **Confidence ceiling: MEDIUM.**

**Prior canon:** none — and that is a gap. `07-business-and-roadmap.md`'s genre comps are Balatro (5M
units) and CloverPit (1M, small team + publisher), with `00-vision` calling Balatro's numbers *"a lottery
ticket, not the plan."* Nubby is the comp that is neither a lottery ticket nor a funded team.

---

## 1. Result cadence

**Observed (structure, store copy).** A plinko-style roguelike: launch Nubby down a pegboard, hit pegs,
make numbers. **Five balls per round** (per a 117-hour reviewer's own summary) against a **production
quota**. Miss the quota and the sun explodes. 50+ items provide the synergies. Supervisors are the
character layer; there is an endless mode and a "Nubby Trials" challenge ladder.

**Observed (cadence) — this is the study's clearest case of a *watched* resolution.** The player commits a
launch and then watches a physics simulation play out, with no further input. That is structurally closer
to SBR's sweat than Balatro, CloverPit or Insider Trading: **one commit, a resolution of real duration,
zero agency inside it.**

**UNREACHED — exact timings.** Not measured.

**Inferred — and the corpus supplies an unusually sharp warning.** When a resolution is *emergent*
(physics) rather than *authored*, players attribute their losses to the presentation layer:

> *(neg, +7, 10.3h)* "**a recent update to the physics basically made the game RNG** in deciding whether or not youll get past the first 20 rounds."
> *(neg, +2, 23.2h)* "i love this game but RNG is rough, feels half baked a lot of the times. i play it all the time until **i always feel cheated** and just kind quit."
> *(pos, +20, 6.4h)* "the bounce physics can be BS a good chunk of the time. Killing any runs super early… **Update: Once you know how to break the game it becomes insanely funny** to rack up an absurd score."

**This validates a canon decision rather than challenging one.** `04-the-sweat.md`'s integrity rule —
*the headless engine samples the outcome first, the drama generator then narrates it, and the drama never
changes the sampled outcome* — structurally immunises SBR against the complaint that sank a chunk of
Nubby's negatives. In SBR the presentation cannot cheat you, because it does not decide anything. **That
is worth stating out loud in `04`, because right now it reads as an architecture note rather than as the
defence of a known failure mode.**

**Falsifier:** if emergent-vs-authored were irrelevant, "feels cheated" language would appear at similar
rates in the authored-resolution titles. It is the top negative thread here and absent from Balatro's.

---

## 2. Compulsion levers

**Observed — inventory.** Escalating quota ✅ (the core). Permanent in-run growth ✅ (50+ items,
synergies). Big-number spectacle ✅ — the entire pitch, *"make numbers, so you can make bigger numbers, so
you can make even BIGGER numbers."* Forced-choice regret ✅ (item restocks). Collection ✅ (skins, at the
0.2% floor). Endless mode ✅. Difficulty ladder ✅ (Nubby Trials, five rungs). Debt ❌. Pity ❌/unobserved.

**Observed — the corpus** (n=600, frozen lexicon, contamination-checked):

| Family | % |
|---|---|
| `luck_vs_skill` | 7.4% |
| `addiction` | 7.2% |
| `depth_thin` | 5.8% |
| `onboard` | 1.8% |
| `gambling_real` | **0.4%** |
| `dread_tension` | 0.4% |

Only **28 negatives in 600** (4.7%). Their themes: too hard 17.9% · early-access/updates 14.3% ·
luck/steering 7.1% · price 7.1%.

**Inferred — the striking thing is what is *absent*.** `addiction` language at 7.2% is less than half
Balatro's 18.8%, CloverPit's 15.1% or Scritchy Scratchy's 19.5% — yet this title has **19,198 reviews at
97.4% positive**, the second-best sentiment in the entire study. **Nubby is beloved without being
described as compulsive.** The corpus praises art, sound and feel:

> *(pos, +21, 6.8h)* "it has vibrant colours and exciting shapes i love this experience"
> *(pos, +6, 0.9h)* "**Thumbs up for the art style and soundtrack alone**, games fun too."
> *(pos, +6, 117.4h)* "Make number go up, you get 5 balls to reach the goal… Get radical items to improve your score and don't let the sun blow up. Have fun"

**Falsifier:** if the compulsion vocabulary were simply unfashionable in this corpus, `addiction` would run
low across all 2026-era titles. It runs 19.5% in Scritchy Scratchy and 17.1% in RACCOIN. It is low here
specifically.

**Pillar-4 read.** `gambling_real` at **0.4%** — the lowest in the study, on a title Steam's own community
tagged **Gambling**. Same lesson RACCOIN taught, stronger: the mechanical family reads as gambling or not
depending entirely on dressing. A pegboard and a payline are the same object with different paint.

---

## 3. Session shape — and an instrument that fails here

**Observed — the funnel, and its limit.** 13 achievements. **The ceiling is 24.6%.** There is no
"complete round 1", no "lose your first run", no onboarding row at all — the whole set is
challenge-oriented.

| Milestone | % |
|---|---|
| "The Big One" — get a 10,000× restock | 24.6 |
| "Attention Span Issue" — skip the main tutorial | 18.3 |
| Unlock all supervisors | 3.9 |
| **"Dopamine Depletion"** — reach round 300 in endless mode | 3.2 |
| Nubby Trials 1 / 2 / 3 / 4 / 5 | 3.1 / 1.8 / 1.4 / 1.2 / 0.7 |
| "Tony Slayer" — beat the game on every supervisor | 2.5 |
| Number Factory CEO — all trials and challenges | 0.6 |
| Skin collections | 0.2 |

**The achievement funnel cannot measure this title's completion rate, and I am not going to estimate one.**
There is no "beat the game once" achievement — only "beat it on *every* supervisor" (2.5%). **UNREACHED**,
and it is an instrument limitation, not a property of the game (`C28` — coverage is reported, never
inferred). The rarity floor still works: **0.2%**, a deep completion space.

**Observed — playtime** (n=600): median **7.4h** · under 2h 12% · over 20h 18% · over 50h **4.8%**.

**Inferred.** A well-liked ~7-hour game with a modest tail and a deep, rarely-entered challenge ladder
(Trials level 1 at 3.1% falling to 0.7% at level 5). The Trials ladder halves cleanly rung to rung —
Balatro's shape, at a far lower base.

---

## 4. Meta hooks

**Observed.** Supervisors (characters) unlock; Nubby Trials is a five-rung challenge ladder; endless mode;
two skin collections at the 0.2% floor.

**Inferred.** Variety plus a steep optional ladder that almost nobody climbs. Compare Insider Trading
(`15`): a comparable matrix, an even lower entry rate. **Two independent titles now show a full completion
matrix that the audience does not enter** — the same correction to the RF-5 family that Luck be a Landlord
started.

## 5. The thing a summary would miss

**A solo developer, self-publishing, at $4.99, has 19,198 reviews at 97.4% positive** — more reviews than
CloverPit's English corpus and a better rating than any title in this study except Balatro. It did that
with thirteen achievements, one board, and a joke about the sun exploding. `00-vision` sets *"$15K net and
500 reviews"* as a strong first-game outcome and calls Balatro's numbers a lottery ticket. **Nubby cleared
the review threshold roughly thirty-eight times over on the exact constraint set SBR is working under.**
That is the most encouraging fact this lane has found, and the most useful comp canon is missing.

## 6. Transfer to SBR

**STEAL**

| What | Axis | Canon | Why |
|---|---|---|---|
| **State the outcome-first integrity rule as a defence, not a note** — "the presentation cannot cheat you" | 4 Resolution | `04-the-sweat.md` | Nubby's top negative thread is exactly the failure this rule prevents. Free — the architecture already does it |
| **Art and sound as first-class compulsion substitutes** — beloved without compulsion language | juice | `00-vision` pillar 1's juice budget; `06-vfx-and-juice.md` | 97.4% positive on a $4.99 solo title, praised for feel |
| The comedy stake — a trivial job with an absurd consequence ("miss quota, the sun explodes") | voice | `00-vision` pillar 4; `01-core-loop.md` failure fiction | Our bookie is the same joke waiting to be told bigger |
| **Nubby as the business comp** | — | `07-business-and-roadmap.md` | Solo, self-published, $4.99, 19,198 reviews. Feeds **RF-16** |

**CONFLICT** — feeds **RF-11**/**RF-15** (a $4.99 solo title outperforming the band on every measure this
lane can see except revenue, which is unmeasured) and the RF-5 correction thread.

**REJECT**

| What | Why |
|---|---|
| Emergent physics as the randomness source | Players attribute emergent losses to the presentation and say they "feel cheated". Our outcome-first design deliberately avoids this |

## 7. Comparison row

`Nubby's Number Factory | 2025-03-07 | $4.99 | 19,198 all-lang Overwhelmingly Positive (97.4%) | UNREACHED (seconds); watched physics resolution, real duration | 1 launch decision per ball, 5 per round | none during resolve | rounds against a quota + endless | 7.4h median lifetime | quota rises per round, curve UNREACHED | miss quota = run over | items × synergies, multiplicative | not observed | supervisors + 5-rung Trials ladder + skins | UNREACHED — no completion achievement exists | funnel + corpus + store copy | MEDIUM`

## 8. Sources

- Steam `appdetails` — https://store.steampowered.com/app/3191030/ — 2026-08-12 — settles identity, solo self-publishing, price, all-language rating, and the structure claims in §1 (developer copy, treated as marketing).
- `steamcommunity.com/stats/3191030/achievements/` — 2026-08-12 — settles the challenge ladder and the 0.2% rarity floor. **Does not settle the completion rate — no such achievement exists.** Recorded UNREACHED rather than estimated.
- Steam `appreviews`, n=600 recent English — 2026-08-12 — settles complaint themes and playtime. Recent-filter skews to the current build; the physics-update thread in §1 is a live example of why that matters, and it also means the negative themes here are partly a snapshot of one patch.
- Contamination check run: no frozen-lexicon term appears in this title's name or core verb.
- **Known-dead families not reported:** `near_miss`, `cash_out` (`C37`).
- The "five balls per round" figure comes from a **player review**, not the developer's copy — lower-grade evidence, flagged.
