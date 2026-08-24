using System;
using System.Collections.Generic;
using System.Threading;
using SBR.Engine;
using SBR.Game;

namespace SBR.ConsoleGame;

/// <summary>
/// THE TEXT OF THE SWEAT — every line the player reads while a ticket runs, composed as a string
/// and nothing else. <see cref="SweatRenderer"/> decides WHEN a line is written and in what ink;
/// this decides WHAT IT SAYS.
///
/// <para><b>Why the split exists, and why this type is public in a console app:</b> §13's gates are
/// assertions about RENDERED LINES — no line over 80 columns (gate 1), no identifier, no struck tag,
/// one casing. A gate that can only reach those lines by driving <see cref="Console"/> is a gate
/// that cannot run: <c>SweatRenderer.Play</c> sleeps, polls the keyboard and ends on
/// <c>Ui.Pause()</c>. Composing the text here makes every one of those lines a pure function of a
/// <see cref="DramaEvent"/> and a <see cref="Leg"/>, so <c>SweatNamingGateTests</c> asserts the exact
/// strings the player sees. The renderer writes these same composers segment by segment, so the two
/// cannot drift.</para>
///
/// <para><b><c>K16</c> AND §9.3 — WHERE A LEG SAYS WHAT IT IS.</b> The beat line used to carry
/// <c>[TotalCorners 9.5 Over] </c> and the resolution line used to read <c>LEG 1: ✘ DEAD</c> — a raw
/// enum identifier on every beat, and a bare ordinal at the one moment the leg's state changed. Both
/// were ruled violations (DD batch 144; spec §9.3). The identity moved onto the two lines that were
/// ALREADY wholly in the display register:</para>
///
/// <list type="bullet">
/// <item><b>The meter gutter</b> — nine columns that held nothing but spaces — now carries the
/// leg's ADDRESS (<c>LEG 2</c>), on EVERY beat. This is what keeps <c>K16</c>'s stated purpose
/// (*the reader needs to know which leg is speaking*) served rather than deleted, and it costs
/// <b>zero rows and zero columns</b>: the gutter is the same nine columns the clock uses on the line
/// above. That mattered — commit 1 measured a sweat screen at 32 rows against §3's 24, and a
/// per-leg header line would have added to an overflow this commit is not permitted to fix.</item>
/// <item><b>The verdict line</b> — now <c>LEG 2: OVER 9.5 CORNERS  ·  ✘ DEAD</c>. §9.3: *a leg is
/// named when its state changes*, in <b>the same name the ledger prints</b> (<c>T69</c>/<c>T70</c>),
/// which is why <see cref="LegName(Leg)"/> reads <see cref="MarketSheet"/> and uppercases exactly
/// where <c>RowGeometry.OfferRow</c> does (<c>S96</c>, §6.5) rather than composing a second name.</item>
/// </list>
///
/// <para><b>Both lines are ALL uppercase, and that is the casing decision, not an accident.</b>
/// <c>T39</c>/<c>T98</c> is one casing per line. The market name is uppercase and a beat is sentence
/// case, so the two cannot share a line — and the name cannot be lowered to join it, because the
/// vocabulary holds an initialism (<c>BTTS — YES</c>) and proper nouns. Putting the identity on
/// lines that carry no prose at all is the only placement where every line has exactly one casing,
/// and it leaves the beat line purer than it was: authored prose, nothing else.</para>
/// </summary>
public static class SweatLines
{
    /// <summary>Whether this beat belongs to the LAST telling on the ticket — the console's
    /// "no fast-forward past here" predicate.
    ///
    /// <para>WAS <c>evt.LegIndex == ticket.Legs.Count - 1</c>, AND THAT IS WRONG UNDER <c>T140</c>
    /// ARM A. A telling is a (ticket, FIXTURE) and <c>DramaEvent.LegIndex</c> is the telling's
    /// ANCHOR — the lowest ticket-order leg on that fixture. Fixture grouping is first-appearance
    /// (<c>JointModel.GroupByMatchup</c>) and a fixture's legs need not be CONTIGUOUS, so on
    /// <c>[matchA, matchB, matchA]</c> the anchors are only ever 0 and 1 — <b>never 2</b>, the last
    /// leg index. The old predicate could not become true on that ticket at all.</para>
    ///
    /// <para>WHAT THAT COSTS IS NOT PACING, IT IS A RULE: the caller clears <c>fastForward</c> when
    /// this goes true ("reached the final leg — it must be sweated") and <see cref="Hold"/> refuses
    /// a fast-forward key on it. Never true means <b>the player can fast-forward through the final
    /// match</b> — the one thing the console states it will not allow.</para>
    ///
    /// <para>Exposed as a named predicate rather than left inline because it is asserted by a gate:
    /// <c>Hold</c> short-circuits on redirected stdin, so the fast-forward path cannot be exercised
    /// by piping input at it and the evidence has to be an assertion on this value.</para></summary>
    public static bool OnFinalFixture(DramaEvent e, SweatSession session)
        => e.FixtureIndex == session.FixtureCount - 1;

