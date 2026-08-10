using System;
using System.Collections.Generic;
using System.Globalization;
using SBR.Engine;

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

    private static string CommandBar()
        => "commands: [B]uild ticket  [M n] match detail  [K] marker  [L]ock round  [Q]uit  > ";

    // ---- rendering ----

    private static void Render(Run run)
    {
        Ui.Clear();
        Ui.WriteLine(ConsoleColor.Cyan,
            $"ROUND {run.Round}/{run.Config.Rounds}  ·  BANK {Ui.Money(run.Bank)}  ·  PAYMENT DUE {Ui.Money(run.CurrentPayment)}"
            + $"  ·  COMPS {run.Comps:0.#}  ·  SEED {run.Rng.RunSeed}");
        Ui.WriteLine(ConsoleColor.DarkGray,
            $"tickets {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}  ·  relics {run.OwnedRelics.Count}/{run.Config.RelicSlots}"
            + $"  ·  consumables {run.OwnedConsumables.Count}/{run.Config.ConsumableSlots}"
            + $"  ·  staking earns comps ({run.Config.CompsPerDollarStaked:0.##}/$)");

        // The whole ledger is public information (design/10) — show what is coming.
        var schedule = new List<string>();
        for (int r = run.Round - 1; r < run.PaymentSchedule.Count; r++)
            schedule.Add(Ui.Money(run.PaymentSchedule[r]));
        Ui.WriteLine(ConsoleColor.DarkGray, "SCHEDULE: " + string.Join(" → ", schedule));

        if (run.ScarStacks > 0)
            Ui.WriteLine(ConsoleColor.Magenta,
                // T44: impersonal — the copy names the thing, not the reader.
                $"SCAR {run.ScarStacks:0.#}pp — the FIRST ticket this round carries it (burns on a hit)");
        GameLoop.WriteEffectStates(run); // chalk/iron/jar/system stacks (rev 5 §20)

        if (run.OwnedRelics.Count > 0)
        {
            var names = new List<string>();
            foreach (RelicDefinition r in run.OwnedRelics) names.Add(r.Name);
            Ui.WriteLine(ConsoleColor.Cyan, "RELICS: " + string.Join(", ", names));
        }

        if (run.OwnedConsumables.Count > 0)
        {
            var names = new List<string>();
            foreach (ConsumableDefinition c in run.OwnedConsumables) names.Add(c.Name);
            Ui.WriteLine(ConsoleColor.Cyan, "CONSUMABLES: " + string.Join(", ", names));
        }

        Ui.Rule();
        RenderSlate(run);

        if (run.Tickets.Count > 0)
        {
            Ui.Line();
            Ui.WriteLine(ConsoleColor.White, "TICKETS");
            for (int i = 0; i < run.Tickets.Count; i++)
                Ui.WriteLine(ConsoleColor.Gray, $" {i + 1}. {DescribeLegs(run.Tickets[i])}  |  {Ui.Money(run.Tickets[i].Stake)} → {Ui.Money(run.Tickets[i].PotentialPayout)}");
        }

        Ui.Line();
    }

    private static void RenderSlate(Run run)
    {
        Ui.WriteLine(ConsoleColor.White, "SLATE  (away @ home · moneyline)");

        foreach (Matchup m in run.CurrentSlate.Matchups)
        {
            string away = $"{m.Away.Name} ({m.Away.Record})";
            string home = $"{m.Home.Name} ({m.Home.Record})";
            string line = $" {m.Index + 1}. {away.PadRight(28)} {Ui.American(m.AwayOdds),5}   @   {home.PadRight(28)} {Ui.American(m.HomeOdds),5}";
            Ui.WriteLine(ConsoleColor.Gray, line);
        }
    }

    private static void ShowDetail(Run run, string number)
    {
        if (!int.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)
            || n < 1 || n > run.CurrentSlate.Matchups.Count)
        {
            Ui.WriteLine(ConsoleColor.Red, $"Matchup must be 1–{run.CurrentSlate.Matchups.Count}.");
            Ui.Pause();
            return;
        }

        Matchup m = run.CurrentSlate.Matchups[n - 1];
        Ui.Clear();
        Ui.WriteLine(ConsoleColor.Cyan, $"MATCH {n}: {m.Away.Name} @ {m.Home.Name}");
        Ui.WriteLine(ConsoleColor.DarkGray,
            $"records  {m.Away.Record} @ {m.Home.Record}   ·   public stats (GF / COR / CRD)");
        Ui.WriteLine(ConsoleColor.Gray,
            $"away     {m.AwayStats.GoalsFor:0.0} / {m.AwayStats.Corners:0.0} / {m.AwayStats.Cards:0.0}");
        Ui.WriteLine(ConsoleColor.Gray,
            $"home     {m.HomeStats.GoalsFor:0.0} / {m.HomeStats.Corners:0.0} / {m.HomeStats.Cards:0.0}");
        Ui.Rule();
        Ui.WriteLine(ConsoleColor.White, "MARKETS  (decimal odds shown as American)");
        foreach (MarketOffer offer in m.Markets)
        {
            if (offer.Selection.Kind == MarketKind.AnytimeScorer) continue;
            Ui.WriteLine(ConsoleColor.Gray,
                $" {MarketLabel(m, offer.Selection),-32} {Ui.American(offer.Odds),5}  p {Ui.Pct(offer.TrueProb),2}%");
        }
        Ui.Line();
        Ui.WriteLine(ConsoleColor.White, "PLAYERS  (S# = anytime scorer)");
        foreach (MarketOffer offer in m.Markets)
        {
            if (offer.Selection.Kind != MarketKind.AnytimeScorer) continue;
            Player player = m.PlayerAt(offer.Selection.PlayerIndex);
            Ui.WriteLine(ConsoleColor.Gray,
                $" S{offer.Selection.PlayerIndex + 1,-2} {player.Name,-22} [{player.Role}]  {Ui.American(offer.Odds),5}");
        }
        Ui.Pause();
    }

    /// <summary>Composes a market's console label from the engine's own
    /// <see cref="MatchModel.Fields"/> (S22 ruling) instead of a hand-rolled code table, so the
    /// console and the laptop UI can never print two different names for the same market. Unlike
    /// the laptop's compact leg label — which maps onto the DS <c>MarginLeg</c>'s two-field
    /// <c>{ team, market }</c> shape and so drops the specific over/under line — the console has no
    /// other place to show that detail, so it keeps every non-empty field <c>Fields</c> exposes
    /// (Subject, Market, Line) rather than dropping any of them; for the scorer market Subject is
    /// already folded into Line ("{PLAYER} ANYTIME"), so only Line is used there to avoid repeating
    /// the player's name.</summary>
    private static string MarketLabel(Matchup matchup, MarketSelection selection)
    {
        MatchModel.MarketFields f = MatchModel.Fields(matchup, selection);
        if (f.Subject.Length > 0 && f.Line.Length > 0)
            return f.Line;
        string subject = f.Subject.Length > 0 ? f.Subject + " " : "";
        string descriptor = f.Line.Length > 0 ? $"{f.Market} {f.Line}" : f.Market;
        return subject + descriptor;
    }

    private static string DescribeLegs(Ticket t)
    {
        var parts = new List<string>();
        foreach (Leg leg in t.Legs)
        {
            string mark = Math.Abs(leg.OfferedOdds - leg.BaseOdds) > 1e-9 ? " ^boosted" : "";
            parts.Add($"{MarketLabel(leg.Matchup, leg.Selection)} {Ui.American(leg.OfferedOdds)}{mark}");
        }
        return string.Join(", ", parts);
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
            Ui.WriteLine(ConsoleColor.Green,
                $"TICKET PLACED: {DescribeLegs(t)}  |  {Ui.Money(t.Stake)} → {Ui.Money(t.PotentialPayout)}{tag}");
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
