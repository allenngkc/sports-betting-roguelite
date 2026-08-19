# MANDATE SCOPE — the two new phases, before either is explored

**Written:** Design Director seat, 2026-08-19 · **Mandate:** `docs/5-orchestration/dd-mandate-2026-08-18.md`
**Status:** SCOPE ONLY. No exploration, no proposal, no ruling. **One question for Allen; Phase 2
starts without waiting on it.**

---

## 1. PHASE 1'S SCOPE NOTE RESTS ON A CITATION THAT DOES NOT SAY WHAT IT IS CITED FOR

The mandate reads:

> Scope note: `game-console` was ruled a dead prototype (**T44**) — establish early which surface
> actually carries this phase (the in-fiction play apparatus, not the dead prototype).

**`T44` is "Event-strip copy drift".** It is a voice violation ruled 2026-08-02 — *"off the bar — a
miracle brewing?!"*, banned register, strip exclamations and superlatives, normalise hyphen to em
dash. **It says nothing about prototypes, and no row in the register rules `game-console` dead.**
Searched the whole register for *dead prototype*, *prototype* and *game-console*: **one hit, `T97`,
and it cites `game-console/EventText.cs:111` as the LIVE source of a shipped scorebug string.**

**What `T44` actually did to `game-console` is the opposite of retiring it.** Its own closing commit
reads *"**T44 closes: the console twin swept** — and this half actually compiles."* The sweep was
applied **to** that surface.

**And it has happened three times.** `T44` swept it; `T97` read a shipped defect out of it; and `T98`'s
commit reads *"the lead-change word comes off **the console twin too (Allen's call)**"* — **Allen
personally directing a DD ruling onto this surface, twelve days ago.**

**Stated plainly and without inference: `game-console` is a live, maintained surface that DD rulings
are routinely applied to.** Whatever its status should be, *"ruled a dead prototype"* is not its
recorded history, and a spec written on that premise would be written on a false one.

## 2. WHAT `game-console` ACTUALLY IS — measured, not characterised

| | |
|---|---|
| what | `SBR.ConsoleGame`, a .NET 10 console application |
| source | **1,282 lines** across 6 files — `BettingScreen` 346, `SweatRenderer` 317, `GameLoop` 238, `EventText` 194, `Ui` 117, `Program` 70 |
| engine | links **`SBR.Engine.dll`** — the real engine, not a stub |
| state | compiles and builds (`SBR.ConsoleGame.exe` present) |

**`BettingScreen.cs` is a betting surface in the literal sense the phase describes** — it renders the
slate, the schedule, tickets, relics and consumables, and takes the commands that place bets.

### 2.1 And it measures as EXACTLY what the mandate describes

The mandate says the surface *"still presents on pre-expansion assumptions."* **Measured against
`MarketKind`'s fifteen members:**

**`BettingScreen.cs` references SIX — Moneyline · TotalGoals · BothTeamsToScore · TotalCorners ·
TotalCards · AnytimeScorer. The other NINE do not appear at all.**

**Those are the same nine `S86` measured as homeless on ENTRY.** It is `C19` one surface over, in the
same shape, with the same nine kinds missing — *an offer the engine prices is reachable on the
surface; hiding it misrepresents the slate.*

**So the phase's description fits this file clause for clause, including the one clause that is
measurable.**

## 3. THE QUESTION FOR ALLEN — and it is a real fork, not a nitpick

The citation is wrong. **The scope note's CONCLUSION may still be right**, and I will not assume
either way, because the two readings produce very different specs.

- **Reading A — the phase is `game-console`'s betting surface.** A presentation pass on an existing
  347-line terminal surface. **Bounded and cheap:** the destinations, the contents block, the folio
  and the counts are all ruled already (`S89`–`S92`, `S95`, `S98`), and most of that thinking
  transfers. The phase is *bring the second surface up to the vocabulary the first one now carries.*
- **Reading B — the phase is a NEW in-fiction apparatus**, and `game-console` is merely the
  out-of-date thing it must supersede. **Coherent, and much larger:** it needs an identity ruling
  first — *what is this object in the fiction, and why does the player use it rather than the
  laptop?* — before a single surface decision can be made.

**RECOMMEND A**, on four grounds: it matches every clause of the phase's own description including
the measured one; the surface is live and Allen has himself directed a ruling onto it; it is the same
`C19` failure just closed on the laptop, so the work is largely transfer rather than invention; and
**B is not a presentation pass at all — it is a new surface**, which is a materially bigger commitment
than the mandate's own framing suggests.

**If Allen means B, I need the identity call before speccing** — the laptop, TV and phone each have a
settled in-fiction identity, and a fourth apparatus without one would be the `08` mistake again.

## 4. PHASE 2 NEEDS NOTHING FROM ALLEN AND STARTS NOW

**Groundwork checked before requesting anything, per the standing rule.** `dd-import/tv-goalless-draw-2026-08-14/`
holds **128 frames** on seed `GOALLESS-5`, `Atlanta Middlemen 0 – 0 Scranton Mallards`, both tickets on
one settlement:

| moment | frames | mandate's bet type |
|---|---|---|
| `goalless-draw-backer-ending` | **60** | the 1X2 draw-backer's win ✓ |
| `goalless-team-backer-ending` | **60** | the team-backer's quiet loss ✓ |
| `goalless-draw-backer-live-need` | 8 | mid-match, `30'`, the live NEED clause |

**Two of the mandate's four bet types are already photographed at 60 contiguous frames each**, on one
matchup and one stake, so the loud half and the quiet half differ only in what was backed. **No
capture window is needed for either.**

**The two that are NOT in evidence, named precisely so a window can be sized rather than guessed:**

- **count legs settling level** — a corners or cards leg on a drawn match
- **correct-score `0-0` and its siblings** — and this one is new territory: `CorrectScore` had no
  reachable home until `S95` gave it one, so **no capture of any kind exists for it**

**Canon already binding this phase, and none of it reopens:** `T87-am2` authored `THE MATCH ENDS
LEVEL`; `T97-am` ruled the strip's words licensed by the **resolved scene**; `T96` gave the draw its
own row; **`T98` already ruled the `— LEAD CHANGE` finding these very frames routed** — *the word
comes off, the tag stays* — so that item is closed, not open.

**The phase's real subject, stated as the exploration's starting point rather than as a finding:** the
drawn ending is currently **one authored line at the whistle**. The mandate asks for *a full ending
arc as a first-class broadcast moment.* The distance between those two is the phase.

## 5. NOT CLAIMED

- **No frame has been read for this note.** §2 is a source count, §4 is a file inventory. Nothing here
  says how anything looks.
- **Nothing is proposed for either phase.** This establishes what they are about, which Phase 1's own
  scope note asked to be established early.
- **No judgement on whether `game-console` SHOULD be a shipping surface.** That is Allen's, and it is
  precisely §3's question. What is recorded is only that it is not what the mandate says it was ruled.
