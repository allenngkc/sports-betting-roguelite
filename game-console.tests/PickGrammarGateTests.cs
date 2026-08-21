using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SBR.Engine;
using SBR.Game;
using Xunit.Abstractions;

namespace SBR.ConsoleGame.Tests;

/// <summary>
/// spec-console-surfaces-2026-08-19.md §7 (the pick grammar), §8 (refusals move before the act),
/// and the two gates they owe: <b>§13 gate 4 — reachability, BOTH directions</b>, and <b>§13 gate 7
/// — no two rows on a page share a name</b>.
///
/// <para>§13 gate 4 is <i>the gate that makes §7 structural</i>. §7.3's claim is that <c>C19</c>
/// stops being something anyone maintains: a kind cannot be printed-but-unbettable, because printing
/// it is what gives it an address. That claim is only worth what an assertion makes it worth, and
/// the assertion has to run in both directions — a parser that accepted every printed offer AND a
/// hundred addresses that print nothing would satisfy the forwards half alone.</para>
///
/// <para>§13 gate 7 is the laptop's own <c>MarketSheetTests</c> gate, transferred unchanged. It is
/// the gate that caught the BTTS arm on that surface — two rows printing one name — and it costs
/// nothing to run here on the same composer.</para>
/// </summary>
public class PickGrammarGateTests
{
    private readonly ITestOutputHelper _output;

    public PickGrammarGateTests(ITestOutputHelper output) => _output = output;

    // ---------------------------------------------------------------------------------------
    // Reaching the surface under test.
    //
    // BettingScreen is `internal static` and its parser is `private static`, which is correct --
    // nothing outside the shell has business calling it. Reflection is how PoolGateTests already
    // reaches SlateGenerator's own private pool arrays, and the reasoning transfers: the honest
    // way to gate the REAL parser is to call it, not to loosen its accessibility for a test's
    // convenience and not to retype its rules here. A hand-written copy of ParseOne would be this
    // fixture asserting its own transcription.
    //
    // MarketSheet is linked BY SOURCE into the console project (see SBR.ConsoleGame.csproj's "ONE
    // COMPOSER, TWO SURFACES" comment), so typeof(MarketSheet).Assembly IS the console assembly.
    // That is asserted below rather than assumed, because if the linking were ever changed to a
    // ProjectReference this fixture would silently start reflecting into the wrong assembly.
    // ---------------------------------------------------------------------------------------

    private const string ConsoleAssemblyName = "SBR.ConsoleGame";

    private static readonly Assembly Shell = ResolveShell();

    private static Assembly ResolveShell()
    {
        Assembly asm = typeof(MarketSheet).Assembly;
        string? name = asm.GetName().Name;
        return name == ConsoleAssemblyName
            ? asm
            : throw new InvalidOperationException(
                $"MarketSheet resolved to assembly '{name}', not '{ConsoleAssemblyName}'. The "
                + "console links MarketSheet.cs by source; if that changed, this fixture is "
                + "reflecting into the wrong assembly. Report it — do not repoint it blindly.");
    }

    private static readonly Type ScreenType = Shell.GetType("SBR.ConsoleGame.BettingScreen")
        ?? throw new InvalidOperationException("SBR.ConsoleGame.BettingScreen was not found.");

    private static readonly Type PageType = Shell.GetType("SBR.ConsoleGame.Page")
        ?? throw new InvalidOperationException("SBR.ConsoleGame.Page was not found.");

    private static MethodInfo Method(string name)
        => ScreenType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException(
                $"BettingScreen.{name} was not found by reflection. That is this gate failing to "
                + "find the parser it exists to measure — report it as a finding, do not stub it.");

    /// <summary>A const read off the shell rather than retyped, so the gate cannot assert against a
    /// number the surface stopped using — §13 gate 5's discipline applied to widths and to
    /// prompts.</summary>
    private static T Const<T>(Type type, string name)
    {
        FieldInfo field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            ?? throw new InvalidOperationException($"{type.Name}.{name} was not found by reflection.");
        return (T)field.GetRawConstantValue()!;
    }

    private static readonly MethodInfo ParseOneMethod = Method("ParseOne");
    private static readonly MethodInfo ParsePicksMethod = Method("ParsePicks");

