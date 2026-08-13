using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.EditorTools
{
    /// <summary>
    /// T88's composition probe: the two numbers the DD is owed on the intervention prompt, neither of
    /// which the T84 sweep can produce.
    ///
    /// <para><b>1. The overrun's breakdown.</b> The sweep reports one figure per slot — the widest
    /// LINE of the widest string — so `InterventionPrompt` reads 1015.0px in a 635.0px box and the
    /// report cannot say which line that is. The DD asked the question directly: <i>"whether the
    /// overrun is on the options line alone or on the whole prompt including SHOT FROZEN, because the
    /// list ruling changes the arithmetic and the two are answered differently."</i> This measures the
    /// title, each option and the separator as separate quantities.</para>
    ///
    /// <para><b>2. The zone verdict.</b> The 380px was answered structurally — three options are a
    /// LIST, one per row — under a stated condition: <i>"a list needs vertical room. If the prompt's
    /// zone cannot carry three rows, that comes back here with the zone's dimensions."</i> The sweep
    /// is a WIDTH instrument and has never measured a height on this surface. Answering the condition
    /// needs the zone's height and the composed list's height in the same units, on the same
    /// component. That is this probe's whole reason to exist.</para>
    ///
    /// <para><b>Why the strings are guarded rather than trusted.</b> This lane has withdrawn three
    /// findings that were measured on string sets it invented instead of enumerating — `PEND` on a
    /// chip that cannot render it, two `LegStatement` forms the switch does not emit, a `RiskPays`
    /// separator of the wrong width. Retyping the prompt's copy here is the same exposure, so the
    /// atoms below are ASSERTED against `TvSweatScreen.cs` on disk before a single width is printed:
    /// if the copy moves, this probe refuses rather than measuring a string nobody ships. The scan
    /// strips comment lines first — batch 16 recorded a guard that matched the very comment recording
    /// why a string was retired, and `HOLD M` appears in this file's own T86(a) comment.</para>
    ///
    /// <para><b>Method, stated with the numbers (C25/C33):</b> widths are measured UNCONSTRAINED, the
    /// same call T84 makes, so every figure here is directly comparable to the sweep's. Heights are
    /// measured on explicit `\n` compositions rather than on wrapping, so the row count is the one
    /// authored and not one the measurer chose. No string here carries a digit, so the sweep's
    /// tabular screen has nothing to correct and is deliberately not applied.</para>
    /// </summary>
    public static class TvPromptComposition
    {
        /// <summary>T84's own constant, same reasoning: a wrapping component asked for its preferred
        /// size at zero width wraps every character and returns the widest GLYPH.</summary>
        private const float Unconstrained = 100000f;

        // The atoms, verbatim from the assignment site (TvSweatScreen.PendingWindowBeat). The
        // separator is written as an escape rather than the literal glyph so that no encoding
        // round-trip on this file can silently change what is being measured; the source scan below
        // proves it is the same character the surface ships.
        private const string Sep = "   \u00B7   ";
        private const string Title = "SHOT FROZEN";
        private const string OptM = "HOLD M MULLIGAN (ONE MULLIGAN SLIP)";
        private const string OptR = "HOLD R SEND TO REVIEW (ONE REF'S WHISTLE)";
        // Batch 56: no HOLD. The decline is a press (T88(c)), and the copy now says so.
        private const string OptN = "N LET IT DIE";

        // T88's gesture copy, added with the wiring. Both are UNRATIFIED and both are composed in the
        // source from `ConfirmKeyWord`, so the scan below asserts the invariant HALF of each \u2014 the
        // part that does not move if the confirm key is ratified to something other than ENTER.
        private const string ConfirmWord = "ENTER";
        private const string ConfirmLine = ConfirmWord + " CONFIRMS" + Sep + "RELEASE ABANDONS";
        private const string StatusPreview = ConfirmWord + " TO CASH OUT";

        /// <summary>The slot this probe is about, and the file its copy is authored in.</summary>
        private const string Slot = "InterventionPrompt";
        private const string SourceRel = "SBR/Runtime/TvSweatScreen.cs";

        [MenuItem("SBR/TV/T88 prompt composition")]
        public static void Measure()
        {
            if (!AssertStringSetIsTheShippedOne()) return;

            var go = new GameObject("PromptComposition");
            go.SetActive(false);
            try
            {
                var screen = go.AddComponent<SBR.Game.TvSweatScreen>();
                screen.theaterEnabled = false;
                typeof(SBR.Game.TvSweatScreen)
                    .GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(screen, null);

                TMP_Text t = null;
                foreach (TMP_Text c in screen.GetComponentsInChildren<TMP_Text>(true))
                    if (c.gameObject.name == Slot) { t = c; break; }

                if (t == null)
                {
                    Debug.Log($"[T88] {Slot} NOT BUILT in this configuration — nothing measured");
                    return;
                }

                // The zone as BUILT, read off the component, not the constant it was built from.
                Rect zone = t.rectTransform.rect;
                Debug.Log($"[T88] zone  {Slot}  {zone.width:0.0} x {zone.height:0.0}px  " +
                          $"type {t.fontSize:0.#}px  face '{t.font?.name}'  " +
                          $"wrap {t.textWrappingMode}  overflow {t.overflowMode}");

                // ---- 1. the width breakdown the DD asked for -------------------------------------
                float wTitle = W(t, Title), wSep = W(t, Sep);
                float wM = W(t, OptM), wR = W(t, OptR), wN = W(t, OptN);

                // The options line exactly as PendingWindowBeat composes it with both consumables
                // owned — option, separator, option, separator, option — and the whole two-line
                // string the sweep measures, so this probe's numbers can be checked against T84's.
                string optionsLine = OptM + Sep + OptR + Sep + OptN;
                string shipped = Title + "\n" + optionsLine;
                float wOptions = W(t, optionsLine), wShipped = W(t, shipped);

                Row(zone.width, "title            ", Title, wTitle);
                Row(zone.width, "option M         ", OptM, wM);
                Row(zone.width, "option R         ", OptR, wR);
                Row(zone.width, "option N         ", OptN, wN);
                Debug.Log($"[T88] separator '{Show(Sep)}' {wSep,7:0.0}px  (two of them on the line)");

                Row(zone.width, "OPTIONS LINE     ", optionsLine, wOptions);
                Row(zone.width, "WHOLE PROMPT     ", shipped, wShipped);

                // The DD's question, answered as a sentence rather than left to arithmetic.
                Debug.Log($"[T88] BREAKDOWN: the overrun is on the OPTIONS LINE " +
                          $"({wOptions:0.0}px, {wOptions - zone.width:0.0} over). " +
                          $"`{Title}` is {wTitle:0.0}px and {(wTitle > zone.width ? "OVERRUNS" : $"clears by {zone.width - wTitle:0.0}px")} — " +
                          $"it contributes nothing to the 380px.");

                // Two self-checks. C34.1: an unasserted pin is a comment.
                //
                // The separator measures NARROWER standing alone than it advances inside a line, and
                // the difference is not kerning: TMP's preferred width drops TRAILING whitespace, so
                // `"   (mid)   "` measured on its own loses its three trailing spaces. Summing atoms
                // therefore understates the composed line, and by a fixed amount per join. Measured
                // both ways rather than asserted, because the first cut of this probe printed the gap
                // as kerning — a wrong reason attached to a right number is how folklore starts.
                float sepInLine = W(t, OptM + Sep + OptM) - 2f * W(t, OptM);
                float joined = wM + sepInLine + wR + sepInLine + wN;
                Debug.Log($"[T88] separator standalone {wSep:0.0}px vs IN LINE {sepInLine:0.0}px  " +
                          $"(+{sepInLine - wSep:0.0}px = the trailing spaces TMP drops when it is measured alone)");
                Debug.Log($"[T88] CHECK atoms + in-line separators {joined:0.0}px vs composed line {wOptions:0.0}px  " +
                          $"delta {wOptions - joined:0.000}px — {(Mathf.Abs(wOptions - joined) < 0.05f ? "AGREE: the line is the sum of its parts, no kerning across the joins" : "DISAGREE: something beyond whitespace acts at the joins; use the composed figure")}");
                // NO HARD-CODED EXPECTED VALUE HERE, and the first cut of this probe had one: it
                // pinned 1015.0px, the sweep's figure for the pre-batch-50 copy. The very next run —
                // the one after T88(b) added MULLIGAN's cost — reported the instrument as broken when
                // only the copy had moved. That is this lane's own recorded failure class, an
                // instrument that silently stops covering what it claims, arriving from the other
                // direction: a pin that goes stale reads as a DEFECT rather than as silence. The
                // sweep's own case string is the thing to agree with, and both now read the list.

                // ---- 2. the zone verdict, which is a HEIGHT question ------------------------------
                // The option count is not fixed: M is offered only while the run owns a Mulligan Slip
                // and the session permits it, R only while it owns a Ref's Whistle. N is always
                // present. So a list is 1 to 3 option rows, and every arm is measured rather than
                // only the worst one — the zone has to hold the worst, but the DD is composing all of
                // them.
                Debug.Log("[T88] --- list composition, one option per row (S24's shape) ---");
                Height(t, zone, "title + M,R,N (worst)", new[] { Title, OptM, OptR, OptN });
                Height(t, zone, "title + R,N          ", new[] { Title, OptR, OptN });
                Height(t, zone, "title + M,N          ", new[] { Title, OptM, OptN });
                Height(t, zone, "title + N            ", new[] { Title, OptN });
                Height(t, zone, "M,R,N (no title)     ", new[] { OptM, OptR, OptN });
                Height(t, zone, "AS SHIPPED (2 lines) ", new[] { Title, optionsLine });

                // The line advance, so the DD can do their own arithmetic on any composition without
                // re-running this.
                float h1 = t.GetPreferredValues(Title, Unconstrained, 0f).y;
                float h2 = t.GetPreferredValues(Title + "\n" + Title, Unconstrained, 0f).y;
                float advance = h2 - h1;
                int rowsHeld = Mathf.FloorToInt((zone.height - h1) / advance) + 1;
                Debug.Log($"[T88] one row {h1:0.0}px · each further row +{advance:0.0}px · " +
                          $"zone height {zone.height:0.0}px carries {rowsHeld} rows at {t.fontSize:0.#}px");

                // The two figures that make the come-back actionable rather than a complaint. §6's
                // fixed grid binds — the zone does not resize to content — so the deficit is a number
                // the DD has to place somewhere, not one this seat may absorb.
                float worstList = h1 + 3f * advance;
                Debug.Log($"[T88] DEFICIT: title + three option rows needs {worstList:0.0}px in a " +
                          $"{zone.height:0.0}px zone — short by {worstList - zone.height:0.0}px. Three rows WITHOUT " +
                          $"the title fit with {zone.height - (h1 + 2f * advance):0.0}px spare, which is " +
                          $"{(zone.height - (h1 + 2f * advance)) / 2f:0.0}px per gap if the list is spaced at all.");
                // ---- the gesture's own rows, and the money control's third status state -----------
                Debug.Log("[T88] --- held preview composition (what a hold actually renders) ---");
                Row(zone.width, "confirm line     ", ConfirmLine, W(t, ConfirmLine));
                Height(t, zone, "preview M (held)     ", new[] { OptM, ConfirmLine });
                Height(t, zone, "preview R (held)     ", new[] { OptR, ConfirmLine });

                // §6.1's money control shares ONE rectangle between the figure and the status word,
                // anchored from opposite edges, and T88's gesture gives that word a third state. The
                // pair already collides (batch 44), so the question this answers is not whether it
                // fits — it does not — but by how much THIS PASS moves it. Reported rather than
                // absorbed: a ruling that makes a known-blocked box worse is still owed its number.
                TMP_Text fig = null, status = null;
                foreach (TMP_Text c in screen.GetComponentsInChildren<TMP_Text>(true))
                {
                    if (c.gameObject.name == "CashOut") fig = c;
                    else if (c.gameObject.name == "CashOutStatus") status = c;
                }
                if (fig != null && status != null)
                {
                    float box = fig.rectTransform.rect.width;
                    Debug.Log($"[T88] --- money control pair, box {box:0.0}px (figure + status, opposite edges) ---");
                    PairAt(fig, status, box, "CASH OUT $1,240", "HOLD E", "at rest");
                    PairAt(fig, status, box, "CASH OUT $1,240", "UPDATING", "mid-tween");
                    PairAt(fig, status, box, "CASHED OUT $1,240", StatusPreview, "HELD PREVIEW — new this pass");

                    // T84's Pair() asks for its preferred size at ZERO width, the call its own header
                    // documents as returning the widest GLYPH on a wrapping component. These two do
                    // not wrap, so it should agree with an unconstrained read — checked rather than
                    // assumed, because if it disagrees every paired figure in the sweep is suspect.
                    float z = fig.GetPreferredValues("CASH OUT $1,240", 0f, 0f).x;
                    float u = fig.GetPreferredValues("CASH OUT $1,240", Unconstrained, 0f).x;
                    Debug.Log($"[T88] CHECK T84 Pair()'s zero-width call {z:0.0}px vs unconstrained {u:0.0}px on '{fig.gameObject.name}' " +
                              $"(wrap {fig.textWrappingMode}) — {(Mathf.Abs(z - u) < 0.05f ? "AGREE: the sweep's pair figures stand" : "DISAGREE: every Pair() figure in the sweep is measured wrong")}");
                }

                // ---- batch 56: the money control's TWO ROWS, measured before they land ------------
                //
                // T74-am3 rules the figure and the status word onto separate rows, because sharing one
                // rectangle from opposite edges makes any pair of long strings collide — and
                // `CASHED OUT $1,240` overruns the box by 14.6 with no second member at all.
                //
                // The design constants say this fits and TMP may disagree: 29*LineBox(1.18) +
                // 15*1.18 = 51.9 in the 52.0 row, which clears by 0.08px on paper. But the prompt
                // measured 27.5px of advance at 22px type — a ratio of 1.25, not 1.18 — and on that
                // ratio the two rows need 55.0 and do NOT fit. Which is right is a measurement, and
                // the ruling asks for one before it lands.
                if (fig != null && status != null)
                {
                    const float GridRow = 52.0f;   // LayoutGrid.BottomRowHeight — the zone, not the rect
                    float hFig = fig.GetPreferredValues("CASHED OUT $1,240", Unconstrained, 0f).y;
                    float hStatus = status.GetPreferredValues(StatusPreview, Unconstrained, 0f).y;
                    float stacked = hFig + hStatus;
                    Debug.Log($"[T88] --- money control as TWO ROWS (T74-am3) ---");
                    Debug.Log($"[T88] figure row {hFig:0.0}px ({fig.fontSize:0.#}px type) + status row {hStatus:0.0}px " +
                              $"({status.fontSize:0.#}px type) = {stacked:0.0}px vs the {GridRow:0.0}px grid row  " +
                              $"{(stacked > GridRow ? $"OVERRUNS by {stacked - GridRow:0.0}px" : $"fits, {GridRow - stacked:0.0}px spare")}");

                    // On its own row each member has the FULL width instead of the other's leftovers.
                    float box = fig.rectTransform.rect.width;
                    foreach (string s in new[] { "CASHED OUT $1,240", "CASH OUT $1,240", "MARKET SUSPENDED" })
                        Row(box, "figure row      ", s, W(fig, s));
                    foreach (string s in new[] { StatusPreview, "UPDATING", "HOLD E" })
                        Row(box, "status row      ", s, W(status, s));
                }

                // ---- batch 57: RiskPays as two rows, measured BEFORE it is built --------------------
                // T74-am4 rules RISK and PAYS onto separate rows, label left and figure
                // right-anchored. The footer is 40.0px (LayoutGrid.TicketFooterHeight) and the type is
                // 24px, so the question is whether two rows of it fit at all — asked before four new
                // elements are built, because a composition that cannot land is not a build task.
                foreach (TMP_Text c in screen.GetComponentsInChildren<TMP_Text>(true))
                    if (c.gameObject.name == "RiskPays")
                    {
                        const float Footer = 40.0f;   // LayoutGrid.TicketFooterHeight
                        float one = c.GetPreferredValues("RISK $1,234", Unconstrained, 0f).y;
                        Debug.Log($"[T88] --- RiskPays as TWO ROWS (T74-am4) --- one row {one:0.0}px " +
                                  $"({c.fontSize:0.#}px type) · two rows {2f * one:0.0}px vs the {Footer:0.0}px footer  " +
                                  $"{(2f * one > Footer ? $"OVERRUNS by {2f * one - Footer:0.0}px" : $"fits, {Footer - 2f * one:0.0}px spare")}");
                        Row(c.rectTransform.rect.width, "risk row        ", "RISK $1,234", W(c, "RISK $1,234"));
                        Row(c.rectTransform.rect.width, "pays row        ", "PAYS $12,340", W(c, "PAYS $12,340"));
                        break;
                    }

                // ---- batch 56: `SHOT FROZEN` leaves the zone; the event strip is its optional home --
                // "Not asserted to fit: measured before it lands." The strip carries ONE authored line
                // at a time, so the question is not whether it fits BESIDE the flavour line but
                // whether it fits AS one.
                foreach (TMP_Text c in screen.GetComponentsInChildren<TMP_Text>(true))
                    if (c.gameObject.name == "Flavor")
                    {
                        Row(c.rectTransform.rect.width, "event strip     ", Title, W(c, Title));
                        break;
                    }

                Debug.Log($"[T88] AFFORDANCE BUDGET (in-row, per the ruling's 'somewhere for a hold affordance to live'): " +
                          $"M {zone.width - wM:0.0}px · R {zone.width - wR:0.0}px · N {zone.width - wN:0.0}px spare on a {zone.width:0.0}px row. " +
                          $"R is the binding one. No copy is proposed here — the strings are the DD's.");
            }
            finally { Object.DestroyImmediate(go); }
        }

        private static float W(TMP_Text t, string s) => t.GetPreferredValues(s, Unconstrained, 0f).x;

        /// <summary>Two slots anchored from opposite edges of one rectangle, in a named state. Neither
        /// has to overrun on its own for the pair to overprint, which is the shape a single-slot sweep
        /// cannot see.</summary>
        private static void PairAt(TMP_Text a, TMP_Text b, float box, string aText, string bText, string state)
        {
            float aw = W(a, aText), bw = W(b, bText);
            float slack = box - (aw + bw);
            Debug.Log($"[T88] pair {state,-28} '{aText}' {aw:0.0} + '{bText}' {bw:0.0} = {aw + bw:0.0}px  " +
                      $"{(slack < 0f ? $"COLLIDES by {-slack:0.0}px" : $"clears by {slack:0.0}px")}");
        }

        private static void Row(float box, string label, string s, float w)
        {
            Debug.Log($"[T88] {label} '{Show(s)}' {w,7:0.0}px  " +
                      $"{(w > box ? $"OVERRUNS {box:0.0} by {w - box:0.0}px" : $"fits, {box - w:0.0}px spare")}");
        }

        /// <summary>A candidate composition's height, measured as the authored rows joined by
        /// newlines. Measured unconstrained on purpose: the rows are broken where the composition
        /// breaks them, so the figure is the one the layout would produce and not one the measurer
        /// introduced by wrapping. Whether any single row then overruns the box is the width question
        /// above, and is reported there.</summary>
        private static void Height(TMP_Text t, Rect zone, string label, string[] rows)
        {
            string s = string.Join("\n", rows);
            Vector2 v = t.GetPreferredValues(s, Unconstrained, 0f);
            float widest = 0f;
            foreach (string r in rows) widest = Mathf.Max(widest, W(t, r));
            Debug.Log($"[T88] {label}  {rows.Length} rows  h {v.y,6:0.0}px vs zone {zone.height:0.0}  " +
                      $"{(v.y > zone.height ? $"OVERRUNS by {v.y - zone.height:0.0}px" : $"fits, {zone.height - v.y:0.0}px spare")}  " +
                      $"· widest row {widest:0.0}px " +
                      $"{(widest > zone.width ? $"OVERRUNS width by {widest - zone.width:0.0}px" : $"fits width, {zone.width - widest:0.0}px spare")}");
        }

        /// <summary>The atoms above are retyped copy, and retyped copy is exactly what this lane has
        /// been wrong about three times. This proves each one still occurs verbatim in the file that
        /// authors it, with comment lines stripped so the check cannot be satisfied by a comment
        /// ABOUT the string — the shape batch 16 recorded. Whole file, no character window: a fixed
        /// scan window has silently stopped covering its target twice here.</summary>
        private static bool AssertStringSetIsTheShippedOne()
        {
            string path = Path.Combine(Application.dataPath, SourceRel);
            if (!File.Exists(path))
            {
                Debug.Log($"[T88] REFUSING: cannot read {path} to verify the string set");
                return false;
            }

            var code = new StringBuilder();
            foreach (string line in File.ReadAllLines(path))
            {
                string tr = line.TrimStart();
                if (tr.StartsWith("//") || tr.StartsWith("*") || tr.StartsWith("/*")) continue;
                code.Append(line).Append('\n');
            }
            string src = code.ToString();

            // The list composition dropped the separators from the option rows (DD batch 50), so the
            // options are asserted bare. The two gesture strings are asserted on the half that does
            // not carry the confirm key, since that key is unratified and may move.
            // `SHOT FROZEN` is NOT asserted: batch 56 took it out of the zone, so it is no longer a
            // shipped string. It stays in this file only as the CANDIDATE measured against the event
            // strip, which is the DD's optional home for it — a proposal, not a slot's content.
            var required = new List<string>
            {
                OptM, OptR, OptN,
                " CONFIRMS" + Sep + "RELEASE ABANDONS",
                " TO CASH OUT",
            };
            bool ok = true;
            foreach (string s in required)
            {
                bool found = src.Contains(s);
                if (!found) ok = false;
                Debug.Log($"[T88] string-set {(found ? "OK     " : "MISSING")} '{Show(s)}'");
            }

            if (!ok)
                Debug.Log($"[T88] REFUSING to measure: the prompt's copy in {SourceRel} is not what this " +
                          "probe was written against. Re-enumerate from the assignment site, then re-run.");
            return ok;
        }

        /// <summary>Newlines and the separator's spaces made visible, so a log line reads as one row
        /// of a table. T84 learned this when the prompt's own newline fractured its row across three
        /// log lines with the verdict on the third.</summary>
        private static string Show(string s) => s.Replace("\n", "\\n").Replace("\u00B7", "(mid)");
    }
}
