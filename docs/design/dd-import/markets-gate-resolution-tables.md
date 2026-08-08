# Markets → Allen · G6's sample-size dial: before/after, and the gate becoming able to fail

**From:** markets/sim lead (`markets-2`) · **2026-08-07**
**Status:** Allen RULED this (option 1), orchestrator co-signed the commit. Built, verified and
committed on `markets-2`. This document is the record of what the ruling bought, not a request.

**Why the DD seat gets a copy:** C32 — *a gate states its resolution* — was promoted from **G3**.
Applying the ruling measured G3 properly for the first time, and **G3 turned out to be the gate that
could not adjudicate its own reading.** The law's origin case is the case it caught. §7 records the
escalation that settled it; §1–§6 are written in the tense of the day and §7 supersedes them where
they differ.

---

## 1. The defect

G6, the martyr guard, is the check that loss-farming never becomes a winning strategy: the
worst-case loss-farmer's win rate must sit within **+2pp** of the skilled bot's. It compares **two
measured rates**, so its resolution is their combined error — and at the `--runs 1000` this seat was
running, that error was **±2.15pp against a 2pp band: 0.9×.**

Its tolerance was narrower than its own noise. It would have reported PASS straight through a real
breach. It passed all session and could not have caught anything.

## 2. The ruling

> **Allen, 2026-08-07 — option 1:** raise `n` to ~4,600 for ±1.0pp resolution, inside the 2pp band,
> so the gate becomes able to fail. Escalation path recorded: **any near-line result re-runs at
> ~18,500.**

Resolution scales as 1/√n, so the two rungs are 1,000 × (2.15/1.00)² ≈ **4,600** and
1,000 × (2.15/0.50)² ≈ **18,500**. They are not arbitrary and the order matters:

| Rung | Resolution | Band ÷ resolution | What it buys |
|---|---|---|---|
| n = 1,000 (was) | ±2.15pp | **0.9×** | nothing — cannot fail for the drift it exists to catch |
| **n = 4,600 (ruled)** | **±0.97pp** measured | **2.1×** | the gate can FAIL: a breach ≥0.97pp past the line reads as a breach |
| **n = 18,500 (escalation)** | ±0.50pp predicted, **±0.48pp measured** | **4.1×** | the gate can ADJUDICATE its whole band — confirmed, §7 |

Allen predicted ±1.0pp at 4,600. **Measured: ±0.97pp.**

## 3. Before / after — every gate, identical seeds

`--gates --seed-prefix TUNE`, before at `--runs 1000`, after at the ruled n (no `--runs`). The
"after" campaign was run **twice** — the first predated two later edits, and the second, against the
committed tree, **reproduced every gate figure identically**. Only wall time moved, which is §5.2.

| Gate | Before (n=1,000) | After (n=4,600) |
|---|---|---|
| G1 honest gambling | **PASS** — median 4, won 0.0% | **PASS** — median 4, won 0.0% |
| G2 engine mandatory | **PASS** — median 5, won 0.0% | **PASS** — median 5, won 0.0% |
| G3 skilled + items wins | **PASS** — median 6, won 5.4% | **PASS** — median 6, won 5.4% · **NOT ADJUDICATED** |
| G4 the EV arc exists | **PASS** — crosses at R3 | **PASS** — crosses at R3 |
| G5 composition superadditive | **PASS** — synergy excess +0.2pp | **PASS** — synergy excess +0.1pp |
| G6 martyr guard | **PASS** — martyr-worst 6.9% vs skilled 5.4% | **PASS** — martyr-worst 6.2% vs skilled 5.4% |
| G7 market coverage | **PASS** — all shipped markets covered | **PASS** — all shipped markets covered |

**No gate flipped.** What changed is what the verdicts are *worth*:

