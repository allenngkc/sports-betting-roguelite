using System;
using System.Collections.Generic;
using System.Globalization;
using SBR.Engine;
using SBR.Game;

namespace SBR.ConsoleGame;

/// <summary>
/// THE PAGE — <c>spec-console-surfaces-2026-08-19.md</c> §3, ruled: <b>80 columns × 24 rows</b>.
///
/// <para>Stated ONCE, here, because constitution §3.5 is that <i>a bound is not a layout</i>: every
/// layout on this surface derives from <see cref="Width"/> and <see cref="Height"/> and nothing
/// re-authors 62, 79 or 80 locally. <c>Ui.Rule()</c>'s old 62 was the title box's 60 plus two — a
/// number that came from a decoration rather than from content, and the exact shape <c>C46</c>
/// names: a fixed box carrying an unstated claim about what fits in it.</para>
///
/// <para><b>80 is derived twice over</b> (§3). Measured against the enumerated pool per <c>S84</c> /
/// <c>S96-am</c> — 320 constructible clubs, widest 26 characters — the widest constructible row name
/// under <c>MarketSheet.NameOf</c> is <c>SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS</c> at 44
/// characters, which a 62-column page refuses by one character and an 80-column page seats with 15
/// leader dots to spare. And the surface was ALREADY printing at 79 — its slate row. 24 is the
/// medium's floor, not a preference: the ANSI/VT100 standard and the Windows console default.</para>
///
/// <para><b>The page does not reflow.</b> <c>Console.WindowHeight</c> throws on a redirected stream,
/// so a floor has to exist regardless, and a layout that reflows cannot be captured or gated
/// (<c>C11</c>, <c>C18</c>). A larger window HOLDS the page; it does not change it.</para>
/// </summary>
internal static class Page
{
    /// <summary>§3, ruled. The one place this number is written down.</summary>
    public const int Width = 80;

    /// <summary>§3, ruled. The medium's floor — every terminal guarantees it.</summary>
    public const int Height = 24;

    /// <summary>
    /// A destination page's fixed chrome: fixture header, rule, rule, folio foot (§4's table counts
    /// exactly these four against the 24-row page). <see cref="BodyRows"/> is what is left for
    /// offers, and it is why §4's table says PLAYERS <i>overflows above 20 offers</i>.
    /// </summary>
    public const int ChromeRows = 4;

    /// <summary>Offer rows one page of a destination can hold — <b>20</b>, derived from §3's page
    /// rather than authored. §4's own table lands on the same number from the other direction.</summary>
    public const int BodyRows = Height - ChromeRows;

    /// <summary>
    /// §6.4's leader dot. ASCII full stop, matching the laptop's <c>MakeLeaders</c>
    /// (<c>new string('.', count)</c>) exactly — one device, two surfaces. §6's diagram sets it with
    /// a middle dot, which is a diagram convention; the shipped laptop character is the vocabulary.
    /// </summary>
    public const char Leader = '.';

    /// <summary>
    /// <c>S100</c>'s minimum-run guard, transferred as law (§6.4). <b>It never fires at this
    /// geometry</b> — the worst constructible row still prints 15 dots — and it is stated here so
    /// nobody removes the guard and nobody thinks it is doing work. Below this many dots the gap is
    /// set as plain space: a two-dot stub reads as debris, not as a rule made of dots.
    /// </summary>
    public const int MinLeaderRun = 2;

    /// <summary>The page's left margin — one column, on every screen §3 touches.</summary>
    public const int LeftPad = 1;

    /// <summary>The gap between two columns of a row. Three of these plus the cells make §6's
    /// 19 columns of fixed chrome.</summary>
    public const int Gap = 2;

    /// <summary>§6's price cell: American odds, right-aligned, five columns (<c>-1234</c>).
    /// A FLOOR, not a box — <see cref="RowGeometry"/> widens it when a matchup actually prices
    /// something longer, and takes the column back out of the name field so the page stays 80.</summary>
    public const int MinPriceCell = 5;

