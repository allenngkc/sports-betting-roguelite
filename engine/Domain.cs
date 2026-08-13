using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

public enum Side { Home, Away }

public enum MarketKind
{
    Moneyline,
    TotalGoals,
    BothTeamsToScore,
    TotalCorners,
    TotalCards,
    AnytimeScorer,
}

/// <summary>The two-way choice vocabulary is kept separate from team side because counting
/// markets use Over/Under and BTTS uses Yes/No.</summary>
public enum MarketChoice { Home, Away, Over, Under, Yes, No }

public enum LegState { Pending, Won, Lost }

public enum TicketState { Open, Won, Lost, CashedOut }

/// <summary>A ticket's final, ticket-local grading of one leg — what OnLegResolved carries
/// after any pending-loss window has closed (charm expansion, PLAN.md rev 5).</summary>
public enum LegGrade { Won, Lost, Voided }

/// <summary>Locked contract modifiers (charm expansion): at most ONE per ticket — the
/// one-modifier law, mirror of the one-product-slot law. Locked at placement, part of the
/// ticket's outcome→cash-flow contract (they price into cash-outs and G4).</summary>
public enum TicketModifier { None, FreeBet, DoubleOrNothing }

public readonly struct MarketSelection : IEquatable<MarketSelection>
{
    public MarketKind Kind { get; }
    public double Line { get; }
    public MarketChoice Choice { get; }
    public int PlayerIndex { get; }

    public MarketSelection(MarketKind kind, double line, MarketChoice choice, int playerIndex = -1)
    {
        Kind = kind;
        Line = line;
        Choice = choice;
        PlayerIndex = playerIndex;
    }

    public MarketSelection(MarketKind kind, double line, Side side)
        : this(kind, line, kind == MarketKind.Moneyline
            ? (side == Side.Home ? MarketChoice.Home : MarketChoice.Away)
            : throw new ArgumentException("Side choices are only valid for moneyline")) { }

    public MarketSelection(MarketKind kind, Side side)
        : this(kind, 0.0, side) { }

    public MarketSelection(MarketKind kind, double line, bool over)
        : this(kind, line, over ? MarketChoice.Over : MarketChoice.Under) { }

    public MarketSelection(MarketKind kind, bool yes)
        : this(kind, 0.0, yes ? MarketChoice.Yes : MarketChoice.No) { }

    public static MarketSelection Moneyline(Side side)
        => new MarketSelection(MarketKind.Moneyline, 0.0,
            side == Side.Home ? MarketChoice.Home : MarketChoice.Away);

    public static MarketSelection TotalGoals(double line, bool over)
        => new MarketSelection(MarketKind.TotalGoals, line, over ? MarketChoice.Over : MarketChoice.Under);

    public static MarketSelection BothTeamsToScore(bool yes)
        => new MarketSelection(MarketKind.BothTeamsToScore, 0.0, yes ? MarketChoice.Yes : MarketChoice.No);

    public static MarketSelection TotalCorners(double line, bool over)
        => new MarketSelection(MarketKind.TotalCorners, line, over ? MarketChoice.Over : MarketChoice.Under);

    public static MarketSelection TotalCards(double line, bool over)
        => new MarketSelection(MarketKind.TotalCards, line, over ? MarketChoice.Over : MarketChoice.Under);

    /// <summary>Player indices address the matchup's scorer board: away roster first, then home.</summary>
    public static MarketSelection AnytimeScorer(int playerIndex)
        => new MarketSelection(MarketKind.AnytimeScorer, 0.0, MarketChoice.Yes, playerIndex);

    public bool Equals(MarketSelection other)
        => Kind == other.Kind && Line.Equals(other.Line) && Choice == other.Choice
            && PlayerIndex == other.PlayerIndex;
    public override bool Equals(object? obj) => obj is MarketSelection other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)Kind, Line, (int)Choice, PlayerIndex);
    public static bool operator ==(MarketSelection left, MarketSelection right) => left.Equals(right);
    public static bool operator !=(MarketSelection left, MarketSelection right) => !left.Equals(right);
}

