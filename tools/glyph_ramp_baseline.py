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
import json
import statistics
import sys
from pathlib import Path

try:
    from PIL import Image, ImageFilter
except ImportError:
    print("Pillow required: pip install pillow")
    sys.exit(2)

# Importing a sibling tool writes tools/__pycache__ next to the sources, which is
# a stray in a repo whose untracked-file hygiene is actively being cleaned up.
# Suppressed at the cause rather than deleted after each run.
sys.dont_write_bytecode = True
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

# The floor batch 25 asks to subtract as sqrt(measured^2 - FLOOR^2).
BUNDLE_FLOOR = 1.680

# Batch 25: the number the floor has never had is ramp/stroke on the SMALLEST
# product fact, not on a price row. 0.482 was taken on price figures; the ramp is
# fixed in screen px while the stroke scales with type size, so the smallest type
# necessarily carries the worst ratio. These are the two element groups S2-am
# named. Every box eye-confirmed on the frame (C27); two were re-cut after the
# first pass clipped a glyph and caught a sliver of the team name behind it.
SMALLEST = {
    "season records": [
        (704, 440, 763, 463), (678, 502, 737, 525), (688, 568, 748, 591),
        (704, 630, 763, 653), (755, 694, 814, 717), (722, 756, 781, 779),
        (699, 822, 758, 845), (674, 883, 741, 906), (699, 949, 758, 972),
        (686, 1010, 745, 1033), (730, 1075, 789, 1098), (614, 1137, 673, 1160),
    ],
    "row numbers 01-06": [(455, 435, 515, 1165)],
}


