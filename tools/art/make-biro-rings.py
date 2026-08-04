#!/usr/bin/env python
"""
Ballpoint ring generator — SureThing "annotated form guide" selection mark.

Draws the ring the way a ballpoint actually lays ink down, rather than stroking a
clean vector ellipse:

  * the path is an ellipse deformed by a few low-frequency harmonics, so no two
    rings are the same shape and none of them is truly round;
  * the pen OVERSHOOTS the start point, because people do not stop where they began;
  * a second partial pass retraces part of the ring, offset from the first;
  * ink density varies along the path — heavier where the hand slows at the turns,
    lighter on the fast run, with a pressure ramp in at the start and a lifted tail;
  * short skips drop out where the ball fails to deposit;
  * crossings accumulate, so where pass two runs over pass one the ink is darker.

Output is WHITE RGB with the ink carried entirely in the ALPHA channel, so Unity
tints it at runtime via Image.color. One asset serves any ink colour.

Usage:
    python make-biro-rings.py                     # writes the default set
    python make-biro-rings.py --out <dir>         # target a different folder
    python make-biro-rings.py --scale 4           # heavier supersampling

Deterministic: a given seed always produces the same ring, so regenerating never
silently changes a shipped asset.
"""

from __future__ import annotations

import argparse
import math
import random
from pathlib import Path

from PIL import Image

TAU = math.pi * 2


# ── the path ────────────────────────────────────────────────────────────────

def ring_points(w: float, h: float, seed: int, pass_index: int, steps: int):
    """Yield (x, y, ink) along one pass of a hand-drawn ring.

    `ink` is 0..1 ink density at that sample, before skips are applied.
    """
    # Shape is decided ONCE per ring, not per pass. A retrace that generates its
    # own harmonics and its own start angle does not follow the first line — it
    # cuts across the interior and reads as a smear instead of a second lap.
    rng = random.Random(seed * 9781)
    prng = random.Random(seed * 9781 + pass_index * 131 + 7)

    margin = min(w, h) * 0.13
    cx, cy = w / 2.0, h / 2.0
    rx, ry = (w / 2.0) - margin, (h / 2.0) - margin

    # Low-frequency shape noise. Three harmonics is enough to read as "hand drawn"
    # without tipping into a wobbly cartoon.
    harmonics = [
        (rng.uniform(0.030, 0.075), rng.uniform(0, TAU), 2),
        (rng.uniform(0.018, 0.048), rng.uniform(0, TAU), 3),
        (rng.uniform(0.010, 0.028), rng.uniform(0, TAU), 5),
    ]

    # Right-handers circling a word tend to start lower-left and go anticlockwise.
    start = rng.uniform(math.pi * 0.72, math.pi * 1.04)

    # The whole ring drifts a little off-centre; nobody centres it perfectly.
    drift_x = rng.uniform(-0.035, 0.035) * w
    drift_y = rng.uniform(-0.045, 0.045) * h
    tilt = rng.uniform(-0.09, 0.09)

    # Ink pooling noise — where the hand slowed down.
    pool = [(rng.uniform(0.14, 0.34), rng.uniform(0, TAU), k) for k in (1, 2, 4)]

    # --- per-pass, derived from the shared shape above -----------------------
    if pass_index == 0:
        sweep = TAU + prng.uniform(0.30, 0.70)     # full lap plus overshoot
        radial_bias = 1.0
        start_offset = 0.0
    else:
        # The retrace re-enters near where the first lap began and follows it,
        # sitting a hair inside or outside.
        sweep = prng.uniform(TAU * 0.26, TAU * 0.46)
        radial_bias = prng.uniform(0.972, 1.028)
        start_offset = prng.uniform(-0.22, 0.22)
    start += start_offset

    for i in range(steps + 1):
        t = i / steps
        a = start - sweep * t                       # anticlockwise

        r = 1.0
        for amp, phase, k in harmonics:
            r += amp * math.sin(k * a + phase)
        r *= radial_bias

        # progressive drift, strongest by the end of the stroke
        dx = drift_x * t
        dy = drift_y * t

        x = cx + dx + (rx * r) * math.cos(a)
        y = cy + dy + (ry * r) * math.sin(a)

        # apply a slight tilt to the whole ring
        ox, oy = x - cx, y - cy
        x = cx + ox * math.cos(tilt) - oy * math.sin(tilt)
        y = cy + ox * math.sin(tilt) + oy * math.cos(tilt)

        ink = 0.80
        for amp, phase, k in pool:
            ink += amp * math.sin(k * a + phase) * 0.8
        ink = max(0.22, min(1.0, ink))

        # pressure ramps in over the first few percent, and the pen lifts at the end
        if t < 0.05:
            ink *= 0.30 + 0.70 * (t / 0.05)
        if t > 0.86:
            ink *= max(0.10, 1.0 - ((t - 0.86) / 0.14) * 0.92)

        yield x, y, ink


