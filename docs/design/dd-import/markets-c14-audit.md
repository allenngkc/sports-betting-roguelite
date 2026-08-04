# C14 audit — market presentation, 1:1 against the kit

**Owner:** markets/sim lead (`markets-2`) · **Date:** 2026-08-01
**Bar:** Allen's standing requirement — shipped work is a **1:1 match** to the intended
design. Deviations only where **physically impossible**, each **DD-signed before build**.
**Intended:** `main-2/docs/design/design-system/` — `ui_kits/surething/` (the runnable
reference), the component `.jsx`/`.d.ts`, the tokens, the law cards, and
`DD Ruling Batch 4.dc.html` (the full ruling text).
**Shipped:** `unity/SBR/Assets/SBR/Runtime/SportsbookApp.cs` + primitives in `LaptopOs.cs`,
at `586355f`.

Method: two read-only sub-agent sweeps (ENTRY screen; margin / MY BETS / LEDGER), every
claim carrying `file:line` on both sides, then lead verification of every citation and
every verdict against the full ruling text. Three verdicts were overturned in that pass —
recorded below rather than quietly fixed.

---

## 0. Headline

**~60 divergences from the intended design.** Two are physically impossible on the shipped
text stack; **the rest are achievable and are therefore C14 defects.** The palette (15/15
hex values) and the production faces are exact — the divergence is almost entirely
typography, structure, and copy.

The single most consequential finding is not a defect in a row of the table. It is that
**S24 and S25 conflict**, and the conflict is what produced the 6-of-14 hidden-scorer
problem I reported earlier. See §1.

### Corrections to the record — three verdicts overturned on review

1. **My "pattern-extension" flag was wrong.** I reported `BTTS — NO`, `TOTAL CORNERS` and
   `TOTAL CARDS` as extensions I had invented beyond the DS-enumerated set, and flagged
   them for sign-off. The full ruling enumerates them explicitly: *"MONEYLINE, TOTAL GOALS,
   BTTS — YES / BTTS — NO, TOTAL CORNERS, TOTAL CARDS, ANYTIME SCORER"*
   (`DD Ruling Batch 4.dc.html:428`). They are DD-verbatim. **No sign-off needed; the
   C14 concern I raised does not exist.** I had been reading the `.d.ts` files, which
   enumerate only a sample.
2. **A sweep verdict was wrong: `RIDING` is not a vocabulary violation.** The sweep marked
   the shipped `RIDING` a DEVIATION against `RevealedState.d.ts`'s union. S23 *amends* that
   union — *"PENDING · RIDING · LIVE · GREEN · DEAD · VOID · CASHED OUT"* (`:444`). The
   shipped word is correct and the `.d.ts` is the stale side. **Verdict overturned to
   MATCH**, with a docs note that the DS file still needs its amendment transcribed.
3. **My `N NOT SHOWN` colour is wrong.** The ruling says the count prints *"at the fact
   floor **in toner**"* (`:468-469`). `--toner` is `#D9D4C5` — `LaptopOs.White`. I shipped
   `LaptopOs.Muted` (`#6E6B5E`, `--toner-3`). A real deviation, mine, one constant.

---

## 1. ⚠ BLOCKING — S24 and S25 conflict, and the conflict is load-bearing

S25 states as fact: *"PlayersPerTeam = 7 fits and 9 does not; that the shipped value happens
to fit is luck, not compliance"* (`DD Ruling Batch 4.dc.html:473`).

**Under S24 that sentence is no longer true.** S24 requires the scorer board to render as a
single-column list (`:456-457`). The 14-offer board fitted the 412px body only because it
was laid out two-up. Single column at the same 42px pitch fits **8**. So S24 silently turned
S25's "7 fits" into "7 does not fit", and the two rulings were issued in the same batch
without that interaction being reconciled.

The consequence I measured and reported: the board is ordered away-roster-then-home-roster
(`Matchup.PlayerAt:283-289`), so a capacity cut at 8 shows **all 7 away players plus one
home player** — 100% of away offers, 14.3% of home. **38.9% of scorer probability mass
becomes unbettable**, and the market reads as away-team-only.

