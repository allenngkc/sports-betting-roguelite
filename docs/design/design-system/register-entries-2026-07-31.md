# Register entries — 2026-07-31

**Transcribe these into `main-2/docs/design/REGISTER.md`.** This seat could not write them there:
the `main-2`, `tv-sweat` and `room-refinement` worktrees are not mounted (see *Blocked* at the foot).

---

## S11-A — SureThing production typeface. **RULED.**

**State change:** OPEN (Bell Centennial, licence-encumbered) → **CLOSED**.

**Ruling.** SureThing's production faces are **Archivo** (data) and **Archivo Narrow** (condensed),
SIL OFL 1.1. Bell Centennial is dropped and is not to be revived. `LaptopScreen.LoadFont` resolves
to these two TextMeshPro assets and nothing else.

**Reasoning, so this is not re-litigated.** Bell Centennial was chosen for its ink traps, which
existed to hold small type together on cheap absorbent paper. No OFL face reproduces ink traps, so
substituting on appearance would be cargo-cult. This picks for the *function* those traps served —
small type surviving a degraded surface — because our equivalent problem is small type on an angled,
graded, bloomed screen read at reduced scale. Archivo is drawn specifically for performance at small
sizes; it ships a true condensed sibling on shared metrics and one hand, so the document's two voices
are one type system rather than a pairing; it carries tabular figures in both widths, which the
fact floor and the change-in-place figures both need; weight 400–700 gives a channel besides dimming;
and it appears on neither exhausted-defaults list (SHARED-SPEC's, nor TV `DESIGN.md` §5's).

**Consequence for the TV.** The TV face stays open and **must not be Archivo.** The two screens are
required to feel like the same hand doing different jobs; sharing one superfamily collapses that.
Raise the TV pick as its own item.

---

## S12 — SureThing form-guide identity. **SPEC ISSUED.**

**State change:** BLOCKED (lead reports "competent dark app") → **SPEC READY FOR ASSIGNMENT**.

The lead's three named absences are three concrete build gaps, not a taste problem. Each is already
specified; none needs new design.

**1. "Default sans" → the two-voice type system is not wired.** `LegacyRuntime.ttf` renders every
string in one neutral roman, which alone destroys the form-guide read: the printed-directory quality
comes from *condensed figures against roman labels*, not from the colours. Fix with S11-A —
`--font-data` Archivo for running text, labels, records, reasons, market navigation, OS chrome;
`--font-cond` Archivo Narrow for the masthead, bank/target figures, team names, prices, margin legs
and action labels. Apply the tracking scale (`.03em` names and prices → `.15em` margin header); short
labels are tracked uppercase, factual copy stays literal.

**2. "Airy rows" → the density geometry is not being honoured.** The band map is locked at
34 + 38 + 68 + 530 + 34 = 704, split 700px house form / 324px player margin. Inside the form: a 26px
column head, then six **78px** two-line entries on a 30px number / flexible matchup / 112px price /
78px More grid, separated by 1px `rule-soft`, with 30px price cells stacked at 8px. If rows read
airy, entry height or the line-box geometry has drifted. The current 660px board with a right-hand
slip is pre-contract and is the likely cause.

**3. "No toner quality" → the document layer is absent.** This is the largest of the three. Required,
in order of effect: ground `#16160F` (a *warm olive-black*, not neutral dark grey) with the
`ground-2` / `ground-3` value steps carrying recessed and raised bands; toner `#D9D4C5` (warm bone,
**not white**) with `toner-2` / `toner-3` beneath it; solid 1–2px `rule` structure with no hairlines;
the local toner-grain layer at `0.05` opacity, sitting *beneath* the room's unified grade and never
substituting for it; the faint 90° biro wash on a marked entry; and selection drawn as the tinted
ink-sprite ring — never a pill, fill, or accent background.

**Reference implementation.** All three are built and running in the design system:
`ui_kits/surething/` (the whole lobby at 1024 × 704), `components/form/`, `components/os-chrome/`,
`tokens/palette-surething.css`, `tokens/space.css`. Hand these over rather than re-deriving.

---

## S13 — Lost-ticket oxide red in Old Slips. **RULED: VIOLATION.**

**State change:** OPEN (pending director) → **CLOSED, remediation assigned**.

`OldSlipsApp.BuildLedgerTicket` tints a lost ticket's **state** *and* **payout** in oxide. This
breaks two laws, and the payout is the worse of the two.

