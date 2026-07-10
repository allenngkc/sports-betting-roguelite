using System;
using System.Collections.Generic;
using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// Orchestration: one full interactive run (betting → sweat → settle → shop → …), the shop screen,
/// the run-over screen, and the non-interactive --auto smoke harness. The engine owns all state; this
/// just drives its transitions and renders around them.
/// </summary>
internal static class GameLoop
{
    /// <summary>Plays a run to its end. Returns true if the player wants to play again.</summary>
    public static bool Play(Run run)
    {
        while (true)
        {
            if (BettingScreen.Run(run) == BetAction.Quit)
                return false;

            SweatRenderer.Play(run); // sweats, finishes, settles (→ Shop / RunWon / RunLost)

            switch (run.Phase)
            {
                case Phase.Shop:
                    ShopScreen(run); // buy loop, then ExitShop → next round's Betting
                    break;

                case Phase.RunWon:
                case Phase.RunLost:
                    return RunOverScreen(run);
            }
        }
    }

    // ---- shop ----

    private static void ShopScreen(Run run)
    {
        while (true)
        {
            Ui.Clear();
            Ui.Rule();
            Ui.WriteLine(ConsoleColor.White, $"SHOP — round {run.Round} cleared, gear up");
            Ui.Rule();

            string piggy = Owns(run, "piggy_bank") ? $"   ·   PIGGY {Ui.Money(run.PiggyBankBalance)}" : "";
            Ui.WriteLine(ConsoleColor.DarkGray,
                $"BANK {Ui.Money(run.Bank)}   ·   SLOTS {run.OwnedRelics.Count}/{run.Config.RelicSlots}{piggy}");
            Ui.Line();

            for (int i = 0; i < run.ShopOffers.Count; i++)
            {
                RelicDefinition o = run.ShopOffers[i];
                Ui.Write(ConsoleColor.White, $" {i + 1}. {o.Name}  ");
                Ui.Write(ConsoleColor.Yellow, Ui.Money(o.Price));
                Ui.WriteLine(ConsoleColor.DarkGray, $"   [{o.Axis}]");
                Ui.WriteLine(ConsoleColor.Gray, $"     {o.Description}");
            }

            Ui.Line();
            string cmd = Ui.Prompt($"buy 1–{run.ShopOffers.Count} or [X] leave: ").ToUpperInvariant();
            if (cmd == "X" || Ui.Eof)
            {
                run.ExitShop();
                return;
            }

            if (int.TryParse(cmd, out int n) && n >= 1 && n <= run.ShopOffers.Count)
            {
                try { run.BuyRelic(n - 1); }
                catch (Exception ex) { Ui.WriteLine(ConsoleColor.Red, ex.Message); Ui.Pause(); }
            }
            else
            {
                Ui.WriteLine(ConsoleColor.Red, "Pick an offer number, or X to leave.");
                Ui.Pause();
            }
        }
    }

    // ---- run over ----

    private static bool RunOverScreen(Run run)
    {
        Ui.Clear();
        Ui.Rule();
        if (run.Phase == Phase.RunWon)
            Ui.WriteLine(ConsoleColor.Green, $"YOU WON — all {run.Config.Rounds} rounds cleared. The house blinks first.");
        else if (run.Debt > 0)
            Ui.WriteLine(ConsoleColor.Red,
                $"THE BOOKIE COLLECTS — round {run.Round}, {Ui.Money(run.Requirement)} due ({Ui.Money(run.Debt)} of it his), and you couldn't pay.");
        else
            Ui.WriteLine(ConsoleColor.Red, $"BUSTED — out in round {run.Round}, short of {Ui.Money(run.CurrentTarget)}.");
        Ui.Rule();

        Ui.WriteLine(ConsoleColor.White, $"Final bank: {Ui.Money(run.Bank)}");
        Ui.WriteLine(ConsoleColor.DarkGray, $"Seed: {run.Rng.RunSeed}");
        Ui.Line();
        return Ui.Confirm("play again? y/n: ");
    }

    // ---- --auto smoke harness (no sleeps, no key reads, no prompts) ----

    public static int AutoRun(string seed)
    {
        var run = new Run(seed);
        bool won = false;
        int reached = run.Round;

        while (true)
        {
            reached = run.Round;

            // Fixed policy: one 2-leg parlay on matchups 0 and 1, both Home, sized to bankroll.
            double stake = Math.Max(10.0, Math.Min(50.0, Math.Floor(0.25 * run.Bank)));
            try
            {
                run.PlaceTicket(new List<Pick> { new Pick(0, Side.Home), new Pick(1, Side.Home) }, stake);
            }
            catch
            {
                // Bank too small for the policy stake: skip the bet, still play the round out.
            }

            run.LockRound();
            run.FastForwardRound(); // never cash out
            double debtBefore = run.Debt;
            run.Settle();

            bool survived = run.Phase != Phase.RunLost;
            string debt = run.Debt > 0 ? $" debt {(long)Math.Round(run.Debt)}" : "";
            Console.WriteLine($"R{reached} bank {(long)Math.Round(run.Bank)} target {(long)Math.Round(run.CurrentTarget)}{debt} {(survived ? "W" : "L")}");
            if (survived && debtBefore == 0 && run.Debt > 0)
                Console.WriteLine($"R{reached} THE BOOKIE FLOATS YOU — owing {(long)Math.Round(run.Debt)}");
            else if (survived && debtBefore > 0 && run.Debt == 0)
                Console.WriteLine($"R{reached} DEBT CLEARED -{(long)Math.Round(debtBefore)}");

            if (run.Phase == Phase.RunLost) { won = false; break; }
            if (run.Phase == Phase.RunWon) { won = true; break; }

            // Phase.Shop: buy offer 0 if affordable and a slot remains.
            if (run.ShopOffers.Count > 0
                && run.OwnedRelics.Count < run.Config.RelicSlots
                && run.ShopOffers[0].Price <= run.Bank)
            {
                try { run.BuyRelic(0); } catch { /* ignore, keep the run moving */ }
            }
            run.ExitShop();
        }

        string how = won ? "WON" : run.Debt > 0 ? "LOST (the bookie collects)" : "LOST";
        Console.WriteLine($"AUTO RESULT: {how}, round {reached}, final bank {Ui.Money(run.Bank)}, seed {seed}");
        return 0;
    }

    private static bool Owns(Run run, string id)
    {
        foreach (RelicDefinition r in run.OwnedRelics)
            if (r.Id == id) return true;
        return false;
    }
}
