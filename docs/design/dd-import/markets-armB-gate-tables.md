# Markets → Allen · Arm B, before/after gate tables and the lead's read

**From:** markets/sim lead (`markets-2`) · **2026-08-06**
**This is not a merge request.** Arm B is implemented and committed on `markets-2` only. Nothing
lands until Allen rules.

---

## What arm B turned out to be

The finding is not the one the item was written against. Arm B was scoped as "broaden the skilled
bot's market selection" on the assumption the bot was excluded from BTTS/corners/cards. **It was
not.** `SkilledStrategy.IncludesMarketOffers` is already `true` and the bot considers every
non-scorer market on every matchup.

The zero coverage came from a **tie broken by list order**. Under exact two-way de-vig every
selection ties at −vig, so the "best candidate" loop kept whichever offer it saw *first*, and
`MatchModel.BuildOffers` emits moneyline → goals → BTTS → corners → cards. When the Longshot Photo
lifts long odds, several offers tie again at the same ×1.6 — and goals won that tie for the same
reason. Corners and cards could never be selected, at any seed, for any build.

**The change:** ties between non-moneyline candidates are now resolved by reservoir sampling on the
bot's own `Pcg32`, instead of by array position. The moneyline persona is deliberately untouched —
a tie between the moneyline and anything else still goes to the moneyline, because that is the
sharp's home. This only decides *which* longshot he takes once an item has already pulled him off
the moneyline. Determinism is unchanged: same seed, same result.

## The gate tables

Identical parameters both sides: `--gates --runs 1000 --seed-prefix TUNE`.

| Gate | Before | After |
|---|---|---|
| G1 honest gambling | **PASS** — median 4, won 0.0% | **PASS** — median 4, won 0.0% |
| G2 engine mandatory | **PASS** — median 5, won 0.0% | **PASS** — median 5, won 0.0% |
| G3 skilled + items wins | **PASS** — median 6, won 6.2% | **PASS** — median 6, won 5.4% |
| G4 the EV arc exists | **PASS** — crosses at R3 | **PASS** — crosses at R3 |
| G5 composition superadditive | **PASS** — synergy excess +0.2pp | **PASS** — synergy excess +0.2pp |
| G6 martyr guard | **PASS** — martyr-worst 6.9% vs skilled 6.2% | **PASS** — martyr-worst 6.9% vs skilled 5.4% |
| G7 market coverage | **FAIL** — uncovered: BTTS, Total Corners, Total Cards | **FAIL** — uncovered: BTTS |

**No gate flipped.** G7 is still red, but its uncovered list went from three markets to one.

### Skilled bot's market exposure — the change arm B was for

| Market | Before (legs / stake share) | After (legs / stake share) |
|---|---|---|
| Moneyline | 9,685 / 72.7% | 9,556 / 74.1% |
| Total Goals | 2,325 / **27.3%** | 1,010 / **13.5%** |
| BTTS | 0 / 0.0% | 0 / 0.0% |
| Total Corners | **0 / 0.0%** | 699 / **7.2%** |
| Total Cards | **0 / 0.0%** | 821 / **5.2%** |
| Anytime Scorer | 0 / 0.0% | 0 / 0.0% (policy-excluded) |

The longshot allocation stopped being alphabetical and spread across the ladders, which is exactly
what removing an ordering bias should look like.

## The read

**Arm B is EV-neutral, and that is a prediction the numbers confirmed rather than a hope.** Under
exact de-vig every selection ties by construction, so changing *which* tied longshot the bot takes
cannot change expected value — only which markets get exercised.

**G3's apparent 0.8pp drop is noise, and I checked rather than assumed.** At `--runs 1000` a win
rate near 6% carries a standard error of about **0.75pp**, so 6.2% → 5.4% is ~1.07 SE — inside the
range where the instrument cannot tell a real change from a coin toss. Re-measured at
`--runs 10000`, where SE falls to ~0.23pp:

| | skilled won % |
|---|---|
| before, n=10,000 | **5.5%** |
| after, n=10,000 | **5.4%** |

A 0.1pp difference at **0.44 SE**. The two arms are statistically indistinguishable. The 1000-run
reading was noise on both sides — note the "before" figure itself moved from 6.2% to 5.5% purely by
increasing n.

**A finding about the instrument, worth more than the result.** The gate campaign's default
`--runs 1000` cannot resolve differences smaller than about **1.5pp** (2 SE) in a win rate near 6%.
G3's acceptance band is 5–8% — three percentage points wide — and it is being judged by an
instrument with ±0.75pp of noise on a single reading. That is not wrong, but it means any future
"the gates moved" claim at n=1000 needs an n=10,000 confirmation before it is believed, in either
direction. It also means a real regression of one point could pass unnoticed.

**BTTS remains uncovered, and cannot be reached by this mechanism.** BTTS is a near-even two-way
market; its odds never clear the Longshot Photo's ≥3.0 threshold, and under exact de-vig it never
strictly wins a tie. So no tie-break can select it. Covering BTTS needs a *different* lever — an
item that rewards short odds, or the v2 pricing-noise model that would give the bot a genuine edge
to find. **I have not built either.** Manufacturing a reason for the bot to take BTTS purely to turn
G7 green would be tuning the instrument to pass, which is the thing this seat has spent a fortnight
refusing to do.

## What I recommend, and what I am not asking for

- **Take arm B.** It costs nothing measurable and it removes a bias that made two shipped markets
  invisible to every balance gate we have.
- **Leave G7 red.** One uncovered market, honestly named, is worth more than a green light bought by
  giving the bot a fake preference.
- **Treat BTTS coverage as a separate item** against the v2 pricing model, not as arm B's unfinished
  business.
- **The scorer-flatness pricing fix stays parked** as ruled — that re-baseline is now available if
  and when it is wanted.

**Not asking to merge.** Arm B is committed on `markets-2` and goes no further until Allen rules.

## Scope of these measurements (C25)

Gate figures are `--runs 1000`; the significance check is `--runs 10000`, skilled only, same seed
prefix. Single machine, one seed prefix — these numbers show the *difference between two arms under
one seed family*, not the absolute win rate of the game. Nothing here exercises the anytime-scorer
market (bots are policy-excluded from it; that market's instrument is the separate `--scorer-ev`
calibration harness). No Unity, no presentation surface, no settlement path beyond what the sim
already drives.
