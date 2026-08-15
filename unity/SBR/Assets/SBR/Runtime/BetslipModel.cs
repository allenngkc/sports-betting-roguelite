using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

// This file is compiled into TWO projects: the Unity assembly that owns it, and `engine.tests` by
// source include (see SBR.Engine.Tests.csproj). Those projects disagree about nullable reference
// annotations, so the file declares its own context rather than warning in one of them.
#nullable disable

namespace SBR.Game
{
    /// <summary>
    /// The working slip's state (M4) — pure C#, no Unity, so EditMode tests exercise it directly and
    /// the laptop UI stays a thin renderer. One slip is built at a time (M4 grill decision): clicking
    /// a slate side toggles the leg (same side removes, other side switches), stake is set by
    /// fraction-of-bank chips (10/25/50/MAX) or $10 nudges, and Place() hands the picks to the engine
    /// (which owns all validation and any compose-time relic odds rewrites).
    ///
    /// <para><b>SAME MATCH (F_0.6.0 step 5, P1–P3).</b> A matchup may now contribute two or more legs.
    /// The slip is a LIST OF LEGS and always was; what changed is that the leg-addressed half of the
    /// API is now public (<see cref="AddLeg"/>, <see cref="RemoveLeg"/>, <see cref="LegIndicesOn"/>),
    /// the price is the ENGINE's ticket price rather than a product of legs (<see cref="TicketOdds"/>),
    /// and a refused combination is a structured verdict available before commit
    /// (<see cref="Refusal"/>). The matchup-keyed accessors are unchanged and are documented below for
    /// what they answer when a matchup carries several legs.</para>
    ///
    /// <para><b>Interaction is not decided here.</b> Whether a second market on a match should REPLACE
    /// or ADD is a design decision owned by the surface; <see cref="Toggle"/> keeps its replace
    /// semantics exactly, and <see cref="AddLeg"/> exists so the screen can choose the other gesture.
    /// The model exposes the capability and never picks the gesture.</para>
    ///
    /// The slip's TO WIN preview uses BASE slate odds — a promo/boost relic can only make the placed
    /// ticket better than the preview, never worse (the placed-tickets list shows the real payout).
    /// </summary>
    public sealed class BetslipModel
    {
        private readonly Run _run;
        private readonly List<Pick> _picks = new List<Pick>();

        // ---- derived-state cache. See EnsurePriced: the price now costs an engine call, and the UI
        // reads it every rebuild, so it is computed once per distinct slip state instead of per read.
        private int _revision;
        private int _pricedRevision = -1;
        private Slate _pricedSlate;
        private double _pricedOverround;
        private double _pricedMargin;
        private double _ticketOdds;
        private SameMatchPrice _pricedSameMatch;
        private TicketRefusal _pricedRefusal;

        /// <summary>Whole-dollar stake. Clamped to [MinStake, MaxStakeFraction × bank] on every set.</summary>
        public double Stake { get; private set; }

        /// <summary>Index into Picks of the leg a held Profit Boost lands on, or −1. Any leg
        /// mutation clears it (indices shift; the player re-aims deliberately).</summary>
        public int BoostLeg { get; private set; } = -1;

        /// <summary>The locked contract modifier this slip will place with (one per ticket —
        /// the one-modifier law; charm expansion). Cleared with the slip.</summary>
        public TicketModifier Modifier { get; private set; } = TicketModifier.None;

        public IReadOnlyList<Pick> Picks => _picks;

        public BetslipModel(Run run)
        {
            _run = run ?? throw new ArgumentNullException(nameof(run));
            SetStakeFraction(0.10);
        }

        // ------------------------------------------------------------------ legs

        /// <summary>The slip's moneyline TEAM on this matchup, if the selected leg backs a team.
        ///
        /// Returns null for a moneyline DRAW, and that is the ratified design rather than a
        /// convenience: the draw is not a team, ever (DD batch 49). The old shape here was
        /// <c>Choice == Home ? Home : Away</c>, which answered **Away** for the X of 1X2 — a
        /// silently wrong team on a surface whose whole job is showing which side you backed.
        /// Unreachable today only because no surface can yet build a draw pick; it goes live the
        /// moment the board grows its third row, which is queued Phase S work.
        ///
        /// <para><b>With several legs on the matchup this answers for the FIRST of them</b>, which is
        /// the pre-P1 behaviour preserved exactly (it always scanned for the first). A surface that
        /// renders a SAME MATCH group must address legs, not matchups — see
        /// <see cref="LegIndicesOn"/>.</para></summary>
        public Side? SideOn(int matchupIndex)
        {
            foreach (Pick p in _picks)
            {
                if (p.MatchupIndex != matchupIndex) continue;
                if (p.Selection.Kind != MarketKind.Moneyline) return null;
                return p.Selection.Choice switch
                {
                    MarketChoice.Home => Side.Home,
                    MarketChoice.Away => Side.Away,
                    _ => (Side?)null,
                };
            }
            return null;
        }

