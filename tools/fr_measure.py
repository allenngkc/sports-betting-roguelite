#!/usr/bin/env python3
"""
Flood-removal verdict measurements, and the C34 guard that makes them a set.

Promoted from the capture-window scratch script that produced the 2026-08-10
flood-removal submission (§0-FR of docs/handoffs/tv-sweat.md). The measurements
below are unchanged from that run and reproduce its published numbers exactly;
everything added here is selection and assertion.

THE THREE MEASUREMENTS (unchanged)
    1. ink vs its own ground, linear relative luminance (C33-am3)
    2. whether that ground is static across the beat (C35/V8)
    3. THE L4 PUNCH-THEN-SETTLE (tv-design 6.1). Batch 27 found the flood
       REDUNDANT with the punch rather than carrying it, so "the punch left with
       the flood" is the live regression risk -- and it degrades quietly while
       every contrast number in 1 and 2 still lands. Contrast reads cannot see it.

WHY THE SELECTION IS THE INTERESTING PART -- C34, "evidence that cannot be
reproduced is not a set"

    A capture directory ACCUMULATES RUNS, and the moment name does not separate
    them. The previous capture's accept frames carry a different scene-grammar
    token, so they do not overwrite the new ones: both sets sit in the same
    directory under the same `moment-` name, and a naive glob collects both. The
    first pass over the 2026-08-10 frames reported ACCEPT as "60 frames" with CR
    alternating 8.47 / 1.70 frame by frame. It was measuring TWO RUNS AT ONCE,
    and it would have reported the pre-removal defect as still present in the
    window whose entire purpose was to show it gone.

    The scratch fix -- mtime-scope, then keep the newest file per frame index --
    was correct on that window and carries two defects that this promotion fixes:

    (a) THE WINDOW ROLLED. `CUTOFF = time.time() - 45*60` makes the selection a
        function of WHEN THE SCRIPT RUNS. The same script over the same disk
        answers differently tomorrow, which is precisely the thing C34 forbids.
        Here the window is ANCHORED: either you pass --since/--until, or the run
        resolves the newest mtime cluster and PRINTS the bounds it chose as a
        pasteable --since/--until line. Selection becomes replayable after the
        fact instead of dependent on the clock.

    (b) NEWEST-PER-INDEX BACKFILLS SILENTLY. It only separates runs when the
        older run is outside the window. Two runs INSIDE one window, with the
        newer one short, resolves indices past the new run's end from the OLD
        run -- and the frame count still reads full, so the output looks correct.
        On the 2026-08-10 window this never fired (the runs were 94 minutes
        apart, so mtime alone separated them and newest-per-index was never
        load-bearing) but nothing in the script said so.

    So the guard is not the mtime window. The guard is the PIN, asserted:

        C34.1  "Every capture flow pins its seed and asserts the run is carrying
                it before shooting. AN UNASSERTED PIN IS A COMMENT."

    The seed, boost, scene and grammar token are all in the filenames. A
    measurement pass can therefore assert the pin instead of trusting it: every
    selected frame must carry ONE seed, ONE grammar token, ONE scene and ONE
    boost, and the indices must be contiguous 0..N-1 at the expected count. Two
    runs cannot survive that check regardless of their mtimes, which is the
    failure mode the window was standing in for.

    C34.3 asks a submission to state whether the flow was pinned "in the same
    breath as the comparison", so the pin is printed with the numbers rather
    than being a thing you go and look up.

USAGE
    python tools/fr_measure.py
    python tools/fr_measure.py --expect 30
    python tools/fr_measure.py --since "2026-08-09 23:50" --until "2026-08-10 00:05"
    python tools/fr_measure.py --dir path/to/captures --moment t68am-accept-slot=ACCEPT

    Exits non-zero if any moment fails its selection assertions, so this is safe
    to put in front of a submission.

CANVAS -> FRAME
    scale x 2176/980, y 1223/550; origin (185.0, 113.7). The slot rect is the
    cash-out slot in canvas coordinates, (6,486)-(259,526), transformed once.
"""
import argparse
import datetime as dt
import glob
import math
import os
import re
import sys

import numpy as np
from PIL import Image

HERE = os.path.dirname(os.path.abspath(__file__))
DEFAULT_DIR = os.path.normpath(
    os.path.join(HERE, "..", "unity", "SBR", "artifacts", "tv-sweat-capture")
)
DEFAULT_MOMENTS = [("t68am-accept-slot", "ACCEPT"), ("t71-win-tally-slot", "WIN TALLY")]
DEFAULT_EXPECT = 30
DEFAULT_GAP = 300.0      # seconds of quiet that separate one capture run from the next
CAPTURE_STEP = 50.0      # frames per second of beat -- captures are 1/50s apart

