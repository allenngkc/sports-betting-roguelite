#!/usr/bin/env python3
"""
The brightness ladder, re-read in the ruled unit.

C33 (DD 2026-08-07): "The ladder's unit is Rec.709 luma on display-encoded
values, studio-wide, quoted with every number." Three conventions had been in
simultaneous use -- RGB-average, Rec.709 luma, and linear-space luminance --
producing a reported gap of 0.21 where the real one was 0.047. RGB-average and
linear both mis-rank a saturated warm element against a neutral one, and this
project's semantics put *money* in the saturated warm element.

    "Nothing tunes against a ladder number taken in the old unit."

So this tool does two things. It reads every tier-assigned element in Rec.709,
and it prints the SAME element in all three conventions side by side -- because
the studio has years of numbers in the other two, and a conversion table is what
makes them translatable instead of merely void.

ZONES ARE READ FROM THE PRODUCTION GRID, NOT EYEBALLED
    LayoutGrid(980, 550) is a pure function of the canvas size, so it is
    recomputed here from the same constants rather than measured off a frame:
        ticketW = round(980 * 0.27) = 265      contentH = 550 - 18 = 532
        bottomY = 532 - 52 = 480               rowH = (480 - 24 - 40) / 6 = 69.33
    Two independent facts confirm the transcription: the CashOut rect comes out
    at canvas (0,480)-(265,532), which is the box the T63 bundle shipped and the
    DD validated, and the row height comes out at 69.33px, which is the "69.3px
    slot" the T24 re-measure was decided against.

CANVAS -> FRAME
    scale x 2176/980 = 2.2204, y 1223/550 = 2.2236 (agreeing to three decimals is
    the check that the panel was framed and not the room); origin (185, 113.7).

WHAT A "TIER" READING IS
    The ladder is about how bright an element GETS, not how bright its zone
    averages -- a zone is mostly unlit substrate by design. So each zone reports
    a robust peak (99.9th percentile) alongside the true max and the mean. The
    99.9th is the headline: a true max is one pixel and moves with antialiasing.

Usage:
    python tools/ladder_read.py <glob-or-dir> [--frame SUBSTRING]
"""
import argparse
import glob
import os
import sys

import numpy as np
from PIL import Image

# ---- LayoutGrid(980, 550), recomputed from the production constants ----------
W, H = 980.0, 550.0
TICKET_W = round(W * 0.27)          # 265
CHROME_H, BOTTOM_H = 18.0, 52.0
SCOREBUG_H, TAPE_H = 62.0, 14.0
HEADER_H, FOOTER_H = 24.0, 40.0
CONTENT_H = H - CHROME_H            # 532
BOTTOM_Y = CONTENT_H - BOTTOM_H     # 480
RIGHT_X, RIGHT_W = TICKET_W, W - TICKET_W

# canvas-space zones: (x0, y0, x1, y1) with the tier the owning document assigns
ZONES = {
    #  name                       x0        y0                     x1              y1                tier
    "cash-out band":        (0.0, BOTTOM_Y, TICKET_W, CONTENT_H, "L4 actionable / L1 suspended"),
    "scoreline":            (RIGHT_X, 0.0, W, SCOREBUG_H - TAPE_H, "L3 quiet, L4 at the goal punch"),
    "momentum tape":        (RIGHT_X, SCOREBUG_H - TAPE_H, W, SCOREBUG_H, "L2 label+current, L1 history"),
    "stage / pitch":        (RIGHT_X, SCOREBUG_H, W, BOTTOM_Y, "L1-L2 markings, L3 actors, L4 ball at payoff"),
    "event strip":          (RIGHT_X, BOTTOM_Y, W, CONTENT_H, "L2 context"),
    "ticket header":        (0.0, 0.0, TICKET_W, HEADER_H, "L1 structure"),
    "ticket rows":          (0.0, HEADER_H, TICKET_W, BOTTOM_Y - FOOTER_H, "L3 live, L1 next, L0 dead"),
    "risk/pays footer":     (0.0, BOTTOM_Y - FOOTER_H, TICKET_W, BOTTOM_Y, "L2 gold"),
    "chrome strip":         (0.0, CONTENT_H, W, H, "lowest priority"),
}
# The ball is an object, not a zone. The DD located it at canvas (921, 343) on
# frame000 of the cash-out burst; read a small box around that.
BALL = (911.0, 333.0, 931.0, 353.0)

SX, SY = 2176.0 / 980.0, 1223.0 / 550.0
OX, OY = 185.0, 113.7


def to_frame(x0, y0, x1, y1):
    return (int(round(OX + x0 * SX)), int(round(OY + y0 * SY)),
            int(round(OX + x1 * SX)), int(round(OY + y1 * SY)))


def rec709(a):
    """C33's ruled unit: Rec.709 luma on DISPLAY-ENCODED values."""
    return 0.2126 * a[..., 0] + 0.7152 * a[..., 1] + 0.0722 * a[..., 2]


