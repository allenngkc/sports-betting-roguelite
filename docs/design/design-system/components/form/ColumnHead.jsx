import React from "react";

/* 26px recessed band above the six entries. Column heads state product facts: 13px floor. */
export function ColumnHead({
  no = "NO.", matchup = "MATCHUP · SEASON RECORD", price = "MONEYLINE", more = "MORE", style, ...rest
}) {
  const cap = { fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)" };
  return (
    <div {...rest} style={{
      height: "var(--st-colhead-h)", display: "flex", alignItems: "center",
      padding: "0 var(--st-pad-x)", background: "var(--ground-2)",
      borderBottom: "var(--rule-w) solid var(--rule)",
      fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{ ...cap, width: "var(--st-col-no)" }}>{no}</div>
      <div style={{ ...cap, flex: 1 }}>{matchup}</div>
      <div style={{ ...cap, width: "var(--st-col-price)", textAlign: "center" }}>{price}</div>
      <div style={{ ...cap, width: "var(--st-col-more)", textAlign: "right" }}>{more}</div>
    </div>
  );
}
