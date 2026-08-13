# The room's warm band — derivation, with every member named

**Room lead · 2026-08-12 · `4bca23d` · desk work, no editor.** Discharges batch 51's ruled
re-derivation. **For DD adjudication.**

**Headline: there is no single band.** The warm members are **bimodal**, with a **14.8° gap that no
member occupies**. Any interval spanning both groups admits that gap and certifies nothing — the DD's
own test, applied. **Two populations, and the split is NOT lights-vs-screens.**

**And the withdrawn band matches neither population:** `85–92°` starts inside the low cluster and ends
inside the empty gap. It was never an interval over these members, which is consistent with both of
its endpoints having been underived.

---

## 1. Membership — every member, its value, space, provenance and surface (requirement 1)

**Space for every row: CIELAB hue angle, computed from LINEAR values through the room's shared
`linear_to_lab` (C33-am3).** Chroma beside every hue.

### Authored — room lights (`GrayboxRoomBuilder`)

| member | light type | authored linear RGB | hue | chroma | class |
|---|---|---|---|---|---|
| **FluorescentBounce** | Point | `(0.85, 0.80, 0.45)` | **102.8°** | 26.94 | warm |
| **FluorescentKey** | Spot | `(0.92, 0.86, 0.42)` | **102.7°** | 33.32 | warm |
| **DeskLampLight** | Spot | `(1.00, 0.82, 0.52)` | **87.9°** | 23.49 | warm |
| **PhoneBuzzLight** | Point | `(1, 0.842, 0.632)` — *derived: emitter ÷ R* | **83.3°** | 16.10 | warm |
| CouchGraze | Spot | `(0.70, 0.74, 0.80)` | 265.3° | 4.47 | cool — excluded |
| MoonDirectional | Directional | `(0.55, 0.62, 0.85)` | 279.1° | 16.67 | cool — excluded |
| WindowGlowLight | Point | `(0.40, 0.56, 0.92)` | 273.6° | 27.02 | cool — excluded |
| TvLight (rest) | Point | `(0.35, 1.00, 0.50)` | 150.7° | 48.18 | **green — in neither warm group** |

### Authored — screen emitters

| member | authored linear RGB | hue | chroma |
|---|---|---|---|
| **shared screen emitter** — `LaptopScreen.GrantedLidEmission`, and `PhoneScreen.RestEmission` is *the same constant* | `(0.038, 0.032, 0.024)` | **83.3°** | 5.41 |

### Rendered — where a rendered figure exists

| member | value | surface / frame |
|---|---|---|
| screens, room cast | 84.3–85.4° | batch 13/15 records (lid 85.1–85.3, phone 85.4, laptop 84.3) |
| **FluorescentKey contribution** | **101.4°** (chroma 7.26) | `BASE − NOKEY`, pool core `(160,1340)-(240,1420)`, standing pose, four-half set 2026-08-12 |
| " (wider box) | 103.0° (chroma 3.76) | same set, `(80,1260)-(320,1440)` |
| lid contribution | **unmeasurable** (chroma 0.03) | `BASE − NOLID`; the emission does not reach the room (R40-cl) |
| *settlement re-tint (the subject, not a member)* | 87.3° / 92.7° | `GLOW − BASE`, ceiling / wall-right, same set |

## 2. The population question — decided, and the offered direction declined (requirement 2)

**The fork the DD named was lights-vs-screens. The evidence falsifies that as the organizing
principle**, and does so structurally rather than by a close call:

> **`PhoneBuzzLight` is a LIGHT and it sits at 83.3° — the screen emitter's hue exactly**, to four
> decimal places (`83.3473` vs `83.3473`, Δ `0.0000°`). Not a coincidence: it is authored as
> `(1, rest.g/rest.r, rest.b/rest.r)`, i.e. the screen emitter scaled to R = 1. **CIELAB hue is
> invariant under uniform scaling of a linear triple** (X, Y, Z all scale by k; in the cube-root
> regime a\* and b\* both scale by k^⅓, so their ratio — the hue angle — is unchanged; chroma is not,
> which is why 16.10 ≠ 5.41). A light and the screens therefore share a hue **by construction**.

