#!/usr/bin/env python3
"""
V6 — room-region readings across an event burst.

The gate on TV's owning document (batch 13, T65): "Room re-tint stays inside the
room's palette." Blind to: the panel's own content.

WHY THIS EXISTS
    Every previous TV frame review measured the panel and cropped the room. The
    loudest thing the TV does to this game was outside every box the studio had
    drawn: on a leg win the room rotated ~90 deg of hue, roughly doubled in
    luminance and reached 71% saturation, and no instrument looked. Room-region
    readings are now a permanent part of the TV's capture contract.

THE UNIT (C33)
    Rec.709 luma on display-encoded values, studio-wide, quoted with every
    number. RGB-average and linear luminance both mis-rank a saturated warm
    element against a neutral one, and this project's semantics put *money* in
    the saturated warm element.

BOX DERIVATION (shipped with the numbers, per the T63 method the DD accepted)
    The panel rect is solved from the canvas->frame mapping, not eyeballed:
        canvas 980x550 -> frame, scale x 2176/980 = 2.2204, y 1223/550 = 2.2236.
    Agreement to three decimals on both axes is the check that the PANEL was
    framed and not the room. The origin follows from the LayoutGrid CashOut zone
    landing where it was rendered and confirmed: canvas (0,480) -> frame
    (185,1181), so canvas (0,0) -> frame (185, 113.7).

    Room regions are then taken OUTSIDE that rect, inset far enough to clear the
    housing. --validate renders the boxes so they are confirmed by eye as well as
    by variance (C27: a low sd/mean proves uniformity, NOT single-surface
    membership).

CALIBRATION
    On the T63 bundle frames this instrument's `above` box reproduces the Design
    Director's own reading to the digit -- 30,50,33 on the GoalFor frame and
    112,86,32 on the LegFinalWon frame. That is what makes these numbers
    comparable to the ruling's.

Usage:
    python tools/v6_room_region.py <glob-or-dir> [--validate OUT.png] [--band LO HI]
"""
import argparse
import glob
import os
import sys

import numpy as np
from PIL import Image, ImageDraw

# ---------------------------------------------------------------- panel rect
PANEL_X0, PANEL_Y0 = 185.0, 113.7
PANEL_W, PANEL_H = 2176.0, 1223.0
PANEL_X1, PANEL_Y1 = PANEL_X0 + PANEL_W, PANEL_Y0 + PANEL_H  # 2361.0, 1336.7
REFERENCE_FRAME = (2560, 1440)

# Room regions, all OUTSIDE the panel rect. x0, y0, x1, y1 in frame px.
#
# NAMED FOR WHAT THEY SIT ON, not for what one wishes they framed. Confirmed by
# rendering the boxes and looking (--validate): on a near-panel capture the four
# margins are the TV's own riveted HOUSING, not plaster. The batch-13 ruling's
# regions are called "wall"; the first four below reproduce them (`housing above
# panel` returns the ruling's 30,50,33 -> 112,86,32 to the digit) and are the
# same boxes under an accurate name.
#
# This does NOT weaken the ruling. The flood is a room event, not a housing
# artefact: red gain falls off with distance from the panel across the right
# margin (+44.5 -> +20.0 -> +9.5 over three successive bands), which is a point
# light's profile. `far field right` samples that falloff and `unlit left edge`
# samples the one surface that does not respond at all (+1.8), so a reading that
# moves everything equally is visibly an exposure change rather than a re-tint.
#
# SCOPE: a complete V6 reading wants the SEATED ROOM CAMERA -- the owning
# document's §1.3 makes the in-room render at seated distance the only valid
# acceptance view, and these near-panel frames contain very little plaster.
REGIONS = {
    "housing above panel": (400, 8, 2000, 100),      # == the ruling's "wall above panel"
    "housing left":        (144, 300, 180, 1100),    # == "wall left"
    "housing right":       (2384, 300, 2420, 1100),  # == "wall right"
    "housing below panel": (400, 1340, 2000, 1372),  # == "wall below panel"
    "far field right":     (2440, 300, 2520, 1100),  # beyond the housing, in the falloff
    "unlit left edge":     (0, 300, 48, 1100),       # the control: does not respond
}

# The room's sanctioned warm band for a re-tint (T65 clause 3): the room's own
# warm key sits at ~92 deg and the laptop lid's sanctioned contribution at
# 85.1-85.3 deg. A saturated 40 deg amber is a new hue, not a warming.
BAND_LO, BAND_HI = 85.0, 92.0


def rec709(rgb01):
    """C33's unit: Rec.709 luma on display-encoded values."""
    return 0.2126 * rgb01[..., 0] + 0.7152 * rgb01[..., 1] + 0.0722 * rgb01[..., 2]


