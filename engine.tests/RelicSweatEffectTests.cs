using System.Linq;
using SBR.Engine;

namespace SBR.Engine.Tests;

// The sweat-time / settlement relics: insurance, mulligan, lucky_charm, early_payout, piggy_bank,
// and the acquisition-order chain. Seeds (config RelicKit.Cheap) found by scan:
//   R3-1    offers bankroll_insurance
//   R3-4    offers mulligan, piggy_bank
//   R3-163  offers lucky_charm; first Relics roll 0.00163 (< 0.03 → a losing final leg flips)
//   R3-2    offers lucky_charm; first Relics roll 0.32   (a losing final leg stays lost)
//   R3-0    offers early_payout
//   R3-82   offers mulligan + early_payout
//   R3-11   offers piggy_bank + boosted_odds
//   R3-4661 offers bankroll_insurance + piggy_bank + lucky_charm; first roll 0.00318 (flip)
public class RelicSweatEffectTests
{
    // ---- bankroll_insurance ----

    [Fact]
    public void Insurance_refunds_the_first_bust_each_round_and_recharges_next_round()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-1", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-1", cfg, "bankroll_insurance");

        Ticket a = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r2, 0))), 100);
        Ticket b = run.PlaceTicket(RelicKit.Picks((1, RelicKit.Lose(r2, 1))), 200);
        double bankAfterBets = run.Bank;
        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(TicketState.Lost, a.State);
        Assert.Equal(TicketState.Lost, b.State);
        Assert.Equal(bankAfterBets + 0.5 * 100, run.Bank, 6); // only the first bust refunds

        // Round 3: the charge is back.
        Side[] r3 = RelicKit.RoundResults("R3-1", cfg, 3);
        run.Settle(); run.ExitShop();
        Ticket c = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r3, 0))), 80);
        double bank3 = run.Bank;
        run.LockRound();
        run.FastForwardRound();

        Assert.Equal(TicketState.Lost, c.State);
        Assert.Equal(bank3 + 0.5 * 80, run.Bank, 6);
    }

    // ---- mulligan ----

    [Fact]
    public void Mulligan_voids_a_dead_middle_leg_and_the_ticket_wins_at_the_reduced_payout()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "mulligan");

        Ticket t = run.PlaceTicket(RelicKit.Picks(
            (0, RelicKit.Win(r2, 0)), (1, RelicKit.Lose(r2, 1)), (2, RelicKit.Win(r2, 2))), 100);
        double bankAfterBet = run.Bank;
        double threeLegPayout = t.PotentialPayout;
        run.LockRound();
        double reduced = 100 * t.Legs[0].OfferedOdds * t.Legs[2].OfferedOdds;
        run.FastForwardRound();

        Assert.True(t.Legs[1].IsVoided);
        Assert.False(t.Legs[0].IsVoided);
        Assert.False(t.Legs[2].IsVoided);
        Assert.Equal(TicketState.Won, t.State);
        Assert.True(reduced < threeLegPayout);
        Assert.Equal(reduced, t.PotentialPayout, 6);   // dropped to the two-leg product
        Assert.Equal(bankAfterBet + reduced, run.Bank, 6);
    }

    [Fact]
    public void Mulligan_does_not_save_a_single_leg_ticket()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "mulligan");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r2, 0))), 100);
        run.LockRound();
        run.FastForwardRound();

        Assert.False(t.Legs[0].IsVoided);
        Assert.Equal(TicketState.Lost, t.State);
    }

    [Fact]
    public void Mulligan_fires_only_once_per_round_across_tickets()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "mulligan");

        Ticket a = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Win(r2, 0)), (1, RelicKit.Lose(r2, 1))), 100);
        Ticket b = run.PlaceTicket(RelicKit.Picks((2, RelicKit.Win(r2, 2)), (3, RelicKit.Lose(r2, 3))), 100);
        run.LockRound();
        run.FastForwardRound();

        Assert.True(a.Legs[1].IsVoided);       // first ticket spends the mulligan
        Assert.Equal(TicketState.Won, a.State);
        Assert.False(b.Legs[1].IsVoided);      // none left for the second
        Assert.Equal(TicketState.Lost, b.State);
    }

    [Fact]
    public void Cash_out_after_a_void_prices_only_the_surviving_legs()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "mulligan");

        Ticket t = run.PlaceTicket(RelicKit.Picks(
            (0, RelicKit.Win(r2, 0)), (1, RelicKit.Lose(r2, 1)), (2, RelicKit.Win(r2, 2))), 200);
        run.LockRound();
        SweatSession s = run.Sweats[0];

        while (s.RevealedLegState(1) == LegState.Pending) s.MoveNext(out _);
        Assert.True(t.Legs[1].IsVoided);
        Assert.Equal(LegState.Won, s.RevealedLegState(0));

        // leg0 settled Won × leg2 current (live prob = its true prob at leg start), voided leg1 skipped.
        double liveProb = t.Legs[2].TrueProb;
        double expected = 200 * t.Legs[0].OfferedOdds * liveProb * t.Legs[2].OfferedOdds;
        Assert.Equal(expected, s.CashOutFair()!.Value, 6);
    }

    // ---- lucky_charm ----

    [Fact]
    public void Lucky_charm_flips_a_losing_final_leg_without_touching_the_matchup()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-163", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-163", cfg, "lucky_charm");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r2, 0))), 100);
        double bankAfterBet = run.Bank;
        run.LockRound();
        Matchup m0 = run.CurrentSlate.Matchups[0];
        Side losingResult = m0.Result!.Value;
        run.FastForwardRound();

        Assert.Equal(losingResult, m0.Result);          // engine truth: untouched
        Assert.NotEqual(t.Legs[0].Side, m0.Result!.Value); // the pick genuinely lost
        Assert.Equal(LegState.Lost, t.Legs[0].State);   // source state still Lost
        Assert.True(t.Legs[0].LuckyFlippedWon);         // graded Won for THIS ticket only
        Assert.Equal(TicketState.Won, t.State);
        Assert.Equal(bankAfterBet + 100 * t.Legs[0].OfferedOdds, run.Bank, 6); // full payout
    }

    [Fact]
    public void Lucky_charm_roll_can_fail_and_the_final_leg_stays_lost()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-2", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-2", cfg, "lucky_charm");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r2, 0))), 100);
        run.LockRound();
        run.FastForwardRound();

        Assert.False(t.Legs[0].LuckyFlippedWon);
        Assert.Equal(TicketState.Lost, t.State);
    }

    [Fact]
    public void Lucky_charm_threshold_adds_exactly_boostPp_of_win_probability()
    {
        double boostPp = RelicCatalog.All.First(r => r.Id == "lucky_charm").Params["boostPp"];
        Assert.Equal(0.03, boostPp, 12);

        foreach (double p in new[] { 0.25, 0.5, 0.6, 0.75 })
        {
            double threshold = boostPp / (1.0 - p);           // P(roll < threshold) = threshold
            double effectiveWin = p + (1.0 - p) * threshold;  // lose w.p. (1-p), then flip w.p. threshold
            Assert.Equal(p + boostPp, effectiveWin, 12);
        }
    }

    // ---- early_payout ----

    [Fact]
    public void Early_payout_credits_each_green_leg_between_events_and_pays_full_on_a_win()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-0", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-0", cfg, "early_payout");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Win(r2, 0)), (1, RelicKit.Win(r2, 1))), 100);
        double bankAfterBet = run.Bank;
        run.LockRound();
        SweatSession s = run.Sweats[0];

        while (s.RevealedLegState(0) == LegState.Pending) s.MoveNext(out _);
        Assert.Equal(bankAfterBet + 0.15 * 100, run.Bank, 6);

        while (s.RevealedLegState(1) == LegState.Pending) s.MoveNext(out _);
        Assert.Equal(bankAfterBet + 2 * 0.15 * 100, run.Bank, 6);

        run.FinishSweat();
        Assert.Equal(TicketState.Won, t.State);
        Assert.Equal(bankAfterBet + 2 * 0.15 * 100 + t.PotentialPayout, run.Bank, 6); // partials on top of full payout
    }

    [Fact]
    public void Early_payout_prices_future_partials_into_the_cash_out_fair_value()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-0", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-0", cfg, "early_payout");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Win(r2, 0)), (1, RelicKit.Win(r2, 1))), 200);
        run.LockRound();
        SweatSession s = run.Sweats[0];

        double q1 = t.Legs[0].TrueProb, q2 = t.Legs[1].TrueProb;
        double baseFair = OddsMath.CashOutFair(200, 1.0,
            new[] { (q1, t.Legs[0].OfferedOdds), (q2, t.Legs[1].OfferedOdds) });
        double b = 0.15 * 200;
        double futurePartials = b * q1 + b * q1 * q2;

        Assert.Equal(baseFair + futurePartials, s.CashOutFair()!.Value, 6);
    }

    [Fact]
    public void Early_payout_pays_no_partial_for_a_voided_leg()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-82", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-82", cfg, "early_payout", "mulligan");

        Ticket t = run.PlaceTicket(RelicKit.Picks(
            (0, RelicKit.Win(r2, 0)), (1, RelicKit.Lose(r2, 1)), (2, RelicKit.Win(r2, 2))), 100);
        double bankAfterBet = run.Bank;
        run.LockRound();
        run.FastForwardRound();

        Assert.True(t.Legs[1].IsVoided);
        Assert.Equal(TicketState.Won, t.State);
        // Two partials (legs 0 and 2), none for the voided leg, plus the two-leg payout.
        Assert.Equal(bankAfterBet + 2 * 0.15 * 100 + t.PotentialPayout, run.Bank, 6);
    }

    // ---- piggy_bank ----

    [Fact]
    public void Piggy_bank_accrues_twice_the_paid_vig_at_lock()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "piggy_bank");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Win(r2, 0)), (1, RelicKit.Win(r2, 1))), 100);
        Assert.Equal(0.0, run.PiggyBankBalance); // nothing accrued until lock
        Assert.True(t.VigPaid > 0);
        run.LockRound();
        Assert.Equal(2.0 * t.VigPaid, run.PiggyBankBalance, 6);
    }

    [Fact]
    public void Piggy_bank_accrues_zero_on_a_negative_vig_ticket()
    {
        var cfg = RelicKit.Cheap();
        Run run = RelicKit.Round2Owning("R3-11", cfg, "piggy_bank", "boosted_odds");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, Side.Away)), 100); // boosted single → priced above fair
        Assert.True(t.VigPaid < 0);
        run.LockRound();
        Assert.Equal(0.0, run.PiggyBankBalance, 9); // 2 × max(0, negative vig) = 0
    }

    [Fact]
    public void Piggy_bank_smashes_into_the_bank_on_a_bust_and_resets()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "piggy_bank");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r2, 0))), 100);
        double bankAfterBet = run.Bank;
        run.LockRound();
        double piggy = run.PiggyBankBalance;
        Assert.True(piggy > 0);
        run.FastForwardRound();

        Assert.Equal(TicketState.Lost, t.State);
        Assert.Equal(bankAfterBet + piggy, run.Bank, 6);
        Assert.Equal(0.0, run.PiggyBankBalance, 9);
    }

    [Fact]
    public void Piggy_bank_balance_survives_a_round_with_no_bust()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4", cfg, "piggy_bank");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Win(r2, 0))), 100);
        run.LockRound();
        double piggy = run.PiggyBankBalance;
        Assert.True(piggy > 0);
        run.FastForwardRound();

        Assert.Equal(TicketState.Won, t.State);
        Assert.Equal(piggy, run.PiggyBankBalance, 9); // no bust → not smashed
        run.Settle(); run.ExitShop();
        Assert.Equal(piggy, run.PiggyBankBalance, 9); // persists across the round boundary
    }

    // ---- acquisition-order chain ----

    [Fact]
    public void A_lucky_flip_stops_the_bust_chain_so_insurance_and_piggy_do_not_fire()
    {
        var cfg = RelicKit.Cheap();
        Side[] r2 = RelicKit.RoundResults("R3-4661", cfg, 2);
        Run run = RelicKit.Round2Owning("R3-4661", cfg, "bankroll_insurance", "piggy_bank", "lucky_charm");

        Ticket t = run.PlaceTicket(RelicKit.Picks((0, RelicKit.Lose(r2, 0))), 100);
        double bankAfterBet = run.Bank;
        run.LockRound();
        double piggy = run.PiggyBankBalance;
        Assert.True(piggy > 0); // accrued at lock
        Matchup m0 = run.CurrentSlate.Matchups[0];
        Side losingResult = m0.Result!.Value;
        run.FastForwardRound();

        Assert.Equal(losingResult, m0.Result);        // matchup untouched
        Assert.Equal(TicketState.Won, t.State);        // lucky flip won it — no bust
        Assert.Equal(piggy, run.PiggyBankBalance, 9);  // piggy did not smash
        // Bank reflects only the win payout: no insurance refund, no piggy smash.
        Assert.Equal(bankAfterBet + 100 * t.Legs[0].OfferedOdds, run.Bank, 6);
    }
}
