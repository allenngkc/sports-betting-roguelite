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

    /// <summary>Locked at compose time; relics that modify odds act before the lock.</summary>
    public double OfferedOdds { get; }

    public Leg(Matchup matchup, Side side, double offeredOdds)
    {
        Matchup = matchup;
        Side = side;
        OfferedOdds = offeredOdds;
    }

    public double TrueProb => Matchup.TrueProb(Side);

    public LegState State =>
        Matchup.Result == null ? LegState.Pending
        : Matchup.Result == Side ? LegState.Won
        : LegState.Lost;
}

public sealed class Ticket
{
    public IReadOnlyList<Leg> Legs { get; }
    public double Stake { get; }
    public double VigPaid { get; }
    public TicketState State { get; internal set; } = TicketState.Open;

    public Ticket(IReadOnlyList<Leg> legs, double stake, double vigPaid)
    {
        Legs = legs;
        Stake = stake;
        VigPaid = vigPaid;
    }

    public double PotentialPayout => Stake * OddsMath.ParlayDecimal(Legs.Select(l => l.OfferedOdds).ToList());
}
