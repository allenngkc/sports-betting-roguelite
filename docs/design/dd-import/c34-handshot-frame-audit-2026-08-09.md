# C34 bookkeeping — findings resting on hand-shot Game-view frames

**From:** room lead · **2026-08-09** · owed by **C38-cl** (register batch 20).

> The ruling's words: *"name any finding that rests on a hand-shot Game-view frame rather than a
> harness frame. I expect the answer to be 'none but Allen's two,' and it should be stated, not
> assumed — which is the whole of C34."*

**Stated, not assumed. The expected answer does not hold.** Room-side it is *none*. Studio-wide it is
**five findings across three frames**, and one of them is a ruling whose evidence half is hand-shot.

Nothing here re-opens a granted item. This is the bookkeeping the ruling asked for.

---

## 1. Room lane — none, and the reason is structural

**No room finding rests on a hand-shot Game-view frame.** Every room capture is taken by
`RoomViewCapture`, which renders `PlayerCamera` into a `RenderTexture` and reads it back with
`ReadPixels` (`Shoot()`, `RoomViewCapture.cs:1883-1921`). It never touches the Game view. The blur
close says the same thing from the other side — *"Harness frames go through a render texture and are
a different path"* — and this is that claim checked in the code rather than assumed.

Two near-misses worth naming so nobody has to re-derive them:

- **Gates 6/7/8** rest on Allen walking the build. That is his *eye*, not a frame, so the toggle
  cannot reach the verdict. The certification records the basis in its own words.
- **The backbuffer arm** would have been the exception — `ScreenCapture` reads the Game view — but it
  never produced a frame. It returned null in `-batchmode`, and the record says so. A finding that
  does not exist cannot rest on anything.

## 2. Beyond the room — three frames, five findings

### The frames

| # | frame | provenance |
|---|---|---|
| 1 | `surething-form-blurry.png` | Allen's playtest, 2026-08-09 |
| 2 | `phone-bookie-blurry.png` | Allen's playtest, 2026-08-09 |
| 3 | **Allen's desk-pose / walk PNG, 2026-08-08** | batch 19 §3; batch 20 calls it *"the frame that started this"* |

**Frame 3 is a third frame, not one of "the two."** Batch 20 names it in the very paragraph that
predicts "Allen's two", and it is dated a day earlier than the playtest pair. If "Allen's two" was
meant to count *surfaces* (laptop + phone) rather than frames, then the count is right and the
wording is loose — but as written the audit finds three.

### The findings

| # | finding | rests on | affected by the toggle? |
|---|---|---|---|
| A | **C38 (a)** — harness exonerated BY DIRECTION, `ramp/stroke 0.482` vs **Allen's 0.613** | 1, 2 | The comparison is *between* the paths, so the toggle is the subject, not a contaminant. Sound. |
| B | **C38-cl** — the cause is Game view's *Low Resolution Aspect Ratios* | 1, 2, 3 | Definitionally. The frame's defect **is** the finding. Sound. |
| C | **Batch 19 §3** — *"Allen's own desk-pose PNG is the decisive arm and the harness cannot clear itself"* | 3 | Sound as an argument about which instrument may rule; it does not read a value off the frame. |
| D | **S2-am — the legibility half** ⚠️ | *"Allen's own frame"*, 2026-08-09 | **UNRESOLVED. This is the one that matters.** |
| E | **Batch 21 — `PRICES FINAL` conformance** | 1 (playtest frame) | **No.** Already argued immune — *"a string's presence does not depend on its sharpness"* — and corroborated at HEAD in `SportsbookApp.cs:98`. Two lines of evidence, one of them source. Sound. |

## 3. Finding D, stated plainly

S2-am ruled that the 13 px authoring floor *"is no longer sufficient alone"* and gains a second half
in the output channel. Its measured half is harness-derived (*"Measured at the ratified acceptance
view"*). Its **legibility** half is not:

> *"Read at review distance on Allen's own frame (this seat, 2026-08-09): the season records (`6-3`,
> `4-5`) and the row numbers `01`–`06` sit at or below legibility."*

**The toggle halves the rendered resolution.** If that frame was shot before the setting was found
and turned off, the legibility verdict is harsher than the build the player runs — and a *new
enforcement half was added to a standing floor* on the strength of it.

**I cannot resolve this from this worktree.** The frame is not in the room lane, and what I would need
is its capture time relative to the moment the toggle went off — which is exactly the state C34 says
an unpinned frame does not carry. Two honest possibilities, and I am not choosing between them:

- shot after the fix → the verdict stands untouched;
- shot before → the named elements may already clear at review distance, and S2-am's second half was
  motivated by a resample rather than by the build.

**Routed, not ruled.** It belongs to the DD and the SureThing seat. It is cheap to settle: the frames
that would settle it now exist as harness captures (§4).

## 4. What this lane can offer toward settling it

Taken the same day, at the same acceptance view, through the harness rather than the Game view —
`artifacts/room-visual-pass/baseline/2026-08-09-s2am2-two-surface-baseline.txt`:

| surface | ramp px | stroke px |
|---|---|---|
| laptop | **1.923** | 2.709 |
| phone | **2.409** | 4.249 |

These are Game-view-free by construction, so re-reading D's named elements against them separates the
build's softness from the display's. **One caveat that must travel with any such comparison:** the
same run found that **ramp ÷ stroke is not monotonic in blur** on dense text — the stroke grows faster
than the ramp as glyphs merge, so the ratio falls while the surface softens. S2-am's enforcement half
is expressed in that ratio. The **ramp** is the well-behaved quantity; the ratio is only safe against
an identical string at an identical size. That is a separate note to the DD and is in the baseline
report.

## 5. Scope (C25)

*Reads:* every `docs/` finding in this worktree that cites a hand-shot, Game-view, playtest or
screenshot frame, plus the room's own capture path in code. *Cannot see:* other worktrees' unmerged
docs (`surething-ui`, `tv-sweat`, `markets-2` at their own tips), anything that used such a frame
without saying so, and the capture time of any of the three frames. **The claim is "none in the room,
five named beyond it," not "five exist in the studio."**
