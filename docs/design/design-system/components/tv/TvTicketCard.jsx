import React from "react";
import { tier } from "./tiers.js";

/* The ticket interstitial. The stage and active-leg card clear before it appears, and no score,
   clock, tape, event line, suspended label or prior offer remains. */
export function TvTicketCard({ heading, legs = [], risk, pays, style, ...rest }) {
  return (
    <div {...rest} style={{
      position: "absolute", inset: 0, background: "var(--tv-substrate)", display: "flex",
      flexDirection: "column", alignItems: "center", justifyContent: "center", gap: "26px",
      padding: "0 48px", textAlign: "center", fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums", ...style
    }}>
      <div style={{
        fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-score)", fontWeight: 700,
        letterSpacing: "var(--tv-track-name)", color: "var(--tv-fact)", opacity: tier("L3"),
        textTransform: "uppercase"
      }}>{heading}</div>
      <div style={{
        fontSize: "var(--tv-size-leg)", letterSpacing: "var(--tv-track-name)",
        color: "var(--tv-fact)", opacity: tier("L2"), textTransform: "uppercase", lineHeight: 1.6
      }}>{legs.join("  •  ")}</div>
      <div style={{ display: "flex", gap: "72px", borderTop: "var(--tv-rule-w) solid var(--tv-rule)", paddingTop: "18px" }}>
        {[["RISK", risk], ["PAYS", pays]].map(([k, v]) => (
          <div key={k}>
            <div style={{
              fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
              color: "var(--tv-context)", opacity: tier("L2")
            }}>{k}</div>
            <div style={{
              fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-risk)", fontWeight: 700,
              color: "var(--tv-gold)", opacity: tier("L3")
            }}>{v}</div>
          </div>
        ))}
      </div>
    </div>
  );
}
