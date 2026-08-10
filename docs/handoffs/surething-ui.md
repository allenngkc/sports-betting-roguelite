# SureThing UI — re-seat state

**STATUS 2026-08-09 · converged at `1d6eb35`, IDENTICAL to main · tree carries two phantom-modified TV
fonts that are NOT this seat's and MUST NOT be staged — see the traps · `artifacts/` untracked, as always**

### Verifiable state

- **This surface's own suites: EditMode 78/78 · PlayMode 58/58**, last run by this seat at `45cb958`.
  **PlayMode moved 57 → 58** at `af0c42c` — the S71 margin-empty-state gate. **A run reporting 57 means
  that gate did not execute**, and the wrapper's count is what tells you.
- **Those two figures were NOT re-run on the converged tree, and do not need to be.** The convergence
  took 89 commits and 109 files including code, so the "nothing under `unity/` has changed" claim that
  used to sit here **is void** — do not carry it forward. What replaces it is better evidence, not
  weaker: **the merged tree was validated studio-wide at `1d6eb35` — engine 183/183, EditMode 250/250,
  PlayMode 84 + 6 by-design skips** — and this surface's 78 and 58 are subsets of those totals.
  **Studio counts are not this surface's counts.** 250 and 84 are every lane together; the 78/58 above
  is what a filtered run of this surface should report, and comparing the two directly is a category
  error waiting to be made by whoever reads a green 250 as covering something specific.
- **Branch is CONVERGED — 0 ahead, 0 behind at the fast-forward.** It was fast-forwarded deliberately
  rather than merged: HEAD was already an ancestor of main (this seat's work landed at `23f9da6`), so a
  `--no-ff` merge commit would have invented a sibling of main instead of becoming it. Read the counts
  separately —
  `git rev-list --count main..HEAD` is ahead, `HEAD..main` is behind. An earlier version of this file
  printed "12 ahead and 3 BEHIND" with the labels **swapped**, off a single
  `--left-right --count main...HEAD` whose output is *behind* then *ahead*. Do not trust a remembered
  pair; run the two commands.
- **Nothing in flight. No editor held. No work in progress.**

### S71's gate, as rebuilt — read this before touching it

Batch 24 granted three proposals off this lane's own audit of the gate it had just written. All three
are built at `1d5a2df`, tests only, no runtime change:

- **The second-person guard is the pronoun class as standalone words** (`YOU`, `YOUR`, `YOURS`),
  enumerated, splitting on runs of non-letters so `YOU'RE` and `YOU’RE` both reduce to `YOU`. It
  replaced a single-substring `YOUR` check that `YOU HAVE NO MARKS` walked straight through.
  **It is a PROXY and is labelled as one at the assertion:** S71 forbids a second *speaker*; second
  person is its commonest symptom, not its definition. **`YOURSELF` is deliberately NOT guarded** —
  outside the granted class, and widening past a grant quietly is its own defect.
- **The player-ink check is enumerated** (`Accent`, `BiroDeep`), replacing an `AreNotEqual` that could
  not fail. **Its independence is stated narrowly on purpose:** it cannot fail today either, while the
  assertion above it pins the empty line to `Muted`. What it survives is that assertion being
  *re-ruled*. Independent of the ruling's future, not of today's constants — overstating that would
  repeat the defect it replaced.
- **A negative control that fires**, and it calls the *same* function as the live check rather than
  re-implementing it. It asserts **what** was caught, not that something was: `YOUR MARGIN IS CLEAR`
  → `YOUR`, `YOU HAVE NO MARKS` → `YOU`, `YOU’RE CLEAR` → `YOU`, and `NOTHING ON YOUTH POLICY` →
  `null` so the widening cannot collapse back into a substring match. **Green on the live string alone
  proved only that the guard does not false-positive** — a broken tokeniser would have passed too.

**Do not propose a pixel close for S71.** Batch 24 refused it, and not on cost: the residue is
*does an authored token survive the rendering path*, which is surface-wide, and three layers already
cover it. The reasoning is recorded at the test's scope note.

### Nothing is flagged unverified

**The debt this section used to carry is closed.** `fb8e248`/`4c2d567` had restored the three font
assets out of HEAD rather than regenerating them. The bootstrap has since run
(`tools/tmp-phase-l-bootstrap.ps1`, exit 0, mirror gate reading *`material mirrors 1024x1024 against
configured 1024x1024 — AGREE`* on both faces, faces resolved by style name with no fallback, weight
600 wired at table index 6, shader arm `[DF]`), both suites are green behind it, and the regenerated
pair is committed at `7acb704`.

**What that re-verification found, and it will confuse the next person who diffs a font asset:**
generation **strips** the OpenType layout tables — 0 kerning pairs, 0 mark records, a 68,700-line
deletion — and **first use restores them.** TMP repopulates a dynamic font asset when the fonts first
render and Unity serialises it; measured back at 71 pairs / 142 value records / 4028 mark records
after the suites. **A generated font asset is not stable until something has rendered with it.** I
measured that diff before the suites, committed after them, and described a change that no longer
existed — the commit's own `--stat` caught it. **Measure the diff you are actually committing, at the
moment you commit it.**

### C13 is SOLVED — do not re-open the hunt

**Allen's Game view had *Low Resolution Aspect Ratios* enabled.** Turned off, both surfaces read clear
to his eye. **No build change was needed and none was made.** It resamples at output resolution, which
is why it is frame-wide, hits the laptop and the phone equally, survives 1.5× supersampling, and no
material property could reach it — and why the harness could never clear itself: *it was never in the
game at all.*

Read §4bb-C13 for the trail, but read it as history. **What survives and must not be undone:**

- **The `_TextureWidth` mirror fix (`6bd6da2`) was a real defect and stays** — exonerated as the
  *cause*, never as a defect, and now gated by the bootstrap's mirror line.
- **The `_Sharpness` margin stays unspent.** Maxed it buys 9.6%, not 50%. Spending it converts a
  diagnosable defect into a marginal one that still fails and no longer points anywhere.
- **A TMP SDF ramp cannot be narrowed by URP render scale.** `TMP_SDF.shader:183-186` normalises
  against `_ScreenParams`; URP binds that from `camera.pixelWidth` (display) and keeps
  `_ScaledScreenParams` separate. Hand that to anyone who proposes supersampling to sharpen text here.
- **Two instrument laws, adopted in batch 19:** *a control must be able to witness the failure it
  guards* (the ink ring is soft by design and could witness nothing), and *a null is invalid if
  success would sit under the instrument's own floor* (the `_Sharpness` arms predicted 0.84px against
  a ~1px floor). **Two verdicts were un-retired on the second one.** A third is proposed in the joint
  note's Part 3: *a search space is a claim, and it needs a control like any other.*

The joint read is at `main-2/docs/design/dd-import/blur-pipeline-read-2026-08-08.md` — Part 1 mine,
Part 2 room's, Part 3 the closing.

### Open items

