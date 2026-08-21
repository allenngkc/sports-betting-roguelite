using System;
using System.Collections.Generic;
using System.Globalization;
using SBR.Engine;
using SBR.Game;

namespace SBR.ConsoleGame;

internal enum BetAction { Locked, Quit }

/// <summary>
/// The Phase.Betting screen and its single-letter command loop. Renders the header (bank, target,
/// any bookie debt), relics, the slate, and tickets placed so far, then dispatches B / L / Q. Every
/// engine call is wrapped so a bad input prints the message and returns to the loop — the shell
/// never crashes.
/// </summary>
internal static class BettingScreen
{
    public static BetAction Run(Run run)
    {
        _slatePage = 0;
        while (true)
        {
            Render(run);
            string cmd = Ui.Prompt(CommandBar()).ToUpperInvariant();
            if (Ui.Eof) return BetAction.Quit;
            switch (cmd)
            {
                case "B":
                    Build(run);
                    break;

                case "N":
                    _slatePage++;
                    break;

                case "P":
                    if (_slatePage > 0) _slatePage--;
                    break;

                case "T":
                    ShowTickets(run);
                    break;

                case "K":
                    try
                    {
                        run.PlayBookiesMarker();
                        Ui.WriteLine(ConsoleColor.Cyan,
                            $"MARKER PLAYED — this round's payment drops to {Ui.Money(run.CurrentPayment)}.");
                    }
                    catch (Exception ex) { Ui.WriteLine(ConsoleColor.Red, ex.Message); }
                    Ui.Pause();
                    break;

                case string detail when detail.StartsWith("M ", StringComparison.Ordinal):
                    ShowDetail(run, detail.Substring(2));
                    break;

                case "L":
                    if (run.Tickets.Count == 0 && !Ui.Confirm("no bets this round? the payment still comes due — y/n: ")) break;
                    run.LockRound();
                    return BetAction.Locked;

                case "Q":
                    if (Ui.Confirm("quit the run? y/n: ")) return BetAction.Quit;
                    break;

                default:
                    Unknown();
                    break;
            }
        }
    }

    private static void Unknown()
    {
        Ui.WriteLine(ConsoleColor.Red, "Unknown command.");
        Ui.Pause();
    }

    // Trimmed from "[M n] match detail": the old bar measured 82 columns against §3's 80-column
    // page, and a prompt is a rendered line like any other (§13 gate 1).
    private static string CommandBar()
        => "[B]uild  [M n] markets  [T]ickets  [K] marker  [L]ock  [Q]uit  > ";

    // ---- rendering ----

