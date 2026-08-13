# SGP correlation reconnaissance (D3)

Numbers-only recon for Lane 2 (same-game parlays). No product code was written; nothing outside
this file was touched. Every number below is measured against the shipped engine, not modelled.

---

## 1. Verification gate — PASS

An exact joint evaluator was built and checked against `MatchModel.TrueProbability` for **every
single selection on the shipped board**, across the whole population defined in §3.

| | |
|---|---|
| single-selection checks | **437,832** (12,162 matchups × 36 selections) |
| **max absolute deviation** | **2.554e-15** |
| gate threshold | 1e-12 absolute |
| verdict | **PASS** (3 orders of magnitude inside the gate) |

Worst case by market kind:

| market / choice | max abs deviation |
|---|---:|
| TotalCorners / Under | 2.554e-15 |
| TotalCorners / Over | 2.331e-15 |
| TotalCards / Over | 1.665e-15 |
| TotalCards / Under | 1.554e-15 |
| **AnytimeScorer / Yes** | **1.422e-15** |
| TotalGoals / Under | 1.277e-15 |
| Moneyline / Home | 8.882e-16 |
| BothTeamsToScore / No | 8.882e-16 |
| Moneyline / Away | 7.772e-16 |
| TotalGoals / Over | 0 (bit-identical) |
| BothTeamsToScore / Yes | 0 (bit-identical) |

Deviation is pure floating-point summation-order noise: the engine computes `Under` as `1 − over`
and the scorer marginal as `1 − E[miss]`, while the joint evaluator accumulates the surviving
outcomes directly. `Over` and `BTTS Yes` come out bit-identical because both paths sum the same
terms in the same order.

**Every number in this document comes from a gate-passing evaluator.**

### How it was built

A throwaway console project in the scratchpad compiles `engine/*.cs` **by source glob** into its own
assembly. That makes `Matchup.Dist` / `MatchDistributions` / `RawPoisson` visible without any
`InternalsVisibleTo`, makes the arithmetic bit-identical to the engine's, and never references
`engine/SBR.Engine.csproj` — so the `CopyEngineToUnityPlugins` target never fires and no tracked
Unity LFS asset was dirtied. No `dotnet build` was run against a repo project. No git commands were
run.

---

## 2. Structural findings from source (read before trusting any table)

### 2.1 The independence claim is CONFIRMED

`MatchModel.SampleStatLine` (`:155–166`) draws, in order: a winner from `TrueHomeProb`; a scoreline
from that winner's `EnumerateScores` list; then home corners, away corners, home cards, away cards —
each from its own truncated-Poisson array built once in `MatchDistributions.Build`. **No count array
is conditioned on the score draw, and no score list is conditioned on a count.** A repo-wide grep
confirms corner/card rates are touched in exactly two places: `MatchModel.LatentsFor` (parameter
construction) and `MatchModel`'s own arrays. There is no coupling anywhere in `engine/`.

So, conditional on a matchup, the joint factors exactly:

```
p_joint = p_goalFamily × p_cornerFamily × p_cardFamily
```

Measured, not assumed: across **3,940,488** cross-family pairs (both populations),
**max |ρ − 1| = 4.408e-14**. That is floating-point zero. Cross-family correlation is not "small";
it is *exactly absent by construction*.

### 2.2 The board is 36 selections per matchup, not 50

`MatchModel.BuildOffers` with the shipped `RunConfig` yields:
2 moneyline + 6 total-goals + 2 BTTS + 6 total-corners + 6 total-cards + **14 anytime-scorer**
(`PlayersPerTeam = 7`, both rosters) = **36**. So one matchup carries C(36,2)=630 pairs,
C(36,3)=7,140 triples, C(36,4)=58,905 four-leg shapes.

### 2.3 Grid caps that matter and were not in the brief

`MaxCornerGrid = 20` and `MaxCardGrid = 12` (per team). Total corners therefore range 0–40 and total
cards 0–24. Both are far outside the offered lines, so truncation does not bind on any shipped
market — but the joint evaluator enumerates the full grid regardless.

### 2.4 Over/Under are exact complements

`MatchModel.Compare` (`:486`) uses strict `>` for Over and strict `<` for Under. Every shipped line
is a half-integer, so there is no push and `P(Under) = 1 − P(Over)` exactly. Confirmed in
`TrueProbability`, which literally returns `1.0 - over`.

### 2.5 One correction to the brief's worked example

The brief suggested "a moneyline paired with a goal total that the winner condition forbids" as an
example of an impossible combination. **That combination does not exist.** A moneyline is compatible
with every offered goal total (a home win can be 1–0, so even Under 1.5 survives). The real
cross-market impossibilities are BTTS-driven — see §6.

### 2.6 A numerical trap in the scorer inclusion–exclusion (worth recording)

The brief's inclusion–exclusion for k backed players on one team,
`P = Σ_{S} (−1)^{|S|} (1 − Σ_{i∈S} w_i)^g`, is correct. But when `g < k` it cancels to
**≈1e-17, not exactly 0**, in IEEE double. Left unguarded, that makes a structurally impossible
ticket (two players scoring inside one goal) register as a vanishingly small *positive* probability
and silently drops it out of any "impossible" enumeration. The evaluator carries an exact
`if (g < k) return 0` guard. Before the guard, 12 of the 57 impossible triple shapes in §6.2 were
misclassified as merely "usually zero". **Any production correlation code must carry the same
guard or an equivalent tolerance.**

---

## 3. Populations

Both deterministic; no sampling noise anywhere in this document.

**POP-A — real generated slates (primary).**
`SlateGenerator.Generate(round, hub, config)` with `new RngHub("recon-{s}")`, seeds s = 0…249,
rounds 0…7 generated in order from a fresh hub per seed, 6 matchups per slate, shipped `RunConfig`
defaults. **12,000 matchups**, each with real jittered rosters (`ScoringWeightJitter = 0.35`).
Zero slates were rejected by `MatchModel.Offer`'s `odds <= 1.0` guard.

**POP-B — latent extremes grid.**
`MatchModel.LatentsFor(p, goalTempo, cornerTempo, disciplineTempo, config)` over
p ∈ {0.25, 0.35, 0.45, 0.55, 0.65, 0.75} × goalTempo ∈ {0.85, 1.00, 1.15} × cornerTempo ∈
{0.80, 1.00, 1.20} × disciplineTempo ∈ {0.80, 1.00, 1.20} = **162 matchups**. Rosters here are
synthetic and **un-jittered** (flat role weights 3.0/1.5/0.5, 3 FW + 2 MF + 2 DF), so POP-B scorer
numbers show the role structure without per-player spread. Flagged rather than hidden.

Sweep coverage:

| sweep | population | combinations |
|---|---|---:|
| verification gate | POP-A + POP-B (12,162) | 437,832 |
| cross-family independence | POP-A + POP-B (12,162) | 3,940,488 |
| exhaustive pairs | POP-A + POP-B (12,162) | **7,662,060** |
| exhaustive triples | 400 POP-A + POP-B (562) | **4,012,680** |
| exhaustive 4-leg | 30 POP-A + 40 POP-B (70) | **4,123,350** |
| curated SGP shapes | POP-A (12,000) | 22 shapes × 12,000 |

Labels below are population-relative, not home/away: `ML FAV` is whichever side has
`TrueHomeProb ≥ 0.5`, `SCR DOG-FW` is a forward on the underdog, and so on. This is the only way the
statistics mean anything, since home is the favourite in exactly half the draws.

---

## 4. Break-even algebra — derived and numerically confirmed

With `n` legs, marginals `p_i`, and offered odds `o_i = 1/(p_i(1+Ω))`:

```
Π o_i          = 1 / (Π p_i · (1+Ω)^n)
p_joint · Π o_i = (p_joint / Π p_i) / (1+Ω)^n  =  ρ / (1+Ω)^n
EV per unit     = ρ / (1+Ω)^n − 1              →  player is +EV  ⟺  ρ > (1+Ω)^n

o_sgp           = 1 / (p_joint (1+Ω))
o_sgp / Π o_i   = (1+Ω)^(n−1) / ρ              →  correct price SHORTENS ⟺ ρ > (1+Ω)^(n−1)
```

Confirmed against direct computation over 200,000 random 2–4 leg draws:
EV identity max |direct − formula| = **1.776e-15**; shortening identity max relative deviation =
**9.437e-16**.

**The two thresholds are different, and that difference matters.** At Ω = 0.05:

| n | +EV threshold `(1+Ω)^n` | odds-shorten threshold `(1+Ω)^(n−1)` |
|---:|---:|---:|
| 2 | 1.102500 | 1.050000 |
| 3 | 1.157625 | 1.102500 |
| 4 | 1.215506 | 1.157625 |