| Gate | Band | Resolution before | Resolution after | Instrument's own verdict |
|---|---|---|---|---|
| G3 | 3pp | ±1.43pp (2.1×) | **±0.67pp (4.5×)** | was "cannot reliably fail" → now **resolves its whole band** |
| G6 | 2pp | ±2.15pp (0.9×) | **±0.97pp (2.1×)** | was "cannot reliably fail" → now **fails on a breach ≥0.97pp past the line** |

### G6's reading, which is the point of the exercise

| | n = 1,000 | n = 4,600 |
|---|---|---|
| martyr-worst | 6.9% | 6.2% |
| skilled | 5.4% | 5.4% |
| **margin vs the +2pp line** | **+1.5pp** (0.5pp clearance) | **+0.7pp** (1.3pp clearance) |
| resolution | ±2.15pp | ±0.97pp |
| verdict | PASS, but decided nothing | **PASS, adjudicated** |

**The margin moved 0.8pp — inside the old ±2.15pp, exactly as the old resolution line warned.** The
"0.5pp of clearance" the n=1,000 campaign appeared to show was never a measurement. Loss-farming is
further from being a winning strategy than this seat could previously demonstrate.

## 4. What was built into the instrument

- **The campaign's `n` is ruled, not chosen.** `--gates` carries a **ruled floor of 10,000** —
  Allen's second call the same day, after the escalation settled. It is the number a bare `--gates`
  always had; what changed is its status, from an unremarked default anyone could undercut in
  silence to a floor that says so when undercut. An explicit `--runs` still wins (that is how the
  escalation is invoked) and going below the floor warns on stderr. G6 resolves ±0.68pp there —
  a 2.9× band, past the ±1.00pp the ruling asked for, and still under the 4× that would let a
  near-line reading adjudicate without escalating.
- **Three tiers on the resolution line**, whose thresholds are Allen's two rungs: **≥4×** resolves
  the whole band · **≥2×** can fail, but not for a reading nearer the line than its own resolution ·
  **<2×** cannot reliably fail.
- **Near-line detection — the half that did the work here.** A band wide enough *in general* says
  nothing about a reading that happens to land *on the line*. Where the criterion edge falls inside
  the reading's own 95% interval, the gate cannot reject "the true value is exactly on the line", so
  it decided nothing whichever way it fell. Those gates print **NOT ADJUDICATED** with the
  escalation command in their own cell, are **named** in the campaign's count line (C28), and
  **drop the report's "ALL GATES PASS" banner**.
- **The exit code is deliberately unchanged.** Allen ruled a re-run, not a failure, so a green 0
  keeps meaning "no gate failed" and never stands in for a verdict nobody reached. One condition in
  `Program.Run` flips it if the studio wants teeth.

## 5. Three corrections this seat owes

1. **"Raise the campaign's *default* `n`" mis-named the defect.** The tool's default was never
   1,000 — a bare `--gates` ran **10,000**, which would have resolved G6 to ±0.68pp. The ±2.15pp
   came from this seat typing `--runs 1000` by hand, all session, with no code path objecting. The
   ruled 4,600 was a 4.6× raise on what was *run* and a ~2× cut on the untouched *default*.
   **Allen closed it the same day: the floor is 10,000** — the value a bare `--gates` always had,
   promoted from unremarked default to ruled floor. The diagnosis had been inherited and repeated
   four times without once being checked against `CliOptions`.
2. **A scaling claim made and falsified within the hour.** This document briefly said "cost does
   not scale linearly — 4.6× the runs cost 6.6× the wall time (121 s → 801 s)". The campaign was
   then run a second time on **identical work** and came in at **625.78 s** — a 28% spread, putting
   the same ratio at 5.2×. The wall clock here cannot resolve a 1.4× effect, so there was no
   scaling finding, only an unreplicated measurement. **Campaign cost measured 10.4 and 13.3 min;
   the 18,500 escalation measured 58.6 min** — above the ~42–54 min this section originally
   predicted, which is §7's first caveat. The single-measurement habit is the defect, not any one
   number it produces.
