# SureThing UI — re-seat state

**Written:** 2026-08-01, at a session hygiene clear. **Last updated:** 2026-08-03, after S48.
**HEAD:** `3a85f23` · **Branch:** `surething-ui` · working tree clean.

This is written for a seat with **no conversation context**. Everything below is either verifiable in
the repo or flagged as unverified.

---

## 1. Where the work stands

The SureThing laptop's first slice **merged to main** at `2e97d13` (2026-07-31). S6 (lobby shell)
and S7 (ink sprites) are **DESIGN-VERIFIED** by the Design Director — the laptop's first. Changes to
those two are regressions, not iteration.

**S8 (OS chrome) was the third, and is back in review as of 2026-08-03**: S48 folds the desktop into
that chrome, which S48's own ruling says returns S8 to review. The frame is submitted (§4-0.5). Until
the DD re-verifies, S8 is **not** a verified item to protect — but do not treat that as licence
either; it is under review, not open.

Since the merge, three more rulings landed and are **implemented, verified and committed**:

- **S18** — a wax primary action is a wax field, wax-ink type and a 2px `--wax-deep` edge.
  `LaptopUi.MakeWaxPrimary` builds all three; `PLACE TICKET` and `LEAVE — NEXT ROUND` route through
  it. Verified by pixel measurement, not eye: 2px on all four sides inside an unchanged 44px
  footprint. `BUY` is deliberately flat — a row-level purchase control, not a screen's primary.
- **S19** — the toner grain is a signed-blend shader, `SBR/TonerGrain`, using `Blend DstColor
  SrcColor` so 0.5 is a no-op and the pass has a **mean effect of zero**. Verified: median luminance
  19.0 with grain on, identical to grain off.
- **S26** — offer rule text never truncates at point of spending; the board shows however many offers
  fit and states how many it could not. The REWARDS banner states rather than exhorts.

**Suites: EditMode 76/76, PlayMode 46/46** (at `3a85f23`). The 75/38 in earlier notes was the
count before the desktop block; PlayMode's total also depends on whether the run passes
`-nographics`, which fails the four capture tests on `RenderTexture.Create` — **do not pass it**,
the command in §5 is the one that holds.

## 2. The C14 audit — the main open item

**`docs/design/C14-LEDGER-AUDIT.md`** (committed at `571675c`) is the full finding. Read it before
touching the LEDGER screen.

**26 gaps: 9 fix-now, 3 needs-window, 14 needs-DD.**

The audit's premise was wrong and the sweep corrected it — worth knowing, because the same mistake is
easy to repeat. There is no `components/ledger/` directory, so I concluded the kit did not spec this
screen. It does: `ui_kits/surething/screens.jsx:132-146`, `app.jsx:94-97`, and
`components/records/LedgerEntry.jsx`. **The screen has drifted from a specification that exists.**

### The 14 needs-DD, grouped — these are the blocking dispositions

1. **Structural shape (4 gaps, one decision).** The kit's persistent four-tab strip is a single fake
   tab; the masthead's `RunFigure`s are absent; the 44px `--ground-2` board header does not exist;
   and the record row **inverts the kit's information hierarchy** — the dollar payout is the final
   scan point and `WON`/`LOST` is buried mid-row. Whether a read-only historical screen carries live
   run figures is a real product call. The inverted hierarchy probably is not, and I expect that one
   back as "fix it".
2. **The margin (2 gaps, one decision).** Kit: biro-ruled `MarginHeader` plus exactly three
   `MarginRow`s and one note. Build: toner header, soft rule, no biro, seven content blocks, and
   mixed type voices.
3. **Ruled-paper texture** absent from the margin — a 26px repeating gradient in the kit. Not
   physically impossible; the toner-grain tile proves the technique. A cost call.
4. **Voice and behaviour** — `SETTLED TICKETS EXPOSED BY RUN.TICKETS ONLY` reads as a leaked property
   path (my lean: genuine defect); the cross-app toast bleeds onto a read-only screen; `CASHED OUT`
   is toner-2 where the kit pairs it with `WON` as wax, though the payout figure legitimately cannot
   go wax because the engine stores no cash-out amount; leg rows carry no per-outcome colour, and
   here the two kit sources contradict each other.