public sealed class MarketOffer
{
    public MarketSelection Selection { get; }
    public double TrueProb { get; }
    public double Odds { get; }

    public MarketOffer(MarketSelection selection, double trueProb, double odds)
    {
        Selection = selection;
        TrueProb = trueProb;
        Odds = odds;
    }
}

public readonly struct TeamStats
{
    public double GoalsFor { get; }
    public double Corners { get; }
    public double Cards { get; }

    public TeamStats(double goalsFor, double corners, double cards)
    {
        GoalsFor = goalsFor;
        Corners = corners;
        Cards = cards;
    }
}

public readonly struct MatchLatents
{
    public double HomeGoalRate { get; }
    public double AwayGoalRate { get; }
    public double HomeCornerRate { get; }
    public double AwayCornerRate { get; }
    public double HomeCardRate { get; }
    public double AwayCardRate { get; }

    public MatchLatents(double homeGoalRate, double awayGoalRate, double homeCornerRate,
        double awayCornerRate, double homeCardRate, double awayCardRate)
    {
        HomeGoalRate = homeGoalRate;
        AwayGoalRate = awayGoalRate;
        HomeCornerRate = homeCornerRate;
        AwayCornerRate = awayCornerRate;
        HomeCardRate = homeCardRate;
        AwayCardRate = awayCardRate;
    }
}

public sealed class MatchStatLine
{
    public int HomeGoals { get; }
    public int AwayGoals { get; }
    public int HomeCorners { get; }
    public int AwayCorners { get; }
    public int HomeCards { get; }
    public int AwayCards { get; }
    public IReadOnlyList<Player> HomeScorers { get; private set; } = Array.Empty<Player>();
    public IReadOnlyList<Player> AwayScorers { get; private set; } = Array.Empty<Player>();

    public Side Winner => HomeGoals > AwayGoals ? Side.Home : Side.Away;

    public MatchStatLine(int homeGoals, int awayGoals, int homeCorners, int awayCorners,
        int homeCards, int awayCards)
    {
        if (homeGoals == awayGoals) throw new ArgumentException("Soccer stat lines cannot draw in v1");
        HomeGoals = homeGoals;
        AwayGoals = awayGoals;
        HomeCorners = homeCorners;
        AwayCorners = awayCorners;
        HomeCards = homeCards;
        AwayCards = awayCards;
    }

    public MatchStatLine(int homeGoals, int awayGoals, int homeCorners, int awayCorners,
        int homeCards, int awayCards, IReadOnlyList<Player> homeScorers, IReadOnlyList<Player> awayScorers)
        : this(homeGoals, awayGoals, homeCorners, awayCorners, homeCards, awayCards)
        => SetScorers(homeScorers, awayScorers);

    internal void SetScorers(IReadOnlyList<Player> homeScorers, IReadOnlyList<Player> awayScorers)
    {
        if (homeScorers.Count != HomeGoals || awayScorers.Count != AwayGoals)
            throw new ArgumentException("Scorer attribution must contain exactly one player per goal");
        HomeScorers = homeScorers;
        AwayScorers = awayScorers;
    }
}

public enum PlayerRole { FW, MF, DF }

public sealed class Player
{
    public string Name { get; }
    public PlayerRole Role { get; }
    public double ScoringWeight { get; }

    public Player(string name, PlayerRole role, double scoringWeight)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Player name is required", nameof(name));
        if (scoringWeight <= 0.0) throw new ArgumentOutOfRangeException(nameof(scoringWeight));
        Name = name;
        Role = role;
        ScoringWeight = scoringWeight;
    }
}

public sealed class Team
{
    public string Name { get; }
    public int Wins { get; }
    public int Losses { get; }
    public string Record => $"{Wins}-{Losses}";
    public IReadOnlyList<Player> Players { get; }

    public Team(string name, int wins, int losses)
        : this(name, wins, losses, Array.Empty<Player>()) { }

