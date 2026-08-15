using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace SBR.Tests.PlayMode
{
    public class SureThingEntryTests
    {
        [UnityTest, Order(1)]
        public IEnumerator Entry_keeps_its_fixed_shell_while_each_named_destination_replaces_only_market_body()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // Carry one staged ticket and one working mark through every destination. Persistence
            // is a user-visible state/content law, not a Unity object-identity law.
            Button stagedOffer = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Invoke(stagedOffer.transform);
            yield return WaitForRebuild();
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();
            Button workingOffer = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Invoke(workingOffer.transform);
            yield return WaitForRebuild();

            Transform app = App(laptop);
            AssertEntryShell(app);
            EntryPersistenceSnapshot persistent = CaptureEntryPersistence(laptop);
            string previousBodyContent = AllText(Required(app, "MarketBody"));

            string[] destinationNames =
            {
                "DetailTabBTTS",
                "DetailTabCORNERS",
                "DetailTabCARDS",
                "DetailTabPLAYERS",
                "DetailTabGOALS",
            };
            // A2 ruling: the per-destination panel title ("BOTH TEAMS TO SCORE" etc.) is deleted —
            // each row now names its own market, so there is no longer a fixed title node to pin
            // per destination. The body-content diff below is what actually proves the destination
            // switched.
            for (int i = 0; i < destinationNames.Length; i++)
            {
                Transform destinations = Required(App(laptop), "MarketDestinations");
                Assert.IsNotNull(Required(destinations, destinationNames[i]).GetComponent<Button>(),
                    $"{destinationNames[i]} must be an independently named destination");
                Invoke(Required(destinations, destinationNames[i]));
                yield return WaitForRebuild();

                app = App(laptop);
                AssertEntryShell(app);
                AssertEntryPersistence(laptop, persistent, destinationNames[i]);

                Transform body = Required(app, "MarketBody");
                string currentBodyContent = AllText(body);
                Assert.AreNotEqual(previousBodyContent, currentBodyContent,
                    $"{destinationNames[i]} must change the displayed MarketBody destination/content");
                previousBodyContent = currentBodyContent;
            }
        }

        [UnityTest, Order(2)]
        public IEnumerator Entry_replacement_stays_selectable_and_wide_ink_is_deterministic_across_rebuilds()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            Button firstOffer = FirstNamedButton(Required(App(laptop), "MarketBody"), "Market");
            Invoke(firstOffer.transform);
            yield return WaitForRebuild();

            MarketSelection? original = laptop.Slip.SelectionOn(0);
            Assert.IsTrue(original.HasValue, "the ENTRY offer must write through to BetslipModel");
            Assert.AreEqual(1, laptop.Slip.Picks.Count);

            Image firstRing = WideRing(App(laptop));
            Assert.IsNotNull(firstRing.sprite, "WideBiroRing sprite missing");
            StringAssert.StartsWith("ring-wide-", firstRing.sprite.name,
                "wide selection ink must be prefix-filtered from the wide family");
            Assert.IsFalse(firstRing.raycastTarget, "decorative wide ink must not intercept the price");
            // The rule is cell + 16 on both axes (ASSETS.md, and the design system's InkMark.rect):
            // A1 widened the market cell from 160 to 176 wide (32 tall, unchanged), so the ring is
            // 192x48.
            AssertRect(firstRing.rectTransform, 192f, 48f, "WideBiroRing");
            string variant = firstRing.sprite.name;

            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabBTTS"));
            yield return WaitForRebuild();
            Transform bttsBody = Required(App(laptop), "MarketBody");
            Button replacement = FirstNamedButton(bttsBody, "MarketBothTeamsToScore");
            Assert.IsTrue(replacement.interactable, "same-match replacement must remain selectable");
            StringAssert.Contains("⇄", TextOf(replacement.transform));
            Assert.IsNotNull(FindPrefix(bttsBody, "ReplacementUnderline"),
                "replacement offer needs its named underline");

            Invoke(replacement.transform);
            yield return WaitForRebuild();
            MarketSelection? replaced = laptop.Slip.SelectionOn(0);
            Assert.IsTrue(replaced.HasValue);
            Assert.AreNotEqual(original.Value, replaced.Value);
            Assert.AreEqual(1, laptop.Slip.Picks.Count,
                "replacement must retain the one-leg-per-matchup invariant");
            Assert.AreEqual(variant, WideRing(App(laptop)).sprite.name,
                "same matchup must retain its deterministic wide-ink variant");

            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Chip25%"));
            yield return WaitForRebuild();
            Image rebuilt = WideRing(App(laptop));
            Assert.AreEqual(variant, rebuilt.sprite.name,
                "stake rebuild must retain the deterministic wide-ink variant");
            Assert.IsFalse(rebuilt.raycastTarget);
        }

        [UnityTest, Order(3)]
        public IEnumerator Working_margin_renders_every_staged_receipt_and_lock_tracks_only_current_marks()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            var expected = new List<ReceiptExpectation>();

            Invoke(Required(Required(App(laptop), "Matchup0"), "AwayOdds"));
            yield return WaitForRebuild();
            Invoke(Required(Required(App(laptop), "Matchup1"), "HomeOdds"));
            yield return WaitForRebuild();
            expected.Add(Capture(laptop.Slip));
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();

            Invoke(Required(Required(App(laptop), "Matchup2"), "AwayOdds"));
            yield return WaitForRebuild();
            expected.Add(Capture(laptop.Slip));
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();

            Assert.AreEqual(expected.Count, run.Tickets.Count,
                "every placed model ticket must be staged by the engine");
            Assert.AreEqual(0, laptop.Slip.Picks.Count, "PLACE must clear the working marks");
            AssertReceipts(App(laptop), run, expected);

            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.IsTrue(Required(margin, "Lock").GetComponent<Button>().interactable,
                "staged receipts with no working marks must enable LOCK");
            Assert.IsNull(Find(margin, "LockReason"));

            Invoke(Required(Required(App(laptop), "Matchup0"), "HomeOdds"));
            yield return WaitForRebuild();
            Assert.AreEqual(1, laptop.Slip.Picks.Count);
            margin = Required(App(laptop), "WorkingMargin");
            AssertReceipts(App(laptop), run, expected);
            Assert.IsFalse(Required(margin, "Lock").GetComponent<Button>().interactable,
                "a new unplaced mark must disable LOCK without erasing receipts");
            Assert.AreEqual("PLACE OR CLEAR THIS WORKING SLIP",
                TextOf(Required(margin, "LockReason")));
        }

        /// <summary>S71, closed by measurement rather than by eye. The ruling was granted
        /// qualitatively on `03-staged-receipt-lock-enabled` — the frame showed one speaker where
        /// there had been two — and NOTHING gated it. A re-authored empty state could put the second
        /// speaker back with every suite green, which is this surface's most-repeated failure shape.
        ///
        /// Asserted BY TOKEN, not by weight. C33b rules that a ranking is asserted by weight only
        /// among neutrals: Muted (0x6E6B5E) is neutral and Accent (0x5E86B8) is biro — chromatic —
        /// so comparing their luminance would measure the wrong axis. Compared as Color32, which is
        /// the space both tokens are authored in (C33-am3: state the space, not only the unit), so
        /// the check cannot drift on float conversion.
        ///
        /// BOTH inks are read, not just the empty line's, because the claim S71 rests on is that
        /// ownership is carried by the HEADER. An empty line that stopped restating ownership while
        /// the header quietly stopped asserting it would satisfy half the ruling and lose the fact.
        ///
        /// Scope (C25): this reads the authored ink at the component. It is the token channel, not a
        /// rendered pixel — the frame stays the authority for how it READS, and this gate only holds
        /// the tokens the frame was granted on.
        ///
        /// DO NOT PROPOSE CLOSING THAT RESIDUE HERE. Batch 24 refused a pixel close for S71, and not
        /// on cost: the residue is *does an authored token survive the rendering path*, which is
        /// surface-wide rather than this element's. Three layers already cover it — this gate catches
        /// ink ABSENT (the header silently ceasing to be biro, which nothing else sees), L3 catches
        /// ink MISUSED on a rendered frame, and the DD read the composed pair on Allen's frame.
        /// Answering it per item means every item eventually demands its own capture window. If the
        /// path is ever suspected, that is a surface-wide investigation: enumerate the path, not the
        /// elements on it (C39).</summary>
        [UnityTest, Order(10)]
        public IEnumerator Margin_empty_state_names_the_state_and_leaves_ownership_to_the_header()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            Assert.AreEqual(0, laptop.Slip.Picks.Count,
                "this gate reads the EMPTY state, so a freshly booted entry screen must carry no " +
                "working marks — if this ever fails the test is measuring the wrong state");

            Transform margin = Required(App(laptop), "WorkingMargin");
            Transform empty = Required(margin, "Empty");
            Transform title = Required(margin, "Title");
            // S71-am3 (batch 28) added the kit's imperative beside the state line. Required() rather
            // than Find(): the pair is the ruling, so a vanished remedy is a failure and not an
            // absence to tolerate. Fetching it is also what lets the guard below reach it — new copy
            // in this slot arriving outside the gate's sight is exactly what S72 was opened about.
            Transform remedy = Required(margin, "EmptyRemedy");

            Assert.AreEqual("NO MARKS ON THIS SHEET", TextOf(empty),
                "S71: the empty state names the STATE. It read 'YOUR MARGIN IS CLEAR' — a second " +
                "speaker addressing the player, three lines under 'MY MARKS', which is him.");
            // (d) GRANTED, WIDENED BY ONE STEP (DD, batch 24). This guarded the single substring
            // "YOUR", which `YOU HAVE NO MARKS` and `NO MARKS FOR YOU` both pass while violating
            // Voice §6 — most of the assertion's value gone. It now guards the second-person
            // PRONOUN CLASS as standalone words, enumerated so the check names its members.
            //
            // RECORDED SO IT IS NOT MISTAKEN FOR THE RULING, in the DD's own terms: THIS IS A PROXY.
            // S71 forbids a second SPEAKER; second person is its most common symptom, not its
            // definition. A pronoun guard is the practical instrument and it is not the law. Copy
            // that addressed the player with no pronoun at all would pass this and still violate
            // S71 — and so would YOURSELF, which sits outside the ruled class and is deliberately
            // NOT added here rather than quietly widened past what was granted.
            //
            // Split on runs of non-letters instead of matching word boundaries directly: it makes
            // YOU'RE and YOU’RE behave identically, both reducing to the token YOU. A typographic
            // apostrophe is exactly the character a re-authoring introduces without anyone noticing.
            string[] secondPerson = { "YOU", "YOUR", "YOURS" };

            // ONE implementation, called twice — once by the control below and once on the live
            // string. It returns the offending pronoun rather than a bool so a failure names what it
            // found. **The control must exercise the same code as the check**: a control that
            // re-implements the matching tests the copy and not the instrument, which is this
            // surface's most-repeated defect (S33, S34, S60, the MY BETS mirror identity — four
            // times, always a second site hand-building what a shared one already did).
            Func<string, string> offendingPronoun = s =>
            {
                foreach (string word in Regex.Split(s.ToUpperInvariant(), "[^A-Z]+"))
                    if (Array.IndexOf(secondPerson, word) >= 0) return word;
                return null;
            };

            // NEGATIVE CONTROL, and it runs BEFORE the real assertion on purpose.
            //
            // `NO MARKS ON THIS SHEET` contains no pronoun, so the check passing proves only that it
            // does not false-positive. If the tokenising were broken the gate would still go green —
            // the vacuous shape, and the first of the two instrument laws this lane's own hunt put in
            // the constitution says a control must be able to witness the failure it guards. So it is
            // demonstrated here rather than reasoned about in a comment.
            Assert.AreEqual("YOUR", offendingPronoun("YOUR MARGIN IS CLEAR"),
                "the guard must fire on the exact string S71 removed");
            Assert.AreEqual("YOU", offendingPronoun("YOU HAVE NO MARKS"),
                "the guard must fire on the case that defeated the old substring check");
            Assert.AreEqual("YOU", offendingPronoun("YOU’RE CLEAR"),
                "a TYPOGRAPHIC apostrophe must behave like a plain one — this is why the split is on " +
                "runs of non-letters rather than on word boundaries, and it is the character a " +
                "re-authoring introduces without anyone noticing");
            Assert.IsNull(offendingPronoun("NOTHING ON YOUTH POLICY"),
                "and it must NOT fire on a substring: YOUTH contains YOU and is not second person. " +
                "Without this line the widening could silently collapse back into a substring match, " +
                "which is the defect it was granted to fix");

            // The live check, through the same function the control just proved fires — over BOTH
            // strings in the slot, not just the statement. S71-am3 put a second string here, and a
            // guard that reads only the first would have let new copy in behind a green run, which
            // is the shape S72 was opened to find. The offending slot is named in the failure so the
            // message points at which of the two, rather than at the pair.
            foreach (Transform slot in new[] { empty, remedy })
                Assert.IsNull(offendingPronoun(TextOf(slot)),
                    $"Voice §6 puts second person in genuine imperatives only. Found " +
                    $"'{offendingPronoun(TextOf(slot))}' in \"{TextOf(slot)}\" ({slot.name}). The " +
                    $"string assertion above pins today's words; this one pins the ruling. Note the " +
                    $"remedy IS an imperative, which §6 permits — what it may not do is address him.");

            Assert.AreEqual((Color32)LaptopOs.Muted, (Color32)empty.GetComponent<TMP_Text>().color,
                "the empty state reports the sheet's condition, so it takes the neutral toner " +
                "rather than the ink that means 'what he chose'");
            Assert.AreEqual((Color32)LaptopOs.Accent, (Color32)title.GetComponent<TMP_Text>().color,
                "MY MARKS stays biro. S71 leaves ownership to this header, so if the header ever " +
                "stops being the player's ink then nothing on the column carries ownership at all");
            // (c) GRANTED AS THE STRENGTHENING, NOT THE DELETION (DD, batch 24). What stood here was
            // AreNotEqual(title, empty), which CANNOT FAIL unless one of the two assertions above it
            // already has: Muted and Accent are distinct constants, so equality with each implies
            // inequality with the other. The gate looked like three ink checks and was two.
            //
            // The claim actually meant is that the empty line does not wear the PLAYER'S ink —
            // "biro is only what the player chose". Enumerated rather than counted, which is C18 §4.1
            // carried by construction: the check has to name the player-ink tokens to make the claim.
            //
            // BE PRECISE ABOUT THE INDEPENDENCE, because overstating it would repeat the defect this
            // replaces. While the assertion above pins the empty line to Muted, this one cannot fail
            // either — Muted is in neither set. What it survives is that assertion being RE-RULED: if
            // the empty line is ever moved to another token, this still forbids the player's ink,
            // where the old AreNotEqual would have passed a margin whose empty line went BiroDeep
            // beside an Accent header. It is independent of the ruling's future, not of today's
            // constants, and that is the whole of its value.
            Color32[] playerInk = { (Color32)LaptopOs.Accent, (Color32)LaptopOs.BiroDeep };
            CollectionAssert.DoesNotContain(playerInk, (Color32)empty.GetComponent<TMP_Text>().color,
                "one voice per column: the header owns it and the empty line reports it. An empty " +
                "line drawn in biro is the composition S71 was ruled against, arriving by a " +
                "different route than the words did.");
        }

        [UnityTest, Order(4)]
        public IEnumerator Market_offer_rows_stay_within_the_market_viewport_horizontally_on_every_destination()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // A4/S27: the list now scrolls, so a row is expected to sit below the visible viewport
            // once content overflows — that is exactly what RectMask2D/ScrollRect exist to clip, so
            // vertical containment is no longer the invariant (see the S27 rail test for the
            // overflow/clip behaviour itself). What must always hold, on every destination, whether
            // it scrolls or not, is horizontal containment against the viewport — A4 requires
            // content to stay clear of the S27 rail, so a row (and its price cell) may never run
            // wider than the viewport it is masked by.
            string[] destinationNames =
            {
                "DetailTabBTTS",
                "DetailTabCORNERS",
                "DetailTabCARDS",
                "DetailTabPLAYERS",
                "DetailTabGOALS",
            };

            foreach (string destinationName in destinationNames)
            {
                Invoke(Required(Required(App(laptop), "MarketDestinations"), destinationName));
                yield return WaitForRebuild();

                Transform bodyTransform = Required(App(laptop), "MarketBody");
                RectTransform viewport = Required(bodyTransform, "MarketViewport") as RectTransform;
                Assert.IsNotNull(viewport, $"{destinationName} MarketViewport must be a RectTransform");

                List<Transform> rows = AllNamed(bodyTransform, "MarketOffer");
                foreach (Transform row in rows)
                    AssertWithinContainerHorizontally(viewport, row as RectTransform, $"{destinationName} {row.name}");
                Assert.Greater(rows.Count, 0,
                    $"{destinationName} must render at least one market offer row for this invariant to mean anything");
            }
        }

        [UnityTest, Order(5)]
        public IEnumerator Every_destination_renders_a_single_column_of_offer_rows()
        {
            // S25 amended / A1: the fixed-body two-up ladder layout is withdrawn. Every destination
            // — including the ladders, which used to legitimately stay two-column — now renders one
            // offer per row, full body width. Inverts the old two-column-ladder assertion this
            // replaced.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            string[] destinationNames =
            {
                "DetailTabGOALS",
                "DetailTabBTTS",
                "DetailTabCORNERS",
                "DetailTabCARDS",
                "DetailTabPLAYERS",
            };

            foreach (string destinationName in destinationNames)
            {
                Invoke(Required(Required(App(laptop), "MarketDestinations"), destinationName));
                yield return WaitForRebuild();

                List<float> rowX = OfferRowX(Required(App(laptop), "MarketBody"));
                Assert.Greater(rowX.Count, 0,
                    $"{destinationName} must render at least one offer for a single-column claim to mean anything");
                var distinctX = new HashSet<float>();
                foreach (float x in rowX) distinctX.Add(Mathf.Round(x * 10f) / 10f);
                Assert.AreEqual(1, distinctX.Count,
                    $"A1: every {destinationName} offer row must share the same column (row x position)");
            }
        }

        [UnityTest, Order(6)]
        public IEnumerator Every_engine_priced_offer_is_reachable_on_every_destination_C19()
        {
            // C19 (law): "an offer the engine prices is reachable on the surface." For every
            // destination, the number of rendered market-offer rows must equal the number of
            // offers the engine actually priced for that destination on this matchup — derived
            // from the engine (matchup.Markets filtered by kind), never a hardcoded number, so a
            // silently hidden offer fails here.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);
            Run run = laptop.director.Run;
            Matchup matchup = run.CurrentSlate.Matchups[0];

            (string destination, MarketKind kind)[] destinations =
            {
                ("DetailTabGOALS", MarketKind.TotalGoals),
                ("DetailTabBTTS", MarketKind.BothTeamsToScore),
                ("DetailTabCORNERS", MarketKind.TotalCorners),
                ("DetailTabCARDS", MarketKind.TotalCards),
                ("DetailTabPLAYERS", MarketKind.AnytimeScorer),
            };

            foreach ((string destinationName, MarketKind kind) in destinations)
            {
                Invoke(Required(Required(App(laptop), "MarketDestinations"), destinationName));
                yield return WaitForRebuild();

                int expected = 0;
                foreach (MarketOffer offer in matchup.Markets)
                    if (offer.Selection.Kind == kind) expected++;
                Assert.Greater(expected, 0,
                    $"{destinationName} must have at least one engine-priced offer for C19 to mean anything");

                int rendered = AllNamed(Required(App(laptop), "MarketBody"), "MarketOffer").Count;
                Assert.AreEqual(expected, rendered,
                    $"C19: {destinationName} must render exactly the offers the engine priced ({expected}) — none hidden");
            }

            // Ladder counts must also match the run's own line configuration, not just an internal
            // engine-list/render-count agreement, so a config change is caught too.
            Assert.AreEqual(run.Config.GoalLines.Length * 2,
                CountByKind(matchup, MarketKind.TotalGoals), "GOALS offer count must track GoalLines");
            Assert.AreEqual(run.Config.CornerLines.Length * 2,
                CountByKind(matchup, MarketKind.TotalCorners), "CORNERS offer count must track CornerLines");
            Assert.AreEqual(run.Config.CardLines.Length * 2,
                CountByKind(matchup, MarketKind.TotalCards), "CARDS offer count must track CardLines");
        }

        [UnityTest, Order(7)]
        public IEnumerator S27_position_rail_appears_only_when_the_list_overflows()
        {
            // S27 ruling: a scrolling interior list carries a printed position rail — exactly two
            // images, present only when the content overflows the viewport, absent when it fits;
            // the thumb is clamped to a 24px floor, never taller than the track, and always lies
            // within the track.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            yield return OpenEntry(laptop);

            // PLAYERS: S25 amended removed the capacity cap, and the shipped roster (PlayersPerTeam
            // per side, both teams) comfortably exceeds the ~7-row viewport at 54px/row, so PLAYERS
            // overflows and must carry the rail.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabPLAYERS"));
            yield return WaitForRebuild();
            Transform overflowingBody = Required(App(laptop), "MarketBody");
            var trackRect = Required(overflowingBody, "PositionRailTrack") as RectTransform;
            var thumbRect = Required(overflowingBody, "PositionRailThumb") as RectTransform;
            Assert.IsNotNull(trackRect, "S27 track must be a RectTransform");
            Assert.IsNotNull(thumbRect, "S27 thumb must be a RectTransform");
            Assert.GreaterOrEqual(thumbRect.sizeDelta.y, 24f, "S27 thumb must never be shorter than its 24px floor");
            Assert.LessOrEqual(thumbRect.sizeDelta.y, trackRect.sizeDelta.y, "S27 thumb must never exceed the track");

            var trackCorners = new Vector3[4];
            var thumbCorners = new Vector3[4];
            trackRect.GetWorldCorners(trackCorners);
            thumbRect.GetWorldCorners(thumbCorners);
            const float epsilon = 0.5f;
            Assert.GreaterOrEqual(thumbCorners[0].y, trackCorners[0].y - epsilon,
                "S27 thumb bottom must lie within the track");
            Assert.LessOrEqual(thumbCorners[2].y, trackCorners[2].y + epsilon,
                "S27 thumb top must lie within the track");

            // BTTS is always exactly 2 rows (108px of content against a 412px viewport) — it never
            // overflows, so the rail must be entirely absent.
            Invoke(Required(Required(App(laptop), "MarketDestinations"), "DetailTabBTTS"));
            yield return WaitForRebuild();
            Transform fittingBody = Required(App(laptop), "MarketBody");
            Assert.IsNull(Find(fittingBody, "PositionRailTrack"), "S27 rail track must be absent when the list fits");
            Assert.IsNull(Find(fittingBody, "PositionRailThumb"), "S27 rail thumb must be absent when the list fits");
        }

        private static int CountByKind(Matchup matchup, MarketKind kind)
        {
            int count = 0;
            foreach (MarketOffer offer in matchup.Markets)
                if (offer.Selection.Kind == kind) count++;
            return count;
        }

        /// <summary>P3's stamp, end to end, on a REAL refused same-match slip — and the C46 fit
        /// measurement the DD asked for in numbers rather than in estimate.
        ///
        /// <para>The model returns the machine token <c>"refused:&lt;Kind&gt;"</c> from PlaceBlocker
        /// precisely so that printing it is loud, and the surface printed it verbatim until P3. The
        /// first assertion here is that the token never reaches the control.</para></summary>
        [UnityTest, Order(9)]
        public IEnumerator Refused_combination_stamps_cause_and_remedy_and_never_the_models_token()
        {
            // The hold is RELEASED (S77 answered the sizing question by changing the stamp's shape),
            // so this gate no longer drives a flag — it asserts the wiring is live.
            Assert.IsTrue(SportsbookApp.StampComposedRefusal,
                "the refusal stamp's hold was released when S77 landed");
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            BetslipModel slip = laptop.Slip;

            // Build a genuinely refused same-match slip through the model's own additive API.
            // Searched rather than hard-coded: which pair conflicts is a property of the board, and
            // the board is re-priced on every boot (RunDirector.seed is blank).
            bool found = false;
            Matchup matchup = run.CurrentSlate.Matchups[0];
            var offers = new List<MarketSelection>();
            foreach (MarketOffer offer in matchup.Markets) offers.Add(offer.Selection);
            for (int a = 0; a < offers.Count && !found; a++)
                for (int b = a + 1; b < offers.Count && !found; b++)
                {
                    slip.Clear();
                    if (!slip.AddLeg(0, offers[a])) continue;
                    if (!slip.AddLeg(0, offers[b])) continue;
                    if (slip.Refusal != null) found = true;
                }
            Assert.IsTrue(found,
                "no two selections on matchup 0 refuse — this gate needs a real refusal to measure, "
                + "and a board that cannot produce one has changed in a way P3 depends on");

            yield return WaitForRebuild();
            Transform margin = Required(App(laptop), "WorkingMargin");
            // Two nodes, not one wrapping node (S77 option 2): cause above, remedy below, so the
            // break lands between them by construction rather than wherever a fitter puts it.
            var reason = Required(Required(margin, "Place"), "PlaceReason").GetComponent<TMP_Text>();
            var remedyLine = Required(Required(margin, "Place"), "PlaceRemedy").GetComponent<TMP_Text>();
            string stamp = reason.text + " " + remedyLine.text;

            Assert.IsFalse(stamp.Contains("REFUSED:"),
                $"the model's machine token reached the control: \"{stamp}\". PlaceBlocker returns "
                + "\"refused:<Kind>\" so this is loud rather than a plausible sentence the model had "
                + "no authority to write — the surface must branch on Refusal and stamp the parts");

            // S73-am5's banned list, checked against the stamp's own CONNECTIVES rather than against
            // the raw string. The distinction is not pedantic: the draws vocabulary names a double
            // chance "TUSCALOOSA LONGHAULERS OR DRAW", so a leg NAME can contain "OR" without the
            // remedy being disjunctive. Testing the raw string would go red on whichever boot puts a
            // double chance in a remedy — a flake that reads as a copy violation.
            string skeleton = stamp;
            foreach (Pick pick in slip.Picks)
                skeleton = skeleton.Replace(
                    SportsbookApp.MarginLegSubject(run.CurrentSlate.Matchups[pick.MatchupIndex],
                        pick.Selection).ToUpperInvariant(), "<LEG>");
            foreach (string banned in new[] { " OR ", "EITHER", "ONE OF", "ANY OF" })
                Assert.IsFalse(skeleton.Contains(banned),
                    $"\"{banned.Trim()}\" is banned in a remedy — the remedy is a SET to remove, and "
                    + $"menu-shaped copy fails when followed. Stamp: \"{stamp}\"");

            // Cause AND remedy, both — §3.3's row has always required both halves.
            // The remedy's verb is the surface's own: the control on each row says RUB OUT, so the
            // instruction and the thing that performs it are the same word (S77's no-translation
            // goal, one step further than matching strings).
            Assert.IsTrue(stamp.Contains("RUB OUT") || stamp.Contains("NO LEG CAN BE RUBBED OUT"),
                $"the stamp states no remedy: \"{stamp}\"");

            // S77: NO LEG IS NAMED IN THE STAMP. The stamp states the act and its arity; the legs it
            // refers to are MARKED on their own rows. So the assertion inverts — a name appearing
            // here is the defect now, not its absence.
            TicketRefusal refusal = slip.Refusal;
            for (int i = 0; i < slip.Picks.Count; i++)
            {
                string name = SportsbookApp.MarginLegSubject(
                    run.CurrentSlate.Matchups[slip.Picks[i].MatchupIndex],
                    slip.Picks[i].Selection).ToUpperInvariant();
                if (name.Length < 3) continue;   // a name too short to distinguish from a stray word
                Assert.IsFalse(stamp.Contains(name),
                    $"leg {i} (\"{name}\") is NAMED in the stamp: \"{stamp}\". S77 puts the names in "
                    + "the flow as marks — three names in a 296x44 control is unbounded and the "
                    + "instruction is not");
            }

            // ...and the whole remedy set IS marked, in the house's ink, on the control that performs
            // the act the stamp names. A surface that marks RemedyLegs[0] alone leaves the slip
            // refused after the mark is spent.
            Transform marginNow = Required(App(laptop), "WorkingMargin");
            foreach (int legIndex in refusal.RemedyLegs)
            {
                var label = Required(Required(marginNow, "Remove" + legIndex), "Label")
                    .GetComponent<TMP_Text>();
                Assert.AreEqual(LaptopOs.MoneyBad, label.color,
                    $"remedy leg {legIndex} is not marked — the stamp points at marks it did not "
                    + "make, which is worse than naming them");
            }
            for (int i = 0; i < slip.Picks.Count; i++)
            {
                if (refusal.RemedyLegs.Contains(i)) continue;
                var label = Required(Required(marginNow, "Remove" + i), "Label").GetComponent<TMP_Text>();
                Assert.AreNotEqual(LaptopOs.MoneyBad, label.color,
                    $"leg {i} is marked but is not in the remedy — the arity in the stamp would then "
                    + "disagree with the marks he can count");
            }

            // ---- C46 FIT — NOW AN ASSERTION, because S77 made the population FINITE.
            //
            // The first build measured leg-name-bearing compositions and could only report: the
            // worst case was 1583-1722px against a 288px box, six lines, and unbounded in principle
            // because it scaled with whatever the board named a team. Taking the names out collapses
            // it to arity-keyed forms, and MaxLegs = 4 bounds the arity — so the whole population is
            // enumerable here, exactly, and every member of it can be required to fit.
            //
            // This is the difference S77 bought, and it is why the gate stopped being a report.
            float box = ((RectTransform)reason.transform).rect.width;
            float worst = 0f;
            string worstText = "";
            foreach (RefusalKind kind in Enum.GetValues(typeof(RefusalKind)))
                for (int causeArity = 2; causeArity <= run.Config.MaxLegs; causeArity++)
                    for (int remedyArity = 0; remedyArity < run.Config.MaxLegs; remedyArity++)
                    {
                        var probe = new TicketRefusal(kind, Enumerable.Range(0, causeArity).ToArray(),
                            Enumerable.Range(0, remedyArity).ToArray(), null, 0.0);
                        foreach (string line in new[]
                                 { SportsbookApp.RefusalCause(probe), SportsbookApp.RefusalRemedy(probe) })
                        {
                            float w = LaptopUi.MeasureWidth(reason.font, line.ToUpperInvariant(), 13,
                                LaptopTrack.StampReason);
                            if (w > worst) { worst = w; worstText = line; }
                        }
                    }

            UnityEngine.Debug.Log($"[P3-FIT] control {box:F1}px/line · widest of the whole authored "
                + $"population {worst:F1}px ({worst / box:P0}) — \"{worstText}\" · this refusal: "
                + $"\"{reason.text}\" / \"{remedyLine.text}\"");

            Assert.Less(worst, box,
                $"the widest authored line does not fit: \"{worstText}\" at {worst:F1}px in a "
                + $"{box:F1}px box. S77's order is (1) a shorter authored form, (2) two lines inside "
                + "the existing 44px box — already taken — and (3) only then geometry, which goes to "
                + "Allen with the flow-budget cost stated. Never truncation, and never smaller type");
            Assert.Greater(reason.fontSize, 12.9f, "the cause line holds the >=13px floor");
            Assert.Greater(remedyLine.fontSize, 12.9f, "the remedy line holds it too");
            Assert.IsFalse(stamp.Contains("…"),
                $"the stamp was ellipsised: \"{stamp}\". A truncated remedy is an unverified remedy");
        }

        /// <summary>S74's middle position, measured rather than asserted. The draw's line sits
        /// physically between the two teams' — so the gap above it and the gap below it are the
        /// same, and the shipped −43 made them 35 and 38.
        ///
        /// <para>Measured off the RENDERED cells, not off the constants, so this fails if a literal
        /// creeps back into one of the four sites that place them.</para></summary>
        [UnityTest, Order(9)]
        public IEnumerator Draw_price_cell_sits_exactly_between_the_two_team_cells()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Transform card = Required(App(laptop), "Matchup0");
            var away = (RectTransform)Required(card, "AwayOdds");
            var home = (RectTransform)Required(card, "HomeOdds");
            Transform drawNode = Find(card, "DrawOdds");
            Assert.IsNotNull(drawNode,
                "a generated board prices a draw on every matchup (SlateGenerator), so the row must "
                + "be here — if it is not, the board stopped being a 1X2 board");
            var draw = (RectTransform)drawNode;

            var basis = (RectTransform)card;
            float gapAbove = LocalTop(away, basis) - LocalTop(draw, basis);
            float gapBelow = LocalTop(draw, basis) - LocalTop(home, basis);
            Assert.AreEqual(gapAbove, gapBelow, 0.05f,
                $"the draw is not in the middle: {gapAbove:F2}px below AWAY, {gapBelow:F2}px above "
                + "HOME. S74 rules the middle position as MEANING, so it is the midpoint by "
                + "construction — if these differ, a literal has been written where the derivation was");

            // And the ring follows the cell. The ring's own comment names this hazard: two elements
            // agreeing by convention rather than by construction is what T95 caught on the TV.
            Invoke(Required(card, "DrawOdds"));
            yield return WaitForRebuild();
            card = Required(App(laptop), "Matchup0");
            var ring = Find(card, "BiroRing") as RectTransform;
            Assert.IsNotNull(ring, "picking the draw must ink its price cell");
            var drawAfter = (RectTransform)Required(card, "DrawOdds");
            Assert.AreEqual(LocalTop(drawAfter, (RectTransform)card), LocalTop(ring, (RectTransform)card),
                12f, "the ring was left behind at the cell's old y");
        }

        /// <summary>P5 on the SCREEN — the composer is tested next door; this is the slot. Toner,
        /// once per slip, present exactly when something is statable and absent when nothing is.</summary>
        [UnityTest, Order(9)]
        public IEnumerator Relation_statement_renders_once_in_toner_and_only_when_there_is_one()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            BetslipModel slip = laptop.Slip;

            // An ordinary two-matchup slip has no relation to state.
            slip.Clear();
            Assert.IsTrue(slip.AddLeg(0, MarketSelection.Moneyline(Side.Away)));
            Assert.IsTrue(slip.AddLeg(1, MarketSelection.Moneyline(Side.Away)));
            yield return WaitForRebuild();
            Assert.IsNull(Find(Required(App(laptop), "WorkingMargin"), "RelationStatement"),
                "a slip with no same-match group has nothing to state");

            // Find a same-match pair the model actually nominates a statable principal for. Searched,
            // because which pairs correlate is a property of the board and the board is re-priced on
            // every boot — and because 46.1% of same-match slips correctly state NOTHING (S79), so
            // the first pair found is usually not one of them.
            string expected = null;
            MarketSelection selA = default, selB = default;
            var offers = run.CurrentSlate.Matchups[0].Markets;
            for (int a = 0; a < offers.Count && expected == null; a++)
                for (int b = a + 1; b < offers.Count; b++)
                {
                    slip.Clear();
                    if (!slip.AddLeg(0, offers[a].Selection)) continue;
                    if (!slip.AddLeg(0, offers[b].Selection)) continue;
                    if (slip.Refusal != null) continue;
                    expected = SportsbookApp.RelationStatement(slip.SameMatchPricing, slip.Picks);
                    if (expected != null) { selA = offers[a].Selection; selB = offers[b].Selection; break; }
                }
            Assert.IsNotNull(expected,
                "no same-match pair on matchup 0 states a relation — P5 has nothing to render and "
                + "this gate cannot mean anything");

            // The search above churned thousands of two-leg slips on matchup 0 without a frame in
            // between, and the OS rebuilds off a SIGNATURE — so the last render can be a different
            // pair with the same signature, and the slot would read stale. Clear to a state that
            // cannot share a signature with a two-leg slip, let it draw, then rebuild the found pair.
            slip.Clear();
            yield return WaitForRebuild();
            Assert.IsTrue(slip.AddLeg(0, selA));
            Assert.IsTrue(slip.AddLeg(0, selB));
            yield return WaitForRebuild();
            Transform margin = Required(App(laptop), "WorkingMargin");
            var statement = Required(margin, "RelationStatement").GetComponent<TMP_Text>();
            Assert.AreEqual(expected, statement.text, "the slot renders the composed statement");
            Assert.AreEqual(LaptopOs.White, statement.color, "the statement is TONER");

            // ONCE PER SLIP. A four-leg same-match slip can carry six pairwise relations and the
            // model nominates one; the surface must not find a second place to say something.
            int statements = 0;
            foreach (TMP_Text t in margin.GetComponentsInChildren<TMP_Text>(true))
                if (t.name == "RelationStatement") statements++;
            Assert.AreEqual(1, statements, "one relation per slip");

            // The slot is FIXED at two lines. Seven of nine sentences fit one, and §2 forbids a zone
            // that resizes to content — so this must not have been quietly sized to the sentence.
            Assert.AreEqual(SportsbookApp.RelationStatementHeight,
                ((RectTransform)statement.transform).rect.height, 0.5f,
                "the slot is a fixed grid constant, not a zone resizing to its content (§2)");
        }

        /// <summary>P5 — the relation statement (S78/S79). The family, the ruled silence, and the
        /// one held pair.</summary>
        [Test, Order(8)]
        public void Relation_statement_is_a_family_states_nothing_when_nothing_is_statable()
        {
            var empty = new List<Pick>();

            // The family: the sentences differ exactly where the relations differ and are identical
            // exactly where the relations are identical. That is what makes them a family rather
            // than templating, and it is why they must not be re-authored apart (S78).
            Assert.AreEqual("THE SAME GOALS SETTLE BOTH.", SportsbookApp.RelationStatement(
                MakePricing(RelationKind.SharedScoreline, RelationSign.Reinforcing,
                    SelectionFamily.Goal, null), empty));
            Assert.AreEqual("THE SAME CORNERS SETTLE BOTH.", SportsbookApp.RelationStatement(
                MakePricing(RelationKind.SharedCount, RelationSign.Reinforcing,
                    SelectionFamily.Corner, null), empty));
            Assert.AreEqual("THE SAME CARDS SETTLE THESE OPPOSITE WAYS.", SportsbookApp.RelationStatement(
                MakePricing(RelationKind.SharedCount, RelationSign.Opposing,
                    SelectionFamily.Card, null), empty));

            // Sign is carried. Reinforcing and opposing are OPPOSITE claims about the same shared
            // thing, so one sentence for both would state one of them falsely about the other.
            Assert.AreNotEqual(
                SportsbookApp.RelationStatement(MakePricing(RelationKind.SharedScoreline,
                    RelationSign.Reinforcing, SelectionFamily.Goal, null), empty),
                SportsbookApp.RelationStatement(MakePricing(RelationKind.SharedScoreline,
                    RelationSign.Opposing, SelectionFamily.Goal, null), empty));

            // The implication statement carries the COST and says WHICH leg — S78 refused the
            // drafted "ONE OF THESE ALREADY COVERS THE OTHER" for dropping both.
            string implies = SportsbookApp.RelationStatement(
                MakePricing(RelationKind.Implies, RelationSign.Reinforcing, SelectionFamily.Goal, null),
                empty);
            StringAssert.Contains("ADDS NOTHING", implies,
                "the statement exists because he would otherwise be quietly charged for a leg that "
                + "cannot lose (S17) — the cost is the part he can act on");
            Assert.IsTrue(implies.Contains("FIRST") && implies.Contains("SECOND"),
                $"the statement must say WHICH leg adds nothing: \"{implies}\"");

            // S79: a null principal states NOTHING, and that is ruled CORRECT — the price did not
            // move, so there is no cost to disclose. A statement is never authored to fill it.
            Assert.IsNull(SportsbookApp.RelationStatement(null, empty),
                "no pricing is no statement");

            // The ScorerOfSide pair, released by DD batch 72 and shipped as approved. Both signs, so
            // the family's sign rule holds here too.
            Assert.AreEqual("THE SAME TEAM'S GOALS SETTLE BOTH.", SportsbookApp.RelationStatement(
                MakePricing(RelationKind.ScorerOfSide, RelationSign.Reinforcing,
                    SelectionFamily.Goal, Side.Home), empty));
            Assert.AreEqual("THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS.",
                SportsbookApp.RelationStatement(MakePricing(RelationKind.ScorerOfSide,
                    RelationSign.Opposing, SelectionFamily.Goal, Side.Away), empty));

            // The side is NOT spoken — that half of S78 was confirmed and is not what batch 72
            // withdrew. A sentence that names a team has stopped stating the relation.
            foreach (Side side in new[] { Side.Home, Side.Away })
            {
                string s = SportsbookApp.RelationStatement(MakePricing(RelationKind.ScorerOfSide,
                    RelationSign.Reinforcing, SelectionFamily.Goal, side), empty);
                Assert.IsFalse(s.Contains("HOME") || s.Contains("AWAY"),
                    $"the statement names a side: \"{s}\"");
            }

            // Lengthening is not remarked, and no formula reaches the face (§8).
            foreach (RelationKind k in Enum.GetValues(typeof(RelationKind)))
                foreach (RelationSign sg in Enum.GetValues(typeof(RelationSign)))
                {
                    string s = SportsbookApp.RelationStatement(
                        MakePricing(k, sg, SelectionFamily.Goal, Side.Home), empty);
                    if (s == null) continue;
                    foreach (string banned in new[] { "%", "BETTER", "VALUE", "BOOST", "DISCOUNT",
                             "PARLAY", "SGP", "CORRELAT" })
                        Assert.IsFalse(s.ToUpperInvariant().Contains(banned),
                            $"\"{banned}\" reaches the face in \"{s}\" — the statement states the "
                            + "relation, never the apparatus and never that the price moved his way");
                }
        }

        /// <summary>S80's OWED STATE SWEEP — the margin flow measured across the compositional
        /// states, before any geometry moves.
        ///
        /// <para><b>Why it exists, stated plainly: the MaxLegs invariant builds ONE state.</b> It
        /// fills the cap across distinct matchups with no consumable held, so it has never seen the
        /// modifiers row — 34px gated on pure RUN state — nor a relation statement. My own blind-spot
        /// list named the sentence and never the consumables, which is exactly the gap the DD found.
        /// A gate that measures one point of a state space cannot report the worst case of it.</para>
        ///
        /// <para>Filter-only: it rebuilds the surface dozens of times.</para></summary>
        [UnityTest, Order(21), Explicit("S80 evidence: margin flow across legs x consumables x "
            + "statement. Rebuilds the surface dozens of times; run by filter only.")]
        public IEnumerator Evidence_S80_margin_flow_across_compositional_states()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            BetslipModel slip = laptop.Slip;
            int maxLegs = run.Config.MaxLegs;

            // A same-match pair the model nominates a statable principal for — the "statement
            // present" arm needs one, and 46.1% of same-match pairs correctly state nothing (S79).
            MarketSelection stateA = default, stateB = default;
            bool haveStatable = false;
            var offers = run.CurrentSlate.Matchups[0].Markets;
            for (int a = 0; a < offers.Count && !haveStatable; a++)
                for (int b = a + 1; b < offers.Count; b++)
                {
                    slip.Clear();
                    if (!slip.AddLeg(0, offers[a].Selection)) continue;
                    if (!slip.AddLeg(0, offers[b].Selection)) continue;
                    if (slip.Refusal != null) continue;
                    if (SportsbookApp.RelationStatement(slip.SameMatchPricing, slip.Picks) == null)
                        continue;
                    stateA = offers[a].Selection; stateB = offers[b].Selection; haveStatable = true;
                    break;
                }
            Assert.IsTrue(haveStatable, "no statable same-match pair on matchup 0");

            float budget = SportsbookApp.MarginFlowBudget;
            UnityEngine.Debug.Log($"[S80-SWEEP] budget {budget:F0}px · legs | consumables | statement "
                + "| flowBottom | flow depth | vs budget");

            // Consumables are append-only on Run (GrantConsumable), so the sweep walks them upward:
            // none -> one -> both. Nothing here removes one, which keeps the run honest rather than
            // reaching for a setter that does not exist.
            string[] modLabels = { "none", "one", "both" };
            string[] modGrants = { null, "free_bet", "double_or_nothing" };
            float worstOverrun = float.MinValue;
            string worstCase = "";

            for (int mi = 0; mi < modLabels.Length; mi++)
            {
                if (modGrants[mi] != null)
                {
                    ConsumableDefinition def = null;
                    foreach (ConsumableDefinition c in RelicCatalog.Consumables)
                        if (c.Id == modGrants[mi]) { def = c; break; }
                    Assert.IsNotNull(def, $"consumable '{modGrants[mi]}' is not in the catalog");
                    run.GrantConsumable(def);
                }

                for (int legs = 1; legs <= maxLegs; legs++)
                    foreach (bool withStatement in new[] { false, true })
                    {
                        if (withStatement && legs < 2) continue;   // a statement needs a pair

                        slip.Clear();
                        yield return WaitForRebuild();   // force a signature change before rebuilding

                        bool built = true;
                        if (withStatement)
                        {
                            built &= slip.AddLeg(0, stateA);
                            built &= slip.AddLeg(0, stateB);
                            for (int extra = 0; extra < legs - 2 && built; extra++)
                                built &= slip.AddLeg(extra + 1, MarketSelection.Moneyline(Side.Away));
                        }
                        else
                        {
                            for (int i = 0; i < legs && built; i++)
                                built &= slip.AddLeg(i, MarketSelection.Moneyline(Side.Away));
                        }
                        if (!built || slip.Picks.Count != legs) continue;
                        yield return WaitForRebuild();

                        var margin = Required(App(laptop), "WorkingMargin") as RectTransform;
                        FlowDepth d = MeasureFlowDepth(margin);
                        bool statementOnScreen = Find(margin, "RelationStatement") != null;
                        float overrun = -budget - d.BottomUntilted;
                        float depthPx = -d.BottomUntilted;

                        if (overrun > worstOverrun)
                        {
                            worstOverrun = overrun;
                            worstCase = $"{legs} legs, consumables {modLabels[mi]}, statement "
                                + $"{(statementOnScreen ? "present" : "absent")}";
                        }
                        UnityEngine.Debug.Log($"[S80-SWEEP] {legs} | {modLabels[mi]} | "
                            + $"{(statementOnScreen ? "present" : "absent")} | "
                            + $"{d.BottomUntilted:F2} | {depthPx:F2} | "
                            + $"{(overrun > 0 ? "+" : "")}{overrun:F2} | deepest {d.DeepestName}");
                    }
            }

            UnityEngine.Debug.Log($"[S80-SWEEP] WORST: {worstCase} -> "
                + $"{(worstOverrun > 0 ? "+" : "")}{worstOverrun:F2}px against a {budget:F0}px budget"
                + (worstOverrun > 0 ? " — OVER" : " — fits"));

            // ---- S80 §1: the statement's own height. The constant is derived from the LONGEST
            // sentence's measured height, and "if the face measures wider and it reaches three lines,
            // 30 is wrong and the whole reservation moves."
            slip.Clear();
            yield return WaitForRebuild();
            slip.AddLeg(0, stateA); slip.AddLeg(0, stateB);
            yield return WaitForRebuild();
            var statementNode = Find(Required(App(laptop), "WorkingMargin"), "RelationStatement");
            if (statementNode != null)
            {
                var t = statementNode.GetComponent<TMP_Text>();
                string longest = "THE SAME TEAM'S GOALS SETTLE THESE OPPOSITE WAYS.";
                string was = t.text;
                t.text = longest;
                t.ForceMeshUpdate();
                UnityEngine.Debug.Log($"[S80-BOX] longest sentence \"{longest}\" in a "
                    + $"{((RectTransform)t.transform).rect.width:F0}px box: {t.textInfo.lineCount} "
                    + $"lines, preferred height {t.preferredHeight:F1}px, against the "
                    + $"{SportsbookApp.RelationStatementHeight:F0}px slot — "
                    + (t.preferredHeight <= SportsbookApp.RelationStatementHeight
                        ? "30 STANDS" : "30 IS WRONG, the reservation moves"));
                t.text = was;
            }

            // ---- A's MEASUREMENT PASS (S82). The spec harvests "the slack between each block's box
            // and its advance", and an advance is not the same thing as air: what can actually be
            // taken is the GAP between one block's rendered bottom and the next block's rendered top.
            // Measured off the tree so a box that draws taller than its rect, or a block whose
            // advance is consumed by something invisible, cannot be mistaken for slack.
            slip.Clear();
            yield return WaitForRebuild();
            for (int i = 0; i < maxLegs; i++) slip.AddLeg(i, MarketSelection.Moneyline(Side.Away));
            yield return WaitForRebuild();
            var flowMargin = Required(App(laptop), "WorkingMargin") as RectTransform;

            var bands = new List<(string Name, float Top, float Bottom)>();
            foreach (Graphic g in flowMargin.GetComponentsInChildren<Graphic>(true))
            {
                var rect = g.rectTransform;
                if (rect == flowMargin) continue;
                if (rect.GetComponentInParent<Button>() != null) continue;
                float top = LocalTop(rect, flowMargin), bot = LocalBottom(rect, flowMargin);
                if (top >= LocalTop(flowMargin, flowMargin) - 0.5f
                    && bot <= LocalBottom(flowMargin, flowMargin) + 0.5f) continue;   // ground
                bands.Add((rect.name, top, bot));
            }
            bands.Sort((x, y2) => y2.Top.CompareTo(x.Top));
            // NAMED, because it is load-bearing: this runs AFTER the sweep granted its consumables,
            // and Run has no ungrant. So the state measured is MaxLegs + the modifiers row — which
            // is the LIVE DEFECT state, and therefore the right one to harvest from. Gaps that only
            // exist without the modifiers row would not be worth taking.
            UnityEngine.Debug.Log($"[S82-A] state: {maxLegs} legs, modifiers row PRESENT, no statement");
            UnityEngine.Debug.Log("[S82-A] element | top | bottom | gap above "
                + "(buttons excluded, so chip/nudge rows show as one wide gap)");
            float prevBottom = 0f;
            for (int i = 0; i < bands.Count; i++)
            {
                float gap = prevBottom - bands[i].Top;
                UnityEngine.Debug.Log($"[S82-A] {bands[i].Name} | {bands[i].Top:F1} | "
                    + $"{bands[i].Bottom:F1} | {(i == 0 ? 0f : gap):F1}");
                prevBottom = Mathf.Min(prevBottom, bands[i].Bottom);
            }

            Assert.Greater(slip.Picks.Count, 0, "the sweep must have built at least one state");
        }

        /// <summary>EVIDENCE, not verification — the DD's two measurements for the S77 forms, run by
        /// filter only so it does not lengthen routine suites (the TvSweatCaptureHarness pattern).
        ///
        /// <para>Emits (1) every authored form against the PLACE control's own 296 × 44 box, at both
        /// the shipped 13px and the 17px S77's analysis quoted, and (2) the ARITY DISTRIBUTION of
        /// real refusals swept off the live board — which is what says which forms actually fire
        /// rather than merely exist.</para></summary>
        [UnityTest, Order(20), Explicit("Evidence for the DD: S77 form widths + refusal arity "
            + "distribution. Sweeps the board and is slow; run by filter only.")]
        public IEnumerator Evidence_S77_form_widths_and_refusal_arity_distribution()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            BetslipModel slip = laptop.Slip;

            // A rendered stamp node, purely to borrow the production font asset.
            slip.Clear();
            slip.AddLeg(0, MarketSelection.Moneyline(Side.Away));
            yield return WaitForRebuild();
            var probeText = Required(Required(Required(App(laptop), "WorkingMargin"), "Place"), "Label")
                .GetComponent<TMP_Text>();
            TMP_FontAsset font = probeText.font;

            // ---- (1) THE FORMS, against the CONTROL's box rather than the reason node's.
            const float controlWidth = 296f;   // PLACE control, S77's own frame of reference
            UnityEngine.Debug.Log("[S77-FORMS] form | 13px | 17px | vs 296px control");
            var seen = new HashSet<string>();
            foreach (RefusalKind kind in Enum.GetValues(typeof(RefusalKind)))
                for (int causeArity = 2; causeArity <= run.Config.MaxLegs; causeArity++)
                    for (int remedyArity = 0; remedyArity < run.Config.MaxLegs; remedyArity++)
                    {
                        var probe = new TicketRefusal(kind, Enumerable.Range(0, causeArity).ToArray(),
                            Enumerable.Range(0, remedyArity).ToArray(), null, 0.0);
                        foreach (string line in new[]
                                 { SportsbookApp.RefusalCause(probe), SportsbookApp.RefusalRemedy(probe) })
                        {
                            if (!seen.Add(line)) continue;
                            float w13 = LaptopUi.MeasureWidth(font, line, 13, LaptopTrack.StampReason);
                            float w17 = LaptopUi.MeasureWidth(font, line, 17, LaptopTrack.StampReason);
                            UnityEngine.Debug.Log($"[S77-FORMS] \"{line}\" | {w13:F1} | {w17:F1} | "
                                + $"{w13 / controlWidth:P0} / {w17 / controlWidth:P0}");
                        }
                    }

            // ---- (1b) `DRAW {price}` JOINS THE C46 POPULATION (S74-am's own closing line: "DRAW and
            // its price are new strings in the canon face; they measure against their cells like
            // everything else and join the sweep's population under C46"). Measured off the RENDERED
            // control — its own font, size, tracking and cell width — rather than against numbers
            // copied out of the call site, because a cell's assumption about its face is exactly what
            // C46 says goes unstated.
            Transform drawNode = Find(Required(App(laptop), "Matchup0"), "DrawOdds");
            if (drawNode != null)
            {
                var drawLabel = Required(drawNode, "Label").GetComponent<TMP_Text>();
                float cell = ((RectTransform)drawNode).rect.width;
                int size = Mathf.RoundToInt(drawLabel.fontSize);
                float worstDraw = 0f; string worstDrawText = "";
                foreach (Matchup mu in run.CurrentSlate.Matchups)
                {
                    if (mu.DrawOdds <= 1.0) continue;
                    string s = $"DRAW  {OddsFormat.American(mu.DrawOdds)}";
                    float w = LaptopUi.MeasureWidth(drawLabel.font, s, size, LaptopTrack.Names);
                    if (w > worstDraw) { worstDraw = w; worstDrawText = s; }
                }
                // The board's own draws are a sample, not the population. The cell must also hold the
                // widest string the FORMAT can produce — five digits and a sign is the ceiling.
                string formatCeiling = "DRAW  +10000";
                float ceilingWidth = LaptopUi.MeasureWidth(drawLabel.font, formatCeiling, size,
                    LaptopTrack.Names);
                UnityEngine.Debug.Log($"[S74-FIT] cell {cell:F0}px at {size}px · widest ON THIS BOARD "
                    + $"\"{worstDrawText}\" {worstDraw:F1}px ({worstDraw / cell:P0}) · FORMAT CEILING "
                    + $"\"{formatCeiling}\" {ceilingWidth:F1}px ({ceilingWidth / cell:P0}) · "
                    + $"AWAY/HOME comparable \"AWAY  -341\" "
                    + $"{LaptopUi.MeasureWidth(drawLabel.font, "AWAY  -341", size, LaptopTrack.Names):F1}px");
            }

            // ---- (2) THE ARITY DISTRIBUTION, swept off the live board.
            // All PAIRS on every matchup, plus all TRIPLES on matchup 0. Pairs are where duplicates
            // and two-leg impossibilities live; triples are what produce the plural remedies the
            // copy had to be authored for. Coverage is stated rather than implied — see the summary
            // line, which reports the combinations examined as well as the refusals found.
            var byKind = new Dictionary<string, int>();
            var causeArityCount = new Dictionary<int, int>();
            var remedyArityCount = new Dictionary<int, int>();
            int examined = 0, refused = 0;

            void Record(TicketRefusal r)
            {
                refused++;
                string k = r.Kind.ToString();
                byKind[k] = byKind.TryGetValue(k, out int c) ? c + 1 : 1;
                causeArityCount[r.CauseLegs.Count] =
                    causeArityCount.TryGetValue(r.CauseLegs.Count, out int a) ? a + 1 : 1;
                remedyArityCount[r.RemedyLegs.Count] =
                    remedyArityCount.TryGetValue(r.RemedyLegs.Count, out int b) ? b + 1 : 1;
            }

            for (int m = 0; m < run.CurrentSlate.Matchups.Count; m++)
            {
                var offers = new List<MarketSelection>();
                foreach (MarketOffer offer in run.CurrentSlate.Matchups[m].Markets)
                    offers.Add(offer.Selection);
                for (int a = 0; a < offers.Count; a++)
                    for (int b = a; b < offers.Count; b++)   // b == a covers the DUPLICATE arm
                    {
                        examined++;
                        TicketRefusal r = run.RefusalFor(new[]
                            { new Pick(m, offers[a]), new Pick(m, offers[b]) });
                        if (r != null) Record(r);
                    }
            }

            var m0 = new List<MarketSelection>();
            foreach (MarketOffer offer in run.CurrentSlate.Matchups[0].Markets) m0.Add(offer.Selection);
            for (int a = 0; a < m0.Count; a++)
                for (int b = a; b < m0.Count; b++)
                    for (int c = b; c < m0.Count; c++)
                    {
                        examined++;
                        TicketRefusal r = run.RefusalFor(new[]
                            { new Pick(0, m0[a]), new Pick(0, m0[b]), new Pick(0, m0[c]) });
                        if (r != null) Record(r);
                    }

            UnityEngine.Debug.Log($"[S77-ARITY] examined {examined} combinations "
                + $"({run.CurrentSlate.Matchups.Count} matchups, all pairs; matchup 0, all triples) "
                + $"-> {refused} refusals");
            foreach (KeyValuePair<string, int> kv in byKind)
                UnityEngine.Debug.Log($"[S77-ARITY] kind {kv.Key}: {kv.Value} "
                    + $"({(double)kv.Value / refused:P1})");
            foreach (int n in causeArityCount.Keys.OrderBy(x => x))
                UnityEngine.Debug.Log($"[S77-ARITY] cause arity {n}: {causeArityCount[n]} "
                    + $"({(double)causeArityCount[n] / refused:P1})");
            foreach (int n in remedyArityCount.Keys.OrderBy(x => x))
                UnityEngine.Debug.Log($"[S77-ARITY] remedy arity {n}: {remedyArityCount[n]} "
                    + $"({(double)remedyArityCount[n] / refused:P1}) -> form: "
                    + $"\"{SportsbookApp.RefusalRemedy(new TicketRefusal(RefusalKind.ImpossibleCombination, new[] { 0, 1 }, Enumerable.Range(0, n).ToArray(), null, 0.0))}\"");

            // ---- (3) WHICH RELATIONS THE MODEL ACTUALLY EMITS AS PRINCIPAL.
            // P5 states ONE relation per slip, composed from `principal`. A sentence for a relation
            // the model never nominates is copy that can never render, so the four drafts are only
            // shippable against this list. Two matchups' worth of pairs — enough to enumerate the
            // KINDS, which is what the drafts are keyed to.
            var principals = new Dictionary<string, int>();
            int sameMatchSlips = 0, nullPrincipal = 0;
            for (int m = 0; m < Mathf.Min(2, run.CurrentSlate.Matchups.Count); m++)
            {
                var offers = new List<MarketSelection>();
                foreach (MarketOffer offer in run.CurrentSlate.Matchups[m].Markets)
                    offers.Add(offer.Selection);
                for (int a = 0; a < offers.Count; a++)
                    for (int b = a + 1; b < offers.Count; b++)
                    {
                        slip.Clear();
                        if (!slip.AddLeg(m, offers[a])) continue;
                        if (!slip.AddLeg(m, offers[b])) continue;
                        if (slip.Refusal != null) continue;      // refused slips never reach P5
                        SameMatchPrice priced = slip.SameMatchPricing;
                        if (priced == null) continue;
                        sameMatchSlips++;
                        if (priced.Principal == null) { nullPrincipal++; continue; }
                        Relation p = priced.Principal.Value;
                        string key = p.Kind.ToString()
                            + (p.Sign != RelationSign.None ? $"/{p.Sign}" : "")
                            + (p.Family != null ? $"/{p.Family}" : "")
                            + (p.ScorerSide != null ? $"/{p.ScorerSide}" : "");
                        principals[key] = principals.TryGetValue(key, out int pc) ? pc + 1 : 1;
                    }
            }
            UnityEngine.Debug.Log($"[S77-PRINCIPAL] {sameMatchSlips} placeable same-match slips "
                + $"(2 matchups, all pairs) · {nullPrincipal} with NO statable relation "
                + $"({(sameMatchSlips == 0 ? 0 : (double)nullPrincipal / sameMatchSlips):P1})");
            foreach (KeyValuePair<string, int> kv in principals.OrderByDescending(x => x.Value))
                UnityEngine.Debug.Log($"[S77-PRINCIPAL] {kv.Key}: {kv.Value} "
                    + $"({(double)kv.Value / sameMatchSlips:P1})");

            // ---- (4) S78's OWED MEASUREMENT: the seven sentences against their actual slot.
            // "Approval above is of the copy; fit is not asserted and never is at this seat."
            // The slot is the margin's 296px content column at 13px, two lines — so the budget a
            // sentence is measured against is 2 x 296, and S77-am's 80% headroom rule applies to it
            // for the same reason it applies to the stamp: ~20% is what absorbs a face that measures
            // wider than the one it was sized against (C46).
            const float column = 296f;
            const float statementBudget = column * 2f;
            UnityEngine.Debug.Log("[S78-FIT] sentence | 13px | vs 2x296 budget | lines");
            var sentences = new List<string>();
            foreach (RelationKind k in new[] { RelationKind.SharedScoreline, RelationKind.ScorerOfSide })
                foreach (RelationSign sg in new[] { RelationSign.Reinforcing, RelationSign.Opposing })
                    sentences.Add(SportsbookApp.RelationStatement(
                        MakePricing(k, sg, SelectionFamily.Goal, Side.Home), slip.Picks));
            foreach (SelectionFamily fam in new[] { SelectionFamily.Corner, SelectionFamily.Card })
                foreach (RelationSign sg in new[] { RelationSign.Reinforcing, RelationSign.Opposing })
                    sentences.Add(SportsbookApp.RelationStatement(
                        MakePricing(RelationKind.SharedCount, sg, fam, null), slip.Picks));
            sentences.Add(SportsbookApp.RelationStatement(
                MakePricing(RelationKind.Implies, RelationSign.Reinforcing, SelectionFamily.Goal, null),
                slip.Picks));

            float worstSentence = 0f;
            string worstSentenceText = "";
            foreach (string s in sentences.Where(x => x != null).Distinct())
            {
                float w = LaptopUi.MeasureWidth(font, s, 13, 0f);
                if (w > worstSentence) { worstSentence = w; worstSentenceText = s; }
                UnityEngine.Debug.Log($"[S78-FIT] \"{s}\" | {w:F1} | {w / statementBudget:P0} | "
                    + $"{Mathf.CeilToInt(w / column)}");
            }
            UnityEngine.Debug.Log($"[S78-FIT] WIDEST \"{worstSentenceText}\" {worstSentence:F1}px = "
                + $"{worstSentence / statementBudget:P0} of the 2-line budget "
                + $"(S77-am's rule: under 80%)");

            // ---- (5) THE FLOW COST. P5 adds a slot to a region T47 reserves and S51 has just shown
            // is already flush. Measured rather than assumed, because the margin invariant fills
            // MaxLegs across DIFFERENT matchups and so never renders a statement at all.
            UnityEngine.Debug.Log($"[S78-FLOW] statement slot {SportsbookApp.RelationStatementHeight:F0}px"
                + $" + 6px separation = {SportsbookApp.RelationStatementHeight + 6f:F0}px added to a "
                + $"{SportsbookApp.MarginFlowBudget:F0}px flow budget that currently clears by 0.10px "
                + "at MaxLegs. A same-match slip AT MaxLegs therefore overruns T47's 6px pad by "
                + $"{SportsbookApp.RelationStatementHeight + 6f - 6f:F0}px. Geometry, and it goes to "
                + "Allen with the cost stated (S77's step 3), not taken here.");

            // ---- (6) The ScorerSide club-name report stood here and is WITHDRAWN by DD
            // batch 72, which also released the pair with no mark. The sweep it ran is not
            // re-run: its one finding is recorded at RelationStatement's own call site, where
            // a reader meets the sentence, rather than kept alive as a measurement nobody
            // asked for again.

            Assert.Greater(refused, 0, "the sweep found no refusals — the board changed shape");
        }

        /// <summary>A `SameMatchPrice` carrying one nominated relation, so the approved sentences can
        /// be measured without hunting the board for a slip that happens to emit each one.</summary>
        private static SameMatchPrice MakePricing(RelationKind kind, RelationSign sign,
            SelectionFamily family, Side? scorerSide)
        {
            var relation = new Relation(kind, new[] { 0, 1 }, sign, family, scorerSide);
            return new SameMatchPrice(0.25, 4.0, new[] { relation }, relation, false,
                new double[] { 0, 0, 0, 4.0 });
        }

        /// <summary>P4 — THE HOUSE'S LINE. Where two picks share a match the house marks the
        /// connection in its OWN ink, and marks nothing where there is no connection.
        ///
        /// <para>The negative is the load-bearing half: §3.1 says the mark is DRAWN, NOT CAPTIONED,
        /// and that the instrument's name never becomes a tag beside every occurrence. So this gate
        /// fails if the words reach the margin at all.</para></summary>
        [UnityTest, Order(9)]
        public IEnumerator House_line_marks_connected_picks_in_the_houses_ink_and_never_captions_them()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            BetslipModel slip = laptop.Slip;

            // One leg on each of two different matchups: no connection, so no mark.
            slip.Clear();
            Assert.IsTrue(slip.AddLeg(0, MarketSelection.Moneyline(Side.Away)));
            Assert.IsTrue(slip.AddLeg(1, MarketSelection.Moneyline(Side.Away)));
            yield return WaitForRebuild();
            Transform margin = Required(App(laptop), "WorkingMargin");
            Assert.IsFalse(slip.IsSameMatch, "two matchups, one leg each, is not a same-match slip");
            Assert.IsNull(Find(margin, "HouseLine0"),
                "the house marked a connection between legs on different matches");
            Assert.AreEqual("COMBINED", TextOf(Required(margin, "CombinedLabel")),
                "an ordinary parlay keeps COMBINED — the instrument name is not a decoration to "
                + "sprinkle on every slip, and legs on different matches DO multiply");

            // Now a second leg on the FIRST matchup — a real connection. Searched, because which
            // second selection is addable is a property of the board and the board is re-priced
            // every boot.
            bool connected = false;
            foreach (MarketOffer offer in run.CurrentSlate.Matchups[0].Markets)
            {
                if (slip.Contains(0, offer.Selection)) continue;
                if (!slip.AddLeg(0, offer.Selection)) continue;
                connected = true;
                break;
            }
            Assert.IsTrue(connected, "no second selection on matchup 0 could be added");
            yield return WaitForRebuild();
            margin = Required(App(laptop), "WorkingMargin");

            Assert.IsTrue(slip.IsSameMatch, "two legs on one matchup IS a same-match slip");
            Assert.AreEqual(2, slip.LegCountOn(0), "matchup 0 must carry the connected pair");
            var spine = Required(margin, "HouseLine0").GetComponent<Image>();
            Assert.AreEqual(LaptopOs.MoneyBad, spine.color,
                "§3.1: the house marks in Stamp — he picks in biro, the house marks in its own ink");

            // A spur per member, so the mark says WHICH rows it is about. Slip order is insertion
            // order, so a connected pair can straddle an unrelated leg and a bare spanning stroke
            // would mark a row it has nothing to do with.
            IReadOnlyList<int> connectedLegs = slip.LegIndicesOn(0);
            for (int m = 0; m < connectedLegs.Count; m++)
                Assert.IsNotNull(Find(margin, $"HouseLineSpur0_{m}"),
                    $"connected leg {connectedLegs[m]} carries no spur");

            // P4's other half: the INSTRUMENT is named, on the slip's own price row. `COMBINED`
            // names a price arrived at by multiplying, which §454 forbids for this ticket — it is
            // "its own instrument, never a parlay with an adjustment" — so on a same-match slip that
            // label was not silent about the instrument, it was wrong about it.
            var priceLabel = Required(margin, "CombinedLabel").GetComponent<TMP_Text>();
            Assert.AreEqual("SAME MATCH", priceLabel.text,
                "a same-match slip names its instrument on the price row");
            Assert.AreEqual(0f, priceLabel.characterSpacing, 0.001f,
                "the instrument name is UNTRACKED — the market vocabulary's treatment, not a badge's");

            // DRAWN, NOT CAPTIONED. The name is what the thing is called, never a tag on every
            // occurrence — "the house does not narrate its own presence on his document" (S44).
            foreach (TMP_Text text in margin.GetComponentsInChildren<TMP_Text>(true))
            {
                string upper = (text.text ?? "").ToUpperInvariant();
                Assert.IsFalse(upper.Contains("HOUSE'S LINE"),
                    $"the mark was captioned in \"{text.name}\": \"{text.text}\"");
                Assert.IsFalse(upper.Contains("SGP"),
                    $"SGP is industry jargon and never reaches him — found in \"{text.name}\"");
            }
        }


        [Test, Order(8)]
        public void TicketStateWord_and_LegStateWord_never_cross_contaminate_their_vocabularies()
        {
            // S23 ruling: RIDING is ticket-level only, LIVE is leg-level only — contractual.
            // Driven over every enum member via Enum.GetValues so a future enum addition cannot
            // slip through unchecked.
            foreach (RevealedTicketState state in Enum.GetValues(typeof(RevealedTicketState)))
                Assert.AreNotEqual("LIVE", SportsbookApp.TicketStateWord(state),
                    $"TicketStateWord must never say LIVE (checked for {state})");

            foreach (RevealedLegState state in Enum.GetValues(typeof(RevealedLegState)))
                Assert.AreNotEqual("RIDING", SportsbookApp.LegStateWord(state),
                    $"LegStateWord must never say RIDING (checked for {state})");
        }

        [Test, Order(8)]
        public void LegStateInk_matches_the_kit_for_every_state_including_the_two_that_shared_an_else()
        {
            // S65. The mapping used to be an inline ternary whose final `else` covered PENDING and
            // VOID together, which is why PENDING shipped at --toner-2: the kit gives them different
            // tones and a fallthrough cannot. This is the contract that branch had no way to state.
            //
            // Tokens are RevealedState.jsx's TONE map, which BuildMirrorLeg's own comment names as
            // its source and did not match.
            Assert.AreEqual(LaptopOs.MoneyGold, SportsbookApp.LegStateInk(RevealedLegState.Won),
                "GREEN is --wax");
            Assert.AreEqual(LaptopOs.White, SportsbookApp.LegStateInk(RevealedLegState.Live),
                "LIVE is --toner");
            Assert.AreEqual(LaptopOs.Muted, SportsbookApp.LegStateInk(RevealedLegState.Lost),
                "DEAD is --toner-3; the oxide is the strike, never the word");
            Assert.AreEqual(LaptopOs.Muted, SportsbookApp.LegStateInk(RevealedLegState.Pending),
                "S65: PENDING is --toner-3");
            Assert.AreEqual(LaptopOs.TonerSecondary, SportsbookApp.LegStateInk(RevealedLegState.Voided),
                "VOID stays --toner-2 — the S65 fix must not drag it along with PENDING");

            // PENDING and DEAD are deliberately the same tone (S65), so tone alone cannot separate
            // them and the strike is load-bearing rather than decorative. Asserted so a later change
            // that drops the strike has something to fail against.
            Assert.AreEqual(SportsbookApp.LegStateInk(RevealedLegState.Pending),
                SportsbookApp.LegStateInk(RevealedLegState.Lost),
                "S65 puts PENDING level with DEAD on purpose — DEAD is carried by its other channels");
        }

        [UnityTest, Order(9)]
        public IEnumerator CompactLegLabel_is_unique_for_every_distinct_selection_on_one_matchup()
        {
            // Composer uniqueness guard: a composer that reaches for the wrong field collapses
            // e.g. OVER 2.5 and UNDER 3.5 into one identical row while settlement still grades
            // them differently. Builds the full selection set for one matchup — both moneylines,
            // both BTTS sides, over+under for every goal/corner/card line in run.Config, and every
            // scorer index — and asserts CompactLegLabel never collides.
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            Matchup matchup = run.CurrentSlate.Matchups[0];

            var selections = new List<MarketSelection>
            {
                MarketSelection.Moneyline(Side.Home),
                MarketSelection.Moneyline(Side.Away),
                MarketSelection.BothTeamsToScore(true),
                MarketSelection.BothTeamsToScore(false),
            };
            foreach (double line in run.Config.GoalLines)
            {
                selections.Add(MarketSelection.TotalGoals(line, true));
                selections.Add(MarketSelection.TotalGoals(line, false));
            }
            foreach (double line in run.Config.CornerLines)
            {
                selections.Add(MarketSelection.TotalCorners(line, true));
                selections.Add(MarketSelection.TotalCorners(line, false));
            }
            foreach (double line in run.Config.CardLines)
            {
                selections.Add(MarketSelection.TotalCards(line, true));
                selections.Add(MarketSelection.TotalCards(line, false));
            }
            int scorerCount = matchup.Away.Players.Count + matchup.Home.Players.Count;
            for (int i = 0; i < scorerCount; i++)
                selections.Add(MarketSelection.AnytimeScorer(i));

            Assert.Greater(selections.Count, 10, "the selection set must be non-trivial for this guard to mean anything");

            var seen = new HashSet<string>();
            foreach (MarketSelection selection in selections)
            {
                string label = SportsbookApp.CompactLegLabel(matchup, selection);
                Assert.IsTrue(seen.Add(label),
                    "CompactLegLabel collision: '" + label + "' for selection Kind=" + selection.Kind
                        + " Choice=" + selection.Choice + " Line=" + selection.Line
                        + " PlayerIndex=" + selection.PlayerIndex);
            }
        }

        /// <summary>Every "MarketOffer"+index row's own x position (anchoredPosition.x, relative to
        /// its MarketContent parent), used to prove a destination renders a single column (A1)
        /// without depending on any particular label-width choice. Recursive — under A4 the rows
        /// are nested inside MarketScroll/MarketViewport/MarketContent, not direct children of
        /// <paramref name="body"/>.</summary>
        private static List<float> OfferRowX(Transform body)
        {
            var xs = new List<float>();
            foreach (Transform row in AllNamed(body, "MarketOffer"))
            {
                var rect = row as RectTransform;
                Assert.IsNotNull(rect, $"{row.name} must be a RectTransform");
                xs.Add(rect.anchoredPosition.x);
            }
            return xs;
        }

        /// <summary>Every descendant of <paramref name="root"/> (root included) whose name starts
        /// with <paramref name="prefix"/>, depth-first. Unlike <see cref="FindPrefix"/> (first
        /// match only), this collects all of them — needed once offer rows can nest arbitrarily
        /// deep under a scroll/viewport/content hierarchy (A4).</summary>
        private static List<Transform> AllNamed(Transform root, string prefix)
        {
            var results = new List<Transform>();
            CollectNamed(root, prefix, results);
            return results;
        }

        private static void CollectNamed(Transform root, string prefix, List<Transform> results)
        {
            if (root.name.StartsWith(prefix, StringComparison.Ordinal)) results.Add(root);
            for (int i = 0; i < root.childCount; i++) CollectNamed(root.GetChild(i), prefix, results);
        }

        /// <summary>Fails if any part of <paramref name="child"/>'s rendered rect falls outside
        /// <paramref name="container"/>'s rendered rect on the X axis, measured in world space via
        /// GetWorldCorners so it holds regardless of anchor/pivot plumbing on either transform.
        /// corners[0]/[2] are the bottom-left/top-right corners for an unrotated rect. Horizontal
        /// only (A4): under a scrolling list a row may legitimately sit above/below the viewport's
        /// visible Y range — RectMask2D exists precisely to clip that — but a row must never run
        /// wider than the viewport, since A4 requires content to stay clear of the S27 rail.</summary>
        /// <summary>S25's general clause: "a container's correctness may not depend on a config
        /// dial's current value. Guard it with a test, not a convention." Allen capped MaxLegs at 4
        /// (2026-08-02) because the kit's two-line MarginLeg overflowed the fixed 324x530 margin at
        /// 6. That cap closes the overflow, but only for as long as nobody raises the dial — so this
        /// reads MaxLegs rather than assuming 4, fills a slip to it, and fails if the margin stops
        /// containing its own content. Both halves of the reported symptom are covered: content
        /// escaping the panel, and the PLACE button colliding with the bottom-fixed LOCK/SKIP band.
        /// </summary>
        [UnityTest, Order(9)]
        public IEnumerator Working_margin_contains_its_content_at_the_legal_maximum_leg_count()
        {
            yield return Boot();
            LaptopScreen laptop = Laptop();
            Run run = laptop.director.Run;
            int maxLegs = run.Config.MaxLegs;

            Assert.GreaterOrEqual(run.Config.MatchupsPerSlate, maxLegs,
                "the slate must be able to supply one leg per matchup up to the cap");

            // One selection per matchup — a second pick on the same matchup replaces rather than
            // adds — so filling to the cap means touching maxLegs distinct entries.
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }

            // S50's named consequence: a STAGED RECEIPT adds flow the bare 4-leg figure never
            // contained, so the worst case is a full slip standing on top of a staged ticket, not a
            // full slip alone. Place once to stage a receipt (which clears the working slip), then
            // refill to the cap. Measuring only the empty-receipt state is what let the original
            // 414px figure understate the flow.
            Invoke(Required(Required(App(laptop), "WorkingMargin"), "Place"));
            yield return WaitForRebuild();
            Assert.Greater(run.Tickets.Count, 0, "a receipt must actually be staged");
            for (int i = 0; i < maxLegs; i++)
            {
                Invoke(Required(Required(App(laptop), "Matchup" + i), "AwayOdds"));
                yield return WaitForRebuild();
            }
            Assert.AreEqual(maxLegs, laptop.Slip.Picks.Count,
                "the slip must actually reach the cap for this invariant to mean anything");

            var margin = Required(App(laptop), "WorkingMargin") as RectTransform;
            Assert.IsNotNull(margin, "WorkingMargin must be a RectTransform");

            // MEASURE IN CANVAS-LOCAL PIXELS, never world units. The laptop is a world-space canvas
            // on a 3D quad: the whole 530px margin spans about 0.043 world units, so a pixel-shaped
            // tolerance like 0.5f is ~12x the entire panel and silently swallows every violation.
            // An earlier version of this test compared world corners with a 0.5f epsilon and passed
            // while the margin was visibly overlapping itself in the captures.
            const float epsilonPx = 0.5f;
            float marginTop = LocalTop(margin, margin);
            float marginBottom = LocalBottom(margin, margin);

            foreach (Graphic graphic in margin.GetComponentsInChildren<Graphic>(true))
            {
                var rect = graphic.rectTransform;
                if (rect == margin) continue;
                Assert.GreaterOrEqual(LocalBottom(rect, margin), marginBottom - epsilonPx,
                    $"{PathOf(rect, margin)} escapes the margin's bottom edge at {maxLegs} legs");
                Assert.LessOrEqual(LocalTop(rect, margin), marginTop + epsilonPx,
                    $"{PathOf(rect, margin)} escapes the margin's top edge at {maxLegs} legs");
            }

            // The reported 4-leg symptom was the action stack colliding, which containment alone
            // would not catch — PLACE is top-anchored below the legs, LOCK/SKIP are bottom-fixed.
            // Every element in the action stack, in the order they must appear down the margin.
            // Comparing only PLACE against LOCK is not enough: the blocked-action reason is its own
            // absolutely-positioned element and can collide with the payout figure above it while
            // PLACE and LOCK stay clear of each other.
            string[] stack = { "Payout", "PlaceReason", "Place", "LockReason", "Lock", "Skip" };
            var present = new List<(string Name, float Top, float Bottom)>();
            foreach (string name in stack)
            {
                Transform node = margin.Find(name);
                if (node == null) continue;          // not every element exists in every state
                var rt = (RectTransform)node;
                present.Add((name, LocalTop(rt, margin), LocalBottom(rt, margin)));
            }

            // Sorted by position, so the check is "nothing occupies the same band as anything else"
            // rather than "the authored order happens to hold" — an element that jumps the stack is
            // still a collision.
            present.Sort((a, b) => b.Top.CompareTo(a.Top));
            for (int i = 1; i < present.Count; i++)
            {
                var above = present[i - 1];
                var below = present[i];
                Assert.GreaterOrEqual(above.Bottom, below.Top - epsilonPx,
                    $"{above.Name} (bottom {above.Bottom:F1}px) overlaps {below.Name} "
                    + $"(top {below.Top:F1}px) at {maxLegs} legs — the action stack collides");
            }

            // T47's reservation, checked directly rather than inferred from the absence of overlap:
            // the flow region must fit its budget, and the anchored band must actually be anchored.
            FlowDepth depth = MeasureFlowDepth(margin);
            float flowBottom = depth.Bottom;
            float flowBottomUntilted = depth.BottomUntilted;
            string deepestName = depth.DeepestName;
            float deepestTiltPx = depth.DeepestTiltPx;
            // S51 — SIGNED, EXPIRING DEVIATION (DD 2026-08-04): the flow's lowest element sits
            // outside its reservation with a staged receipt at MaxLegs, cost recorded as one
            // UN-OWNED excursion, expiring "when the owner is identified — at which point it is
            // FIXED, not re-signed."
            //
            // THE OWNER IS IDENTIFIED (2026-08-14). It is the WAX HIGHLIGHT — the tilted amber band
            // laid behind the payout figure — and the earlier acquittal was arithmetic, not
            // evidence. The note this replaces read: "the wax highlight was the lead's candidate and
            // the frames falsified it — it measures 23–24px, and at 0.5° a 24px band grows 0.21px,
            // twelve times too small." That computes the band's HEIGHT times sin(0.5°). The band
            // rotates about its PIVOT, which SportsbookApp sets to the top-left corner, so the
            // corner that goes lowest is the bottom-RIGHT one and it drops by the band's WIDTH times
            // sin(0.5°) — and the width is `Mathf.Max(40f, payout.preferredWidth) + 8f`, i.e. the
            // payout figure's own measured width. Two terms were being confused for one, and the
            // smaller was tested.
            //
            // That is why this pin could not hold a number. The excursion is
            //
            //     4.00px structural  +  sin(0.5°) x the wax band's width
            //
            // and the second term is not a constant: `RunDirector.seed` is blank in the Room scene,
            // so every boot rolls a fresh seed, every seed prices the board differently, and the
            // payout figure is a different string of a different width each time. The pin read
            // 4.563px on the frame at 20260809-002525-948 and 4.748px when TV measured it after
            // draws landed — a 0.185px move with NO commit touching the margin flow's layout in
            // between (ead9396 re-sourced it and is the last such change; af0c42c and 45cb958 are
            // comment-only and empty-state-only here). Nothing moved. The money did.
            //
            // So the repair was not a new number. Both terms were separated and each held to what it
            // can honestly be held to: the structural part DERIVED and pinned two-sided, the tilt
            // held to a clearance rather than a value.
            //
            // The reservation is not slackened and no element is excluded — the ruling forbids both,
            // because either would have gone green while a real overrun continued. The wax highlight
            // is still measured; what is no longer counted as an overrun is the part of its depth
            // that is a rotation rather than a position.
            //
            // **S51 IS CLOSED (DD batch 66, 2026-08-14), AND THE STRUCTURAL PART IS FIXED.** The DD
            // refused all three seating options I routed and ruled the 4.00px a KIT-FIDELITY gap
            // instead: `PayoutFigure.jsx` places the band `bottom:-2px` against a line box of
            // `--st-size-payout` 31px x `--st-lh-fig` 1.1 = 34.1px, so the kit's band bottom sits
            // 36.1px below the figure's top and this build had it at 40px. One cause, two symptoms —
            // the frame read the band as a detached rule under the figure rather than the
            // highlighter behind it. THE BAND MOVED; the payout block did not.
            //
            // RE-SOURCED ONCE, as the ruling directs, and derived rather than measured:
            //
            //   the payout figure's box is 36.00px tall and its bottom lands exactly on the budget
            //   (-370px). The band's bottom now sits at the kit's 36.10px below that box's top.
            //
            //   0.10px  = 36.10 - 36.00, the kit's 2px overshoot against a 34.1px line box, laid
            //             against a build box that is 36px rather than 34.1px.
            //
            // The DD's "closes at zero" is that tenth of a pixel — the 3.9px the band moved against
            // the 4.00px that was there. It is written out rather than rounded away so the next
            // reader knows the residue is the box-height difference and not drift.
            // RE-SOURCED for S82 option A (2026-08-15), and the SIGN FLIPPED — this is the first time
            // the margin flow has CLEARED its reservation rather than overrun it.
            //
            //   +0.10  before A: the kit's 36.10px band bottom against a 36.00px box (S51).
            //   −10.00 A's harvest, measured per block: the header's 8px gap halved to 4 (−4), the
            //          bare undervied 4px after the leg list deleted (−4), and the payout label's
            //          18px advance on a 16px box (−2).
            //   ──────
            //   −9.90  measured. The ordinary composition now fits, with 9.90px to spare.
            //
            // This does NOT close the live bill: four legs plus a held consumable is still +24.10
            // over, because A recovered 10.00 of the 34.10 it was aimed at. That is S82's
            // disposition (2) and it is Allen's call, not this gate's.
            const float structuralOverrunPx = -9.90f;
            const float structuralTolerancePx = 0.05f;
            float overrunPx = -SportsbookApp.MarginFlowBudget - flowBottom;
            float structuralPx = -SportsbookApp.MarginFlowBudget - flowBottomUntilted;

            // S75 (DD batch 66): "a hand-laid mark reserves with the figure it marks", and "where the
            // mark is transformed, the reserved extent is the TRANSFORMED extent". So the tilt is not
            // bounded by a number I picked — it is held to the boundary it must actually clear:
            // T47's 6px separation between the flow region and the anchored action band, the `+ 6f`
            // inside SportsbookApp.ActionBandReservedHeight.
            //
            // This is what earned the band its pixels. Before the move the total was
            // 4.00 + 0.0087*w, which crosses 6px at w > 229px — reachable, because money never
            // abbreviates (C49) and same-game parlays lengthen the figure. After it, the same
            // arithmetic needs a 677px band inside a 324px panel. The check is kept anyway: it is
            // the invariant, not the margin of safety, and it now holds for every renderable string.
            const float actionBandPadPx = 6f;
            Assert.Less(overrunPx, actionBandPadPx,
                $"the flow's deepest element ({deepestName}) reaches {overrunPx:F2}px past its "
                + $"reservation, into T47's {actionBandPadPx:F0}px separation from the action band "
                + $"({structuralPx:F2}px structural + {deepestTiltPx:F2}px tilt). S75: a transformed "
                + "mark reserves its TRANSFORMED extent, and the tilt term is the band's width times "
                + "sin(0.5°) — so this fires when the payout figure grows, which is exactly the "
                + "collision the band move was ruled to close.");
            Assert.AreEqual(structuralOverrunPx, structuralPx, structuralTolerancePx,
                $"the margin flow's STRUCTURAL overrun moved: measured {structuralPx:F2}px against "
                + $"the derived {structuralOverrunPx:F2}px. Deepest flow element {deepestName} at "
                + $"{flowBottom:F2}px ({deepestTiltPx:F2}px of that is its own tilt, leaving "
                + $"{flowBottomUntilted:F2}px), raw overrun {overrunPx:F2}px, budget "
                + $"-{SportsbookApp.MarginFlowBudget:F0}px, action band reserves "
                + $"{SportsbookApp.ActionBandReservedHeight:F0}px. NEGATIVE IS CLEARANCE: −9.90 is "
                + "the kit's 36.10px band bottom against a 36.00px box (+0.10, S51) less A's "
                + "measured 10.00px harvest (header gap 4, the bare post-leg 4, payout label 2). If "
                + "it went back to ~+0.10 the harvest was reverted; if to ~+4.00 the wax band came "
                + "off the kit's `bottom:-2px` and S51 has been reopened. If it GREW "
                + "otherwise, something entered the margin flow: staged receipts live in the 700px "
                + "sheet and must never re-enter it (both-screens kit amendment, DD 2026-08-04). If "
                + "a RULED size changed, re-derive at this call site with the new arithmetic written "
                + "out — never shrink a figure to fit the pin.");

            // T53 — every gate states what it cannot see. THIS ONE CANNOT SEE:
            //  · rendered glyphs. It measures RectTransforms, so text bleeding outside its own rect
            //    (MakeText falls back to Overflow rather than clipping) is invisible here.
            //  · anything without a Graphic — empty layout containers contribute no bounds.
            //  · horizontal collisions. It is a vertical-stack check only.
            //  · z-order. Two elements sharing a band are caught; one correctly drawn over another
            //    on purpose is not distinguished from one accidentally buried.
            //  · any leg count other than MaxLegs. It now exercises a full slip ON TOP OF a staged
            //    receipt (S50's named consequence), but not multiple staged receipts, and not the
            //    board-frozen state, whose copy differs.
            //  · a tilt that is a genuine layout defect rather than a rotation. The structural pin
            //    subtracts every flow element's own rotation before measuring depth, so a band that
            //    was tilted BY MISTAKE reads as no structural overrun — only the T47 clearance check
            //    above catches it, and only once the total reaches 6px.
            //  · S75's design-time clearance constant. The ruling asks for the population swept
            //    (C46), the widest renderable money string taken, and the clearance pinned as a
            //    CONSTANT — "a zone that moves with the string is not legal". This gate still reads
            //    the band's width at runtime, so it proves the boundary holds for the string this
            //    boot happened to price, not for the widest one that exists. OWED, and cheap now
            //    that the band move put ~677px of headroom between the two.
            //  · which slate it ran on. `RunDirector.seed` is blank in the Room scene, so the board,
            //    the prices and therefore every money string differ on every boot. That is why the
            //    pin is derived rather than measured; it also means this gate has never tested one
            //    fixed set of numbers, and a defect that needs a particular price to appear will
            //    show up here as a flake rather than a failure.
            //  · the REWARDS and MY BETS passive margins, which have their own content.
        }

        /// <summary>A RectTransform's top/bottom edge expressed in <paramref name="basis"/>'s local
        /// pixels. Every containment check on this surface must go through these: the laptop canvas
        /// is world-space on a 3D quad, so GetWorldCorners yields metres and any pixel-shaped
        /// tolerance compared against them is orders of magnitude too large to catch anything.</summary>
        private static float LocalTop(RectTransform rect, RectTransform basis)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return basis.InverseTransformPoint(corners[1]).y;
        }

        /// <summary>How deep the margin's FLOW region reaches, and which element got there.</summary>
        private readonly struct FlowDepth
        {
            public readonly float Bottom;            // lowest measured corner, tilt included
            public readonly float BottomUntilted;    // the same with each element's own rotation out
            public readonly string DeepestName;
            public readonly float DeepestTiltPx;
            public FlowDepth(float bottom, float untilted, string name, float tilt)
            { Bottom = bottom; BottomUntilted = untilted; DeepestName = name; DeepestTiltPx = tilt; }
        }

        /// <summary>The margin flow's depth, in the panel's own local pixels.
        ///
        /// <para>Factored out so the MaxLegs invariant and S80's state sweep measure with ONE
        /// function. Two copies of this would let the sweep's numbers and the pin's number drift,
        /// and the whole point of the sweep is that its figures are comparable to the pin's.</para>
        ///
        /// <para>Exclusions are unchanged and are the ruled ones: anything parented into an action
        /// control belongs to the anchored band rather than the flow, and a ground that covers the
        /// whole panel is SUBSTRATE — excluded by measured coverage, never by name and never by
        /// anchoring, because the first version of this predicate keyed on anchoring and stopped
        /// matching the one thing it existed to skip the very next time the ground changed.</para>
        /// </summary>
        private static FlowDepth MeasureFlowDepth(RectTransform margin)
        {
            const float epsilonPx = 0.5f;
            float marginTop = LocalTop(margin, margin);
            float marginBottom = LocalBottom(margin, margin);
            float bottom = float.MaxValue, untilted = float.MaxValue, deepestTilt = 0f;
            string deepest = "(nothing measured)";
            foreach (Graphic graphic in margin.GetComponentsInChildren<Graphic>(true))
            {
                var rect = graphic.rectTransform;
                if (rect == margin) continue;
                if (rect.GetComponentInParent<Button>() != null) continue;
                bool coversWholePanel = LocalTop(rect, margin) >= marginTop - epsilonPx
                    && LocalBottom(rect, margin) <= marginBottom + epsilonPx;
                if (coversWholePanel) continue;
                float b = LocalBottom(rect, margin);
                bottom = Mathf.Min(bottom, b);
                float tilt = TiltDepth(rect);
                if (b + tilt < untilted)
                {
                    untilted = b + tilt;
                    deepest = PathOf(rect, margin);
                    deepestTilt = tilt;
                }
            }
            return new FlowDepth(bottom, untilted, deepest, deepestTilt);
        }

        private static float LocalBottom(RectTransform rect, RectTransform basis)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return basis.InverseTransformPoint(corners[3]).y;
        }

        /// <summary>How much deeper <paramref name="rect"/>'s own local z-rotation puts the corner
        /// <see cref="LocalBottom"/> reads than that corner would sit unrotated. Positive when the
        /// rotation pushes it DOWN.
        ///
        /// This exists because the margin's deepest flow element is a TILTED one — the wax highlight
        /// behind the payout figure — and a tilt is not a position. Rotation is about the PIVOT, not
        /// the centre, so for the wax band (pivot top-left, 0.5° clockwise) the term scales with the
        /// band's WIDTH, and the band is sized from the payout figure's measured width. Reading that
        /// as an overrun makes the margin's budget check a function of how much money is on the
        /// screen, which is what it had silently become.
        ///
        /// Closed-form rather than a re-measure, and general in pivot and angle: `rect.rect` is
        /// pivot-relative, so the corner in question is `(xMax, yMin)` and rotating it about the
        /// origin is the whole transform. Assumes no rotation is contributed by the chain BETWEEN
        /// the element and the basis — true on this panel, where every flow element is a direct
        /// child of the margin, and loud rather than silent if that ever stops holding, since the
        /// structural pin below is a two-sided equality.</summary>
        private static float TiltDepth(RectTransform rect)
        {
            float deg = Mathf.DeltaAngle(0f, rect.localEulerAngles.z);
            if (Mathf.Abs(deg) < 1e-4f) return 0f;
            float rad = deg * Mathf.Deg2Rad;
            Rect r = rect.rect;
            float rotatedY = r.xMax * Mathf.Sin(rad) + r.yMin * Mathf.Cos(rad);
            return r.yMin - rotatedY;
        }

        /// <summary>The leg's subject — the first token after the "N. " index, e.g. "LONGHAULERS".
        /// FitLabelKeepingSuffix trims from the END of the label and protects the price suffix, so
        /// the subject is the one part of the string that is never width-fitted and therefore never
        /// moves with font-atlas state. Comparing subjects tests what the persistence snapshot
        /// actually claims — that the same leg is still rendered after a destination switch —
        /// without re-testing the text fitter's boundary behaviour as a side effect.</summary>
        private static string LegSubjectOf(string legLabel)
        {
            if (string.IsNullOrEmpty(legLabel)) return legLabel ?? "";
            string body = legLabel;
            int dot = body.IndexOf(". ", StringComparison.Ordinal);
            if (dot >= 0) body = body.Substring(dot + 2);
            int space = body.IndexOf(' ');
            return space > 0 ? body.Substring(0, space) : body;
        }

        private static string PathOf(Transform node, Transform root)
        {
            string path = node.name;
            for (Transform t = node.parent; t != null && t != root; t = t.parent) path = t.name + "/" + path;
            return path;
        }

        private static void AssertWithinContainerHorizontally(RectTransform container, RectTransform child, string label)
        {
            Assert.IsNotNull(child, $"{label} RectTransform missing");
            // Canvas-local pixels, for the same reason as LocalTop/LocalBottom: this comparison used
            // to run against world corners with a 0.5f tolerance, which on a world-space canvas is
            // larger than the whole surface — it could not have failed for any layout.
            var childCorners = new Vector3[4];
            child.GetWorldCorners(childCorners);
            float childLeft = container.InverseTransformPoint(childCorners[0]).x;
            float childRight = container.InverseTransformPoint(childCorners[2]).x;
            Rect bounds = container.rect;
            const float epsilonPx = 0.5f;
            Assert.GreaterOrEqual(childLeft, bounds.xMin - epsilonPx,
                $"{label} left edge escapes the viewport ({childLeft:F1}px vs {bounds.xMin:F1}px)");
            Assert.LessOrEqual(childRight, bounds.xMax + epsilonPx,
                $"{label} right edge escapes the viewport ({childRight:F1}px vs {bounds.xMax:F1}px)");
        }

        private static ReceiptExpectation Capture(BetslipModel slip)
            => new ReceiptExpectation(slip.Stake, slip.CombinedOdds, slip.ToWin);

        private static EntryPersistenceSnapshot CaptureEntryPersistence(LaptopScreen laptop)
        {
            Transform app = App(laptop);
            Transform margin = Required(app, "WorkingMargin");
            Assert.AreEqual(1, laptop.director.Run.Tickets.Count,
                "persistence setup needs one staged ticket");
            Assert.AreEqual(1, laptop.Slip.Picks.Count,
                "persistence setup needs one working mark");
            return new EntryPersistenceSnapshot
            {
                Machine = TextOf(Required(app, "Machine")),
                Sticker = TextOf(Required(app, "Sticker")),
                Clock = TextOf(Required(app, "Clock")),
                EventIdentity = TextOf(Required(app, "EventIdentity")),
                EventRecords = TextOf(Required(app, "EventRecords")),
                EventForm = TextOf(Required(app, "EventForm")),
                MarginTitle = TextOf(Required(margin, "Title")),
                MarginCount = TextOf(Required(margin, "Count")),
                WorkingLeg = TextOf(Required(margin, "Leg0")),
                Stake = TextOf(Required(margin, "Stake")),
                Payout = TextOf(Required(margin, "Payout")),
                ReceiptHeader = TextOf(Required(app, "ReceiptHeader")),
                TicketCount = laptop.director.Run.Tickets.Count,
                PickCount = laptop.Slip.Picks.Count,
                ModelStake = laptop.Slip.Stake,
                Selection = laptop.Slip.SelectionOn(0),
            };
        }

        private static void AssertEntryPersistence(LaptopScreen laptop,
            EntryPersistenceSnapshot expected, string destination)
        {
            Transform app = App(laptop);
            Transform margin = Required(app, "WorkingMargin");
            AssertRect(Required(app, "EventIdentity") as RectTransform, 398f, 32f,
                $"{destination} ENTRY header identity");
            AssertRect(Required(app, "EventRecords") as RectTransform, 150f, 32f,
                $"{destination} ENTRY header records");
            AssertRect(Required(app, "EventForm") as RectTransform, 670f, 32f,
                $"{destination} ENTRY header form");

            Assert.AreEqual(expected.Machine, TextOf(Required(app, "Machine")),
                $"{destination} machine chrome");
            Assert.AreEqual(expected.Sticker, TextOf(Required(app, "Sticker")),
                $"{destination} ownership chrome");
            Assert.AreEqual(expected.Clock, TextOf(Required(app, "Clock")),
                $"{destination} clock chrome");
            Assert.AreEqual(expected.EventIdentity, TextOf(Required(app, "EventIdentity")),
                $"{destination} ENTRY identity");
            Assert.AreEqual(expected.EventRecords, TextOf(Required(app, "EventRecords")),
                $"{destination} ENTRY records");
            Assert.AreEqual(expected.EventForm, TextOf(Required(app, "EventForm")),
                $"{destination} ENTRY form");

            Assert.AreEqual(expected.MarginTitle, TextOf(Required(margin, "Title")),
                $"{destination} working-margin title");
            Assert.AreEqual(expected.MarginCount, TextOf(Required(margin, "Count")),
                $"{destination} working-margin rule");
            // Compared against the leg's SUBJECT, not the captured full string. The full label is
            // width-fitted at render time by MeasureWidth, which reads a dynamic font atlas, and
            // the atlas is not guaranteed identical between the moment the snapshot was taken and
            // the moment it is re-checked — so a label sitting within a glyph of its fit boundary
            // lands either side of it and the ellipsis moves by one character. That produced an
            // intermittent failure on this assertion (filed as the captured-string flake signature:
            // single test, difference at the ellipsis, passes on re-run) which said nothing about
            // the margin surviving a destination switch, the thing this snapshot exists to test.
            // The subject is the part that must persist and is not width-fitted.
            Assert.AreEqual(LegSubjectOf(expected.WorkingLeg),
                LegSubjectOf(TextOf(Required(margin, "Leg0"))),
                $"{destination} working mark");
            Assert.AreEqual(expected.Stake, TextOf(Required(margin, "Stake")),
                $"{destination} working stake");
            Assert.AreEqual(expected.Payout, TextOf(Required(margin, "Payout")),
                $"{destination} working payout");
            Assert.AreEqual(expected.ReceiptHeader, TextOf(Required(app, "ReceiptHeader")),
                $"{destination} staged receipt");
            Assert.AreEqual(expected.TicketCount, laptop.director.Run.Tickets.Count,
                $"{destination} staged-ticket model count");
            Assert.AreEqual(expected.PickCount, laptop.Slip.Picks.Count,
                $"{destination} working-pick model count");
            Assert.AreEqual(expected.ModelStake, laptop.Slip.Stake, 1e-9,
                $"{destination} working stake model");
            Assert.AreEqual(expected.Selection, laptop.Slip.SelectionOn(0),
                $"{destination} working selection model");
        }

        /// <summary>Takes the app root, not the margin: E-07 moved staged receipts out of the 324px
        /// working margin and into the 700px sheet, where the kit puts them (screens.jsx:50-57).
        /// Required searches recursively, so the app root spans both regions.</summary>
        private static void AssertReceipts(Transform app, Run run,
            IReadOnlyList<ReceiptExpectation> expected)
        {
            Transform receipts = Required(app, "StagedTickets");
            int rendered = 0;
            for (int i = 0; i < receipts.childCount; i++)
                if (receipts.GetChild(i).name.StartsWith("StagedTicket", StringComparison.Ordinal))
                    rendered++;
            Assert.AreEqual(run.Tickets.Count, rendered, "rendered staged-ticket count");

            for (int ticketIndex = 0; ticketIndex < run.Tickets.Count; ticketIndex++)
            {
                Ticket ticket = run.Tickets[ticketIndex];
                ReceiptExpectation model = expected[ticketIndex];
                Assert.AreEqual(model.Stake, ticket.Stake, 1e-9, $"ticket {ticketIndex} stake");

                double combined = 1.0;
                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                    combined *= ticket.Legs[legIndex].OfferedOdds;
                Assert.AreEqual(model.Combined, combined, 1e-9,
                    $"ticket {ticketIndex} combined odds");
                Assert.AreEqual(model.Payout, ticket.PotentialPayout, 1e-9,
                    $"ticket {ticketIndex} potential payout");

                Transform receipt = Required(receipts, "StagedTicket" + ticketIndex);
                // S62: asserts against the production formatter rather than a second copy of it.
                // The old line restated the render's own expression, which is how a fixture ends up
                // agreeing with a defect — it would have happily asserted "1.0" forever.
                string identity = LaptopUi.TicketIdentity(ticket.Id, run.Round, ticketIndex,
                    withRound: false);
                // Matches BuildStagedReceipt's own width-fitting formula rather than a duplicated
                // literal, so a legitimate format change (fixing the mid-word truncation defect)
                // can never quietly desync the fixture from the render code.
                // **Merge resolution, main into surething-ui.** Both branches found this defect
                // independently and fixed it the same way — read the fitting width off the rendered
                // header rather than restating it — so the intent below is both sides', not a
                // choice between them. Only the FONT TYPE differs, and that is the migration itself:
                // main is pre-C15 and still holds a UnityEngine.Font, which cannot be handed to a
                // TextMeshProUGUI at all.
                TMP_FontAsset font = TestFont(receipt);
                // MEASURED from the rendered header, never a literal. This was 280f — the width the
                // receipt had when it lived in the 324px working margin — and E-07 moved receipts to
                // the 700px sheet without this following. The fixture then fitted to a narrower box
                // than the render used, so it expected an ellipsis the surface had not drawn. The
                // comment above already promised "not a duplicated literal" for the formula; the
                // width was the literal nobody noticed.
                //
                // **It presented as an intermittent one-character failure and was filed as a
                // font-atlas flake. That was wrong** — kept from main's side, because a misdiagnosis
                // on the record is worth more than the fix.
                //
                // And the mechanism, kept from this side, because it explains why the wrong
                // diagnosis was so easy: the test and the renderer only disagree when a label's
                // fitted width falls BETWEEN the two numbers. Shorter than 280 and both return the
                // string untouched; longer than the real width and both truncate identically. Only
                // the band between them fails, so this passed on every run whose generated names
                // happened to be short enough and failed on the first one that was not — which is
                // exactly what noise looks like until you measure it.
                // **Read off a LEG ROW now, not the header.** S70(3) split the header into a narrow
                // identity and a right-aligned leg count, so it is no longer the receipt's text
                // width — reading it there would under-report by 40% and quietly re-create the exact
                // stale-width defect this line was written to kill, one element over. The leg rows
                // are what this width actually governs, so they are what it is measured from.
                float receiptTextWidth = ((RectTransform)Required(receipt, "TicketLeg0")).rect.width;

                // S70(3): the kit's header grammar — identity and leg count as separate elements at
                // separate trackings. **Asserted separately because the separation IS the ruling**;
                // one concatenated string is precisely what it replaced, so a single assertion over
                // a joined string would pass while conforming to nothing.
                Assert.AreEqual(identity, TextOf(Required(receipt, "ReceiptHeader")),
                    "S70(3): the header carries the identity alone");
                Assert.AreEqual($"{ticket.Legs.Count} {SportsbookApp.Pluralize(ticket.Legs.Count, "LEG")}",
                    TextOf(Required(receipt, "ReceiptLegCount")),
                    "S70(3): the count is the kit's `key` cell, not part of the identity string");

                // The money facts moved to the footer row, which is where the kit has always had
                // them. Asserted against the model rather than the render's own expression, so the
                // fixture cannot agree with a defect (S62's lesson).
                Assert.AreEqual(Money(model.Stake), TextOf(Required(receipt, "ReceiptStakeValue")));
                Assert.AreEqual(OddsFormat.American(model.Combined),
                    TextOf(Required(receipt, "ReceiptCombinedValue")));
                Assert.AreEqual(Money(model.Payout), TextOf(Required(receipt, "ReceiptPaysValue")));

                for (int legIndex = 0; legIndex < ticket.Legs.Count; legIndex++)
                {
                    Leg leg = ticket.Legs[legIndex];
                    // C15/S28: the expectation carries LaptopTrack.Records because the render does.
                    // This assertion is computed through the same helper the surface uses, so it
                    // would follow a tracking change silently if the value were omitted here — it
                    // would simply assert a narrower string and pass against a build that trims
                    // somewhere else. Threading it is what keeps this a real check of S26's
                    // no-silent-truncation rule rather than a tautology.
                    Assert.AreEqual(
                        LaptopUi.FitLabelKeepingSuffix(font, $"{legIndex + 1}. ",
                            SportsbookApp.CompactLegLabel(leg.Matchup, leg.Selection),
                            $"  {OddsFormat.American(leg.OfferedOdds)}", 13, receiptTextWidth,
                            LaptopTrack.Records),
                        TextOf(Required(receipt, "TicketLeg" + legIndex)));
                }
            }
        }

        private static IEnumerator Boot()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
            Assert.IsNotNull(load, "Room scene is not available");
            while (!load.isDone) yield return null;

            LaptopScreen laptop = Laptop();
            float start = Time.realtimeSinceStartup;
            while (laptop.director == null || laptop.director.Run == null || laptop.Os.OnDesktop)
            {
                if (Time.realtimeSinceStartup - start > 10f)
                {
                    Assert.Fail("SureThing did not reach the betting lobby within 10 seconds");
                    yield break;
                }
                yield return null;
            }
            yield return null;
        }

        private static IEnumerator OpenEntry(LaptopScreen laptop)
        {
            Invoke(Required(Required(App(laptop), "Matchup0"), "Details"));
            yield return WaitForRebuild();
            Assert.AreEqual(SportsbookApp.Tab.Detail, laptop.Os.CurrentTab);
        }

        private static IEnumerator WaitForRebuild()
        {
            yield return null;
            yield return null;
        }

        private static LaptopScreen Laptop()
        {
            LaptopScreen laptop = UnityEngine.Object.FindAnyObjectByType<LaptopScreen>();
            Assert.IsNotNull(laptop, "LaptopScreen missing");
            return laptop;
        }

        private static Transform App(LaptopScreen laptop) => Required(laptop.transform, "App");

        private static void AssertEntryShell(Transform app)
        {
            AssertRect(Required(app, "Chrome") as RectTransform, 1024f, 140f, "chrome");
            AssertRect(Required(app, "NotebookRail") as RectTransform, 1024f, 34f, "rail");
            AssertRect(Required(app, "FormTabs") as RectTransform, 1024f, 38f, "tabs");
            AssertRect(Required(app, "FormMasthead") as RectTransform, 1024f, 68f, "masthead");
            AssertRect(Required(app, "EntryBoard") as RectTransform, 700f, 530f, "ENTRY board");
            AssertRect(Required(app, "WorkingMargin") as RectTransform, 324f, 530f,
                "working margin");
            AssertRect(Required(app, "NotebookTray") as RectTransform, 1024f, 34f, "tray");
        }

        private static Image WideRing(Transform app)
        {
            Transform ring = Required(Required(app, "MarketBody"), "WideBiroRing");
            Image image = ring.GetComponent<Image>();
            Assert.IsNotNull(image, "WideBiroRing must be an Image");
            return image;
        }

        private static Button FirstNamedButton(Transform root, string prefix)
        {
            var matches = new List<Button>();
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button.name.StartsWith(prefix, StringComparison.Ordinal))
                    matches.Add(button);
            matches.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
            Assert.Greater(matches.Count, 0,
                $"No button beginning '{prefix}' exists beneath '{root.name}'");
            return matches[0];
        }

        private static Transform Required(Transform root, string name)
        {
            Transform found = Find(root, name);
            Assert.IsNotNull(found, $"Required named UI node '{name}' missing beneath '{root.name}'");
            return found;
        }

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = Find(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private static Transform FindPrefix(Transform root, string prefix)
        {
            if (root.name.StartsWith(prefix, StringComparison.Ordinal)) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindPrefix(root.GetChild(i), prefix);
                if (found != null) return found;
            }
            return null;
        }

        private static void Invoke(Transform node)
        {
            Button button = node.GetComponent<Button>();
            Assert.IsNotNull(button, $"{node.name} must be a button");
            Assert.IsTrue(button.interactable, $"{node.name} must be interactable");
            button.onClick.Invoke();
        }

        private static string TextOf(Transform node)
        {
            TMP_Text text = node.GetComponent<TMP_Text>();
            if (text == null) text = node.GetComponentInChildren<TMP_Text>();
            Assert.IsNotNull(text, $"{node.name} has no readable text");
            return text.text;
        }

        /// <summary>The same built-in font LaptopScreen loads, fetched independently so fixture-side
        /// width calculations measure glyphs identically to production.</summary>
        /// <summary>
        /// The face the receipt is actually rendered in, read off the rendered element rather than
        /// assumed. This fixture recomputes expected strings through the same FitText the runtime
        /// uses, so it has to measure with the same font: it was pinned to LegacyRuntime and started
        /// failing the moment the production faces were wired, because Archivo Narrow is narrower
        /// and more characters now fit before the ellipsis. That was the fixture being wrong about
        /// the font, not the UI being wrong about the text.
        /// </summary>
        private static TMP_FontAsset TestFont(Transform receipt)
        {
            var sample = Required(receipt, "ReceiptHeader").GetComponent<TMP_Text>();
            Assert.IsNotNull(sample, "ReceiptHeader must carry a Text to measure against");
            Assert.IsNotNull(sample.font, "ReceiptHeader has no font; the production face failed to load");
            return sample.font;
        }

        private static string AllText(Transform root)
        {
            var content = new List<string>();
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                content.Add(text.name + "=" + text.text);
            content.Sort(StringComparer.Ordinal);
            return string.Join("\n", content);
        }

        private static string Money(double value)
        {
            long rounded = (long)Math.Round(value, MidpointRounding.AwayFromZero);
            return "$" + rounded.ToString("N0", CultureInfo.InvariantCulture);
        }

        private static void AssertRect(RectTransform rect, float width, float height, string label)
        {
            Assert.IsNotNull(rect, $"{label} RectTransform missing");
            Assert.AreEqual(width, rect.sizeDelta.x, 0.01f, $"{label} width");
            Assert.AreEqual(height, rect.sizeDelta.y, 0.01f, $"{label} height");
        }

        private sealed class ReceiptExpectation
        {
            public readonly double Stake;
            public readonly double Combined;
            public readonly double Payout;

            public ReceiptExpectation(double stake, double combined, double payout)
            {
                Stake = stake;
                Combined = combined;
                Payout = payout;
            }
        }

        private sealed class EntryPersistenceSnapshot
        {
            public string Machine;
            public string Sticker;
            public string Clock;
            public string EventIdentity;
            public string EventRecords;
            public string EventForm;
            public string MarginTitle;
            public string MarginCount;
            public string WorkingLeg;
            public string Stake;
            public string Payout;
            public string ReceiptHeader;
            public int TicketCount;
            public int PickCount;
            public double ModelStake;
            public MarketSelection? Selection;
        }
    }
}