    /// <summary>The gutter's width. ONE number for both lines of a beat — the clock line's
    /// <c>{Clock,-9}</c> and the meter line's leg address are the same nine columns, which is what
    /// makes the pair read as a two-column stub (WHEN / WHICH) rather than as a ragged indent.</summary>
    public const int GutterWidth = 9;

    /// <summary>The page's left margin for sweat lines — the two spaces the beat line already had.</summary>
    public const int LeftPad = 2;

    /// <summary>Width of the win-prob bar. Was an inline <c>20</c> at its one call site; named here
    /// so <see cref="MeterLine"/> and the renderer cannot disagree about it.</summary>
    public const int BarWidth = 20;

    /// <summary>
    /// What sits between a leg's name and its verdict. <b>NOT an em dash, and the reason is a
    /// measurement:</b> <c>T39</c> is one casing AND ONE DASH per line, and the market vocabulary
    /// already spends the line's dash — <c>MatchModel.Fields</c> composes BTTS as <c>BTTS — YES</c>,
    /// so <c>LEG 1: BTTS — YES — ✘ DEAD</c> would carry two. The middle dot is this surface's own
    /// existing separator (<c>PAYMENT MADE …  ·  bank $240</c>, the sweat's command bar), so nothing
    /// is invented and the rule holds for all fifteen kinds rather than for the fourteen that happen
    /// not to contain a dash. Caught by the gate's one-dash sweep, not by reading.
    /// </summary>
    public const string Separator = "  ·  ";

    /// <summary>§9.3's verdict words. Authored here rather than at three call sites so the shape of
    /// a state change is one thing.</summary>
    public const string WonVerdict = "✔ GREEN";

    public const string LostVerdict = "✘ DEAD";

    public const string VoidVerdict = "VOID";

    /// <summary>Left margin + a nine-column gutter cell. Both lines of a beat start here.</summary>
    public static string Gutter(string cell)
        => new string(' ', LeftPad) + (cell ?? string.Empty).PadRight(GutterWidth);

