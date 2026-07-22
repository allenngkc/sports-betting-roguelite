using System;
using System.Collections.Generic;
using System.Linq;
using SBR.Engine;

namespace SBR.Game
{
    /// <summary>
    /// The working slip's state (M4) — pure C#, no Unity, so EditMode tests exercise it directly and
    /// the laptop UI stays a thin renderer. One slip is built at a time (M4 grill decision): clicking
    /// a slate side toggles the leg (same side removes, other side switches), stake is set by
    /// fraction-of-bank chips (10/25/50/MAX) or $10 nudges, and Place() hands the picks to the engine
    /// (which owns all validation and any compose-time relic odds rewrites).
    ///
    /// The slip's TO WIN preview uses BASE slate odds — a promo/boost relic can only make the placed
    /// ticket better than the preview, never worse (the placed-tickets list shows the real payout).
    /// </summary>
    public sealed class BetslipModel
    {
        private readonly Run _run;
        private readonly List<Pick> _picks = new List<Pick>();

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

        /// <summary>The slip's moneyline side on this matchup, if the selected leg is moneyline.</summary>
        public Side? SideOn(int matchupIndex)
        {
            foreach (Pick p in _picks)
            {
                if (p.MatchupIndex != matchupIndex) continue;
                if (p.Selection.Kind != MarketKind.Moneyline) return null;
                return p.Selection.Choice == MarketChoice.Home ? Side.Home : Side.Away;
            }
            return null;
        }

        /// <summary>The selected market on this matchup, if any.</summary>
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
        /// removes the leg when the same selection is clicked again. A matchup contributes at
        /// most one leg to the slip.</summary>
        public bool Toggle(int matchupIndex, MarketSelection selection)
        {
            if (matchupIndex < 0 || matchupIndex >= _run.CurrentSlate.Matchups.Count) return false;
            _ = _run.CurrentSlate.Matchups[matchupIndex].Odds(selection);
            BoostLeg = -1;
            for (int i = 0; i < _picks.Count; i++)
            {
                if (_picks[i].MatchupIndex != matchupIndex) continue;
                if (_picks[i].Selection == selection) _picks.RemoveAt(i);
                else _picks[i] = new Pick(matchupIndex, selection);
                return true;
            }

            if (_picks.Count >= _run.Config.MaxLegs) return false;
            _picks.Add(new Pick(matchupIndex, selection));
            return true;
        }

        public void Remove(int matchupIndex)
        {
            BoostLeg = -1;
            _picks.RemoveAll(p => p.MatchupIndex == matchupIndex);
        }

        public void Clear()
        {
            BoostLeg = -1;
            _picks.Clear();
        }

        /// <summary>Aims (or un-aims) the held Profit Boost at a slip leg. No-op without the item.</summary>
        public void ToggleBoost(int pickIndex)
        {
            if (pickIndex < 0 || pickIndex >= _picks.Count) return;
            if (!_run.OwnsConsumable("profit_boost")) return;
            BoostLeg = BoostLeg == pickIndex ? -1 : pickIndex;
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

        /// <summary>Parlay decimal odds over the slip's BASE slate prices.</summary>
        public double CombinedOdds
        {
            get
            {
                if (_picks.Count == 0) return 0.0;
                var odds = new List<double>(_picks.Count);
                foreach (Pick p in _picks)
                    odds.Add(_run.CurrentSlate.Matchups[p.MatchupIndex].Odds(p.Selection));
                return OddsMath.ParlayDecimal(odds);
            }
        }

        public double ToWin => _picks.Count == 0 ? 0.0 : Stake * CombinedOdds;

        /// <summary>Why PLACE is disabled, or null when it's live. UI renders the reason verbatim.</summary>
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
                return null;
            }
        }

        public bool CanPlace => PlaceBlocker == null;

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
