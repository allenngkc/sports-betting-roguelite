using SBR.Engine;

namespace SBR.Engine.Tests;

/// <summary>
/// The payment settle (economy rework, PLAN.md 2026-07-13): payments DEDUCT, shortfalls kill,
/// and the Totem of Undying is the only mercy — an itemized float that surcharges the next
/// payment and never saves the final one.
/// </summary>
public class PaymentAndTotemTests
{
    private static RelicDefinition Totem =>
        RelicCatalog.All.First(r => r.Id == RelicCatalog.TotemId);

    [Fact]
    public void Totem_covers_a_non_final_shortfall_and_surcharges_the_next_payment()
    {
        var cfg = new RunConfig { StartingBank = 500, Payments = new double[] { 800, 400 } };
        var run = new Run("TOTEM-FIRE", cfg);
        run.GrantRelic(Totem);

        run.LockRound();
        run.FastForwardRound();
        run.Settle(); // 500 < 800: the totem fires

        Assert.Equal(Phase.Shop, run.Phase);
        Assert.Equal(0, run.Bank, 10); // the bookie takes everything you have
        SettlementReport report = run.LastSettlement!.Value;
        Assert.True(report.TotemFired);
        Assert.Equal(300, report.Shortfall, 10);

        run.ExitShop();
        Assert.Equal(400 + 300 * 1.5, run.CurrentPayment, 10); // shortfall × (1 + juice) lands on P2
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
        var cfg = new RunConfig { StartingBank = 500, Payments = new double[] { 600, 10000, 10 } };
        var run = new Run("TOTEM-ONCE", cfg);
        run.GrantRelic(Totem);

        run.LockRound(); run.FastForwardRound(); run.Settle(); // shortfall 100 → totem fires
        Assert.True(run.LastSettlement!.Value.TotemFired);
        run.ExitShop();

        run.LockRound(); run.FastForwardRound(); run.Settle(); // bank 0 vs 10150: no charge left
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
