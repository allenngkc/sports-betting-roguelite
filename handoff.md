# SureThing UI — lead ownership handoff

**Handoff date:** 2026-07-28  
**Ownership returned to Claude:** 2026-07-30 (Allen's call — Claude remains the leads)  
**Incoming owner:** Claude (Opus 5) acting as SureThing product, UX, and implementation lead  
**Worktree:** `C:\Users\Allen\orca\workspaces\sports-betting-roguelite\surething-ui`  
**Branch:** `surething-ui`  
**Starting HEAD:** `d66543898f2841e1b8e0f33c7c33a49ed9d1594b`  
**Current gate:** visual direction chosen; production Unity implementation has not started

> **2026-07-30 update:** Leads were handed to GPT/Codex on 2026-07-28; Allen has returned
> ownership to Claude. The body below reflects the 2026-07-28 state. Since then the design
> package was committed (`93d695a`, `8bb8d76`, `3d69fcc`) and the annotated form-guide lobby
> shell landed (`cb83c90`), so §2's untracked-package warning is resolved. The worktree now
> carries substantial uncommitted Unity changes — read `git status` and the recent log before
> continuing.
>
> Decision routing has also changed: critical or strategy decisions escalate lead →
> orchestrator (`main-2`) → Allen, and all design decisions (visual direction, UI,
> interaction, art, 3D) belong to the Design Director — this lead implements approved
> specs and makes essentially no design calls. Where this document says "ask Allen",
> route accordingly. See `main-2/docs/5-orchestration/STUDIO.md`.

> **2026-07-31 studio briefing:**
> - A dedicated Orchestrator session (Fable 5, `main-2`) is live: it sweeps worktrees,
>   owns `main-2/docs/5-orchestration/STATUS.md`, merge order, and integration. It may
>   message this terminal via Orca; treat its dispatches as coordination — Allen's word
>   is final.
> - A Design Director seat (Claude Design) is live and inherits every existing design
>   decision; a studio design system is being built from the approved packages, and
>   future specs will cite it. Do not preempt the pending Allen rulings: C1 TV
>   "Decision A", C2 TV light-spill colour, T8 scanlines/static.
> - Report telegraphically (Done / Next / Risk / Need Allen); keep evidence local;
>   never send raw logs upward.
> - Sweep flags for this worktree: modified `ProjectSettings/*` and URP global settings
>   are integration-only files — justify them to the orchestrator or revert before
>   merge; clean the stray test XML/log files at `unity/SBR/` root; commit `handoff.md`.

## 1. Ownership transfer

Take full ownership of this worktree. Continue through implementation and validation without
asking Allen to approve routine files, tests, refactors, or intermediate checkpoints.

Ask Allen only when a choice materially changes the product, art direction, scope, licensing,
or another worktree's ownership. Report progress at meaningful milestones.

Communicate in simple telegraphic language:

- result first;
- short sentences;
- no giant walls of text;
- no raw tool logs unless Allen asks;
- finish updates with `Done`, `Next`, `Risk`, and `Need Allen`;
- use `Need Allen: nothing` when unblocked.

## 2. Preserve this work before doing anything else

At handoff, `git status --short` showed:

```text
?? PRODUCT.md
?? docs/design/
?? tools/
```

This is the complete product/design package created by the outgoing lead. It is untracked, not
disposable. Do not clean, reset, regenerate, or overwrite it.

First actions:

1. Run `git status --short --branch`.
2. Read the sources listed below.
3. Inspect the untracked package.
4. Create a named checkpoint commit for the accepted design package before production UI edits.
5. Keep unrelated user changes intact.

No fresh Unity test run was performed at handoff because production code is still unchanged.

## 3. Current product decision

**Approved Direction — The Annotated Form Guide.**

The Orca worktree comment saying Allen must choose between concepts is stale. The decision is
recorded in `docs/design/direction-concepts/INDEX.html`.

The implementation reference is:

- `docs/design/direction-concepts/element-kit.html`
- `docs/design/direction-concepts/assets/ASSETS.md`

**Rejected comparison — The Catalogue Sleeve** remains comparison evidence only. Earlier discarded
explorations must not be revived.

## 4. What is already complete

- Product context and cross-surface laws: `PRODUCT.md`
- Bet365/FanDuel task-pattern research: `docs/design/surething-ui-revamp/visual-study.md`
- Earlier structural UI package: `docs/design/surething-ui-revamp/`
- Approved-direction rationale: `docs/design/direction-concepts/DIRECTIONS.md`
- Fixed content, states, and legibility contract:
  `docs/design/direction-concepts/SHARED-SPEC.md`
- Chosen 1024×704 lobby concept:
  `docs/design/direction-concepts/direction-1-form-guide.html`
- Rejected comparison:
  `docs/design/direction-concepts/direction-2-catalogue-sleeve.html`
- Real-size component/state kit:
  `docs/design/direction-concepts/element-kit.html`
- Deterministic biro/strike sprites and generator:
  `docs/design/direction-concepts/assets/` and `tools/art/make-biro-rings.py`

The previous lead's stated next step was to write a durable `DESIGN.md` before editing Unity.
Do that first, place it beside the approved design package, and link it from `INDEX.html`.

## 5. Locked product laws

- Runtime is Unity UGUI on a fixed **1024×704** world-space laptop canvas.
- This is the occupant's personal, cheap, grubby machine. It must not look like the
  institution-installed TV.
- Laptop owns choices: slate, markets, slip, stake, staging, lock, shop, and placed tickets.
- TV owns unrevealed drama. MY BETS may mirror only `TvSweatScreen.RevealedView`.
- The interface never re-derives engine truth.
- Odds are locked.
- One selection per matchup; a new market on the same matchup replaces the old one.
- Build state is calm and deliberate, but not bland. Sweat state can become loud.
- No real operator branding, copy, marks, team names, or characteristic color system.
- No pure black. The screen renders inside the room's unified grade.
- Critical facts survive a 50% thumbnail.
- Product-fact text is at least 13px. OS-only chrome may be 12px. Nothing is smaller.
- Normal text meets 4.5:1 contrast.
- Status is never color alone.
- Targets are at least 44×32px with 8px separation.

If an old document conflicts with `PRODUCT.md` or the approved design package, the newer package wins.
In particular, SureThing owns its own color language; the TV worktree cannot impose its palette.

## 6. Chosen visual system

Thesis: the player does not tap generic sportsbook pills; they mark up a late-night form guide.

Core language:

- lifted warm olive-black ground `#16160F`;
- inverted toner for the house document;
- biro blue `#5E86B8` for the player's choices;
- wax amber `#D9A441` for money and primary action;
- oxide red `#B4483A` only for the house's mark, which includes the strike on a dead leg
  (amended by Allen, 2026-07-30);
- ruled columns and a right-side working margin;
- selection shown as a hand-drawn ring, not color alone;
- no rounded-card sportsbook shell;
- no retro-terminal costume;
- no cyberpunk neon-on-black default.

Five component laws from the element kit:

1. Oxide red belongs to the house's mark. A dead leg's strike counts as the house's mark and may
   be red (Allen, 2026-07-30); red is still never decoration or a general "bad" tint.
2. Amber is money/action; blue is the player's choice.
3. Nothing is pure black.
4. Product facts never fall below 13px.
5. Status is never color alone.

The intended Bell Centennial production face may require a commercial license. Do not download
or commit an unlicensed font. Use a licensed project asset or bring one concise font decision to
Allen.

## 7. Implementation scope and file ownership

Primary owned files:

- `unity/SBR/Assets/SBR/Runtime/SportsbookApp.cs`
- `unity/SBR/Assets/SBR/Runtime/LaptopOs.cs`
- `unity/SBR/Assets/SBR/Runtime/LaptopScreen.cs`, only when screen integration requires it
- `unity/SBR/Assets/Tests/EditMode/BetslipModelTests.cs`
- `unity/SBR/Assets/Tests/EditMode/AnytimeScorerBetslipTests.cs`
- `unity/SBR/Assets/Tests/PlayMode/LaptopOsTests.cs`
- new SureThing-only fonts, sprites, materials, and import helpers
- `PRODUCT.md` and `docs/design/direction-concepts/**`

Read-only unless a demonstrated behavior defect requires a separately approved expansion:

- `engine/**`
- `unity/SBR/Assets/SBR/Runtime/RunDirector.cs`
- `unity/SBR/Assets/SBR/Runtime/TvSweatScreen.cs`
- all TV/theater code
- `Room.unity`
- `GrayboxRoomBuilder.cs`
- room art and lighting
- `ProjectSettings/**`

To prevent merge conflicts, do not edit shared canonical files such as `docs/ARCHI.md`,
`DECISIONS.md`, or root planning documents. Record any required canonical update in the final
handoff to the principal integrator.

## 8. Recommended execution sequence

1. Preserve and checkpoint the design package.
2. Write `DESIGN.md`: tokens, typography, component anatomy, state matrix, motion, OS chrome,
   asset rules, and explicit mapping from HTML concepts to UGUI.
3. Audit current `SportsbookApp`, `LaptopOs`, and their tests. Preserve every existing flow.
4. Freeze a narrow file plan. Keep behavior changes separate from visual changes.
5. Build reusable UGUI helpers/tokens instead of duplicating style values across methods.
6. Implement the 1024×704 lobby and working slip from the element kit.
7. Implement all functional states: default, hover, selected, disabled, staged, locked,
   revealed GREEN/DEAD, remove, replacement, and empty/error states.
8. Apply the same system to event detail and MY BETS; do not stop after one attractive lobby.
9. Import the `@2x` ink sprites using `assets/ASSETS.md`. Variant selection must be deterministic
   by matchup index, never random on rebuild.
10. Validate in the actual angled laptop view, not only a flat browser or Game view.
11. Run targeted EditMode and PlayMode tests; then perform the complete relevant Unity suite once.
12. Provide matched captures for lobby, event detail, staged ticket, disabled lock reason,
    and revealed MY BETS.

Do not copy Bet365 or FanDuel. Reuse only general task principles already documented: compact
comparison, persistent context, progressive disclosure, and clear economics.

## 9. Acceptance gate

Product:

- all current betting, staging, locking, shop, and MY BETS flows still work;
- every price, stake, payout, limit, and disabled reason is truthful;
- one-selection-per-match replacement remains intact;
- laptop never reveals ahead of the TV.

Visual:

- chosen form-guide identity is unmistakable without decorative explanation;
- OS chrome and app feel personal, not institutional;
- the 50% thumbnail test passes;
- no factual text below 13px;
- no status depends on color alone;
- all required states match the element kit;
- the canvas fits 1024×704 with no accidental clipping.

Engineering:

- no engine, TV, room, scene, or project-setting drift;
- new assets are licensed, reproducible, and committed with Unity metadata;
- targeted tests pass;
- any architecture-doc change needed at integration is listed for the principal.

## 10. First update to Allen

Keep it short:

```text
SureThing handoff loaded.
Done: preserved the Approved Direction package and confirmed the current code boundary.
Next: checkpoint design, write DESIGN.md, then implement the first end-to-end UGUI state.
Risk: <one real risk or none>.
Need Allen: nothing.
```
