# SureThing — Approved Direction — The Annotated Form Guide

> **Status:** authoritative pre-implementation contract for the SureThing laptop and its fictional OS.
> **Chosen world:** Approved Direction — *The Annotated Form Guide*.
> **Runtime:** Unity UGUI, fixed 1024 × 704 world-space laptop canvas.

## Overview

**Creative North Star: “The Annotated Form Guide.”**

SureThing is the occupant's cheap, personal document reader at 2 a.m. The house printed a dense inverted betting form; the player compares it, circles prices in ballpoint blue, works the right margin, and commits. Build is calm, deliberate, and dense. Sweat gets loud by marking the same document at the player, not by becoming a slot machine.

This is the durable design-to-UGUI contract for the sportsbook app, laptop OS chrome, working slip, event detail, staged tickets, MY BETS, rewards, and old slips. It does not authorize changes to `/engine`, `TvSweatScreen`, the room, TV, scene, or project settings.

### Authority and supersession

Resolve design questions in this order:

1. [PRODUCT.md](../../../PRODUCT.md): product truth, surface ownership, accessibility, and scope.
2. [SHARED-SPEC.md](SHARED-SPEC.md): fixed artboard content, states, and legibility.
3. This contract, [element-kit.html](element-kit.html), and [direction-1-form-guide.html](direction-1-form-guide.html): approved implementation reference. Where the shared artboard asks for a state and the element kit defines that component's behavior, the element kit is the component-specific resolution.
4. [ASSETS.md](assets/ASSETS.md): sprite generation and Unity import.

[INDEX.html](INDEX.html) records **Approved Direction — The Annotated Form Guide**. **Rejected comparison — The Catalogue Sleeve** remains comparison evidence only. The earlier violet package in `../surething-ui-revamp/` remains evidence for information architecture and behavior but is superseded as visual authority. Earlier discarded explorations must not be revived.

**The ownership rule.** Laptop owns slate, markets, working slip, stake, staging, lock, shop, and placed tickets. TV owns unrevealed drama. MY BETS renders only `TvSweatScreen.RevealedView`; it never derives engine truth or reveals score, clock, probability, or outcomes ahead of TV.

**The personal-machine rule.** Do not borrow the TV's institutional steel, coarse grid, monumental type, brightness-only semantics, or quantized motion. This close-range tool is personal, cheap, and grubby.

## Colors

The form is an inverted document: warm lifted olive-black ground, house toner, and two rare player-facing inks. Nothing on the laptop is pure black.

| Token | Value | Semantic role |
| --- | --- | --- |
| `ground` | `#16160F` | Canvas; darkest screen value. |
| `ground-2` | `#1C1C13` | Recessed bands: column head and tab strip. |
| `ground-3` | `#232319` | Raised OS rail, tray, and nudge keys. |
| `rule` | `#3C3C2C` | Structural 1–2px rules. |
| `rule-soft` | `#2C2C20` | Secondary rules and ruled-paper structure. |
| `toner` | `#D9D4C5` | Primary factual text: names, prices, figures. |
| `toner-2` | `#9C9888` | Secondary text. |
| `toner-3` | `#6E6B5E` | Field keys and labels; readable-text floor. |
| `biro` | `#5E86B8` | Player choices, tally, marks, selection rings. |
| `biro-deep` | `#3F6996` | Replacement underline and biro structural detail. |
| `wax` | `#D9A441` | Money and primary action only. |
| `wax-lit` | `#F0C066` | Wax hover value. |
| `wax-deep` | `#8A6620` | Wax action edge. |
| `stamp` | `#B4483A` | House mark only. |

**The house-mark rule.** Oxide `stamp` is never generic loss or error. It is only the house's stamp and a dead-leg strike. Loss also uses a strike, literal state label, and an entry dropping toward ground.

**The two-ink rule.** Wax is money/primary action; biro is the player's choice. No other meaning borrows either. `GREEN` and `DEAD` remain literal revealed-state words, not color claims.

**The lifted-black rule.** No screen region uses `#000000`; the laptop remains lighter than room shadows and participates in the unified room grade. Status is never color alone: pair color with ring/strike, label, glyph, border, or position/state change.

## Typography

**Production face:** undecided, pending the Design Director. **Bell Centennial is dropped for good** (Allen, 2026-07-31): no licence-encumbered typeface ships in this product, so the intended face is out regardless of whether a licence could be bought. The replacement must be free-licence (OFL or equivalent) and is being specced by the Design Director as part of the form-guide identity work.

