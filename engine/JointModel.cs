using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

namespace SBR.Engine;

/// <summary>Which of the match model's three independent draws a selection reads. The joint
/// factorizes across these — corners and cards are drawn without reference to the score or to each
/// other (<c>MatchModel.SampleStatLine</c>), measured at max |rho - 1| = 4.4e-14 over 3.94M pairs
/// (<c>docs/sgp/correlation-recon.md</c> §2.1).</summary>
public enum SelectionFamily
{
    Goal,
    Corner,
    Card,
}

/// <summary>The S73 relation vocabulary (<c>design/02-betting-math.md</c> § *Same-game tickets*).
/// Structured data only: presentation composes the sentence, the model never emits English.
///
/// <para><b>SharedCount is an extension to the canon table, flagged not smuggled.</b> The design
/// doc gives <see cref="SharedScoreline"/> for "two GOAL-family legs" and offers no relation for two
/// legs of the same COUNT family. With three corner lines and three card lines on the shipped board
/// that leaves six real pair shapes unlabelable (e.g. corners OVER 8.5 + UNDER 10.5: a band, neither
/// impossible nor an implication, rho far from 1). Under the doc's own no-label fallback those would
/// price at the naive product — the money leak S73 exists to prevent. This member is the minimal
/// structural parallel to SharedScoreline; the Design Director owns whether it keeps this name.</para></summary>
public enum RelationKind
{
    /// <summary>p_joint = 0 — these cannot both happen. Never subject to the no-label fallback:
    /// the design doc's load-bearing carve-out makes the zero check a validity test, not a price
    /// movement.</summary>
    MutuallyExclusive,

    /// <summary>One leg strictly entails another: p_joint = min p_i. <c>Legs[0]</c> implies
    /// <c>Legs[1]</c>.</summary>
    Implies,

    /// <summary>Two GOAL-family legs read the same scoreline.</summary>
    SharedScoreline,

    /// <summary>A scorer leg beside a leg on that team's goals — the same goals settle both.
    /// <see cref="Relation.ScorerSide"/> carries which side.</summary>
    ScorerOfSide,

    /// <summary>EXTENSION (see the enum's own remarks): two legs of the same COUNT family read the
    /// same corner or card draw. <see cref="Relation.Family"/> carries which.</summary>
    SharedCount,

    /// <summary>Legs drawn from different families — unrelated, no adjustment.</summary>
    Independent,
}

/// <summary>Direction of a shared-draw relation, from whether the pair's joint exceeds the product
/// of its marginals. <see cref="None"/> is the exact-tie case; it does not occur on the shipped
/// board and exists so a tie can never be silently reported as a direction it does not have.</summary>
public enum RelationSign
{
    None,
    Reinforcing,
    Opposing,
}

/// <summary>One labelled structural cause of correlation inside a ticket. Carries indices into the
/// caller's selection list plus the structured parts a surface needs to compose a sentence. There is
/// deliberately no display text and no <c>ToString</c> override on this type.</summary>
public readonly struct Relation : IEquatable<Relation>
{
    public RelationKind Kind { get; }

    /// <summary>Indices into the selection list this relation was computed over. Ordered where
    /// order carries meaning: for <see cref="RelationKind.Implies"/>, <c>Legs[0]</c> implies
    /// <c>Legs[1]</c>. A ticket-level <see cref="RelationKind.MutuallyExclusive"/> spans every leg.</summary>
    public IReadOnlyList<int> Legs { get; }

    public RelationSign Sign { get; }

    /// <summary>The family the relation lives in; null for a cross-family
    /// <see cref="RelationKind.Independent"/> or a ticket-level exclusion spanning families.</summary>
    public SelectionFamily? Family { get; }

    /// <summary>Set only on <see cref="RelationKind.ScorerOfSide"/>.</summary>
    public Side? ScorerSide { get; }

    /// <summary>|ln rho_pair| for this pair — how much work the relation does on the price, and the
    /// ranking key behind a ticket's principal relation (<c>design/02-betting-math.md</c>, batch 48:
    /// the slip states the relation ONCE, so the model nominates which one). Zero for a ticket-level
    /// <see cref="RelationKind.MutuallyExclusive"/>, which spans every leg rather than a pair, and
    /// zero for a pairwise exclusion, whose rho is 0 — an exclusion is a validity verdict, not a
    /// price movement, and is never eligible to be principal.
    ///
    /// <para><b>Diagnostic, not identity.</b> Deliberately excluded from
    /// <see cref="Equals(Relation)"/> and <see cref="GetHashCode"/>: a relation IS its structure,
    /// and two structurally identical relations must compare equal whatever arithmetic produced
    /// them.</para></summary>
    public double Strength { get; }

    /// <summary>True when the model measured correlation on this pair, i.e. |rho - 1| beyond
    /// <see cref="JointModel.CorrelationTolerance"/>. An <see cref="RelationKind.Independent"/>
    /// relation carrying this flag is correlation the vocabulary could not label — the sole trigger
    /// of the ticket-wide no-label fallback, and the money leak that fallback exists to bound.
    /// Excluded from equality for the same reason as <see cref="Strength"/>.</summary>
    public bool Correlated { get; }

    public Relation(RelationKind kind, IReadOnlyList<int> legs, RelationSign sign,
        SelectionFamily? family, Side? scorerSide, double strength = 0.0, bool correlated = false)
    {
        Kind = kind;
        Legs = legs ?? throw new ArgumentNullException(nameof(legs));
        Sign = sign;
        Family = family;
        ScorerSide = scorerSide;
        Strength = strength;
        Correlated = correlated;
    }

    public bool Equals(Relation other)
    {
        if (Kind != other.Kind || Sign != other.Sign || Family != other.Family
            || ScorerSide != other.ScorerSide || Legs.Count != other.Legs.Count)
            return false;
        for (int i = 0; i < Legs.Count; i++)
            if (Legs[i] != other.Legs[i]) return false;
        return true;
    }

    public override bool Equals(object? obj) => obj is Relation other && Equals(other);

    public override int GetHashCode()
    {
        int hash = HashCode.Combine((int)Kind, (int)Sign, Family, ScorerSide, Legs.Count);
        for (int i = 0; i < Legs.Count; i++) hash = HashCode.Combine(hash, Legs[i]);
        return hash;
    }

    public static bool operator ==(Relation left, Relation right) => left.Equals(right);
    public static bool operator !=(Relation left, Relation right) => !left.Equals(right);
}

