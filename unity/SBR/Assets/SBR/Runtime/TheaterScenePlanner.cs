using System;
using System.Collections.Generic;

namespace SBR.Game
{
    /// <summary>
    /// One point in the truth-constrained candidate space a <see cref="SceneFactContract"/> owns.
    /// Private construction detail of <see cref="TheaterScenePlanner"/>'s catalog, exposed publicly
    /// only so tests can assert §7.3's variety floor against the real catalog rather than trusting
    /// it (PRD §7.3's closing instruction: "assert catalog counts rather than trusting them").
    /// </summary>
    public readonly struct SceneCandidate : IEquatable<SceneCandidate>
    {
        public readonly SceneFactContract Contract;
        public readonly MovementGrammar Grammar;
        public readonly ChanceShape? ChanceShape;
        public readonly ScenePayoff Payoff;
        public readonly PressureMode Pressure;
        public readonly SpacingMode Spacing;
        public readonly ReactionPattern Reaction;
        public readonly bool HasWinTheCornerLeadIn;

        /// <summary>Null: legal for every <see cref="SceneTemplate"/> that classifies to
        /// <see cref="Contract"/>. Non-null: legal only for that one template. Needed because
        /// §7.2's table groups multiple templates under one fact-contract row (Kickoff+Fallback
        /// under Structural; LegFinalWon+LegFinalLost under Final) whose legal movement/payoff
        /// sets are NOT identical to each other, unlike every other grouped row (Goal's four
        /// templates, NearMiss's two, Territory's three, Corner's two DO share one identical set,
        /// so their candidates leave this null).</summary>
        public readonly SceneTemplate? RestrictToTemplate;

        public SceneCandidate(SceneFactContract contract, MovementGrammar grammar, ChanceShape? chanceShape,
            ScenePayoff payoff, PressureMode pressure, SpacingMode spacing, ReactionPattern reaction,
            bool hasWinTheCornerLeadIn, SceneTemplate? restrictToTemplate = null)
        {
            Contract = contract;
            Grammar = grammar;
            ChanceShape = chanceShape;
            Payoff = payoff;
            Pressure = pressure;
            Spacing = spacing;
            Reaction = reaction;
            HasWinTheCornerLeadIn = hasWinTheCornerLeadIn;
            RestrictToTemplate = restrictToTemplate;
        }

        public bool Equals(SceneCandidate other)
            => Contract == other.Contract && Grammar == other.Grammar && ChanceShape == other.ChanceShape
            && Payoff == other.Payoff && Pressure == other.Pressure && Spacing == other.Spacing
            && Reaction == other.Reaction && HasWinTheCornerLeadIn == other.HasWinTheCornerLeadIn
            && RestrictToTemplate == other.RestrictToTemplate;

        public override bool Equals(object obj) => obj is SceneCandidate other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + (int)Contract;
                h = h * 31 + (int)Grammar;
                h = h * 31 + (ChanceShape.HasValue ? (int)ChanceShape.Value + 1 : 0);
                h = h * 31 + (int)Payoff;
                h = h * 31 + (int)Pressure;
                h = h * 31 + (int)Spacing;
                h = h * 31 + (int)Reaction;
                return h;
            }
        }
    }

    /// <summary>
    /// PRD §7.4's revealed-history ring buffer. See the "impossible, not merely discouraged" and
    /// "stage/commit split" reasoning in the doc below — both are load-bearing design decisions,
    /// not incidental implementation details.
    ///
    /// <para><b>Why reading ahead is structurally impossible, not just discouraged.</b> This type
    /// has exactly one mutator, <see cref="Record"/>, which appends a single entry and has no
    /// counterpart that accepts a batch, a future index, or "the rest of the timeline". There is
    /// no constructor that takes a pre-built list either. The only way an entry gets into this
    /// buffer is one <see cref="Record"/> call at a time, made by whoever just finished planning
    /// THAT beat. So at the instant <see cref="TheaterScenePlanner.Plan"/> runs for beat N, this
    /// object's contents are, by construction, whatever the caller has recorded so far — which,
    /// in any real call sequence, can only be beats before N, because beat N's own signature does
    /// not exist to record until <c>Plan</c> itself returns it. The planner is never handed a
    /// list of "all beats" it could index past the current one; the type simply does not offer
    /// that shape of data.</para>
    /// </summary>
    public sealed class TheaterSceneHistory
    {
        public readonly struct HistoryEntry
        {
            public readonly PlanSignature Signature;
            public readonly SceneFactContract FactContract;
            public readonly bool IsStructural;

            public HistoryEntry(PlanSignature signature, SceneFactContract factContract, bool isStructural)
            {
                Signature = signature;
                FactContract = factContract;
                IsStructural = isStructural;
            }
        }

        private readonly int _capacity;
        private readonly List<HistoryEntry> _entries;

        /// <summary>12 is generous headroom over what any §7.4 rule actually reads (the deepest
        /// lookback is rule 4's "last three non-structural", and structural entries can push that
        /// window further back in the raw buffer) — cheap to keep, comfortably avoids the window
        /// ever running out of real entries to skip past.</summary>
        public TheaterSceneHistory(int capacity = 12)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            _capacity = capacity;
            _entries = new List<HistoryEntry>(capacity);
        }

        public int Count => _entries.Count;

        /// <summary>PRD §7.4 rule 3 reads exactly this: "the immediately previous signature".
        /// Structural entries count here — a repeated kickoff is still a repeat.</summary>
        public HistoryEntry? MostRecent => _entries.Count == 0 ? (HistoryEntry?)null : _entries[_entries.Count - 1];

        /// <summary>The only mutator. Appends, then evicts the oldest entry past capacity —
        /// a genuine ring buffer, never a random-access rewrite.</summary>
        public void Record(PlanSignature signature, SceneFactContract factContract, bool isStructural)
        {
            _entries.Add(new HistoryEntry(signature, factContract, isStructural));
            if (_entries.Count > _capacity) _entries.RemoveAt(0);
        }

        /// <summary>PRD §7.4 rule 4: "the last three non-structural scenes". Structural entries
        /// (Kickoff/Fallback) are real entries in the buffer — <see cref="MostRecent"/> sees them —
        /// but this query walks past them as if they were not there, so a kickoff sandwiched
        /// between two goals never "uses up" one of the three grammar-freshness slots that
        /// rule 4 actually cares about. See <see cref="TheaterScenePlanner"/>'s type doc for why.</summary>
        public HashSet<MovementGrammar> RecentNonStructuralGrammars(int maxCount)
        {
            var result = new HashSet<MovementGrammar>();
            int found = 0;
            for (int i = _entries.Count - 1; i >= 0 && found < maxCount; i--)
            {
                if (_entries[i].IsStructural) continue;
                result.Add(_entries[i].Signature.Grammar);
                found++;
            }
            return result;
        }

        /// <summary>PRD §7.4 rule 5: "the last two compatible scenes" — compatible means the same
        /// fact contract as the scene currently being planned. Payoff/reaction vocabularies are
        /// contract-specific by construction (§7.2), so comparing a Corner's payoff against a
        /// Booking's would never usefully collide anyway; scoping the lookback to the same
        /// contract makes that explicit rather than accidental.</summary>
        public HashSet<(ScenePayoff Payoff, ReactionPattern Reaction)> RecentCompatiblePayoffReactionCombos(
            SceneFactContract contract, int maxCount)
        {
            var result = new HashSet<(ScenePayoff, ReactionPattern)>();
            int found = 0;
            for (int i = _entries.Count - 1; i >= 0 && found < maxCount; i--)
            {
                if (_entries[i].FactContract != contract) continue;
                result.Add((_entries[i].Signature.Payoff, _entries[i].Signature.Reaction));
                found++;
            }
            return result;
        }
    }

    /// <summary>
    /// Phase 2B (PRD §7, §9): elaborates a factual <see cref="SceneSpec"/> into a rich, deterministic
    /// <see cref="TheaterScenePlan"/>. Standalone — nothing in this file is called by
    /// <see cref="TheaterStage"/> or <see cref="TheaterChoreographer"/> yet (Phase 2C wires that up).
    /// Constructing or calling anything here has no effect on what currently renders.
    ///
    /// <para><b>Truth filtering is structural, not conventional (PRD §7.2, §11 boundary check).</b>
    /// <see cref="ClassifyFactContract"/> reads the incoming <see cref="SceneSpec"/> and returns
    /// which of §7.2's table rows this beat belongs to. Every legal candidate for a given
    /// <see cref="SceneFactContract"/> lives in its OWN dedicated block inside
    /// <see cref="BuildCatalog"/> — e.g. the Territory block's payoff loop array literally never
    /// contains <see cref="ScenePayoff.Goal"/> or <see cref="ScenePayoff.BookingMarker"/>. There is
    /// no code path anywhere in this file that builds a cross-contract candidate and then checks
    /// whether it happens to be legal — illegal combinations are never representable in the first
    /// place, because the generator that would have to produce one does not exist. <see cref="Plan"/>
    /// itself never widens or overrides a contract once classified, and never touches the score or
    /// count ledgers — it only reads the already-resolved facts <see cref="SceneSpec"/> carries.</para>
    ///
    /// <para><b>The signature (repetition-control identity) excludes lane, on purpose.</b> This
    /// dispatch exists because prior scene "variety" was really just re-rolling
    /// <c>SceneSpec.Variant</c>'s lane — the same script, a different flank. If <see cref="SceneLane"/>
    /// were part of <see cref="PlanSignature"/>, the planner could satisfy §7.4's repetition rules by
    /// doing exactly that again: flip the lane, keep everything else identical, call it "different".
    /// Excluding lane forces rules 3–5 to only ever recognize a scene as fresh when its GRAMMAR,
    /// CHANCE SHAPE, PAYOFF, PRESSURE, SPACING, or REACTION actually changed — the dimensions that
    /// make a couch viewer read two beats as different stories. Lane still varies (via
    /// <see cref="PickLane"/>, its own independent, deterministic, key-driven choice) — it is just
    /// never what repetition control is allowed to call "enough".
    ///
    /// The signature also excludes every <see cref="PresentationSceneKey"/> identity field (run
    /// seed, round, ticket, match, event step): those identify WHICH BEAT this is, not what the
    /// beat looked like. Folding them in would make every signature unique by construction (two
    /// different beats can never share an event step), which would make rules 3–5 permanently
    /// vacuous — nothing would ever "repeat" because nothing would ever be comparable. The
    /// signature keeps exactly the seven dimensions whose repetition would actually read as "the
    /// same move again" — <see cref="SceneTemplate"/> (which already carries the for/against truth
    /// distinction a bare fact-contract label would lose) plus <see cref="MovementGrammar"/>,
    /// <see cref="ChanceShape"/>, <see cref="ScenePayoff"/>, <see cref="PressureMode"/>,
    /// <see cref="SpacingMode"/>, <see cref="ReactionPattern"/>.</para>
    ///
    /// <para><b>Structural scenes (Kickoff, Fallback) are real history for some rules and invisible
    /// for one.</b> They DO enter <see cref="TheaterSceneHistory"/> via <c>Record</c> — so PRD §7.4
    /// rule 3 ("reject the immediately previous signature") still works correctly the instant a
    /// structural scene actually was the previous one; a kickoff immediately followed by an
    /// identical kickoff is exactly the kind of literal back-to-back repeat rule 3 exists to catch,
    /// and there is no principled reason to blind rule 3 to it. They are also visible, unfiltered,
    /// to rule 5's "compatible" (same-contract) lookback — a Structural scene can only ever be
    /// "compatible" with another Structural scene, so this costs nothing and needs no special case.
    ///
    /// They are deliberately made INVISIBLE, however, to rule 4's "last three NON-STRUCTURAL
    /// scenes" window (<see cref="TheaterSceneHistory.RecentNonStructuralGrammars"/> skips them by
    /// name in the rule's own wording). Rule 4 is about buildup-grammar fatigue across the
    /// dramatic, non-structural scenes a viewer actually tracks; a kickoff's grammar
    /// (<see cref="MovementGrammar.Restart"/>) is not a buildup shape competing for the same
    /// "freshness" budget as Central/Wing/Switch/Counter/SetPiece, and letting a kickoff occupy one
    /// of the three lookback slots would mean a match with frequent restarts gets WEAKER grammar
    /// variety enforcement than one without — the opposite of what the rule is for. So: enters the
    /// buffer (real history), invisible to the one query whose own text says "non-structural".</para>
    /// </summary>
    public sealed class TheaterScenePlanner
    {
        public TheaterScenePlanner() { }

        /// <summary>PRD §7.2's table, as a function from a resolved beat to which row it belongs
        /// to. Reads only <see cref="SceneSpec.Template"/> and, for the Goal-family templates,
        /// <see cref="SceneSpec.Goal"/>'s <c>Commits</c> flag — never chooses or infers a fact,
        /// only classifies one already decided upstream by <see cref="TheaterChoreographer"/> and
        /// the score/count ledgers.</summary>
        public static SceneFactContract ClassifyFactContract(in SceneSpec spec)
        {
            switch (spec.Template)
            {
                case SceneTemplate.GoalFor:
                case SceneTemplate.GoalAgainst:
                case SceneTemplate.BreakawayFor:
                case SceneTemplate.BreakawayAgainst:
                    // Every real Goal/Breakaway scene from ResolveBeat always sets Goal (mirrors
                    // the same "?? true" defensive-default pattern TheaterStage.cs itself uses
                    // for a bare template-completion test with no staged fact at all).
                    bool commits = !spec.Goal.HasValue || spec.Goal.Value.Commits;
                    return commits ? SceneFactContract.Goal : SceneFactContract.ChalkedGoal;

                case SceneTemplate.NearMissHope:
                case SceneTemplate.NearMissScare:
                    return SceneFactContract.NearMiss;

                case SceneTemplate.TerritoryFor:
                case SceneTemplate.TerritoryAgainst:
                case SceneTemplate.CalmPossession:
                    return SceneFactContract.Territory;

                case SceneTemplate.CornerFor:
                case SceneTemplate.CornerAgainst:
                    return SceneFactContract.Corner;

                case SceneTemplate.Booking:
                    return SceneFactContract.Booking;

                case SceneTemplate.LegFinalWon:
                case SceneTemplate.LegFinalLost:
                    return SceneFactContract.Final;

                case SceneTemplate.Kickoff:
                case SceneTemplate.Fallback:
                    return SceneFactContract.Structural;

                default:
                    // Mirrors TheaterChoreographer's and ScenePlaybook's own "future enum
                    // additions play, never throw" philosophy. Structural is the one fact
                    // contract whose §7.2 row forbids ANY statistical payoff — the truth-safe
                    // default for a template this classifier does not yet recognize.
                    return SceneFactContract.Structural;
            }
        }

        /// <summary>PRD §7.4 step 1: the valid candidate set for one beat, built from the fact
        /// contract alone — a lookup into the already-contract-partitioned static catalog
        /// (further narrowed by <see cref="SceneCandidate.RestrictToTemplate"/> for the two rows
        /// that group templates with different legal sets), never a truth check applied after the
        /// fact.</summary>
        public static List<SceneCandidate> CandidatesFor(SceneTemplate template, SceneFactContract contract)
        {
            var result = new List<SceneCandidate>();
            for (int i = 0; i < _catalog.Count; i++)
            {
                SceneCandidate c = _catalog[i];
                if (c.Contract != contract) continue;
                if (c.RestrictToTemplate.HasValue && c.RestrictToTemplate.Value != template) continue;
                result.Add(c);
            }
            return result;
        }

        /// <summary>
        /// PRD §7.4's full pipeline. Pure: reads <paramref name="history"/>'s current contents but
        /// never mutates it — the caller records the returned plan's <see cref="TheaterScenePlan.Signature"/>
        /// (via <see cref="TheaterSceneHistory.Record"/>) only once it actually accepts the plan
        /// for playback. Every discrete choice this method makes (candidate ranking, lane) derives
        /// only from <paramref name="key"/>'s named channels — no <c>RngHub</c>, no
        /// <c>UnityEngine.Random</c>, no clock, no frame count (PRD §4.3).
        /// </summary>
        public TheaterScenePlan Plan(SceneSpec spec, PresentationSceneKey key, TheaterSceneHistory history)
        {
            if (history == null) throw new ArgumentNullException(nameof(history));

            SceneFactContract contract = ClassifyFactContract(spec);

            // Step 1 — truth filtering, before any ranking.
            List<SceneCandidate> valid = CandidatesFor(spec.Template, contract);
            if (valid.Count == 0)
            {
                throw new InvalidOperationException(
                    $"TheaterScenePlanner catalog has no candidate for template {spec.Template} " +
                    $"/ contract {contract} — the catalog is incomplete for this template.");
            }
            int totalValid = valid.Count;

            // Step 2 — rank with named presentation-hash channels.
            List<SceneCandidate> ranked = RankDeterministically(valid, key);

            // Step 3 — reject the immediately previous signature when an alternative exists.
            PlanSignature? prevSignature = history.MostRecent?.Signature;
            List<SceneCandidate> pool = PreferFilter(
                ranked,
                c => !prevSignature.HasValue || !SignatureOf(spec.Template, c).Equals(prevSignature.Value),
                out bool rejectedPrevious);

            // Step 4 — prefer a grammar absent from the last three non-structural scenes.
            HashSet<MovementGrammar> recentGrammars = history.RecentNonStructuralGrammars(3);
            pool = PreferFilter(pool, c => !recentGrammars.Contains(c.Grammar), out bool preferredFreshGrammar);

            // Step 5 — prefer a payoff/reaction combo absent from the last two compatible scenes.
            var recentCombos = history.RecentCompatiblePayoffReactionCombos(contract, 2);
            pool = PreferFilter(pool, c => !recentCombos.Contains((c.Payoff, c.Reaction)),
                out bool preferredFreshPayoffReaction);

            // Step 6 — if constraints leave exactly one candidate, record why.
            bool collapsed = pool.Count == 1;
            string collapseReason = null;
            if (collapsed)
            {
                collapseReason = totalValid == 1
                    ? $"Fact contract {contract} (template {spec.Template}) admits exactly one " +
                      "legal candidate under PRD §7.2's truth table; no ranking or preference " +
                      "rule had anything left to choose between."
                    : BuildNarrowedReason(totalValid, rejectedPrevious, preferredFreshGrammar,
                        preferredFreshPayoffReaction);
            }

            SceneCandidate chosen = pool[0];
            SceneLane lane = PickLane(chosen, key);
            PlanSignature signature = SignatureOf(spec.Template, chosen);

            var diagnostics = new TheaterScenePlanDiagnostics(totalValid, rejectedPrevious,
                preferredFreshGrammar, preferredFreshPayoffReaction, collapsed, collapseReason);

            bool? scoredByPicked = spec.Goal.HasValue ? (bool?)spec.Goal.Value.ScoredByPicked : null;

            return new TheaterScenePlan(
                spec.Template, contract, chosen.Grammar, chosen.ChanceShape, chosen.Payoff, chosen.Pressure,
                chosen.Spacing, chosen.Reaction, lane, spec.CountBeneficiaryIsHome, spec.ForPicked,
                scoredByPicked, chosen.HasWinTheCornerLeadIn, signature, diagnostics);
        }

        // ------------------------------------------------------------------ ranking

        /// <summary>Folds each of a candidate's dimension values into the shared, named
        /// presentation-hash channels (PRD §4.3's <see cref="PresentationSceneKey.Channels"/>,
        /// plus two ad hoc channel names for the dimensions the key doc explicitly says are not an
        /// exhaustive set) and XORs the results. Every input to the XOR is independently
        /// avalanched by <see cref="PresentationSceneKey.Channel"/> already, so the combination
        /// stays well distributed; this is a pure function of (key, candidate identity) only.</summary>
        private static ulong Score(SceneCandidate c, PresentationSceneKey key)
        {
            ulong h = 0;
            h ^= key.Channel(PresentationSceneKey.Channels.Grammar + "/" + c.Grammar);
            h ^= key.Channel("chanceShape/" + (c.ChanceShape.HasValue ? c.ChanceShape.Value.ToString() : "none"));
            h ^= key.Channel(PresentationSceneKey.Channels.Payoff + "/" + c.Payoff);
            h ^= key.Channel(PresentationSceneKey.Channels.Pressure + "/" + c.Pressure);
            h ^= key.Channel("spacing/" + c.Spacing);
            h ^= key.Channel(PresentationSceneKey.Channels.Reaction + "/" + c.Reaction);
            return h;
        }

        private static List<SceneCandidate> RankDeterministically(List<SceneCandidate> candidates, PresentationSceneKey key)
        {
            var scored = new List<(ulong Score, SceneCandidate Candidate)>(candidates.Count);
            foreach (SceneCandidate c in candidates) scored.Add((Score(c, key), c));

            scored.Sort((a, b) =>
            {
                int cmp = a.Score.CompareTo(b.Score);
                if (cmp != 0) return cmp;
                // Deterministic tie-break. Never expected to matter at 64-bit hash width across a
                // catalog of, at most, a few thousand candidates — kept anyway so "deterministic"
                // never quietly depends on List<T>.Sort's unspecified tie behavior.
                cmp = a.Candidate.Grammar.CompareTo(b.Candidate.Grammar);
                if (cmp != 0) return cmp;
                cmp = Nullable.Compare(a.Candidate.ChanceShape, b.Candidate.ChanceShape);
                if (cmp != 0) return cmp;
                cmp = a.Candidate.Payoff.CompareTo(b.Candidate.Payoff);
                if (cmp != 0) return cmp;
                cmp = a.Candidate.Pressure.CompareTo(b.Candidate.Pressure);
                if (cmp != 0) return cmp;
                cmp = a.Candidate.Spacing.CompareTo(b.Candidate.Spacing);
                if (cmp != 0) return cmp;
                return a.Candidate.Reaction.CompareTo(b.Candidate.Reaction);
            });

            var result = new List<SceneCandidate>(scored.Count);
            foreach (var s in scored) result.Add(s.Candidate);
            return result;
        }

        /// <summary>Applies a soft preference: keeps only candidates matching <paramref name="keep"/>
        /// UNLESS doing so would empty the pool, in which case the original pool passes through
        /// unchanged. <paramref name="narrowed"/> reports whether the filter actually excluded
        /// anything (used for diagnostics). Order is preserved, so the caller's ranking survives
        /// every filter stage untouched.</summary>
        private static List<SceneCandidate> PreferFilter(List<SceneCandidate> pool, Func<SceneCandidate, bool> keep,
            out bool narrowed)
        {
            var filtered = new List<SceneCandidate>(pool.Count);
            foreach (SceneCandidate c in pool)
                if (keep(c)) filtered.Add(c);

            if (filtered.Count > 0 && filtered.Count < pool.Count)
            {
                narrowed = true;
                return filtered;
            }
            narrowed = false;
            return filtered.Count > 0 ? filtered : pool;
        }

        private static PlanSignature SignatureOf(SceneTemplate template, SceneCandidate c)
            => new PlanSignature(template, c.Grammar, c.ChanceShape, c.Payoff, c.Pressure, c.Spacing, c.Reaction);

        private static string BuildNarrowedReason(int totalValid, bool rejectedPrevious, bool freshGrammar,
            bool freshPayoffReaction)
        {
            var reasons = new List<string>(3);
            if (rejectedPrevious) reasons.Add("rejecting the immediately-previous signature (§7.4 rule 3)");
            if (freshGrammar) reasons.Add("preferring a grammar absent from the last three non-structural scenes (§7.4 rule 4)");
            if (freshPayoffReaction) reasons.Add("preferring a payoff/reaction combination absent from the last two compatible scenes (§7.4 rule 5)");
            string applied = reasons.Count > 0 ? string.Join("; then ", reasons) : "ranking alone";
            return $"Narrowed from {totalValid} legal candidates to 1 by: {applied}.";
        }

        // ------------------------------------------------------------------ lane

        private static readonly SceneLane[] AllLanes = { SceneLane.Center, SceneLane.NearFlank, SceneLane.FarFlank };
        private static readonly SceneLane[] CenterOnlyLane = { SceneLane.Center };

        /// <summary>Lane never affects truth or the signature (see this type's doc), so it is
        /// picked independently after the candidate is chosen, on its own named channel. A
        /// kickoff restart always lines up at the center circle — there is no flank to vary — so
        /// it is the one grammar that legally admits only one lane.</summary>
        private static SceneLane PickLane(SceneCandidate chosen, PresentationSceneKey key)
        {
            SceneLane[] legal = chosen.Grammar == MovementGrammar.Restart ? CenterOnlyLane : AllLanes;
            int index = key.Pick(PresentationSceneKey.Channels.Lane, legal.Length);
            return legal[index];
        }

        // ------------------------------------------------------------------ catalog

        private static readonly MovementGrammar[] BuildupGrammars =
            { MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch, MovementGrammar.Counter };

        private static readonly MovementGrammar[] GoalMovements =
            { MovementGrammar.Central, MovementGrammar.Wing, MovementGrammar.Switch, MovementGrammar.Counter, MovementGrammar.SetPiece };

        private static readonly MovementGrammar[] CornerShapes =
            { MovementGrammar.NearPost, MovementGrammar.FarPost, MovementGrammar.Cleared };

        private static readonly ChanceShape[] ChanceShapes =
            { ChanceShape.ThroughBall, ChanceShape.Cross, ChanceShape.Cutback, ChanceShape.Rebound, ChanceShape.Direct };

        private static readonly ScenePayoff[] NonGoalEndings =
            { ScenePayoff.Block, ScenePayoff.Interception, ScenePayoff.KeeperSave, ScenePayoff.Clearance, ScenePayoff.Post, ScenePayoff.NearWide };

        private static readonly ScenePayoff[] TerritoryPayoffs =
            { ScenePayoff.RetainedPossession, ScenePayoff.ForcedBackPass, ScenePayoff.Interception, ScenePayoff.Clearance };

        private static readonly PressureMode[] Pressures =
            { PressureMode.LowBlock, PressureMode.MidPress, PressureMode.HighPress };

        private static readonly SpacingMode[] Spacings =
            { SpacingMode.Compact, SpacingMode.Balanced, SpacingMode.Stretched };

        // NOTE ON DECLARATION ORDER: C# runs a type's static field initializers in the exact
        // textual order they appear in the class, as part of the type's static constructor. The
        // catalog field below is declared AFTER every array `BuildCatalog()` reads (BuildupGrammars,
        // GoalMovements, CornerShapes, ChanceShapes, NonGoalEndings, TerritoryPayoffs, Pressures,
        // Spacings, all above) specifically so `BuildCatalog()` never runs before its own inputs are
        // initialized. Moving this field above those arrays reintroduces a null-reference at
        // static-init time — caught by this dispatch's own test suite the first time it ran headless:
        // every test touching `Catalog` or constructing a `TheaterScenePlanner` failed with a
        // `TypeInitializationException` wrapping a `NullReferenceException` inside `BuildCatalog()`
        // until this field moved below its dependencies.
        private static readonly IReadOnlyList<SceneCandidate> _catalog = BuildCatalog();

        /// <summary>The complete, static candidate catalog across every fact contract. Exposed so
        /// tests can assert PRD §7.3's variety floor against real counts (§7.3: "assert catalog
        /// counts rather than trusting them") instead of re-deriving them.</summary>
        public static IReadOnlyList<SceneCandidate> Catalog => _catalog;

        /// <summary>Each pressure mode's exclusive defending reaction — no other mode's
        /// candidates ever reach it (see <see cref="ReactionPattern"/>'s doc). This is what makes
        /// §7.3's "visibly different defending and reaction behavior per pressure mode" a fact a
        /// test can check on the catalog, not a hope about how the values happened to be chosen.</summary>
        private static ReactionPattern PrimaryReactionFor(PressureMode pressure)
        {
            switch (pressure)
            {
                case PressureMode.LowBlock: return ReactionPattern.Drop;
                case PressureMode.MidPress: return ReactionPattern.Step;
                case PressureMode.HighPress: return ReactionPattern.Chase;
                default: return ReactionPattern.Recover;
            }
        }

        /// <summary>The legal reaction set for one (contract, payoff, pressure) combination.
        /// Always the pressure's exclusive primary plus the universal <see cref="ReactionPattern.Recover"/>
        /// "regroup" reaction; <see cref="ReactionPattern.Celebrate"/> is added only for a
        /// COMMITTING goal (never <see cref="SceneFactContract.ChalkedGoal"/> — §7.2's "forbidden
        /// implication: score increment" means nothing here may look like a celebration of a goal
        /// that did not count) and <see cref="ReactionPattern.Collapse"/> only for a keeper-save
        /// near miss.</summary>
        private static List<ReactionPattern> ReactionsFor(SceneFactContract contract, ScenePayoff payoff, PressureMode pressure)
        {
            var result = new List<ReactionPattern>(3) { PrimaryReactionFor(pressure), ReactionPattern.Recover };
            if (contract == SceneFactContract.Goal) result.Add(ReactionPattern.Celebrate);
            if (contract == SceneFactContract.NearMiss && payoff == ScenePayoff.KeeperSave) result.Add(ReactionPattern.Collapse);
            return result;
        }

        /// <summary>
        /// PRD §7.2's table, realized as data: one dedicated generator block per fact contract, each
        /// drawing only from that row's own legal movement/chance-shape/payoff arrays. No block here
        /// ever reads another block's arrays, and no candidate is filtered out for illegality after
        /// being built — illegal candidates are simply never constructed (see this type's own doc
        /// comment for the full argument).
        /// </summary>
        private static List<SceneCandidate> BuildCatalog()
        {
            var list = new List<SceneCandidate>();

            // ---- Goal / ChalkedGoal (§7.2 rows 1–2): central/wing/switch/counter/set-piece
            // buildup into a through-ball/cross/cutback/rebound/direct chance, ending only ever
            // in ScenePayoff.Goal (the fact IS a goal — no non-goal payoff is representable here).
            foreach (SceneFactContract contract in new[] { SceneFactContract.Goal, SceneFactContract.ChalkedGoal })
            foreach (MovementGrammar g in GoalMovements)
            foreach (ChanceShape cs in ChanceShapes)
            foreach (PressureMode p in Pressures)
            foreach (SpacingMode sp in Spacings)
            foreach (ReactionPattern r in ReactionsFor(contract, ScenePayoff.Goal, p))
                list.Add(new SceneCandidate(contract, g, cs, ScenePayoff.Goal, p, sp, r, false));

            // ---- Near miss (§7.2 row 3): same movement/chance-shape vocabulary as Goal, but the
            // ending is always one of the six non-goal endings — Goal is never reachable here.
            foreach (MovementGrammar g in GoalMovements)
            foreach (ChanceShape cs in ChanceShapes)
            foreach (ScenePayoff payoff in NonGoalEndings)
            foreach (PressureMode p in Pressures)
            foreach (SpacingMode sp in Spacings)
            foreach (ReactionPattern r in ReactionsFor(SceneFactContract.NearMiss, payoff, p))
                list.Add(new SceneCandidate(SceneFactContract.NearMiss, g, cs, payoff, p, sp, r, false));

            // ---- Territory / calm (§7.2 row 4): recycled possession only — no set-piece
            // movement (no dead ball to recycle) and no shot-outcome payoff (no chance was ever
            // taken). ChanceShape is null: there is no "chance" dimension to a scene where no
            // chance occurred.
            foreach (MovementGrammar g in BuildupGrammars)
            foreach (ScenePayoff payoff in TerritoryPayoffs)
            foreach (PressureMode p in Pressures)
            foreach (SpacingMode sp in Spacings)
            foreach (ReactionPattern r in ReactionsFor(SceneFactContract.Territory, payoff, p))
                list.Add(new SceneCandidate(SceneFactContract.Territory, g, null, payoff, p, sp, r, false));

            // ---- Corner (§7.2 row 5): §7.2 places "near-post/far-post/cleared" under the table's
            // MOVEMENT column for this row — these three values are this contract's MovementGrammar
            // set, not a buildup grammar (see MovementGrammar's doc). Ending is always the single
            // representative CornerDelivery payoff; HasWinTheCornerLeadIn is always true (§7.3's
            // "the corner does not begin at the flag with no cause" is a structural fact about every
            // Corner candidate, not a chosen dimension).
            foreach (MovementGrammar g in CornerShapes)
            foreach (PressureMode p in Pressures)
            foreach (SpacingMode sp in Spacings)
            foreach (ReactionPattern r in ReactionsFor(SceneFactContract.Corner, ScenePayoff.CornerDelivery, p))
                list.Add(new SceneCandidate(SceneFactContract.Corner, g, null, ScenePayoff.CornerDelivery, p, sp, r, true));

            // ---- Booking (§7.2 row 6): "pressure/challenge setup" ending in the single
            // BookingMarker payoff. Draws on the four buildup grammars for where on the pitch the
            // challenge happens — which side committed it is exogenous attribution
            // (SceneSpec.CountBeneficiaryIsHome), never a candidate dimension, so the exact same
            // candidate set serves both fouling sides (§7.3: "booking setups for both sides").
            foreach (MovementGrammar g in BuildupGrammars)
            foreach (PressureMode p in Pressures)
            foreach (SpacingMode sp in Spacings)
            foreach (ReactionPattern r in ReactionsFor(SceneFactContract.Booking, ScenePayoff.BookingMarker, p))
                list.Add(new SceneCandidate(SceneFactContract.Booking, g, null, ScenePayoff.BookingMarker, p, sp, r, false));

            // ---- Final (§7.2 row 7): "deterministic plan from final grade plus staged goal/count
            // corrections" — deliberately degenerate, exactly one legal candidate per grade. There
            // is nothing left for the planner to choose; the grade already came from the FINAL
            // ticket-local LegGrade (ScoreLedger.PlanFinal / TheaterChoreographer.ResolveFinal),
            // never from a live WinProbAfter, so §7.2's "hidden score/count jump ... selected from
            // stale WinProbAfter" forbidden implication cannot arise here even in principle: this
            // type is never given WinProbAfter to read.
            list.Add(new SceneCandidate(SceneFactContract.Final, MovementGrammar.FinalSequence, null,
                ScenePayoff.FinalWhistleWin, PressureMode.HighPress, SpacingMode.Stretched, ReactionPattern.Celebrate,
                false, SceneTemplate.LegFinalWon));
            list.Add(new SceneCandidate(SceneFactContract.Final, MovementGrammar.FinalSequence, null,
                ScenePayoff.FinalWhistleLoss, PressureMode.HighPress, SpacingMode.Stretched, ReactionPattern.Collapse,
                false, SceneTemplate.LegFinalLost));

            // ---- Structural (§7.2 row 8): "structural restart or neutral possession ... no
            // statistical payoff". Kickoff is a single restart shape (center circle — no flank, no
            // grammar variety to offer). Fallback (the engine's own catch-all "a neutral attack
            // fizzles out") draws on the four buildup grammars for visual variety while keeping the
            // same NoStatisticalPayoff ending — it never resembles a goal, count, or decisive scene.
            list.Add(new SceneCandidate(SceneFactContract.Structural, MovementGrammar.Restart, null,
                ScenePayoff.NoStatisticalPayoff, PressureMode.LowBlock, SpacingMode.Balanced, ReactionPattern.Recover,
                false, SceneTemplate.Kickoff));
            foreach (MovementGrammar g in BuildupGrammars)
            foreach (PressureMode p in Pressures)
            foreach (SpacingMode sp in Spacings)
                list.Add(new SceneCandidate(SceneFactContract.Structural, g, null, ScenePayoff.NoStatisticalPayoff,
                    p, sp, ReactionPattern.Recover, false, SceneTemplate.Fallback));

            return list;
        }
    }
}
