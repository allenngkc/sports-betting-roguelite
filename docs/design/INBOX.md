# Design Director inbox — from the orchestrator

Items routed to this seat. Clear a line when it's registered or resolved.

## 2026-07-31

1. **C3 coverage rule (TV HDR).** Capability is fixed — since `1aa74c3` (2026-07-28)
   `TvSweatHdrUI.shader` carries an unclamped material float past the UGUI `Color32`
   clamp. The open question is coverage: which elements may exceed 1.0 without
   breaking the one-full-brightness rule. TV lead's detail:
   `[TV] docs/tv-sweat-refinement/c3-hdr-canvas-proposal.md`. Register C3 updated;
   room worktree is unblocked.
2. **Studio art-authority gap.** `design/08-art-direction.md` was deprecated
   2026-07-24 and nothing replaced it for the non-TV surfaces: `DESIGN.md` covers the
   TV only — room, laptop, and phone have no binding art authority. Raised by the TV
   lead; studio-level. Needs a proposal (scope + format) for Allen.
3. **SureThing form-guide identity.** The SureThing lead flags the shipped lobby as a
   "competent dark app": default sans, airy rows, no toner quality — the direction's
   most recognisable traits absent. Re-routed here under the new decision routing;
   related: S11 typeface licence unresolved, build renders in `LegacyRuntime.ttf`.
4. **R9 — ambient rebalance (room).** Candidate. Room lead's Phase B data says flat
   ambient now suppresses relief that directional bounce carries better. Changes the
   8/8-gated value structure, so approval + gate re-run required.
5. **R10 — couch-corner grazing source (room).** Candidate. Strongest normal map in
   the room still reads at 2.3%; bounce cannot rescue that corner.
