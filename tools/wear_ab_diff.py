#!/usr/bin/env python3
"""R7/R8 -- does the wear read? Per-pixel diff of a wear-on / wear-off capture pair.

WHY A PAIR AND NOT A STATISTIC. A wear decal is small and sits against busy
geometry, so a box drawn around it measures the pipe fitting or the window sill
next to it. Tried on this room: two of four wear sites scored above the ceiling
benchmark while brightened crops showed no drip, no damp boundary, no rust and no
scuff at any of them. A per-pixel diff of two otherwise identical renders is
unambiguous -- whatever differs IS the wear, because nothing else changed.

R7's parking verdict came from this measurement: 1.92% of pixels changed against
a 1.69% baseline, "very nearly invisible". Those numbers are the bar. Reporting a
new percentage without them would be a number without its comparison.

Usage:
    python tools/wear_ab_diff.py <capture-root>        # expects wear-on/ and wear-off/
    python tools/wear_ab_diff.py <on-dir> <off-dir>

Reports, per pose: % of pixels changed at all, % changed by more than a
just-noticeable amount, and the mean magnitude on those. The middle number is the
one to argue about; the first counts dithering.
"""
import sys
from pathlib import Path

try:
    from PIL import Image, ImageChops
except ImportError:
    print("Pillow required: pip install pillow")
    sys.exit(2)

POSES = ["standing-overview.png", "seated-tv-couch.png", "focused-laptop-desk.png"]

# R7's own baseline, so a new figure is never reported alone.
R7_CHANGED_PCT = 1.92
R7_BASELINE_PCT = 1.69
JND = 2          # 8-bit levels; below this is dithering, not a visible change


def diff(on_path, off_path):
    a = Image.open(on_path).convert("RGB")
    b = Image.open(off_path).convert("RGB")
    if a.size != b.size:
        raise ValueError(f"size mismatch: {a.size} vs {b.size}")
    d = ImageChops.difference(a, b).convert("L")
    hist = d.histogram()
    total = sum(hist)
    any_change = total - hist[0]
    over = sum(hist[JND + 1:])
    mag = (sum(i * hist[i] for i in range(JND + 1, 256)) / over) if over else 0.0
    return total, any_change, over, mag, d.getbbox()


def main():
    args = sys.argv[1:]
    if not args:
        print(__doc__)
        sys.exit(2)
    if len(args) == 1:
        on, off = Path(args[0]) / "wear-on", Path(args[0]) / "wear-off"
    else:
        on, off = Path(args[0]), Path(args[1])

    missing = [str(p) for p in (on, off) if not p.is_dir()]
    if missing:
        print(f"FAILED: missing capture directory: {', '.join(missing)}")
        sys.exit(1)

    # The R7 figures are the WEAR baseline. Printing them beside an emission A/B
    # would invite a comparison between two unrelated quantities -- the shape of
    # mistake this file exists to prevent -- so they appear only for the wear pair,
    # which is the single-root invocation.
    if len(args) == 1:
        print(f"R7's parked verdict for comparison: {R7_CHANGED_PCT}% changed against a "
              f"{R7_BASELINE_PCT}% baseline -- 'very nearly invisible'\n")
    else:
        print(f"Comparing {on.name} vs {off.name}. No baseline is quoted: R7's wear figures are a "
              f"different quantity and would be a false comparison here.\n")
    print(f"{'pose':26s} {'any change':>11s} {'>JND':>8s} {'mean mag':>9s}  changed region")
    seen_any = False
    for pose in POSES:
        pa, pb = on / pose, off / pose
        if not (pa.is_file() and pb.is_file()):
            print(f"{pose:26s} (missing from one or both halves)")
            continue
        total, any_change, over, mag, bbox = diff(pa, pb)
        seen_any = True
        print(f"{pose:26s} {100.0 * any_change / total:10.2f}% "
              f"{100.0 * over / total:7.2f}% {mag:9.1f}  {bbox if bbox else 'IDENTICAL'}")

    # C29's shape: a comparison that compared nothing is a failure, not a pass.
    if not seen_any:
        print("\nFAILED: no pose was present in both halves -- this run compared nothing.")
        sys.exit(1)
    print("\nThe '>JND' column is the honest one; 'any change' counts single-level dithering.")
    if len(args) == 1:
        print("A figure at or under R7's 1.92% means the re-place has not moved the needle and the\n"
              "inventory still does not read -- a placement or scale finding, not a technique one,\n"
              "until a frame shows otherwise.")


if __name__ == "__main__":
    main()
