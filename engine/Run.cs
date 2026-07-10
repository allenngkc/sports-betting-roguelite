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
/// complete. FastForwardRound drains the sweat without cashing out. Settle applies debt-as-HP
/// (DECISIONS.md 2026-07-09): a clean miss borrows from the bookie instead of ending the run.
///
/// Relics (PRD F8) live in an <see cref="EffectEngine"/> in acquisition order and hook the sites
/// above. All relic randomness (the lucky-charm rolls) is baked from the independent Relics
/// stream at lock, never during stepping, so determinism holds.
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

    /// <summary>piggy_bank's running balance; smashes into the bank when a ticket busts.</summary>
    public double PiggyBankBalance { get; private set; }

    /// <summary>What the run owes the bookie (debt-as-HP, DECISIONS.md 2026-07-09). 0 when clean.
    /// Set when a no-debt settle misses the target (shortfall × (1 + juice)); cleared in cash by the
    /// first settle that reaches <see cref="Requirement"/>. Missing a settle while indebted, or any
    /// final-round miss, loses the run.</summary>
    public double Debt { get; private set; }

    public double CurrentTarget => Config.Targets[Round - 1];

    /// <summary>What <see cref="Settle"/> actually checks: the round target plus any outstanding debt.</summary>
    public double Requirement => CurrentTarget + Debt;

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

        double maxStake = Config.MaxStakeFraction * Bank;
        if (stake > maxStake)
            throw new ArgumentException($"Stake {stake} exceeds the max stake {maxStake} for bank {Bank}");

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

        // Placement effects (high_roller's all-in bonus) see the bank BEFORE the stake is deducted.
        _effects.ApplyTicketPlaced(ticket, stake, Bank);

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

    /// <summary>
    /// Debt-as-HP settle (DECISIONS.md 2026-07-09). Clearing <see cref="Requirement"/> (target + debt)
    /// repays any debt in cash — by construction the bank stays ≥ the target — then advances (final
    /// round → RunWon, else → Shop). Missing it with no debt on a non-final round means the bookie
    /// floats you: the bank is topped up to the target as working capital and the shortfall is booked
    /// at (1 + juice). Missing while indebted, or any miss on the final round, loses the run.
    /// </summary>
    public void Settle()
    {
        RequirePhase(Phase.Settlement);
        bool finalRound = Round == Config.Rounds;

        if (Bank >= Requirement)
        {
            if (Debt > 0)
            {
                Bank -= Debt; // repaid in cash; Bank ≥ Requirement ⇒ Bank − Debt ≥ CurrentTarget
                Debt = 0;
            }
            Phase = finalRound ? Phase.RunWon : Phase.Shop;
        }
        else if (Debt == 0 && !finalRound)
        {
            // The bookie floats you: working capital up to the target, the shortfall on the books.
            double shortfall = CurrentTarget - Bank;
            Bank = CurrentTarget;
            Debt = shortfall * (1.0 + Config.DebtJuiceRate);
            Phase = Phase.Shop;
        }
        else
        {
            Phase = Phase.RunLost; // missed while indebted, or missed the final round
        }

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
        Phase = Phase.Betting;
    }

    private void RequirePhase(Phase expected)
    {
        if (Phase != expected)
            throw new InvalidOperationException($"Expected phase {expected}, but run is in {Phase}");
    }
}
