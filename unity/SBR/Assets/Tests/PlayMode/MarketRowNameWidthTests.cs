using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using TMPro;
using UnityEngine;

namespace SBR.Tests.PlayMode
{
    /// <summary>
    /// <c>S96</c>'s two measurement bindings, from DD batch 113.
    ///
    /// <para><c>S96</c> rules that the market sheet UPPERCASES its row names at the presentation
    /// layer (<see cref="SportsbookApp.PrintedRowName"/>). Uppercase is WIDER PER CHARACTER, and a
    /// name that used to fit in title case is not thereby a name that fits in caps. The DD named
    /// <c>MOOSE JAW OVERHEADS OR DRAW</c> as a long reachable form and said the row is 700px with a
    /// ~490px annotation gap, so there is <i>very probably</i> room — and then bound this seat to
    /// MEASURE it rather than reason about it.</para>
    ///
    /// <para><b>BINDING 1 — <c>C46</c>, the width.</b> The LONGEST REACHABLE row name across every
    /// <see cref="MarketKind"/>, uppercased, measured with the same <c>LaptopUi.MeasureWidth</c> the
    /// row itself uses, at the row's own size and tracking, against the row's own name cell. Not a
    /// sample: the club and player pools are enumerated whole and every one of them is put through
    /// the real composition path (<see cref="MatchModel.Fields"/> → <see cref="MarketSheet.Build"/>),
    /// so the strings measured here are strings the sheet can actually print.</para>
    ///
    /// <para><b>BINDING 2 — <c>S84</c>, the meaning.</b> Uppercasing destroys meaning if a name
    /// depends on its case — a lowercase particle (<c>van der Berg</c>, <c>de Silva</c>), an
    /// <c>O'Brien</c>-style form, a <c>McX</c> internal capital. The DD's measured expectation is
    /// that the pools hold none, but the check is the POOL'S, not the sample's.</para>
    ///
    /// <para><b>Why the pools are read by reflection.</b> <c>SlateGenerator</c>'s name arrays are
    /// private, and the alternative — generating slates until the observed set stops growing — is a
    /// sampling argument, which is precisely what these bindings exist to forbid. Reflection reads
    /// the pool ITSELF, and fails loudly by name if the engine ever renames the fields.</para>
    ///
    /// <para><b>Why PlayMode.</b> This has to reach <c>LaptopUi.MeasureWidth</c> and
    /// <c>LaptopTrack.Records</c>, both <c>internal</c> to <c>SBR.Game</c>, and
    /// <c>AssemblyInfo.cs</c> grants <c>InternalsVisibleTo</c> to this assembly only. Nothing here
    /// needs a scene, so these are plain <c>[Test]</c>s rather than <c>[UnityTest]</c>s.</para>
    /// </summary>
    public class MarketRowNameWidthTests
    {
        /// <summary>The face the offer row's name renders in — <c>MakeOfferRow</c>'s
        /// <c>_fontCond</c>, resolved by <c>LaptopScreen.Awake</c> from this same path.</summary>
        private const string CondensedFontPath = "SureThing/Fonts/ArchivoNarrow SDF";

        /// <summary>The row name's own size and tracking, from <c>MakeOfferRow</c>'s MakeText call.
        /// Whatever a slot renders with, it measures with (LaptopUi.MeasureWidth's own comment) —
        /// measuring at a different tracking would report narrow by length × tracking × size.</summary>
        private const int RowNameSize = 19;

        private const float RowNameTracking = LaptopTrack.Records;

        /// <summary>Any ordinary seed. The seed decides WHICH names a slate happens to draw and this
        /// gate does not care: the matchup below is re-skinned with every name in the pool, so the
        /// only thing the seed supplies is a real priced market set.</summary>
        private const string Seed = "48120736";

        // ── the pools ───────────────────────────────────────────────────────────────────────────

