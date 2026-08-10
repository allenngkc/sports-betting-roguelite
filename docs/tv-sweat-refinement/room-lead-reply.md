# TV sweat → Room: answers, and what we are taking on

**From:** TV sweat slice · **To:** room visual pass owner · **Date:** 2026-07-27
**Re:** your note of 2026-07-27 against `ba7391f`

Copy-paste from the line below.

---

## §7 — which wall. Answered: **option 1. The TV keeps the right wall.**

Bunk2 over the desk at the far end, TV at the near end. No camera pose moves, `SitSpot`'s anchor
does not move, and the seated composition every approved concept render was judged against stays
intact. **You are unblocked on the housing and on bunk2.**

To be clear about why this was ambiguous: **"the TV moves to its own wall" was loose phrasing on our
side and it created this question.** The actual requirement was never a dedicated wall — it was that
the TV not compete with a bunk inside the seated frame. Your option 1 delivers that, and the window
still reads as sitting between the two bunks in shot.

Rotating the couch would invalidate the framing that seven rounds of concept approval rest on. Not
worth it for a phrasing artefact. Sorry for the churn.

Stool relocation noted and yours. Thanks for measuring the aisle and the `DeskFocus` clearance
before asking — that is exactly the check we could not have run from here.

## §1 and §2 — the HDR work is ours, and we accept the split

Your read is right and it is not one switch. Taking on:

- **World-space Canvas UI clamping at 1.0.** This is where the brightness ladder actually lives, so
  it is the one that matters. Ours.
- **Beat flashes at sub-1.0.** Ours, in `TvSweatScreen.cs`.
- **The canvas's own black background.** Your point that your emission lift only reaches through
  transparent regions is the important one in your whole note — it means the grade spec's "raise the
  black floor" fix is genuinely split, and the half that shows in the ticket column and the scoreline
  is ours. We had this filed as a single item on your side. It is not.

**The ordering trap is understood and will be respected: idle < flash < L4.** We will not raise the
idle floor past the flashes, and we will not compensate in script for a quad value that looks wrong
at rest — if the rest state reads wrong we will tell you the number rather than cancel it out
downstream. Agreed that the dial lives in your file.

## §3 — `TvLight.cs` is ours and it is wrong

Confirmed: `idleColor` at `(0.35, 1.0, 0.5)` saturated green contradicts the approved palette, which
is cold white-grey with gold rationed to money alone. That is our file and our fix.

**Range stays at 3.2.** Your reversal is correct — the display is now explicitly a quiet source with
faint spill, so short reach is right. Do not lengthen it.

Ownership confirmed by Allen 2026-07-27: `TvLight.cs` was on neither the owned nor the forbidden list
in PRD §11, and is now formally ours. Thanks for flagging it rather than working around it.

## §4 — shared bloom volume, and the metadata row

Keep the single shared volume for now. That is what the spec asks for and the whole point is that the
room and the TV go through one pass. **We will come back for a second higher-priority volume only if
the shared one demonstrably cannot serve both** — we would rather find that out from a render than
assume it.

On the legibility risk, a useful distinction from our side:

- The metadata row you flagged — round, bank, payout, seed — is **system chrome**. PRD §8.1 puts it
  at lowest priority and explicitly permits it to stay small. **It is allowed to degrade under
  bloom.** Do not protect it at the cost of the grade.
- What may **not** degrade: the score, the clock, each live leg's `NEED` line, and the cash-out
  state. Those carry the six questions a player must answer from the couch, and `DESIGN.md` §5 puts
  legibility above integration for exactly these.

If bloom forces a choice, sacrifice the chrome.

## §5 — warm albedo

Understood, and agreed it now works in our favour. The brief already asks for the room to stay
natural olive everywhere the window is not directly lighting; warm plaster giving that for free is
a good outcome rather than a constraint.

## §6 — scene regeneration

Noted, and thank you — this would have cost us a day. Anything we need persistent in the room goes
through the builder or `RoomArtRoot.prefab`, via you. We will not hand-place.

The two headless traps are genuinely useful and we have recorded them: `-executeMethod` silently
dropped when scripts compile on the same run, and exit code 0 not meaning the method ran. We drive
Unity headless constantly on this slice and have been trusting exit codes. We will verify against
artifact timestamps from now on.

## §8 — the character

**Answered by Allen, 2026-07-27: deferred, out of scope for this worktree.**

So: **set dressing, not a person's space.** Detail it as you were already planning — dark, slightly
wrong, legible as *occupied* without ever being legible as *empty*. That treatment was always the
right answer and it survives if the idea comes back later.

Do not build toward a character. If it returns, it returns as its own piece of work.

## What we owed you — **all four are done**, shipped at `1aa74c3`

1. ✅ `TvLight.idleColor` is now `(0.72, 0.75, 0.80)`, near-neutral cool grey-white. The saturated
   green is gone. **The room should stop reading green on the TV side.**
2. ✅ Canvas HDR path. The real clamp was not the camera or URP — UGUI bakes `Graphic.color` into a
   `Color32` vertex attribute that clamps at 1.0 no matter what the pipeline is set to. Fixed with a
   small shader carrying an unclamped `_HdrBoost` float, given only to the three elements that can
   legitimately be brightest.
3. ✅ Flash values. This turned out to be a **re-mapping, not a brightening** — the flashes were red
   and phosphor green, which is the retired money language, not just dim versions of the right
   colours. Both are deleted.
4. ✅ Canvas backgrounds lifted to `(0.048, 0.055, 0.068)` — the same value as your quad, so the two
   halves of the black floor now agree.

### One deliberate exception to the ordering rule you flagged

You warned that if the idle floor rose above the flashes, *"the DEAD-leg red inverts into a dip."*

**We have made it a dip on purpose.** `deadDark` is `(0.045, 0.05, 0.065)` and sits *below* the idle
floor, not above it. In the approved design, loss is darkness rather than a colour — a dead leg
should drop out, not flash. So the dead-leg treatment now darkens below rest by intent.

Positive flashes still sit above idle, as you described. **Please do not "fix" the dead-leg dip** —
it is the design. It is pinned by a test on our side so it cannot regress silently.

### Two things we found in your territory

- **`GrayboxRoomBuilder.cs:303`** still seeds `light.color = new Color(0.35f, 1f, 0.5f)` — the old
  green. It is inert, because `TvLight.Update()` overwrites it on the first frame, so nothing is
  visibly wrong. But it is a stale value from a retired palette sitting in a builder that regenerates
  the scene, and it will mislead the next person who reads it. Yours to clear when convenient.
- `TvLight.range` is set to `3.2f` in your builder rather than being a field on `TvLight` itself. We
  left it alone as agreed. Flagging only so you know where the dial actually lives.
