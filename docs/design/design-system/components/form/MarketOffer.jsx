import React from "react";
import { InkMark } from "./InkMark.jsx";

/* Event-detail market row: 160x30 offer cell, 176x46 wide ring. Reuses the lobby's price grammar,
   so a selection from any destination replaces that matchup's existing selection. */
export function MarketOffer({
  line, price, state = "default", ringVariant = "ring-wide-a", onSelect, inkBase, style, ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const ring = InkMark.rect(160, 30);
  const figure = state === "picked" ? "var(--toner)"
    : state === "replace" ? (hover ? "var(--toner)" : "var(--toner-2)")
    : hover ? "var(--wax-lit)" : "var(--toner)";
  return (
    <button type="button" onClick={onSelect} {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      aria-pressed={state === "picked"}
      style={{
        position: "relative", width: "160px", minHeight: "32px", display: "flex",
        alignItems: "center", justifyContent: "space-between", gap: "12px",
        background: "transparent", border: 0, padding: 0, borderRadius: "var(--radius)",
        cursor: "pointer", fontFamily: "var(--font-data)", color: figure, ...style
      }}>
      <span style={{
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-rec)",
        textTransform: "uppercase", textAlign: "left", whiteSpace: "nowrap"
      }}>{line}</span>
      <span style={{
        fontFamily: "var(--font-cond)", fontSize: "var(--st-size-price)",
        letterSpacing: "var(--st-track-name)", fontVariantNumeric: "tabular-nums",
        textDecoration: state === "replace" ? "underline" : "none",
        textDecorationStyle: "dashed", textDecorationColor: "var(--biro-deep)", textUnderlineOffset: "5px"
      }}>{price}</span>
      {state === "picked" && (
        <InkMark variant={ringVariant} color="var(--biro)" base={inkBase}
          width={ring.width} height={ring.height}
          style={{ position: "absolute", left: ring.left + "px", top: "50%", transform: "translateY(-50%)" }} />
      )}
    </button>
  );
}