/// <summary>
/// The exact joint probability of a set of selections on ONE matchup, plus its S73 relation labels.
///
/// <para>The sim's match model is a finite joint distribution, so this is enumerated, not modelled —
/// no copula, no latent factor, no simulation (<c>design/02-betting-math.md</c> § *Same-game
/// tickets — the correlation model*).</para>
///
/// <para>Legs on DIFFERENT matchups are independent; grouping them and multiplying the groups'
/// joints is the caller's job (plan F_0.6.0 Phase 2).</para>
///
/// <para><b>Output shape is binding.</b> S73 prohibits collapsing this to a bare scalar: a price
/// that cannot be explained is a price presentation cannot compose a sentence about. The scalar
/// core is private for that reason — <see cref="JointProbability"/> is the only entry point.</para>
/// </summary>
public static class JointModel
{
    /// <summary>Absolute tolerance for detecting a logical implication (p_joint = min p_i). Matches
    /// the reconnaissance sweep's threshold, which found 22 implication shapes holding in 100% of
    /// 12,162 matchups with max |rho - 1/p| = 4.0e-14 — three orders of margin under this value.</summary>
    public const double ImplicationTolerance = 1e-12;

    /// <summary>Relative tolerance for "this ticket is correlated at all", i.e. p_joint differs from
    /// the product of its marginals. Loose enough that a genuinely independent cross-family pair
    /// (which lands within ~1e-16 relative) is never called correlated.</summary>
    public const double CorrelationTolerance = 1e-9;

    /// <summary>Exact joint probability of <paramref name="selections"/> on
    /// <paramref name="matchup"/>, together with the structural relations that explain it and the
    /// one relation a slip would state.</summary>
    /// <returns><c>pJoint</c> — the enumerated joint, exactly 0.0 for an impossible ticket;
    /// <c>relations</c>, one label per selection pair plus, when the whole ticket is impossible
    /// without any impossible pair, a ticket-level <see cref="RelationKind.MutuallyExclusive"/>;
    /// and <c>principal</c>, the single relation the slip states — see
    /// <see cref="SelectPrincipal"/>. <c>principal</c> is null when nothing is statable.</returns>
    public static (double pJoint, IReadOnlyList<Relation> relations, Relation? principal) JointProbability(
        Matchup matchup, IReadOnlyList<MarketSelection> selections)
    {
        if (matchup == null) throw new ArgumentNullException(nameof(matchup));
        if (selections == null) throw new ArgumentNullException(nameof(selections));
        if (selections.Count == 0)
            throw new ArgumentException("A ticket needs at least one leg", nameof(selections));

        double pJoint = Probability(matchup, selections);
        var relations = new List<Relation>();
        if (selections.Count == 1) return (pJoint, relations, null);

        var marginals = new double[selections.Count];
        for (int i = 0; i < selections.Count; i++)
            marginals[i] = Probability(matchup, new[] { selections[i] });

        bool sawExclusion = false;
        for (int i = 0; i < selections.Count; i++)
            for (int j = i + 1; j < selections.Count; j++)
            {
                // A two-leg ticket's pair joint IS the ticket joint — do not recompute it.
                double pair = selections.Count == 2
                    ? pJoint
                    : Probability(matchup, new[] { selections[i], selections[j] });
                Relation relation = Classify(matchup, selections, i, j, pair, marginals[i], marginals[j]);
                if (relation.Kind == RelationKind.MutuallyExclusive) sawExclusion = true;
                relations.Add(relation);
            }

        // The 57 impossible triple shapes that contain no impossible PAIR (recon §6.2) reach zero
        // only jointly — three legs whose pairwise joints are all positive. Without this the ticket
        // would carry no exclusion label at all, and the design's validity carve-out would have
        // nothing to fire on.
        if (pJoint == 0.0 && !sawExclusion)
        {
            var all = new int[selections.Count];
            for (int i = 0; i < all.Length; i++) all[i] = i;
            relations.Add(new Relation(RelationKind.MutuallyExclusive, all, RelationSign.None, null, null,
                strength: 0.0, correlated: true));
        }

        return (pJoint, relations, SelectPrincipal(relations));
    }

    /// <summary>
    /// The one relation a slip states (<c>design/02-betting-math.md</c>, batch 48: "a slip states one
    /// relation, so the model must nominate which one"). It is the labelable relation carrying the
    /// largest <see cref="Relation.Strength"/> — the one doing the most work on the price — with ties
    /// broken by the precedence <c>Implies &gt; ScorerOfSide &gt; SharedScoreline &gt; SharedCount</c>
    /// and, beyond that, by pair order, so the choice is deterministic.
    ///
    /// <para>Two kinds are never principal. <see cref="RelationKind.Independent"/> has nothing to
    /// state, and <see cref="RelationKind.MutuallyExclusive"/> never reaches a slip at all — it is a
    /// rejection rather than a price. An exclusion is still returned in <c>relations</c>, because the
    /// caller must be able to reach it for refusal handling.</para>
    ///
    /// <para>Public because a ticket spans several matchups: the ticket layer re-ranks the union of
    /// every group's relations through this same function rather than re-deriving the rule.</para>
    /// </summary>
    public static Relation? SelectPrincipal(IReadOnlyList<Relation> relations)
    {
        if (relations == null) throw new ArgumentNullException(nameof(relations));

        Relation? best = null;
        double bestStrength = 0.0;
        int bestPrecedence = int.MaxValue;
        for (int i = 0; i < relations.Count; i++)
        {
            Relation candidate = relations[i];
            int precedence = Precedence(candidate.Kind);
            if (precedence == NotPrincipal) continue;

            bool wins = best == null
                || candidate.Strength > bestStrength
                || (candidate.Strength == bestStrength && precedence < bestPrecedence);
            if (!wins) continue;

            best = candidate;
            bestStrength = candidate.Strength;
            bestPrecedence = precedence;
        }
        return best;
    }

    private const int NotPrincipal = int.MaxValue;

    /// <summary>Tie-break precedence, lowest wins. <see cref="NotPrincipal"/> marks the two kinds
    /// that are never eligible.</summary>
    private static int Precedence(RelationKind kind) => kind switch
    {
        RelationKind.Implies => 0,
        RelationKind.ScorerOfSide => 1,
        RelationKind.SharedScoreline => 2,
        RelationKind.SharedCount => 3,
        _ => NotPrincipal,
    };