**The 1:1 answer is scroll, and it was in front of me the whole time.** The reference kit's
ENTRY market body is `overflowY: "auto"` (`ui_kits/surething/screens.jsx:49`) — in the
intended design *every* market list scrolls and nothing is ever hidden. S25 permits exactly
this: *"An interior market list may legally scroll, but only with a visible position
indicator"* (`:470`). With scroll, all 14 are reachable and S25's "7 fits" becomes true
again.

**I chose the wrong branch.** I implemented fixed-body + `N NOT SHOWN` because it was the
simpler of the two options S25 allowed. Against the kit it is the non-1:1 one, and it is
what created the one-sided market. Correcting it is my highest-priority item.

Achievability: `RectMask2D` is already used on this stack (`PhoneScreen.cs:199`). The
"visible position indicator" is the part with no reference implementation in the kit (a
browser scrollbar is not a Unity affordance) — **that is the one piece here that needs a DD
call**, and it is a small one.

`N NOT SHOWN` should stay implemented regardless — it is the correct treatment whenever a
list is deliberately capped, and S25 binds it.

---

## 2. Physically impossible on the shipped stack — DD sign-off requested

Only two. Everything else in this audit is achievable, and I have not labelled anything
impossible that is merely inconvenient.

### I-01 · Letter-spacing / tracking — every tracked string on the surface

`typography.css:19-24` defines six tracking tokens (`--st-track-name` .03em through
`--st-track-head` .15em), and **at least 21 elements across ENTRY, the margin, MY BETS and
LEDGER carry an explicit tracking value.** `LaptopUi.MakeText` (`LaptopOs.cs:412-448`)
builds a `UnityEngine.UI.Text`; the legacy uGUI component exposes `fontSize`, `lineSpacing`,
`fontStyle` and `alignment`, and no per-character advance control of any kind — advances
come straight from `Font.GetCharacterInfo`. There is no workaround short of hand-placing
glyphs.

Also blocked by the same limitation: the ruling's own instruction that the scorer role print
as a *"tracked-uppercase word"* (`:431`) cannot be satisfied as written.

**Escape hatch:** TextMeshPro exposes `characterSpacing`. TMP is **not** in the project —
`Packages/manifest.json` carries `com.unity.ugui` only, and no runtime file references
`TMPro`. So this is a stack migration, not a layout fix, and it is an orchestrator/Allen
decision rather than a lead one.

### I-02 · `font-variant-numeric: tabular-nums` on prices

`MarketOffer.jsx:30` and `PriceCell.jsx:36` request tabular figures. Legacy `Text` has no
OpenType feature control, so `tnum` cannot be enabled.

**This one is already resolved in practice and needs sign-off only for the record.** I
measured the shipped font binaries directly (F-02 in the gap-list): **Archivo Narrow's
default digits are exactly uniform** (all 456/1000 em, spread 0) and it is the face carrying
every price and money figure. Roman Archivo spreads 2 units — 0.14px at 18px, sub-pixel.
The requested behaviour is achieved by the font's defaults rather than by the feature.

---

## 3. Deviation register

Achievable divergences. Grouped by region, most severe first within each. Verdicts are
against the intended design; `SportsbookApp.cs` = `S`, `LaptopOs.cs` = `L`.

### 3a. ENTRY screen — structure

| # | Element | Intended | Shipped | Note |
|---|---|---|---|---|
| E-01 | Market row layout | one market per row, full 700px, `height 54` (`screens.jsx:59-72`) | two-up, x 14/354, pitch 42, cells 32 (`S:322-326`) | The kit is single-column for **every** destination, not only scorer. My new PlayMode test asserts GOALS stays two-column — **that test pins a deviation and must be inverted.** |
| E-02 | Market body scroll | `overflowY:"auto"` (`screens.jsx:49`) | fixed 700×412, no mask (`S:278-279`) | §1. |
| E-03 | Ladder label literal | `OVER 2.5 GOALS` (ruling `:428`); kit `"Over 2.5 goals"` uppercased by `screens.jsx:67` | `$"OVER {line:0.0}"` → `OVER 2.5`, noun dropped (`S:323-326`) | The engine already emits the correct string via `Fields.Line`; the call site bypasses it. |
| E-04 | BTTS labels | `Both teams to score — Yes/No` (`data.js:20`) | bare `YES`/`NO`, meaning displaced into an added panel title (`S:336-339`) | |
| E-05 | Per-destination titles | none — the tab strip names the market | added 16px headings "GOALS TOTAL" / "BOTH TEAMS TO SCORE" / "ANYTIME GOALSCORER" (`S:314,333,362`) | Invented element. |
| E-06 | FORM stats strip | not present in `EntryScreen` | added "FORM {team}: GF/COR/CRD" line (`S:262-265`) | Invented element. |
| E-07 | Staged receipts | in the 700px sheet under "PLACED THIS ROUND" (`screens.jsx:50-57`) | in the 324px margin (`S:546,659-710`) | Wrong region entirely. |
| E-08 | Row separator | `1px solid --rule-soft` per row (`screens.jsx:64`) | no per-row rule | |
| E-09 | ENTRY header band | h44 recessed `--ground-2` + 1px `--rule` (`screens.jsx:29-30`) | no band; 2px `--rule-soft` at −74 (`S:250-267`) | |
| E-10 | Records line | inline after title, `{away} · {home} · ENTRY {no}`, `--toner-3` (`screens.jsx:37`) | right-anchored, `--toner-2`, **`· ENTRY {no}` dropped** (`S:258-260`) | |
| E-11 | Matchup title | `{away} at {home}` (`screens.jsx:35`) | `{away} @ {home}` (`S:250`) | |