What the face still has to do, as the brief for whoever picks it: carry three-digit American prices and W–L records at 13px on a surface read at an angle, hold a condensed figure set so a price column stays narrow, and read as a cheap personal machine rather than institutional signage.

**Licensing policy:** free-licence faces only. Do not download, commit, or distribute a commercial face, and do not preemptively pick a substitute — that call belongs to the Design Director.

**Swap cost:** the runtime resolves its face through one seam (`LaptopScreen.LoadFont`), and every builder takes the resulting `Font` by parameter. Swapping the face is a one-function change plus a font asset; nothing else in the UI names a typeface. Keep it that way.

**Final runtime route:** production uses licensed TextMeshPro font assets. `LegacyRuntime.ttf` and legacy UGUI `Text` are current implementation evidence only; they are not acceptable final visual implementation because the 50% legibility contract needs the selected face, stable glyph metrics, and a reproducible asset import path. HTML system stacks are mockup-only.

| Size | Use |
| --- | --- |
| 31px | Potential payout. |
| 26px | SureThing Form masthead. |
| 21px | Persistent bank and target figures. |
| 19px | Team names and prices. |
| 16px | Margin legs and action labels. |
| 13px | Minimum product fact: labels, records, reasons, market navigation. |
| 12px | OS chrome only; never product meaning. |

Use the document data face for labels/secondary text and the condensed production face for masthead, figures, prices, team names, and action labels. Short labels are tracked uppercase; factual copy remains literal.

**The fact-floor rule.** Prices, records, field labels, state labels, disabled reasons, and market navigation are never below 13px. Nothing is below 12px.

## Layout

Runtime is one fixed 1024 × 704 composition on an approximately 0.32 × 0.22m world-space laptop surface. It is not responsive, scrollable, or browser-like. Only interior market lists may scroll when real content exceeds their panel.

### Layer and band map

| Z | Band | Bounds | Contract |
| --- | --- | --- | --- |
| 0 | Document ground | `0,0,1024,704` | `ground`; optional local toner grain is beneath room grade. |
| 1 | OS rail | `y 0–34` | 34px, `ground-3`, 1px bottom `rule`. |
| 2 | App tabs | `y 34–72` | 38px, `ground-2`, 2px bottom `rule`. |
| 3 | Form masthead | `y 72–140` | 68px, 2px bottom `rule`. |
| 4 | Main work area | `y 140–670` | 530px: left form 700px, right margin 324px. |
| 5 | OS tray | `y 670–704` | 34px, `ground-3`, 1px top `rule`. |
| 6 | Decoration | Over owning control | Rings, strikes, payout highlight; no raycasts. |
| 7 | Focus | Over focused control | 2px wax outline, 1px offset. |

Preserve the resulting 34 + 38 + 68 + 530 + 34 = 704px composition. The table names visual bands, not a mandatory UGUI anchor convention.

### OS chrome anatomy

- **Rail (34px):** `NOTEBOOK` identity, “property of nobody” sticker, clock, battery. The concept's fixed example clock is `02:47`.
- **Tabs (38px):** `FORM`, `ENTRY`, `MY BETS`, `REWARDS`, plus `SHEET 1 OF 1`. Active tab joins document ground; inactive tabs are ruled/muted.
- **Tray (34px):** SureThing, Ledger, Messages and non-product facts such as `DISK 61% FULL` and `NO UPDATES AVAILABLE`.
- **Masthead (68px):** SureThing Form, round/prices-final context, bank, target, relic count, tickets placed, and literal locked-odds note. No promo rail.

Runtime names map as `Lobby → FORM`, `Detail → ENTRY`, `MyBets → MY BETS`, and `Rewards → REWARDS`. Existing Old Slips maps to Ledger and stays read-only.

### Lobby: house form and player margin

**House form — left 700px.** A 26px column head precedes six 78px two-line entries. Each entry has a 30px number column, flexible matchup/record column, 112px price column, and 78px More column. Team names are 19px; records are 13px. A 1px `rule-soft` separates entries. Lobby shows Moneyline and `MORE ›` only.

**Player margin — right 324px.** It has 14px horizontal padding, a biro-ruled header, and subtle horizontal form ruling. Its fixed vertical order is:

1. `MY MARKS` and selection count.
2. One explicit leg per selection: blue check, team/market identity, price, and `RUB OUT`.
3. Combined odds.
4. Stake figure, 10% / 25% / 50% / MAX, then −$10 / +$10.
5. Potential payout: the one loud margin figure.
6. PLACE TICKET, LOCK IT IN state, separate SKIP ROUND state.