5. **Restatement** — scope restated 38px below the masthead, round number appearing three times.

## 3. Next actions, in order

1. **The 9 fix-now gaps.** Full detail in the audit; the sequencing constraints are:
   - **`MakeRule()` can only ever draw `--rule-soft`** (`LaptopOs.cs:616-618`); `LaptopOs.Rule` is
     dead code. Fix this **first** — the two missing-rule gaps cannot be done correctly until the
     strong token is reachable.
   - **The tabs-meta fix and the masthead's `READ ONLY` must move together.** Setting the meta to
     `READ ONLY` per `app.jsx:121` makes the masthead's existing one a *second* instance and
     regresses the redundancy ruling S9 closed. Neither sweep could see this; each held half.
   - Two fix-now items (`F5`, `F6`) are corrections to **my own S15 work** — I filled the `LOST`
     word with oxide and used the brightest toner for `$0`. `LedgerEntry.jsx` is more precise: only
     the *strike* is oxide, and word and figure are both `--toner-3`.
2. **Then the 3 needs-window gaps**, which need an editor slot.
3. **S10 (loud register) remains parked** pending a DD spec. Do not guess at it.

## 4. Caveat that gates item 3 of the audit

**Every capture in existence shows the LEDGER empty.** The populated-state findings — the missing
overflow guard, the possible `PENDING` leg inside a terminal ticket, and the column maths behind the
hierarchy gap — are read from source. They are deterministic in UGUI, but **unphotographed**.

**Capture a populated ledger before rebuilding the record row.** This is not caution for its own
sake: a `BUY`-in-biro Law Two violation survived weeks of review on this surface because no capture
ever showed an affordable offer, and every reviewer including me looked at a screenshot where the
control was greyed out. The fix for that was capture state `09-rewards-affordable`, which asserts a
BUY is interactable *before* shooting. A populated-ledger state should do the same.

## 4b. Cross-seat dependency — engine ticket retention

**Approved by Allen, 2026-08-01. Lands via the markets seat; settlement is theirs. This seat
consumes the result and builds none of it.**

The defect that produced it: `Run.ExitShop()` does `Round++; _tickets.Clear();`, and `_tickets` is
the only ticket list on `Run` — there is no archive. So `run.Tickets` holds **the current round
only**, while the LEDGER captions itself `CURRENT RUN` in four separate places. A player who bets in
rounds 1–3 and opens the LEDGER in round 4 sees an empty screen reading *"NO SETTLED TICKETS IN THE
CURRENT RUN"*, which is false.

Two consequences once retention lands, both of which this seat must then act on:

1. **The scope copy becomes true on its own.** `CURRENT RUN`, `THIS RUN` and `CURRENT-RUN RECORD` are
   correct the moment tickets persist across rounds. **Do not relabel them to `THIS ROUND` in the
   meantime** — Allen chose retention precisely so the honest wording survives, and relabelling would
   write the wrong scope into canon and the kit.
2. **The overflow arithmetic changes and gets worse.** Today the list is bounded at 3 tickets because
   the engine clears them. With retention it becomes 8 rounds × 3 tickets, so the worst case moves
   from 3 rows to 24. The current measured overflow is already 142px unclipped against a 458px board
   with no `RectMask2D`. **Re-measure before assuming any layout still holds**, and note this makes
   the scroll question below load-bearing rather than theoretical.

Also waiting on the same landing: S36's cash-out figure. The engine retains no cash-out amount, so
that money column prints an em dash in `--toner-3`. **Keep printing the honest absence** — never
`$0`, never `AMOUNT NOT RETAINED` — until the retained figure exists, then consume it.

## 4-0. Batch 7 — where the LEDGER actually stands

**Do not work from batch 7's closing list as written.** Two of its items were already satisfied when
it was ruled, and one it lists as pending has since landed. Corrected:

| Item | State |
|---|---|
| S38 + S39 | **Done**, `e1f0602` — one change, as ruled |
| S40 | **Done**, `e1f0602` |
| S37 live | **Done**, `e1f0602` |
| S43 | **Done**, `e1f0602` — not pending |
| S42 | **Done**, `f8138cc` |
| S34 | **Was never absent.** See §4a.1 — present on both margins, measured |
| S32 | **Cause recorded**, §4a — fixed between HEADs, no rebuild needed |
| **S41** | **The only one left, and blocked** — see below |

**S41 is blocked on the engine, not on this seat.** It needs `9e55d0d` (ticket retention) in this
tree; that commit is not an ancestor of HEAD and no retention field exists in `engine/Domain.cs`.
**Re-checked 2026-08-03: still not an ancestor**, though the object is now present in this worktree,
so it is fetchable rather than missing. Note the DD inbox (`main-2/docs/design/INBOX.md`) records
S36 as *resolved* on the strength of that landing in markets — **it has not reached this surface**,
and the register's S36/S41 entries read as though it has. Do not take the inbox's word for it;
`git merge-base --is-ancestor` is the check.
Until it lands the margin's RETURNED total keeps printing the em-dash absence. **Do not "fix" that
absence** — it is S36 as ruled and it is honest; S41 replaces it only when there is a real figure
to print.

Once S41 lands, re-shoot the same twelve-state set and re-submit. The grant is made on that set —
no new evidence list, no new window.

## 4-0.1 Two things the scroll still needs, and no capture can give either

S42 landed and the suites are green, but its whole reason for existing is unphotographed:

1. **The rail has never been seen.** It is correctly *absent* everywhere today, because no capture
   state populates any list past its budget — REWARDS is capped, and the ledger's three 2-leg
   tickets fit. So S27's track and thumb have never rendered in a frame. **A capture state that
   overflows a list is the next piece of evidence to get** — the ledger with three 6-leg tickets
   would do it.
2. **Nobody has scrolled it.** Wheel routing through the existing `InputSystemUIInputModule` to the
   viewport plate is standard UGUI, but no capture can prove a human can operate it. This wants
   thirty seconds of someone driving the laptop, and it should be asked for explicitly rather than
   assumed on a green suite.

Also recorded, because it will bite whoever writes the overflow capture: `AssertChildrenContained`
assumes children fit their parent, which is **structurally false for a correct `ScrollRect`** —
content is supposed to be taller than its viewport. No current test points it at a scrolled list.
Do not "fix" the ScrollRect to satisfy that helper; fix or scope the helper.

## 4-0.2 S46 landed, and four things it turned up

**S46 is done** (`4957997`). Five spellings became one; the tray slot was already right. Suites
**EditMode 76/76, PlayMode 43/43** — up from 75 and 42, one new gate on each.

Two scoping decisions to know before S44/S45 and S47, because a later reader will otherwise read
them as things I missed:

- **`Mail (soon)` and `Bank (soon)` keep their sentence case.** S46 says icon labels take the
  machine's voice — caps, condensed — and I applied condensed to the whole class but caps only to
  the name. S47 rules those two labels' text *and* treatment and deletes `(soon)` outright, so
  recasing them here would only be undone there.
- **The wallpaper wordmark and the tagline are untouched, on purpose.** They are S44 and S45. The
  new test deliberately does not match `SURE` + `THING.` — that is two Text objects, not one
  string, and a test that failed on them would be claiming a ruling it does not hold.

Two findings:

1. **Nothing on this surface renders below 13px.** `LaptopUi.MakeText` does
   `Mathf.Max(13, fontSize)`, and so does `MeasureWidth` — so measurement and render agree, and
   there is no defect here. But **every authored size below 13 in this file is fiction**:
   `NotebookChrome.ChromeText = 12` renders at 13, and the desktop icon caption's authored `11`
   renders at 13. **The fact floor's "12px only for OS chrome" clause is unreachable on this
   surface**, and any type-size finding read from source rather than from a frame will be wrong.
   I left the authored numbers alone rather than "correcting" them to values that change nothing.
