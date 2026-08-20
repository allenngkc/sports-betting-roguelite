# Register entries — batch 129 (2026-08-19)

**`T133`'s MEASUREMENT LANDED AND THE REAL FINDING IS THE OTHER LINE OF IT: `PAYS` AT ITS OWN
ENUMERATED MAXIMUM HAS 9.3px OF SPARE.** The `Pays` slot was already sized to within 3.7% of its
worst case, so it cannot carry **any** second word — `RETURNED` is not too long, the box is full.

**Two rows.** **Destination table:** TV (`T141`, `T133-am`).

TV measured it at build time, recorded it at the site, refused to shorten the word because copy is
the DD's and `C11` puts copy on a frame, and routed it. **That is the behaviour, and the refusal was
correct.**

---

## The measurement, as TV recorded it

```
PAYS $73,318,376,502      239.7px  against box 249.0px  fits,   9.3px spare
RETURNED $73,318,376,502  300.9px  against box 249.0px  OVERRUNS by 51.9px
RETURNED $0               146.5px  against box 249.0px  fits, 102.5px spare
```

---

## The rows

| T141 | The `Pays` box has 9.3px of spare at its own enumerated maximum — it can never carry a second word | **RULED — standing constraint, and it is the durable half of `T133`** · DD 2026-08-19 batch 129, on tv-theater's build-time measurement. **`PAYS $73,318,376,502` measures 239.7px in a 249.0px box: NINE POINT THREE PIXELS OF SPARE, 3.7% of the box, on the slot whose worst case was established by enumeration over 648,000 priced offers (`T74-am5`, `PayoutMaximumTests`).** **THE CONCLUSION NOBODY HAD DRAWN: `RETURNED` is not too long — THE BOX IS FULL.** Any word of five characters or more breaks it, and a four-character word clears only if it is no wider than `PAYS` plus 9.3px. **`T114-am` and `T121` each authored a word into this slot and neither priced it, because from the register the slot looks like a place words go.** **AND THE CASHED-OUT BOUND NEEDS NO NEW ENUMERATION: `_lastCashOutAmount` is the ticket's expected terminal credit, which is at most its `PotentialPayout`, so the cashed-out worst case IS the `PAYS` worst case and `T74-am5`'s enumeration transfers unchanged.** **RECORDED AS A CONSTRAINT RATHER THAN A DEFECT because nothing is currently wrong with `PAYS` — it fits.** What is wrong is that the headroom was never written down, so two rulings spent it without knowing it existed. **`S94-cl`'s shape one surface over: *the protection I wrote does not exist; naming that it does not is worth more than pretending a fold restores it.*** **BINDS: any future copy ruling on this slot states its width against 249.0px BEFORE it is authored, and `T133`'s own instruction — measure at the ENUMERATED maximum, never the seed's — is what makes that check meaningful** | batch 129 |
| T133-am | The remedy — `G1`'s ladder as pre-committed; `PAID` is the CANDIDATE and is NOT ruled as the string | **RULED — DD 2026-08-19 batch 129, discharging `T133`'s flag on the measurement it asked for.** **`T133` pre-committed the remedy path and it holds unchanged: `T69` bars truncation, the box does not widen for a word, and the answer is `G1`'S TWO-RUNG LADDER CHOSEN BY MEASUREMENT (`FitOrFallback`) — the mechanism the moneyline and scorer arms already use.** **RUNG 1: `RETURNED $n`, unchanged. RUNG 2: an authored shorter form, reached only where rung 1 does not fit.** **THE CANDIDATE IS `PAID`, AND IT IS DERIVED RATHER THAN PICKED: it is the PAST TENSE OF THE WORD THE SLOT ALREADY CARRIES.** The slot says what the ticket `PAYS` while it rides and what it `PAID` once it is closed — **the state change IS the tense change**, it coins no new vocabulary, and at four characters it is `PAYS`'s own width class. **NOT RULED AS THE STRING, AND `T112`'s SENTENCE APPLIES VERBATIM: dropping four of eight characters will VERY PROBABLY clear 51.9px, and *very probably* is the phrase this studio has been corrected on repeatedly. `PAID` clears IF AND ONLY IF IT IS NO MORE THAN 9.3px WIDER THAN `PAYS` (`T141`) — `I` is narrow, `D` is wide, `Y` and `S` are mid, and that is a MEASUREMENT, never something readable off a character count (batch 95).** **IF IT DOES NOT CLEAR, the box cannot hold any four-character word beside the maximum figure, and the question moves to the FIGURE — a different ruling, and it goes to Allen rather than being solved here.** **WHY THE LADDER RATHER THAN RE-AUTHORING `RETURNED` OUTRIGHT, since the simpler move was considered and refused: `S38` borrowed `STAKE`/`RETURNED` as a PAIR, and replacing it wholesale would make the TV and the laptop print different words for one fact — `K8`'s defect, which this seat ruled two batches ago. THE LADDER KEEPS THE SURFACES IN STEP FOR EVERY TICKET ANYONE WILL EVER SEE and degrades only at a figure no player reaches.** **`C11`'s FRAME, SIZED SO IT IS CHEAP: the read is NOT *"does an $73bn cash-out look right"* — it is *"do the two rungs read as one slot."* Force rung 2 on an ORDINARY cash-out and shoot the pair, the way `S99` kept the `PriceTakesAmber` switch to force an arm. One frame, no absurd ticket** | batch 129 · `T141` |

---

## What is NOT in this batch

- **No string is authored.** `PAID` is named as a candidate with a stated pass condition, exactly as
  `T112` named `SUSPENDED` and `T112-cl` closed it on the number.
- **`S38` is not reopened.** Rung 1 stays `RETURNED` and the laptop is untouched.
- **The dead case is not changed** — `RETURNED $0` fits with 102.5px spare and never reaches rung 2.
- **No claim that `PAID` reads better than `RETURNED`.** It is the fallback rung; `C11`'s frame asks
  only whether the two rungs read as one slot.
