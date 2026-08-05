import React from "react";

/* The house stamps a blocked action. Cause AND remedy, in place, at the 13px fact floor, inside the
   house's own oxide border. Never a tooltip, never colour alone, never below the floor. */
export function StampReason({ reason, style, ...rest }) {
  return (
    <small {...rest} style={{
      fontSize: "var(--st-size-fact)", letterSpacing: ".04em", color: "var(--stamp)",
      border: "var(--rule-w) solid var(--stamp)", padding: "1px 7px",
      fontFamily: "var(--font-data)", whiteSpace: "nowrap", ...style
    }}>{reason}</small>
  );
}