2. **The verdict screen has never been ruled and breaks two standing laws.** `RenderVerdict`
   (`LaptopOs.cs`) paints its ground `rgba(.03, .02, .06, 1)` — effectively black and blue-tinted,
   which is the *exact* pair of violations already fixed on the desktop taskbar and recorded in the
   comment above it — and prints `THE BOOKIE COLLECTS` in `MoneyBad`, oxide as a generic "bad"
   tint. S46 only corrected the name on it. **This screen is not in any capture state and not in
   the register.** Worth raising with the DD rather than fixing unruled.

And one lesson, in the C18 family: **`MakeButton` names its text child `Label`, so
`MakeDesktopIcon`'s caption was a second sibling under the same name and unreachable by lookup.**
Both Texts drew and the frame looked right, so nothing ever failed — the first draft of the S46
test asked the icon what it called the app and was handed `"S"`, the glyph. The caption is
`Caption` now. A duplicate sibling name is invisible to every test that does not do a lookup.

**Also corrected:** `LaptopScreen`'s `_fontCond` comment claimed Archivo Narrow was not in the repo
and that the condensed seam resolved to the same `Font` object as `_font`. Untrue since S11 — both
faces load with no fallback warning, and the same string measures 64px condensed against 78px
roman (ratio 0.82) on frames 11 and 01. **C15's scoping was written against the old claim** and
should be re-read with this in mind.

## 4-0.3 S44 + S45 landed. S47 is next

**Done** (`916d4f4`). Suites **EditMode 76/76, PlayMode 44/44**. The wallpaper is the lifted ground
and its toner grain and nothing else: the `SURE THING.` wordmark and `the number never lies` are
deleted rather than restyled, and the app icon's `S` left the player's ink for full `--toner` —
**S47's own wording names that last one as S44's**, which is easy to miss if you read S44 alone.

Three things to carry:

- **The optional dead-manufacturer wordmark is deliberately not built.** S44 permits one in
  `--toner-3`. S48 folds this desktop into `NotebookChrome`, whose rail already carries the
  machine's own marks (`NOTEBOOK`, the `PROPERTY OF NOBODY` sticker), so a wallpaper mark would be
  a second instance of exactly that. If the DD wants one anyway it is four lines.
- **The vacated top band is not headroom.** R30 is new this morning (batch 8, promoted from S50)
  and binds all three surfaces: a locked band is not spare space. S48's 34px rail lands in that
  strip. It is named in the source so the emptiness is not later priced as free.
- **S47 is next, and three of its items are visible on frame 11 rather than hidden**: the LEDGER
  icon's `$` is drawn at `--ground-3` and is nearly invisible — the chip colour looks to have been
  passed as the glyph colour — all four icons carry a chip where only installed ones should, and
  `(soon)` is still on Mail and Bank. `Mail (soon)` and `Bank (soon)` also still read in sentence
  case: **that is deliberate**, see §4-0.2.

**Evidence and its scope (C25).** Frame 11 re-shot: zero blue-dominant pixels anywhere on the
desktop, sampling every other column of all 1024×640 above the taskbar. Not covered: the
wallpaper's own corner colours are per-vertex data inside `OnPopulateMesh`, so neither the new
test's colour-field scan nor any `Graphic.color` read touches them.

**Filed, not fixed:** the run-verdict screen (§4-0.2 finding 2) is written up at
`main-2/docs/design/dd-import/dd-followup-surething-verdict-screen.md` in C25 form, per the
orchestrator. **Left untracked on purpose** — main-2 is on `main`, other leads' notes sit there the
same way, and it rides the next drag. A third violation turned up while writing it: `NEW RUN` is a
**biro-filled field**, which is Law Two and S18 at once.

**Batch 8 (2026-08-03) checked and does not touch the desktop block.** Also from it: **C26** — this
surface's owning document does not exist and is owed after the LEDGER close-out and the S48 fold.
Not blocking, but it is on the surface's account.