    /// <summary>Which draw a selection reads. GOAL is the only family holding more than one market
    /// kind, which is why it is the only one needing the scoreline enumeration.</summary>
    public static SelectionFamily FamilyOf(MarketSelection selection) => selection.Kind switch
    {
        MarketKind.Moneyline => SelectionFamily.Goal,
        MarketKind.TotalGoals => SelectionFamily.Goal,
        MarketKind.BothTeamsToScore => SelectionFamily.Goal,
        MarketKind.AnytimeScorer => SelectionFamily.Goal,
        MarketKind.TotalCorners => SelectionFamily.Corner,
        MarketKind.TotalCards => SelectionFamily.Card,
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "Unfamilied market kind"),
    };

    // ---------------------------------------------------------------------------------------
    // The joint itself.  p_joint = p_GOAL * p_CORNER * p_CARD.
    // ---------------------------------------------------------------------------------------

    /// <summary>The scalar joint. Private by design (S73) — callers get it only alongside its
    /// relation labels, via <see cref="JointProbability"/>.</summary>
    private static double Probability(Matchup matchup, IReadOnlyList<MarketSelection> selections)
    {
        Split split = SplitFamilies(matchup, selections);

        // Multiplying by an untouched family's 1.0 is exact, so a single-family ticket is
        // bit-identical to the corresponding MatchModel sum rather than merely close to it.
        double p = 1.0;
        if (split.HasGoal) p *= GoalFamily(matchup, split);
        if (split.Corner.Count > 0)
            p *= CountFamily(matchup.Dist.HomeCornerRaw, matchup.Dist.HomeCornerTotal,
                matchup.Dist.AwayCornerRaw, matchup.Dist.AwayCornerTotal, split.Corner);
        if (split.Card.Count > 0)
            p *= CountFamily(matchup.Dist.HomeCardRaw, matchup.Dist.HomeCardTotal,
                matchup.Dist.AwayCardRaw, matchup.Dist.AwayCardTotal, split.Card);
        return p;
    }

    /// <summary>
    /// p_GOAL = SUM over w in W of P(w) * SUM over (h,a) of P(h,a|w) * 1[goal predicates] * PROD_t Q_t(g_t).
    ///
    /// <para>The outer sum runs over the model's outcome partition W, discovered from what the model
    /// exposes rather than written as a home/away pair — see <see cref="DiscoverPartition"/>. The
    /// inner sum mirrors <c>MatchModel.ScoreProbability</c>'s shape and order exactly.</para>
    /// </summary>
    private static double GoalFamily(Matchup matchup, Split split)
    {
        double[] weights = ClassWeights(matchup);
        MatchDistributions dist = matchup.Dist;

        double[]? home = split.HomeScorers.Count > 0
            ? NormalizedWeights(matchup, Side.Home, split.HomeScorers) : null;
        double[]? away = split.AwayScorers.Count > 0
            ? NormalizedWeights(matchup, Side.Away, split.AwayScorers) : null;

        double sum = 0.0;
        for (int c = 0; c < Partition.Length; c++)
        {
            double weight = weights[c];
            IReadOnlyList<MatchModel.ScoreOutcome> scores = Partition[c].Scores(dist);
            for (int s = 0; s < scores.Count; s++)
            {
                MatchModel.ScoreOutcome x = scores[s];
                if (!PredicatesHold(split.GoalPredicates, x.HomeGoals, x.AwayGoals)) continue;

                double q = 1.0;
                if (home != null)
                {
                    q *= ScorerTerm(home, x.HomeGoals);
                    if (q == 0.0) continue;
                }
                if (away != null) q *= ScorerTerm(away, x.AwayGoals);

                sum += weight * x.Probability * q;
            }
        }
        return sum;
    }

    /// <summary>
    /// Inclusion-exclusion over the k backed players on one team, against that team's g goals:
    /// Q_t(g) = SUM over S subset of {1..k} of (-1)^|S| * (1 - SUM_{i in S} w_i)^g, and 0 when g &lt; k.
    ///
    /// <para><b>The g &lt; k guard is normative, not an optimization</b> (design doc, verbatim). The
    /// sum cancels to ~1e-17 rather than 0 in IEEE double, which turns a structurally impossible
    /// ticket — two players both scoring inside one goal — into a vanishingly small POSITIVE
    /// probability that passes every zero check. Twelve impossible triple shapes were misclassified
    /// exactly this way before the guard existed.</para>
    /// </summary>
    private static double ScorerTerm(double[] weights, int goals)
    {
        int k = weights.Length;
        if (goals < k) return 0.0;
        if (k > 24) throw new ArgumentOutOfRangeException(nameof(weights), k,
            "Inclusion-exclusion is exponential in the backed-player count");

        double sum = 0.0;
        int subsets = 1 << k;
        for (int mask = 0; mask < subsets; mask++)
        {
            double excluded = 0.0;
            int bits = 0;
            for (int i = 0; i < k; i++)
                if ((mask & (1 << i)) != 0) { excluded += weights[i]; bits++; }

            // Roster-normalized weights sum to 1, so the remainder is non-negative in exact
            // arithmetic; the clamp only absorbs the ~1e-16 undershoot of a whole-roster subset.
            double rest = 1.0 - excluded;
            if (rest < 0.0) rest = 0.0;

            double term = Math.Pow(rest, goals);
            sum += (bits & 1) == 0 ? term : -term;
        }
        return sum;
    }

    /// <summary>p_COUNT = SUM_{c_h} SUM_{c_a} P(c_h) * P(c_a) * 1[every predicate in the family
    /// holds]. Mirrors <c>MatchModel.CountTotalProbability</c>'s loop order exactly.</summary>
    private static double CountFamily(double[] homeRaw, double homeTotal,
        double[] awayRaw, double awayTotal, List<MarketSelection> legs)
    {
        double p = 0.0;
        for (int h = 0; h < homeRaw.Length; h++)
            for (int a = 0; a < awayRaw.Length; a++)
                if (CountPredicatesHold(legs, h + a))
                    p += (homeRaw[h] / homeTotal) * (awayRaw[a] / awayTotal);
        return p;
    }

    // ---------------------------------------------------------------------------------------
    // Predicates.  These mirror MatchModel.Grades, so a ticket prices on exactly the outcomes
    // that would settle it.
    // ---------------------------------------------------------------------------------------

    private static bool PredicatesHold(List<MarketSelection> legs, int homeGoals, int awayGoals)
    {
        for (int i = 0; i < legs.Count; i++)
            if (!GoalPredicateHolds(legs[i], homeGoals, awayGoals)) return false;
        return true;
    }

    private static bool GoalPredicateHolds(MarketSelection selection, int homeGoals, int awayGoals)
    {
        switch (selection.Kind)
        {
            case MarketKind.Moneyline:
                RequireChoice(selection, MarketChoice.Home, MarketChoice.Away);
                return selection.Choice == MarketChoice.Home ? homeGoals > awayGoals : awayGoals > homeGoals;

            case MarketKind.TotalGoals:
                RequireChoice(selection, MarketChoice.Over, MarketChoice.Under);
                return selection.Choice == MarketChoice.Over
                    ? homeGoals + awayGoals > selection.Line
                    : homeGoals + awayGoals < selection.Line;

            case MarketKind.BothTeamsToScore:
                RequireChoice(selection, MarketChoice.Yes, MarketChoice.No);
                return (homeGoals >= 1 && awayGoals >= 1) == (selection.Choice == MarketChoice.Yes);

            default:
                throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind,
                    "Not a scoreline-predicate market");
        }
    }

    private static bool CountPredicatesHold(List<MarketSelection> legs, int total)
    {
        for (int i = 0; i < legs.Count; i++)
        {
            MarketSelection leg = legs[i];
            RequireChoice(leg, MarketChoice.Over, MarketChoice.Under);
            bool holds = leg.Choice == MarketChoice.Over ? total > leg.Line : total < leg.Line;
            if (!holds) return false;
        }
        return true;
    }

    private static void RequireChoice(MarketSelection selection, MarketChoice first, MarketChoice second)
    {
        if (selection.Choice != first && selection.Choice != second)
            throw new ArgumentException($"Invalid choice {selection.Choice} for {selection.Kind}");
    }

    // ---------------------------------------------------------------------------------------
    // Relation classification.
    // ---------------------------------------------------------------------------------------

    private static Relation Classify(Matchup matchup, IReadOnlyList<MarketSelection> selections,
        int i, int j, double pair, double pi, double pj)
    {
        var legs = new[] { i, j };
        double product = pi * pj;
        bool correlated = IsCorrelated(pair, product);
        double strength = Strength(pair, product);

        // Validity runs first and is never subject to the no-label fallback (design doc's carve-out).
        // An exclusion carries no strength: |ln 0| is unbounded, and ranking a rejection against
        // prices would be a category error — it never reaches a slip to be stated.
        if (pair == 0.0)
            return new Relation(RelationKind.MutuallyExclusive, legs, RelationSign.None, null, null,
                strength: 0.0, correlated: correlated);

        // rho > 1 as well as p_joint = min p_i: an implication always reinforces (rho = 1/p of the
        // implying leg), and requiring both is what stops a near-certain independent leg from
        // being read as an entailment.
        if (correlated && pair > product && Math.Abs(pair - Math.Min(pi, pj)) <= ImplicationTolerance)
        {
            int a = pi <= pj ? i : j;
            int b = pi <= pj ? j : i;
            return new Relation(RelationKind.Implies, new[] { a, b }, RelationSign.Reinforcing, null, null,
                strength, correlated);
        }

        SelectionFamily fi = FamilyOf(selections[i]);
        SelectionFamily fj = FamilyOf(selections[j]);
        if (fi != fj)
            return new Relation(RelationKind.Independent, legs, RelationSign.None, null, null,
                strength, correlated);

        RelationSign sign = pair > product ? RelationSign.Reinforcing
            : pair < product ? RelationSign.Opposing
            : RelationSign.None;

        if (fi != SelectionFamily.Goal)
            return new Relation(RelationKind.SharedCount, legs, sign, fi, null, strength, correlated);

        Side? side = SharedScorerSide(matchup, selections[i], selections[j]);
        return side.HasValue
            ? new Relation(RelationKind.ScorerOfSide, legs, sign, SelectionFamily.Goal, side, strength, correlated)
            : new Relation(RelationKind.SharedScoreline, legs, sign, SelectionFamily.Goal, null, strength, correlated);
    }

    /// <summary>|ln rho| for one pair, the ranking key behind the principal relation. Zero where rho
    /// is not a finite positive number — an impossible pair or a zero marginal — because those carry
    /// no price movement to rank.</summary>
    private static double Strength(double pair, double product)
    {
        if (pair <= 0.0 || product <= 0.0) return 0.0;
        return Math.Abs(Math.Log(pair / product));
    }

    /// <summary>"A scorer leg beside a leg on that team's goals." Moneyline, total goals and BTTS
    /// all read BOTH teams' goals, so any of them beside a scorer qualifies; two scorers on one team
    /// qualify for that team. Two scorers on OPPOSITE teams do not — they are linked only through the
    /// shared scoreline (they are conditionally independent given it), which is the label they get.</summary>
    private static Side? SharedScorerSide(Matchup matchup, MarketSelection a, MarketSelection b)
    {
        bool scorerA = a.Kind == MarketKind.AnytimeScorer;
        bool scorerB = b.Kind == MarketKind.AnytimeScorer;
        if (!scorerA && !scorerB) return null;
        if (scorerA && scorerB)
        {
            Side sa = matchup.PlayerSide(a.PlayerIndex);
            Side sb = matchup.PlayerSide(b.PlayerIndex);
            return sa == sb ? sa : (Side?)null;
        }
        return matchup.PlayerSide(scorerA ? a.PlayerIndex : b.PlayerIndex);
    }

    private static bool IsCorrelated(double joint, double product)
    {
        double slack = product > 0.0 ? CorrelationTolerance * product : 0.0;
        return Math.Abs(joint - product) > slack;
    }

    // ---------------------------------------------------------------------------------------
    // Splitting selections into families.
    // ---------------------------------------------------------------------------------------

    private sealed class Split
    {
        public readonly List<MarketSelection> GoalPredicates = new List<MarketSelection>();
        public readonly List<int> HomeScorers = new List<int>();
        public readonly List<int> AwayScorers = new List<int>();
        public readonly List<MarketSelection> Corner = new List<MarketSelection>();
        public readonly List<MarketSelection> Card = new List<MarketSelection>();

        public bool HasGoal => GoalPredicates.Count > 0 || HomeScorers.Count > 0 || AwayScorers.Count > 0;
    }

    /// <summary>Scorer legs carry matchup-board indices (away roster first, then home), so the side
    /// split needs the matchup. Duplicate scorer legs are deduped here: backing one player twice is
    /// the same event, P(A and A) = P(A). Left undeduped it would inflate k and the g &lt; k guard
    /// would call a perfectly ordinary ticket impossible.</summary>
    private static Split SplitFamilies(Matchup matchup, IReadOnlyList<MarketSelection> selections)
    {
        var split = new Split();
        for (int i = 0; i < selections.Count; i++)
        {
            MarketSelection selection = selections[i];
            switch (FamilyOf(selection))
            {
                case SelectionFamily.Corner:
                    split.Corner.Add(selection);
                    break;
                case SelectionFamily.Card:
                    split.Card.Add(selection);
                    break;
                default:
                    if (selection.Kind == MarketKind.AnytimeScorer)
                    {
                        if (selection.Choice != MarketChoice.Yes)
                            throw new ArgumentException("Anytime scorer is a YES-only market");
                        if (selection.Line != 0.0) throw new ArgumentException("Anytime scorer has no line");
                        List<int> side = matchup.PlayerSide(selection.PlayerIndex) == Side.Home
                            ? split.HomeScorers : split.AwayScorers;
                        if (!side.Contains(selection.PlayerIndex)) side.Add(selection.PlayerIndex);
                    }
                    else
                    {
                        split.GoalPredicates.Add(selection);
                    }
                    break;
            }
        }
        return split;
    }

    private static double[] NormalizedWeights(Matchup matchup, Side side, List<int> boardIndices)
    {
        IReadOnlyList<Player> roster = side == Side.Home ? matchup.Home.Players : matchup.Away.Players;
        double total = 0.0;
        foreach (Player player in roster) total += player.ScoringWeight;
        if (total <= 0.0) throw new InvalidOperationException("Scorer roster has no positive weights");

        var weights = new double[boardIndices.Count];
        for (int i = 0; i < boardIndices.Count; i++)
            weights[i] = matchup.PlayerAt(boardIndices[i]).ScoringWeight / total;
        return weights;
    }

    // ---------------------------------------------------------------------------------------
    // The outcome partition W.
    // ---------------------------------------------------------------------------------------

    /// <summary>One outcome class of the model's partition W: the score list it carries and how its
    /// unconditional weight is read off a matchup.</summary>
    private sealed class OutcomeClass
    {
        public string Name = "";
        public Func<MatchDistributions, IReadOnlyList<MatchModel.ScoreOutcome>> Scores = null!;

        /// <summary>Null for the residual class, whose weight is 1 minus every explicit weight.</summary>
        public Func<Matchup, double>? Weight;
    }

    private static readonly OutcomeClass[] Partition = DiscoverPartition();

    /// <summary>
    /// Derives W from what the model exposes, so a third outcome class costs nothing.
    ///
    /// <para>The engine constructs P(h,a) as SUM over w in W of P(w) * P(h,a|w). Today W is
    /// {home, away}; Lane 1 is adding draws, making it {home, draw, away}. The design doc's
    /// instruction is verbatim: "Write that sum over W, never over a hard-coded pair of branches."</para>
    ///
    /// <para>The score lists are discovered by shape — every <c>IReadOnlyList&lt;ScoreOutcome&gt;</c>
    /// property on <see cref="MatchDistributions"/>, in declaration order. The weights are discovered
    /// by the model's own naming convention: a class named X takes <c>Matchup.TrueXProb</c> if that
    /// property exists, and exactly one class must have no such property — it is the residual, and
    /// gets one minus the rest. Today that reproduces <c>TrueHomeProb</c> and
    /// <c>1.0 - TrueHomeProb</c> bit-for-bit. Add <c>DrawScores</c> + <c>TrueDrawProb</c> and this
    /// picks up three classes with no edit here; add a score list with no matching probability and
    /// it throws at type-load rather than silently pricing a partition that no longer sums to 1.</para>
    /// </summary>
    private static OutcomeClass[] DiscoverPartition()
    {
        Type listType = typeof(IReadOnlyList<MatchModel.ScoreOutcome>);
        var properties = new List<PropertyInfo>();
        foreach (PropertyInfo property in typeof(MatchDistributions)
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance))
            if (property.PropertyType == listType && property.GetMethod != null)
                properties.Add(property);

        if (properties.Count == 0)
            throw new InvalidOperationException(
                "MatchDistributions exposes no scoreline lists — the goal-family joint has no outcome partition");

        // Declaration order, so the summation order matches MatchModel.ScoreProbability's.
        properties.Sort((x, y) => x.MetadataToken.CompareTo(y.MetadataToken));

        var classes = new List<OutcomeClass>(properties.Count);
        int residuals = 0;
        foreach (PropertyInfo property in properties)
        {
            string name = property.Name;
            if (name.EndsWith("Scores", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "Scores".Length);
            if (name.EndsWith("Win", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "Win".Length);

            PropertyInfo? weightProperty = typeof(Matchup).GetProperty(
                "True" + name + "Prob", BindingFlags.Public | BindingFlags.Instance);
            Func<Matchup, double>? weight = null;
            if (weightProperty != null && weightProperty.PropertyType == typeof(double)
                && weightProperty.GetMethod != null)
                weight = (Func<Matchup, double>)weightProperty.GetMethod.CreateDelegate(typeof(Func<Matchup, double>));
            else
                residuals++;

            classes.Add(new OutcomeClass
            {
                Name = name,
                Scores = (Func<MatchDistributions, IReadOnlyList<MatchModel.ScoreOutcome>>)
                    property.GetMethod!.CreateDelegate(typeof(Func<MatchDistributions, IReadOnlyList<MatchModel.ScoreOutcome>>)),
                Weight = weight,
            });
        }

        if (residuals != 1)
            throw new InvalidOperationException(
                $"The outcome partition needs exactly one residual class, found {residuals}. Every class "
                + "except one must expose its unconditional weight as Matchup.True<Class>Prob.");

        return classes.ToArray();
    }

    private static double[] ClassWeights(Matchup matchup)
    {
        var weights = new double[Partition.Length];
        double explicitSum = 0.0;
        int residual = -1;
        for (int i = 0; i < Partition.Length; i++)
        {
            Func<Matchup, double>? read = Partition[i].Weight;
            if (read == null) { residual = i; continue; }
            weights[i] = read(matchup);
            explicitSum += weights[i];
        }
        weights[residual] = 1.0 - explicitSum;

        for (int i = 0; i < weights.Length; i++)
            if (weights[i] < -1e-12)
                throw new InvalidOperationException(
                    $"Outcome class {Partition[i].Name} carries negative weight {weights[i]:R} — the partition does not sum to 1");
        return weights;
    }
}

