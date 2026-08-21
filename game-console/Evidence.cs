using System;
using System.Globalization;
using SBR.Engine;

namespace SBR.ConsoleGame;

/// <summary>
/// THE EVIDENCE HOOK — <c>spec-console-surfaces-2026-08-19.md</c> §14.
///
/// <para>§14 owes transcripts of states an ordinary run does not deal. <b>Two of them cannot be shot
/// on whatever the default config produces, and they are NOT the same kind of problem</b>, so this
/// hook deliberately answers only one of them.</para>
///
/// <list type="bullet">
/// <item><b>A chosen seed needs NO hook and this class does not provide one.</b>
/// <c>Program.PromptSeed</c> already reads the seed off stdin, so a piped transcript picks its own
/// slate by writing the seed as its first line. <c>B3</c>'s 44-character club is a real, reachable
/// state found by SEARCHING numeric seeds on the shipped config — nothing is forced, and a found
/// seed needs no disclosure beyond naming it.</item>
/// <item><b><c>B5</c>'s empty destination is genuinely unreachable at the shipped config</b> —
/// <c>RunConfig.CorrectScoreFloor</c> is 0.02 and no matchup's score grid falls entirely under it.
/// So the floor, and only the floor, can be raised from outside. That is what this hook is.</item>
/// </list>
///
/// <para><b>It prints NOTHING into the surface's own output, ever</b> (<c>R38</c> / <c>S57</c>,
/// transferred from the laptop). On the laptop a caption naming the rig inside the frame would
/// itself have been rig state in a player slot; a line of disclosure printed into a transcript is
/// the same defect in a new medium. <b>The non-shipped value is disclosed in the transcript's
/// FILENAME and in the dock record, which are not the surface.</b></para>
///
/// <para><b>Ordinary play is untouched.</b> With the variable unset — which is every case except a
/// deliberate capture — <see cref="ConfigOverride"/> returns <c>null</c>, and <c>Run</c>'s own
/// <c>config ?? new RunConfig()</c> is today's behaviour exactly. It is read ONCE at startup,
/// before anything renders, so a malformed value fails before it can produce a transcript that
/// looks shot but is not.</para>
/// </summary>
internal static class Evidence
{
    /// <summary>
    /// The one field this hook exposes, named after what it is for. Narrow on purpose: a general
    /// "set any config property" backdoor would let a future capture change the economy without
    /// saying so, and the disclosure rule only works if what was changed is nameable.
    /// </summary>
    public const string CorrectScoreFloorVariable = "SBR_EVIDENCE_CORRECT_SCORE_FLOOR";

    /// <summary>
    /// Reads the hook. <c>null</c> — the ordinary case — means the shipped config, byte for byte.
    /// </summary>
    /// <exception cref="ArgumentException">The variable is set to something that is not a
    /// probability. Thrown rather than ignored: a capture silently falling back to the shipped
    /// config would be evidence of the wrong thing.</exception>
    public static RunConfig? ConfigOverride()
    {
        string? raw = Environment.GetEnvironmentVariable(CorrectScoreFloorVariable);
        if (string.IsNullOrWhiteSpace(raw)) return null;

        if (!double.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double floor)
            || double.IsNaN(floor) || floor < 0.0 || floor > 1.0)
        {
            throw new ArgumentException(
                $"{CorrectScoreFloorVariable}='{raw}' is not a probability in [0,1]. "
                + "It is the §14 evidence hook and it is NOT part of ordinary play.");
        }

        return new RunConfig { CorrectScoreFloor = floor };
    }
}
