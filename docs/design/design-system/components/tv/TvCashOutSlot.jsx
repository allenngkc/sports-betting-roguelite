import React from "react";
import { tier } from "./tiers.js";

/* One fixed rectangle owning all six states. It never reflows: reserved space stays reserved and
   simply goes dark. THE BRIGHTNESS OF THIS SLOT IS A PROMISE ABOUT INPUT — L4 means the key works
   right now. If the slot is bright and the press does nothing, the surface has lied. */
export function TvCashOutSlot({ state = "actionable", amount, keyHint = "[E]", style, ...rest }) {
  const inverted = state === "actionable";
  const money =
    state === "actionable" ? "CASH OUT " + amount
    : state === "updating" ? "CASH OUT " + amount
    : state === "accepted" ? "CASHED OUT " + amount
    : null;
  /* The status word rides at label scale beside the figure, never at money scale — the rectangle is
     fixed and its copy is ONE line. Suspended and unavailable carry no amount, so their whole copy
     sits at label scale: L1, quiet, no reflow, explaining an absence. */
  const status =
    state === "actionable" ? keyHint
    : state === "updating" ? "UPDATING"
    : state === "suspended" || state === "pending" ? "MARKET SUSPENDED"
    : state === "accepted" ? null
    : "CASH OUT UNAVAILABLE";
  const level = inverted ? "L4" : state === "updating" || state === "accepted" ? "L3" : "L1";
  const hue = inverted ? "var(--tv-gold-ink)"
    : state === "updating" || state === "accepted" ? "var(--tv-gold)" : "var(--tv-context)";
  return (
    <div {...rest} style={{
      height: "58px", flex: "none", display: "flex", alignItems: "center", justifyContent: "center",
      gap: "10px", overflow: "hidden", padding: "0 12px",
      background: inverted ? "var(--tv-gold)" : "transparent",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)",
      opacity: inverted ? 1 : tier(level) + 0.3,
      fontFamily: "var(--font-tv-cond)", fontVariantNumeric: "tabular-nums", ...style
    }}>
      {money && (
        <span style={{
          fontSize: "var(--tv-size-cashout)", fontWeight: 700, letterSpacing: "var(--tv-track-name)",
          color: hue, textTransform: "uppercase", whiteSpace: "nowrap", lineHeight: 1
        }}>{money}</span>
      )}
      {status && (
        <span style={{
          fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-eyebrow)", fontWeight: 500,
          letterSpacing: "var(--tv-track-label)", color: hue, textTransform: "uppercase",
          whiteSpace: "nowrap", lineHeight: 1
        }}>{status}</span>
      )}
    </div>
  );
}