A combination can price shorter than the naive product and still be −EV for the player.

---

## 5. The ρ distribution

### 5.1 By family pair (7,662,060 pairs)

| family pair | n | min | p05 | median | p95 | max | % ρ=0 | % clearing 1.1025 |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| GOAL × GOAL | 3,356,712 | 0.0000 | 0.3380 | 0.9472 | 1.6877 | **3.1081** | 3.62 | 32.36 |
| CORNER × CORNER | 182,430 | 0.0000 | 0.0000 | 0.4160 | 2.5835 | **4.1255** | 40.00 | 40.00 |
| CARD × CARD | 182,430 | 0.0000 | 0.0000 | 0.5461 | 2.1802 | **3.5928** | 40.00 | 40.00 |
| GOAL × CORNER | 1,751,328 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 0.00 | 0.00 |
| GOAL × CARD | 1,751,328 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 0.00 | 0.00 |
| CORNER × CARD | 437,832 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 0.00 | 0.00 |

**51.4% of all two-leg combinations on the shipped board carry exactly zero correlation.** All of
the structure lives inside the three families, and the only family with more than one market *kind*
is GOAL (moneyline / total goals / BTTS / anytime scorer).

Overall clearing rate against the +EV threshold: **1,232,314 / 7,662,060 = 16.083% of pairs are
+EV for the player under naive product pricing.**

### 5.2 Every non-unit pair shape class

228 of the 384 shape classes have ρ ≡ 1 (cross-family) and are omitted. The remaining 156 are the
whole correlation story of this board. `shorten` = median `o_sgp / Π o_i`; below 1 the correct price
is shorter than the naive product, above 1 it is longer.

