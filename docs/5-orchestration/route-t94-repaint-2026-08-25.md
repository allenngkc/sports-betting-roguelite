# Route: three questions from TV's T156 re-take + bucket 1 turn — TV → DD (2026-08-25)

Commits merged: `5e5348e` (T156 re-take post-T168), `ca4f410` (WinningMargin
bucket 1 from T151-am3). EditMode 342/341/0/1, PlayMode 152/125/0/27, editor
closed. TV holds T94's seam until the DD answers §1.

## 1. T94's seam — repaint, not deletion? (blocks the build)

Batch 197 says the fix is a DELETION of the `LegsOfFixtureAfter` advance at
the two sites (`TvSweatScreen.cs` ~:2169 and ~:4870). TV read the sites: if the
line is simply removed, nothing repaints between the mark and the next
`RenderEvent`, so the ended fixture's rows keep their last paint — LIVE, still
pulsing — for the whole whistle-and-slam beat. T94 inverted: fixture f stays
lit too long instead of f+1 lighting too early. The pre-committed frame would
read that as (b) and conclude the deletion didn't land.

TV's proposed build — one line CHANGED, not deleted, at each site:

    UpdateTicketColumn(LegsOfFixtureAfter(evt.LegIndex));  →  UpdateTicketColumn(_liveLegsShown);

Drops the advance (197's objective), repaints so ended legs render RESOLVED
and `IsLive` clears, leaves nothing live during the beat — what the
pre-committed frame expects. T62's idiom, already used verbatim at :3006
("the CURRENT live set, unchanged"). `LegsOfFixtureAfter` still goes dead and
is still deleted. Same scope, same two lines, no new code — but a third thing
against 197's "deletion, not an ordering change". TV asks for the call rather
than substituting silently.

## 2. Bucket 1's ladder ends a rung early — 6.1px spare

Rung 2 `1 GOAL APART AT FT` fits at 254.9 against 261.0, so rung 3 never
renders for bucket 1. The singular GOAL is worth 13.9px and that is the whole
margin. Is a slender-margin gate owed, the way T143-am8 required one?

## 3. The multi-scorer counter may count one leg only (unverified)

`OnGoalPlayed` only inspects `_ticket.Legs[_stageLeg]`; on a same-match ticket
a multi-scorer leg that is not the anchor may never count.
`_scorerRevealedForActiveLeg` has carried that shape since before this build —
pre-existing structure the four-arm build makes visible as a wrong number.
TV is verifying on a run now (with T169-am's owed PlayMode fixture); routed
early so the DD can say whether it is T169-am's scope or a new row.
