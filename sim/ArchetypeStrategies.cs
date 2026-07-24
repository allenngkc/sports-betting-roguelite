using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// The archetype TELEMETRY bots (PLAN.md rev 5 §14): thin SkilledStrategy variants whose tier
/// lists and dials chase one named build each. They never gate — their curves sit beside
/// skilled in the report so Allen can see whether the archetypes breathe. Same honesty rules.
/// </summary>
public sealed class ChalkGrinderStrategy : SkilledStrategy
{
    private static readonly string[] Priority =
    {
        "chalk_eater", RelicCatalog.MultiplierId, "the_system", "compd_suite",
        "whale_card", RelicCatalog.TotemId, "rakes_rebate", "golden_parachute",
        RelicCatalog.ScarTissueId, "iron_hands", "longshot_photo", "house_key",
        "the_collection",
    };

    public override string Name => "chalk";
    protected override string[] RelicPriorityList => Priority;
    protected override bool IncludesMarketOffers => false;

    // Chalk lives on leg volume: always stretch to 4 favorites when the slate allows.
    protected override int PrimaryLegCap(Run run) => 4;
}

public sealed class VipHoarderStrategy : SkilledStrategy
{
    private static readonly string[] Priority =
    {
        "rakes_rebate", "whale_card", "compd_suite", RelicCatalog.MultiplierId,
        RelicCatalog.TotemId, "the_system", "chalk_eater", "golden_parachute",
        RelicCatalog.ScarTissueId, "longshot_photo", "iron_hands", "house_key",
        "the_collection",
    };

    public override string Name => "hoarder";
    protected override string[] RelicPriorityList => Priority;
    protected override bool IncludesMarketOffers => false;

    // The hoard IS the build: once the engine pieces exist, comps stay home.
    protected override double CompsHoldFloor(Run run)
        => OwnsRelic(run, "whale_card") || OwnsRelic(run, "rakes_rebate") ? 40.0 : 10.0;
}

public sealed class IronHandsStrategy : SkilledStrategy
{
    private static readonly string[] Priority =
    {
        "iron_hands", RelicCatalog.MultiplierId, "the_system", "longshot_photo",
        "chalk_eater", RelicCatalog.TotemId, "whale_card", "compd_suite",
        RelicCatalog.ScarTissueId, "rakes_rebate", "house_key", "the_collection",
        "golden_parachute",
    };

    public override string Name => "ironhands";
    protected override string[] RelicPriorityList => Priority;
    protected override bool IncludesMarketOffers => false;

    // Commitment as identity: DoN whenever held (the stack only grows on full rides), and
    // cash-out only as literal survival (the base survival-take rule stays in ShouldCashOut
    // via target-clearing; EV-ratio exits are off).
    protected override TicketModifier PickModifier(Run run, double planWinProb, double planTicketEv)
    {
        if (run.OwnsConsumable("double_or_nothing")) return TicketModifier.DoubleOrNothing;
        if (run.OwnsConsumable("free_bet")) return TicketModifier.FreeBet;
        return TicketModifier.None;
    }

    public override bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng)
    {
        if (evt.Type == DramaEventType.LegFinal) return false;
        double remainingNeeded = target - bankNow;
        return remainingNeeded > 0 && offer >= remainingNeeded; // survival only — hands of iron
    }
}
