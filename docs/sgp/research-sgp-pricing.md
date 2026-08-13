# External research: how real sportsbooks price and settle same-game parlays

**Dispatch:** D1 · sgp worktree, Lane 2 (same-game parlays) · **Type:** external research, docs-only
**Date:** 2026-08-12/13
**Scope:** publicly available sources only (vendor sites, operator house rules, journalism, practitioner
analysis). No proprietary, NDA'd, or paywalled-beyond-public data was used.

## How to read the confidence labels

Q1 in the brief asks for four distinct categories; the claims table uses all four so nothing gets
laundered into a false "industry standard":

- **Vendor-stated** — an odds-tech vendor (Genius Sports, Sportradar, OpticOdds) describing its own
  product, in its own published material. Marketing-adjacent; still the closest thing to primary
  source on pricing mechanics because operators themselves publish essentially nothing technical.
- **Operator-stated** — a sportsbook's own published house rules, help-center article, or official
  account statement. The highest-confidence category for *rules* (settlement, void, cash-out), not
  for *pricing mechanics* (no operator publishes its pricing model).
- **Journalism** — reporting by a named outlet/reporter, or a bettor's documented, verifiable
  interaction with a book (e.g., a screenshotted bet slip).
- **Practitioner-inference** — a bettor's or analyst's own reasoning, worked example, or estimate,
  not sourced to an operator disclosure. This is most of Q2 and Q4 — flagged every time.

Every row below is independently checked against the fetched primary text, not against a search
engine's AI-generated summary of that text (those summaries were used only to find candidate sources,
then discarded — two of them turned out to materially misstate their own cited page, noted inline
where relevant).

## Claims table

