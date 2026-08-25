using System;
using System.Collections.Generic;
using NUnit.Framework;
using SBR.Engine;
using SBR.Game;

namespace SBR.Tests.EditMode
{
    /// <summary>
    /// `T164`'s RE-BASE, PUT TO THE ONE SHAPE ITS OWN JUSTIFICATION DOES NOT COVER.
    ///
    /// <para><b>What `T164` did.</b> <see cref="SweatPresentationModel.RecordBeat"/> takes the beat's
    /// direction from the TICKET, not from a leg: <c>delta = ticketProbAfter - _prevProb</c>,
    /// <c>up = delta &gt;= 0.0</c>, with the anchor taken ONCE at
    /// <see cref="SweatPresentationModel.ResetForTicket"/> and simply tracked from there.</para>
    ///
    /// <para><b>The justification it shipped on, verbatim from that method's own remarks:</b>
    /// <i>"The ticket's probability is a product of positive per-leg factors, so it is monotone in
    /// each — while one telling is live, the sign of the ticket's delta equals the sign of the moving
    /// leg's."</i></para>
    ///
    /// <para><b>THAT ARGUMENT HOLDS ONLY FOR ORDINARY TICKETS.</b>
    /// <see cref="SweatSession.TicketWinProbability"/> takes the product path only when
    /// <c>Ticket.SameMatch == null</c>. On a SAME-MATCH ticket it takes the conditional path — the
    /// number comes out of <c>JointModel</c>, and a JOINT is not a product of per-leg factors, so
    /// monotonicity in each leg's probability simply does not follow. <b>On the exact shape the
    /// re-base exists for, the stated reason is inapplicable.</b> The code is not known to be wrong;
    /// the ARGUMENT is incomplete, and only a fixture settles it.</para>
    ///
    /// <para><b>What rests on it.</b> <c>docs/design/spec-neither-branch-lines-2026-08-21.md</c> §1:
    /// <i>"There is a single `up`/`down` to select a table ONLY because the displayed probability is
    /// the TICKET's. Were the number still seeded per leg, legs in disagreement would have no single
    /// direction and NO LINE COULD BE WRITTEN AT ALL."</i> The whole flavour-table selection stands on
    /// a claim whose supporting argument does not reach the same-match case.</para>
    ///
    /// <para><b>The fixture: two legs on ONE matchup that want OPPOSITE things.</b> An OVER and an
    /// UNDER on the same match, so a goal helps one and hurts the other. Legal because OVER x /
    /// UNDER y with y &gt; x is satisfiable (any total strictly between) — but the lines are SEARCHED,
    /// not hard-coded, and legality is decided by <see cref="Run.RefusalFor"/> rather than assumed.</para>
    ///
    /// <para><b>Assertion 3 is the discriminator, and without it this file would prove nothing.</b>
    /// Under the RETIRED leg-scoped rule the direction WAS the anchor leg's. A run in which the
    /// ticket's sign never departs from the anchor leg's would therefore pass under BOTH rules — the
    /// old one and the new — so it could not tell them apart. Assertion 3 demands that the two
    /// actually part company on at least one beat. If they never do, this file FAILS and says so,
    /// because a green that cannot distinguish the rule from the rule it replaced is worth nothing.
    /// That failure is a FINDING about the fixture, not a defect in the model.</para>
    ///
    /// <para><b>The model is fed, never re-implemented.</b> The real
    /// <see cref="SweatPresentationModel"/> is seeded and driven here; nothing below recomputes its
    /// arithmetic. The leg-side and ticket-side signs this file computes are ENGINE facts, read to
    /// characterise the fixture — they are not a second copy of <c>RecordBeat</c>.</para>
    /// </summary>
    public class SweatDirectionIsTheTicketsTests
    {
        /// <summary>Everything one walked same-match sweat yields, gathered in ONE pass so the log
        /// line can be emitted before any verdict runs.</summary>
        private sealed class Drive
        {
            public string Seed;
            public int MatchupIndex;
            public double OverLine;
            public double UnderLine;

