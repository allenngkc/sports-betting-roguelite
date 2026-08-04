# C14 — TV surface reconciliation gap-list

**Owner:** TV sweat lead (`tv-sweat`) · **Date:** 2026-08-01
**Audited:** all landed TV work against `main-2/docs/design/design-system` — `components/tv/*` (jsx +
`prompt.md`), `ui_kits/tv-sweat/`, `tokens/*` — plus DD Ruling Batch 4 and its embedded addendum.
**Branch state:** `slice/tv-sweat-refinement` at `ccc6f56`, 12 ahead of `main`, tree clean but for
three standing exclusions. **Method:** two bounded sub-agent sweeps (ruling extraction; canon-vs-build
conformance), both citing file:line on **both** sides of every claim, synthesised here.
**Standard:** C14 — 1:1 is the bar. Deviation only where physically impossible, DD-signed **before**
build.

Tags: **fix-now** (no window, no ruling) · **needs-window** (editor/frames) · **needs-DD** (ruling first).

---

## 0. State change found at seating

1. **T25.1 RESOLVED** (`4e4585a`). Batch 4 lists it blocking; the register records it fixed. The DD
   states the T26 refusal was issued on pre-fix frames and is "expected to invert".
2. **T24 rules against my own last proposal.** I read "supersedes the fixed-rows interim" as
   authorising expanding rows. The ruling is the reverse — *"fixed rows stand; canon amends to match
   it… the slot was wrong rather than the row."* Nothing was built on the misreading. Logged because
   it is precisely what C14 exists to prevent: a lead inferring a design decision from a summary.
3. **The face landed** (`ccc6f56`), so pixel comparison is meaningful for the first time. **Every type
   judgement in T25 — including the favourable one — was made on `LegacyRuntime.ttf` frames** and is
   provisional until re-captured. The DD says so itself in T26.

## 1. Baseline (measured, no editor held)

| Check | Result |
|---|---|
| `dotnet test engine.tests` | **160/160** |
| Unity EditMode | **211/211** |
| Unity PlayMode | **46 passed + 1 `[Explicit]` skipped**, 0 failed |

The surface is not broken. What follows is conformance, ruled-but-unbuilt work, and two process defects.

## 1A. Progress — fix-now block in flight (uncompiled)

**Closed:** TV-01 (tape hues out; every mark now colourless under an L2 ceiling), TV-S1 (ladder
applied — `AtTier` at every touched slot), TV-19 (five condensed slots wired per the component
references), TV-20, TV-21, TV-22, TV-05, TV-10, TV-11, TV-31, TV-32.

**Also closed:** TV-03 (the actionable field is now an inverted solid-gold panel with `--tv-gold-ink`
added), TV-04 (money and status split — status at eyebrow scale beside the figure, never at money
scale), and T22's copy (`[E]` retired; the slot prints `HOLD E`). The field/status pair is
render-aware — it follows the money element's own state rather than being set at the eight places
that hide the slot, so no path can leave a gold field or a stale status word behind it. The inversion
is gated on `CanAcceptCashOutNow`, not on visibility: a gold field over an offer that would be
refused is the surface lying about input.

**STILL OPEN in the fix-now block:** TV-14 (compact row into three spans), TV-15 (risk/pays into four
elements), TV-16/23/24/25/26/29/30/33.

**Not yet compiled — ~35 edits across five files.** Stopped deliberately at one structural item rather
than three: each further restructure compounds the blast radius of any error sitting in the earlier
edits, and TV-41 is a live example of a guard that read as correct and was not. The window opens with
a compile before anything else runs.

Two findings the tape work surfaced, both new:

- **TV-39 · The Lost cap may be below the legible floor — needs-DD.** Under T16's L2 ceiling the three
  resolution grades can only separate on a colourless ramp, so Lost lands at value 0.33 × alpha 0.15
  and composites near **1.17:1** on this substrate. That is the same complaint T25.6 made about NEXT
  rows. "Loss is darkness" and "readable at four metres" are in tension at L1, and which wins is a
  tier ruling, not a bug in the fix.