## 4-0.4 S47 landed. Only S48 is left on this surface, and it re-opens S8

**Done** (`5e33b30`). Suites **EditMode 76/76, PlayMode 45/45**. The desktop icons now derive
everything from an `IconState` — glyph ink, caption ink, whether a chip draws, whether the thing
opens — instead of a hand-passed colour plus an inference from whether `onClick` was null.

**What that refactor found is the reason to keep it that way.** LEDGER's `$` was drawn in
`--ground-3`, the chip's colour sitting in the glyph's argument, so the one destination on this
machine that is not the sportsbook announced itself with a glyph the same value as its own tile.
Measured peak luminance is now 214 against SURETHING's 215; before, it was the chip. **It was on
every desktop capture ever taken.** Nothing caught it because there was no state for it to
disagree with — the same shape as the `Label`/`Caption` collision in §4-0.2, and the third
member of that family on this surface in two days.

`(soon)` is deleted, and MAIL and BANK take the machine's voice, which **closes the half of S46
deliberately deferred** in §4-0.2. Nothing is left over from that deferral.

**The pairing invariant is the one to preserve if this code is ever rewritten:** treatment and
behaviour may never disagree. An icon that looks installed and refuses to open is the surface
lying about itself in the exact direction the ruling exists to stop. The test holds it across all
four icons.

**Next, and last on this surface: S48.** It folds the desktop into `NotebookChrome` (34px rail +
34px tray, wallpaper resizing to the remainder), and **it returns S8 to review** — S8 is one of
this laptop's three Design-verified items, so this is not an ordinary build. Nothing is blocked
behind it. **S49 is not this seat's** — the desktop enters `ui_kits/surething/` and is DD-authored;
do not write to `main-2` for it.

## 4-0.5 S48 landed. The desktop block is complete, and S8 is awaiting re-verification

**Done** (`3a85f23`). Suites **EditMode 76/76, PlayMode 46/46**. The desktop's 54px taskbar is gone;
it carries the shared 34px rail and 34px tray, and the wallpaper is the remainder.

**Submitted for S8's re-review:** `main-2/docs/design/dd-import/surething-s8-refold-2026-08-03/` —
the desktop frame flat and through the room camera, the in-app lobby frame for comparison, and a
README naming what I measured and the three composition questions I could not answer. **Left
untracked, like the verdict-screen note**, to ride the drag.

**The evidence worth re-using:** comparing the desktop frame against the lobby frame from the same
run, the **rail band (y 0–33) is 100% pixel-identical** — 17408 samples, zero differing — and the
tray band past the app slots is likewise 100% identical. That is the fold's actual claim (one chrome
consumed twice, not two copies) stated as a comparison, which is the only kind of colour check this
surface has never been burned by. **If the chrome is ever touched again, re-run that comparison** —
it is the cheapest possible detector for the drift S8 exists to prevent.

Two things the fold needed, both the same shape as the last three items' findings — a value with
nothing able to tell two cases apart:

- **`Running` was a two-value enum** and the tray derived the ledger's state as `!sportsbookRunning`
  with its action from the other branch of the sportsbook's ternary. That encoded "exactly one app
  is running", which made the desktop **unrepresentable rather than merely unwritten**. Each slot
  asks about itself now; `Running.None` is the state that could not previously be said.
- **The icon and the tray slot disagreed.** The icon set `_activeApp` inline and left the tab alone;
  the slot calls `OpenSportsbook`, which restores the phase's tab. Two controls for one app landing
  in different places — invisible until the fold put them on the same screen. Both route through one
  action now and a test clicks each and compares.

**Noticed, not fixed:** `OpenLedger()` (private, line ~173) and `OpenOldSlips()` (internal) have
identical bodies — two methods for one navigation. Harmless today and the exact seed of the next
drift. A three-line deletion for whoever is next in this file.

### What this seat does next