def rgb_average(a):
    """The superseded unit. Weights the near-zero blue channel at one third, so
    it systematically UNDER-reports saturated warm colour -- which is precisely
    the colour the entire gold ration lives in."""
    return a.mean(axis=-1)


def linear_luminance(a):
    """The third convention. sRGB -> linear, then Rec.709 weights. This is what
    the earlier 0.737 scoreline figure was in."""
    lin = np.where(a <= 0.04045, a / 12.92, ((a + 0.055) / 1.055) ** 2.4)
    return rec709(lin)


def peak_hue_sat(block255, lum, q=99.9):
    """Hue/sat of the pixels AT the robust peak -- what colour the element's
    brightest ink actually is, rather than the zone's average colour."""
    thr = np.percentile(lum, q)
    sel = block255[lum >= thr]
    if sel.size == 0:
        return float("nan"), 0.0
    r, g, b = sel[..., 0].mean(), sel[..., 1].mean(), sel[..., 2].mean()
    mx, mn = max(r, g, b), min(r, g, b)
    if mx <= 0 or mx == mn:
        return float("nan"), 0.0
    d = mx - mn
    if mx == r:
        h = 60.0 * (((g - b) / d) % 6.0)
    elif mx == g:
        h = 60.0 * (((b - r) / d) + 2.0)
    else:
        h = 60.0 * (((r - g) / d) + 4.0)
    return h, d / mx


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("target")
    ap.add_argument("--frame", default=None, help="only frames whose name contains this")
    args = ap.parse_args()

    t = args.target
    paths = sorted(glob.glob(os.path.join(t, "*.png")) if os.path.isdir(t) else glob.glob(t))
    if args.frame:
        paths = [p for p in paths if args.frame in os.path.basename(p)]

    print("THE BRIGHTNESS LADDER, RE-READ IN THE RULED UNIT")
    print("unit: Rec.709 luma on display-encoded values (C33). Every number below is that unit")
    print("      unless a column says otherwise. Headline figure is the 99.9th percentile.")
    print("scope (C25): reads the PANEL only. Blind to the room (that is V6), to whether an")
    print("      element holds the HDR token (that is the one-token invariant), and to any")
    print("      state the capture did not force. Zones come from LayoutGrid, so a zone that is")
    print("      dark simply had no lit element in it on that frame -- not a failure.")
    print("resolution (C32): 8-bit display-encoded input, so one code value is ~0.004 luma;")
    print("      differences under ~0.01 are not resolvable and are not reported as ordering.\n")

    executed = 0
    for p in paths:
        a255 = np.asarray(Image.open(p).convert("RGB"), dtype=np.float32)
        a01 = a255 / 255.0
        name = os.path.basename(p)
        gram = next((s.split("grammar-")[1] for s in name.split("__") if s.startswith("grammar-")), "?")
        mom = next((s.split("moment-")[1] for s in name.split("__") if s.startswith("moment-")), "?")
        fr = next((s for s in name.replace(".png", "").split("__") if s.startswith("frame")), "?")
        executed += 1

        print(f"=== {fr}  {gram}  ({mom})")
        print(f"    {'element':<20} {'Rec.709':>8} {'  max':>7} {' mean':>7} | "
              f"{'RGB-avg':>8} {'linear':>7} | {'hue':>6} {'sat':>6}   tier")
        rows = list(ZONES.items()) + [("ball", BALL + ("L4 only at a payoff",))]
        for zname, spec in rows:
            x0, y0, x1, y1, tier = spec
            fx0, fy0, fx1, fy1 = to_frame(x0, y0, x1, y1)
            blk255 = a255[fy0:fy1, fx0:fx1]
            blk01 = a01[fy0:fy1, fx0:fx1]
            if blk01.size == 0:
                continue
            l709 = rec709(blk01)
            p999 = float(np.percentile(l709, 99.9))
            hue, sat = peak_hue_sat(blk255, l709)
            print(f"    {zname:<20} {p999:8.3f} {float(l709.max()):7.3f} {float(l709.mean()):7.3f} | "
                  f"{float(np.percentile(rgb_average(blk01), 99.9)):8.3f} "
                  f"{float(np.percentile(linear_luminance(blk01), 99.9)):7.3f} | "
                  f"{hue:6.1f} {sat*100:5.1f}%   {tier}")

        # substrate: the darkest 2% of the panel, the value the ladder's L0 sits on
        px0, py0, px1, py1 = to_frame(0, 0, W, H)
        panel = rec709(a01[py0:py1, px0:px1])
        print(f"    {'substrate (dk 2%)':<20} {float(np.percentile(panel, 2)):8.3f}")
        print()

    print(f"executed: {executed} frame(s)")   # C29
    return 1 if executed == 0 else 0


if __name__ == "__main__":
    sys.exit(main())