### 3b. ENTRY screen — price grammar

| # | Element | Intended | Shipped | Note |
|---|---|---|---|---|
| E-12 | Row line label | `--font-cond` 19px `--toner` (`screens.jsx:66-67`) | `_font` roman 13px `--toner-2`, 156px cell (`S:466-469`) | Face, size and colour all differ. |
| E-13 | Picked figure colour | `--toner`; the **ring alone** is biro (`MarketOffer.jsx:11`) | label *and* price both `Accent` biro (`S:469,474`) | Two-ink: biro is the mark, not the figure. |
| E-14 | Picked row wash | `linear-gradient(90deg,--marked-wash,transparent 70%)` (`screens.jsx:65`) | none | **`MarkedWashGraphic` already exists** (`L:386-401`) with the exact 0.7 stop — it is simply never called from `BuildDetail`. |
| E-15 | Price cell ground | transparent, ring painted last (`MarketOffer.jsx:19-23,34-38`) | opaque `Ink` 160×32 created *after* the ring (`S:471-473`) | The opaque cell occludes the ring's shoulder arcs. Plausibly the real cause of the long-running "the ring does not close" defect. |
| E-16 | Replace affordance | dashed underline in `--biro-deep`, **no glyph** on `MarketOffer` (`MarketOffer.jsx:12,31-32`) | `"⇄ "` prefix + solid 2px `--toner-2` underline (`S:472-483`) | Nuance: the ruling *does* specify `⇄` for **scorer** replacement (`:458`). So `⇄` is right on PLAYERS, wrong on the ladders. |
| E-17 | Price cell width | 176 with `justify-content:flex-end` (`screens.jsx:69`) | 160, centred (`S:431-433`, `L:552-553`) | |
| E-18 | Ring geometry | ring = printed figure 160×**30** + 16 → 176×46 (`InkMark.jsx:34`, `space.css:29-31`) | derived from the 160×**32** hit area → 176×48 (`S:453-457`) | The ring must frame the printed figure, not the touch target. |
| E-19 | Tab strip | transparent, gap 2, `borderBottom 1px --rule` (`screens.jsx:40`) | filled `Surface` panel, gap 8, no rule (`S:269-276`) | |
| E-20 | Tab button | h27, auto width, 1px borders, inactive `--toner-3` (`screens.jsx:43-46`) | h32, fixed 96/108, no border, inactive `--toner-2` (`S:305-308`) | |
| E-21 | Back control | `‹ BACK TO FORM`, transparent + 1px `--rule`, `--toner-2` (`screens.jsx:31-33`) | `← FORM`, `SurfaceRaised` fill, **`Accent` biro** (`S:252-254`) | Two-ink violation: biro on a navigation control. |
| E-22 | Hover on the figure | `--wax-lit` (`MarketOffer.jsx:13`) | 1.25× multiply on the **background** only; label never changes (`L:546-550`) | Achievable — the cell is a real `Selectable`. |
| E-23 | `N NOT SHOWN` colour | "in toner" `#D9D4C5` (ruling `:468`) | `Muted` `#6E6B5E` (`S:415-417`) | **Mine.** See §0.3. |
| E-24 | Scorer role field | first-class field, printed as a word in **`--toner-3`** after the name (ruling `:429-431`) | folded into one string `"{Line} — {Role}"` sharing the name's colour (`S:401-404`) | Colour and field-separateness both differ; the "tracked" part is I-01. |

