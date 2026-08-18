using System;
using System.Collections.Generic;
using System.Globalization;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// One offer row on ENTRY's market sheet (spec-market-surfaces-2026-08-17.md §4.1: one offer
    /// per row, ratified as <c>S92</c>). Carries the priced <see cref="MarketOffer"/> and the
    /// display text composed for it — never a rendered thing, never a UI type.
    /// </summary>
    public sealed class MarketSheetRow
    {
        /// <summary>The offer's market kind — the same value as <c>Offer.Selection.Kind</c>,
        /// surfaced here so a renderer never has to reach through the selection.</summary>
        public MarketKind Kind { get; }

        /// <summary>The engine's priced offer. The PRICE is read from here (<c>Offer.Odds</c>) and
        /// is deliberately not pre-formatted: §4.4 leaves the price's ink unsettled and
        /// <c>OddsFormat</c> owns its typesetting.</summary>
        public MarketOffer Offer { get; }

        /// <summary>
        /// The printed market name, composed from <see cref="MatchModel.Fields"/> (<c>S22</c>, DD
        /// batch 4: the engine emits fields, the surface composes). <c>MatchModel.DisplayLabel</c>
        /// is the legacy packed form and is NOT used here.
        ///
        /// <para>The rule is <c>Line</c> when the engine gave one, else <c>Subject</c>. Every kind
        /// but the moneyline's two sides emits a <c>Line</c> ("OVER 2.5 GOALS", "DRAW", "2-1",
        /// "SMITH ANYTIME"); home and away moneyline emit no line at all and their name is the team
        /// (<c>Subject</c>). No kind emits neither, and <c>MarketSheetTests</c> asserts that across
        /// seeds so a future kind that leaves both blank fails the gate rather than printing an
        /// empty row.</para>
        /// </summary>
        public string Name { get; }

        /// <summary>The scorer's role word ("FORWARD"/"MIDFIELDER"/"DEFENDER"), straight from
        /// <c>MarketFields.Role</c>. Empty for every non-scorer market — an empty string, never
        /// null, so a renderer can bind it unguarded.</summary>
        public string Role { get; }

        /// <summary>
        /// This row's GLOBAL 1-based line number across the whole sheet — the number the folio
        /// counts and the contents block ranges over (§5.1, §5.2). Not an index within its group.
        /// </summary>
        public int Line { get; }

        internal MarketSheetRow(MarketKind kind, MarketOffer offer, string name, string role, int line)
        {
            Kind = kind;
            Offer = offer;
            Name = name;
            Role = role;
            Line = line;
        }
    }

    /// <summary>
    /// One <see cref="MarketKind"/>'s rows inside a destination — the second level of §5.2's
    /// contents block ("TOTAL GOALS ..... 12–17"). A group with no offers is a REAL runtime state,
    /// not a hypothetical: <c>CorrectScore</c> and <c>PlayerMultiScorer</c> are priced only above
    /// <c>RunConfig.CorrectScoreFloor</c>, so either can come back empty. §5.3 prints it anyway.
    /// </summary>
    public sealed class MarketSheetGroup
    {
        public MarketKind Kind { get; }

        /// <summary>The printed heading, from <see cref="MarketDestinations.KindLabel"/>.</summary>
        public string Label { get; }

        /// <summary>This group's rows in <c>Matchup.Markets</c> order, which is the engine's
        /// pricing order and therefore the book's own print order (the 1X2 draw sits BETWEEN the
        /// two sides because <c>BuildOffers</c> emits it there).</summary>
        public IReadOnlyList<MarketSheetRow> Rows { get; }

        public int Count => Rows.Count;

        public bool IsEmpty => Rows.Count == 0;

        /// <summary>Lowest global line number in this group, or 0 when empty. DERIVED from the rows
        /// themselves (§5.1 / <c>S74-am3</c>), never carried alongside them.</summary>
        public int FirstLine { get; }

        /// <summary>Highest global line number in this group, or 0 when empty.</summary>
        public int LastLine { get; }

        /// <summary>§5.2's contents range for this group — "12–17", or <see cref="MarketSheet.NoPricesOffered"/>
        /// when it holds nothing.</summary>
        public string RangeText => MarketSheet.RangeText(FirstLine, LastLine);

        /// <summary>§5.3 / <c>S89</c>'s heading form: a group with offers prints its count, a group
        /// with none prints <c>no prices offered</c>.</summary>
        public string CountText => MarketSheet.CountText(Count);

        internal MarketSheetGroup(MarketKind kind, string label, IReadOnlyList<MarketSheetRow> rows,
            int firstLine, int lastLine)
        {
            Kind = kind;
            Label = label;
            Rows = rows;
            FirstLine = firstLine;
            LastLine = lastLine;
        }
    }

    /// <summary>
    /// One destination's slice of the sheet — the first level of §5.2's contents block
    /// ("GOALS ..... 12–29") and one stop on §3's rail.
    ///
    /// <para><see cref="Groups"/> holds EVERY kind the destination is home to, empty ones included,
    /// because §5.3 prints them. That is what makes §3.1's rail a constant.</para>
    /// </summary>
    public sealed class MarketSheetSection
    {
        public MarketDestination Destination { get; }

        /// <summary>The printed rail label, from <see cref="MarketDestinations.Label"/>.</summary>
        public string Label { get; }

        /// <summary>Every kind this destination holds, in <see cref="MarketDestinations.KindsIn"/>
        /// order — INCLUDING kinds with no priced offers (§5.3).</summary>
        public IReadOnlyList<MarketSheetGroup> Groups { get; }

        /// <summary>Rows across all of this section's groups, in printed order.</summary>
        public IReadOnlyList<MarketSheetRow> Rows { get; }

        public int Count => Rows.Count;

        public bool IsEmpty => Rows.Count == 0;

        /// <summary>Lowest global line number in this section, or 0 when it holds nothing. Derived
        /// from its own rows — not summed from group counts, which is the arithmetic that drifts.</summary>
        public int FirstLine { get; }

        /// <summary>Highest global line number in this section, or 0 when it holds nothing.</summary>
        public int LastLine { get; }

        /// <summary>§5.2's contents range — "12–29", or <see cref="MarketSheet.NoPricesOffered"/>.</summary>
        public string RangeText => MarketSheet.RangeText(FirstLine, LastLine);

        /// <summary>§5.3 / <c>S89</c>'s count form for the rail entry.</summary>
        public string CountText => MarketSheet.CountText(Count);

        internal MarketSheetSection(MarketDestination destination, string label,
            IReadOnlyList<MarketSheetGroup> groups, IReadOnlyList<MarketSheetRow> rows,
            int firstLine, int lastLine)
        {
            Destination = destination;
            Label = label;
            Groups = groups;
            Rows = rows;
            FirstLine = firstLine;
            LastLine = lastLine;
        }
    }

    /// <summary>
    /// The derivation under ENTRY's market sheet (spec-market-surfaces-2026-08-17.md): every priced
    /// offer on a matchup, seated in its destination, numbered once, with the folio and contents
    /// strings computed FROM that numbering.
    ///
    /// <para><b>Pure logic — no UnityEngine type appears in this file and none may.</b> The renderer
    /// is a separate workstream; this is what it renders, and what the gate asserts. That split is
    /// the only reason §7's invariants are machine-checkable instead of promised.</para>
    ///
    /// <para><b>The numbering is the spine.</b> Rows are numbered 1..N in reading order —
    /// destination in <see cref="MarketDestinations.All"/> rail order, then kind in
    /// <see cref="MarketDestinations.KindsIn"/> order, then offer in <c>Matchup.Markets</c> order.
    /// The folio's "of 80" and the contents block's "12–29" are two readings of that ONE numbering,
    /// so they cannot disagree with each other or with the page. <c>S74-am3</c>: a constant that
    /// happens to equal the right answer is a constant that will stop equalling it.</para>
    /// </summary>
    public sealed class MarketSheet
    {
        /// <summary>
        /// <c>S89</c>'s exact form for a group or section with nothing priced (§5.3). Lowercase and
        /// verbatim — a racecard prints the race even when it is abandoned.
        /// </summary>
        public const string NoPricesOffered = "no prices offered";

        /// <summary>
        /// EN DASH, U+2013 — the character §5.1's folio ("46–66 of 80") and §5.2's contents ranges
        /// are set with. Written as an escape rather than a literal so the printed glyph cannot be
        /// changed by a source-file re-encoding; <c>MarketSheetTests</c> asserts the codepoint, so a
        /// hyphen-minus substituted here fails the gate.
        /// </summary>
        public const string EnDash = "\u2013";

        private readonly IReadOnlyList<MarketSheetSection> _sections;
        private readonly IReadOnlyList<MarketSheetRow> _rows;

        private MarketSheet(IReadOnlyList<MarketSheetSection> sections, IReadOnlyList<MarketSheetRow> rows)
        {
            _sections = sections;
            _rows = rows;
        }

        /// <summary>
        /// The six sections, ALWAYS all six and ALWAYS in rail order, whatever happens to be priced
        /// (§3.1, spec §7.2). The rail never reflows, so it can be authored once and measured once.
        /// </summary>
        public IReadOnlyList<MarketSheetSection> Sections => _sections;

        /// <summary>Every row on the sheet in global printed order; <c>AllRows[i].Line == i + 1</c>.</summary>
        public IReadOnlyList<MarketSheetRow> AllRows => _rows;

        /// <summary>The folio's denominator, and the count §5.4 forbids virtualising away: a rail
        /// reading "46 of 80" must be backed by 80 real rows.</summary>
        public int TotalRows => _rows.Count;

        /// <summary>
        /// Seats every offer on <paramref name="matchup"/> and numbers the sheet.
        ///
        /// <para>Nothing is filtered and nothing is invented: the row set is exactly
        /// <c>matchup.Markets</c>, re-ordered. An offer whose kind has no destination cannot be
        /// silently dropped — <see cref="MarketDestinations.For"/> throws on it first (§7.1 gate 1),
        /// and the leftover check below is the second lock in case a kind is mapped to a
        /// destination that does not list it.</para>
        /// </summary>
        public static MarketSheet Build(Matchup matchup)
        {
            if (matchup == null) throw new ArgumentNullException(nameof(matchup));
            IReadOnlyList<MarketOffer> markets = matchup.Markets;
            if (markets == null)
                throw new InvalidOperationException(
                    "MarketSheet.Build: matchup has no priced markets — build the sheet from a "
                    + "generated slate, never from a hand-made Matchup");

            // Bucket by kind, preserving Matchup.Markets order inside each bucket. For() is called
            // on every offer's kind here rather than only on the kinds the rail asks for, so an
            // unmapped kind throws even if no destination would have gone looking for it.
            var byKind = new Dictionary<MarketKind, List<MarketOffer>>();
            foreach (MarketOffer offer in markets)
            {
                MarketKind kind = offer.Selection.Kind;
                MarketDestinations.For(kind);
                if (!byKind.TryGetValue(kind, out List<MarketOffer> bucket))
                {
                    bucket = new List<MarketOffer>();
                    byKind[kind] = bucket;
                }
                bucket.Add(offer);
            }

            var allRows = new List<MarketSheetRow>(markets.Count);
            var sections = new List<MarketSheetSection>(MarketDestinations.All.Count);
            var consumed = new HashSet<MarketKind>();
            int line = 0;

            foreach (MarketDestination destination in MarketDestinations.All)
            {
                var groups = new List<MarketSheetGroup>();
                var sectionRows = new List<MarketSheetRow>();
                int sectionFirst = 0;
                int sectionLast = 0;

                // Every kind the destination holds, empty ones included — §5.3 prints them, and
                // that is precisely what keeps §3.1's rail constant across matchups.
                foreach (MarketKind kind in MarketDestinations.KindsIn(destination))
                {
                    consumed.Add(kind);
                    var groupRows = new List<MarketSheetRow>();
                    int groupFirst = 0;
                    int groupLast = 0;

                    if (byKind.TryGetValue(kind, out List<MarketOffer> offers))
                    {
                        foreach (MarketOffer offer in offers)
                        {
                            line++;
                            MatchModel.MarketFields fields = MatchModel.Fields(matchup, offer.Selection);
                            var row = new MarketSheetRow(kind, offer, NameOf(kind, fields), fields.Role, line);
                            groupRows.Add(row);
                            sectionRows.Add(row);
                            allRows.Add(row);
                            if (groupFirst == 0) groupFirst = line;
                            groupLast = line;
                            if (sectionFirst == 0) sectionFirst = line;
                            sectionLast = line;
                        }
                    }

                    groups.Add(new MarketSheetGroup(kind, MarketDestinations.KindLabel(kind),
                        groupRows, groupFirst, groupLast));
                }

                sections.Add(new MarketSheetSection(destination, MarketDestinations.Label(destination),
                    groups, sectionRows, sectionFirst, sectionLast));
            }

            // C19 made structural: the sheet is a re-ordering of matchup.Markets, so anything that
            // did not reach a row is an offer the player cannot bet. Fail loudly rather than
            // shipping a sheet that quietly omits a market.
            foreach (KeyValuePair<MarketKind, List<MarketOffer>> pair in byKind)
            {
                if (!consumed.Contains(pair.Key))
                    throw new InvalidOperationException(
                        $"MarketSheet.Build: {pair.Value.Count} {pair.Key} offer(s) reached no "
                        + "destination group — MarketDestinations.For() and KindsIn() disagree "
                        + "about where that kind lives");
            }
            if (allRows.Count != markets.Count)
                throw new InvalidOperationException(
                    $"MarketSheet.Build: seated {allRows.Count} rows for {markets.Count} priced "
                    + "offers — the sheet must be a re-ordering of matchup.Markets, nothing added, "
                    + "nothing lost (C19)");

            return new MarketSheet(sections, allRows);
        }

        /// <summary>The section for <paramref name="destination"/>. Always present — §3.1.</summary>
        public MarketSheetSection Section(MarketDestination destination)
        {
            foreach (MarketSheetSection section in _sections)
            {
                if (section.Destination == destination) return section;
            }
            throw new ArgumentOutOfRangeException(nameof(destination), destination,
                "MarketSheet.Section: no section for that destination — Sections is built from "
                + "MarketDestinations.All and must hold every member");
        }

        /// <summary>
        /// §5.1's folio: "46–66 of 80". The visible window is the caller's (it is a scroll fact);
        /// the TOTAL is this sheet's own row count and cannot be passed in, which is the whole
        /// point — "a folio that lies is worse than no folio".
        ///
        /// <para>Throws when the window is not a real window into this sheet. A folio is a claim
        /// about the page, so an impossible claim fails here rather than being printed.</para>
        /// </summary>
        public string Folio(int firstVisibleLine, int lastVisibleLine)
        {
            if (TotalRows == 0)
                throw new InvalidOperationException(
                    "MarketSheet.Folio: an empty sheet has no folio to print — there is no matchup "
                    + "that prices nothing, so this means the sheet was built from a Matchup whose "
                    + "markets were never priced");
            if (firstVisibleLine < 1 || firstVisibleLine > TotalRows)
                throw new ArgumentOutOfRangeException(nameof(firstVisibleLine), firstVisibleLine,
                    $"MarketSheet.Folio: outside 1..{TotalRows}");
            if (lastVisibleLine < 1 || lastVisibleLine > TotalRows)
                throw new ArgumentOutOfRangeException(nameof(lastVisibleLine), lastVisibleLine,
                    $"MarketSheet.Folio: outside 1..{TotalRows}");
            if (lastVisibleLine < firstVisibleLine)
                throw new ArgumentOutOfRangeException(nameof(lastVisibleLine), lastVisibleLine,
                    $"MarketSheet.Folio: last ({lastVisibleLine}) precedes first ({firstVisibleLine})");

            return string.Concat(
                firstVisibleLine.ToString(CultureInfo.InvariantCulture), EnDash,
                lastVisibleLine.ToString(CultureInfo.InvariantCulture), " of ",
                TotalRows.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// §5.2's printed line range — "12–29", or <see cref="NoPricesOffered"/> when the range is
        /// empty. A one-row range prints as "30–30": the spec shows only multi-row ranges, and
        /// collapsing it to "30" would make the contents block's shape depend on a count, which is
        /// exactly the kind of conditional a printed page does not have.
        /// </summary>
        public static string RangeText(int firstLine, int lastLine)
        {
            if (firstLine <= 0 || lastLine <= 0) return NoPricesOffered;
            return string.Concat(
                firstLine.ToString(CultureInfo.InvariantCulture), EnDash,
                lastLine.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>§5.3 / <c>S89</c>: a group with offers prints its count, a group with none
        /// prints <c>no prices offered</c>.</summary>
        public static string CountText(int count)
            => count <= 0 ? NoPricesOffered : count.ToString(CultureInfo.InvariantCulture);

        /// <summary>
        /// The composition rule for a row's printed name (see <see cref="MarketSheetRow.Name"/>):
        /// the engine's offer LINE where it emits one, otherwise its SUBJECT.
        /// </summary>
        private static string NameOf(MarketKind kind, MatchModel.MarketFields fields)
        {
            // BTTS is the one kind whose choice does NOT reach Line or Subject: Fields.Line is
            // "BOTH TEAMS TO SCORE" for both offers and Subject is empty, so the general rule below
            // would print two identical rows at two different prices — the player cannot bet the
            // offer they mean. Fields.Market is where the choice lives ("BTTS — YES"/"BTTS — NO").
            //
            // This is not a new call: A2 already ruled it on the shipped surface, and
            // BuildBothTeamsScore in SportsbookApp.cs carries the same arm for the same stated
            // reason. Matching it here keeps the sheet and the shipped rows in step. Caught by
            // MarketSheetTests' no-two-rows-share-a-name gate, which fails without this arm.
            if (kind == MarketKind.BothTeamsToScore && !string.IsNullOrEmpty(fields.Market))
                return fields.Market;

            if (!string.IsNullOrEmpty(fields.Line)) return fields.Line;
            if (!string.IsNullOrEmpty(fields.Subject)) return fields.Subject;
            return string.Empty;
        }
    }
}