        private static string[] Pool(string field)
        {
            FieldInfo info = typeof(SlateGenerator).GetField(
                field, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(info,
                $"SlateGenerator.{field} is gone — C46/S84 are POOL checks, and a pool this gate "
                + "cannot read is a pool it cannot check. Re-point this at the engine's name arrays "
                + "rather than deleting the binding.");
            var pool = info.GetValue(null) as string[];
            Assert.IsNotNull(pool, $"SlateGenerator.{field} is no longer a string[]");
            Assert.Greater(pool.Length, 0, $"SlateGenerator.{field} is empty");
            return pool;
        }

        /// <summary>Every club the generator can name: <c>MakeTeam</c> composes
        /// <c>"{City} {Noun}"</c>, the city drawn uniformly and the noun from a shuffle, so every
        /// pairing is reachable.</summary>
        private static List<string> Clubs()
        {
            string[] cities = Pool("Cities");
            string[] nouns = Pool("Nouns");
            var clubs = new List<string>(cities.Length * nouns.Length);
            foreach (string city in cities)
                foreach (string noun in nouns)
                    clubs.Add(city + " " + noun);
            return clubs;
        }

        /// <summary>Every player the generator can name: <c>MakeRoster</c> composes
        /// <c>"{First} {Last}"</c> from two pools, rejecting only duplicates within one board.</summary>
        private static List<string> Players()
        {
            string[] first = Pool("PlayerFirst");
            string[] last = Pool("PlayerLast");
            var players = new List<string>(first.Length * last.Length);
            foreach (string f in first)
                foreach (string l in last)
                    players.Add(f + " " + l);
            return players;
        }

        // ── BINDING 2 · S84 ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>S84</c>: uppercasing a name that DEPENDS on its case destroys the name. The pools are
        /// enumerated whole and every entry is checked for the three forms that would break —
        /// a lowercase particle, an apostrophe-carrying form, an internal capital — plus the
        /// general property that the transform is a pure case fold and loses no character.
        ///
        /// <para>A hit here is NOT a bug to fix in this file. It means <c>S96</c> was ruled against
        /// a pool that has since acquired a name the ruling cannot serve, and it goes back to the
        /// Design Director with the name in the failure message.</para>
        /// </summary>
        [Test]
        public void No_pooled_name_depends_on_its_case()
        {
            string[] cities = Pool("Cities");
            string[] nouns = Pool("Nouns");
            string[] first = Pool("PlayerFirst");
            string[] last = Pool("PlayerLast");
            List<string> clubs = Clubs();
            List<string> players = Players();

            var offenders = new List<string>();
            void Check(string where, string value)
            {
                foreach (string word in value.Split(' '))
                {
                    if (word.Length == 0) continue;
                    if (char.IsLower(word[0]))
                        offenders.Add($"{where} '{value}': lowercase particle '{word}'");
                    if (word.IndexOf('\'') >= 0 || word.IndexOf('’') >= 0)
                        offenders.Add($"{where} '{value}': apostrophe form '{word}'");
                    for (int i = 1; i < word.Length; i++)
                        if (char.IsUpper(word[i]))
                        {
                            offenders.Add($"{where} '{value}': internal capital in '{word}'");
                            break;
                        }
                }
                // The general property, which catches anything the three shapes above miss: the
                // uppercase form must differ from the original ONLY in case, and must not change
                // length (ß → SS, ﬁ → FI and the Turkish dotted i are all length- or identity-
                // changing folds).
                string upper = SportsbookApp.PrintedRowName(value);
                if (upper.Length != value.Length)
                    offenders.Add($"{where} '{value}': uppercasing changes length ('{upper}')");
                if (!string.Equals(upper, value, StringComparison.OrdinalIgnoreCase))
                    offenders.Add($"{where} '{value}': uppercase form '{upper}' is not a case fold");
            }

            foreach (string city in cities) Check("City", city);
            foreach (string noun in nouns) Check("Noun", noun);
            foreach (string name in first) Check("PlayerFirst", name);
            foreach (string name in last) Check("PlayerLast", name);
            foreach (string club in clubs) Check("Club", club);
            foreach (string player in players) Check("Player", player);

            string sizes = $"cities {cities.Length} · nouns {nouns.Length} · clubs {clubs.Count} · "
                + $"first {first.Length} · last {last.Length} · players {players.Count}";
            Debug.Log($"[S84] pool scan clean — {sizes}");

            Assert.IsEmpty(offenders,
                "S84: a pooled name DEPENDS ON ITS CASE, so S96's uppercasing would destroy it. "
                + "This is a Design Director matter, not a fix — S96 ruled that case is typography "
                + "on the assumption that every club and player is plain title case. Pools scanned: "
                + sizes + ". Offending: " + string.Join(" · ", offenders));
        }

        // ── BINDING 1 · C46 ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// <c>C46</c>: the LONGEST reachable uppercase row name must fit between the name's left pad
        /// and the price cell's left edge, less the annotation gap — <c>MakeOfferRow</c>'s own
        /// numbers, read through <see cref="SportsbookApp.OfferNameCellWidth"/> so this gate and the
        /// row can never be given different ones.
        ///
        /// <para><b>Judged on the SCROLLING row width</b> (<see cref="SportsbookApp.ScrollingOfferRowWidth"/>),
        /// because the market sheet scrolls: <c>BuildScrollingBody</c> takes the position rail's 8px
        /// out of the row when the content overflows, and every ENTRY destination overflows at the
        /// shipped config. Gating on the 700px fitting case would be gating on the case the player
        /// almost never sees.</para>
        ///
        /// <para><b>Nothing here is composed by the test.</b> A real priced matchup is re-skinned
        /// with each pooled name and put through <see cref="MarketSheet.Build"/>, so the wording
        /// rules — <c>{CLUB} OR DRAW</c>, <c>{CLUB} OVER 0.5 GOALS</c>, <c>{PLAYER} ANYTIME</c>,
        /// <c>{PLAYER} 2+</c> and the rest — come from the engine and the derivation layer rather
        /// than from a second copy of them written down here.</para>
        ///
        /// <para><b>A failure is a DD matter.</b> Every way of making an over-long name fit is a
        /// design call this seat does not hold: truncating changes a ruled string, shrinking breaks
        /// §4.5's 13px floor, and widening the name cell takes width from the price. So this fails
        /// with every measured number instead.</para>
        ///
        /// <para><b>It DID fail once, and the DD ruled on it.</b> First measurement:
        /// <c>SAN FRANCISCO SPREADSHEETS UNDER 4.5 CORNERS</c> at 493.68px against a 480px cell —
        /// five names over, and seventeen printing no leader dots at all. The ruling was to widen
        /// the name cell out of the price cell's slack, which was measured at 129.39px (the widest
        /// reachable price is 46.61px against a 176px cell) and 16px of it spent — see
        /// <see cref="SportsbookApp.OfferPriceCellWidth"/>. The assertion below is UNCHANGED by that;
        /// only the geometry it reads moved.</para>
        ///
        /// <para><b>The headroom is 2.32px.</b> That is deliberate — the DD authorised the deficit,
        /// not a margin — and it is why this gate matters more now than when it was failing: one
        /// more city or noun in <c>SlateGenerator</c> can spend it. If this goes red again the
        /// price cell still holds ~113px of slack, but spending it further compresses the selection
        /// ring below its native aspect, which is a DD call and not a repair to make here.</para>
        /// </summary>
        [Test]
        public void The_longest_uppercase_row_name_fits_the_row_it_is_printed_on()
        {
            var font = Resources.Load<TMP_FontAsset>(CondensedFontPath);
            Assert.IsNotNull(font, $"missing font asset: {CondensedFontPath}");

            List<string> clubs = Clubs();
            List<string> players = Players();

            var run = new Run(Seed, new RunConfig());
            Matchup source = run.CurrentSlate.Matchups[0];
            int rosterSlots = source.Home.Players.Count + source.Away.Players.Count;
            Assert.Greater(rosterSlots, 0,
                "the seeded matchup has no roster, so no player-derived row name would be reached");

            // The widest club by MEASURE, not by character count — it is the club held constant
            // while the player pool is walked.
            string widestClub = Widest(font, clubs);

            var measured = new Dictionary<string, float>(StringComparer.Ordinal);
            var kindOf = new Dictionary<string, MarketKind>(StringComparer.Ordinal);

            // Pass A — every club in the pool, roster held at one arbitrary window of the player
            // pool (pass B is what walks the players).
            foreach (string club in clubs)
                Collect(font, Reskin(source, club, players, 0), measured, kindOf);

            // Pass B — every player in the pool, club held at the widest. The roster is a window
            // into the pool, so it takes ceil(pool / slots) re-skins to cover all of it.
            for (int offset = 0; offset < players.Count; offset += rosterSlots)
                Collect(font, Reskin(source, widestClub, players, offset), measured, kindOf);

            string widestName = null;
            float widest = -1f;
            foreach (KeyValuePair<string, float> pair in measured)
                if (pair.Value > widest)
                {
                    widest = pair.Value;
                    widestName = pair.Key;
                }

            float scrolling = SportsbookApp.ScrollingOfferRowWidth;
            float cell = SportsbookApp.OfferNameCellWidth(scrolling);
            float fitting = SportsbookApp.OfferNameCellWidth(SportsbookApp.EntryBoardWidth);

            string report =
                $"longest of {measured.Count} distinct reachable row names is "
                + $"'{widestName}' [{kindOf[widestName]}] at "
                + widest.ToString("0.##", CultureInfo.InvariantCulture) + "px, against a "
                + cell.ToString("0.##", CultureInfo.InvariantCulture) + "px name cell on the "
                + scrolling.ToString("0.##", CultureInfo.InvariantCulture) + "px scrolling row ("
                + fitting.ToString("0.##", CultureInfo.InvariantCulture) + "px on the "
                + SportsbookApp.EntryBoardWidth.ToString("0.##", CultureInfo.InvariantCulture)
                + "px fitting row) · headroom "
                + (cell - widest).ToString("0.##", CultureInfo.InvariantCulture) + "px · pools: "
                + $"{clubs.Count} clubs, {players.Count} players · measured at {RowNameSize}px, "
                + "tracking " + RowNameTracking.ToString("0.###", CultureInfo.InvariantCulture)
                + ", " + CondensedFontPath;
            Debug.Log("[C46] " + report);

            Assert.LessOrEqual(widest, cell,
                "C46 — THE LONGEST UPPERCASE ROW NAME DOES NOT FIT. S96 uppercases the row name at "
                + "the presentation layer, and uppercase is wider per character; the title-case form "
                + "this replaced fitted. " + report + ". This is a Design Director call, not a "
                + "layout fix: truncating changes a ruled string, shrinking breaks §4.5's 13px "
                + "product-fact floor, and widening the name cell spends the price cell's width.");
        }

        // ── plumbing ────────────────────────────────────────────────────────────────────────────

        private static string Widest(TMP_FontAsset font, IReadOnlyList<string> names)
        {
            string best = names[0];
            float bestWidth = -1f;
            foreach (string name in names)
            {
                float width = LaptopUi.MeasureWidth(font, SportsbookApp.PrintedRowName(name),
                    RowNameSize, RowNameTracking);
                if (width <= bestWidth) continue;
                bestWidth = width;
                best = name;
            }
            return best;
        }

        /// <summary>Builds <paramref name="matchup"/>'s sheet and measures every row name the way
        /// the row prints it. Distinct strings only — the same name reached twice measures the same
        /// and MeasureWidth is the expensive part.</summary>
        private static void Collect(TMP_FontAsset font, Matchup matchup,
            IDictionary<string, float> measured, IDictionary<string, MarketKind> kindOf)
        {
            MarketSheet sheet = MarketSheet.Build(matchup);
            foreach (MarketSheetRow row in sheet.AllRows)
            {
                string printed = SportsbookApp.PrintedRowName(row.Name);
                if (measured.ContainsKey(printed)) continue;
                measured[printed] = LaptopUi.MeasureWidth(font, printed, RowNameSize, RowNameTracking);
                kindOf[printed] = row.Kind;
            }
        }

        /// <summary>The same matchup wearing other names. Latents, stats, config and the priced
        /// market list are carried over untouched, so the sheet built from it holds exactly the rows
        /// the real slate holds — only the names move, which is the only thing C46 measures.
        ///
        /// <para>Home and away take the SAME club name on purpose: it is the widest-case question
        /// being asked, and two rows sharing a name is a width fact, not a sheet the player sees.</para></summary>
        private static Matchup Reskin(Matchup source, string club, IReadOnlyList<string> playerPool,
            int playerOffset)
        {
            Team away = ReskinTeam(source.Away, club, playerPool, playerOffset);
            Team home = ReskinTeam(source.Home, club, playerPool,
                playerOffset + source.Away.Players.Count);
            return new Matchup(source.Index, home, away, source.TrueHomeProb,
                source.HomeOdds, source.AwayOdds, source.Latents, source.HomeStats, source.AwayStats,
                source.Markets, source.ModelConfig);
        }

        private static Team ReskinTeam(Team source, string club, IReadOnlyList<string> playerPool,
            int offset)
        {
            var players = new List<Player>(source.Players.Count);
            for (int i = 0; i < source.Players.Count; i++)
            {
                Player original = source.Players[i];
                players.Add(new Player(playerPool[(offset + i) % playerPool.Count],
                    original.Role, original.ScoringWeight));
            }
            return new Team(club, source.Wins, source.Losses, players);
        }
    }
}
