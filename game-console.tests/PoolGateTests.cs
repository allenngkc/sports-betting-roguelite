using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SBR.Engine;
using SBR.Game;
using Xunit.Abstractions;

namespace SBR.ConsoleGame.Tests;

/// <summary>
/// spec-console-surfaces-2026-08-19.md §13 gate 3: "<c>C46</c> against the POOL, not the seed
/// (<c>S84</c>, <c>S96-am</c>): the widest-name assertion is constructed from <c>Cities × Nouns</c>
/// and <c>PlayerFirst × PlayerLast</c>, in-code, not from whatever the run deals." This is the one
/// gate of the spec's eight that depends only on the engine — the other seven need the rebuilt
/// <c>BettingScreen.cs</c> to assert against.
///
/// <para>The discipline this enforces has a three-time track record on this project (§13.3's own
/// citation, <c>S84</c> / <c>S96-am</c>): a named "worst case" that was not the worst, a "full" cell
/// that was 73.5% empty, and two candidates that tied. All three are the same failure — trusting
/// what one seed happened to deal instead of enumerating the pool. This fixture enumerates the
/// pool, in code, by reflecting <see cref="SlateGenerator"/>'s own arrays, and composes the widest
/// row name through the real <see cref="MarketSheet"/> composer rather than re-deriving its rules
/// by hand.</para>
/// </summary>
public class PoolGateTests
{
    private readonly ITestOutputHelper _output;

    public PoolGateTests(ITestOutputHelper output) => _output = output;

    // ---- Reflection into SlateGenerator's own arrays. They are `private static readonly` by
    // design — SlateGenerator has no reason to expose them to a caller — so reflection is the
    // honest way to read the ACTUAL pool rather than retyping it by hand. A hand-typed copy could
    // silently drift from the array this gate exists to measure, which would defeat the gate: it
    // would then be asserting its own transcription, not the engine's.
    private static string[] ReflectPool(string fieldName)
    {
        FieldInfo? field = typeof(SlateGenerator).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        if (field is null)
            throw new InvalidOperationException(
                $"SlateGenerator.{fieldName} was not found by reflection. That is the pool gate "
                + "failing to find the pool it exists to measure — report it as a finding, do not "
                + "hand-type a replacement list to make the test pass.");
        var value = field.GetValue(null) as string[];
        return value is { Length: > 0 }
            ? value
            : throw new InvalidOperationException($"SlateGenerator.{fieldName} reflected as null or empty.");
    }

    private static string[] Cities => ReflectPool("Cities");
    private static string[] Nouns => ReflectPool("Nouns");
    private static string[] PlayerFirst => ReflectPool("PlayerFirst");
    private static string[] PlayerLast => ReflectPool("PlayerLast");

    // The DD's own identified champion (GEOMETRY.txt, spec §3) and the tie this fixture found
    // beside it — see Widest_pool_entries_match_the_DDs_measured_widths_and_surface_the_tie_it_missed.
    private const string LaptopChampionClub = "San Francisco Spreadsheets";
    private const string TiedChampionClub = "San Francisco Gravediggers";
    private const string ChampionPlayer = "Darryl Pavement";
    private const string FillerClub = "Atlanta Yams"; // any distinct, unremarkable pool member

    [Fact]
    public void Enumerated_pools_match_the_DDs_measured_sizes()
    {
        List<string> clubs = CrossProduct(Cities, Nouns);
        List<string> players = CrossProduct(PlayerFirst, PlayerLast);

        _output.WriteLine($"clubs   : {clubs.Count} constructible ({Cities.Length} cities x {Nouns.Length} nouns)");
        _output.WriteLine($"players : {players.Count} constructible ({PlayerFirst.Length} first x {PlayerLast.Length} last)");

        // spec §3 / GEOMETRY.txt: "SlateGenerator: 16 cities x 20 nouns = 320 constructible clubs";
        // "12 x 12 = 144 constructible players". Verified here by reflecting the real arrays, not
        // trusted from the citation — see the DD's own S84/S96-am discipline this gate enforces.
        Assert.Equal(16, Cities.Length);
        Assert.Equal(20, Nouns.Length);
        Assert.Equal(12, PlayerFirst.Length);
        Assert.Equal(12, PlayerLast.Length);
        Assert.Equal(320, clubs.Count);
        Assert.Equal(144, players.Count);
    }