The desktop block is finished except **S49, which is the DD's** (the desktop enters
`ui_kits/surething/`; do not write to `main-2` for it).

**The open work is the LEDGER close-out.** Design-verified is withheld pending S38, S39, S40, S34,
S37-live — **all landed** — and **S41, which is still blocked**: `9e55d0d` is not an ancestor of this
HEAD (§4-0). When it lands, build S41, then **re-shoot the same twelve-state set and re-submit**; the
grant is made on that set, no new evidence list. Also still owed and unphotographed: an
overflow-a-list capture so S27's rail has ever been seen (§4-0.1), and thirty seconds of a human
actually scrolling it.

## 4a. S32 — which happened: fixed between HEADs

S32 closed on rendered evidence and asks this handoff to record the cause, because the register
records causes. **It was fixed between HEADs. It was not misread at source.**

The C14 audit read the inversion at `11fabaa` and was **correct at that HEAD** — the payout was the
row's last scan point and `WON`/`LOST` sat mid-row. `be15621` then rebuilt the record row to
`LedgerEntry`'s column order, 181 lines of `SportsbookApp.cs`, moving the terminal word rightmost.
The frames submitted at `89aeac9` show that rebuild. So the DD withheld a rebuild that had already
been built, and the capture that unblocked the ruling was shot from the corrected build.

The sequence worth keeping, because it is the cheap lesson: the audit found it from source, the fix
landed under the ruling that the audit produced, and the capture then arrived showing no violation.
Nothing was misread and nothing was built twice — but a source-read finding and a frame taken two
commits apart described different builds, and neither was wrong.

**C17 has now paid twice on this surface, and both times the capture dissolved the finding rather
than confirming it** (T26 was the first). That is not an argument against C17 — an unphotographed
state is still unruled — but it is an argument for reading the HEAD a finding was taken at before
scheduling work against it.

## 4a.1 S34 — present, contrary to the register

Batch 7 records "**S34 is absent** on both margins". It is present on both, and measured on the very
frames the batch was ruled against:

- passive margin, `12-ledger-populated-multi`: rule lines at y 451, 477, 503, 529, 555, 581, 607
- working margin, `01-form-lobby`: 477, 503, 529 before content begins

Exact **26px pitch** in both, which is `margin.jsx`'s `repeating-linear-gradient` period.

Why it reads as absent is worth recording rather than just correcting: `--rule-soft` (44,44,32)
against `--ground` (22,22,15) is a small delta, and the pass is one pixel every twenty-six. It is
findable by sampling and genuinely hard to see at review scale. **If the intent is that it should
read at review scale, that is a strength question and a new ruling — not a build gap.** Do not "add"
it; it is there.

## 4bb. C15 — the TextMeshPro migration, scoped

**Ruled by Allen 2026-08-02: Option 1, TMP, both surfaces. SCHEDULED, not now** — the conformance
wave lands first and the orchestrator sequences per surface. **No build work until then.** The signed
type deviations (S28 tracking, S29 tabular figures, S20 weight) stay in force until this surface
migrates, and expire the moment it does.

Scoped here so the phase can be planned rather than discovered.

### What it touches

**Slots: 98 `MakeText` call sites and 23 button labels**, across `SportsbookApp.cs` and `LaptopOs.cs`.
That count is misleading in the good direction — every one of them goes through `LaptopUi.MakeText`
or `MakeButton`, so the migration is largely two helpers plus their signature, not 121 edits. The
`Font` parameter becomes a `TMP_FontAsset`, and `LaptopScreen.LoadFont` — already the single seam
resolving both voices — resolves two asset references instead.

**Font assets.** Archivo and Archivo Narrow ship as variable TTFs under
`Resources/SureThing/Fonts` with their OFL licences beside them. TMP needs generated `TMP_FontAsset`s,
which is editor work and cannot be authored blind. **This is the step that unlocks weight 600**:
TMP font assets can carry named instances, which is exactly what S20 said was required and legacy
UGUI could not give.

