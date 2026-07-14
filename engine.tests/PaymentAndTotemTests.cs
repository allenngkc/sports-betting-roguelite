using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The payment settle (economy rework, PLAN.md 2026-07-13): payments DEDUCT, shortfalls kill,
/// and the Totem of Undying is the only mercy — the whole payment defers (bank untouched,
/// playtest #8: mercy must leave working capital), payment × (1 + juice) lands on the next one,
/// and it never saves the final payment.
/// </summary>
public class PaymentAndTotemTests
{
    private static RelicDefinition Totem =>
        RelicCatalog.All.First(r => r.Id == RelicCatalog.TotemId);

    [Fact]
    public void Totem_defers_a_non_final_payment_leaving_the_bank_untouched()
    {
        var cfg = new RunConfig
        {
            StartingBank = 500,
            Payments = new double[] { 800, 400 },
            TotemJuiceRate = 0.5, // pinned: the surcharge math, not the tuned default
        };
        var run = new Run("TOTEM-FIRE", cfg);
        run.GrantRelic(Totem);

        run.LockRound();
        run.FastForwardRound();
        run.Settle(); // 500 < 800: the totem fires

        Assert.Equal(Phase.Shop, run.Phase);
        Assert.Equal(500, run.Bank, 10); // working capital survives (playtest #8)
        SettlementReport report = run.LastSettlement!.Value;
        Assert.True(report.TotemFired);
        Assert.Equal(500, report.BankAfter, 10);
        Assert.Equal(300, report.Shortfall, 10); // informational: how short you were

        run.ExitShop();
        Assert.Equal(400 + 800 * 1.5, run.CurrentPayment, 10); // full payment × (1 + juice) lands on P2
    }

    [Fact]
    public void Totem_never_saves_the_final_payment()
    {
        var cfg = new RunConfig { StartingBank = 500, Payments = new double[] { 800 } };
        var run = new Run("TOTEM-FINAL", cfg);
        run.GrantRelic(Totem);

        run.LockRound();
        run.FastForwardRound();
        run.Settle();

        Assert.Equal(Phase.RunLost, run.Phase);
        Assert.False(run.LastSettlement!.Value.TotemFired); // the charge is not even consumed
    }

    [Fact]
    public void Totem_charge_is_single_use()
    {
        var cfg = new RunConfig
        {
            StartingBank = 500,
            Payments = new double[] { 600, 10000, 10 },
            TotemJuiceRate = 0.5,
        };
        var run = new Run("TOTEM-ONCE", cfg);
        run.GrantRelic(Totem);

        run.LockRound(); run.FastForwardRound(); run.Settle(); // 500 < 600 → totem fires, bank stays 500
        Assert.True(run.LastSettlement!.Value.TotemFired);
        run.ExitShop();

        run.LockRound(); run.FastForwardRound(); run.Settle(); // bank ~500 vs 10900: no charge left
        Assert.Equal(Phase.RunLost, run.Phase);
        Assert.False(run.LastSettlement!.Value.TotemFired);
    }

    [Fact]
    public void Settlement_report_carries_the_paid_round_numbers()
    {
        var cfg = new RunConfig { StartingBank = 500, Payments = new double[] { 150, 150 } };
        var run = new Run("REPORT", cfg);
        run.LockRound();
        run.FastForwardRound();
        run.Settle();

        SettlementReport r = run.LastSettlement!.Value;
        Assert.Equal(1, r.Round);
        Assert.Equal(150, r.Payment, 10);
        Assert.Equal(500, r.BankBefore, 10);
        Assert.Equal(350, r.BankAfter, 10);
        Assert.True(r.Paid);
        Assert.Equal(Phase.Shop, r.Outcome);
    }
}
