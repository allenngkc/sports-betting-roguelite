using System;
using System.Globalization;
using System.Threading;
using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// Thin console primitives: ConsoleColor writes that always restore the prior colour, money/odds
/// formatting, the win-prob bar, and the handful of input helpers. No raw ANSI (Windows-safe).
/// Every method that could throw on a redirected stream (Clear, sleeps) degrades quietly so the
/// --auto smoke harness and piped runs never blow up.
/// </summary>
internal static class Ui
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static void Write(ConsoleColor c, string s)
    {
        ConsoleColor prev = Console.ForegroundColor;
        Console.ForegroundColor = c;
        Console.Write(s);
        Console.ForegroundColor = prev;
    }

    public static void WriteLine(ConsoleColor c, string s)
    {
        Write(c, s);
        Console.WriteLine();
    }

    public static void Line() => Console.WriteLine();

    /// <summary>
    /// The page's horizontal rule. <b>Fixed at the source, not per call site</b>
    /// (spec-console-surfaces-2026-08-19.md §3): this authored 62 for its whole life, which was
    /// <c>Ui.Title</c>'s 60-column box plus two — a width that came from a decoration rather than
    /// from content, and the exact <c>C46</c> shape the spec names. It now derives from
    /// <see cref="Page.Width"/> like every other layout on this surface, so the number is written
    /// down once (constitution §3.5) and every screen that rules gets the same page.
    /// </summary>
    public static void Rule() => WriteLine(ConsoleColor.DarkGray, Page.Rule());

    /// <summary>Whole-dollar money, thousands-separated. Negatives render as "-$X".</summary>
    public static string Money(double v)
    {
        double r = Math.Round(v);
        string body = Math.Abs(r).ToString("#,##0", Inv);
        return (r < 0 ? "-$" : "$") + body;
    }

    /// <summary>Signed whole-dollar delta, e.g. "+$412" / "-$200".</summary>
    public static string Signed(double v)
    {
        double r = Math.Round(v);
        return (r < 0 ? "-$" : "+$") + Math.Abs(r).ToString("#,##0", Inv);
    }

    /// <summary>American odds string with an explicit leading sign (+150 / -145).</summary>
    public static string American(double decimalOdds)
    {
        int a = OddsMath.AmericanFromDecimal(decimalOdds);
        return a >= 0 ? "+" + a.ToString(Inv) : a.ToString(Inv);
    }

    public static int Pct(double p) => (int)Math.Round(p * 100.0);

    /// <summary>A [####----]-style win-prob bar of the given width.</summary>
    public static string Bar(double prob, int width)
    {
        int filled = (int)Math.Round(Math.Clamp(prob, 0.0, 1.0) * width);
        filled = Math.Clamp(filled, 0, width);
        return "[" + new string('#', filled) + new string('-', width - filled) + "]";
    }

    public static void Clear()
    {
        try { Console.Clear(); } catch { /* redirected stream: nothing to clear */ }
    }

    /// <summary>Sleep, unless input is redirected (piped/auto) — then return instantly so nothing blocks.</summary>
    public static void Beat(int ms)
    {
        if (Console.IsInputRedirected) return;
        Thread.Sleep(ms);
    }

    /// <summary>True once stdin hit end-of-file (a piped script ran out of lines). Callers treat it
    /// as "quit" — otherwise a premature-EOF piped session would spin on the command loop forever.</summary>
    public static bool Eof { get; private set; }

    public static string Prompt(string label)
    {
        Write(ConsoleColor.White, label);
        string? s = Console.ReadLine();
        if (s == null)
        {
            Eof = true;
            return "";
        }
        return s.Trim();
    }

    public static void Pause()
    {
        Write(ConsoleColor.DarkGray, "  (enter to continue) ");
        Console.ReadLine();
    }

    public static bool Confirm(string label)
    {
        string s = Prompt(label).ToUpperInvariant();
        return s.StartsWith("Y", StringComparison.Ordinal);
    }

    public static void Title()
    {
        Clear();
        WriteLine(ConsoleColor.Cyan, "╔══════════════════════════════════════════════════════════╗");
        WriteLine(ConsoleColor.Cyan, "║   S B R   —   build it · lock it · sweat it              ║");
        WriteLine(ConsoleColor.Cyan, "╚══════════════════════════════════════════════════════════╝");
        WriteLine(ConsoleColor.DarkGray, "  a sports-betting roguelite prototype · the text era");
        Line();
    }
}
