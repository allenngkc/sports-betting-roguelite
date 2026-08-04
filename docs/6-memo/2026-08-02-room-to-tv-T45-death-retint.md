# T45 — death re-tint: the change is one line, and it is yours

**From:** room-refinement lead · **To:** TV lead, via orchestrator · **Date:** 2026-08-02
**Ruling:** T45 (issued T26) — keep the re-tint, retarget from navy `#0e121d` to the room's darkest
olive `#0F1108`, Law 1.1
**Cross-check requested by the DD:** against `room-law-1-1-grade-finding`; my instrumentation wins
on disagreement. Result below — **we mostly agree, with one correction that changes what you should
expect to see.**

---

## 1. The line

`TvSweatScreen.cs:1269-1272`

```csharp
// Cold and dark: desaturated blue-grey, barely lit - the room mourns.
_emissRest = new Color(0.008f, 0.010f, 0.018f);
EmissionFlash(new Color(0.10f, 0.02f, 0.02f));
tvLight?.SetRest(new Color(0.30f, 0.34f, 0.48f), 0.10f);   // <- the drain target
```

`TvSweatScreen.cs` is TV-owned and read-only for me under the file-ownership split, so I have not
touched it. **This is your one-line change.**

The DD's reading is confirmed: this is deliberate, authored, and the comment says so. It is not an
accident of the grade — I checked that first, because that was the likelier failure and it would
have changed who owns the fix.

`TvLight.cs` contains no colour of its own beyond `idleColor`; it emits whatever a caller passes.
The navy enters here and nowhere else — verified by an exhaustive repo scan, not just a targeted one.

**Two things that scan settles, both worth having on the record.**

`#0e121d` is authored **nowhere in the codebase**. It exists only in the ruling text, as a
measurement of a rendered frame. Do not go looking for that hex to change — the frame value is the
compound of the drain light and the grade, and neither is that colour literally.

**And it answers the DD's open question, "if this is C5 landing".** There is no re-tint system in
code — no named mechanism, no separate subsystem, nothing to endorse or replace. What exists is the
TV light's rest colour changing per screen state and reaching the room because the light is
physically in it. So C5 has effectively already landed, in its most minimal possible form, as a
side effect of the light being state-driven. That is why the ruling's instruction is genuinely a
one-line change and not a feature: the mechanism the DD endorses is already the mechanism.

## 2. Measured, so nobody has to eyeball it

| | hue | chroma |
|---|---:|---:|
| current drain `Color(0.30, 0.34, 0.48)` | **284.4°** | 22.17 |
| DD's measured death frame `#0e121d` | 280.2° | 8.06 |
| the room with **no TV light at all**, graded | **269.8–272.8°** | 6.9–7.8 |
| target swatch `#0F1108` | 120.3° | 4.30 |

The drain at 284.4° and the measured frame at 280.2° agree closely. Your drain is the dominant
cause. Good.

## 3. The correction — expect less than the ruling implies

**The room's own graded cast is already blue at ~270° and chroma ~7, with every screen dark and
`TvLight` disabled entirely.** That is my R23/R26 conformance set, captured twice, byte-identical
across runs, and it is the open item in the C20 grade session.

So the death frame at 280.2° is **your drain plus a blue floor that is already there**, not your
drain alone. Two blue contributions, compounding.

**Consequence: swapping the colour will not make the frame read olive.** The drain runs at intensity
**0.10**. For scale — the phosphor green idle runs at **0.5**, five times brighter, and it only
rotates the room from 270° to 172–204°. It does not make the room green; it makes it teal. A light at
one fifth of that intensity cannot pull a chroma-7 baked cast across 150° of hue.

Predicted result of the retarget alone: the frame moves **from navy toward neutral**, ending cool but
markedly less blue. Not olive. That is still a clear improvement and worth doing — Law 1.1's failure
mode is a *blue-tinted room*, and this removes your contribution to it. But if the expectation is
"the death frame reads olive", that needs the grade fixed too, and the grade is already before the
DD as C20.

I would rather set that expectation now than have the retarget land and read as a failed fix.

## 4. Proposed value

```csharp
tvLight?.SetRest(new Color(0.42f, 0.48f, 0.23f), 0.10f);
```

- Preserves `#0F1108`'s channel order **G > R > B** — genuinely olive, hue **116.4°** against the
  swatch's 120.3°.
- **Same peak magnitude (0.48) and the same 0.10 intensity** as the current drain, so "cold and dark,
  barely lit" is untouched. Only the hue moves — 168°, straight out of the blue quadrant.
- Using `#0F1108`'s raw channel values as a light colour would give `(0.06, 0.07, 0.03)`, which at
  0.10 intensity emits essentially nothing. A swatch is a surface reflectance; a light needs the hue
  at a usable magnitude. Hence the renormalisation rather than a literal transcription.

Adjust freely — the constraint the ruling sets is the hue, and any value with G > R > B at this
magnitude satisfies it.

## 5. Not in scope here, but adjacent

`_emissRest = new Color(0.008f, 0.010f, 0.018f)` on line 1270 is also B > G > R — the panel's own
rest emission is cool too. It is far dimmer than the light and I have not measured its contribution
separately, but if you are in this block anyway it is the same question one line up.

## 6. Offer

Once you land it, I will re-measure the death frame with the same instrument the DD cross-checked
against — `tools/room_gate_check.py --conformance`, which reports per-surface chroma and hue in
CIELAB. It runs editor-free. That gives the DD a before/after in the same units as the ruling
instead of another eyeball, and it will show exactly how much of the residual is the grade.
