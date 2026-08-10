using System;

namespace SBR.Game
{
    /// <summary>
    /// Phase 2A (PRD §4.3, §9): an immutable, deterministic presentation key plus named,
    /// independent sub-hash channels. This is pure logic — it is not wired into
    /// <see cref="TheaterChoreographer"/> or <see cref="TheaterStage"/> yet, and constructing or
    /// querying it has no effect on anything that currently renders.
    ///
    /// <para><b>What "deterministic" means here.</b> Every discrete presentation choice this key
    /// feeds (movement grammar, lane, pressure, spacing, payoff shape, actor routing, reaction
    /// pattern) must reproduce identically for the same key across runs, processes, and machines.
    /// This type therefore touches no engine RNG (<c>RngHub</c> or any <c>Pcg32</c> stream), no
    /// <c>UnityEngine.Random</c>, no wall clock, no <c>Environment.TickCount</c>, no frame count,
    /// and no <c>Time.*</c>. It hashes only the immutable fields captured at construction using a
    /// self-contained FNV-1a / SplitMix64-style mixer implemented entirely in this file.</para>
    ///
    /// <para><b>Channel independence (PRD §4.3, "adding a new channel does not reshuffle existing
    /// channels").</b> Every channel query is a pure function of
    /// <c>(precomputed material hash, channel name)</c> — never a draw off a shared, advancing
    /// stream. Concretely: the seven key fields are folded once, in a fixed order, into a single
    /// <see cref="_materialHash"/> at construction time. Each call to <see cref="Channel"/>,
    /// <see cref="Normalized"/>, or <see cref="Pick"/> re-mixes that same fixed material hash with
    /// the requested channel name and returns the result — it never mutates any field, never
    /// consumes a cursor, and never depends on which channels were queried before it or in what
    /// order. A brand-new channel name therefore can never perturb <c>"grammar"</c>,
    /// <c>"lane"</c>, <c>"pressure"</c>, <c>"payoff"</c>, or <c>"reaction"</c> — it simply produces
    /// its own independent value alongside them. This is the property a sequential-stream design
    /// (e.g. seeding one RNG from the key and drawing five times) cannot guarantee, because the
    /// N-th draw's value depends on the N-1 draws before it — insert a new channel earlier in the
    /// draw order and every later channel silently changes.</para>
    ///
    /// <para><b>Key material and the leg-index design question (PRD §4.3 vs §8.2A).</b> The PRD's
    /// literal key formula is <c>run seed + round + ticket index + leg index + event step + scene
    /// template + beneficiary</c>. §8.2A establishes that two or more legs can be live on the same
    /// match simultaneously (a moneyline leg and a total-goals leg riding one match, for example).
    /// A scene beat is staged on the shared theater stage for that MATCH — §8.2A is explicit that
    /// "the stage serves all legs on that match simultaneously; it is not the stage for the active
    /// leg." If this key included <c>leg index</c> literally, the same on-screen beat would hash
    /// to two different keys depending on which of the match's concurrently-live legs happened to
    /// be asking, which breaks "same beat, same scene" the moment concurrency is real — and
    /// whichever leg's index actually got used would become an arbitrary, unstable choice that
    /// changes as legs settle out from under a still-live match.
    ///
    /// The fix implemented here: this type has <b>no leg-index field at all</b>. In its place the
    /// key material carries <see cref="MatchIndex"/> — the stable identity of the match itself
    /// (today, <c>Leg.Matchup.Index</c>; engine <c>Matchup</c> objects, and their locked stat
    /// line, are already shared by every leg that references them, per
    /// <c>Matchup.StatLine</c>'s own doc comment). Every leg riding the same match supplies the
    /// same <see cref="MatchIndex"/>, so two concurrently-live legs on one match necessarily
    /// construct the identical <see cref="PresentationSceneKey"/> for the same beat — scene
    /// selection is stable regardless of which leg is asking, and stays stable as one leg settles
    /// while another remains live, because settling a leg does not change the match's identity.
    /// A ticket that spans more than one match (an ordinary multi-match parlay) still gets
    /// distinct keys per match, because <see cref="MatchIndex"/> differs — only
    /// <see cref="TicketIndex"/> would not have been enough to tell those apart.
    ///
    /// This corroborates a pattern already present in the engine: <c>RngHub.DeriveMatch(round,
    /// matchupIndex, purpose)</c> already derives match-scoped randomness (rosters, scorer
    /// attribution) keyed by matchup index and kept deliberately separate from
    /// <c>RngHub.Derive(round, ticketId, legIndex, action, ordinal)</c>, which is leg/ticket
    /// scoped on purpose. The engine's own RNG hub already treats "which match" and "which leg's
    /// wager" as different axes; this key follows the same split for presentation.
    ///
    /// <b>Recommended PRD correction:</b> §4.3's key formula should read <c>run seed + round +
    /// ticket index + match index + event step + scene template + beneficiary</c>, not
    /// <c>leg index</c>. Leg identity is real and still matters for LEG-SPECIFIC presentation
    /// (per-leg NEED/LIVE copy, the §7.7 backed-player locator's "which leg's player") — but that
    /// is a different concern from scene selection, which this type owns. A future leg-scoped
    /// derivation should fold the leg index into the CHANNEL NAME it queries (e.g.
    /// <c>key.Pick($"locator/{legIndex}", n)</c>) rather than into this key's shared material,
    /// keeping the base key match-scoped while still landing on a leg-specific, deterministic
    /// value when one is needed. That is a later dispatch's decision, not this one's.
    ///
    /// One further note for whoever builds that later work: today's engine still hands each leg
    /// its own independently-stepped <c>DramaEvent</c> stream (<c>DramaEvent.Step</c> is
    /// "1-based step within the leg," not within the match). Fixing <see cref="MatchIndex"/> here
    /// removes the "which leg" ambiguity from the KEY, but a fully shared, single event cursor per
    /// match (so that two legs on one match agree on what "the same beat" even IS) is an engine
    /// concern outside this file's boundary, and outside Phase 2A's scope.</para>
    ///
    /// <para><b>Stateless and immutable.</b> This is a <c>readonly struct</c>. Every field is
    /// <c>readonly</c>, set once in the constructor. No method mutates anything; every query
    /// method returns a freshly computed value from the fields already captured.</para>
    /// </summary>
    public readonly struct PresentationSceneKey : IEquatable<PresentationSceneKey>
    {
        /// <summary>Names for the five channels PRD §4.3 calls out explicitly. Any other string
        /// is an equally valid channel name — these are a convenience, not an exhaustive set.</summary>
        public static class Channels
        {
            public const string Grammar = "grammar";
            public const string Lane = "lane";
            public const string Pressure = "pressure";
            public const string Payoff = "payoff";
            public const string Reaction = "reaction";
        }

        /// <summary>The run's seed string (matches <c>RngHub.RunSeed</c>'s type). Never read via
        /// <c>RngHub</c> itself — this type only hashes the string.</summary>
        public readonly string RunSeed;

        /// <summary>The round number.</summary>
        public readonly int Round;

        /// <summary>Which ticket, within the run, this beat belongs to.</summary>
        public readonly int TicketIndex;

        /// <summary>Which MATCH this beat belongs to — stable across every leg (and every ticket)
        /// that rides the same match. See the type doc for why this replaces "leg index" in the
        /// PRD's stated key material. Today's natural source is <c>Leg.Matchup.Index</c>.</summary>
        public readonly int MatchIndex;

        /// <summary>The event step this beat resolves. Caller-supplied; see the type doc's closing
        /// note on today's per-leg-scoped <c>DramaEvent.Step</c> versus a future match-scoped
        /// cursor.</summary>
        public readonly int EventStep;

        /// <summary>The factual scene template this beat resolved to (<see cref="SceneTemplate"/>
        /// from <c>ScenePlaybook</c>) — the truth-constrained fact contract this key elaborates,
        /// never a choice this key makes.</summary>
        public readonly SceneTemplate Template;

        /// <summary>Caller-defined beneficiary bit for this beat — mirrors the existing
        /// <c>SceneSpec.ForPicked</c> / <c>CountBeneficiaryIsHome</c> convention of using a bool
        /// for "which side benefits" rather than inventing a new vocabulary here.</summary>
        public readonly bool Beneficiary;

        private readonly ulong _materialHash;

        public PresentationSceneKey(
            string runSeed,
            int round,
            int ticketIndex,
            int matchIndex,
            int eventStep,
            SceneTemplate template,
            bool beneficiary)
        {
            RunSeed = runSeed ?? string.Empty;
            Round = round;
            TicketIndex = ticketIndex;
            MatchIndex = matchIndex;
            EventStep = eventStep;
            Template = template;
            Beneficiary = beneficiary;

            // Fold the key material once, in a fixed field order, into a single accumulator.
            // This happens exactly once, at construction — it is NOT a stream any channel query
            // advances. Every later Channel()/Normalized()/Pick() call re-mixes this same fixed
            // value with a channel name; none of them touch this loop again.
            ulong h = FnvOffset;
            h = Combine(h, Fnv1a64(RunSeed));
            h = Combine(h, HashInt(Round));
            h = Combine(h, HashInt(TicketIndex));
            h = Combine(h, HashInt(MatchIndex));
            h = Combine(h, HashInt(EventStep));
            h = Combine(h, HashInt((int)Template));
            h = Combine(h, Beneficiary ? 1UL : 0UL);
            _materialHash = h;
        }

        /// <summary>A stable 64-bit value for the named channel. Pure function of
        /// <c>(this key's material, channelName)</c> only — independent of every other channel,
        /// independent of call order, independent of how many other channels exist or have been
        /// queried before it.</summary>
        public ulong Channel(string channelName)
        {
            if (channelName == null) throw new ArgumentNullException(nameof(channelName));
            return Avalanche(Combine(_materialHash, Fnv1a64(channelName)));
        }

        /// <summary>The named channel's value normalized to [0, 1).</summary>
        public double Normalized(string channelName)
        {
            // Top 53 bits fill a double's mantissa cleanly and evenly across [0, 1).
            return (Channel(channelName) >> 11) * (1.0 / (1UL << 53));
        }

        /// <summary>Deterministically chooses an index in [0, candidateCount) for the named
        /// channel. Stable for a given (key, channelName, candidateCount); varies with any of
        /// them.</summary>
        public int Pick(string channelName, int candidateCount)
        {
            if (candidateCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(candidateCount), "candidateCount must be positive.");
            ulong h = Channel(channelName);
            return (int)(h % (ulong)candidateCount);
        }

        public bool Equals(PresentationSceneKey other)
            => RunSeed == other.RunSeed
            && Round == other.Round
            && TicketIndex == other.TicketIndex
            && MatchIndex == other.MatchIndex
            && EventStep == other.EventStep
            && Template == other.Template
            && Beneficiary == other.Beneficiary;

        public override bool Equals(object obj) => obj is PresentationSceneKey other && Equals(other);

        public override int GetHashCode() => unchecked((int)_materialHash);

        public override string ToString()
            => $"PresentationSceneKey(seed={RunSeed}, round={Round}, ticket={TicketIndex}, " +
               $"match={MatchIndex}, step={EventStep}, template={Template}, beneficiary={Beneficiary})";

        // ---- Self-contained deterministic hashing. No engine RNG, no UnityEngine.Random, no
        // wall clock, no Environment.TickCount, no frame count, no Time.*. Every function below
        // is a pure mapping from its inputs to a ulong. ----

        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        private static ulong Fnv1a64(string s)
        {
            ulong hash = FnvOffset;
            for (int i = 0; i < s.Length; i++)
            {
                hash ^= s[i];
                hash *= FnvPrime;
            }
            return hash;
        }

        private static ulong HashInt(int value)
        {
            ulong hash = FnvOffset;
            uint bits = unchecked((uint)value);
            for (int shift = 0; shift < 32; shift += 8)
            {
                hash ^= (bits >> shift) & 0xFF;
                hash *= FnvPrime;
            }
            return hash;
        }

        /// <summary>SplitMix64's finalizer — a well-known, public-domain avalanche mix. Pure
        /// function of its input; no internal state.</summary>
        private static ulong Avalanche(ulong x)
        {
            unchecked
            {
                x ^= x >> 30;
                x *= 0xBF58476D1CE4E5B9UL;
                x ^= x >> 27;
                x *= 0x94D049BB133111EBUL;
                x ^= x >> 31;
                return x;
            }
        }

        /// <summary>boost::hash_combine, 64-bit adapted, with the incoming value pre-avalanched
        /// so two structurally different values never combine to the same accumulator by
        /// coincidence of the raw FNV output alone.</summary>
        private static ulong Combine(ulong acc, ulong value)
        {
            unchecked
            {
                return acc ^ (Avalanche(value) + 0x9E3779B97F4A7C15UL + (acc << 6) + (acc >> 2));
            }
        }
    }
}