/// <summary>The price of one ticket under the correlation model, locked at placement. Produced by
/// <see cref="SameMatchModel.Price"/> and carried on <see cref="Ticket.SameMatch"/>; a ticket with at
/// most one leg per matchup has none, and keeps the product-of-leg-odds path untouched.</summary>
public sealed class SameMatchPrice
{
    /// <summary>p_ticket: the product, over matchups, of each matchup's exact joint. Different
    /// matchups are independent, which is what makes the product legitimate. Exactly 0.0 when the
    /// combination cannot happen.</summary>
    public double PTicket { get; }

    /// <summary>o_ticket = 1 / (p_ticket × κ × (1 + Ω)^n), before any leg-targeted Profit Boost —
    /// the boost is applied by the placing layer, which owns whether the relic was played.
    ///
    /// <para>Under <see cref="NaiveFallback"/> this is instead the product of the legs' offered odds:
    /// where the model finds correlation it cannot label, the price does not move. Exactly 0.0 when
    /// <see cref="Impossible"/> — no finite price exists, and a caller that ignores the refusal pays
    /// nothing rather than paying infinity.</para></summary>
    public double Price { get; }

    /// <summary>Every relation the ticket carries, from every matchup group. <see cref="Relation.Legs"/>
    /// indexes the TICKET's leg list, not a group's — a surface points at legs it can name.</summary>
    public IReadOnlyList<Relation> Relations { get; }

