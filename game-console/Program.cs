using System;
using System.Text;
using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// Entry point. `--auto [seed]` runs the non-interactive smoke harness; otherwise the interactive
/// game. All colour is restored on every exit path via the outer try/finally.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            if (args.Length > 0 && args[0] == "--auto")
            {
                string autoSeed = args.Length > 1 ? args[1] : "AUTO-1";
                return GameLoop.AutoRun(autoSeed);
            }

            // §14's evidence hook, resolved BEFORE anything renders so a malformed value cannot
            // produce a transcript that looks shot but is not. Null — the ordinary case — is the
            // shipped config; see Evidence.cs for why only ONE field is exposed and why a chosen
            // seed is not one of them (stdin already answers the seed prompt).
            RunConfig? evidenceConfig = Evidence.ConfigOverride();

            TrySetupConsole();
            InteractiveLoop(evidenceConfig);
            return 0;
        }
        finally
        {
            try { Console.ResetColor(); } catch { /* redirected: nothing to reset */ }
        }
    }

    private static void InteractiveLoop(RunConfig? config = null)
    {
        Ui.Title();
        bool again = true;
        while (again)
        {
            string seed = PromptSeed();
            var run = new Run(seed, config);
            again = GameLoop.Play(run);
        }
        Ui.WriteLine(ConsoleColor.DarkGray, "thanks for playing.");
    }

    private static string PromptSeed()
    {
        string entry = Ui.Prompt("seed (blank = random): ");
        string seed = string.IsNullOrWhiteSpace(entry) ? RandomSeed() : entry.Trim();
        Ui.Line();
        Ui.WriteLine(ConsoleColor.Yellow, $"SEED: {seed}   (share it — same seed, same run)");
        Ui.Beat(700);
        return seed;
    }

    private static string RandomSeed()
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var rng = new Random(); // the console shell may use System.Random; determinism binds the engine, not this
        char[] c = new char[8];
        for (int i = 0; i < c.Length; i++) c[i] = alphabet[rng.Next(alphabet.Length)];
        return new string(c);
    }

    private static void TrySetupConsole()
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { /* some terminals reject this; fine */ }
        try { Console.Title = "SBR — the sweat"; } catch { /* not a TTY */ }
    }
}
