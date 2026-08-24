# Register entries — batch 183 (2026-08-24)

**TV's ask accepted in full but one condition — and that one decides whether LEG B can be read at
all. Amended before any frame exists, which is the only window an amendment is legitimate in.**

**One row.** **Destination table:** TV (`T163-am4`).

**Amended:** `docs/design/anchor-precommit-2026-08-24.md`. **Ask:** `8d18e33`.

---

## The row

| T163-am4 | The ask's LEG B condition is ONE ZONE SHORT — the backed marker is moneyline-only, so a Handicap frame cannot show which side he backed | **AMENDED — DD 2026-08-24 batch 183, against TV's scoped ask at `8d18e33`, **written before any frame exists**, which is the only state an amendment to a pre-commitment is legitimate in (`T133-am3`'s precedent).** **THE ONE EXCEPTION: the ask requires *"the strip AND the scorebug SIMULTANEOUSLY"*. **THAT IS ONE SHORT AND IT CANNOT SHOW WHICH SIDE HE BACKED.** Verified at this seat: `TvSweatScreen.cs:2906-2907` gates both `awayMark` and `homeMark` on `isMl` — **the `●` backed marker renders on MONEYLINE legs only**, and LEG B is a Handicap. So the scorebug names both clubs and says which is away, the strip names one, **and nothing in frame says which one he took.** §2's condition 6 stands and binds: **the leg's own ROW, the STRIP and the SCOREBUG, in one frame.** Without the row the capture shows an anchor with nothing to check it against — and that is `C55` exactly, on a subject that is an AGREEMENT rather than an element.** **EVERYTHING ELSE IN THE ASK IS ACCEPTED, AND ITS CLAIMS WERE CHECKED RATHER THAN RELAYED: **DoubleChance is genuinely unshootable** — zero `offers.Add(…DoubleChance…)` remain in `MatchModel`, so the removal landed and no new ticket can carry one; its surviving `AnchorSide` arm is precisely what `spec-doublechance-removal` §1 required, grading outliving offering; and `MatchModel.AnchorSide` THROWS on an unknown kind (`:635`), which is `K17-cl`'s exhaustive-and-throw design now sitting engine-side where Allen ruled it belongs. **The exclusion is a shape that no longer exists, exactly as the ask says.*** **`PlayerMultiScorer`'s EXCLUSION ACCEPTED — *"its frame would test LEG B's proposition by LEG B's mechanism"* is correct scoping, and naming it is what keeps its absence from reading as an oversight rather than a decision.** **THE CORNERS REASONING ACCEPTED, AND IT IS SHARPER THAN MY OWN §1: `SweatFlavor.For` returns `CornerLine`/`BookingLine` **EARLY, BEFORE THE ANCHOR IS USED.** So no corners dock could ever have shown this **however or whenever it was shot** — a stronger and more durable statement than *those frames predate the build*, and it generalises to every future ask about this path.** **AND THE BEFORE-PAIR IS UPGRADED FROM AN ECONOMY TO EVIDENCE. The ask states its own gap honestly — *"the suites prove no regression rather than correctness"* — and names LEG A's change as *"it used to name the HOME club and must now name none."* **`drawn-ending-t129-2026-08-19/arm2` IS THAT BEFORE**: `UNDER 1.5 GOALS` + `BTTS — NO` on `GOALLESS-5`, docked, predating `c24b32c` by construction. **Shooting LEG A on that seed and matchup costs nothing and turns a change into a demonstrated FIX** — it is the evidence that closes the gap the ask names.** **CREDIT, BECAUSE IT IS THE THING THAT KILLED THE LAST WINDOW: the ask RAN `C59` BEFORE ASKING, and ran BOTH halves — the mechanism is built (`c24b32c`, `10907a8`) **and** both legs are reachable on a real board. `T148-vf` died because the first half went unchecked; `T149-am2` cost a frame because the second did. **This ask is the first in the rotation to check both before spending a window** | batch 183 |

---

## For the orchestrator

- **The lease can be granted on the amended conditions** — one change from the ask: **LEG B carries
  the leg's ROW as well as the strip and the scorebug.**
- **LEG A on `GOALLESS-5` matchup 0** buys a docked before-pair for nothing. Offered, not imposed.
- **Nothing else in the ask is altered**, and both exclusions stand as scoped.
- **Backlog is 182–183.**

## Limits

- **Nothing measured, no frame read.** The marker check is `TvSweatScreen.cs:2906-2907`; the
  DoubleChance check is the absence of its offer calls; both are quoted rather than summarised.
- **The amendment is legitimate only because no frame exists yet** — stated because that is the
  condition my own §5 imposes on amendments, and it will not be available again once the shutter
  opens.
- **I have not read the ask's suite numbers** and take the baseline as reported.
