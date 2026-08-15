using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// T98 (batch 70) — the strip printed `— LEAD CHANGE` over a `MALLARDS 0 — 0 MIDDLEMEN`
    /// scorebug on 8 of 8 mid-match frames, and the WORD came off.
    ///
    /// <para><b>The tag is real, and this was NOT T97's law a third time.</b> `DramaGenerator`
    /// assigns `TensionTag.LeadChange` on the WIN PROBABILITY crossing 0.5, never on the scoreline —
    /// so nothing phantom happened, and T97's guard would have suppressed a real fact. Three defects
    /// in this one slot have now had three different mechanisms; the guard fixes only one of
    /// them.</para>
    ///
    /// <para><b>It comes off because the fact is an OPINION.</b> §8 stands: the theatre prints facts
    /// and offers, never opinions. A price is an offer — the house stands behind it and the player
    /// transacts against it; a probability is the house's opinion, which he can only agree or
    /// disagree with. A strip line announcing that the probability crossed 50% is the deleted
    /// win-prob numeral's MEANING without its digits. <b>Nothing he can act on is lost:</b> the
    /// cash-out price prices off `WinProbAfter`, so the crossing is already visible AS AN OFFER —
    /// the price moving through its own midpoint.</para>
    ///
    /// <para><b>A TAG MAY DRIVE TIMING AND STAGING WITHOUT EARNING A WORD.</b> The tag itself is
    /// untouched and correct; that its staging and pacing survive is pinned one file over by
    /// `TheaterChoreographerTests.Overlays_modify_playback_but_never_choose_the_template`
    /// (`LeadChangeIntro` set, `Duration` grown). This test owns only the WORD, which is the half
    /// that was ruled off — so a future seat reading "remove LEAD CHANGE" cannot take the tag with
    /// it without turning that test red.</para>
    ///
    /// <para><b>Swept over the population, never a champion beat.</b> The suffix was appended after
    /// the base table was picked, so it could return on one arm and hide behind every other — which
    /// is the shape a single sample misses by construction.</para>
    ///
    /// <para>The removal closed a second defect the same way: the suffix appended UPPERCASE to a
    /// sentence-case line, against §8's one casing, one dash. The equality assertion below carries
    /// it — a suffix in any register breaks it.</para>
    /// </summary>
    public class SweatFlavorLeadChangeTests
    {
        private static readonly DramaEventType[] Types =
        {
            DramaEventType.Momentum, DramaEventType.Score, DramaEventType.BigPlay,
            DramaEventType.LegFinal,
        };

        private static readonly int[] Steps = { 0, 1, 2, 3, 7 };

        private static Leg MoneylineLeg()
        {
            // A team market on purpose: the count families (corners, cards) return before the tag is
            // ever consulted, so testing one of those would pass without reaching the code at issue.
            var run = new Run("FLAVOUR-LEADCHANGE");
            Matchup m = run.CurrentSlate.Matchups[0];
            return new Leg(m, MarketSelection.Moneyline(Side.Home), 2.10);
        }

        [Test]
        public void T98_the_lead_change_tag_adds_no_word_to_the_strip()
        {
            Leg leg = MoneylineLeg();
            const double prev = 0.50;
            int cases = 0;

            foreach (DramaEventType type in Types)
            foreach (bool up in new[] { true, false })
            foreach (int step in Steps)
            {
                // `up` is the sign of the move against the anchor, so the two probabilities
                // straddle it — this is what walks both halves of every base table.
                double after = up ? 0.62 : 0.38;
                string tagged = SweatFlavor.For(
                    new DramaEvent(0, step, 12, type, after, TensionTag.LeadChange), leg, prev);
                string plain = SweatFlavor.For(
                    new DramaEvent(0, step, 12, type, after, TensionTag.Calm), leg, prev);

                // THE PRECONDITION BEFORE THE PROPERTY — a "no words were added" assertion passes
                // vacuously when no line was produced at all, which is the shape that went green
                // once already in this lane while the key was never down.
                Assert.IsNotEmpty(plain, $"no line was produced for {type}/up={up}/step={step} — this "
                    + "test proves nothing about a beat the strip never wrote");

                Assert.AreEqual(plain, tagged,
                    $"T98: TensionTag.LeadChange must add NO WORDS to the strip line. {type}, up={up}, "
                    + $"step={step} produced '{tagged}' where the untagged beat produced '{plain}'");
                StringAssert.DoesNotContain("LEAD CHANGE", tagged,
                    "T98: the word is banned — it is the win-probability readout's meaning without "
                    + "its digits. The tag stays and drives staging; it does not earn a word.");

                cases++;
            }

            Assert.AreEqual(Types.Length * 2 * Steps.Length, cases,
                "the sweep must cover every beat type in both directions across several steps — a "
                + "loop that stopped covering its population reports absence as compliance");
        }
    }
}
