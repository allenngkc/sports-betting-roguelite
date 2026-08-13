using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

/// <summary>Which TEAM — rosters, per-team corners and cards, scorer attribution. Deliberately
/// still two-valued after draws landed: a draw is not a team. What became three-valued is the
/// match RESULT (<see cref="MatchResult"/>), and keeping the two separate is what stopped the
/// draw ruling from touching every roster call site.</summary>
public enum Side { Home, Away }

/// <summary>How a match finished. Replaces the two-valued winner the no-draws-in-v1 constraint
/// allowed (Allen lifted it 2026-08-12). The engine always computed the draw mass — the truncated
/// score grid conditioned it away — so this renders a distribution the model already had.</summary>
public enum MatchResult { Home, Draw, Away }

public enum MarketKind
{
    Moneyline,
    TotalGoals,
    BothTeamsToScore,
    TotalCorners,
    TotalCards,
    AnytimeScorer,
}

/// <summary>The choice vocabulary is kept separate from team side because counting markets use
/// Over/Under and BTTS uses Yes/No. <c>Draw</c> is moneyline-only — the market is 1X2 now, and it
/// is the one non-two-way member of this enum, which is why every de-vig site had to stop asking
/// for "the opposite" and start asking for the whole sibling set.</summary>
public enum MarketChoice { Home, Away, Over, Under, Yes, No, Draw }

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

    /// <summary>The X of 1X2. Has no <see cref="Side"/> by construction — which is exactly why
    /// <see cref="Pick.Side"/> and <see cref="Leg.Side"/> throw on it rather than guessing.</summary>
    public static MarketSelection MoneylineDraw()
        => new MarketSelection(MarketKind.Moneyline, 0.0, MarketChoice.Draw);

    public static MarketSelection Moneyline(MatchResult result) => result switch
    {
        MatchResult.Home => Moneyline(Side.Home),
        MatchResult.Away => Moneyline(Side.Away),
        MatchResult.Draw => MoneylineDraw(),
        _ => throw new ArgumentOutOfRangeException(nameof(result)),
    };

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

    /// <summary>How the match finished. Was <c>Winner</c>, a two-valued <see cref="Side"/>, while
    /// the no-draws constraint held; a level score is a Draw, not a team.</summary>
    public MatchResult Result => HomeGoals > AwayGoals ? MatchResult.Home
        : HomeGoals < AwayGoals ? MatchResult.Away
        : MatchResult.Draw;

    public MatchStatLine(int homeGoals, int awayGoals, int homeCorners, int awayCorners,
        int homeCards, int awayCards)
    {
        // The "Soccer stat lines cannot draw in v1" throw stood here and WAS the constraint,
        // in code. Allen lifted it 2026-08-12. A level score is now representable, which is what
        // gives the correct-score grid its 0-0 and 1-1 and makes double chance a real market.
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

    /// <summary>**P(home wins | the match is decisive)** — NOT the moneyline probability.
    /// Its value and its tuning band are unchanged by the draw ruling, because before draws every
    /// match WAS decisive and so this already meant the conditional. The unconditional 1X2 prices
    /// are <see cref="TrueProb(MatchResult)"/>: home is <c>TrueHomeProb × (1 − DrawProb)</c>.
    /// Keeping the conditional as the generated dial is what let <c>MinTrueProb</c>/<c>MaxTrueProb</c>
    /// and everything tuned against them survive the ruling untouched.</summary>
    public double TrueHomeProb { get; }

    /// <summary>1X2 prices. These were the two-way pair; they are now three, and
    /// <see cref="Odds(Side)"/> returns the 1X2 price so a bot that SELECTS on this pair and
    /// PLACES a moneyline pick cannot price one thing and be paid another.</summary>
    public double HomeOdds { get; private set; }
    public double AwayOdds { get; private set; }
    public double DrawOdds { get; private set; }

    /// <summary>Set by slate generation once the latents are known, because the draw price is a
    /// READ off those latents (<see cref="DrawProb"/>) rather than a constructor argument — the
    /// 1X2 triple cannot be priced before the distributions exist.</summary>
    internal void SetMoneylineOdds(double home, double draw, double away)
    {
        HomeOdds = home;
        DrawOdds = draw;
        AwayOdds = away;
    }

    public MatchLatents Latents { get; }
    public TeamStats HomeStats { get; }
    public TeamStats AwayStats { get; }
    public IReadOnlyList<MarketOffer> Markets { get; private set; }

    /// <summary>Derived from the locked stat line, shared by every leg referencing this matchup.
    /// Null still means "not locked yet"; <see cref="MatchResult.Draw"/> now means drawn, and the
    /// two were indistinguishable while this was a <c>Side?</c>.</summary>
    public MatchResult? Result => StatLine?.Result;

    /// <summary>P(draw), read off the matchup's own latents rather than dialled: the truncated
    /// Poisson grid already implies it (measured 22.6%–28.4% across the generator's latent box,
    /// higher for even matches, which is what real football does). No new RunConfig knob exists
    /// for it deliberately — a hand-set draw rate would be a number nobody measured.</summary>
    public double DrawProb => Dist.DrawProb;
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

    internal void SetMarkets(IReadOnlyList<MarketOffer> markets)
    {
        Markets = markets;
        _bySelection = null; // the board changed; the index below must be rebuilt against it
    }

    private Dictionary<MarketSelection, MarketOffer>? _bySelection;

    /// <summary>Board index, built once lazily (the <see cref="Dist"/> pattern). Both lookups
    /// below were linear scans over <see cref="Markets"/>, and the sim's sharp calls them twice
    /// per candidate inside a loop over every offer — so the bot's per-matchup cost was quadratic
    /// in board size, which is a cost the pre-game vocabulary expansion multiplies rather than
    /// adds to. FIRST WINS on a duplicate selection, exactly as the old scan's early return did:
    /// a config with a repeated line must keep behaving as it does today, not start throwing on
    /// dictionary construction.</summary>
    private Dictionary<MarketSelection, MarketOffer> BySelection
    {
        get
        {
            if (_bySelection != null) return _bySelection;
            var index = new Dictionary<MarketSelection, MarketOffer>(Markets.Count);
            foreach (MarketOffer offer in Markets) index.TryAdd(offer.Selection, offer);
            return _bySelection = index;
        }
    }

    /// <summary>The unconditional 1X2 probability. The old <c>TrueProb(Side)</c> overload was
    /// DELETED rather than redefined: under draws "the true probability of Home" has two defensible
    /// readings — <see cref="TrueHomeProb"/> (conditional) and this one (unconditional) — and a
    /// silently-wrong answer at a call site expecting the other is exactly the defect class this
    /// lane keeps finding. Every caller now names which one it wants, and the compiler found them.</summary>
    public double TrueProb(MatchResult result) => result switch
    {
        MatchResult.Home => TrueHomeProb * (1.0 - DrawProb),
        MatchResult.Away => (1.0 - TrueHomeProb) * (1.0 - DrawProb),
        MatchResult.Draw => DrawProb,
        _ => throw new ArgumentOutOfRangeException(nameof(result)),
    };

    public double Odds(Side side) => side == Side.Home ? HomeOdds : AwayOdds;
    public double FairOdds(Side side)
        => 1.0 / TrueProb(side == Side.Home ? MatchResult.Home : MatchResult.Away);

    /// <summary>The offered board already carries this exact number: <c>MatchModel.Offer</c>
    /// stores the same <c>TrueProbability</c> call's result on the <see cref="MarketOffer"/>, so
    /// reading it back is bit-identical, not merely equivalent. Recomputing it walked the full
    /// score enumeration on EVERY access, and <see cref="Leg.TrueProb"/> is a property the sweat
    /// re-reads per leg transition and per remaining leg when it prices a cash-out.
    /// The fallback is not dead code and must stay: this method's contract is a pure function of
    /// the model, defined for selections that were never offered (a bare <see cref="Matchup"/>
    /// carries an empty board), and tests price selections off the board.</summary>
    public double TrueProb(MarketSelection selection)
        => BySelection.TryGetValue(selection, out MarketOffer? offer)
            ? offer.TrueProb
            : MatchModel.TrueProbability(this, selection);

    public double Odds(MarketSelection selection)
        => BySelection.TryGetValue(selection, out MarketOffer? offer)
            ? offer.Odds
            : throw new ArgumentException($"Market selection is not offered: {selection.Kind}");

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
    public Side Side => Selection.Kind != MarketKind.Moneyline
        ? throw new InvalidOperationException($"Pick.Side is undefined for {Selection.Kind}; use Selection")
        : Selection.Choice switch
        {
            MarketChoice.Home => Side.Home,
            MarketChoice.Away => Side.Away,
            // The draw has no side. The old shape was `Choice == Home ? Home : Away`, which would
            // have answered "Away" for the X of 1X2 — silently, at every call site.
            _ => throw new InvalidOperationException(
                "Pick.Side is undefined for a moneyline DRAW; use Selection"),
        };

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
    public Side Side => Selection.Kind != MarketKind.Moneyline
        ? throw new InvalidOperationException($"Leg.Side is undefined for {Selection.Kind}; use Selection")
        : Selection.Choice switch
        {
            MarketChoice.Home => Side.Home,
            MarketChoice.Away => Side.Away,
            _ => throw new InvalidOperationException(
                "Leg.Side is undefined for a moneyline DRAW; use Selection"),
        };

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

    public Ticket(IReadOnlyList<Leg> legs, double stake, double vigPaid)
    {
        Legs = legs;
        Stake = stake;
        VigPaid = vigPaid;
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

    /// <summary>Payout on a win: stake × product of the active legs' offered odds (voided legs drop out),
    /// scaled by any relic payout multiplier.</summary>
    public double PotentialPayout => Stake * OddsMath.ParlayDecimal(ActiveLegs.Select(l => l.OfferedOdds).ToList()) * PayoutMultiplier;
}