The Round 3 values in `SHARED-SPEC.md` prove density only; runtime figures remain engine-backed. The document is stable; selection adds/replaces player marks and never turns rows into rounded sportsbook cards or a floating drawer.

### Surface mappings

| Product state | Form-guide mapping | Authoritative data |
| --- | --- | --- |
| Event detail / ENTRY | Preserve rail, tabs, masthead, matchup identity, working margin. Replace only form body with Goals, BTTS, Corners, Cards, Players. | Slate and `BetslipModel`. |
| Staged ticket / ENTRY | Clear working marks; show placed ticket as dated/numbered form receipt with literal stake, odds, payout, legs. Enable LOCK only with ≥1 staged ticket and empty working marks. | `Run.Tickets`; no UI ticket model. |
| MY BETS / revealed | Read-only marked form: ticket identity, stake, payout, legs, revealed states only. Never score, clock, probability, next event, or unrevealed result. | `TvSweatScreen.RevealedView` only. |
| REWARDS | Ruled entries for relic/consumable offer, price, affordability, description, buy/sell, error. | Existing `Run` / `RunDirector` verbs. |
| Old Slips / Ledger | Read-only settled-ticket ledger in the same form grammar. | Settled `Run.Tickets` only. |

### Reproducible secondary-surface composition

The 34px rail, 38px tabs, 68px masthead, 530px work area, 700px left document, 324px right region,
and 34px tray are reused on every surface below. Those are locked values. The inner arrangements
below are **derived implementation compositions**, not new locked measurements; preserve the
existing region boundaries and the component sizes already specified in this contract.

#### ENTRY — event detail and staged ticket

- **Hierarchy:** the left document begins with a literal back-to-FORM control, matchup identity and
  records, then five market destinations: Goals, BTTS, Corners, Cards, Players. The selected
  destination owns the remaining scrollable market list. The right region remains the working
  margin so selection count, legs, stake, payout, and next action never disappear.
- **Region reuse:** reuse lobby column/rule treatment for the event header and market rows. Use
  the 160 × 30px market cell and 176 × 46px wide ring for selected market offers. The header and
  margin do not rebuild merely because a destination changes.
- **States:** default/hover/focus/replacement use the same price grammar as FORM. A selection from
  any destination replaces the existing selection for that matchup; it must be visibly available,
  never presented as unavailable. Long lists scroll inside the left document only.
- **Staged substate:** after PLACE TICKET, show a numbered placed-ticket receipt before the action
  area, clear working marks, and retain the ticket count. The receipt uses literal legs, stake,
  odds, and payout from `Run.Tickets`; it does not invent a date, settlement, or outcome. LOCK
  becomes the next primary commitment only when its actual conditions are true.
- **UGUI mapping:** keep one detail root and replace the market-body child on destination switch;
  keep the shared margin child and its controls bound to the current `BetslipModel`. A staged
  receipt is a `Run.Tickets` renderer, not copied UI state.

#### MY BETS — revealed mirror

- **Hierarchy:** retain rail, tabs, masthead, and tray. The work area starts with an explicit
  read-only/TV-owned status line, then ticket identity and ticket state, then leg rows. The right
  region is a passive ticket summary or blank ruled margin; it never exposes stake, selection, or
  lock controls that mutate a round during sweat.
- **Region reuse:** ticket and leg rows inherit ruled document structure, literal figures, and
  the same 13px fact floor. This is not a second TV dashboard and does not inherit TV score or
  probability presentation.
- **States:** no mirror renders an honest waiting state; pending/live legs retain document/biro
  treatment; GREEN uses literal state plus wax re-ink; DEAD uses literal state plus `strike-a` and
  groundward dim; cash-out, void, and final ticket state remain literal. Every state is sourced
  only from the revealed payload.
- **UGUI mapping:** rebuild ticket/leg structure only for revealed structural revision; update
  any changing revealed display value in place. Read `TvSweatScreen.RevealedView` only, never
  `Run`, engine session, score, clock, or probability for live truth.

#### REWARDS — shop

- **Hierarchy:** rail/tabs/masthead persist. The left document lists offers as ruled entries:
  name, literal description, price, affordability, and buy action. The right region is the run's
  current resource/tally context plus explicit buy/sell result or error; it is not a promotional
  rail.
- **Region reuse:** offer rows use document rules and fact-floor labels. Buy and sell controls use
  rectangular secondary-action grammar; primary money emphasis remains wax and does not turn every
  affordance amber.
