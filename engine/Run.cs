using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

public enum Phase { Betting, Sweat, Settlement, Shop, RunWon, RunLost }

/// <summary>
/// The run state machine (PRD F1). Callers (console, sim, later Unity) drive the
/// transitions: PlaceTicket* → LockRound → (sweat the sessions / cash out) →
/// FinishSweat → Settle → ExitShop → … LockRound samples the fixed outcome universe
/// and builds one SweatSession per ticket; the round no longer settles at lock —
/// FinishSweat does, once every session is complete. FastForwardRound drains the
/// sweat without cashing out (the sim and Week 1-era callers use it).
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

    private readonly List<SweatSession> _sweats = new List<SweatSession>();

    /// <summary>One session per ticket in placement order; empty until the round is locked.</summary>
    public IReadOnlyList<SweatSession> Sweats => _sweats;

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

        // Every game on the slate resolves, bet or not, in slate order: outcomes for a seed are
        // identical no matter what the player wagered (the fixed universe). This is unchanged from Week 1.
        foreach (Matchup matchup in CurrentSlate.Matchups)
            matchup.Result = Rng.Outcomes.NextDouble() < matchup.TrueHomeProb ? Side.Home : Side.Away;

        // Build one steppable session per ticket in placement order, baking all drama paths now.
        _sweats.Clear();
        foreach (Ticket ticket in _tickets)
        {
            IReadOnlyList<IReadOnlyList<DramaEvent>> paths =
                DramaGenerator.BuildTicketPaths(ticket, Rng.Drama, Config.Drama);
            _sweats.Add(new SweatSession(ticket, paths, Config, CreditBank));
        }

        Phase = Phase.Sweat;
    }

    /// <summary>Settles the round after the sweat: every still-Open ticket resolves Won (all legs green,
    /// bank += payout) or Lost. CashedOut and already-Lost tickets are settled — no double credit.</summary>
    public void FinishSweat()
    {
        RequirePhase(Phase.Sweat);
        if (!_sweats.All(s => s.IsComplete))
            throw new InvalidOperationException("Every sweat session must be complete before finishing the sweat.");

        foreach (Ticket ticket in _tickets)
        {
            if (ticket.State != TicketState.Open) continue; // CashedOut or dead-leg Lost: already settled

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

        Phase = Phase.Settlement;
    }

    /// <summary>Drains every session without cashing out, then finishes the sweat. The sim and most
    /// Week 1-era callers use this to skip straight from lock to settlement.</summary>
    public void FastForwardRound()
    {
        RequirePhase(Phase.Sweat);
        foreach (SweatSession session in _sweats)
            while (session.MoveNext(out _)) { }
        FinishSweat();
    }

    private void CreditBank(double amount) => Bank += amount;

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
        _sweats.Clear();
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
        Phase = Phase.Betting;
    }

    private void RequirePhase(Phase expected)
    {
        if (Phase != expected)
            throw new InvalidOperationException($"Expected phase {expected}, but run is in {Phase}");
    }
}
