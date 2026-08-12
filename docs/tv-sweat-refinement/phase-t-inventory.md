# Phase T — TV surface type inventory and phase plan

**Step 1 deliverable (desk work, no editor).** Inventory of every text component on the TV
surface, each mapped to its `tv-design.md` authority or flagged unowned, plus the proposed
staging. Nothing built; no design decision taken.

**Read at** `1e539a9` (branch `slice/tv-sweat-refinement`, clean, pushed).

**Canon currency checked, not assumed.** My HEAD is 89 commits behind main, so the authority
copies were diffed rather than trusted: `docs/design/tv-design.md` is **identical** to main's,
and main carries **no TV runtime changes** since my HEAD (only `Editor/RoomViewCapture.cs`).
`REGISTER.md` and `constitution.md` *have* moved and were read from `main-2`. So the inventory
below reads current canon against current code.

---

## 1. Coverage — why this inventory is complete

`TvSweatScreen.cs` has exactly **one** text-creation path: `MakeText` (line 3942), whose body
holds the file's only `typeof(Text)`. Zero `AddComponent<Text>`. So enumerating `MakeText` call
sites is exhaustive by construction rather than by search diligence.

`TheaterStage.cs` has **no text at all** — its `Text` grep hit is a comment saying so (line 181).
`MomentumTape.cs` builds **one** label and is in scope: `TvSweatScreen` owns `_tape` (line 921)
and parents it (3533).

| | count |
|---|---|
| slot **types** | **23** — 17 singleton + 5 per-leg + 1 momentum label |
| live **instances** | **48** — 17 + (5 × `TicketRowSlots` = 6) + 1 |
| creation paths | 2 (`TvSweatScreen.MakeText`, `MomentumTape`) |

---

## 2. The inventory

`MakeText(parent, name, anchor, pivot, pos, size, fontSize, align, color, style, face)`.
Face defaults to `Regular`; style defaults to `Normal`.

### 2.1 Ticket column and cash-out — the condensed group

| line | slot | field | px | weight | face | canon role |
|---|---|---|---|---|---|---|
| 3671 | `LegRowLine{i}` | — | `TypeEyebrow` 15 | Bold | **Condensed** | compact statement |
| 3676 | `LegRowPrice{i}` | — | `TypeEyebrow` 15 | Normal | **Condensed** | price |
| 3685 | `LegRowNeed{i}` | — | `TypeNeed` 28 | Bold | **Condensed** | NEED |
| 3688 | `LegRowProgress{i}` | — | `TypeProgress` 19 | Normal | **Condensed** | progress |
| 3711 | `RiskPays` | `_tRiskPays` | `TypeRisk` 24 | Bold | **Condensed** | risk/pays |
| 3845 | `CashOut` | `_tCashOut` | `TypeCashOut` 29 | Bold | **Condensed** | cash-out band |

### 2.2 Regular group — canon-named roles

| line | slot | field | px | weight | face | canon role |
|---|---|---|---|---|---|---|
| 3681 | `LegRowState{i}` | — | `TypeEyebrow` 15 | Normal | Regular | state chip |
| 3796 | `Flavor` | `_tFlavor` | `TypeEvent` 22 | Bold | Regular | event line |
| 3761 | `Score` | `_tScoreFlash` | `TypeScore` 36 | Bold | Regular | SCORE figures |
| 3736 | `Matchup` | `_tMatchup` | `TypeScore` 36 | Bold | Regular | **see G1 — team names *and* score** |

### 2.3 Slots the canon face split does not name — **unowned**

| line | slot | field | px | weight | face |
|---|---|---|---|---|---|
| 3637 | `TicketHeader` | `_tTicketHeader` | `TypeEyebrow` 15 | Normal | Regular |
| 3728 | `Leg` | `_tLeg` | `TypeEyebrow` 15 | Normal | Regular |
| 3732 | `Clock` | `_tClock` | `TypeClock` 28 | Normal | Regular |
| 3852 | `CashOutStatus` | `_tCashOutStatus` | `TypeEyebrow` 15 | Normal | Regular |
| 3496 | `Attract` | `_tAttract` | **46 raw** | Bold | Regular |
| 3505 | `TakeoverTitle` | `_tTakeoverTitle` | **30 raw** | Bold | Regular |
| 3508 | `TakeoverSub` | `_tTakeoverSub` | **18 raw** | Normal | Regular |
| 3515 | `Subtitle` | `_tSubtitle` | **22 raw** | Normal | Regular |
| 3581 | `BigAmount` | `_tBigAmount` | **96 raw** | Bold | Regular |
| 3590 | `Consolation` | `_tConsolation` | **28 raw** | Italic | Regular |
| 3873 | `InterventionPrompt` | `_tInterventionPrompt` | **22 raw** | Bold | Regular |
| 3888 | `Chrome` | `_tChrome` | **14 raw** | Normal | Regular |
| — | `MomentumLabel` | `MomentumTape._label` | `LabelSize` 15 | Normal | Regular (canon-cited) |