    private static int PageWidth => Const<int>(PageType, "Width");
    private static int BodyRows => Const<int>(PageType, "BodyRows");
    private static string PicksPrompt => Const<string>(ScreenType, "PicksPrompt");

    /// <summary>The parser, called for real. Refusals arrive as
    /// <see cref="TargetInvocationException"/>; the message is unwrapped so the gate can assert on
    /// what the PLAYER is shown, which is <c>ex.Message</c> and nothing else.</summary>
    private static bool TryPick(string token, IReadOnlyList<Matchup> matchups, out Pick pick, out string refusal)
    {
        pick = default;
        refusal = string.Empty;
        try
        {
            pick = (Pick)ParseOneMethod.Invoke(null, new object?[] { token, matchups })!;
            return true;
        }
        catch (TargetInvocationException ex)
        {
            refusal = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    private static bool TryPicks(string line, IReadOnlyList<Matchup> matchups, int maxLegs,
        out int count, out string refusal)
    {
        count = 0;
        refusal = string.Empty;
        try
        {
            var picks = (System.Collections.ICollection)ParsePicksMethod.Invoke(
                null, new object?[] { line, matchups, maxLegs })!;
            count = picks.Count;
            return true;
        }
        catch (TargetInvocationException ex)
        {
            refusal = ex.InnerException?.Message ?? ex.Message;
            return false;
        }
    }

    // ---------------------------------------------------------------------------------------
    // The sweep. Real slates from the real generator, across seeds AND rounds -- "assert it over
    // many seeds, not one". RngHub is threaded through the rounds the way Run threads it, so the
    // later rounds are the slates a real run would actually deal rather than four copies of round 1.
    // ---------------------------------------------------------------------------------------

    private const int SeedCount = 8;
    private const int RoundCount = 3;

    private static List<(string Label, Matchup Matchup)> SweepMatchups()
    {
        var config = new RunConfig();
        var all = new List<(string, Matchup)>();
        for (int s = 0; s < SeedCount; s++)
        {
            string seed = "GRAMMAR-" + s.ToString(CultureInfo.InvariantCulture);
            var hub = new RngHub(seed);
            for (int round = 1; round <= RoundCount; round++)
            {
                Slate slate = SlateGenerator.Generate(round, hub, config);
                for (int i = 0; i < slate.Matchups.Count; i++)
                    all.Add(($"{seed} r{round} m{i + 1}", slate.Matchups[i]));
            }
        }
        return all;
    }

    /// <summary>One matchup's slate, as the parser sees it — the parser takes the whole matchup
    /// list and a 1-based number into it, so a single-matchup list makes the address
    /// <c>1#{line}</c> for every case and keeps the token construction honest.</summary>
    private static IReadOnlyList<Matchup> Only(Matchup m) => new List<Matchup> { m };

    // =======================================================================================
    // §13 GATE 4 — REACHABILITY, BOTH DIRECTIONS. The gate that makes §7 structural.
    // =======================================================================================

    /// <summary>
    /// <b>Forwards:</b> every offer in <c>matchup.Markets</c> has an address the parser accepts, and
    /// that address resolves to THAT offer. <b>Backwards:</b> every address the parser accepts
    /// inside the printed range names a printed offer — the mapping is onto AND one-to-one, so there
    /// is no address that quietly lands somewhere else.
    ///
    /// <para><b>BLIND SPOTS — stated, per §13 gate 4's own instruction.</b></para>
    /// <list type="number">
    /// <item>It proves the ADDRESS is total over <c>matchup.Markets</c>. It does not prove the pages
    /// DRAW every row: a row that exists on the sheet but is never rendered (a pagination fault, a
    /// section skipped) would pass here and still be invisible. That is §13 gates 1, 2 and 5's
    /// territory, not this one's.</item>
    /// <item>It is a SEED SWEEP, not a population enumeration. Unlike §13 gate 3 — which can
    /// enumerate <c>Cities × Nouns</c> because the pool is a static array — the population here is
    /// "whatever the generator prices", which no array holds. A market kind that none of these
    /// <see cref="SeedCount"/> seeds × <see cref="RoundCount"/> rounds happens to price is not
    /// covered. The kinds actually swept are reported below so the coverage is a stated number
    /// rather than a hope.</item>
    /// <item>The backwards direction is checked over a BOUNDED BAND of out-of-range integers
    /// (0, −1, and TotalRows+1..TotalRows+8), not over all of <c>int</c>. It assumes an address of
    /// 2,147,483,647 behaves like TotalRows+8, which is true of the lookup as written but is an
    /// assumption rather than an assertion.</item>
    /// <item>It says nothing about the six mnemonic ALIASES (the next test), and nothing about
    /// whether a resolved pick then GRADES correctly — that is the engine's suite.</item>
    /// </list>
    /// </summary>
    [Fact]
    public void Gate_13_4_every_printed_offer_has_an_address_and_every_address_names_that_offer()
    {
        var kindsSwept = new SortedSet<string>();
        int matchups = 0;
        int addresses = 0;

        foreach ((string label, Matchup m) in SweepMatchups())
        {
            matchups++;
            MarketSheet sheet = MarketSheet.Build(m);
            IReadOnlyList<Matchup> slate = Only(m);

            // The sheet is MarketSheet.Build's own re-ordering of matchup.Markets with nothing
            // added and nothing lost (it throws otherwise), so asserting over AllRows IS asserting
            // over matchup.Markets. Assert the identity anyway -- it is the hinge of §7.3's claim
            // and it costs one line.
            Assert.Equal(m.Markets.Count, sheet.AllRows.Count);

            var seen = new HashSet<int>();
            foreach (MarketSheetRow row in sheet.AllRows)
            {
                kindsSwept.Add(row.Kind.ToString());
                string token = $"1#{row.Line.ToString(CultureInfo.InvariantCulture)}";

                // FORWARDS: the printed row has an address the parser accepts.
                Assert.True(TryPick(token, slate, out Pick pick, out string refusal),
                    $"{label}: printed offer on line {row.Line} ({row.Kind}, \"{row.Name}\") has no "
                    + $"address the parser accepts — '{token}' was refused with \"{refusal}\". That "
                    + "is C19's defect, and §7.3 says it cannot happen.");

                // BACKWARDS: the address the parser accepted names THAT printed offer.
                Assert.Equal(0, pick.MatchupIndex);
                Assert.True(pick.Selection.Equals(row.Offer.Selection),
                    $"{label}: '{token}' resolved to {pick.Selection.Kind} but line {row.Line} "
                    + $"prints {row.Kind} (\"{row.Name}\") — the address does not name its own row.");

                // ONE-TO-ONE: no two printed rows share an address.
                Assert.True(seen.Add(row.Line),
                    $"{label}: line {row.Line} is printed twice — one address, two offers.");
                addresses++;
            }

            // BACKWARDS, the other half: the accepted address space is EXACTLY the printed one.
            // 1..TotalRows all resolved above; nothing outside it may resolve.
            foreach (int outside in OutOfRange(sheet.TotalRows))
            {
                string token = $"1#{outside.ToString(CultureInfo.InvariantCulture)}";
                Assert.False(TryPick(token, slate, out Pick stray, out _),
                    $"{label}: '{token}' was ACCEPTED, but this matchup prints only "
                    + $"1–{sheet.TotalRows}. An address that names no printed offer is the "
                    + "backwards half of C19 — a pick the player cannot see.");
            }
        }

        _output.WriteLine($"§13.4 swept {matchups} matchups ({SeedCount} seeds × {RoundCount} rounds "
            + $"× {new RunConfig().MatchupsPerSlate} per slate), {addresses} addresses, both directions.");
        _output.WriteLine($"Kinds covered ({kindsSwept.Count}): {string.Join(", ", kindsSwept)}");
        _output.WriteLine("BLIND SPOT: a kind no swept slate prices is not covered — this is a seed "
            + "sweep, not a population enumeration (§13.3 can enumerate its pool; this cannot).");

        Assert.True(addresses > 0, "the sweep dealt no offers at all — the gate asserted nothing.");
    }

    private static IEnumerable<int> OutOfRange(int totalRows)
    {
        yield return -1;
        yield return 0;
        for (int over = 1; over <= 8; over++) yield return totalRows + over;
    }

    /// <summary>
    /// §8's third row: <b>an out-of-range address is refused AT THE PICKS PROMPT and names the
    /// matchup's range</b> — <c>matchup 1 lists 1–84</c>. The range is read off the sheet, so a
    /// matchup that lists 79 says 79; a test that hard-coded 84 would be testing nothing
    /// (<c>S74-am3</c>, §13 gate 5).
    /// </summary>
    [Fact]
    public void Gate_13_4_an_out_of_range_address_is_refused_naming_the_matchups_own_range()
    {
        var distinctDenominators = new SortedSet<int>();

        foreach ((string label, Matchup m) in SweepMatchups().Take(12))
        {
            MarketSheet sheet = MarketSheet.Build(m);
            distinctDenominators.Add(sheet.TotalRows);
            string expected = $"matchup 1 lists 1–{sheet.TotalRows.ToString(CultureInfo.InvariantCulture)}";

            Assert.False(TryPick($"1#{sheet.TotalRows + 1}", Only(m), out _, out string refusal));
            Assert.Contains(expected, refusal, StringComparison.Ordinal);
            Assert.True(refusal.Length <= PageWidth, $"{label}: refusal is {refusal.Length} columns.");

            // A non-numeric address is a different first fault and says so, but it still owes the
            // range, because the range is the remedy.
            Assert.False(TryPick("1#nine", Only(m), out _, out string nonNumeric));
            Assert.Contains(expected, nonNumeric, StringComparison.Ordinal);
            Assert.Contains("not a line number", nonNumeric, StringComparison.Ordinal);
        }

        // The denominator must actually vary across the sweep, or "read off the sheet" is
        // indistinguishable from a constant that happens to be right.
        _output.WriteLine($"denominators seen across the sweep: {string.Join(", ", distinctDenominators)}");
        Assert.True(distinctDenominators.Count > 1,
            "every swept matchup listed the same number of offers, so this gate cannot tell a "
            + "derived range from a hard-coded one. Widen the sweep before trusting it.");
    }

    // =======================================================================================
    // §7 — THE SIX MNEMONICS STAY AS ALIASES
    // =======================================================================================

    /// <summary>
    /// Every one of the six mnemonics still resolves to <b>the same offer its <c>#</c>-form does</b>.
    /// An alias that drifted off its address would be two grammars, not one grammar and its
    /// shorthand.
    ///
    /// <para>The mnemonics are constructed FROM the printed rows — <c>GO2.5</c> is built out of the
    /// row's own line and side — so the test cannot pass by asserting a line the board does not
    /// actually offer, and every mnemonic-able row on every swept matchup is checked rather than one
    /// example per kind.</para>
    ///
    /// <para><b>And the finding this test surfaces:</b> the MONEYLINE DRAW has no mnemonic and never
    /// had one — <c>H</c> and <c>A</c> are the whole set, and §10's slate has been printing the draw
    /// in the middle of every matchup block the entire time. It is exactly the printed-but-unbettable
    /// shape <c>C19</c> names, sitting on the surface's most-read screen, and §7 fixes it without a
    /// line of new vocabulary. It is asserted below in both states: no mnemonic, and a working
    /// address.</para>
    /// </summary>
    [Fact]
    public void Every_mnemonic_alias_resolves_to_the_same_offer_as_its_hash_form()
    {
        var checkedPerKind = new SortedDictionary<string, int>();
        int draws = 0;

        foreach ((string label, Matchup m) in SweepMatchups())
        {
            MarketSheet sheet = MarketSheet.Build(m);
            IReadOnlyList<Matchup> slate = Only(m);

            foreach (MarketSheetRow row in sheet.AllRows)
            {
                string address = $"1#{row.Line.ToString(CultureInfo.InvariantCulture)}";
                string? mnemonic = Mnemonic(row.Offer.Selection);

                if (mnemonic == null)
                {
                    if (row.Kind == MarketKind.Moneyline && row.Offer.Selection.Choice == MarketChoice.Draw)
                    {
                        draws++;
                        // The draw is printed on §10's slate and has no mnemonic. Under §7 it has an
                        // address like everything else — which is the whole ruling, demonstrated.
                        Assert.True(TryPick(address, slate, out Pick drawPick, out string why),
                            $"{label}: the moneyline DRAW on line {row.Line} is printed on the slate "
                            + $"and '{address}' was refused with \"{why}\".");
                        Assert.True(drawPick.Selection.Equals(row.Offer.Selection));
                    }
                    continue;
                }

                string token = "1" + mnemonic;
                Assert.True(TryPick(token, slate, out Pick viaAlias, out string refusal),
                    $"{label}: mnemonic '{token}' was refused with \"{refusal}\", but line "
                    + $"{row.Line} prints {row.Kind} (\"{row.Name}\").");
                Assert.True(TryPick(address, slate, out Pick viaAddress, out _));

                Assert.True(viaAlias.Selection.Equals(viaAddress.Selection),
                    $"{label}: '{token}' resolves to {viaAlias.Selection.Kind}/"
                    + $"{viaAlias.Selection.Choice} but '{address}' resolves to "
                    + $"{viaAddress.Selection.Kind}/{viaAddress.Selection.Choice} — the alias has "
                    + "drifted off its address.");
                Assert.Equal(viaAlias.MatchupIndex, viaAddress.MatchupIndex);

                string key = row.Kind.ToString();
                checkedPerKind[key] = checkedPerKind.TryGetValue(key, out int c) ? c + 1 : 1;
            }
        }

        foreach (KeyValuePair<string, int> pair in checkedPerKind)
            _output.WriteLine($"alias kind {pair.Key}: {pair.Value} rows checked");
        _output.WriteLine($"moneyline DRAW rows with no mnemonic, reachable only by address: {draws}");

        // All six mnemonic kinds must actually have been exercised, or the sweep proved less than
        // the test's name claims.
        foreach (string kind in new[]
        {
            nameof(MarketKind.Moneyline), nameof(MarketKind.TotalGoals), nameof(MarketKind.TotalCorners),
            nameof(MarketKind.TotalCards), nameof(MarketKind.BothTeamsToScore), nameof(MarketKind.AnytimeScorer),
        })
        {
            Assert.True(checkedPerKind.ContainsKey(kind),
                $"no {kind} row was checked — the sweep did not exercise all six aliases.");
        }
        Assert.True(draws > 0, "no moneyline DRAW was swept, so the finding above is unasserted.");
    }

    /// <summary>The six mnemonics, built from the offer itself. Null for a kind the closed mnemonic
    /// set never reached — which under §7 is no longer a problem, and is the point.</summary>
    private static string? Mnemonic(MarketSelection s) => s.Kind switch
    {
        MarketKind.Moneyline => s.Choice switch
        {
            MarketChoice.Home => "H",
            MarketChoice.Away => "A",
            _ => null, // the DRAW: printed on every slate, never mnemonicable
        },
        MarketKind.BothTeamsToScore => s.Choice == MarketChoice.Yes ? "Y" : "N",
        MarketKind.TotalGoals => "G" + OverUnder(s) + Line(s),
        MarketKind.TotalCorners => "C" + OverUnder(s) + Line(s),
        MarketKind.TotalCards => "K" + OverUnder(s) + Line(s),
        MarketKind.AnytimeScorer => "S" + (s.PlayerIndex + 1).ToString(CultureInfo.InvariantCulture),
        _ => null,
    };

    private static string OverUnder(MarketSelection s) => s.Choice == MarketChoice.Over ? "O" : "U";

    private static string Line(MarketSelection s) => s.Line.ToString(CultureInfo.InvariantCulture);

    // =======================================================================================
    // §13 GATE 7 — NO TWO ROWS ON A PAGE SHARE A NAME
    // =======================================================================================

    /// <summary>
    /// The laptop's own <c>MarketSheetTests</c> gate, transferred unchanged: <b>no two rows on a
    /// page share a name</b>. On the laptop it caught the BTTS arm — two rows both reading
    /// <c>BOTH TEAMS TO SCORE</c>, indistinguishable to a reader deciding which one to back.
    ///
    /// <para>A "page" on this surface is a rendered destination page: one section's rows, in windows
    /// of <see cref="BodyRows"/> (§4). That window is read off <c>Page.BodyRows</c> rather than
    /// retyped, so if §3's page ever changes the gate follows it.</para>
    ///
    /// <para>The name compared is <c>MarketSheetRow.Name</c> — the composer's, which is what both
    /// surfaces print (§6.6). The role WORD a scorer row carries (§6.7) is deliberately NOT folded
    /// in: it would let two identically-named players pass as distinct because one plays midfield,
    /// which is precisely the ambiguity this gate exists to refuse.</para>
    /// </summary>
    [Fact]
    public void Gate_13_7_no_two_rows_on_a_page_share_a_name()
    {
        int pages = 0;
        int rows = 0;

        foreach ((string label, Matchup m) in SweepMatchups())
        {
            MarketSheet sheet = MarketSheet.Build(m);
            foreach (MarketSheetSection section in sheet.Sections)
            {
                for (int start = 0; start < section.Count; start += BodyRows)
                {
                    pages++;
                    var onPage = new Dictionary<string, int>(StringComparer.Ordinal);
                    int take = Math.Min(BodyRows, section.Count - start);
                    for (int i = start; i < start + take; i++)
                    {
                        MarketSheetRow row = section.Rows[i];
                        rows++;
                        Assert.False(onPage.ContainsKey(row.Name),
                            $"{label}: page {section.Label}[{start / BodyRows + 1}] prints "
                            + $"\"{row.Name}\" on BOTH line {onPage.GetValueOrDefault(row.Name)} and "
                            + $"line {row.Line}. Two rows, one name — the reader cannot tell them "
                            + "apart, and this is the laptop's BTTS defect on a second surface.");
                        onPage[row.Name] = row.Line;
                    }
                }
            }
        }

        _output.WriteLine($"§13.7 swept {pages} rendered pages, {rows} rows, page window = {BodyRows}.");
        Assert.True(rows > 0, "the sweep rendered no rows — the gate asserted nothing.");
    }

    // =======================================================================================
    // §8 — REFUSALS MOVE BEFORE THE ACT
    // =======================================================================================

    /// <summary>
    /// §8's first row: <b>a fifth leg is refused at the fifth TOKEN</b>, naming the cap and the count
    /// already held — not by <c>Run.PlaceTicket</c> after the stake, the profit-boost and the
    /// modifier prompts have all been answered about a ticket that could never be placed.
    ///
    /// <para>The cap comes from <c>RunConfig.MaxLegs</c>, so the assertion holds at any cap rather
    /// than at 4. A four-token line still parses — the refusal is the fifth token, not the fourth.</para>
    /// </summary>
    [Fact]
    public void Gate_S85_a_fifth_leg_is_refused_at_the_fifth_token()
    {
        var config = new RunConfig();
        int cap = config.MaxLegs;
        Slate slate = SlateGenerator.Generate(1, new RngHub("GRAMMAR-S85"), config);
        IReadOnlyList<Matchup> matchups = slate.Matchups;

        var tokens = new List<string>();
        for (int i = 0; i < cap; i++) tokens.Add($"{i + 1}#1");

        Assert.True(TryPicks(string.Join(" ", tokens), matchups, cap, out int held, out string none),
            $"a {cap}-leg line was refused with \"{none}\" — the cap is {cap}, not {cap - 1}.");
        Assert.Equal(cap, held);

        tokens.Add($"{cap + 1}#2");
        Assert.False(TryPicks(string.Join(" ", tokens), matchups, cap, out _, out string refusal),
            $"a {cap + 1}-leg line was accepted at the picks prompt — the cap is discovered after "
            + "the stake, which is the dead click S85 refuses.");

        _output.WriteLine("fifth-leg refusal: " + refusal);
        Assert.Contains(cap.ToString(CultureInfo.InvariantCulture), refusal, StringComparison.Ordinal);
        Assert.Contains("already held", refusal, StringComparison.Ordinal);
        Assert.Contains($"{cap + 1}#2", refusal, StringComparison.Ordinal);
        Assert.True(refusal.Length <= PageWidth, $"refusal is {refusal.Length} columns.");

        // The count is checked BEFORE the fifth token is resolved: a fifth token that is ALSO
        // garbage still gets the cap refusal, because the cap is the first thing actually wrong.
        tokens[cap] = "9#9999";
        Assert.False(TryPicks(string.Join(" ", tokens), matchups, cap, out _, out string alsoBad));
        Assert.Contains("already held", alsoBad, StringComparison.Ordinal);
    }

    /// <summary>
    /// §8's fourth finding: <b>a refusal names the first thing that is actually wrong.</b>
    /// <c>1CS1-1</c> used to return "Bad line in '1CS1-1'" — a verdict on the LINE of a market that
    /// does not exist, because the line was parsed before the over/under check. <c>CS</c> is not a
    /// market prefix at all, and that is the first fault.
    /// </summary>
    [Fact]
    public void Gate_S85_a_refusal_names_the_first_thing_that_is_actually_wrong()
    {
        Slate slate = SlateGenerator.Generate(1, new RngHub("GRAMMAR-FAULT"), new RunConfig());
        IReadOnlyList<Matchup> matchups = slate.Matchups;

        Assert.False(TryPick("1CS1-1", matchups, out _, out string refusal));
        _output.WriteLine("1CS1-1 → " + refusal);
        Assert.Contains("Bad market", refusal, StringComparison.Ordinal);
        Assert.DoesNotContain("Bad line", refusal, StringComparison.Ordinal);

        // Same class, second letter: 'GX' has a real market letter and a bogus side.
        Assert.False(TryPick("1GX2.5", matchups, out _, out string side));
        Assert.Contains("Bad market", side, StringComparison.Ordinal);

        // And the line refusal survives where the line IS the first fault.
        Assert.False(TryPick("1GOxyz", matchups, out _, out string badLine));
        _output.WriteLine("1GOxyz → " + badLine);
        Assert.Contains("Bad line", badLine, StringComparison.Ordinal);
    }

    // =======================================================================================
    // §13 GATE 1 — THE PAGE, re-asserted over everything §7 and §8 add
    // =======================================================================================

    /// <summary>
    /// <b>A prompt is a rendered line</b> (§13 gate 1 — commit 1 found the old command bar at 82
    /// against an 80-column page). Everything §7 and §8 add to the surface is measured here: the new
    /// picks prompt, and every refusal the parser itself authors, at the WORST echo rather than a
    /// typical one.
    ///
    /// <para><b>Stated exclusion:</b> engine validation messages are not measured. §8 rules that they
    /// print verbatim — <c>S85</c> moves WHEN a refusal happens, never who authors it — so their
    /// width is the engine's business and shortening one here would be this surface rewriting the
    /// engine's words.</para>
    /// </summary>
    [Fact]
    public void Gate_13_1_nothing_this_grammar_adds_exceeds_the_page()
    {
        Assert.True(PicksPrompt.Length <= PageWidth,
            $"the picks prompt is {PicksPrompt.Length} columns against a {PageWidth}-column page: "
            + $"\"{PicksPrompt}\"");
        _output.WriteLine($"picks prompt: {PicksPrompt.Length} columns — \"{PicksPrompt}\"");

        Slate slate = SlateGenerator.Generate(1, new RngHub("GRAMMAR-WIDTH"), new RunConfig());
        IReadOnlyList<Matchup> matchups = slate.Matchups;
        int cap = new RunConfig().MaxLegs;

        // A deliberately over-long token on every refusal path: the echo is bounded so the SENTENCE
        // survives, which is what a refusal is for.
        const string Long = "999999999999999999999999999999999999999999999999";
        var probes = new List<string>
        {
            "1", "x", "1#", "1#0", "1#99999", "1#nine", "1D", "1CS1-1", "1GX2.5", "1GOxyz",
            "9#1", "0#1", "#5", "1S0", "1S999", "1GO99.5", "1" + Long, Long + "#" + Long,
            "1#" + Long, "1G" + Long, Long,
        };

        int measured = 0;
        foreach (string probe in probes)
        {
            if (TryPick(probe, matchups, out _, out string refusal)) continue;
            measured++;
            _output.WriteLine($"{refusal.Length,3}  {refusal}");
            Assert.True(refusal.Length <= PageWidth,
                $"refusal for '{probe}' is {refusal.Length} columns: \"{refusal}\"");
        }

        // The leg-cap refusal, at its own worst echo.
        var over = new List<string>();
        for (int i = 0; i < cap; i++) over.Add($"{i + 1}#1");
        over.Add(Long);
        Assert.False(TryPicks(string.Join(" ", over), matchups, cap, out _, out string capRefusal));
        measured++;
        _output.WriteLine($"{capRefusal.Length,3}  {capRefusal}");
        Assert.True(capRefusal.Length <= PageWidth,
            $"the leg-cap refusal is {capRefusal.Length} columns: \"{capRefusal}\"");

        _output.WriteLine($"{measured} surface-authored refusals measured, all ≤ {PageWidth}.");
        Assert.True(measured >= probes.Count / 2, "too few probes were actually refused to measure.");
    }
}
