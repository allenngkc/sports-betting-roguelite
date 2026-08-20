# Register entries — batch 140 (2026-08-20)

**`C56`'s SWEEP IS RUN, AND IT FOUND THE REMEDY RATHER THAN A FOURTH INSTANCE.** The TV's silent
`default` **shadows a loud guard that already exists one layer down.** The same enum is mapped three
ways in this codebase, two of them fail loudly by design, and the one that fails silently sits in
front of them.

**Three rows.** **Destination tables:** Cross-surface (`C56-am`) · TV (`T130-am`) · Phone (`P9`).

---

## The population, swept

Every `MarketKind` reference in the Unity runtime, by file, with its `case` arms:

| mapping | style | on an unmapped kind |
|---|---|---|
| `MarketDestinations.For` (laptop) | switch **expression**, all 15 mapped, **no discard arm** | **THROWS** `SwitchExpressionException` |
| `SweatActiveLegModel.Describe` (TV model) | switch statement, 6 arms | **THROWS** `ArgumentOutOfRangeException("unsupported market kind")` |
| `TvSweatScreen.DescribeActiveLeg` | switch statement, 6 arms | **returns empty strings** |
| `TvSweatScreen.LegStatement` | switch statement, 6 arms | **returns `Kind.ToString()`** |

**`PhoneScreen.cs`: zero references.** **`SportsbookApp.cs`: four references, zero case arms.**

---

## The rows

| C56-am | The sweep is RUN — and the sharpest form of the clause is a returning `default` SHADOWING a throwing one | **AMENDED — DD 2026-08-20 batch 140, and the amendment is the remedy `C56` did not have.** **`C56` clause 2 says *a `default` that RETURNS something is more dangerous than one that THROWS.* THIS CODEBASE HAS BOTH, IN ONE CALL CHAIN, AND THE RETURNING ONE IS IN FRONT.** **`SweatActiveLegModel.Describe` ends `default: throw new ArgumentOutOfRangeException(… "unsupported market kind")` — a loud guard, correctly written. `TvSweatScreen.DescribeActiveLeg` switches on the same enum FIRST and its `default` returns `new ActiveLegCopy(string.Empty, …)` WITHOUT CALLING IT.** **So the nine unauthored kinds would have failed loudly at the model, and never reach it: the surface catches the fall.** **THE CLAUSE, SHARPENED: A SILENT `default` IN FRONT OF A LOUD ONE IS WORSE THAN EITHER ALONE — it converts a designed failure into a printed enum name AND makes the guard unreachable, so the guard's existence is evidence of nothing.** **AND THE REMEDY IS NAMED BY THE CODEBASE ITSELF: `MarketDestinations.For` maps all fifteen kinds as a switch EXPRESSION with NO DISCARD ARM, and its own comment says the loudness is deliberate — *"…rather than hiding on the surface) instead of silently resolving to some destination."* THE LAPTOP AVOIDED THIS ENTIRE CLASS BY NOT USING A FALLIBLE SWITCH.** **THE POPULATION IS NOW SWEPT AND THE SWEEP IS CLOSED: four surfaces, three exposures (`S86` laptop, `K2` console, `T130` TV), one surface with NO exposure (`P9`, the phone), and no fifth site. `C56`'s test — sweep the population, not the suspects — is discharged for this enum** | batch 140 |
| T130-am | `T130` refined — THREE switches, not two, and the remedy is to DELETE the silent defaults, not only to author | **AMENDED — DD 2026-08-20 batch 140. `T130`'s ruling stands; its mechanism was one layer short.** **`T130` recorded two `default` arms failing differently. THERE ARE THREE SWITCHES ON THIS PATH and the third is the one that matters: `SweatActiveLegModel.Describe` has its own six-arm switch ending in a THROW.** **So the TV's silent `default` is not merely a gap — it is a BYPASS of a guard the model already provides.** **CONSEQUENCE FOR `G1`, and it changes what "done" means: authoring the nine forms (`T151`, `T152`) is necessary and NOT SUFFICIENT. Nine authored forms with the two silent `default`s left in place leaves a SIXTEENTH kind printing an enum name exactly as the ninth did.** **RULED: when `G1`'s forms land, the two silent `default`s in `TvSweatScreen` are DELETED so the model's throw becomes reachable — `DescribeActiveLeg` falls through to `SweatActiveLegModel.Describe`, and `LegStatement` stops falling back to `leg.DisplayLabel`.** **THAT IS A SMALLER CHANGE THAN IT SOUNDS AND IT IS THE ONE THAT MAKES THE FIX DURABLE: it converts this defect from one that must be re-found every time the vocabulary grows into one that cannot ship.** **`MatchModel.DisplayLabel`'s legacy path is then unreferenced from this surface, which discharges the debt its own comment names — *"pending that lead's own migration to `Fields`"* — as a consequence rather than as a separate job** | batch 140 |
| P9 | The phone has NO exposure to the market vocabulary — a NEGATIVE check, recorded so it is not re-run | **CHECKED AND RECORDED — DD 2026-08-20 batch 140, under `T62-am`'s precedent that a negative check which is not written down gets re-run.** **`C56` is explicit that the surfaces fail DIFFERENTLY, so finding the nine-kind gap on three surfaces says nothing about the fourth. The phone was the unchecked one.** **`PhoneScreen.cs` (320 lines) contains ZERO references to `MarketKind`, `Leg`, `Ticket`, `DisplayLabel` or any market composer.** **THE REASON IS STRUCTURAL RATHER THAN LUCKY, which is why this closes rather than merely passing: the phone is the BOOKIE THREAD — *"M5's face-up bookie thread… with stamped rounds from the model rather than the live run"* — and `R28-am` keeps the live `BookieFeed` and FORBIDS anything being authored onto the screen. It carries the bookie's messages, not the slate.** **So the phone cannot acquire this defect while that ruling holds, and if the phone is ever given market content it inherits the exposure and this row is where a reader should start.** **NOT A CLEARANCE FOR THE ROOM: the room owns the phone's OBJECT and its emission (`R28`, `S63`), not its content, and no room surface renders market text — checked in the same sweep, zero references** | batch 140 |

---

## What is NOT in this batch

- **No authoring.** `T151` and `T152`'s forms are unchanged; `T130-am` adds a deletion to the same
  build, not a new string.
- **No build.** The measurement brief is queued at TV and this rides with it.
- **No claim about `SportsbookApp.cs`'s four references** — zero case arms, and the laptop's mapping
  is `MarketDestinations`, which is exhaustive. Not swept further.