    /// <summary>
    /// The round screen: the round's facts, then §10's slate.
    ///
    /// <para><b>Three ruled things collide on this screen and something had to become its own page.</b>
    /// §10 puts three rows on every matchup (18 for a six-match card), §9.2 puts one row on every leg
    /// (up to 15 for three four-leg tickets), and §3 rules the page at 24 rows. 3 + 1 + 1 + 18 + 1 is
    /// already exactly 24 with no ledger at all. So the ledger — a READ-BACK, which is what a
    /// separate page is for — moved to <c>[T]</c>, and the slate paginates against whatever the
    /// round's own facts leave it, with the folio saying so (§13 gate 2's own remedy, in §5's
    /// existing vocabulary rather than new words).</para>
    ///
    /// <para>The header lines are counted rather than assumed, because four of them are conditional
    /// (scar, effect stacks, relics, consumables) and a slate page sized by a guess would be exactly
    /// the unstated claim <c>C46</c> is about.</para>
    /// </summary>
    private static void Render(Run run)
    {
        Ui.Clear();
        int rows = 0;
        Ui.WriteLine(ConsoleColor.Cyan,
            $"ROUND {run.Round}/{run.Config.Rounds}  ·  BANK {Ui.Money(run.Bank)}  ·  PAYMENT DUE {Ui.Money(run.CurrentPayment)}"
            + $"  ·  COMPS {run.Comps:0.#}  ·  SEED {run.Rng.RunSeed}");
        Ui.WriteLine(ConsoleColor.DarkGray,
            $"tickets {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}  ·  relics {run.OwnedRelics.Count}/{run.Config.RelicSlots}"
            + $"  ·  consumables {run.OwnedConsumables.Count}/{run.Config.ConsumableSlots}"
            + $"  ·  staking earns comps ({run.Config.CompsPerDollarStaked:0.##}/$)");
        rows += 2;

        // The whole ledger is public information (design/10) — show what is coming.
        var schedule = new List<string>();
        for (int r = run.Round - 1; r < run.PaymentSchedule.Count; r++)
            schedule.Add(Ui.Money(run.PaymentSchedule[r]));
        Ui.WriteLine(ConsoleColor.DarkGray, "SCHEDULE: " + string.Join(" → ", schedule));
        rows++;

        if (run.ScarStacks > 0)
        {
            Ui.WriteLine(ConsoleColor.Magenta,
                // T44: impersonal — the copy names the thing, not the reader.
                $"SCAR {run.ScarStacks:0.#}pp — the FIRST ticket this round carries it (burns on a hit)");
            rows++;
        }
        rows += GameLoop.WriteEffectStates(run); // chalk/iron/jar/system stacks (rev 5 §20)

        if (run.OwnedRelics.Count > 0)
        {
            var names = new List<string>();
            foreach (RelicDefinition r in run.OwnedRelics) names.Add(r.Name);
            Ui.WriteLine(ConsoleColor.Cyan, "RELICS: " + string.Join(", ", names));
            rows++;
        }

        if (run.OwnedConsumables.Count > 0)
        {
            var names = new List<string>();
            foreach (ConsumableDefinition c in run.OwnedConsumables) names.Add(c.Name);
            Ui.WriteLine(ConsoleColor.Cyan, "CONSUMABLES: " + string.Join(", ", names));
            rows++;
        }

        Ui.Rule();
        rows++;

        var sheets = new List<MarketSheet>();
        foreach (Matchup m in run.CurrentSlate.Matchups) sheets.Add(MarketSheet.Build(m));

        // What the slate has left: the page, less the facts above, less its own folio line, less the
        // blank that separates it from the command bar.
        RenderSlate(run, sheets, Page.Height - rows - 2);

        Ui.Line();
    }

    /// <summary>
    /// §10's slate. <b>Three offers per matchup with the DRAW in the middle</b> (<c>S74</c>,
    /// transferred without amendment): the draw's position is meaning, not convention, because it is
    /// the outcome where neither wins. It is named <c>DRAW</c>, never as a team, and it carries no
    /// team's treatment — the two club rows note their venue and record, the draw row notes nothing.
    /// <c>1X2</c> never reaches the player, so nothing here prints that string.
    ///
    /// <para><b>Why three rows and not one</b> (§10.2, the lane's layout to propose). The old row
    /// padded each side to 28 columns while the enumerated pool constructs
    /// <c>San Francisco Spreadsheets (8-0)</c> at 32 — over by four on each side, an 87-column row
    /// that passed 80 as well as 62. Derived instead of authored, a one-row form needs
    /// <c>2W + 23 ≤ 80</c> for TWO prices, i.e. <c>W ≤ 28</c>: the old pad was not merely wrong, it
    /// was the widest a one-row slate could ever be, and the pool needs 32. Adding the draw — a
    /// name and a third price — makes it worse, not better: the pool's two sides alone eat 64 of the
    /// page's 80 columns. <b>So the one-row form cannot carry §10.1's three offers at any pad, and
    /// that is arithmetic rather than taste.</b> <c>S74</c>'s three-row form is what the page admits,
    /// and at 61 columns of name field it seats the worst constructible side with 28 dots to spare.</para>
    ///
    /// <para>The names come from <c>MarketSheet</c> (§6.6) rather than from <c>m.Home.Name</c>, so
    /// the slate, the sheet and the ledger cannot print three different names for one market — which
    /// is <c>MarketLabel</c>'s own stated purpose, unmet until this build.</para>
    /// </summary>
    private static void RenderSlate(Run run, IReadOnlyList<MarketSheet> sheets, int budget)
    {
        int total = run.CurrentSlate.Matchups.Count;
        int perPage = Math.Max(1, budget / SlateRowsPerMatch);
        int pages = (total + perPage - 1) / perPage;
        _slatePage = Math.Clamp(_slatePage, 0, pages - 1);
        int from = _slatePage * perPage;
        int take = Math.Min(perPage, total - from);

        var hints = new List<string>();
        if (pages > 1 && _slatePage > 0) hints.Add("[P]rev");
        if (pages > 1 && _slatePage + 1 < pages) hints.Add("[N]ext");
        hints.Add("[M n] opens a match");

        // The folio rides the slate's own title line rather than taking a row of its own — which is
        // the row that keeps a six-match card whole on the base page.
        Ui.WriteLine(ConsoleColor.White, Page.Leadered(Page.LeftPad,
            $"SLATE   MATCHES {from + 1}{MarketSheet.EnDash}{from + take} of {total}",
            string.Join("  ·  ", hints)));

        var moneyline = new List<MarketOffer>();
        foreach (MarketSheet sheet in sheets)
        {
            foreach (MarketSheetRow row in MoneylineRows(sheet)) moneyline.Add(row.Offer);
        }
        RowGeometry geo = RowGeometry.ForOffers(total, moneyline);

        for (int i = from; i < from + take; i++)
        {
            Matchup m = run.CurrentSlate.Matchups[i];
            List<MarketSheetRow> block = SlateOrder(sheets[i]);
            for (int r = 0; r < block.Count; r++)
            {
                // The address column carries the MATCHUP number, and only on the block's first row:
                // one number, one referent, and it is the referent `M n` takes.
                Ui.WriteLine(ConsoleColor.Gray, geo.OfferRow(
                    r == 0 ? (m.Index + 1).ToString(CultureInfo.InvariantCulture) : string.Empty,
                    block[r].Name, SlateNote(m, block[r]),
                    block[r].Offer.Odds, block[r].Offer.TrueProb));
            }
        }
    }

