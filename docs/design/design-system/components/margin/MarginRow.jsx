import React from "react";

/* A key/value line in the margin — COMBINED and anything like it. Not money: no wax. */
export function MarginRow({ label, value, tone = "toner", style, ...rest }) {
  return (
    <div {...rest} style={{
      display: "flex", alignItems: "baseline", padding: "9px 0",
      borderBottom: "var(--rule-w) solid var(--rule)", fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)", whiteSpace: "nowrap"
      }}>{label}</div>
      <div style={{
        marginLeft: "auto", fontFamily: "var(--font-cond)", fontSize: "var(--st-size-row)",
        color: tone === "wax" ? "var(--wax)" : "var(--toner)", fontVariantNumeric: "tabular-nums"
      }}>{value}</div>
    </div>
  );
}
