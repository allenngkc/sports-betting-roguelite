# Fresh reference set — shoot record

**2026-08-11 · room lead · shot on `e85f8fb`.** Replaces the raw playtest frames deleted at
`7487481`. Delivered to
`main-2/docs/design/dd-import/fresh-reference-set-2026-08-11/` (README there carries the full read).

## Pins — asserted, not merely set

| set | seed | asserted in | on mismatch | log line (once per run) |
|---|---|---|---|---|
| room (`CaptureAll`) | **`ROOMREF01`** | `PinRoomSlate()` | throws, refuses to shoot | `slate PINNED and asserted: ROOMREF01` |
| phone (`CapturePhoneReference`) | **`PHONEREF01`** | `PhoneInit()` | throws, refuses to shoot | `seed PINNED and asserted: PHONEREF01` |

Both read the seed back from `director.Run.Rng.RunSeed` and compare before any frame is shot. Each
log line is emitted only after its comparison passes.

## Reproduce it

```
Unity.exe -batchmode -projectPath <proj> -executeMethod SBR.RoomViewCapture.CaptureAll            -outDir <abs>/room
Unity.exe -batchmode -projectPath <proj> -executeMethod SBR.RoomViewCapture.CapturePhoneReference -outDir <abs>/phone
```

No `-quit`, never `-nographics`, absolute `-outDir`, **serialized** — wait for `Temp/UnityLockfile`
to clear and the PNGs to exist before the next run, and confirm one `wrote` line per frame. Exit 0
with an empty directory is what a healthy run looks like for its first minute
(`docs/design/rig-r23-recipe.md` §1).

## MD5 — 13 frames

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

**`control-a`, `control-b` and `msgs-01/phone-focused.png` share one hash** — the controls are shot
at the one-message state, so a three-way identical MD5 is the phone harness reproducing the same
frame twice over. Its validity check, passing.

The **room** set is deliberately not hash-comparable: `CaptureAll` renders live content and two
pinned runs differ by ≤5/255 (animation phase + rounding). The slate reproduces; the pixels do not.
Judge it by what is measured on it — R43, batch 30.

## Why the bytes are not committed here

The set is **35 MB of raw PNG**, and `7487481` had just deleted raw playtest frames in the other
direction; no `.gitattributes` rule matches `artifacts/`, so these would land raw in history while
the evidence-in-git question is still open. **Because both pins are asserted, the set is
reproducible from the recipe above** — which is what the pinning work bought, and the honest reason
the bytes need not be stored.

**Flagged, not decided:** the delivered copy lives in `dd-import`, which is untracked by channel
convention, so a `git clean` in `main-2` would remove it. Re-shooting restores it, but say the word
and I will commit the bytes instead.