        /// <summary>The selected market on this matchup, if any — the FIRST leg on it when the
        /// matchup carries several (pre-P1 behaviour, preserved exactly). Ask
        /// <see cref="Contains"/> instead when the question is "is THIS selection on the slip",
        /// which is what a market row on a SAME MATCH board needs.</summary>
        public MarketSelection? SelectionOn(int matchupIndex)
        {
            foreach (Pick p in _picks)
                if (p.MatchupIndex == matchupIndex) return p.Selection;
            return null;
        }

        /// <summary>Slate-side click, retained as a convenience for the moneyline board.</summary>
        public bool Toggle(int matchupIndex, Side side)
            => Toggle(matchupIndex, MarketSelection.Moneyline(side));

        /// <summary>Adds a market leg, replaces a different selection for the same matchup, or
        /// removes the leg when the same selection is clicked again.
        ///
        /// <para><b>REPLACE semantics are deliberate and unchanged</b> (P1). Whether a second market
        /// on a match should add rather than replace is an interaction decision the surface owns;
        /// this gesture keeps meaning what it has always meant, and <see cref="AddLeg"/> is the
        /// explicit other gesture.</para>
        ///
        /// <para>The two scans below are one behaviour on any slip with at most one leg per matchup —
        /// every slip the pre-P1 model could hold — because both find the same pick. They differ only
        /// on a matchup carrying several legs, where scanning for THIS selection first is what stops
        /// an un-click from being read as a replacement and silently manufacturing a duplicate.</para></summary>
        public bool Toggle(int matchupIndex, MarketSelection selection)
        {
            if (matchupIndex < 0 || matchupIndex >= _run.CurrentSlate.Matchups.Count) return false;
            _ = _run.CurrentSlate.Matchups[matchupIndex].Odds(selection);
            BoostLeg = -1;

            for (int i = 0; i < _picks.Count; i++)
            {
                if (_picks[i].MatchupIndex != matchupIndex || _picks[i].Selection != selection) continue;
                _revision++;
                _picks.RemoveAt(i);
                return true;
            }

            for (int i = 0; i < _picks.Count; i++)
            {
                if (_picks[i].MatchupIndex != matchupIndex) continue;
                _revision++;
                _picks[i] = new Pick(matchupIndex, selection);
                return true;
            }

            if (_picks.Count >= _run.Config.MaxLegs) return false;
            _revision++;
            _picks.Add(new Pick(matchupIndex, selection));
            return true;
        }

        /// <summary>Puts an ADDITIONAL leg on the slip, even when the matchup already carries one —
        /// the SAME MATCH gesture (P1). Returns false when the matchup is off the slate or
        /// <c>MaxLegs</c> is reached; throws, as <see cref="Toggle"/> does, when the selection is not
        /// on this matchup's board.
        ///
        /// <para><b>A combination this builds may be unsellable, and that is the point.</b> The slip
        /// is a working document; the verdict is <see cref="Refusal"/>, which needs the combination to
        /// exist before it can name a cause and a remedy for it. So a repeat of a selection already on
        /// the slip is accepted here and refused there, rather than being silently swallowed by the
        /// one API that could have reported it.</para></summary>
        public bool AddLeg(int matchupIndex, MarketSelection selection)
        {
            if (matchupIndex < 0 || matchupIndex >= _run.CurrentSlate.Matchups.Count) return false;
            _ = _run.CurrentSlate.Matchups[matchupIndex].Odds(selection);
            if (_picks.Count >= _run.Config.MaxLegs) return false;

            BoostLeg = -1;
            _revision++;
            _picks.Add(new Pick(matchupIndex, selection));
            return true;
        }