    [Fact]
    public void Widest_pool_entries_match_the_DDs_measured_widths_and_surface_the_tie_it_missed()
    {
        List<string> clubs = CrossProduct(Cities, Nouns);
        List<string> players = CrossProduct(PlayerFirst, PlayerLast);

        int widestClubLen = clubs.Max(s => s.Length);
        int widestPlayerLen = players.Max(s => s.Length);
        List<string> widestClubs = clubs.Where(s => s.Length == widestClubLen).Distinct()
            .OrderBy(s => s, StringComparer.Ordinal).ToList();
        List<string> widestPlayers = players.Where(s => s.Length == widestPlayerLen).Distinct()
            .OrderBy(s => s, StringComparer.Ordinal).ToList();

        _output.WriteLine($"widest club(s)   ({widestClubLen} chars): {string.Join(" | ", widestClubs)}");
        _output.WriteLine($"widest player(s) ({widestPlayerLen} chars): {string.Join(" | ", widestPlayers)}");

        // spec §3 / GEOMETRY.txt: "widest 26 characters" (clubs) / "widest 15" (players). Both
        // confirmed by enumeration.
        Assert.Equal(26, widestClubLen);
        Assert.Equal(15, widestPlayerLen);

        string championPlayer = Assert.Single(widestPlayers);
        Assert.Equal(ChampionPlayer, championPlayer);

        // FINDING — reported here, not adjusted away. The DD's own GEOMETRY.txt and spec §3 name
        // "San Francisco Spreadsheets" as THE widest club, singular. It is not unique: "Spreadsheets"
        // and "Gravediggers" are both 12-character nouns — the two widest, tied — and "San Francisco"
        // (13 chars) is the unique widest city, so pairing it with EITHER noun produces a 26-char
        // club name. Nothing downstream breaks the tie: MarketSheet.NameOf's team-scoped suffixes
        // are a function of the SELECTION (kind/line/choice), never of the team name, so every
        // team-scoped row name ties too — proved through the real composer in
        // Widest_constructible_row_name_matches_the_laptops_champion_through_the_real_composer below,
        // not just asserted here from the string arithmetic.
        Assert.Equal(2, widestClubs.Count);
        Assert.Contains(LaptopChampionClub, widestClubs);
        Assert.Contains(TiedChampionClub, widestClubs);
    }

    [Fact]
    public void Widest_constructible_row_name_matches_the_laptops_champion_through_the_real_composer()
    {
        Matchup matchup = BuildChampionMatchup(LaptopChampionClub);
        MarketSheet sheet = MarketSheet.Build(matchup);

        // S96 / spec §6.5: "the words are the engine's, the case is the surface's." MarketSheetRow
        // .Name is mixed case wherever it carries a Team.Name — MatchModel.Fields never uppercases
        // a team name; only the AnytimeScorer/PlayerMultiScorer arms uppercase the PLAYER name,
        // inside the engine, because that one is a DS choice-column value. The console uppercases
        // team-derived names at the presentation layer, so this reproduces that step explicitly
        // rather than assume the engine already did it. Uppercasing ASCII never changes LENGTH, so
        // this step cannot change which row is widest — it is done anyway so the asserted TEXT
        // matches what the surface actually prints, not the engine's raw case.
        List<string> namesByLength = sheet.AllRows
            .Select(r => r.Name.ToUpperInvariant())
            .OrderByDescending(n => n.Length)
            .ToList();
        string champion = namesByLength[0];

        _output.WriteLine($"sheet rows: {sheet.TotalRows}");
        _output.WriteLine($"widest row name ({champion.Length} chars): {champion}");

        // ---- THE GATE. spec §13: "a real advantage of this medium: every geometric gate here is
        // a string-length assertion, which is exact rather than measured. Where the laptop needed
        // an in-engine 493.69px measurement, the console needs line.Length <= 80." This IS that
        // assertion — monospace end to end, so a character count is the whole proof, no rendered
        // pixel measurement required or possible on this surface.
        Assert.Equal(44, champion.Length);
        Assert.Equal("SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS", champion);

        const int PageColumns = 80; // spec §3: "RULED: the console's page is 80 columns x 24 rows"
        const int ChromeColumns = 19; // spec §6: line number + price + probability
        const int NameFieldColumns = PageColumns - ChromeColumns; // 61 — spec §6's ruled figure
        Assert.Equal(61, NameFieldColumns);
        Assert.True(champion.Length <= NameFieldColumns,
            $"widest constructible row name is {champion.Length} chars, wider than the "
            + $"{NameFieldColumns}-column name field (page {PageColumns} - chrome {ChromeColumns}) "
            + "-- the 80-column page ruling (spec section 3) no longer holds and needs re-deriving");

        // ---- The tie, proved through MarketSheet.Build itself, not just club-name arithmetic: a
        // second matchup, differing ONLY in which 26-char club name sits at home, reaches the exact
        // same 44-char maximum with DIFFERENT text.
        Matchup tiedMatchup = BuildChampionMatchup(TiedChampionClub);
        string tiedChampion = MarketSheet.Build(tiedMatchup).AllRows
            .Select(r => r.Name.ToUpperInvariant())
            .OrderByDescending(n => n.Length)
            .First();
        _output.WriteLine($"tie check ({TiedChampionClub}): {tiedChampion.Length} chars: {tiedChampion}");
        Assert.Equal(44, tiedChampion.Length);
        Assert.Equal("SAN FRANCISCO GRAVEDIGGERS UNDER 4.5 CORNERS", tiedChampion);
        Assert.NotEqual(champion, tiedChampion); // same LENGTH, different TEXT -- that is the tie
    }

