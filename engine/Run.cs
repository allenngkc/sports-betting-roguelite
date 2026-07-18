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
/// verbs: Profit Boost at PlaceTicket, Mulligan Slip at a sweat's pending-loss window. The
/// bookie GIFTS a consumable after consecutive losing rounds (the
/// pity/retention channel — he wants you betting). LockRound still samples the fixed outcome
/// universe; consumable timing can never perturb the run seed.
/// </summary>
public sealed class Run
{
    public RunConfig Config { get; }
    public RngHub Rng { get; }
    public int Round { get; private set; } = 1;
    public double Bank { get; private set; }

    /// <summary>COMPS (design/10 F): the book's loyalty currency, earned per dollar staked,
    /// spent in the shop. Charm-expansion accounting (PLAN.md rev 5): the authoritative balance
    /// is an INTEGER count of tenths — no hidden fractional state. Wagering earnings pool in a
    /// per-round raw buffer and commit ONCE at LockRound (remainder discarded); every other
    /// mutation moves exact tenths at its own commit point.</summary>
    public double Comps => _deciComps / 10.0;

    private long _deciComps;
    private double _compsAccrualRaw;

    /// <summary>Converts a comps amount to integer tenths (AwayFromZero — PLAN.md rev 5).</summary>
    internal static long ToDeci(double comps)
        => (long)Math.Round(comps * 10.0, MidpointRounding.AwayFromZero);

    public Phase Phase { get; private set; } = Phase.Betting;
    public Slate CurrentSlate { get; private set; }

    private readonly List<Ticket> _tickets = new List<Ticket>();
    public IReadOnlyList<Ticket> Tickets => _tickets;

    private readonly List<SweatSession> _sweats = new List<SweatSession>();

    /// <summary>One session per ticket in placement order; empty until the round is locked.</summary>
    public IReadOnlyList<SweatSession> Sweats => _sweats;

    private readonly EffectEngine _effects;

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

    /// <summary>The BASE payment schedule: Config.Payments plus Totem surcharges (booked at base
    /// rates — PLAN.md rev 5 §9) plus Marker relief. House Key never mutates this — its ×1.15
    /// applies through the getters below on UNPAID entries only, so selling the Key simply drops
    /// the factor.</summary>
    private readonly double[] _payments;

    public double CurrentPayment => _payments[Round - 1] * _effects.PaymentFactor;

    /// <summary>The following round's payment (surcharges and payment factors included), or null
    /// on the final round — what a player planning shop spending is planning against.</summary>
    public double? NextPayment
        => Round < Config.Rounds ? _payments[Round] * _effects.PaymentFactor : (double?)null;

    /// <summary>The full live schedule — the book's public ledger. Unpaid entries (current round
    /// onward) carry the live payment factor; settled entries show their base history.</summary>
    public IReadOnlyList<double> PaymentSchedule
    {
        get
        {
            var view = new double[_payments.Length];
            double factor = _effects.PaymentFactor;
            for (int i = 0; i < _payments.Length; i++)
                view[i] = i >= Round - 1 ? _payments[i] * factor : _payments[i];
            return view;
        }
    }

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

    // Charm-expansion legality latches (PLAN.md rev 5 §7-8).
    private bool _markerPlayedThisRound;
    private bool _managerUsedThisVisit;
    private int _managerRedealOrdinal;

    /// <summary>The visible effect-state snapshot (ratchet stacks, streaks) — the UI's window
    /// into persistent item state (design/10 mandate, PLAN.md rev 5 §20).</summary>
    public IReadOnlyList<EffectStat> EffectStates => _effects.StateSnapshot();

    public Run(string runSeed, RunConfig? config = null)
    {
        Config = config ?? new RunConfig();
        Rng = new RngHub(runSeed);
        Bank = Config.StartingBank;
        _effects = new EffectEngine(GrantComps);
        _payments = (double[])Config.Payments.Clone();
        _bankAtBettingStart = Bank;
        _roundsSinceGift = Config.GiftCooldownRounds; // the first eligible gift is not cooldown-blocked
        CurrentSlate = SlateGenerator.Generate(Round, Rng.Slate, Config);
    }

    // ------------------------------------------------------------------ betting

    /// <summary>Places a ticket. profitBoostLeg ≥ 0 plays a held Profit Boost on that leg (its
    /// offered odds ×<see cref="RelicCatalog.ProfitBoostMult"/> before pricing) and consumes it.
    /// A locked contract <paramref name="modifier"/> (Free Bet / Double or Nothing — one per
    /// ticket, the one-modifier law) consumes its consumable and prices into the contract.</summary>
    public Ticket PlaceTicket(IReadOnlyList<Pick> picks, double stake, int profitBoostLeg = -1,
        TicketModifier modifier = TicketModifier.None)
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

