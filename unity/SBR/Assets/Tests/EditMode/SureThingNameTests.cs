using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// S46 — one name, SURETHING, everywhere the player sees it.
    ///
    /// The defect this pins was four names for one app on one machine: `Sportsbook` under the
    /// desktop icon, `SURETHING.` in the taskbar, `SURETHING` in the tray, `SURETHING FORM` in the
    /// masthead — and a fifth, `SureThing.`, on the verdict screen. None of them was wrong on its
    /// own screen, which is exactly why it survived: no reviewer sees two of these at once.
    ///
    /// LaptopOsTests asserts what actually renders on the three destinations a player can reach
    /// without ending a run, and that is the stronger gate. This one exists for the two things a
    /// rendered scan cannot do: it covers the verdict screen (reachable only by driving a run to
    /// RunWon/RunLost), and it fails at the moment a retired spelling is typed rather than the next
    /// time someone happens to walk that screen.
    ///
    /// S16 exempts code identifiers, so the patterns below match only complete string literals that
    /// can be nothing but copy. `"SureThing"` on its own is deliberately legal — it is the tray
    /// slot's GameObject name, the Resources folder under `SureThing/Fonts`, and the name of a
    /// runtime material, and SureThingLedgerTests reaches the tray by it.
    /// </summary>
    public class SureThingNameTests
    {
        /// The SureThing surface, matching SureThingPaletteMarkupTests' own scan list.
        private static readonly string[] SurfaceFiles =
        {
            "SBR/Runtime/SportsbookApp.cs",
            "SBR/Runtime/LaptopOs.cs",
            "SBR/Runtime/LaptopScreen.cs",
        };

        /// <summary>Each pattern matches a whole string literal, quotes included, so an identifier
        /// never trips it: `SportsbookApp.Pluralize(...)` inside an interpolated string holds the
        /// token `Sportsbook` in code and is correctly ignored, while the retired caption
        /// `"Sportsbook"` is not.</summary>
        private static readonly (string Retired, Regex Pattern)[] RetiredSpellings =
        {
            // The old app name, as a caption. The enum member `Running.Sportsbook` and the type
            // `SportsbookApp` are identifiers and stay.
            ("Sportsbook", new Regex("\"Sportsbook\"", RegexOptions.Compiled)),

            // Any literal carrying the name with a full stop after it — the taskbar's
            // "SURETHING.   ·   LEDGER" and the verdict screen's "SureThing.". A path separator is
            // a slash, never a dot, so "SureThing/Fonts/Archivo" is untouched.
            ("the name with a trailing full stop",
                new Regex("\"[^\"\n]*(SURETHING|SureThing)\\.[^\"\n]*\"", RegexOptions.Compiled)),

            // The name with a screen's name welded onto it — S16 deleted "SURETHING LEDGER" and
            // S46 deletes "SURETHING FORM" for the same reason. And the spaced spellings.
            ("the name plus a screen, or a spaced spelling",
                new Regex("\"[^\"\n]*(SURETHING FORM|SURETHING LEDGER|SURE THING|Sure Thing)[^\"\n]*\"",
                    RegexOptions.Compiled)),
        };

        [Test]
        public void SureThing_surface_spells_the_app_one_way()
        {
            var offences = new List<string>();

            foreach (string relativePath in SurfaceFiles)
            {
                string absolutePath = Path.GetFullPath(
                    Path.Combine(Application.dataPath, relativePath));
                Assert.IsTrue(File.Exists(absolutePath),
                    $"{relativePath}: surface file is missing — update SurfaceFiles if it moved");

                string[] lines = File.ReadAllLines(absolutePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    // Comments name the retired spellings on purpose — every S46 edit records what
                    // it replaced, and the class note above quotes all five.
                    string code = StripComment(lines[i]);
                    if (code.Length == 0) continue;

                    foreach ((string retired, Regex pattern) in RetiredSpellings)
                        if (pattern.IsMatch(code))
                            offences.Add($"{relativePath}:{i + 1}: {retired} — {code.Trim()}");
                }
            }

            Assert.IsEmpty(offences,
                "S46: the app has one name, SURETHING, everywhere the player sees it — desktop icon, "
                + "tray slot, masthead. FORM is a screen, not part of the name, and the taskbar's "
                + "full stop is a spelling of its own. Code identifiers are exempt (S16); these "
                + "patterns only match complete string literals.\n" + string.Join("\n", offences));
        }

        /// <summary>Drops a trailing line comment. Same crude-by-design helper as
        /// SureThingPaletteMarkupTests, and duplicated rather than shared for the same reason a
        /// guard test avoids depending on another guard test: it only has to be good enough that
        /// prose about a retired name does not read as shipping it.</summary>
        private static string StripComment(string line)
        {
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("//") || trimmed.StartsWith("*") || trimmed.StartsWith("/*"))
                return string.Empty;

            int comment = line.IndexOf("//", System.StringComparison.Ordinal);
            return comment >= 0 ? line.Substring(0, comment) : line;
        }
    }
}