    /// <summary>The single relation a slip states (<see cref="JointModel.SelectPrincipal"/>), ranked
    /// across every matchup on the ticket. Null when nothing is statable.</summary>
    public Relation? Principal { get; }

    /// <summary>The ticket priced at the naive product because some correlated relation on it could
    /// not be labelled. Should never be true on the shipped board — Phase 1 verified the vocabulary
    /// is total — which is exactly why <see cref="SameMatchModel.NoLabelFallbacks"/> counts it.</summary>
    public bool NaiveFallback { get; }

    /// <summary>No outcome wins this ticket. A validity verdict, never a price: the placing layer
    /// refuses it (design/02's load-bearing carve-out) rather than selling it at some large number.</summary>
    public bool Impossible => PTicket == 0.0;

    /// <summary>
    /// The replacement price of EVERY survivor subset (<c>design/02-betting-math.md</c> § *Void:
    /// re-price on the survivors*), indexed by SURVIVOR BITMASK over the ticket's leg list: bit
    /// <c>i</c> set means leg <c>i</c> survives. <c>SubsetPrices[m]</c> is
    /// <c>1 / (p_joint(legs in m) × κ × (1 + Ω)^popcount(m))</c>.
    ///
    /// <para><b>κ applies only while a same-match group survives (canon 2026-08-12).</b> A subset whose
    /// legs are all on DISTINCT matchups is an ordinary parlay — there is no correlation left to charge
    /// for — so it prices at κ = 1 whatever the ticket was sold under. Moot at the shipped default and
    /// real the moment the gate campaign moves the dial.</para>
    ///
    /// <para><b>Computed here, at ticket lock, and never re-derived at settlement.</b> That is the
    /// design's requirement, not an optimization: it makes settlement deterministic and independent of
    /// WHEN — and now, how often — a void is discovered. Note the exponent drops with each leg, so the
    /// ticket pays less margin after a void — intended.</para>
    ///
    /// <para><b>Multiple voids are supported (canon CLOSED 2026-08-12, reversing the earlier OPEN).</b>
    /// Books cap themselves at a single void for latency and volume reasons we do not have; two Slips
    /// on a three-leg ticket is an ordinary hand. With <c>MaxLegs = 4</c> the whole table is at most 15
    /// live prices, so every subset is priced up front rather than composing a rule at settlement.</para>
    ///
    /// <para>Layout: length <c>1 &lt;&lt; n</c>. Slot 0 (no survivors) is 0.0 and unreachable — the
    /// engine never voids a ticket's last active leg. The full mask holds <see cref="Price"/> itself,
    /// so "no voids yet" is the same lookup as any other.</para>
    ///
    /// <para>Empty in the two cases with no void scenario at all: an <see cref="Impossible"/> ticket
    /// (refused, so it never reaches a void) and a one-leg list.</para></summary>
    public IReadOnlyList<double> SubsetPrices { get; }