- **States:** affordable/unaffordable is never color-only; unavailable purchase tells the literal
  engine/director reason when supplied. Successful purchase updates the existing tally; errors are
  literal, visible, and do not fabricate a remedy.
- **UGUI mapping:** render offers and ownership from existing `Run` collections; dispatch buys,
  sells, manager/redeal, and leave-shop only to existing `RunDirector` verbs. Keep error text from
  the returned verb result rather than reimplementing validation.

#### LEDGER / Old Slips — settled record

- **Hierarchy:** retain the personal OS rail/tray. The left document is a chronological-looking
  ruled list of settled ticket identity, literal terminal state, stake, and payout. The right
  region is a passive record summary, not a live betslip.
- **Region reuse:** settled tickets use the same ticket/leg document anatomy as MY BETS, but no
  live styling, action buttons, or TV-watch instruction. Empty ledger uses an explicit no-settled-
  tickets message at the fact floor.
- **States:** won, lost, cashed out, and voided are literal labels. Use the existing revealed
  outcome treatment only where it is truthful; no status relies on color alone.
- **UGUI mapping:** filter/render existing settled `Run.Tickets`; do not create a separate ticket
  history store or infer payouts/outcomes.

## Elevation & Depth

This is a flat printed document, not a card system. Depth comes from three tonal grounds, structural rules, the physical rail/tray, and player ink over house toner. No rounded-card shell, floating slip, glossy shadow, terminal scanline, or neon-glow default.

The payout uses the specified hand-laid wax highlight: a 6px amber band, approximately `0.26` opacity, rotated `-0.5deg`, behind the 31px figure. Primary PLACE may use a 2px `wax-deep` edge; pressed state removes that edge and moves 2px down. These are the specified depth cues.

Glass, dust, grain, haze, bloom, chromatic aberration, and vignette belong to the room's unified grade. Local document grain is beneath that pass, never a substitute.

## Shapes

Form language is ruled, rectangular, and precise. Controls are square-cornered unless an irregular source asset supplies its silhouette. Borders are solid 1–2px `rule`, never hairlines.

- Tabs: 27px high, ruled labels without bottom border; active joins `ground`.
- Price visual: 96 × 30px, 19px toner, transparent; selection is a drawn ring, never pill/fill.
- More: 74 × 44px, rectangular, ruled, literal.
- PLACE: 44px high, at least 200px wide.
- LOCK: 52px high, at least 280px wide.
- SKIP: 34px high, at least 230px wide; dashed unarmed, solid stamp armed.
- Quick stake: 68 × 32px; nudge: 88 × 32px; RUB OUT: 60 × 32px.

The shared target floor is 44 × 32px with 8px separation. Price visual height is 30px, so UGUI must provide a ≥32px interactive hit area without changing the 96 × 30px price figure or 112 × 46px ring placement. This resolves two accepted exact specifications without changing visible design.

## Components

### Price cell and selection ring

| State/part | Exact treatment |
| --- | --- |
| Default | 96 × 30px, 19px toner figure, transparent. |
| Hover | `wax-lit` price; no fill. |
| Focus | 2px `wax` outline, 1px offset. |
| Selected | Toner price plus blue drawn ring. |
| Replacement candidate | Selectable `⇄` in biro plus dashed biro-deep underline. |
| Won in sweat | Wax figure and wax re-inked ring. |
| Dead in sweat | Toner-3 figure and strike sprite; literal DEAD state also shown. |

Price ring: 112 × 46px at offset `x −8, y −8`, centered on 96 × 30px price. Event-detail market row: 160 × 30px visual cell with 176 × 46px wide ring at same offset.

### Market navigation and margin economics

`MORE ›` enters detail. Detail destinations are Goals, BTTS, Corners, Cards, Players. Switching destination changes only market body; matchup and working margin persist. Current runtime nests BTTS under Goals; promoting it is UI work, not engine work.

### Replacement is not disabled odds

`SHARED-SPEC.md` asks the comparison artboard to show a disabled odds control, while the
component-specific element kit says “REPLACE, NEVER BLOCK”: v0 has no limiting, padlock, disabled
odds, or suspension for another market on an already-selected matchup. The runtime resolution is:

- A selectable price or market on a matchup that already has a selection is a **replacement**. It
  uses the available `⇄`/dashed-underline treatment and calls the existing one-selection-per-
  matchup replacement behavior.
- It is never disabled merely because another selection exists on that matchup. Do not satisfy the
  artboard example by falsely disabling an alternative price or market.
