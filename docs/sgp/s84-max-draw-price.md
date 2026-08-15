# S84 — the model's maximum reachable draw price

**For:** the Design Director's open ledger, S84 · **From:** sgp lane · **2026-08-15**
**Scope:** the board offer, pre-relic. Odds-rewriting relics (Profit Boost, promo) rewrite a placed
leg's odds after pricing; they are not something the board emits.

## The number

> **4.2058** (decimal) — the longest draw price the engine can put on the board.

Its opposite end, for the shape of the thing: **3.3527**. That is the whole range.

## Method

The draw is **not dialled**. `Matchup.DrawProb` reads off the latents as the draw class's share of
the untruncated goal grid, and `Matchup.TrueProb(MatchResult.Draw)` returns it verbatim. A moneyline
offer prices at `1 / (p × (1 + Overround))` (`MatchModel.Offer`), and the moneyline branch of
`TrueProbability` returns `TrueProb(ResultOf(choice))` — so a draw offer is exactly
`1 / (DrawProb × 1.05)`, and **the longest price is set by the smallest reachable `DrawProb`**.

`DrawProb` moves with the two goal latents only, so just `p` and `goalTempo` touch it — corner and
discipline tempos are irrelevant to the draw. The sampled box is `p ∈ [0.25, 0.75)` and
`goalTempo ∈ [0.85, 1.15)`. Swept at 2001 × 2001 over the closed box, computed by the engine's own
code (compiled by source include, so the arithmetic is the engine's rather than a transcription of
it), and the extremum was located by search rather than assumed.

| | `DrawProb` | price | at |
|---|---|---|---|
| minimum draw probability → **longest price** | 0.2264470563 | **4.2058** | `p` extreme, `goalTempo` max |
| maximum draw probability → shortest price | 0.2840624619 | 3.3527 | `p = 0.5`, `goalTempo` min |

## Why it sits there, and why the range is so tight

`homeAttack + awayAttack = 2.0` for **every** `p` — the two are `0.6 + 0.8p` and `0.6 + 0.8(1−p)`.
So total goal rate is a function of `goalTempo` alone, and `p` only shifts the *split*. A draw is
therefore least likely when the split is most lopsided and the tempo highest, which is exactly where
the sweep found it.

That also explains the tightness: **the draw price spans only 3.35 to 4.21 across the entire latent
box.** `DrawProb` never leaves `[0.226, 0.284]`. Whatever else the board does, the draw is very
nearly a fixed price — it cannot be made short, and it cannot be made long.

`p` and `1 − p` give the same attack *pair*, so they give the same draw probability; verified at
`|Δ| = 2.8e-17`.

## One caveat on "reachable"

`SlateGenerator` draws both parameters with `NextDouble()`, which is `[0, 1)` — so the **low** end of
each range is attainable and the **high** end is approached but never reached. 4.2058 is therefore a
supremum. One ulp inside the open corner the price is **4.205755**, which is the same number at any
precision a surface would ever show, so the distinction is real but not operative.