**12 of 23 slot types are unowned by the canon face split** (§4/§1.1 names 7 condensed roles and
4 regular ones; the surface has 23). `MomentumLabel` is the exception among them — its source
cites canon directly (`TvMomentumTape.jsx:23`, regular + `--tv-context`).

---

## 3. Size — tokens, and where canon does not reconcile

Ten `Type*` constants (lines 756–765). **T20's column derivation matches the code exactly**:
NEED 28 ✓, live progress 19 ✓, resolved/pending 15 ✓.

**§4.1's ratio law does not reconcile with the shipped px, and no single base fixes it:**

| slot | px | px/score | canon ratio | score-size that ratio implies |
|---|---|---|---|---|
| score | 36 | 1.000 | 1.00 | 36.0 |
| cash-out | 29 | 0.806 | 0.70 | 41.4 |
| team | 28 | 0.778 | 0.55 | 50.9 |
| clock | 28 | 0.778 | 0.50 | 56.0 |
| need | 28 | 0.778 | 0.50 | 56.0 |
| risk | 24 | 0.667 | 0.40 | 60.0 |
| event | 22 | 0.611 | 0.36 | 61.1 |
| progress | 19 | 0.528 | 0.40 | 47.5 |
| leg | 19 | 0.528 | 0.34 | 55.9 |
| label | 15 | 0.417 | 0.22 | 68.2 |

The implied base ranges 36 → 68. Every shipped size is *above* its canon ratio, consistently.
The likeliest reading is that the ratio line predates T20/T24-am's re-derivation and the px are
the live truth — but §4.1 says "**Ratios are the law**", so this is not mine to decide (**G3**).

**8 of 23 slots carry raw integers rather than tokens** (46, 30, 18, 22, 96, 28, 22, 14).

---

## 4. Findings