    /// <summary><c>S74</c>'s form is three offers, so a matchup's block is three rows. Named so the
    /// slate's page arithmetic reads off the form rather than off a literal.</summary>
    private const int SlateRowsPerMatch = 3;

    /// <summary>Which page of the slate the round screen is showing. Reset on entry so a new round
    /// never opens mid-card.</summary>
    private static int _slatePage;

    /// <summary>The matchup's three moneyline rows, in the sheet's own order (HOME, DRAW, AWAY).</summary>
    private static List<MarketSheetRow> MoneylineRows(MarketSheet sheet)
    {
        var rows = new List<MarketSheetRow>();
        foreach (MarketSheetRow row in sheet.Section(MarketDestination.Result).Rows)
        {
            if (row.Kind == MarketKind.Moneyline) rows.Add(row);
        }
        return rows;
    }

    /// <summary>
    /// The slate's reading order: <b>AWAY, DRAW, HOME</b>. The draw is in the middle either way, so
    /// <c>S74</c>'s invariant holds; away-first is chosen because it is the order this surface has
    /// always read a fixture in (<c>away @ home</c>, and the match header still says so). Selected by
    /// <see cref="MarketChoice"/> rather than by position, so a future reordering inside
    /// <c>MatchModel</c> cannot silently move the draw off the middle.
    /// </summary>
    private static List<MarketSheetRow> SlateOrder(MarketSheet sheet)
    {
        var byChoice = new Dictionary<MarketChoice, MarketSheetRow>();
        foreach (MarketSheetRow row in MoneylineRows(sheet)) byChoice[row.Offer.Selection.Choice] = row;
        var ordered = new List<MarketSheetRow>();
        foreach (MarketChoice choice in new[] { MarketChoice.Away, MarketChoice.Draw, MarketChoice.Home })
        {
            if (byChoice.TryGetValue(choice, out MarketSheetRow? row)) ordered.Add(row);
        }
        return ordered;
    }

    /// <summary>The venue and record that ride after a club's name — and the empty string for the
    /// draw, which is <c>S74</c>'s "never with a team's treatment" made structural.</summary>
    private static string SlateNote(Matchup m, MarketSheetRow row) => row.Offer.Selection.Choice switch
    {
        MarketChoice.Away => $"AWAY ({m.Away.Record})",
        MarketChoice.Home => $"HOME ({m.Home.Record})",
        _ => string.Empty,
    };

