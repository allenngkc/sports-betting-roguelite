import React from "react";
import { tier } from "./tiers.js";

/* Compact scorebug at the top of the right region. Team names in their hues either side of white
   tabular figures, clock at the far right, ticket/leg index at L1. Records do not appear live.
   The score is the largest element on the surface at all times. Nothing outgrows it.

   §3 names the score at a goal as one of the surface's L4 occupants, so `goal` punches it to full
   brightness for that callback. Per the C3 coverage ruling (2026-07-31) a momentary punch transiently
   preempts a sustained L4: at a goal the cash-out is legitimately re-pricing and sits at L3, so the
   one-L4-at-any-instant invariant holds without contrivance. */
export function TvScorebug({
  ticket, leg, away, home, score = [0, 0], clock = "PRE", backed = null, marketPick = false,
  goal = false, style, ...rest
}) {
  const name = (t, hue, isBacked) => (
    <div style={{ display: "flex", alignItems: "center", gap: "8px", minWidth: 0 }}>
      <span style={{
        fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-team)", color: hue,
        opacity: tier("L3"), textTransform: "uppercase", letterSpacing: "var(--tv-track-name)", whiteSpace: "nowrap"
      }}>{t}</span>
      {isBacked && (
        <span style={{
          fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-eyebrow)", color: "var(--tv-fact)",
          opacity: tier("L2"), letterSpacing: "var(--tv-track-label)", border: "var(--tv-rule-w) solid currentColor", padding: "0 5px"
        }}>BACKED</span>
      )}
    </div>
  );
  return (
    <div {...rest} style={{
      display: "flex", alignItems: "center", gap: "var(--tv-gutter)", padding: "10px 16px",
      borderBottom: "var(--tv-rule-w) solid var(--tv-rule)", fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums", ...style
    }}>
      <div style={{
        fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
        color: "var(--tv-structure)", opacity: tier("L1"), whiteSpace: "nowrap"
      }}>TICKET {ticket} • LEG {leg}</div>
      <div style={{ marginLeft: "auto", display: "flex", alignItems: "center", gap: "18px" }}>
        {name(away, "var(--tv-team-a)", backed === "away")}
        <div style={{
          fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-score)", fontWeight: 700,
          color: "var(--tv-fact)", opacity: goal ? tier("L4") : tier("L3"),
          transition: "opacity var(--tv-dur-punch) var(--tv-step)",
          letterSpacing: ".02em", whiteSpace: "nowrap"
        }}>{score[0]} — {score[1]}</div>
        {name(home, "var(--tv-team-b)", backed === "home")}
      </div>
      <div style={{
        marginLeft: "auto", fontSize: "var(--tv-size-clock)", fontWeight: 700,
        color: "var(--tv-fact)", opacity: tier("L3")
      }}>{clock}</div>
      {marketPick && (
        <div style={{
          fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
          color: "var(--tv-context)", opacity: tier("L2"), whiteSpace: "nowrap"
        }}>MARKET PICK</div>
      )}
    </div>
  );
}
