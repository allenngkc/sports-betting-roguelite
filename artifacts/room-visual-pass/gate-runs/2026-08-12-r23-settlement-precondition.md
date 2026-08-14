# R23 settlement-cast spec §0 — the precondition, answered from source

**Room lead · 2026-08-12 · desk read at `458d6f9`. No editor, no window booked.**

## The answer

**`RoomSettlementGlow()` is COUPLED to `TvLight`. It is not independent.**

**Route A is impossible. Route B is the route** — per the spec's own pre-commitment, and no third
route is improvised.

## The chain, five links, all at HEAD

| # | fact | site |
|---|---|---|
| 1 | `RoomSettlementGlow()`'s body is **one line** and does nothing else | `TvSweatScreen.cs:3213-3216` |
| 2 | that line is `tvLight?.Flash(roomSettlementWarm, roomSettlementIntensity);` | `:3215` |
| 3 | `tvLight` is a **`TvLight` component reference** | `:226` — `public TvLight tvLight;` |
| 4 | `TvLight.Flash()` sets `_flashColor/_flashIntensity/_flash01`, and `Update()` lerps **`pointLight`**'s colour and intensity toward them | `TvLight.cs` |
| 5 | R23's recipe disables lights **by name**, `"TvLight"` among them — the same object the room builds at `GrayboxRoomBuilder.cs:694` | `RoomViewCapture.cs:190` |

**So the subject and the confound are the same object.** Disabling `TvLight` per R23's recipe
switches off the very light whose cast is being measured. The capture would show no cast, and C37
forbids reading that as a null — with the subject off, success could not have appeared however the
code behaved.

**This is exactly the scenario §0 named.** T65's fix routed every room re-tint through one painting
point so no future site could invent its own; `:2902` confirms the old `tvLight.Flash(gold, 3.0f)`
leg-win calls are gone. **The fix changed where the re-tint is called from, not what it drives.** One
painting point, still painting through `TvLight`.

## One condition on Route B that this lane can already bound

Route B requires the two halves to differ by **the glow and nothing else** — *"a control pair that is
not frame-locked measures the match, not the glow."*

**That condition collides with a known property of this rig, and the collision is measurable rather
than hypothetical.** R43 (batch 30) established that `CaptureAll` is **not byte-deterministic even
with the seed pinned and asserted**: two pinned runs differ by **≤5/255**, localised to the desk and
**the TV panel's live ticker** — animation phase and elapsed time, which `StartNewRun` does not pin.

Consequences for whoever shoots Route B:

- **Two separate runs are not a frame-locked pair.** The residue lands on and beside the panel, which
  is where the housing box sits. The pair must come from **one run at two moments**, with nothing
  else advancing between them.
- **The signal is comfortably above the residue, but check it rather than assume it.** The glow moved
  the plaster wall by **L\* +3.9 / Rec.709 luma +8.0** (2026-08-11 measurement); the residue bound is
  ≤5/255. Roughly a 40:1 margin on luma — but that was *my* box, not the housing box, and the residue
  is largest exactly where the housing box is. **Quantify the pair's off-glow difference and state
  it beside the delta**, so the reader can see the glow exceeded the noise rather than take it on
  faith.
- **R43's read-back assert is already the mechanism** the spec asks for; `ROOMREF01` is asserted in
  `PinRoomSlate()` and throws rather than shooting on mismatch.

## Scope (C25)

*Reads:* `RoomSettlementGlow`, its one call target, the `tvLight` binding, `TvLight.Flash`/`Update`,
R23's disable list, and the room's build of that object — all at `458d6f9`. *Cannot see:* whether any
**other** component also lifts the room on settlement (I traced the painting point the spec named and
the four call sites into it; the `:404` comment asserts it is the only one, and that assertion is not
something a source read of this method can confirm on its own).

**No window is being asked for in this document.** The precondition is discharged; the route is
determined.
