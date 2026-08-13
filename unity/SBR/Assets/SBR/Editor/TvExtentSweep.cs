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
            // CORRECTED. The first set here was {LIVE, NEXT, WON, LOST, VOID, PEND} and I invented
            // it — the plausible vocabulary for a state chip, not this build's. Enumerated properly
            // from every SetRowChip call site, the chip renders exactly five things, and PEND is not
            // among them. The 6px overrun this slot reported was on a string it cannot display.
            // My own rule, broken by me on the one slot I did not grep: a slot is only as swept as
            // its string set is complete.
            ("LegRowState0", "every SetRowChip call site: 2215, 2221, 2230, 2254, 2287",
                new[] { "VOID", "NEXT", "W", "L", "" }),
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

            // The six that were UNSWEPT, now enumerated from their assignment sites rather than
            // guessed. Five resolve to literals or to formats with bounded fields; the sixth is
            // marked CONSTRUCTED because its content is engine-generated and has no bound readable
            // from this surface.
            ("Attract", "4 assignment sites: 2 literals, the win/lose pair, RenderIdle titles", new[]
                { "ROUND 10 OF 12 · BOARD OPEN", "SIT TO WATCH THE SWEAT", "THE HOUSE BLINKS FIRST",
                  "THE BOOKIE COLLECTS", "SHOP OPEN" }),
            ("TakeoverTitle", "3 assignment sites (a fourth clears it)", new[]
                { "SHORT — $12,340 AGAINST $20,000", "TICKET 1 OF 2", "PAYMENT MADE" }),
            ("TakeoverSub", "the deferral line, plus the leg list — CONSTRUCTED, see note", new[]
                { "PAYMENT DEFERRED — YOUR BANK STANDS. THE NEXT ONE GROWS BY $1,200",
                  "LANYARD TO SCORE ANYTIME +450   ·   BOTH TEAMS TO SCORE -110   ·   UNDER 10.5 CORNERS +240" }),
            ("Subtitle", "RenderIdle sub, plus the run-over line", new[]
                { "FINAL BANK $12,340  —  NEW RUN AT THE LAPTOP",
                  "gear up at the laptop, then the next round" }),
            ("Consolation", "the authored four-line deck, verbatim", new[]
                { "the model remains extremely confident.", "the book thanks you for your patronage.",
                  "a courtesy: nobody saw that.", "so close. they always are." }),
            ("InterventionPrompt", "one site; widest LINE with both consumables owned", new[]
                { "SHOT FROZEN\n[M] MULLIGAN   ·   [R] SEND TO REVIEW (99%)   ·   [N] LET IT DIE" }),
        };

        /// <summary>Slots reported but NOT swept. Empty now: the six that stood here were enumerated
        /// from their assignment sites, which is what "a slot is only as swept as its string set is
        /// complete" obliges.
        ///
        /// <para><b>One residual, carried rather than cleared.</b> `TakeoverSub` renders the ticket's
        /// leg list — <c>DisplayLabel</c> joined by a separator, one entry per leg — and
        /// <c>DisplayLabel</c> is the ENGINE's label, the long concatenated form T69 ruled against on
        /// the leg row. Its length is not bounded by anything readable on this surface, so its entry
        /// is a CONSTRUCTED three-leg worst case, not an enumeration. A longer ticket or a longer
        /// fixture makes it longer, and this sweep cannot say by how much.</para></summary>
        private static readonly string[] Unswept = { };

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
                    float[] digitPx = MeasureDigits(t);
                    // The tabular advance, as an UPPER BOUND measured on this component rather than
                    // assumed. The first cut used 0.5 * fontSize — 1000 of the font's 2000 upem — and
                    // that is only true of the DEFAULT instance. These assets are other instances of
                    // a variable family, where the same glyph's advance scales, and the tell was
                    // LegRowPrice screening NARROWER on a string of narrow digits, which no tabular
                    // set can do. Using the widest proportional digit instead: a tabular set is
                    // uniform and sits at or below its family's widest figure (1000 against 1036 in
                    // the default instance here), so this never understates, which is the only
                    // direction a screen may err in.
                    float tabularPx = 0f;
                    foreach (float d in digitPx) tabularPx = Mathf.Max(tabularPx, d);

                    float worst = float.MinValue, worstTab = float.MinValue;
                    string worstS = "", worstTabS = "";
                    foreach (string s in strings)
                    {
                        float w = t.GetPreferredValues(s, Unconstrained, 0f).x;
                        if (w > worst) { worst = w; worstS = s; }
                        float tab = w;
                        foreach (char c in s)
                            if (c >= '0' && c <= '9') tab += tabularPx - digitPx[c - '0'];
                        if (tab > worstTab) { worstTab = tab; worstTabS = s; }
                    }

                    bool over = worstTab > box;
                    if (over) overrunning++;
                    string digitNote = Mathf.Approximately(worstTab, worst) ? "no digits" : $"screened from '{Show(worstTabS)}'";
                    Debug.Log($"[T84] {slot,-16} box {box,6:0.0}px  widest '{Show(worstS)}' {worst,6:0.0}px  " +
                              $"TABULAR {worstTab,6:0.0}px ({digitNote})  " +
                              $"{(over ? $"OVERRUNS by {worstTab - box:0.0}px" : $"fits, {box - worstTab:0.0}px spare")}  " +
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

        /// <summary>A multi-line string on one log line. `InterventionPrompt` carries a newline, and
        /// its row came out fractured across three log lines with its verdict on the third — a table
        /// that cannot be read as a table. The measurement was right; the report was not.</summary>
        private static string Show(string s) => s.Replace("\n", "\\n");

        /// <summary>Each digit's CURRENT advance on this component, in px, measured as ten of it.
        ///
        /// <para>Batch 41 binds digit rows to TABULAR metrics: the assets are still built from the
        /// source face, so every digit-bearing row measured as-shipped is measured proportionally,
        /// and against the widest realistic digits that UNDERSTATES what the wired surface will do.
        /// The screen replaces each digit's proportional advance with the tabular one — 1000 of the
        /// font's 2000 upem, exactly half an em, which the derived font makes true for all ten.</para>
        ///
        /// <para>Measured rather than read from hmtx so the number is in the same units, on the same
        /// component, as the string widths it corrects. Screening only: a flagged box takes rendered
        /// confirmation once the wiring lands.</para></summary>
        private static float[] MeasureDigits(TMP_Text t)
        {
            var px = new float[10];
            for (int d = 0; d < 10; d++)
                px[d] = t.GetPreferredValues(new string((char)('0' + d), 10), Unconstrained, 0f).x / 10f;
            return px;
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