    /// <summary>The leg's ADDRESS — the ledger's own 1-based ordinal, which is what <c>LEG 1</c> has
    /// always meant on this surface. §9.3 struck the ordinal as a leg's NAME; it is still the right
    /// thing to point WITH, which is why it addresses the meter line and never stands alone at a
    /// state change.</summary>
    public static string LegAddress(int legIndex) => "LEG " + (legIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>
    /// A leg's printed name, read off <see cref="MarketSheet"/> — the ONE composer this surface and
    /// the laptop both print through (§6.6, <c>K8</c>) — and uppercased at the presentation layer
    /// exactly as <c>RowGeometry.OfferRow</c> does it (<c>S96</c>, §6.5). That is what makes §9.3's
    /// *the same name the ledger prints* true by construction rather than by matching strings.
    ///
    /// <para><b>There is no fallback and that is deliberate.</b> A <c>default:</c> arm returning
    /// <c>Kind.ToString()</c> is precisely how <c>T130</c> and <c>K16</c> happened. A leg that is not
    /// on its own matchup's sheet is a bug in placement, so it throws.</para>
    ///
    /// <para>The lookup mirrors <c>BettingScreen.LegName</c>'s five lines. Both walk the same sheet
    /// and both return <c>MarketSheetRow.Name</c>, so the composer is shared and only the walk is
    /// written twice; folding the two into one helper crosses a file this commit's other half owns.</para>
    /// </summary>
    public static string LegName(Leg leg)
    {
        if (leg == null) throw new ArgumentNullException(nameof(leg));
        return LegName(leg.Matchup, leg.Selection);
    }

    /// <summary>The same name, for a selection that has not been placed as a leg — the gate reaches
    /// all fifteen kinds through this without having to seat fifteen legs on a four-leg ticket.</summary>
    public static string LegName(Matchup matchup, MarketSelection selection)
    {
        if (matchup == null) throw new ArgumentNullException(nameof(matchup));
        foreach (MarketSheetRow row in MarketSheet.Build(matchup).AllRows)
        {
            if (row.Offer.Selection.Equals(selection)) return row.Name.ToUpperInvariant();
        }
        throw new InvalidOperationException(
            $"SweatLines.LegName: {selection.Kind} is not on its matchup's sheet — a sweating leg "
            + "must name a printed offer (spec §13 gate 4). Nothing here falls back to the enum "
            + "name: that fallback IS K16/T130.");
    }

    /// <summary>A fake broadcast clock derived from the event's position in its leg.</summary>
    public static string Clock(DramaEvent e)
    {
        if (e.Type == DramaEventType.LegFinal) return "FINAL";
        double f = e.TotalSteps <= 1 ? 0.999 : Math.Min(0.999, (double)e.Step / e.TotalSteps);
        int quarter = Math.Min(4, (int)(f * 4) + 1);
        double within = f * 4 - Math.Floor(f * 4);      // fraction through the quarter
        int secs = (int)Math.Round((1.0 - within) * 15 * 60); // clock counts down within a 15:00 quarter
        return $"Q{quarter} {secs / 60:00}:{secs % 60:00}";
    }

    // ---- the beat line: gutter + authored prose, and nothing else ----

    public static string BeatGutter(DramaEvent e) => Gutter(Clock(e));

    public static string BeatLine(DramaEvent e, Leg leg, double prevProb)
        => BeatGutter(e) + EventText.For(e, leg, prevProb);

    // ---- the meter line: gutter + bar + probability + the live offer ----

    public static string MeterGutter(int legIndex) => Gutter(LegAddress(legIndex));

    public static string MeterBar(DramaEvent e) => Ui.Bar(e.WinProbAfter, BarWidth);

    public static string MeterProbability(DramaEvent e) => $" {Ui.Pct(e.WinProbAfter)}%";

    public static string MeterOffer(double? cashOutOffer)
        => cashOutOffer.HasValue ? $"   CASH OUT: {Ui.Money(cashOutOffer.Value)} [C]" : string.Empty;

    public static string MeterLine(DramaEvent e, double? cashOutOffer)
        => MeterGutter(e.LegIndex) + MeterBar(e) + MeterProbability(e) + MeterOffer(cashOutOffer);

    // ---- the verdict line: §9.3, the leg named at its state change ----

    public static string VerdictLine(int legIndex, Leg leg, string verdict)
        => VerdictLine(legIndex, (leg ?? throw new ArgumentNullException(nameof(leg))).Matchup,
            leg.Selection, verdict);

    /// <summary>The same line for an unplaced selection — how the gate reaches all fifteen kinds
    /// without seating fifteen legs on a four-leg ticket.</summary>
    public static string VerdictLine(int legIndex, Matchup matchup, MarketSelection selection, string verdict)
        => new string(' ', LeftPad) + LegAddress(legIndex) + ": " + LegName(matchup, selection) + Separator + verdict;

    /// <summary>Every verdict a leg's state change can print — one list so the gate sweeps the set
    /// rather than the three it happens to remember.</summary>
    public static IReadOnlyList<string> Verdicts { get; } = new[] { WonVerdict, LostVerdict, VoidVerdict };
}

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
        Ui.WriteLine(ConsoleColor.White, $"ROUND {run.Round} — THE SWEAT   (payment due at settle: {Ui.Money(run.CurrentPayment)})");
        Ui.WriteLine(ConsoleColor.DarkGray,
            "[C] cash out  ·  [M] mulligan / [R] whistle (when a leg dies)  ·  [F] fast-forward");
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
        PaymentBeat(run);