    /// <summary>The single-void row of <see cref="SubsetPrices"/>, one per leg index:
    /// <c>VoidPrices[v]</c> is what this ticket prices at when leg <c>v</c> — and only leg <c>v</c> —
    /// voids. A view, not a second source of truth: it is read straight out of the subset table.</summary>
    public IReadOnlyList<double> VoidPrices { get; }

    public SameMatchPrice(double pTicket, double price, IReadOnlyList<Relation> relations,
        Relation? principal, bool naiveFallback, IReadOnlyList<double> subsetPrices)
    {
        PTicket = pTicket;
        Price = price;
        Relations = relations ?? throw new ArgumentNullException(nameof(relations));
        Principal = principal;
        NaiveFallback = naiveFallback;
        SubsetPrices = subsetPrices ?? throw new ArgumentNullException(nameof(subsetPrices));
        VoidPrices = SingleVoidRow(SubsetPrices);
    }

    /// <summary>How many legs a subset table of this length covers: <c>Count == 1 &lt;&lt; n</c>. Zero
    /// for an empty table.</summary>
    public static int LegCountOf(IReadOnlyList<double> subsetPrices)
    {
        if (subsetPrices == null) throw new ArgumentNullException(nameof(subsetPrices));
        if (subsetPrices.Count == 0) return 0;
        int n = 0;
        while ((1 << n) < subsetPrices.Count) n++;
        if ((1 << n) != subsetPrices.Count)
            throw new ArgumentException(
                $"A survivor-subset table is 2^n entries; {subsetPrices.Count} is not a power of two",
                nameof(subsetPrices));
        return n;
    }

    /// <summary>The single-void row of a survivor-subset table: entry <c>v</c> is the subset with
    /// every leg but <c>v</c> surviving. Shared by <see cref="SameMatchPrice"/> and
    /// <c>Ticket.LockedVoidPrices</c> so the two never drift.</summary>
    public static IReadOnlyList<double> SingleVoidRow(IReadOnlyList<double> subsetPrices)
    {
        int n = LegCountOf(subsetPrices);
        if (n == 0) return Array.Empty<double>();
        int full = subsetPrices.Count - 1;
        var row = new double[n];
        for (int v = 0; v < n; v++) row[v] = subsetPrices[full & ~(1 << v)];
        return row;
    }
}

