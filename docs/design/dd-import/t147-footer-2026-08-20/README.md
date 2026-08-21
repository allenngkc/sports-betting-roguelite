# T147 — the ticket footer's two money facts, a row each · shot 2026-08-20

36 frames, 6 bursts of 6. Built at `83d8072`; arm is
`SBR.Tests.PlayMode.TvSweatCaptureHarness.Capture_T147_TwoRowFooter` (`[Explicit]`).

**Shot against `docs/design/footer-precommit-2026-08-20.md`.** C55 asserted before EVERY burst, in
the canvas's LOCAL space, on BOTH axes — the two money rows, the chrome row (precommit §1.2), and
on live bursts the leg row's own two lines.

## The bursts

| burst | forced? | what it shows |
|---|---|---|
| `t147-E1E3-unforced-live-row` | **NO** | the real ticket (`RISK $25` / `PAYS $330`) with leg 0 LIVE on `LEVEL`. **This is E3** — live row and footer in one frame. |
| `FORCED-t147-E1-ordinary-cited` | copy | `RISK $1,234` / `PAYS $12,340` — T74-am6's own 10x parlay |
| `FORCED-t147-E2-fact-floor` | copy | `RISK $13,639` / `PAYS $73,318,376,502` — the live-state fact floor |
| `t147-S1-settled-unforced` | **NO** | **a genuinely CASHED-OUT ticket**: `STAKE $25` / `RETURNED $42`, and **both leg chips blank** — T147's cancelled rows, never shot before |
| `FORCED-t147-S2-settled-factfloor-left-left` | amount only | the settled fact floor as SHIPPED |
| `FORCED-t147-S3-settled-factfloor-opposite-anchor` | amount **and layout** | T147-am2's counter-arm. **Not a state the product has.** |

**The settled bursts are a REAL settle.** An earlier cut forced `STAKE`/`RETURNED` onto an open
ticket; that set was deleted. The settled branch renders only at `CashedOut`/`Lost`, so forcing its
strings onto a live ticket photographs a composition the code path cannot produce — the precommit's
§0. Here the cash-out is taken through the player's own preview-then-accept path and **an assertion
now fails the arm if the settled footer has not actually rendered.**

## THE ALIGNMENT ARM (`T147-am2`), read on the settled state

Canvas spans local `-490..490`; row 2's box spans `-482..-233`. At the fact floor:

| arm | `RETURNED $73,318,376,502` ink | verdict |
|---|---|---|
| **left/left (shipped)** | `-482.0 .. -181.1` | over its own box by **51.9px**, spilling RIGHTWARD into the neighbouring zone — **survives the mask** |
| **right-anchored (counter-arm)** | `-533.9 .. -233.0` | **43.9px CLIPPED off the left by the `RectMask2D`** — the opening characters are destroyed |

**Left/left overruns visibly; right/right destroys characters.**

## What these frames do NOT settle

- **`T133` is untouched.** `RETURNED`'s 51.9px overrun of its own row is not fixed by separate rows
  and was never claimed to be. The word is the DD's.
- **The two-rung ladder is NOT BUILT.** The settled branch sets `RETURNED $x` unconditionally;
  `FitOrFallback` appears once in the file and it is on the NEED line. The precommit's §3 asks
  whether the ladder FIRES — **no frame can show a rung that does not exist.**
- **Sparseness** at the new 99.0px pitch is the DD's read off `t147-E1E3-unforced-live-row`. The
  precommit names what it will look at first — whether a row's content is top-anchored or centred.
  **It is TOP-anchored:** `AnchorTopLeft(row, ColumnInkFloor, 4f)`, so the extra ~40px pools beneath
  each row.

Frames are untracked by standing rule: the harness commits, the frames do not.
