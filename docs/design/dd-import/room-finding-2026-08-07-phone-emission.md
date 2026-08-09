# Room → Design Director — the phone's emission, and the blind spot that kept it

**From:** room lead · **2026-08-07** · filed in **C25 form** (every measurement carries its scope in
the same breath — what it reads *and* what it cannot see).

**Nothing here is fixed and no replacement colour is proposed.** S63 set that precedent: the
replacement is a design call and the lead's job was to measure and route.

---

## 1. The finding

The room owns the phone **object** (R28). Emission into the room is light, not content — S63's split
exactly, one surface over. The phone carries **three emission values and a real `Light`**, all four
blue-dominant, and **none has ever been audited.**

**Scope of this table:** authored linear values converted linear RGB → XYZ(D65) → CIELAB. Calibrated
against the register's own ratified figure — the struck laptop violet `(0.28, 0.10, 0.55)` reproduces
at **chroma 64.1 / hue 312.4°**, and the register says 64.1 / 312°. *What it reads:* the authored
value. *What it cannot see:* how any of it renders — §2 measures that separately.

| value | where | L\* | chroma | hue | order |
|---|---|---|---|---|---|
| *struck laptop violet (calibration)* | — | 48.36 | *64.1* | *312.4°* | B>R>G |
| **granted laptop idle** | `LaptopScreen.cs` | 21.09 | **5.4** | **83.3°** | **R>G>B** ✓ |
| `idleEmission` — **always on** | `PhoneScreen.cs:26` | 20.06 | **14.5** | **278.9°** | B>G>R |
| `unreadEmission` — **live in the batch-13 frame** | `PhoneScreen.cs:27` | 37.80 | **18.0** | **264.5°** | B>G>R |
| `buzzEmission` | `PhoneScreen.cs:28` | 75.22 | **31.9** | **271.4°** | B>G>R |
| `PhoneBuzzLight` colour | `GrayboxRoomBuilder.cs:894` | 90.58 | 16.3 | **241.7°** | B>G>R |

**The phone's always-on rest state carries 2.7× the chroma of the laptop's granted rest state, in the
blue quadrant §1.1 names as its failure mode.** The laptop's granted value sits at 83.3°, just red of
the room's 92° key. The phone sits at 278.9°.

`GrayboxRoomBuilder.cs:886` cites its authority inline: *"a tiny cyan/white blink is chrome, never
money-green (design/08 palette law)."* **`design/08` is T3, the deprecated anti-reference.**

## 2. Rendered

R19's `phone body (his)` box, measured with the gate harness's own `region_cast`, on two sets at the
**identical** camera pose (both eye `(0.300, 1.640, −1.400)`, +Z, 68°).

| set | L\* | chroma | hue | |
|---|---|---|---|---|
| screens-**DARK** (the only set R19 reads) | 16.66 | 1.61 | 251.8° | COOL, but chroma is barely over the 1.5 floor |
| screens-**LIT** (what ships) | **36.31** | **7.84** | **228.7°** | COOL |
| *laptop body control, dark → lit* | *52.98 → 53.11* | *20.78 → 20.69* | *78.6° → 79.0°* | *unchanged — a real body region* |

The phone's own emission **doubles its L\* and multiplies its chroma 4.9×**. The laptop-body control
is flat across the same two sets, which is what a body region looks like.

**Scope.** *What it reads:* the phone's face in the standing pose at 2.55m, on committed captures.
*What it cannot see:* whether the box is surface-pure (C27) — I have not eye-confirmed that it
excludes the shell rim, and the dark→lit delta shows it is dominated by the emissive quad either way.
It also says **nothing about the buzz**, which is a 0.55s transient that no still frame in the repo
contains.

## 3. The blind spot — and it is why this survived

**No instrument in this room reads an emission value and judges it.** Four of them look like they
would, and each is silent for its own good reason:

- **R23**, the §1.1 conformance instrument, forces all three panel emissions to **black** and disables
  the two screen-driven lights. By construction and *correctly* — it exists to separate the room's own
  cast from the screens'.
- **R33**, palette conformance, checks **which material asset** is referenced and nothing else. Its own
  blind-spot line says a renamed-but-recoloured asset passes.
- **T30**, the retired-hue scan, matches **named retired constants verbatim**. `PhoneBuzzLight` is
  `(0.55, 0.82, 1.0)`; `chromeCyan` is `(0.62, 0.86, 0.96)`. Same family, hand-typed differently,
  invisible to a verbatim match. *Scope: I cannot inspect T30's implementation — it does not live in
  this worktree. What I can state is that its recorded scope included room light colours, and the
  value is still there.*
- **R19**'s only phone region is read on the **screens-dark** set — the one state in which the phone's
  emission is off.

So the sole region that samples the phone reads it with its emission silenced, and the sole rendered
§1.1 instrument silences it on purpose. **The phone's light has never appeared in any measurement this
studio has taken.** That is how S63 could rule *"idleEmission is the same defect unaudited — fix
both"* about the laptop's two ends while a third emitter sat 15cm away, untouched.

C18 §4.2's shape: a gate that is silent about the thing it appears to cover.

## 4. Weighting, honestly

- **Strong — `idleEmission`.** Always on, chroma 14.5, blue quadrant. Needs no frame, exactly as S63
  needed none.
- **Strong — `unreadEmission`.** Not hypothetical: it was **live in the batch-13 capture**, logged at
  the moment of the shot (`phone emission at capture = 0.055, 0.105, 0.180`). During Betting, with a
  feed carrying unread items, this is the normal state.
- **Weaker — the buzz is not R37's pulse.** R37 struck a *continuously animated* light. This is a
  0.55s event flash on a message arriving. Different object, and I am not flattening them together.
  It remains blue at chroma 31.9 and it does drive a real `Light`.
- **Qualifier the DD should not have to discover:** `buzzLight` sits at intensity 0 and
  `enabled = false` at rest, so "a fifth light" is true only intermittently.

## 5. Same blind spot, one object over — the laptop's *material*

`ScreenLaptop`'s material emission (`GrayboxRoomBuilder.cs:353-354`) is `(0.025, 0.055, 0.035)` —
**hue 155.5°, chroma 13.5, G>B>R.** The granted lid colour is 83.3° / 5.4 / R≥G>B. **The material
disagrees with the ruling by 72° of hue at 2.5× the chroma.**

At runtime the property block overrides it, so the player sees the granted colour. *What does see the
material's own value:* the APV bake, and **every Edit-Mode capture**. Not fixed here — changing a
baked emission re-opens the bake and the structural gates.

## 6. What would falsify this

- If the phone's screen is judged **content** rather than light, R28's split moves and this is not the
  room's to raise. I do not think so — R28 gives the room the object, and S63 ruled the lid's emission
  was light one surface over — but that line is the DD's, not mine.
- If §1.2's *"quiet, with faint spill"* is read as permitting a cool screen provided it is dim, then
  `idleEmission` at L\* 20.06 may sit inside it and only `unread`/`buzz` are at issue.

## 7. Constraints, if useful

The phone is **his** (§6) — personal register, same as the laptop, not the institution's. §1.2 requires
screens quiet with faint spill. The laptop's granted family is warm near-neutral **R ≥ G > B**; whether
the phone joins that family or is deliberately distinct from it is the question I am not answering.

---

**Evidence.** Commits `638e592`, `10de3a0`. Gate report
`artifacts/room-visual-pass/gate-runs/2026-08-07-batch13-recertified.txt` (9 PASS, 0 FAIL, 0 VOID).
Capture sets `artifacts/room-visual-pass/batch13-poststrike/` (lit) and
`r23-conformance-postmove/` (dark). Full write-up in `[RM] docs/handoffs/room-refinement.md`, batch-13
section.