/// <summary>
/// Ticket-level pricing under the correlation model (<c>design/02-betting-math.md</c> § *Same-game
/// tickets*, plan F_0.6.0 Phase 2). <see cref="JointModel"/> knows one matchup; this knows a ticket.
///
/// <para>Legs on different matchups are independent, so the ticket's probability is the product over
/// matchups of each matchup's joint, and margin is charged once per leg exactly as it is on an
/// ordinary parlay:</para>
///
/// <code>
/// p_ticket = PROD over matchups m of p_joint(legs on m)
/// o_ticket = 1 / (p_ticket × κ × (1 + Ω)^n)
/// </code>
///
/// <para><b>This type is entered only when some matchup carries two or more legs.</b> A ticket with
/// at most one leg per matchup never reaches here — it keeps the pre-existing product-of-leg-odds
/// path, verbatim, so its price, payout, void behaviour and settlement stay bit-identical. The two
/// paths agree algebraically at κ = 1 on an independent combination, but not necessarily in the last
/// bits of an IEEE double, and the whole gate baseline sits downstream of those bits.</para>
/// </summary>
public static class SameMatchModel
{
    /// <summary>The instrument's canon name (Allen, batch 48) — uppercase market vocabulary,
    /// untracked. A NAME, not a caption: presentation decides where an instrument name appears, and
    /// this type never composes a sentence out of it.</summary>
    public const string Instrument = "SAME MATCH";

    /// <summary>The mark's canon name (Allen, batch 48). <b>Authoring boundary:</b> the mark is drawn,
    /// not captioned — this string is never emitted beside the mark, a price or a relation. It belongs
    /// to rules copy, the ledger, and first-encounter only. Named here so pricing and presentation
    /// inherit one spelling instead of a placeholder.</summary>
    public const string Mark = "THE HOUSE'S LINE";

    private static long _noLabelFallbacks;

    /// <summary>Process-wide count of tickets priced at the naive product because a correlated
    /// relation on them could not be labelled. Zero is the only acceptable value on the shipped
    /// board; a non-zero reading is a live money leak worth up to +274% EV on an implication pair,
    /// which is why the design doc requires it counted and surfaced rather than logged. The gate
    /// campaign reads this; per-ticket, <see cref="SameMatchPrice.NaiveFallback"/> says which.</summary>
    public static long NoLabelFallbacks => Interlocked.Read(ref _noLabelFallbacks);

    /// <summary>Zeroes the fallback counter. For a test or a campaign run that wants a clean window;
    /// nothing in the engine calls it.</summary>
    public static void ResetNoLabelFallbacks() => Interlocked.Exchange(ref _noLabelFallbacks, 0L);

    /// <summary>Whether this ticket is a SAME MATCH ticket at all: does any matchup carry two or more
    /// legs. This is the switch between the joint path and the untouched product path.</summary>
    public static bool IsSameMatch(IReadOnlyList<Leg> legs)
    {
        if (legs == null) throw new ArgumentNullException(nameof(legs));
        for (int i = 0; i < legs.Count; i++)
            for (int j = i + 1; j < legs.Count; j++)
                if (ReferenceEquals(legs[i].Matchup, legs[j].Matchup)) return true;
        return false;
    }

    /// <summary>The most legs this model will build a survivor-subset table for. The table is
    /// <c>2^n</c> entries, so the bound is on the exponent, not on taste: at the shipped
    /// <see cref="RunConfig.MaxLegs"/> of 4 a ticket costs 15 live prices, and 12 leaves an order of
    /// magnitude of headroom before the cost is worth a second thought. Above it, fail with the reason
    /// rather than quietly allocating.</summary>
    public const int MaxSubsetLegs = 12;

    /// <summary>
    /// Prices a ticket on its exact joint, and — in the same pass — prices EVERY survivor subset it
    /// can ever be voided down to (<see cref="SameMatchPrice.SubsetPrices"/>).
    ///
    /// <para>The replacement prices are LOCKED HERE by design: canon requires settlement to be
    /// deterministic and independent of when a void is discovered, so a void is a table lookup at
    /// settlement rather than a re-derivation. A scenario's price is simply this same function applied
    /// to the survivors, in ticket order, so the survivors' grouping — and therefore the last bits of
    /// their price — is a function of the ticket alone.</para>
    ///
    /// <para><b>Every subset, not just the single-void row</b> (canon CLOSED 2026-08-12): multiple
    /// voids are ordinary play, and at <c>MaxLegs = 4</c> the complete table is at most 15 prices.
    /// Composing a rule for the second void at settlement would break the locked-at-placement property;
    /// enumerating the subsets keeps it.</para>
    /// </summary>
    /// <param name="legs">The ticket's legs, in ticket order.</param>
    /// <param name="overround">Ω, the per-leg margin the book already charges on every single.</param>
    /// <param name="margin">κ, the SAME MATCH margin dial (<see cref="RunConfig.SgpMargin"/>).</param>
    public static SameMatchPrice Price(IReadOnlyList<Leg> legs, double overround, double margin)
    {
        SameMatchPrice core = PriceCore(legs, overround, margin, countFallback: true);

        // No void scenario exists for an impossible ticket (it is refused before it can be sweated) or
        // for a one-leg list (there would be nothing left to price, and the engine never voids a
        // ticket's last active leg). Both leave the table empty rather than fabricating one.
        if (core.Impossible || legs.Count < 2) return core;

        if (legs.Count > MaxSubsetLegs)
            throw new ArgumentException(
                $"A survivor-subset table for {legs.Count} legs is {1L << legs.Count} prices; the model "
                + $"builds them up to {MaxSubsetLegs} legs (RunConfig.MaxLegs ships at 4)", nameof(legs));

        // Indexed by SURVIVOR mask: bit i set means leg i survives. Slot 0 (nothing survives) stays
        // 0.0 and is unreachable — a Mulligan needs two active legs, so the last one never voids.
        int full = (1 << legs.Count) - 1;
        var subsetPrices = new double[full + 1];
        for (int mask = 1; mask <= full; mask++)
        {
            // The full mask IS this ticket. Assigned rather than re-priced so "no voids yet" is the
            // same number, to the bit, as the price the ticket was sold at.
            if (mask == full) { subsetPrices[mask] = core.Price; continue; }

            Leg[] survivors = Survivors(legs, mask);

            // κ APPLIES ONLY WHILE A SAME-MATCH GROUP SURVIVES (canon 2026-08-12). Voiding can leave
            // survivors that no longer share a matchup — two legs on one match plus a third elsewhere,
            // void one of the pair — and what remains is an ordinary parlay. κ is the price of
            // correlation, so with no correlation left there is nothing to charge for and the subset
            // prices at κ = 1. A one-leg subset is the degenerate case of the same rule.
            //
            // Invisible at the shipped κ = 1, and real the moment the gate campaign moves the dial. It
            // also bounds the sub-evens case: a subset with at most one leg per matchup prices at
            // Π 1/(p_i (1+Ω)) — the board's own singles, each above evens — so a replacement can only
            // fall to or below evens while a correlated group is still on the ticket.
            double subsetMargin = IsSameMatch(survivors) ? margin : 1.0;

            // Under the no-label fallback "the price does not move" — on void too. The scenario
            // prices at the board's own product over the survivors, not at their joint, so this
            // ticket's replacements stay consistent with the price it was actually sold at.
            subsetPrices[mask] = core.NaiveFallback
                ? NaivePrice(survivors)
                : PriceCore(survivors, overround, subsetMargin, countFallback: false).Price;
        }

        return new SameMatchPrice(core.PTicket, core.Price, core.Relations, core.Principal,
            core.NaiveFallback, subsetPrices);
    }