| pair shape | n | min ρ | median ρ | max ρ | % ρ=0 | % +EV | med shorten |
|---|---:|---:|---:|---:|---:|---:|---:|
| GLS U1.5 + GLS U2.5 | 12162 | 1.9678 | 2.4441 | 3.1081 | 0.0 | 100.0 | 0.430 |
| CRD O4.5 + CRD O5.5 | 12162 | 1.6469 | 2.2281 | 3.5928 | 0.0 | 100.0 | 0.471 |
| CNR U8.5 + CNR U9.5 | 12162 | 1.3954 | 2.1836 | 4.1255 | 0.0 | 100.0 | 0.481 |
| BTTS N + GLS U1.5 | 12162 | 1.6061 | 1.9042 | 2.2812 | 0.0 | 100.0 | 0.551 |
| BTTS N + GLS U2.5 | 12162 | 1.6061 | 1.9042 | 2.2812 | 0.0 | 100.0 | 0.551 |
| CNR O10.5 + CNR O9.5 | 12162 | 1.3199 | 1.8449 | 3.5289 | 0.0 | 100.0 | 0.569 |
| BTTS Y + GLS O3.5 | 12162 | 1.5925 | 1.8351 | 2.2159 | 0.0 | 100.0 | 0.572 |
| ML DOG + SCR DOG-DF | 24324 | 1.5297 | 1.8347 | 2.3523 | 0.0 | 100.0 | 0.572 |
| CRD U3.5 + CRD U4.5 | 12162 | 1.3857 | 1.8143 | 2.5457 | 0.0 | 100.0 | 0.579 |
| ML DOG + SCR DOG-MF | 24324 | 1.5048 | 1.8068 | 2.3224 | 0.0 | 100.0 | 0.581 |
| ML DOG + SCR DOG-FW | 36486 | 1.4619 | 1.7663 | 2.2778 | 0.0 | 100.0 | 0.594 |
| CNR U10.5 + CNR U8.5 | 12162 | 1.2257 | 1.7150 | 2.8799 | 0.0 | 100.0 | 0.612 |
| CNR U10.5 + CNR U9.5 | 12162 | 1.2257 | 1.7150 | 2.8799 | 0.0 | 100.0 | 0.612 |
| BTTS Y + GLS O2.5 | 12162 | 1.4744 | 1.6925 | 2.0333 | 0.0 | 100.0 | 0.620 |
| GLS O2.5 + GLS O3.5 | 12162 | 1.4744 | 1.6925 | 2.0333 | 0.0 | 100.0 | 0.620 |
| GLS O3.5 + SCR DOG-DF | 24324 | 1.5197 | 1.6845 | 1.9202 | 0.0 | 100.0 | 0.623 |
| GLS O3.5 + SCR DOG-MF | 24324 | 1.4794 | 1.6485 | 1.8938 | 0.0 | 100.0 | 0.637 |
| GLS O3.5 + SCR FAV-DF | 24324 | 1.4846 | 1.6441 | 1.8472 | 0.0 | 100.0 | 0.639 |
| GLS O3.5 + SCR DOG-FW | 36486 | 1.4121 | 1.5965 | 1.8530 | 0.0 | 100.0 | 0.658 |
| GLS O3.5 + SCR FAV-MF | 24324 | 1.4198 | 1.5948 | 1.8097 | 0.0 | 100.0 | 0.658 |
| CRD O3.5 + CRD O4.5 | 12162 | 1.2952 | 1.5612 | 2.1382 | 0.0 | 100.0 | 0.673 |
| CRD O3.5 + CRD O5.5 | 12162 | 1.2952 | 1.5612 | 2.1382 | 0.0 | 100.0 | 0.673 |
| GLS O3.5 + SCR FAV-FW | 36486 | 1.3473 | 1.5232 | 1.7461 | 0.0 | 100.0 | 0.689 |
| BTTS Y + SCR DOG-DF | 24324 | 1.3125 | 1.5204 | 1.8945 | 0.0 | 100.0 | 0.691 |
| BTTS Y + SCR DOG-MF | 24324 | 1.3097 | 1.5180 | 1.8939 | 0.0 | 100.0 | 0.692 |
| BTTS Y + SCR DOG-FW | 36486 | 1.3042 | 1.5146 | 1.8931 | 0.0 | 100.0 | 0.693 |
| CNR O10.5 + CNR O8.5 | 12162 | 1.1835 | 1.4989 | 2.4543 | 0.0 | 100.0 | 0.701 |
| CNR O8.5 + CNR O9.5 | 12162 | 1.1835 | 1.4989 | 2.4543 | 0.0 | 100.0 | 0.701 |
| GLS U1.5 + GLS U3.5 | 12162 | 1.2631 | 1.4194 | 1.6434 | 0.0 | 100.0 | 0.740 |
| GLS U2.5 + GLS U3.5 | 12162 | 1.2631 | 1.4194 | 1.6434 | 0.0 | 100.0 | 0.740 |
| CRD U4.5 + CRD U5.5 | 12162 | 1.1696 | 1.3891 | 1.7638 | 0.0 | 100.0 | 0.756 |
| CRD U3.5 + CRD U5.5 | 12162 | 1.1696 | 1.3891 | 1.7638 | 0.0 | 100.0 | 0.756 |
| GLS O2.5 + SCR DOG-DF | 24324 | 1.2655 | 1.3826 | 1.5741 | 0.0 | 100.0 | 0.759 |
| ML FAV + SCR FAV-DF | 24324 | 1.2144 | 1.3743 | 1.6143 | 0.0 | 100.0 | 0.764 |
| GLS O2.5 + SCR DOG-MF | 24324 | 1.2549 | 1.3732 | 1.5665 | 0.0 | 100.0 | 0.765 |
| ML FAV + SCR FAV-MF | 24324 | 1.2038 | 1.3640 | 1.6042 | 0.0 | 100.0 | 0.770 |
| GLS O2.5 + SCR DOG-FW | 36486 | 1.2370 | 1.3597 | 1.5556 | 0.0 | 100.0 | 0.772 |
| **ML FAV + SCR FAV-FW** | 36486 | 1.1914 | **1.3487** | 1.5912 | 0.0 | 100.0 | 0.779 |
| GLS O2.5 + SCR FAV-DF | 24324 | 1.2349 | 1.3304 | 1.4661 | 0.0 | 100.0 | 0.789 |
| BTTS Y + GLS O1.5 | 12162 | 1.2127 | 1.3195 | 1.4767 | 0.0 | 100.0 | 0.796 |
| GLS O1.5 + GLS O2.5 | 12162 | 1.2127 | 1.3195 | 1.4767 | 0.0 | 100.0 | 0.796 |
| GLS O1.5 + GLS O3.5 | 12162 | 1.2127 | 1.3195 | 1.4767 | 0.0 | 100.0 | 0.796 |
| BTTS N + GLS U3.5 | 12162 | 1.2027 | 1.3173 | 1.4687 | 0.0 | 100.0 | 0.797 |
| GLS O2.5 + SCR FAV-MF | 24324 | 1.2149 | 1.3155 | 1.4541 | 0.0 | 100.0 | 0.798 |
| BTTS Y + SCR FAV-DF | 24324 | 1.1956 | 1.3148 | 1.5068 | 0.0 | 100.0 | 0.799 |
| BTTS Y + SCR FAV-MF | 24324 | 1.1864 | 1.3084 | 1.5006 | 0.0 | 100.0 | 0.802 |
| BTTS Y + SCR FAV-FW | 36486 | 1.1761 | 1.2989 | 1.4922 | 0.0 | 100.0 | 0.808 |
| GLS O2.5 + SCR FAV-FW | 36486 | 1.1909 | 1.2931 | 1.4339 | 0.0 | 100.0 | 0.812 |
| GLS O1.5 + SCR DOG-DF | 24324 | 1.1513 | 1.2090 | 1.2855 | 0.0 | 100.0 | 0.868 |
| GLS O1.5 + SCR FAV-DF | 24324 | 1.1429 | 1.2044 | 1.2841 | 0.0 | 100.0 | 0.872 |
| GLS O1.5 + SCR DOG-MF | 24324 | 1.1466 | 1.2035 | 1.2801 | 0.0 | 100.0 | 0.872 |
| GLS O1.5 + SCR FAV-MF | 24324 | 1.1340 | 1.1967 | 1.2768 | 0.0 | 100.0 | 0.877 |
| GLS O1.5 + SCR DOG-FW | 36486 | 1.1367 | 1.1947 | 1.2726 | 0.0 | 100.0 | 0.879 |
| GLS O1.5 + SCR FAV-FW | 36486 | 1.1226 | 1.1847 | 1.2660 | 0.0 | 100.0 | 0.886 |
| SCR DOG-MF + SCR DOG-MF | 12162 | 1.0423 | 1.0917 | 1.1545 | 0.0 | 33.8 | 0.962 |
| SCR DOG-DF + SCR DOG-FW | 72972 | 1.0415 | 1.0917 | 1.1537 | 0.0 | 33.0 | 0.962 |
| SCR DOG-DF + SCR DOG-MF | 48648 | 1.0384 | 1.0916 | 1.1557 | 0.0 | 34.8 | 0.962 |
| SCR DOG-FW + SCR DOG-MF | 72972 | 1.0432 | 1.0916 | 1.1523 | 0.0 | 31.9 | 0.962 |
| SCR DOG-DF + SCR DOG-DF | 12162 | 1.0367 | 1.0914 | 1.1570 | 0.0 | 35.9 | 0.962 |
| SCR DOG-FW + SCR DOG-FW | 36486 | 1.0488 | 1.0909 | 1.1501 | 0.0 | 30.3 | 0.962 |
| GLS U1.5 + ML DOG | 12162 | 1.0000 | 1.0879 | 1.2485 | 0.0 | 43.0 | 0.965 |
| GLS O3.5 + ML FAV | 12162 | 1.0000 | 1.0420 | 1.0630 | 0.0 | 0.0 | 1.008 |
| GLS U2.5 + ML DOG | 12162 | 1.0000 | 1.0349 | 1.1031 | 0.0 | 0.2 | 1.015 |
| GLS U3.5 + ML DOG | 12162 | 1.0000 | 1.0293 | 1.0921 | 0.0 | 0.0 | 1.020 |
| BTTS Y + ML DOG | 12162 | 1.0000 | 1.0200 | 1.0498 | 0.0 | 0.0 | 1.029 |
| GLS O1.5 + ML FAV | 12162 | 1.0000 | 1.0163 | 1.0281 | 0.0 | 0.0 | 1.033 |
| GLS O2.5 + ML FAV | 12162 | 1.0000 | 1.0143 | 1.0232 | 0.0 | 0.0 | 1.035 |
| BTTS N + ML FAV | 12162 | 1.0000 | 1.0099 | 1.0194 | 0.0 | 0.0 | 1.040 |
| SCR FAV-FW + SCR FAV-FW | 36486 | 0.9364 | 1.0082 | 1.0557 | 0.0 | 0.0 | 1.041 |
| SCR FAV-FW + SCR FAV-MF | 72972 | 0.9284 | 1.0030 | 1.0528 | 0.0 | 0.0 | 1.047 |
| SCR FAV-DF + SCR FAV-FW | 72972 | 0.9235 | 0.9996 | 1.0515 | 0.0 | 0.0 | 1.050 |
| SCR FAV-MF + SCR FAV-MF | 12162 | 0.9243 | 0.9976 | 1.0488 | 0.0 | 0.0 | 1.053 |
| SCR FAV-DF + SCR FAV-MF | 48648 | 0.9187 | 0.9939 | 1.0476 | 0.0 | 0.0 | 1.056 |
| SCR FAV-DF + SCR FAV-DF | 12162 | 0.9147 | 0.9900 | 1.0443 | 0.0 | 0.0 | 1.061 |
| BTTS Y + ML FAV | 12162 | 0.9834 | 0.9881 | 1.0000 | 0.0 | 0.0 | 1.063 |
| GLS U3.5 + ML FAV | 12162 | 0.9693 | 0.9831 | 1.0000 | 0.0 | 0.0 | 1.068 |
| BTTS N + ML DOG | 12162 | 0.9418 | 0.9823 | 1.0000 | 0.0 | 0.0 | 1.069 |
| GLS U2.5 + ML FAV | 12162 | 0.9656 | 0.9795 | 1.0000 | 0.0 | 0.0 | 1.072 |
| GLS O2.5 + ML DOG | 12162 | 0.9304 | 0.9756 | 1.0000 | 0.0 | 0.0 | 1.076 |
| GLS O1.5 + ML DOG | 12162 | 0.9156 | 0.9720 | 1.0000 | 0.0 | 0.0 | 1.080 |
| GLS U1.5 + ML FAV | 12162 | 0.9172 | 0.9483 | 1.0000 | 0.0 | 0.0 | 1.107 |
| GLS O3.5 + ML DOG | 12162 | 0.8110 | 0.9286 | 1.0000 | 0.0 | 0.0 | 1.131 |
| SCR DOG-FW + SCR FAV-DF | 72972 | 0.8265 | 0.8947 | 0.9595 | 0.0 | 0.0 | 1.174 |
| SCR DOG-FW + SCR FAV-MF | 72972 | 0.8269 | 0.8934 | 0.9561 | 0.0 | 0.0 | 1.175 |
| **SCR DOG-FW + SCR FAV-FW** | 109458 | 0.8278 | **0.8916** | 0.9513 | 0.0 | 0.0 | 1.178 |
| SCR DOG-MF + SCR FAV-DF | 48648 | 0.8248 | 0.8890 | 0.9502 | 0.0 | 0.0 | 1.181 |
| SCR DOG-MF + SCR FAV-MF | 48648 | 0.8256 | 0.8882 | 0.9479 | 0.0 | 0.0 | 1.182 |
| SCR DOG-MF + SCR FAV-FW | 72972 | 0.8263 | 0.8872 | 0.9440 | 0.0 | 0.0 | 1.183 |
| SCR DOG-DF + SCR FAV-DF | 48648 | 0.8238 | 0.8855 | 0.9457 | 0.0 | 0.0 | 1.186 |
| SCR DOG-DF + SCR FAV-MF | 48648 | 0.8244 | 0.8850 | 0.9435 | 0.0 | 0.0 | 1.186 |
| SCR DOG-DF + SCR FAV-FW | 72972 | 0.8256 | 0.8845 | 0.9396 | 0.0 | 0.0 | 1.187 |
| GLS O1.5 + GLS U3.5 | 12162 | 0.8619 | 0.8661 | 0.8751 | 0.0 | 0.0 | 1.212 |
| CRD O3.5 + CRD U5.5 | 12162 | 0.7746 | 0.7817 | 0.8069 | 0.0 | 0.0 | 1.343 |
| GLS U3.5 + SCR FAV-FW | 36486 | 0.7090 | 0.7804 | 0.8514 | 0.0 | 0.0 | 1.345 |
| GLS U3.5 + SCR FAV-MF | 24324 | 0.6873 | 0.7506 | 0.8167 | 0.0 | 0.0 | 1.399 |
| GLS U3.5 + SCR DOG-FW | 36486 | 0.6670 | 0.7493 | 0.8274 | 0.0 | 0.0 | 1.401 |
| BTTS N + SCR FAV-FW | 36486 | 0.6101 | 0.7312 | 0.8239 | 0.0 | 0.0 | 1.436 |
| GLS U3.5 + SCR FAV-DF | 24324 | 0.6725 | 0.7296 | 0.7899 | 0.0 | 0.0 | 1.439 |
| GLS U3.5 + SCR DOG-MF | 24324 | 0.6516 | 0.7277 | 0.8008 | 0.0 | 0.0 | 1.443 |
| BTTS N + SCR FAV-MF | 24324 | 0.6061 | 0.7228 | 0.8096 | 0.0 | 0.0 | 1.453 |
| BTTS N + SCR FAV-DF | 24324 | 0.6033 | 0.7172 | 0.8013 | 0.0 | 0.0 | 1.464 |
| GLS U3.5 + SCR DOG-DF | 24324 | 0.6409 | 0.7125 | 0.7796 | 0.0 | 0.0 | 1.474 |
| BTTS N + GLS O1.5 | 12162 | 0.6889 | 0.7110 | 0.7513 | 0.0 | 0.0 | 1.477 |
| GLS O2.5 + GLS U3.5 | 12162 | 0.6948 | 0.7094 | 0.7282 | 0.0 | 0.0 | 1.480 |
| BTTS Y + GLS U3.5 | 12162 | 0.6185 | 0.6491 | 0.6802 | 0.0 | 0.0 | 1.618 |
| CNR O8.5 + CNR U10.5 | 12162 | 0.6429 | 0.6476 | 0.6718 | 0.0 | 0.0 | 1.621 |
| GLS U2.5 + SCR FAV-FW | 36486 | 0.4780 | 0.5771 | 0.6822 | 0.0 | 0.0 | 1.819 |
| ML FAV + SCR DOG-FW | 36486 | 0.4057 | 0.5450 | 0.6919 | 0.0 | 0.0 | 1.927 |
| GLS U2.5 + SCR FAV-MF | 24324 | 0.4597 | 0.5446 | 0.6356 | 0.0 | 0.0 | 1.928 |
| CRD O3.5 + CRD U4.5 | 12162 | 0.5415 | 0.5434 | 0.5610 | 0.0 | 0.0 | 1.932 |
| GLS O1.5 + GLS U2.5 | 12162 | 0.5333 | 0.5391 | 0.5560 | 0.0 | 0.0 | 1.948 |
| BTTS N + SCR DOG-FW | 36486 | 0.3685 | 0.5347 | 0.6758 | 0.0 | 0.0 | 1.964 |
| BTTS N + SCR DOG-MF | 24324 | 0.3689 | 0.5318 | 0.6694 | 0.0 | 0.0 | 1.974 |
| BTTS N + SCR DOG-DF | 24324 | 0.3691 | 0.5298 | 0.6635 | 0.0 | 0.0 | 1.982 |
| GLS U2.5 + SCR FAV-DF | 24324 | 0.4474 | 0.5227 | 0.6041 | 0.0 | 0.0 | 2.009 |
| CRD O4.5 + CRD U5.5 | 12162 | 0.5059 | 0.5222 | 0.5601 | 0.0 | 0.0 | 2.011 |
| ML FAV + SCR DOG-MF | 24324 | 0.3906 | 0.5203 | 0.6595 | 0.0 | 0.0 | 2.018 |
| ML FAV + SCR DOG-DF | 24324 | 0.3813 | 0.5038 | 0.6397 | 0.0 | 0.0 | 2.084 |
| GLS U2.5 + SCR DOG-FW | 36486 | 0.3629 | 0.4804 | 0.5994 | 0.0 | 0.0 | 2.185 |
| GLS U2.5 + SCR DOG-MF | 24324 | 0.3532 | 0.4615 | 0.5700 | 0.0 | 0.0 | 2.275 |
| GLS U2.5 + SCR DOG-DF | 24324 | 0.3458 | 0.4481 | 0.5461 | 0.0 | 0.0 | 2.343 |
| GLS U1.5 + SCR FAV-FW | 36486 | 0.3483 | 0.4216 | 0.5142 | 0.0 | 0.0 | 2.491 |
| CNR O8.5 + CNR U9.5 | 12162 | 0.4093 | 0.4136 | 0.4266 | 0.0 | 0.0 | 2.539 |
| ML DOG + SCR FAV-FW | 36486 | 0.2979 | 0.4124 | 0.5449 | 0.0 | 0.0 | 2.546 |
| CNR O9.5 + CNR U10.5 | 12162 | 0.3934 | 0.3972 | 0.4293 | 0.0 | 0.0 | 2.643 |
| GLS U1.5 + SCR DOG-FW | 36486 | 0.3105 | 0.3901 | 0.4824 | 0.0 | 0.0 | 2.692 |
| ML DOG + SCR FAV-MF | 24324 | 0.2856 | 0.3861 | 0.5031 | 0.0 | 0.0 | 2.720 |
| GLS U1.5 + SCR FAV-MF | 24324 | 0.3267 | 0.3838 | 0.4567 | 0.0 | 0.0 | 2.736 |
| BTTS N + GLS O2.5 | 12162 | 0.3260 | 0.3734 | 0.4452 | 0.0 | 0.0 | 2.812 |
| ML DOG + SCR FAV-DF | 24324 | 0.2761 | 0.3688 | 0.4742 | 0.0 | 0.0 | 2.847 |
| GLS U1.5 + SCR DOG-MF | 24324 | 0.2948 | 0.3625 | 0.4394 | 0.0 | 0.0 | 2.896 |
| GLS U1.5 + SCR FAV-DF | 24324 | 0.3121 | 0.3598 | 0.4195 | 0.0 | 0.0 | 2.918 |
| GLS U1.5 + SCR DOG-DF | 24324 | 0.2847 | 0.3448 | 0.4083 | 0.0 | 0.0 | 3.045 |
| **BTTS N + GLS O3.5** | 12162 | **0.2071** | **0.2409** | 0.3066 | 0.0 | 0.0 | 4.359 |
| BTTS N + BTTS Y | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| BTTS Y + GLS U1.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| BTTS Y + GLS U2.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CNR O10.5 + CNR U10.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CNR O10.5 + CNR U8.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CNR O10.5 + CNR U9.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CNR O8.5 + CNR U8.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CNR O9.5 + CNR U8.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CNR O9.5 + CNR U9.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CRD O3.5 + CRD U3.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CRD O4.5 + CRD U3.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CRD O4.5 + CRD U4.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CRD O5.5 + CRD U3.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CRD O5.5 + CRD U4.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| CRD O5.5 + CRD U5.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| GLS O1.5 + GLS U1.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| GLS O2.5 + GLS U1.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| GLS O2.5 + GLS U2.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| GLS O3.5 + GLS U1.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| GLS O3.5 + GLS U2.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| GLS O3.5 + GLS U3.5 | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |
| ML DOG + ML FAV | 12162 | 0 | 0 | 0 | 100.0 | 0.0 | ∞ |