    /// <summary>§5.1's address column: two digits, because a matchup lists 79–90 offers. A floor for
    /// the same reason as <see cref="MinPriceCell"/>.</summary>
    public const int MinNumberCell = 2;

    /// <summary>§6.3's true-probability cell: <c>p NN%</c>. Two digits is the floor.</summary>
    public const int MinPctDigits = 2;

    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>The page's horizontal rule — <see cref="Width"/> columns, never 62.</summary>
    public static string Rule() => new string('─', Width);

    /// <summary>
    /// §6.4's device, generalised: a left-hand statement, a right-hand fact, and leader dots making
    /// the gap between them look like the work it is doing. Exactly <see cref="Width"/> columns.
    ///
    /// <para>The contents block's ranges (§5), the folio foot and the fixture header are all this
    /// one rule, so the page has one gap treatment rather than three.</para>
    /// </summary>
    public static string Leadered(int indent, string left, string right)
    {
        left ??= string.Empty;
        right ??= string.Empty;
        int dots = Width - indent - left.Length - 1 - 1 - right.Length;
        string body = dots >= MinLeaderRun
            ? left + " " + new string(Leader, dots) + " " + right
            // S100's guard: too short a run is set as space. Unreachable at this geometry for every
            // line this surface prints, and asserted rather than assumed by the width sweep.
            : left + new string(' ', Math.Max(1, Width - indent - left.Length - right.Length)) + right;
        return Fit(new string(' ', indent) + body);
    }

    /// <summary>
    /// The last lock on §13 gate 1. Every derivation above already lands on <see cref="Width"/>; this
    /// exists so that a line which somehow does not cannot silently print past the page, and it is
    /// deliberately a hard cut rather than an ellipsis — an ellipsis would be a new mark nobody ruled.
    /// </summary>
    public static string Fit(string line)
        => line.Length <= Width ? line : line.Substring(0, Width);

    internal static int Digits(int value)
        => Math.Abs(value).ToString(Inv).Length;
}