- **TV-41 · The retired-red predicate misses the retired red — needs-DD, high.** `LooksLikeRetiredRed`
  requires `c.g < 0.25f`. `#FF4038`'s green channel is `0x40/255 = 0.25098`. **The heuristic does not
  catch the exact colour its own comment says it is "calibrated against"**, by 0.00098 — so three
  shipped guards have been asserting less than they read as asserting, and a naive reuse of the
  predicate would have caught `_green` and missed `_red`. The new scan sidesteps it by asking a
  strictly narrower question (is this field one of the named constants, verbatim?), which does catch
  it. **The threshold itself is untouched on purpose** — widening it changes what three shipped
  guards assert, which is a palette ruling, not a lead call.
- **TV-42 · Field scans are structurally blind to two shapes — needs-window.** (a) Colours constructed
  **inside method bodies** are invisible to any field reflection. `TheaterStage` declares **zero**
  `Color` fields — every colour is a local or parameter, e.g. `new Color(0.62f,0.86f,0.96f,0.9f)` at
  `TheaterStage.cs:2127`, a live cyan no field scan can reach. (b) `Color[]` / `List<Color>` fields are
  not read, so `TheaterPalette`'s team-colour pools remain uncovered. Both are named gaps, not closed
  ones — the source scan is the only instrument that can see (a).
- **TV-40 · `AppendBeat`'s `beneficiary` parameter is now vestigial — minor.** The component no longer
  applies whatever hue it is handed. Worth noting the live caller already passed `contextGrey`, not a
  team hue — the violation was that the *component* would have applied one. Removing the parameter
  touches `TvSweatScreen.cs` and `MomentumTapeTests.cs` together; batch it with the next structural pass.

---

## 2. Systemic findings — these explain most of the table below

### TV-S1 · The brightness ladder is declared and never applied — **violation, systemic** · fix-now
`TierL3/L2/L1` exist (`TvSweatScreen.cs:433`) and the **only** call site in the file is the cash-out
preview step-down (`:1780`). Every other slot ships at its colour's own alpha, which is `1f` for
`flavorColor`, `contextGrey`, `structureGrey`, `goldL2` and `gold` (`:310, :319-330`).

Canon makes tier the **primary semantic channel** (`palette-tv.css:2`, `tiers.js:1-4`). So score, clock,
NEED, progress and the event strip all render at identical maximum brightness — **the ladder carries
no hierarchy at all among fact-tier elements.** This is the single largest 1:1 gap on the surface and
it is the parent of ~8 rows below.

### TV-S2 · Two canon slots are sized but never built — **violation** · fix-now
`TypeTeam` (`:532`) and `TypeLeg` (`:538`) are declared and **never referenced**. The px table matches
canon exactly (`:530-539` vs `typography.css:27-36`) — the sizes are right, the **slot assignment** is
missing. T20 imported the scale without building the slots it belongs to.

### TV-S3 · The palette scan cannot see the class it exists to catch — **high, process** · fix-now
T25.5's real finding: *"a scan that keeps missing this class is the finding, not the pixel."* Here is
why it missed. All three guards look in the wrong place:

- `Retired_green_and_red_fields_no_longer_exist_on_the_type` → reflects **`TvSweatScreen`'s public fields**
- `No_public_colour_field_reads_as_the_retired_saturated_red_or_green` → same surface
- `No_retired_money_colour_hides_in_rich_text_markup_in_owned_runtime_source` → scans for **markup**

The offenders are **private fields on a different owned type** (`MomentumTape.cs:24-26`) — invisible to
all three by construction. Fix: reflect over public **and private** colour fields across **every** owned
runtime type.

---

## 3. Violations