### 5.3 Marginals on the shipped board (POP-A, 12,000 matchups)

| selection | min p | median p | max p | odds range |
|---|---:|---:|---:|---|
| ML FAV | 0.5000 | 0.6271 | 0.7500 | 1.270 – 1.905 |
| ML DOG | 0.2500 | 0.3729 | 0.5000 | 1.905 – 3.810 |
| GLS O1.5 | 0.6772 | 0.7578 | 0.8245 | 1.155 – 1.406 |
| GLS O2.5 | 0.4919 | 0.5907 | 0.6781 | 1.405 – 1.936 |
| GLS O3.5 | 0.2083 | 0.2953 | 0.3913 | 2.434 – 4.572 |
| GLS U1.5 | 0.1755 | 0.2422 | 0.3228 | 2.950 – 5.426 |
| GLS U2.5 | 0.3219 | 0.4093 | 0.5081 | 1.874 – 2.958 |
| GLS U3.5 | 0.6087 | 0.7047 | 0.7917 | 1.203 – 1.565 |
| BTTS Y | 0.3786 | 0.4747 | 0.5616 | 1.696 – 2.516 |
| BTTS N | 0.4384 | 0.5253 | 0.6214 | 1.533 – 2.173 |
| CNR O8.5 / O9.5 / O10.5 | 0.4075 / 0.2834 / 0.1841 | 0.6660 / 0.5408 / 0.4157 | 0.8450 / 0.7576 / 0.6528 | 1.127 – 5.172 |
| CNR U8.5 / U9.5 / U10.5 | 0.1550 / 0.2424 / 0.3472 | 0.3340 / 0.4593 / 0.5844 | 0.5925 / 0.7166 / 0.8159 | 1.167 – 6.143 |
| CRD O3.5 / O4.5 / O5.5 | 0.4677 / 0.2783 / 0.1450 | 0.6410 / 0.4493 / 0.2805 | 0.7721 / 0.6072 / 0.4330 | 1.234 – 6.566 |
| CRD U3.5 / U4.5 / U5.5 | 0.2279 / 0.3928 / 0.5670 | 0.3590 / 0.5507 / 0.7195 | 0.5323 / 0.7217 / 0.8550 | 1.114 – 4.179 |
| SCR FAV-FW | 0.1771 | 0.3222 | 0.5177 | 1.840 – 5.377 |
| SCR FAV-MF | 0.0868 | 0.1771 | 0.3171 | 3.003 – 10.967 |
| SCR FAV-DF | 0.0292 | 0.0629 | 0.1303 | 7.308 – 32.564 |
| SCR DOG-FW | 0.1176 | 0.2347 | 0.4085 | 2.331 – 8.099 |
| SCR DOG-MF | 0.0545 | 0.1267 | 0.2327 | 4.093 – 17.465 |
| SCR DOG-DF | 0.0178 | 0.0442 | 0.0945 | 10.077 – 53.574 |

