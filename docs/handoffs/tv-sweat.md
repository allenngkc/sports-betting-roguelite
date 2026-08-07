# TV Sweat — lead ownership contract

**Worktree:** `tv-sweat` · **Branch:** `slice/tv-sweat-refinement` · **Lead:** Claude (Opus 5)
**Contract authority:** `main-2/docs/5-orchestration/STUDIO.md` · **Board:** `main-2/docs/5-orchestration/STATUS.md`
**Written:** 2026-07-31 · **HEAD at writing:** `220c5ec`

Supersedes `handoff.draft.md`, which was a briefing rather than a contract and carried none of the
four sections STUDIO.md requires. Its briefing content is folded into §5 below; the draft may be
discarded.

---

## 0. WINDOW RESULTS — 2026-08-03, editor released

**Correction to the brief this answers: no capture sets landed. Not the lighting-comparison pair,
not the bloom A/B. There are no paths or sizes to report, because there are no files.** Code and
suites are green; the evidence half of the window produced nothing. Both reasons are below and
neither is a retry-and-it-works.

### Merge — done, `e2143e6`

`main` → `slice/tv-sweat-refinement`. Was 51 ahead / 190 behind. Three conflicts:

| file | kind | resolution |
|---|---|---|
| `SBR/Resources.meta` | folder-GUID collision | took main's; **verified** nothing references the dropped GUID |
| `Runtime/Shaders.meta` | folder-GUID collision | same |
| `PRODUCT.md` | **add/add** (no merge base) | took main's |

`PRODUCT.md` is mine per §1 and I dropped my version deliberately: it was a 2026-07-24 TV-only draft
carrying facts now false (`Engine 144/144, Unity 40/40`; "Decision A, open as of 2026-07-24" — C1
closed it 07-31), and its own header says the source documents win. Main's is the studio-wide record
covering three surfaces. **Recoverable at tag `pre-main-merge-2026-08-03`** if that call was wrong.

### Compile — CLEAN

Zero `error CS`. Not inferred from exit code 0: `SBR.Game.dll` and `SBR.Tests.EditMode.dll` rebuilt
**22:08** against sources last edited **21:54** — newer, checked. T43/T46/T42 compile as written.

### Suites

| suite | result | vs baseline |
|---|---|---|
| engine | **160 / 160** | matches |
| EditMode | **222 / 222**, 0 failed, 0 skipped | see note |
| PlayMode | 70 total, **62 passed, 3 failed, 5 skipped** | 3 failures are not TV's |

**The 133 target was wrong and so was my 134.** Both were pre-merge arithmetic off a 129 baseline.
The merge brought main's SureThing EditMode suites in, so the real number is **222**. Nothing is
missing — all five new tests passed by name: `T46_right_hand_zone_content_is_owned_and_clipped_by_its_own_zone`,
`T42_the_only_team_hues_are_canons_two_muted_ones`, `T43_suspending_dims_the_whole_slot_on_the_same_frame_as_the_label`,
`T43_the_gold_taunt_cannot_repaint_a_suspended_slot`, `T43_a_tweening_price_never_lights_the_field_or_takes_the_L4_token`.

**No TV regression.** Every `TvSweatScreenTests` case passed, including
`Interact_DuringSuspendedMarket_StandsAndDoesNotCashOut` — the one T43's rewrite most threatened —
and the flake-prone `Standing_Freezes_CashOutTween_NoResumeCatchUp`. The regression string
`kept ticking while standing` occurs **0** times in the results XML. The documented mid-tween flake
did not fire this run.

**The 3 PlayMode failures are SureThing's `SureThingVisualCaptureTests`, and they are mine to
explain:** I ran `-nographics`, and they die in `UnityEngine.Camera:Render` /
`Rendering.Blitter.Initialize`. Environmental, not code — and it is the rig recipe's own warning
("Never `-nographics`. Post-processing needs a graphics device"). **SureThing's captures need a
graphics-enabled run.** Their untracked outputs are at `artifacts/surething-ui/` and
`unity/SBR/artifacts/`.

### T48 — STILL BLOCKED, and the merge did not fix it

Attempted, exit 1, `-outDir` empty. Log, verbatim:

> `executeMethod method 'CaptureConformance' in class 'SBR.RoomViewCapture' could not be found.`

**The rig recipe documents a harness that is not on `main`.** `CaptureConformance` and
`DarkenScreens` are on `room-refinement` (4 matches) and absent from `main` (0). The
`RoomViewCapture.cs` the merge delivered exposes only `CaptureEditMode()` and `CaptureAll()`.

So the merge did unblock the *other* two preconditions — `tools/room_gate_check.py` and `RoomPostFx`
in `Room.unity` are both present now — but not this one. **T48 needs the room lead's
`RoomViewCapture.cs` on main.** That is a room-side merge, not an editor window, and not mine to
take: `room-refinement` is another worktree's in-flight branch. The recipe's own §0 says the code
wins where the two disagree; this is that case, and the room lead asked to be told.

### T49 — not attempted, deliberately

`HdrBoostL4` is `private const float = 1.8f`. It is not runtime-settable, so the A/B is: edit the
const, warm compile, shoot a ship-paced `[Explicit]` set, edit back, compile, shoot again. Two source
edits and two recompiles around two capture sets. That does not fit a window with SureThing queued
behind it, and starting a set I could not finish is the precise failure §4 rule 4 was chartered
against — a half-set is not evidence (C11/C17), it is a wasted arm.

### Editor released

0 Unity processes. The stale `UnityLockfile` — the documented segfault-on-`-quit` fault, safe to
clear at 0 processes — was cleared. `EditorBuildSettings.asset` reverted;
`SBR.Engine.dll` restored to HEAD's bytes and **`cmp`-verified identical**, so its lingering
`git status` line is the inert-`[attr]lfs` artifact (§4D), not a real change.

## 0-T63i. T63 ISOLATION — the invert-before-label defect is NOT real (no editor used)

**Answer: the cash-out FIELD never coexists with a `MARKET SUSPENDED` label.** Isolated code-side
first, as instructed; frames were needed only to confirm what the gold actually was. Bundle restaged
with the correction.

**Three independent checks, any one sufficient:**

