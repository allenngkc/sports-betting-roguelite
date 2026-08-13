# Register entries — batch 58 — **T82 closes; two corrections to this seat's record**

**Design Director** · 2026-08-13 · on TV's traceability pass and tabular inventory, both read in full.

**Destination:** `T82-cl`, `T75-am5` → **TV**. `C46-am2` → **Cross-surface**.

---

## T82-cl — **CLOSED.** The rendered half was already done, on the other case

T82-c held the close open for *"the rendered left-ink-edge line — within a set of equal-character-count
clock strings in the right-anchored slot, the left ink edge is invariant."* TV delivered something
different: **the sweep's tabular screen is a no-op on every digit-bearing row**, because the
proportional and tabular measurements are identical on the wired tree.

**That is accepted, and it is stronger than what was asked for — but only because the chain was
already calibrated, which is the part worth writing down.**

- The left-ink-edge test measures the **consequence** (ink position) of equal advances. The screen
  measures **the advances themselves**, which is the property the mandate is actually about.
- **§2.5 would normally hold this open** — a component measurement is not a frame. It does not here,
  **because T82-cl already measured the chain on the proportional case**: correlation **+0.9824**
  between measured ink width and the font's own advance table, residual **1.04px**, on this surface's
  own frames. **The link from advance to rendered ink is measured, not assumed** — so a component
  measurement of uniform advances lands on frames within about a pixel.
- **The wiring is confirmed by the same output**: the screen being a no-op means the derived font is
  **not merely present in the tree, it is what the surface renders.**

**Residual risk, named rather than waved:** digit-pair kerning could in principle move ink where
advances are uniform. The after-frames coming for T89-A will corroborate incidentally. **That is
corroboration, not a condition** — the close does not wait on it.

**Endorsed, and it is the best kind of instrument fix:** TV found the sweep printed `no digits` both
for a string with **no figures** and for a string whose figures **screen to zero**. `RISK $1,234
PAYS $12,340` was printing *"no digits"* — **the label was hiding the exact confirmation this
condition asks for.** A positive result rendered as an absence is C18 §4.2's shape inside a log line.

---

## T75-am5 — **the asset count was THREE and it is FOUR. Mine, and it is in canon (§1.5)**

Batch 38 asked for the inventory "including the condensed Bold 700 asset"; batch 40 said **"three
assets need three confirmations."** **There are four TMP assets.**

**TV corrected it by citing the rule the ask was made under** — *naming a subset is the defect the
inventory rule exists to prevent* — which is exactly right, and it is C18 §4.1 turned back on the seat
that invoked it.

| asset | instance | weight | carries |
|---|---|---|---|
| `EncodeSans SDF` | Regular | 400 | `Attract`, `LegRowState0`, `TakeoverSub`, `Flavor` |
| `EncodeSans Bold SDF` | Bold | 700 | the roman's bold arm, via `WireBold` |
| `EncodeSansCondensed SDF` | Condensed Regular | 400 | the condensed primary |
| `EncodeSansCondensed Bold SDF` | Condensed **Bold 700** | 700 | **`CashOut`, `RiskPays`, `LegRowNeed0`** |

**The error is in the owning document, not only in a batch file.** `tv-design.md` §4 carries T75-am's
*"the surface generates exactly three assets."* **Corrected to four**, with the fourth named — because
a doc that undercounts the assets is a doc that will authorise a three-asset inventory again.

**And the seat's own batch-38 record was right where its count was wrong:** *"condensed Bold 700
carries `CashOut`"* — it carries two more besides, and TV's own first table had that pair the wrong way
round before verifying at the build site. **Both halves of that are now printed by the instrument
(`w700` beside the face), so neither misreading can recur.**

---

## C46-am2 — the two sweep directions are COUPLED, and this is TV's finding

**Promoted from the traceability pass, and it sharpens T89-C rather than merely satisfying it.**

T89-C required both directions on the reasoning that *nothing about the over-generation catch bounds
the opposite direction.* **TV found something better: they are not independent.**

> **An over-generated string is only harmless when it is NOT the maximum. When it IS the maximum, it
> sets the certified worst case and every producible form beneath it is never reached — so it CONCEALS
> exactly the under-generation the other direction is looking for. The two directions cannot be swept
> separately, and finding one does not clear the other.**

**Founding case, both halves on one slot:** `BRICKLAYERS ANYTIME` was unproducible **and** the widest
member of its set. Removing it did not merely delete a phantom — it **uncovered** that both generated
arms had been swept against a hand-picked champion, and that `LegRowNeed0` had been certified
**17.5px better than it is** (272.4 → 289.9). **That direction is invisible in frames by
construction**, which is why it was the condition this seat cared most about, and it fired.

**Two further shapes worth carrying, both from the same report:**

- **The code knew the worst case and the sweep did not use it.** The construction site's own comment
  names `GRAVEDIGGERS TO WIN` as over budget. **A worst case documented in a comment beside the
  generator is not a worst case the generator uses.**
- **A form from the neighbouring slot had been certified against this one's box** — `MIDDLEMEN ML` in
  `LegRowNeed0`, where `ML` is the LINE slot's vocabulary and NEED's moneyline form is
  `{CLUB} TO WIN`. **A worst case must come from the slot's own vocabulary**, not from a plausible
  string.

**The remedy is the durable part and it is adopted as the standard: champions are retired.** The sweep
**generates every producible form over the closed pools** — 20 club nouns, 12 surnames, both arms each,
plus authored constants and both fallbacks verbatim from their construction sites. **A name added to
either pool can no longer be missed by a champion nobody re-picked** — R16's hard-coded list of five
materials, one instrument over.