    [Fact]
    public void Ranked_widest_name_per_market_kind_is_reported_for_diffing_against_GEOMETRY_txt()
    {
        Matchup matchup = BuildChampionMatchup(LaptopChampionClub);
        MarketSheet sheet = MarketSheet.Build(matchup);
        // C29: a run with zero executed cases is a failed run, not a pass -- and the same logic
        // applies one level down, to the sheet this test's whole report is read off.
        Assert.True(sheet.TotalRows > 0, "C29: a sheet with zero rows proves nothing");

        var ranked = sheet.AllRows
            .Select(r => (Bucket: BucketOf(r), Name: r.Name.ToUpperInvariant()))
            .GroupBy(x => x.Bucket)
            .Select(g => g.OrderByDescending(x => x.Name.Length).First())
            .OrderByDescending(x => x.Name.Length)
            .ToList();

        _output.WriteLine("WIDEST CONSTRUCTIBLE ROW NAME under MarketSheet.NameOf (game-console.tests, independently re-derived)");
        foreach (var row in ranked)
            _output.WriteLine($"{row.Name.Length,4}  {row.Bucket,-22} {row.Name}");
        _output.WriteLine("");
        _output.WriteLine($"WORST = {ranked[0].Name.Length} chars, {ranked[0].Bucket}: {ranked[0].Name}");
        _output.WriteLine(
            $"(bucket count: {ranked.Count} of 15 expected -- 14 OFFERED MarketKinds, with Moneyline "
            + "split team/fixed-phrase, matching GEOMETRY.txt's own bucketing. Was 17: DoubleChance "
            + "left the offered set 2026-08-24 and took BOTH its buckets -- team and fixed-phrase -- "
            + "with it; spec-doublechance-removal-2026-08-24.md)");

        // Loose structural sanity only -- the per-row TEXT is reported above for a human diff
        // against docs/design/dd-import/console-read-2026-08-19/GEOMETRY.txt, not re-asserted here.
        // Where a specific LINE VALUE ties in width within one bucket (e.g. TeamGoalLines
        // {0.5, 1.5} are both 3 characters), which tied value this LINQ pipeline happens to print
        // depends on MarketSheet's own row order, not on anything this test controls -- a
        // different printed line at an IDENTICAL length is not a disagreement. A different length
        // would be, and that is what CorrectChampionLength below still checks.
        // 15, down from 17: DoubleChance left the offered set 2026-08-24 and it carried TWO buckets
        // (HomeOrAway's fixed phrase and the two team-named choices). Asserted rather than relaxed —
        // the count is the point of this gate, and a bucket vanishing unnoticed is what it catches.
        Assert.Equal(15, ranked.Count);
        Assert.Equal(44, ranked[0].Name.Length);
    }

