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

/// <summary>A ticket's terminal state. <c>Voided</c> (F_0.6.0) is neither a win nor a loss: a void
/// re-priced the ticket to at or below evens, so the ticket voids IN FULL and the stake is returned
/// unconditionally (<c>design/02-betting-math.md</c> § *Void: re-price on the survivors*, CORRECTED
/// 2026-08-12). It is a distinct state rather than a flavour of Lost because every consumer that
/// counts losses — the Bad Beat Jar, the Scar, the run's own tally — would otherwise book a refund as
/// a bust.</summary>
public enum TicketState { Open, Won, Lost, CashedOut, Voided }

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

    /// <summary>The STAKE-RETURN latch — exactly-once, set by the terminal-realization ledger. Two
    /// things reach it: a Lost Free Bet ticket, and (F_0.6.0) a ticket that
    /// <see cref="VoidedInFull"/>. One latch rather than two because they must never both pay: the
    /// stake comes back once or not at all, and a ticket that voids in full is never
    /// <see cref="TicketState.Lost"/>, so a Free Bet on it refunds the same single stake.</summary>
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
    /// The CONTRACT price this ticket re-prices to for EVERY survivor subset
    /// (<c>design/02-betting-math.md</c> § *Void: re-price on the survivors*), indexed by survivor
    /// bitmask: bit <c>i</c> set means leg <c>i</c> survives. Locked at placement and never re-derived
    /// at settlement, which is what keeps settlement deterministic and independent of when — and how
    /// often — a void is discovered.
    ///
    /// <para>This is <see cref="SameMatchPrice.SubsetPrices"/> after the placing layer's own
    /// adjustments — the same relationship <see cref="LockedPrice"/> has to
    /// <see cref="SameMatchPrice.Price"/> — so it is the number to settle on, not the model's raw
    /// figure.</para>
    ///
    /// <para>EMPTY on an ordinary ticket, and that emptiness is load-bearing: such a ticket has no
    /// locked price to replace. It re-multiplies its surviving legs at read time exactly as it did
    /// before F_0.6.0, for any number of voids.</para></summary>
    public IReadOnlyList<double> LockedSubsetPrices { get; }

    /// <summary>The single-void row of <see cref="LockedSubsetPrices"/> — <c>LockedVoidPrices[v]</c> is
    /// the contract price when leg <c>v</c>, and only leg <c>v</c>, voids. A view over the subset
    /// table, so the two can never disagree.</summary>
    public IReadOnlyList<double> LockedVoidPrices { get; }

    public Ticket(IReadOnlyList<Leg> legs, double stake, double vigPaid, double lockedPrice,
        SameMatchPrice? sameMatch = null, IReadOnlyList<double>? lockedSubsetPrices = null)
    {
        Legs = legs;
        Stake = stake;
        VigPaid = vigPaid;
        LockedPrice = lockedPrice;
        SameMatch = sameMatch;
        LockedSubsetPrices = lockedSubsetPrices ?? Array.Empty<double>();
        LockedVoidPrices = SameMatchPrice.SingleVoidRow(LockedSubsetPrices);
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
    /// <see cref="SameMatchContractPrice"/>.</para>
    ///
    /// <para><b>Zero once the ticket <see cref="VoidedInFull"/>.</b> Such a ticket has no payout at
    /// all: it returns the stake, and a refund is not a payout (canon 2026-08-12), so it must not be
    /// reachable through this expression — where <see cref="PayoutMultiplier"/> and Double or Nothing
    /// would act on it and turn a void into a profit. The stake comes back down the run's stake-return
    /// ledger instead, raw, and a zero here is the fail-safe: any caller that still credited this would
    /// credit nothing rather than a phantom sub-evens payout.</para></summary>
    public double PotentialPayout => VoidedInFull
        ? 0.0
        : SameMatch == null
            ? Stake * OddsMath.ParlayDecimal(ActiveLegs.Select(l => l.OfferedOdds).ToList()) * PayoutMultiplier
            : Stake * SameMatchContractPrice * PayoutMultiplier;

    /// <summary>The SURVIVOR bitmask over <see cref="Legs"/>: bit <c>i</c> set means leg <c>i</c> is
    /// not voided. The index into <see cref="LockedSubsetPrices"/>.</summary>
    private int SurvivorMask()
    {
        int mask = 0;
        for (int i = 0; i < Legs.Count; i++)
            if (!Legs[i].IsVoided) mask |= 1 << i;
        return mask;
    }

    /// <summary>
    /// A void has re-priced this ticket to AT OR BELOW EVENS, so the ticket voids IN FULL and the stake
    /// is returned unconditionally (<c>design/02-betting-math.md</c> § *Void: re-price on the
    /// survivors*, CORRECTED 2026-08-12).
    ///
    /// <para><b>This replaces the price floor, it does not sit beside it.</b> The superseded rule
    /// floored the contract price at 1.0 and justified that as the outcome the full-void camp of real
    /// books produces — which it is not. A live ticket priced at 1.0 returns the stake only IF IT WINS
    /// and still loses everything if it does not: strictly worse for the player than the full void it
    /// claimed to imitate, and the absurd contract <i>win and receive nothing</i>. The outcome no
    /// longer depends on whether the surviving legs win.</para>
    ///
    /// <para>Reachable for real. Placement refuses a sub-evens ticket, but a void re-prices a ticket
    /// that is ALREADY SOLD and refusal is unavailable by then; the tightest replacement on the shipped
    /// board is ~1.118 at κ = 1, so any κ past that — inside the range the gate campaign explores —
    /// produces one.</para>
    ///
    /// <para>Read off <see cref="LockedSubsetPrices"/> rather than the model's raw table, so a Profit
    /// Boost that travelled with a surviving leg counts: what matters is the price the ticket would
    /// actually settle on. Structurally false for an ordinary ticket (no locked replacement exists) and
    /// for an unvoided one (placement already refused it if it were sub-evens), which is what keeps the
    /// at-most-one-leg-per-matchup path untouched.</para></summary>
    public bool VoidedInFull
    {
        get
        {
            if (SameMatch == null) return false;
            int survivors = SurvivorMask();
            if (survivors == (1 << Legs.Count) - 1) return false; // nothing voided
            if (survivors == 0 || survivors >= LockedSubsetPrices.Count) return false;
            return LockedSubsetPrices[survivors] <= 1.0;
        }
    }

    /// <summary>
    /// A SAME MATCH ticket's contract price given the voids that have actually happened
    /// (<c>design/02-betting-math.md</c> § *Void: re-price on the survivors*). Dropping the voided
    /// leg's factor out of a product is wrong under a joint price and is what no real book does; the
    /// ticket re-prices against the SURVIVORS' joint, at the figure locked for that scenario when the
    /// ticket locked.
    ///
    /// <para><b>Any number of voids (canon CLOSED 2026-08-12).</b> The survivors are a bitmask and the
    /// replacement is a table lookup, so a second and third void are the same operation as the first —
    /// no rule is composed at settlement, and every price this ticket can ever show was locked at
    /// placement.</para>
    ///
    /// <para><b>No floor — the sub-evens case is not priced at all.</b> The superseded rule floored
    /// this at 1.0; canon CORRECTED that 2026-08-12, because a live ticket priced at 1.0 returns the
    /// stake only if it WINS. A replacement at or below evens now voids the ticket in full and returns
    /// the stake unconditionally: see <see cref="VoidedInFull"/>. This property therefore reports the
    /// raw locked replacement even when that is at or below evens — it is the figure that DECIDES the
    /// void, and no money is ever computed from it, because a ticket that voids in full has a
    /// <see cref="PotentialPayout"/> of zero and never settles Won or Lost.</para>
    ///
    /// <para>The no-void case returns <see cref="LockedPrice"/> directly rather than through the table.
    /// The full-mask entry holds the same number to the bit; reading the field is simply the shorter
    /// proof that an unvoided ticket's price is untouched by any of this.</para></summary>
    public double SameMatchContractPrice
    {
        get
        {
            if (SameMatch == null)
                throw new InvalidOperationException(
                    "This ticket has no SAME MATCH price; an ordinary ticket re-multiplies its active legs");

            int survivors = SurvivorMask();

            if (survivors == (1 << Legs.Count) - 1) return LockedPrice; // nothing voided

            if (survivors == 0)
                throw new InvalidOperationException(
                    "Every leg of this SAME MATCH ticket is voided; there is no event left to price "
                    + "(a Mulligan needs two active legs, so the engine never voids the last one)");

            if (survivors >= LockedSubsetPrices.Count)
                throw new InvalidOperationException(
                    "No void-replacement price was locked for this ticket — it cannot re-price on a void");

            double replacement = LockedSubsetPrices[survivors];
            if (!(replacement > 0.0) || double.IsInfinity(replacement))
                throw new InvalidOperationException(
                    $"The locked replacement price for survivor set 0x{survivors:X} is {replacement:R}, "
                    + "not a price");

            return replacement;
        }
    }
}
