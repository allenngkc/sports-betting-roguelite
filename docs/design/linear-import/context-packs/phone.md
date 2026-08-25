## What this is

His cheap personal phone, on his desk, in the laptop's register — not the TV housing's equipment and
not the house's. It shows **exactly one thing: the bookie's messages.** No apps, no other threads, no
contacts, and **the player never replies.** At one message the screen is mostly empty and **that
ships** — he has one contact and it is the man he owes; a nearly empty phone is characterisation, not
an unfinished screen. The first instinct of anyone opening this surface is to fill it, and every way
of filling it is already forbidden. Three ownerships, kept separate: **the room owns the object**
(geometry, material, placement, its interaction `MeshCollider`, R16); **this document owns the
treatment** (composition, colour, type, voice); **nobody owns the content** — it is engine-emitted
and stays that way (R28-am).

## Canon (read before any ticket here)

- **Owning document:** `docs/design/phone-design.md` — CANON, Allen-approved 2026-08-09 (C26-am3),
  drafted by the Design Director the same day ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/phone-design.md
  Every clause transcribes a ruled row — P1–P8 (batch 23), R19(a), R28, R28-am, R39, R39-am, S31-am,
  S44, T70-am. **Nothing in it is new law.**
- **Evidence base, stated plainly (C25):** this document stands on **one** set —
  `phone-reference-set-2026-08-09`, seed `PHONEREF01` pinned and asserted, three message states, two
  views. The laptop's document had a fortnight behind it; this one has an afternoon. Where a clause
  rests on a single frame it says so, and §10 lists what the set could not see.