    /// <summary>
    /// §9.2's ledger. <b>A ticket is not a line — one leg per row</b> (<c>S92</c> applied to the
    /// read-back). The comma-joined run-on this replaced measured 166 columns on a mid-length seed
    /// and 256 at the widest constructible, against an 80-column page.
    /// </summary>
    private static void ShowTickets(Run run)
    {
        int page = 0;
        while (true)
        {
            List<List<int>> pages = TicketPages(run);
            page = Math.Clamp(page, 0, pages.Count - 1);
            RenderTickets(run, pages, page);

            string command = Ui.Prompt("> ").Trim().ToUpperInvariant();
            if (Ui.Eof) return;
            if (command == "N" && page + 1 < pages.Count) { page++; continue; }
            if (command == "P" && page > 0) { page--; continue; }
            return;
        }
    }

    /// <summary>Which tickets sit on which page, split on WHOLE tickets: a ticket is a block, and a
    /// block broken across a page fold would put a stake on one page and its legs on another.</summary>
    private static List<List<int>> TicketPages(Run run)
    {
        var pages = new List<List<int>>();
        var current = new List<int>();
        int used = 0;
        for (int i = 0; i < run.Tickets.Count; i++)
        {
            int cost = 1 + run.Tickets[i].Legs.Count;
            if (current.Count > 0 && used + cost > Page.BodyRows)
            {
                pages.Add(current);
                current = new List<int>();
                used = 0;
            }
            current.Add(i);
            used += cost;
        }
        pages.Add(current);
        return pages;
    }

    private static void RenderTickets(Run run, List<List<int>> pages, int page)
    {
        Ui.Clear();
        List<int> shown = pages[page];

        var hints = new List<string>();
        if (page > 0) hints.Add("[P]rev");
        if (page + 1 < pages.Count) hints.Add("[N]ext");
        hints.Add("[X] back");

        Ui.WriteLine(ConsoleColor.Cyan, Page.Leadered(Page.LeftPad,
            run.Tickets.Count == 0
                ? "TICKETS   none placed this round"
                : $"TICKETS   {shown[0] + 1}{MarketSheet.EnDash}{shown[shown.Count - 1] + 1} of {run.Tickets.Count}",
            string.Join("  ·  ", hints)));
        Ui.Rule();

        var odds = new List<MarketOffer>();
        foreach (Ticket t in run.Tickets)
        {
            foreach (Leg leg in t.Legs)
                odds.Add(new MarketOffer(leg.Selection, leg.TrueProb, leg.OfferedOdds));
        }
        RowGeometry geo = RowGeometry.ForOffers(Math.Max(1, run.Tickets.Count), odds);

        foreach (int i in shown)
        {
            Ticket t = run.Tickets[i];
            string tag = t.Modifier == TicketModifier.FreeBet ? "   FREE BET"
                : t.Modifier == TicketModifier.DoubleOrNothing ? "   DOUBLE OR NOTHING" : string.Empty;
            Ui.WriteLine(ConsoleColor.White, Page.Leadered(Page.LeftPad,
                $"{i + 1}   {Ui.Money(t.Stake)} → {Ui.Money(t.PotentialPayout)}{tag}",
                LegCount(t.Legs.Count)));
            foreach (Leg leg in t.Legs)
            {
                bool boosted = Math.Abs(leg.OfferedOdds - leg.BaseOdds) > 1e-9;
                Ui.WriteLine(ConsoleColor.Gray, geo.OfferRow(
                    string.Empty, LegName(leg), boosted ? "BOOSTED" : string.Empty,
                    leg.OfferedOdds, leg.TrueProb));
            }
        }
    }

    private static string LegCount(int legs)
        => legs == 1 ? "1 LEG" : $"{legs} LEGS";

    /// <summary>
    /// A leg's printed name, read off <c>MarketSheet</c> (§6.6) so the ledger, the sheet and the
    /// slate are one composer. The row is found by <see cref="MarketSelection"/> value equality —
    /// the selection is a <c>readonly struct : IEquatable</c>, and the sheet's row set is exactly
    /// <c>matchup.Markets</c>, so a placed leg always has a row.
    /// </summary>
    private static string LegName(Leg leg)
    {
        foreach (MarketSheetRow row in MarketSheet.Build(leg.Matchup).AllRows)
        {
            if (row.Offer.Selection.Equals(leg.Selection)) return row.Name;
        }
        // Unreachable while a leg can only be placed against a listed offer. Stated rather than
        // swallowed: an empty row name would be a silent hole in the ledger.
        throw new InvalidOperationException(
            $"BettingScreen.LegName: {leg.Selection.Kind} is not on its matchup's sheet — a placed "
            + "leg must name a printed offer (spec §13 gate 4)");
    }

