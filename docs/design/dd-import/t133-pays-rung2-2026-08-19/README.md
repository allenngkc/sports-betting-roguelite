# EVIDENCE DOCK — `T133`: the footer's right half, measured and shot

**Shot:** tv-theater lane, 2026-08-19 · **18 frames, three states, six frames each**
**⚠ EVERY FRAME IN THIS SET IS FORCED. Filenames say so; so does this document.**
Frames UNTRACKED; this README commits.

---

## 1. THE MEASUREMENT — `PAID` clears, with more room than the word it replaces

Taken on the real slot at the real face via the pre-authoring instrument
(`SBR/TV/T84 candidate measure`), box **249.0px**:

| string | width | verdict |
|---|---|---|
| `PAYS $73,318,376,502` — **incumbent** | 239.7px | fits, **9.3px** spare |
| `RETURNED $73,318,376,502` — **ruled by `T114-am`/`T121`** | **300.9px** | **OVERRUNS by 51.9px** |
| **`PAID $73,318,376,502` — rung-2 candidate** | **235.8px** | **fits, 13.2px spare** |
| `PAID $199` — at `T114-am`'s own authored amount | 108.8px | fits, 140.3px spare |

**`PAID` is four characters, the same as `PAYS` — but its glyphs are narrower, so at the enumerated
worst case it has MORE headroom than the incumbent (13.2px against 9.3px).**

**The width case against `PAID` is closed.** What is not closed is batch 108's own objection: `PAID`
was rejected there for **colliding at the root with `PAY $60` on the same screen.** Nothing measured
here touches that, and **whether a root collision is worth 51.9px is a copy call this lane does not
hold.**

### Why the worst case is the right number and not a pessimism

`$73,318,376,502` is not invented. It is the **enumerated** maximum — eleven digits, established over
**648,000 priced offers** by `PayoutMaximumTests`, and already carried in the sweep's pool for that
reason. `T133`'s whole finding is that `T121` put an eight-character word onto **the one slot whose
worst case was already established by enumeration**, and priced nothing.

---

## 2. THE FRAMES — `S99`-style forcing, and what forced means here

**`S3`'s precedent:** it reached an otherwise-unreachable empty group with a **non-shipped**
`CorrectScoreFloor = 0.08` and put the disclosure on the frame's face. **Same device, same reason.**

**The worst-case amount cannot be dealt for.** An eleven-digit return needs a bank and a parlay term
no capture can arrange, so the slot's text is forced directly and **every frame is named
`FORCED-t133-…`.** A forced frame that does not disclose its forcing is evidence for a state the
product does not have.

| moment | slot renders |
|---|---|
| `FORCED-t133-incumbent-PAYS` | `PAYS $73,318,376,502` |
| `FORCED-t133-ruled-RETURNED` | `RETURNED $73,318,376,502` |
| `FORCED-t133-rung2-PAID` | `PAID $73,318,376,502` |

**Three states rather than one, deliberately.** The ask was one frame forcing rung 2; shooting the
candidate alone would show a string that looks fine **with nothing to look fine against**. The
incumbent is the control and the ruled form is the problem, so all three sit on one ruler at the
same face, same box, same acceptance view.

**Why a frame at all when the widths are settled:** they answer different questions. **The px numbers
say `PAID` FITS. No px number says whether it READS**, and `C11` puts a copy decision on a frame.

---

## 3. WHAT THIS SET DOES NOT CLAIM

- **Nothing about whether `PAID` should be adopted.** Batch 108's root-collision rejection stands
  untouched. This set closes the WIDTH question and only that.
- **Nothing about the footer's LEFT half.** `STAKE` was measured earlier and fits with 90.1px spare;
  it is not in question here.
- **These are not shipped states.** The forced literal latches nothing — the next repaint overwrites
  it — and the amount is unreachable in play.
- **The dead case was never at risk.** `RETURNED $0` measures 146.5px with 102.5px spare. **`T133`'s
  exposure is the CASHED-OUT case alone**, whose return is not bounded to zero.
- **No claim about the seated read at four metres.** These are capture-camera frames at the
  acceptance view; whether the word reads there is the judgement this set exists to enable, not one
  it makes.
