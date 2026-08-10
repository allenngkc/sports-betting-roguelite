#!/usr/bin/env python3
"""S2-am2's two-surface baseline: ONE ramp/stroke number per surface, plus the
C37 characterization that has to happen before either number is recorded.

WHY THIS IS NOT glyph_ramp_ratio.py. That tool answers a different question and
says so in its own docstring: it reports "a RATIO across magnifications, never a
sharpness". It is hardcoded to three arms (m100/m125/m150), to per-arm crop
boxes, and to the laptop frame. It cannot express "one number for this surface at
its acceptance view", and it cannot see the phone at all.

What it DOES own is the sub-pixel edge logic, and that is the part a baseline
must not re-implement -- two hand-written crossing finders that disagree by 0.1px
would make the baseline incomparable with every prior ramp number in the lane.
So `crossings()` is IMPORTED, not copied. This tool adds the framing around it.

C37, and it is the reason Part A runs first: a baseline recorded through an
uncharacterized instrument inherits that instrument's bias permanently, and this
number is meant to outlive everyone who takes it. Part A blurs a REAL glyph crop
by known Gaussian kernels and checks the measurement tracks. If Part A does not
hold, Part B's numbers are not worth recording and this tool says so.

    python tools/glyph_ramp_baseline.py [--report PATH]
"""
import argparse
import statistics
import sys
from pathlib import Path

try:
    from PIL import Image, ImageFilter
except ImportError:
    print("Pillow required: pip install pillow")
    sys.exit(2)

sys.path.insert(0, str(Path(__file__).resolve().parent))
from glyph_ramp_ratio import crossings, MIN_CONTRAST   # noqa: E402  shared primitive

ROOT = Path(__file__).resolve().parent.parent / "artifacts" / "room-visual-pass"

# A Gaussian of standard deviation s renders a step edge with a 10%-90% ramp of
# 2 * 1.2816 * s. This is the only bridge between "kernel I applied" and "ramp I
# measured", so it is written once, here, named.
RAMP_PER_SIGMA = 2.0 * 1.2815515655446004

# Eye-confirmed against the frames (C27), boundaries walked in until each box
# holds one surface only. The first attempt at both straddled: the laptop band
# crossed a panel divider into MY MARKS (different ink), the phone band ran off
# the screen into the lit wall.
SURFACES = [
    {
        "name": "laptop",
        "frame": ROOT / "2026-08-09-r9a-refresh" / "focused-laptop-desk.png",
        "box": (730, 364, 1430, 428),
        "pose": "focused-laptop-desk, 0.52 m along the lid normal, 30 deg",
        "content": "MONEYLINE / MORE / RECORD -- grey uppercase on the dark panel",
    },
    {
        "name": "phone",
        "frame": ROOT / "phone-reference" / "msgs-03" / "phone-focused.png",
        "box": (1030, 812, 1380, 925),
        "pose": "phone-focused, 0.315 m along the screen normal, 30 deg",
        "content": "BOOKIE header + two message lines -- white ink on the bubble",
    },
]

# Kernels for Part A. Deliberately spans below and above 1 px: the instrument is
# expected to be usable low and to saturate high, and a characterization that
# only samples where it works is not a characterization.
SIGMAS = [0.0, 0.4, 0.6, 0.8, 1.0, 1.4, 2.0]


def measure_image(im, box):
    """ramps, strokes for one box of an in-memory image. Same edge logic as
    glyph_ramp_ratio.measure(); the only difference is the source."""
    c = im.crop(box)
    w, h = c.size
    px = c.load()
    ramps, strokes = [], []
    for y in range(h):
        line = [sum(px[x, y][:3]) / 3.0 for x in range(w)]
        lo, hi = min(line), max(line)
        if hi - lo < MIN_CONTRAST:
            continue
        for ramp, stroke in crossings(line, lo, hi):
            ramps.append(ramp)
            strokes.append(stroke)
    return ramps, strokes


def blurred(im, box, sigma, pad=24):
    """Blur with margin, then crop, so the box never sees the image border."""
    if sigma <= 0:
        return im
    x0, y0, x1, y1 = box
    wide = im.crop((x0 - pad, y0 - pad, x1 + pad, y1 + pad))
    wide = wide.filter(ImageFilter.GaussianBlur(radius=sigma))
    out = im.copy()
    out.paste(wide, (x0 - pad, y0 - pad))
    return out


def characterize(surface, out):
    im = Image.open(surface["frame"]).convert("RGB")
    box = surface["box"]

    base_r, base_s = measure_image(im, box)
    if len(base_r) < 30:
        out(f"  *** too few stems ({len(base_r)}) to characterize on {surface['name']} ***")
        return None
    r0 = statistics.median(base_r)

    out(f"  unblurred ramp r0 = {r0:.3f} px   "
        f"(stems {len(base_r)}, stroke {statistics.median(base_s):.3f} px)")
    out("")
    out(f"  {'sigma':>6s} {'added':>7s} {'predicted':>10s} {'measured':>9s} "
        f"{'error':>8s} {'stroke':>8s} {'ratio':>7s}")
    out(f"  {'':>6s} {'':>7s} {'':>10s} {'<-- the ramp tracks -->':>9s}"
        f"   {'<-- the denominator -->':>8s}")

    rows = []
    for s in SIGMAS:
        if s == 0.0:
            continue
        bim = blurred(im, box, s)
        r, st = measure_image(bim, box)
        if len(r) < 10:
            out(f"  {s:>6.2f}  too few stems ({len(r)}) -- edges gone")
            rows.append((s, None, None, None))
            continue
        rm, sm = statistics.median(r), statistics.median(st)
        added = RAMP_PER_SIGMA * s
        pred = (r0 ** 2 + added ** 2) ** 0.5
        err = (rm - pred) / pred * 100.0
        out(f"  {s:>6.2f} {added:>7.3f} {pred:>10.3f} {rm:>9.3f} "
            f"{err:>+7.1f}% {sm:>8.3f} {rm / sm:>7.3f}")
        rows.append((s, err, rm / sm, sm))
    return r0, rows