# canvas -> frame, and the cash-out slot inside it
SX, SY, OX, OY = 2176 / 980, 1223 / 550, 185.0, 113.7
fx = lambda x: int(round(OX + x * SX))
fy = lambda y: int(round(OY + y * SY))
SLOT = (fx(6), fy(486), fx(259), fy(526))

NAME_RE = re.compile(
    r"seed-(?P<seed>\d+)__boost(?P<boost>[\d.]+)__scene(?P<scene>\d+)__"
    r"grammar-(?P<grammar>[A-Za-z]+)__moment-(?P<moment>.+?)__frame(?P<frame>\d+)\.png$"
)


def lin(v):
    return np.where(v <= 0.04045, v / 12.92, ((v + 0.055) / 1.055) ** 2.4)


def rel(a):
    return 0.2126 * lin(a[..., 0]) + 0.7152 * lin(a[..., 1]) + 0.0722 * lin(a[..., 2])


def cr(a, b):
    hi, lo = max(a, b), min(a, b)
    return (hi + 0.05) / (lo + 0.05)


def parse_name(path):
    m = NAME_RE.search(os.path.basename(path))
    return m.groupdict() if m else None


def stamp(ts):
    return dt.datetime.fromtimestamp(ts).strftime("%Y-%m-%d %H:%M:%S")


def parse_ts(s):
    return dt.datetime.fromisoformat(s).timestamp()


def newest_cluster(records, gap):
    """Split mtime-sorted records into runs on `gap` seconds of quiet; return the last."""
    runs = []
    for r in sorted(records, key=lambda r: r["mtime"]):
        if not runs or r["mtime"] - runs[-1][-1]["mtime"] > gap:
            runs.append([])
        runs[-1].append(r)
    return (runs[-1] if runs else []), len(runs)


def select(directory, moment, since, until, gap, expect):
    """Return (frames, pin, problems). Frames are mtime-anchored and pin-asserted."""
    problems = []
    records = []
    for p in glob.glob(os.path.join(directory, f"*moment-{moment}__frame*.png")):
        meta = parse_name(p)
        if meta is None:
            problems.append(f"unparseable filename, cannot verify its pin: {os.path.basename(p)}")
            continue
        meta.update(path=p, mtime=os.path.getmtime(p), frame=int(meta["frame"]))
        records.append(meta)

    if not records:
        return [], None, [f"no frames on disk matching moment-{moment}"]

    total_on_disk = len(records)
    n_runs = None
    if since is not None or until is not None:
        lo = since if since is not None else float("-inf")
        hi = until if until is not None else float("inf")
        chosen = [r for r in records if lo <= r["mtime"] <= hi]
        origin = "explicit --since/--until"
    else:
        chosen, n_runs = newest_cluster(records, gap)
        origin = f"newest of {n_runs} mtime cluster(s), gap {gap:g}s"

    if not chosen:
        return [], None, [f"window selected 0 of {total_on_disk} frames on disk ({origin})"]

    # --- C34.1: assert the pin. One seed, one grammar, one scene, one boost.
    pin = {}
    for key in ("seed", "boost", "scene", "grammar"):
        values = sorted({r[key] for r in chosen})
        pin[key] = values[0] if len(values) == 1 else values
        if len(values) > 1:
            problems.append(
                f"PIN BROKEN -- {len(values)} distinct {key} values in the selected set: "
                f"{values}. This is two runs measured as one; the moment name does not "
                f"separate them."
            )

    # --- indices: contiguous 0..expect-1, each appearing exactly once
    by_index = {}
    for r in chosen:
        by_index.setdefault(r["frame"], []).append(r)
    dupes = sorted(k for k, v in by_index.items() if len(v) > 1)
    if dupes:
        shown = ", ".join(str(k) for k in dupes[:8]) + (" ..." if len(dupes) > 8 else "")
        problems.append(
            f"{len(dupes)} frame index/indices appear more than once in the window: {shown}"
        )

    indices = sorted(by_index)
    missing = [n for n in range(expect) if n not in by_index]
    extra = [n for n in indices if n >= expect]
    if len(indices) != expect:
        problems.append(
            f"EXPECTED {expect} indices, selected {len(indices)}"
            + (f"; missing {missing}" if missing else "")
            + (f"; unexpected {extra}" if extra else "")
        )
    elif missing or extra:
        problems.append(f"index range not contiguous 0..{expect-1}: missing {missing}, extra {extra}")

    frames = [by_index[n][0] for n in indices]
    # bounds over the whole selection -- `chosen` is in glob order, not mtime order
    pin["window"] = (min(r["mtime"] for r in chosen), max(r["mtime"] for r in chosen))
    pin["origin"] = origin
    pin["on_disk"] = total_on_disk
    pin["selected"] = len(chosen)
    return frames, pin, problems


