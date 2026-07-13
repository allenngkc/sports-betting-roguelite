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

    /// <summary>Fixed shop price.</summary>
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

    public static IReadOnlyList<RelicDefinition> All { get; } = new List<RelicDefinition>
    {
        new RelicDefinition(MultiplierId, "The Multiplier",
            "Parlays of 3 or more legs pay 1.5x.",
            "Payout", "ParlayPayoutMult", 100,
            new Dictionary<string, double> { ["minLegs"] = 3, ["mult"] = 1.5 }),

        new RelicDefinition(ScarTissueId, "Scar Tissue",
            "Every busted ticket adds scar stacks (bigger stakes scar harder). The first ticket " +
            "you place each round carries them: if it hits, its payout grows by your stacks and " +
            "they burn. Busts only feed the scar.",
            "Payout", "ScarTissue", 80,
            new Dictionary<string, double> { ["ppPerBust"] = 5.0, ["fullStakeFraction"] = 0.25 }),

        new RelicDefinition(TotemId, "Totem of Undying",
            "One charge, sold once per run, ever: when you cannot make a payment, the bookie " +
            "covers it - and your next payment grows by the shortfall, plus his juice. Never " +
            "saves the final payment.",
            "Survival", "TotemOfUndying", 120,
            new Dictionary<string, double> { ["charges"] = 1 }),
    };

    public static IReadOnlyList<ConsumableDefinition> Consumables { get; } = new List<ConsumableDefinition>
    {
        new ConsumableDefinition("mulligan_slip", "Mulligan Slip",
            "Play it the moment a leg dies on a multi-leg ticket: the leg is voided and the " +
            "ticket lives. The window closes when the ticket settles.", 40),

        new ConsumableDefinition("profit_boost", "Profit Boost",
            "Play it at the betslip: one chosen leg's odds are boosted 30% before you lock.", 30),

        new ConsumableDefinition("timeout", "Timeout",
            "Play it mid-sweat: the cash-out offer holds its price for the next 3 events. A dead " +
            "ticket still pays nothing - the hold freezes the price, not fate.", 30),
    };
}
