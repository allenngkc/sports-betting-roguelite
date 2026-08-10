# R9-A refresh — the control arm, recorded as numbers

**2026-08-09 · room lead.** Companion to `2026-08-09-r9a-refresh.txt`.

The control was **shot**, not skipped. Its bytes are deliberately **not** committed: they are
9.8 MB that differ from arm A in every frame while agreeing with it on the only quantity the gate
reads. The control's value is the number, and the evidence-in-git question is open (main carries
~1 GB of raw frame history and **no `.gitattributes` rule has ever matched `artifacts/`**, so these
would land raw). Allen's call, 2026-08-09.

## What was run

Both arms, `-batchmode`, serialized, one editor at a time, lock confirmed clear between them:

```
Unity.exe -batchmode -projectPath <proj> -executeMethod SBR.RoomViewCapture.CaptureAll -outDir <abs>
```

| arm | shot | outDir | committed |
|---|---|---|---|
| **A** — the set | 21:53:59–21:54:00 | `2026-08-09-r9a-refresh/` | **yes** |
| **B** — the control | 21:55:02–21:55:04 | `2026-08-09-r9a-refresh-control/` | no — this file |

Each set's three frames were written within ~1 s, and each run's log holds exactly three
`[RoomViewCapture] wrote` lines. Neither directory is a mix of two runs.

## The numbers

| set | host | R9-A (mattress, screens LIT) |
|---|---|---|
| `batch14-postbake` — prior, **stale evidence** | — | 38.28 |
| **arm A** | `-batchmode` | **38.30** |
| **arm B** (control) | `-batchmode` | **38.21** |
| earlier same-day pair, GUI editor | GUI | 38.03 / 38.34 |

Tolerance **38.3 ± 1.0**. All PASS. Arm A lands **0.02** off the stale figure it replaces, so the
refresh **confirms** the prior pass rather than overturning it — R41-am's luma-parity change did not
move the mattress, which was assumed before and is now measured.

## Arm B — MD5, so it can be re-derived but need not be stored

```
0344f71f6acd08f6e3d811f3812f96e8  standing-overview.png
3d9edf7ce60d278f4040c4190f709cf4  seated-tv-couch.png
92e0f599898b0c30e5f1985a2eaf984e  focused-laptop-desk.png
```

Arm A, for the pair (these files *are* committed):

```
488b7b5bd0df9c0f245c3c899c9d944e  standing-overview.png
5a5c91f59f5a81f7f334e392ce5218e4  seated-tv-couch.png
c1ea0bab54f141ee3bfd0fd05301bccf  focused-laptop-desk.png
```

## What the control actually established — and it is not what a control usually establishes

**The frames are not byte-identical between runs, and that is not a fault.** Every frame differs;
`CaptureAll` renders **live Play Mode content**, so the screens carry run-dependent state.

**R9-A does not move with them.** Run-to-run scatter, same host: **0.09** in batchmode
(38.30 / 38.21), **0.31** in the GUI editor (38.03 / 38.34). Full spread across all four fresh runs
is **0.31 — about a third of the ±1.0 tolerance.**

Two consequences worth inheriting rather than re-deriving:

1. **A byte-comparison control on `CaptureAll` will always read red and always mean nothing.** That
   is §1.5's lesson one instrument over: the room's *content* reproduces, its *serialisation* does
   not — here the content that varies is on the screens, and the gate does not sample them. Compare
   the measured quantity, not the hash. The recipe doc's §7.1 reproducibility claim (identical MD5s)
   is about `CaptureConformance`, which is **Edit Mode with the screens silenced** — it does not
   transfer to this harness, and should not be quoted at it.
2. **R9-A has never had a stated repeatability, and now it does: ≤0.31 across hosts.** Prior seats
   quoted single `CaptureAll` values to two decimals as though exact. A future delta smaller than
   ~0.3 is inside this instrument's own scatter and is not a finding on one run's evidence.

## Scope (C25)

*Reads:* two batchmode runs of the ratified three-pose set at HEAD `42f8ccd`, scene unchanged
between them. *Cannot see:* whether the scatter is stable over more than two runs per host — n=2
each, so 0.09 and 0.31 are observations, **not** characterized bounds. Do not quote them as limits.