        /// <summary>Adds a moneyline side as an additional leg. The slate-side convenience of
        /// <see cref="AddLeg"/>, mirroring <see cref="Toggle(int, Side)"/>.</summary>
        public bool AddLeg(int matchupIndex, Side side)
            => AddLeg(matchupIndex, MarketSelection.Moneyline(side));

        /// <summary>Removes ONE leg, addressed by its index into <see cref="Picks"/> — the removal a
        /// SAME MATCH group needs, since <see cref="Remove"/> is matchup-wide and would take the whole
        /// group. Indices are the same ones <see cref="TicketRefusal.CauseLegs"/> and
        /// <see cref="TicketRefusal.RemedyLegs"/> speak in, so a remedy can be spent directly.</summary>
        public bool RemoveLeg(int legIndex)
        {
            if (legIndex < 0 || legIndex >= _picks.Count) return false;
            BoostLeg = -1;
            _revision++;
            _picks.RemoveAt(legIndex);
            return true;
        }

        /// <summary>Removes the leg carrying this exact selection on this matchup, if it is on the
        /// slip. Index-free, so a surface rebuilt between the render and the click cannot remove the
        /// wrong leg.</summary>
        public bool RemoveSelection(int matchupIndex, MarketSelection selection)
        {
            for (int i = 0; i < _picks.Count; i++)
                if (_picks[i].MatchupIndex == matchupIndex && _picks[i].Selection == selection)
                    return RemoveLeg(i);
            return false;
        }

        /// <summary>Removes EVERY leg on this matchup — unchanged from before P1, where "every" was
        /// always "the one". On a SAME MATCH group this takes the whole group, which is the honest
        /// reading of a matchup-keyed remove; use <see cref="RemoveLeg"/> to take one.</summary>
        public void Remove(int matchupIndex)
        {
            BoostLeg = -1;
            _revision++;
            _picks.RemoveAll(p => p.MatchupIndex == matchupIndex);
        }

        public void Clear()
        {
            BoostLeg = -1;
            _revision++;
            _picks.Clear();
        }

        /// <summary>How many legs this matchup contributes. Two or more is a SAME MATCH group.</summary>
        public int LegCountOn(int matchupIndex)
        {
            int count = 0;
            foreach (Pick p in _picks)
                if (p.MatchupIndex == matchupIndex) count++;
            return count;
        }

        /// <summary>Is this exact selection on the slip. What a market row asks to draw itself
        /// selected — <see cref="SelectionOn"/> can only answer for one leg per matchup.</summary>
        public bool Contains(int matchupIndex, MarketSelection selection)
        {
            foreach (Pick p in _picks)
                if (p.MatchupIndex == matchupIndex && p.Selection == selection) return true;
            return false;
        }

        /// <summary>The indices into <see cref="Picks"/> of every leg on this matchup, ascending —
        /// the group a surface draws THE HOUSE'S LINE across.</summary>
        public IReadOnlyList<int> LegIndicesOn(int matchupIndex)
        {
            List<int> found = null;
            for (int i = 0; i < _picks.Count; i++)
            {
                if (_picks[i].MatchupIndex != matchupIndex) continue;
                if (found == null) found = new List<int>(2);
                found.Add(i);
            }
            return found ?? (IReadOnlyList<int>)Array.Empty<int>();
        }

        /// <summary>Does any matchup carry two or more legs — is this a SAME MATCH ticket at all.
        /// Computed off the picks, so it costs nothing and cannot throw.</summary>
        public bool IsSameMatch
        {
            get
            {
                for (int i = 0; i < _picks.Count; i++)
                    for (int j = i + 1; j < _picks.Count; j++)
                        if (_picks[i].MatchupIndex == _picks[j].MatchupIndex) return true;
                return false;
            }
        }

        /// <summary>Aims (or un-aims) the held Profit Boost at a slip leg. No-op without the item.</summary>
        public void ToggleBoost(int pickIndex)
        {
            if (pickIndex < 0 || pickIndex >= _picks.Count) return;
            if (!_run.OwnsConsumable("profit_boost")) return;
            BoostLeg = BoostLeg == pickIndex ? -1 : pickIndex;
            _revision++; // the boost lifts the price the sub-evens rule judges — see Refusal.
        }

