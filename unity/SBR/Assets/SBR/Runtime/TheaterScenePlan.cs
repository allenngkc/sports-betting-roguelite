using System;

namespace SBR.Game
{
    /// <summary>
    /// Phase 2B (PRD §7.1): the eight independent, truth-constrained dimensions a resolved beat's
    /// <see cref="ScenePlaybook.SceneSpec"/> elaborates into. This file is pure data — enums plus
    /// the plan struct itself — built and consumed entirely by <see cref="TheaterScenePlanner"/>.
    /// Nothing here is wired into <see cref="TheaterStage"/> or <see cref="TheaterChoreographer"/>
    /// yet (that is Phase 2C); constructing or querying any type in this file has no effect on
    /// anything that currently renders.
    /// </summary>

    /// <summary>
    /// The PRD §7.2 table's row identity — which truth contract a beat carries. This is derived
    /// FROM the factual <see cref="SceneSpec"/> (via <see cref="TheaterScenePlanner.ClassifyFactContract"/>),
    /// never chosen by the planner. It is the axis every truth-filtering rule in §7.2 keys off:
    /// each fact contract owns its own closed, hand-authored candidate set in
    /// <see cref="TheaterScenePlanner"/>'s catalog, so an illegal combination (a goal ending on a
    /// possession scene, a booking marker on a corner) is never constructed in the first place —
    /// there is no cross-contract candidate list that a later filter has to police.
    ///
    /// <see cref="Goal"/> and <see cref="ChalkedGoal"/> are kept SEPARATE despite sharing one
    /// §7.2 table row ("Goal / breakaway goal" vs "Chalked goal" are two different rows, but both
    /// map from <see cref="SceneTemplate.GoalFor"/>/<see cref="SceneTemplate.GoalAgainst"/>/
    /// <see cref="SceneTemplate.BreakawayFor"/>/<see cref="SceneTemplate.BreakawayAgainst"/>,
    /// disambiguated by <c>SceneSpec.Goal.Value.Commits</c>) because their allowed reaction sets
    /// differ: a chalked goal may never stage <see cref="ReactionPattern.Celebrate"/> (§7.2:
    /// "forbidden implication: score increment" — nothing committed, nothing to celebrate).
    /// </summary>
    public enum SceneFactContract
    {
        /// <summary>A goal that commits (<c>StagedGoal.Commits == true</c>). Covers both the
        /// Goal (#1/#2) and Breakaway (#3/#4) templates — §7.2's table gives them one identical
        /// row ("Goal / breakaway goal"), so this type does not re-split them.</summary>
        Goal,
        /// <summary>A goal that does NOT commit — the ball crosses the line, then the VAR
        /// no-goal treatment lands with the scoreline unchanged (§7.2's "Chalked goal" row).
        /// Same templates as <see cref="Goal"/>; disambiguated by <c>Commits == false</c>.</summary>
        ChalkedGoal,
        /// <summary>#7/#8 — never a goal, regardless of the beat's direction.</summary>
        NearMiss,
        /// <summary>#5/#6/#11 — territory and calm possession share one §7.2 row.</summary>
        Territory,
        /// <summary>#16/#17 — a corner. Movement here is which corner shape wins the ball back
        /// (near post / far post / cleared), not a buildup grammar — see
        /// <see cref="MovementGrammar"/>'s doc for why those values live on that enum.</summary>
        Corner,
        /// <summary>#18 — a booking. No For/Against template split (routing is read from
        /// <c>SceneSpec.CountBeneficiaryIsHome</c> directly), so both fouling sides share one
        /// candidate set; which side is exogenous, never chosen here (§7.6).</summary>
        Booking,
        /// <summary>#12/#13 — the final whistle. §7.2: "deterministic plan from final grade plus
        /// staged goal/count corrections" — there is no independent movement/chance choice left
        /// to make here; the grade (already resolved by <c>ScoreLedger.PlanFinal</c> /
        /// <c>TheaterChoreographer.ResolveFinal</c> from the FINAL ticket-local grade, never from
        /// <c>WinProbAfter</c>) fixes the outcome. This fact contract's candidate set is
        /// deliberately degenerate — exactly one legal candidate per grade — which is also this
        /// type's most natural, honest example of PRD §7.4 rule 6's "constraints leave one
        /// candidate" case.</summary>
        Final,
        /// <summary>#14 (Kickoff) / #15 (Fallback) — "structural restart or neutral possession...
        /// no statistical payoff" (§7.2). See <see cref="TheaterScenePlanner"/>'s type doc for how
        /// structural scenes are (and are not) visible to repetition control.</summary>
        Structural,
    }