    // ---- M n: the market sheet (§4, §5) ----

    /// <summary>
    /// <c>M n</c> opens the CONTENTS page, not the sheet (§5, <c>S90</c> transferred whole);
    /// <c>M n GOALS</c> opens a destination page (§4). Everything below is a read off
    /// <c>MarketSheet</c> — the destinations, their order (<c>S95</c>), the grouping, the
    /// matchup-global line numbers that are §5.1's addresses, the counts and the folio are all
    /// already built and already gated by the laptop's own suite. Nothing here re-derives them.
    /// </summary>
    private static void ShowDetail(Run run, string argument)
    {
        argument = (argument ?? string.Empty).Trim();
        int split = argument.IndexOf(' ');
        string numberPart = split < 0 ? argument : argument.Substring(0, split);
        string destinationPart = split < 0 ? string.Empty : argument.Substring(split + 1).Trim();

        if (!int.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
            || n < 1 || n > run.CurrentSlate.Matchups.Count)
        {
            Ui.WriteLine(ConsoleColor.Red, $"Matchup must be 1–{run.CurrentSlate.Matchups.Count}.");
            Ui.Pause();
            return;
        }

        Matchup m = run.CurrentSlate.Matchups[n - 1];
        MarketSheet sheet = MarketSheet.Build(m);
        RowGeometry geo = RowGeometry.For(sheet);

        MarketDestination? open = null;
        int page = 0;
        if (destinationPart.Length > 0 && TryResolve(destinationPart, out MarketDestination direct))
            open = direct;

        while (true)
        {
            if (open == null) RenderContents(n, m, sheet);
            else RenderDestination(n, m, sheet, geo, open.Value, page);

            string typed = Ui.Prompt("> ").Trim();
            if (Ui.Eof) return;
            string command = typed.ToUpperInvariant();

            if (command.Length == 0 || command == "X")
            {
                if (open == null) return;
                open = null;
                page = 0;
                continue;
            }

            if (open != null && command == "N")
            {
                if (page + 1 < PageCount(sheet.Section(open.Value))) page++;
                continue;
            }

            if (open != null && command == "P")
            {
                if (page > 0) page--;
                continue;
            }

            if (TryResolve(typed, out MarketDestination chosen))
            {
                open = chosen;
                page = 0;
                continue;
            }

            // A navigation refusal, shown at the prompt and before anything happens. It names the
            // whole destination set rather than the fragment that failed, because the set is a
            // constant (§4.1) and naming it is the remedy.
            Ui.WriteLine(ConsoleColor.Red, Page.Fit($"No destination '{typed}'."));
            Ui.WriteLine(ConsoleColor.Red, Page.Fit(" " + DestinationList()));
            Ui.Pause();
        }
    }

    /// <summary>
    /// §5's contents page — <b>exactly 24 lines of a 24-row page</b>: the fixture header, a rule,
    /// twenty lines of destinations and their markets, a rule, and the foot. There is no slack in it
    /// and §4 says so; a seventh destination requires a DD ruling before it is proposed.
    ///
    /// <para><c>S98</c>'s long form (TEAM TOTAL GOALS / CORNERS / CARDS) and <c>S102</c>'s
    /// suppression both arrive from <c>MarketDestinations</c> and the rule below. <c>S102</c> is what
    /// makes this page fit: without it the page is 25 lines.</para>
    /// </summary>
    private static void RenderContents(int matchupNumber, Matchup m, MarketSheet sheet)
    {
        Ui.Clear();
        Ui.WriteLine(ConsoleColor.Cyan, FixtureHeader(matchupNumber, m, "CONTENTS"));
        Ui.Rule();

        foreach (MarketSheetSection section in sheet.Sections)
        {
            Ui.WriteLine(section.IsEmpty ? ConsoleColor.DarkGray : ConsoleColor.White,
                Page.Leadered(Page.LeftPad, section.Label, section.RangeText));
            foreach (MarketSheetGroup group in section.Groups)
            {
                // S102 (batch 117), read verbatim off the laptop's ContentsChildIsRedundant: a
                // destination holding exactly one market under that market's own name — CORRECT
                // SCORE today — would print its name AND its range twice, one line apart. The rule
                // fires on IDENTITY, not on childlessness: a future destination holding one market
                // under a DIFFERENT name still prints that child, because then the child is telling
                // the reader something the parent line did not.
                if (section.Label == group.Label && section.RangeText == group.RangeText) continue;

                Ui.WriteLine(group.IsEmpty ? ConsoleColor.DarkGray : ConsoleColor.Gray,
                    Page.Leadered(ContentsChildIndent, group.Label, group.RangeText));
            }
        }

        Ui.Rule();
        Ui.WriteLine(ConsoleColor.DarkGray, Page.Leadered(Page.LeftPad,
            $"{sheet.TotalRows} offers   ·   type a destination", "[X] back"));
    }