        /// <summary>Toggles a locked contract modifier onto the slip (Free Bet xor DoN — picking
        /// one replaces the other). No-op without the matching consumable held.</summary>
        public void ToggleModifier(TicketModifier modifier)
        {
            if (modifier == TicketModifier.FreeBet && !_run.OwnsConsumable("free_bet")) return;
            if (modifier == TicketModifier.DoubleOrNothing && !_run.OwnsConsumable("double_or_nothing")) return;
            Modifier = Modifier == modifier ? TicketModifier.None : modifier;
        }

        // ------------------------------------------------------------------ stake

        /// <summary>A fraction chip: stake = floor(fraction × bank), clamped. 1.0 = the MAX chip.</summary>
        public void SetStakeFraction(double fraction)
            => Stake = ClampStake(Math.Floor(fraction * _run.Bank));

        /// <summary>The −/+ $10 nudge buttons.</summary>
        public void Nudge(double dollars) => Stake = ClampStake(Stake + dollars);

        private double ClampStake(double stake)
        {
            double max = Math.Floor(_run.Config.MaxStakeFraction * _run.Bank);
            return Math.Max(_run.Config.MinStake, Math.Min(stake, max));
        }

        // ------------------------------------------------------------------ derived

        /// <summary>
        /// THE TICKET'S PRICE over the slip's BASE slate prices — the engine's own, not a product of
        /// legs (P2, F_0.6.0).
        ///
        /// <para><b>Why this is not <c>ParlayDecimal</c> any more.</b> Two legs on one match are
        /// correlated, so multiplying them is not this ticket's price; and by S73 / the 2026-08-12
        /// amendment the product-of-legs figure is the one number the surface may never display for a
        /// SAME MATCH ticket. The model therefore no longer computes it, and the surface cannot render
        /// it by accident. This is the same branch <c>Run.PlaceTicket</c> takes, on the same legs.</para>
        ///
        /// <para><b>Ordinary tickets are unaffected to the bit.</b> With at most one leg per matchup
        /// the joint path is not entered and the expression is literally the old one — the same odds,
        /// in the same order, through the same <see cref="OddsMath.ParlayDecimal"/>.</para>
        ///
        /// <para><b>BASE odds, as before.</b> An aimed Profit Boost is deliberately NOT priced in, so
        /// the placed ticket can only beat the preview (<c>ProfitBoostMult</c> &gt; 1 multiplies the
        /// ticket price on both paths). The refusal verdict does read the boost — see
        /// <see cref="Refusal"/> — because a verdict that disagreed with placement would be wrong.</para>
        /// </summary>
        public double TicketOdds
        {
            get { EnsurePriced(); return _ticketOdds; }
        }

        /// <summary>The ticket's price. Retained name; <see cref="TicketOdds"/> is the same figure and
        /// says what it now is — this has not been a combination of leg odds since P2.</summary>
        public double CombinedOdds => TicketOdds;

        public double ToWin => _picks.Count == 0 ? 0.0 : Stake * TicketOdds;

        /// <summary>The joint pricing of this slip, or null when no matchup carries two or more legs.
        /// Carries <c>Principal</c> and <c>Relations</c> — the parts a surface composes the slip's one
        /// relation statement from, and never an English string.</summary>
        public SameMatchPrice SameMatchPricing
        {
            get { EnsurePriced(); return _pricedSameMatch; }
        }

        /// <summary>
        /// THE STRUCTURED REFUSAL, or null when the combination would be sold (P3; S73-am4,
        /// <c>docs/design/surething-design.md</c> §3.3). Non-throwing, so a surface stamps a Blocked
        /// control with CAUSE and REMEDY <i>before</i> the player commits rather than discovering the
        /// verdict when <see cref="Place"/> throws.
        ///
        /// <para>It is <c>Run.RefusalFor</c>'s answer, on this slip's picks and this slip's aimed
        /// Profit Boost — the same rule set placement runs, so a slip that reads null here places.
        /// <c>CauseLegs</c> and <c>RemedyLegs</c> index <see cref="Picks"/>; the remedy is a SET, not
        /// a leg, and must be spent as one (at κ = 1 it is always a single leg; above it, it is not).</para>
        ///
        /// <para>Judges the combination only — phase, bank, stake and ticket count are
        /// <see cref="PlaceBlocker"/>'s business, so this stays answerable while a slip is still being
        /// built and a refused leg stays reachable on its own.</para>
        /// </summary>
        public TicketRefusal Refusal
        {
            get { EnsurePriced(); return _pricedRefusal; }
        }

