import React from "react";

/* Entry to event detail — Goals, BTTS, Corners, Cards, Players. Rectangular, ruled, literal. */
export function MoreButton({ label = "MORE", onClick, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  return (
    <button type="button" onClick={onClick} {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        width: "var(--st-more-w)", height: "var(--st-more-h)", background: "transparent",
        border: "var(--rule-w) solid " + (hover ? "var(--toner-3)" : "var(--rule)"),
        borderRadius: "var(--radius)", color: hover ? "var(--toner)" : "var(--toner-2)",
        cursor: "pointer", fontFamily: "var(--font-data)", fontSize: "var(--st-size-fact)",
        letterSpacing: ".05em", display: "flex", alignItems: "center",
        justifyContent: "center", gap: "4px", ...style
      }}>
      {label}<i style={{ fontStyle: "normal", fontSize: "15px", lineHeight: 1 }}>›</i>
    </button>
  );
}