def skip_mask(seed: int, pass_index: int, steps: int):
    """Short dropouts where the ball fails to deposit ink."""
    rng = random.Random(seed * 3307 + pass_index * 17 + 5)
    mask = [1.0] * (steps + 1)
    for _ in range(rng.randint(2, 5)):
        start = rng.randint(0, steps - 1)
        length = rng.randint(int(steps * 0.006), int(steps * 0.022) + 2)
        depth = rng.uniform(0.0, 0.35)
        for i in range(start, min(steps + 1, start + length)):
            mask[i] = min(mask[i], depth)
    return mask


# ── rasterising ─────────────────────────────────────────────────────────────

def make_kernel(radius: float):
    """Soft round dab, returned as (offsets, weights)."""
    r = int(math.ceil(radius)) + 1
    offs, wts = [], []
    for dy in range(-r, r + 1):
        for dx in range(-r, r + 1):
            d = math.hypot(dx, dy)
            if d > radius + 0.75:
                continue
            # smooth falloff at the rim keeps the downsample from looking chewed
            w = 1.0 if d <= radius - 0.5 else max(0.0, (radius + 0.5 - d) / 1.0)
            if w > 0:
                offs.append((dx, dy))
                wts.append(w)
    return offs, wts


def draw_ring(w: int, h: int, seed: int, scale: int, pen: float, passes: int = 2):
    """Render one ring to an alpha buffer at `scale`× then downsample."""
    W, H = w * scale, h * scale
    buf = [0.0] * (W * H)

    for p in range(passes):
        steps = int(1100 * scale / 2)
        mask = skip_mask(seed, p, steps)
        # the retrace is lighter than the first pass
        pass_gain = 1.0 if p == 0 else 0.62

        rng = random.Random(seed * 61 + p)
        base_r = pen * scale * (1.0 if p == 0 else rng.uniform(0.82, 0.96))

        for i, (x, y, ink) in enumerate(ring_points(W, H, seed, p, steps)):
            a = ink * mask[i] * pass_gain
            if a <= 0.004:
                continue
            # pen radius tracks pressure a little — heavier ink is a wider line
            radius = base_r * (0.80 + 0.30 * ink)
            offs, wts = KERNEL_CACHE.setdefault(
                round(radius, 2), make_kernel(round(radius, 2))
            )
            ix, iy = int(x), int(y)
            for (dx, dy), kw in zip(offs, wts):
                px, py = ix + dx, iy + dy
                if 0 <= px < W and 0 <= py < H:
                    idx = py * W + px
                    # partial accumulation: crossings darken, but never blow out
                    buf[idx] = min(1.0, buf[idx] + a * kw * 0.34)

    big = Image.new("L", (W, H))
    big.putdata([int(v * 255) for v in buf])
    small = big.resize((w, h), Image.LANCZOS)

    out = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    out.putalpha(small)
    # white RGB throughout so Unity's Image.color tint is the only colour source
    px = out.load()
    for y in range(h):
        for x in range(w):
            _, _, _, a = px[x, y]
            px[x, y] = (255, 255, 255, a)
    return out