**F1 — Two "not yet wired" notes are stale, provably.** `TvSweatScreen.cs:825` ("MakeText still
assigns `_font` to every slot, so the whole surface renders regular") and `FONTS.md`'s *Still
open* section both deny the condensed wiring. `MakeText:3952` honours `face`, and **6 call sites
pass `Face.Condensed`**. `Face.Condensed` landed `c53d7ca` on **2026-08-02**; both notes were
last touched **2026-08-01**. The wiring landed the next day and neither note followed.
Note the irony recorded at line 827: that comment was written *because* an earlier version
claimed call sites that did not exist. It now denies call sites that do.

**F2 — Two dead size tokens.** `TypeTeam = 28` and `TypeLeg = 19` have exactly one reference
each: their own declaration.

**F3 — `_tBigAmount` (96px) renders nothing.** Already flagged at its declaration (lines 838–844)
and gated by `SanctionedL4Elements`. Phase T would otherwise migrate a component that has no
content, and 96px also exceeds `TypeScore` 36 against §4.1's "nothing outgrows the score".

**F4 — Encode Sans is a VARIABLE font (`wdth,wght`)**, per `FONTS.md`. The SemiBold-default trap
applies here in full: `SureThingTmpFontAssets.cs:20` records Archivo's faceIndex 0 reporting
SemiBold and the first cut shipping the roman voice at 600 unchosen. **Faces must be resolved by
style name, never by index**, and the weight verified by measurement.

**F5 — Encode Sans Condensed is STATIC Regular only** ("upstream ships no variable build").
**Four condensed slots request Bold** — `LegRowLine`, `LegRowNeed`, `RiskPays`, `CashOut`. Legacy
UGUI synthesises that; TMP's equivalent is a material-level faux bold. There is no real bold
condensed face in the repo (**G2**).

**F6 — Zero TMP on the TV today.** The single `TextMeshPro` mention (line 3695) is a comment
explaining that the VOID strikethrough is drawn as an `Image` rule *because* "the whole surface is
UI.Text". Phase T removes that premise — but §6 forbids geometry computed from content, which the
rule was also chosen to satisfy, so this is not automatically a cleanup (**G5**).

**F7 — §4.1's ratio law and shipped px do not reconcile** (§3 above).

**F8 — Tabular numerals are mandatory** (§4, T11: "measured, not assumed"). Enabling them in TMP
is a font-feature concern at asset-build time, not a call-site property. Treated as a build
requirement with its own verification, not an assumption.

---

## 5. Design gaps — routing to the DD through the orchestrator

**G1 — Team names vs score figures share one component.** Canon puts team names on *condensed*
and SCORE figures on *regular*. `_tMatchup` renders both in one string:
`$"{awayMark}{away}  {awayScore} — {homeScore}  {home}{homeMark}"`. The split is unsatisfiable
without either splitting the component or ruling one face for the line. Note T32 already ruled
team names are `--tv-fact`; this is the *face*, not the colour.

**G2 — Bold on condensed slots, with no bold condensed face.** Add an Encode Sans Condensed
weight (new font file + OFL, an asset decision), drop those four slots to regular weight, or
accept TMP faux bold. Affects the cash-out figure and NEED — the two loudest facts on the surface.

**G3 — Which size authority governs Phase T?** §4.1's ratio law, or T20/T24-am's re-derived
column px that the code implements? They do not reconcile and TMP sizes must come from one.

**G4 — Face for the 12 unowned slots.** Default them to regular (status quo) or rule them.

**G5 — Strikethrough.** Keep the `Image` matrix rule, or adopt TMP's native strikethrough now
that the "whole surface is UI.Text" premise is gone? The rule is fixed-width by design because
§6 forbids content-derived geometry; native strikethrough is content-derived by nature.

---

## 6. Proposed phase plan — Phase L's shape

**T-0 — inventory and plan.** This document. Gate: orchestrator + DD answers to G1–G5.

**T-1 — the generator, no call-site changes.** Extend or mirror `SureThingTmpFontAssets.cs` to
emit Encode Sans TMP assets from the committed TTFs. Every atlas parameter a named constant (its
C34 rationale applies unchanged). **Faces resolved by style name**; the built weight verified by
measurement, not by trusting the default instance (F4). Tabular figures verified present and
enabled (F8). Gate: assets rebuild identically from a clean delete.

**T-2 — before-set.** Capture at current HEAD on the pinned seeds, the TV's equivalent of
`fresh-reference-set-2026-08-11`. Needs an editor window through the orchestrator. Per C41, any
expectation stated from these frames is a direction of travel, not a number to land on.

**T-3 — mechanical migration.** `MakeText` is a single seam, so the whole surface moves in one
step with **no design change**: same sizes, same faces, same weights, UGUI `Text` → `TMP_Text`.
Deliberately boring and separately reviewable. Gate: suites green, capture harness and its pinned
seeds still working.

**T-4 — rulings applied.** G1–G5 as decided. Tokenise the 8 raw sizes (F8); retire or wire the
dead tokens (F2); dispose of `_tBigAmount` per its gate (F3).

**T-5 — typographic finish.** Tracking groups and tabular lining numerals where numbers change
live (score, clock, money, counts, progress).

**T-6 — after-set and verification package.** Same shape as Phase L. Correct F1's two stale notes
in the same commit that makes them false — the discipline line 827 asks for.

---

## 7. Risks carried into this phase

- **The variable-font default (F4)** is the highest-consequence one: it is silent, it lands on
  every glyph, and the last surface to hit it shipped an entire migration at the wrong weight.
  The DD's stroke ruling names what caught it — a coherence check across two groups on one frame,
  not the source note. That check belongs in T-1.
- **Condensed bold (F5)** may force an asset decision before T-3 can be purely mechanical.
- **Capture harness and pinned seeds must keep working** — the T-row evidence paths depend on
  them, and my own de-flake work just pinned `48151623` in the PlayMode tests.
- **§7a settings churn** unchanged: the Sentis define field reappears after every editor run and
  is checked out, never committed. Explicit-path staging throughout.
- **Engine DLL is an LFS pointer on main** — restore by checkout and verify by loading the
  assembly, never by hashing against what was just written.