So the warm members do not divide into lights and screens. **They divide by fixture family:**

| population | members | span | independent authored values |
|---|---|---|---|
| **LOW** | shared screen emitter 83.3° · PhoneBuzzLight 83.3° · DeskLampLight 87.9° *(+ rendered screens 84.3–85.4°)* | **83.3–87.9°**, width 4.6° | **2** (the emitter; the desk lamp) |
| **HIGH** | FluorescentKey 102.7° · FluorescentBounce 102.8° *(+ rendered key 101.4°)* | **102.7–102.8°**, width 0.1° | **1** (the bounce is authored to match its own key — one fixture, not two witnesses) |

**Largest gap: 14.8°, between 87.9° and 102.7°. No member occupies it.**

**Decision: TWO populations.** A 19.5°-wide interval over both would be, in the DD's own words, *a
bound so loose it certifies nothing* — and §3 shows exactly that.

## 3. C44's practice, run and recorded beside the bound (requirement 3)

Each candidate band, fed its own founding values:

| candidate | width | members failing | note |
|---|---|---|---|
| **A** — all warm members `[83.3, 102.8]` | 19.5° | **0** | passes only because it spans everything — **admits the 14.8° gap no member occupies** |
| **B** — LOW cluster `[83.3, 87.9]` | 4.6° | 2 — both fluorescents | coherent band; the HIGH family is simply not in it |
| **C** — fluorescent family `[102.7, 102.8]` | 0.1° | 3 — emitter, desk lamp, buzz light | coherent band; the LOW cluster is not in it |

**A is vacuous, B and C are each coherent over their own population.** That is the same result stated
three ways: no single band survives its own founding values without admitting a region the room never
exhibits.

**Process finding, as the ruling asks (not a new law):** had C44's practice been run when C44 was
ruled, this would have surfaced then. It costs three lines of arithmetic and needs no capture.

## 4. The derived band

**For a warm-room-palette question, the derived band is the LOW cluster:**

> ### **83.3 – 87.9°** (CIELAB on linear), width 4.6°
> **Members:** shared screen emitter `83.3°` · PhoneBuzzLight `83.3°` · DeskLampLight `87.9°`.
> **Corroborated rendered:** screens 84.3–85.4°, inside it.
> **C44:** all three founding members pass; the two fluorescents are excluded *by population*, not by
> failure.

**The fluorescent family is a second band, `102.7–102.8°`,** and it is one fixture. **Recommended, not
ruled:** a single fixture's colour is a poor bound in either direction — it has no spread to
generalise from, and its own comment describes it as a deliberately *"sick yellow-green"*, i.e. a
character choice rather than a palette centre.

**Why the old `85–92°` matched neither:** its bottom sits inside the LOW cluster and its top sits
**inside the empty gap**. It was not an interval over any population of members — which is what
"underived" looks like once the members are listed.

## 5. What this does and does not do to T65

**It does not adjudicate the subject, and must not be read as doing so.** The settlement re-tint's
rendered cast measured **87.3° / 92.7°**, which straddles the derived band's top — but disposition 3's
blockers are untouched by this derivation:

- the subject's own two-box spread is **5.4°**, larger than the derived band's whole width (4.6°);
- the low anchor is still unmeasurable in a rendered frame (chroma 0.03).

**T65's subject stays unadjudicated**, exactly as batch 51 ruled, and a band moving underneath it is
not a pass. What the derivation changes is only *what it would be measured against* once the
instrument can resolve it.

## 6. Scope (C25)

*Reads:* every `Light` colour in `GrayboxRoomBuilder` and the shared screen-emitter constant at
`4bca23d`, all converted by the room's own `linear_to_lab`; rendered figures from batch 13/15 records
and this lane's four-half set. *Cannot see:* whether any warm member exists outside those two
sources — a light created elsewhere, or an emitter I did not enumerate, would change the populations;
the derivation is only as complete as that enumeration, which is why every member is listed rather
than summarised. **No frame is missing for this derivation** — nothing here needed a capture that does
not exist, so nothing is owed before adjudication.