    /// <summary>The contents block's child indent — <c>S90</c>'s indented second level.</summary>
    private const int ContentsChildIndent = 5;

    /// <summary>
    /// §4's destination page: the fixture header, a rule, up to <see cref="Page.BodyRows"/> offer
    /// rows, a rule, and §5's folio at the foot.
    ///
    /// <para><b>The folio is derived, never authored</b> (<c>S74-am3</c>): its range is the printed
    /// line numbers of the rows actually on this page and its denominator is the sheet's own row
    /// count. <i>A folio that lies is worse than no folio</i>, because its whole value is that it is
    /// true inside a game about being lied to.</para>
    ///
    /// <para>Where a destination overflows the page it paginates and the folio says so —
    /// <c>PLAYERS   66–83 of 84   [N]ext</c>. The folio already carries the range, so a next page is
    /// the folio's own next page and needs no new vocabulary (<c>S25-am</c>, transferred).</para>
    /// </summary>
    private static void RenderDestination(int matchupNumber, Matchup m, MarketSheet sheet,
        RowGeometry geo, MarketDestination destination, int page)
    {
        MarketSheetSection section = sheet.Section(destination);
        Ui.Clear();
        Ui.WriteLine(ConsoleColor.Cyan, FixtureHeader(matchupNumber, m, section.Label));
        Ui.Rule();

        int pages = PageCount(section);
        int from = page * Page.BodyRows;
        int take = Math.Min(Page.BodyRows, Math.Max(0, section.Count - from));

        if (section.IsEmpty)
        {
            // §4.1 / S89: an empty group still prints. A racecard prints the race even when it is
            // abandoned, and it is what makes the destination set a constant that never reflows.
            Ui.WriteLine(ConsoleColor.DarkGray,
                Page.Leadered(Page.LeftPad, section.Label, MarketSheet.NoPricesOffered));
        }
        else
        {
            for (int i = from; i < from + take; i++)
            {
                MarketSheetRow row = section.Rows[i];
                Ui.WriteLine(ConsoleColor.Gray, geo.OfferRow(
                    row.Line.ToString(CultureInfo.InvariantCulture),
                    row.Name,
                    // §6.7: the role prints as a WORD. S22 struck the bracketed [FW]/[MF]/[DF] tag on
                    // 2026-07-31; MarketSheetRow.Role carries FORWARD/MIDFIELDER/DEFENDER straight
                    // from MatchModel's own RoleWord, and it is empty for every non-scorer market.
                    row.Role,
                    row.Offer.Odds, row.Offer.TrueProb));
            }
        }

        Ui.Rule();
        Ui.WriteLine(ConsoleColor.DarkGray,
            Page.Leadered(Page.LeftPad, FolioText(sheet, section, from, take), FootHints(page, pages)));
    }

    /// <summary>§5's folio, read off the rendered list. An empty destination has no window to fold
    /// into <c>MarketSheet.Folio</c> — which throws rather than print an impossible claim — so it
    /// states the count it does have against the same denominator.</summary>
    private static string FolioText(MarketSheet sheet, MarketSheetSection section, int from, int take)
        => take <= 0
            ? $"{section.Label}   0 of {sheet.TotalRows}"
            : $"{section.Label}   {sheet.Folio(section.Rows[from].Line, section.Rows[from + take - 1].Line)}";

    private static string FootHints(int page, int pages)
    {
        var parts = new List<string>();
        if (page > 0) parts.Add("[P]rev");
        if (page + 1 < pages) parts.Add("[N]ext");
        parts.Add("[X] back");
        return string.Join("  ·  ", parts);
    }

