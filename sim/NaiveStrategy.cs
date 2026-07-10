using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.Sim;

/// <summary>
/// NAIVE — the PRD S3 baseline: "bet favorites, never cash out, no relics" (target: median death round 3–4).
///
/// Policy, every round:
///   • One 2-leg parlay on the two biggest favorites. The bot cannot see true probs, so it reads the
///     ODDS: the two shortest-priced sides on the slate (each matchup's favorite is its shorter side, so
///     the two globally shortest sides always land on distinct matchups — a legal ticket).
///   • Stake = 25% of the current bank, floored to whole dollars, never below MinStake; skip the round's
///     bet entirely if the bank is under MinStake (per the OPEN-QUESTIONS playtest note — a bank fraction,
///     NOT the old flat $50 cap that made a naive bettor mathematically unable to clear round 1).
///   • Never cashes out (ControlsSweat = false → the harness fast-forwards the sweat).
///   • Buys nothing.
///
/// HONESTY: reads only Matchup.Odds/HomeOdds/AwayOdds (public book prices). Never touches true probs.
/// </summary>
public sealed class NaiveStrategy : IStrategy
{
    public string Name => "naive";
    public bool ControlsSweat => false;

    private const double StakeFraction = 0.25;
    private const int Legs = 2;

    public void Bet(Run run, BotState state, Pcg32 rng)
    {
        if (run.Bank < run.Config.MinStake) return;

        double stake = Math.Max(run.Config.MinStake, Math.Floor(StakeFraction * run.Bank));
        if (stake > run.Bank) return; // defensive; StakeFraction < 1 keeps this from tripping

        IReadOnlyList<Matchup> slate = run.CurrentSlate.Matchups;
        if (slate.Count < Legs) return;

        // Each matchup's favorite (shorter-priced side) and its odds.
        var favs = new List<(int matchup, Side side, double odds)>(slate.Count);
        foreach (Matchup m in slate)
        {
            Side fav = m.HomeOdds <= m.AwayOdds ? Side.Home : Side.Away;
            favs.Add((m.Index, fav, m.Odds(fav)));
        }
        favs.Sort((a, b) => a.odds.CompareTo(b.odds)); // shortest-priced first

        var picks = new List<Pick>(Legs);
        for (int i = 0; i < Legs; i++)
            picks.Add(new Pick(favs[i].matchup, favs[i].side));

        run.PlaceTicket(picks, stake);
    }

    // Never called (ControlsSweat is false), but the interface requires it.
    public bool ShouldCashOut(Run run, Ticket ticket, SweatSession session, DramaEvent evt,
        double offer, double bankNow, double target, BotState state, Pcg32 rng) => false;

    public void Shop(Run run, BotState state, Pcg32 rng) { /* buys nothing */ }
}