    public Team(string name, int wins, int losses, IReadOnlyList<Player> players)
    {
        Name = name;
        Wins = wins;
        Losses = losses;
        Players = players ?? throw new ArgumentNullException(nameof(players));
    }
}

public sealed class Matchup
{
    public int Index { get; }
    public Team Home { get; }
    public Team Away { get; }
    public double TrueHomeProb { get; }
    public double HomeOdds { get; }
    public double AwayOdds { get; }

    public MatchLatents Latents { get; }
    public TeamStats HomeStats { get; }
    public TeamStats AwayStats { get; }
    public IReadOnlyList<MarketOffer> Markets { get; private set; }

    /// <summary>Derived from the locked stat line, shared by every leg referencing this matchup.</summary>
    public Side? Result => StatLine?.Winner;
    public MatchStatLine? StatLine { get; internal set; }
    /// <summary>The RunConfig this matchup's markets were priced under — public read access
    /// for the sim's honest estimators (they price from the same dials, never engine internals).</summary>
    public RunConfig ModelConfig { get; }

    private MatchDistributions? _dist;
    /// <summary>The matchup's exact finite distributions, built once and shared by pricing,
    /// grading, and the stat-line sampler — the sim locks millions of rounds, so the score/count
    /// enumerations must not be redone per offer (ARCHI §19).</summary>
    internal MatchDistributions Dist => _dist ??= MatchDistributions.Build(Latents, ModelConfig);

    public Matchup(int index, Team home, Team away, double trueHomeProb, double homeOdds, double awayOdds)
        : this(index, home, away, trueHomeProb, homeOdds, awayOdds,
            default, default, default, Array.Empty<MarketOffer>(), new RunConfig()) { }

    public Matchup(int index, Team home, Team away, double trueHomeProb, double homeOdds, double awayOdds,
        MatchLatents latents, TeamStats homeStats, TeamStats awayStats,
        IReadOnlyList<MarketOffer> markets, RunConfig modelConfig)
    {
        Index = index;
        Home = home;
        Away = away;
        TrueHomeProb = trueHomeProb;
        HomeOdds = homeOdds;
        AwayOdds = awayOdds;
        Latents = latents;
        HomeStats = homeStats;
        AwayStats = awayStats;
        Markets = markets;
        ModelConfig = modelConfig;
    }

    internal void SetMarkets(IReadOnlyList<MarketOffer> markets) => Markets = markets;

    public double TrueProb(Side side) => side == Side.Home ? TrueHomeProb : 1.0 - TrueHomeProb;
    public double Odds(Side side) => side == Side.Home ? HomeOdds : AwayOdds;
    public double FairOdds(Side side) => 1.0 / TrueProb(side);

    public double TrueProb(MarketSelection selection) => MatchModel.TrueProbability(this, selection);
    public double Odds(MarketSelection selection)
    {
        foreach (MarketOffer offer in Markets)
            if (offer.Selection == selection) return offer.Odds;
        throw new ArgumentException($"Market selection is not offered: {selection.Kind}");
    }
    public double FairOdds(MarketSelection selection) => 1.0 / TrueProb(selection);
    /// <summary>Scorer-board ordering is stable: away players then home players.</summary>
    public Player PlayerAt(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= Away.Players.Count + Home.Players.Count)
            throw new ArgumentOutOfRangeException(nameof(playerIndex));
        return playerIndex < Away.Players.Count ? Away.Players[playerIndex]
            : Home.Players[playerIndex - Away.Players.Count];
    }
    public Side PlayerSide(int playerIndex)
    {
        PlayerAt(playerIndex); // validates
        return playerIndex < Away.Players.Count ? Side.Away : Side.Home;
    }
    public bool Grades(MarketSelection selection)
    {
        if (StatLine == null) return false;
        return MatchModel.Grades(this, StatLine, selection);
    }
}

public sealed class Slate
{
    public int Round { get; }
    public IReadOnlyList<Matchup> Matchups { get; }

    public Slate(int round, IReadOnlyList<Matchup> matchups)
    {
        Round = round;
        Matchups = matchups;
    }
}

