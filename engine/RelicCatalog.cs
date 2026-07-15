using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>
/// One passive relic as pure data: an id, display copy, the design axis it sits on, a behavior
/// key (<see cref="Op"/>) the effect engine dispatches on, a fixed price, and a parameter bag the
/// matching behavior reads. Content lives here so the catalog can grow without touching engine logic.
/// </summary>
public sealed class RelicDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    /// <summary>Design axis: Payout | Accounting | Survival (the rework catalog; design/10).</summary>
    public string Axis { get; }

    /// <summary>Behavior key the effect engine keys on.</summary>
    public string Op { get; }

    /// <summary>Fixed shop price, in COMPS (design/10 F: the second currency, earned by staking).</summary>
    public double Price { get; }

    public IReadOnlyDictionary<string, double> Params { get; }

    public RelicDefinition(string id, string name, string description, string axis, string op, double price,
        IReadOnlyDictionary<string, double> @params)
    {
        Id = id;
        Name = name;
        Description = description;
        Axis = axis;
        Op = op;
        Price = price;
        Params = @params;
    }
}

/// <summary>A single-use consumable as pure data. Consumables are player-TIMED verbs (design/10 D):
/// the moment of use is the skill, so their effects live at explicit call sites on Run/SweatSession
/// rather than passive hooks.</summary>
public sealed class ConsumableDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public double Price { get; }

    public ConsumableDefinition(string id, string name, string description, double price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }
}

/// <summary>
/// The economy-rework catalog (PLAN.md 2026-07-13): 3 passives on three power curves — a static
/// engine, a ratchet, and a survival piece — plus 3 consumables. The old 8-relic catalog is retired
/// (git history is the archive; design/03 keeps the parked note). All payout effects multiply into
/// the single Ticket.PayoutMultiplier product so items STACK (design/10 B2: stacking strategy is
/// the fun).
/// </summary>
public static class RelicCatalog
{
    public const string MultiplierId = "the_multiplier";
    public const string ScarTissueId = "scar_tissue";
    public const string TotemId = "totem_of_undying";

    /// <summary>Profit Boost's odds multiplier on the chosen leg (the "30%" in its copy).</summary>
    public const double ProfitBoostMult = 1.3;

    /// <summary>Double or Nothing's payout factor (the "don" entry in the factor map).</summary>
    public const double DoubleOrNothingMult = 2.0;

    /// <summary>Bookie's Marker: the fraction shaved off this round's payment.</summary>
    public const double MarkerRelief = 0.25;

