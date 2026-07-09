using System.Collections.Generic;

namespace SBR.Engine;

/// <summary>
/// One relic as pure data (PRD F8): an id, display copy, the design axis it sits on, a behavior
/// key (<see cref="Op"/>) the effect engine dispatches on, a fixed price, and a parameter bag the
/// matching behavior reads. Content lives here so the catalog can grow without touching engine logic.
/// </summary>
public sealed class RelicDefinition
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }

    /// <summary>Design axis: Information | Odds | Capital | Resolution | PayoffStructure | Accounting.</summary>
    public string Axis { get; }

    /// <summary>Behavior key the effect engine keys on.</summary>
    public string Op { get; }

    /// <summary>Fixed shop price, 150..400.</summary>
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

/// <summary>The fixed 10-relic catalog for v0 (PRD F8). Order is stable; the shop draws from it.</summary>
public static class RelicCatalog
{
    public static IReadOnlyList<RelicDefinition> All { get; } = new List<RelicDefinition>
    {
        new RelicDefinition("tout_sheet", "Tout Sheet",
            "At the start of each round, reveals a tight probability band around the truth for two games.",
            "Information", "RevealTrueProb", 200,
            new Dictionary<string, double> { ["count"] = 2, ["halfWidthPp"] = 0.05 }),

        new RelicDefinition("sharp_eye", "Sharp Eye",
            "Once per round, study one line: reveals its exact true win probability.",
            "Information", "RevealLineTrueProb", 250,
            new Dictionary<string, double> { ["usesPerRound"] = 1 }),

        new RelicDefinition("boosted_odds", "Boosted Odds",
            "The first leg of every ticket has its odds boosted by 15%.",
            "Odds", "BoostLegOdds", 300,
            new Dictionary<string, double> { ["legIndex"] = 0, ["mult"] = 1.15 }),

        new RelicDefinition("promo_code", "Promo Code",
            "Your first ticket each round is priced at fair odds, no vig.",
            "Odds", "FairOddsFirstTicket", 350,
            new Dictionary<string, double>()),

        new RelicDefinition("high_roller", "High Roller",
            "Doubles the maximum stake you may put on a single ticket.",
            "Capital", "MaxStakeMult", 250,
            new Dictionary<string, double> { ["mult"] = 2.0 }),

        new RelicDefinition("bankroll_insurance", "Bankroll Insurance",
            "The first ticket you bust each round refunds half its stake.",
            "Capital", "RefundBustedStake", 200,
            new Dictionary<string, double> { ["fraction"] = 0.5, ["usesPerRound"] = 1 }),

        new RelicDefinition("mulligan", "Mulligan",
            "Once per round, a dead leg on a multi-leg ticket is voided instead of killing it.",
            "Resolution", "VoidDeadLeg", 400,
            new Dictionary<string, double> { ["usesPerRound"] = 1 }),

        new RelicDefinition("lucky_charm", "Lucky Charm",
            "Your ticket's final leg gets a small second-chance shot at winning when it would lose.",
            "Resolution", "SecondChanceFinalLeg", 300,
            new Dictionary<string, double> { ["boostPp"] = 0.03 }),

        new RelicDefinition("early_payout", "Early Payout",
            "Each leg that comes in pays you 15% of your stake immediately, on top of the parlay.",
            "PayoffStructure", "PayPerGreenLeg", 350,
            new Dictionary<string, double> { ["fractionOfStake"] = 0.15 }),

        new RelicDefinition("piggy_bank", "Piggy Bank",
            "Banks twice the vig you pay; when a ticket busts, the piggy smashes into your bankroll.",
            "Accounting", "VigRebate", 150,
            new Dictionary<string, double> { ["mult"] = 2.0 }),
    };
}
