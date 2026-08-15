# Register entries — 2026-08-15, batch 70

**THE ACCEPTANCE READ: ALL THREE PASS** — and the `LEAD CHANGE` question ruled, against neither of
the two readings offered. Read at the DD seat on the reshot
`dd-import/tv-goalless-draw-2026-08-14/` (128 frames: 120 endings + 8 mid-match), verified with this
seat's own instrument rather than the harness log.

**Rows shipped:** `T96` **DESIGN-VERIFIED** · `T97` **DESIGN-VERIFIED** · `T87-am2`
**DESIGN-VERIFIED** · `T98` (the `LEAD CHANGE` word).

---

## 1. All three verified — independently measured, not read off the log

| ruling | criterion | measured at this seat |
|---|---|---|
| **T87-am2** | the line **visible for multiple frames BEFORE the grade**, never the same frame | **PASS.** `scene002`: `THE MATCH ENDS LEVEL` frames **000–050**, `LEG 1 — WON` frames **051–059**. **51 frames = 1.02 sim-seconds**, matching the ruled `drawnEndingHoldDuration = 1.0f`. The first five states are the entrance punch decaying, not a text change |
| **T97** | **no goal sentence anywhere** in the set | **PASS.** Across all 128 frames the strip carries only `THE MATCH ENDS LEVEL`, `LEG 1 — WON`, and the mid-match line. **No `ScoreUp`/`ScoreDown`/`BigPlay` sentence appears** |
| **T96** | the live **NEED** pair — the clause the previous set asserted without a frame | **PASS.** Mid-match shot: `TICKET 1/2` · **`LEVEL AT FULL TIME`** over progress **`LEVEL`**, on a `MALLARDS 0 — 0 MIDDLEMEN` scorebug at **`30'`**. Exactly S74's authored pair, and the T70-am pairing holds on the frame |

**Coverage note, not a defect:** `scene003` (team-backer) holds `THE MATCH ENDS LEVEL` for **all 60
frames** — its grade beat falls outside the 1.2 sim-second window — so **`LEG 1 — DEAD` is not in
evidence in this set.** It was verified on the batch-68 set and nothing has changed in that path. **No
supplemental shot is required for it.**

## 2. This seat's hypothesis was wrong, and the process is why that cost nothing

Batch 69 offered a **race** between `_pendingFlavor` writers, explicitly as a hypothesis rather than a
frame claim, and asked for one diagnostic. **The diagnostic was run and the answer was different:**
`RevealBeatChrome` — the only thing that lands `_pendingFlavor` — sits inside `TheaterBeat`'s
`evt.Type != LegFinal` branch, **so on the whistle the stash was simply dropped. There was no race;
the line was correct, reachable and never displayed.**

**The half that matters more is T97's, and it vindicates the law's exact wording.** The guard was
gating on `spec.Goal.HasValue` — the beat's **staged intent** — where `Commits` is what the scene
**resolves into**. **The law says the words are licensed by what the resolved scene CONTAINS; the
implementation read "what it staged".** Now gated on `spec.Goal.Value.Commits`. **A law written one
word loosely would not have caught that**, and the distinction between *staged* and *resolved* is now
worth carrying forward explicitly.

---

## T98 — `— LEAD CHANGE` OVER A 0–0. Neither reading, and the word comes off.

**The frame:** `Middlemen squeezing the half. — LEAD CHANGE` over `MALLARDS 0 — 0 MIDDLEMEN` at
`30'`, on 8 of 8 mid-match frames.

### The tag is real. Reading A's diagnosis is wrong.

`DramaGenerator.cs:116` assigns the tag on

```
(p[i-1] − 0.5) × (p[i] − 0.5) < 0.0
```

— **the win probability crossing 0.5.** It is **not** derived from the scoreline. `DramaEvent`'s own
doc confirms the series: *"`WinProbAfter` is the honest live probability of the PICKED side after this
beat; cash-out prices off it."*

**So nothing phantom happened, and this is NOT T97's law a third time.** T97 governs *words asserting
an event the resolved scene does not contain*; **here the scene does contain the event.** **Applying
T97's guard would suppress a real fact** — the wrong remedy, reached through the wrong diagnosis.
**Saying so precisely matters: three defects in this one slot have now had three different
mechanisms, and the guard fixes only one of them.**

### Reading B's diagnosis is right and its remedy is BANNED

**Making the win-probability distinction visible re-introduces exactly what this surface deleted.**
§8, standing:

> **The theatre prints facts and offers. It does not print opinions** (T16, T23, T32, T86-am). A price
> is an offer — the house stands behind it and the player transacts against it. **A probability is the
> house's opinion**, with nothing attached: he can take or leave a price, but he can only agree or
> disagree with an opinion, and this surface does not ask him to. **That is why the win-probability
> numeral, the backed-player numeral and the 10px numeral all went.**

**A strip line announcing that the probability crossed 50% is the deleted numeral's MEANING without
its digits.** Authoring the distinction visible would state the house's opinion more plainly than the
numeral ever did.

### RULED — the word comes off. The TAG stays.

**`— LEAD CHANGE` is removed from the strip** (`SweatFlavor.cs:47`). Not because the event is fake,
but because **the event is the house's opinion, and this surface does not print opinions.**

**The fact is not lost, and this is what makes the removal honest rather than a suppression.**
`WinProbAfter` already reaches the player **as an offer**: the cash-out price prices off it, so a
probability crossing 50% is already visible as **the cash-out price moving through its own midpoint.**
**The offer is this surface's permitted expression of the probability; the word is the banned one.**
Nothing he can act on disappears.

**THE TAG IS NOT REMOVED.** `TheaterChoreographer.cs:90,117` and `TvSweatScreen.cs:3811`
(`leadChangeMs`) use it for **staging and pacing**, and that use is untouched and correct.

> **A tag may drive timing and staging without earning a word.**

**That is the third distinction this one slot has forced, and the set is now worth stating together:**
a beat's **type** does not license its words (T97); a beat's **staged** intent is not its **resolved**
content (T97, as corrected on this set); and a beat's **cue** is not its **copy** (T98).

### The second defect the same fix closes

`Middlemen squeezing the half. — LEAD CHANGE` carries **sentence case and uppercase in one line**,
against §8's **one casing, one dash**. **Removing the suffix closes this too** — the sentence-case
line stands alone and correct. **One fix, two defects**, and the casing breach is only visible because
the suffix was appended to a line authored in another register.

### If a scoreline lead change ever earns a word

**It is licensed by the SCORELINE and takes its own tag and its own authored word** — never this one.
**It probably never needs one:** a scoreline lead change is already visible in the scorebug, which
changes on a single frame with no intermediate state (T38). **Nothing is owed here; the door is left
open rather than a requirement created.**

---

**Routing:** T98 to **tv-sweat** — a one-line removal, no capture owed (the absence of a suffix is
verifiable on any existing mid-match frame). **T96, T97 and T87-am2 are closed.**