| Claim | Source | URL | Confidence | Quote |
|---|---|---|---|---|
| **— Q1: Pricing mechanism —** | | | | |
| Genius Sports frames its bet-builder pricing as Monte Carlo simulation feeding a "MultiBet" engine that handles correlated market types while staying consistent with single-market prices | Genius Sports, "Trader's View" blog, Thomas Holland (VP Product), 8 May 2025 | https://www.geniussports.com/content-hub/traders-view-monte-carlo-models-the-hidden-engine-behind-bettings-next-revolution/ | Vendor-stated | "Monte Carlo models have transformed our betbuilder capabilities... our MultiBet solution leverages this automation for sharp multi-leg pricing at scale, including when market-types are correlated, but at the same time ensuring that pricing is consistent with singles." |
| Genius Sports describes simulation-derived probabilities as literally a frequency count over simulated outcomes | same as above | same | Vendor-stated | "Imagine betbuilder pricing driven by millions of simulations, where every possible bet combination is considered, including the relationship between legs, and generating these probabilities becomes as easy as counting the number of times it happened in our simulation." |
| Sportradar's CustomBet (bet-builder) product classifies every leg pair into one of three correlation types before pricing | Sportradar, official UOF/CustomBet developer documentation | https://docs.sportradar.com/uof/custombet-cb/intro | Vendor-stated | "We use simulation data to assess the correlation between all selections. There are three types: Independent... Positively correlated... Negatively correlated..." |
| Sportradar's disclosed formula is marginal-probability multiplication adjusted by a scalar correlation factor, not a full joint simulation returned per-bet | same as above | same | Vendor-stated | "Calculate combined probabilities: Multiply the individual fair probabilities of each selection and adjust by the correlation factor to get the combined probability... Combined Probability = Correlation factor \* Probability 1 \* Probability 2 \* Probability 3..." then "Convert probability to odds: fair odds = 1/fair probability" then "a margin is added on top of the fair odds to generate the final odds offered" |
| Sportradar's Alpha Odds (a *separate* product from CustomBet — general AI-driven risk/liability-based odds recalculation, not specifically SGP correlation) claims a measured client-profit uplift | Sportradar press release via GlobeNewswire, republished by Yahoo Finance, 30 Jan 2024 | https://finance.yahoo.com/news/sportradar-announces-automated-odds-recalculation-070000503.html | Vendor-stated | "Alpha Odds, its automated odds recalculation tool which allows operators to generate bespoke betting prices in line with their risk exposure and liabilities, delivered an average profit increase of 10% for clients in 2023... Launched in 2022, Alpha Odds boasts a client roster of more than 60 betting operators around the world." |
| OpticOdds (a competing odds-data vendor) sells its own bet-builder/SGP pricing engine, "AlgoOdds," built on blended consensus lines rather than disclosed simulation | OpticOdds blog | https://opticodds.com/blog/correlation-in-same-game-parlays | Vendor-stated | "These tools use cutting-edge analytics and real-time predictive modeling to handle correlation dynamically. At the heart of this solution is AlgoOdds, a sophisticated technology that blends multiple consensus betting lines from top sources." |
| A professional gambling mathematician's *reconstruction* of plausible SGP pricing math uses a Gaussian-copula transform of marginals plus a correlation matrix, evaluated by Monte Carlo or numerical integration — explicitly presented as the author's own explanatory model, not a disclosed operator method | Wizard of Odds ("The Mathematics of Player Props," Article 4/5), updated Aug 2026 | https://wizardofodds.com/article/same-game-parlays-the-mathematics-of-correlation/ | Practitioner-inference | Article disclaimer: "This article is for educational purposes only... The goal is to understand the mathematical principles behind their pricing." Method: "This integral over the multivariate normal distribution is typically computed using Monte Carlo simulation or numerical integration methods." |
| Same author's inference on real-world practice: books likely blend two methods rather than using one pure model | same as above | same | Practitioner-inference | "Most sophisticated sportsbooks use a hybrid approach: empirical frequencies where data is abundant, Gaussian copulas or other models to fill in gaps and smooth estimates." |
| FanDuel is widely credited as the first US operator to build in-house SGP correlation models (2019), rather than licensing a vendor product, though this is not confirmed by FanDuel itself in any source found | juicereel.beehiiv.com, "Same Game Parlays: The Books Play Dirty," Dec 2024 | https://juicereel.beehiiv.com/p/the-dangers-of-same-game-parlays | Journalism/practitioner-inference | "Then, in 2019, FanDuel flipped the script. They began to model the correlation between events in the same game, going beyond spreads and totals to include a whole universe of prop bets..." |
| Sportradar's Bet Builder infrastructure has a dedicated API endpoint that **precomputes, at bet-placement time, the correlation-adjusted odds for every possible single-leg-void outcome** — i.e. the vendor-level architecture is built for genuine re-pricing on a void, not for dropping the leg at odds of 1.0 | Sportradar UOF/CustomBet API docs, "Void Recalculation" endpoint | https://docs.sportradar.com/uof/custombet-cb/api/void-recalculation | Vendor-stated | "Void Recalculation: Gives you precomputed odds for all possible single-void scenarios." Integration step: "call the Void Recalculation API using the same selections to get the latest price with the precomputed void combination... If one selection is later declared void, retrieve the matching precomputed void combination. Use the returned odds to settle the remaining valid selections." |
| That same Sportradar mechanism has a hard limitation: it only supports **one** voided leg per bet; multiple simultaneous voids fall outside the precomputed-recalculation path | same as above | same | Vendor-stated | "Only one voided selection is supported. Multiple voided selections are not supported. If the Bet Builder contains only 2 selections, no precomputed void combinations are returned. If one selection becomes void, simply use the original odds of the remaining selection." |
| **— Q2: Margin / hold —** | | | | |
| SGP hold is estimated at 4–6x straight-bet hold and ~50% higher than ordinary (cross-game) parlay hold | juicereel.beehiiv.com, Dec 2024 | https://juicereel.beehiiv.com/p/the-dangers-of-same-game-parlays | Practitioner-inference | "The average hold (house edge) on SGPs is 4x to 6x higher than straight bets, and 50% higher than traditional parlays." |
| Two components of the "extra tax": a correlation adjustment (legitimate, protects the book from being over-paid on positively correlated legs) plus an additional, opaque margin layered on top simply because the true correlation isn't visible to the bettor | same as above | same | Practitioner-inference | "First, the correlation between legs in the parlay is estimated, and the price is adjusted up or down accordingly... Second, the sportsbook takes advantage of the fact that their correlation estimates are not visible to the bettor, and hits the bettor with an extra tax... simply because they can." |
| A separate estimate puts SGP hold at 3–5x straight-bet hold, citing unspecified "public data and earnings reports" | OddsIndex guide | https://oddsindex.com/guides/same-game-parlay-correlation | Practitioner-inference | "Public data and earnings reports suggest that SGP hold rates can be three to five times higher than straight bets." |
| Sharp-adjacent analyst outlet explicitly notes operators do **not** break out SGP-specific hold; the commonly cited 20–30% figure is inferred from blended parlay-category data, not disclosed for SGP alone | Establish The Run, Jack Miller, 30 Mar 2024 | https://establishtherun.com/should-you-bet-same-game-parlays/ | Journalism/practitioner-inference | "Books aren't required to report numbers for SGPs specifically, but other numbers indicate sportsbooks regularly profit more from parlays than they do on straight bets, all while holding a significantly higher percentage — sometimes in the 20-30% range as opposed to single digits on straights. It's fair to assume hold percentages on SGPs are at least that high, if not far higher." |
| ESPN's betting reporter profiles the typical SGP bettor as recreational/narrative-driven and small-stakes, which is the demand side of why the product is high-margin | ESPN, David Payne Purdum, 2023 (as quoted by Establish The Run) | https://www.espn.com/chalk/story/_/id/35572104/2023-super-bowl-betting-rise-same-game-parlay | Journalism | "The typical SGP bettor likes to create narratives about how the game will play out and fill their parlay with a variety of point spreads, over/under totals, and player props that align with their expected storylines. They play for small stakes, $5-$20 on average..." |
| A specific, checkable state-level figure: New Jersey, Sept 2024, ~24.2% calculated hold on $352.7M of parlay handle vs ~4.6% on single-event handle the same month. **Not independently re-verified by this dispatch against the underlying NJ DGE filing** — attributed to unnamed "industry trackers" by a secondary content site | ysnlive.com | https://ysnlive.com/how-parlay-margins-work-and-how-to-use-them/ | Practitioner-inference | "In September 2024, New Jersey reported parlay figures that industry trackers calculated to a 24.2 percent hold on 352.7 million dollars of parlay handle, while single-event wagers held around 4.6 percent in the same month." |
| Estimate that built-in parlay hold reaches ~15–20% at five legs, and that a $20 stake on a five-leg parlay carries >35% expected cost vs 4.5% for the same stake on straights. Source does not cite primary data for these specific numbers | igamingnews.biz, trade-press commentary | https://www.igamingnews.biz/same-game-parlay-sportsbook-margin-engine/ | Practitioner-inference | "Industry analysis puts the built-in hold at roughly 15 to 20% once a parlay reaches five legs... The same 20 dollars staked on straight bets faces a 4.5% expected cost; on a five-leg parlay it faces an edge above 35%." |
| Claim that operators' own quarterly investor disclosures report 20–30% "realized parlay hold." **Flagged low-confidence**: this source reads as generic SEO/AI-assisted content (generic byline, formulaic structure) and the claim was not independently traced to an actual DraftKings/Flutter earnings call or 10-K by this dispatch | tech-insider.org | https://tech-insider.org/sports-betting/parlay-betting-explained/ | Practitioner-inference (low confidence — unverified) | "The 20–30% realized parlay hold operators report on quarterly investor calls (see Flutter Entertainment and DraftKings public disclosures)..." |
| **— Q3: Settlement, void, restriction, cash-out —** | | | | |
| DraftKings defines SGP as win-all-legs *except* legs settled void/push, with sport-specific settlement detail deferred elsewhere in the same document | DraftKings Massachusetts House Rules, filed with MA Gaming Commission, effective 8/1/24 | https://massgaming.com/wp-content/uploads/DraftKings-House-Rules-8.1.24.pdf | Operator-stated | "A 'Same Game Parlay' is a single bet combining multiple selections from the same event and is dependent on all of those selections winning, with the exception of some selections which are settled as void or push." |
| DraftKings explicitly carves Same Game Parlay **out** of the rule that lets an ordinary parlay survive a void leg (i.e. SGP is *not* guaranteed the drop-and-continue treatment by default) | same document | same | Operator-stated | "A bet made as a parlay, except made as a Same Game Parlay, shall remain valid notwithstanding a game or an event which is part of the parlay bet being void." |
| DraftKings also carves SGP **out** of the rule banning correlated legs on ordinary parlays — confirming correlation is the deliberate product, not an accident to be voided, *only* inside SGP | same document | same | Operator-stated | "A bet made as a parlay, except made as a Same Game Parlay, shall never include two or more offers where the outcomes of which might turn out to be related... DraftKings reserves the right, solely at its own discretion, to declare void all parts of the accumulative/parlay bet which include the correlated outcomes." |
| DraftKings' one *concrete, documented* SGP repricing case (soccer, pre-live, non-participating player): that leg voids and the **whole SGP is repriced** on remaining legs using last available pre-match odds | same document, Soccer § Pre-live Same Game Parlays | same | Operator-stated | "in the event a pre-live Same Game Parlay contains a selection applicable to a player who did not participate in the match ('Soccer Non-Participating Player'), the selection... will be voided and the pre-live Same Game Parlay will be repriced based on the last odds available to DraftKings prior to the start of the match." |
| But that same DraftKings clause makes repricing the **narrow exception, not the rule**: any other void/push selection in the same pre-live SGP voids the *entire* ticket regardless of how the rest graded | same document | same | Operator-stated | "in the event a pre-live Same Game Parlay contains at least one (1) selection, other than a selection containing a Soccer Non-Participating Player, which is settled as void or push, then the whole pre-live Same Game Parlay will be settled as void, irrespective of whether the pre-live Same Game Parlay contains other winning or losing select[ions]." |
| A documented 2021 bettor complaint is consistent with "void voids the whole SGP" being DraftKings' default outside that narrow soccer carve-out: an 8-of-9-leg NFL SGP was cancelled in full after the 9th leg (an inactive player's prop) voided | Sportshandle, quoting @Ray_Levay13's public tweet | https://sportshandle.com/same-game-parlay/ | Journalism | "Hit 8/9 legs of a Same Game Parlay and the last leg (Zack Moss) gets voided due to him being inactive, which makes sense. But, instead of adjusting the odds, the whole bet gets cancelled?" |
| DraftKings' Cash Out feature is described generally as available on "single and parlay bets" pre-game and live, with no SGP-specific exclusion located in the general clause (absence of an exclusion is not proof of inclusion — see Gaps) | same DraftKings document | same | Operator-stated | "The 'Cash Out' function allows the Authorized Account Holder the possibility to redeem a bet, which status has not been settled yet... It is available on selected events both in pre-game and live, as well as on both single and parlay bets." |
| DraftKings' rules define "Progressive Parlay" as its own distinct wager type (separate from SGP) in the same Definitions section that defines SGP — i.e. partial/progressive payout is a *named, different product*, not a behavior of ordinary SGPs | same document | same | Operator-stated | Definitions list: "...settlement rules for Same Game Parlays are set forth in the Sports Rules for the relevant sport. Progressive Parl[ay]..." (definition continues past extraction boundary; existence of the distinct defined term is confirmed) |
| BetMGM's **current** rule (2025 MA filing) is recalculation-by-default: a cancelled SGP pick reprices the ticket on remaining legs, with full-void reserved only for when repricing isn't possible | BetMGM Massachusetts House Rules, revised 3/28/2025 | https://massgaming.com/wp-content/uploads/BetMGM-House-Rules-3.28.25.pdf | Operator-stated | "'Same Game Parlay' (SGP)... If a pick within a SGP is cancelled, then the wager odds at the time of bet placement will be re-calculated using the remaining legs, unless specified otherwise in the specific sport's betting rules. BetMGM reserves the right to cancel a SGP or SGP+ if it is unable to adjust the odds of the wager after a selection is cancelled." |
| BetMGM's expanded "SGP+" product (SGP combined with other games/straights) follows the same recalculate-on-remaining-legs logic | same document | same | Operator-stated | "'Same Game Parlay Plus' (SGP+)... If a straight selection or SGP within the SGP+ wager is voided, then the SGP+ wager odds will be re-calculated using the remaining legs." |
| **Discrepancy flagged**: an older secondary source describes a BetMGM product under the name "One Game Parlay" with the opposite rule (any cancelled leg voids the whole ticket). This may reflect a since-changed product name/rule, an older house-rules version, or a different market — not reconciled by this dispatch | Sportshandle (undated in capture; product-naming context suggests ~2022) | https://sportshandle.com/same-game-parlay/ | Journalism (older, conflicts with current operator-stated rule above) | "At BetMGM, the 'One Game Parlay' rules state simply: 'If a pick within a One Game Parlay is cancelled, then the entire One Game Parlay will be cancelled.'" |
| Caesars explicitly excludes Same Game Parlay from its ordinary accumulator "reduce and continue" void rule | Caesars Massachusetts House Rules, filed 3/8/2023 (oldest of the three filings used — see Gaps) | https://massgaming.com/wp-content/uploads/Caesars-House-Rules-3.8.23.pdf | Operator-stated | "In accumulative bets the stake will run on to the remaining selection(s) with a two-leg parlay becoming a straight, a three-leg parlay becoming a two-leg parlay and so on. Note: this does not apply to Same Game Parlay." |
| Caesars bans correlated legs on ordinary (non-SGP) parlays outright, with an error-handling fallback rather than a repricing mechanism | same document | same | Operator-stated | "RELATED CONTINGENCIES: Accumulative/multiple bets are not accepted where the outcome of one part of the bet contributes to the outcome of another. If taken in error, the stake will be invested on the selection with the largest price." |
| Caesars reserves the right to cancel correlated wagers placed across duplicate/mirrored listings of the same game (a distinct issue from ordinary same-game correlation) | same document | same | Operator-stated | "When multiple/duplicate versions of the same game are shown... the operator has the ability to cancel any correlated wagers placed on both/multiple events." |
| Caesars explicitly disallows cash-out on Odds Boost markets and separately states a void leg on a boosted multi-game or same-game parlay voids the whole boosted bet | same document | same | Operator-stated | "Cash out my bet is not available for odds boost markets" ... "If any leg of a boosted multi-game or same-game p[arlay is void, the entire boost market is void]" |
| A secondary source's characterization of Caesars' plain (non-boosted) SGP void rule as full-void, filling a gap this dispatch's own PDF extraction did not conclusively resolve (see Gaps) | Sportshandle | https://sportshandle.com/same-game-parlay/ | Journalism | "Caesars: 'If any leg of the Same-Game Parlay bet is made void or settles as a push, then the whole bet would become a void or a push.'" |
| FanDuel's own house rules apply a duplicate/mirrored-listing correlation rule matching Caesars' | FanDuel Sportsbook House Rules — Ohio, effective 22 Jul 2026 | https://www.fanduel.com/fanduel-sportsbook-house-rules-oh | Operator-stated | "In the event that odds for the same exact game are displayed on the FanDuel Sportsbook more than once (regardless of whether the markets are related and/or displayed odds are different), FanDuel Sportsbook reserves the right to cancel any correlated..." |
| FanDuel's NFL player-prop void trigger is narrowly defined (zero snaps played), which matters for how often an SGP leg actually voids at all | same document | same | Operator-stated | "For player prop markets, only when a player does not play a snap in that game are the selections voided." |
| FanDuel's house rules contain **no separately named "Same Game Parlay" void-exception clause** — parlay payout is described by one general rule ("Odds will be calculated based on the prices of the individual selections"), consistent with the journalism claim that FanDuel folds SGP into ordinary recalculate-on-void parlay handling rather than carving it out (contrast with DraftKings/Caesars, which explicitly carve SGP *out*) | same document (structural absence) + Sportshandle | https://www.fanduel.com/fanduel-sportsbook-house-rules-oh ; https://sportshandle.com/same-game-parlay/ | Operator-stated (structure) / Journalism (interpretation) | Sportshandle: "In FanDuel's case, if a leg is voided, the odds are simply recalculated — like they would be for any other parlay." |
| theScore Bet (regulated Canadian/US operator) states its SGP explicitly recalculates on remaining legs when a leg pushes or voids | theScore Bet Help Center | https://thescorebethelp.zendesk.com/hc/en-us/articles/27763403068045-Same-Game-Parlay | Operator-stated | "If any selection(s) within a pre-game Same Game Parlay are a push or void, and all remaining selection(s) within that pre-game Same Game Parlay are a win... the wager will be graded as a win with re-calculated odds and payout reflecting the remaining legs that are not a push or void." |
| theScore Bet explicitly and unconditionally excludes SGP from cash-out | same document | same | Operator-stated | "Same Game Parlay bets will not be subject to cash out." |
| bet365's official company account stated (2021) that a void leg voids the **entire** Bet Builder, with player-statistics markets voiding if the player doesn't start | bet365 official account, reply on X (formerly Twitter), 2021 | https://x.com/bet365/status/1409944293230497795 | Operator-stated (but dated — see next row and Gaps) | "For Player Statistics markets specifically, bets will be void if the player does not start the match. When you have a void selection in a Bet Builder the whole Bet Builder will be made void." |
| **Discrepancy flagged**: current secondary betting-guide sources describe the opposite for soccer player markets specifically — void-and-recalculate, not full void. bet365's own current help-center pages returned HTTP 403 to automated fetch and could not be checked directly, so this is not resolved to a current primary source either way | footyaccumulators.com (secondary, undated but recent) | https://footyaccumulators.com/bet365/bet-builder | Journalism (secondary, unverified against current primary) | "if your specified player does not start the match then selections will be made void and the odds of the Bet Builder will be recalculated for the remaining selections" |
| A regulator-confirmed real-world case shows operators *do* build automated safeguards specifically to block parlaying correlated/nested outcomes from the same player-market — and shows what happens when that safeguard fails | NY Post via AOL, Ariel Zilber, 19 Dec 2025, citing Bookies.com and the Massachusetts Gaming Commission's public vote | https://www.aol.com/articles/massachusetts-orders-draftkings-pay-934k-175522307.html | Journalism (regulatory vote is public record) | "Because of a misclassification inside DraftKings' trading tools, [the player] was incorrectly labeled a 'non-participant' rather than an active player. That designation disabled safeguards designed to block bettors from parlaying correlated outcomes from the same market." A Massachusetts bettor placed "27 multi-leg parlays" on stacked hit-total thresholds for one player; "The Massachusetts Gaming Commission voted 5-0... to reject DraftKings' bid to void $934,137 in payouts." |
| **— Q4: Correlation exploits —** | | | | |
| Negative correlation is the theoretically more exploitable side (books may apply a flat correlation discount that doesn't distinguish positive from negative), but is harder to execute in practice and books are closing the gap | OddsIndex guide | https://oddsindex.com/guides/same-game-parlay-correlation | Practitioner-inference | "Negative correlation can be attractive because sportsbooks sometimes fail to fully adjust their pricing. If a book applies a standard correlation discount to all same game parlays, negatively correlated legs might actually be underpriced relative to their true probability." ... "Books are learning: Sportsbooks are increasingly sophisticated about pricing negative correlation, and they may offer lower payouts than the math suggests." |
| Respected professional-gambling author's standing advice: parlays (correlated ones specifically) are the *only* parlay type worth considering, precisely because of the correlation edge | Stanford Wong, *Sharp Sports Betting*, excerpted at bj21.com, 20 Jun 2023 | https://bj21.com/articles/sports-betting/correlated-parlays | Practitioner-inference (published book author) | "A time to consider betting a parlay is when you can take advantage of correlation." ... "Forget about parlays unless you can find bets that are correlated or a parlay card with selections you think are strong." |
| A rival odds vendor's stated rationale for why correlation pricing breaks down: in-game volatility (injuries, tactical changes, weather) outruns static correlation models, and cross-state regulatory speed limits compound the lag | OpticOdds blog (commercially motivated — this is OpticOdds explaining why its own tool is needed) | https://opticodds.com/blog/correlation-in-same-game-parlays | Vendor-stated | "Situations can change instantly—a key player gets injured, coaches make strategic adjustments mid-game, or weather conditions shift unexpectedly... Conventional models often aren't built to handle such rapid changes or unusual scenarios." |
| A plain-language example of a *logically redundant* (not merely correlated) combination that books block outright: spread and moneyline on the same side of the same game, because covering the spread logically implies winning the moneyline | rg.org guide to BetMGM parlays | https://rg.org/guides/betmgm/betmgm-parlay-rules | Journalism/practitioner-inference | "if the Bills are favored by 6 points in their game against the Jets, you can't back them on the spread and moneyline on two different bets with a same-game parlay. That's because if they cover the spread, they have, by definition, won on the moneyline, so a sportsbook will not give you extra credit..." |
| The DraftKings Massachusetts case (above) doubles as the clearest documented instance of a correlation-adjacent exploit in the wild — though it was a software misclassification bug disabling an existing safeguard, not a genuine model blind spot on true game correlation | same as Q3 case above | https://www.aol.com/articles/massachusetts-orders-draftkings-pay-934k-175522307.html | Journalism | See full quote above. |

## Q1 — Pricing mechanism

**What vendors say about their own machinery.** Two odds-technology vendors publish enough to say
something concrete. Sportradar's CustomBet/Bet Builder developer documentation is the most precise
public source found: it states plainly that correlation is assessed from simulation data, sorted into
three types (independent / positive / negative), and that the *combined probability* is computed as
`Correlation factor × P1 × P2 × P3 × ...` — i.e. marginal-probability multiplication adjusted by a
single scalar correlation factor per combination, not a full joint distribution returned per bet slip.
Margin is then layered on top of the resulting fair odds. This is a **correlation-factor-adjustment
on top of marginal prices**, generated using simulation as the source of the correlation factors
themselves — closer to "simulate offline to build a correlation table, then apply that table live" than
"simulate the whole match live for every bet slip." Genius Sports' public material is vaguer but
points the same direction: it describes its MultiBet engine as driven by "millions of simulations"
where joint probabilities emerge from simulation frequency counts. Neither vendor discloses the
underlying statistical model (copula family, factor model, or literal Monte Carlo path count) in
technical depth — both stop at marketing-blog level detail.

**What a practitioner reconstructs mathematically.** Wizard of Odds (Michael Shackleford's site)
publishes the most rigorous public treatment found, explicitly labeled as an educational
reconstruction, not disclosed operator methodology: a Gaussian-copula transform of each leg's marginal
probability into a latent normal variable, joined via a correlation matrix, with the joint tail
probability evaluated by Monte Carlo or numerical integration. The same article separately walks an
"empirical frequency" method (bucket historical games by matchup profile, count joint hit rate
directly) and concludes real books likely run a **hybrid**: empirical frequencies where volume of
historical data supports it, model-based smoothing (copula or otherwise) to fill gaps. This is
consistent with, but not confirmation of, what Sportradar and Genius Sports describe — convergent
inference, not verification.

**Vendor landscape.** Three vendors showed up with published SGP/bet-builder products: Genius Sports
(Monte Carlo-based MultiBet, plus a separate "Edge" pricing-optimization tool per
[Sportshandle's coverage](https://sportshandle.com/genius-sports-launches-edge-pricing-tool/)),
Sportradar (CustomBet, plus the unrelated Alpha Odds risk-recalculation product), and OpticOdds
(AlgoOdds, built on blended consensus lines rather than disclosed simulation). **Which vendor powers
which named US operator's actual production SGP engine (DraftKings, FanDuel, BetMGM, Caesars) is not
publicly confirmed by any source found** — see Gaps. Multiple secondary sources assert FanDuel built
its original (2019) SGP correlation modeling in-house rather than licensing it, which would make
FanDuel's engine unrelated to any of the three vendors above, but this is not confirmed by FanDuel
itself.

## Q2 — Margin / hold

No operator, and no regulator, publishes a same-game-parlay-specific hold or margin figure. Every
number found is a secondary estimate, and the best of the sources (Establish The Run) says this
outright: "Books aren't required to report numbers for SGPs specifically." State regulatory filings
(e.g., New Jersey's Division of Gaming Enforcement monthly reports) break revenue out by broad wager
category — "parlay" as a bucket that mixes ordinary cross-game parlays with same-game parlays — not by
SGP alone.

Within that constraint, the figures in circulation converge directionally but vary numerically:

- **4–6x** straight-bet hold, **50%** higher than ordinary parlay hold (juicereel.beehiiv.com,
  practitioner-inference).
- **3–5x** straight-bet hold (OddsIndex, practitioner-inference, sourcing described only as "public
  data and earnings reports").
- **20–30%** hold range cited for parlays generally, with SGP "at least that high, if not far
  higher" (Establish The Run, explicitly hedged as an assumption).
- A specific, checkable claim: New Jersey, September 2024, ~24.2% calculated hold on $352.7M parlay
  handle vs ~4.6% on single-event handle (ysnlive.com, attributed to unnamed "industry trackers,"
  **not independently re-verified against the underlying NJ filing by this dispatch**).
- **~15–20%** built-in hold once a parlay reaches five legs, and **>35%** effective edge on a 5-leg,
  $20 parlay vs 4.5% on the same stake straight (igamingnews.biz, no primary citation given).

Treat all of the above as the same order-of-magnitude claim from different, non-authoritative angles
(SGP hold is a low-single-digit multiple of straight-bet hold, likely landing somewhere in the
high-teens-to-30%+ range depending on leg count and market mix) rather than as five independent
confirmations of a precise number. One source (tech-insider.org) claims operators disclose "20-30%
realized parlay hold" on investor calls; this dispatch did not trace that claim to an actual earnings
call or 10-K, and the site's own content quality (generic byline, formulaic structure typical of
programmatic content) is reason for added caution — flagged low-confidence rather than dropped.

Wizard of Odds' worked example is useful for *mechanism*, not for a real-world number: correlating
three legs pushed a hypothetical fair joint probability from 16.0% (independence) to 21.2%
(Gaussian-copula-adjusted) — a 33% relative increase — which is the size of correlation adjustment a
book would need to apply just to stay fair, before adding any margin on top. This is illustrative math
from a practitioner's own constructed example, not a measured figure from any real sportsbook.

## Q3 — Settlement, void, and restriction rules

This section leans on primary sources wherever they could be found: three sets of official house
rules filed with the Massachusetts Gaming Commission (DraftKings 8/1/24, BetMGM 3/28/25, Caesars
3/8/23 — dates matter, see Gaps), FanDuel's Ohio house rules page (effective 7/22/26, very current),
and theScore Bet's own help center. Secondary sources fill specific, flagged gaps.

### Void handling — the question the dispatcher cares about most

**There is no single industry rule.** Books split into two camps, and at least one book (DraftKings)
straddles both depending on sport and leg type:

- **Recalculate-on-remaining-legs is the current, primary-sourced, operator-stated default at
  BetMGM, theScore Bet, and (by structural inference — no separate SGP exception clause exists in
  its house rules) FanDuel.** BetMGM's 2025 Massachusetts filing states a cancelled SGP pick causes
  the "wager odds at the time of bet placement" to be "re-calculated using the remaining legs,"
  falling back to full cancellation only if BetMGM "is unable to adjust the odds." theScore Bet's
  help center states the same outcome in plainer language. FanDuel's house rules never define a
  separate SGP void rule at all — parlay payout is one general rule — and Sportshandle's reporting
  matches that structure ("the odds are simply recalculated — like they would be for any other
  parlay").
- **Full-ticket void is DraftKings' default, with one narrow, sport-specific exception.**
  DraftKings' house rules explicitly exclude Same Game Parlay from the general "parlay survives a
  void" rule. The only documented repricing case is soccer, pre-live, and only for a
  "Non-Participating Player" leg — and even then, if *any other* leg on that same ticket also voids
  or pushes, the whole thing voids anyway "irrespective of whether the pre-live Same Game Parlay
  contains other winning or losing selections." A 2021 bettor complaint (8 of 9 legs won, ticket
  voided in full because the 9th leg's player was inactive) is consistent with full-void being the
  norm outside that narrow carve-out.
- **Caesars excludes SGP from its ordinary "reduce and continue" accumulator rule** ("this does not
  apply to Same Game Parlay") but this dispatch's own extraction of the primary 2023 PDF did not
  turn up the specific downstream sentence stating what *does* happen. A secondary source
  (Sportshandle) fills that gap with a full-void quote attributed to Caesars' rules, not
  independently re-verified against the primary document in this pass, and the Caesars filing used
  is roughly two years older than the DraftKings/BetMGM filings, so it may not reflect current terms.
- **bet365 is a genuine, unresolved source conflict.** bet365's own official account stated in 2021
  that any void selection voids the entire Bet Builder (with player-stat markets voiding if the
  player doesn't start). Current secondary betting-guide content describes the opposite for soccer
  player markets specifically — void-and-recalculate. bet365's current help-center pages returned
  HTTP 403 to every automated fetch attempt in this dispatch, so the current primary rule could not
  be checked directly. This is recorded as a gap, not resolved by picking a side.

The practical takeaway for a correlated-pricing design: **"drop the leg, odds go to 1.0" is not what
any of these books do, and is not what the one disclosed vendor mechanism (Sportradar's Void
Recalculation API) does either.** Sportradar's documented approach is to precompute, at the moment the
bet is placed, the correlation-adjusted price for every possible single-void outcome, then use that
precomputed price if a void actually happens later — a genuine re-price on the surviving legs'
*correlated* joint distribution, not a mechanical strip-and-multiply. Its one disclosed limitation is
that it only handles a single void per ticket; multiple simultaneous voids are out of scope for that
precomputed path. Whether any of the four named US operators actually uses this exact Sportradar
mechanism is not confirmed by any source found (see Gaps) — but it is the only place in this research
where the *mechanism* of correlated re-pricing on void, as opposed to just the *policy outcome*, is
documented anywhere.

### Restricted / blocked combinations

No operator publishes an itemized list of blocked SGP leg pairs. All rules found are general,
discretionary principles enforced at bet-slip construction time (the UI simply refuses to let you
build the combination) rather than a published rulebook:

- **Logically redundant combinations** (e.g., a team's spread and moneyline on the same side, where
  covering the spread necessarily means winning the moneyline) are blocked as a matter of definition,
  not correlation policy — confirmed by a practitioner guide's plain-language BetMGM example.
- **Ordinary (non-SGP) parlays** are where the hard "no correlated legs" rule lives at both
  DraftKings and Caesars — DraftKings "reserves the right... to declare void all parts... which
  include the correlated outcomes"; Caesars simply does not accept such combinations and reinvests
  the stake on the largest-priced leg if one slips through. Both operators carve SGP itself **out**
  of this ban, since offering correlated legs together, priced for the correlation, is the entire
  point of the product.
- **Duplicate/mirrored event listings** are a distinct, separately-named restriction at both Caesars
  and FanDuel: when the same real-world game is listed more than once (e.g., under different market
  groupings), both operators reserve the right to cancel correlated wagers spanning the duplicate
  listings. The DraftKings Massachusetts case below is the real-world failure mode this rule exists
  to prevent.
- DraftKings' own regulatory filing in the Massachusetts case confirms operators build **automated,
  market-level safeguards** specifically to block parlaying nested/correlated thresholds from the
  same player-market (e.g., stacking "3+ hits" and "5+ hits" for the same player in the same series).
  A configuration bug mislabeled the player a "non-participant," which disabled that safeguard and
  let a bettor build 27 stacked parlays before DraftKings caught it. The Massachusetts Gaming
  Commission voted 5–0 to force payment of the $934,137 in winnings, rejecting DraftKings' argument
  that the bettor acted unethically by exploiting an "obvious error."

### Cash-out on SGP

Mixed and book-specific, with the clearest primary-sourced answer at the small-book end:

- **theScore Bet: explicit, unconditional, operator-stated exclusion** — "Same Game Parlay bets will
  not be subject to cash out."
- **DraftKings: no SGP-specific exclusion located.** The general Cash Out clause covers "single and
  parlay bets" with no carve-out for SGP found in this dispatch's extraction of the primary document
  — but absence of an exclusion in a large PDF is weak evidence of inclusion (see Gaps).
  Secondary/aggregator sources claim cash-out unavailability varies by sport at DraftKings (e.g.,
  unavailable for tennis SGPs specifically), consistent with a sport-by-sport rather than blanket
  policy.
- **Caesars: cash-out is explicitly excluded for Odds Boost markets**, confirmed in the primary
  document. A plain (non-boosted) SGP cash-out rule was not conclusively located in this dispatch's
  extraction; secondary sources (not independently verified here) describe Caesars SGP as generally
  ineligible for cash-out because the bet is "closed as soon as the game ends."
  Same caveat applies to **BetMGM** — secondary sources describe SGP cash-out as unavailable, the
  primary 2025 filing's cash-out language covers error-correction and static-handicap cash-out
  mechanics but this dispatch did not locate an explicit SGP exclusion sentence in a 423,816-character
  document.

### Partial / progressive settlement

No evidence that ordinary SGPs settle progressively leg-by-leg mid-event. What exists instead is a
**separately named, different product**: DraftKings' house rules define "Progressive Parlay" as its
own distinct wager type in the same definitions section that defines Same Game Parlay — implying
progressive/partial payout is a deliberately different product a bettor opts into, not a behavior of
standard SGPs. Live "my bets" screens that show which legs have already hit are a UI status display,
not a settlement or payout event, and shouldn't be conflated with partial settlement.

## Q4 — Correlation exploits

Every source in this section is practitioner-inference or vendor-commercial framing — there is no
academic or regulator-audited study of SGP mispricing in the sources found. With that ceiling stated:

- **Positive correlation is the well-covered case.** Every pricing description found (vendor and
  practitioner alike) explicitly names positive same-game correlation — QB yards with WR yards, a
  large spread with the game total, a heavy-run game script with the opposing QB's yardage going
  under — as the thing correlation engines exist to price down. This is treated as the solved,
  expected part of the problem across every source.
- **Negative correlation is repeatedly named as the softer spot**, on the theory that a book applying
  one flat correlation discount to "any SGP" rather than distinguishing the sign of the correlation
  will overpay on negatively correlated combinations. Every source raising this also immediately
  hedges it: negative correlation is harder to construct a narrative around (so less bet on, so less
  data pressure on the book to fix it), harder to hit (higher variance), and — per OddsIndex —
  "books are learning," i.e. this edge is described as closing over time, not a standing exploit.
  No source quantified how much edge, if any, currently remains.
  **Practitioner-inference throughout — not one operator or vendor source in this research
  acknowledges mispricing negative correlation.**
- **Stanford Wong's standing advice** (a respected published author in this space, not a marketing
  source) is blunt: don't bet parlays at all unless you have a specific correlation read, which is
  effectively an endorsement of the "positive correlation you're confident in, not offered by the
  book's UI as a single market" pattern as the only parlay angle worth pursuing.
  **Practitioner-inference.**
- **The stated reason correlation pricing breaks in-play** (from OpticOdds, a vendor with a
  commercial interest in this exact pitch): static correlation tables lag real events — injuries,
  tactical substitutions, weather — faster than models re-fit, and multi-state regulatory approval
  timelines slow down how fast odds can move in response. This points at **live/in-play SGP legs
  around a just-occurred event** (e.g., building a bet builder in the seconds after a red card or a
  key injury) as a plausible window where pricing lags reality, though no source measured this
  directly. **Vendor-stated, commercially motivated — treat as a hypothesis, not a finding.**
- **The DraftKings Massachusetts case is the one instance in this research of a documented,
  regulator-confirmed, dollar-quantified correlation-adjacent exploit** — 27 parlays stacking nested
  hit-count thresholds for one player, cashing for a combined $934,137 before DraftKings caught it.
  Important caveat for game design purposes: this was a **software misclassification bug disabling an
  existing anti-correlation safeguard**, not evidence that DraftKings' correlation *model* itself
  misprices nested same-market thresholds under normal operation — the rule the bug defeated was
  specifically built to prevent exactly this stack. It's better evidence for "safeguards fail
  silently and expensively when a data-labeling edge case slips through" than for "the pricing model
  has a blind spot." **Journalism, but resting on a public regulatory vote.**

## Gaps — not publicly documented

1. **Which vendor powers which named operator's SGP engine.** No source confirms whether
   DraftKings, FanDuel, BetMGM, or Caesars runs Sportradar's CustomBet, Genius Sports' MultiBet, an
   in-house engine, or some blend, for their actual production same-game-parlay pricing. Multiple
   secondary sources assert FanDuel built its own in-house in 2019; none of the big four confirm a
   vendor relationship for SGP specifically in any source found.
2. **The actual statistical model in production anywhere.** Vendors describe "simulation" and
   "correlation factor" at marketing-blog depth; no vendor or operator has published the underlying
   model (copula family, factor model, literal per-bet Monte Carlo vs. precomputed correlation
   table) at technical/reproducible depth. The Gaussian-copula treatment in this document is an
   independent practitioner's (Wizard of Odds) educational reconstruction, explicitly labeled as
   such by its own author — not a disclosed operator or vendor method.
3. **bet365's current (2026) Bet Builder void rule.** Official help-center pages returned HTTP 403 to
   every automated fetch attempt. Only a 2021 company statement (primary but stale) and unverified,
   undated secondary summaries (claiming the opposite rule for soccer specifically) exist in this
   research. Genuinely unresolved, not adjudicated either direction.
4. **An operator-published, itemized list of blocked SGP combinations.** Every operator's rule is a
   general discretionary principle ("reserves the right to declare void," "not accepted where...")
   enforced by bet-slip UI logic, not a public rulebook naming specific blocked leg pairs.
5. **A same-game-parlay-specific hold/margin figure from any operator, regulator, or auditor.** Every
   number in Q2 is a secondary-source estimate inferred from blended "parlay" category revenue in
   state filings (which mix SGP with ordinary cross-game parlays). No source breaks out SGP alone.
6. **Dead-heat interaction with SGP correlation repricing.** House rules document dead-heat
   mechanics (divide stake across tied outcomes) and SGP mechanics as separate sections; no source
   addresses how a dead-heat leg inside an SGP interacts with the correlation-adjusted repricing of
   the rest of the ticket.
7. **Whether DraftKings or FanDuel offer any SGP-specific partial/progressive settlement**, as
   opposed to the blanket "no cash-out" confirmed at theScore Bet or the separately-named
   "Progressive Parlay" product at DraftKings. Not confirmed either way.
8. **Caesars' and BetMGM's exact, current, verbatim plain-SGP cash-out and void-settlement clauses.**
   This dispatch's own extraction of the primary PDFs (via a custom text-stream parser, not a
   commercial PDF-to-text tool — see methodology note below) confirmed the *existence* of SGP
   carve-outs from general rules but did not conclusively locate every downstream sentence in
   documents running 400,000–1,400,000 extracted characters. Secondary sources fill some of these
   gaps but are flagged, not treated as confirmed. The Caesars filing used (3/8/2023) is also roughly
   two years older than the DraftKings and BetMGM filings used, so it may not reflect current rules.
9. **Any independent, statistically rigorous audit of a real book's SGP pricing** (as opposed to a
   bettor forum anecdote, a single illustrative worked example, or a content site's estimate). None
   found. Every mispricing claim in Q4 is inference or hypothesis, not measurement.
10. **The exact current source and methodology behind the widely repeated "20-30%" and "24.2%" parlay
    hold figures.** These trace back to state regulatory revenue filings (e.g., NJ DGE) via secondary
    aggregation ("industry trackers"); this dispatch did not pull the underlying regulator filings
    directly to re-derive the figures.

**Methodology note:** the three Massachusetts house-rules PDFs (DraftKings, BetMGM, Caesars) were not
renderable by this dispatch's standard fetch tool (it returned raw compressed PDF stream bytes, not
text). Quotes from those three documents were recovered with a purpose-built script (zlib-inflate each
PDF content stream, extract parenthesized text-show operands, decode standard PDF octal escapes for
curly quotes/dashes). This is a reliable extraction method for the quotes reported above — each was
sanity-checked for surrounding context — but is not a substitute for a proper PDF text layer, and a
keyword not found by this method is not proof the underlying document doesn't contain it (see gaps 4,
8). No file other than this one was written or modified in producing this research.