- A disabled market control may exist only if its bound data/model supplies a real unavailable
  state and literal reason. Current v0 sources do not provide such a market-availability state, so
  no disabled odds control is rendered in production. This document does not invent engine support
  or a fake reason.
- The disabled action required by the current build surface is LOCK IT IN: while a working slip is
  nonempty, use `PLACE OR CLEAR THIS WORKING SLIP`; while no staged ticket exists, use the actual
  staged-ticket prerequisite when that UI state is implemented. These are action states, not odds
  availability states.

Each margin leg shows blue check, team/market identity, American odds, explicit `RUB OUT`. Removal clears that matchup and immediately recalculates combined odds, stake, and payout through `BetslipModel`. Payout is the only loud margin number. Quick stake/nudge controls retain existing fraction and $10 behavior. Display stake and payout together after every input.

### Actions

| Control | Contract |
| --- | --- |
| PLACE TICKET | 44px wax action. Valid working slip only. Stages engine-backed ticket, clears marks, re-anchors stake through existing model. |
| LOCK IT IN | 52px ruled control. Disabled while marks exist or no ticket is staged; gives literal cause/remedy. Enabled only with staged ticket(s) and empty slip, then commits round. |
| SKIP ROUND | Separate 34px secondary action. First press arms; second commits empty round. Never masquerades as lock. |
| RUB OUT | Explicit 60 × 32px removal target, never a tiny unlabeled ×. |

For nonempty working slip, use the fixed literal `PLACE OR CLEAR THIS WORKING SLIP` from `SHARED-SPEC.md`. The element kit's “PLACE OR CLEAR THESE MARKS” is visual-reference copy, never permission to omit cause/remedy.

### State matrix

| State | Visible treatment | Interaction/truth |
| --- | --- | --- |
| Default | Toner price, ruled controls, muted labels. | Available behavior remains literal. |
| Hover | Wax-lit price or documented border/text shift. | No false-urgency loop. |
| Keyboard focus | 2px wax outline, 1px offset. | Visible on every ground. |
| Selected | Biro ring plus margin leg/count. | One selection per matchup. |
| Replacement | Biro `⇄` plus dashed underline. | Replaces same-matchup leg; never blocks. |
| Remove | `RUB OUT` target. | Removes leg and recalculates economics. |
| Disabled action | Muted action, bordered stamp reason at ≥13px. | Literal cause and remedy in place; not a replacement-price treatment. |
| Unavailable market | Not implemented in v0. | Render only if model data supplies actual unavailable state and literal reason. |
| Empty | Explicit empty-slip copy; no invented economics. | PLACE disabled; lock/skip rules clear. |
| Staged | Ticket receipt present; working marks clear. | LOCK enabled only if no working marks remain. |
| Locked | Frozen board; prices visibly final. | No choice/stake/place/lock mutation; sweat routes MY BETS. |
| Skip confirmation | Dashed secondary becomes solid stamp with `PRESS AGAIN TO SKIP`. | Second press commits. |
| Live | Entry lifts toward toner; biro ring holds. | MY BETS remains TV-revealed only. |
| GREEN | Literal `GREEN` plus wax-filled/re-inked result. | Only after TV reveal. |
| DEAD | Literal `DEAD`, `strike-a`, entry dims toward ground. | Only after TV reveal. |
| Error | Literal engine/director error; not color-only. | Never revalidate rules in UI. |

### HTML/CSS concept to UGUI

| Concept | UGUI mapping |
| --- | --- |
| Screen, rail, tabs, mast, body, tray | Fixed-pixel root and child `RectTransform` bands. |
| CSS tokens/rules | Central SureThing-only UGUI style/token helpers; no repeated literals across builders. |
| Text face/size/tracking | Licensed font asset(s), UGUI text, exact sizes above, engine-backed literal data. |
| Form rows/margin | RectTransform rows; use layout groups only when they preserve dimensions, otherwise anchors. |
| CSS ring/strike mask | White-alpha sprite `Image` tinted by `Image.color`. |
| CSS hover/focus/pressed | Button transition/events plus explicit focus-outline graphic. |
| Ruled repeat gradient | Reproducible UI background/material or tiled generated asset; carries no factual content. |
| Payout pseudo-element | Non-raycasting child `Image` behind payout text. |

Do not create a production sidecar token/component file yet: production UGUI token helpers and components do not exist. This document is their contract.

### Ink sprite pipeline

Use [ASSETS.md](assets/ASSETS.md). Regenerate deterministically with `python tools/art/make-biro-rings.py`.

