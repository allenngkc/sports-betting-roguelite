using NUnit.Framework;
using TMPro;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// S29's signature, converted from a claim into an assertion.
    ///
    /// S29 was SIGNED rather than fixed, on the finding that the production faces already deliver
    /// uniform digits — "Archivo Narrow digits uniform (456/1000 em, spread 0); roman spread
    /// sub-pixel below 64px, largest laptop figure is 31px". C15 then expected the deviation to
    /// EXPIRE at the TMP migration, on the assumption that TMP could enable tabular figures.
    ///
    /// **It cannot.** `OTL_FeatureTag` in com.unity.ugui declares exactly four members — kern, liga,
    /// mark, mkmk — and TMP's layout tests only those four. There is no `tnum` to turn on, in either
    /// stack. So S29 does not expire at the migration; it stands, and it stands on a property of the
    /// faces rather than on anything the renderer does.
    ///
    /// Which makes the right move an assertion, not a build: the signature rests on a measurable
    /// fact, so measure it every run instead of trusting a figure written down once.
    /// </summary>
    public class SureThingTypeFeatureTests
    {
        private const string RomanPath = "SureThing/Fonts/Archivo SDF";
        private const string CondensedPath = "SureThing/Fonts/ArchivoNarrow SDF";

        /// <summary>The condensed face renders every figure on this surface — payout, stake, prices,
        /// records, bank, target. Its digits must be exactly uniform, because a settled record's
        /// columns and the ledger's money column line up on that and nothing else.</summary>
        [Test]
        public void Condensed_digits_are_exactly_uniform_so_figures_column_without_tnum()
        {
            (float spread, float min, float max) = DigitSpread(CondensedPath);
            Assert.AreEqual(0f, spread, 0.0001f,
                $"S29: the condensed face's digits must be tabular by construction " +
                $"(min {min}, max {max}) — TMP cannot enable tnum, so the face is the only guarantee");
        }

        /// <summary>The roman face carries labels and running text rather than figures, and S29
        /// signed its spread as sub-pixel at this surface's sizes rather than as zero. Asserted at
        /// the size that actually decides it: the 31px payout is the largest figure on the surface,
        /// and S29 lapses only above 64px.</summary>
        [Test]
        public void Roman_digit_spread_stays_sub_pixel_at_the_largest_figure_on_the_surface()
        {
            var font = Resources.Load<TMP_FontAsset>(RomanPath);
            Assert.IsNotNull(font, $"missing font asset: {RomanPath}");
            (float spread, _, _) = DigitSpread(RomanPath);

            // **This constant tracks the surface, and the surface moved twice.**
            //
            // It began at 31 — wrong, because the 31px payout is the largest figure overall but is
            // CONDENSED. Corrected to 21, the masthead's run figures, which were roman: it failed
            // there at 1.112px, and that failure is what got S29 ruled.
            //
            // 13 now, because the ruling moved those figures to the condensed face (S29 closed,
            // Allen 2026-08-08). The largest figure the ROMAN face still carries is 13px — the
            // masthead subline's round numbers, `SHEET 1 OF 1`, the team records, the margin row
            // labels. **Not a retune to make a red test green:** the assertion is unchanged and the
            // face is unchanged; what changed is which slots use it, and this constant exists to say
            // so. If a roman figure above ~19px is ever added back, this fails again — which is the
            // whole point of leaving it expressed as a size rather than as a pass.
            //
            // Advances are stored against the atlas sampling size, so scale to rendered pixels the
            // same way LaptopUi.MeasureWidth does.
            const int largestRomanFigure = 13;
            float renderedSpread = spread / font.faceInfo.pointSize * largestRomanFigure;
            Assert.Less(renderedSpread, 1f,
                $"S29: roman digit spread must stay sub-pixel at {largestRomanFigure}px " +
                $"(spread {spread} units = {renderedSpread:0.####}px). S29's roman clause was measured " +
                "against Archivo's DEFAULT face, which is SemiBold and near-tabular (spread 0.1875). " +
                "At the ruled Regular 400 the digits are proportional, so the masthead's money " +
                "figures vary in width as the bank changes — which is what tabular figures exist to " +
                "stop and what TMP cannot enable (no tnum in OTL_FeatureTag). Owning doc §4.1 already " +
                "assigns figures AND the masthead to the condensed face, which measures spread 0.");
        }

        private static (float spread, float min, float max) DigitSpread(string resourcePath)
        {
            var font = Resources.Load<TMP_FontAsset>(resourcePath);
            Assert.IsNotNull(font, $"missing font asset: {resourcePath}");
            // Dynamic atlas: the digits are not in the lookup table until something asks for them.
            font.TryAddCharacters("0123456789", out _);

            float min = float.MaxValue, max = float.MinValue;
            for (char c = '0'; c <= '9'; c++)
            {
                Assert.IsTrue(font.characterLookupTable.TryGetValue(c, out TMP_Character ch) && ch.glyph != null,
                    $"'{c}' has no glyph in {resourcePath} — it would render as nothing, not as a box");
                float advance = ch.glyph.metrics.horizontalAdvance;
                if (advance < min) min = advance;
                if (advance > max) max = advance;
            }
            return (max - min, min, max);
        }
    }
}
