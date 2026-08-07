# SureThing — the laptop surface

**Owning document** under C9's two-tier authority · **Status:** DRAFT for Allen · **Drafted:** Design Director, 2026-08-06
**Canonical home on approval:** `main-2/docs/design/surething-design.md`
**Companion:** `docs/design/constitution.md` (authority and evidence) · **Sibling:** `docs/design/room-design.md` (R13)

---

## 0. Scope and precedence

This is the binding art authority for **the laptop** — the SureThing sportsbook app, the LEDGER, and
the NOTEBOOK OS chrome the two run inside. It carries the colour, type, layout and state law that the
constitution deliberately excludes.

Precedence: Allen → the constitution (authority and evidence) → **this document** (everything about
this surface) → the register's ruling for the item → the slice's specs.

Every clause below is a transcription of a ruled register row. Where a clause states a value, that
value has been measured on a rendered frame, and the frame is named. **Nothing here is new law.**

---

## 1. What this surface is

The occupant's own cheap machine at 2 a.m. **The Annotated Form Guide** (S1, approved 2026-07-28,
world 4 of 7): the house prints a dense inverted betting form; the player compares it, circles prices
in ballpoint, works the right margin, commits. **Selection is annotation.** The document never changes;
only his marks accumulate.

### 1.1 The register split — the constraint most likely to be got wrong

| | The laptop | The TV |
|---|---|---|
| Object | **his own machine** — personal, chosen, cheaper, grubbier, possibly customised | a hardened display **an institution bolted to the wall** |
| Register | **Calm.** A tool you operate. | **Loud.** An instrument you watch. |
| Owns | slate, markets, working slip, stake, staging, lock, shop, placed tickets | unrevealed drama: score, clock, probability, outcome reveals |
| Motion | laid ink — continuous, hand-paced | panel refresh — quantised, discrete |

**Do not carry the TV's vocabulary here.** Not its coarse grid, monumental type, institutional steel,
brightness-only semantics or un-eased motion. Anything on this surface built from that vocabulary has
become a second TV and has failed.

MY BETS may only mirror what the TV has already revealed (S35c). It never reads engine state and never
runs ahead of the broadcast. **The laptop decides; the TV reveals.**

### 1.2 Ownership boundary

**The house owns the app. The player owns the machine.** (S44) The wallpaper, the rail, the tray, the
icon set and the sticker are his; SureThing and LEDGER are what he runs on it. The machine never wears
the house's brand — the wordmark is drawn in biro, and biro is only ever what *he* chose.

---

## 2. Canvas and bands — locked

**1024 × 704**, one fixed composition on a ~0.32 × 0.22 m world-space surface, read at an angle
(S2). Not responsive, not a web page.

```
 34  OS rail          --st-band-rail
 38  app tabs         --st-band-tabs
 68  masthead         --st-band-mast
530  work area        --st-band-body     = 700 house form + 324 player margin
 34  OS tray          --st-band-tray
───
704
```

**A locked band is not headroom** (R30). No band is spare space regardless of what it currently draws —
the tray draws little and carries the entire personal-machine register. A proposal to grow into a band
**cites the band by name**.

**Yield order for any layout deficit: spacing, then repetition, then nothing** (S50). Nothing that
states a product fact is deleted to make a layout fit, and no hierarchy is reordered for a shortfall
(T51). A deficit that survives that list returns to the DD seat.

Re-deriving a fixed grid constant **once at design time** is legal (T51, S40). A zone **resizing in
response to content at runtime** is not.

### 2.1 The margin

324px, ruled paper at **26px** (S34 — measured on frame `15-ledger-across-rounds`, rules at
y 425/451/477/503, identical on working and passive margins). Vertical order never changes:

**header → legs → combined → stake → payout → actions.**

The action stack is **anchored** (T47). It does not flow: an un-anchored stack moves LOCK IT IN
depending on how many legs were marked — the most consequential control in the game moving because you
bet more. The flow region above it is bounded by MaxLegs = 4 and the reserved height is re-derived in
the same commit as any change to that cap.

The margin **does not scroll**. Interior market lists do (S25-am), with S27's rail.

### 2.2 Control sizes — exact

`--st-place-h 44 / min 200` · `--st-lock-h 52 / min 280` · `--st-skip-h 34 / min 230` ·
`--st-price 96×30` (kit) / `112×32` (runtime) · `--st-more 74×44` · `--st-tab-h 27` ·
`--st-quick 68×32` · `--st-nudge 88×32` · `--st-rub 60×32` · `--st-entry-h 78`.

`--radius: 0`. **There are no rounded corners and no cards on this surface.**

RUB OUT stays 60×32 with its word: an explicit removal target, never a tiny unlabelled ×. A mis-click
here costs money.

Ink geometry: **ring = cell + 16px, offset −8/−8**, additive, derived from the real control's measured
rect — never proportional, never eyeballed.

---

## 3. Colour

### 3.1 The two-ink rule — the surface's first law