    // ---- Construction. Two-step, mirroring SlateGenerator.Generate's own shape: MatchModel
    // .BuildOffers reads a Matchup's Latents/Players/ModelConfig to price a board, but
    // Matchup.Markets has no public setter (Matchup.SetMarkets is internal -- SlateGenerator's own
    // privilege, not this test's, and not one InternalsVisibleTo is asked for here). So a shell
    // matchup (empty board) prices the offers, and a second, otherwise-identical Matchup is
    // constructed carrying that priced board for MarketSheet.Build. Both share the same
    // Latents/ModelConfig instance, so nothing about the pricing differs between the two -- this
    // is not a re-roll, just a second constructor call with the board filled in.
    private static Matchup BuildChampionMatchup(string homeClubName)
    {
        var config = new RunConfig();
        // p = 0.5, every tempo = 1.0: the exact centre-of-the-box point that OfferabilityTests'
        // Every_offered_selection_prices_above_evens_across_the_whole_latent_box (engine.tests)
        // already proves cannot make MatchModel.Offer throw for ANY selection this board can carry.
        const double p = 0.5;
        MatchLatents latents = MatchModel.LatentsFor(p, 1.0, 1.0, 1.0, config);

        var otherNames = new Queue<string>(
            CrossProduct(PlayerFirst, PlayerLast).Where(n => n != ChampionPlayer));
        IReadOnlyList<Player> homeRoster = BuildRoster(config, otherNames, ChampionPlayer);
        IReadOnlyList<Player> awayRoster = BuildRoster(config, otherNames, null);

        var home = new Team(homeClubName, 5, 4, homeRoster);
        var away = new Team(FillerClub, 4, 5, awayRoster);

        var shell = new Matchup(0, home, away, p, 2.0, 2.0, latents, default, default,
            Array.Empty<MarketOffer>(), config);
        IReadOnlyList<MarketOffer> offers = MatchModel.BuildOffers(shell, config);
        return new Matchup(0, home, away, p, 2.0, 2.0, latents, default, default, offers, config);
    }

    /// <summary>Same role/weight pattern <c>SlateGenerator.MakeRoster</c> uses (3 FW, 2 MF, 2 DF
    /// repeating every 7, unjittered role weight -- a legal <c>ScoringWeightJitter = 0</c> roster),
    /// so this is a roster a real slate could deal. Only the NAMES are chosen here, to seat the
    /// pool's own champion directly rather than search seeds until one deals him (exactly the
    /// seed's-champion discipline this gate exists to avoid).</summary>
    private static IReadOnlyList<Player> BuildRoster(RunConfig config, Queue<string> namePool, string? championAtSlot0)
    {
        var players = new List<Player>(config.PlayersPerTeam);
        for (int i = 0; i < config.PlayersPerTeam; i++)
        {
            PlayerRole role = i % 7 < 3 ? PlayerRole.FW : i % 7 < 5 ? PlayerRole.MF : PlayerRole.DF;
            double weight = role == PlayerRole.FW ? config.ForwardScoringWeight
                : role == PlayerRole.MF ? config.MidfielderScoringWeight : config.DefenderScoringWeight;
            string name = i == 0 && championAtSlot0 != null ? championAtSlot0 : namePool.Dequeue();
            players.Add(new Player(name, role, weight));
        }
        return players;
    }

    private static List<string> CrossProduct(string[] a, string[] b)
    {
        var result = new List<string>(a.Length * b.Length);
        foreach (string x in a)
            foreach (string y in b)
                result.Add($"{x} {y}");
        return result;
    }

    /// <summary>Groups rows the way a human reads GEOMETRY.txt: one bucket per <see cref="MarketKind"/>,
    /// except Moneyline and DoubleChance -- each has one choice that carries a team name (variable
    /// width) and one that does not (a fixed phrase), which GEOMETRY.txt lists as its own line.</summary>
    private static string BucketOf(MarketSheetRow row) => row.Kind switch
    {
        MarketKind.Moneyline => row.Offer.Selection.Choice == MarketChoice.Draw
            ? "Moneyline (draw)" : "Moneyline (team)",
        MarketKind.DoubleChance => row.Offer.Selection.Choice == MarketChoice.HomeOrAway
            ? "Double chance (both)" : "Double chance (team)",
        MarketKind.TotalGoals => "Total goals",
        MarketKind.BothTeamsToScore => "BTTS",
        MarketKind.TotalCorners => "Total corners",
        MarketKind.TotalCards => "Total cards",
        MarketKind.AnytimeScorer => "Anytime scorer",
        MarketKind.Handicap => "Handicap",
        MarketKind.TeamTotalGoals => "Team total goals",
        MarketKind.CorrectScore => "Correct score",
        MarketKind.WinningMargin => "Winning margin",
        MarketKind.TotalGoalsOddEven => "Odd/even",
        MarketKind.TeamTotalCorners => "Team total corners",
        MarketKind.TeamTotalCards => "Team total cards",
        MarketKind.PlayerMultiScorer => "Player multi",
        _ => throw new ArgumentOutOfRangeException(nameof(row), row.Kind, "unhandled MarketKind -- extend BucketOf()"),
    };
}
