# SureThing UI — re-seat state

**Written:** 2026-08-01, at a session hygiene clear. **HEAD:** `571675c` · **Branch:** `surething-ui`
· 3 commits ahead of `main` · working tree clean.

This is written for a seat with **no conversation context**. Everything below is either verifiable in
the repo or flagged as unverified.

---

## 1. Where the work stands

The SureThing laptop's first slice **merged to main** at `2e97d13` (2026-07-31). S6 (lobby shell),
S7 (ink sprites) and S8 (OS chrome) are **DESIGN-VERIFIED** by the Design Director — the laptop's
first. Changes to those three are regressions, not iteration.

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

**Suites: EditMode 75/75, PlayMode 38/38.**

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
  defect that was obvious in a screenshot at least three times. Tests here assert structure; they do
  not assert appearance.
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

The REWARDS screen's masthead reads `SURETHING FORM`. It is the shared masthead, and against the kit
1:1 that is very likely wrong copy for a non-FORM screen. Noticed during S26 verification, not yet
audited or ruled.