---

## 6. Impossible combinations (p_joint = 0 exactly)

This is a correctness finding, not a tuning note. Under naive product pricing every combination
below is a ticket sold at positive decimal odds that **cannot win under any outcome the engine can
sample**. All zeros measured here are *structural* — the sweep found **zero** shape classes that are
zero for some matchups and non-zero for others, across both populations (§ "parameter-dependent
zeros: none").

### 6.1 Impossible PAIRS — 22 shapes, every matchup

**267,564 of 7,662,060 pairs = 3.49% of the two-leg combination space.**

| pair | mean Π p_i | mean naive decimal price |
|---|---:|---:|
| GLS O3.5 + GLS U1.5 | 0.0704 | **12.88** |
| CRD O5.5 + CRD U3.5 | 0.0963 | 9.48 |
| **BTTS Y + GLS U1.5** | 0.1139 | **8.00** |
| GLS O3.5 + GLS U2.5 | 0.1192 | 7.62 |
| CNR O10.5 + CNR U8.5 | 0.1269 | 7.20 |
| GLS O2.5 + GLS U1.5 | 0.1418 | 6.43 |
| CRD O5.5 + CRD U4.5 | 0.1486 | 6.23 |
| CRD O4.5 + CRD U3.5 | 0.1551 | 5.86 |
| CNR O9.5 + CNR U8.5 | 0.1674 | 5.52 |
| CNR O10.5 + CNR U9.5 | 0.1747 | 5.24 |
| GLS O1.5 + GLS U1.5 | 0.1831 | 5.02 |
| CRD O5.5 + CRD U5.5 | 0.1959 | 4.82 |
| **BTTS Y + GLS U2.5** | 0.1923 | **4.72** |
| CNR O8.5 + CNR U8.5 | 0.2099 | 4.49 |
| GLS O3.5 + GLS U3.5 | 0.2060 | 4.45 |
| CNR O10.5 + CNR U10.5 | 0.2237 | 4.13 |
| CRD O3.5 + CRD U3.5 | 0.2241 | 4.09 |
| ML DOG + ML FAV | 0.2288 | 3.99 |
| CNR O9.5 + CNR U9.5 | 0.2290 | 3.99 |
| CRD O4.5 + CRD U4.5 | 0.2379 | 3.83 |
| GLS O2.5 + GLS U2.5 | 0.2393 | 3.80 |
| BTTS N + BTTS Y | 0.2470 | 3.67 |

Twenty of these are same-market self-contradictions (an Over and an Under that cannot both hold, or
both moneylines, or both BTTS sides) — obvious once stated, but the board offers them and nothing in
the engine blocks them.

**The two non-obvious ones are BTTS-driven, and one of them is a v1-model artefact:**

- **BTTS YES + Under 1.5 goals** — both teams scoring needs a total ≥ 2. Impossible in any model.
- **BTTS YES + Under 2.5 goals** — both teams scoring with a total ≤ 2 forces **1–1**, and
  `MatchStatLine`'s constructor throws on `homeGoals == awayGoals`; `EnumerateScores` only ever
  enumerates `h > a` or `a > h`. **Draws are unrepresentable in v1, so this pair is impossible here
  and would be perfectly ordinary in a model with draws.** It sells at a mean decimal 4.72.

### 6.2 Impossible TRIPLES that are not already impossible as a sub-pair — 57 shapes

Exhaustive over 562 matchups. All 57 are always-zero across the population; none is
parameter-dependent. Three generating mechanisms, all exact:

1. **Two scorers on opposite teams + a low total or BTTS NO.** Both teams scoring means total ≥ 2,
   and no draws means total ≥ 3 — so `GLS U1.5` and `GLS U2.5` both die, and `BTTS N` dies by
   definition.
2. **Two scorers on the same team + Under 1.5.** Two distinct players scoring needs ≥ 2 goals from
   that team.
3. **A moneyline + a scorer on the losing team + a low total.** The dog winning while a favourite
   player scores forces total ≥ 3.

Highest naive prices in this set (a full-length ticket that cannot win):

| triple | mean naive decimal price |
|---|---:|
| GLS U1.5 + SCR DOG-DF + SCR DOG-DF | **2070.70** |
| GLS U1.5 + SCR DOG-DF + SCR FAV-DF | 1364.33 |
| GLS U1.5 + SCR FAV-DF + SCR FAV-DF | 950.39 |
| GLS U2.5 + SCR DOG-DF + SCR FAV-DF | 806.43 |
| GLS U1.5 + SCR DOG-DF + SCR DOG-MF | 707.05 |
| BTTS N + SCR DOG-DF + SCR FAV-DF | 627.68 |
| GLS U1.5 + SCR DOG-DF + SCR FAV-MF | 485.76 |
| GLS U1.5 + SCR DOG-MF + SCR FAV-DF | 467.28 |
| GLS U1.5 + SCR DOG-DF + SCR DOG-FW | 380.04 |
| GLS U1.5 + SCR FAV-DF + SCR FAV-MF | 337.50 |
| GLS U2.5 + SCR DOG-DF + SCR FAV-MF | 287.12 |
| GLS U2.5 + SCR DOG-MF + SCR FAV-DF | 276.16 |
| GLS U1.5 + SCR DOG-DF + SCR FAV-FW | 263.94 |
| GLS U1.5 + SCR DOG-FW + SCR FAV-DF | 252.98 |
| GLS U1.5 + SCR DOG-MF + SCR DOG-MF | 241.35 |
| BTTS N + SCR DOG-DF + SCR FAV-MF | 223.46 |
| BTTS N + SCR DOG-MF + SCR FAV-DF | 214.91 |
| GLS U1.5 + SCR FAV-DF + SCR FAV-FW | 182.41 |
| GLS U1.5 + SCR DOG-MF + SCR FAV-MF | 166.72 |
| GLS U1.5 + ML DOG + SCR FAV-DF | 163.47 |
| GLS U2.5 + SCR DOG-DF + SCR FAV-FW | 155.94 |
| GLS U2.5 + SCR DOG-FW + SCR FAV-DF | 149.53 |
| GLS U1.5 + ML FAV + SCR DOG-DF | 134.71 |
| GLS U1.5 + SCR DOG-FW + SCR DOG-MF | 130.12 |
| BTTS N + SCR DOG-DF + SCR FAV-FW | 121.32 |
| GLS U1.5 + SCR FAV-MF + SCR FAV-MF | 119.73 |
| BTTS N + SCR DOG-FW + SCR FAV-DF | 116.39 |
| GLS U2.5 + SCR DOG-MF + SCR FAV-MF | 98.52 |
| GLS U2.5 + ML DOG + SCR FAV-DF | 96.18 |
| GLS U1.5 + SCR DOG-MF + SCR FAV-FW | 90.48 |
| GLS U1.5 + SCR DOG-FW + SCR FAV-MF | 90.19 |
| GLS U2.5 + ML FAV + SCR DOG-DF | 79.42 |
| BTTS N + SCR DOG-MF + SCR FAV-MF | 76.65 |
| BTTS N + ML DOG + SCR FAV-DF | 74.45 |
| GLS U1.5 + SCR DOG-FW + SCR DOG-FW | 69.96 |
| GLS U1.5 + SCR FAV-FW + SCR FAV-MF | 64.88 |
| BTTS N + ML FAV + SCR DOG-DF | 61.65 |
| GLS U1.5 + ML DOG + SCR FAV-MF | 58.31 |
| GLS U2.5 + SCR DOG-MF + SCR FAV-FW | 53.45 |
| GLS U2.5 + SCR DOG-FW + SCR FAV-MF | 53.30 |
| GLS U1.5 + SCR DOG-FW + SCR FAV-FW | 48.95 |
| GLS U1.5 + ML FAV + SCR DOG-MF | 46.21 |
| BTTS N + SCR DOG-MF + SCR FAV-FW | 41.57 |
| BTTS N + SCR DOG-FW + SCR FAV-MF | 41.49 |
| GLS U1.5 + SCR FAV-FW + SCR FAV-FW | 35.12 |
| GLS U2.5 + ML DOG + SCR FAV-MF | 34.30 |
| GLS U1.5 + ML DOG + SCR FAV-FW | 31.69 |
| GLS U2.5 + SCR DOG-FW + SCR FAV-FW | 28.92 |
| GLS U2.5 + ML FAV + SCR DOG-MF | 27.24 |
| BTTS N + ML DOG + SCR FAV-MF | 26.55 |
| GLS U1.5 + ML FAV + SCR DOG-FW | 25.02 |
| BTTS N + SCR DOG-FW + SCR FAV-FW | 22.50 |
| BTTS N + ML FAV + SCR DOG-MF | 21.14 |
| GLS U2.5 + ML DOG + SCR FAV-FW | 18.64 |
| GLS U2.5 + ML FAV + SCR DOG-FW | 14.75 |
| BTTS N + ML DOG + SCR FAV-FW | 14.42 |
| BTTS N + ML FAV + SCR DOG-FW | 11.45 |

Whole-space zero rates: **3.49% of pairs, 13.26% of triples, 27.48% of four-leg combinations** have
`p_joint = 0` exactly.

### 6.3 The mirror problem: 22 pair shapes are logical implications

Not impossible — the opposite. One leg strictly implies the other, so the second leg adds **zero
risk** while the naive product charges a full extra decimal for it.

These were **detected, not assumed**: a pair is flagged when `p_joint == min(p_i, p_j)` to within
1e-12 and ρ > 1. Result: **22 shape classes, each holding in 100% of 12,162 matchups; zero classes
where the relation holds only sometimes.** Confirming the algebra,
`max |ρ − 1/P(weaker-implying leg)| = 4.041e-14` across all 267,564 implication pairs.

| implication shape | min ρ | median ρ | max ρ |
|---|---:|---:|---:|
| GLS U1.5 + GLS U2.5 | 1.9678 | 2.4441 | 3.1081 |
| CRD O4.5 + CRD O5.5 | 1.6469 | 2.2281 | 3.5928 |
| CNR U8.5 + CNR U9.5 | 1.3954 | 2.1836 | **4.1255** |
| BTTS N + GLS U1.5 | 1.6061 | 1.9042 | 2.2812 |
| **BTTS N + GLS U2.5** | 1.6061 | 1.9042 | 2.2812 |
| CNR O10.5 + CNR O9.5 | 1.3199 | 1.8449 | 3.5289 |
| CRD U3.5 + CRD U4.5 | 1.3857 | 1.8143 | 2.5457 |
| CNR U10.5 + CNR U8.5 | 1.2257 | 1.7150 | 2.8799 |
| CNR U10.5 + CNR U9.5 | 1.2257 | 1.7150 | 2.8799 |
| **BTTS Y + GLS O2.5** | 1.4744 | 1.6925 | 2.0333 |
| GLS O2.5 + GLS O3.5 | 1.4744 | 1.6925 | 2.0333 |
| CRD O3.5 + CRD O4.5 | 1.2952 | 1.5612 | 2.1382 |
| CRD O3.5 + CRD O5.5 | 1.2952 | 1.5612 | 2.1382 |
| CNR O10.5 + CNR O8.5 | 1.1835 | 1.4989 | 2.4543 |
| CNR O8.5 + CNR O9.5 | 1.1835 | 1.4989 | 2.4543 |
| GLS U1.5 + GLS U3.5 | 1.2631 | 1.4194 | 1.6434 |
| GLS U2.5 + GLS U3.5 | 1.2631 | 1.4194 | 1.6434 |
| CRD U3.5 + CRD U5.5 | **1.1696** | 1.3891 | 1.7638 |
| CRD U4.5 + CRD U5.5 | **1.1696** | 1.3891 | 1.7638 |
| BTTS Y + GLS O1.5 | 1.2127 | 1.3195 | 1.4767 |
| GLS O1.5 + GLS O2.5 | 1.2127 | 1.3195 | 1.4767 |
| GLS O1.5 + GLS O3.5 | 1.2127 | 1.3195 | 1.4767 |

**267,564 pairs = 3.492% of the space** — coincidentally the same count as the impossible set, since
both are 22 shapes offered on every matchup. Every one of them clears the +EV threshold in 100% of
matchups. Range: worst case `CRD U3.5 + CRD U5.5` / `CRD U4.5 + CRD U5.5` at ρ = 1.1696 →
**+6.1% EV**; best case `CNR U8.5 + CNR U9.5` at ρ = 4.1255 → **+274.2% EV**.

Fourteen of the 22 are the obvious same-market line nestings (three lines per market gives three
nested pairs, across goals/corners/cards, plus the six Under mirrors). **The four BTTS ones are
not obvious, and two of them are v1 draw artefacts:**

- `BTTS Y ⊂ O1.5` — both teams scoring means total ≥ 2. True in any model.
- **`BTTS Y ⊂ O2.5`** — both teams scoring with no draws forces at least 2–1, so total ≥ 3.
- `U1.5 ⊂ BTTS N` — total ≤ 1 means at most one team scored. True in any model.
- **`U2.5 ⊂ BTTS N`** — total ≤ 2 with no draws forces 2–0 or 0–2.

Impossible plus implied: **6.98% of all two-leg tickets on this board are either logically dead or
logically redundant.**

---

## 7. Does correlation beat compounded vig?

Clearing rates against `ρ > (1+Ω)^n` — i.e. the player is +EV if the book sells the SGP at the bare
product of leg odds:

| n | threshold | combinations evaluated | clearing | share |
|---:|---:|---:|---:|---:|
| 2 | 1.102500 | 7,662,060 | 1,232,314 | **16.08%** |
| 3 | 1.157625 | 4,012,680 | 1,088,543 | **27.13%** |
| 4 | 1.215506 | 4,123,350 | 1,170,724 | **28.39%** |

**Answer: yes, on a large minority of the board, and the margin is not small.** The winners are not
marginal — the classic shapes clear by 20–120 percentage points of EV (§9), and the nested-line
shapes of §6.3 clear unconditionally.

But the clearing set is concentrated, not diffuse. **All 51.4% of pairs that span two families
clear nothing** (ρ = 1 exactly, so EV = 1/1.1025 − 1 = −9.30%, the full compounded vig). Naive
product pricing is simultaneously far too generous on within-family correlated shapes and exactly
correct-but-vig-heavy on everything else.

### Triples by family composition (4,012,680)

| composition | n | min | median | p95 | max | % ρ=0 | % +EV |
|---|---:|---:|---:|---:|---:|---:|---:|
| 3 GOAL | 1,137,488 | 0.0000 | 0.8782 | 2.3782 | **7.0179** | 21.49 | 30.44 |
| 2 GOAL + 1 CORNER | 930,672 | 0.0000 | 0.9468 | 1.6938 | 3.1081 | 3.62 | 28.88 |
| 2 GOAL + 1 CARD | 930,672 | 0.0000 | 0.9468 | 1.6938 | 3.1081 | 3.62 | 28.88 |
| 1 GOAL + 1 CORNER + 1 CARD | 485,568 | 1.0000 | 1.0000 | 1.0000 | 1.0000 | 0.00 | **0.00** |
| 1 GOAL + 2 CORNER | 202,320 | 0.0000 | 0.4202 | 2.7798 | 4.1255 | 40.00 | 40.00 |
| 1 GOAL + 2 CARD | 202,320 | 0.0000 | 0.5474 | 2.2281 | 3.5928 | 40.00 | 40.00 |
| 2 CORNER + 1 CARD | 50,580 | 0.0000 | 0.4202 | 2.7798 | 4.1255 | 40.00 | 40.00 |
| 1 CORNER + 2 CARD | 50,580 | 0.0000 | 0.5474 | 2.2281 | 3.5928 | 40.00 | 40.00 |
| 3 CORNER | 11,240 | 0.0000 | 0.0000 | 3.1904 | **11.8812** | 80.00 | 10.68 |
| 3 CARD | 11,240 | 0.0000 | 0.0000 | 2.9397 | 7.6822 | 80.00 | 10.64 |

Note the row that should govern any "one leg per market" intuition: a triple that takes exactly one
leg from each family has ρ = 1.0000 in **every one of 485,568 cases**. There is no correlation to
capture on those tickets at all.

### Extremes by shape

**Top 10 triple shapes by median ρ** (562 matchups, exhaustive):

| triple shape | min | median | max | med shorten |
|---|---:|---:|---:|---:|
| BTTS N + GLS U1.5 + GLS U2.5 | 3.1830 | **4.6613** | 7.0179 | 0.237 |
| CNR U10.5 + CNR U8.5 + CNR U9.5 | 1.7103 | 3.7454 | **11.8812** | 0.294 |
| CRD O3.5 + CRD O4.5 + CRD O5.5 | 2.1331 | 3.4784 | 7.6822 | 0.317 |
| GLS U1.5 + GLS U2.5 + GLS U3.5 | 2.4860 | 3.4757 | 5.1079 | 0.317 |
| BTTS Y + GLS O3.5 + SCR DOG-DF | 2.4885 | 3.3341 | 4.8686 | 0.331 |
| BTTS Y + GLS O3.5 + SCR DOG-MF | 2.4477 | 3.2786 | 4.7963 | 0.336 |
| BTTS Y + GLS O3.5 + SCR DOG-FW | 2.3443 | 3.1981 | 4.6921 | 0.345 |
| BTTS Y + GLS O2.5 + GLS O3.5 | 2.3485 | 3.1041 | 4.5055 | 0.355 |
| BTTS Y + GLS O3.5 + SCR FAV-DF | 2.2972 | 2.9652 | 4.0973 | 0.372 |
| GLS O2.5 + GLS O3.5 + SCR DOG-DF | 2.2596 | 2.8492 | 3.8759 | 0.387 |

**Bottom 10 non-zero triple shapes by median ρ:**

| triple shape | min | median | max |
|---|---:|---:|---:|
| ML DOG + SCR FAV-DF + SCR FAV-DF | **0.0581** | **0.1018** | 0.1795 |
| ML DOG + SCR FAV-DF + SCR FAV-MF | 0.0610 | 0.1077 | 0.1938 |
| ML DOG + SCR FAV-MF + SCR FAV-MF | 0.0641 | 0.1140 | 0.1989 |
| ML DOG + SCR FAV-DF + SCR FAV-FW | 0.0634 | 0.1175 | 0.2039 |
| ML DOG + SCR FAV-FW + SCR FAV-MF | 0.0668 | 0.1232 | 0.2182 |
| ML DOG + SCR FAV-FW + SCR FAV-FW | 0.0724 | 0.1321 | 0.2285 |
| BTTS N + GLS O3.5 + ML DOG | 0.1075 | 0.1638 | 0.2393 |
| BTTS N + GLS O3.5 + SCR DOG-FW | 0.0904 | 0.1666 | 0.2899 |
| BTTS N + GLS O3.5 + SCR DOG-MF | 0.1080 | 0.1893 | 0.3117 |
| GLS U2.5 + SCR FAV-DF + SCR FAV-DF | 0.1551 | 0.1906 | 0.2354 |

**Four-leg (4,123,350 combinations over 70 matchups).** ρ = 0 in 27.48%; +EV in 28.39%.
Global ρ range **[0, 14.8220]**.

| top shape | min | median | max | med shorten |
|---|---:|---:|---:|---:|
| BTTS N + GLS U1.5 + GLS U2.5 + GLS U3.5 | 4.0622 | **6.5550** | 11.0817 | 0.177 |
| GLS O3.5 + ML DOG + SCR DOG-DF + SCR DOG-DF | 4.3170 | 6.3365 | 10.0768 | 0.183 |
| GLS O3.5 + ML DOG + SCR DOG-DF + SCR DOG-MF | 4.1526 | 6.1537 | 9.7687 | 0.188 |
| BTTS Y + GLS O2.5 + GLS O3.5 + SCR DOG-DF | 4.0267 | 6.0004 | 9.8273 | 0.193 |
| GLS O3.5 + ML DOG + SCR DOG-MF + SCR DOG-MF | 4.0663 | 5.9663 | 9.4538 | 0.194 |
| CRD O4.5 + CRD O5.5 + GLS U1.5 + GLS U2.5 | 3.2482 | 5.6419 | **11.1667** | 0.205 |
| CNR U8.5 + CNR U9.5 + GLS U1.5 + GLS U2.5 | 2.7522 | 5.3593 | **12.8223** | 0.216 |
| CNR U8.5 + CNR U9.5 + CRD O4.5 + CRD O5.5 | 2.2982 | 5.1073 | **14.8220** | 0.227 |

| bottom shape | min | median | max |
|---|---:|---:|---:|
| ML DOG + SCR FAV-DF + SCR FAV-DF + SCR FAV-MF | **0.0103** | **0.0245** | 0.0465 |
| ML DOG + SCR FAV-DF + SCR FAV-MF + SCR FAV-MF | 0.0108 | 0.0259 | 0.0492 |
| ML DOG + SCR FAV-DF + SCR FAV-DF + SCR FAV-FW | 0.0111 | 0.0265 | 0.0506 |
| ML DOG + SCR FAV-FW + SCR FAV-FW + SCR FAV-FW | 0.0142 | 0.0338 | 0.0640 |
| ML FAV + SCR DOG-DF + SCR DOG-DF + SCR DOG-MF | 0.0290 | 0.0842 | 0.1502 |

---

## 8. Correctly-priced odds vs the naive product (the UX number)

`o_sgp / Π o_i = (1+Ω)^(n−1) / ρ`. **Below 1 the correct price is shorter than the naive product;
above 1 it is longer.** Reported over the full exhaustive sweeps.

| n | total | ρ=0 (unpriceable) | ρ=1 exactly | correct price **SHORTER** | correct price **LONGER** |
|---:|---:|---:|---:|---:|---:|
| 2 | 7,662,060 | 267,564 (3.49%) | 226,646 (2.96%) | 1,412,666 (**18.44%**) | 5,981,830 (**78.07%**) |
| 3 | 4,012,680 | 532,214 (13.26%) | 18,120 (0.45%) | 1,173,088 (**29.23%**) | 2,307,378 (**57.50%**) |
| 4 | 4,123,350 | 1,133,300 (27.48%) | 0 (0.00%) | 1,256,917 (**30.48%**) | 1,733,133 (**42.03%**) |

Distribution of the ratio (ρ = 0 excluded — those have no finite price):

| n | min | p05 | p25 | median | p75 | p95 | max |
|---:|---:|---:|---:|---:|---:|---:|---:|
| 2 | 0.255 | 0.670 | **1.050** | **1.050** | **1.050** | 2.257 | 5.071 |
| 3 | 0.093 | 0.530 | 0.854 | **1.103** | 1.331 | 2.914 | 18.985 |
| 4 | 0.078 | 0.418 | 0.739 | **1.192** | 1.608 | 4.000 | 112.907 |

**The headline is counter-intuitive and worth stating flatly: on this board, correct SGP pricing
LENGTHENS the odds far more often than it shortens them.** At two legs the median ratio is exactly
1.050 — the correct price is 5% longer than the naive product — because the modal two-leg ticket is
cross-family and genuinely independent, and the naive product charges vig twice on a bet that only
carries it once. Only 18.4% of two-leg tickets shorten at all.

The tail in the other direction is severe, though: the p05 ratio at four legs is 0.418 and the
minimum is 0.078, i.e. a correct price can be one-thirteenth of the naive product.

**Read the "ρ=1 exactly" column carefully — it is a bit-equality count, not an independence count.**
A genuinely independent pair computes `ρ = p_joint/(p_i·p_j)` as 1 ± 1e-16, which lands on the
double 1.0 only sometimes; the true independent share at n=2 is 51.43% (§5.1), not 2.96%. The
column is included only to show it collapses to zero by n=4, which is arithmetic rather than a
finding: a four-leg ticket cannot draw one leg from each of only three families without doubling up.
The SHORTER/LONGER split either side of it is unaffected, since the threshold comparison is not a
bit-equality test.

---

## 9. Curated real-world SGP shapes (POP-A, all 12,000 matchups)

"best FW on FAV" is the highest-weight forward on the favourite (deterministic tiebreak by roster
index). EV is per unit stake at naive product pricing.

| shape | legs | min ρ | median ρ | max ρ | median EV | % +EV | med shorten |
|---|---:|---:|---:|---:|---:|---:|---:|
| **FAV ML + OVER 2.5 + best FW on FAV** | 3 | 1.3770 | **1.6257** | 1.9842 | **+40.4%** | 100.0 | 0.678 |
| FAV ML + OVER 2.5 + best FW FAV + BTTS YES | 4 | 1.9252 | **2.6175** | 3.8701 | **+115.4%** | 100.0 | 0.442 |
| DOG ML + OVER 2.5 + best FW on DOG | 3 | 1.6722 | 2.0596 | 2.6875 | +77.9% | 100.0 | 0.535 |
| **DOG ML + UNDER 2.5 + BTTS NO** (mirror) | 3 | 1.6561 | **1.9742** | 2.3921 | **+70.5%** | 100.0 | 0.559 |
| UNDER 2.5 + BTTS NO | 2 | 1.6093 | 1.9038 | 2.2812 | +72.7% | 100.0 | 0.552 |
| FAV ML + OVER 3.5 + best FW on FAV | 3 | 1.5644 | 1.8895 | 2.3285 | +63.2% | 100.0 | 0.584 |
| FAV ML + UNDER 2.5 + BTTS NO | 3 | 1.5725 | 1.8672 | 2.2795 | +61.3% | 100.0 | 0.590 |
| DOG ML + best FW on DOG | 2 | 1.4619 | 1.7510 | 2.2437 | +58.8% | 100.0 | 0.600 |
| OVER 2.5 + BTTS YES | 2 | 1.4748 | 1.6929 | 2.0331 | +53.6% | 100.0 | 0.620 |
| FAV ML + OVER 2.5 + BTTS YES | 3 | 1.4503 | 1.6755 | 2.0295 | +44.7% | 100.0 | 0.658 |
| FAV ML + OVER 1.5 + best FW on FAV | 3 | 1.3241 | 1.5534 | 1.8814 | +34.2% | 100.0 | 0.710 |
| FAV ML + OVER 1.5 + best FW FAV + OVER 9.5 CNR | 4 | 1.3241 | 1.5534 | 1.8814 | +27.8% | 100.0 | 0.745 |
| **FAV ML + best FW on FAV** | 2 | 1.1914 | **1.3436** | 1.5752 | **+21.9%** | 100.0 | 0.782 |
| OVER 2.5 + best FW on FAV | 2 | 1.1909 | 1.2854 | 1.4198 | +16.6% | 100.0 | 0.817 |
| DOG ML + UNDER 2.5 | 2 | 1.0000 | 1.0349 | 1.1026 | −6.1% | 0.0 | 1.015 |
| **FAV ML + OVER 2.5** | 2 | 1.0000 | **1.0143** | 1.0232 | **−8.0%** | 0.0 | 1.035 |
| FAV ML + BTTS NO | 2 | 1.0000 | 1.0099 | 1.0194 | −8.4% | 0.0 | 1.040 |
| best FW FAV + 2nd FW FAV (same team) | 2 | 0.9395 | 1.0099 | 1.0557 | −8.4% | 0.0 | 1.040 |
| FAV ML + BTTS YES | 2 | 0.9834 | 0.9882 | 1.0000 | −10.4% | 0.0 | 1.063 |
| best FW FAV + best FW DOG (opposite teams) | 2 | 0.8292 | 0.8926 | 0.9470 | −19.0% | 0.0 | 1.176 |
| **OVER 2.5 + OVER 9.5 CNR + OVER 4.5 CRD** | 3 | **1.0000** | **1.0000** | **1.0000** | **−13.6%** | 0.0 | **1.103** |
| **FAV ML + best FW on DOG** | 2 | 0.4188 | **0.5532** | 0.6919 | **−49.8%** | 0.0 | 1.898 |

Five things this table says that the aggregates do not:

1. **The classic SGP works.** Favourite ML + Over + a forward from that team is +40.4% median EV at
   naive pricing, and it is +EV in **100.0%** of 12,000 matchups — never once negative.
2. **The mirror also works**, and slightly harder on the dog side (`DOG ML + best FW on DOG` at
   ρ = 1.7510 vs `FAV ML + best FW on FAV` at 1.3436) — a dog win requires the dog to score, and
   the dog scores less often, so the conditioning bites harder.
3. **Moneyline and total goals are nearly independent** (`FAV ML + OVER 2.5`, ρ = 1.0143). The
   correlation everyone expects between "favourite wins" and "goals" barely exists in this model,
   because the winner is drawn first from `TrueHomeProb` and the score only afterwards. The ρ = 1.0
   *minimum* on every ML+total shape is the p → 0.5 corner of the space, where the two branches are
   mirror images and the correlation vanishes exactly.
4. **Two forwards on the favourite is not a correlated bet** (ρ median 1.0099, min 0.9395). Needing
   ≥ 2 goals from that team pushes up; the two players competing for the same goals pushes down; on
   the favourite they nearly cancel. On the underdog the first effect wins (ρ ≈ 1.09).
5. **Cross-family "value" tickets are pure vig.** Over 2.5 + Over 9.5 corners + Over 4.5 cards is
   ρ = 1.0000 in every single matchup, so the correct price is exactly 10.3% *longer* than the
   naive product and the naive ticket is −13.6% EV.

---

## 10. What the numbers constrain for step 2

Only what is forced by the measurements above. Pricing-rule selection is the lead's call.

1. **A correlation-aware price is not optional for correctness, independent of any EV argument.**
   22 pair shapes and 57 additional triple shapes have `p_joint = 0` exactly, on every matchup, in
   both populations. 3.49% of pairs / 13.26% of triples / 27.48% of four-leg combinations. Naive
   product pricing sells these at finite positive odds — up to a mean decimal **2070.70** — and they
   cannot win. Whatever step 2 ships must either compute the joint or block these combinations.

2. **Two of the impossible pairs are model artefacts, not football.** `BTTS YES + Under 2.5` and
   `Under 2.5 ⊂ BTTS NO` are consequences of draws being unrepresentable in v1
   (`MatchStatLine` throws on equal goals; `EnumerateScores` enumerates only `h > a` / `a > h`).
   Any decision taken here is coupled to that v1 constraint and would need revisiting if draws are
   ever added.

3. **22 pair shapes are logical implications, and they are the largest free edge on the board.**
   One leg adds zero risk while the naive product charges a full extra decimal for it: ρ =
   1/P(implying leg), +EV in 100% of matchups, from +6.1% to +274.2%. 3.492% of the pair space —
   detected empirically, not assumed, with zero parameter-dependent cases. Any rule that prices the
   joint handles these automatically; any rule that does not must handle them explicitly. Note that
   two of them (`BTTS Y ⊂ O2.5`, `U2.5 ⊂ BTTS N`) exist only because draws are unrepresentable, so
   they share constraint 2's coupling to the v1 model.

4. **Cross-family legs are exactly independent (max |ρ − 1| = 4.4e-14 over 3.94M pairs).** 51.4% of
   pairs and 12.1% of triples fall here. A correlation model that produces anything other than
   exactly 1.0 on these would be introducing error the engine does not contain. This also bounds the
   work: all real correlation lives inside the GOAL family plus same-market line nesting.

5. **Correlation does beat compounded vig, on 16.1% of pairs / 27.1% of triples / 28.4% of four-leg
   combinations**, and on the classic SGP shapes it beats it by 20–120 percentage points of EV with
   a 100% hit rate across 12,000 matchups. Naive product pricing is not a safe placeholder.

6. **The shortening intuition is backwards at low leg counts.** Correct pricing lengthens 78.07% of
   two-leg, 57.50% of three-leg and 42.03% of four-leg tickets; the two-leg median ratio is exactly
   1.050. Any surface that tells the player an SGP price is "shorter because the legs are related"
   would be wrong for most tickets built on this board. The full distribution is in §8; the tail in
   the shortening direction is nonetheless extreme (p05 = 0.418 at four legs, min 0.078).

7. **The magnitude range a pricing implementation must survive:** ρ ∈ [0, 3.1081] at two legs,
   [0, 11.8812] at three, [0, 14.8220] at four. Shortening ratio up to 112.907 at four legs. Any
   fixed-width odds display or fixed-point price representation has to hold those.

---

## 11. Honest uncertainty

- **The evaluator is exact, not an approximation.** It enumerates the same finite distributions the
  sampler draws from, so `p_joint` is the true joint probability of the simulator, not a model of
  it. Verified structurally against `SampleStatLine` (draw order and independence),
  `SampleScorers` (per-goal categorical over the same roster weights) and `Grades`
  (reference-equality scorer matching). The gate in §1 is the empirical backstop.
- **All of this is conditional on one matchup.** Cross-matchup parlays are ordinary independent
  parlays and were not swept; nothing here says anything about them.
- **POP-B rosters are synthetic and un-jittered.** POP-B scorer statistics show role structure
  without the shipped ±35% per-player spread. Every scorer statistic quoted from POP-A (including
  all of §9 and the marginals table) uses the real jittered rosters.
- **The triple and four-leg sweeps use reduced matchup populations** (562 and 70) because they are
  exhaustive over C(36,3) and C(36,4). Both include the full 162-point latent grid, so the corners
  of the parameter space are covered even where the random-slate sample is thin. Shape-class
  statistics with n < 50 (triples) or n < 20 (four-leg) were excluded from the ranked tables.
- **Relics are out of scope.** `boosted_odds` / `promo_code` rewrite `Leg.OfferedOdds` after
  pricing; every EV number here assumes the base offered odds `1/(p(1+Ω))`.
- **Overround is assumed at the default 0.05 throughout.** The thresholds `(1+Ω)^n` and every EV
  figure move if that dial moves; ρ itself does not — it is a pure property of the joint
  distribution and is overround-independent.
