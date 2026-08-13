using System;
using System.Collections.Generic;
using System.Reflection;

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

    public Relation(RelationKind kind, IReadOnlyList<int> legs, RelationSign sign,
        SelectionFamily? family, Side? scorerSide)
    {
        Kind = kind;
        Legs = legs ?? throw new ArgumentNullException(nameof(legs));
        Sign = sign;
        Family = family;
        ScorerSide = scorerSide;
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
    /// <paramref name="matchup"/>, together with the structural relations that explain it.</summary>
    /// <returns><c>pJoint</c> — the enumerated joint, exactly 0.0 for an impossible ticket — and
    /// <c>relations</c>, one label per selection pair plus, when the whole ticket is impossible
    /// without any impossible pair, a ticket-level <see cref="RelationKind.MutuallyExclusive"/>.</returns>
    public static (double pJoint, IReadOnlyList<Relation> relations) JointProbability(
        Matchup matchup, IReadOnlyList<MarketSelection> selections)
    {
        if (matchup == null) throw new ArgumentNullException(nameof(matchup));
        if (selections == null) throw new ArgumentNullException(nameof(selections));
        if (selections.Count == 0)
            throw new ArgumentException("A ticket needs at least one leg", nameof(selections));

        double pJoint = Probability(matchup, selections);
        var relations = new List<Relation>();
        if (selections.Count == 1) return (pJoint, relations);

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
            relations.Add(new Relation(RelationKind.MutuallyExclusive, all, RelationSign.None, null, null));
        }

        return (pJoint, relations);
    }

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

        // Validity runs first and is never subject to the no-label fallback (design doc's carve-out).
        if (pair == 0.0)
            return new Relation(RelationKind.MutuallyExclusive, legs, RelationSign.None, null, null);

        double product = pi * pj;
        bool correlated = IsCorrelated(pair, product);

        // rho > 1 as well as p_joint = min p_i: an implication always reinforces (rho = 1/p of the
        // implying leg), and requiring both is what stops a near-certain independent leg from
        // being read as an entailment.
        if (correlated && pair > product && Math.Abs(pair - Math.Min(pi, pj)) <= ImplicationTolerance)
        {
            int a = pi <= pj ? i : j;
            int b = pi <= pj ? j : i;
            return new Relation(RelationKind.Implies, new[] { a, b }, RelationSign.Reinforcing, null, null);
        }

        SelectionFamily fi = FamilyOf(selections[i]);
        SelectionFamily fj = FamilyOf(selections[j]);
        if (fi != fj)
            return new Relation(RelationKind.Independent, legs, RelationSign.None, null, null);

        RelationSign sign = pair > product ? RelationSign.Reinforcing
            : pair < product ? RelationSign.Opposing
            : RelationSign.None;

        if (fi != SelectionFamily.Goal)
            return new Relation(RelationKind.SharedCount, legs, sign, fi, null);

        Side? side = SharedScorerSide(matchup, selections[i], selections[j]);
        return side.HasValue
            ? new Relation(RelationKind.ScorerOfSide, legs, sign, SelectionFamily.Goal, side)
            : new Relation(RelationKind.SharedScoreline, legs, sign, SelectionFamily.Goal, null);
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
