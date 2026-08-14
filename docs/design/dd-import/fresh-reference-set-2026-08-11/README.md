# Fresh reference set — the three player surfaces, harness-shot

**From:** room lead · **2026-08-11** · shot on `e85f8fb`. Replaces the raw playtest frames deleted at
**`7487481`**. **Both seeds pinned AND asserted.**

---

## 1. What this replaces, and why it is better evidence

| deleted at `7487481` | replacement here |
|---|---|
| `allen-playtest-2026-08-09/surething-form-blurry.png` | `room/focused-laptop-desk.png` |
| `allen-playtest-2026-08-09/phone-bookie-blurry.png` | `phone/msgs-03/phone-focused.png` |
| `allen-walk-2026-08-08/surething-laptop-blurry.png` | `room/standing-overview.png` (laptop in situ) |

**Toggle-immune by construction — this is the point.** Every frame here is rendered by
`RoomViewCapture.Shoot()`, which draws `PlayerCamera` into a `RenderTexture` and reads it back
(`RoomViewCapture.cs:1883`). **It never touches the Editor Game view**, so it cannot carry the *Low
Resolution Aspect Ratios* resample that contaminated the old set and cost the studio the blur hunt.
The old frames' defect is not merely absent here; it is unreachable by this code path.

## 2. Pin assertions (C34 — an unasserted pin is a comment)

| | room set | phone set |
|---|---|---|
| **seed** | **`ROOMREF01`** | **`PHONEREF01`** |
| entry point | `CaptureAll` | `CapturePhoneReference` |
| asserted in | `PinRoomSlate()` | `PhoneInit()` |
| how | `StartNewRun(seed)`, then the seed is **read back** from `director.Run.Rng.RunSeed` and compared | same |
| on mismatch | **throws — refuses to shoot** | **throws — refuses to shoot** |
| when | play frame 1; poses shot on frame 8 | before any frame is shot |
| log line | `[RoomViewCapture] slate PINNED and asserted: ROOMREF01` | `[PhoneRef] seed PINNED and asserted: PHONEREF01` |

Each log line is emitted **only after** its comparison passes, so its presence evidences the assert
*succeeding*, not merely existing. Both appear exactly once in their run's log.

## 3. Reproducibility, stated per set — they are not the same

**Phone set: byte-reproducible, and it proved it.** `control-a`, `control-b` and
`msgs-01/phone-focused.png` are **all three MD5-identical** (`2e2f6cae…`). The controls are shot at
the one-message state, so a three-way identical hash is the harness reproducing the same frame twice
over — its own validity check, passing.

**Room set: NOT byte-reproducible, by nature, and that is not a fault.** `CaptureAll` renders live
Play-Mode content. Two pinned runs differ by ≤5/255 on one pose and 1/255 on the other two — animation
phase on the TV panel plus sub-quantization rounding. **The slate itself reproduces exactly**: the
pin fixes the deal, not the clock. Measured quantities are stable (R43, batch 30: the mattress
luminance reads identically across pinned runs, with the unpinned arm as the positive control
certifying that null). **Compare these frames by what is measured on them, never by hash.**

## 4. Contents

```
room/                     CaptureAll, ROOMREF01, 2560x1440
  focused-laptop-desk.png   0.52 m along the lid normal, 30 deg — the FORM board
  seated-tv-couch.png       the couch pose, 17 deg
  standing-overview.png     standing, 68 deg — laptop and phone in situ
phone/                    CapturePhoneReference, PHONEREF01
  msgs-01|02|03/            1, 2, 3 messages — phone-focused.png + seated-room.png each
  Z-final/                  end state (3 messages)
```

Feed content is engine-emitted, never authored (R28-am). States reached: 1 / 2 / 3 messages, longest
**60 chars** at every state — the pool's ceiling, unchanged from the 2026-08-09 set, which is the
pinned seed reproducing.

**Controls not staged.** `control-a`/`control-b` were shot and are byte-identical to
`msgs-01/phone-focused.png`; two more copies of the same 3 MB would add nothing. Their hash is in §5.

## 5. MD5 — every frame shot, including the two not staged

```
2e2f6cae02b8f8145fe658165e397823  phone/control-a/phone-focused.png
2e2f6cae02b8f8145fe658165e397823  phone/control-b/phone-focused.png
2e2f6cae02b8f8145fe658165e397823  phone/msgs-01/phone-focused.png
382bc12a30c7d180141c9907ac5c9624  phone/msgs-01/seated-room.png
658302a9dc275aa4ec63c379c169ce7b  phone/msgs-02/phone-focused.png
5c4b4f895141653f4e26316e45bf29ab  phone/msgs-02/seated-room.png
3df88573b087827049fe49208fab2544  phone/msgs-03/phone-focused.png
0da4edc4e067d56d60c40a06de8bc0fc  phone/msgs-03/seated-room.png
25e530358cc39ade3ced14c76d6b6b40  phone/Z-final/phone-focused.png
9ccfaea2a06ccc37642f9e2a9490d0d3  phone/Z-final/seated-room.png
b616ea10ca962d3d7a310b25b58a9e88  room/focused-laptop-desk.png
d626f2df0b6b9d52c91becdec1fa726f  room/seated-tv-couch.png
0b2a3ede017dab91b0b07d0d6e75093e  room/standing-overview.png
```

## 6. Scope (C25)

*Reads:* the three player surfaces at their canonical views, one pinned run of each harness, both
seeds asserted, editor free and single-instance throughout. *Cannot see:* any laptop state other than
`FORM`, sheet 1 of 1, bank $350 — one board, one deal; whether 3 is the feed's message ceiling (it is
the step budget's, not the engine's — carried forward from the 2026-08-09 set and still unproven);
and any pose outside these. **The room frames are eye-confirmed populated** — the FORM board carries a
full six-row deal and the phone carries three engine-emitted messages; neither is a blank surface
stabilised by an early shot.