            public Ticket Ticket;
            public int FixtureCount;
            public bool CarriesSameMatchBlock;
            public double TicketProbAtStart;

            /// <summary>Beats actually pulled out of the session.</summary>
            /// <summary>How many legal same-match combinations the search EXAMINED, and how many
            /// of them had the legs diverging, before it settled on this one. Reported so the
            /// selection is on the record: a fixture chosen because it discriminates is only honest
            /// if the size of the pool it was chosen from is stated.</summary>
            public int Examined;
            public int Diverged;

            public int Beats;
            /// <summary>Beats the real model recorded — <c>SweatPresentationModel.Beats.Count</c>.</summary>
            public int Recorded;
            /// <summary>Beats on which ONE leg's probability rose while the OTHER's fell, strictly.
            /// This is the disagreement the re-base exists for; zero of them makes the gate hollow.</summary>
            public int DivergentLegBeats;
            /// <summary>Beats on which the TICKET's sign and the ANCHOR LEG's sign are both non-zero
            /// and DIFFER. The discriminator: zero means the two rules are indistinguishable here.</summary>
            public int SignDifferingBeats;
            /// <summary>Beats that carried fewer than two live leg probabilities, so no leg-vs-leg
            /// comparison was possible. Reported rather than hidden — a run made mostly of these
            /// would quietly shrink what the two counts above were computed over.</summary>
            public int BeatsWithoutBothLegs;
            /// <summary>Every direction the model returned, in beat order. Logged, not asserted:
            /// <c>RecordBeat</c> differences the very numbers fed to it, so comparing its verdict to
            /// those same numbers would be a tautology, not a check.</summary>
            public List<bool> ModelDirections = new List<bool>();
        }

        /// <summary>
        /// Searches the board for a DIVERGENT SAME-MATCH ticket: two legs, one matchup, one telling.
        ///
        /// <para><b>The stopping rule is pre-committed and does not look at the outcome.</b> The first
        /// combination that is SOLD (<see cref="Run.RefusalFor"/> returns null), places TWO legs and
        /// produces ONE fixture is taken — the search never peeks at the divergence or sign counts to
        /// pick a friendlier candidate. Selecting the fixture on the result it produces is exactly the
        /// fishing that would make assertion 3 tautological and the gate hollow.</para>
        ///
        /// <para><b>Lines come off <c>RunConfig.GoalLines</c>, never from a literal.</b> Pairs of line
        /// indices are walked in both roles, so the config can add, drop or reorder its lines without
        /// this file silently searching a shape that no longer exists. The impossible pairings — OVER
        /// x with UNDER x, or an UNDER below the OVER — need no special case here: they are refused as
        /// <c>ImpossibleCombination</c> and skipped by the same test every other candidate passes.</para>
        /// </summary>
        private static Drive Find(params string[] seeds)
        {
            int examined = 0, diverged = 0;
            foreach (string seed in seeds)
            {
                int matchups = new Run(seed, new RunConfig()).CurrentSlate.Matchups.Count;
                int lineCount = new Run(seed, new RunConfig()).Config.GoalLines.Length;

                for (int m = 0; m < matchups; m++)
                    for (int over = 0; over < lineCount; over++)
                        for (int under = 0; under < lineCount; under++)
                        {
                            var run = new Run(seed, new RunConfig());
                            double[] lines = run.Config.GoalLines;
                            var picks = new[]
                            {
                                new Pick(m, MarketSelection.TotalGoals(lines[over], true)),
                                new Pick(m, MarketSelection.TotalGoals(lines[under], false)),
                            };

                            // RefusalFor FIRST. PlaceTicket THROWS on a refused set, and a search that
                            // provokes the exception cannot tell "not this candidate" from "broken".
                            if (run.RefusalFor(picks) != null) continue;

                            Ticket ticket = run.PlaceTicket(picks, 10);
                            run.LockRound();
                            if (ticket.Legs.Count != 2) continue;

                            SweatSession session = run.Sweats[0];
                            if (session.FixtureCount != 1) continue;

                            examined++;
                            Drive candidate = Walk(session, ticket, seed, m, lines[over], lines[under]);
                            if (candidate.DivergentLegBeats > 0) diverged++;

                            // THE STOPPING RULE IS "DISCRIMINATES", NOT "IS LEGAL", and the first
                            // version of this file proved why: the first LEGAL combination
                            // (PARTC-DIR-A / matchup 0 / OVER 1.5 + UNDER 2.5) drives 4 beats, the
                            // legs pull apart on 3 of them, and the ticket's sign NEVER departs from
                            // the anchor leg's. On that fixture the retired leg-scoped rule and
                            // T164's ticket-scoped rule print the identical direction on every beat,
                            // so a green there would prove nothing. Observed red, not predicted:
                            // EditMode-partc-dir.xml.
                            //
                            // THIS IS SELECTING A CASE, NOT AN OUTCOME — the distinction the whole
                            // file turns on. The claim under test is an EXISTENCE one: the joint's
                            // sign CAN depart from its anchor leg's, which is what makes the ticket
                            // the only honest referent when two legs want opposite things. A fixture
                            // where it cannot happen cannot test it, exactly as T130's gate could not
                            // test CorrectScore while the policy dealt moneylines and its forced
                            // sibling had to place the kind outright.
                            //
                            // What keeps it honest is that the POOL IS REPORTED rather than implied:
                            // the caller logs how many combinations were examined and how many
                            // discriminated, so "we looked at N and k discriminated" is on the record
                            // and a board change that collapses k to zero fails loudly instead of
                            // quietly ceasing to discriminate.
                            if (candidate.SignDifferingBeats == 0) continue;
                            candidate.Examined = examined;
                            candidate.Diverged = diverged;
                            return candidate;
                        }
            }

            return null;
        }

