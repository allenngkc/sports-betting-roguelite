# Wall-clock time audit — last 3 days (Aug 12 evening → Aug 15 early)

Speed-brief item 3. Sources: 301 main commits since Aug 12 noon, STATUS.md cycle
stamps, dispatch records. Estimates, not accounting.

## The shape of the clock

The studio runs in evening/night sessions (~19:00 → ~02:00), roughly 6–7 active
hours per day. Two overnight halts of ~18–19 hours each (Aug 13 01:00→19:16,
Aug 14 01:53→20:57). Of ~72 hours of calendar, **~20–22 were active studio time;
~36–38 were overnight halt.**

## Where the active hours went (parallel lanes overlap; foreground ≠ sum)

| Bucket | Est. wall | Notes |
|---|---|---|
| Lead build/implementation | ~12–14 h (parallel across 2–3 lanes) | The productive core: TV strip/hold/captures, sgp model+cash-out, laptop pin/void/stamp/sentences |
| Sim/gate campaigns | ~11–12 h (mostly detached, overlapped) | Draws A+B ~3 h · control+first holdout ~2.5 h · merged-main holdout ~1.3 h · the killed slow campaign ~4–5 h |
| Unity editor lease | ~6–7 h, strictly serialized | Suites, captures, verification boots. The single editor queued sgp's screen phases behind the laptop lane twice |
| DD verdict turnaround | ~3–4 h active reading | Batches 64–72; docket→landed usually <1 h. Pre-written verdicts kept lanes unblocked — rarely a bottleneck |
| Incident recovery | ~1.5–2 h | Two Orca/Claude restarts (seat rebuilds, monitor re-arms), one seat rotation, one API-error turn |
| Idle on Allen | Low direct cost | Research parked ~3 days (its own lane, nothing queued behind it); flow-cost call open ~2 h before ruled |

## The two findings that matter

1. **The biggest bucket is the overnight halt — half the calendar.** And it
   compounds: the merged-main holdout campaign finished 23:55 Aug 13 and sat
   unread ~19 hours until the next session's sweep found it. Work that finishes
   at night waits for the studio to wake. If overnight campaigns ran on a
   schedule and results were swept at session start (or a scheduled task did
   it), the studio would start each evening with tables already on the desk.

2. **Campaigns are the second bucket and the parallelization (item 2) attacks
   exactly that.** ~85 min serial today; across (cores − 2) workers with
   deterministic seed-splitting, a campaign should drop to a fraction — and the
   killed slow campaign shows the cash-out path also needs its profile fix
   before any re-run.

The editor lease (~6–7 h serialized) is third; it is structural (one editor)
but scheduling screen-phase work outside TV's capture windows kept it mostly
absorbed.