| ID | Gap | Canon | Build | Tag |
|---|---|---|---|---|
| **TV-01** | Momentum tape carries three retired hues | T16: no numerals, **no hue**, never above L2 | `_green #3CE873`, `_red #FF4038`, `_cyan #9EDCF6` (`MomentumTape.cs:24-26`) | fix-now |
| **TV-02** | Tape is a different component entirely | one 28px strip, `MOMENTUM` label + shared centre line (`TvMomentumTape.jsx:18-52`) | one 14px row **per leg**, team-coloured dots (`MomentumTape.cs:15-19,59-113`) | needs-DD |
| **TV-03** | Cash-out actionable is **inverted** in canon | solid `--tv-gold` field, `--tv-gold-ink #0A0C10` punched out (`TvCashOutSlot.jsx:24,30`; `prompt.md:7`) | gold type on dark; zone panel always `screenBg` (`TvSweatScreen.cs:2950,2954`) | fix-now |
| **TV-04** | Status word at money scale | money 29 condensed + status at **eyebrow 15**, "never at money scale" (`TvCashOutSlot.jsx:35-47`) | one Text at 29: `CASH OUT $x   •   UPDATING` (`:2206-2208`) | fix-now |
| **TV-05** | Event strip uses money hues | "never uses money hues" (`TvEventStrip.jsx:5`, `prompt.md:7`) | `_tFlavor` set **gold** (`:1251,1873,2263`) and **chromeCyan** (`:1404,2243`) | fix-now |
| **TV-06** | Event-strip punch is a scale tween | L2→L3 **opacity** step (`TvEventStrip.jsx:11,13`); `DESIGN.md:353` "does not tween between poses" | `localScale 1.12` tween (`:2034,2587-2591`) | fix-now |
| **TV-07** | Pitch markings are cold white | `--tv-pitch #3E4A3C`, **L1–L2 green** (`palette-tv.css:24`, `TvStage.jsx:6,8`) | `pitchLineColor (0.85,0.92,0.95,0.50)` (`:294`) | fix-now |
| **TV-08** | Goal mouths at maximum brightness | markings L1–L2 | `Color.white` α=1 (`TheaterStage.cs:330-331`) | fix-now |
| **TV-09** | Scoreline team hue + saturated dots | names → `--tv-fact`; dots → muted `--tv-team-a/b` (T25.2, `palette-tv.css:22-23`) | saturated pool `0x3D7BFF…` (`SweatPresentationModel.cs:685`), markup-injected (`:1614-1616`) | fix-now |
| **TV-10** | Flavour line owns the stage centre | T25.4 — **out** | `the book is open on the laptop` dead-centre, fact brightness, every frame | fix-now |
| **TV-11** | Idle instructs the player to bet | T27 — `ROUND n OF 8 · BOARD OPEN` in `--tv-fact`, bar no hue | `PLACE YOUR BETS`, saturated, largest on surface; also on ticket-interstitial foot | fix-now |
| **TV-12** | Ticket statement overflows onto the stage | T25.3 — column clips, strings authored to fit | `HorizontalWrapMode.Overflow` (`:3043`) | needs-window |
| **TV-13** | Cash-out copy clipped (`ARKET SUSPENDED`) | condensed band, string fits | regular face + overflow | needs-window |
| **TV-14** | Compact leg row is one span, canon is three | statement · price (`--tv-context` L2) · right-aligned state chip (`TvLegRow.jsx:56-63`) | one Text, state word **first** (`:1747-1790`) | fix-now |
| **TV-15** | Risk/pays is one string, canon is four elements | label eyebrow 15 `--tv-context` above value 24 condensed gold (`TvRiskPays.jsx:7-18`) | `RISK $x     PAYS $y` at 24 gold bold (`:1801`) | fix-now |
| **TV-16** | Ticket-card risk/pays block replaced | two labelled gold cells above a rule (`TvTicketCard.jsx:23-35`) | `$x TO WIN $y` in the event strip (`:1835`) | fix-now |
| **TV-17** | `CASH OUT UNAVAILABLE` never renders | copy at label scale, L1, "explaining an absence" | slot goes blank, `enabled=false` (`:2096,2122`) | fix-now |
| **TV-18** | Stats panel does not exist | full component (`TvStatsPanel.jsx:7-51`) | zero hits for `MATCH STATS` / `PLAYBACK FROZEN` in Runtime | needs-window |
| **TV-19** | One face where canon splits two | condensed: NEED, statement, price, progress, cash-out, risk/pays, team names, stat values, ticket card | `MakeText` assigns `_font` only (`:3037`) | fix-now (in flight) |

## 4. Defects