    /// <summary>
    /// The buildup/approach shape (§7.1's "movement grammar" dimension). §7.3's floor names four
    /// buildup grammars explicitly — <see cref="Central"/>, <see cref="Wing"/>, <see cref="Switch"/>,
    /// <see cref="Counter"/> — which is why those four, and only those four, are what "4 buildup
    /// grammars" coverage tests count. The remaining members exist because §7.2's table places
    /// contract-specific movement vocabularies in the SAME "movement" column for Corner and
    /// Structural rows (corner shape, restart), and giving them their own enum values — rather
    /// than overloading <see cref="Central"/> etc. — keeps a Corner candidate impossible to
    /// mistake for an open-play buildup candidate at the type level.
    /// </summary>
    public enum MovementGrammar
    {
        Central,
        Wing,
        Switch,
        Counter,
        /// <summary>A dead-ball delivery is the "movement" for a goal/near-miss chance built
        /// directly off a set piece (free kick, direct restart) — §7.2's "set piece" movement
        /// option on the Goal row. Never legal for Territory (§7.2: no dead-ball recycling in a
        /// possession scene).</summary>
        SetPiece,
        /// <summary>§7.2 Corner row / §7.3 "3 corner shapes": win the ball back and take the
        /// near-post delivery. Legal only under <see cref="SceneFactContract.Corner"/>.</summary>
        NearPost,
        /// <summary>As <see cref="NearPost"/>, far-post delivery.</summary>
        FarPost,
        /// <summary>As <see cref="NearPost"/>, the corner is won by the delivery being cleared
        /// behind for a corner rather than a direct dispossession.</summary>
        Cleared,
        /// <summary>§7.2 Structural row: "structural restart" — the kickoff itself. Legal only
        /// under <see cref="SceneFactContract.Structural"/> and only for
        /// <see cref="SceneTemplate.Kickoff"/> (never <see cref="SceneTemplate.Fallback"/>, which
        /// draws on the four buildup grammars instead — a fizzled neutral attack still looks like
        /// an attack, just one that goes nowhere).</summary>
        Restart,
        /// <summary>§7.2 Final row: "deterministic plan from final grade plus staged goal/count
        /// corrections" — the final's own dedicated, non-buildup shape. Legal only under
        /// <see cref="SceneFactContract.Final"/>.</summary>
        FinalSequence,
    }

    /// <summary>
    /// §7.1's "chance shape" dimension, verbatim: through ball / cross / cutback / rebound /
    /// direct. Meaningful only where a shooting chance exists (<see cref="SceneFactContract.Goal"/>,
    /// <see cref="SceneFactContract.ChalkedGoal"/>, <see cref="SceneFactContract.NearMiss"/>);
    /// every other fact contract's candidates carry <c>null</c> for this dimension, because
    /// Corner/Booking/Territory/Final/Structural have no "chance" in the shooting sense at all.
    ///
    /// <para><b>A PRD internal inconsistency, resolved here (report it back — see the dispatch's
    /// final-response instructions):</b> §7.1 names this dimension's fifth value <c>direct</c>.
    /// §7.3's variety floor instead calls the fifth REQUIRED "scoring shape" <c>set piece</c>.
    /// Those are two different words for what §7.2's Goal row calls one combined ending,
    /// "direct/set-piece goal" — so the two sections do not actually disagree on the STORY (a
    /// shot struck directly, often off a dead ball), only on which of two labels names it. This
    /// enum keeps §7.1's literal name, <see cref="Direct"/>, as the fifth chance-shape value (so
    /// the enum has exactly the five members §7.1 lists), and §7.3's "set piece" scoring shape is
    /// satisfied by the catalog always containing at least one Goal-contract candidate pairing
    /// <see cref="Direct"/> with <see cref="MovementGrammar.SetPiece"/> — the set-piece movement
    /// producing the direct chance shape, rather than "set piece" being a second, sixth chance
    /// value. <see cref="TheaterScenePlannerTests"/>' variety-floor test asserts that pairing
    /// exists, not just that <see cref="Direct"/> exists somewhere.</para>
    /// </summary>
    public enum ChanceShape
    {
        ThroughBall,
        Cross,
        Cutback,
        Rebound,
        Direct,
    }