    private static int PageCount(MarketSheetSection section)
        => section.Count <= Page.BodyRows ? 1 : (section.Count + Page.BodyRows - 1) / Page.BodyRows;

    /// <summary>
    /// The page's own name, leadered like everything else on it. The right-hand fact is the page —
    /// <c>CONTENTS</c> or the destination — and it is bounded by the longest destination label, so
    /// the pool's two widest clubs still leave a leader run. The records and the public stat line the
    /// old detail screen carried do NOT fit a contents page ruled at exactly 24 lines; they are on
    /// the slate and in no other place, and that is a stated loss rather than a silent one.
    /// </summary>
    private static string FixtureHeader(int matchupNumber, Matchup m, string page)
        => Page.Leadered(Page.LeftPad,
            $"MATCH {matchupNumber.ToString(CultureInfo.InvariantCulture)}   "
            + $"{m.Away.Name.ToUpperInvariant()} @ {m.Home.Name.ToUpperInvariant()}", page);

    private static string DestinationList()
    {
        var labels = new List<string>();
        foreach (MarketDestination d in MarketDestinations.All) labels.Add(MarketDestinations.Label(d));
        return string.Join("  ", labels);
    }

    /// <summary>Resolves typed text onto a destination: the printed label, or an unambiguous prefix
    /// of one. Ambiguity resolves to nothing and is refused by the caller rather than guessed —
    /// <c>CO</c> names both CORNERS and CORRECT SCORE.</summary>
    private static bool TryResolve(string typed, out MarketDestination destination)
    {
        destination = default;
        string want = typed.Trim().ToUpperInvariant();
        if (want.Length == 0) return false;

        int hits = 0;
        foreach (MarketDestination d in MarketDestinations.All)
        {
            string label = MarketDestinations.Label(d);
            if (label == want) { destination = d; return true; }
            if (label.StartsWith(want, StringComparison.Ordinal)) { destination = d; hits++; }
        }
        return hits == 1;
    }

    // ---- B: build a ticket ----

    private static void Build(Run run)
    {
        string picksLine = Ui.Prompt("picks> (e.g. 1H 3GO2.5 5CO9.5 2Y 1S3)  ");
        List<Pick> picks;
        try
        {
            picks = ParsePicks(picksLine, run.CurrentSlate.Matchups);
        }
        catch (Exception ex)
        {
            Ui.WriteLine(ConsoleColor.Red, ex.Message);
            Ui.Pause();
            return;
        }

        if (picks.Count == 0)
        {
            Ui.WriteLine(ConsoleColor.Red, "No picks entered.");
            Ui.Pause();
            return;
        }

        string stakeLine = Ui.Prompt("stake> ");
        if (!double.TryParse(stakeLine, NumberStyles.Any, CultureInfo.InvariantCulture, out double stake))
        {
            Ui.WriteLine(ConsoleColor.Red, "Stake must be a number.");
            Ui.Pause();
            return;
        }

        // A held Profit Boost is played at the betslip (design/10 D): pick the leg it lands on.
        int boostLeg = -1;
        if (run.OwnsConsumable("profit_boost") && picks.Count > 0)
        {
            string b = Ui.Prompt($"PROFIT BOOST held — boost which leg? (1-{picks.Count}, enter to skip) ");
            if (int.TryParse(b, NumberStyles.Integer, CultureInfo.InvariantCulture, out int bl)
                && bl >= 1 && bl <= picks.Count)
            {
                boostLeg = bl - 1;
            }
        }

        // Locked contract modifiers (one per ticket — the one-modifier law).
        TicketModifier modifier = TicketModifier.None;
        bool hasFree = run.OwnsConsumable("free_bet");
        bool hasDon = run.OwnsConsumable("double_or_nothing");
        if (hasFree || hasDon)
        {
            string opts = hasFree && hasDon ? "[F]ree bet / [D]ouble-or-nothing"
                : hasFree ? "[F]ree bet" : "[D]ouble-or-nothing";
            string m = Ui.Prompt($"modifier? {opts} (enter to skip) ").ToUpperInvariant();
            if (m == "F" && hasFree) modifier = TicketModifier.FreeBet;
            else if (m == "D" && hasDon) modifier = TicketModifier.DoubleOrNothing;
        }

        try
        {
            Ticket t = run.PlaceTicket(picks, stake, boostLeg, modifier);
            string tag = t.Modifier == TicketModifier.FreeBet ? "  [FREE BET]"
                : t.Modifier == TicketModifier.DoubleOrNothing ? "  [DOUBLE OR NOTHING — no cash-out]" : "";
            // §9.1 (S77's form): the stamp states the ACT and its ARITY; the ledger holds the legs.
            // This printed the same string the TICKETS block prints, one screen apart — T69/T70, do
            // not reprint the subject — and at four legs it measured 166 columns against an 80-column
            // page. The legs are named once, below, by §9.2's ledger block.
            Ui.WriteLine(ConsoleColor.Green, Page.Fit(
                $"TICKET PLACED — {LegCount(t.Legs.Count)} · {Ui.Money(t.Stake)} → {Ui.Money(t.PotentialPayout)}{tag}"));
        }
        catch (Exception ex)
        {
            Ui.WriteLine(ConsoleColor.Red, ex.Message); // engine validation, verbatim
        }

        Ui.Pause();
    }

