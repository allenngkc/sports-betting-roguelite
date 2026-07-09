using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

public enum Phase { Betting, Sweat, Settlement, Shop, RunWon, RunLost }

/// <summary>
/// The run state machine (PRD F1). Callers (console, sim, later Unity) drive the
/// transitions: PlaceTicket* → LockRound → (sweat the sessions / cash out) →
/// FinishSweat → Settle → (Shop: BuyRelic*) → ExitShop → … LockRound samples the fixed
/// outcome universe, bakes drama paths and the lucky-charm rolls, accrues the piggy bank, and
/// builds one SweatSession per ticket; the round settles in FinishSweat once every session is
/// complete. FastForwardRound drains the sweat without cashing out.
///
/// Relics (PRD F8) live in an <see cref="EffectEngine"/> in acquisition order and hook the sites
/// above. All relic randomness (tout intel, lucky-charm rolls) is baked from the independent Relics
/// stream at lock/shop-exit, never during stepping, so determinism holds.
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

    private readonly EffectEngine _effects = new EffectEngine();

    /// <summary>Owned relics, in acquisition (purchase) order.</summary>
    public IReadOnlyList<RelicDefinition> OwnedRelics => _effects.Owned;

    private readonly List<RelicDefinition> _shopOffers = new List<RelicDefinition>();

    /// <summary>The relics on offer in the current shop; empty outside Phase.Shop.</summary>
    public IReadOnlyList<RelicDefinition> ShopOffers => _shopOffers;

    private IReadOnlyList<MatchupIntel> _intel = Array.Empty<MatchupIntel>();

    /// <summary>tout_sheet intel for the current round; empty when the relic is not owned.</summary>
    public IReadOnlyList<MatchupIntel> Intel => _intel;

    /// <summary>piggy_bank's running balance; smashes into the bank when a ticket busts.</summary>
    public double PiggyBankBalance { get; private set; }

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

        double maxStake = _effects.MaxStakeFraction(Config.MaxStakeFraction) * Bank;
        if (stake > maxStake)
            throw new ArgumentException($"Stake {stake} exceeds the max stake {maxStake} for bank {Bank}");
        if (stake > Bank)
            throw new ArgumentException($"Stake {stake} exceeds bank {Bank}");

        var legs = picks
            .Select(p =>
            {
                Matchup matchup = CurrentSlate.Matchups[p.MatchupIndex];
                return new Leg(matchup, p.Side, matchup.Odds(p.Side));
            })
            .ToList();

        // Compose-time relics (promo_code, boosted_odds) rewrite offered odds in acquisition order,
        // BEFORE vig is computed — so a boosted or promo'd leg legitimately lowers (or erases) the vig.
        _effects.ApplyCompose(legs, isFirstTicketThisRound: _tickets.Count == 0);

        double offered = OddsMath.ParlayDecimal(legs.Select(l => l.OfferedOdds).ToList());
        double fair = OddsMath.FairDecimal(OddsMath.ParlayProb(legs.Select(l => l.TrueProb).ToList()));
        var ticket = new Ticket(legs, stake, OddsMath.VigPaid(stake, offered, fair));

        Bank -= stake;
        _tickets.Add(ticket);
        return ticket;
    }

    /// <summary>sharp_eye: reveals the exact true win probability of one chosen line. Consumes the
    /// once-per-round charge. Betting phase and ownership required; no RNG. (Redesigned from a
    /// "+EV flag" that was always false against a vig-priced book — DECISIONS.md 2026-07-08.)</summary>
    public double QuerySharpEye(int matchupIndex, Side side)
    {
        RequirePhase(Phase.Betting);
        if (!_effects.HasSharpEye)
            throw new InvalidOperationException("Sharp Eye is not owned.");
        if (matchupIndex < 0 || matchupIndex >= CurrentSlate.Matchups.Count)
            throw new ArgumentOutOfRangeException(nameof(matchupIndex));
        if (!_effects.TryConsumeSharpEye())
            throw new InvalidOperationException("Sharp Eye has no charge left this round.");

        return CurrentSlate.Matchups[matchupIndex].TrueProb(side);
    }

    public void LockRound()
    {
        RequirePhase(Phase.Betting);

        // Every game on the slate resolves, bet or not, in slate order: outcomes for a seed are
        // identical no matter what the player wagered (the fixed universe). This is unchanged from Week 1.
        foreach (Matchup matchup in CurrentSlate.Matchups)
            matchup.Result = Rng.Outcomes.NextDouble() < matchup.TrueHomeProb ? Side.Home : Side.Away;

        // Build one steppable session per ticket in placement order, baking all randomness now:
        // drama paths from the Drama stream, then the lucky-charm second-chance roll from the Relics
        // stream (only when owned). piggy_bank accrues the paid vig here too.
        _sweats.Clear();
        foreach (Ticket ticket in _tickets)
        {
            IReadOnlyList<IReadOnlyList<DramaEvent>> paths =
                DramaGenerator.BuildTicketPaths(ticket, Rng.Drama, Config.Drama);

            double finalLegTrueProb = ticket.Legs[ticket.Legs.Count - 1].TrueProb;
            bool luckyBaked = _effects.BakeLuckyRoll(finalLegTrueProb, Rng.Relics);

            if (_effects.HasPiggyBank)
                PiggyBankBalance += _effects.PiggyRebateMult * Math.Max(0.0, ticket.VigPaid);

            _sweats.Add(new SweatSession(ticket, paths, Config, CreditBank, _effects, luckyBaked, HandleBust));
        }

        Phase = Phase.Sweat;
    }

    /// <summary>Settles the round after the sweat: every still-Open ticket resolves Won (all active legs
    /// green, bank += payout) or Lost. CashedOut and already-Lost tickets are settled — no double credit.</summary>
    public void FinishSweat()
    {
        RequirePhase(Phase.Sweat);
        if (!_sweats.All(s => s.IsComplete))
            throw new InvalidOperationException("Every sweat session must be complete before finishing the sweat.");

        foreach (Ticket ticket in _tickets)
        {
            if (ticket.State != TicketState.Open) continue; // CashedOut or dead-leg Lost: already settled

            if (ticket.GradesWon)
            {
                ticket.State = TicketState.Won;
                Bank += ticket.PotentialPayout;
            }
            else
            {
                ticket.State = TicketState.Lost;
                HandleBust(ticket);
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

    // The bust chain (bankroll_insurance refund, piggy_bank smash) resolves in acquisition order.
    private void HandleBust(Ticket ticket)
        => _effects.OnBust(new BustContext(ticket, CreditBank, SmashPiggy));

    private void SmashPiggy()
    {
        Bank += PiggyBankBalance;
        PiggyBankBalance = 0.0;
    }

    public void Settle()
    {
        RequirePhase(Phase.Settlement);
        if (Bank >= CurrentTarget)
            Phase = Round == Config.Rounds ? Phase.RunWon : Phase.Shop;
        else
            Phase = Phase.RunLost;

        if (Phase == Phase.Shop)
            GenerateShopOffers();
    }

    /// <summary>Buys the offer at the given index: deducts its fixed price, appends it to the owned
    /// relics (acquisition order), and removes it from the shop. Phase.Shop only.</summary>
    public void BuyRelic(int offerIndex)
    {
        RequirePhase(Phase.Shop);
        if (offerIndex < 0 || offerIndex >= _shopOffers.Count)
            throw new ArgumentOutOfRangeException(nameof(offerIndex));

        RelicDefinition def = _shopOffers[offerIndex];
        if (def.Price > Bank)
            throw new InvalidOperationException($"Relic {def.Id} costs {def.Price}, bank is {Bank}");
        if (OwnedRelics.Count >= Config.RelicSlots)
            throw new InvalidOperationException($"All {Config.RelicSlots} relic slots are full");

        Bank -= def.Price;
        _effects.Add(def);
        _shopOffers.RemoveAt(offerIndex);
    }

    private void GenerateShopOffers()
    {
        _shopOffers.Clear();

        var candidates = new List<RelicDefinition>();
        foreach (RelicDefinition d in RelicCatalog.All)
            if (!_effects.Owns(d.Id)) candidates.Add(d);

        int take = Math.Min(Config.ShopOfferCount, candidates.Count);
        // Partial Fisher-Yates over the Shop stream: draw `take` distinct relics.
        for (int i = 0; i < take; i++)
        {
            int j = Rng.Shop.NextInt(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            _shopOffers.Add(candidates[i]);
        }
    }

    public void ExitShop()
    {
        RequirePhase(Phase.Shop);
        _shopOffers.Clear();
        _effects.ResetRoundCharges(); // per-round charges recharge for the new round
        Round++;
        _tickets.Clear();
        _sweats.Clear();
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
        _intel = _effects.GenerateIntel(CurrentSlate, Rng.Relics); // tout_sheet fires on the fresh slate
        Phase = Phase.Betting;
    }

    private void RequirePhase(Phase expected)
    {
        if (Phase != expected)
            throw new InvalidOperationException($"Expected phase {expected}, but run is in {Phase}");
    }
}
