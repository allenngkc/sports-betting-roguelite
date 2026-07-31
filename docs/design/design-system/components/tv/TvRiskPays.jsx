import React from "react";
import { tier } from "./tiers.js";

/* One risk figure and one payout figure, at the foot of the ticket column, in gold at L2.
   PRD 8.4: ticket-level, never per leg — the approved concept render got this wrong. */
export function TvRiskPays({ risk, pays, style, ...rest }) {
  const cell = (k, v) => (
    <div>
      <div style={{
        fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-eyebrow)",
        letterSpacing: "var(--tv-track-label)", color: "var(--tv-context)", opacity: tier("L2")
      }}>{k}</div>
      <div style={{
        fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-risk)", fontWeight: 700,
        color: "var(--tv-gold)", opacity: tier("L2"), fontVariantNumeric: "tabular-nums"
      }}>{v}</div>
    </div>
  );
  return (
    <div {...rest} style={{
      display: "flex", justifyContent: "space-between", padding: "12px 16px",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)", ...style
    }}>{cell("RISK", risk)}{cell("PAYS", pays)}</div>
  );
}