def hue_sat(r, g, b):
    """HSV hue (deg) and saturation from a mean RGB triple in 0..255."""
    mx, mn = max(r, g, b), min(r, g, b)
    d = mx - mn
    if mx <= 0 or d <= 0:
        return float("nan"), 0.0
    if mx == r:
        h = 60.0 * (((g - b) / d) % 6.0)
    elif mx == g:
        h = 60.0 * (((b - r) / d) + 2.0)
    else:
        h = 60.0 * (((r - g) / d) + 4.0)
    return h, d / mx


def read_region(a255, box):
    x0, y0, x1, y1 = box
    blk = a255[y0:y1, x0:x1]
    r, g, b = blk[..., 0].mean(), blk[..., 1].mean(), blk[..., 2].mean()
    h, s = hue_sat(r, g, b)
    lum = rec709(blk / 255.0)
    return {
        "rgb": (r, g, b), "hue": h, "sat": s,
        "luma": float(lum.mean()), "luma_peak": float(lum.max()),
        # C27: uniformity only. It does not establish that the box sits on one surface.
        "sdmean": float(lum.std() / lum.mean()) if lum.mean() > 0 else float("nan"),
    }


def validate_render(path, out):
    im = Image.open(path).convert("RGB")
    d = ImageDraw.Draw(im)
    d.rectangle([PANEL_X0, PANEL_Y0, PANEL_X1, PANEL_Y1], outline=(0, 160, 255), width=5)
    for name, (x0, y0, x1, y1) in REGIONS.items():
        d.rectangle([x0, y0, x1, y1], outline=(255, 0, 0), width=4)
        d.text((x0 + 6, max(0, y0 - 12)), name, fill=(255, 255, 0))
    im.resize((im.width // 2, im.height // 2), Image.LANCZOS).save(out)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("target", help="glob or directory of frames")
    ap.add_argument("--validate", metavar="OUT.png", default=None,
                    help="render the boxes on the first frame and exit")
    ap.add_argument("--band", nargs=2, type=float, default=[BAND_LO, BAND_HI])
    args = ap.parse_args()

    t = args.target
    paths = sorted(glob.glob(os.path.join(t, "*.png")) if os.path.isdir(t) else glob.glob(t))
    if args.validate:
        if not paths:
            print("V6: no frames matched", file=sys.stderr)
            return 2
        print(validate_render(paths[0], args.validate))
        return 0

    lo, hi = args.band
    print("V6 — room-region readings across an event burst")
    print(f"unit: Rec.709 luma, display-encoded (C33) · band: {lo:.0f}-{hi:.0f} deg hue")
    print(f"panel rect (excluded): x[{PANEL_X0:.0f}..{PANEL_X1:.0f}] y[{PANEL_Y0:.0f}..{PANEL_Y1:.0f}]")
    print("blind to: the panel's own content; which of the two room-facing channels "
          "(point light / emissive quad) carries a reading; anything the capture "
          "states do not force.")
    print("resolution (C32): hue is derived from a region MEAN, so it resolves a "
          "few tenths of a degree on a uniform box and much less on a mixed one; "
          "sd/mean is printed so a mixed box is visible rather than silent.\n")

    executed, worst = 0, []
    for p in paths:
        a = np.asarray(Image.open(p).convert("RGB"), dtype=np.float32)
        if (a.shape[1], a.shape[0]) != REFERENCE_FRAME:
            print(f"  SKIP {os.path.basename(p)} — {a.shape[1]}x{a.shape[0]}, "
                  f"boxes are derived for {REFERENCE_FRAME[0]}x{REFERENCE_FRAME[1]}")
            continue
        executed += 1
        name = os.path.basename(p)
        # the grammar token is the beat this frame belongs to; it is what makes a
        # reading attributable to a code path rather than to a frame index.
        gram = next((s.split("grammar-")[1] for s in name.split("__") if s.startswith("grammar-")), "?")
        frame = next((s for s in name.replace(".png", "").split("__") if s.startswith("frame")), "?")
        print(f"{frame}  {gram}")
        for rn, box in REGIONS.items():
            m = read_region(a, box)
            flag = ""
            if not np.isnan(m["hue"]):
                flag = "  IN-BAND" if lo <= m["hue"] <= hi else "  out-of-band"
                if not (lo <= m["hue"] <= hi):
                    worst.append((abs(m["hue"] - (lo + hi) / 2), frame, gram, rn, m["hue"]))
            print(f"    {rn:<18} rgb {m['rgb'][0]:6.1f},{m['rgb'][1]:6.1f},{m['rgb'][2]:6.1f}"
                  f"  hue {m['hue']:6.1f}  sat {m['sat']*100:5.1f}%"
                  f"  luma {m['luma']:.3f}  sd/mean {m['sdmean']:.3f}{flag}")
        print()

    # C29: a run that executed nothing demonstrated nothing.
    print(f"executed: {executed} frame(s) x {len(REGIONS)} regions = {executed*len(REGIONS)} readings")
    if executed == 0:
        print("V6: FAIL — zero frames executed (C29)", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