        // Atomic legality (PLAN.md rev 5 §7): every consumable this placement needs is validated
        // BEFORE anything is consumed.
        if (profitBoostLeg >= 0)
        {
            if (profitBoostLeg >= picks.Count)
                throw new ArgumentException($"Profit Boost leg {profitBoostLeg} is not on this ticket");
            if (!OwnsConsumable("profit_boost"))
                throw new InvalidOperationException("No Profit Boost held");
        }
        if (modifier == TicketModifier.FreeBet && !OwnsConsumable("free_bet"))
            throw new InvalidOperationException("No Free Bet Token held");
        if (modifier == TicketModifier.DoubleOrNothing && !OwnsConsumable("double_or_nothing"))
            throw new InvalidOperationException("No Double or Nothing Slip held");

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
        var ticket = new Ticket(legs, stake, OddsMath.VigPaid(stake, offered, fair))
        {
            Id = $"{Round}.{_tickets.Count}",
        };

        if (modifier != TicketModifier.None)
        {
            ConsumeConsumable(modifier == TicketModifier.FreeBet ? "free_bet" : "double_or_nothing");
            ticket.Modifier = modifier;
            if (modifier == TicketModifier.DoubleOrNothing)
                ticket.SetFactor("don", RelicCatalog.DoubleOrNothingMult);
        }

        // Placement effects (Multiplier, Scar carrier) see the bank BEFORE the stake is deducted.
        _effects.ApplyTicketPlaced(ticket, stake, Bank, isFirstTicketThisRound: _tickets.Count == 0);