    /// <summary>§7.1 / §7.3: the three defending shapes. §7.3 additionally requires "visibly
    /// different defending and reaction behavior per pressure mode" — enforced in
    /// <see cref="TheaterScenePlanner"/> by giving each mode its own reaction with no other mode
    /// legally reaching it (see <see cref="ReactionPattern"/>'s doc).</summary>
    public enum PressureMode
    {
        LowBlock,
        MidPress,
        HighPress,
    }

    /// <summary>§7.1 / §7.3: the three spatial-compression shapes.</summary>
    public enum SpacingMode
    {
        Compact,
        Balanced,
        Stretched,
    }

    /// <summary>
    /// §7.1's "payoff" dimension. §7.1 lists seven shot-chance outcomes (goal plus the six
    /// non-goal endings §7.3's floor names explicitly: block, interception, keeper save,
    /// clearance, post, near wide) — those seven are reproduced here verbatim as
    /// <see cref="Goal"/> through <see cref="NearWide"/>. Every OTHER fact contract's "ending"
    /// per §7.2's table is a genuinely different kind of resolution (a possession simply being
    /// retained, a corner delivery landing, a booking marker, a final whistle) that does not fit
    /// the shot-outcome vocabulary at all — rather than force those endings into a shot word
    /// (calling a retained possession a "clearance" would be a lie), this enum is extended with
    /// one dedicated value per contract's own ending. Which values are reachable under which fact
    /// contract is enforced entirely by <see cref="TheaterScenePlanner"/>'s per-contract catalog
    /// generators — no candidate anywhere pairs, say, <see cref="SceneFactContract.Territory"/>
    /// with <see cref="Goal"/>.
    /// </summary>
    public enum ScenePayoff
    {
        Goal,
        Block,
        Interception,
        KeeperSave,
        Clearance,
        Post,
        NearWide,
        /// <summary>Territory only: the move is recycled, no stat.</summary>
        RetainedPossession,
        /// <summary>Territory only: possession dies in a back-pass, no stat.</summary>
        ForcedBackPass,
        /// <summary>Corner only: "one representative corner payoff carrying the staged positive
        /// batch" (§7.2) — the count itself already came from <c>CountLedger</c>; this is the
        /// single visual ending every corner shape converges on.</summary>
        CornerDelivery,
        /// <summary>Booking only: "visible challenge and booking marker" (§7.2).</summary>
        BookingMarker,
        /// <summary>Structural only: §7.2's "no statistical payoff", made an explicit value
        /// rather than null so a Structural candidate's payoff is never mistaken for "not yet
        /// decided".</summary>
        NoStatisticalPayoff,
        /// <summary>Final, Won grade only.</summary>
        FinalWhistleWin,
        /// <summary>Final, Lost grade only.</summary>
        FinalWhistleLoss,
    }

    /// <summary>
    /// §7.1's "reactions" dimension: step / chase / drop / recover / celebrate / collapse.
    /// <see cref="TheaterScenePlanner"/> ties <see cref="Step"/>/<see cref="Chase"/>/
    /// <see cref="Drop"/> to specific <see cref="PressureMode"/> values as each mode's exclusive
    /// "primary" defending reaction (no other mode's candidates ever reach that primary), with
    /// <see cref="Recover"/> as a secondary "regroup" reaction every mode's candidates share —
    /// this is what makes "visibly different defending... per pressure mode" (§7.3) a structural
    /// property of the catalog rather than a hopeful convention: a test can assert
    /// <see cref="Drop"/> never appears opposite <see cref="PressureMode.HighPress"/>, etc.
    /// <see cref="Celebrate"/> is reachable only from a committing <see cref="SceneFactContract.Goal"/>
    /// candidate or a Won <see cref="SceneFactContract.Final"/>; <see cref="Collapse"/> only from
    /// a <see cref="ScenePayoff.KeeperSave"/> near miss or a Lost Final.
    /// </summary>
    public enum ReactionPattern
    {
        Step,
        Chase,
        Drop,
        Recover,
        Celebrate,
        Collapse,
    }

    /// <summary>§7.1: center / near flank / far flank. Deliberately excluded from
    /// <see cref="PlanSignature"/> — see <see cref="TheaterScenePlanner"/>'s type doc for why lane
    /// alone must never be what makes two scenes count as "different".</summary>
    public enum SceneLane
    {
        Center,
        NearFlank,
        FarFlank,
    }

