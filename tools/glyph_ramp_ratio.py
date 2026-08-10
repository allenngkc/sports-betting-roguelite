#!/usr/bin/env python3
"""Does the laptop's glyph softness scale with the GLYPH or with the SCREEN?

At 13px a glyph is all edge: ramp width and stroke weight are the same size and no
flat instrument can separate them. Magnify the same glyph and they diverge, and the
two surviving candidates behave in opposite ways:

    intrinsic to glyph rendering  ->  ramp scales WITH the glyph  ->  ratio CONSTANT
    fixed screen-space blur       ->  ramp fixed in screen px     ->  ratio FALLS

So this reports a RATIO across magnifications, never a sharpness.

WHY SUB-PIXEL. The first run counted whole pixels. Ramps measure 2-3 px, so each was
carrying about +/-25% and a three-point trend built on that is not a trend -- C32 on
this instrument's own numbers. Crossings are interpolated linearly between adjacent
samples here, which puts the precision near +/-0.1 px.

WHY A SPAN REFERENCE AND NOT THE RING. The ring was the geometric check on the first
run and it did its job -- it caught two arms that had clipped the frame edge and were
crops rather than magnifications. But it is 1634 px wide at 1.0x in a 2560 frame, so
it cannot itself survive past 1.57x. The reference here is the STEM SPAN inside the
measured band: the distance from the first stem to the last. It scales exactly with
magnification and cannot leave the frame, because it is inside the region already
being measured.

    python tools/glyph_ramp_ratio.py <capture-root>
"""
import sys
import statistics
from pathlib import Path

try:
    from PIL import Image
except ImportError:
    print("Pillow required: pip install pillow")
    sys.exit(2)

# label, magnification, and the crop that holds the same string at that magnification.
ARMS = [("m100", 1.00), ("m125", 1.25), ("m150", 1.50)]

# A ramp is only meaningful on an edge with real contrast behind it.
MIN_CONTRAST = 40


def crossings(line, lo, hi):
    """Sub-pixel x where the profile crosses 10%, 50% and 90%, per rising edge."""
    t10, t50, t90 = (lo + f * (hi - lo) for f in (0.10, 0.50, 0.90))
    out = []
    for i in range(len(line) - 1):
        a, b = line[i], line[i + 1]
        if a < t10 <= b:                      # a rising edge starts here
            x10 = i + (t10 - a) / (b - a)
            x50 = x90 = None
            for j in range(i, len(line) - 1):
                c, d = line[j], line[j + 1]
                if x50 is None and c < t50 <= d:
                    x50 = j + (t50 - c) / (d - c)
                if c < t90 <= d:
                    x90 = j + (t90 - c) / (d - c)
                    break
                if d < c:                     # fell away before reaching 90%
                    break
            if x50 is not None and x90 is not None and x90 > x10:
                # stroke: this 50% crossing to the next FALLING 50% crossing
                fall = None
                for j in range(int(x90), len(line) - 1):
                    c, d = line[j], line[j + 1]
                    if c >= t50 > d:
                        fall = j + (c - t50) / (c - d)
                        break
                if fall is not None and fall > x50:
                    out.append((x90 - x10, fall - x50))
    return out


def measure(path, box):
    im = Image.open(path).convert("RGB")
    c = im.crop(box)
    w, h = c.size
    px = c.load()
    ramps, strokes, first, last = [], [], None, None
    for y in range(h):
        line = [sum(px[x, y][:3]) / 3.0 for x in range(w)]
        lo, hi = min(line), max(line)
        if hi - lo < MIN_CONTRAST:
            continue
        found = crossings(line, lo, hi)
        for ramp, stroke in found:
            ramps.append(ramp)
            strokes.append(stroke)
        if found:
            # span reference: leftmost to rightmost stem on this scanline
            xs = [i for i in range(w - 1)
                  if line[i] < lo + 0.5 * (hi - lo) <= line[i + 1]]
            if len(xs) >= 2:
                first = xs[0] if first is None else min(first, xs[0])
                last = xs[-1] if last is None else max(last, xs[-1])
    span = (last - first) if (first is not None and last is not None) else None
    return ramps, strokes, span


def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(2)
    root = Path(sys.argv[1])

    # The same string at each magnification. Boxes grow with the magnification because
    # the glyphs do; they are centred on the same content.
    boxes = {
        "m100": (1190, 425, 1458, 475),
        "m125": (1120, 400, 1455, 462),
        "m150": (1035, 370, 1450, 445),
    }

    print(f"  {'mag':>6s} {'stems':>6s} {'stroke px':>10s} {'ramp px':>9s} "
          f"{'ratio':>7s} {'span':>7s} {'span scale':>11s}")
    rows, base_span = [], None
    for label, mag in ARMS:
        p = root / f"{label}-a" / "focused-laptop-desk.png"
        if not p.is_file():
            print(f"  {mag:>5.2f}x  frames missing")
            continue
        ramps, strokes, span = measure(p, boxes[label])
        if not ramps:
            print(f"  {mag:>5.2f}x  no usable stems")
            continue
        r, s = statistics.median(ramps), statistics.median(strokes)
        if base_span is None:
            base_span = span
        scale = (span / base_span) if (span and base_span) else float("nan")
        rows.append((mag, r / s, scale))
        print(f"  {mag:>5.2f}x {len(ramps):>6d} {s:>10.3f} {r:>9.3f} "
              f"{r / s:>7.3f} {span if span else 0:>7d} {scale:>10.2f}x")

    if len(rows) < 2:
        print("\n  *** fewer than two valid arms -- nothing to compare (C29) ***")
        sys.exit(1)

    # The geometric reference decides whether the arms are magnifications at all.
    bad = [f"{m:.2f}x (span scaled {sc:.2f}x)" for m, _, sc in rows
           if abs(sc - m) > 0.08 * m]
    if bad:
        print(f"\n  *** VOID: the span did not scale with the magnification on {', '.join(bad)}. "
              f"Those arms are not magnifications of the same content, so no ratio from them "
              f"means anything. This is the check that caught the clipped frames. ***")
        sys.exit(1)

    lo_m, lo_r, _ = rows[0]
    hi_m, hi_r, _ = rows[-1]
    drop = hi_r / lo_r
    predicted_fixed = lo_m / hi_m          # a fixed screen blur falls exactly like this
    print(f"\n  ratio {lo_r:.3f} at {lo_m:.2f}x -> {hi_r:.3f} at {hi_m:.2f}x  ({drop:.3f}x)")
    print(f"  a FIXED screen-space blur predicts {predicted_fixed:.3f}x; "
          f"an INTRINSIC one predicts 1.000x")
    # Call it only if it is nearer one prediction than the other by a clear margin.
    d_fixed, d_intrinsic = abs(drop - predicted_fixed), abs(drop - 1.0)
    if min(d_fixed, d_intrinsic) > 0.5 * abs(1.0 - predicted_fixed):
        print("  -> AMBIGUOUS: sits between the two predictions; a wider magnification "
              "range or a finer measurement is needed before this is called.")
    elif d_fixed < d_intrinsic:
        print("  -> FIXED SCREEN-SPACE BLUR: the ramp does not scale with the glyph. "
              "Sampling point size is NOT the lever.")
    else:
        print("  -> INTRINSIC TO GLYPH RENDERING: the ramp scales with the glyph. "
              "The sampling-size lever is worth testing in-render.")


if __name__ == "__main__":
    main()
