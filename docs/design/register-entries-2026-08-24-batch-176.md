# Register entries — batch 176 (2026-08-24)

**There is no shared-telling form to invent — `T140` already decomposed the shared whistle into N
per-leg statements, so `LEG k` stays and `k` is EACH LEG's number.** What ships is the anchor's
number printed once, and on a split fixture that states a grade which is not the other leg's.

**One row.** **Destination table:** TV (`T167`).

**Two source reads and one spec clause. No frames — and the frame question is named rather than
answered.**

---

## The row

| T167 | What a shared telling's copy calls itself — `LEG k` STAYS, and `k` is each leg's own number | **RULED — DD 2026-08-24 batch 176, on TV's routed question at `9d7735e`. THE ANSWER IS DERIVED FROM `T140`, NOT AUTHORED HERE.** **CREDIT FIRST, BECAUSE IT IS THE REASON THIS IS CHEAP: TV refused to invent a form and named the question at the call site — `TvSweatScreen.cs:2162-2164`, *"What a shared telling's copy should call itself ('LEG 1', 'LEGS 1 & 3', something else) is a DESIGN question and it is NOT ruled; inventing a form here would be this lane deciding it."* **That is exactly right, and the form it would have invented is the one the spec makes unnecessary.*** **THE QUESTION IS ALREADY ANSWERED. `T140` and `spec-drawn-ending-2026-08-19.md` §3.2 read: *"At the whistle, the grades land in LEG ORDER, after `T87-am2`'s hold. **N LEGS, N GRADES, ONE HOLD** — not N holds."* **A shared whistle is already decomposed into N PER-LEG statements. There is no telling-level copy to name, because the telling does not speak — its legs do, in order, under one hold.*** **SO `LEG k` STAYS, UNCHANGED, AND `k` IS EACH LEG's OWN NUMBER. What ships is `int k = evt.LegIndex + 1` — the ANCHOR's number, once (`:2165`, feeding `WonLegBeat(k)`/`DeadLegBeat(k)` and `$"LEG {k} — WON"` at `:4342`). **On a fixture carrying legs 1 and 3 that names leg 1, silently omits leg 3, and — where the fixture SPLITS — states a grade that is not leg 3's.** BUILD ORDER: N lines, leg order, each with its own number and its own grade; `evt.LegIndices` already carries the set.** **WHY NOT `LEGS 1 & 3`: it asserts ONE grade for TWO legs, which is false the moment the fixture splits — and a fixture splits whenever its legs are `UNDER 2.5 GOALS` and `BTTS YES`. **The register has ruled this shape twice: `T156`'s two markets rendering as one string, and `T96`'s draw borrowing a team's row. A form that cannot express a split is not a shorter form, it is a WRONG one.*** **WHY NOT THE FIXTURE'S NAME: refused on `T87-am2`'s own ground — the scorebug carries the fixture one slot away, and `T126` already refused `FULL TIME — LEVEL` for exactly that duplication.** **AND KEEPING `LEG k` DUPLICATES NOTHING: the COLUMN carries each leg's STANDING state (the `W`/`L`/`VOID` chip); the STRIP states each grade AS IT LANDS. Different tense, different job — `T87-am2`'s *the strip's job is to say what the score and clock cannot*, one slot over.** **THE SINGLE-LEG CASE IS UNTOUCHED and the engine says so itself (`DramaEvent.cs:20-21`): *"on a ticket with at most one leg per matchup every telling has exactly one leg, so all four agree and nothing moves."*** **NOT RULED, AND IT IS A FRAME QUESTION RATHER THAN A COPY ONE: HOW N GRADE LINES SHARE ONE HOLD. `_tFlavor` is a single slot, so N lines in one hold must either subdivide it or overwrite. **Whether that reads as a list or a stutter cannot be settled from a source read** — it is what `D1` and `D3` are for, and `D1` is already the frame the phase holds on.** **ALSO NOT THIS ROW: the scorebug's `LEG n/m` counter (`:2747`, `:3730`). That is `T165` and step 3 — a DIFFERENT SLOT, already ruled to count FIXTURES rather than legs, and the two do not conflict: the counter says which telling this is, the strip says what each leg did** | batch 176 |

---

## For the orchestrator

- **Non-blocking, as TV said** — this is a build order for whenever step 2's grade path is next
  touched, not a stop.
- **`T167` (strip) and `T165` (counter) are different slots** and step 3 can take the counter without
  waiting on this.
- **One thing to watch on `D1`:** N grade lines sharing one hold. It is the only part of this that a
  frame decides, and `D1` is already the phase's holding frame — no extra capture is asked for.
- **Backlog is 173–176.**

## Limits

- **Nothing measured, no frame read.** The defect in what ships is derived from `:2165`'s single `k`
  against §3.2's *N grades*, both quoted.
- **The split case is argued, not observed** — a fixture whose legs grade oppositely is constructible
  (`UNDER 2.5 GOALS` + `BTTS YES`) but no frame of one exists.
- **`T167` rules the FORM, not the timing.** How N lines occupy one hold is named as owed to `D1`.
