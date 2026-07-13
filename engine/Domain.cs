using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

public enum Side { Home, Away }

public enum LegState { Pending, Won, Lost }

public enum TicketState { Open, Won, Lost, CashedOut }

public sealed class Team
{
    public string Name { get; }
    public int Wins { get; }
    public int Losses { get; }
    public string Record => $"{Wins}-{Losses}";

    public Team(string name, int wins, int losses)
    {
        Name = name;
        Wins = wins;
        Losses = losses;
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

    /// <summary>Set once at round lock, shared by every leg referencing this matchup.</summary>
    public Side? Result { get; internal set; }

    public Matchup(int index, Team home, Team away, double trueHomeProb, double homeOdds, double awayOdds)
    {
        Index = index;
        Home = home;
        Away = away;
        TrueHomeProb = trueHomeProb;
        HomeOdds = homeOdds;
        AwayOdds = awayOdds;
    }

    public double TrueProb(Side side) => side == Side.Home ? TrueHomeProb : 1.0 - TrueHomeProb;
    public double Odds(Side side) => side == Side.Home ? HomeOdds : AwayOdds;
    public double FairOdds(Side side) => 1.0 / TrueProb(side);
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
    public Side Side { get; }

    public Pick(int matchupIndex, Side side)
    {
        MatchupIndex = matchupIndex;
        Side = side;
    }
}

public sealed class Leg
{
    public Matchup Matchup { get; }
    public Side Side { get; }

    /// <summary>The matchup's own price for the picked side — the odds before any relic touched them.</summary>
    public double BaseOdds { get; }

    /// <summary>The locked contract odds. Equals <see cref="BaseOdds"/> unless a compose-time relic
    /// (boosted_odds, promo_code) rewrote it before the ticket locked.</summary>
    public double OfferedOdds { get; internal set; }

    /// <summary>Set when a Mulligan Slip voids this leg after it reveals Lost: the leg is struck from
    /// the ticket (excluded from payout, win condition, and cash-out products).</summary>
    public bool IsVoided { get; internal set; }

    public Leg(Matchup matchup, Side side, double offeredOdds)
    {
        Matchup = matchup;
        Side = side;
        OfferedOdds = offeredOdds;
        BaseOdds = offeredOdds;
    }

    public double TrueProb => Matchup.TrueProb(Side);

    public LegState State =>
        Matchup.Result == null ? LegState.Pending
        : Matchup.Result == Side ? LegState.Won
        : LegState.Lost;

    /// <summary>This ticket's grading of the leg. Voided legs never count as won.</summary>
    public bool GradesWon => !IsVoided && State == LegState.Won;
}

public sealed class Ticket
{
    public IReadOnlyList<Leg> Legs { get; }
    public double Stake { get; }
    public double VigPaid { get; }
    public TicketState State { get; internal set; } = TicketState.Open;

    /// <summary>Payout scale from relic effects — THE product slot (design/10 B2): every payout
    /// effect multiplies in (The Multiplier × Scar Tissue × future feeders), so items stack
    /// multiplicatively. Scales the win payout and the cash-out fair value.</summary>
    public double PayoutMultiplier { get; internal set; } = 1.0;

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