- **The red law (S3, as amended 2026-07-30).** Oxide is the mark the house *makes*: the stamp on a
  blocked action, the strike through a dead leg. It is "never decoration and never a general 'bad'
  tint — it appears only where the house has acted." Tinting a settled state word red is exactly a
  general bad tint. The house's action here is the **strike**, not a colour wash.
- **The two-ink rule (S2).** Wax is money. A payout figure wearing oxide puts a non-money colour on
  a money slot, which is the failure the two-ink rule exists to prevent.

**Correct treatment.** Terminal state stays the literal word `LOST`, struck through in oxide — the
strike is the house's mark and carries the state without the colour, satisfying status-never-colour-
alone. The row drops toward ground (`--toner-3`). The returned figure reads `$0` in `--toner-3`:
**not oxide** (no money event happened) and **not wax** (there is no money to name). Wax on a
returned figure is reserved for a return that actually exists — a won or cashed-out ticket.

Reference implementation: `components/records/LedgerEntry.jsx`, already compliant.

---

## S14 — LEDGER / Old Slips / SURETHING LEDGER naming clash. **RULED.**

**State change:** OPEN → **CLOSED. Copy may harden (S9 unblocked).**

One player-facing name: **LEDGER**.

- **`LEDGER`** — the tray chip. The Ledger is a *peer app on his machine*, alongside SureThing and
  Messages, per the tray anatomy and the "Old Slips maps to Ledger and stays read-only" mapping.
- **`Ledger`** — the masthead title while it is open. Its dateline names what it records
  (`… · SETTLED RECORD`), never `PRICES FINAL`, which is a betting-board fact.
- **`Old Slips`** — retired from all player-facing copy. Retain as the runtime class identifier only,
  until a rename is cheap; it must not appear on screen.
- **`SURETHING LEDGER`** — deleted. It welds the sportsbook's brand onto a machine-level app and
  breaks the personal-machine rule: the OS is his, and SureThing is one app running on it. A second
  branded surface makes the machine read as SureThing-issued hardware, which is the exact drift that
  turns the laptop into a second TV.

Applied in `ui_kits/surething/` this turn.

---

## T-slip — TV slip-strip raw-hex colours (inbox item 6). **RULED: VIOLATION, same class as T8.**

**State change:** LOGGED (`[TV] DESIGN.md` §9A item 5) → **CLOSED, remediation assigned**.

`UpdateSlipStrip` embeds `#3CE873`, `#FF4038` and `#9EDCF6` as rich-text string markup. Two separate
retired languages, hidden from field-level palette scans by being string literals:

- **`#3CE873` / `#FF4038`** are the retired green/red money language. §4 retired it game-wide for the
  TV: money-good and money-bad are carried by **gold at L3** and by **L0 extinguishing**, not by hue.
- **`#9EDCF6`** is `chromeCyan`, a role from the *previous* palette that §4 does not have. Context is
  **grey** at L2. §9A item 2 already records cyan doing general duty across the surface as debt.

Remediate as T8 was: delete the literals, route the strip through the palette roles, and add a scan
that catches colours embedded in rich-text markup, not only in serialised colour fields — that blind
spot is what let this survive the T8 pass.

---

## Blocked — could not be actioned this run

The `main-2`, `tv-sweat` and `room-refinement` worktrees are **not mounted in this seat**. The
inheritance bundle that was previously mounted is also gone. I can read nothing beyond the inbox text
itself, so the following are untouched rather than deferred by judgement:

| Item | Needs |
|---|---|
| 1 — C3 HDR coverage rule (gates TV Phase 3D) | `tv-sweat/docs/tv-sweat-refinement/c3-hdr-canvas-proposal.md` |
| 4, 5 — R9 ambient rebalance, R10 couch-corner grazing | `room-refinement/docs/6-memo/2026-07-31-room-to-design-director-R9-R10.md` |
| 9 — Win-probability display and momentum tape (gates 3C) | PRD §4.2 and §8.4 verbatim |
| 2 — Studio art-authority gap | `REGISTER.md` + `STATUS.md`, to scope it against what is already governed |
| Backlog — S6/S7/S8, R5/R6, T6 evidence | the same |

Ruling on any of these from the inbox summary alone would mean inventing the content of documents
written specifically for this seat to read. Not done.