        Bank -= stake;
        _compsAccrualRaw += stake * Config.CompsPerDollarStaked; // pools raw; commits once at lock
        _tickets.Add(ticket);
        return ticket;
    }

    /// <summary>Plays a held Bookie's Marker: this round's payment drops by the relief fraction.
    /// Once per round — he keeps count (PLAN.md rev 5 §7).</summary>
    public void PlayBookiesMarker()
    {
        RequirePhase(Phase.Betting);
        if (_markerPlayedThisRound)
            throw new InvalidOperationException("The Marker is once per round");
        if (!OwnsConsumable("bookies_marker"))
            throw new InvalidOperationException("No Bookie's Marker held");

        ConsumeConsumable("bookies_marker");
        _markerPlayedThisRound = true;
        _payments[Round - 1] *= 1.0 - RelicCatalog.MarkerRelief;
    }

    public void LockRound()
    {
        RequirePhase(Phase.Betting);

        // The round's comps accrual COMMITS here — once, in tenths, remainder discarded — so
        // the Whale/Collection snapshots below read the same balance the player sees.
        _deciComps += ToDeci(_compsAccrualRaw);
        _compsAccrualRaw = 0;

        // Lock-time factor snapshots (chalk/iron/jar/system/whale/collection/housekey/photo):
        // no RNG involved, and the factor maps are frozen after this except the designed toggles.
        _effects.ApplyLock(this);

        // Every game on the slate resolves, bet or not, in slate order: outcomes for a seed are
        // identical no matter what the player wagered (the fixed universe).
        foreach (Matchup matchup in CurrentSlate.Matchups)
            matchup.Result = Rng.Outcomes.NextDouble() < matchup.TrueHomeProb ? Side.Home : Side.Away;

        _sweats.Clear();
        foreach (Ticket ticket in _tickets)
        {
            IReadOnlyList<IReadOnlyList<DramaEvent>> paths =
                DramaGenerator.BuildTicketPaths(ticket, Rng.Drama, Config.Drama, Round);

            _sweats.Add(new SweatSession(ticket, paths, Config, CreditBank, _effects,
                mulliganAvailable: () => OwnsConsumable("mulligan_slip"),
                whistleAvailable: () => OwnsConsumable("refs_whistle"),
                HandleBust));
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
        // The designed post-lock toggle (PLAN.md rev 5 §2): a void can strip the ticket's last
        // qualifying longshot leg — only the photo factor re-evaluates.
        _effects.RefreshPhotoFactor(session.TicketRef);
    }

    /// <summary>Plays a held Ref's Whistle on the session's pending dead leg: the leg's GRADING
    /// re-rolls once at the win-prob the player was living on before the killing event (this
    /// slip only — the shared result never bends). Overturned → the leg stands at full odds;
    /// confirmed → the ticket dies. Works on single-leg tickets (PLAN.md rev 5 §4-5).</summary>
    public void PlayRefsWhistle(SweatSession session)
    {
        RequirePhase(Phase.Sweat);
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (!session.HasPendingLoss)
            throw new InvalidOperationException("No dead leg is awaiting a Ref's Whistle");
        if (!OwnsConsumable("refs_whistle"))
            throw new InvalidOperationException("No Ref's Whistle held");

        ConsumeConsumable("refs_whistle");
        Pcg32 roll = Rng.Derive(Round, session.TicketRef.Id, session.PendingDeadLegIndex, "whistle", 0);
        session.ResolvePendingLossWithWhistle(roll);
    }

    // Timeout (a cash-out offer hold) was CUT at playtest #8 — its PlayTimeout verb lived here.
    // The SweatSession live-intervention seam it rode (ApplyLiveEffect/OfferHoldEffect) remains.

    // ------------------------------------------------------------------ settle

    /// <summary>Settles the round after the sweat: every still-Open ticket resolves Won (all active
    /// legs green — payout credited, scar carrier burns) or Lost (the scar feeds). Then the
    /// terminal-realization ledger runs (PLAN.md rev 5 §4): Free Bet refunds issue exactly once,
    /// early busts included.</summary>
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

        // Terminal-realization ledger: every Lost Free Bet ticket refunds its stake, once.
        // The loss still happened everywhere it matters (scar fed, jar counts it).
        foreach (Ticket ticket in _tickets)
        {
            if (ticket.State == TicketState.Lost
                && ticket.Modifier == TicketModifier.FreeBet
                && !ticket.Refunded)
            {
                ticket.Refunded = true;
                Bank += ticket.Stake;
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
    /// has a charge and this is not the final payment: the whole payment is DEFERRED (the bank is
    /// untouched — working capital survives, playtest #8) and the NEXT payment grows by
    /// payment × (1 + juice). The final payment is never deferrable ("no next favor").
    /// </summary>
    public void Settle()
    {
        RequirePhase(Phase.Settlement);
        bool finalRound = Round == Config.Rounds;
        // The EFFECTIVE payment: base schedule × the live payment factor (House Key). The base
        // array is never factor-mutated — getters own the view (PLAN.md rev 5 §9).
        double payment = _payments[Round - 1] * _effects.PaymentFactor;
        double bankBefore = Bank;
        double shortfall = 0;
        bool totemFired = false;

        // RoundResolution fires BEFORE the payment (after the refund ledger): The System and
        // the Bad Beat Jar read the round as played, not as taxed.
        double roundPnl = bankBefore - _bankAtBettingStart;
        int won = 0, lost = 0, cashed = 0;
        double refunds = 0;
        foreach (Ticket t in _tickets)
        {
            if (t.State == TicketState.Won) won++;
            else if (t.State == TicketState.Lost) lost++;
            else if (t.State == TicketState.CashedOut) cashed++;
            if (t.Refunded) refunds += t.Stake;
        }
        _effects.OnRoundResolved(new RoundResolution(
            Round, roundPnl, _tickets.Count, won, lost, cashed, refunds));

        if (Bank >= payment)
        {
            Bank -= payment;
            Phase = finalRound ? Phase.RunWon : Phase.Shop;
        }
        else if (!finalRound && _effects.TryConsumeTotem())
        {
            // Playtest #8: the totem DEFERS the whole payment — bank untouched, and the payment
            // plus juice lands on the next one. The surcharge books at BASE rates (Codex r2 #3):
            // the House Key factor applies once, through the getters, never compounded in.
            shortfall = payment - Bank;
            _payments[Round] += _payments[Round - 1] * (1.0 + Config.TotemJuiceRate);
            totemFired = true;
            Phase = Phase.Shop;
        }
        else
        {
            shortfall = payment - Bank;
            Phase = Phase.RunLost;
        }

        LastSettlement = new SettlementReport(Round, payment, bankBefore, Bank, shortfall, totemFired, Phase);

        _consecutiveLosingRounds = roundPnl < 0 ? _consecutiveLosingRounds + 1 : 0;

        if (Phase == Phase.Shop)
            EnterShop();
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
        long price = ToDeci(def.Price);
        if (price > _deciComps)
            throw new InvalidOperationException($"Relic {def.Id} costs {def.Price} comps, you have {Comps}");
        if (OwnedRelics.Count >= Config.RelicSlots)
            throw new InvalidOperationException($"All {Config.RelicSlots} relic slots are full");

        _deciComps -= price;
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
        long price = ToDeci(def.Price);
        if (price > _deciComps)
            throw new InvalidOperationException($"{def.Id} costs {def.Price} comps, you have {Comps}");
        if (_consumables.Count >= Config.ConsumableSlots)
            throw new InvalidOperationException($"All {Config.ConsumableSlots} consumable slots are full");

        _deciComps -= price;
        _consumables.Add(def);
        _consumableOffers.RemoveAt(offerIndex);
    }

    /// <summary>The single resale truth (PLAN.md rev 5 §10): Bobblehead's 3× override rides its
    /// resaleMult param; a fired (spent) Totem is worth 0; everything else sells at the global
    /// SellBackFraction of list.</summary>
    public double GetResaleValue(RelicDefinition def)
    {
        if (_effects.TotemSpent(def)) return 0.0;
        if (def.Params.TryGetValue("resaleMult", out double mult))
            return def.Price * mult;
        return def.Price * Config.SellBackFraction;
    }

    /// <summary>Sells the owned passive at the index for its <see cref="GetResaleValue"/>. A sold
    /// stateful passive loses its state (stacks die with the behavior); a sold Totem cannot be
    /// re-bought.</summary>
    public void SellRelic(int ownedIndex)
    {
        RequirePhase(Phase.Shop);
        if (ownedIndex < 0 || ownedIndex >= _effects.Owned.Count)
            throw new ArgumentOutOfRangeException(nameof(ownedIndex));

        _deciComps += ToDeci(GetResaleValue(_effects.Owned[ownedIndex]));
        _effects.RemoveAt(ownedIndex);
    }

    public void SellConsumable(int ownedIndex)
    {
        RequirePhase(Phase.Shop);
        if (ownedIndex < 0 || ownedIndex >= _consumables.Count)
            throw new ArgumentOutOfRangeException(nameof(ownedIndex));

        _deciComps += ToDeci(_consumables[ownedIndex].Price * Config.SellBackFraction);
        _consumables.RemoveAt(ownedIndex);
    }

    /// <summary>Plays a held Ask for the Manager: the whole hand is redealt once per visit. The
    /// redeal draws from a DERIVED stream — the Shop stream is untouched, so future visits deal
    /// identically whether or not the Manager was called (PLAN.md rev 5 §6, §8).</summary>
    public void PlayAskManager()
    {
        RequirePhase(Phase.Shop);
        if (_managerUsedThisVisit)
            throw new InvalidOperationException("The Manager comes out once per visit");
        if (!OwnsConsumable("ask_manager"))
            throw new InvalidOperationException("No Ask for the Manager held");

        ConsumeConsumable("ask_manager");
        _managerUsedThisVisit = true;
        DealOffers(Rng.Derive(Round, "", -1, "redeal", _managerRedealOrdinal++));
    }

    /// <summary>Shop entry: one-time effects fire (Rake's Rebate interest), the Manager latch
    /// resets, and the hand is dealt. A Manager redeal calls <see cref="DealOffers"/> ONLY —
    /// entry effects never replay (PLAN.md rev 5 §8).</summary>
    private void EnterShop()
    {
        _effects.OnShopEnter(this);
        _managerUsedThisVisit = false;
        DealOffers(Rng.Shop);
    }

    /// <summary>Pure dealing (the DEALT HAND, PLAN.md rev 5 §8): PassiveOfferCount distinct
    /// passives from the unowned pool (a purchased-ever Totem is out forever; short pools deal
    /// what remains) + ConsumableOfferCount distinct consumables.</summary>
    private void DealOffers(Pcg32 rng)
    {
        _shopOffers.Clear();
        var pool = new List<RelicDefinition>();
        foreach (RelicDefinition d in RelicCatalog.All)
        {
            if (_effects.Owns(d.Id)) continue;
            if (d.Id == RelicCatalog.TotemId && TotemEverPurchased) continue;
            pool.Add(d);
        }
        int deal = Math.Min(Config.PassiveOfferCount, pool.Count);
        for (int i = 0; i < deal; i++)
        {
            int j = rng.NextInt(i, pool.Count);
            (pool[i], pool[j]) = (pool[j], pool[i]);
            _shopOffers.Add(pool[i]);
        }

        _consumableOffers.Clear();
        var candidates = new List<ConsumableDefinition>(RelicCatalog.Consumables);
        int take = Math.Min(Config.ConsumableOfferCount, candidates.Count);
        for (int i = 0; i < take; i++)
        {
            int j = rng.NextInt(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            _consumableOffers.Add(candidates[i]);
        }
    }

    /// <summary>Rake's Rebate seam: interest on the held balance, committed in exact tenths.
    /// Public for the deci-comp quantization tests; gameplay reaches it only via the Rebate.</summary>
    public void ApplyCompsInterest(double rate)
        => _deciComps += (long)Math.Round(_deciComps * rate, MidpointRounding.AwayFromZero);

    public void ExitShop()
    {
        RequirePhase(Phase.Shop);
        _shopOffers.Clear();
        _consumableOffers.Clear();
        Round++;
        _tickets.Clear();
        _sweats.Clear();
        _markerPlayedThisRound = false;
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

    /// <summary>Grants comps directly (Comp'd Suite's award seam + tests), in exact tenths.</summary>
    public void GrantComps(double comps) => _deciComps += ToDeci(comps);

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
