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
   most recognisable traits absent. Re-routed here under the new decision routing.
   S11 is **ruled** (Allen, 2026-07-31): no licence-encumbered typefaces — Bell
   Centennial is dropped; spec a free-licence replacement (OFL or similar) as part of
   this item. Build currently renders in `LegacyRuntime.ttf`.
4. **R9 — ambient rebalance (room).** Candidate. Room lead's Phase B data says flat
   ambient now suppresses relief that directional bounce carries better. Changes the
   8/8-gated value structure, so approval + gate re-run required.
5. **R10 — couch-corner grazing source (room).** Candidate. Strongest normal map in
   the room still reads at 2.3%; bounce cannot rescue that corner.
6. **TV slip-strip raw-hex colours.** `UpdateSlipStrip` embeds `#3CE873` (green),
   `#FF4038` (red), `#9EDCF6` (cyan) as rich-text string markup — the retired money
   language surviving where field-level palette scans can't see it. Same violation
   class as T8. Logged as `[TV] DESIGN.md` §9A item 5; untouched pending your
   ruling.
7. **Lost-ticket oxide red (SureThing).** `OldSlipsApp.BuildLedgerTicket` tints a
   lost ticket's state and payout in oxide red. Plausibly legitimate as the house's
   mark on a settled ticket; sits against the amended red law (S3, house's mark
   only). Untouched pending your ruling.
8. **Three-way naming clash (SureThing).** LEDGER / Old Slips / SURETHING LEDGER —
   overlapping names across surfaces; needs a naming ruling before S9's screens
   harden the copy.