    // ---- parsing ----

    private static List<Pick> ParsePicks(string line, IReadOnlyList<Matchup> matchups)
    {
        var picks = new List<Pick>();
        foreach (string tok in line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            picks.Add(ParseOne(tok, matchups));
        }
        return picks;
    }

    private static Pick ParseOne(string token, IReadOnlyList<Matchup> matchups)
    {
        token = token.Trim();
        if (token.Length < 2)
            throw new ArgumentException($"Bad pick '{token}' — use 1H, 1GO2.5, 1CO9.5, 1KO4.5, 1Y, or 1S3.");

        int marker = 0;
        while (marker < token.Length && char.IsDigit(token[marker])) marker++;
        if (!int.TryParse(token.Substring(0, marker), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
            throw new ArgumentException($"Bad matchup number in '{token}'.");

        int idx = n - 1;
        if (idx < 0 || idx >= matchups.Count)
            throw new ArgumentException($"Matchup {n} is off the slate (pick 1–{matchups.Count}).");
        string code = token.Substring(marker).ToUpperInvariant();
        Matchup matchup = matchups[idx];
        if (code == "H" || code == "A")
            return new Pick(idx, MarketSelection.Moneyline(code == "H" ? Side.Home : Side.Away));
        if (code == "Y" || code == "N")
            return new Pick(idx, MarketSelection.BothTeamsToScore(code == "Y"));
        if (code.StartsWith("S", StringComparison.Ordinal)
            && int.TryParse(code.Substring(1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int scorerNumber)
            && scorerNumber >= 1)
        {
            MarketSelection scorer = MarketSelection.AnytimeScorer(scorerNumber - 1);
            matchup.Odds(scorer); // validates the listed index against the locked board
            return new Pick(idx, scorer);
        }
        if (code.Length < 3)
            throw new ArgumentException($"Bad market in '{token}'. Use GO/GU, CO/CU, KO/KU, Y/N, or S#.");
        string prefix = code.Substring(0, 2);
        if (!double.TryParse(code.Substring(2), NumberStyles.Float, CultureInfo.InvariantCulture, out double line))
            throw new ArgumentException($"Bad line in '{token}'.");
        if (prefix[1] != 'O' && prefix[1] != 'U')
            throw new ArgumentException($"Bad market in '{token}'. Use GO/GU, CO/CU, KO/KU, Y/N, or S#.");
        bool over = prefix[1] == 'O';
        MarketSelection selection = prefix[0] == 'G'
            ? MarketSelection.TotalGoals(line, over)
            : prefix[0] == 'C'
                ? MarketSelection.TotalCorners(line, over)
                : prefix[0] == 'K'
                    ? MarketSelection.TotalCards(line, over)
                    : throw new ArgumentException($"Bad market in '{token}'. Use GO/GU, CO/CU, KO/KU, Y/N, or S#.");
        // This also rejects syntactically valid but unavailable ladder lines.
        matchup.Odds(selection);
        return new Pick(idx, selection);
    }

    // ---- helpers ----

    private static string Short(string teamName)
    {
        int i = teamName.LastIndexOf(' ');
        return i >= 0 ? teamName.Substring(i + 1) : teamName;
    }
}
