# S67 — helper-bypass inventory, the laptop surface

**From:** SureThing UI lead · 2026-08-07 · at `58ecfe1`
**Asking for:** nothing yet. **Inventory only, as ruled — nothing here is fixed.** One item wants a
ruling before it can be fixed at all; the rest are reported so the list is bounded.

**Scope swept:** `SportsbookApp.cs` and `LaptopOs.cs` — the laptop's two runtime files — for **values
composed by hand where a shared helper already produces them**: identities, money, odds, state words,
counts, scope lines.

---

## 1. Confirmed bypass — one, and no helper exists to bypass

### The scope line is built inline, twice

| Site | Builds |
|---|---|
| `SportsbookApp.cs:97` (masthead) | `$"ROUND {run.Round} OF {run.Config.Rounds}  ·  PRICES FINAL"` |
| `SportsbookApp.cs:2097` (ledger) | `$"ROUND {run.Round} OF {run.Config.Rounds}"` |

**This is the one with a ruling already attached to it.** S37 rules that the masthead carries the
run's scope, the board header carries the screen's, nothing restates either, and **the live round
number appears exactly once**. Owning doc §5 repeats it. S61 was a violation of exactly this clause
and cost a batch.

**And there is nothing for that ruling to land on.** Both strings are assembled at their call site
from `run.Round` and `run.Config.Rounds`. A third screen that wants to say what round it is will
write a third one, and the only thing standing between the surface and another S61 is that somebody
notices.

**This differs from S64 in the way that matters.** S64 was a call site that *failed to call* an
existing formatter — visible by comparing three sites against one helper. Here the helper was never
written, so there is nothing to compare against and the duplication is the only evidence. **Left
unfixed deliberately**: extracting a scope formatter is a design decision about what the canonical
scope string is, and the two current sites already disagree (one carries `· PRICES FINAL`, one does
not). That is a ruling, not a refactor.

---

## 2. Hazards — not defects, but a ruling cannot reach them

### 2.1 Two `LegStateWord` overloads with different vocabularies

| Overload | Returns | Serves |
|---|---|---|
| `LegStateWord(RevealedLegState)` — `:1295` | GREEN · DEAD · VOID · LIVE · PENDING | the MY BETS mirror (S23, S35c) |
| `LegStateWord(Leg)` — `:2211` | VOID · WON · LOST · PENDING | the LEDGER's settled legs |

**Both are correct and the difference is deliberate** — a revealed leg and a settled record do not
speak the same vocabulary. That is why this is listed as a hazard rather than a defect.

**The hazard is that the compiler chooses between them silently, by parameter type.** A call site
that acquires the other type gets the other vocabulary with no error and no warning, and a ruling
phrased as *"the leg state word"* reaches exactly one of them. This is S64's mechanism with the
polarity reversed: there, one screen never called the helper; here, two helpers answer to one name.

### 2.2 The LEDGER's terminal ticket word has no helper at all

`SportsbookApp.cs:2233` builds `WON / LOST / CASHED OUT / OPEN` inline, while the mirror's equivalent
is the named `TicketStateWord` (`:1288`) returning `GREEN / DEAD / CASHED OUT / RIDING`.

Again the vocabularies legitimately differ. **But one has a name and one does not**, and the one
without a name is the one on the Design-verified screen.

**This is precisely S64's shape before S64 was ruled** — and it was not found by the S64 fix, because
the S64 fix was about identity, not state. It was found by this sweep, which is the argument for the
sweep.

---

## 3. Unresolved — I could not tell whether this is a bypass

**`SportsbookApp.cs:351-352`** prints `matchup.Away.Name` and `matchup.Home.Name` in full, mixed
case, inside the FORM stats line. `LaptopUi.TeamShort` exists, is used at seven other sites, and
returns the last word uppercased (`Tulsa Middlemen` → `MIDDLEMEN`).

**Whether the stats line wants the long form is a kit question I did not resolve.** A stats row
plausibly wants the full name where a price row wants the short one — but it is equally plausible
that this line simply never called the helper, which is the whole class this sweep is looking for. I
have not opened the kit's stats component. **Naming it rather than guessing, and it is one file to
check.**

---

## 4. Clean — a helper exists and every site calls it

| Helper | Sites | Bypasses found |
|---|---|---|
| `LaptopUi.Money` | 15 | **0** |
| `SportsbookApp.Pluralize` | 4 | **0** |
| `LaptopUi.TicketIdentity` | 3 | **0** (was 1 — S64, fixed at `58ecfe1`) |
| `LaptopUi.TeamShort` | 7 | 0 confirmed, **1 unresolved** (§3) |
| `SportsbookApp.LegStateInk` | 1 | **0** (created by S65; the inline ternary it replaced was the defect) |

`Money` being clean at fifteen sites is worth stating as the counter-example: **this shape is not
inevitable.** Where a helper exists, is named for the value rather than for a screen, and predates
the call sites, it gets called.

---

## 5. What this sweep cannot see (C18 §4.2)

1. **It is source-only.** No rendered check. A hand-built string that happens to produce the same
   characters as the helper is invisible here and would be invisible on a frame too — until the
   helper changes and only one of them follows.

2. **The largest blind spot, and it is structural: a value composed once, wrongly, with no helper and
   no duplicate, cannot be found by this method.** Every item above was found either by comparing
   call sites against an existing helper (§1 excepted) or by noticing duplication. A single screen
   quietly formatting something its own way, only once, matches neither test. **The sweep bounds the
   recurring shape; it does not prove the surface is clean.**

3. **Two files only** — `SportsbookApp.cs` and `LaptopOs.cs`. `TvSweatScreen.cs:1155` builds
   `$"TICKET {SweatIndex + 1}/{Sweats.Count}"`, which I read as a progress counter ("ticket N of M")
   rather than an identity, on another surface, and did not pursue. **If that reading is wrong it is
   a fifth instance and it is the TV's.**

4. **Value-producing helpers only.** Layout and geometry helpers — `MakeRule`, `MakeMarginRow`,
   `InkRingGeometry`, `MakeWaxPrimary` — were not swept for bypass. `MakeRule` has form: it could
   only ever draw `--rule-soft` for a period, and `LaptopOs.Rule` sat dead beside it. **A geometry
   sweep is a different item and I am not folding it in here**, for the reason S67 exists.

5. **Bypass versus deliberate difference is a judgement I made per case, from the kit and the
   register.** Where I could not make it (§3) I said so rather than deciding. Two of the four items
   above are reported as hazards precisely because the difference *is* deliberate — the defect is
   structural, not the value.
