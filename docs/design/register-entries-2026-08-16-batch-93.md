# Register entries — 2026-08-16, batch 93

**(c) RULED — THE PANEL IS THE PER-TEAM SPLIT OF THE COUNTS THE TICKET RIDES ON.** Written at the DD
seat on Allen's word bringing batch 89's option (c) forward.

**Destination table: TV — match theater.** **Rows shipped:** `T104` (the panel's subject and row set) ·
`T104-am` (the GOALS row, removed) · `T105` (the title, flagged with its coupling).

---

## T104 — THE SUBJECT FIRST, because the row set falls out of it

**Three surfaces carry the match's numbers, and until now nobody had said which number is whose.**

| surface | what it gives |
|---|---|
| the **scorebug** | **the RESULT** — the scoreline and the clock |
| the **ticket column** | **each leg's own PROGRESS** toward its line |
| the **panel** | *— unstated, and that is why it drifted into a general summary* |

> **RULED: the panel gives the PER-TEAM SPLIT of the counts the ticket rides on — the breakdown
> neither of its neighbours can give.**

**§6.6 already ships per-team corners and cards from `CountLedger` (T36); the panel has always been a
per-team surface.** What (c) adds is that **its subject is the ticket's**, which is what *opens from
the ticket column* has meant all along and was never true of its content.

### The row set

> **One row per countable quantity the ticket's legs ride on, deduped — the unit is the QUANTITY, not
> the leg.**

Two corners legs at different lines ride on one count and take **one** `CORNERS` row. **Rows appear in
a fixed canonical order: the ticket selects WHICH rows appear, never in what order** — leg order is
insertion order and arbitrary, and the same quantity must always read in the same place.

---

## T104-am — THE GOALS ROW IS REMOVED, and the argument is STRUCTURAL rather than a duplication complaint

**T103 found that the row duplicates a scorebug that is never covered. True, and not the strongest
reason.**

> **The panel exists to give the per-team SPLIT. For goals there is no split to give.**
> **`Yams 0 | Zambonis 0` and the scorebug's `YAMS 0 — 0 ZAMBONIS` are the same two numbers, in the
> same order, one slot apart.**

**There is nothing to add, not merely something repeated** — and that holds however the panel is
keyed, which is why (c) does not dissolve it on its own and it had to be ruled.

**And every goals market is served by the scorebug directly:** moneyline reads the result; BTTS reads
both numbers; correct score **is** the scoreline; over/under totals is their sum, of two single
digits. **Removing the row costs no fact.**

**A scorer leg takes no row either, and by the same rule rather than a special case:** it rides on
goals, which have no split — and player stats are barred outright (§6.6, a generator-truth leak).

---

## The consequence, stated plainly rather than buried, because it is what (c) means

**A ticket that rides only on goals gives the panel nothing to show. Moneyline-only tickets — very
likely the commonest — have no rows at all.**

> **RULED: the panel does not open empty. Where the ticket gives it nothing, its affordance is
> UNAVAILABLE and says so BEFORE the key is pressed.**

**That is S85's law, surface-independent and made on the laptop three batches ago:** *where a refusal
is knowable before the act, the surface shows it before and the act never happens; a dead click is a
knowable refusal left to be discovered.* **Here it is knowable at all times from the ticket, so it
never needs discovering at all.**

**THE PRODUCT CONSEQUENCE, NAMED: (c) makes the panel a COUNT-TICKET feature.** **It is available
where it has something to say and unavailable where it does not.** **That is also the honest answer
to batch 87's scope question** — the panel stopped being a surface that takes the stage to show two
numbers, because it now only exists when it has numbers worth taking it for. **Allen ruled (c) and
this is what (c) is; it is stated so he is not told it later.**

---

## §2 is satisfied, and here is why — because it is the objection a careful reviewer will raise

**A varying row count looks like a zone resizing to content**, which §2 forbids and which the draws
block was held to (*an empty line is honest where a collapsing block is not*).

**It is not the same case:**

- **The row set is derived ONCE, when the ticket is placed, and is constant for that ticket's life.**
  A ticket's legs are fixed at placement.
- **§2 forbids a zone that resizes IN RESPONSE TO CONTENT while the player is watching. This one
  cannot change under him** — there is no frame in which he sees it move.
- **And he can never see two of them at once**, which is the side-by-side comparison the ragged-list
  case is actually about. **The draws block had siblings; the panel has none.**

---

## The unrevealed mark SURVIVES — and it changes meaning, which is the prize

**TV named the engine fact: `_countLedger` is null off a count leg, carries exactly one kind, and
resets per leg.** **So a ticket may ride on more quantities than the ledger can currently populate**,
and the unpopulated row keeps its `—`.

**That is not a leftover. It is the mark finally meaning something:**

| | today | **under (c)** |
|---|---|---|
| `CARDS —` | *this quantity is not in your ticket* — a row you never asked for | **your ticket rides on this and it is not revealed yet** |

> **The unrevealed mark stops meaning IRRELEVANT and starts meaning NOT YET — which is a fact he
> wants, where the first was noise.**

**OWED, not blocking: whether the ledger can carry more than one kind at once.** **The panel is
correct either way** — it only changes how often a second row shows `—` instead of a number. **An
engine question, routed as one, and no geometry waits on it.**

---

## T105 — `MATCH STATS` may no longer be the title, and it is coupled to the box

**Under (c) the panel does not show the match's stats. It shows the counts one ticket rides on.**
**The title now overstates its subject** — the same class as the masthead's deleted `PRICES FINAL`
and the strip's refused `FULL TIME`.

**NOT AUTHORED HERE**, and the coupling is the reason it is flagged rather than changed in passing:

> **`MATCH STATS` at 155.8px IS the widest ink in the label column and therefore sets `labelW = 195`
> (T102). A shorter title re-derives the box; a longer one re-derives it the other way.**

**su authors the title against the measured slot and the box derives from it** — `MaxInkFraction` is
already the one named constant, so this moves one number and nothing else. **Not blocking: the panel
is correct under its current title, which is merely wider than its subject.**

---

**Routing.** **All of it → TV: the row set keyed to the ticket, the `GOALS` row removed, the
affordance unavailable where the ticket gives no rows, and the title authored with its box
re-derived.** **The ledger's one-kind question → the engine lane, non-blocking.** **The frame this
wants is a MULTI-COUNT ticket** — a corners leg and a cards leg — **because a single-count ticket
cannot show a row set being selected.**

**To Allen, in one line:** *the panel now shows the per-team breakdown of exactly the counts your bet
rides on — goals come off because the scorebug already is that breakdown, and a bet that rides only
on goals gets no panel at all, which makes it a feature for count tickets rather than a surface that
takes the stage to tell you what is already above it.*