| Ink | Value | Means | Never |
|---|---|---|---|
| **Wax** | `#D9A441` | money and the primary action | mood, celebration, emphasis |
| **Biro** | `#5E86B8` | anything **he** chose | a product fact, a primary action (S18) |
| **Stamp** | `#B4483A` | the house acting on the document | generic loss, generic error, decoration |

**Nothing else may borrow any of the three.** `--wax-ink #1A1305` is the type punched out of a solid
wax field; `--wax-deep #8A6620` is the 2px pressed edge.

Money grammar, ratified on frames and **not to be "fixed"**:
- **Stake reads toner; payout reads wax.** In the MY BETS tally that is `AT RISK` toner (218,213,198)
  and `IF EVERYTHING LANDS` wax (220,167,65) — the working margin's own grammar, one screen over.
- **`$0` wears wax in a tally** (a sum of zero is money arithmetic) and **`--toner-3` in a row**
  (a dead record is drained). Both correct (batch 10).

### 3.2 Grounds and toner

`--ground #16160F` canvas · `--ground-2 #1C1C13` recessed · `--ground-3 #232319` raised ·
`--rule #3C3C2C` · `--rule-soft #2C2C20`
`--toner #D9D4C5` facts · `--toner-2 #9C9888` secondary · `--toner-3 #6E6B5E` labels, **floor for
readable text**

**No pure black anywhere** (S2). The ground is lifted so the room's shadows stay darker than the
laptop — the strongest belongs / does-not-belong signal in the project.

**Every screen shares `--ground`.** The verdict is the app's last screen, not a different product
(S53-am): measured 22,22,13 on `13-verdict-run-won`, matching the desktop's own ground. **No screen
authors a bespoke ground.**

### 3.3 Status is never colour alone (S2)

Every state carries a mark, glyph, word, border or position change as well as its tone.

| State | Carried by |
|---|---|
| Selected | biro ring over the figure + a leg in the margin + the count |
| Replacement | biro ⇄ + dashed underline — **never** rendered disabled |
| Blocked | muted label + stamped literal reason ≥13px, cause **and** remedy, inside the control |
| GREEN | the word + wax figure + ring re-inked in wax |
| DEAD | the word in `--toner-3` + the oxide strike across it + the row drained to .55 |

Measured on `05-my-bets-green-dead`: DEAD glyph rows at (110,106,95), strike rows at (181,74,59).
**Only the strike is oxide** (S15-am). Leg sub-rows carry **no per-outcome hue** at all (S35c, S40).

---

## 4. Type

### 4.1 Faces

**Archivo + Archivo Narrow**, SIL OFL 1.1 (S11). One superfamily, shared metrics, tabular figures in
both widths. Condensed for figures, prices, names, masthead, action labels; roman for running text and
labels. Resolved through `LaptopScreen.LoadFont` alone.

No licence-encumbered typeface ships in this product, ever (Allen, S11).

### 4.2 Scale and the fact floor

`31` payout · `26` masthead / stake · `21` bank, target · `19` names, prices · `16` legs, actions ·
**`13` product-fact floor** · `12` OS chrome only.

**13px is the floor for any text a player must read to make a decision.** Prices, records, field keys,
state words, disabled reasons, market navigation. 12px exists only for OS chrome carrying no product
meaning — the tray's `DISK 61% FULL`, the clock.

