using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.EditorTools
{
    /// <summary>
    /// T84's sweep: every slot whose extent, or whose neighbour's position, was derived against the
    /// pre-migration face — inventory naming its members, per C18 §4.1.
    ///
    /// <para><b>What the exposure actually is.</b> This surface computes no geometry from content:
    /// §6 forbids it and the row builder says so at its own call site — <i>"Widths are fixed, never
    /// derived from content (§6): the chip reserves canon's 38px"</i>. So nothing here has to be
    /// re-derived from a measurement. What every fixed box DOES carry is an assumption that its
    /// content fits, and every one of those was sized against a face 20% narrower than the one the
    /// surface now renders. The boxes did not move; the strings grew inside them.</para>
    ///
    /// <para>Two members were found by eye, in two of nine moments, and the DD recorded that nobody
    /// had looked at the other seven. This looks at all of them at once, by measurement rather than
    /// by frame inspection — a frame only shows the string that seed happened to produce, while a
    /// reserved box either holds the longest form it can be asked to render or it does not.</para>
    ///
    /// <para><b>Blind spot, stated rather than implied:</b> a slot is only as swept as its string set
    /// is complete. Where the longest renderable form is authored in a deck this reads that deck.
    /// Where content is generated — team names, money, clocks — it uses the widest realistic form and
    /// says so. Any slot whose content this could not enumerate is reported UNSWEPT rather than
    /// silently passed.</para>
    /// </summary>
    public static class TvExtentSweep
    {
        /// <summary>Measure width with NO wrapping constraint.
        ///
        /// <para>The first run of this sweep passed 0 and reported the compact statement slot at
        /// 12.5px — one character. That slot is the only one with wrapping enabled, and a wrapping
        /// component asked for its preferred size at zero width wraps every character, so the answer
        /// is the widest glyph rather than the widest string.</para>
        ///
        /// <para>It matters far beyond this instrument: the runtime's own <c>Fits</c> and
        /// <c>FitToColumn</c> make the same zero-width call, on the same wrapping slot. UGUI's
        /// <c>GetPreferredWidth</c> returned the unwrapped width regardless of wrap mode, so the
        /// behaviour did not survive the port. Reported as a finding of this sweep.</para></summary>
        private const float Unconstrained = 100000f;

        /// <summary>Per slot: the strings it can be asked to render, longest-form first where known.
        /// Sources are named so a reader can check the set rather than trust it.</summary>
        private static readonly (string slot, string source, string[] strings)[] Cases =
        {
            ("LegRowNeed0", "G1 deck (authored NEED statements)", new[]
                { "ONE TEAM SCORELESS", "ONE TEAM BLANKED", "LANYARD TO SCORE", "BOTH TEAMS SCORE",
                  "MIDDLEMEN ML", "NOT YET" }),
            ("LegRowLine0", "G1 deck (compact statements)", new[]
                { "UNDER 10.5 CORNERS", "UNDER 10.5 CNRS", "LANYARD TO SCORE", "BOTH TEAMS SCORE",
                  "MIDDLEMEN ML" }),
            ("LegRowPrice0", "price forms, generated", new[] { "+450", "-110", "2.75", "+1200" }),
            ("LegRowState0", "the state vocabulary", new[] { "LIVE", "NEXT", "WON", "LOST", "VOID", "PEND" }),
            ("LegRowProgress0", "revealed progress lines, generated", new[]
                { "0-0, 62' PLAYED", "NEEDS 1 MORE, 78'", "2-1, 88' PLAYED" }),
            ("CashOut", "§6.1 money control, six states", new[]
                { "MARKET SUSPENDED", "CASHED OUT $1,240", "CASH OUT $1,240", "CASH OUT $183" }),
            ("CashOutStatus", "§6.1 status words", new[] { "UPDATING", "HOLD E" }),
            ("RiskPays", "risk/pays figures, generated", new[] { "RISK $1,234   PAYS $12,340", "RISK $50   PAYS $450" }),
            ("Matchup", "scoreline, generated team names", new[]
                { "ZAMBONIS 0 — REGULATORS 1", "BRICKLAYERS 0 — MIDDLEMEN 0", "STARTUPS 1 — PLUMBERS 2" }),
            ("Score", "the punch overlay mirrors Matchup", new[] { "ZAMBONIS 0 — REGULATORS 1" }),
            ("Clock", "clock forms seen in the after-set manifest", new[] { "90'+2", "90'+1", "PRE", "FT", "45'" }),
            ("TicketHeader", "tv.card.html:20", new[] { "TICKET 1 OF 2", "TICKET 2 OF 2" }),
            ("Flavor", "event strip, authored narration", new[]
                { "REGULATORS BREAK AWAY DOWN THE RIGHT", "ZAMBONIS CLEAR THE LINE", "GOAL" }),
            ("Chrome", "PRD §8.1 chrome row", new[] { "ROUND 3   BANK $1,240   PAYMENT $800   SEED 48151623" }),
        };

        /// <summary>Slots reported but NOT swept, because their longest renderable form is not
        /// enumerable from here. Named so the inventory says what it does not cover.</summary>
        private static readonly string[] Unswept =
        {
            "Attract", "TakeoverTitle", "TakeoverSub", "Subtitle", "Consolation", "InterventionPrompt",
        };

        [MenuItem("SBR/TV/T84 extent sweep")]
        public static void Sweep()
        {
            var go = new GameObject("ExtentSweep");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<SBR.Game.TvSweatScreen>();
                screen.theaterEnabled = false;
                typeof(SBR.Game.TvSweatScreen)
                    .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(screen, null);

                var all = new Dictionary<string, TMP_Text>();
                foreach (TMP_Text t in screen.GetComponentsInChildren<TMP_Text>(true))
                    all[t.gameObject.name] = t;

                Debug.Log($"[T84] sweeping {Cases.Length} slots with string sets; " +
                          $"{Unswept.Length} reported unswept; {all.Count} text slots exist on the surface");

                int overrunning = 0;
                foreach ((string slot, string source, string[] strings) in Cases)
                {
                    if (!all.TryGetValue(slot, out TMP_Text t))
                    {
                        Debug.Log($"[T84] {slot,-16} NOT BUILT in this configuration — not swept");
                        continue;
                    }
                    float box = t.rectTransform.rect.width;
                    float worst = float.MinValue;
                    string worstS = "";
                    foreach (string s in strings)
                    {
                        float w = t.GetPreferredValues(s, Unconstrained, 0f).x;
                        if (w > worst) { worst = w; worstS = s; }
                    }
                    bool over = worst > box;
                    if (over) overrunning++;
                    Debug.Log($"[T84] {slot,-16} box {box,6:0.0}px  widest '{worstS}' {worst,6:0.0}px  " +
                              $"{(over ? $"OVERRUNS by {worst - box:0.0}px" : $"fits, {box - worst:0.0}px spare")}  " +
                              $"· set: {source}");
                }

                // The two-into-one-box members: §6.1's money control and the leg row's three spans
                // share a rectangle from opposite edges, so each one's slack is the other's overrun.
                Pair(all, "CashOut", "CASH OUT $1,240", "CashOutStatus", "HOLD E");
                Pair(all, "CashOut", "CASH OUT $183", "CashOutStatus", "UPDATING");

                foreach (string s in Unswept)
                    Debug.Log($"[T84] {s,-16} UNSWEPT — longest renderable form not enumerable from here");

                Debug.Log($"[T84] slots overrunning their fixed box: {overrunning} of {Cases.Length} swept");
            }
            finally { Object.DestroyImmediate(go); }
        }

        /// <summary>Two slots anchored from opposite edges of one rectangle. Neither overruns its own
        /// box; together they overprint, which is the shape the pair caught and which no single-slot
        /// measurement can see.</summary>
        private static void Pair(Dictionary<string, TMP_Text> all, string aName, string aText,
                                 string bName, string bText)
        {
            if (!all.TryGetValue(aName, out TMP_Text a) || !all.TryGetValue(bName, out TMP_Text b)) return;
            float box = a.rectTransform.rect.width;
            float aw = a.GetPreferredValues(aText, 0f, 0f).x;
            float bw = b.GetPreferredValues(bText, 0f, 0f).x;
            float slack = box - (aw + bw);
            Debug.Log($"[T84] PAIR {aName}+{bName}  box {box:0.0}px  '{aText}' {aw:0.0} + '{bText}' {bw:0.0} " +
                      $"= {aw + bw:0.0}px  {(slack < 0f ? $"COLLIDES by {-slack:0.0}px" : $"clears by {slack:0.0}px")}");
        }
    }
}