| Asset | Display use |
| --- | --- |
| `ring-price-a`, `-b`, `-c` | 112 × 46px price selection ring. |
| `ring-wide-a`, `-b` | 176 × 46px event-detail ring. |
| `strike-a` | 112 × 46px dead-leg house strike. |

Import the `@2x` files, display at 1× RectTransform values. Unity import: Texture Type **Sprite (2D and UI)**; Sprite Mode **Single**; Mesh Type **Full Rect**; Alpha Is Transparency **On**; Generate Mip Maps **Off**; Wrap **Clamp**; Filter **Bilinear**; Compression **None / High Quality**. Image: **Simple**, Preserve Aspect **Off**, Raycast Target **Off**.

Sprites are white RGB with alpha ink. Tint selection rings `biro`, strikes `stamp`. Variant is deterministic:

```csharp
Sprite ring = _ringVariants[matchupIndex % _ringVariants.Length];
```

Never randomize per frame or rebuild; stake changes must not redraw a selected ring.

### Motion grammar

**The laid-ink rule.** Motion is continuous, hand-paced, and caused by document marking. No duration or easing curve is specified; do not invent a flashy motion system.

- Live: entry lifts toward toner; biro ring holds.
- Win: figure fills wax; ring re-inks over it.
- Death: `strike-a` is drawn; entry drops toward ground.
- Return change: margin tally crosses out and rewrites beneath in biro.
- PLACE press: documented 2px downward response is permitted.

No TV-style quantized refresh, panel flips, confetti, pulse loops, brightness-only status, or casino urgency.

## Do's and Don'ts

### Do

- **Do** preserve locked odds, one selection per matchup, replacement, stake clamp, up to three tickets, and TV reveal boundary.
- **Do** keep factual copy literal: selection count, stake, payout, price, state, and disabled reason.
- **Do** use fictional teams, leagues, and players only.
- **Do** validate at 50% thumbnail and actual angled laptop view, not only flat Game view/HTML.
- **Do** hold normal text to ≥4.5:1 contrast, targets to ≥44 × 32px with 8px separation, essential strokes to 1–2px.
- **Do** preserve persistent chrome on FORM, ENTRY, MY BETS, REWARDS.

### Don't

- **Don't** revive Rejected comparison — The Catalogue Sleeve, earlier discarded explorations, violet ledger, modern sportsbook shell, retro-terminal costume, cyberpunk neon-on-black, or institutional TV vocabulary.
- **Don't** use rounded odds pills, card grids, floating betslip drawer, promo rails, real operator branding/copy/marks, or pure black.
- **Don't** use color-only status, sub-floor product text, hairline essential strokes, or low-opacity facts.
- **Don't** re-derive odds, probability, outcome, payout, or disabled truth in UI.
- **Don't** expose score, clock, probability, or unrevealed result on MY BETS.
- **Don't** alter engine, TV, room, scene, or project settings for this redesign.

### Existing behavior preservation and validation

Presentation may change and staging/skip UI may be added, but preserve these seams:

- `BetslipModel` owns selection replacement/toggle, stake clamp, combined odds, preview payout, blockers, and post-place re-anchor.
- `SportsbookApp` remains a renderer over `BetslipModel`, `RunDirector`, and TV view; never a second rules engine.
- `LaptopOs` phase routing remains Betting → FORM, Sweat → MY BETS, Shop → REWARDS.
- MY BETS remains a `RevealedView` causal mirror with no direct engine reads.

Validate targeted behavior with `BetslipModelTests`, `AnytimeScorerBetslipTests`, and `LaptopOsTests`, then the relevant Unity suite. Capture actual angled laptop lobby, event detail, staged ticket, disabled lock reason, and revealed MY BETS. Check persistent Chrome, deterministic ring stability across rebuilds, and no clipping at 1024 × 704.

### Open risks

1. **Production typeface:** Bell Centennial dropped (Allen, 2026-07-31) — no licence-encumbered typefaces in this product. A free-licence replacement is pending from the Design Director. Until it lands the build renders in `LegacyRuntime.ttf`, so no capture to date shows the direction's intended voice; judge captures on structure, not type.
2. **Current implementation divergence:** purple tokens, 660px board/right slip, sub-13px product text, `LegacyRuntime.ttf`, nested BTTS, lock-with-working-slip behavior, and no separate skip confirmation do not meet this contract. They are implementation work.
3. **Perspective proof:** HTML/fixed-size concept is not proof at physical laptop angle; final acceptance requires the in-room readability check.