        /// <summary>Why PLACE is disabled, or null when it's live. UI renders the reason verbatim.
        ///
        /// <para><b>Except for a refusal, which no single string can carry.</b> A refused combination
        /// is a Blocked state owing CAUSE and REMEDY, so this returns the machine token
        /// <c>"refused:&lt;Kind&gt;"</c> — enough to keep <see cref="CanPlace"/> false, and
        /// deliberately not copy. A surface MUST branch on <see cref="Refusal"/> and stamp the parts;
        /// printing this token verbatim is a bug, and it is a token precisely so that bug is loud
        /// instead of a plausible-looking sentence the model had no authority to write.</para>
        ///
        /// <para>Order is unchanged, and the refusal is checked last: it can only fire on a matchup
        /// carrying two or more legs, so every answer reachable before P1 is the answer it was.</para></summary>
        public string PlaceBlocker
        {
            get
            {
                if (_run.Phase != Phase.Betting) return "betting is closed";
                if (_run.Tickets.Count >= _run.Config.MaxTicketsPerRound)
                    return $"max {_run.Config.MaxTicketsPerRound} tickets";
                if (_picks.Count == 0) return "pick a side";
                if (_run.Bank < _run.Config.MinStake) return "bank too small";
                if (Stake > _run.Config.MaxStakeFraction * _run.Bank) return "stake exceeds bank";

                TicketRefusal refusal = Refusal;
                return refusal == null ? null : "refused:" + refusal.Kind;
            }
        }

        public bool CanPlace => PlaceBlocker == null;

        /// <summary>
        /// Prices the slip and reads its refusal, once per distinct slip state.
        ///
        /// <para>Cached because the price is now an engine call — the joint path also builds the
        /// survivor-subset table — while <see cref="CombinedOdds"/> is a property an immediate-mode UI
        /// reads on every rebuild. The key is the whole of what the answer depends on: which legs
        /// (<c>_revision</c>, bumped by every mutation including re-aiming the boost), which slate, and
        /// the two margin dials. A matchup's odds are fixed once its slate is generated, so a slate
        /// reference is a sound part of that key.</para>
        /// </summary>
        private void EnsurePriced()
        {
            if (_pricedRevision == _revision
                && ReferenceEquals(_pricedSlate, _run.CurrentSlate)
                && _pricedOverround == _run.Config.Overround
                && _pricedMargin == _run.Config.SgpMargin)
                return;

            _pricedRevision = _revision;
            _pricedSlate = _run.CurrentSlate;
            _pricedOverround = _run.Config.Overround;
            _pricedMargin = _run.Config.SgpMargin;

            _ticketOdds = 0.0;
            _pricedSameMatch = null;
            _pricedRefusal = null;
            if (_picks.Count == 0) return;

            var legs = new List<Leg>(_picks.Count);
            foreach (Pick p in _picks)
            {
                Matchup matchup = _pricedSlate.Matchups[p.MatchupIndex];
                legs.Add(new Leg(matchup, p.Selection, matchup.Odds(p.Selection)));
            }

            // The verdict comes through Run's own route, not straight to SameMatchModel: its doc
            // comment is explicit that a pre-checked slip and the placement that follows it must be
            // judging the same legs, built once.
            _pricedRefusal = _run.RefusalFor(_picks, BoostLeg);

            if (SameMatchModel.IsSameMatch(legs))
                _pricedSameMatch = SameMatchModel.Price(legs, _pricedOverround, _pricedMargin);

            // PlaceTicket's own expression, with boost = 1 (the BASE-odds preview). Under the no-label
            // fallback "the price does not move", so it is the product there too — as it is sold.
            _ticketOdds = _pricedSameMatch == null || _pricedSameMatch.NaiveFallback
                ? OddsMath.ParlayDecimal(legs.Select(l => l.OfferedOdds).ToList())
                : _pricedSameMatch.Price;
        }

        // ------------------------------------------------------------------ place

        /// <summary>Places the slip as an engine ticket (playing an aimed Profit Boost), clears it,
        /// and re-anchors the stake. Callers should check CanPlace; engine exceptions bubble.</summary>
        public Ticket Place()
        {
            Ticket ticket = _run.PlaceTicket(_picks.ToList(), Stake, BoostLeg, Modifier);
            Clear();
            Modifier = TicketModifier.None;
            SetStakeFraction(0.10);
            return ticket;
        }
    }
}
