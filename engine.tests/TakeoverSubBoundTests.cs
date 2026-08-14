using System;
using System.Collections.Generic;
using SBR.Engine;
using Xunit;

namespace SBR.Engine.Tests;

/// <summary>
/// T89-B's second named unbounded case, enumerated: <b><c>TakeoverSub</c>'s bound.</b>
///
/// <para>The sweep has carried <c>TakeoverSub</c> as CONSTRUCTED — <i>"it has no bounded worst case
/// at all"</i> — because it renders the ticket's legs joined, one entry per leg, and each entry is
/// the ENGINE's <c>DisplayLabel</c> plus the American price. That was honest and it was also the same
/// error the payout maximum turned out to be: <b>un-enumerated is not unbounded.</b></para>
///
/// <para>Every term is finite. <c>MaxLegs</c> is 4. <c>DisplayLabel</c> is a function of a matchup
/// and a selection, and both come from a seeded generator whose name pools are closed. The American
/// price is a function of the same odds range already enumerated for the payout maximum. So the
/// longest renderable <c>TakeoverSub</c> is a join of four finite terms.</para>
///
/// <para><b>This yields the STRINGS, not the width.</b> Character count is not extent — that is this
/// lane's oldest correction — so the longest few are printed and the widths are measured on the
/// component by <c>SBR/TV/T88 prompt composition</c>. A char-count champion is exactly the kind of
/// picked champion the traceability pass retired, so several are emitted rather than one.</para>
/// </summary>
public class TakeoverSubBoundTests
{
    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public TakeoverSubBoundTests(Xunit.Abstractions.ITestOutputHelper output) => _output = output;

    private const int Seeds = 3000;

    /// <summary>Mirrors <c>OddsFormat.American</c>'s rendering: positives carry an explicit sign,
    /// negatives already have one.</summary>
    private static string American(double decimalOdds)
    {
        int a = OddsMath.AmericanFromDecimal(decimalOdds);
        return a >= 0 ? $"+{a}" : a.ToString();
    }

    [Fact]
    public void T89_B_takeover_sub_has_a_bound_and_here_it_is()
    {
        var config = new RunConfig();
        var longest = new List<string>();
        long entriesSeen = 0;

        for (int i = 0; i < Seeds; i++)
        {
            var hub = new RngHub($"TAKEOVER-{i}");
            Slate slate = SlateGenerator.Generate(1, hub, config);
            foreach (Matchup m in slate.Matchups)
            {
                foreach (MarketOffer o in MatchModel.BuildOffers(m, config))
                {
                    entriesSeen++;
                    string entry = $"{MatchModel.DisplayLabel(m, o.Selection)} {American(o.Odds)}";
                    if (longest.Count < 6) { longest.Add(entry); longest.Sort(ByLengthDesc); continue; }
                    if (entry.Length > longest[longest.Count - 1].Length)
                    {
                        longest[longest.Count - 1] = entry;
                        longest.Sort(ByLengthDesc);
                    }
                }
            }
        }

        _output.WriteLine($"seeds swept   : {Seeds}");
        _output.WriteLine($"entries built : {entriesSeen:N0}");
        _output.WriteLine($"MaxLegs       : {config.MaxLegs}   (the join is one entry per leg)");
        _output.WriteLine("");
        _output.WriteLine("LONGEST ENTRIES BY CHARACTER COUNT (width is measured on the component, not here):");
        foreach (string s in longest) _output.WriteLine($"  {s.Length,3} chars  {s}");
        _output.WriteLine("");

        // The worst case is the longest entry repeated MaxLegs times, joined. A real ticket cannot
        // repeat one matchup, but it CAN carry four legs each as long as the longest, so repeating
        // the champion is the correct upper bound rather than an impossible string.
        string worst = string.Join("   ·   ", Repeat(longest[0], config.MaxLegs));
        _output.WriteLine($"WORST CASE ({config.MaxLegs} legs, '   (mid)   ' separators): {worst.Length} chars");
        _output.WriteLine(worst);

        Assert.True(longest.Count > 0, "the enumeration must produce entries");
        Assert.True(entriesSeen > 100_000, "the sweep must actually enumerate the offer space");
    }

    private static int ByLengthDesc(string a, string b) => b.Length.CompareTo(a.Length);

    private static IEnumerable<string> Repeat(string s, int n)
    {
        for (int i = 0; i < n; i++) yield return s;
    }
}
