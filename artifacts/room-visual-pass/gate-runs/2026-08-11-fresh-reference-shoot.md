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

## The bytes ARE committed — as LFS objects (resolved, Allen 2026-08-11)

This section originally argued the bytes need not be stored, because asserted pins make the set
reproducible. **Allen ruled otherwise and the set is committed** at `193a7b7`, tracked at
`docs/design/dd-import/fresh-reference-set-2026-08-11/`. Superseded rather than deleted, because the
reasoning it replaced is the reasoning a future seat will reach for.

**Committed as LFS, under a deliberately narrow rule:**

```
docs/design/dd-import/fresh-reference-set-2026-08-11/**/*.png lfs
```

**The scope check is the load-bearing part.** 82 PNGs are already tracked **raw** under
`docs/design/dd-import/` — the blur-bundle crops among them. A rule broad enough to catch those
would clean each to a pointer on diff while HEAD still held the raw blob: permanent phantom
modifications in every worktree, and any commit including them converts them to pointers with no LFS
object behind. That is the Encode Sans landmine at forty times the scale. Verified **before**
committing: `git ls-files` under the new path returned zero, and `check-attr` over all 82
pre-existing PNGs returned `unspecified` for every one.

**`*.png`, not `**`** — a `**` rule also turned the set's README into a 132-byte pointer, silently
losing the document that explains the frames. Caught on the first check and narrowed.

Verified after: 11 files LFS-tracked, git blobs 132 B, worktree files real PNGs (`89504e47`), README
still 5,393 B of prose, all 11 OIDs resolving. Objects pushed (32 MB, 11/11); **no ref pushed**.
