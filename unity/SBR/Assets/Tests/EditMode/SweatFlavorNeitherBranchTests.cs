using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// `T163`'s NEITHER-BRANCH LINE SET, pinned against the spec that authored it —
    /// <c>docs/design/spec-neither-branch-lines-2026-08-21.md</c> §5, batch 171.
    ///
    /// <para><b>Why a transcription needs a gate.</b> These twelve lines were authored in a document
    /// and typed into a table. Until something compares the two, the only copy that can be checked is
    /// the one in the doc — and this studio has just been bitten by exactly that: two register rows
    /// cited in an order (`K21`, `C60`) existed in batches 174–175 and nowhere in `docs/`, because
    /// transcription had lagged since batch 154. A ruled line set that lives only in prose is one
    /// seat away from being lost.</para>
    ///
    /// <para><b>What this does NOT assert.</b> When the branch FIRES is `T163`'s three-branch rule
    /// and is not built yet — it needs the backed-side table, which Allen ruled the ENGINE owns and
    /// which is queued behind that lane's campaign. So this pins the SET and its shape, not its
    /// wiring. Stated here so a later reader does not mistake a green file for a wired branch.</para>
    /// </summary>
    public class SweatFlavorNeitherBranchTests
    {
        /// <summary>§5.2's table, transcribed as DATA rather than as prose, so the assertion below is
        /// a comparison rather than a reading. Order within a direction is the spec's variant order.</summary>
        private static readonly (DramaEventType Type, bool Up, string[] Lines)[] Spec =
        {
            (DramaEventType.Score, true, new[]
            {
                "a goal — the number ticks with it.",
                "a goal in the churn; the number moves.",
                "one goes in — the slip gains.",
            }),
            (DramaEventType.Score, false, new[]
            {
                "a goal against the slip.",
                "a goal; the slip flinches.",
                "one goes in, the wrong way for the slip.",
            }),
            (DramaEventType.Momentum, true, new[]
            {
                "the half tightens.",
                "territory, and the clock with it.",
                "the pitch shrinks.",
            }),
            (DramaEventType.Momentum, false, new[]
            {
                "the ball stays in midfield.",
                "slow through the middle, and no one in a hurry.",
                "sideways, and the clock with it.",
            }),
        };

        [Test]
        public void The_neither_branch_emits_spec_5_2_verbatim()
        {
            foreach ((DramaEventType type, bool up, string[] lines) in Spec)
                for (int i = 0; i < lines.Length; i++)
                    Assert.AreEqual(lines[i], SweatFlavor.NeitherLine(type, up, i),
                        $"{type}/{(up ? "up" : "down")} variant {i} does not match spec §5.2 verbatim");
        }

        /// <summary>BigPlay is a GOAL family and takes the goal set — §5 authors ONE goal set per
        /// direction, so splitting it would invent two sets the spec does not have.</summary>
        [Test]
        public void BigPlay_takes_the_same_goal_set_as_Score()
        {
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(SweatFlavor.NeitherLine(DramaEventType.Score, true, i),
                                SweatFlavor.NeitherLine(DramaEventType.BigPlay, true, i));
                Assert.AreEqual(SweatFlavor.NeitherLine(DramaEventType.Score, false, i),
                                SweatFlavor.NeitherLine(DramaEventType.BigPlay, false, i));
            }
        }

        /// <summary>THE WHOLE POINT OF THE BRANCH: no line may name a club, or the anchor is back.
        /// Checked against every club noun the generator can produce, not against a sample.</summary>
        [Test]
        public void No_neither_line_names_a_club()
        {
            var nouns = new HashSet<string>();
            foreach (string seed in new[] { "NEITHER-A", "NEITHER-B", "NEITHER-C" })
            {
                var run = new Run(seed, new RunConfig());
                foreach (Matchup m in run.CurrentSlate.Matchups)
                {
                    nouns.Add(SweatFlavor.Short(m.Home.Name));
                    nouns.Add(SweatFlavor.Short(m.Away.Name));
                }
            }
            Assert.Greater(nouns.Count, 0, "C29: no club nouns were collected, so this checks nothing");

            foreach ((DramaEventType type, bool up, string[] lines) in Spec)
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = SweatFlavor.NeitherLine(type, up, i);
                    foreach (string noun in nouns)
                        Assert.IsFalse(line.ToUpperInvariant().Contains(noun.ToUpperInvariant()),
                            $"'{line}' names the club '{noun}' — the neither branch exists because "
                            + "there is no anchor club to name");
                    Assert.IsFalse(line.Contains("{picked}") || line.Contains("{other}"),
                        $"'{line}' carries an anchor slot. Batch 171: the slot change is unbuildable "
                        + "because DramaEvent has no actor, which is why these lines exist at all");
                }
        }

        /// <summary>§5.1's CORRECTED casing rule: a club-free line takes the casing its own FILE uses
        /// for club-free copy — here lowercase with a terminal period, as
        /// <c>"off the bar and away."</c> already ships. NOT the casing of the table it joins, which
        /// is the rule that produced a branch split two capitalised and two lowercase elsewhere.</summary>
        [Test]
        public void Every_neither_line_is_lowercase_opening_with_a_terminal_period()
        {
            foreach ((DramaEventType type, bool up, string[] lines) in Spec)
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = SweatFlavor.NeitherLine(type, up, i);
                    Assert.IsTrue(char.IsLower(line[0]),
                        $"'{line}' opens capitalised; §5.1 puts club-free copy in this file's own "
                        + "lowercase convention");
                    Assert.IsTrue(line.EndsWith("."),
                        $"'{line}' has no terminal period — the convention is sentence case with one");
                }
        }

        /// <summary>Three variants per table, because <c>variants[step % length]</c> on a
        /// single-element table makes every beat in the branch read identically. That repetition is
        /// the defect §5 was written to close, so a table that shrinks back to one must fail.</summary>
        [Test]
        public void Each_table_carries_three_distinct_variants()
        {
            foreach ((DramaEventType type, bool up, string[] _) in Spec)
            {
                var seen = new List<string>();
                for (int i = 0; i < 3; i++) seen.Add(SweatFlavor.NeitherLine(type, up, i));
                Assert.AreEqual(3, seen.Distinct().Count(),
                    $"{type}/{(up ? "up" : "down")} does not carry three DISTINCT variants: "
                    + string.Join(" | ", seen));
            }
        }

        /// <summary>The step index is a beat counter and this must not throw on any of them —
        /// including the wrap, and including a negative if a caller ever hands one over.</summary>
        [Test]
        public void The_step_index_wraps_rather_than_throwing()
        {
            for (int step = -3; step < 12; step++)
                foreach ((DramaEventType type, bool up, string[] _) in Spec)
                    Assert.IsNotEmpty(SweatFlavor.NeitherLine(type, up, step),
                        $"step {step} produced no line for {type}/{(up ? "up" : "down")}");
        }
    }
}