**Text-metric helpers are the real work, not the text itself.** Four things measure glyphs today and
all four are `Font`/`Text`-based:

- `LaptopUi.MeasureWidth` — `Font.GetCharacterInfo`
- `LaptopUi.FitText` and `FitLabelKeepingSuffix` — deliberate ellipsis truncation, which S26 makes
  load-bearing at the point of spending
- `SportsbookApp.InkRingGeometry` — **sizes and places the biro rings and strikes off measured text**

That last one matters more than it looks: the ink assets are positioned from text metrics, so a
metric change moves every ring and every strike. TMP's `GetPreferredValues` is not a drop-in for
`GetCharacterInfo`, and the ring/strike geometry must be re-verified against captures afterwards,
not assumed.

**The S2 box rule must be re-expressed.** `MakeText` currently protects against a box shorter than
one line rendering *nothing* by falling back to vertical overflow. That is a UGUI `Text` truncation
behaviour; TMP has its own overflow model (`TextOverflowModes`) and the same protection has to be
rebuilt in it. Losing it silently reintroduces the defect that deleted the masthead and the payout
figure with every test green.

### Materials and C3

The laptop canvas is world-space, inside the room's URP grade with bloom. TMP renders through its own
SDF shader rather than the default UI material, so **text will not respond to that grade identically**
— SDF edges under bloom are the specific risk, and the surface has a 31px wax payout figure that is
already the brightest type on it.

C3's one-token invariant is primarily a TV concern, but C15 names it because a TMP migration touches
the HDR material path both surfaces share. **This seat should not migrate before the TV's material
path is settled**, or the laptop becomes a second variable in someone else's measurement.

### A new risk the migration introduces

**TMP enables rich text by default.** `<color=#...>` markup is exactly the class of defect
`SureThingPaletteMarkupTests` exists to catch — retired colour hiding in a string where no
field-level palette scan can see it. That guard currently scans source, which still works, but the
migration widens the surface: TMP makes such markup trivially available at every one of those 98
slots.

**Set `richText = false` in the `MakeText` helper as part of the migration**, unless a spec
genuinely requires mixed runs. Do it in the helper, once, rather than trusting 98 call sites.

### What it buys

