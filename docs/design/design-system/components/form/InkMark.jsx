import React from "react";

/* The selection ring and the dead-leg strike are real drawn assets: white RGB with all the ink in
   the alpha channel, tinted at runtime exactly as Unity tints them via Image.color. Never a CSS
   border, never a fill, never a hand-drawn approximation. Regenerate with
   `python tools/art/make-biro-rings.py` — a given seed always produces the same mark. */
const FILES = {
  "ring-a": ["ring-price-a@2x.png", 112, 46],
  "ring-b": ["ring-price-b@2x.png", 112, 46],
  "ring-c": ["ring-price-c@2x.png", 112, 46],
  "ring-wide-a": ["ring-wide-a@2x.png", 176, 46],
  "ring-wide-b": ["ring-wide-b@2x.png", 176, 46],
  "strike": ["strike-a@2x.png", 112, 46]
};

export function InkMark({ variant = "ring-a", color = "var(--biro)", width, height, base, style, ...rest }) {
  const spec = FILES[variant] || FILES["ring-a"];
  const root = base || (typeof window !== "undefined" && window.SURETHING_INK_BASE) || "assets/ink/";
  const w = width == null ? spec[1] : width;
  const h = height == null ? spec[2] : height;
  const url = "url(" + root + spec[0] + ")";
  return (
    <span aria-hidden="true" {...rest} style={{
      display: "block", width: w + "px", height: h + "px", pointerEvents: "none",
      backgroundColor: color, WebkitMaskImage: url, maskImage: url,
      WebkitMaskRepeat: "no-repeat", maskRepeat: "no-repeat",
      WebkitMaskSize: w + "px " + h + "px", maskSize: w + "px " + h + "px", ...style
    }} />
  );
}

/* ring = cell + 16px on both axes, offset -8/-8. Additive, never proportional: the pen's overshoot
   needs somewhere to go. Apply to whatever the real control is — do not assume 96x30. */
InkMark.rect = (cellW, cellH) => ({ width: cellW + 16, height: cellH + 16, left: -8, top: -8 });

/* Deterministic per matchup index. Never randomise per frame or per canvas rebuild — the board
   would visibly redraw itself every time the player nudged a stake. */
InkMark.variantFor = (i, wide) => wide
  ? ["ring-wide-a", "ring-wide-b"][i % 2]
  : ["ring-a", "ring-b", "ring-c"][i % 3];