1. **Code.** T43's fix sets the flag, the label and the field in **one call** — `live = slotVisible
   && !_cashOutSlotSuspended`, so `fieldLit` is false whenever the label reads SUSPENDED. Pinned by
   `T43_suspending_dims_the_whole_slot_on_the_same_frame_as_the_label`, green in 228/228.
2. **Geometry.** The suspect gold spans canvas y **428–555**. `_cashOutField` is sized exactly to
   `grid.CashOut` (**480–532**) and cannot paint outside its own rect.
3. **Luminance.** Gold pixels inside the zone, by floor:

| frame | floor 0.10 | 0.25 | **0.40** | |
|---|---|---|---|---|
| 000 (real invert) | 64,131 | 60,950 | **60,950** | the field — mean 0.569, peak 0.663 |
| 006 (suspect) | 24,112 | 5,093 | **0** | vanishes entirely |

The real field peaks 0.66 and clears 0.40 trivially. Frame 006 has **zero** gold above it. What it
has is the won-leg row's L3 gold, the goldL2 RISK/PAYS footer, and the column's warm-tinted dark
ground — all sub-0.40, none of it the field.

**Instrument error, disclosed (C25).** My first reading used a saturation test with a luminance floor
of **0.10**. A near-black warm pixel like `(0.12, 0.09, 0.05)` scores saturation 0.58 at hue 34° and
registers as "gold". **This is the second time in this slice a low-luminance hue test produced a false
positive** — and the instructive part is that in T42's case raising the floor did *not* explain the
residual (§0-VP2 records that theory being tested and failing), while here it explains it completely.
Same trap, opposite outcome. It has to be tested each time and assumed in neither direction.

**What this does not touch:** the §0-W finding stands unchanged — fully inverted, the band peaks
0.660–0.663 against a same-frame scoreline of 0.875–0.877, so the designated L4 element is still
~0.21 off being the brightest thing on its surface.

---

## 0-W. WINDOW — T61+T62 verified, EditMode 228/228, T63 MEASURED, editor released

**EditMode 228 / 228, zero failed** (C29 guard clean: 228 executed, 228 discovered). T61's two and
T62's two all pass. Staged `t63-cashout-band-invert.zip` (8.2 MB).

### T63 — the finding, and it is not closed by T41 or T58

Cash-out band, its own region, across the invert burst:

| state | band mean | band peak | hue | sat | **scoreline peak, same frame** |
|---|---|---|---|---|---|
| **fully inverted** (frames 000/001) | 0.569 | **0.660–0.663** | 47–58° | 65–75% | **0.875–0.877** |
| partial invert (006) | 0.143 | 0.271 | 40.8° | 73.5% | 0.890 |
| suspended slate (002–005/007, sat-down) | 0.104 | 0.166–0.169 | 205–216° | 11–15% | 0.875–0.890 |

**The designated L4 element is still not the brightest thing on its own surface — by ~0.21.** T41
capped the stage; T58 took the hue off the flash; **this gap is neither of those and survives both.**

**Scope (C25) — read the ORDERING, not the absolute scale.** My quiet-scoreline peak reads **0.875**
where the DD's earlier figure for the same thing was **0.737**. There is a systematic offset between
the two instruments (different box derivations) which I can name but not resolve from here, so
absolute values must **not** be cross-compared between the two measurements. The within-frame
comparison is unaffected — both numbers come from one frame, one method, one run.

**The box derivation ships with the numbers**, because the previous attempt's boxes framed the wall:
canvas→frame scale solved on both axes (2.2204 / 2.2236 — agreeing to three decimals is the check
that the panel was framed, not the room), CashOut zone taken from `LayoutGrid` rather than eyeballed,
and **validated by rendering the box and looking at it** (`t63-box-validation.png` is in the bundle).

**What the burst actually shows:** the market suspends and reopens *inside* the 1.05 s burst — full
invert at 000/001, slate through 002–005/007, partial invert at 006. The band is not in one state
"across the invert"; **the trajectory is the measurement.**

### A latent flake found and removed — `TvLightTests`

The first run came back 227/228. `Flash_and_SetRest_still_drive_the_wired_light` failed with
intensity 0.844 against `> 1.0`. **Not a regression** — nothing in this session touches TV lighting,
and it fails in isolation too. `Update()` decays the flash *before* reading it
(`_flash01 = MoveTowards(_flash01, 0, flashDecay * Time.deltaTime)`), so with `flashDecay = 2.6` the
assertion needs **`Time.deltaTime < 0.308 s`** — an input no EditMode batch run controls. The measured
0.844 back-solves to `_flash01 = 0.137`, `dt = 0.332 s`.

It passed for months on fast frames and failed the first time four unrelated tests pushed the frame
past ~308 ms. Fixed by pinning `flashDecay = 0` for the assertion: **the assertion is unchanged, only
the uncontrolled input is removed.** Flash still has to reach the wired Light *through* `Update()` to
pass. Decay-rate coverage is not lost, because this test never asserted it — it calls `Update()` once
and checks a lower bound.

---

## 0-B12. BATCH 12 — T58 CLOSED · T62 fixed (UNCOMPILED) · T63 owed an editor slot

**T58: GRANTED, Design-verified, CLOSED** — measured by the DD personally, perfectly neutral flash,
gold back to money. The owning doc (C26) is the next DD session.

### T62 — FIXED. One ledger, two mirrors, one repaint

The DD found it **on this slice's own T58 proof frames**: the live leg's progress line printed the
pre-goal score for a whole beat while the scoreline above printed the goal — same revealed value,
same frame, correcting 51 match-minutes later.

**Mechanism.** `OnGoalPlayed` advances the revealed score at `_ledger.CompleteGoal(goal)`, then
repainted the **scorebug only**. The live leg row reads *the same* `_ledger.Picked/Opponent` (via
`DescribeActiveLeg`) but was repainted only by `UpdateTicketColumn`, which next runs at the following
beat's `RenderEvent`. Not two sources — **one source, two repaint schedules.**

**Fixed at the ledger-advance site**, not by adding another call to another path:
`RepaintRevealedScore(leg)` does scorebug + column together, and that is what `OnGoalPlayed` calls.
Same rule as T43's slate and T59's gate — one value, one repaint, no window where mirrors disagree.

**I had this frame and did not call it.** §0-VP2 records the column reading `LEADING 1–0` against a
scoreline of `0 — 0` and I treated it as two matchups rather than one contradiction. The verdict pass
looked at *type* and *hue* and never asked whether the two score readings agreed. Worth carrying: a
pass scoped to one property will not see a defect in another, however plainly it is in frame.

**Full audit of every ledger-advance site**, since fix-by-rule means checking the siblings:

| site | status |
|---|---|
| `_ledger.CompleteGoal` (goal) | **the defect — fixed** |
| `ResetForLeg`/`ConfigureEndpoint` (kickoff) | **reasoned benign** — the column's live row is still the previous, resolved leg, so there is nothing to contradict. Changing it would alter *when* a row reads LIVE, which is a design call and not taken unruled |
| `PlanFinal` (T17's reserve release) | **covered by construction** — it mutates no score, it emits a plan; those staged goals advance the ledger through `CompleteGoal` and so through the fix |

Two EditMode guards: `T62_advancing_the_ledger_repaints_every_mirror_of_it` (scans every
`CompleteGoal` site) and `T62_the_repaint_helper_drives_both_mirrors` (the helper cannot quietly lose
its column call). **Both are SOURCE scans and that is weaker than it looks** — they pin the call, not
the pixels. The rendered pin needs a PlayMode goal and is owed with T63.

### T63 — owed, needs the editor slot (after SureThing and room)

Measure the **cash-out band's own region** across the invert burst. The DD's boxes hit the wall behind
the panel — the same framing failure this slice hit twice in §0-VP and once more in §0-VP2, now from
the other seat. **Deliver the box derivation with the numbers, not just the numbers**, and validate
the framing by rendering it before trusting it; that is the only method that has actually worked here.

### Status

**UNCOMPILED — no editor grant.** Pending compile: T61's contract fold-in *and* T62. Expect EditMode
**228** (224 + T61's 2 + T62's 2).

---

## 0-T61. T61 FOLDED IN — contract taken from markets' test, not from a green re-run (UNCOMPILED)

Markets diagnosed T61 and the answer is sharper than the hypothesis I filed. **My harness is fixed
against their contract, and a contract test now sits in my own suite** — deliberately instead of
re-running and taking green as proof.

### Why a green re-run would have proved nothing

Markets tested my round-advance hypothesis and found it **half right — the wrong half being the
useful one**:

| ticket outcome | what ticket 0 does mid-sweat | what a ticket-keyed poller sees |
|---|---|---|
| **dies** (dead-leg Lost) | settles immediately | early **false "done"** |
| **survives** | stays `Open` until `FinishSweat` | **no signal at all** |

So whether ticket 0 is terminal mid-sweat depends on the **OUTCOME, not the position** — which is
exactly why this harness failed four seeds and passed one **from identical code**. A seed that happens
to lose would have made a re-run look fixed while the defect sat untouched. **The defect is
seed-decided, so only a contract can settle it.** That is the whole reason this section exists.

Confirmed separately: `Run.Tickets` is the round's working set and is **cleared at ExitShop**, so a
held `Tickets[0]` becomes permanently terminal while `Tickets[0]` names a different, open ticket.

### The contract, and where it now lives

> **Completion is a property of the RUN's phase, never of any one ticket or session. Phase leaves
> `Sweat` exactly once, after every session is drained.**

- `TvSweatCaptureHarness.SweatEnded(director)` → `director.Run.Phase != Phase.Sweat`. Replaces
  `CurrentSession.IsComplete`, which stopped **too early** (sessions drain while the phase is still
  `Sweat`).
- Two EditMode tests pin it TV-side: `T61_sweat_completion_is_a_phase_property_not_a_ticket_property`
  and `T61_a_captured_ticket_reference_goes_stale_across_a_round`. Engine-side proof is markets'
  `SweatPollingContractTests`.
- The drain helper **asserts it actually drained** — an unfinished drain would make both tests pass
  while proving nothing about the state they claim to reach. Same failure mode C29 names, one level in.

### The grace window matters MORE now, not less

A surviving ticket settles at `FinishSweat` — i.e. at the very edge of the phase change. So the
legitimate resolution and the stop signal now arrive **within a frame of each other**. Without the
existing 2 s settle window the harness would miss exactly the moment it exists to photograph, on
precisely the seeds that win. Kept and re-commented, not removed.

### Status

**UNCOMPILED — no editor grant held.** Needs a warm compile and EditMode (expect **226**: 224 + 2).
The capture harness itself needs no re-run to prove this: the contract is pinned by tests, which is
the point.

---

## 0-B10. BATCH 10 — C29 guard retrofitted, and T58's evidence needs NO window

### T58 is already demonstrated on frames in hand

**The next editor window is not required for T58.** Staged
`main-2/docs/design/dd-import/t58-goldflash-fix.zip` (10.9 MB — quiet reference, the flash frame, two
more goal frames, and `MEASUREMENTS.txt`).

Same statistic the DD used — scoreline peak pixel, canvas x285–960 y8–56:

| | hue | saturation | luminance |
|---|---|---|---|
| DD pre-fix, **gold flash** | **56–58°** | **48.7–66.8%** | 0.72 |
| DD pre-fix, quiet | 205° | 5.2% | 0.737 |
| **post-fix, all 8 goal frames** | **198–207°** | **4.4–5.2%** | 0.875–0.877 |

The goal moment is now **indistinguishable from the quiet reference**. The gold is gone.

`frame000`, first of the goal burst, reads **`#e8e8e8` at 0.0% saturation, luminance 0.910** against a
0.875 rest: perfectly neutral and the brightest frame in the set. That is the punch carried entirely
by brightness with no hue — T58's instruction, executed.

**Scope (C25):** a PNG cannot confirm the `_tScoreFlash` overlay was enabled on that frame. It is
identified as the flash by position in the burst, by being brightest, and by being exactly neutral.
**One burst = one sample of the flash instant.** If the DD wants n>1 the window is still worth taking;
if the measurement suffices, it is not.

### C29 — zero-case guard retrofitted

`tools/assert_test_run.py`. **Run it on every results XML before any verdict, gate or Design-verified
claim rests on the run.**

```
python tools/assert_test_run.py <results.xml> [--min N] [--expect-passed N]
```

Verified against real artifacts, all four branches:

| case | source | result |
|---|---|---|
| the run that caused C29 | `hz.xml`, 0 cases | **exit 1** |
| a good run | `em3.xml`, 224/224 | exit 0 |
| missing report | absent path | **exit 2** |
| expectation unmet | `--expect-passed 999` | **exit 1** |

`total` (executed) is what decides, not `testcasecount` (discovered) — they differ exactly when a
filter matches names that never run, which is the case C29 exists for. Both are printed so the gap is
never invisible. A **missing** report fails too: a run whose report never appeared demonstrated
nothing, and treating that as neutral is the same mistake one level up.

The failure message names the cause that actually bit here — a parameterised `-testFilter` needs its
quotes, or use the regex form `".*<seed>.*"`.

### Also from batch 10

- **T60 struck** — no body, so never a ruling (C22). Nothing was built against it.
- **T61** (my scorer finding, ruled): diagnosis routes to markets, **round-advance hypothesis first** —
  the harness-scope explanation I named first and asked to have excluded. The design answer is
  pre-committed for every outcome. **Blocks nothing here.**

---

## 0-VP2. VERDICT PASSES — all four PASS, on the post-T58/T59 frames (no editor used)

Run against the 17-frame set from the post-batch-9 build (seed `48151623`, `boost1.4`), which is the
current shipped state. **All four pass.** Two new items surfaced and are below as candidate build
items — neither is a failure of the four.

| pass | verdict | basis |
|---|---|---|
| T46 right-zone clipping | **PASS** | boundary read at magnification + EditMode structural test |
| T42 team hues | **PASS** | measured, 8 goal frames |
| T44 event-strip copy | **PASS** | strip read at magnification |
| T50 column type in situ | **PASS** | column read at magnification |

**T46.** The pitch's left edge sits at canvas ~268 and every stage element stays right of it — penalty
area, dots, goal, momentum tape. Leg text is unobscured. **NetRipple, which used to reach ~155px past
the stage edge, is contained.** The gold cash-out field stops at the column edge (a few px of bloom
halo, not geometry). *Limit:* no worst-case long-fixture frame exists in this set — the seeds did not
produce one. That case is covered by the EditMode canary, not by these frames.

**T42.** Pitch interior, 8 goal frames: **blue mean saturation 0.483** against canon `--tv-team-a`
0.452, **pink 0.353** against `--tv-team-b` **0.354 — exact**. 97.8% of saturated pitch pixels fall in
the two canon bands, and **no dot is rendered in a retired hue.**