def measure(frames):
    grounds, inks, crs = [], [], []
    for r in frames:
        a = np.asarray(Image.open(r["path"]).convert("RGB"), dtype=np.float32) / 255.0
        L = rel(a[SLOT[1]:SLOT[3], SLOT[0]:SLOT[2]])
        g = float(np.mean(L[L >= np.percentile(L, 85)]))
        i = float(np.mean(L[L <= np.percentile(L, 2)]))
        grounds.append(g)
        inks.append(i)
        crs.append(cr(g, i))
    return grounds, inks, crs


def main(argv=None):
    ap = argparse.ArgumentParser(description=__doc__.split("\n")[1],
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--dir", default=DEFAULT_DIR, help="capture directory")
    ap.add_argument("--expect", type=int, default=DEFAULT_EXPECT,
                    help=f"frames required per moment, indices 0..N-1 (default {DEFAULT_EXPECT})")
    ap.add_argument("--since", help='window start, e.g. "2026-08-09 23:50" (local)')
    ap.add_argument("--until", help='window end, e.g. "2026-08-10 00:05" (local)')
    ap.add_argument("--gap", type=float, default=DEFAULT_GAP,
                    help=f"seconds of quiet separating runs when auto-selecting (default {DEFAULT_GAP:g})")
    ap.add_argument("--moment", action="append", metavar="TOKEN=LABEL",
                    help="override the moments measured; repeatable")
    args = ap.parse_args(argv)

    moments = DEFAULT_MOMENTS
    if args.moment:
        moments = []
        for spec in args.moment:
            token, _, label = spec.partition("=")
            moments.append((token, label or token))

    since = parse_ts(args.since) if args.since else None
    until = parse_ts(args.until) if args.until else None

    print(f"capture dir : {args.dir}")
    print(f"expecting   : {args.expect} frames per moment, indices 0..{args.expect - 1}")
    failed = []

    for moment, label in moments:
        frames, pin, problems = select(args.dir, moment, since, until, args.gap, args.expect)
        print(f"\n===== {label}   ({moment})")

        if pin:
            w0, w1 = pin["window"]
            # The replay bounds are widened to whole seconds -- floor the start, ceil the
            # end -- so that pasting them back selects the SAME set. Printing truncated
            # stamps verbatim silently drops any frame in the final fractional second.
            # Widening cannot reach a neighbouring run: runs are separated by --gap (300s).
            print(f"  selection : {pin['selected']} of {pin['on_disk']} on disk -- {pin['origin']}")
            print(f"  window    : {stamp(w0)}  ->  {stamp(w1)}")
            print(f"  replay    : --since \"{stamp(math.floor(w0))}\" "
                  f"--until \"{stamp(math.ceil(w1))}\"")
            print(f"  PIN (C34) : seed {pin['seed']}  boost {pin['boost']}  "
                  f"scene {pin['scene']}  grammar {pin['grammar']}")

        if problems:
            for p in problems:
                print(f"  !! {p}")
            print(f"  RESULT    : SELECTION FAILED -- not a set under C34, no numbers reported.")
            failed.append(label)
            continue

        print(f"  PIN ASSERTED: one seed, one grammar, one scene, one boost across all "
              f"{len(frames)} frames.")

        grounds, inks, crs = measure(frames)
        print(f"\n  --- {len(frames)} frames, 1/{CAPTURE_STEP:g}s apart "
              f"({len(frames) / CAPTURE_STEP:.2f}s of beat)")
        print(f"  INK       min {min(inks):.4f}   max {max(inks):.4f}   "
              f"spread {max(inks) - min(inks):.4f}")
        print(f"  CR        min {min(crs):.2f} : 1    max {max(crs):.2f} : 1")

        if len(grounds) < 12:
            print("  PUNCH     : not read -- needs at least 12 frames for the L4 window.")
            continue

        hi = max(grounds[:12])
        lo = min(grounds[-6:])
        step = hi - lo
        at = next((n for n in range(1, len(grounds)) if grounds[n - 1] - grounds[n] > 0.03), None)
        print(f"  PUNCH -> SETTLE   L4 {hi:.4f}  ->  L3 {lo:.4f}   step {step:.4f} "
              f"({100 * step / hi:.1f}%)   at frame {at}")
        if step < 0.05:
            print("  *** REGRESSION: no punch-then-settle. The brief L4 punch that section 6.1 "
                  "requires is ABSENT -- the failure batch 27 predicted, and every contrast "
                  "number above still lands.")
            failed.append(f"{label} (punch regression)")
        else:
            print(f"  section 6.1 punch INTACT: one step down at frame {at}, held after. The "
                  f"flood was redundant with the punch, and removing it did not take it.")

    if failed:
        print(f"\nFAILED: {', '.join(failed)}")
        return 1
    print("\nAll moments selected cleanly and measured.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