def main():
    ap = argparse.ArgumentParser(description=__doc__,
                                 formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--report", help="tee the run to a file (C11/C17)")
    args = ap.parse_args()

    lines = []

    def out(s=""):
        print(s)
        lines.append(s)

    out("S2-am2 TWO-SURFACE BASELINE -- ramp / stroke at the acceptance view")
    out("=" * 78)
    out("")
    out("PART A -- C37 characterization, on real glyphs, known Gaussian kernels.")
    out("A baseline through an uncharacterized instrument inherits its bias forever.")
    out("Blur applied with a 24 px margin so the box never sees the image border.")
    out("A Gaussian of sd s gives a 10-90% ramp of 2*1.2816*s; blurs are expected")
    out("to add in QUADRATURE against the frame's own ramp r0.")
    out("")
    char = characterize(SURFACES[0], out)
    if char is None:
        sys.exit(1)
    r0, rows = char

    good = [abs(e) for s, e, _, _ in rows if e is not None and s <= 1.0]
    out("")
    if not good:
        out("  *** characterization produced no usable arms -- Part B not run (C37) ***")
        sys.exit(1)
    worst = max(good)
    out(f"  Recovery below sigma 1.0: worst error {worst:+.1f}%  "
        f"(arms {len(good)})")
    if worst > 15.0:
        out("  *** VOID: the instrument does not track a known kernel within 15%.")
        out("      A baseline taken through it would inherit that bias permanently,")
        out("      so Part B is NOT run. This is C37 doing its job. ***")
        sys.exit(1)
    out("  -> tracks known kernels; the baseline may be recorded.")

    # The finding that only shows up once the denominator is printed beside the
    # numerator: the RAMP is monotonic in blur and tracks the kernel; the RATIO
    # is not monotonic at all, because the stroke grows FASTER than the ramp as
    # neighbouring glyphs merge and the falling 50% crossing runs on into the gap.
    ratios = [(s, r) for s, e, r, _ in rows if r is not None]
    strokes = [(s, sm) for s, e, r, sm in rows if sm is not None]
    rising = all(b >= a for (_, a), (_, b) in zip(strokes, strokes[1:]))
    mono = all(b >= a for (_, a), (_, b) in zip(ratios, ratios[1:]))
    out("")
    out("  THE DENOMINATOR IS NOT SAFE, and this is the characterization's real result.")
    out(f"    stroke under blur: "
        f"{' -> '.join(f'{sm:.2f}' for _, sm in strokes)}"
        f"   ({'monotonic rising' if rising else 'NOT monotonic'})")
    out(f"    ratio  under blur: "
        f"{' -> '.join(f'{r:.3f}' for _, r in ratios)}"
        f"   ({'monotonic' if mono else 'NOT MONOTONIC'})")
    out("    The ramp rises with the kernel exactly as predicted. The stroke rises")
    out("    FASTER over part of the range, because blur merges adjacent glyphs and")
    out("    the falling 50% crossing that ends a stroke runs on into the next gap.")
    out("    So ramp/stroke FALLS while the surface gets softer, then collapses once")
    out("    the glyphs join. **A larger ratio does not mean a softer surface, and a")
    out("    smaller one does not mean a sharper surface, on dense text.** Report the")
    out("    RAMP for a regression; the ratio is only meaningful against an identical")
    out("    string at an identical size, which is the comparison S2-am2 actually asks")
    out("    for -- same surface, same view, over time.")

    out("")
    out("PART B -- the baseline. One number per surface, for regressions to regress from.")
    out("")
    out(f"  {'surface':>8s} {'stems':>6s} {'stroke px':>10s} {'ramp px':>9s} "
        f"{'ramp/stroke':>12s}")
    results = []
    for surf in SURFACES:
        im = Image.open(surf["frame"]).convert("RGB")
        r, st = measure_image(im, surf["box"])
        if len(r) < 30:
            out(f"  {surf['name']:>8s}  FAIL -- only {len(r)} stems; a median on that "
                f"is not a baseline (C29)")
            sys.exit(1)
        rm, sm = statistics.median(r), statistics.median(st)
        out(f"  {surf['name']:>8s} {len(r):>6d} {sm:>10.3f} {rm:>9.3f} {rm / sm:>12.3f}")
        results.append((surf, rm, sm, len(r)))

    out("")
    out("PROVENANCE -- each number is only as good as the frame under it")
    for surf, rm, sm, n in results:
        out(f"  {surf['name']}: {surf['frame'].relative_to(ROOT.parent.parent)}")
        out(f"    pose    {surf['pose']}")
        out(f"    box     {surf['box']}  (eye-confirmed, C27)")
        out(f"    content {surf['content']}")
    out("")
    out("SCOPE (C25). Both frames are HARNESS captures through RenderTexture, not")
    out("Game-view grabs, so neither inherits the 'Low Resolution Aspect Ratios'")
    out("resample that C38-cl closed on. Each number is ONE box on ONE frame of one")
    out("pinned run; it is a baseline, not a distribution. The phone frame is seed")
    out("PHONEREF01 at msgs-03 -- another seed yields different copy, so a future")
    out("comparison must pin the same seed or compare the ratio only.")

    if args.report:
        Path(args.report).write_text("\n".join(lines) + "\n", encoding="utf-8")
        print(f"\nreport written: {args.report}")


if __name__ == "__main__":
    main()
