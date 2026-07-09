using System;
using System.Collections.Generic;
using System.Threading;
using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// Plays the sweat (design/04): each ticket's session is stepped serially, one event at a time, with
/// timed pacing and live keyboard polling for cash-out [C] / fast-forward [F]. Renders the ticker,
/// the live win-prob bar + cash-out taunt, the resolution beats, and any live bank change (early
/// payout drips, insurance refunds, piggy smashes) — then finishes and settles the round.
/// </summary>
internal static class SweatRenderer
{
    private const int PollSliceMs = 50;
    private const int DeadBeatMs = 600; // the silence before a bad-beat DEAD line

    private enum Input { None, CashOut, FastForward }

    public static void Play(Run run)
    {
        Ui.Clear();
        Ui.Rule();
        Ui.WriteLine(ConsoleColor.White, $"ROUND {run.Round} — THE SWEAT");
        Ui.WriteLine(ConsoleColor.DarkGray, "[C] cash out (when offered)  ·  [F] fast-forward (never the final leg)");
        Ui.Rule();

        double lastBank = run.Bank;
        var cashOuts = new Dictionary<Ticket, double>();
        int n = run.Tickets.Count;

        for (int i = 0; i < n; i++)
        {
            Ui.Line();
            SweatOne(run, i, n, cashOuts, ref lastBank);
        }

        run.FinishSweat();
        Settlement(run, cashOuts);
        run.Settle();
        Ui.Line();
        Ui.Pause();
    }

    private static void SweatOne(Run run, int index, int total, Dictionary<Ticket, double> cashOuts, ref double lastBank)
    {
        SweatSession session = run.Sweats[index];
        Ticket ticket = run.Tickets[index];
        int lastLeg = ticket.Legs.Count - 1;

        Ui.WriteLine(ConsoleColor.White, $"TICKET {index + 1}/{total} — {Ui.Money(ticket.Stake)} to win {Ui.Money(ticket.PotentialPayout)}");

        double prevProb = 0.0;
        int legSeen = -1;
        bool fastForward = false;

        while (session.MoveNext(out DramaEvent? e))
        {
            DramaEvent evt = e!;
            Leg leg = ticket.Legs[evt.LegIndex];
            if (evt.LegIndex != legSeen)
            {
                legSeen = evt.LegIndex;
                prevProb = leg.TrueProb; // the pre-event anchor for this leg's first beat
            }

            RenderEvent(evt, leg, session, prevProb, fastForward);
            prevProb = evt.WinProbAfter;
            RenderBankDelta(run, ref lastBank);

            if (session.IsComplete) break;

            bool onFinalLeg = evt.LegIndex == lastLeg;
            int hold = fastForward && !onFinalLeg ? 0 : PacingFor(evt, onFinalLeg);
            if (fastForward && onFinalLeg) fastForward = false; // reached the final leg — it must be sweated
            if (hold == 0) continue;

            switch (Hold(hold, session, onFinalLeg))
            {
                case Input.CashOut:
                    double amt = session.CashOutOffer()!.Value;
                    session.AcceptCashOut();
                    cashOuts[ticket] = amt;
                    Ui.WriteLine(ConsoleColor.Yellow, $"  CASHED OUT: {Ui.Money(amt)}");
                    lastBank = run.Bank; // the credit is the CASHED OUT line; don't double-report it
                    return;

                case Input.FastForward:
                    fastForward = true;
                    break;
            }
        }
    }

    private static void RenderEvent(DramaEvent e, Leg leg, SweatSession session, double prevProb, bool fast)
    {
        Ui.Write(ConsoleColor.DarkGray, $"  {Clock(e),-9}");
        Ui.WriteLine(ConsoleColor.White, EventText.For(e, leg, prevProb));

        Ui.Write(ConsoleColor.DarkGray, "           ");
        Ui.Write(ConsoleColor.Gray, Ui.Bar(e.WinProbAfter, 20));
        Ui.Write(ConsoleColor.White, $" {Ui.Pct(e.WinProbAfter)}%");
        double? offer = session.CashOutOffer();
        if (offer.HasValue)
            Ui.Write(ConsoleColor.Yellow, $"   CASH OUT: {Ui.Money(offer.Value)} [C]");
        Ui.Line();

        if (e.Type == DramaEventType.LegFinal)
            RenderResolution(e, leg, session, fast);
    }

    private static void RenderResolution(DramaEvent e, Leg leg, SweatSession session, bool fast)
    {
        int k = e.LegIndex + 1;

        if (leg.IsVoided)
        {
            Ui.WriteLine(ConsoleColor.Cyan, "  MULLIGAN — leg voided, the ticket lives");
            return;
        }

        if (leg.LuckyFlippedWon)
        {
            Ui.WriteLine(ConsoleColor.Magenta, "  THE BOOK GRADES IT YOUR WAY?!");
            Ui.WriteLine(ConsoleColor.Green, $"  LEG {k}: ✔ GREEN");
            return;
        }

        if (session.RevealedLegState(e.LegIndex) == LegState.Won)
        {
            Ui.WriteLine(ConsoleColor.Green, $"  LEG {k}: ✔ GREEN");
        }
        else
        {
            if (!fast) Ui.Beat(DeadBeatMs); // restraint IS the joke on bad beats
            Ui.WriteLine(ConsoleColor.Red, $"  LEG {k}: ✘ DEAD");
        }
    }

