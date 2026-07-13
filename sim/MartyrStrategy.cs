using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// MARTYR — the G6 adversary: farms Scar Tissue stacks as ruthlessly as a human would, then
/// cashes them on one safe carrier. If this beats organic skilled play by more than the gate's
/// margin, the farming guard (stake-scaled stacks) has failed.
///
/// Policy:
///   • Shop: buys scar_tissue and nothing else (the pure-farm caricature).
///   • Farming rounds (stacks below the cash-in bar): two 2-leg LONGSHOT parlays (the lowest-p̂
///     sides) staked at the full-scar fraction (25% of bank each) — deliberately likely busts
///     that each feed ~+5pp; the payment reserve is never staked.
///   • Cash-in rounds (stacks ≥ the bar): ONE ticket placed first (the carrier) — the single
///     highest-p̂ side, staked with everything above the payment reserve.
///   • Cash-out: survival-take only (offer clears the payment when the bank cannot).
///
/// HONESTY: de-vig estimates only, like every bot.
/// </summary>
public sealed class MartyrStrategy : IStrategy
{
    private const double CashInStacks = 30.0;   // pp of scar before the martyr cashes in
    private const double FarmStakeFraction = 0.25; // the full-scar threshold — max pp per bust
    private const double PaymentReserve = 1.0;  // keep 100% of the payment out of farm stakes

    public string Name => "martyr";
    public bool ControlsSweat => true;

    public void Bet(Run run, BotState state, Pcg32 rng)
    {
        IReadOnlyList<Matchup> slate = run.CurrentSlate.Matchups;
        var sides = new List<(int m, Side s, double pHat, double odds)>(slate.Count);
        foreach (Matchup m in slate)
        {
            double implHome = 1.0 / m.HomeOdds;
            double pHome = implHome / (implHome + 1.0 / m.AwayOdds);
            // Track BOTH sides; the farmer wants longshots, the carrier wants the favorite.
            sides.Add((m.Index, Side.Home, pHome, m.HomeOdds));
            sides.Add((m.Index, Side.Away, 1.0 - pHome, m.AwayOdds));
        }

        double reserve = PaymentReserve * run.CurrentPayment;
        double budget = run.Bank - reserve;
        if (budget < run.Config.MinStake) return;

        if (run.ScarStacks >= CashInStacks)
        {
            // Cash-in: the carrier is the FIRST ticket placed — one safe single, everything spare.
            var best = sides[0];
            foreach (var c in sides) if (c.pHat > best.pHat) best = c;
            double stake = Math.Clamp(Math.Floor(budget), run.Config.MinStake, run.Bank);
            run.PlaceTicket(new List<Pick> { new Pick(best.m, best.s) }, stake);
            return;
        }

        // Farming: two 2-leg longshot parlays at the full-scar stake, distinct matchups.
        sides.Sort((a, b) => a.pHat.CompareTo(b.pHat)); // longest shots first
        var used = new HashSet<int>();
        var legsPool = new List<(int m, Side s)>();
        foreach (var c in sides)
            if (used.Add(c.m)) legsPool.Add((c.m, c.s));

        for (int t = 0; t < 2 && legsPool.Count >= (t + 1) * 2; t++)
        {
            double stake = Math.Floor(Math.Min(FarmStakeFraction * run.Bank, budget));
            if (stake < run.Config.MinStake) break;
            var picks = new List<Pick>
            {
                new Pick(legsPool[t * 2].m, legsPool[t * 2].s),
                new Pick(legsPool[t * 2 + 1].m, legsPool[t * 2 + 1].s),
            };
            run.PlaceTicket(picks, stake);
            budget -= stake;
        }
    }

    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng)
    {
        if (evt.Type == DramaEventType.LegFinal) return false;
        double remainingNeeded = target - bankNow;
        return remainingNeeded > 0 && offer >= remainingNeeded; // survival only — scars want busts
    }

    public void Shop(Run run, BotState state, Pcg32 rng)
    {
        for (int i = 0; i < run.ShopOffers.Count; i++)
        {
            if (run.ShopOffers[i].Id != RelicCatalog.ScarTissueId) continue;
            if (run.ShopOffers[i].Price > run.Comps) return;
            run.BuyRelic(i);
            return;
        }
    }
}
