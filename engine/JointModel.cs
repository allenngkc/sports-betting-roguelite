using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

    /// <summary>Legs that read different draws — unrelated, no adjustment.
    ///
    /// <para>Canon's gloss on this row reads "legs drawn from different families", which was exact
    /// while every count market was a MATCH total. F_0.5.0's team totals split each count family
    /// across its two independent draws, so HOME corners beside AWAY corners is same-family and still
    /// exactly the product. The binding half of the canon row — "unrelated, no adjustment" — is what
    /// this member means, and it is what that pair is.</para></summary>
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

    /// <summary>The family the relation lives in; null for an
    /// <see cref="RelationKind.Independent"/> pair — whether it is independent because the legs sit in
    /// different families or because they read different DRAWS of one count family — and null for a
    /// ticket-level exclusion spanning families.</summary>
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

    /// <summary>
    /// Which of the three sampled draws a selection reads.
    ///
    /// <para><b>This switch is the ONLY per-kind knowledge this model keeps</b>, and it is knowledge
    /// the grader cannot supply: <c>MatchModel.Grades</c> answers "did this selection win", never
    /// "which draw did it read". Everything else about a market kind — the predicate itself — is
    /// delegated to <see cref="MatchModel.Grades(Matchup, MatchStatLine, MarketSelection)"/>, so
    /// adding a kind here is one line rather than a new predicate arm that can disagree with
    /// settlement.</para>
    ///
    /// <para>An unmapped kind THROWS rather than defaulting to a family. Guessing Goal for a card
    /// market would price a ticket off the wrong draw and never fail a test; F_0.5.0's nine new kinds
    /// arrived through exactly this arm, and the throw is what turned that into a red build instead of
    /// a silently mispriced board.</para></summary>
    public static SelectionFamily FamilyOf(MarketSelection selection) => selection.Kind switch
    {
        MarketKind.Moneyline => SelectionFamily.Goal,
        MarketKind.TotalGoals => SelectionFamily.Goal,
        MarketKind.BothTeamsToScore => SelectionFamily.Goal,
        MarketKind.AnytimeScorer => SelectionFamily.Goal,
        MarketKind.DoubleChance => SelectionFamily.Goal,
        MarketKind.Handicap => SelectionFamily.Goal,
        MarketKind.TeamTotalGoals => SelectionFamily.Goal,
        MarketKind.CorrectScore => SelectionFamily.Goal,
        MarketKind.WinningMargin => SelectionFamily.Goal,
        MarketKind.TotalGoalsOddEven => SelectionFamily.Goal,
        MarketKind.PlayerMultiScorer => SelectionFamily.Goal,
        MarketKind.TotalCorners => SelectionFamily.Corner,
        MarketKind.TeamTotalCorners => SelectionFamily.Corner,
        MarketKind.TotalCards => SelectionFamily.Card,
        MarketKind.TeamTotalCards => SelectionFamily.Card,
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "Unfamilied market kind"),
    };

    /// <summary>Whether a kind is settled by scorer ATTRIBUTION rather than by a predicate over the
    /// stat line. These two are the only kinds the joint cannot hand to <c>MatchModel.Grades</c>: the
    /// grader reads <c>MatchStatLine.HomeScorers</c>, which a synthetic scoreline cell does not carry,
    /// and would answer "did not score" for every cell. They are priced by
    /// <see cref="ScorerTerm"/> instead, which is the exact multinomial the attribution samples.</summary>
    internal static bool IsPlayerMarket(MarketKind kind)
        => kind == MarketKind.AnytimeScorer || kind == MarketKind.PlayerMultiScorer;

    /// <summary>Which SIDES of a count family's two independent draws a selection reads: a match
    /// total reads both, a team total reads one. Two count legs whose side sets are disjoint read
    /// different draws and are genuinely independent — see <see cref="Classify"/>.</summary>
    private static (bool home, bool away) CountSidesOf(MarketSelection selection) => selection.Kind switch
    {
        MarketKind.TotalCorners or MarketKind.TotalCards => (true, true),
        MarketKind.TeamTotalCorners or MarketKind.TeamTotalCards =>
            (selection.Team ?? throw new ArgumentException($"{selection.Kind} carries no Team")) == Side.Home
                ? (true, false)
                : (false, true),
        _ => throw new ArgumentOutOfRangeException(nameof(selection), selection.Kind, "Not a counting market"),
    };

    // ---------------------------------------------------------------------------------------
    // The joint itself.  p_joint = p_GOAL * p_CORNER * p_CARD.
    // ---------------------------------------------------------------------------------------

    /// <summary>The scalar joint. Private by design (S73) — callers get it only alongside its
    /// relation labels, via <see cref="JointProbability"/>.</summary>
    /// <summary>The joint probability ALONE — no relation classification, no principal nomination.
    /// <para>Internal rather than private because the conditional cash-out quote is a ratio of three
    /// joints and needs none of the labelling. Routing it through
    /// <see cref="JointProbability(Matchup, IReadOnlyList{MarketSelection})"/> instead cost a marginal
    /// per leg plus a joint per PAIR plus a <c>Classify</c> per pair — 11 enumerations for a four-leg
    /// group where one was wanted — three times per leg boundary, with the labels discarded at the
    /// call site. That is what turned a gate campaign into a four-CPU-hour crawl (F_0.6.0, profiled
    /// 2026-08-15). Labels are for slips; quotes want the number.</para></summary>
    internal static double Probability(Matchup matchup, IReadOnlyList<MarketSelection> selections)
    {
        Split split = SplitFamilies(matchup, selections);

        // Multiplying by an untouched family's 1.0 is exact, so a single-family ticket is
        // bit-identical to the corresponding MatchModel sum rather than merely close to it.
        double p = 1.0;
        if (split.HasGoal) p *= GoalFamily(matchup, split);
        if (split.Corner.Count > 0)
            p *= CountFamily(matchup, SelectionFamily.Corner,
                matchup.Dist.HomeCornerRaw, matchup.Dist.HomeCornerTotal,
                matchup.Dist.AwayCornerRaw, matchup.Dist.AwayCornerTotal, split.Corner);
        if (split.Card.Count > 0)
            p *= CountFamily(matchup, SelectionFamily.Card,
                matchup.Dist.HomeCardRaw, matchup.Dist.HomeCardTotal,
                matchup.Dist.AwayCardRaw, matchup.Dist.AwayCardTotal, split.Card);
        return p;
    }

    /// <summary>
    /// p_GOAL = SUM over w in W of P(w) * SUM over (h,a) of P(h,a|w) * 1[goal predicates] * PROD_t Q_t(g_t).
    ///
    /// <para>The outer sum runs over the model's outcome partition W — the <see cref="MatchResult"/>
    /// enum, in <c>MatchModel.Classes</c>' order — and the inner sum mirrors
    /// <c>MatchModel.ScoreProbability</c>'s shape and order exactly, which is what keeps a single-leg
    /// joint as close to the engine's own price as double arithmetic allows.</para>
    ///
    /// <para><b>That is close, not bit-identical — measured, and this note used to overclaim it.</b>
    /// 132 of 244 board legs disagree in the last bits: ~70 ulp on <c>PlayerMultiScorer</c>, 38 on
    /// <c>AnytimeScorer</c>, 26 on <c>TotalCorners</c>; <c>CorrectScore</c>, <c>Handicap</c> and
    /// <c>WinningMargin</c> are exact. Agreement is ~1.5e-14 relative, comfortably inside the 1e-12
    /// verification gate, and a pinned diagnostic test carries the per-kind table. The claim that
    /// <em>is</em> exact, and the one the same-match path actually rests on, is
    /// <c>p_joint(all legs) == SameMatch.PTicket</c> — asserted with <c>==</c>.</para>
    /// </summary>
    private static double GoalFamily(Matchup matchup, Split split)
    {
        double[] weights = ClassWeights(matchup);
        MatchDistributions dist = matchup.Dist;

        int maxGoals = matchup.ModelConfig.MaxGoalsGrid;
        double[]? home = split.HomeScorers.Count > 0 ? ScorerTerms(matchup, Side.Home, split.HomeScorers, maxGoals) : null;
        double[]? away = split.AwayScorers.Count > 0 ? ScorerTerms(matchup, Side.Away, split.AwayScorers, maxGoals) : null;

        List<MarketSelection> predicates = split.GoalPredicates;
        MatchStatLine[] cells = predicates.Count > 0 ? Cells.Goals(maxGoals) : Cells.None;
        int stride = maxGoals + 1;

        double sum = 0.0;
        for (int c = 0; c < Partition.Length; c++)
        {
            double weight = weights[c];
            IReadOnlyList<MatchModel.ScoreOutcome> scores = ScoresOf(dist, Partition[c]);
            for (int s = 0; s < scores.Count; s++)
            {
                MatchModel.ScoreOutcome x = scores[s];
                if (predicates.Count > 0
                    && !AllGrade(matchup, predicates, cells[x.HomeGoals * stride + x.AwayGoals])) continue;

                double q = 1.0;
                if (home != null)
                {
                    q *= home[x.HomeGoals];
                    if (q == 0.0) continue;
                }
                if (away != null) q *= away[x.AwayGoals];

                sum += weight * x.Probability * q;
            }
        }
        return sum;
    }

    /// <summary>Q_t(g) for every reachable goal count on one team, evaluated once per count instead of
    /// once per scoreline cell. Purely a hoist — the term is a function of g alone — but the grid holds
    /// several cells per goal count and the inclusion-exclusion runs <c>Math.Pow</c> per subset, which
    /// made it the model's hot spot under the gates' triple sweep.</summary>
    private static double[] ScorerTerms(Matchup matchup, Side side, List<ScorerLeg> backed, int maxGoals)
    {
        (double[] weights, int[] needs) = NormalizedWeights(matchup, side, backed);
        var terms = new double[maxGoals + 1];
        for (int g = 0; g <= maxGoals; g++) terms[g] = ScorerTerm(weights, needs, g);
        return terms;
    }

    /// <summary>
    /// The exact multinomial term for the k backed players on one team against that team's g goals:
    /// P(player i scores at least n_i, for every i). Goals are attributed independently from the
    /// roster-normalized weights, so the count vector is Multinomial(g; w), and the term is written as
    /// inclusion-exclusion over the events "player i falls SHORT of n_i":
    ///
    /// <code>
    /// Q_t(g) = 0                                                       if g &lt; SUM_i n_i
    /// Q_t(g) = SUM over S subset of {1..k} of (-1)^|S| * P(X_i &lt; n_i for every i in S)
    /// </code>
    ///
    /// <para><b>Anytime is the n_i = 1 case and is untouched by the generalization.</b> With every
    /// threshold at 1, "falls short" is "does not score", the inner probability collapses to
    /// <c>(1 - SUM_{i in S} w_i)^g</c>, and this reduces to the design doc's formula term for term and
    /// bit for bit. The 2+ case is a genuinely different combinatorial object — "at least n" is not a
    /// union of "scores" events — so the short-fall probability is enumerated exactly over the count
    /// vectors below rather than approximated.</para>
    ///
    /// <para><b>The g &lt; SUM n_i guard is normative, not an optimization</b> (design doc, verbatim,
    /// where it reads g &lt; k for the all-ones case). The sum cancels to ~1e-17 rather than 0 in IEEE
    /// double, which turns a structurally impossible ticket — two players both scoring inside one goal,
    /// or one player scoring twice inside one goal — into a vanishingly small POSITIVE probability that
    /// passes every zero check. Twelve impossible triple shapes were misclassified exactly this way
    /// before the guard existed.</para>
    /// </summary>
    private static double ScorerTerm(double[] weights, int[] needs, int goals)
    {
        int k = weights.Length;
        int required = 0;
        for (int i = 0; i < k; i++) required += needs[i];
        if (goals < required) return 0.0;
        if (k > 24) throw new ArgumentOutOfRangeException(nameof(weights), k,
            "Inclusion-exclusion is exponential in the backed-player count");

        Span<int> members = stackalloc int[k];
        double sum = 0.0;
        int subsets = 1 << k;
        for (int mask = 0; mask < subsets; mask++)
        {
            double excluded = 0.0;
            int bits = 0;
            for (int i = 0; i < k; i++)
                if ((mask & (1 << i)) != 0) { excluded += weights[i]; members[bits++] = i; }

            // Roster-normalized weights sum to 1, so the remainder is non-negative in exact
            // arithmetic; the clamp only absorbs the ~1e-16 undershoot of a whole-roster subset.
            double rest = 1.0 - excluded;
            if (rest < 0.0) rest = 0.0;

            double term = ShortFall(weights, needs, members.Slice(0, bits), 0, goals, 1.0, 1.0, rest);
            sum += (bits & 1) == 0 ? term : -term;
        }
        return sum;
    }

    /// <summary>P(every player in <paramref name="members"/> scores strictly fewer than his threshold),
    /// summed over the count vectors that satisfy it. The multinomial coefficient is built down the
    /// recursion as <c>C(g, c_1) * C(g - c_1, c_2) * ...</c>, and the lumped "everyone else" category
    /// carries <paramref name="rest"/>.
    ///
    /// <para>When every threshold is 1 the only vector is all-zero, and this returns
    /// <c>1.0 * 1.0 * Math.Pow(rest, goals)</c> — the anytime term, unchanged in the last bit.</para></summary>
    private static double ShortFall(double[] weights, int[] needs, ReadOnlySpan<int> members, int at,
        int remaining, double coefficient, double weightProduct, double rest)
    {
        if (at == members.Length) return coefficient * weightProduct * Math.Pow(rest, remaining);

        int i = members[at];
        double sum = 0.0;
        for (int c = 0; c < needs[i] && c <= remaining; c++)
            sum += ShortFall(weights, needs, members, at + 1, remaining - c,
                coefficient * Binomial(remaining, c), weightProduct * Math.Pow(weights[i], c), rest);
        return sum;
    }

    /// <summary>Mirrors <c>MatchModel.Binomial</c>, which is private to that class. Same loop, so the
    /// multi-scorer coefficient here and the one the engine prices a single 2+ leg with agree.</summary>
    private static double Binomial(int n, int k)
    {
        if (k < 0 || k > n) return 0.0;
        double c = 1.0;
        for (int i = 0; i < k; i++) c = c * (n - i) / (i + 1);
        return c;
    }

    /// <summary>p_COUNT = SUM_{c_h} SUM_{c_a} P(c_h) * P(c_a) * 1[every predicate in the family
    /// holds]. Mirrors <c>MatchModel.CountTotalProbability</c>'s loop order exactly.
    ///
    /// <para>The two axes are kept separate rather than summed into a total, because a TEAM total
    /// reads ONE of them. The grid is unchanged — the two draws were always independent and always
    /// enumerated as a product — and a match-total leg still grades on <c>h + a</c>, through
    /// <c>MatchModel.Grades</c> rather than a restatement of it.</para></summary>
    private static double CountFamily(Matchup matchup, SelectionFamily family,
        double[] homeRaw, double homeTotal, double[] awayRaw, double awayTotal,
        List<MarketSelection> legs)
    {
        MatchStatLine[] cells = Cells.Counts(family, homeRaw.Length - 1, awayRaw.Length - 1);
        int stride = awayRaw.Length;

        double p = 0.0;
        for (int h = 0; h < homeRaw.Length; h++)
            for (int a = 0; a < awayRaw.Length; a++)
                if (AllGrade(matchup, legs, cells[h * stride + a]))
                    p += (homeRaw[h] / homeTotal) * (awayRaw[a] / awayTotal);
        return p;
    }

    // ---------------------------------------------------------------------------------------
    // Predicates.  There are none here any more: a ticket prices on exactly the outcomes that
    // would SETTLE it because it asks the settlement grader, MatchModel.Grades, cell by cell.
    // Restating the predicates was the "JointModel forgot a market" bug class, and F_0.5.0's nine
    // new kinds are what collected on it.
    // ---------------------------------------------------------------------------------------

    /// <summary>Whether every leg in one family grades on one synthetic cell of that family's grid.
    ///
    /// <para><b>The cell carries only its own family's fields</b> (a goal cell has zero corners and
    /// zero cards), which is sound because every non-player kind's grader reads exactly one family's
    /// fields — verified selection by selection in <c>MatchModel.Grades</c>, and pinned by
    /// <c>JointModelTests.Grades_reads_only_its_own_familys_fields</c>. Routing a leg to the wrong
    /// family would therefore grade it against zeros, so <see cref="FamilyOf"/> is the single router
    /// and <see cref="SplitFamilies"/> the single caller.</para></summary>
    private static bool AllGrade(Matchup matchup, List<MarketSelection> legs, MatchStatLine cell)
    {
        for (int i = 0; i < legs.Count; i++)
        {
            MarketSelection leg = legs[i];
            // A scorer leg reaching here would grade against an EMPTY attribution and silently
            // answer "did not score" for every cell. It is priced by ScorerTerm instead.
            if (IsPlayerMarket(leg.Kind))
                throw new ArgumentException(
                    $"{leg.Kind} is settled by scorer attribution and cannot grade on a synthetic cell");
            if (!MatchModel.Grades(matchup, cell, leg)) return false;
        }
        return true;
    }

    /// <summary>
    /// The synthetic stat lines the joint grades against: one immutable cell per grid coordinate,
    /// built once per grid size and shared.
    ///
    /// <para>They depend on the GRID, never on the matchup — cell (2,1) of the goal grid is 2-1
    /// whatever the latents are — so caching them is a memo of immutable values, not engine state a
    /// preceding workload could perturb. It is here because the gates evaluate the joint hundreds of
    /// millions of times: the corner grid alone is 21x21 cells per evaluation, and allocating those
    /// per call dominated everything else the model does.</para></summary>
    private static class Cells
    {
        public static readonly MatchStatLine[] None = Array.Empty<MatchStatLine>();

        private static readonly ConcurrentDictionary<(int family, int home, int away), MatchStatLine[]> Grids
            = new ConcurrentDictionary<(int, int, int), MatchStatLine[]>();

        /// <summary>The scoreline grid, indexed <c>h * (max + 1) + a</c>.</summary>
        public static MatchStatLine[] Goals(int maxGoals) => Grid(SelectionFamily.Goal, maxGoals, maxGoals);

        /// <summary>A counting grid, indexed <c>h * (maxAway + 1) + a</c>.</summary>
        public static MatchStatLine[] Counts(SelectionFamily family, int maxHome, int maxAway)
            => Grid(family, maxHome, maxAway);

        private static MatchStatLine[] Grid(SelectionFamily family, int maxHome, int maxAway)
            => Grids.GetOrAdd(((int)family, maxHome, maxAway), key =>
            {
                var (f, mh, ma) = key;
                var cells = new MatchStatLine[(mh + 1) * (ma + 1)];
                for (int h = 0; h <= mh; h++)
                    for (int a = 0; a <= ma; a++)
                        cells[h * (ma + 1) + a] = (SelectionFamily)f switch
                        {
                            SelectionFamily.Goal => new MatchStatLine(h, a, 0, 0, 0, 0),
                            SelectionFamily.Corner => new MatchStatLine(0, 0, h, a, 0, 0),
                            SelectionFamily.Card => new MatchStatLine(0, 0, 0, 0, h, a),
                            _ => throw new ArgumentOutOfRangeException(nameof(family)),
                        };
                return cells;
            });
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
        {
            // SAME FAMILY IS NOT THE SAME DRAW, since F_0.5.0's team totals. A count family holds two
            // independent draws (home count, away count); HOME corners beside AWAY corners share the
            // family and nothing else, and their joint is the product to the last bits. Labelling that
            // SharedCount would state "one makes the other likelier" about a pair where it does not —
            // the model asserting a correlation it has just measured as absent. It is Independent,
            // which is what the vocabulary's own sentence for that row says ("unrelated — no
            // adjustment"); canon's gloss on that row reads "different families" and is now narrower
            // than the board, flagged for the Design Director rather than fixed here.
            (bool hi, bool ai) = CountSidesOf(selections[i]);
            (bool hj, bool aj) = CountSidesOf(selections[j]);
            return (hi && hj) || (ai && aj)
                ? new Relation(RelationKind.SharedCount, legs, sign, fi, null, strength, correlated)
                : new Relation(RelationKind.Independent, legs, RelationSign.None, null, null,
                    strength, correlated);
        }

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
        // 2+ is a scorer leg for this purpose exactly as 1+ is — the same attributed goals settle it.
        bool scorerA = IsPlayerMarket(a.Kind);
        bool scorerB = IsPlayerMarket(b.Kind);
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

    /// <summary>One backed player and the number of goals the ticket needs from him: 1 for anytime,
    /// <c>Line</c> for a 2+ market. Two legs on one player collapse to the STRONGER threshold, which
    /// is what their intersection is.</summary>
    private struct ScorerLeg
    {
        public int PlayerIndex;
        public int Needs;
    }

    private sealed class Split
    {
        public readonly List<MarketSelection> GoalPredicates = new List<MarketSelection>();
        public readonly List<ScorerLeg> HomeScorers = new List<ScorerLeg>();
        public readonly List<ScorerLeg> AwayScorers = new List<ScorerLeg>();
        public readonly List<MarketSelection> Corner = new List<MarketSelection>();
        public readonly List<MarketSelection> Card = new List<MarketSelection>();

        public bool HasGoal => GoalPredicates.Count > 0 || HomeScorers.Count > 0 || AwayScorers.Count > 0;
    }

    /// <summary>Scorer legs carry matchup-board indices (away roster first, then home), so the side
    /// split needs the matchup. Repeated scorer legs on one player are merged here at the HIGHEST
    /// threshold, because "scores 1+ AND scores 2+" is "scores 2+": backing one player twice is one
    /// event. Left unmerged it would inflate the required goal count and the shortfall guard would
    /// call a perfectly ordinary ticket impossible.</summary>
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
                    if (IsPlayerMarket(selection.Kind))
                        AddScorer(matchup, split, selection);
                    else
                        split.GoalPredicates.Add(selection);
                    break;
            }
        }
        return split;
    }

    private static void AddScorer(Matchup matchup, Split split, MarketSelection selection)
    {
        if (selection.Choice != MarketChoice.Yes)
            throw new ArgumentException($"{selection.Kind} is a YES-only market");

        int needs;
        if (selection.Kind == MarketKind.AnytimeScorer)
        {
            if (selection.Line != 0.0) throw new ArgumentException("Anytime scorer has no line");
            needs = 1;
        }
        else
        {
            needs = (int)selection.Line;
            if (needs < 2 || needs != selection.Line)
                throw new ArgumentException($"Multi-scorer needs a whole goal count of 2 or more, got {selection.Line}");
        }

        List<ScorerLeg> side = matchup.PlayerSide(selection.PlayerIndex) == Side.Home
            ? split.HomeScorers : split.AwayScorers;
        for (int i = 0; i < side.Count; i++)
            if (side[i].PlayerIndex == selection.PlayerIndex)
            {
                if (needs > side[i].Needs) side[i] = new ScorerLeg { PlayerIndex = selection.PlayerIndex, Needs = needs };
                return;
            }
        side.Add(new ScorerLeg { PlayerIndex = selection.PlayerIndex, Needs = needs });
    }

    private static (double[] weights, int[] needs) NormalizedWeights(Matchup matchup, Side side,
        List<ScorerLeg> backed)
    {
        IReadOnlyList<Player> roster = side == Side.Home ? matchup.Home.Players : matchup.Away.Players;
        double total = 0.0;
        foreach (Player player in roster) total += player.ScoringWeight;
        if (total <= 0.0) throw new InvalidOperationException("Scorer roster has no positive weights");

        var weights = new double[backed.Count];
        var needs = new int[backed.Count];
        for (int i = 0; i < backed.Count; i++)
        {
            weights[i] = matchup.PlayerAt(backed[i].PlayerIndex).ScoringWeight / total;
            needs[i] = backed[i].Needs;
        }
        return (weights, needs);
    }

    // ---------------------------------------------------------------------------------------
    // The outcome partition W.
    // ---------------------------------------------------------------------------------------

    /// <summary>
    /// W, the model's outcome partition: the <see cref="MatchResult"/> enum, in the order
    /// <c>MatchModel.Classes</c> yields it. The design doc's instruction is verbatim — "Write that sum
    /// over W, never over a hard-coded pair of branches" — and W is the engine's own three-valued
    /// result vocabulary, so this IS that sum, written in the engine's words.
    ///
    /// <para><b>The order is load-bearing.</b> <c>MatchModel.ScoreProbability</c> accumulates
    /// Home, Draw, Away; matching it is what makes a single-leg joint bit-identical to the engine's
    /// own price rather than agreeing to fifteen places. Reordering these silently changes the last
    /// bits of every same-match price.</para>
    ///
    /// <para><b>This replaced a reflection-based discovery of W</b> (F_0.6.0 Phase 1), which read the
    /// score lists off <see cref="MatchDistributions"/> by shape and their weights off a
    /// <c>Matchup.True&lt;Class&gt;Prob</c> naming convention, with one class left implicit as the
    /// residual. Lane 1 landed draws with three score lists and the weights exposed as a METHOD,
    /// <see cref="Matchup.TrueProb(MatchResult)"/>, so the convention found no weights, two classes
    /// went residual, and the type initializer threw — taking every same-match path with it. The
    /// explicit form has no naming convention to break, no residual to be ambiguous about, and reads
    /// the weights from the one accessor the engine actually publishes.</para></summary>
    private static readonly MatchResult[] Partition =
        { MatchResult.Home, MatchResult.Draw, MatchResult.Away };

    private static IReadOnlyList<MatchModel.ScoreOutcome> ScoresOf(MatchDistributions dist, MatchResult result)
        => result switch
        {
            MatchResult.Home => dist.HomeWinScores,
            MatchResult.Draw => dist.DrawScores,
            MatchResult.Away => dist.AwayWinScores,
            _ => throw new ArgumentOutOfRangeException(nameof(result)),
        };

    /// <summary>Absolute tolerance on "the outcome partition sums to 1". Three unconditional 1X2
    /// probabilities close to ~1e-16, so this is four orders of margin over the floating-point noise
    /// and still far tighter than any real partition defect.</summary>
    public const double PartitionTolerance = 1e-12;

    /// <summary>
    /// P(w) for every class of W, with the closure check that the partition is a partition.
    ///
    /// <para><b>The loud failure is deliberate and is the reason this file has survived two model
    /// changes.</b> A partition that no longer sums to 1 prices every goal-family ticket on the board
    /// wrong, by an amount no test asserts and no player can see. Refusing to price it is the correct
    /// answer; pricing it anyway is a silent, board-wide mispricing. What CHANGED after F_0.5.0 is
    /// where the failure lands: this throws per matchup, at pricing time, so a defect costs the
    /// same-match path rather than the type initializer taking the whole engine down with it.</para>
    /// </summary>
    private static double[] ClassWeights(Matchup matchup)
    {
        var weights = new double[Partition.Length];
        double sum = 0.0;
        for (int i = 0; i < Partition.Length; i++)
        {
            weights[i] = matchup.TrueProb(Partition[i]);
            sum += weights[i];
            if (weights[i] < -PartitionTolerance)
                throw new InvalidOperationException(
                    $"Outcome class {Partition[i]} carries negative weight {weights[i]:R} — "
                    + "the outcome partition is not a partition");
        }

        if (Math.Abs(sum - 1.0) > PartitionTolerance)
            throw new InvalidOperationException(
                $"The outcome partition sums to {sum:R}, not 1 (classes: {string.Join(", ", Partition)}). "
                + "Every goal-family price on this matchup would be wrong, so the model refuses to produce one.");
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

/// <summary>Which validity rule refused a ticket — the three of them
/// (<c>design/02-betting-math.md</c> § *Same-game tickets*, ruled 2026-08-12). Structured so a caller
/// branches on the RULE rather than matching an English string.</summary>
public enum RefusalKind
{
    /// <summary>p_ticket = 0. No outcome wins this combination, so it has no finite price, and a bet
    /// that cannot win must never be purchasable.</summary>
    ImpossibleCombination,

    /// <summary>One selection appears more than once. The joint is idempotent — the repeat adds no
    /// risk — while (1 + Ω)^n charges a full extra leg of margin for it.</summary>
    DuplicateSelection,

    /// <summary>The ticket prices at or below evens: pay one, win less than one.</summary>
    SubEvens,
}

/// <summary>
/// A refused combination, IN PARTS (S73-am4, <c>docs/design/surething-design.md</c> §3.3 — the owning
/// law; <c>design/02-betting-math.md</c> § *A refusal must emit cause AND remedy, structurally*).
///
/// <para>A refused combination is a <i>Blocked</i> state, and that row has always required both
/// halves: what cannot happen is the CAUSE, what to drop is the REMEDY. So a rejection is not an
/// exception string — it carries the offending leg set and a droppable leg that makes the ticket
/// valid. Same seam as <see cref="SameMatchPrice.Principal"/>: <b>the model emits structured data and
/// never an English sentence</b>; presentation composes the words and keeps copy authority.</para>
///
/// <para>There is deliberately no display text and no <c>ToString</c> override on this type.</para>
/// </summary>
public sealed class TicketRefusal
{
    public RefusalKind Kind { get; }

    /// <summary>THE CAUSE: the MINIMAL set of legs that conflict, as indices into the ticket's leg
    /// list, ascending. Minimal by construction rather than by convenience — a cause naming four legs
    /// where two conflict is a worse answer, so
    /// <see cref="RefusalKind.ImpossibleCombination"/> searches subsets smallest-first for the
    /// smallest one that still reaches zero. A duplicate names the repeated pair. A
    /// <see cref="RefusalKind.SubEvens"/> refusal names every leg, because the price is a property of
    /// the whole combination and no proper subset of it prices any worse.</summary>
    public IReadOnlyList<int> CauseLegs { get; }

    /// <summary>THE REMEDY: the legs to drop, as indices into the ticket's leg list, ascending.
    ///
    /// <para><b>Computed and verified, never guessed.</b> Every set here was checked by construction:
    /// the remaining legs were run back through all three validity rules and came out placeable. The
    /// search is smallest-first, so a one-leg remedy is emitted wherever one exists, and among equal
    /// sizes it prefers dropping the legs added LAST — the surface refuses the second pick, not the
    /// first.</para>
    ///
    /// <para><b>Usually one leg, and not always one.</b> Three or more repeats of a selection have no
    /// single-leg remedy at all: drop one and the rest are still a duplicate. Reporting a leg whose
    /// removal does not fix the ticket would be a remedy that lies, so the type carries a SET and the
    /// common case is a set of one.</para>
    ///
    /// <para>Empty only if no removal whatever yields a placeable ticket. Not reachable on the
    /// shipped board — dropping to a single leg always is placeable, since a lone leg takes the
    /// ordinary path and the board guarantees it prices above evens — and left representable rather
    /// than asserted away, so an engine that ever produces one says so instead of naming a leg that
    /// does not help.</para></summary>
    public IReadOnlyList<int> RemedyLegs { get; }

    /// <summary>The model's own label for the conflict, when one covers exactly
    /// <see cref="CauseLegs"/> — a <see cref="RelationKind.MutuallyExclusive"/> for an impossible
    /// combination, the self-<see cref="RelationKind.Implies"/> for a duplicate. Null when the cause
    /// spans a set the relation vocabulary does not name as a unit.</summary>
    public Relation? CauseRelation { get; }

    /// <summary>The price the ticket would have carried, boost included — the figure a
    /// <see cref="RefusalKind.SubEvens"/> refusal is ABOUT. Exactly 0.0 for an impossible
    /// combination, which has no price at all.</summary>
    public double Price { get; }

    /// <summary>Whether any removal fixes this ticket. See <see cref="RemedyLegs"/>.</summary>
    public bool HasRemedy => RemedyLegs.Count > 0;

    public TicketRefusal(RefusalKind kind, IReadOnlyList<int> causeLegs, IReadOnlyList<int> remedyLegs,
        Relation? causeRelation, double price)
    {
        Kind = kind;
        CauseLegs = causeLegs ?? throw new ArgumentNullException(nameof(causeLegs));
        RemedyLegs = remedyLegs ?? throw new ArgumentNullException(nameof(remedyLegs));
        CauseRelation = causeRelation;
        Price = price;
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

    /// <summary>
    /// The exact joint probability that EVERY leg in <paramref name="legs"/> wins: the product, over
    /// matchups in first-appearance order, of each matchup group's enumerated joint. Legs on different
    /// matchups are independent, which is what makes the product legitimate — the same factorisation
    /// <see cref="Price"/> uses, through the same grouping, in the same order.
    ///
    /// <para><b>Why this is public.</b> A locked price is not the only question asked of the joint. The
    /// sweat's cash-out needs <c>P(the rest win | the settled legs won)</c>, and under the sweat's
    /// one-leg-at-a-time resolution that conditional is exactly a RATIO OF TWO OF THESE — no new sample
    /// space and no new machinery, which is the whole reason F_0.6.0 Phase 4 is engine-local. This is
    /// the numerator and the denominator both; <see cref="SweatSession"/> divides.</para>
    ///
    /// <para><b>The empty set is 1.0</b> — the empty conjunction, and the value that makes an unstarted
    /// sweat's conditional collapse to the unconditional joint exactly rather than nearly.</para>
    ///
    /// <para>A one-leg group returns that selection's marginal. That marginal agrees with
    /// <c>MatchModel</c>'s own price to ~1.5e-14 relative — **not** to the bit; see
    /// <see cref="JointModel"/>'s goal-family note for the per-kind ulp table. Both sides of a
    /// conditional must therefore come from THIS evaluator: reading the denominator off
    /// <see cref="Leg.TrueProb"/> instead would mix two arithmetics and the ratio would stop being a
    /// conditional.</para>
    ///
    /// <para>Voided legs are NOT filtered here: which legs are on the ticket is the caller's question,
    /// not the model's. The caller passes the set it means.</para></summary>
    public static double JointProbabilityOf(IReadOnlyList<Leg> legs)
    {
        if (legs == null) throw new ArgumentNullException(nameof(legs));
        if (legs.Count == 0) return 1.0;

        (List<Matchup> matchups, List<List<int>> groups) = GroupByMatchup(legs);

        double pJoint = 1.0;
        for (int g = 0; g < groups.Count; g++)
        {
            List<int> members = groups[g];
            var selections = new MarketSelection[members.Count];
            for (int i = 0; i < members.Count; i++) selections[i] = legs[members[i]].Selection;
            // Probability, not JointProbability: this is a quote, not a slip. See Probability's own
            // note — asking for the labelled form here and discarding the labels is what made the
            // cash-out path an order of magnitude more expensive than the number it needed.
            pJoint *= JointModel.Probability(matchups[g], selections);
        }
        return pJoint;
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

        (List<Matchup> matchups, List<List<int>> groups) = GroupByMatchup(legs);

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

    /// <summary>First-appearance grouping of a leg list by matchup identity, returning the matchups in
    /// the order they first appear and, for each, the leg indices it holds.
    ///
    /// <para>Deliberately not a hash-ordered group-by: the multiplication order — and therefore the
    /// last bits of every price derived from it — must be a function of the ticket alone, reproducible
    /// from a seed on any run. Shared by <see cref="PriceCore"/> and
    /// <see cref="JointProbabilityOf"/> so a locked price and a cash-out conditional can never disagree
    /// about which legs share a match or in which order their joints multiply.</para></summary>
    /// <summary>Legs partitioned by matchup, matchups in FIRST-APPEARANCE order over the list. Internal
    /// rather than private since <c>T140</c> arm A: <see cref="DramaGenerator"/> assembles one telling
    /// per (ticket, fixture) and must group through THIS helper, in THIS order, or the sweat's idea of
    /// a fixture and the joint price's would be two implementations of one rule.</summary>
    internal static (List<Matchup> matchups, List<List<int>> groups) GroupByMatchup(IReadOnlyList<Leg> legs)
    {
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
        return (matchups, groups);
    }

    // ================================================================================ refusal
    // The three validity rules, and the structured verdict they produce (S73-am4, owned by
    // docs/design/surething-design.md §3.3; design/02-betting-math.md § *A refusal must emit cause AND
    // remedy, structurally*). The model supplies the parts; the placing layer decides how to raise
    // them and presentation composes the words.

    /// <summary>
    /// The validity verdict on a ticket: null if it may be sold, otherwise the refusal IN PARTS —
    /// which legs conflict, and which to drop.
    ///
    /// <para><b>The three rules, in canon order.</b> A duplicate selection is refused as a duplicate
    /// even when it would also price under evens: design/02 is explicit that "the second does not
    /// subsume the first", since two repeats of a LONG leg clear evens comfortably and are still a
    /// pure ripoff. Then the impossible combination, then the sub-evens backstop.</para>
    ///
    /// <para><b>A ticket with at most one leg per matchup leaves on the first line, untouched.</b>
    /// None of the three rules can reach it — a duplicate needs one matchup twice, and the other two
    /// judge a joint price that such a ticket does not have — so it exits before a single joint is
    /// enumerated and its price, payout, void and settlement stay bit-identical.</para>
    ///
    /// <para><b>Does not count a no-label fallback.</b> <see cref="NoLabelFallbacks"/> means tickets
    /// SOLD at the naive product, and a refused ticket is never sold; the counting call in
    /// <c>Run.PlaceTicket</c> runs after this verdict clears.</para>
    /// </summary>
    /// <param name="legs">The ticket's legs, in ticket order.</param>
    /// <param name="overround">Ω, the per-leg margin.</param>
    /// <param name="margin">κ, the SAME MATCH margin dial.</param>
    /// <param name="boost">The multiplier a leg-targeted Profit Boost puts on the ticket price, or
    /// 1.0 for none. It scales the price the sub-evens rule judges, so a boosted ticket that clears
    /// evens is not refused for failing to.</param>
    /// <param name="boostLeg">Which leg carries that boost, or −1. A remedy that drops the boosted
    /// leg drops the boost with it, exactly as it does on the void path.</param>
    public static TicketRefusal? Refuse(IReadOnlyList<Leg> legs, double overround, double margin,
        double boost = 1.0, int boostLeg = -1)
    {
        if (legs == null) throw new ArgumentNullException(nameof(legs));
        if (legs.Count == 0) throw new ArgumentException("A ticket needs at least one leg", nameof(legs));

        // THE INVARIANT'S GUARD — see the summary. Not an optimization.
        if (!IsSameMatch(legs)) return null;

        if (legs.Count > MaxSubsetLegs)
            throw new ArgumentException(
                $"A refusal search over {legs.Count} legs walks {1L << legs.Count} subsets; the model "
                + $"searches up to {MaxSubsetLegs} legs (RunConfig.MaxLegs ships at 4)", nameof(legs));

        int n = legs.Count;
        int full = (1 << n) - 1;

        SameMatchPrice priced = PriceCore(legs, overround, margin, countFallback: false);
        double price = priced.Impossible ? 0.0 : priced.Price * BoostOn(full, boost, boostLeg);

        RefusalKind kind;
        int[] cause;

        int repeatOf = FirstRepeat(legs, full, out int repeat);
        if (repeatOf >= 0)
        {
            // The cause is the repeated selection, the remedy the repeat — but the remedy is still
            // SEARCHED rather than assumed to be `repeat`, because three repeats of one selection are
            // not fixed by dropping one of them.
            kind = RefusalKind.DuplicateSelection;
            cause = new[] { repeatOf, repeat };
        }
        else if (priced.Impossible)
        {
            kind = RefusalKind.ImpossibleCombination;
            cause = MinimalConflict(legs, overround, margin);
        }
        else if (!priced.NaiveFallback && price <= 1.0)
        {
            // Every leg, and that is minimal here: dropping a leg raises the joint (weakly) and drops
            // a factor of (1 + Ω) off the margin, so no proper subset of a sub-evens ticket is itself
            // sub-evens for a reason this one is not. The whole combination IS the cause.
            kind = RefusalKind.SubEvens;
            cause = Indices(full);
        }
        else return null;

        return new TicketRefusal(kind, cause,
            MinimalRemedy(legs, overround, margin, boost, boostLeg),
            RelationOver(priced.Relations, cause), price);
    }

    /// <summary>The first repeated selection in <paramref name="mask"/>, scanning in ticket order:
    /// returns the index it repeats and sets <paramref name="repeat"/> to the repeat itself, or −1.</summary>
    private static int FirstRepeat(IReadOnlyList<Leg> legs, int mask, out int repeat)
    {
        for (int i = 0; i < legs.Count; i++)
        {
            if ((mask & (1 << i)) == 0) continue;
            for (int j = i + 1; j < legs.Count; j++)
            {
                if ((mask & (1 << j)) == 0) continue;
                if (ReferenceEquals(legs[i].Matchup, legs[j].Matchup)
                    && legs[i].Selection == legs[j].Selection)
                {
                    repeat = j;
                    return i;
                }
            }
        }
        repeat = -1;
        return -1;
    }

    /// <summary>The smallest set of legs that already reaches p = 0 — the minimal cause. Searched
    /// smallest-first, so a two-leg conflict inside a four-leg ticket is named as two legs; a triple
    /// that reaches zero only jointly (recon §6.2's 57 shapes, every sub-pair positive) is named as
    /// all three, because there is no smaller true answer.
    ///
    /// <para>Ties inside a size are broken by taking the HIGHEST subset mask, which prefers the legs
    /// added last — the same preference the remedy uses, so cause and remedy read as one verdict on a
    /// ticket carrying two independent conflicts.</para></summary>
    private static int[] MinimalConflict(IReadOnlyList<Leg> legs, double overround, double margin)
    {
        int n = legs.Count, full = (1 << n) - 1;
        for (int size = 1; size <= n; size++)
            for (int mask = full; mask >= 1; mask--)
            {
                if (PopCount(mask) != size) continue;
                if (PriceCore(Survivors(legs, mask), overround, margin, countFallback: false).PTicket == 0.0)
                    return Indices(mask);
            }
        return Indices(full); // unreachable: only asked when the whole ticket is already zero
    }

    /// <summary>The smallest set of legs whose removal leaves a ticket that ACTUALLY PLACES —
    /// verified against all three rules, not inferred from the cause. Searched smallest-first so a
    /// one-leg remedy wins wherever one exists; ties inside a size prefer dropping the legs added
    /// last.
    ///
    /// <para>Empty only if nothing works, which the shipped board cannot produce: the search reaches
    /// the one-leg remainders, and a lone leg is an ordinary ticket priced above evens by the board's
    /// own guard.</para></summary>
    private static int[] MinimalRemedy(IReadOnlyList<Leg> legs, double overround, double margin,
        double boost, int boostLeg)
    {
        int n = legs.Count, full = (1 << n) - 1;
        for (int size = 1; size < n; size++)
            for (int drop = full; drop >= 1; drop--)
            {
                if (PopCount(drop) != size) continue;
                if (Placeable(legs, full & ~drop, overround, margin, boost, boostLeg))
                    return Indices(drop);
            }
        return Array.Empty<int>();
    }

    /// <summary>Whether the legs in <paramref name="keepMask"/> would survive all three validity
    /// rules — the same tests <see cref="Refuse"/> applies, on the same prices, which is what makes a
    /// remedy verified rather than guessed.</summary>
    private static bool Placeable(IReadOnlyList<Leg> legs, int keepMask, double overround, double margin,
        double boost, int boostLeg)
    {
        Leg[] kept = Survivors(legs, keepMask);
        if (kept.Length == 0) return false; // a ticket takes at least one leg

        // An ordinary remainder takes the untouched product path, where none of the three rules
        // exists: no matchup twice, so no duplicate and no joint to judge. Its price is a product of
        // board singles, each above evens by MatchModel.Offer's own guard.
        if (!IsSameMatch(kept)) return true;

        if (FirstRepeat(kept, (1 << kept.Length) - 1, out int _) >= 0) return false;

        SameMatchPrice core = PriceCore(kept, overround, margin, countFallback: false);
        if (core.Impossible) return false;
        return core.NaiveFallback || core.Price * BoostOn(keepMask, boost, boostLeg) > 1.0;
    }

    /// <summary>A leg-targeted Profit Boost scales the ticket price only while its leg is on the
    /// ticket — the same rule the survivor-subset table applies on a void.</summary>
    private static double BoostOn(int mask, double boost, int boostLeg)
        => boostLeg >= 0 && (mask & (1 << boostLeg)) != 0 ? boost : 1.0;

    /// <summary>The model's own label for a set of legs, when one relation covers exactly that set.
    /// Order-insensitive: a cause is a set, while <see cref="Relation.Legs"/> is ordered where
    /// direction carries meaning.</summary>
    private static Relation? RelationOver(IReadOnlyList<Relation> relations, int[] cause)
    {
        for (int r = 0; r < relations.Count; r++)
        {
            IReadOnlyList<int> on = relations[r].Legs;
            if (on.Count != cause.Length) continue;

            bool same = true;
            for (int i = 0; i < cause.Length && same; i++)
            {
                same = false;
                for (int j = 0; j < on.Count; j++) if (on[j] == cause[i]) { same = true; break; }
            }
            if (same) return relations[r];
        }
        return null;
    }

    private static int PopCount(int mask)
    {
        int count = 0;
        for (; mask != 0; mask &= mask - 1) count++;
        return count;
    }

    /// <summary>A bitmask as ascending leg indices — the shape every part of a
    /// <see cref="TicketRefusal"/> is published in.</summary>
    private static int[] Indices(int mask)
    {
        var indices = new int[PopCount(mask)];
        for (int i = 0, k = 0; (mask >> i) != 0; i++)
            if ((mask & (1 << i)) != 0) indices[k++] = i;
        return indices;
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
