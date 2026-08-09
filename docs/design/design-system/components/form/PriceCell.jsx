import React from "react";
import { InkMark } from "./InkMark.jsx";

/* A price is a printed figure. Choosing it means CIRCLING it. Selection is never a pill or a fill.
   REPLACE, NEVER BLOCK: picking the other side of a matchup that already carries a selection
   replaces it. v0 has no limiting, no padlock, no disabled odds, no suspension. */
export function PriceCell({
  price, state = "default", ringVariant = "ring-a", size = "kit", inkBase,
  onSelect, title, style, ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const cellW = size === "runtime" ? 112 : 96;
  const cellH = size === "runtime" ? 32 : 30;
  const ring = InkMark.rect(cellW, cellH);

  const figure = state === "won" ? "var(--wax)"
    : state === "dead" ? "var(--toner-3)"
    : state === "replace" ? (hover ? "var(--toner)" : "var(--toner-2)")
    : hover ? "var(--wax-lit)" : "var(--toner)";

  const mark = state === "picked" ? { variant: ringVariant, color: "var(--biro)" }
    : state === "won" ? { variant: ringVariant, color: "var(--wax)" }
    : state === "dead" ? { variant: "strike", color: "var(--stamp)" }
    : null;

  return (
    <button type="button" onClick={onSelect} title={title}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      aria-pressed={state === "picked"} {...rest}
      style={{
        position: "relative", width: cellW + "px", height: Math.max(cellH, 32) + "px",
        display: "flex", alignItems: "center", justifyContent: "center",
        background: "transparent", border: 0, padding: 0, borderRadius: "var(--radius)",
        cursor: "pointer", font: "inherit", fontFamily: "var(--font-cond)",
        fontSize: "var(--st-size-price)", letterSpacing: "var(--st-track-name)",
        fontVariantNumeric: "tabular-nums", color: figure, ...style
      }}>
      {state === "replace" ? (
        <span style={{ display: "inline-flex", alignItems: "baseline", gap: "5px" }}>
          <span style={{ fontFamily: "var(--font-data)", fontSize: "15px", color: "var(--biro)" }}>⇄</span>
          <u style={{
            textDecoration: "underline", textDecorationStyle: "dashed",
            textDecorationColor: "var(--biro-deep)", textUnderlineOffset: "5px"
          }}>{price}</u>
        </span>
      ) : price}
      {mark && (
        <InkMark variant={mark.variant} color={mark.color} base={inkBase}
          width={ring.width} height={ring.height}
          style={{ position: "absolute", left: ring.left + "px", top: "50%", transform: "translateY(-50%)" }} />
      )}
    </button>
  );
}