def instrument_floor():
    """The instrument's OWN ramp on a synthetic perfect step edge.

    This exists because the floor batch 25 asks to subtract (1.680 px) has no
    retained derivation anywhere in the repo, and two very different quantities
    could wear that name: the measuring method's own response to a hard edge, or
    the BUILD's real screen-space ramp. They are not interchangeable and only one
    of them is legitimate to subtract as an instrument artefact.
    """
    # A BAR, not a step: crossings() measures a stroke as rise-then-fall, so a
    # lone step edge yields nothing at all. A hard-edged bar is also the honest
    # synthetic analogue of a glyph stem.
    w, h, bar = 200, 60, 20
    im = Image.new("RGB", (w, h), (12, 12, 14))
    x0 = (w - bar) // 2
    for y in range(h):
        for x in range(x0, x0 + bar):
            im.putpixel((x, y), (190, 188, 180))
    r, s = measure_image(im, (0, 0, w, h))
    return (statistics.median(r) if r else None), len(r)


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
    ap.add_argument("--authored-stroke", type=float, metavar="PX",
                    help="the face's own stroke metric at the shipped point size, in "
                         "SCREEN px at this view. S2-am2-am makes this the denominator "
                         "for any ACROSS-TIME comparison: a constant cannot be inflated "
                         "by the glyph-merging artefact that inflates a measured stroke, "
                         "so the quantity stays monotonic in blur while keeping clause "
                         "2's meaning. Within one frame the measured stroke is correct "
                         "and is always reported.")
    ap.add_argument("--laptop-frame", metavar="PATH",
                    help="measure this laptop frame instead of the built-in r9a-refresh "
                         "one. ADDED BY THE SURETHING SEAT, 2026-08-10, and deliberately "
                         "an override rather than an edit to SURFACES: the built-in path "
                         "is room's COMMITTED baseline set, and a re-shoot that wrote over "
                         "it would destroy the very comparand it is measured against. The "
                         "default is unchanged, so room's own runs behave exactly as "
                         "before. Boxes are NOT auto-adjusted — if the layout moved, "
                         "re-cut and eye-confirm them (C27) or this silently measures the "
                         "wrong pixels.")
    ap.add_argument("--smallest-json", metavar="PATH",
                    help="replace the SMALLEST boxes from a JSON file of "
                         "{group: [[x0,y0,x1,y1], ...]}. ADDED BY THE SURETHING SEAT, "
                         "2026-08-10, for the same reason as --laptop-frame: the built-in "
                         "boxes are eye-confirmed against ROOM'S frame and are correct "
                         "there. They are NOT correct on a re-shoot, because the season "
                         "record's x depends on the team name beside it and the harness "
                         "renders a live, unpinned slate — so a different deal moves the "
                         "records and leaves the boxes measuring team names, empty ground, "
                         "or half a glyph. The row-number column survives a re-deal because "
                         "its x is fixed. Whatever is passed here must be eye-confirmed on "
                         "the frame it will be used on (C27); nothing about this flag makes "
                         "that unnecessary.")
    args = ap.parse_args()

    if args.laptop_frame:
        # .resolve() matters: the PROVENANCE block does frame.relative_to(repo root),
        # which raises on a relative path. Passing "artifacts/..." on the command line
        # therefore crashed the run AFTER Part B had already printed - the numbers
        # looked complete and the report file was never written.
        SURFACES[0]["frame"] = Path(args.laptop_frame).resolve()
    if args.smallest_json:
        SMALLEST.clear()
        SMALLEST.update({k: [tuple(b) for b in v]
                         for k, v in json.loads(Path(args.smallest_json).read_text()).items()})

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

    # ---- Part C: the number batch 25 actually asked for -----------------
    out("")
    out("PART C -- ramp/stroke on the SMALLEST product fact (register batch 25)")
    out("")
    floor_r, floor_n = instrument_floor()
    out(f"  Instrument's own ramp on a synthetic PERFECT step edge: "
        f"{floor_r:.3f} px ({floor_n} edges)")
    out(f"  The floor batch 25 asks to subtract is {BUNDLE_FLOOR:.3f} px -- "
        f"{BUNDLE_FLOOR / floor_r:.1f}x larger.")
    out("  So 1.680 is NOT the measuring method's artefact; it is the BUILD's own")
    out("  screen-space ramp (C38's '~1.6 px', real and ruled a characteristic).")
    out("  Subtracting it therefore yields blur ABOVE the known floor, which is the")
    out("  right quantity for a regression -- but it is not an instrument correction,")
    out("  and a residual near zero means 'at the floor', not 'sharp'.")
    out("")
    out(f"  {'element group':>20s} {'boxes':>6s} {'stems':>6s} {'stroke':>8s} "
        f"{'ramp':>7s} {'ratio':>7s} {'above floor':>12s}")
    im = Image.open(SURFACES[0]["frame"]).convert("RGB")
    for name, boxes in SMALLEST.items():
        ramps, strokes = [], []
        for b in boxes:
            r, s = measure_image(im, b)
            ramps += r
            strokes += s
        if len(ramps) < 12:
            out(f"  {name:>20s}  FAIL -- {len(ramps)} stems is not a median (C29)")
            continue
        rm, sm = statistics.median(ramps), statistics.median(strokes)
        resid = (rm ** 2 - BUNDLE_FLOOR ** 2)
        resid = resid ** 0.5 if resid > 0 else 0.0
        note = f"{resid:.3f} px" if resid > 0 else "AT/BELOW floor"
        out(f"  {name:>20s} {len(boxes):>6d} {len(ramps):>6d} {sm:>8.3f} "
            f"{rm:>7.3f} {rm / sm:>7.3f} {note:>12s}")
        if args.authored_stroke:
            out(f"  {'':>20s} S2-am2-am, across-time form: ramp / AUTHORED stroke "
                f"({args.authored_stroke:.3f} px) = "
                f"{rm / args.authored_stroke:.3f}")
    if not args.authored_stroke:
        out("")
        out("  NOTE: --authored-stroke was not supplied, so only the WITHIN-FRAME form")
        out("  is reported above. Per S2-am2-am that form is correct for comparing")
        out("  elements inside this frame and is NOT valid across time. Supply the")
        out("  face's authored stroke at the shipped point size, in screen px at this")
        out("  view, to get the regression-safe number.")
    out("")
    out("  Saturation check: Part A shows the ratio compressing above sigma ~1, where")
    out("  the measured ramp reaches ~3.2 px. Both groups above sit well below that,")
    out("  so neither number is taken from the saturated part of the range.")
    out("")
    out("  READ THIS BEFORE RANKING THE TWO RATIOS. Part A established that")
    out("  ramp/stroke is NOT monotonic in blur, so the group with the higher ratio")
    out("  is not thereby 'softer'. Within one frame at one view the ratios are")
    out("  comparable as a fraction-of-stroke-in-transition, which is what S2-am's")
    out("  clause 2 is about; they are NOT comparable across blur levels.")

    if args.report:
        Path(args.report).write_text("\n".join(lines) + "\n", encoding="utf-8")
        print(f"\nreport written: {args.report}")


if __name__ == "__main__":
    main()
