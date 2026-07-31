using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// Guards the SureThing surface against colour that hides inside strings.
    ///
    /// A palette audit that reads Color fields is blind to a colour written as rich-text markup —
    /// `&lt;color=#3CE873&gt;` renders green regardless of what the palette constants say, and no
    /// field-level scan will ever see it. That blind spot was found on another surface in this
    /// project, where retired green/red money language survived a palette pass by living in
    /// interpolated strings.
    ///
    /// SureThing has no such markup today. This test exists so it stays that way: the cheapest
    /// moment to catch a hardcoded colour is the moment it is written, not during a later audit
    /// that may not think to look in string literals.
    ///
    /// If a future design genuinely needs mixed colour inside one run of text, that is a real
    /// requirement — but it must go through the palette constants and a deliberate decision, so
    /// the fix is to extend this test with a named, justified exemption, not to delete it.
    /// </summary>
    public class SureThingPaletteMarkupTests
    {
        /// The SureThing surface. TV, theater and room files are other teams' and are not scanned.
        private static readonly string[] SurfaceFiles =
        {
            "SBR/Runtime/SportsbookApp.cs",
            "SBR/Runtime/LaptopOs.cs",
            "SBR/Runtime/LaptopScreen.cs",
        };

        /// <summary>Rich-text tags that carry appearance. Colour is the one that matters most, but
        /// size and alpha smuggle presentation past a palette review the same way.</summary>
        private static readonly Regex AppearanceMarkup = new Regex(
            @"<\s*(color|size|alpha|material)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>A literal HTML colour. Deliberately does NOT match C# hex like 0x3C, so the
        /// palette's own Color32(0x16, 0x16, 0x0F) definitions are unaffected.</summary>
        private static readonly Regex HtmlColourLiteral = new Regex(
            @"#[0-9A-Fa-f]{3,8}\b", RegexOptions.Compiled);

        [Test]
        public void SureThing_surface_declares_no_colour_inside_string_markup()
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
                    string line = lines[i];

                    // Comments may legitimately discuss a colour by hex — the class note above does
                    // exactly that, and so do several palette rationale comments in the runtime.
                    string code = StripComment(line);
                    if (code.Length == 0) continue;

                    if (AppearanceMarkup.IsMatch(code))
                        offences.Add($"{relativePath}:{i + 1}: appearance markup in a string — {code.Trim()}");
                    else if (HtmlColourLiteral.IsMatch(code))
                        offences.Add($"{relativePath}:{i + 1}: literal colour — {code.Trim()}");
                }
            }

            Assert.IsEmpty(offences,
                "Colour must come from the LaptopOs palette constants, never from markup or a literal "
                + "inside a string. A colour written this way is invisible to a field-level palette "
                + "audit and survives exactly the kind of retirement ruling this project has already "
                + "issued once.\n" + string.Join("\n", offences));
        }

        /// <summary>Drops a trailing line comment. Crude by design — it only has to be good enough
        /// that prose about a colour does not read as shipping that colour, and erring toward
        /// scanning too much is the safe direction for a guard test.</summary>
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
