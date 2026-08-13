using System;
using System.Collections.Generic;
using SBR.Engine;
using Xunit;

namespace SBR.Engine.Tests;

/// <summary>
/// T74-am4 / C49's owed arithmetic: <b>the maximum renderable payout</b>.
///
/// <para>The word the sweep used was "unbounded" and the DD corrected it: parlay multiplication is
/// not unbounded, it is <b>UN-ENUMERATED</b>. <c>MaxLegs</c> is finite, and every leg's odds come
/// from a pricing model with its own range — so the largest renderable payout is a product of finite
/// terms, and nobody had multiplied them.</para>
///
/// <para><b>This enumerates the generator's own offer space rather than reasoning about it.</b>
/// <c>MatchModel.Offer</c> prices every selection at <c>1 / (p * (1 + Overround))</c>, so the odds
/// ceiling is set by the SMALLEST true probability the generator can produce — and that is a property
/// of the slate generator's rosters and line grids, which are seeded. Sweeping seeds and taking the
/// maximum over every offer of every matchup is the inventory C18 §4.1 asks for.</para>
///
/// <para><b>Three terms, and only two of them are bounded by config — stated rather than buried:</b>
/// <list type="number">
/// <item><b>The parlay term</b> — <c>maxOdds ^ MaxLegs</c>. Enumerated here.</item>
/// <item><b>The stake</b> — <c>MaxStakeFraction * Bank</c>, and <c>MaxStakeFraction</c> is 1.0, so
/// the stake ceiling IS the bank. The bank is run state, not config: it grows with wins. So the
/// payout maximum is reported as a MULTIPLE of stake, and the dollar figure is given at named bank
/// assumptions rather than at one invented ceiling.</item>
/// <item><b>PayoutMultiplier</b> — the product of up to eleven named relic factors. It is NOT
/// included: the DD's formula names <c>MaxLegs</c>, the stake ceiling and the odds range, and folding
/// in a relic product nobody asked for would overstate the figure while looking more rigorous. It is
/// a further multiplier ON TOP of everything below, and it is named here so the omission is a stated
/// scope rather than a miss.</item>
/// </list></para>
/// </summary>
public class PayoutMaximumTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public PayoutMaximumTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    /// <summary>Seeds swept. The offer space per seed is every market of every matchup, so this is
    /// tens of thousands of prices; the maximum stabilises long before the end and the run prints
    /// where it was last beaten so a reader can see that it did.</summary>
    private const int Seeds = 3000;

    [Fact]
    public void T74_am4_the_maximum_renderable_payout_is_enumerated_not_unbounded()
    {
        var config = new RunConfig();
        double maxOdds = 0.0;
        double minProb = 1.0;
        string where = "";
        int lastImprovedAt = -1;
        long offersSeen = 0;

        for (int i = 0; i < Seeds; i++)
        {
            var hub = new RngHub($"PAYOUT-MAX-{i}");
            Slate slate = SlateGenerator.Generate(1, hub, config);
            foreach (Matchup m in slate.Matchups)
            {
                foreach (MarketOffer o in MatchModel.BuildOffers(m, config))
                {
                    offersSeen++;
                    if (o.Odds > maxOdds)
                    {
                        maxOdds = o.Odds;
                        minProb = o.TrueProb;
                        where = $"{o.Selection.Kind} line={o.Selection.Line} choice={o.Selection.Choice}";
                        lastImprovedAt = i;
                    }
                }
            }
        }

        double parlay = Math.Pow(maxOdds, config.MaxLegs);

        _output.WriteLine($"seeds swept              : {Seeds}");
        _output.WriteLine($"offers priced            : {offersSeen:N0}");
        _output.WriteLine($"maximum single-leg odds  : {maxOdds:F4} decimal   (true prob {minProb:F6})");
        _output.WriteLine($"  produced by            : {where}");
        _output.WriteLine($"  last improved at seed  : {lastImprovedAt} of {Seeds - 1}");
        _output.WriteLine($"MaxLegs                  : {config.MaxLegs}");
        _output.WriteLine($"Overround                : {config.Overround}");
        _output.WriteLine($"MaxStakeFraction         : {config.MaxStakeFraction}  (stake ceiling IS the bank)");
        _output.WriteLine("");
        _output.WriteLine($"PARLAY TERM  maxOdds^MaxLegs = {parlay:N2} x stake");
        _output.WriteLine("  (PayoutMultiplier is a further multiplier on top and is NOT included)");
        _output.WriteLine("");
        _output.WriteLine("PAYS figure at named bank assumptions (stake = bank, MaxStakeFraction 1.0):");
        foreach (double bank in new[] { 1_000.0, 10_000.0, 100_000.0 })
        {
            double pays = bank * parlay;
            _output.WriteLine($"  bank ${bank:N0}".PadRight(24) +
                              $"-> PAYS ${pays:N0}".PadRight(34) +
                              $"({FormatDigits(pays)} digits + separators)");
        }

        // The pin: the maximum is a real number, and a run that stops producing one has changed the
        // pricing model or the generator. Deliberately loose bounds -- this asserts that the
        // enumeration FOUND something priceable, not a specific magnitude, because the magnitude is
        // the output rather than the contract.
        Assert.True(maxOdds > 1.0, "every offer must price above evens-with-no-return");
        Assert.True(offersSeen > 100_000, "the sweep must actually enumerate an offer space");
    }

    /// <summary>Digit count of the rendered dollar figure, which is what a tabular box is sized
    /// against: T82 made digit advances equal, so width is digits x advance plus the separators, and
    /// C49 forbids abbreviating any of them away.</summary>
    private static string FormatDigits(double amount)
    {
        string whole = Math.Floor(amount).ToString("F0");
        return whole.Length.ToString();
    }
}
