using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// A LIVE LEG'S ROW MUST NEVER BE BLANK, FOR ANY MARKET THE BOARD OFFERS — `T130`'s law, moved
    /// from "walk a sweat and hope the policy deals it" to an exhaustive check at the site.
    ///
    /// <para><b>This exists because a capture found the defect that four gates missed.</b> The anchor
    /// window's frame B forced an away-backed <c>Handicap</c> — a market no test had ever rendered
    /// live — and the row came back empty in all three spans.
    /// <c>TvSweatScreen.DescribeActiveLeg</c>'s <c>default:</c> returned an all-empty
    /// <c>ActiveLegCopy</c>, and a LIVE row blanks its compact line by design, so NEED and progress
    /// were the only spans left and both were empty.</para>
    ///
    /// <para><b>It is item `1.3`'s defect, surviving on a different kind.</b> `1.3`'s own record:
    /// <i>"the arm AND the caller wiring — the arm alone would not have fixed it, the caller's
    /// default: returned an empty copy, which IS the blank column."</i> `1.3` added the
    /// <c>CorrectScore</c> arm and left the <c>default:</c> standing, so every unarmed kind kept it.</para>
    ///
    /// <para><b>Why EXHAUSTIVE over the offered set rather than a list of kinds.</b> `T130` walks
    /// whatever <c>DemoTicketPolicy</c> deals, which has been moneyline every time; its forced
    /// sibling covers <c>CorrectScore</c>. Both are single kinds chosen in advance — and the defect
    /// was on a kind nobody had thought to choose. Enumerating what the BOARD offers is the only
    /// form that catches the next one, and it fails when a kind joins the offered set without copy
    /// rather than when someone remembers to add a case here.</para>
    /// </summary>
    public class ActiveLegCopyIsNeverBlankTests
    {
        /// <summary>Every distinct selection the board actually prices, across several seeds so a
        /// kind that only one slate offers is still covered.</summary>
        private static List<(Matchup Matchup, MarketSelection Selection)> EveryOfferedSelection()
        {
            var all = new List<(Matchup, MarketSelection)>();
            var seen = new HashSet<string>();
            foreach (string seed in new[] { "BLANKROW-A", "BLANKROW-B", "BLANKROW-C" })
            {
                var run = new Run(seed, new RunConfig());
                foreach (Matchup m in run.CurrentSlate.Matchups)
                    foreach (MarketOffer offer in m.Markets)
                    {
                        string key = $"{offer.Selection.Kind}/{offer.Selection.Choice}";
                        if (seen.Add(key)) all.Add((m, offer.Selection));
                    }
            }
            return all;
        }

        [Test]
        public void No_offered_market_renders_a_live_leg_row_with_nothing_in_it()
        {
            List<(Matchup Matchup, MarketSelection Selection)> offered = EveryOfferedSelection();
            Assert.Greater(offered.Count, 0, "C29: no offered selection was collected, so this checks nothing");

            MethodInfo describe = typeof(TvSweatScreen).GetMethod(
                "DescribeActiveLeg", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(describe,
                "TvSweatScreen.DescribeActiveLeg not found by reflection — renamed? This pin fails "
                + "rather than silently checking nothing. ⚠ REFLECTED SEAM: the compiler does not "
                + "check this name, so a signature change compiles green and throws here at run time.");

            var go = new GameObject("ActiveLegCopyBlankRow");
            try
            {
                TvSweatScreen screen = BuildScreenForCopy(go);
                var blank = new List<string>();
                var kinds = new HashSet<MarketKind>();

                foreach ((Matchup m, MarketSelection sel) in offered)
                {
                    kinds.Add(sel.Kind);
                    var leg = new Leg(m, sel, 2.00);
                    object copy;
                    try
                    {
                        copy = describe.Invoke(screen, new object[] { leg });
                    }
                    catch (TargetInvocationException ex)
                    {
                        blank.Add($"{sel.Kind}/{sel.Choice} THREW {ex.InnerException?.GetType().Name}: "
                            + ex.InnerException?.Message);
                        continue;
                    }

                    System.Type t = copy.GetType();
                    var need = (string)t.GetField("Need").GetValue(copy);
                    var live = (string)t.GetField("Live").GetValue(copy);

                    // PER ROW, NEVER PER SPAN — T130's own rule. A live row blanks its compact line
                    // by design, so NEED and progress are the spans that must carry it.
                    if (string.IsNullOrWhiteSpace(need) && string.IsNullOrWhiteSpace(live))
                        blank.Add($"{sel.Kind}/{sel.Choice} — need='' live='' (the whole row would be empty)");
                }

                Debug.Log($"[BLANKROW] {offered.Count} offered selections across "
                    + $"{kinds.Count} kinds: {string.Join(", ", kinds.OrderBy(k => k.ToString()))}");

                CollectionAssert.IsEmpty(blank,
                    "these OFFERED markets render a live leg row with no text in any span — a leg of "
                    + "the player's ticket saying nothing about itself, which is T130's defect and "
                    + "item 1.3's, at the caller's default: arm:\n  " + string.Join("\n  ", blank));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>A built screen, enough for <c>DescribeActiveLeg</c> — which reads the leg and the
        /// revealed ledgers, never the scene.</summary>
        private static TvSweatScreen BuildScreenForCopy(GameObject go)
        {
            TvSweatScreen screen = go.AddComponent<TvSweatScreen>();
            MethodInfo build = typeof(TvSweatScreen).GetMethod(
                "BuildCanvas", BindingFlags.NonPublic | BindingFlags.Instance);
            build?.Invoke(screen, System.Array.Empty<object>());
            return screen;
        }
    }
}