**T44.** `Zambonis settle in; the drift runs the other way.` — the T39-corrected line. No second
person, no hype, no superlative, no promise; one line, cold white per TV-05's neutral strip. Other
frames in the set show `THE BOARD IS SET` (this slice's casing fix) and
`Meatballs rip through on the break. — LEAD CHANGE`.

**T50.** Encode Sans and Encode Sans Condensed render in situ with the hierarchy intact — header,
then identity, then price/state. `−228` uses U+2212 MINUS and `+380` its sign, per S30. RISK/PAYS
gold at the footer. No team hue anywhere in the column. T11 stands on rendered evidence.

### The instrument was wrong THREE times — and the pattern is the point

T42's residual 2.17% took three attributions, two of which were wrong:

1. **"Edge fringe of my crop box."** Partly true for the loose box, false for the tight one.
2. **"Dark pixels where hue is meaningless."** Testable and **tested: false.** Raising the luminance
   floor from 0.06 → 0.25 moved the residual 2.18% → 2.15%. The theory predicted it would collapse.
3. **Marking the pixels and looking at them.** Correct: violet is antialiasing on pink dot rims
   (violet 250–290° lies *between* blue 215° and pink 319°, so a blend lands there by construction),
   and orange is a thin **pitch marking line** at the right penalty area.

Every time, the thing that settled it was rendering the question as an image. **Three numeric
theories, one look.** C11 says rendered evidence or no claim; this is that law aimed at the
instrument rather than the design, and it is now the third distinct framing/threshold error this
slice has caught in its own measurements (see §0-VP for the first two).

### Two candidate build items, surfaced by the passes — DD calls, not taken

**(a) Two dashes for one fact, both visible in a single frame.** The ticket column prints
`LEADING 1–0` using **U+2013 EN DASH** (`SweatActiveLegModel.cs:160`, `const char Dash = '–'`), while
the scorebug prints `0 — 0` using **U+2014 EM DASH** (`TvSweatScreen.cs:1448` and `:1817`). Same fact,
same surface, same frame. Defensible on typographic role — an en dash is the range dash — but S30
ruled signed numbers with "no per-region exception", and TV-32 called the em dash "the system's own
dash". **Not fixed unilaterally: which dash a score takes is a typography ruling.**

**(b) A pending leg's market label wraps to four lines.** `TROY MUFFIN ANYTIME — Tuscaloosa
Spreadsheets v Tulsa Muskrats` sets as four wrapped lines. It fits inside its fixed 69.3px slot, so
§6 is not breached and nothing reflows — but T20 rules that "live rows are display, resolved rows are
index", and a four-line wrapped label reads as display. **Flagged for the DD's eye, not called a
violation.**

---

## 0-VP. VERDICT-PASS PREP — T42 measured on frames; T46/T44 read on frames (no editor used)

Done while markets held the lease. **T42, T46 and T44 are all already implemented and green; what
they lack is Design-verified, which is the DD's act against rendered frames (C11).** So this is the
evidence, measured off the 101 frames already on disk — and the measurement itself needed correcting
twice, which is the most useful thing in this section.

### T42 — the dots measure as canon

Pitch interior, saturated pixels (sat > 0.30), five goal frames, one per seed:

| band | count (5 seeds) | mean saturation | canon |
|---|---|---|---|
| blue 190–240° | 93,648 | 0.479–0.501 | `--tv-team-a` **0.452** @ 215.5° |
| pink 300–340° | 63,497 | **0.348–0.354** | `--tv-team-b` **0.354** @ 319.0° |
| retired orange 15–40° | 2,975 | — | not in the TV palette at all |
| retired violet 250–290° | 824 | — | not in the TV palette at all |

**Pink lands on canon almost exactly** (0.348–0.354 against 0.354). Blue reads ~0.03–0.05 high, which
is what a bloom-and-grade lift does to the more luminous of the two. Blue and pink together are ~97%
of the pitch's saturated pixels, and **no dot is rendered in a retired hue** — checked by marking the
orange pixels red and looking at them.

**The residual is not dots.** Two-thirds of the orange disappeared when the box was inset 3%,
identifying it as fringe along the box's own edge; what remains is sub-pixel antialiasing between the
dots, the white ring and the near-black pitch, at counts orders below anything dot-shaped.

### The instrument was wrong twice before it was right — the useful part

1. **First box framed the room, not the screen.** Derived from an inter-moment diff, on the assumption
   that only the TV changes between moments. **The TV's own light spill changes the whole room**, so
   the box swallowed the walls and reported a median hue of **128° — green** — on a surface whose
   palette contains no green. Caught because green was impossible, not because the number looked odd.
2. **Second box overran the pitch into the panel surround**, inflating the retired-orange count
   threefold with edge fringe.

Both are the exact failure `rig-r23-recipe.md` §6 names: *"a box that no longer frames the intended
surface would still report a plausible-looking number."* The fix that actually worked was **looking at
the rendered frame** rather than reasoning about coordinates — C11's own point, turned on the
instrument instead of the design. **Any region box quoted from this slice should state how it was
framed and how that was checked.**

### T46 and T44 — read on the frame, not measured

On `seed-16180339 … moment-goal`: the stage's left edge is a **hard vertical line at the ticket
column boundary**, leg text (`BRICKLAYERS TO WIN`, the two leg rows, `RISK $87  PAYS $705`) is
unobscured, and the NetRipple — the element that used to reach ~155px past the stage — is fully
contained. **T46 reads as fixed.** The event strip prints
`Meatballs rip through on the break. — LEAD CHANGE`: observational voice, em dash, no hype. **T44
reads as fixed.** Both stated as *read*, not measured — no pixel criterion was applied.

### And the frames confirm T58 the DD found

The same frame shows the scoreline `MEATBALLS 1—0 BRICKLAYERS` in **gold**. These frames predate the
T58 fix, so that is exactly the defect batch 9 ruled — visible, and consistent with the fix now
written but not yet re-shot. **The next capture is what closes it.**

---

## 0-HD. HARNESS DEBT — both items written, UNCOMPILED (2026-08-04, markets held the editor)

Both of the DD's C25 disclosures are addressed. **No editor was available, so none of this is
compiled.** It needs a warm compile plus one filtered `TvSweatCaptureHarness` run to prove out.

### 1. Frame-locked A/B arms

The T49 pair could not answer the question it was shot for: its arms did not share sim state, so the
whole-frame diff measured **actors that had moved**, not bloom, and the DD fell back to fixed-box
region statistics. Three things had to be pinned, **all presentation-local** — the engine was always
deterministic from the run seed, which is exactly why the arms' *events* matched while their *pixels*
did not:

| source | was | now |
|---|---|---|
| `TheaterStage` presentation RNG | `Environment.TickCount * 31 + salt` | `PresentationSeedOverride` when set |
| idle emission flicker phase | `UnityEngine.Random.value` | derived from the same override |
| `Time.deltaTime` | real frame time | `Time.captureDeltaTime = 1/50` |

**The third is the one that is easy to miss.** Pinning the RNG makes both arms take the same
*decisions*; it does not make them integrate the same *motion*, because a real frame time varies run
to run. Without the fixed step the actors still drift apart and the per-pixel diff stays invalid.

Two traps closed while writing it, both of which would have produced a frame-lock that silently did
nothing while looking correct:

- **`StableSeed` is FNV-1a, not `string.GetHashCode`.** .NET randomises string hashing per process,
  so two arms shot in separate editor runs would have seeded differently.
- **The override path does not consume `s_seedSalt`.** The salt is a static counter, so mixing it in
  would make the seed depend on how many stages the session had already built — i.e. on seed *order*.
  Two arms that reordered or skipped a seed would quietly stop being locked.

`[TearDown]` releases both. `Time.captureDeltaTime` is global and session-lived; leaving it set would
put every later PlayMode test on a synthetic clock, which is the kind of cross-suite contamination
that is very hard to attribute once it bites.

**Named cost, because it works against item 2:** wall-clock per simulated second now depends on
render speed, and these are 2560×1440 frames. Ship pacing is preserved in the sense that matters —
same simulated seconds, same per-frame step — but the 420 s wall budget may cover fewer of them.

### 2. The 420 s shared budget

**Proven, not assumed:** in the T49 run the four failing seeds captured **zero** dangerous beats — all
24 `scorer-leg-dangerous-*` frames belonged to the single passing seed. So in every failing seed that
loop ran from entry to the wall doing nothing, and the named moment after it began with its deadline
already gone.

The loop is **opportunistic** (gather what the sweat offers); the scorer wait is a **named moment** the
set is expected to contain. An opportunistic collector must never starve a named one. So the budget is
now *partitioned*, not enlarged — `ScorerWaitFloorSeconds = 150f` is reserved and the total stays
420 s, because raising it past the NUnit `[Timeout]` would replace the harness's own diagnostic
message with an opaque framework kill.

The loop also now logs **why** it exited — `cap reached` / `leg resolved` / `budget` — with the time
left for the wait. C18: a scorer-leg failure after this line is now about the sweat, not about this
loop having eaten the clock.

**What this does not claim:** it does not prove the four seeds will now resolve. Their legs were
genuinely still LIVE. It makes the failure *trustworthy* — if it still fires with a reserved floor,
that is a real finding about the sweat and worth escalating rather than a budget artefact.

---

## 0-B9V. BATCH 9 VERIFIED — EditMode 224/224, input contract re-pinned, editor released

**T58 / T59 / T49-lock compile clean and pass.** EditMode **224 / 224**, zero failed, zero skipped
(222 → 224 is the two new tests). Assemblies verified fresh, 23:01 against sources at 22:55.

**T49's ruling broke a TEST INSTRUMENT, not the code — and it is worth keeping the reason.** The
first EditMode run came back 222/224 with both C3 one-token tests failing `Expected: 1, But was: 0`.
`MaterialsAtL4` counted materials with `_HdrBoost > 1.5f` — a threshold hand-calibrated to the old
1.8 — so a correct L4 element at the newly-ruled **1.4** was invisible to it and the helper reported
*zero elements lit* on a surface that was behaving perfectly.

That is **T30's lesson landing a second time**: an approximated threshold is always wrong at some
boundary, and a ruling eventually walks the value past it. The fix is not a looser number — that
would just relocate the boundary. `ConstBoost()` now reads `HdrBoostL4` off the production constant
by reflection and matches it verbatim (epsilon is float representation only), so the instrument
cannot go stale whatever the DD rules next. Swept the suites for other boost thresholds: none.

**T59 was verified where EditMode cannot see it.** An input-contract change is invisible to a test
that never presses a key, so a filtered PlayMode run on `TvSweatScreenTests` followed: **10/10**,
with all three `Interact_*` contract tests green — including
`Interact_DuringLegalOpenOffer_CashesOutAndDoesNotStand`, the one the new refusal term most
threatened. `kept ticking while standing` occurs **0** times, so TVS-H02 holds.

Editor released: 0 processes, lockfile clear, side-effects reverted.

---

## 0-B9. BATCH 9 — T58 + T59 + T49's ruling WRITTEN, unverified (2026-08-04)

**Ladder is LAW-CLEAN and Phase 3 is open.** T41 Design-verified CLOSED (zero saturated pixels),
T48 CLOSED, **T49 RULED 1.4 and SEALED**. Written this session, **no editor grant held — a work
assignment is not a lease (§4 step 0a), so none of this is compiled.**

| item | change | file |
|---|---|---|
| **T58** | goal-flash overlay gold → **cold white** (`flavorColor`, same as `_tMatchup`) | `BuildScoreBug` |
| **T59** | `CanAcceptCashOutNow` now reads `_cashOutSlotSuspended` — one value drives slot AND key | `CanAcceptCashOutNow` |
| **T49** | `HdrBoostL4` 1.8 → **1.4**, sealed | `TvSweatScreen.cs:577` |

Two new EditMode tests: `T58_the_goal_flash_carries_no_hue_of_its_own`,
`T59_a_suspended_slot_refuses_the_key`.

**T58 — the fix is structural, not a colour swap.** The punch overlay now carries the *same* colour
as the scoreline it superimposes, so boosting it can only brighten what is already there and
releasing settles back. There is no hue to change **by construction** — a future edit cannot
reintroduce one without making the two elements visibly disagree at rest, which the test pins.

**Where the gold was NOT:** `_ballFlash` is already cold white and fires only when the goal does
*not* commit, so it is mutually exclusive with the score punch; the stage's own ball is `Color.white`
(cyan only on VOID) and T41's closure holds. **`_tScoreFlash` was the only gold at the goal moment.**
The ruling's "and the ball dot goes with it" is therefore either the gold scoreline's bloom halo or
the ball read in the same region — worth confirming on the next frames, but there is no second gold
source in the code to fix.

**T59 — this is the question T43 routed up, answered.** The presentation flag now gates the accept.
`suspended`/`pending` refuse E, `actionable` accepts, `updating` refuses via the existing
`_cashOutAnimation` term. TVS-H01 survives by construction because `CashOutLive` and `TryCashOut`
both read this one predicate — pinned by the new test. **The "refused press draws nothing" clause
already held**: `TryCashOut` opens with `if (!CanAcceptCashOutNow()) return;` and has no refusal
branch, so nothing flashes and nothing explains. Nothing added.

**T49 — sealed means sealed.** The constant carries the ruling and the reason inline, including the
finding worth more than the pick: a ±0.4 bloom change moves nothing on this surface except one
element that was the wrong colour. **Bloom is not the lever for any future finding.**

### Harness debt, both from the DD's own C25 disclosure — NOT started

1. **Frame-locked A/B arms.** The two arms do not share sim state: actor positions differ at the same
   seed/scene/grammar/frame index, so the whole-frame per-pixel diff (2.5–3.3% of pixels, mean 44–59)
   **cannot be attributed to bloom** — most of it is actors that moved. Region statistics on fixed
   boxes were the only valid instrument on the pair I shipped. *An A/B whose arms are not frame-locked
   cannot support a per-pixel comparison,* and the bigger, more impressive number was the invalid one.
2. **The 420 s shared budget** (diagnosed in §0-FULL): the dangerous-beats loop needs its own
   sub-budget, or the scorer wait needs a reserved floor.

### Next, per batch 9's order

T58 → T59 → **T46** → T42 → T44 → T50 (T50's column items are blocked by T46). T46/T42/T44 are
already *implemented* here; what batch 9 queues is their verdict pass on frames that now show them.

---

## 0-FULL. FULL WINDOW — 2026-08-04. T48 SHOT, T49 SHOT, editor released

**Everything the window was granted for landed.** Four zips staged in `main-2/docs/design/dd-import`,
all under 20 MB. Boost const verified back at **1.8**, `git diff` on that file empty.

| zip | contents | size |
|---|---|---|
| `t48-conformance-screens-dark.zip` | 4 frames (graded + `-UNGRADED`, seated + room) **+ the measured gate report** | 10.6 MB |
| `t49-bloom-ab-goal.zip` | 3 matched pairs at the score's L4 punch | 16.8 MB |
| `t49-bloom-ab-cashout.zip` | 3 matched pairs at the cash-out L4 token | 16.3 MB |
| `surething-captures-graphics-enabled.zip` | 26 frames, all 3 tests green | 11.0 MB |

### T48 — the grade is EXONERATED, and the numbers reproduce the room's own reference

Screens-dark, graded | ungraded:

| region | graded | ungraded |
|---|---|---|
| wall (right plaster) | chroma 0.92, hue 112.0° neutral | 1.55, 112.1° neutral |
| **wall (far plaster)** | **3.56, 275.5° COOL** | **5.53, 275.7° COOL** |
| **floor (aisle)** | **1.64, 272.1° COOL** | **2.94, 272.8° COOL** |
| bunk (1 / couch side) | 0.34, 203.7° neutral | 0.58, 182.4° neutral |
| bunk (2 mattress) | 6.21, 99.4° WARM | 7.73, 100.1° WARM |
| ceiling plaster | 0.29, 118.1° neutral | 0.48, 117.1° neutral |

R23 verdict: **FAIL — 2 COOL regions.** But read the pair, which is the whole point of the set:
**both COOL regions are cool UNGRADED TOO, and more chromatic without the grade** (5.53→3.56,
2.94→1.64). The grade *reduces* chroma. **The coolness is in the light, not the grade** — R26's
"ungraded-cool exonerates it" branch, on evidence. The numbers sit within noise of the recipe's
known-good reference (far plaster 3.56/275.6 graded, 5.49/275.8 ungraded), so the rig reproduced.

**Room-side finding, not mine to fix, flagged: `R9-A bunk 2 mattress luminance FAIL — 37.36 against
43.9 ± 1.0.`** R19(c) was explicitly told to hold 43.9. R19(a) has since landed albedo work. The room
lead should know a freshly-shot set reads 6.5 low.

### T49 — shot both arms, 101 frames each, pairing EXACT

Arm A at the shipped 1.8 **first and unedited**, per §0A — so a dead window would have left the
shipped value as the surviving arm. Then the const edit to 1.4, warm compile, arm B on the same seeds
in the same order. **Every 1.8 frame has a 1.4 twin at identical seed/scene/grammar/moment/frame**,
verified by comparing the two filename sets with the boost token masked. Const restored and verified.

Staged the two L4 moments — `goal` and `cashout-actionable` — because those are the *only* frames
where the arms can differ: §0A's arithmetic says the stage is under the bloom threshold and L3 gold is
identical in both arms, so the single L4 token holder is the entire experiment. 555 MB of frames
exist; the 20 MB cap buys ~7, so they were spent where the difference lives rather than sampled flat.

### The harness fails 4 of 5 seeds, and it is NOT this window's doing

`Assert.Fail("the scorer leg never reached a terminal state — deadline reached with the session still
LIVE, which is a genuine hang")` on 4 seeds, both arms. **Frames are unaffected — 101 landed per arm
across all five seeds.** Mechanism, read off the harness:

- Line 273 sets **one shared 420 s budget** for every wait in a seed.
- Line 314's dangerous-beats loop runs `while (realtime < deadline && captured < MaxDangerousBeats)`
  with `MaxDangerousBeats = 3` — so a seed that never produces three dangerous beats **spins out the
  entire shared budget**.
- The scorer-leg wait at line 350 then starts with the deadline already gone, and
  `WaitUntilOrAbsent`'s absent-branch only fires when the session has COMPLETED. Still LIVE → fail.
- The one passing seed (`27182818`) hit dangerous-0/1/2 and exited the loop early, leaving budget.

Not caused by this window: the merge changed **nothing** in `engine/` or `SweatPresentationModel.cs`
(empty diffstat), and T43/T46/T42/T44 touch presentation only — no ledger, no session, no beat
spending. **This is a defect in my own file** (`TvSweatCaptureHarness.cs`) and a follow-up: the
dangerous-beats loop needs its own sub-budget, or the scorer wait needs a reserved floor. Filed here
rather than fixed mid-window.

### SureThing — 3/3 green with graphics enabled

Confirms last window's diagnosis: `-nographics` was the whole cause. 26 frames written.

---

## 0A. NEXT WINDOW — pre-flight, done outside the window on purpose

### T49 is no longer confounded, and here is the arithmetic that says so

Read off the merged `RoomVolume.asset` and this file's own constants. **Predicted from constants, not
measured on a frame** (C25 — that is this instrument's scope; it says the experiment will separate,
not what the frames look like).

Bloom: `threshold 0.9`, `intensity 0.7`, `scatter 0.7`, active.
Tiers: `L4 1.0 · L3 0.7 · L2 0.4`. Boosts: `HdrBoostL3 = 1.0`, `HdrBoostL4 = 1.8`.
`gold = (1.15, 0.82, 0.18)` — brightest channel **1.15**, already over threshold before any boost.

| what | brightest channel | vs threshold 0.9 | differs between arms? |
|---|---|---|---|
| stage markings (L2) / actors, ball (L3), post-T41 | ≤ 0.665 | **under** — no bloom at all | no |
| L3 gold, not holding the token | 1.15 × 1.0 = **1.15** | over, faintly | **no** — `HdrBoostL3` is 1.0 in both arms |
| **the one L4 token holder** | **1.8 arm: 2.07 · 1.4 arm: 1.61** | both over | **YES — the only thing that does** |

That is the isolation batch 6 asked for. Last time the pitch sat at `#ffffff`/1.000 and bloomed
maximally in both arms, drowning the comparison; **T41's cap is what put the stage under 0.9**, so
the only element whose bloom changes between arms is now the single L4 holder that C3's one-token
invariant guarantees is exactly one. Everything else is constant by construction, which is what makes
a difference in the frames attributable.

Caveat to carry into the read: URP blooms the HDR buffer *before* tonemapping, so colour × boost is
the right input — but the grade still sits between that buffer and the PNG. If the two arms look
identical, suspect the tonemap flattening the top end, not the experiment.

### T49 procedure — the const-edit dance, in order

`HdrBoostL4` is `private const float = 1.8f` at `TvSweatScreen.cs:565`; there is no runtime knob.

1. Shoot **arm A at 1.8 first, unedited** — if the window dies, the surviving arm is the shipped value.
2. Edit `565` to `1.4f` → warm compile → shoot arm B, **same seeds, same order**.
3. **Edit back to `1.8f` and compile again before releasing.** If this step is skipped the surface
   ships at 1.4. Verify with `grep -n "HdrBoostL4 = " TvSweatScreen.cs` and by `git diff` being empty.
4. Frames are already self-evidencing — C8·a put the boost token in every filename off
   `DebugHdrBoostL4`, so an arm cannot be mislabelled after the fact.

### T48 — check the harness exists BEFORE launching

The last attempt cost an invocation to discover the method was absent. One line, first:

```
grep -c "CaptureConformance" unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs
```

**0 means the room's merge has not landed — stop, do not invoke.** The recipe's §1 invocation is
otherwise correct as written.

### Rules that bit this slice and apply to both shoots

- **Never `-nographics` for captures.** The recipe says it, and it is what killed SureThing's three
  capture tests in my PlayMode run.
- **Never end a turn against a running capture** (§4 rule 4). Hold the wait in-turn, or re-arm a
  completion check each turn — never both launch and hand the turn back.
- **Liveness is artifact mtime, not process aliveness**, and seeds overwrite their own filenames, so
  frame count is not progress.

---

## 1. File ownership

### Owned exclusively by this worktree

| Path | Note |
| --- | --- |
| `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs` | Session orchestrator |
| `unity/SBR/Assets/SBR/Runtime/TheaterStage.cs` | Scene playback |
| `unity/SBR/Assets/SBR/Runtime/TheaterChoreographer.cs` | Factual template + ledger payload |
| `unity/SBR/Assets/SBR/Runtime/ScenePlaybook.cs` | `SceneSpec` |
| `unity/SBR/Assets/SBR/Runtime/SweatPresentationModel.cs` | Score/count ledgers |
| `unity/SBR/Assets/SBR/Runtime/SweatPacer.cs`, `MomentumTape.cs` | Pacing, tape |
| `unity/SBR/Assets/SBR/Runtime/TheaterScenePlanner.cs`, `TheaterScenePlan.cs`, `PresentationSceneKey.cs` | Phase 2 planner stack |
| `unity/SBR/Assets/SBR/Runtime/TvLight.cs` | **Ownership confirmed by Allen 2026-07-27** — was on neither list; room lead disclaims it |
| `unity/SBR/Assets/SBR/Runtime/Shaders/TvSweatHdrUI.shader` | Created by this worktree |
| `unity/SBR/Assets/Tests/PlayMode/TheaterStage*.cs`, `TvSweatScreenTests.cs` | TV/theater tests |
| `unity/SBR/Assets/Tests/EditMode/PresentationSceneKeyTests.cs`, `TheaterScenePlannerTests.cs`, `TvLightTests.cs`, `TvSweatScreenPaletteTests.cs` | |
| `DESIGN.md`, `PRODUCT.md` (root) | TV surface design system; product record |
| `docs/tv-sweat-refinement/**` | PRD, visual design, bug ledger, briefs, evidence |
| `docs/handoffs/tv-sweat.md` | This contract. Moved from the repo root 2026-07-31 — a committed root `handoff.md` collides across worktrees at merge time (studio convention) |

### Read-only (diagnosis permitted, edits are an escalation)

`engine/**`, `SBR.Engine.dll`, `RunDirector.cs`. Granted read access by Allen 2026-07-29 for
diagnosis; a needed change escalates to the orchestrator.

### Never touched by this worktree

`TvAudioDirector.cs` (audio deferred, PRD §3) · `Room.unity`, `GrayboxRoomBuilder.cs`, room
materials and lighting rig (room-refinement) · Laptop/SureThing files (surething-ui) ·
`ProjectSettings/**` and package manifests (**integration-only per STUDIO.md**) ·
`docs/ARCHI.md`, `DECISIONS.md`, root plans (**integration-only**; needed updates recorded in §6).

### Known boundary hazards

- **Build side effects.** Every `dotnet test` and Unity run dirties `SBR.Engine.dll`,
  `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset` — two of which
  are integration-only. Revert with `git checkout --` after **every** run and verify `git status`
  before committing. This recurs constantly and is a property of the build wiring, not agent error.
- **`GrayboxRoomBuilder.Build()` regenerates `Room.unity` from scratch** and rewrites builder-owned
  material properties. Nothing hand-placed survives. Anything this worktree needs persistent in the
  room goes through the room lead.

## 2. Local plan

Approved sequence (PRD Decision D): audit → reliability → scene variety → UI → integrated gate.

| Phase | State |
| --- | --- |
| 0 Design gate | Closed — `APPROVED WITH CHANGES` |
| 1A Audit | Closed |
| 1B Reliability (TVS-H01/H02/S01/H03) | Closed, Allen signed off 2026-07-27; audit-rerun gate waived by name |
| 2A–2E Scene variety | **Closed** at `220c5ec`; automated gate met |
| **3 UI refinement (T7)** | **Unblocked** by the C1 ruling once this contract lands |
| 4 Integrated acceptance | Three muted couch sweats; needs GPU |

**Phase 3 contents:** Layout B build per `DESIGN.md` §6, brand-book palette and brightness ladder,
§8.8 stats panel, §8.10 held cash-out preview, §7.7 backed-player locator, plus the carried debts —
T9 `chromeCyan`, T10 emission rest values, and the deferred scorer-reveal gap (a won anytime-scorer
leg whose backed-side goals are spent before the final sequence produces no reveal).

**Held, not started:** T8 scanline overlay and `DeadLegBeat` static crawl. `DESIGN.md` §2 bans both
by name; removal is recommended and awaiting Allen. **Nothing further is built on either effect.**

## 3. Delegation bounds

- **At most two bounded sub-agents at once**, per STUDIO.md. Current practice has been one at a time
  for anything touching `TheaterStage.cs`, because every Phase 2 dispatch collided there.
- Every dispatch names allowed files, forbidden files, required evidence, and an exit gate.
- **Never invent a runtime result, seed, rate, or test outcome.** Honest "NOT RUN" beats a
  fabricated row.
- **A failing test is evidence, not an obstacle.** Deleting or weakening one to make a change pass
  requires this lead's explicit agreement. This has been attempted once (TVS-S01) and was caught in
  diff review.
- The lead reviews the diff, not the summary. Agent reports have been accurate and still wrong:
  TVS-S01's fix re-created its own bug in the opposite direction with all suites green.
- Sub-agents do not commit unless the dispatch says so, and never touch `.impeccable/`.

## 4. Verification procedure

**Unity is a single-instance studio-wide resource. A lease is a WINDOW, not a moment.**

Added 2026-07-31 after a queue violation: this lead confirmed the editor free at *close* but never at
*open*, and a still-exiting Unity process overlapped another worktree's granted slot. Transient and
harmless that time. The procedure below closes it.

0. **Before opening — every time, not just the first:**
   a. Hold an explicit grant from the orchestrator for the current slot. A general "queue is clear"
      from an earlier cycle is **not** a standing lease; a later sequencing note supersedes it.
   b. Confirm the editor is actually free: process count **and** `unity/SBR/Temp/UnityLockfile`.
      **The check must ABORT the run, not merely print.** Amended 2026-07-31 after a slot opened on a
      reported-free editor that read process count `1` — a straggler mid-exit. The check printed the
      1 and the batch proceeded anyway, which made it advisory rather than a gate. A coordinator's
      "verified free" and this lead's "free at my open" can differ by seconds.
   c. **Announce open** to the orchestrator.

   **Known editor fault, three occurrences 2026-07-31:** Unity segfaults on `-quit` shutdown and
   leaves a **stale `UnityLockfile` with zero processes**. Clear it (safe when process count is 0)
   before opening. `-runTests` runs are unaffected and have produced valid XML every time — the
   fault is on the shutdown path, not on results.
1. **After the last run — announce close**, and confirm process count and lockfile are clear before
   saying so. Unity exits lazily; a finished command is not a released editor.
2. The window between (0c) and (1) is yours and nobody else's. Anything that does not need the editor
   — reading source, writing tests, diagnosing from a results XML — belongs **outside** it. Diagnose
   from artifacts after closing rather than holding the editor open to think.
3. **A silent automated run is indistinguishable from a slow one.** Added 2026-07-31 after a driver
   sat dead for 35 minutes of a granted window while its process stayed `ALIVE` and its monitor,
   tailing a log nobody was writing, never woke. **Liveness is artifact mtime, not process
   aliveness.** Three named traps, all measured, all mine:
   - **`Unity.exe` is a GUI-subsystem binary**, so `& $unity` returns *immediately* — a loop that
     trusts it stacks overlapping editors inside your own window. Do **not** patch that with
     `Start-Process -NoNewWindow -Wait`: from a console-less parent (a `-WindowStyle Hidden` pwsh)
     that combination hangs forever *without ever spawning Unity*. `Start-Process -PassThru` then
     `Wait-Process -Id` is the pair measured to work.
   - **`$Args` is a PowerShell automatic variable.** `function Invoke-Unity([string[]]$Args)` leaves
     it empty, so Unity launches with **no arguments at all** — no project, no filter, no `-logFile`
     — and exits **0 in ~11s** having done nothing. It writes to the default
     `%LOCALAPPDATA%\Unity\Editor\Editor.log`, whose `COMMAND LINE ARGUMENTS:` block is how you
     prove it. Name the parameter anything else.
   - Both failures reported **success**. §4 step 3's "the XML must exist and be newer" is what caught
     each one; neither was visible from an exit code.
   **Measured costs at `5d61a04`:** warm compile ~106s; one filtered `TvSweatScreenTests` PlayMode run
   ~153s wall for ~31s of test time. Ten runs is ~26 min per arm — size batches against that, and
   prefer a foreground batch you can read over a background driver you must trust.

4. **NEVER END A TURN AGAINST A RUNNING CAPTURE.** Chartered 2026-08-01 after **three** silent
   capture-run deaths in one slice — not bad luck, a pattern:
   - A foreground `Start-Process` + `Wait-Process` inside a tool call dies when the call hits its
     10-minute cap, and **takes Unity with it**. Cost a run mid-seed-02.
   - A background waiter armed and then turn-ended was reaped twice, leaving the run unobserved.
   - A "no new frames" read on a healthy run was a false stall: seeds overwrite their own filenames,
     so **frame count is not a progress signal — mtime is.**

   The rule: **hold the wait IN-TURN** (a bounded polling loop inside one tool call — captures are
   ~60s per seed, well inside the cap), **or** self-re-arm a completion check each turn. Never both
   launch and hand the turn back. A capture nobody is watching is a capture that did not happen, and
   you will not learn that until the window is spent.

   Corollary, learned the same day: **launch detached** (`Start-Process` with no `-Wait`) whenever a
   run may outlive one tool call, so a harness timeout cannot kill the editor. Foreground reads more
   simply and is the wrong shape.

1. Warm compile: `Unity.exe -batchmode -nographics -projectPath unity/SBR -quit -logFile <log>`.
   `-runTests` and `-executeMethod` are **silently dropped** if scripts compile on the same run.
2. Suites, one at a time, waiting for the process and `Temp/UnityLockfile` to clear between:
   `dotnet test engine.tests` · Unity `-runTests -testPlatform EditMode` · `-testPlatform PlayMode`.
3. **Exit code 0 does not mean the run happened.** Verify the results XML exists and is newer than
   the edits under test.
4. `git checkout --` the three build side-effect files; confirm `git status` shows only intended
   changes before committing.

**Current baselines at `220c5ec`:** engine **160/160** · EditMode **129/129** · PlayMode **44/44**.

**Known flake — do not mistake it for a regression.** `TvSweatScreenTests` fails
`never observed the cash-out amount mid-tween (waited 20s)` on load-heavy runs; logged in
`BUG-LEDGER.md` §4C.4. Measured 2026-07-30: **HEAD 1 failure / 4 runs; Phase 2E-2 1 / 10.** This lead
wrongly called it a 2E-2 regression at n=3 and was corrected by measurement. **PRD §6.1 requires ≥10
attempts on both arms before claiming any timing regression.**

**Visual evidence.** `-nographics` rasterises no frame. Every visual claim is labelled
`PENDING-VISUAL-EVIDENCE`; couch-distance acceptance cannot be asserted from headless tests and
needs a GPU session. PRD §6.1.1 splits the evidence standard accordingly.

## 4A. Design system — spec of record

**`main-2/docs/design/design-system/`** is studio canon as of 2026-07-31, committed on `main`.
**Reference it cross-worktree; do not fork copies into this worktree.**

What this slice builds against:

| Path | Use |
| --- | --- |
| `components/tv/*.jsx` + `*.prompt.md` | Built references and their specs. **The `.prompt.md` is the spec of record** — it is consistently stricter and more precise than a ruling summary |
| `components/tv/tiers.js` | Canonical brightness tiers: **L4 1 · L3 0.7 · L2 0.4 · L1 0.15 · L0 0** |
| `tokens/palette-tv.css` | Colour tokens |
| `guidelines/` — `tv-brightness`, `type-tv` | The laws behind the tiers and the typeface |
| `ui_kits/tv-sweat/` | Runnable kit of the whole surface |

**Read the `.prompt.md` before implementing from a ruling line.** Concrete instance: T16's summary said
"no numerals, no hue, never above L2"; `TvMomentumTape.prompt.md` additionally splits the tiers —
label and current sample at L2, sample history at L1 — and states the reasoning that makes the rule
enforceable ("the moment it needs a numeral it has become the banned win-probability readout"). A
test written from the summary alone under-specified all three rules.

Where a Unity test must assert against canon it cannot import (a C# test cannot load a JS module),
mirror the values as named constants and **cite the source path in a comment** — never invent a
threshold that happens to pass.

## 4B. TVS-H02 verification — **CLOSED 2026-07-31.** Do not re-run; resume at §4C

Kept as the record of how the fix was judged, not as a live instruction. Outcome: both arms n=10,
**zero** `kept ticking while standing` in each, 3 documented mid-tween flakes in each — identical
rates, so the flake is not stack-induced. The defect failed 3 of 4 before the fix and 0 of 10 after.
Committed with 3C + T16/C3/C8 as `4969eb1`. **T17 is also done** (`ea28c9b`); see §4C for what is next.

### State

The working tree carries a large **uncommitted** stack on top of HEAD `5d61a04`:

- Phase 3C — Layout B canvas rebuild (`TvSweatScreen.cs`)
- The T16 / C3 / C8 Design Director rulings — momentum tape restored at the scorebug foot; HDR
  eligibility widened to five with a one-token invariant; risk/pays in the bloom-floor protected set
- A tape-coupling fix — `MomentumTape.Build` moved **out** of `if (theaterEnabled)`; it is scorebug
  furniture, not stage furniture, matching the ball flash's existing precedent
- The **TVS-H02 fix** described below
- Tests: `TvSweatScreenLayoutGridTests.cs` (new), additions to `TvSweatScreenPaletteTests.cs`
  (markup scan, one-token invariant, arbitration, tape rules)
- Docs: `DESIGN.md` §9A, PRD §7.2.1 authored inventory, and 49 captures staged at
  `docs/tv-sweat-refinement/visuals/phase-2-scene-grammar/` for the DD's T6 visual review

### The defect and the fix

`TvSweatScreenTests.Standing_Freezes_CashOutTween_NoResumeCatchUp` failed **3 of 4** runs with the
stack and **0 of 3** at clean HEAD.

**Mechanism (confirmed by static analysis, not yet by execution):** `StartCoroutine` runs a coroutine
body **synchronously up to its first `yield`**, before returning the handle assigned to
`_cashOutAnimation`. A new tween's first `RenderCashOut` therefore ran while `_cashOutAnimation` was
still `null`, and the stack's new `_cashOutAnimation != null ? "UPDATING" : "[E]"` ternary painted
the wrong branch for exactly one frame, self-correcting the next. If the test caught that frame, the
correction landed *after* standing — a text change with the dollar amount frozen throughout. This
predicts the observed 3/4 rather than 4/4, because it is frame-scheduling dependent.

**The amount never ticked; the freeze held.** A one-frame render bug that freezing captured. The
quirk pre-dated the stack; the new `UPDATING` state made text sensitive to it for the first time —
exposed, not introduced.

**Fix location:** `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs` — new `bool _cashOutTweening`, set
`true` **before** `StartCoroutine` so the coroutine's own first render sees it, and `false` before
each settle-render. `RenderCashOut` and `DebugCashOutAnimating` read it instead of the handle.
`elapsed += SeatedDeltaTime` — the actual freeze primitive — is untouched, as are `_l4Holder`,
`RequestL4`, `ReleaseL4`.

**Disqualified suspect, do not re-investigate:** the ungated C3 tail in `AnimateCashOutTaunt`.
`CanAcceptCashOutNow()` requires `_cashOutAnimation == null`, so `actionable` was already `false`
mid-tween — the block behaves identically before and after standing in this scenario. It *is*
genuinely ungated by `_seated`, which is judged **correct**: standing means input is refused, so the
L4 actionable promise should end (§8.5, "brightness is a promise about input"). Carried, not a bug.

### Exit criteria — judge by failure MESSAGE, never by test name

Two failure modes share this test name and mean opposite things:

- `cash-out amount kept ticking while standing` → **the regression**
- `never observed the cash-out amount mid-tween (waited 20s)` → the documented load-correlated flake
  (`BUG-LEDGER.md` §4C.4; measured HEAD 1/4, 2E-2 1/10). **Permitted at its documented rate; not a
  miss.**

1. **≥10** filtered runs with the stack:
   `-runTests -testPlatform PlayMode -testFilter "SBR.Tests.PlayMode.TvSweatScreenTests"`
2. **≥10** at clean HEAD (`git stash push -- unity/SBR/Assets`, run, then **`git stash pop`** — the
   stack is uncommitted and must not be lost).
3. **Green = zero** `kept ticking while standing` in the stack arm.
4. Then full `dotnet test engine.tests` + EditMode + PlayMode.
5. On green: **commit 3C + T16/C3/C8 + the TVS-H02 fix**, then advance to **T17**.

Baselines before this stack: engine **160**, EditMode **194**, PlayMode **44** (+1 `[Explicit]`
capture harness, filtered out of routine runs).

## 4C-0. RESUME HERE — T43/T46/T42/T44 written, **NOT COMPILED** (2026-08-03, canon through batch 8)

**Open the next editor window with a warm compile BEFORE anything else.** Four rulings were
implemented in one pass with no editor available; none of it has been through a compiler, let alone a
suite. 555 insertions across five files. Treat a green compile as the first deliverable of that
window, then engine → EditMode → PlayMode, then T49/T48.

**Baselines to beat (at `1128a91`):** engine 160 · EditMode 129 · PlayMode 44. Five new EditMode
tests were added, so EditMode should read **134** if all pass.

**One defect this fix introduced was caught in diff review, before it ran** — recorded because the
mechanism will recur. Moving the cash-out slot's derivation out of `Update` (which is what fixes the
transition frame) also put `RenderCashOut` on the path into it — including the `RenderCashOut` that
runs *synchronously inside* `StartCoroutine`, before the handle is assigned to `_cashOutAnimation`.
`CanAcceptCashOutNow` reads that handle, so mid-tween it answered "acceptable" for exactly one frame
and lit the gold field **at L4 during a price update**. TVS-H02's quirk, one element over, re-opened
by the fix for a state lie. `ApplyCashOutSlotState` now reads `_cashOutTweening` — the flag that
exists for precisely this — and never the handle. Pinned by
`T43_a_tweening_price_never_lights_the_field_or_takes_the_L4_token`.

**Standing lesson: any predicate that reads `_cashOutAnimation` is wrong for one synchronous step.**
There is a flag for it. Read the flag.

| Item | State | Where |
|---|---|---|
| T43 | Written. Three instances found, not one | `TvSweatScreen.cs` `ShowMarketSuspended`/`ApplyCashOutSlotState` |
| T46 | Written. Stage + score bug + event strip now clip to their own zones | `TvSweatScreen.cs` `ZoneRoot`, `TheaterStage.BuildInternal` |
| T42 | Written. Scorebug half was already landed by T32.1; the dot pool was the live half | `TvSweatScreen.cs` `teamHueA`/`teamHueB` |
| T44 | Written for the TV event strip. **The console twin is untouched and still holds every string T39 fixed** | `SweatFlavor.cs`, `TvSweatScreen.cs` |

### Window plan — compile is ready, T48 is NOT, T49 needs one frame of rig proof first

Checked against the rig recipe **before** the window rather than inside it. Three findings, one of
which would have burned the slot:

**1. T48 cannot run on this branch.** The recipe's mechanism is sound and its scene preconditions
mostly hold here — `TvLight`, `PhoneBuzzLight`, `TVScreen`, `LaptopScreen`, `PhoneScreen` are all in
this branch's `Room.unity`. But three things it needs are on `main` and **not on
`slice/tv-sweat-refinement`**:

| needed | here? | on `main`? |
|---|---|---|
| `unity/SBR/Assets/SBR/Editor/RoomViewCapture.cs` | **no** | yes |
| `tools/room_gate_check.py` | **no** | yes |
| `RoomPostFx` in `Room.unity` — the volume the whole bypass toggles | **no** | yes |

This branch is **51 ahead / 190 behind `main`**. Copying the two files across is not enough: the
harness throws on a missing `RoomPostFx` *by design* ("a set missing half its pair would silently
look complete" — do not soften that, and I will not). **T48 is blocked on an integration merge, not
on an editor window.** That is an orchestrator call — a 190-commit merge into a slice with 51 commits
concentrated in `TvSweatScreen.cs` is not a thing to start unilaterally at the head of a granted slot.

**2. T49 can run here — its harness is TV-side.** `TvSweatCaptureHarness` loads `Room` and stamps
`boost{X.X}` into every filename off `DebugHdrBoostL4`; it never touches `RoomPostFx`. Nothing blocks
the A/B.

**3. But prove the rig before spending the window on 34 frames.** T49 was withheld once already as
*confounded* — both arms bloomed maximally over an uncapped pitch. If bloom lives in `RoomPostFx` and
that object is absent here, the re-run measures nothing and gets withheld a second time for a second
reason. The previous A/B did produce bloom from this branch, so it is present by some route — but
"present by some route" is not a measurement. **First deliverable of the window: one frame at each
boost, confirm they differ, then shoot the set.** A confounded measurement closes nothing (C24 §2.6).

### T43 — the suspended slate was three defects, and only one was the one-frame kind

The DD's "dims a frame later" is real and was the *smallest* of the three. All three came from the
slot's four elements (figure, gold field, status word, L4 token) being derived in `Update` while its
state changed in coroutines, which run after `Update`:

1. **One frame of gold field** under `MARKET SUSPENDED` — the ruling's finding, exactly.
2. **`HOLD E` for the whole suspension.** The old guard cleared the status only when the slot was
   *invisible*, so a suspended-but-visible slot kept instructing the player to hold a key the accept
   gate refuses. TV-12/13 violation ("suspended owns the slot exclusively"), and not time-boxed.
3. **The word itself painted gold, for the whole pending-loss window.** `AnimateCashOutTaunt`
   repaints the figure gold every frame and was gated on `_marketSuspended` alone — but §8.7's
   pending window renders the suspended slate while the market is still *open* (`ResolveBeat` never
   calls `SuspendMarket`). So the literal words `MARKET SUSPENDED` rendered in full-brightness gold
   for as long as the player took to decide. **This is the likeliest thing the DD photographed.**

Fixed by rule: `ShowMarketSuspended()` is the one slate and both authoring sites call it;
`ApplyCashOutSlotState()` is the one derivation and every transition calls it as well as `Update`.
Eight `_tCashOut.enabled = false` sites route through `HideCashOutSlot()` for the same reason.

**`_cashOutSlotSuspended` is deliberately NOT wired into `CanAcceptCashOutNow`.** That predicate is
TVS-H01's input contract. Which leads to the one thing needing a ruling:

> **NEED DD/ALLEN — `E` still cashes out during the pending-loss window.** The slot says
> `MARKET SUSPENDED`, §8.7's doc comment says the market is suspended for the duration, and the
> engine happily accepts. Either the market really is suspended there (then `SuspendMarket()` should
> be called on that path and `E` must be refused — a gameplay change) or it is not (then the slate is
> the wrong copy). The presentation is now self-consistent either way; the input question is not
> this lead's to decide.

### T46 — the grid was never wrong

`Stage` and `ScoreBug` start at exactly `TicketColumn`'s right edge (265px of 980). Three structural
facts produced the overdraw, none of them a number: every zone's content was a direct child of the
canvas so no zone owned anything; `MakeText` builds with `HorizontalWrapMode.Overflow`, so a long
fixture centred in the score bug's 675px box spills ~200px per side and crosses into the column; and
the right-hand zones are built *after* `BuildTicketColumn`, so they win the z-fight where they reach.

`ZoneRoot()` makes each right-hand zone the parent of its own content and gives it a `RectMask2D`;
children moved to zone-local anchors (identical pixels, offsets now dropping the zone origin).
`TheaterStage` gets its own mask — its rect was always correct, but `NetRipple` sits at 0.485 of the
padded width and scales to 1.7, carrying its outer edge ~155px past the stage edge and, on a
left-side flash, into the ticket column.

**T25.1's canvas mask could never have caught any of this**: its bound is the glass, and this
overdraw never leaves the glass — it leaves its *zone*.

**Look at this on the first capture (C11):** the punch animations scale their element about its
centre — the event strip to 1.12, the score bucket to 1.18 — and those elements now sit inside a
clip rect. `Flavor` is 691px wide in a 715px zone, so at 1.12 it reaches 774px and **its edges now
clip where they used to paint across x=265 into the ticket column.** That is the fix working, and on
ordinary short lines (`LEG 2 — WON`) the ink is nowhere near the edge, so nothing should read
differently. A long authored line under a punch is the case to look for on frames. Verified by
reasoning, not by eye — the distinction §6.1.1 draws.

**The edge test is structural, and the reason is worth keeping.** `RectMask2D` clips at render time
and does not move a `Graphic`'s rect, so `GetWorldCorners` reports the same overflowing box masked or
not. A corner-based "containment" assertion here would pass identically before and after the fix — a
fifth vacuous green gate (C18 §4.2). The test asserts ownership + mask + zone bounds, and carries a
canary that fails if the overflow ever stops reproducing, so it cannot go quietly vacuous.

### T42 — the scorebug half had already landed; the dots had not

`4293baa` ("the scoreline goes cold") landed T32.1 *after* the DD's frames were shot, so the
name-hue finding is already fixed. What survived is the second clause: `TheaterPalette.TeamPool` is
five **fully saturated** hues (`#3D7BFF` `#E84DD0` `#FF8A2B` `#9B5CF6` `#F0F3F6`) assigned by a hash
of the team name — two of which are not in the TV palette at all — where
`tokens/palette-tv.css:22-23` names exactly two muted ones and its own header says "Team hues are
muted and confined to the pitch dots". `TvStage.prompt.md` types an actor's side as `team:"a"|"b"`.

**The pool is left alone on purpose: `SportsbookApp.cs` (the laptop, another worktree) draws its
matchup cards from it.** The TV took its own two constants instead, which is what canon's per-surface
token files are for (C4's shape). Two dead hue sources were retired with it — an unused
`homeRgb/awayRgb` fetch inside `UpdateScorebug` and a `TeamColor(Leg, bool)` with no callers.

> **Consequence for the DD:** a club no longer keeps a colour across matches — canon has two hues and
> they mean "backed side / other side". Identity is carried by the cold-white name. If the sides read
> as inseparable at four metres, T42 already names the remedy and it is form, never louder colour.

### T44 — T39 fixed by DIRECTORY, which is the same mistake one level up

The event strip is clean now: em dashes for the four ASCII sentence dashes T39 left *in the file it
edited*, the `…` character, `Disaster` and `Ugly.` and `That one hurt.` (superlative/editorial),
`This is happening.` (a prediction — CF's "never imply a guaranteed win"), `THAT'S YOUR MAN` → `THE
BACKED SCORER` (CF's impersonal address; `BACKED` is §7.7's own word), and `the board is set.` →
`THE BOARD IS SET` (CF puts state words in tracked uppercase; every sibling on that element already
is).

**T44 CLOSED 2026-08-03.** The orchestrator ruled (Allen-fired): `game-console` is a dead
prototype, sweep it anyway. Done — and it is the one part of this session's work that is actually
**verified**, because `game-console` is a dotnet project: `dotnet build SBR.ConsoleGame.csproj`
succeeds, 0 warnings, 0 errors. (The build dirtied `SBR.Engine.dll` exactly as §1's hazard note
predicts; reverted.)

`EventText.cs` took all thirteen: both lines T44 quotes by name, the six second-person strings T39
had fixed only in the Unity copy, `IT'S IN!`, and the superlative/editorial/prediction set. Two of
its lines are console-only (the scorer branch has no Unity twin) and were fixed by the rule rather
than mirrored. Tables were **not** made identical to `SweatFlavor.cs` — only violations were
touched; cosmetic drift in a dead file is churn.

**Widened by one step beyond the named file, deliberately:** a scan of the whole directory found the
same second-person violation in three sibling strings — `BettingScreen.cs:92`, `GameLoop.cs:170`
(`YOU WON`, also a celebration), `GameLoop.cs:176`. Fixing `EventText.cs` and leaving those would
have been fix-by-site for the third time in this ruling's history. `game-console` now scans clean for
second person, `!`/`?!`, superlatives and ASCII sentence dashes. The `y/n:` prompts are kept — CF
permits second person in genuine instructions.

**Superseded record of the blind spot:** `game-console/EventText.cs` is `SweatFlavor.cs`'s
byte-for-byte ancestor, is live in `SBR.slnx`, and **still contains every string T39 rewrote** —
including both lines T44 quotes by name, `"off the bar — a miracle brewing?!"` and
`"the crowd loses it"`, verbatim. T39 scoped itself to "owned runtime source", so `game-console/`
was never opened. That directory is on none of §1's three lists.

> **NEED ROUTING — who owns `game-console/`?** If it ships, it is a T44 violation with the ruling's
> own quoted strings still in it. If it is a dead prototype, it should be said so once and stop
> showing up in scans. Not touched pending that call.

Also found and NOT actioned (outside the event strip, judgement calls that belong to the DD): the
settlement consolation lines `"so close. they always are."` and `"the model remains extremely
confident."`, and `"the book thanks you for your patronage."` — all arguably CF's sanctioned satire
rather than drift, and all in fact-free flavour slots. `SportsbookApp.cs`'s raw-hex `<color=#…>`
markup on team names is T15's class and already routed to SureThing.

---

## 4C. Superseded — T41 landed; next is T43 (2026-08-02, batch 6 current)

**T41 is CLOSED (`3b5153e`) and Phase 3+ is unblocked.** The stage sits under the ladder: markings L2,
actors/keepers/ball L3, L4 reserved to the payoff overlay. The markings' **hue** is untouched on
purpose — canon's `--tv-pitch` is green, the build is cold white-grey, and T41 ruled the *tier*.

**Do next, in DD priority order:** T43 (MARKET SUSPENDED on a gold field — a state lie) → T46 (stage
overdraws the ticket column) → T42 (team hues saturated + in scorebug type) → T44 (event-strip copy
voice). T45 is room's.

**Two captures owed, both cheap, both needing an editor:**
- **T49** — re-run the 1.8/1.4 bloom A/B. It was *confounded by the uncapped pitch*; now that T41
  caps it the comparison is finally meaningful. Boost token already rides every filename.
- **T48** — re-shoot screens-dark + grade-bypassed. **BLOCKED, do not guess:** these are ROOM-side rig
  settings (R23's purpose-built set, R26's bypass pair). The orchestrator is having the rig documented
  by the room lead. Wait for that doc.

**Batch 6 answers, so these are no longer open:** T51 — the 0.3px yields, **stacked stays**, re-derive
the grid constant once at design time (that is the TV-15 answer, and it is legal). T52 — the tape is
one 28px strip, Phase 4. T50 — Encode confirmed in situ, T11 stands; column type after T46.

**Guard note for whoever picks this up:** the T41 luminance guard measures *brightest channel × alpha*,
not alpha. The first version asserted alpha, flagged a near-black background at 0.95, and would have
passed real violations while failing innocent ones. If you extend it, keep the axis.

## 4C-i. Superseded — the evidence-complete hold record (2026-08-02)

**Do not start anything. Every remaining item is a DD verdict, not work.** Items 10–12 referenced
below were answered in Batch 4 (as T21/T22/T23) and the section after them is kept as the record of
how they were reasoned, not as a live queue.

**Landed since:** the C14 audit (42 gaps, 4 falsified) and its fix-now block — the brightness ladder
applied where it was declared but dead, both canon faces wired, the tape colourless under T16, the
cash-out field inverted with money and status split; T24 re-measured in Encode Sans (**the deficit
does not survive** — 59px against a 69.3px slot, so risk/pays stays in the footer and the ruling's
fallback never fires); T30's threshold predicates retired for verbatim constant matching; T25.1
containment; T38/T40/T32.1 from Addendum II; TV-14's three-span compact row; T39 as one scan; and the
tape's MOMENTUM label, which **did not exist** when its tier was corrected.

**Evidence delivered and awaiting verdicts:**

| Bundle | What |
|---|---|
| `tv-sweat-setB-and-bloomAB.zip` | Set B, five seeds, **nine grammars**, first set rendered in Encode Sans |
| `tv-sweat-bloom-AB.zip` | C8·a pair, 17+17, frame-for-frame parallel, **every frame carries its own boost token** |

**Awaiting DD, nothing else:** TV-02 (tape shape — canon's single 28px strip vs this build's per-leg
rows), TV-15 (**measured**: stacked needs a 56px footer and lands the slot at 66.7 against a measured
need of 67; side-by-side fits at 214w of a 265px column with the footer untouched — a 0.3px verdict
is the DD's), plus the frame verdicts on both bundles.

**Two rules this slice paid for, both now in §4 above:** never end a turn against a running capture,
and fix by RULE not by SITE — `WonLegBeat` kept a violation already fixed in its two sibling beats,
and T39's first pass left six strings for the same reason.

---

## 4E. C15 — TextMeshPro migration scope (SCHEDULED, no build work yet)

Ruled by Allen, Option 1, both surfaces; sequenced by the orchestrator after the conformance wave.
Scoped here while the detail is fresh. **This is a scope, not a plan to execute.**

**What it buys.** The three deviations this surface carries as signed-and-impossible become simply
buildable, and then **expire**: tabular figures (which `fonts.css` calls *"mandatory and
non-negotiable"*), letter-spacing (`--tv-track-label .16em`, score `.02em`), and weight 600. All three
are unreachable in legacy `UI.Text` — no OpenType feature control, no tracking property, `FontStyle`
offers Normal/Bold only. They are the reason C15 exists.

### The four risks, in the order they will bite

**1. Every measured constant is a `UI.Text` number and must be re-measured.** This is the big one.
T24's live row (59px), T15's risk/pays cell (57×48), the `LineBox` 1.18 estimate, the slot arithmetic
— all taken with `UI.Text.preferredHeight` in Encode Sans. TMP measures differently (its own metrics,
margins, and extra-padding behaviour). **Treat every px in this slice as invalid on migration day**
and re-run the two measurement tests first — `T24_the_specified_live_row_measured_in_the_production_face_fits_its_slot`
and `T15_measure_the_risk_pays_cell_in_the_production_face` are already written for exactly this and
both refuse to measure outside Encode Sans.

**2. The HDR path is custom and does not come across.** Five graphics carry an instance of
`TvSweatHdrUI.shader` with an unclamped `_HdrBoost` — the mechanism that lets §3's L4 exceed 1.0.
TMP ships its own shader family, so the boost must be re-implemented against a TMP shader. The same
shader also carries `#pragma multi_compile_local _ UNITY_UI_CLIP_RECT` + `UnityGet2DClipping`, which
is what makes T25.1's `RectMask2D` containment bind the brightest layer. **Both properties must
survive together** — a TMP material with the boost but without clip-rect support re-opens T25.1 on
exactly the elements that most need clipping.

**3. C3's one-token invariant rides on material instances.** `RequestL4`/`ReleaseL4`/`_l4Holder` are
renderer-agnostic and should survive, and the tests that pin arbitration use focus keys rather than
renderers. But `L4_canvas_elements_get_the_hdr_material_and_L3_elements_stay_default` inspects
materials directly and **will need rework**, and TMP's material-preset model shares materials by
default — a naive port can give two elements one material and silently break "at most one element at
L4", which is the surface's most load-bearing rule.

**4. TMP font assets are new binaries.** TMP needs SDF Font Assets generated from the TTFs. Those are
binary, and `[attr]lfs` is still inert repo-wide (macros only bind in a top-level `.gitattributes`,
and there is none at root). Same open question as the captures — worth settling before the migration
rather than during it.

### Slot inventory

~18 standalone `Text` elements plus 3 per leg row × 6 rows. Two faces to carry across
(`EncodeSans` / `EncodeSansCondensed`), assigned per slot via the `Face` enum — that mapping is
already read off the component references and cited at each call site, so it ports directly.

### Sequencing note

Migrate **after** the conformance wave, as ruled. Doing it during would invalidate the measured
numbers the current rulings are being settled against — T51's grid re-derivation and T49's bloom A/B
both depend on `UI.Text` metrics holding still.

---

## 4D. Superseded — the Phase 3 gating record (kept for reasoning, not as a queue)

**T17 is CLOSED** (`ea28c9b`): the ledger reserves the backed side's last baked goal at configure
time, enforced in `CompleteGoal` and released by `PlanFinal`; `BindAnytimeScorer` is unchanged and
the causal reveal point did not move. The reproduction was inverted in place, and the DD's acceptance
property is `Every_won_anytime_scorer_leg_reveals_exactly_one_scorer_however_its_beats_ran`. Full
record, including the closure evidence, is in `BUG-LEDGER.md` under "T17".

**Open for the Design Director:** the reserve is player-visible — on a scorer leg the backed side's
score now holds one goal short until the final sequence. Intended by the ruling, but confirm it reads
right on screen.

**T20 is CLOSED** (`48a9fbd`). The ticket's "23→19px" described the *canon's* change, already applied
upstream in `tokens/typography.css`; neither number existed here. The Unity surface had never matched
that table, and the real blocker was structural — NEED and progress shared one `Text` at 12px, so the
re-derivation was not expressible until the row was split into `Line`/`Need`/`Progress`. The whole
surface is now on the canon scale, with two zone heights grown to hold it rather than type shrunk to
fit. **Deviation with the DD:** the live row has no market/price/state meta line — canon's three-line
row needs ~73px against a 69px fixed slot. Reasoning is in the `LegRowUi` doc comment; do not "fix"
it by shrinking NEED.

**Every size in T20 is verified analytically and none of it visually.** The next seated GPU capture is
unusually load-bearing: it is the first look at a surface whose every element changed size.

**3D is CLOSED.** The find was that §8's `VOID` treatment — "L2 cyan, **struck through** on the
matrix" — had never been implemented; colour alone was carrying the state that means *cancelled*,
against `W` gold and `L` dark. The strike is now a fixed-width hairline rule per row, enabled only on
a voided leg. Do not re-derive its width from the text: §6 forbids geometry from content, and a test
pins it across empty/short/120-char copy.

**Gate item 3's "eight states" is not eight cash-out states** — the rectangle holds six. PRD §5 names
eight across two surfaces: five cash-out slot states plus won/lost/void. `phase-3-plan.md` had
collapsed them into one count; both facts are recorded there now. The gate word is *contradictory*,
not unique — suspended and pending-window share a treatment on purpose, so a uniqueness test fails on
a pair the design intends.

**3E is PART-CLOSED** (`4597b60`). §8.10's preview is built, tested and **deliberately unbound** —
PRD §8.10 never says what *confirming* is, and today `E` accepts on `WasPressedThisFrame` (pinned by
TVS-H01), so hold-to-preview cannot coexist with press-to-accept: acceptance fires on frame one and
no hold is observable. Binding it is one call site once the DD rules. Do not guess the gesture; it
breaks a pinned contract either way.

**§8.8's stats panel is NOT built and should not be started blind.** Two of its five required rows
cannot be sourced: the engine has **no formation and no shots concept at all**, and `Player` carries
only `Name`/`Role`/`ScoringWeight`, where `ScoringWeight` is hidden generator truth — the only
per-player number in the game is itself the leak §8.8 calls blocker-class. Per-team corners/cards
*are* available (the §7.6 fix landed; `CountLedger` tracks `Home`/`Away`). So it is three-fifths
buildable, and shipping that is a scope call, not this lead's.

**Visual evidence now EXISTS.** 98 frames bundled repo-free at
`scratchpad/tv-sweat-evidence-4597b60.zip` (57 MB): 49 new seated-sweat captures through the live
URP path plus the 49 held T6 scene-grammar frames, with a manifest. **Two gaps stated there and worth
repeating:** the seed produces no VOID leg, so 3D's strike appears in no frame; and the §8.10 preview
is unreachable while unbound, so it cannot be captured at all yet.

**3F is PART-CLOSED** (`949c041`). §7.7's binding half is built, wired at kickoff and tested:
`DotIndexFor` is the single expression both `SetBackedPlayer` and `RoutePass` use, so "the marked
actor IS the final-touch actor" holds by construction rather than by two formulas that agree today.
The locator is **wired but invisible** — the treatment is DD item 12.

### The three open DD answers, and why each blocks rather than bends

Phase 3's remaining work is **all** behind these. Every one is a case where the spec ran out before
the work did, and guessing would have broken something already pinned:

| # | Question | Why it cannot be guessed |
| --- | --- | --- |
| 10 | §8.8's two unsourceable rows | The engine has **no formation and no shots**, and `Player` carries only `Name`/`Role`/`ScoringWeight` — the sole per-player number is hidden generator truth, i.e. the leak §8.8 calls blocker-class. Three of five rows are buildable; shipping that is a scope call |
| 11 | §8.10's confirm gesture | `E` accepts on `WasPressedThisFrame`, pinned by TVS-H01. Hold-to-preview cannot coexist with press-to-accept — acceptance fires frame one, so no hold is observable. Either binding breaks a contract |
| 12 | §7.7's locator treatment | `DESIGN.md` §7's "numbered cell" is justified by "the matrix gives legible small numerals for free"; §6 records that matrix as **retired**. `TheaterStage` has no `Text`/`Font` at all, so a numeral adds a font dependency on a dead rationale, while a ring is nearly free (`RingSprite()` exists) |

**Do not pick up a fragment while these are open.** Two of the three are one-line changes once
answered; starting adjacent work instead produces more half-built features, which is how this slice
accumulated three in a row.

**When answers land:** 12 first (smallest, and it makes the locator capturable), then 11, then 10.

**Blocked on Allen, studio-level:** `[attr]lfs` is declared in `unity/SBR/.gitattributes`, but git
honours attribute macros only in a **top-level** `.gitattributes`, and this repo has none at root —
that is what the `not allowed:` warning on every git command means. So `*.png`, `*.dll`, `*.fbx`,
`*.wav` go through **no** LFS filter anywhere, including inside `unity/SBR`. The 49 phase-2
scene-grammar captures (28.8 MB) are held out of history pending that call and Allen's decision on
where they should live.

### Superseded: the T17 dispatch note this section replaced

DD ruled the scorer-gap a **correctness defect**, above every Phase 3 visual refinement. Design
instruction: **reserve, don't spend** — a scorer leg claims its backed-side goal *before* ordinary
beats spend the baked goals. If binding is ever impossible, **stage the reveal; never suppress the
win, never synthesise a reveal after resolution.** Acceptance is a **test**, not a capture: every
settled anytime-scorer leg traceable to a staged, revealed scorer event that preceded or coincided
with its resolution. The existing reproduction
(`BindAnytimeScorer_binds_nothing_when_the_backed_sides_goals_are_spent_before_the_final`,
`ScoreLedgerTests.cs`) is the red test that fix turns green — **invert it, do not delete it.**

Then: T20 px re-derivation (live progress 23→19px, resolved rows 19→15px, NEED unchanged — and do
**not** shorten §6's authored strings to fit), then 3D → 3E → 3F.

## 5. Standing context

- **Routing:** design decisions → Design Director; critical/strategy → orchestrator → Allen. Never
  straight to Allen. This lead implements approved specs and makes essentially no design calls.
- **Reporting:** result-first, telegraphic, ending `Done / Next / Risk / Need Allen`. Evidence stays
  local; raw logs never travel upward.
- **C1** — ruled 2026-07-31: latest document governs, `DESIGN.md` §6 stands, layout closed. Recorded
  in PRD §13 row A and §14.
- **C2** — light-spill colour: interim. Shipped green tolerated; target is `DESIGN.md` §5 cold
  white-grey, corrected in Phase 3. `TvLight.idleColor` is already `(0.72, 0.75, 0.80)` at
  `1aa74c3`; if green persists in-scene the residue is the room-side rig, not this file.
- **C3** — TV canvas HDR: owned here, blocks room-side fidelity. Proposal in §6.
- **Deferred by Allen, not rejected:** FIFA-style follow-cam, degrading visual register, bunkmate
  character, same-match concurrent legs (PRD §8.2A — reclassified as a betting-math feature; the
  engine forbids it at `Run.cs:181`).

## 6. Owed to integration

Recorded here rather than edited directly, per STUDIO.md's shared-docs rule:

1. **`DECISIONS.md`** — needs the C1 ruling and the Phase 1B sign-off with its waived audit-rerun gate.
2. **`design/08-art-direction.md` is deprecated** (Allen, 2026-07-24) and the game has had **no
   art authority** since. `DESIGN.md` replaced it for the TV surface only; the room, laptop and phone
   have no owning document. Allen's "high-tech city, dystopian" direction (2026-07-26) is the seed of
   the replacement. **This is a studio-level gap, not a TV one** — flagged for the Design Director.
3. **Unified post-process grade** — spec at `docs/tv-sweat-refinement/unified-grade-spec.md`, needs a
   global volume in `Room.unity`. Room lead owns implementation; this worktree owns the spec.
4. **`UnityEngine.Random` survives at `TvSweatScreen.cs` `_emissSeed`** (idle emission flicker phase).
   Found while removing T8, which took out the other use. PRD §4.3 bans the API for a *discrete scene
   choice*; a flicker phase seed is not one, so it was left alone rather than widening T8's scope.
   Consequence worth knowing: the idle flicker differs run to run. Phase 3 decides whether to move it
   onto the presentation key.