public readonly struct Pick
{
    public int MatchupIndex { get; }
    public MarketSelection Selection { get; }

    /// <summary>The picked team — MONEYLINE ONLY. Throws for market selections so a
    /// counting-market pick can never be silently misread as a team side.</summary>
    public Side Side => Selection.Kind == MarketKind.Moneyline
        ? (Selection.Choice == MarketChoice.Home ? Side.Home : Side.Away)
        : throw new InvalidOperationException($"Pick.Side is undefined for {Selection.Kind}; use Selection");

    public Pick(int matchupIndex, Side side)
    {
        MatchupIndex = matchupIndex;
        Selection = MarketSelection.Moneyline(side);
    }

    public Pick(int matchupIndex, MarketSelection selection)
    {
        MatchupIndex = matchupIndex;
        Selection = selection;
    }
}

public sealed class Leg
{
    public Matchup Matchup { get; }
    public MarketSelection Selection { get; }

    /// <summary>The picked team — MONEYLINE ONLY. Throws for market selections so a
    /// counting-market leg can never be silently misread as a team side.</summary>
    public Side Side => Selection.Kind == MarketKind.Moneyline
        ? (Selection.Choice == MarketChoice.Home ? Side.Home : Side.Away)
        : throw new InvalidOperationException($"Leg.Side is undefined for {Selection.Kind}; use Selection");

    public string DisplayLabel => MatchModel.DisplayLabel(Matchup, Selection);

    /// <summary>The matchup's own price for the picked side — the odds before any relic touched them.</summary>
    public double BaseOdds { get; }

    /// <summary>The locked contract odds. Equals <see cref="BaseOdds"/> unless a compose-time relic
    /// (boosted_odds, promo_code) rewrote it before the ticket locked.</summary>
    public double OfferedOdds { get; internal set; }

    /// <summary>Set when a Mulligan Slip voids this leg after it reveals Lost: the leg is struck from
    /// the ticket (excluded from payout, win condition, and cash-out products).</summary>
    public bool IsVoided { get; internal set; }

    /// <summary>Ref's Whistle rescue (charm expansion): a successful grading re-roll marks THIS
    /// ticket's copy of the leg Won. Ticket-local by construction — Legs belong to one ticket;
    /// the shared <see cref="Matchup.Result"/> never bends (Lucky Charm precedent).</summary>
    public bool RescuedWon { get; internal set; }

    public Leg(Matchup matchup, Side side, double offeredOdds)
        : this(matchup, MarketSelection.Moneyline(side), offeredOdds) { }

    public Leg(Matchup matchup, MarketSelection selection, double offeredOdds)
    {
        Matchup = matchup;
        Selection = selection;
        OfferedOdds = offeredOdds;
        BaseOdds = offeredOdds;
    }

    public double TrueProb => Matchup.TrueProb(Selection);

    public LegState State =>
        Matchup.StatLine == null ? LegState.Pending
        : Matchup.Grades(Selection) ? LegState.Won : LegState.Lost;

    /// <summary>This ticket's grading of the leg. Voided legs never count as won; a whistle-rescued
    /// leg counts as won for this ticket only.</summary>
    public bool GradesWon => !IsVoided && (RescuedWon || State == LegState.Won);
}

public sealed class Ticket
{
    public IReadOnlyList<Leg> Legs { get; }
    public double Stake { get; }
    public double VigPaid { get; }
    public TicketState State { get; internal set; } = TicketState.Open;

    /// <summary>What this ticket actually returned when it was cashed out, retained so the run's
    /// settled record can print it (S36). Null in every other state.
    ///
    /// A cash-out is the ONE terminal state whose return cannot be re-derived after the fact:
    /// a win pays <see cref="PotentialPayout"/> and a loss pays nothing, but the cash-out figure
    /// is a live quote off the remaining legs' probabilities at one instant of one sweat, and that
    /// instant is gone the moment the session ends. Not retaining it meant the LEDGER could only
    /// print an honest absence for money the player had actually banked.</summary>
    public double? CashedOutFor { get; internal set; }

