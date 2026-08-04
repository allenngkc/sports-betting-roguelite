# Room visual-pass baseline

## Capture method

Captured on Unity `6000.5.3f1 (c2eb47b3a2a9)` with a temporary, Editor-only batch harness and the existing `PlayerCamera` rendered to a `RenderTexture` at **2560 x 1440**.  The harness opened `Assets/Scenes/Room.unity` without saving, entered real Play Mode, waited eight player-loop frames for the runtime TV and laptop world-space UI to initialize, set the same final camera poses used by the existing interaction code, rendered each PNG, exited Play Mode, and terminated the editor without saving the scene.

This is a deterministic runtime reproduction of the completed `SitSpot` and `DeskFocus` glides, rather than interactive input: both components glide to their serialized anchors over 0.35 seconds and apply their serialized FOVs.  The rendered images were inspected directly as image assets (not desktop captures); all are non-empty 2560 x 1440 PNGs, visually distinct, and contain room/runtime UI content with no editor chrome.

| Capture | Final camera pose and lens | Current source/scene mapping | SHA-256 |
| --- | --- | --- | --- |
| `standing-overview.png` | position `(0.300, 1.640, -1.400)`; forward `(0, 0, 1)`; up `(0, 1, 0)`; vertical FOV `68°` | `GrayboxRoomBuilder.BuildPlayer`: Player spawn `(0.3, 0.02, -1.4)` plus `PlayerCamera` local eye `(0, 1.62, 0)`; Room serializes FOV 68. | `00e42159b811b47187841d89589917661bda030aacf22415f8d019604c04e93c` |
| `seated-tv-couch.png` | position `(-0.950, 1.150, 0.300)`; forward `(0.99974, -0.02291, 0)` toward `(1.232, 1.100, 0.300)`; up `(0.02291, 0.99974, 0)`; vertical FOV `17°` | Couch `SeatAnchor`; builder uses `Quaternion.LookRotation(tvScreenCenter - seatedEye, Vector3.up)`.  `SitSpot.seatedFov` is 17 in both source and Room. | `802dd87b674cf751e39d8d6ab96c6c6e9c3e5d976093a81ebdf2c9be1e78a673` |
| `focused-laptop-desk.png` | position `(0.738982, 1.051217, 1.620000)`; forward `(0.939693, -0.342020, 0)`; up `(0.342020, 0.939693, 0)`; vertical FOV `30°` | Laptop `FocusAnchor` in Room; builder places it `0.52m` on the lid outward normal and uses `Quaternion.LookRotation(-outward, lid.up)`. `DeskFocus.focusFov` is 30. | `175cf5929670f1930b85d2dd4fc0626f587a6940e2e11c08f75c5dea482a29de` |

## Observed baseline constraints

- The standing overview confirms the compact room’s left bunk/couch mass, right TV, desk/laptop cluster, stool, and narrow central walkable lane; future decoration must preserve the door-to-desk/couch clearance and not hide the interactive laptop or TV read.
- The seated 17° composition intentionally makes the TV dominant, but the runtime screen is now readable (`PLACE YOUR BETS`, progress bar, and run metadata) with a visible room edge rather than a solid green plane.
- The desk 30° composition intentionally fills the view with the laptop; its initialized SureThing board, betslip, controls, and screen perimeter are readable, while the small visible surrounding room frame preserves the focused-desk context.

## Generated-room source of truth

`Assets/SBR/Editor/GrayboxRoomBuilder.cs` is the room’s source of truth: `Build()` starts a new empty scene, deletes and recreates `Assets/Scenes/Room.unity`, rebuilds the shell/couch/TV/window/desk/player/HUD/event system/lighting, and saves the scene.  A content rebuild therefore erases all hand-authored Room scene content and also rewrites the builder-owned material assets; it can additionally create the `Interactable` layer in `ProjectSettings/TagManager.asset` and registers the Room in build settings.

Verification after capture: `Room.unity` SHA-1 is `dec08cc864ef0f859b91896b57788132f3ce9004`, identical to `HEAD`; no scene save occurred.