    /// <summary>The legs <paramref name="survivorMask"/> keeps, ticket order preserved — the survivors
    /// ARE a ticket, and their price must be reproducible from the seed like any other.</summary>
    private static Leg[] Survivors(IReadOnlyList<Leg> legs, int survivorMask)
    {
        int kept = 0;
        for (int i = 0; i < legs.Count; i++) if ((survivorMask & (1 << i)) != 0) kept++;

        var rest = new Leg[kept];
        for (int i = 0, k = 0; i < legs.Count; i++)
            if ((survivorMask & (1 << i)) != 0) rest[k++] = legs[i];
        return rest;
    }

    /// <summary>The price of one leg set, with no void scenarios attached.</summary>
    /// <param name="countFallback">Whether a fired no-label fallback increments
    /// <see cref="NoLabelFallbacks"/>. TRUE for the ticket actually being sold; FALSE for the
    /// hypothetical survivor sets priced beside it — the counter means "tickets SOLD at the naive
    /// product", and a scenario that may never happen must not inflate it.</param>
    private static SameMatchPrice PriceCore(IReadOnlyList<Leg> legs, double overround, double margin,
        bool countFallback)
    {
        if (legs == null) throw new ArgumentNullException(nameof(legs));
        if (legs.Count == 0) throw new ArgumentException("A ticket needs at least one leg", nameof(legs));
        if (overround < 0.0 || double.IsNaN(overround))
            throw new ArgumentException($"Overround must be >= 0, got {overround}", nameof(overround));
        if (margin <= 0.0 || double.IsNaN(margin) || double.IsInfinity(margin))
            throw new ArgumentException($"The SAME MATCH margin dial must be > 0, got {margin}", nameof(margin));

        // First-appearance grouping, by matchup identity. Deliberately not a hash-ordered group-by:
        // the multiplication order — and therefore the last bits of the price — must be a function of
        // the ticket alone, reproducible from a seed on any run.
        var matchups = new List<Matchup>();
        var groups = new List<List<int>>();
        for (int i = 0; i < legs.Count; i++)
        {
            int group = -1;
            for (int m = 0; m < matchups.Count; m++)
                if (ReferenceEquals(matchups[m], legs[i].Matchup)) { group = m; break; }
            if (group < 0)
            {
                matchups.Add(legs[i].Matchup);
                groups.Add(new List<int>());
                group = groups.Count - 1;
            }
            groups[group].Add(i);
        }

        double pTicket = 1.0;
        var relations = new List<Relation>();
        bool unlabelled = false;

        for (int g = 0; g < groups.Count; g++)
        {
            List<int> members = groups[g];
            var selections = new MarketSelection[members.Count];
            for (int i = 0; i < members.Count; i++) selections[i] = legs[members[i]].Selection;

            (double pJoint, IReadOnlyList<Relation> groupRelations, Relation? _) =
                JointModel.JointProbability(matchups[g], selections);
            pTicket *= pJoint;

            for (int r = 0; r < groupRelations.Count; r++)
            {
                Relation relation = groupRelations[r];

                // Relation.Legs indexes the GROUP's selection list; re-key it onto the ticket, or a
                // surface would annotate the wrong leg on any ticket with more than one matchup.
                var mapped = new int[relation.Legs.Count];
                for (int i = 0; i < mapped.Length; i++) mapped[i] = members[relation.Legs[i]];

                relations.Add(new Relation(relation.Kind, mapped, relation.Sign, relation.Family,
                    relation.ScorerSide, relation.Strength, relation.Correlated));

                // The only unlabelable shape the vocabulary can produce: measurable correlation that
                // classified as Independent. Every other kind IS a label.
                if (relation.Kind == RelationKind.Independent && relation.Correlated) unlabelled = true;
            }
        }

        Relation? principal = JointModel.SelectPrincipal(relations);

        // Validity first, and never subject to the fallback. An unlabelable zero that fell through to
        // naive pricing would sell a ticket that cannot win, at odds up to a mean decimal of 2070.
        if (pTicket == 0.0)
            return new SameMatchPrice(0.0, 0.0, relations, principal, naiveFallback: false,
                subsetPrices: Array.Empty<double>());

        if (unlabelled)
        {
            if (countFallback) Interlocked.Increment(ref _noLabelFallbacks);
            return new SameMatchPrice(pTicket, NaivePrice(legs), relations, principal, naiveFallback: true,
                subsetPrices: Array.Empty<double>());
        }

        double price = 1.0 / (pTicket * margin * Math.Pow(1.0 + overround, legs.Count));
        return new SameMatchPrice(pTicket, price, relations, principal, naiveFallback: false,
            subsetPrices: Array.Empty<double>());
    }

    /// <summary>The board's own product of leg odds — today's price, which is what the no-label
    /// fallback falls back TO ("the price does not move").</summary>
    private static double NaivePrice(IReadOnlyList<Leg> legs)
    {
        var odds = new double[legs.Count];
        for (int i = 0; i < legs.Count; i++) odds[i] = legs[i].OfferedOdds;
        return OddsMath.ParlayDecimal(odds);
    }
}