        Ui.Line();
        Ui.Pause();
    }

    /// <summary>The payment settle beats (design/10): the deduction, or the Totem covering a
    /// shortfall. A missed payment is the run-over screen's line, not ours.</summary>
    private static void PaymentBeat(Run run)
    {
        SettlementReport r = run.LastSettlement!.Value;
        if (r.Outcome == Phase.RunLost) return;

        if (r.TotemFired)
        {
            Ui.WriteLine(ConsoleColor.Magenta, " THE TOTEM BURNS — the payment is deferred.");
            Ui.WriteLine(ConsoleColor.Yellow,
                $" Your bank is untouched ({Ui.Money(r.BankAfter)}); the next payment grows by "
                + $"{Ui.Money(r.Payment * (1.0 + run.Config.TotemJuiceRate))}.");
        }
        else
        {
            Ui.WriteLine(ConsoleColor.Green,
                $" PAYMENT MADE {Ui.Signed(-r.Payment)}  ·  bank {Ui.Money(r.BankAfter)}");
        }
    }

    private static void SweatOne(Run run, int index, int total, Dictionary<Ticket, double> cashOuts, ref double lastBank)
    {
        SweatSession session = run.Sweats[index];
        Ticket ticket = run.Tickets[index];

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

            // The pending-loss window (rev 5): a dead leg suspended the session — the player's
            // timed save. [M] voids (Mulligan, ≥2 legs), [R] sends it to review at the odds you
            // were living on (Whistle — full odds on an overturn, dead for real on a confirm).
            if (session.HasPendingLoss)
            {
                bool canM = run.OwnsConsumable("mulligan_slip") && session.CanMulliganPendingLoss;
                bool canR = run.OwnsConsumable("refs_whistle");
                switch (PromptSave(session, canM, canR))
                {
                    case ConsoleKey.M when canM:
                        run.PlayMulliganSlip(session);
                        Ui.WriteLine(ConsoleColor.Cyan, "  MULLIGAN SLIP — leg voided, the ticket lives");
                        continue;

                    case ConsoleKey.R when canR:
                        run.PlayRefsWhistle(session);
                        if (!session.IsComplete)
                        {
                            Ui.WriteLine(ConsoleColor.Green, "  REVIEWED — OVERTURNED. The leg STANDS at full odds.");
                            continue;
                        }
                        Ui.WriteLine(ConsoleColor.Red, "  REVIEWED — the call is CONFIRMED. Dead.");
                        break;

                    default:
                        session.DeclinePendingLoss();
                        break;
                }
            }

            if (session.IsComplete) break;

            // T140 arm A: the FIXTURE is the unit, not the leg — see SweatLines.OnFinalFixture for
            // why the old `evt.LegIndex == lastLeg` cannot fire on an interleaved ticket. The local
            // keeps its name because this grant is line-scoped; the rename to `onFinalFixture`
            // (and Hold's parameter with it) is owed to the markets lane.
            bool onFinalLeg = SweatLines.OnFinalFixture(evt, session);
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

    /// <summary>The window prompt: [M] mulligan / [R] whistle, anything else declines. Redirected
    /// input (the smoke pipeline) auto-declines — autoplay never consumes items.</summary>
    private static ConsoleKey PromptSave(SweatSession session, bool canM, bool canR)
    {
        if (Console.IsInputRedirected || (!canM && !canR)) return ConsoleKey.NoName;
        var verbs = new List<string>();
        if (canM) verbs.Add("[M] void the leg");
        if (canR) verbs.Add($"[R] send to review at {Ui.Pct(session.PendingLossProbBefore)}%");
        Ui.Write(ConsoleColor.Cyan, $"  SAVE? {string.Join("  ·  ", verbs)}  ·  any other key lets it die: ");
        ConsoleKey key = Console.ReadKey(true).Key;
        Ui.Line();
        return key;
    }

    /// <summary>
    /// A beat is two lines: the clock and the words, then the leg and the meter. <b>Every segment
    /// written here comes from <see cref="SweatLines"/></b> — the renderer owns the ink and the
    /// order, never the text — so <c>SweatLines.BeatLine</c> and <c>SweatLines.MeterLine</c> are
    /// exactly what lands on the screen, and the gate asserting them is asserting the real thing.
    ///
    /// <para><c>K16</c>: the meter line's gutter was eleven blank columns and is now the leg's
    /// address, so the reader knows which leg is speaking on every beat without the beat line
    /// carrying a tag. Same width, same ink, one casing.</para>
    /// </summary>
    private static void RenderEvent(DramaEvent e, Leg leg, SweatSession session, double prevProb, bool fast)
    {
        Ui.Write(ConsoleColor.DarkGray, SweatLines.BeatGutter(e));
        Ui.WriteLine(ConsoleColor.White, EventText.For(e, leg, prevProb));

        Ui.Write(ConsoleColor.DarkGray, SweatLines.MeterGutter(e.LegIndex));
        Ui.Write(ConsoleColor.Gray, SweatLines.MeterBar(e));
        Ui.Write(ConsoleColor.White, SweatLines.MeterProbability(e));
        double? offer = session.CashOutOffer();
        if (offer.HasValue)
            Ui.Write(ConsoleColor.Yellow, SweatLines.MeterOffer(offer));
        Ui.Line();

        if (e.Type == DramaEventType.LegFinal)
            RenderResolution(e, leg, session, fast);
    }

    /// <summary>
    /// §9.3 — <b>A LEG IS NAMED WHEN ITS STATE CHANGES.</b> This printed <c>LEG 1: ✘ DEAD</c>, and on
    /// frame a two-leg ticket died with neither leg's market appearing anywhere in the sweat: the
    /// player had to remember what LEG 1 was. It now prints the leg's market in the same name the
    /// ledger prints (<c>T69</c>/<c>T70</c>) — <c>LEG 1: OVER 9.5 CORNERS  ·  ✘ DEAD</c> — at no cost
    /// in rows, because the name goes onto the line that was already there.
    ///
    /// <para><b><c>S88</c> DOES NOT REACH THIS SURFACE, verified rather than assumed.</b> <c>S88</c>'s
    /// subject is <c>RevealedView</c>, the Unity-side mirror in <c>TvSweatScreen.cs</c> whose
    /// <c>ResolveLeg</c> has exactly one call site (<c>FinalSlam</c>), so an intermediate leg never
    /// leaves it. The console reads a DIFFERENT mirror: <c>SweatSession._revealed</c>, written by
    /// <c>ResolveLegFinal()</c> — which <c>MoveNext</c> calls on the LegFinal beat BEFORE it hands
    /// the event back (<c>SweatSession.cs:150-154</c>), on both the Won arm (<c>:164</c>) and the
    /// Lost arm (<c>:170</c>), and repaired on an overturned whistle (<c>:227</c>). So the state
    /// this line reads was written by the very event it is rendering. Two mirrors, two write paths;
    /// this one is not the stale one.</para>
    /// </summary>
    private static void RenderResolution(DramaEvent e, Leg leg, SweatSession session, bool fast)
    {
        if (leg.IsVoided)
        {
            // Not reachable through the mulligan today — the save window opens AFTER this method has
            // already rendered the leg's resolution, and no second LegFinal is emitted for a leg. It
            // is named anyway: a void IS a state change, and §9.3 does not have an arm that is
            // exempt because it is hard to reach. The authored line below is unchanged.
            Ui.WriteLine(ConsoleColor.Cyan, SweatLines.VerdictLine(e.LegIndex, leg, SweatLines.VoidVerdict));
            Ui.WriteLine(ConsoleColor.Cyan, "  MULLIGAN — leg voided, the ticket lives");
            return;
        }

        if (session.RevealedLegState(e.LegIndex) == LegState.Won)
        {
            Ui.WriteLine(ConsoleColor.Green, SweatLines.VerdictLine(e.LegIndex, leg, SweatLines.WonVerdict));
        }
        else
        {
            if (!fast) Ui.Beat(DeadBeatMs); // restraint IS the joke on bad beats
            Ui.WriteLine(ConsoleColor.Red, SweatLines.VerdictLine(e.LegIndex, leg, SweatLines.LostVerdict));
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

        // The payment model: the settle DEDUCTS CurrentPayment — show whether the bank holds it.
        bool met = run.Bank >= run.CurrentPayment;
        Ui.WriteLine(met ? ConsoleColor.Green : ConsoleColor.Red,
            $" BANK {Ui.Money(run.Bank)}  vs  PAYMENT DUE {Ui.Money(run.CurrentPayment)}   {(met ? "✔ COVERED" : "✘ SHORT")}");
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

    // Clock() moved to SweatLines — it is TEXT, and the gate has to be able to compose a beat line
    // without driving the console.
}
