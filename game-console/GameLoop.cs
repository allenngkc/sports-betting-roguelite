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
            Ui.WriteLine(ConsoleColor.White, $"SHOP — round {run.Round} paid, gear up");
            Ui.Rule();

            Ui.WriteLine(ConsoleColor.DarkGray,
                $"BANK {Ui.Money(run.Bank)}   ·   COMPS {run.Comps:0.#}   ·   relic slots {run.OwnedRelics.Count}/{run.Config.RelicSlots}"
                + $"   ·   consumable slots {run.OwnedConsumables.Count}/{run.Config.ConsumableSlots}");
            Ui.WriteLine(ConsoleColor.DarkGray,
                $"next payment {(run.NextPayment.HasValue ? Ui.Money(run.NextPayment.Value) : "—")}   ·   comps buy items; cash pays the bookie");
            Ui.Line();

            int k = run.ShopOffers.Count;
            for (int i = 0; i < k; i++)
            {
                RelicDefinition o = run.ShopOffers[i];
                Ui.Write(ConsoleColor.White, $" {i + 1}. {o.Name}  ");
                Ui.Write(ConsoleColor.Yellow, $"{o.Price:0.#} comps");
                Ui.WriteLine(ConsoleColor.DarkGray, $"   [{o.Axis}]");
                Ui.WriteLine(ConsoleColor.Gray, $"     {o.Description}");
            }
            for (int i = 0; i < run.ConsumableOffers.Count; i++)
            {
                ConsumableDefinition o = run.ConsumableOffers[i];
                Ui.Write(ConsoleColor.White, $" {k + i + 1}. {o.Name}  ");
                Ui.Write(ConsoleColor.Yellow, $"{o.Price:0.#} comps");
                Ui.WriteLine(ConsoleColor.DarkGray, "   [consumable]");
                Ui.WriteLine(ConsoleColor.Gray, $"     {o.Description}");
            }

            Ui.Line();
            int total = k + run.ConsumableOffers.Count;
            string cmd = Ui.Prompt($"buy 1–{total}, [S]ell, or [X] leave: ").ToUpperInvariant();
            if (cmd == "X" || Ui.Eof)
            {
                run.ExitShop();
                if (run.LastGift != null)
                {
                    Ui.WriteLine(ConsoleColor.Cyan,
                        $" the bookie texts: \"rough stretch. house sends its regards.\" — FREE {run.LastGift.Name}");
                    Ui.Pause();
                }
                return;
            }

            if (cmd == "S")
            {
                SellScreen(run);
                continue;
            }

            if (int.TryParse(cmd, out int n) && n >= 1 && n <= total)
            {
                try
                {
                    if (n <= k) run.BuyRelic(n - 1);
                    else run.BuyConsumable(n - 1 - k);
                }
                catch (Exception ex) { Ui.WriteLine(ConsoleColor.Red, ex.Message); Ui.Pause(); }
            }
            else
            {
                Ui.WriteLine(ConsoleColor.Red, "Pick an offer number, S to sell, or X to leave.");
                Ui.Pause();
            }
        }
    }

    private static void SellScreen(Run run)
    {
        if (run.OwnedRelics.Count == 0 && run.OwnedConsumables.Count == 0)
        {
            Ui.WriteLine(ConsoleColor.Red, "Nothing to sell.");
            Ui.Pause();
            return;
        }

        Ui.Line();
        int k = run.OwnedRelics.Count;
        for (int i = 0; i < k; i++)
            Ui.WriteLine(ConsoleColor.Gray,
                $" {i + 1}. {run.OwnedRelics[i].Name}  (sells for {run.OwnedRelics[i].Price * run.Config.SellBackFraction:0.#} comps)");
        for (int i = 0; i < run.OwnedConsumables.Count; i++)
            Ui.WriteLine(ConsoleColor.Gray,
                $" {k + i + 1}. {run.OwnedConsumables[i].Name}  (sells for {run.OwnedConsumables[i].Price * run.Config.SellBackFraction:0.#} comps)");

        string cmd = Ui.Prompt($"sell 1–{k + run.OwnedConsumables.Count} (enter to cancel): ");
        if (!int.TryParse(cmd, out int n) || n < 1 || n > k + run.OwnedConsumables.Count) return;

        try
        {
            if (n <= k) run.SellRelic(n - 1);
            else run.SellConsumable(n - 1 - k);
        }
        catch (Exception ex) { Ui.WriteLine(ConsoleColor.Red, ex.Message); Ui.Pause(); }
    }

    // ---- run over ----

    private static bool RunOverScreen(Run run)
    {
        Ui.Clear();
        Ui.Rule();
        if (run.Phase == Phase.RunWon)
        {
            Ui.WriteLine(ConsoleColor.Green, $"YOU WON — all {run.Config.Rounds} payments made. The house blinks first.");
        }
        else
        {
            SettlementReport r = run.LastSettlement!.Value;
            Ui.WriteLine(ConsoleColor.Red,
                $"THE BOOKIE COLLECTS — round {r.Round}: {Ui.Money(r.Payment)} due, you had {Ui.Money(r.BankBefore)} (short {Ui.Money(r.Shortfall)}).");
        }
        Ui.Rule();

        Ui.WriteLine(ConsoleColor.White, $"Final bank: {Ui.Money(run.Bank)}   ·   comps left: {run.Comps:0.#}");
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
            run.FastForwardRound(); // never cash out; pending windows auto-decline
            run.Settle();

            SettlementReport r = run.LastSettlement!.Value;
            bool survived = run.Phase != Phase.RunLost;
            Console.WriteLine($"R{reached} bank {(long)Math.Round(run.Bank)} payment {(long)r.Payment}"
                + $" comps {run.Comps:0.#} {(survived ? "W" : "L")}");
            if (r.TotemFired)
                Console.WriteLine($"R{reached} THE TOTEM BURNS — payment {(long)r.Payment} deferred, bank untouched, next payment surcharged");

            if (run.Phase == Phase.RunLost) { won = false; break; }
            if (run.Phase == Phase.RunWon) { won = true; break; }

            // Phase.Shop: buy the first affordable offer (comps) if a slot remains.
            if (run.ShopOffers.Count > 0
                && run.OwnedRelics.Count < run.Config.RelicSlots
                && run.ShopOffers[0].Price <= run.Comps)
            {
                try { run.BuyRelic(0); } catch { /* ignore, keep the run moving */ }
            }
            run.ExitShop();
        }

        string how = won ? "WON" : "LOST (the bookie collects)";
        Console.WriteLine($"AUTO RESULT: {how}, round {reached}, final bank {Ui.Money(run.Bank)}, seed {seed}");
        return 0;
    }
}