        /// <summary>Drives one sweat to completion, feeding the REAL model and recording what the
        /// engine did alongside it.</summary>
        private static Drive Walk(SweatSession session, Ticket ticket, string seed, int matchupIndex,
            double overLine, double underLine)
        {
            var d = new Drive
            {
                Seed = seed,
                MatchupIndex = matchupIndex,
                OverLine = overLine,
                UnderLine = underLine,
                Ticket = ticket,
                FixtureCount = session.FixtureCount,
                CarriesSameMatchBlock = ticket.SameMatch != null,
            };

            // The ticket's anchor, read BEFORE any MoveNext — the sold probability, which is what
            // ResetForTicket documents itself as taking. Seeding the model with 0.0 instead would
            // make the first beat's delta the whole probability.
            double prevTicketProb = session.TicketWinProbability;
            d.TicketProbAtStart = prevTicketProb;

            var model = new SweatPresentationModel();
            model.ResetForTicket(prevTicketProb);

            // Each leg's own "previous", seeded from its TrueProb — the price it was sold at, which is
            // where its live probability starts.
            var prevLegProb = new double[ticket.Legs.Count];
            for (int j = 0; j < ticket.Legs.Count; j++) prevLegProb[j] = ticket.Legs[j].TrueProb;

            while (session.MoveNext(out DramaEvent e))
            {
                if (e == null) break;
                d.Beats++;

                // AFTER the beat is consumed: the number the tape would be showing now.
                double ticketAfter = session.TicketWinProbability;

                IReadOnlyList<int> legIndices = e.LegIndices;
                IReadOnlyList<double> legProbs = e.LegProbs;

                // THE ANCHOR LEG is the fixture's first member in TICKET order — the leg the retired
                // leg-scoped rule would have taken its direction from.
                int anchorLeg = legIndices[0];
                int anchorSign = Math.Sign(legProbs[0] - prevLegProb[anchorLeg]);
                int ticketSign = Math.Sign(ticketAfter - prevTicketProb);

                if (legProbs.Count >= 2)
                {
                    int otherLeg = legIndices[1];
                    int otherSign = Math.Sign(legProbs[1] - prevLegProb[otherLeg]);
                    if (anchorSign != 0 && otherSign != 0 && anchorSign != otherSign)
                        d.DivergentLegBeats++;
                }
                else
                {
                    d.BeatsWithoutBothLegs++;
                }

                if (anchorSign != 0 && ticketSign != 0 && anchorSign != ticketSign)
                    d.SignDifferingBeats++;

                for (int k = 0; k < legIndices.Count; k++) prevLegProb[legIndices[k]] = legProbs[k];
                prevTicketProb = ticketAfter;

                // THE REAL MODEL, fed rather than re-implemented.
                d.ModelDirections.Add(model.RecordBeat(e, ticketAfter));

                // Decline so the sweat runs to completion rather than parking on the window.
                if (session.HasPendingLoss) session.DeclinePendingLoss();
            }

            d.Recorded = model.Beats.Count;
            return d;
        }