    private static void RenderBankDelta(Run run, ref double lastBank)
    {
        double d = run.Bank - lastBank;
        if (Math.Abs(d) < 0.005) return;
        Ui.WriteLine(ConsoleColor.Yellow, $"  BANK {Ui.Signed(d)}");
        lastBank = run.Bank;
    }

    // ---- settlement ----

    private static void Settlement(Run run, Dictionary<Ticket, double> cashOuts)
    {
        Ui.Line();
        Ui.Rule();
        Ui.WriteLine(ConsoleColor.White, "SETTLE");

        for (int i = 0; i < run.Tickets.Count; i++)
        {
            Ticket t = run.Tickets[i];
            switch (t.State)
            {
                case TicketState.Won:
                    Ui.WriteLine(ConsoleColor.Green,
                        $" TICKET {i + 1}: WON  {Ui.Signed(t.PotentialPayout - t.Stake)}  (paid {Ui.Money(t.PotentialPayout)})");
                    break;

                case TicketState.CashedOut:
                    double cash = cashOuts.TryGetValue(t, out double c) ? c : 0.0;
                    Ui.WriteLine(ConsoleColor.Yellow,
                        $" TICKET {i + 1}: CASHED OUT  {Ui.Signed(cash - t.Stake)}  (took {Ui.Money(cash)})");
                    break;

                default: // Lost
                    Ui.WriteLine(ConsoleColor.Red, $" TICKET {i + 1}: DEAD  {Ui.Signed(-t.Stake)}");
                    break;
            }
        }

        bool met = run.Bank >= run.CurrentTarget;
        Ui.WriteLine(met ? ConsoleColor.Green : ConsoleColor.Red,
            $" BANK {Ui.Money(run.Bank)}  vs  TARGET {Ui.Money(run.CurrentTarget)}   {(met ? "✔ CLEARED" : "✘ SHORT")}");
        Ui.Rule();
    }

    // ---- pacing + input ----

    /// <summary>
    /// The pacing dials, one place so playtests can retune them. Base delay by tension tag, an extra
    /// beat right before a leg's final whistle, and everything slowed on the ticket's final leg.
    /// </summary>
    private static int PacingFor(DramaEvent e, bool isFinalLeg)
    {
        int ms = e.Tag switch
        {
            TensionTag.Calm => 450,
            TensionTag.Swing => 650,
            TensionTag.LeadChange => 800,
            TensionTag.NearMiss => 1000,
            TensionTag.Decisive => 1200,
            _ => 450,
        };
        if (e.Step == e.TotalSteps - 1) ms += 300; // suspense held right before the whistle
        if (isFinalLeg) ms = (int)Math.Round(ms * 1.5);
        return ms;
    }

    /// <summary>
    /// Holds for <paramref name="ms"/> in ~50ms slices, polling the keyboard. C cashes out (only when an
    /// offer is live), F fast-forwards — but F on the final leg is refused inline so the sweat continues.
    /// On a redirected stream there is no keyboard, so it just waits out the (skipped) delay.
    /// </summary>
    private static Input Hold(int ms, SweatSession session, bool onFinalLeg)
    {
        if (Console.IsInputRedirected) return Input.None;

        int elapsed = 0;
        bool refused = false;
        while (elapsed < ms)
        {
            int slice = Math.Min(PollSliceMs, ms - elapsed);
            Thread.Sleep(slice);
            elapsed += slice;

            while (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.C && session.CashOutOffer() != null)
                    return Input.CashOut;
                if (key == ConsoleKey.F)
                {
                    if (!onFinalLeg) return Input.FastForward;
                    if (!refused)
                    {
                        Ui.WriteLine(ConsoleColor.DarkGray, "  (the final leg must be sweated — no fast-forward)");
                        refused = true;
                    }
                }
                // any other key: ignored
            }
        }
        return Input.None;
    }

    /// <summary>A fake broadcast clock derived from the event's position in its leg.</summary>
    private static string Clock(DramaEvent e)
    {
        if (e.Type == DramaEventType.LegFinal) return "FINAL";
        double f = e.TotalSteps <= 1 ? 0.999 : Math.Min(0.999, (double)e.Step / e.TotalSteps);
        int quarter = Math.Min(4, (int)(f * 4) + 1);
        double within = f * 4 - Math.Floor(f * 4);      // fraction through the quarter
        int secs = (int)Math.Round((1.0 - within) * 15 * 60); // clock counts down within a 15:00 quarter
        return $"Q{quarter} {secs / 60:00}:{secs % 60:00}";
    }
}
