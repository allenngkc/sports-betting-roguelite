# Measurement ask — the team-total NEED fallback (DD, 2026-08-25)

**For TV, behind the part-C gate. Not urgent: `T156` is already ruled LIVE without this number
(batch 187), and nothing here can un-rule it. What this measurement decides is whether there is a
SECOND defect underneath it, and what the copy has to survive.**

This is a pre-commitment as well as an ask. §4 states what each possible result rules **before the
number exists**, including the result that would prove batch 187 wrong. That is the point of
writing it down first.

---

## 1. The case to measure

The **live leg row's NEED span** (`LegRowNeed0`, the 261.0px box) for a team total, which reaches
the fallback because `DescribeActiveLeg`'s `default:` arm passes an EMPTY NEED fallback:

```
DescribeActiveLeg  default:  -> new ActiveLegCopy(LegStatement(leg), string.Empty, ...)
LegStatement       default:  -> SheetName(leg) ?? leg.DisplayLabel
SheetName                    -> MarketSheet.Build(...).AllRows -> row.Name.ToUpperInvariant()
MarketSheet.NameOf           -> fields.Line   (non-empty for these kinds)
MatchModel team-total arm    -> "{tname} {ou} {line:0.0} {noun}"   <- FULL club name
FitOrFallback(t, primary, "")-> fallback is empty, so: FitToColumn(t, primary)
FitToColumn                  -> drops whole words at LastIndexOf(' '), i.e. FROM THE END
```

**Four selections, not one** — `C46`, and the ladder genuinely differs between them:

| # | kind | line | club | why this one |
|---|---|---|---|---|
| 1 | `TeamTotalGoals` | 1.5 | one-word city (e.g. Duluth Auditors) | the common case |
| 2 | `TeamTotalCards` | 1.5 | **the same club as #1** | this pair IS `T156` — the strings must be compared directly |
| 3 | `TeamTotalGoals` | 1.5 | **two-word city** — `San Francisco` or `Moose Jaw` | the long ladder; those are `SlateGenerator`'s ONLY two-word cities, so the case is rare but reachable |
| 4 | `TeamTotalCorners` | 4.5 | any | ruled NOT to collide (unshared line) — measured to confirm, cheap |

## 2. What to report

**The SURVIVING STRING, character for character — not the width alone.** A width cannot
distinguish a clean `AUDITORS UNDER 1.5` from a city-only survivor; both are "a number under 261.0".

For each of the four: the input string, its measured width, the string `FitToColumn` returns, and
that string's width.

**Flag explicitly if the DISTINCTIVE WORD does not survive.** `FitToColumn` cuts from the end, so
on a full-name club the ladder runs `SAN FRANCISCO SPREADSHEETS UNDER 1.5 GOALS` → … →
`SAN FRANCISCO SPREADSHEETS` → **`SAN FRANCISCO`** → `SAN`. That is the inverse of `T69`'s shipped
convention (keep the distinctive word, drop the city) and would be a worse defect than `T156`.

**Flag explicitly if a single over-wide word is returned whole** — `if (cut <= 0) return cur`. That
is `T46`'s containment backstop being reached by shipped copy, which §8 says should not happen.

## 3. Conditions (`C58-am2`)

State both, or the number cannot close anything:

- **The commit** it was measured at.
- **Whether `T168-am` was built** — it moves the club token from the full name to
  `SweatFlavor.Short` at the render, so the same row measures differently before and after. The
  ask names this because the lane cannot be expected to track a ruled-but-unbuilt change; that is
  the routing seat's job and it was missed the first time this went out.

## 4. Pre-committed reading — what each result rules

**(a) The noun survives** (e.g. `AUDITORS UNDER 1.5 GOALS` fits 261.0). Then goals and cards are
distinguishable and **`T156` is NOT live in the shipped build — batch 187 is WRONG and I retract
it.** Recorded as the falsification condition: batch 187 argues the short club form already
measures 449.5 against 261.0 with the noun attached, so this result should be impossible. If it
happens, the 449.5 figure or my reading of it is the error, not the build.

**(b) The noun drops, the distinctive word survives** (`AUDITORS UNDER 1.5`, or shorter but still
club-led). **`T156` live exactly as ruled, no new defect.** The scope call proceeds on batch 187's
terms and this measurement changes nothing but the copy's target.

**(c) The distinctive word does NOT survive** (`SAN FRANCISCO`, or `SAN`). **A second defect,
independent of `T156` and worse:** it inverts `T69`, and the survivor collides across *every*
market that club appears in, not just goals vs cards at 1.5. This escalates — I rule it as its own
row and Allen's scope call should be told before he decides.

**(d) A single over-wide word returned whole.** Mid-word clipping by the element. Its own defect,
ruled separately; `T46`'s backstop is a structural guard, not a copy strategy.

**In (b), (c) and (d) alike, `T156` stays live.** No result here reopens it — the collision is
proven at every truncation depth at or past the noun, and (a) is the only escape.

## 5. What this does NOT ask for

- **No copy.** The three team totals are held by Allen (`T152-am`); their forms may not be
  authored, and this measurement is not a route around that hold.
- **No fix.** If a repair is wanted the site is the RENDER, per `T168-am` — never
  `MatchModel.Fields`, which feeds `MarketSheet`, the one composer the TV, laptop and console all
  print through (`S96`).
- **No capture window.** These are extents, not frames.