3. **The first version of this fix had the defect it was fixing.** The new count line said
   "1 NOT ADJUDICATED" while the Resolution column it pointed at named nothing — the tier check
   returned early, so the *weakest* tier, the one where a reading is most likely to be sitting on
   its own line, was the one place the flag could not print. Caught by the first smoke run at
   n=200. Not by a test.

## 6. What the ruling surfaced — two gates, unasked

**G3 — NOT ADJUDICATED. C32's own origin gate.**
Skilled reads **5.4% against a 5.0% floor: 0.43pp of clearance on a ±0.67pp instrument.** Its
*band* is fine — 3pp is 4.5× resolution, it resolves its whole band. It is this *reading* that sits
on the line. The campaign banner therefore reads **7/7 PASS but G3 DID NOT ADJUDICATE**.

Escalation to 18,500 gives ±0.33pp against 0.43pp of clearance — enough, but only just. **If G3
still does not adjudicate there, the honest reading is that the economy is tuned to sit on its own
gate line, and the thing to take to Allen is the band, not the sample size.**

**G5 — the same defect in a sharper form, reported not fixed.**
G5 passes on `synergy excess > 0`: a **threshold at zero**, with no stated resolution, read off a
combination of four measured rates. Its reading went **+0.2pp → +0.1pp → +0.1pp** across n = 1,000,
4,600 and 18,500 — it halved once, then held. The third point is recorded because it *weakens* the
sharpest version of the case: "it moved by as much as its own value" was true of the first step
only. The case that survives needs no movement at all — **a threshold at exactly zero, read off four
measured rates, cleared by +0.1pp.** No error figure is asserted because none has been measured;
measuring it is the next step and not this seat's to take unasked.

## 7. The escalation ran — every gate now adjudicates

Allen fired it 2026-08-07. `--gates --runs 18500 --seed-prefix TUNE`, **2,812,000 total runs,
3514.63 s, exit 0.**

| Gate | Reading | Resolution | Band ÷ resolution | Verdict |
|---|---|---|---|---|
| **G3** | won **5.5%**, 0.5pp above the 5.0 floor | **±0.33pp** | **9.0×** | **PASS — adjudicated** |
| **G6** | martyr-worst **6.0%** vs skilled 5.5%, margin **+0.5pp** | **±0.48pp** | **4.1×** | **PASS — adjudicated** |

`Gates evaluated: 7 · passed: 7 · produced a verdict: 7` — **ALL 7 GATES PASS, the economy holds.**

**G6's full arc, which is what the ruling bought:**

| n | Resolution | Band ÷ res | Martyr margin | What the verdict was worth |
|---|---|---|---|---|
| 1,000 | ±2.15pp | 0.9× | +1.5pp | nothing — could not fail |
| 4,600 | ±0.97pp | 2.1× | +0.7pp | can fail; adjudicated with 1.3pp clearance |
| 18,500 | ±0.48pp | 4.1× | **+0.5pp** | **resolves its whole band** |

**The martyr margin converged +1.5 → +0.7 → +0.5pp.** The n=1,000 reading was **roughly three times**
the settled one — "roughly" doing real work, since both are printed to 1dp and the ratio is only
pinned to about 2.6–3.4×. Loss-farming is nowhere near winning, and this seat could not previously
demonstrate it.

**Predictions scored, not re-fitted** — both were written down before the run: G3 → ±0.33pp
predicted, **±0.33pp measured**; G6 → ~4.1× predicted, **4.1× measured**.

**Two caveats, both this seat's:**

1. **Cost missed a third time.** Predicted ~42–54 min, **actual 58.6 min**. The fix is not a better
   formula — quote measured wall times and stop deriving ranges from two samples.
2. **The escalation report does not carry its own seed line (C34).** It was produced by the binary
   from before that fix landed. The run *was* pinned — `TUNE`, passed explicitly, recorded here —
   but the artifact does not assert it, and under C34 that is the point. It is the last campaign
   artifact with the gap; the header now states the prefix, and `--grid` with it.
