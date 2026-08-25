# Register entries — batch 190 (2026-08-25)

**A NOTICE, not a ruling. TV's first pass suggests a second defect under the one already ruled, and
Allen is deciding the scope call now — so it goes to him flagged rather than after.**

**One row.** **Destination table:** TV (`T168-am2`).

**Basis: TV's preliminary (pending its re-run) plus this seat's own read of which kinds can reach
the fallback at all. Nothing measured here.**

---

## For Allen — the paragraph, in plain terms

TV's first pass suggests that when a club's city is two words, the team-total line on the sweat
screen gets cut back so far that the club's actual name disappears and only the city is left — the
screen would say **SAN FRANCISCO** where everywhere else it says **SPREADSHEETS**. That is backwards
from how the TV has named clubs since we settled it: keep the distinctive word, drop the city. It is
a different fault from the one already ruled — that one makes two different bets read as the same
string; this one misnames the club. **It is not confirmed:** TV found a duplicate row in its own
harness and is re-running with provenance, so treat it as a strong hint, not a result. If it holds,
it can only reach four bet types — the handicap and the three team totals — because those are the
only ones that put a full club name into the line the screen falls back to. The handicap drops out
of that set the moment we build the copy already written for it, **so the fault lands almost
entirely on the three markets you are being asked to scope, and taking them out would remove it
along with the collision.** None of this blocks your decision, and none of it changes the ruling
already made: the collision is live either way.

---

## The row

| T168-am2 | A SECOND club-naming fault, UNCONFIRMED — the truncation may delete the club and leave the CITY, and the exposed set is exactly four kinds | **FLAGGED — DD 2026-08-25 batch 190. NOT RULED, and deliberately: this is TV's PRELIMINARY, taken before its re-run, and `C58-am2`'s conditions are not yet met. **It is recorded now only because Allen's scope call is open TODAY and the ask doc pre-committed that this outcome would be told to him BEFORE he decides** — §4(c), written before the number existed.** **WHAT IS SUGGESTED: on a club whose city is two words, `FitToColumn` truncates past the distinctive word and leaves the CITY — `SAN FRANCISCO` where `T69`'s shipped convention says `SPREADSHEETS`. **This is the ask doc's outcome (c) verbatim**, and it inverts the naming rule rather than colliding two markets, so it is a defect of a different KIND from `T156` and not a worse version of it.** **THE EXPOSED SET IS FOUR KINDS, AND THIS SEAT MEASURED THE CLAIM BEFORE REPEATING TV's: only `Handicap` and the three team totals put a FULL club name into `fields.Line` — `Handicap` as `{hteam} {line:+0.0;-0.0}`, the team totals as `{tname} {ou} {line} {noun}`. **`Moneyline` carries its club in `Subject`, which `NameOf` WOULD fall through to — but Moneyline never reaches the fallback**, having its own `LegStatement` arm through `SweatFlavor.Short`. `AnytimeScorer` likewise. **So "every market that club appears in" — which is what this seat said when it first named outcome (c) — IS AN OVERSTATEMENT, corrected here before it reached Allen.*** **AND THE SET SHRINKS FURTHER ON WORK ALREADY ORDERED: `Handicap` and `PlayerMultiScorer` both have authored, measured copy ready to build (`T169`), and a kind with its own arm no longer reaches `default:`. **After `T169`'s buildable four land, the residual exposed set is EXACTLY the three held team totals** — which is why this sharpens the scope call rather than complicating it: removing them takes the club-naming fault with the collision.** **WHAT WOULD FALSIFY IT: the re-run showing the distinctive word surviving. **`T156` IS UNAFFECTED EITHER WAY** — only the ask doc's outcome (a), the market noun surviving, touches batch 187, and this is not that. A lane reading "batch 187 may need amending" off this preliminary would be reading it wrong.** **THIS ROW IS SUPERSEDED, NOT AMENDED, BY THE REPORT: when the measurement lands with its commit and build state, the ruling replaces this notice. **A flagged preliminary must never be cited as the finding** | batch 190 |

---

## For the orchestrator

- **The paragraph above is the version for Allen**, plain-language per standing practice, flagged
  unconfirmed in its own words.
- **One correction carried into it:** the exposed set is four kinds, not "every market that club
  appears in" — my own phrase when I first named outcome (c), and wrong.
- **And one to the docket:** batch 187 needs amending only under outcome **(a)**. This preliminary
  is (c), which leaves 187 standing.
- **Backlog is 190.**

## Limits

- **This is a notice about a preliminary.** It is not evidence and must not be cited as the finding.
- **The four-kind set is a code read**, not a measurement — it says which kinds CAN reach the
  fallback carrying a full club name, not what any of them renders.
- **I did not verify TV's duplicate-row problem** or what it affected; that is the lane's to report.