### 3c. Working margin

| # | Element | Intended | Shipped |
|---|---|---|---|
| M-01 | Ruled-paper ground | `repeating-linear-gradient` — 1px `--rule-soft` every 26px (`margin.jsx:9`) | flat `Ink` (`S:488`). Achievable: `LaptopWallpaperGraphic`/`MarkedWashGraphic` (`L:362-402`) already subclass `Graphic` and emit untextured geometry. |
| M-02 | Leg row | 4 parts — biro ✓, team cond 16, price right-flushed, 2nd line `{market} · ENTRY {n}` 13px `--toner-3`, dotted rule (`MarginLeg.jsx:11-26`) | one joined line + invented `"N. "` ordinal; no ✓, no market sub-line, no ENTRY, no rule (`S:523-528`) |
| M-03 | COMBINED row | label 13px `--toner-3` + value **cond 18px `--toner`**, right-flushed, bottom rule (`MarginRow.jsx:10-16`) | one joined string, roman, whole line `Muted`, left-aligned, no rule (`S:550-552`) |
| M-04 | Stake figure | cond **26px** `--toner`, right-flushed; label separate (`StakeControls.jsx:16-17`) | `"STAKE $N"` fused, **16px**, roman (`S:578-580`) — this is D-06 from the gap-list, now confirmed against the kit |
| M-05 | Stake block order | figure → fractions → nudges (`StakeControls.jsx:11-29`) | fractions → nudges → figure (`S:567-580`) |
| M-06 | `PayoutFigure` label | `"POTENTIAL PAYOUT"` 13px `--toner-3` (`PayoutFigure.jsx:6,10`) | no label at all (`S:582`) |
| M-07 | Wax highlight z-order | band drawn **over** the value (`PayoutFigure.jsx:19-23`) | `SetAsLastSibling()` puts the text over the band (`S:595`) |
| M-08 | MarginHeader | title cond 16px `.15em` **`--biro`** + right-flushed count; 2px `--biro-deep` rule (`MarginHeader.jsx:10-19`) | one joined roman 15px White string; no rule (`S:493-497`) |
| M-09 | "PRICES FINAL…" | no such element | invented (`S:498-501`) |
| M-10 | Locked state | replaces the actions with `"ROUND LOCKED"` + StampReason `"BOARD FROZEN — WATCH THE TV"` (`margin.jsx:38-42`) | buttons remain, merely inert; different copy (`S:606-614`) |
| M-11 | Empty margin | `"No marks on this sheet. Circle a price to start a ticket."` sentence case (`margin.jsx:24`) | `"YOUR MARGIN IS CLEAR"` (`S:508`) |
| M-12 | LOCK / SKIP / PLACE chrome | transparent fills, 1px/2px borders, dashed→solid on arm, wax-deep press edge (`LockAction.jsx:13-15`, `SkipAction.jsx:19`, `PlaceAction.jsx:18-19`) | opaque fills, **no borders in any state**, no press edge (`S:601-642`) |
| M-13 | `StampReason` box | 1px `--stamp` bordered box, reason **inside** the LOCK button (`StampReason.jsx:9`, `LockAction.jsx:24`) | bare oxide text placed **above** the button (`S:638-640`) |
| M-14 | Quick-fraction chips | transparent + 1px `--rule`, `--toner-2` (`StakeButton.jsx:14,17`) | `SurfaceRaised` fill, White, no border (`S:568-571`) |
| M-15 | RUB OUT | transparent + 1px border, hover → `--stamp` (`RubOutButton.jsx:14-18`) | `Ink` fill, no border, brightness-multiply hover (`S:540-542`) |

### 3d. MY BETS and LEDGER