KERNEL_CACHE: dict[float, tuple] = {}


# ── the shipped set ─────────────────────────────────────────────────────────

# Pen radius is deliberately generous. The laptop is read at an angle at reduced
# scale, where a 1px line disintegrates — the legibility floor bans hairlines, and
# an ink mark is no exception to it.
VARIANTS = [
    # name,                w,    h,   seed, pen(px @1x)
    ("ring-price-a",      112,   46,   11,  1.55),
    ("ring-price-b",      112,   46,   27,  1.46),
    ("ring-price-c",      112,   46,   43,  1.64),
    ("ring-wide-a",       176,   46,   58,  1.55),
    ("ring-wide-b",       176,   46,   71,  1.46),
    ("strike-a",          112,   46,   90,  1.95),  # see draw_strike
]


def draw_strike(w: int, h: int, seed: int, scale: int, pen: float):
    """A single struck-through line — the house killing a dead leg during the sweat."""
    W, H = w * scale, h * scale
    buf = [0.0] * (W * H)
    rng = random.Random(seed)
    steps = int(900 * scale / 2)

    y0 = H * rng.uniform(0.46, 0.54)
    y1 = H * rng.uniform(0.46, 0.54)
    x0 = W * rng.uniform(0.02, 0.06)
    x1 = W * rng.uniform(0.94, 0.98)
    bow = rng.uniform(-0.10, 0.10) * H
    base_r = pen * scale

    mask = skip_mask(seed, 0, steps)
    for i in range(steps + 1):
        t = i / steps
        x = x0 + (x1 - x0) * t
        y = y0 + (y1 - y0) * t + math.sin(t * math.pi) * bow
        ink = 0.85
        if t < 0.06:
            ink *= 0.25 + 0.75 * (t / 0.06)
        if t > 0.90:
            ink *= max(0.12, 1.0 - ((t - 0.90) / 0.10) * 0.88)
        a = ink * mask[i]
        if a <= 0.004:
            continue
        radius = base_r * (0.85 + 0.25 * ink)
        offs, wts = KERNEL_CACHE.setdefault(
            round(radius, 2), make_kernel(round(radius, 2))
        )
        ix, iy = int(x), int(y)
        for (dx, dy), kw in zip(offs, wts):
            px_, py_ = ix + dx, iy + dy
            if 0 <= px_ < W and 0 <= py_ < H:
                idx = py_ * W + px_
                buf[idx] = min(1.0, buf[idx] + a * kw * 0.40)

    big = Image.new("L", (W, H))
    big.putdata([int(v * 255) for v in buf])
    small = big.resize((w, h), Image.LANCZOS)
    out = Image.new("RGBA", (w, h), (255, 255, 255, 0))
    out.putalpha(small)
    px = out.load()
    for y in range(h):
        for x in range(w):
            _, _, _, a = px[x, y]
            px[x, y] = (255, 255, 255, a)
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default=None, help="output directory")
    ap.add_argument("--scale", type=int, default=3, help="supersampling factor")
    args = ap.parse_args()

    here = Path(__file__).resolve()
    repo = here.parents[2]
    out = Path(args.out) if args.out else repo / "docs/design/direction-concepts/assets"
    out.mkdir(parents=True, exist_ok=True)

    for name, w, h, seed, pen in VARIANTS:
        for mult, suffix in ((1, ""), (2, "@2x")):
            W, H, P = w * mult, h * mult, pen * mult
            if name.startswith("strike"):
                img = draw_strike(W, H, seed, args.scale, P)
            else:
                img = draw_ring(W, H, seed, args.scale, P)
            path = out / f"{name}{suffix}.png"
            img.save(path)
            print(f"  {path.name}  {W}x{H}")

    print(f"\nwrote {len(VARIANTS) * 2} files to {out}")


if __name__ == "__main__":
    main()