1. **No SureThing BUILD item is open.** Two things are in flight that are not builds:
   - **S72 — the empty-state voice inventory — came back CLEAN and is with the DD**, filed at
     `main-2/docs/design/dd-import/surething-s72-empty-state-voice-inventory-2026-08-09.md`
     (untracked, rides the drag). **Eleven members named**, every one naming the state, and **zero
     second-person pronouns exist in live code anywhere on this surface** — the only `YOU/YOUR` hit
     in the whole runtime is inside the S71 comment recording the string that was removed. Two
     observations were raised and NOT ruled: **first person appears four times** (`MY MARKS`,
     `MY BETS`, that screen's scope line, and a status line naming it) against §6's "exactly once",
     three of them being one destination's proper name; and **the kit pairs a statement with an
     imperative while three of six statements have no pair**, S71's own slot among them.
   - **The sweep's own method is the reusable part:** no single pass was complete. The structural
     pass over emptiness branches missed three members (the MY BETS mirror gates on
     `!view.HasTicket`, not a count); the absence-copy string pass missed the zero-ticket lock
     reason; the `*Empty*`/`*Waiting*` naming pass caught the last. **Any one alone would have
     reported a clean sweep that was not one.**
2. **⚠ NEW AND UNROUTED — batch 25's `S2-am3` puts a measurement on this surface.** A legibility
   claim about *this* surface's smallest type (season records `6-3`/`4-5`, row numbers `01`–`06`) was
   **returned unadjudicated**: it had been read on Allen's `surething-form-blurry.png`, which is **the
   complaint frame**, shot before the Game-view toggle fix and measured at **0.613** ramp÷stroke
   against the harness's **0.482**. A confounded measurement closes nothing.
   **The sharpened ask, and it is arithmetic rather than opinion:** 0.482 was taken on **price
   figures**, the ramp is fixed in screen pixels while stroke scales with type size, **so the smallest
   type is worse than 0.482 by construction** — the hunt's headline number was measured on type larger
   than the floor it is now used to reason about. What is wanted is **ramp ÷ stroke on the smallest
   product fact on the surface**. The DD stated the bias direction *before* the re-read: Allen's path
   ran ~27% worse, so those elements likely read **better** than the returned claim said.
   **Whose it is has not been settled** — the measurement is on this surface, the harness is room's.
   **Do not assume it is this seat's and do not assume it is not.** Raise it.
3. **Owed by others, not me:** whether design evidence lives in git long-term (Allen, unhurried —
   405 files under `artifacts/` are tracked on main, of which 356 are PNGs totalling ~1.05 GB of
   **raw** blobs; none are LFS pointers and `.gitattributes` routes neither `artifacts/` nor `*.png`
   to LFS). Main also carries `SENTIS_ANALYTICS_ENABLED` **committed** — main's to resolve.
4. **Routed away, do not pick up:** M-04/M-05 stake block → markets. The ledger's `OPEN` status →
   engine-contract list. **B-01's ticket axis → needs a real ruling**; it has been ruled around four
   times without being ruled on, and ratified-by-silence is not a ruling.
5. **S67's residue is still open and was deliberately not tidied.** Batch 21 ruled the *string* —
   `PRICES FINAL` left the masthead subline, so `SportsbookApp.cs:98` and the LEDGER's `Scope` now
   print identically. **The two sites still differ in NAME (`Run` vs `Scope`) and both still build the
   string inline rather than through a `LaptopUi` helper.** That is S67's actual finding; the ruling
   did not touch it and neither should a passing tidy.
6. **The capture-state collision resolved as `11-desktop` stays, markets moved to 16.** Markets' tree
   holds the only frames of `16-margin-max-legs-staged-receipt` — **this tree cannot regenerate them**,
   because it still carries `11-`.

### The standing traps

*(Not "the three" any more. The count was in this heading and went stale the moment a fourth arrived —
the same defect as the `(gitignored)` line below, in the heading rather than the body.)*

- **⚠ DESTRUCTIVE AND LIVE — do not `git add -A`, do not `commit -a`, do not touch the TV fonts.**
  `unity/SBR/Assets/SBR/Resources/Tv/Fonts/EncodeSans.ttf` and `EncodeSansCondensed.ttf` show
  **permanently modified in every worktree on main** and **`git checkout` cannot clear them.**
  Verified here rather than relayed: HEAD's blob is **raw TTF** (285,792 bytes, real `GDEF`/`GPOS`
  tables), `.gitattributes` routes the path to LFS (`git check-attr filter` → `lfs`), and
  **`git lfs ls-files` has no record of them.** So the clean filter would write a pointer to an LFS
  object **that does not exist** — staging them converts real fonts into **dangling pointers**, and
  the bytes stop being reachable from that commit.
  **Stage by explicit path only until TV's conversion fix lands on main.** It is TV's to fix; the
  remedy (`git lfs migrate` versus untracking the path) is repo-wide and has history implications.
  **It also breaks `git merge --abort`:** the stale index throws *"Entry … not uptodate. Cannot
  merge. Could not reset index file to revision HEAD"*. `git reset --hard HEAD` clears it, and costs
  nothing **provided this seat's own work is committed first** — which is the reason to commit before
  merging rather than after.
- **`artifacts/` is NOT git-ignored.** A bare `git add -A` sweeps ~100 PNGs. Stage explicitly, always.
  Verify rather than trust: `git check-ignore -v artifacts/surething-ui` matches nothing.
  **This file told two seats the opposite** — §5 read "(gitignored)" until 2026-08-09, cancelling the
  warning it gives here. Corrected at `47d4d51`, and found only by consequence: **368 stray frames had
  accumulated across all four other worktrees** because other lanes run this surface's capture suite.
  Cleared. Four preserved in markets' tree on purpose (item 5 above).
- **Ask the orchestrator for the editor every time**, including batch probes. Contention does not look
  like contention, it looks like a flaky run. Verify zero yourself before starting — `Get-Process
  Unity` — and again on release; on 2026-08-09 another lane's editor opened *during* this seat's
  PlayMode run and the suite passed anyway, which is luck rather than design.
- **`ProjectSettings.asset` and the Sentis define — the trap has changed shape.** Unity adds
  `SENTIS_ANALYTICS_ENABLED` on open and normalises it away again on exit, so the working copy churns
  in both directions. **Main now carries the define COMMITTED** (`:839`). Under the settings-churn
  charter nobody commits the churn: cmp-verify, then `git checkout` the file. **Do not strip main's
  committed value through a merge** — that silently flips a scripting define for every lane and
  diverges on a line the next merge re-fights.

### Evidence lives in two places, and only one of them is disposable

- **`artifacts/surething-ui/`** is working output. Regenerable by construction (C34 pins every seed),
  and safe to clear. It is not evidence, and nothing in the register cites it.
- **`docs/design/dd-import/surething-*/`** are the granted bundles — batch 10's sixteen-state set, the
  ledger resubmit, S6–S8, the S8 refold, the verdict ground. **87 files, tracked on main, checked out
  in every worktree that merges main. Leave them alone**; deleting them from another lane's tree is a
  content change to committed state, not a cleanup.

---

