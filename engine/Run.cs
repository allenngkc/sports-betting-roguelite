using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

public enum Phase { Betting, Sweat, Settlement, Shop, RunWon, RunLost }

/// <summary>
/// The run state machine (PRD F1). Callers (console, sim, later Unity) drive the
/// transitions: PlaceTicket* → LockRound → Settle → ExitShop → … Week 1 resolves
/// instantly; Week 2 swaps ResolveRound's interior for the drama event stream
/// without changing this machine's shape.
/// </summary>
public sealed class Run
{
    public RunConfig Config { get; }
    public RngHub Rng { get; }
    public int Round { get; private set; } = 1;
    public double Bank { get; private set; }
    public Phase Phase { get; private set; } = Phase.Betting;
    public Slate CurrentSlate { get; private set; }

    private readonly List<Ticket> _tickets = new List<Ticket>();
    public IReadOnlyList<Ticket> Tickets => _tickets;

    public double CurrentTarget => Config.Targets[Round - 1];

    public Run(string runSeed, RunConfig? config = null)
    {
        Config = config ?? new RunConfig();
        Rng = new RngHub(runSeed);
        Bank = Config.StartingBank;
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
    }

    public Ticket PlaceTicket(IReadOnlyList<Pick> picks, double stake)
    {
        RequirePhase(Phase.Betting);
        if (_tickets.Count >= Config.MaxTicketsPerRound)
            throw new InvalidOperationException($"Max {Config.MaxTicketsPerRound} tickets per round");
        if (picks.Count < 1 || picks.Count > Config.MaxLegs)
            throw new ArgumentException($"Tickets take 1 to {Config.MaxLegs} legs, got {picks.Count}");
        if (picks.Select(p => p.MatchupIndex).Distinct().Count() != picks.Count)
            throw new ArgumentException("A ticket cannot have two legs on the same matchup");
        if (stake < Config.MinStake)
            throw new ArgumentException($"Minimum stake is {Config.MinStake}, got {stake}");
        if (stake > Bank)
            throw new ArgumentException($"Stake {stake} exceeds bank {Bank}");

        var legs = picks
            .Select(p =>
            {
                Matchup matchup = CurrentSlate.Matchups[p.MatchupIndex];
                return new Leg(matchup, p.Side, matchup.Odds(p.Side));
            })
            .ToList();

        double offered = OddsMath.ParlayDecimal(legs.Select(l => l.OfferedOdds).ToList());
        double fair = OddsMath.FairDecimal(OddsMath.ParlayProb(legs.Select(l => l.TrueProb).ToList()));
        var ticket = new Ticket(legs, stake, OddsMath.VigPaid(stake, offered, fair));

        Bank -= stake;
        _tickets.Add(ticket);
        return ticket;
    }

    public void LockRound()
    {
        RequirePhase(Phase.Betting);
        Phase = Phase.Sweat;
        ResolveRound();
        Phase = Phase.Settlement;
    }

    public void Settle()
    {
        RequirePhase(Phase.Settlement);
        if (Bank >= CurrentTarget)
            Phase = Round == Config.Rounds ? Phase.RunWon : Phase.Shop;
        else
            Phase = Phase.RunLost;
    }

    public void ExitShop()
    {
        RequirePhase(Phase.Shop);
        Round++;
        _tickets.Clear();
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
        Phase = Phase.Betting;
    }

    private void ResolveRound()
    {
        // Every game on the slate resolves, bet or not, in slate order: outcomes
        // for a seed are identical no matter what the player wagered.
        foreach (Matchup matchup in CurrentSlate.Matchups)
            matchup.Result = Rng.Outcomes.NextDouble() < matchup.TrueHomeProb ? Side.Home : Side.Away;

        foreach (Ticket ticket in _tickets)
        {
            if (ticket.Legs.All(l => l.State == LegState.Won))
            {
                ticket.State = TicketState.Won;
                Bank += ticket.PotentialPayout;
            }
            else
            {
                ticket.State = TicketState.Lost;
            }
        }
    }

    private void RequirePhase(Phase expected)
    {
        if (Phase != expected)
            throw new InvalidOperationException($"Expected phase {expected}, but run is in {Phase}");
    }
}