| ID | Gap | Tag |
|---|---|---|
| **TV-20** | `VOID` uses `chromeCyan #9EDBF5`; canon `--tv-void #7FB2C4` at L2 | fix-now |
| **TV-21** | Lost row needs an **extinguished background** `--tv-extinguished #151B21`, not just dark text (`TvLegRow.jsx:36`) | fix-now |
| **TV-22** | NEXT rows below the legible floor — T25.6 rules them to **L2** | fix-now |
| **TV-23** | No per-row divider; canon rules every row (`TvLegRow.jsx:34`) | fix-now |
| **TV-24** | Progress copy lacks the kit's `LIVE • ` prefix (`ui_kits/tv-sweat/data.js:20,25,30`) | fix-now |
| **TV-25** | TICKET/LEG index split across two zones; canon is one scorebug element (`TvScorebug.jsx:36-39`) | fix-now |
| **TV-26** | `BACKED` chip absent; `MARKET PICK` computed (`SweatActiveLegModel.cs:158`) but never rendered | fix-now |
| **TV-27** | Cash-out rectangle is scaled by `AnimateCashOutTaunt` (`:2564`); canon "never resizes" | fix-now |
| **TV-28** | `CASHED OUT $x` belongs in the slot at L3; build sends it to a 96px full-screen figure (`:2420`) | needs-DD |
| **TV-29** | Event strip ships mixed case (`the board is set.`); canon uppercase | fix-now |
| **TV-30** | Event strip overflows instead of truncating; `Wrap`+`Truncate` is available | fix-now |
| **TV-31** | Ticket card heading 30px regular; canon 36 condensed. Copy `TICKET 2/2` vs `TICKET 2 OF 2` | fix-now |
| **TV-32** | Hyphens where the system uses an em dash — `LEG 3 - DEAD` (T25.7) | fix-now |
| **TV-33** | `◄ ATTACKING` indicator absent (`TvStage.jsx:35-41`) | fix-now |

---

## 5. Falsified — do not re-run

### TV-F01 · `COMPS 10.4` is not a count given a decimal
The engine denominates comps in **tenths**: *"…is an INTEGER count of tenths — no hidden fractional
state"*, `public double Comps => _deciComps / 10.0;` (`engine/Run.cs:59-64`). `10.4` is exact — 104
tenths. "Fixing" it loses information or requires redenominating comps in the economy, on a false
premise. **Routed back.**

### TV-F02 · The footer seed is PRD-specified chrome, not a debug string
T25.7 reads `TVCAPTURE01` as a debug token. It is `r.Rng.RunSeed`, and PRD §8.1 specifies chrome as
*"round, bank, payment, **seed**"*. What the DD saw was the capture harness's seed, which merely looks
debuggy. **needs-DD** — I will not delete a PRD-specified field on a frame reading.

### TV-F03 · MomentumTape is not mis-anchored
Suspected as a sibling of the T25.1 stage bug — it is centre-anchored exactly as `TheaterStage` was.
3C gives it a correctly-placed parent panel, so its zero offset is right. Not an offender.

### TV-F04 · Scanlines / static crawl are already gone
The kit README's "Known debt" item 1 (`ui_kits/tv-sweat/README.md:56-58`) is **stale** — both were
removed under T8 (`842382d`). Items 2 and 3 (`chromeCyan`, emission rest values) remain accurate.

---

## 6. Physically impossible in the current text stack — **needs-DD, high**

C14 admits deviation only where physically impossible. Three canon requirements are unreachable with
Unity's legacy `UI.Text`:

| Requirement | Canon | Why unreachable |
|---|---|---|
| **Tabular figures** | `fonts.css:29-31` — "**mandatory and non-negotiable**" | no OpenType feature control in `UI.Text` |
| **Letter-spacing** | `--tv-track-label .16em`, score `.02em` (`typography.css:59`, `TvScorebug.jsx:46`) | no tracking property |
| **Weight 600** | `TvEventStrip.jsx:10` | `FontStyle` offers Normal/Bold only |

**But "impossible in `UI.Text`" is not "physically impossible" — TextMeshPro supports all three.** The
honest framing is a stack decision: canon relaxes these three, or the TV surface migrates to TMP. A
migration touches every slot, the HDR material path and the C3 one-token invariant, so it is not a
lead call. **This is the largest single needs-DD item in the audit** and it gates any claim that the
surface is 1:1.

---

## 7. Canon-internal tensions — route, do not reconcile

1. **Team hue placement.** `palette-tv.css:4` + `README.md:37-38` + `DESIGN.md:210` confine team hues to
   the pitch dots; `TvScorebug.jsx:19,41,48` and its `prompt.md:7` put team **names** in team hue.
   Audited against the `.prompt.md` per §4A precedence — but T25.2 rules names to `--tv-fact`, which
   agrees with the palette and **contradicts the component**. Canon disagrees with itself.