| # | Element | Intended | Shipped |
|---|---|---|---|
| B-01 | Ticket axis | stacked **vertically**, full width, 1px rule (`screens.jsx:93-105`) | laid out **horizontally** in equal columns (`S:852-856`) |
| B-02 | `RevealedLeg` row | biro ✓; team cond 16; market sub-line 13px `--toner-3`; DEAD = `opacity .55` (`RevealedLeg.jsx:14-31`) | no ✓; 13px; **no market sub-line**; price on a 2nd line; DEAD = row filled `Surface` (`S:911-963`) |
| B-03 | Header / empty copy | `"READ-ONLY MIRROR"` + StampReason; `"No tickets locked this round."` (`screens.jsx:84-90`) | different strings, 16px, no band (`S:819-839`) |
| L-01 | `LedgerEntry.legs` | **one joined summary string**, single cell (`LedgerEntry.jsx:5,17`; `data.js:41`) | one child row per leg + a per-leg state column with no counterpart (`S:1514-1536`) |
| L-02 | Terminal vocabulary | `WON \| LOST \| CASHED OUT \| VOIDED` (`LedgerEntry.d.ts`) | emits **`OPEN`**; never emits `VOIDED` (`S:1468-1470`) |
| L-03 | Terminal styling | wax if WON/CASHED OUT else `--toner-3`; non-won carries `line-through` in `--stamp` (`LedgerEntry.jsx:29-35`) | LOST → `MoneyBad`; CASHED OUT → `--toner-2`; strike only on LOST (`S:1474-1492`) |
| L-04 | Columns | keys `STAKE` / **`RETURNED`** over cond 16px values (`LedgerEntry.jsx:18-27`) | `"STAKE $N"` / `"PAYOUT $N"` fused, roman 13px (`S:1505-1510`) |
| L-05 | `number` format | `"R2 · TICKET 02"` — round, `·`, zero-padded (`data.js:41`) | `"TICKET 2.1"` (`S:1467,1487`) |
| L-06 | Minus sign | ledger summaries use **U+2212** `−` (`data.js:41-43`) | `OddsFormat.cs:21` emits **U+002D** (`S:1530`). Note: slate/market literals in `data.js:6-31` use U+002D, so this is ledger-specific — worth a DD confirmation rather than a blind change. |
| L-07 | Cashed-out payout | the figure, e.g. `"$164"` (`data.js:43`) | `"AMOUNT NOT RETAINED"` (`S:1473`) |
| L-08 | Header copy | `"SETTLED TICKETS · THIS RUN"` + `"3 RECORDS"` (`screens.jsx:138-139`) | different string + an added column head (`S:1382-1385`) |
| P-01 | Passive margin | `MarginHeader` + `MarginRow` treatment (`margin.jsx:57-66`) | free-text roman lines, neither component's grammar (`S:966-993`, `S:1544-1576`) |

---

## 4. Confirmed 1:1 — no action

Palette: **all 15 hex values match** (`palette-surething.css:8-32` vs `L:14-32`). Production
faces correct (`LaptopScreen.cs:60-61`). Fact floor holds everywhere and is enforced at the
primitive (`L:419`). Corner radius 0. Payout figure 31px wax. PLACE enabled state. Ink-ring
asset family, biro tint and per-matchup determinism. GREEN wax ring / DEAD oxide strike
geometry. SKIP copy and box. LOCK reason strings. Scorer single-column and role-as-word
(S22/S24, DD-signed). `N NOT SHOWN` mechanism (S25, DD-signed) — colour excepted, E-23.

---

## 5. Disposition

**Needs a DD decision (3):**
1. **I-01 letter-spacing** — sign off the deviation, or authorise a TextMeshPro migration of
   this surface. Nothing else unblocks ~21 elements.
2. **I-02 tabular figures** — sign off for the record; the font's defaults already deliver it.
3. **The scroll position indicator** (§1) — the kit relies on a browser scrollbar, which has
   no Unity counterpart. Small call, but it gates the S24/S25 fix.

**Needs an orchestrator/Allen call (1):**
- **§1, the S24/S25 conflict.** My reading is that scroll is the 1:1 answer and my fixed-body
  choice was wrong. Confirm before I rebuild it — it reverses a shipped decision.

**Mine to fix, no permission needed** — everything in §3 not listed above. Largest clusters:
the market row structure (E-01…E-04), the price grammar (E-12…E-18), the margin's component
grammar (M-02…M-08), and the ledger's row model (L-01…L-05). Note **E-01 requires inverting
a test I wrote** — it currently asserts the two-column ladder layout, which pins a deviation.

**Sequencing note:** this is a large rebuild of a surface that is currently green. I will
not start it until the S24/S25 call lands, because §1 changes the container that most of
§3a sits inside, and doing §3 first would mean building much of it twice.