`NOT INSTALLED` sits at the machine's own register and renders at the tray's tone (measured 111,107,96
= `--toner-3`, identical to the tray's system facts). It states what is **true**, never what is planned.

Every critical value survives a 50% thumbnail check.

### 4.3 Tracking

`.03` names/prices/masthead · `.08` records · `.11` tabs · `.12` field keys · `.14` actions ·
`.15` margin header. Signed numbers use **U+2212 MINUS**, no per-region exception (S30).

Tracking is a **signed C14 deviation** in `UI.Text` (S28), expiring at the C15 TMP migration. Signed
only where the colour split and two-voice type split are 1:1.

---

## 5. Chrome and destinations

**One `NotebookChrome`**, consumed by the app, the LEDGER, the desktop and the verdict alike (S48,
S52 — rail band 100% pixel-identical desktop-vs-app across 17,408 samples; tray likewise).

- **Rail** — identity mark, his sticker, clock, battery. 12px. Never institutional hardware.
- **Tabs** — FORM · ENTRY · MY BETS · REWARDS. **No tab is active on LEDGER** (S31-am): the LEDGER is
  a destination on the machine, not a section of the sportsbook — the strip persists non-interactive
  at `--toner-3`.
- **Masthead** — carries **the run's scope**. Board header carries **the screen's scope**. **Nothing
  restates either** (S37). The live round number appears exactly once.
- **Tray** — other apps, non-product system facts.

**The chrome is the argument.** It renders on every screen including the verdict (S55). A full-screen
takeover is a game-over card, and this product does not have those.

### 5.1 One name

**SURETHING**, everywhere the player sees it — icon, tray, masthead (S46). `Sportsbook` and the
taskbar full stop are gone; FORM is a screen, not part of the name. **LEDGER** is the one name for the
settled record (S16); "Old Slips" and "SURETHING LEDGER" are retired from copy. Code identifiers exempt.

### 5.2 The desktop

Wallpaper is the lifted ground plus toner grain, nothing above the fact floor. **The machine does not
wear the house's brand** (S44). MAIL and BANK are **required, not tolerated** — dead apps are the
machine having a life outside the app (S49-au).

Two icon states, two channels, both measured on `11-desktop`: glyph tone **220 vs 112**, and the
printed word at **85 above ground** where the retired chip managed 3 (S47, S56).

---

## 6. Voice

**The number never lies.** Copy is impersonal and transactional — it names the thing, not the reader.
"2 selections", not "you've picked 2". Second person only in genuine instructions.

First person appears **exactly once** in the whole surface, and it is not the product speaking — it is
him, in the column he owns: **MY MARKS**.

Personality: incisive, nocturnal, dry, orderly. Never celebratory of gambling, never fake urgency,
never an implied guaranteed win **in any voice** (S45).

Satire is permitted only in non-critical flavour labels that state no product fact — the sticker, a
tray readout, a relic description. **It never occupies a slot where a fact belongs**, and on an
otherwise empty screen the slot is the whole screen (S45).

Fictional leagues, teams and players only.

Errors state **cause and remedy, in place, at 13px, inside the house's stamp** — and inside the control
they explain (T47). Never a tooltip, never colour alone, never a remedy the engine did not supply.

---

## 7. Motion

**The laid-ink rule.** Continuous, hand-paced, caused by marking the document. **No duration and no
easing curve is specified, deliberately** — do not invent a motion system here.

Leg goes live → the entry lifts toward toner, his ring holds. Wins → the figure fills wax, the ring is
re-inked over it. Dies → the strike is drawn across, the entry drops toward ground. Return changes →
the tally is crossed out and rewritten beneath, in the same ink. PLACE press → 2px down, the wax-deep
edge drops.

Banned: confetti, pulse loops, casino urgency, celebration, any full-field wash.

---

## 8. Out of bounds

1. **The modern sportsbook app** — navy ground, one saturated accent, rounded odds pills, a floating
   betslip drawer, promo rails. The superseded violet package shipped exactly this; it is the
   anti-reference.
2. **The retro terminal costume** — phosphor green, scanlines, ASCII borders.
3. **Cyberpunk neon-on-black** — the city is neon; **his cheap laptop is not.**
4. **Skeuomorphic kitsch** — torn edges, drop-shadowed paper, faux stitching.

Also banned outright: colour-only status · sub-floor product text · low-opacity facts · hairline
essential strokes · pure black · a disabled odds control (v0 has no limiting, so no market can honestly
be unavailable — the correct treatment is *replacement*) · a padlock on an alternative price ·
a toast over a read-only mirror (S35b) · promotional rails · acquisition art.

---

## 9. Gates

Real gates, per C9. Each states its instrument and, per C18 §4.2, **what it cannot see**.

| # | Gate | Instrument | Blind to |
|---|---|---|---|
| L1 | Band arithmetic sums to 704; every band named | layout assert, canvas-local px | rendered glyph bleed |
| L2 | No text below 13px states a product fact | rendered measurement at review distance | strings not exercised by a capture state |
| L3 | Two-ink conformance — no wax, biro or stamp outside its meaning | palette scan incl. markup + rendered frame | HDR emission (§10, no capture path exists) |
| L4 | Every state carries ≥2 channels | rendered frames, both states side by side | states without a forced capture |
| L5 | Chrome is pixel-identical across all destinations | per-band sample comparison | anything outside the two bands |
| L6 | Anchored stack never moves at MaxLegs with a staged receipt | PlayMode margin invariant | horizontal collisions, z-order, `Graphic`-less elements |
| L7 | Every priced offer reachable (C19) | list-length vs engine offer count | whether the rail rendered |

Every invocation reports its executed case count and exits non-zero on zero cases (C29).

---

## 10. Open items

| Item | State |
|---|---|
| **S59** verdict hierarchy — lost headline dimmer than its own subline | Ruled batch 11; unbuilt |
| **S60** MY BETS margin header renders toner, not biro (S33) | Ruled batch 11; unbuilt |
| **S61** read-only scope stated four times on MY BETS (S37) | Ruled batch 11; unbuilt |
| **S62** `TICKET 1.0` — zero-indexed, decimal-ambiguous ticket identity | Ruled batch 11; unbuilt |
| **S63** `attentionEmission` saturated violet on the lid | Struck batch 11; replacement waits on a frame that does not yet exist |
| **C15** TMP migration — S28/S29 deviations expire at it | Scheduled, orchestrator-side |
| **S10** sweat "loud register" for the laptop | Candidate; no built spec |

**Not open:** S53-am, S55, S56, S57, S58 (figures), S34, S49 — all granted on the batch-10 frames.