    /// <summary>Stable per-run identity: "round.placementIndex" — the DeriveRng key component
    /// (charm expansion, PLAN.md rev 5).</summary>
    public string Id { get; internal set; } = "";

    /// <summary>The named factor map behind <see cref="PayoutMultiplier"/> (charm expansion):
    /// each payout effect owns one named ×(1+x) factor ("multiplier", "scar", "photo", "whale",
    /// "collection", "chalk", "iron", "jar", "system", "housekey", "don"). Immutable after lock
    /// EXCEPT the designed toggles (photo drops when its last qualifying leg is voided).</summary>
    private readonly Dictionary<string, double> _factors = new Dictionary<string, double>();

    internal void SetFactor(string name, double value) => _factors[name] = value;
    internal void RemoveFactor(string name) => _factors.Remove(name);
    internal bool HasFactor(string name) => _factors.ContainsKey(name);

    /// <summary>Payout scale from relic effects — THE product slot (design/10 B2): the product
    /// of every named factor, so items stack multiplicatively. Scales the win payout and the
    /// cash-out fair value.</summary>
    public double PayoutMultiplier
    {
        get
        {
            double p = 1.0;
            foreach (double f in _factors.Values) p *= f;
            return p;
        }
    }

    /// <summary>The locked contract modifier (one per ticket — the one-modifier law): Free Bet
    /// refunds the stake on a loss; Double or Nothing doubles the product and suppresses
    /// cash-out offers.</summary>
    public TicketModifier Modifier { get; internal set; } = TicketModifier.None;

    /// <summary>Free Bet's exactly-once latch, set by the terminal-realization ledger.</summary>
    public bool Refunded { get; internal set; }

    /// <summary>Scar Tissue bookkeeping (design/10 B): the stacks this ticket's bust would add,
    /// baked at placement from its stake fraction; and whether this ticket carries (and on a win
    /// or cash-out, burns) the current stacks — the round's FIRST-placed ticket carries.</summary>
    internal double ScarStacksIfBust { get; set; }
    internal bool ScarCarrier { get; set; }

    /// <summary>The ticket's contract price in decimal odds, locked at placement — every promo and
    /// relic that rewrote the price before lock is already in it. For an ordinary ticket this is the
    /// placement-time product of the legs' offered odds; for a SAME MATCH ticket it is the joint
    /// price, which is NOT that product.</summary>
    public double LockedPrice { get; }

    /// <summary>The correlation model's output for this ticket, non-null exactly when some matchup on
    /// it carries two or more legs (F_0.6.0). Carries the joint probability, the relation labels and
    /// the one relation a slip states. Null on every ordinary ticket, and that null is what routes
    /// <see cref="PotentialPayout"/> down the untouched pre-F_0.6.0 path.</summary>
    public SameMatchPrice? SameMatch { get; }

    /// <summary>
    /// The CONTRACT price this ticket re-prices to if leg <c>v</c> — and only leg <c>v</c> — voids
    /// (<c>design/02-betting-math.md</c> § *Void: re-price on the survivors*). One entry per leg,
    /// locked at placement and never re-derived at settlement, which is what keeps settlement
    /// deterministic and independent of when a void is discovered.
    ///
    /// <para>This is <see cref="SameMatchPrice.VoidPrices"/> after the placing layer's own
    /// adjustments — the same relationship <see cref="LockedPrice"/> has to
    /// <see cref="SameMatchPrice.Price"/> — so it is the number to settle on, not the model's raw
    /// figure.</para>
    ///
    /// <para>EMPTY on an ordinary ticket, and that emptiness is load-bearing: such a ticket has no
    /// locked price to replace. It re-multiplies its surviving legs at read time exactly as it did
    /// before F_0.6.0.</para></summary>
    public IReadOnlyList<double> LockedVoidPrices { get; }

    public Ticket(IReadOnlyList<Leg> legs, double stake, double vigPaid, double lockedPrice,
        SameMatchPrice? sameMatch = null, IReadOnlyList<double>? lockedVoidPrices = null)
    {
        Legs = legs;
        Stake = stake;
        VigPaid = vigPaid;
        LockedPrice = lockedPrice;
        SameMatch = sameMatch;
        LockedVoidPrices = lockedVoidPrices ?? Array.Empty<double>();
    }

