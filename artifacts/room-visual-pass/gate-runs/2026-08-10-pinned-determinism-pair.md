# Pinned determinism pair — the verdict, on disk this time

**Room lead · 2026-08-10 · on `4dfb053` with the `ROOMREF01` pin live.**

The pre-crash pair completed and its report died with the seat. This is a fresh pair on the pinned
rig, written to a file rather than a terminal — which is the same defect this lane already named once
(*"the claim was the artifact and no run was reproducible"*) arriving as a lost report instead of a
lost number.

---

## 1. The assertion, stated explicitly (C34, DD's binding condition)

**An unasserted pin is a comment. This pin is asserted.**

| | |
|---|---|
| **Seed value** | **`ROOMREF01`** |
| **Constant** | `RoomViewCapture.RoomSeed` |
| **Asserted in** | `RoomViewCapture.PinRoomSlate()` |
| **How** | `director.StartNewRun(RoomSeed)`, then the seed is **read back** from `director.Run.Rng.RunSeed` and compared |
| **On failure** | **throws `InvalidOperationException`** — it refuses to shoot. It does not warn and continue |
| **When** | play frame **1**; the poses are shot on frame **8** — the assert precedes every `Shoot()` call |
| **Both runs carried it** | **yes** — `[RoomViewCapture] slate PINNED and asserted: ROOMREF01` appears in `pv-a.log` and `pv-b.log`, once each |

The log line is emitted **only after** the comparison passes, so its presence is evidence of the
assert succeeding rather than of the assert existing. Both logs also carry exactly three
`[RoomViewCapture] wrote` lines, so neither set is a partial shoot.

## 2. Determinism verdict

**The frames are NOT byte-identical. They are also not meaningfully different.** Both statements are
needed; either alone misleads.

| pose | changed px | max delta | mean \|delta\| where changed |
|---|---|---|---|
| `standing-overview.png` | 0.104 % | **5**/255 | 1.04 |
| `seated-tv-couch.png` | 0.012 % | **1**/255 | 1.00 |
| `focused-laptop-desk.png` | 0.729 % | **1**/255 | 1.00 |

Two poses differ **only in the last bit** — a delta of 1/255 is rounding, not content. The third
carries a single localized zone, bbox `(1426, 895)–(1923, 1279)`, eye-confirmed as the desk and the
**TV panel with its live ticker** (`ROUND 1 OF 8 · BOARD OPE…`).

**Diagnosis: `StartNewRun` pins the deal, not the clock.** The slate reproduces — same teams, same
records, same prices. What still varies is animation phase and elapsed time on a live panel, plus
sub-quantization render rounding. That is a different kind of residue from the unpinned case, where
the *content itself* changed.

## 3. What it means for a measurement

**The R9-A mattress box `(1582, 686, 1652, 710)` is UNTOUCHED — max delta 0 across the pair.** The
residual variation does not reach the measured region at all.

| arm | R9-A | gate |
|---|---|---|
| pinverify-a | **38.20** | 10 PASS, 5 SKIP, 1 INFO, 0 FAIL, 0 VOID |
| pinverify-b | **38.20** | 10 PASS, 5 SKIP, 1 INFO, 0 FAIL, 0 VOID |

Identical, and identical to the pre-crash pinned pair (38.20 / 38.20). Unpinned was 38.30 / 38.21.

**Against batch 29's ~0.037 bound:** run-to-run movement of the measured quantity on the pinned rig
is **0.00** — an order of magnitude below the bound. So movement at that scale is **rulable**: a
future reading that differs by ~0.037 is the surface changing, not the rig breathing. That claim
rests on the assertion in §1; without it, C37 would void the null.

## 4. Scope (C25)

*Reads:* two batchmode runs of the ratified three-pose set at `4dfb053`, serialized, one editor at a
time, both seed-asserted. *Cannot see:* whether the residue stays ≤5/255 over more than two runs
(**n=2** — this is an observation, not a characterized bound); whether any *other* measurement box
intersects the TV-panel zone (R9-A does not — checked; the S2-am2 boxes live on the laptop pose,
whose max delta is 1/255); and anything about poses outside these three.

**Capture bytes deliberately not committed** — 2 × 9.8 MB whose value is the table above, and the
evidence-in-git question is still open. MD5s:

```
pinverify-a  standing a50b113c331441a9a37ede2cd5c2c0a9
             seated   d0abb8e5202a13aa16ebdeee1856ae71
             focused  8f6c69ec437727fa4a204c8a919eb8a6
pinverify-b  standing 306770f369e33e6cfcb5991395f3513a
             seated   f56b2c24889805d325a2b8ba95d29dc0
             focused  40fc4c3c48a753d5b524425c6abd36aa
```