    public static IReadOnlyList<RelicDefinition> All { get; } = new List<RelicDefinition>
    {
        new RelicDefinition(MultiplierId, "The Multiplier",
            "Parlays of 3 or more legs pay 1.6x.",
            "Payout", "ParlayPayoutMult", 5,
            new Dictionary<string, double> { ["minLegs"] = 3, ["mult"] = 1.6 }),

        new RelicDefinition(ScarTissueId, "Scar Tissue",
            "Every busted ticket adds scar stacks (bigger stakes scar harder). The first ticket " +
            "you place each round carries them: if it hits, its payout grows by your stacks and " +
            "they burn. Busts only feed the scar.",
            "Payout", "ScarTissue", 4,
            new Dictionary<string, double> { ["ppPerBust"] = 5.0, ["fullStakeFraction"] = 0.25 }),

        new RelicDefinition(TotemId, "Totem of Undying",
            "One charge, sold once per run, ever: when you cannot make a payment, the whole " +
            "payment is DEFERRED - your bank is untouched, and the payment plus his juice lands " +
            "on the next one. Never saves the final payment.",
            "Survival", "TotemOfUndying", 6,
            new Dictionary<string, double> { ["charges"] = 1 }),

        // ---- Charm expansion (PLAN.md rev 5, Allen 2026-07-14): 12 new passives ----

        new RelicDefinition("chalk_eater", "Chalk Eater",
            "Every winning leg at -200 or shorter feeds it: +2% payout on every ticket, " +
            "forever. Grinders get paid.",
            "Payout", "LegBandRatchet", 5,
            new Dictionary<string, double> { ["maxOdds"] = 1.50, ["ppPerLeg"] = 2.0 }),

        new RelicDefinition("longshot_photo", "Longshot Larry's Photo",
            "Any ticket holding a live leg at +200 or longer pays 1.6x. Larry believes in you.",
            "Payout", "LegBandProductFlag", 5,
            new Dictionary<string, double> { ["minOdds"] = 3.00, ["mult"] = 1.6 }),

        new RelicDefinition("iron_hands", "Iron Hands",
            "Every ticket that WINS at full ride adds +4% payout to everything. Cash ANY " +
            "ticket out and the stack shatters to zero.",
            "Payout", "FullRideRatchet", 5,
            new Dictionary<string, double> { ["ppPerWin"] = 4.0 }),

        new RelicDefinition("golden_parachute", "Golden Parachute",
            "The book waives its margin on cash-outs: every cash-out pays 8% more.",
            "Accounting", "CashOutQuoteScale", 4,
            new Dictionary<string, double> { ["scale"] = 1.08 }),

        new RelicDefinition("rakes_rebate", "The Rake's Rebate",
            "Every time the shop opens, your held comps earn 10% interest. Loyalty pays " +
            "the loyal.",
            "Accounting", "ShopEnterCompsInterest", 6,
            new Dictionary<string, double> { ["rate"] = 0.10 }),

        new RelicDefinition("whale_card", "Whale Card",
            "Every comp you HOLD when the round locks adds +0.5% payout to every ticket. " +
            "Big balances get big treatment.",
            "Payout", "CompsHeldProduct", 6,
            new Dictionary<string, double> { ["perComp"] = 0.005 }),

        new RelicDefinition("bad_beat_jar", "Bad Beat Jar",
            "Every round where every ticket you placed dies drops +10% payout in the jar, " +
            "forever. The jar remembers.",
            "Payout", "AllLossRoundRatchet", 4,
            new Dictionary<string, double> { ["ppPerRound"] = 10.0 }),

        new RelicDefinition("house_key", "House Key",
            "All payouts x1.4 - and every remaining payment runs 15% heavier while you " +
            "hold it. Power has rent.",
            "Survival", "ProductPlusPaymentFactor", 7,
            new Dictionary<string, double> { ["mult"] = 1.4, ["paymentFactor"] = 1.15 }),

        new RelicDefinition("the_system", "The System",
            "Each consecutive profitable round adds +12% payout. One down round and the " +
            "binder starts over.",
            "Payout", "StreakRatchet", 5,
            new Dictionary<string, double> { ["ppPerRound"] = 12.0 }),

        new RelicDefinition("compd_suite", "Comp'd Suite",
            "A winning ticket with 4+ legs comps you 8 on the spot. The book loves a show-off.",
            "Accounting", "LegCountCompsPay", 4,
            new Dictionary<string, double> { ["minLegs"] = 4, ["comps"] = 8.0 }),

        new RelicDefinition("bobblehead", "Unopened Bobblehead",
            "Does nothing. Collectors pay double.",
            "Accounting", "NoOp", 2,
            new Dictionary<string, double> { ["resaleMult"] = 2.0 }),

        new RelicDefinition("the_collection", "The Collection",
            "Every comp of resale value across your owned passives adds +1% payout at lock. " +
            "Full shelves, full pockets.",
            "Payout", "ResaleValueProduct", 5,
            new Dictionary<string, double> { ["perComp"] = 0.01 }),
    };

    public static IReadOnlyList<ConsumableDefinition> Consumables { get; } = new List<ConsumableDefinition>
    {
        new ConsumableDefinition("mulligan_slip", "Mulligan Slip",
            "Play it the moment a leg dies on a multi-leg ticket: the leg is voided and the " +
            "ticket lives. The window closes when the ticket settles.", 1.5),

        new ConsumableDefinition("profit_boost", "Profit Boost",
            "Play it at the betslip: one chosen leg's odds are boosted 30% before you lock.", 2.5),

        // Timeout CUT at playtest #8 (Allen: "useless") — the sim's ≈0 audit agreed. The engine's
        // offer-hold seam (OfferHoldEffect via ApplyLiveEffect) stays: it is the design/04 Band-2
        // intervention plumbing, and future actives may buy it back.

        // ---- Charm expansion (PLAN.md rev 5): 5 new consumables ----

        new ConsumableDefinition("free_bet", "Free Bet Token",
            "Flag one ticket before you lock: if it loses, the stake comes back in cash. " +
            "The bust still counts everywhere it hurts.", 3),

        new ConsumableDefinition("ask_manager", "Ask for the Manager",
            "Play it in the shop: the whole hand is redealt once. He is not happy about it.", 1),

        new ConsumableDefinition("double_or_nothing", "Double or Nothing Slip",
            "Flag one ticket before you lock: it pays DOUBLE if it wins - and the cash-out " +
            "button is gone. No exits.", 2),

        new ConsumableDefinition("bookies_marker", "Bookie's Marker",
            "Play it before you lock: this round's payment drops 25%. One per round - " +
            "he keeps count.", 3),

        new ConsumableDefinition("refs_whistle", "Ref's Whistle",
            "Play it the moment a leg dies: the call goes to review at the odds you were " +
            "living on. Overturned, the leg stands at FULL odds - confirmed, the ticket dies. " +
            "Works where a Mulligan cannot: single-leg tickets.", 2),
    };
}