Tracking (`--st-track-*`, currently unreachable and signed under S28), tabular figures (S29 — though
Archivo Narrow's digits are already uniform, so this is insurance rather than a fix), and weight 600
(S20). Those three signatures all expire on landing, which is the point.

## 4c. Open decisions this seat must not assume

- ~~Scroll input~~ — **ruled S42 and built** (`f8138cc`). See §4-0.1 for what it still needs.
- ~~Legs as one string vs sub-rows~~ — **ruled S40: the sub-rows stand**, and the reserved-and-blank
  legs cell is deleted. Built in `e1f0602`.
- **The desktop block**, in this order and no other: ~~**S46**~~ (**done**, `4957997` — see §4-0.2)
  → ~~**S44 + S45**~~ (**done**, `916d4f4` — see §4-0.3) → ~~**S47**~~ (**done**, `5e33b30` — see
  §4-0.4) → ~~**S48**~~ (**done**, `3a85f23` — see §4-0.5; **the desktop block is complete**) → **S49**
  (**DD-authored — the desktop enters `ui_kits/surething/`; this seat does not write to `main-2`**)
  → **S48 last**, because folding the desktop into `NotebookChrome` re-opens S8 and returns it to
  review.
- **C15 — TextMeshPro migration** — with Allen. Until it lands, S28 and S29 hold, and tracking,
  tabular figures and weight 600 stay unreachable on this surface.

## 5. How this seat works

- **Unity is one editor, studio-wide.** Do not launch it without a slot granted by the orchestrator.
  Announce open and close; other worktrees queue.
- **Run results and logs go to `evidence/`**, never the Unity project root — it is kept clean and is
  gitignored.
- **Grunt work is dispatched** to bounded sub-agents (Sonnet by default, max two at once). Each
  dispatch names allowed files, forbidden files, required evidence and an exit gate; sub-agents never
  commit. **Tell them explicitly not to use `run_in_background` for Unity runs and not to end a turn
  with a run pending** — that pattern burned two cycles.
- **Verify against pixels, not test output.** On this surface a fully green suite has hidden a
  defect that was obvious in a screenshot at least four times. Tests here assert structure; they do
  not assert appearance.
- **This project renders in LINEAR colour space** (`ProjectSettings.asset`, `m_ActiveColorSpace: 1`).
  When checking a measured pixel against a token — a `rgba(...)` overlay, an alpha blend, any
  composite — **blend in linear and convert back**, or the number will be wrong and the build will
  look guilty. Naive sRGB arithmetic under-predicts badly on this surface's dark ground, and most on
  blue: for the marked wash it predicted 27 where the correct model and the build both give 55.

  I reported that wash as 2–3× over-strength and queued a fix for it. It was correct all along; my
  model was wrong. Note what saved every earlier measurement: they were **comparisons** — ground
  before and after grain, strike x-extent against a column's x, one rule colour against another —
  and comparisons are unaffected by the colour space because both sides carry the same error. The
  first absolute check against a hand-computed token is the one that bit.
- **A `Graphic` subclass needs `typeof(CanvasRenderer)` in its `GameObject` constructor.**
  `[RequireComponent]` is honoured by `AddComponent` and ignored by the constructor's type list.
  Without it, `OnPopulateMesh` is never called — the element draws nothing, throws nothing, and
  passes every test. All three custom graphics here shipped that way and none had ever drawn.
- **Unverified work is not committed.** Hold it in the working tree until a slot proves it.

### Commands

```
# tests — always absolute paths, results into evidence/
"C:/Program Files/Unity/Hub/Editor/6000.5.3f1/Editor/Unity.exe" -batchmode -runTests \
  -projectPath "<worktree>/unity/SBR" -testPlatform EditMode \
  -testResults "<worktree>/evidence/test-results/x.xml" -logFile "<worktree>/evidence/logs/x.log"
```

Captures land in `artifacts/surething-ui/` (gitignored). Nine states across two `[UnityTest]`s in
`SureThingVisualCaptureTests`.

## 6. Standing laws

Full set in `docs/design/direction-concepts/DESIGN.md`; the ones that bite most often:

- **C14 (hardened 2026-08-01):** 1:1 with the design system is the bar, not the aspiration.
  Deviations only where physically impossible, **each DD-signed before build**.
- **C10:** never tune a wrong-in-kind effect toward invisibility. Diagnose the kind first.
- **S2 (amended):** a text box is at least one line tall or it overflows — never empty. Unity
  truncation clips whole *lines*, so a short box renders nothing at all, silently.
- **S20:** no weight tiers without TMP named instances. Both production faces are variable fonts and
  legacy UGUI renders only the default instance.
- **Oxide is the house's mark only** — blocked actions and the strike on a dead leg or lost ticket.
  Never a price, a cost, or a generic "bad" tint.
- **Wax is money and the primary action; biro is only what the player chose.**
- **Fact floor:** product facts ≥13px; 12px only for OS chrome carrying no product meaning.

### One open C14 deviation awaiting signature

The grain. The kit specifies feTurbulence at 5% opacity; UGUI cannot reproduce that — under normal
alpha blending a white overlay can **only add light**, which bleached the ground to `(52,52,48)`.
Shipped as a signed `DstColor SrcColor` blend at the same 5% token: same intent, mean-preserving,
**different mechanism**. Physically forced, but it needs DD sign-off rather than my say-so. It is the
only deviation I am aware of on this surface.

## 7. Also worth a look, not yet raised

~~The REWARDS screen's masthead reads `SURETHING FORM`.~~ **Ruled and fixed** — S46 deleted FORM from
the brand for the general reason (FORM is a screen, not part of the name), which resolves the
REWARDS instance as a side effect. The shared masthead now reads `SURETHING` on every screen that
carries it.

Still open on this list: the verdict screen, §4-0.2 finding 2.
