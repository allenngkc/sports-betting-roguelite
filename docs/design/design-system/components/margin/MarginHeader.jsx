import React from "react";

/* The margin is his. Its header is biro-ruled, and the selection count is a literal product fact. */
export function MarginHeader({ title = "My Marks", count, style, ...rest }) {
  return (
    <div {...rest} style={{
      padding: "12px 0 9px", borderBottom: "var(--rule-w-strong) solid var(--biro-deep)",
      display: "flex", alignItems: "baseline", fontFamily: "var(--font-data)", ...style
    }}>
      <h2 style={{
        margin: 0, fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
        letterSpacing: "var(--st-track-head)", color: "var(--biro)",
        textTransform: "uppercase", fontWeight: 400, whiteSpace: "nowrap"
      }}>{title}</h2>
      {count != null && (
        <div style={{
          marginLeft: "auto", fontSize: "var(--st-size-fact)",
          letterSpacing: ".1em", color: "var(--toner-2)", whiteSpace: "nowrap"
        }}>{count}</div>
      )}
    </div>
  );
}