    /// <summary>
    /// The repetition-control identity of a plan (PRD §7.4). Deliberately narrower than the full
    /// <see cref="TheaterScenePlan"/>: it carries the seven dimensions that make two scenes read
    /// as "the same move" to a couch viewer — <see cref="Template"/> (which already encodes
    /// for/against truth identity), <see cref="Grammar"/>, <see cref="ChanceShape"/>,
    /// <see cref="Payoff"/>, <see cref="Pressure"/>, <see cref="Spacing"/>, and
    /// <see cref="Reaction"/> — and excludes <see cref="SceneLane"/> and every
    /// <see cref="PresentationSceneKey"/> identity field (round/ticket/match/event step). See
    /// <see cref="TheaterScenePlanner"/>'s type doc for the full justification of both exclusions.
    /// </summary>
    public readonly struct PlanSignature : IEquatable<PlanSignature>
    {
        public readonly SceneTemplate Template;
        public readonly MovementGrammar Grammar;
        public readonly ChanceShape? ChanceShape;
        public readonly ScenePayoff Payoff;
        public readonly PressureMode Pressure;
        public readonly SpacingMode Spacing;
        public readonly ReactionPattern Reaction;

        public PlanSignature(SceneTemplate template, MovementGrammar grammar, ChanceShape? chanceShape,
            ScenePayoff payoff, PressureMode pressure, SpacingMode spacing, ReactionPattern reaction)
        {
            Template = template;
            Grammar = grammar;
            ChanceShape = chanceShape;
            Payoff = payoff;
            Pressure = pressure;
            Spacing = spacing;
            Reaction = reaction;
        }

        public bool Equals(PlanSignature other)
            => Template == other.Template
            && Grammar == other.Grammar
            && ChanceShape == other.ChanceShape
            && Payoff == other.Payoff
            && Pressure == other.Pressure
            && Spacing == other.Spacing
            && Reaction == other.Reaction;

        public override bool Equals(object obj) => obj is PlanSignature other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + (int)Template;
                h = h * 31 + (int)Grammar;
                h = h * 31 + (ChanceShape.HasValue ? (int)ChanceShape.Value + 1 : 0);
                h = h * 31 + (int)Payoff;
                h = h * 31 + (int)Pressure;
                h = h * 31 + (int)Spacing;
                h = h * 31 + (int)Reaction;
                return h;
            }
        }

        public override string ToString()
            => $"PlanSignature(template={Template}, grammar={Grammar}, " +
               $"chanceShape={(ChanceShape.HasValue ? ChanceShape.Value.ToString() : "none")}, " +
               $"payoff={Payoff}, pressure={Pressure}, spacing={Spacing}, reaction={Reaction})";
    }

    /// <summary>
    /// PRD §7.4 rule 6: "If constraints leave one candidate, play it and record the reason in
    /// diagnostics" — required, not optional. This struct is that record, plus enough of the
    /// intermediate pipeline state (which preference rules actually narrowed the candidate pool)
    /// for a test or a bug report to reconstruct why a plan came out the way it did without
    /// re-deriving the whole selection by hand.
    /// </summary>
    public readonly struct TheaterScenePlanDiagnostics
    {
        /// <summary>How many candidates survived §7.4 step 1 (truth filtering) before any
        /// ranking or preference rule ran.</summary>
        public readonly int ValidCandidateCount;
        /// <summary>§7.4 rule 3 fired: the immediately-previous signature was excluded from the
        /// pool because a legal alternative existed.</summary>
        public readonly bool RejectedImmediatePrevious;
        /// <summary>§7.4 rule 4 fired: at least one candidate was excluded for using a grammar
        /// seen in the last three non-structural history entries, and a grammar-fresh candidate
        /// existed.</summary>
        public readonly bool PreferredFreshGrammar;
        /// <summary>§7.4 rule 5 fired: at least one candidate was excluded for repeating a
        /// payoff/reaction combination seen in the last two compatible (same fact-contract)
        /// history entries, and a fresh combination existed.</summary>
        public readonly bool PreferredFreshPayoffReaction;
        /// <summary>True when, after all of the above, exactly one candidate remained.</summary>
        public readonly bool CollapsedToSingleCandidate;
        /// <summary>Non-null and non-empty exactly when <see cref="CollapsedToSingleCandidate"/>
        /// is true (§7.4 rule 6's mandatory reason). Null otherwise.</summary>
        public readonly string CollapseReason;

        public TheaterScenePlanDiagnostics(int validCandidateCount, bool rejectedImmediatePrevious,
            bool preferredFreshGrammar, bool preferredFreshPayoffReaction, bool collapsedToSingleCandidate,
            string collapseReason)
        {
            ValidCandidateCount = validCandidateCount;
            RejectedImmediatePrevious = rejectedImmediatePrevious;
            PreferredFreshGrammar = preferredFreshGrammar;
            PreferredFreshPayoffReaction = preferredFreshPayoffReaction;
            CollapsedToSingleCandidate = collapsedToSingleCandidate;
            CollapseReason = collapseReason;
        }
    }

    /// <summary>
    /// PRD §7.1's <c>ScenePlan</c>, realized: one resolved beat's independent, truth-constrained
    /// dimensions, plus the pass-through attribution facts the beat already carried (never chosen
    /// here — see <see cref="TheaterScenePlanner"/>'s type doc, §7.6) and the diagnostics §7.4
    /// rule 6 requires. Pure data; <see cref="TheaterScenePlanner"/> is the only thing that
    /// constructs one.
    /// </summary>
    public readonly struct TheaterScenePlan
    {
        /// <summary>The factual template this plan elaborates — unchanged from the input
        /// <c>SceneSpec.Template</c>. The planner never alters truth (§7.2).</summary>
        public readonly SceneTemplate Template;
        public readonly SceneFactContract FactContract;
        public readonly MovementGrammar Grammar;
        /// <summary>Null when this fact contract has no shooting chance (every contract except
        /// Goal/ChalkedGoal/NearMiss).</summary>
        public readonly ChanceShape? ChanceShape;
        public readonly ScenePayoff Payoff;
        public readonly PressureMode Pressure;
        public readonly SpacingMode Spacing;
        public readonly ReactionPattern Reaction;
        public readonly SceneLane Lane;

        /// <summary>Pass-through of <c>SceneSpec.CountBeneficiaryIsHome</c> — which team the
        /// engine actually credited a corner/booking batch to. Null for every non-count contract.
        /// The planner never computes or chooses this (§7.6); it only relays it.</summary>
        public readonly bool? BeneficiaryIsHome;
        /// <summary>Pass-through of <c>SceneSpec.ForPicked</c> — goal direction on a goal-family
        /// scene, or the bettor's hope/dread mood on a count scene. Never used by the planner to
        /// choose movement, payoff, or attribution (TVS-S01's guard).</summary>
        public readonly bool ForPicked;
        /// <summary>Pass-through of <c>SceneSpec.Goal.Value.ScoredByPicked</c>. Null unless this
        /// is a Goal or ChalkedGoal contract.</summary>
        public readonly bool? ScoredByPicked;

        /// <summary>§7.3: "for each corner shape, a visible win-the-corner lead-in attributed to
        /// the staged beneficiary — the corner does not begin at the flag with no cause." Always
        /// true for <see cref="SceneFactContract.Corner"/>, always false otherwise — not a choice,
        /// a structural fact about how every Corner candidate is built (see
        /// <see cref="TheaterScenePlanner"/>).</summary>
        public readonly bool HasWinTheCornerLeadIn;

        /// <summary>This plan's repetition-control identity. The caller (Phase 2C) records this
        /// into a <see cref="TheaterSceneHistory"/> once the plan is accepted for playback.</summary>
        public readonly PlanSignature Signature;

        public readonly TheaterScenePlanDiagnostics Diagnostics;

        public TheaterScenePlan(SceneTemplate template, SceneFactContract factContract, MovementGrammar grammar,
            ChanceShape? chanceShape, ScenePayoff payoff, PressureMode pressure, SpacingMode spacing,
            ReactionPattern reaction, SceneLane lane, bool? beneficiaryIsHome, bool forPicked,
            bool? scoredByPicked, bool hasWinTheCornerLeadIn, PlanSignature signature,
            TheaterScenePlanDiagnostics diagnostics)
        {
            Template = template;
            FactContract = factContract;
            Grammar = grammar;
            ChanceShape = chanceShape;
            Payoff = payoff;
            Pressure = pressure;
            Spacing = spacing;
            Reaction = reaction;
            Lane = lane;
            BeneficiaryIsHome = beneficiaryIsHome;
            ForPicked = forPicked;
            ScoredByPicked = scoredByPicked;
            HasWinTheCornerLeadIn = hasWinTheCornerLeadIn;
            Signature = signature;
            Diagnostics = diagnostics;
        }
    }
}
