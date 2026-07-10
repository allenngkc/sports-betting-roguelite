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

                case "L":
                    if (run.Tickets.Count == 0 && !Ui.Confirm("no bets this round? y/n: ")) break;
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
        => "commands: [B]uild ticket  [L]ock round  [Q]uit  > ";

    // ---- rendering ----

    private static void Render(Run run)
    {
        Ui.Clear();
        Ui.WriteLine(ConsoleColor.Cyan,
            $"ROUND {run.Round}/{run.Config.Rounds}  ·  BANK {Ui.Money(run.Bank)}  ·  TARGET {Ui.Money(run.CurrentTarget)}  ·  SEED {run.Rng.RunSeed}");
        Ui.WriteLine(ConsoleColor.DarkGray,
            $"tickets {run.Tickets.Count}/{run.Config.MaxTicketsPerRound}  ·  relics {run.OwnedRelics.Count}/{run.Config.RelicSlots}");

        if (run.Debt > 0)
            Ui.WriteLine(ConsoleColor.Red,
                $"DEBT {Ui.Money(run.Debt)}  ·  the settle needs {Ui.Money(run.Requirement)} or the bookie collects");

        if (OwnsPiggy(run))
            Ui.WriteLine(ConsoleColor.Cyan, $"PIGGY {Ui.Money(run.PiggyBankBalance)}");

        if (run.OwnedRelics.Count > 0)
        {
            var names = new List<string>();
            foreach (RelicDefinition r in run.OwnedRelics) names.Add(r.Name);
            Ui.WriteLine(ConsoleColor.Cyan, "RELICS: " + string.Join(", ", names));
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

    private static string DescribeLegs(Ticket t)
    {
        var parts = new List<string>();
        foreach (Leg leg in t.Legs)
        {
            string team = Short(leg.Side == Side.Home ? leg.Matchup.Home.Name : leg.Matchup.Away.Name);
            string mark = Math.Abs(leg.OfferedOdds - leg.BaseOdds) > 1e-9 ? " ^boosted" : "";
            parts.Add($"{team} {Ui.American(leg.OfferedOdds)}{mark}");
        }
        return string.Join(", ", parts);
    }

    // ---- B: build a ticket ----

    private static void Build(Run run)
    {
        string picksLine = Ui.Prompt("picks> (e.g. 1H 3A 5H)  ");
        List<Pick> picks;
        try
        {
            picks = ParsePicks(picksLine, run.CurrentSlate.Matchups.Count);
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

        try
        {
            Ticket t = run.PlaceTicket(picks, stake);
            Ui.WriteLine(ConsoleColor.Green,
                $"TICKET PLACED: {DescribeLegs(t)}  |  {Ui.Money(t.Stake)} → {Ui.Money(t.PotentialPayout)}");
        }
        catch (Exception ex)
        {
            Ui.WriteLine(ConsoleColor.Red, ex.Message); // engine validation, verbatim
        }

        Ui.Pause();
    }

    // ---- parsing ----

    private static List<Pick> ParsePicks(string line, int matchupCount)
    {
        var picks = new List<Pick>();
        foreach (string tok in line.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries))
        {
            (int idx, Side side) = ParseOne(tok, matchupCount);
            picks.Add(new Pick(idx, side));
        }
        return picks;
    }

    private static (int idx, Side side) ParseOne(string token, int matchupCount)
    {
        token = token.Trim();
        if (token.Length < 2)
            throw new ArgumentException($"Bad pick '{token}' — use a number then H or A, e.g. 1H.");

        char sc = char.ToUpperInvariant(token[token.Length - 1]);
        Side side = sc == 'H' ? Side.Home
                  : sc == 'A' ? Side.Away
                  : throw new ArgumentException($"Bad side in '{token}' — end with H (home) or A (away).");

        string num = token.Substring(0, token.Length - 1);
        if (!int.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
            throw new ArgumentException($"Bad matchup number in '{token}'.");

        int idx = n - 1;
        if (idx < 0 || idx >= matchupCount)
            throw new ArgumentException($"Matchup {n} is off the slate (pick 1–{matchupCount}).");

        return (idx, side);
    }

    // ---- helpers ----

    private static bool OwnsPiggy(Run run) => Owns(run, "piggy_bank");

    private static bool Owns(Run run, string id)
    {
        foreach (RelicDefinition r in run.OwnedRelics)
            if (r.Id == id) return true;
        return false;
    }

    private static string Short(string teamName)
    {
        int i = teamName.LastIndexOf(' ');
        return i >= 0 ? teamName.Substring(i + 1) : teamName;
    }
}