/// <summary>
/// §6's row, measured. <b>19 columns of fixed chrome and 61 for the name/leader field</b> at the
/// ordinary geometry — <c>1 + 2 + 2 + 2 + 5 + 2 + 5 = 19</c>, and <c>80 − 19 = 61</c>, which is
/// GEOMETRY.txt's arithmetic exactly.
///
/// <code>
///  nn  NAME ..........................................   PRICE  p NN%
///  |   |                                                 |      |
///  |   name/leader field, 61 cols                        |      true probability (00-vision §3)
///  |                                                     American odds, right-aligned, 5
///  printed line number = §5.1's address
/// </code>
///
/// <para><b>The three cells are DERIVED from the sheet, not authored.</b> The engine puts no ceiling
/// on a correct-score price, so a matchup that prices a <c>+17900</c> longshot needs six columns and
/// a 5-column box would be <c>C46</c>'s defect wearing this spec's own clothes. So the cells widen to
/// what the sheet actually prints and the NAME FIELD gives up the columns — the page stays exactly 80
/// either way, and the field never falls below the 44 the pool's worst constructible name needs.
/// This is <c>S94-cl</c>'s lesson on a second surface: <i>the rail fit because its packing is derived
/// from measured labels rather than authored constants.</i></para>
/// </summary>
internal readonly struct RowGeometry
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>§5.1's address column.</summary>
    public int NumberCell { get; }

    /// <summary>American odds, right-aligned.</summary>
    public int PriceCell { get; }

    /// <summary>Digits inside <c>p NN%</c>.</summary>
    public int PctDigits { get; }

    private RowGeometry(int numberCell, int priceCell, int pctDigits)
    {
        NumberCell = numberCell;
        PriceCell = priceCell;
        PctDigits = pctDigits;
    }

    /// <summary><c>p NN%</c> — the literal <c>p</c>, a space, the digits, the sign.</summary>
    public int PctCell => 2 + PctDigits + 1;

    /// <summary>§6's fixed chrome. 19 at the ordinary geometry.</summary>
    public int Chrome => Page.LeftPad + NumberCell + Page.Gap + Page.Gap + PriceCell + Page.Gap + PctCell;

    /// <summary>§6's name/leader field. 61 at the ordinary geometry; never below the pool's 44.</summary>
    public int Field => Page.Width - Chrome;

    /// <summary>The geometry every page of one matchup's sheet shares, so its columns line up from
    /// RESULT through PLAYERS. Read off the sheet — §13 gate 5's discipline applied to widths.</summary>
    public static RowGeometry For(MarketSheet sheet)
    {
        int number = Page.MinNumberCell;
        int price = Page.MinPriceCell;
        int pct = Page.MinPctDigits;
        number = Math.Max(number, Page.Digits(sheet.TotalRows));
        foreach (MarketSheetRow row in sheet.AllRows)
        {
            price = Math.Max(price, Ui.American(row.Offer.Odds).Length);
            pct = Math.Max(pct, Page.Digits(Ui.Pct(row.Offer.TrueProb)));
        }
        return new RowGeometry(number, price, pct);
    }

    /// <summary>The same derivation for a hand-picked run of offers — §10's slate, whose rows are
    /// the 1X2 triple of every matchup on the card and which therefore shares one set of columns
    /// down the whole block.</summary>
    public static RowGeometry ForOffers(int widestNumber, IEnumerable<MarketOffer> offers)
    {
        int number = Math.Max(Page.MinNumberCell, Page.Digits(widestNumber));
        int price = Page.MinPriceCell;
        int pct = Page.MinPctDigits;
        foreach (MarketOffer offer in offers)
        {
            price = Math.Max(price, Ui.American(offer.Odds).Length);
            pct = Math.Max(pct, Page.Digits(Ui.Pct(offer.TrueProb)));
        }
        return new RowGeometry(number, price, pct);
    }

    /// <summary>
    /// §6's row, composed to exactly <see cref="Page.Width"/> columns.
    ///
    /// <param name="number">§5.1's printed line number — the address. Blank for a row that
    /// continues the one above it (§10's slate block, whose address is the matchup).</param>
    /// <param name="name">The engine's words, from <c>MarketSheet.NameOf</c>. <b>§6.5: uppercased
    /// HERE, at the presentation layer.</b> <c>A2</c> is not overridden — the words are the
    /// engine's and the CASE is the surface's, which is <c>S96</c>'s whole ruling.</param>
    /// <param name="note">The annotation that rides after the name inside the field and is not part
    /// of it: §6.7's role WORD on a scorer row, the record on §10's slate. Separate from
    /// <paramref name="name"/> so §13 gate 6's composer-parity assertion still compares like with
    /// like — the laptop's <c>MakeOfferRow</c> seats the role the same way.</param>
    /// </summary>
    public string OfferRow(string number, string name, string note, double odds, double trueProb)
    {
        // §6.5 — S96, at the presentation layer, exactly as the laptop's PrintedRowName does it.
        string printed = string.IsNullOrEmpty(name) ? string.Empty : name.ToUpperInvariant();
        string head = string.IsNullOrEmpty(note) ? printed : printed + "  " + note;

        int dots = Field - head.Length - 1;
        string field = dots >= Page.MinLeaderRun
            ? head + " " + new string(Page.Leader, dots)
            : head.PadRight(Field);
        if (field.Length > Field) field = field.Substring(0, Field);

        string price = Ui.American(odds).PadLeft(PriceCell);
        string pct = "p " + Ui.Pct(trueProb).ToString(Inv).PadLeft(PctDigits) + "%";

        return Page.Fit(
            new string(' ', Page.LeftPad)
            + (number ?? string.Empty).PadLeft(NumberCell)
            + new string(' ', Page.Gap)
            + field
            + new string(' ', Page.Gap)
            + price
            + new string(' ', Page.Gap)
            + pct);
    }
}
