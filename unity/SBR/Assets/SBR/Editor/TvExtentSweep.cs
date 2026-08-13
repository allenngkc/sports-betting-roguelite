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

        /// <summary>The engine's CLOSED name pools, transcribed from `SlateGenerator`.
        ///
        /// <para><b>The traceability pass's finding, and it is the direction T89-C says is invisible in
        /// frames.</b> Both generated arms of the leg row were swept against a PICKED champion rather
        /// than an enumerated pool — `REGULATORS ML` for the club arm, when `Gravediggers` and
        /// `Spreadsheets` are both in the pool and both two characters longer. The surface's own source
        /// names the case this missed: <i>"Fallback for an unlucky club — `GRAVEDIGGERS TO WIN` is 19
        /// and over budget."</i> The code knew the worst club; the sweep did not use it.</para>
        ///
        /// <para><b>And the two directions were not independent.</b> The old set also carried
        /// `BRICKLAYERS ANYTIME` — a CLUB noun in the SURNAME slot, which
        /// <c>{Surname} ANYTIME</c> cannot emit. That invented string was the widest member of its
        /// set, so it SET the measured worst case and hid the fact that the real widest producible
        /// form had never been measured. `PEND`'s class of error was concealing the opposite class.
        /// An over-generated string is not merely noise: while it is the maximum, it suppresses the
        /// under-generation it sits on top of.</para>
        ///
        /// <para>Picked champions are retired here. The pools are closed and small — 20 nouns and 12
        /// surnames — so every producible form is generated and measured, and no future name added to
        /// either pool can be missed by a champion nobody re-picked.</para></summary>
        private static readonly string[] ClubNouns =
        {
            "Yams", "Startups", "Bricklayers", "Longhaulers", "Mallards", "Spreadsheets",
            "Turnips", "Middlemen", "Regulators", "Plumbers", "Meatballs", "Auditors",
            "Ferrets", "Overheads", "Gravediggers", "Notaries", "Muskrats", "Zambonis",
            "Loopholes", "Refunds",
        };

        /// <summary>`SlateGenerator.PlayerLast`. The scorer arms take a SURNAME, never a club.</summary>
        private static readonly string[] Surnames =
        {
            "Ledger", "Cinder", "Muffin", "Pavement", "Coupon", "Wobble",
            "Gasket", "Pylon", "Ketchup", "Lanyard", "Racket", "Stapler",
        };

        /// <summary>Every member of a closed pool through one authored format, upper-cased the way the
        /// surface upper-cases it. Generating beats picking: the sweep then re-derives its own worst
        /// case whenever a pool grows.</summary>
        private static string[] From(string[] pool, string format)
        {
            var forms = new string[pool.Length];
            for (int i = 0; i < pool.Length; i++) forms[i] = string.Format(format, pool[i].ToUpperInvariant());
            return forms;
        }

        private static string[] And(params string[][] sets)
        {
            var all = new List<string>();
            foreach (string[] s in sets) all.AddRange(s);
            return all.ToArray();
        }

        /// <summary>Per slot: the strings it can be asked to render, longest-form first where known.
        /// Sources are named so a reader can check the set rather than trust it.</summary>
        private static readonly (string slot, string source, string[] strings)[] Cases =
        {
            // RE-ENUMERATED by the T89-C pass. The old set picked `LANYARD TO SCORE` out of a
            // 12-surname pool and carried `MIDDLEMEN ML` — an ML form, which is the LINE slot's
            // vocabulary and one NEED never emits (its moneyline form is `{CLUB} TO WIN`). Both
            // generated arms are now generated. The authored constants and the two fallbacks are
            // verbatim from ActiveLegCopy's construction sites.
            ("LegRowNeed0", "G1's NEED deck: two arms generated over the closed pools, constants verbatim",
                And(From(ClubNouns, "{0} TO WIN"), From(Surnames, "{0} TO SCORE"), new[]
                { "ONE TEAM SCORELESS", "ONE TEAM BLANKED", "BOTH TEAMS SCORE", "NOT YET",
                  "TO WIN", "TO SCORE" })),
            // CORRECTED by the traceability pass, in BOTH directions. The old set was
            // {UNDER 10.5 CORNERS, UNDER 10.5 CNRS, LANYARD TO SCORE, BOTH TEAMS SCORE, MIDDLEMEN ML}.
            // Two of those — LANYARD TO SCORE and BOTH TEAMS SCORE — are forms LegStatement does NOT
            // produce (it emits `{SURNAME} ANYTIME` and `BTTS YES`/`BTTS NO`), and the set missed the
            // GOALS and CARDS totals entirely. Enumerated from LegStatement's switch, every arm.
            // RE-ENUMERATED by the T89-C pass, and this is the slot whose 1.8px relief rested on the
            // old set. `BRICKLAYERS ANYTIME` was unproducible (a club noun in the surname slot) and
            // was the set's own maximum; `REGULATORS ML` was a picked champion two characters short
            // of the pool's longest. Both arms are generated now.
            ("LegRowLine0", "LegStatement's six arms: ML and ANYTIME generated over the closed pools — see the UNBOUNDED note on its default",
                And(From(ClubNouns, "{0} ML"), From(Surnames, "{0} ANYTIME"), new[]
                { "UNDER 10.5 CORNERS", "UNDER 10.5 GOALS", "UNDER 10.5 CARDS",
                  "BTTS YES", "BTTS NO" })),
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
            // T88's gesture gave the status word a THIRD state: under a held preview the only act left
            // is the commit, so the word says so. Added with the wiring rather than after it.
            ("CashOutStatus", "§6.1 status words — all three states of CashOutStatusWord()",
                new[] { "UPDATING", "HOLD E", "ENTER TO CASH OUT" }),
            // CORRECTED: the separator is FIVE spaces in the format string, not three, and
            // PotentialPayout is parlay-multiplied so its magnitude has no ceiling here.
            ("RiskPays", "the format string at :2299 — payout magnitude UNBOUNDED", new[]
                { "RISK $1,234     PAYS $12,340", "RISK $50     PAYS $450" }),
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
            // The prompt is a LIST now (DD batch 50), so its renderable forms are ROWS rather than one
            // run-on line, and a row is what this sweep's widest-line measure was always reporting.
            // T88's gesture adds the two held-preview rows. Height is not this instrument's question
            // and is answered by `SBR/TV/T88 prompt composition`, which owns the zone's 90px.
            ("InterventionPrompt", "the list's rows + the held preview's, from PendingWindowBeat", new[]
                { "SHOT FROZEN", "HOLD M MULLIGAN (ONE MULLIGAN SLIP)",
                  "HOLD R SEND TO REVIEW (ONE REF'S WHISTLE)", "HOLD N LET IT DIE",
                  "ENTER CONFIRMS   ·   RELEASE ABANDONS" }),
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
                    // T89-B wants the tabular BASIS per row, and "no digits" was doing two jobs: it
                    // printed for a string with no figures in it AND for a string whose figures are
                    // already uniform, which are different facts. `RISK $1,234     PAYS $12,340` is
                    // full of digits and still screened to zero — because the derived tabular font is
                    // wired and the advances are already equal. Reporting that as "no digits" hid the
                    // very confirmation the condition asks for.
                    bool hasDigits = false;
                    foreach (char c in worstTabS) if (c >= '0' && c <= '9') { hasDigits = true; break; }
                    string digitNote = !hasDigits ? "no digits in the widest form"
                        : Mathf.Approximately(worstTab, worst) ? "digits ALREADY TABULAR — screen is a no-op"
                        : $"screened from '{Show(worstTabS)}'";
                    Debug.Log($"[T84] {slot,-16} box {box,6:0.0}px  widest '{Show(worstS)}' {worst,6:0.0}px  " +
                              $"TABULAR {worstTab,6:0.0}px ({digitNote})  " +
                              // `t.font` is the PRIMARY asset, not the arm that renders. A slot built
                              // at FontWeight.Bold draws through the bold asset wired by WireBold
                              // while `font.name` still reads the regular one — so printing the face
                              // alone said `EncodeSansCondensed SDF` for CashOut, which is real
                              // Condensed BOLD 700 (T73) and is recorded as such in the DD's own
                              // batch-38 inventory. The weight is what disambiguates the pair, so the
                              // weight ships beside the face.
                              $"[face '{t.font?.name}' w{(int)t.fontWeight} style {t.fontStyle} " +
                              $"tracking {t.characterSpacing / 100f:0.000}em type {t.fontSize:0.#}px]  " +
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