    /// <summary>Legs that still count toward the ticket: voided (mulligan'd) legs are excluded.</summary>
    public IEnumerable<Leg> ActiveLegs => Legs.Where(l => !l.IsVoided);

    /// <summary>A ticket wins when it has at least one active leg and every active leg grades Won.</summary>
    public bool GradesWon
    {
        get
        {
            bool any = false;
            foreach (Leg l in Legs)
            {
                if (l.IsVoided) continue;
                any = true;
                if (!l.GradesWon) return false;
            }
            return any;
        }
    }

    /// <summary>Payout on a win: stake × the ticket's price × any relic payout multiplier.
    ///
    /// <para><b>The ordinary path is preserved verbatim, and that is the whole safety story.</b> A
    /// ticket with at most one leg per matchup carries no <see cref="SameMatch"/> block and still
    /// multiplies its ACTIVE legs' offered odds at read time — the same expression in the same order,
    /// so a voided leg drops out and the number is bit-identical to before F_0.6.0. Routing it through
    /// <see cref="LockedPrice"/> instead would agree algebraically and could still differ in the last
    /// bits, and the golden seeds and the whole gate baseline sit downstream of those bits.</para>
    ///
    /// <para>A SAME MATCH ticket reads its locked joint price: the product of its legs' odds is not
    /// its price, so re-multiplying them would be wrong rather than merely imprecise. On a void it
    /// reads the replacement price locked for that scenario at placement — see
    /// <see cref="SameMatchContractPrice"/>.</para></summary>
    public double PotentialPayout => SameMatch == null
        ? Stake * OddsMath.ParlayDecimal(ActiveLegs.Select(l => l.OfferedOdds).ToList()) * PayoutMultiplier
        : Stake * SameMatchContractPrice * PayoutMultiplier;

    /// <summary>
    /// A SAME MATCH ticket's contract price given the voids that have actually happened
    /// (<c>design/02-betting-math.md</c> § *Void: re-price on the survivors*). Dropping the voided
    /// leg's factor out of a product is wrong under a joint price and is what no real book does; the
    /// ticket re-prices against the SURVIVORS' joint, at the figure locked for that scenario when the
    /// ticket locked.
    ///
    /// <para><b>Two or more voided legs throw.</b> Canon leaves multiple simultaneous voids OPEN —
    /// the one documented commercial mechanism covers a single void — so there is no rule to apply,
    /// and pricing something arbitrary here would be inventing one silently. Failing loudly is the
    /// designed behaviour, and <c>Run.PlayMulliganSlip</c> refuses the second void before it happens
    /// so this is a backstop rather than the primary guard.</para></summary>
    public double SameMatchContractPrice
    {
        get
        {
            if (SameMatch == null)
                throw new InvalidOperationException(
                    "This ticket has no SAME MATCH price; an ordinary ticket re-multiplies its active legs");

            int voided = -1;
            int count = 0;
            for (int i = 0; i < Legs.Count; i++)
                if (Legs[i].IsVoided) { voided = i; count++; }

            if (count == 0) return LockedPrice;
            if (count > 1)
                throw new NotSupportedException(
                    $"{count} legs of this SAME MATCH ticket are voided. Re-pricing covers a SINGLE void "
                    + "(design/02 § Void: re-price on the survivors — multiple simultaneous voids are OPEN); "
                    + "there is no rule to price this ticket.");

            if (voided >= LockedVoidPrices.Count)
                throw new InvalidOperationException(
                    "No void-replacement price was locked for this ticket — it cannot re-price on a void");

            double replacement = LockedVoidPrices[voided];
            if (!(replacement > 0.0) || double.IsInfinity(replacement))
                throw new InvalidOperationException(
                    $"The locked void-replacement price for leg {voided} is {replacement:R}, not a price");
            return replacement;
        }
    }
}