2. **Cash-out suspended hue.** `TvCashOutSlot.jsx:25` says `--tv-context`; `DESIGN.md §8.5` says "unlit
   slate".
3. **T27 copy.** Ruling prose says `ROUND 1 OF 8 · BOARD OPEN`; the register line in the same document
   and `REGISTER.md:96` say `ROUND n OF 8`. Prose says "cold white", register says `--tv-fact`.
4. **Backed-player marker.** Canon's own 10px numeral (`TvStage.jsx:21-26`) breaks its 12px floor
   (`typography.css:2-3`) — already superseded by T23's ring, noted so it is not re-derived.
5. **`DESIGN.md:462` is stale** — lists the typeface as open; T11 ruled it 2026-07-31.

---

## 8. Ruled and not yet built

### TV-34 · T24's 76px slot does not fit the current budget — **needs-DD**
```
550 canvas − 18 chrome = 532 → −52 bottom row = 480 bottomY
480 − 24 header − 40 footer = 416 rows region
416 / 6 = 69.33px            T24 requires 6 × 76 = 456   →  40px DEFICIT
```
No lever closes it: bottom row 52→12 is impossible (cash-out at 29px needs ~34); 6→5 slots contradicts
`RunConfig.MaxLegs = 6`; header+footer 64→24 cannot hold a 15px eyebrow and a 24px risk/pays. 76px
needs a **590px-tall canvas**; this is 550 (980 × 0.55/0.98). Either the measurement assumed a
different zone set, or a named zone gives up 40px. **I will not choose which.**

### TV-35 · T23 locator ring — **needs-window**
Detached **2px `--tv-fact` ring**, dot ⌀14, gap 1, L3, hue unchanged, no pulse, held while the scorer
leg is live, removed on the resolving frame. Binding half already wired (`949c041`); `RingSprite()`
exists.

### TV-36 · T22 confirm gesture — **needs-window / needs-DD**
Hold previews, **release always abandons, release is never confirm**; commit belongs on the laptop. My
§8.10 preview matches the preview half exactly. Copy is fix-now-shaped: `CASH OUT $183` with `HOLD E`
beneath in context grey, `[E]` **retired**. The cross-surface binding is outside this worktree —
needs-DD on whether Phase 3 takes the bounded fallback (second key during hold).

### TV-37 · T21 stats panel — the two dropped rows, **named** — fix-now (documentation)
The DD asked that they be named and neither the ruling nor `REGISTER.md` does. They are:
1. **current formation for both sides** — no formation concept exists anywhere in `engine/`
2. **player stats** — `Player` carries only `Name`, `Role`, `ScoringWeight`; `ScoringWeight` is hidden
   generator truth, so the only per-player number *is* the leak §8.8 calls blocker-class

Per-team corners and cards **are** sourceable (`CountLedger` tracks `Home`/`Away`).

### TV-38 · T17·a — the reserved goal's spend window must vary — **needs-window**
Ruled confirmed-conditional: *"Weight it late, do not pin it there. If beat-spending cannot vary the
window, that is a defect and it comes back to me."* My T17 reserve is released **in `PlanFinal`** —
i.e. always the final sequence. **On the ruling's own terms that is the defect condition.** Needs a
beat-spending change plus five scorer-leg seeds to show the pattern. `WAITING FOR RACKET` stays.

---

## 9. Proposed order

1. **fix-now, no editor** — TV-S1 (the ladder; parent of most defects), TV-S2, TV-S3, TV-19 (faces),
   then the copy/colour rows: TV-01, 05, 07, 08, 09, 10, 11, 20–33.
2. **One capture window**, exactly the DD's own list: Set B five seeds with **scene index + per-frame
   grammar label + named face**, the 1.8/1.4 bloom A/B (C8·a), and the harness deadline raise that
   stopped seeds 02–05. Re-judge TV-12 and TV-13 from those frames — **not before**.
3. **needs-DD before build** — §6 (the `UI.Text` ceiling; largest), TV-34 (76px vs 416px), TV-02 (tape
   component shape), TV-28, TV-F02, TV-36's binding, and §7's five canon-internal tensions.

**C11 binds all of it:** any claim about how this surface *reads* is made against rendered frames at
the review distance, or it is not made. Sections 2–4 are measurable from source and cited as such;
the overflow rows are not, and are tagged accordingly.
