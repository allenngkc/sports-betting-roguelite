import React from "react";

/* Bank, target, relics, tickets. Persistent, literal, and legible at the 50% thumbnail check.
   Money wears the wax pencil; counts do not. */
export function RunFigure({ label, value, tone = "toner", size, style, ...rest }) {
  return (
    <div {...rest} style={{ display: "flex", flexDirection: "column", gap: "3px", fontFamily: "var(--font-data)", ...style }}>
      <div style={{
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)", whiteSpace: "nowrap"
      }}>{label}</div>
      <div style={{
        fontFamily: "var(--font-cond)", fontSize: size || "var(--st-size-figure)",
        letterSpacing: ".02em", color: tone === "wax" ? "var(--wax)" : "var(--toner)",
        lineHeight: "var(--st-lh-tight)", fontVariantNumeric: "tabular-nums"
      }}>{value}</div>
    </div>
  );
}
