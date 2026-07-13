using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Engine;

public enum Phase { Betting, Sweat, Settlement, Shop, RunWon, RunLost }

/// <summary>One settle's telemetry (the economy rework's payment model): what was due, what
/// happened, whether the Totem fired. Presentation renders it; the sim audits it.</summary>
public readonly struct SettlementReport
{
    public readonly int Round;
    public readonly double Payment;
    public readonly double BankBefore;
    public readonly double BankAfter;
    public readonly double Shortfall;   // 0 when the payment was met
    public readonly bool TotemFired;
    public readonly Phase Outcome;

    public SettlementReport(int round, double payment, double bankBefore, double bankAfter,
        double shortfall, bool totemFired, Phase outcome)
    {
        Round = round;
        Payment = payment;
        BankBefore = bankBefore;
        BankAfter = bankAfter;
        Shortfall = shortfall;
        TotemFired = totemFired;
        Outcome = outcome;
    }

    public bool Paid => Shortfall == 0;
}

/// <summary>
/// The run state machine, economy-rework edition (PLAN.md 2026-07-13). Callers drive:
/// PlaceTicket* → LockRound → (sweat the sessions / cash out / play consumables) → FinishSweat →
/// Settle → (Shop: buy/sell) → ExitShop → … Settle now DEDUCTS the round's payment from the bank
/// (the income-rate race, design/10): a payment you cannot meet ends the run — unless the Totem
/// of Undying fires (never on the final payment). Debt-as-HP and the float are deleted.
///
/// Passives live in the <see cref="EffectEngine"/> (Multiplier / Scar Tissue / Totem — all payout
/// effects multiply into Ticket.PayoutMultiplier). Consumables are a separate pool of player-TIMED
/// verbs: Profit Boost at PlaceTicket, Mulligan Slip at a sweat's pending-loss window, Timeout as
/// a cash-out offer hold. The bookie GIFTS a consumable after consecutive losing rounds (the
/// pity/retention channel — he wants you betting). LockRound still samples the fixed outcome
/// universe; consumable timing can never perturb the run seed.
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

    /// <summary>Owned passives, in acquisition (purchase) order.</summary>
    public IReadOnlyList<RelicDefinition> OwnedRelics => _effects.Owned;

    private readonly List<ConsumableDefinition> _consumables = new List<ConsumableDefinition>();

    /// <summary>Held consumables (≤ Config.ConsumableSlots), separate pool from relics.</summary>
    public IReadOnlyList<ConsumableDefinition> OwnedConsumables => _consumables;

    private readonly List<RelicDefinition> _shopOffers = new List<RelicDefinition>();
    private readonly List<ConsumableDefinition> _consumableOffers = new List<ConsumableDefinition>();

    /// <summary>Passive offers in the current shop (all unowned; Totem only if never purchased).</summary>
    public IReadOnlyList<RelicDefinition> ShopOffers => _shopOffers;

    /// <summary>Consumable offers in the current shop.</summary>
    public IReadOnlyList<ConsumableDefinition> ConsumableOffers => _consumableOffers;

    /// <summary>The runtime payment schedule. Starts as Config.Payments; the Totem surcharges the
    /// next entry when it fires.</summary>
    private readonly double[] _payments;

    public double CurrentPayment => _payments[Round - 1];

    /// <summary>The following round's payment (surcharges included), or null on the final round —
    /// what a player planning shop spending is actually planning against.</summary>
    public double? NextPayment => Round < Config.Rounds ? _payments[Round] : (double?)null;

    /// <summary>The full live schedule (surcharges included) — shown to the player like any book's
    /// ledger; the whole ladder is public information.</summary>
    public IReadOnlyList<double> PaymentSchedule => _payments;

    /// <summary>Telemetry of the most recent settle; null before the first.</summary>
    public SettlementReport? LastSettlement { get; private set; }

    /// <summary>The consumable the bookie just gifted at the round's open (null when none) —
    /// presentation's cue for the gift text.</summary>
    public ConsumableDefinition? LastGift { get; private set; }

    /// <summary>Scar Tissue's visible ratchet state, in percentage points.</summary>
    public double ScarStacks => _effects.ScarStacks;

    /// <summary>The Totem is sellable and its charge consumable, but it can be BOUGHT only once
    /// per run, ever (design/10 B).</summary>
    public bool TotemEverPurchased { get; private set; }

    private double _bankAtBettingStart;
    private int _consecutiveLosingRounds;
    private int _roundsSinceGift;

    public Run(string runSeed, RunConfig? config = null)
    {
        Config = config ?? new RunConfig();
        Rng = new RngHub(runSeed);
        Bank = Config.StartingBank;
        _payments = (double[])Config.Payments.Clone();
        _bankAtBettingStart = Bank;
        _roundsSinceGift = Config.GiftCooldownRounds; // the first eligible gift is not cooldown-blocked
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
    }

    // ------------------------------------------------------------------ betting

    /// <summary>Places a ticket. profitBoostLeg ≥ 0 plays a held Profit Boost on that leg (its
    /// offered odds ×<see cref="RelicCatalog.ProfitBoostMult"/> before pricing) and consumes it.</summary>
    public Ticket PlaceTicket(IReadOnlyList<Pick> picks, double stake, int profitBoostLeg = -1)
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

        if (profitBoostLeg >= 0)
        {
            if (profitBoostLeg >= picks.Count)
                throw new ArgumentException($"Profit Boost leg {profitBoostLeg} is not on this ticket");
            if (!OwnsConsumable("profit_boost"))
                throw new InvalidOperationException("No Profit Boost held");
        }

        var legs = picks
            .Select(p =>
            {
                Matchup matchup = CurrentSlate.Matchups[p.MatchupIndex];
                return new Leg(matchup, p.Side, matchup.Odds(p.Side));
            })
            .ToList();

        // The played Profit Boost rewrites the chosen leg's offered odds BEFORE vig is computed —
        // like every promo, it can legitimately drive the ticket's vig to zero or below.
        if (profitBoostLeg >= 0)
        {
            legs[profitBoostLeg].OfferedOdds *= RelicCatalog.ProfitBoostMult;
            ConsumeConsumable("profit_boost");
        }

        double offered = OddsMath.ParlayDecimal(legs.Select(l => l.OfferedOdds).ToList());
        double fair = OddsMath.FairDecimal(OddsMath.ParlayProb(legs.Select(l => l.TrueProb).ToList()));
        var ticket = new Ticket(legs, stake, OddsMath.VigPaid(stake, offered, fair));

        // Placement effects (Multiplier, Scar carrier) see the bank BEFORE the stake is deducted.
        _effects.ApplyTicketPlaced(ticket, stake, Bank, isFirstTicketThisRound: _tickets.Count == 0);

        Bank -= stake;
        _tickets.Add(ticket);
        return ticket;
    }

    public void LockRound()
    {
        RequirePhase(Phase.Betting);

        // Every game on the slate resolves, bet or not, in slate order: outcomes for a seed are
        // identical no matter what the player wagered (the fixed universe).
        foreach (Matchup matchup in CurrentSlate.Matchups)
            matchup.Result = Rng.Outcomes.NextDouble() < matchup.TrueHomeProb ? Side.Home : Side.Away;

        _sweats.Clear();
        foreach (Ticket ticket in _tickets)
        {
            IReadOnlyList<IReadOnlyList<DramaEvent>> paths =
                DramaGenerator.BuildTicketPaths(ticket, Rng.Drama, Config.Drama);

            _sweats.Add(new SweatSession(ticket, paths, Config, CreditBank, _effects,
                mulliganAvailable: () => OwnsConsumable("mulligan_slip"), HandleBust));
        }

        Phase = Phase.Sweat;
    }

    // ------------------------------------------------------------------ sweat-time consumables

    /// <summary>Plays a held Mulligan Slip on the session's pending dead leg: the leg is voided
    /// and the sweat continues. Only valid inside the pending-loss window.</summary>
    public void PlayMulliganSlip(SweatSession session)
    {
        RequirePhase(Phase.Sweat);
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (!session.HasPendingLoss)
            throw new InvalidOperationException("No dead leg is awaiting a Mulligan Slip");
        if (!OwnsConsumable("mulligan_slip"))
            throw new InvalidOperationException("No Mulligan Slip held");

        ConsumeConsumable("mulligan_slip");
        session.ResolvePendingLossAsMulligan();
    }

    /// <summary>Plays a held Timeout on the session: the cash-out offer holds its current price
    /// for the next 3 events. Requires a live offer.</summary>
    public void PlayTimeout(SweatSession session)
    {
        RequirePhase(Phase.Sweat);
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (!session.CashOutOffer().HasValue)
            throw new InvalidOperationException("No live cash-out offer to hold");
        if (!OwnsConsumable("timeout"))
            throw new InvalidOperationException("No Timeout held");

        ConsumeConsumable("timeout");
        session.ApplyLiveEffect(new OfferHoldEffect(3));
    }

    // ------------------------------------------------------------------ settle

    /// <summary>Settles the round after the sweat: every still-Open ticket resolves Won (all active
    /// legs green — payout credited, scar carrier burns) or Lost (the scar feeds).</summary>
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
                _effects.OnTicketRealized(ticket);
            }
            else
            {
                ticket.State = TicketState.Lost;
                HandleBust(ticket);
            }
        }

        Phase = Phase.Settlement;
    }

    /// <summary>Drains every session without cashing out or playing consumables (pending losses
    /// auto-decline), then finishes the sweat. The sim's fast path.</summary>
    public void FastForwardRound()
    {
        RequirePhase(Phase.Sweat);
        foreach (SweatSession session in _sweats)
            while (session.MoveNext(out _)) { }
        FinishSweat();
    }

    private void CreditBank(double amount) => Bank += amount;

    private void HandleBust(Ticket ticket) => _effects.OnBust(ticket);

    /// <summary>
    /// The payment settle (design/10): the round's payment is DEDUCTED. Meet it and the run
    /// advances (final round → RunWon). Miss it and the run is over — unless the Totem of Undying
    /// has a charge and this is not the final payment: the bookie covers the shortfall, the bank
    /// zeroes, and the NEXT payment grows by shortfall × (1 + juice). The final payment is never
    /// coverable ("no next favor").
    /// </summary>
    public void Settle()
    {
        RequirePhase(Phase.Settlement);
        bool finalRound = Round == Config.Rounds;
        double payment = _payments[Round - 1];
        double bankBefore = Bank;
        double shortfall = 0;
        bool totemFired = false;

        if (Bank >= payment)
        {
            Bank -= payment;
            Phase = finalRound ? Phase.RunWon : Phase.Shop;
        }
        else if (!finalRound && _effects.TryConsumeTotem())
        {
            shortfall = payment - Bank;
            Bank = 0;
            _payments[Round] += shortfall * (1.0 + Config.TotemJuiceRate);
            totemFired = true;
            Phase = Phase.Shop;
        }
        else
        {
            shortfall = payment - Bank;
            Phase = Phase.RunLost;
        }

        LastSettlement = new SettlementReport(Round, payment, bankBefore, Bank, shortfall, totemFired, Phase);

        double roundPnl = bankBefore - _bankAtBettingStart;
        _consecutiveLosingRounds = roundPnl < 0 ? _consecutiveLosingRounds + 1 : 0;

        if (Phase == Phase.Shop)
            GenerateShopOffers();
    }

    // ------------------------------------------------------------------ shop

    /// <summary>Buys the passive at the offer index. Buying the Totem marks it purchased for the
    /// rest of the run (it never reappears in the shop, even after selling or burning).</summary>
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
        if (def.Id == RelicCatalog.TotemId)
            TotemEverPurchased = true;
        _shopOffers.RemoveAt(offerIndex);
    }

    public void BuyConsumable(int offerIndex)
    {
        RequirePhase(Phase.Shop);
        if (offerIndex < 0 || offerIndex >= _consumableOffers.Count)
            throw new ArgumentOutOfRangeException(nameof(offerIndex));

        ConsumableDefinition def = _consumableOffers[offerIndex];
        if (def.Price > Bank)
            throw new InvalidOperationException($"{def.Id} costs {def.Price}, bank is {Bank}");
        if (_consumables.Count >= Config.ConsumableSlots)
            throw new InvalidOperationException($"All {Config.ConsumableSlots} consumable slots are full");

        Bank -= def.Price;
        _consumables.Add(def);
        _consumableOffers.RemoveAt(offerIndex);
    }

    /// <summary>Sells the owned passive at the index for SellBackFraction of list price. A sold
    /// Scar Tissue loses its stacks; a sold Totem cannot be re-bought.</summary>
    public void SellRelic(int ownedIndex)
    {
        RequirePhase(Phase.Shop);
        if (ownedIndex < 0 || ownedIndex >= _effects.Owned.Count)
            throw new ArgumentOutOfRangeException(nameof(ownedIndex));

        Bank += _effects.Owned[ownedIndex].Price * Config.SellBackFraction;
        _effects.RemoveAt(ownedIndex);
    }

    public void SellConsumable(int ownedIndex)
    {
        RequirePhase(Phase.Shop);
        if (ownedIndex < 0 || ownedIndex >= _consumables.Count)
            throw new ArgumentOutOfRangeException(nameof(ownedIndex));

        Bank += _consumables[ownedIndex].Price * Config.SellBackFraction;
        _consumables.RemoveAt(ownedIndex);
    }

    private void GenerateShopOffers()
    {
        _shopOffers.Clear();
        foreach (RelicDefinition d in RelicCatalog.All)
        {
            if (_effects.Owns(d.Id)) continue;
            if (d.Id == RelicCatalog.TotemId && TotemEverPurchased) continue;
            _shopOffers.Add(d);
        }

        // Consumable offers: draw ConsumableOfferCount distinct types via the Shop stream.
        _consumableOffers.Clear();
        var candidates = new List<ConsumableDefinition>(RelicCatalog.Consumables);
        int take = Math.Min(Config.ConsumableOfferCount, candidates.Count);
        for (int i = 0; i < take; i++)
        {
            int j = Rng.Shop.NextInt(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            _consumableOffers.Add(candidates[i]);
        }
    }

    public void ExitShop()
    {
        RequirePhase(Phase.Shop);
        _shopOffers.Clear();
        _consumableOffers.Clear();
        Round++;
        _tickets.Clear();
        _sweats.Clear();
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
        Phase = Phase.Betting;
        _bankAtBettingStart = Bank;

        // The bookie's gift (pity/retention channel, design/10 D): after enough consecutive
        // losing rounds, off cooldown, with a slot free, he texts you a promo — he wants you
        // betting. Deterministic per seed via the Shop stream.
        LastGift = null;
        _roundsSinceGift++;
        if (_consecutiveLosingRounds >= Config.GiftAfterLosingRounds
            && _roundsSinceGift >= Config.GiftCooldownRounds
            && _consumables.Count < Config.ConsumableSlots)
        {
            ConsumableDefinition gift =
                RelicCatalog.Consumables[Rng.Shop.NextInt(0, RelicCatalog.Consumables.Count)];
            _consumables.Add(gift);
            LastGift = gift;
            _roundsSinceGift = 0;
        }
    }

    // ------------------------------------------------------------------ audit/test seams

    /// <summary>Grants a passive directly — no shop, no price, no slot check. The sim's
    /// granted-free item audit and the engine tests script item states through this.</summary>
    public void GrantRelic(RelicDefinition def)
    {
        _effects.Add(def);
        if (def.Id == RelicCatalog.TotemId)
            TotemEverPurchased = true;
    }

    /// <summary>Grants a consumable directly (audit/test seam; ignores slot limits).</summary>
    public void GrantConsumable(ConsumableDefinition def) => _consumables.Add(def);

    // ------------------------------------------------------------------ helpers

    public bool OwnsConsumable(string id)
    {
        foreach (ConsumableDefinition c in _consumables)
            if (c.Id == id) return true;
        return false;
    }

    private void ConsumeConsumable(string id)
    {
        for (int i = 0; i < _consumables.Count; i++)
        {
            if (_consumables[i].Id == id)
            {
                _consumables.RemoveAt(i);
                return;
            }
        }
        throw new InvalidOperationException($"No {id} held");
    }

    private void RequirePhase(Phase expected)
    {
        if (Phase != expected)
            throw new InvalidOperationException($"Expected phase {expected}, but run is in {Phase}");
    }
}
