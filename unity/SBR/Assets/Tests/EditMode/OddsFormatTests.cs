using NUnit.Framework;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>Playtest #6: American odds display. Decimal in, book-style string out.</summary>
    public class OddsFormatTests
    {
        [TestCase(2.00, "+100")]
        [TestCase(3.50, "+250")]
        [TestCase(2.10, "+110")]
        // S30: every printed signed number uses U+2212 MINUS, never the U+002D hyphen.
        [TestCase(1.50, "−200")]
        [TestCase(1.87, "−115")]
        [TestCase(1.05, "−2000")]
        [TestCase(11.0, "+1000")]
        public void American_matches_book_convention(double dec, string expected)
            => Assert.AreEqual(expected, OddsFormat.American(dec));

        [Test]
        public void Degenerate_price_renders_an_em_dash()
            // S30/S36: an absence marker, not a signed number, so it is never U+2212 — and no
            // longer the bare "-" hyphen a real minus-signed price could be mistaken for.
            => Assert.AreEqual("—", OddsFormat.American(1.0));
    }
}
