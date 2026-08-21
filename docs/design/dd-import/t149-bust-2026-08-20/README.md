# T149 criterion 2 — the BUSTED ticket's cancelled rows · shot 2026-08-20

8 frames, one burst, **UNFORCED throughout** — no string and no layout was forced, so nothing here
carries `FORCED-`. The state was dealt for.

Arm: `SBR.Tests.PlayMode.TvSweatCaptureHarness.Capture_T149_BustedTicket` (`[Explicit]`).
Shot against `docs/design/t149-bust-precommit-2026-08-20.md`, which binds this window.

## THE STATE, dealt not forced

```
state=Lost   footer='STAKE $25' / 'RETURNED $0'   lost leg=0 of 3, cancelled behind it=2
```

Ticket `[MIDDLEMEN ML, DRAW, DRAW]` on `GOALLESS-5` — the recipe the pre-commitment offered. Leg 0
lost; **two unplayed legs sit behind it**, which is the state criterion 2 needs and the state both
earlier attempts lacked.

## EVERY BINDING CONDITION, and how it was met

| condition | met |
|---|---|
| 1. ≥2 legs, loser NOT last | **asserted, not assumed** — the arm finds the `L` row and fails unless a leg follows it. Lost leg 0 of 3. |
| 2. the loss is REVEALED | the arm waits for `State == Lost` **and** the footer to render settled before the shutter |
| 3. `C55` — lost row and struck row in ONE frame | `LegRowLine0` (lost) and `LegRowLine1` (struck) both asserted in frame, in the canvas's LOCAL space, before the burst |
| 4. chrome row and footer in frame, footer settled | `Chrome`, `RiskPays`, `Pays` all asserted in frame; footer reads `STAKE` / `RETURNED $0` |
| 5. forcing disclosed | nothing was forced |

**Condition 4 is asserted as a gate, not checked by eye.** The arm fails if the ticket reaches
`Lost` while the footer still reads `RISK`/`PAYS` — reaching the state is not rendering it, which is
`T133-am2`'s mistake restated for this state.

## THE FOUR CHANNELS, measured off the built rows

The pre-commitment's source table said LOST and cancelled differ on four channels. They do:

| leg | state | chip | text | text alpha | strike | extinguished |
|---|---|---|---|---|---|---|
| 0 | **LOST** | `L` | `MIDDLEMEN ML` | **0.15** | off | **on** |
| 1 | cancelled | *(blank)* | `DRAW` | **0.40** | **on** | off |
| 2 | cancelled | *(blank)* | `DRAW` | **0.40** | **on** | off |

**All four separate, and the tier difference is 2.7× in alpha** — 0.15 against 0.40. Criterion 1 is
re-confirmed in the bust's presence: no row prints `NEXT`, and every unplayed leg carries the strike.

## WHAT THIS SET DOES NOT CLAIM

- **Criterion 2 is the DD's read, not this lane's.** The numbers above say the four channels differ.
  Whether the struck rows READ as cancelled rather than as lost is what the frames are for.
- **The pre-commitment names the specific way it can still fail: THE BLANK CHIP.** Cancelled rows
  are the only rows on this surface carrying no state word at all. Nothing measured here can answer
  whether that absence reads as *cancelled* or as *nothing happened here* — an absence is not a mark,
  and only the frame can settle it.
- **Nothing about the footer's copy.** `STAKE` / `RETURNED $0` appearing correctly is condition 4's
  screen-state check, per the pre-commitment's §4.

## One note on the recipe

The pre-commitment's route worked as offered. The arm also carries a guard the recipe needed but did
not mention: `ResolveLegFinal` busts instantly only when no save is legal, and `mulliganLegal` is
`_mulliganAvailable() && ActiveLegCount() >= 2` — which a three-leg ticket satisfies. **On this run
the window never opened** (no slip was available), so the guard did not fire; on a run that owns a
mulligan slip it would have, and without it the ticket would have sat in the pending-loss window and
never busted.

Frames are untracked by standing rule: the harness commits, the frames do not.