- **Constitution** (`docs/design/constitution.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/constitution.md)
  **— clauses that bind here:**
  - **C9** — the owning doc is this surface's binding art authority. Its stub was a legitimate state
    until Allen put the surface in scope; C26-am3 expired the stub rather than let it stand as cover.
  - **C22 / C22.1** — a ruling exists only as a register row; one finding, one ID.
  - **C11** — rendered evidence or no claim, Design-verified included.
  - **§2.5** — measure the rendered thing, not the source; P-G5's own blind spot is that TMP point
    size and screen px are not the same quantity.
  - **§3.5 / T51** — a bound is not a layout: a *zone* may not resize at runtime, but message bubbles
    varying inside a fixed list is the list working. The panel is fixed; items vary inside it.
  - **C19** — reachability: every message the engine emits is reachable, no cap that hides one, no
    silent drop; the position indicator is present iff the list scrolls.
  - **C18 §4.2 + C29** — each gate states what it cannot see; every invocation reports its executed
    case count and exits non-zero on zero cases.
  - **C27** — gate regions are eye-confirmed, not merely low-variance.
  - **C34** — pin and assert the seed on every capture set.
  - **C31** — §10's open-items list is the whole list.
  - **C14 / C16** — 1:1 fidelity; only the platform makes a thing impossible.
  - **C12** — frames travel in the DD import, not in git. **C55** — a capture must contain its
    subject.
- **Anti-reference:** `design/08-art-direction.md` (T3, deprecated) and `chromeCyan` (T9, a retired
  hue) — `PhoneScreen.cs` cited the former as its authority and printed the latter on `BOOKIE`.
  **Amplitude has never rescued a retired hue at this studio.**
- **Design system:** `docs/design/design-system/` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/design/design-system
- **Product laws:** PRODUCT.md §… — **TO CONFIRM**; DECISIONS.md entries — **TO CONFIRM**.

## Ownership

- **May touch:** `unity/SBR/Assets/SBR/Runtime/PhoneScreen.cs` (the *treatment* only — composition,
  colour, type, case, the header label), this surface's tests
  (`unity/SBR/Assets/Tests/PlayMode/PhoneTests.cs`,
  `unity/SBR/Assets/Tests/EditMode/BookieFeedModelTests.cs`), `docs/design/phone-design.md`.
- **Must never touch:**
  - **The copy.** `BookieFeed.cs`, `BookieFeedModel.cs`, `BookieScript.cs` — a message this document
    dislikes is a ruling against the engine's copy, made in the register, **never a string edited
    into the surface** (R28-am). No authored content on this screen.
  - **The object.** `GrayboxRoomBuilder.cs` (`BuildPhone`), `unity/SBR/Assets/Scenes/Room.unity`,
    `PhoneShell.mat` — the room lead's (R16 / R28).
  - **Emission.** Settled at R39 and explicitly out of this document's scope (§6.3): warm
    near-neutral R ≥ G > B, one chromaticity family with the laptop, amplitudes 1/3/15 off one shared
    base. No cue, state or signal may ever be built on the glow (R39-am, closed).
  - `engine/**`, the laptop and TV surfaces, `ProjectSettings/**`, `docs/ARCHI.md`, `DECISIONS.md`.
- **Worktree / lane currently executing: none — TO CONFIRM.** The surface has been worked from the
  room lane (`room-refinement`, retired) and the DD seat; the reference set was shot by the session
  recorded in `docs/handoffs/session-hygiene-2026-08-09.md` ·
  https://github.com/allenngkc/sports-betting-roguelite/blob/main/docs/handoffs/session-hygiene-2026-08-09.md
- **Unassigned scope, flagged not hidden:** C15's TMP migration names the laptop and the TV. It does
  **not** name the phone, and the room handoff's read-only list does not name `PhoneScreen` either
  (`docs/handoffs/room-refinement.md` §12.2). **Ask before the phase, not during it.**

## How work here is verified

- **Gates (phone-design §9), each with its stated blind spot:** **P-G1** no canvas region exceeds
  chroma 3.0 (CIELAB from linear on a rendered frame, regions eye-confirmed) · **P-G2** the topmost
  message sits at the panel's content origin at every message count · **P-G3** the sender name
  appears at most once per screen · **P-G4** rendered message count == engine-emitted count, **or**
  the position indicator is present · **P-G5** no text element below the ratified body size ·
  **P-G6** no animated property on any phone canvas element.
- **Capture convention:** shoot **on message-count change**, name each set by the count it holds, pin
  and assert the seed (C34). Sets land under `docs/design/dd-import/<set>/` and **stay untracked**
  (C12). In-frame rule (C55): assert the subject is in frame.
- **Tests:** `./tools/run-unity-tests.ps1 -Platform EditMode`, then `-Platform PlayMode`
  (`BookieFeedModelTests`, `PhoneTests`, `RoomSmokeTests` coverage). Current `main` baseline:
  **EditMode 342 / PlayMode 152** (`docs/handoffs/tv-theater.md`, 2026-08-25) — confirm at seating.
  Then `python tools/check_test_results.py <results.xml> --min-cases "…"` before believing a green
  suite. **Never pass `-quit` with `-runTests`.**
- **Editor lease:** one Unity Editor across all worktrees, serialized through the orchestrator;
  warm-compile before `-executeMethod`; wait for the Unity process *and* `Temp/UnityLockfile`.
- **CI:** `.github/workflows/ci.yml` must conclude `success` on merged `main` (`gh` is not
  installed — REST API or browser).

## Standing risks / traps

- **The phone focus PlayMode test is a known environmental flake** (STATUS, cycle 444). A red there
  is diagnosed before it is believed.
- **Driving a run inside one `EditorApplication.update` callback does not tick `Update()`** — the
  feed never processes the director's verbs and three "states" come back holding the same single
  message. That is how the reference set was nearly mis-shot.
- **`ScreenCapture` returns null in `-batchmode`.** Both of that session's errors were assumptions
  about the *host*, not about the surface.
- **Three messages is not the feed's ceiling** — the reference run stopped on a step budget, not on
  the feed running dry. Anyone reasoning about stack composition must treat 3 as a floor on the max.
- **The 16-line / 60-char pool is half-verified:** the 60-character line was observed live, so the
  ceiling is confirmed; the **pool size** is read from `BookieScript.cs` and was never exercised.
- **TMP hazard (room handoff §12.3):** the plain `TextMeshPro` component is a `MeshRenderer`, and
  `MarkStaticForGI` sweeps `MeshRenderer` — pick it and the phone's glyphs get baked into the
  Adaptive Probe Volume. Use `TextMeshProUGUI` and keep the world-space Canvas.
- **The collider inventory is ratified at 29** and `PhoneScreen`'s `MeshCollider` is one of the two
  named `MeshCollider` members. Restructuring the object changes a gated number — re-run
  `tools/room_gate_check.py`.
- **`Mat()`'s `RealtimeEmissive` guard does not extend to TMP materials.** Do not assume coverage.
- **A Unity run dirties `URP.png`, `ProjectSettings.asset` and `LiberationSans SDF - Fallback.asset`.**
  `git checkout --` them; **stage by explicit path.**