- **PHASE L IS GRANTED AND ON MAIN.** The whole laptop text stack is TextMeshPro. **C15, S20, S28 and S29 are CLOSED and every type deviation has expired** (batch 15). The migration was verified 1:1 — five product-fact slots measured identical ink heights at identical scanlines through a complete text-stack replacement. **S8 and S52 re-verified on it.**
- **Batches 16–17 are closed too.** S68 (kit trackings — the SKIP headroom recovers by construction), S69 (disabled grounds un-inverted), S70 (three untokened kit values, receipt header split, height re-derived) and S71 (`NO MARKS ON THIS SHEET`) all granted.
- **THE THREE PARKED QUESTIONS ARE RATIFIED AS BUILT — do not "fix" any of them.** Footer stays 13px (**the receipt is index, not display**); the wax highlight stays over the value (**it marks the slot, not the amount**); the disabled PLACE fill is **explicitly not to be deepened** — the other two channels carry that distinction.
- **The C14 audit is now a bounded list, not an open document** — `dd-import/dd-followup-surething-c14-audit-sweep.md`. **S71 was M-11 in that audit**, rediscovered from a frame months after it was named, which is why the sweep exists. Four rows still live; **M-04/M-05 (the stake block) routed to markets' seat**; the ledger's `OPEN` status is on the engine-contract list; **B-01's ticket axis is filed as needing a real ruling — it has been ruled around four times without being ruled on, and ratified-by-silence is not a ruling.**
- **EditMode is 78, not 76** — the two S29 digit-spread assertions. **PlayMode is 58 since `af0c42c`** (the S71 gate), not the 57 this line claimed until 2026-08-09. **Baseline any later comparison against `78/58`**, which the re-seat block at the top states as the live figure — this bullet is kept only to record that the 76 → 78 step was S29's.
- **⚠ THE ROMAN VOICE WAS AT WEIGHT 600 FOR MONTHS AND NOBODY CHOSE IT.** `Archivo.ttf`'s **default face is SemiBold**; Regular is a named instance. UGUI loaded the default under S11 and my first TMP generator did the same. Ruled Regular 400 (Allen, 2026-08-08). **Faces now resolve by STYLE NAME and the generator refuses to fall back to the default face** — falling back is exactly how this happened, twice. See §4bb.
- **The surface had tabular digits by ACCIDENT.** SemiBold is near-tabular (spread 0.1875); the ruled Regular is proportional (**4.7656 units**). Correcting the weight removed a guarantee nobody knew was load-bearing, which is what opened S29 — see §4bb.
- **C34 is tested, not asserted — and the residual is named.** `01-form-lobby`, pinned since `102a571`, re-shot in a separate Unity process: **alpha 0 differing; RGB 440 of 720,896 (0.061%), max delta 2/255.** Content is fully reproducible. The 440 sit on **exactly two rows, y=581 and y=607** — 26px apart, inside the margin: two of **S34's ruled-paper rule lines**, a 1px untextured line rasterising with different subpixel coverage between processes. **Not seed-dependent, not content.** It is why two runs' PNGs are never byte-identical; do not chase it as a regression.
- **BATCH 14 GRANTED the riding and pending captures and CLOSED R38.** Built since, all unverified: **S66** (every flow pinned *and asserted*), **S65** (PENDING → `--toner-3`), **S64** (mirror routes through `TicketIdentity`). **S67** filed as an inventory at `dd-import/dd-followup-surething-s67-helper-bypass-inventory.md` — **inventory only, nothing in it is fixed.**
- **C34 is law and it is this surface's:** evidence that cannot be reproduced is not a set. Every capture flow pins its seed and **asserts the run is carrying it before shooting — an unasserted pin is a comment.** The fifth instrument axis, beside C25 scope, C28 coverage, C32 resolution, C33 unit.
- **R38 is a SureThing item despite its `R` prefix.** Batch 14 recorded the mis-prefix as a seat error and ruled it stays — re-keying delivered work costs more than it saves. The room's next items took R39/R40. **Do not "correct" it.**
- **THIS SURFACE HAS AN OWNING DOCUMENT.** Approved by Allen, canon at `main-2/docs/design/surething-design.md`. **Read it before this file.** The LEDGER is Design-verified (batch 10); the zero-dollar wax/grey split is **ratified as considered — never "fix" it.**
- **Done:** batch 10, **all of batch 11 — and batch 13 GRANTED all four of them on measured frames** (S59 drain, S60 biro header, S61 scope once, S62 identity). Since: **R38** (forced capture states take a numeric run seed), **`04a-my-bets-riding`** and **`04b-my-bets-pending`**. **The surface is now at zero unphotographed states** — every ruled treatment on it has a frame.
- **Now:** nothing in flight. Phase L merged; the lane waits on the DD's grant. **Phase T (the TV's own TMP migration) is next in the plan and is NOT this seat's.**
- **A capture state number collides: `11-desktop` and `11-margin-max-legs-staged-receipt`.** Merge-introduced and neither side could see it — main renumbered markets' state from `09-` to `11-` to clear the old 09/09 collision, correctly, because on main `11` is free. **`11-desktop` must not move:** it is cited by name in the register (S47, S56) and owning doc §5.2 as the frame two measurements were taken on. **16 is free.** Routed to markets' seat; no filenames collide, so nothing breaks — it misleads anyone reading the set in order, exactly as 09/09 did.
- **THE SET WAS NEVER REPRODUCIBLE, AND IS NOW ONLY HALF FIXED.** `RunDirector.seed` ships **blank** in the scene; the director reads that as "roll a fresh random 8-char seed", so every `Boot()` flow dealt a **different slate every run**. Measured: batch 11's `05` reads `Tulsa Plumbers v Pawtucket Ferrets −516 · PAYS $71`, the next run reads `Sheboygan Bricklayers v Waterloo Zambonis −410 · PAYS $85`. **All six shooting flows now pin a numeric seed** through one `PinRun` helper that proves the director is carrying it before a frame is shot; `AssertPinnedSeed` states the rule once. **Every seed literal appears exactly once, in the constant block at the top of the file** — that is S62's standard applied to seeds, and `grep '"[0-9]\{8\}"'` is the check. **One unpinned seed is left: `15-ledger-across-rounds` carries the label `"ledger-across-rounds"`** (R38's own defect class, one line, out of the four-flow scope). **Never compare a frame across runs shot before `0200efa`** — that is not a regression, that is a different slate.
- **Emphasis on this surface is not one scalar** — now **law C33b**, promoted from S59's build. Wax outranks toner by *chroma*; toner-2 outranks toner-3 by *value*. **Assert a ranking by weight only among neutrals, and by token otherwise.**
- **Request the editor slot through the orchestrator, every time** — corrected 2026-08-06; the standing grant in `STATUS.md` is stale and this seat's runs collided with a validation pass because of it.
- **Run every suite through `tools/run-unity-tests.ps1`, never `-runTests` directly** (C29). It exited non-zero on both collided runs rather than reporting them as passes, which is the whole reason the collision cost two re-runs and nothing else.
- **Measure at full resolution and on all four channels, or do not say "identical".** My first reproducibility pass reported **zero** differing pixels — it sampled every other row from zero and compared RGB only, and the two rows that differ are **both odd**. It could not have seen them, by construction rather than by luck. I nearly reported the set byte-identical on it.
- **S65 and S64 are built and verified, and both carry a trap for whoever touches them next.** S65: PENDING is `--toner-3` **and VOID is still `--toner-2`** — they shared an `else`, and moving that fallthrough would have fixed one by breaking the other. The mapping now lives in `SportsbookApp.LegStateInk` with a contract test over all five states. **PENDING and DEAD are deliberately the same tone**, so the oxide strike is load-bearing, not decorative. S64: all three identity sites route through `TicketIdentity`; the mirror passes round `0` because `withRound: false` never reads it — **anything flipping that to `true` must supply a real round first.**
- **The four `LaptopUi`-bypass items S67 found are NOT fixed and must not be tidied in passing** — the inventory is the deliverable and the highest-value one (the `ROUND n OF N` scope line, built inline at `SportsbookApp.cs:97` and `:2097`) **needs a ruling, not a refactor**: the two sites already disagree about whether `· PRICES FINAL` belongs, and S37 governs.
- **The kit is at `docs/design/design-system/`, not `ui_kits/`** — those paths are relative to that root. **Read it before calling anything unspecified**; this surface has now twice concluded a screen was unspecced when it was not.
- **Two traps still live in this tree:** `artifacts/` is no longer git-ignored (a bare `git add -A` sweeps ~100 PNGs), and two capture states share the number `09` (markets' test, needs their nod).

**Written:** 2026-08-01, at a session hygiene clear. **Last updated:** 2026-08-09, after C13 closed, the
font-asset re-verification, batch 21, the strays collection, S71's gate rebuild and S72's clean sweep.
**Converged at:** `1d6eb35`, identical to main · **Branch:** `surething-ui` · this entry is the only
commit beyond the convergence. **The working tree is NOT clean and cannot be made clean:** two TV font
files are phantom-modified under a broken LFS attribute and `checkout` will not clear them. That is
expected, it is not this seat's, and staging them destroys them — see the first standing trap.

**Read the re-seat block at the top of this file first.** Everything below it is the accumulated
record and some of it describes states that have since closed — where that is true the section says
so at its own head, rather than being deleted, because the reasoning is usually worth more than the
conclusion. The top block is the only part that claims to be current.

This is written for a seat with **no conversation context**. Everything below is either verifiable in
the repo or flagged as unverified.

---

## 1. Where the work stands

The SureThing laptop's first slice **merged to main** at `2e97d13` (2026-07-31). S6 (lobby shell),
S7 (ink sprites) and S8 (OS chrome) are **DESIGN-VERIFIED** by the Design Director — the laptop's
first. Changes to those three are regressions, not iteration.

**S8 went out and came back.** S48 folded the desktop into that chrome, which by S48's own terms
returned S8 to review; **S52 (batch 9, 2026-08-04) re-verified it** on the pixel-identity evidence,
with one required change (the icon margin, §4-0.6). So the sentence above is true again — but the
round trip is the point: a Design-verified item is verified against a *configuration*, and folding a
new surface into it re-opens it. Expect the same if the chrome is consumed anywhere else.

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

**Suites: EditMode 76/76, PlayMode 56/56** (at `a235bfc`), and every run goes through `tools/run-unity-tests.ps1` (C29). The 75/38 in earlier notes was the
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

### How retention actually reaches this tree, and the trap in the last step

**Measured 2026-08-03, not assumed.** `9e55d0d` exists as an object here but lives on **`markets-2`
only** — `git branch -a --contains 9e55d0d` returns that one branch, and it is **not** an ancestor
of `main`. The route the orchestrator confirmed on 2026-08-03: **markets B1 merges to main, then
this branch merges main.** B1 is one DD ruling away. There is no shortcut worth taking — do not
cherry-pick `9e55d0d` onto this branch to unblock S41 early; settlement is the markets seat's and
this seat consumes the result.

**The trap is the merge itself, not the wait.** `git rev-list --left-right --count main...HEAD`
reads **159 / 19** — main is 159 commits ahead of this branch's fork point, from three other seats.
So the merge that unblocks S41 also lands a large amount of work this surface has never been
photographed against.

Consequences, in the order they will bite:

1. **Re-shoot the twelve-state set *after* the merge, never before.** The grant is made on that set
   (§4-0) and a set shot on the pre-merge tree describes a build that no longer exists. This surface
   has already produced two findings that a later capture dissolved (§4a); shooting against a stale
   tree is the same error committed deliberately.
2. **Re-run the rail comparison from §4-0.5** — the desktop-versus-in-app pixel identity — as the
   first check after merging. It is the cheapest detector the surface has for shared chrome
   drifting, and 159 commits is exactly the circumstance it exists for.
3. **Expect the suite counts to move for reasons that are not this seat's**, and establish the
   post-merge baseline before reading any failure as a regression here.

### The merge, forecast — measured 2026-08-05, before touching anything

**Retention is on main.** `bbf9241` and `9e55d0d` are both ancestors of `main`. The distance is now
**221 / 23** (main ahead / this branch ahead) — it was 159 when first measured, so re-measure rather
than trusting either number.

Reproduce this forecast without touching the branch:

```
git merge-tree --write-tree --name-only main HEAD    # writes nothing; exit 1 means conflicts
TREE=$(git merge-tree --write-tree main HEAD | head -1)
git show "$TREE:unity/SBR/Assets/SBR/Runtime/SportsbookApp.cs"   # the conflicted file, markers and all
```

**Exactly one file conflicts: `SportsbookApp.cs`, three hunks.** `LaptopOs.cs` is not touched by main
at all, so **the entire desktop block merges clean** — S44 through S48 and S52 are not at risk.

**What the conflict actually is.** Both branches implemented interior market-list scrolling
independently. Main's side is 632/178 since the fork and is **ahead on that surface** — it cites T47,
S28, S22, S23 and A2–A5, and carries S27 seven times. This branch's side is 272/117 and is ahead
**on the ledger** (S40, S43) and carries S46's masthead. All three conflict hunks sit in the
market/detail region — `BuildMarketLines`, `BuildBothTeamsScore`, `BuildPlayerLines` and their scroll
plumbing, where main took `Run run` parameters and `MakeOfferRow` while this branch returns content
heights and takes a `title`.

**So resolve all three toward main**, then confirm this branch's side survived outside them. The dry
run says it does: the S46 brand string is present once, and S40/S43 nine times. **Confirm, do not
assume** — that is a prediction from a tree nobody has compiled.

**Two files auto-merge that both sides changed** — `SureThingLedgerTests.cs` and
`SureThingVisualCaptureTests.cs`. Git reconciling them textually is not the same as them being
right; read both before trusting the green.

**A numbering collision to raise before the re-submit, and it is markets', not this seat's.** Main
added a capture state numbered `09-margin-max-legs-staged-receipt` beside the existing
`09-rewards-affordable`. After the merge the set reads 01–08, **09, 09**, 10, 11, 12, 13, 14. No
filenames collide, so nothing fails — but the re-submit is precisely a read-the-set-in-order
exercise, and two states share a number. Renumbering touches the markets seat's test, so it wants
their nod rather than a unilateral fix here.

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

## 4-0.6 Batch 9 (S52) — landed. One thing is still with the DD

**Done** (`eca7f36`, `d1a8382`). Suites **EditMode 76/76, PlayMode 47/47**. S8 re-verified on the
pixel-identity evidence from §4-0.5.

- **Icon column moved to `--st-pad-x` (14px) below the rail**, from 86px. That space was the
  wordmark's and S44 deleted the wordmark out from under it. **"The standard margin" is not a
  token** — the phrase appears twice in the corpus, both inside S52, and nothing defines it; I swept
  both repos before asking, and Allen chose `--st-pad-x` over the column's own 34px on 2026-08-04.
  It is a decision recorded in source, not a lookup, and the constant is `LaptopOs.DesktopIconMarginY`.
- **The verdict screen is off oxide and off biro.** `THE BOOKIE COLLECTS` is `--toner-3` (a loss is
  carried by value, as the ledger's record row carries it); `NEW RUN` is a wax primary through
  `MakeWaxPrimary`. Read narrowly: **no oxide strike on the headline** — the ledger pairs toner-3
  with a strike, but that strike marks a dead *record row*. One `MakeRule` call if the DD wants it.

**Still open, with the DD: the verdict ground.** Deferred pending frames; the frames now exist
(states 13 and 14, staged at `dd-import/surething-verdict-ground-2026-08-04/`).

### Two things from this batch worth carrying, both about measurement

1. **The verdict ground is not what its source says.** Authored `new Color(.03f, .02f, .06f, 1f)`;
   measured on frame 13, the ground is **R≈13, G=0, B≈13 — green at exactly zero on 2449 of 2449
   samples**, and darker than `--ink` (22,22,15). That is magenta at near-black, and it corresponds
   to the authored token under neither a linear nor a gamma reading (linear would give ~51,42,69),
   while `Color32`-authored grounds elsewhere render ~1:1. **Do not fix this by editing the token
   until someone has traced why.** I reported it and changed nothing. Note also I first described
   this ground as "blue-tinted" from source — the measurement corrected me, which is the whole
   argument for capturing before characterising.
2. **28px of any rail-to-icon measurement on the desktop is the chip's dead space.** S52 recorded a
   114px gap where I had reported 86. Both were right: 86 is rail-to-tile, 114 is rail-to-ink. They
   diverge because the `--ground-3` chip is a **3/255 step** against the wallpaper there (34,34,22 on
   31,31,19), so the tile edge is invisible and the eye lands on the glyph. The DD ruled
   "chip/ground-3 fine" and that stands — but at the top of the wallpaper gradient the chip does not
   read as a chip, and I have flagged that in the drop rather than acted on it.

**A capture-fixture technique this surface did not have before:** terminal run states are forced
through the payment schedule, not played. `RunConfig.Rounds` is `Payments.Length`, so a one-element
schedule makes round 1 the final round, and a payment the 350 bank can or cannot meet decides the
ending — no RNG, no eight-round grind, no lucky seed. The run is swapped onto the director by
reflection against its private setter, deliberately, because the alternative is a seam on
`RunDirector`, which three seats share and which is about to take that 159-commit merge. Reuse this
for any other state the engine makes expensive to reach.

## 4-0.7 The merge and S41 — the slice's last items

**Done** (merge `5f749a0`, S41 `f05332c`). Post-merge baseline **EditMode 76/76, PlayMode 55/55** —
establish any later comparison against this, not against the 47 that preceded it. The union is 55
because 8 tests were unique to each side.

### The merge

221 commits, one conflicted file: `SportsbookApp.cs`, three hunks, all in the market/detail region.
**Resolved toward main**, which is keeping both intents rather than overriding one — both branches
had independently built interior market-list scrolling, and main's side is ahead there (T47, S28,
S22, S23, A2–A5). **A2 in particular deletes the per-destination panel titles this branch was still
passing**, so taking this branch's side would have reinstated copy a later ruling removed. This
branch's own contribution to that file — the ledger (S38–S43, S37, S31) and S46's masthead — is in
regions main never touches, so nothing had to be traded. `LaptopOs.cs` is untouched by main, so the
whole desktop block merged clean.

Two things the auto-merge got textually right and semantically wrong, both fixed:

1. Two `return MarketRowsContentHeight(...)` statements from this branch were grafted onto main's
   `void` method bodies.
2. **`SureThingEntryTests` hardcoded a 280px receipt width while the renderer derives it**, and E-07
   has since moved staged receipts from the 324px margin into the 700px sheet. It reads the width off
   the rendered header now — which its own comment already claimed it did.

**That second one is a pre-existing latent flake on main, not a merge regression, and the shape is
worth remembering.** The test and the renderer only disagree when a label's fitted width falls
BETWEEN the two numbers: shorter than 280 and both return the string untouched, longer than the real
width and both truncate identically. Only the band between them fails. It passed on every run whose
generated team names were short enough and failed on the first one that produced "REGULATORS
MONEYLINE — v SPREADSHEETS". **Main will hit it the same way on a long enough slate.**

### S41, and the thing it was really for

The figure prints in wax, the RETURNED total is a sum and never an em dash, and an unknowable record
would leave the absence in its own cell rather than blanking the total. Checkable on frame 12: `$8`
cashed out, `$29` won, `$0` lost, total `$37`.

**The ledger also now reads retained history.** It read `run.Tickets`, which `ExitShop` clears every
round, so a player who bet in rounds 1–3 and opened it in round 4 met an empty screen captioned
`SETTLED TICKETS · THIS RUN`. That was the defect retention was approved to fix — it is item 1 of
§4b's two consequences, and it is now done. It unions the retained list with the current round,
de-duplicated by reference, so a ticket that goes terminal mid-round does not vanish until the round
settles.

**The gap in the evidence, stated plainly: every ledger frame in the set is ROUND 1.** Retention
across rounds is proven by construction and by the suite and by nothing photographic. That capture
is worth building and does not exist.

### Two traps the merge introduced

- **`artifacts/` is no longer git-ignored.** Main un-ignored it deliberately (2026-07-28, to stop
  design evidence being silently swallowed), so a bare `git add -A` in this tree now sweeps in every
  capture PNG. Stage explicitly.
- **Two capture states share the number `09`** — `09-rewards-affordable` and
  `09-margin-max-legs-staged-receipt`. Nothing collides on disk; the numbering just lies to anyone
  reading the set in order. The second is the markets seat's test, so renumbering wants their nod.

## 4-0.8 Batch 10 — the LEDGER is granted, and everything else in it is built

**Done.** Suites **EditMode 76/76, PlayMode 56/56**, every run through the C29 wrapper.

**The LEDGER is Design-verified** on the sixteen-state set. **The zero-dollar wax/grey split is
ratified as considered — never "fix" it.** C31 also landed as law: a named closing-condition set is
exhaustive, so findings on the same frames open new items and do not retroactively withhold a grant.

### C29 — do this before anything, and it is not this surface's alone

`tools/run-unity-tests.ps1`. **Never call Unity's `-runTests` directly again.** A filter matching
nothing makes Unity exit green with `testcasecount=0` — a run that did nothing, reported as a pass,
and unlike the four vacuous gates before it this is the *runner*, so one typo can green any suite in
any seat. The wrapper reports the executed count on every path and exits non-zero on zero cases.

It paid for itself three times in one batch: the proof run, a Unity boot crash that wrote no results,
and a filtered diagnostic. **The boot crash is worth knowing on its own** — Unity died four seconds
in, wrote nothing, and without the wrapper that is indistinguishable from a pass.

### What changed on the surface

- **The verdict screen** is `--ground` (measured 21.5, 21.5, 12.7 against the token's 22, 22, 15)
  and renders inside the rail and tray like every other destination. It was a full-screen takeover
  with the OS deleted, which is a game-over card, and the chrome is the argument rather than
  decoration.
- **S57 answered: capture data.** The verdict derives from bank-versus-payment, and **the engine
  does not deduct a payment the bank cannot meet** — so a forced loss kept its whole bank. Figures
  are now chosen to read: win $290, bust $40 against a $155 payment.
- **The desktop chip is gone** and dead apps print `NOT INSTALLED`. The chip was a 3/255 step;
  the word is 85. Pitch 105 → 126 to fit it.
- **The MY BETS tally is run context**, not a second copy of the sheet. `MarginRow` was promoted to
  `LaptopUi.MakeMarginRow` so both margins draw it from one place.

### Two findings the DD should see, neither fixed here

1. **`LaptopUi.FromRgb` is dead** — no call sites; the live `FromRgb` calls resolve to
   `TheaterStage`'s. Recommended for deletion; left because the ruling asked for a report.
2. **`attentionEmission` = (0.28, 0.10, 0.55)** — a saturated violet on the laptop lid, in a project
   that retired purple. It is a **serialized** field, so the scene ships and source is only a
   fallback; the scene agrees for the laptop. Room lighting rather than the document, so not mine.

### The retention capture, and what it cost

`15-ledger-across-rounds` shows **ROUND 2 OF 8** with `TICKET 1.0` beside `TICKET 2.0`. The gate is
that the board renders **more rows than `run.Tickets` holds**, which is only possible if it reads
retention. Two traps met building it, both now guarded in `SettleOneRound`:

- **The run kept ending.** The shipped schedule against a 350 bank busts inside two rounds of real
  betting. Only the bank is rigged now (5000), never the schedule — an earlier cut used `{1,1,1,…}`
  and printed `TARGET $1`, which is exactly the arbitrariness S57 rules against.
- **Navigation was silently undone.** The sweat loop exits the instant the *engine* leaves Sweat,
  frames before `LaptopOs` runs `ApplyPhaseDefault` — and that default sets `_activeApp` itself. Any
  navigation in that window looks like it worked and is overwritten on the next tick. **If a fixture
  navigates right after a settle and lands somewhere unexpected, this is why.**

## 4-0.9 Batch 11 — four items, and one lesson worth more than the four

**Done** (`6439059`, `5894f53`, `89f4963`, `a235bfc`). **EditMode 76/76, PlayMode 56/56.**

Also this batch: **the surface's owning document was approved** and is canon at
`main-2/docs/design/surething-design.md`. This surface now has what only the room had. Read that
before this file — this one is re-seat state, that one is the surface.

- **S60** — the MY BETS margin header renders biro over a 2px `--biro-deep` rule, measured
  96,136,186 against the ledger's identical 96,136,186. Both margins now draw one shared
  `LaptopUi.MakeMarginHeader`; S60 caught them as two renderings of one component in a single
  submission, and leaving two copies would have been the third drift of this kind on this surface.
- **S61** — the screen stated its scope four times; it states it once. `TV-OWNED TALLY` → `TALLY`,
  margin subline deleted. **The shape is the lesson:** S58 asked this column to stop restating the
  *sheet* and it did — then restated the *scope* instead. A restatement removed from one register
  reappears in another.
- **S62** — `R2 · TICKET 02`. **The engine is deliberately untouched:** `Ticket.Id` is the DeriveRng
  key component, so reformatting it would change what the game rolls. The key is read and
  translated, never printed. The round qualifier prints on the LEDGER (its list spans rounds, and it
  comes from the *ticket's* round) and not on a staged receipt (always the current round, whose
  masthead already says so — printing it there would be S37 restatement).
- **S59** — the losing verdict drains as a group: headline `--toner-2`, subline `--toner-3`, both
  measured. `NEW RUN` stays full wax.

### The lesson, and it generalises past this surface

Building S59's gate, the obvious assertion — *the headline outranks its subline* — **fails on the
winning screen.** Wax (`D9A441`) measures 0.66 Rec.709 luminance against toner (`D9D4C5`) at 0.83.

**Emphasis on this surface is not one scalar.** Wax outranks toner by *chroma*; toner-2 outranks
toner-3 by *value*. The losing screen is the one where both elements are neutral and value alone
does the ranking — which is exactly why the inversion happened there and nowhere else.

So: rankings are asserted by weight only among neutrals, and by token otherwise. And the general
form, which is why S59 existed at all — **a per-element value check cannot see a ranking.** S53 was
correct element-by-element and produced an inverted composition.

**This is now law C33b**, promoted from this build. It binds past this surface.

## 4-0.10 Batch 13 granted. R38 and the riding capture — and the finding they turned up

**Batch 13 granted all four batch-11 items on measured frames.** Nothing from them is outstanding.

**Done since** (`6ece398`). **EditMode 76/76, PlayMode 56/56**, both through the C29 wrapper.

### R38 — the rig was printing its own name on a photographed frame

`LaptopOs.RenderVerdict` prints `FINAL BANK $x · SEED {run.Rng.RunSeed}`. The rig seeded its two
forced runs `verdict-RunWon` / `verdict-RunLost`, so frames 13 and 14 photographed **the capture
apparatus's own label for the state** in the one slot on that screen where a product fact belongs.

**The runtime was never wrong** — it prints whatever seed the run carries. The fix is in the rig,
which is where the defect lives. Seeds are now `40719355` / `68204137`: all-digit, 8 characters,
ordinary members of `RunDirector.NewSeed`'s `A-Z0-9` space, so each reads as a run the player could
have been dealt. **Numeric rather than merely seed-shaped is the point** — T31 is the precedent one
surface over, where a harness seed shaped like a label (`TVCAPTURE01`) was read as a debug token and
cost the DD a withdrawn finding.

Two guards added, because **nothing was watching that line**: the seed must be numeric, and the
subline must actually print it. The suite read the headline's colour through two rounds of review
and never read the line underneath it. That is the C18 family again — a value with nothing able to
tell two cases apart.

**`15-ledger-across-rounds` is also a forced state with a label seed, and is deliberately left
alone.** The seed renders on the verdict screen and nowhere else, and **leg count is a function of
the seed** (`DemoTicketPolicy.cs:37`) — so re-seeding it re-rolls the content of an already-granted
frame. Raised with the DD rather than changed; recommendation on the record is to leave it. **Do not
"finish the job" here without asking** — it costs a re-shoot and a re-grant of 15 for nothing visible.

### `04a-my-bets-riding` — and the two things one frame closed

RIDING is the **ticket-level** word (S23 makes that split contractual: RIDING never on a leg, LIVE
never on a ticket). Every MY BETS capture in the set shot a ticket whose legs had **both** already
resolved, so the word a ticket wears for its whole life until settlement had never been in a frame.
Frame reads `TICKET 1 · RIDING`, leg 1 `GREEN` with its ring re-inked in wax, leg 2 `LIVE`.

**It also closes the tally item this file has carried since batch 10.** `AT RISK` and
`IF EVERYTHING LANDS` sum over RIDING tickets **only**, so a resolved round correctly reads
`0 RIDING · $0 · $0` — the true answer to "what is still live" and also a photograph of the column
doing nothing. This frame reads `1 RIDING · $35` and `$85`, and is the first to show the ratified
AT-RISK-toner / IF-EVERYTHING-LANDS-wax split (owning doc §3.1) on non-zero figures.

**Shot one reveal BEFORE 05, on the same ticket and run — 05 does not move**, and its content is
identical to the granted frame. It takes a **letter** (02b's precedent) rather than renumbering a
set the register, the owning doc and this file all cite by name.

### The finding: the MY BETS mirror never got S62

**Three sites print a ticket identity. S62 reached two.**

| Site | Source | Prints |
|---|---|---|
| LEDGER | `SportsbookApp.cs:2186` — `LaptopUi.TicketIdentity(…, withRound: true)` | `R2 · TICKET 02` |
| Staged receipt | `SportsbookApp.cs:1098` — `LaptopUi.TicketIdentity(…, withRound: false)` | `TICKET 02` |
| **MY BETS mirror** | `SportsbookApp.cs:1316` — **hand-built string** | **`TICKET 1`** |

**The kit settles it, and it needs no ruling — only a slot.** `ui_kits/surething/app.jsx:43` builds
`"TICKET " + String(t.length + 1).padStart(2, "0")`; `TicketReceipt.d.ts:12` gives the form as
`"TICKET 01"`; and `TicketReceipt.prompt.md` names the screens that component serves — ENTRY, **MY
BETS during the sweat**, and the Ledger. `LaptopUi.TicketIdentity` already ends
`(placement + 1).ToString("00")`. **The mirror is the one site that does not call it.** The fix is
line 1316 through `TicketIdentity(…, withRound: false)` — the staged receipt's exact call, for the
staged receipt's exact reason (MY BETS is the current round, whose masthead already says so; printing
`R1` there would be S37 restatement).

**I first wrote this up as an open question with "two defensible readings", and the kit corrected me
— I had not read it before filing.** Recorded rather than quietly fixed, because **the C14 audit made
this same mistake on this same surface**: it concluded the kit did not spec the LEDGER because there
was no `components/ledger/` directory, and the screen had drifted from a spec that existed. Six days
apart, same error, caught only because filing the question forced me to go and look. **Read the kit
before calling anything unspecified.** It is at `docs/design/design-system/` — the audit's
`ui_kits/...` paths are relative to that root, which is itself part of why it is easy to miss.

**Why it survived, and this is the reusable part:** the S60 shape verbatim — one component, two
renderings — and the **fourth** on this surface (S33, S34, S60, now this). Every time, a second call
site hand-builds what a shared helper already builds correctly, so **a ruling only reaches the call
sites that route through the thing it was ruled on.** Probably worth more than the fix: nobody has
swept this surface for other strings that bypass a `LaptopUi` helper. Named as its own candidate in
the filing rather than buried inside this one.

**Raised, not fixed** — filed in C25 form at
`main-2/docs/design/dd-import/dd-followup-surething-mirror-ticket-identity.md`, untracked, riding the
next drag. Under C31 it is a new item on new frames, not anything against the batch-11 grant.
**Cost when built: `04a` and `05` both re-shoot** (one paired run); no other state carries a mirror
title.

Also stale and trivial: the comment at `SportsbookApp.cs:2081` still calls identity `"TICKET n.n"`.

### Still unphotographed after this

~~PENDING legs~~ — **done**, `04b-my-bets-pending` (§4-0.11). **Nobody has scrolled the surface**:
S27's rail renders on `02b`, but no capture can prove a human can operate it. Still wants thirty
seconds of someone driving the laptop — that is an interaction, not a state.

## 4-0.11 PENDING captured, and the set was never reproducible

**Done** (`102a571`). **EditMode 76/76, PlayMode 56/56** through the C29 wrapper. **The surface is at
zero unphotographed states** — every ruled treatment on it now has a frame.

`04b-my-bets-pending` is the mirror at the instant the broadcast releases the ticket: RIDING, both
legs PENDING, the tally carrying the whole stake because nothing has resolved. With `04a` and `05` it
is **one ticket's entire life on one slate** — `1 RIDING · $35 · $102` twice, then `0 · $0 · $0`.

**Reading order is `04b` → `04a` → `05`, and the letters do not encode it.** Insertion order, because
`04a` was filed before this state was asked for. Renaming delivered evidence is worse than a suffix
that needs a sentence.

### The finding that outranks the capture

**`RunDirector.seed` ships blank in the scene**, which the director reads as *roll a fresh random
8-char A-Z0-9 seed*. Every `Boot()` capture flow has therefore dealt a **different slate on every
run**, and no two submissions of "the same twelve states" have ever been the same frames.

| Run | `05-my-bets-green-dead` |
|---|---|
| batch 11, `a235bfc` | `Tulsa Plumbers v Pawtucket Ferrets` · −516 · **PAYS $71** |
| next run, `6ece398` | `Sheboygan Bricklayers v Waterloo Zambonis` · −410 · **PAYS $85** |

**This has not produced wrong verdicts** — treatment is what gets ruled and treatment is stable
across seeds. It produces a set where a **content-dependent** finding can appear and vanish between
runs with nothing changed. The recorded instance is the `SureThingEntryTests` width flake, which
passed on every run whose team names were short enough (§4-0.7).

**Flow 1 now pins `52830174`** and asserts the run carries it before shooting. **The other four flows
still roll.** Pinning them moves ten more frames' content at once — one flow per commit, and it is
worth doing. The verdict pair is already pinned under R38.

**I got this wrong first.** I told the orchestrator `05`'s content was "identical to the granted
frame" when inserting `04a` before it — asserted from the shape of the change, never checked. The
table above is what checking looks like. **On this surface, claim reproducibility only from two
frames side by side.**

### Second conformance gap — photographed, not fixed

`SportsbookApp.cs:1342` derives leg colour as `Won → MoneyGold · Lost → Muted · Live → White · else →
TonerSecondary`, so **PENDING renders `--toner-2`** in the same branch as VOID. **S43** rules PENDING
prints `--toner-3`, and the kit's `RevealedState.jsx` maps `PENDING: var(--toner-3)` with VOID
separately at `--toner-2` — and that file is the one `BuildMirrorLeg`'s own comment names as its
source.

**Shot as built, deliberately: fixing first would have left no photograph of the violation**, which
is the evidence needed to confirm it. Consequence to state before it is ruled — at `--toner-3`,
PENDING and DEAD share a tone, separated by the strike and the drained ground. The kit and owning doc
§3.3 specify exactly that (DEAD carries three channels to PENDING's one), so it is consistent, but it
is a composition change and the seat's call.

**Found the same way as the last two: by building a frame for a state nobody had photographed.** C17
paying a fifth time here.

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

**PHASE L IS COMPLETE AND MERGED TO MAIN** (`5903750`, 2026-08-08). Plan at
`main-2/docs/5-orchestration/tmp-migration-plan.md`. Before/after ran on the **same pinned seeds**,
which is the only reason the pair is a comparison of one subject — C34 landing first is what bought
that, and it was sequenced deliberately.

### What Phase L actually taught, in the order it will bite someone again

1. **A default font face is not the face you think.** `Archivo.ttf` defaults to **SemiBold**; Regular
   is a named instance at a FreeType index. Both UGUI (S11) and my first TMP generator took the
   default, so the roman voice shipped at weight 600 unchosen for months. **The generator now matches
   on style name and refuses to fall back** — the fallback is the defect, not a safety net.
2. **A correct fix can remove an accidental guarantee.** SemiBold is near-tabular (spread 0.1875);
   Regular is proportional (4.7656). Fixing the weight broke tabular figures nobody knew were
   accidental, which is what opened and then closed S29 — resolved by moving the masthead run figures
   to the condensed face, **which owning doc §4.1 had already assigned them to.**
3. **TMP cannot enable tabular figures at all.** `OTL_FeatureTag` declares only kern, liga, mark,
   mkmk. There is no `tnum` in this stack; tabular digits come from the face or not at all. C15
   expected S29 to expire at the migration — it did not, it was answered by measurement instead.
4. **Measurement must follow the render, three times over.** Tracking widened strings, weight 600
   widened glyphs, and both feed `MeasureWidth` — which sizes truncation AND the rail sticker AND the
   tray badge. `LaptopUi.WeightFace` and the tracking parameter exist so a slot measures with exactly
   what it renders with. **Any fourth type channel must do the same.**
5. **S20's whole scope was one word.** The kit gives the laptop a single weight tier — `OsRail.jsx:17`,
   the rail identity at 600. Everything else laptop-side is 400; every 700 in the kit is TV.
6. **A gate that checks existence is not a gate.** The bootstrap reported PRESENT while the assets had
   no atlas, and again while the roman voice was the wrong weight. It now fails on both, and on an
   unwired weight table — an unwired tier renders Regular and looks like it worked.

**S52 survived all of it:** rail band 0 differing of 34,816, tray past the app slots 0 of 22,576,
through a type-stack replacement, a weight change and a face change.

**Both items that section used to carry are closed.** The three untokened kit values landed as S70.
And **L2's fact-floor gate was AMENDED, not satisfied** (batch 15): at 1024×704 Archivo's 0.73em cap
puts 13px at 9.49px of ink and 12px at 8.76px — **both render 9px**, so the gate specified an
instrument coarser than the distinction it exists to make. It now checks *which constant each slot
uses*; `MakeText`'s `Mathf.Max(13, …)` clamp is the instrument. The debt was never obtainable.

## 4bb-C13. The laptop reads soft in the room — what is eliminated, and what is left

> ## ⚠ SOLVED 2026-08-09 — READ THIS SECTION AS HISTORY
>
> **The cause was *Low Resolution Aspect Ratios* enabled in Allen's Game view.** Off, both surfaces
> read clear. **No build change was needed and none was made.** Everything below is the elimination
> trail, and it is accurate — but every candidate in it was inside the rendering pipeline, and the
> cause was outside it, in the display path the complaint arrived through.
>
> **Do not re-run any arm below.** What survives is in the re-seat block at the top of this file: the
> mirror fix stays and is gated, the 9.6% `_Sharpness` margin stays unspent, and a TMP SDF ramp
> cannot be narrowed by URP render scale. The closing is Part 3 of
> `main-2/docs/design/dd-import/blur-pipeline-read-2026-08-08.md`.
>
> **Two corrections to what is written below, so it is not read as still-standing:**
> `docs/handoffs/c13-supersampling-cost.md` recommends *A at 1.5, or C* — **option A is disproven by
> measurement** (room's runtime render-scale arm, `5c9ad05`: ramp 1.686 → 1.815px against a predicted
> 1.12), so that document is superseded and its option set never had to be chosen between. And the
> `_Sharpness` table below **did not retire `scale`**: the arms were run where a success would have
> landed under the rasterisation floor, which is the second of the two instrument laws.

**The laptop is not a package.** World-space canvas, `Camera.main`, no RenderTexture — see
`docs/handoffs/c13-surething-side.md`. Room gets the surface by merging main.

**One real defect found and fixed** (`6bd6da2`): the generator mirrored `atlas.width/height` into the
material's `_TextureWidth/_TextureHeight`, and a **Dynamic atlas serialises at 1×1**, so the SDF
shader derived its gradient against a 1-pixel texture. Correct as authored, gated so it cannot
regress — **and exonerated by room as the cause**: the reimport did not sharpen.

**Eliminated, each by measurement, not argument:**

| suspect | killed by |
|---|---|
| the grade | room: 1.44× against the UI's 1.14× |
| atlas health, `_GradientScale` | room: 10 against padding 9 is correct |
| the material mirror | room: reimport, no change |
| **canvas render scale · world-space pixel density** | **bitmaps stay sharp while glyphs are soft** — same canvas, same magnification; a coarse canvas would soften the ink sprites too |
| **TMP shader variant** | the A/B: arms differ on **43 scanlines** at clip boundaries (y≈641–649, y≈48) and are **byte-identical** across the form header, matchup rows and margin labels. A shader softening glyphs would change every glyph |

**`_Sharpness` was the next candidate and it is DEAD, measured.** `TMP_SDF.shader:186` multiplies the
SDF scale by `(_Sharpness + 1)`, so +1 should halve the ramp uniformly — the only shape that can move
a fixed screen-space ramp. Three arms generated, shot and measured at 1:1:

| slot | s0.0 | s0.5 | s1.0 | predicted |
|---|---|---|---|---|
| SURETHING 26px | 2.04 | 1.83 | 2.00 | ×0.5 |
| team name 19px | 1.26 | 1.38 | 1.33 | ×0.5 |
| column head 13px | 1.68 | 1.69 | 1.55 | ×0.5 |
| payout 31px | 1.14 | 1.00 | 1.00 | ×0.5 |

**×0.88–1.06.** The frames differ between arms, so the property reaches the renderer — it just does
not move the ramp. **And the failure is the finding: if doubling `scale` does not halve the ramp,
`scale` is not what limits the edge.** Measured ramps sit at 1.0–2.0px against a **~1px floor for
single-sampled rasterisation**, so no material property can go lower.

### ⚠ The control that eliminated canvas render scale is NOT VALID

"Bitmaps stay sharp while glyphs are soft" killed canvas render scale and world-space pixel density.
Measured, same frame, same 1:1:

| element | ramp |
|---|---|
| **wide biro ring (bitmap)** | **6.50px**, and no measurable edge at all on most scanlines |
| column head (glyph) | 2.92px |

**The ink sprites are more than twice as soft as the glyphs.** They are hand-drawn soft-edged ink
strokes — **they have no hard edge to lose**, so they can show neither softening nor sharpening. They
read as sharp because they are supposed to be soft. **Those eliminations should go back on the board.**

**Left standing: more samples per pixel** — supersampling the canvas, or MSAA — which is where the
1px floor points and which returns to canvas render mode. **Held for a separate call: it is the first
candidate that makes the room pay for the UI's gain**, so the trade-off question recorded as dead may
need reopening on different grounds. Sampling point size (90pt against 13–31px rendered) also remains
untested.

**The edge table, for that shoot.** Ramp share normalised to each element's own ground-to-ink span:

| element | size | ramp share |
|---|---|---|
| column head (toner-3) | 13px | 69.4% |
| masthead subline (toner-3) | 13px | 74.0% |
| POTENTIAL PAYOUT (toner-3) | 13px | 80.1% |
| payout figure (wax) | 31px | 70.7% |
| SURETHING (toner) | 26px | 44.0% |

**Suggestive, not conclusive, and the caveat travels with it.** No clean size relationship — if ramp
width scaled with rendered size the 31px figure would be softest and it is not. **At 13px a stem is
1–2px wide and everything is edge**, so this instrument cannot separate ramp width from stroke weight.
My first cut of it was worse: absolute luminance thresholds, which made every `--toner-3` element read
100% transitional **by construction**, because those glyphs peak near 106 and can never clear a >150
"solid" cutoff. It would have reported ink colour as softness.

**Arm B is one constant.** `SureThingTmpFontAssets.UseMobileSdfShader` → `true`, re-run the bootstrap.
The arm is stamped into every material name and the Verify line, and **Verify fails if the assets in
the tree were built with the other arm** — regenerate one face and forget another and the comparison
mixes silently.

The signed type deviations (**S28** tracking, **S29** tabular figures, **S20** weight 600, and
**markets' ladder letter-spacing**) stay in force until this surface migrates and **expire the moment
it does** — named in the close, per C15.

**Scope re-confirmed against the tree 2026-08-07** (plan precondition 4). Two things below are new
since this section was first written; the second is the one that changes the shape of the phase.

### What it touches

**Slots: 97 `MakeText` call sites and 21 `MakeButton`**, across `SportsbookApp.cs` (79 + 18) and
`LaptopOs.cs` (18 + 3). **Recounted — this section said 98 and 23**, which was true before S46–S66
moved things; the drift is small but the number is quoted in planning, so it is corrected rather than
left. The count is misleading in the good direction: every one goes through `LaptopUi.MakeText` or
`MakeButton`, so the migration is largely two helpers plus their signature, not 118 edits. The `Font`
parameter becomes a `TMP_FontAsset`, and `LaptopScreen.LoadFont` — already the single seam resolving
both voices — resolves two asset references instead.

**Font assets.** Archivo and Archivo Narrow ship as variable TTFs under
`Resources/SureThing/Fonts` with their OFL licences beside them. TMP needs generated `TMP_FontAsset`s,
which is editor work and cannot be authored blind. **This is the step that unlocks weight 600**:
TMP font assets can carry named instances, which is exactly what S20 said was required and legacy
UGUI could not give.

### The editor-gated prerequisites, and neither is a code change

Both were missing from this section and both block the first build step:

1. **TMP essential resources are not in this project.** There is no `Assets/TextMesh Pro` folder and
   no `TMP_Settings`. `com.unity.ugui 2.5.0` carries TextMeshPro in Unity 6, so the *package* is
   present — the *project resources* (settings asset, default material, shaders) are not, and
   importing them is an editor operation.
2. **`SBR.Game.asmdef` does not reference TMP.** It lists `Unity.InputSystem` and `UnityEngine.UI`
   with `overrideReferences: true`, so `Unity.TextMeshPro` must be added there and in the test
   asmdefs. That one *is* a text edit.

**Generate the two font assets from a committed Editor script, not from the Font Asset Creator
window.** `TMP_FontAsset.CreateFontAsset` plus `AssetDatabase.CreateAsset` makes the atlas parameters
— sampling point size, padding, render mode, atlas dimensions — **source rather than click history**.
A hand-clicked asset is exactly the unreproducible artifact C34 exists to stop, one layer below the
frames.

### The finding that re-opens a Design-verified item

**`MeasureWidth` does not only truncate — it sizes the shared chrome**, and this section previously
named only `InkRingGeometry` as metric-dependent.

- **The rail's sticker** (`LaptopOs.cs:1334-1339`): its x origin is `wordX + MeasureWidth(NOTEBOOK)`
  and its width is `MeasureWidth(PROPERTY OF NOBODY) + 12`.
- **The tray's MESSAGES badge** (`:1440-1445`): its x origin is
  `messagesLabelX + MeasureWidth(MESSAGES)` and its width is `MeasureWidth(badge text) + 10`.

**So TMP metrics move the rail and the tray geometrically, and S8 returns to review.** S8 is one of
this laptop's three Design-verified items; S48's fold already returned it once and S52 re-verified it.
The mechanism is identical — a Design-verified item is verified against a *configuration*, and the
type stack is part of that configuration.

**Re-run S52's pixel-identity comparison as the first check after the migration builds** — the
desktop frame against the in-app lobby frame from the same run, rail band y 0–33 and the tray band
past the app slots. It was 100% identical across 17,408 and 10,268 samples. It is the cheapest drift
detector this surface owns and this is precisely the circumstance it exists for.

**The layout survives by construction even though the pixels move:** the sticker's own comment records
that its left edge is provably clear of `NOTEBOOK` *for any font metrics*, because it is derived
rather than eyeballed. So this is a re-verification, not an expected defect — but it is not optional.

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

### Materials and C3 — the restatement owed before the first commit

The laptop canvas is world-space, inside the room's URP grade with bloom. TMP renders through its own
SDF shader rather than the default UI material, so **text will not respond to that grade identically**
— SDF edges under bloom are the specific risk, and the surface has a 31px wax payout figure that is
already the brightest type on it.

**C3 as ruled** (DD 2026-07-31, TV): eligibility is not simultaneity. The L4 occupants are *eligible*
for the HDR material; **an explicit one-token invariant holds that at most one is lit at a time**,
arbitrated so a momentary punch preempts a sustained state, at one boost value (1.8).

**Restated for TMP materials on this surface, and this is the form the migration is bound by:**

> **No migrated text slot on the laptop acquires an HDR or emissive material.** Every TMP slot renders
> on the default SDF material at the same tokens it renders today, so **the laptop contributes zero L4
> tokens before and after the migration**, and C3's count and arbitration are untouched by Phase L.

Three things follow, and they are checkable rather than declarative:

1. **The invariant is about tokens lit, not about which shader draws them.** Swapping `UI/Default` for
   TMP's SDF shader does not create an L4 occupant — but a TMP material with emission or a boost
   above 1.0 would, silently, on a surface that has never had one. **Do not enable one to fix a
   legibility complaint**; that is C10's shape and the lever is the token, not the material.
2. **The laptop's brightest type must not get brighter.** The 31px wax payout is the ceiling on this
   surface. Before/after on the same pinned seed makes this a measurement rather than an assurance:
   **it measures no higher after than before, or the material path changed and Phase L owns it.**
3. **A laptop that gained an HDR token would corrupt the TV's ladder measurements, not just its own.**
   C3 is a cross-surface law and both surfaces are composited through one grade; that is exactly why
   C15 named it as a precondition rather than leaving it to Phase T.

**The precondition this section used to state is met.** It said this seat should not migrate before
the TV's material path is settled. T41 (cap the stage), T48 (grade black point), T49 (bloom, sealed)
and T58 (gold at the goal flash) are all closed as of batch 14. **Phase T's own TMP work is still
ahead**, which is the remaining exposure and the reason clause 1 above is written as a prohibition
rather than an observation.

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
  §4-0.4) → ~~**S48**~~ (**done**, `3a85f23` — see §4-0.5; **the desktop block is complete**, and batch 9's S52 is in §4-0.6) → **S49**
  (**DD-authored — the desktop enters `ui_kits/surething/`; this seat does not write to `main-2`**)
  → **S48 last**, because folding the desktop into `NotebookChrome` re-opens S8 and returns it to
  review.
- **C15 — TextMeshPro migration** — with Allen. Until it lands, S28 and S29 hold, and tracking,
  tabular figures and weight 600 stay unreachable on this surface.

## 5. How this seat works

- **Run every suite through `tools/run-unity-tests.ps1`. Never call Unity's `-runTests` directly.**
  C29 (LAW, batch 10): a run that executed zero cases exits non-zero, and every run reports its
  executed case count. The wrapper does both, defaults its paths per worktree, and refuses to read a
  stale results file left by a run that died. `./tools/run-unity-tests.ps1 -Platform PlayMode` is the
  whole invocation.

  **Why it exists, demonstrated rather than asserted:** a bogus filter through the wrapper prints
  `executed 0 of 0 discovered · Passed` and exits 3 — because that is what **Unity itself** reports
  for a filter matching nothing: `testcasecount=0, result=Passed`. A run that did nothing, green.
  One typo can do that to any suite in any seat.

  It deliberately does **not** pass `-nographics`; that fails the four capture tests on
  `RenderTexture.Create` and reads as four regressions.
- **Unity is one editor, studio-wide. Request a slot through the orchestrator, every time.**
  Corrected 2026-08-06 after this seat ran on a standing grant in `STATUS.md` instead of asking. That
  grant is stale and must not be relied on again: this seat's suites collided with a post-merge
  validation pass and put two editors on the machine at once.
  **Announce open and close; other lanes queue.**

  **Contention does not look like contention — it looks like a flaky test run.** Two PlayMode runs
  died mid-suite writing no results, at `22:00:08` and `22:07:18`, and this seat reported both as
  transient Unity failures. They were the collision, and the timestamps say so plainly once you look
  (`evidence/logs/` filename stamps against `evidence/test-results/`). **Before diagnosing a Unity
  run that produced no results, check whether anyone else held the editor.** A genuine transient
  looks the same from inside the log.

  This is also the strongest argument for the C29 wrapper: both collided runs exited non-zero rather
  than being read as passes, so the only cost was two re-runs.
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

Captures land in `artifacts/surething-ui/`, which is **NOT gitignored.** Main un-ignored it
deliberately on 2026-07-28 so design evidence stops being silently swallowed, so a bare `git add -A`
after a capture run sweeps in ~100 PNGs. **Stage explicitly, always.** Do not trust this line either —
check it: `git check-ignore -v artifacts/surething-ui` prints nothing, and `git status --short` shows
the directory as `??`.

**Seven** `[UnityTest]`s in `SureThingVisualCaptureTests`, covering **20** distinct capture states.

This sentence read *"(gitignored). Nine states across two `[UnityTest]`s"* until 2026-08-09 — wrong on
all three counts, and the first is the expensive kind: **it told the next seat that the landmine this
file warns about twice elsewhere did not exist.** A warning is worth nothing while another sentence
quietly cancels it. Corrected after clearing **368 stray frames out of four other worktrees** — which
is exactly what the un-ignored path produces once other lanes start running this surface's capture
suite, and which no one had looked for because this line said there was nothing to find.

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