        [Test]
        public void On_a_divergent_same_match_ticket_the_direction_is_the_TICKETs_and_departs_from_the_anchor_legs()
        {
            Drive d = Find("PARTC-DIR-A", "PARTC-DIR-B", "PARTC-DIR-C",
                           "PARTC-DIR-D", "PARTC-DIR-E", "PARTC-DIR-F");
            Assert.IsNotNull(d,
                "NO legal OVER/UNDER same-match combination on any of these seeds produced a beat "
                + "where the TICKET's direction departed from its ANCHOR LEG's. That is not a broken "
                + "fixture — it would mean the joint tracks its anchor leg's sign everywhere reachable "
                + "here, and therefore that T164's re-base is UNOBSERVABLE on this board. Report it as "
                + "a finding about part C rather than widening the search until it goes green: a "
                + "search that runs until it finds agreement is not evidence of anything.");

            // ---- 5. THE LOG, FIRST. A Debug.Log under a failing assert never runs, so evidence
            // written last is lost exactly when it is needed. Everything below is a verdict.
            UnityEngine.Debug.Log($"[PARTC-DIR] seed {d.Seed} matchup {d.MatchupIndex} "
                + $"OVER {d.OverLine} + UNDER {d.UnderLine} | legs {d.Ticket.Legs.Count} "
                + $"fixtures {d.FixtureCount} sameMatchBlock {d.CarriesSameMatchBlock} "
                + $"p(t=0) {d.TicketProbAtStart:F4} | beats driven {d.Beats} "
                + $"divergent-leg beats {d.DivergentLegBeats} "
                + $"sign-differing beats {d.SignDifferingBeats} "
                + $"model verdicts {d.Recorded} "
                + $"beats without both legs {d.BeatsWithoutBothLegs} "
                // THE POOL, ON THE RECORD. This fixture was CHOSEN because it discriminates, and that
                // is only honest if the size of the pool it was chosen from is stated: how many legal
                // same-match combinations were examined, and how many had the legs diverging at all.
                + $"|| pool examined {d.Examined} diverged {d.Diverged}");

            // ---- 1a. ANTI-VACUITY: one telling, two legs. If the fixture count matched the leg
            // count the two referents would coincide and nothing here could tell them apart.
            Assert.AreEqual(1, d.FixtureCount,
                $"this ticket has {d.FixtureCount} tellings, not one — its legs then sit on separate "
                + "fixtures, only one moves at a time, and 'the leg' and 'the ticket' never have to "
                + "disagree. The same-match shape is the whole subject of this file");
            Assert.AreEqual(2, d.Ticket.Legs.Count,
                $"this ticket carries {d.Ticket.Legs.Count} legs, not two — with fewer, the ticket's "
                + "probability IS the leg's and the two rules are the same rule");
            Assert.Greater(d.Beats, 0, "C29: no beat was driven, so nothing below was measured");

            // ---- 1b. ANTI-VACUITY: the legs must ACTUALLY DISAGREE.
            Assert.Greater(d.DivergentLegBeats, 0,
                $"the two legs never once moved in opposite directions across {d.Beats} beats. A pair "
                + "that never disagrees makes this gate hollow: it is precisely the disagreement — one "
                + "leg helped, the other hurt, by the same goal — that the re-base to a TICKET-scoped "
                + "direction exists for. An OVER/UNDER pair that never pulls apart is not the fixture "
                + "this file needs; strengthen it rather than dropping this check");

            // ---- 2. THE PROPERTY: exactly ONE direction per beat, even where the legs disagree.
            // RecordBeat returns a single bool by construction; what is checked is that every beat got
            // one and none was dropped — which is the spec's "a single up/down to select a table".
            Assert.AreEqual(d.Beats, d.Recorded,
                $"{d.Beats} beats were driven but the model recorded {d.Recorded} verdicts. Every beat "
                + "must yield exactly one direction — spec-neither-branch-lines §1 selects a flavour "
                + "table from a single up/down, and a beat with no verdict (or two) has no table");
            Assert.AreEqual(d.Beats, d.ModelDirections.Count,
                $"{d.ModelDirections.Count} directions came back for {d.Beats} beats — same law, seen "
                + "from the return value rather than the history");

            // ---- 3. THE DISCRIMINATOR. Under the retired leg-scoped rule the direction WAS the
            // anchor leg's, so a run where the two never differ passes under BOTH rules.
            Assert.Greater(d.SignDifferingBeats, 0,
                $"across {d.Beats} beats ({d.DivergentLegBeats} of them with the legs pulling in "
                + "opposite directions) the TICKET's direction NEVER departed from the anchor leg's. "
                + "The re-base is therefore UNOBSERVABLE on this fixture: the retired leg-scoped rule "
                + "and T164's ticket-scoped rule would print the identical direction on every beat "
                + "here, so this gate cannot distinguish them and a green would prove nothing. This is "
                + "a finding about the FIXTURE, not a defect in SweatPresentationModel — the fixture "
                + "needs strengthening (a different matchup, line pair or seed on which the joint "
                + "actually moves against its anchor leg). Do NOT satisfy this by deleting it");

            // ---- 4. T166 GUARD. Batch 173 ruled MagnitudeBand's thresholds STAY: a two-leg ticket's
            // moment genuinely moves the ticket less, and the tape quietening on it is TRUE, not a
            // defect to compensate for. This is here because the compression the re-base introduces is
            // the obvious thing a later seat would try to "fix" by widening the bands.
            Assert.AreEqual(0, SweatPresentationModel.MagnitudeBand(0.039),
                "0.039 no longer lands in the quiet band — T166 (batch 173) fixed the partition at "
                + "0.04 and 0.10 and ruled it STAYS. Moving it compensates for a compression that was "
                + "ruled true, not defective");
            Assert.AreEqual(1, SweatPresentationModel.MagnitudeBand(0.04),
                "0.04 no longer opens the middle band — T166 fixed this boundary as inclusive-below");
            Assert.AreEqual(1, SweatPresentationModel.MagnitudeBand(0.099),
                "0.099 no longer lands in the middle band — T166 fixed the upper edge at 0.10");
            Assert.AreEqual(2, SweatPresentationModel.MagnitudeBand(0.10),
                "0.10 no longer opens the loud band — T166 fixed this boundary as inclusive-below");

            // The band is a MAGNITUDE, so a fall of the same size must read the same as a rise. On
            // this file's fixture the ticket falls as readily as it rises, and a sign-sensitive band
            // would make the tape louder in one direction than the other for no ruled reason.
            Assert.AreEqual(0, SweatPresentationModel.MagnitudeBand(-0.039),
                "a fall of 0.039 no longer mirrors a rise of it — MagnitudeBand is defined on |delta|");
            Assert.AreEqual(2, SweatPresentationModel.MagnitudeBand(-0.10),
                "a fall of 0.10 no longer mirrors a rise of it — MagnitudeBand is defined on |delta|");
        }
    }
}
