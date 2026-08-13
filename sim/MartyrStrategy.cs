using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// MARTYR — the G6 adversary, upgraded for the charm expansion (rev 5 §14): farms EVERY
/// loss-feeder — Scar Tissue stacks, Bad Beat Jar rounds, and Free Bet refunded busts — then
/// cashes on one safe carrier. If this beats organic skilled play by more than the gate's
/// margin, the farming guards have failed. The WORST-CASE variant (G6's actual gate) gets Scar
/// + Jar granted and a Free Bet refilled every round by the harness.
///
/// Policy:
///   • Shop: buys scar_tissue, bad_beat_jar, and free_bet tokens — nothing else.
///   • Farming rounds (stacks below the cash-in bar): two 2-leg LONGSHOT parlays (the lowest-p̂
///     sides) staked at the full-scar fraction — deliberately likely busts; a held Free Bet
///     rides the first farm ticket (refunded martyrdom — the loss still feeds Scar AND Jar).
///   • Cash-in rounds (stacks ≥ the bar): ONE ticket placed first (the carrier) — the single
///     highest-p̂ side, staked with everything above the payment reserve.
///   • Cash-out: survival-take only. Saves: never — busts are the harvest.
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

    /// <summary>The martyr NEVER saves a dying leg — busts are the harvest.</summary>
    public PendingLossAction ChoosePendingLossAction(Run run, Ticket ticket, SweatSession session,
        BotState state, Pcg32 rng) => PendingLossAction.Decline;

    public void Bet(Run run, BotState state, Pcg32 rng)
    {
        IReadOnlyList<Matchup> slate = run.CurrentSlate.Matchups;
        var sides = new List<(int m, Side s, double pHat, double odds)>(slate.Count);
        foreach (Matchup m in slate)
        {
            // 1X2 de-vig: the draw's implied probability belongs in the denominator. Normalizing
            // Home against Away ALONE recovers P(home | decisive) while a moneyline leg pays on
            // P(home) — the martyr would have farmed against the wrong probabilities, and it is
            // G6's guard bot, so that error would have landed inside a gate rather than beside it.
            // The away estimate is de-vigged in its own right, not taken as 1 − pHome, which stops
            // being the complement the moment a third outcome exists.
            double total = 1.0 / m.HomeOdds + 1.0 / m.DrawOdds + 1.0 / m.AwayOdds;
            // Track BOTH sides; the farmer wants longshots, the carrier wants the favorite.
            // The DRAW is deliberately not tracked here: this bot's identity is picking a TEAM
            // badly, and widening its surface would silently redefine G6's guard (the F_0.4.0 P5
            // precedent — one inheritance default changed the meaning of the campaign).
            sides.Add((m.Index, Side.Home, (1.0 / m.HomeOdds) / total, m.HomeOdds));
            sides.Add((m.Index, Side.Away, (1.0 / m.AwayOdds) / total, m.AwayOdds));
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
            run.PlaceTicket(new List<Pick> { new Pick(best.m, MarketSelection.Moneyline(best.s)) }, stake);
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
                new Pick(legsPool[t * 2].m, MarketSelection.Moneyline(legsPool[t * 2].s)),
                new Pick(legsPool[t * 2 + 1].m, MarketSelection.Moneyline(legsPool[t * 2 + 1].s)),
            };
            // Refunded martyrdom: a held Free Bet rides the first farm ticket — the bust still
            // feeds Scar and the Jar, but the stake comes home.
            TicketModifier mod = t == 0 && run.OwnsConsumable("free_bet")
                ? TicketModifier.FreeBet : TicketModifier.None;
            run.PlaceTicket(picks, stake, -1, mod);
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
        // Loss-feeders only: Scar, Jar, and Free Bet tokens (rev 5 §14).
        bool bought = true;
        while (bought)
        {
            bought = false;
            if (run.OwnedRelics.Count < run.Config.RelicSlots)
            {
                for (int i = 0; i < run.ShopOffers.Count; i++)
                {
                    string id = run.ShopOffers[i].Id;
                    if (id != RelicCatalog.ScarTissueId && id != "bad_beat_jar") continue;
                    if (run.ShopOffers[i].Price > run.Comps) continue;
                    run.BuyRelic(i);
                    bought = true;
                    break;
                }
                if (bought) continue;
            }

            if (run.OwnedConsumables.Count < run.Config.ConsumableSlots)
            {
                for (int i = 0; i < run.ConsumableOffers.Count; i++)
                {
                    if (run.ConsumableOffers[i].Id != "free_bet") continue;
                    if (run.ConsumableOffers[i].Price > run.Comps) continue;
                    run.BuyConsumable(i);
                    bought = true;
                    break;
                }
            }
        }
    }
}
