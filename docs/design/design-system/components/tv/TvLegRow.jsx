import React from "react";
import { tier } from "./tiers.js";

/* One row per leg, in ticket order, brightness carrying the state. Multiple legs can be live at once:
   L3 is a tier, not a slot. Live rows expand in place; rows below are pushed, never reordered.
   Resolved and pending rows compress to a single line.

   The headline is ALWAYS the market statement, in every state — VISUAL-DESIGN §6 prints it verbatim
   ("NORTHSIDE TO WIN", "OVER 2.5 GOALS", "MARCUS VALE TO SCORE") and §3 calls that line the active
   NEED statement, so the two are one slot, not two. It may wrap to two lines; nothing else in the
   rail wraps. Never paraphrase it: it carries the team name, which §4 names as the primary carrier
   of team identity, and the market's own line value. */
const LEVEL = { NEXT: "L1", LIVE: "L3", W: "L3", L: "L0", VOID: "L2" };

export function TvLegRow({
  market, price, statement, state = "NEXT", progress, expanded, style, ...rest
}) {
  const level = LEVEL[state] || "L2";
  const hue = state === "W" ? "var(--tv-gold)" : state === "VOID" ? "var(--tv-void)" : "var(--tv-fact)";
  const open = expanded == null ? state === "LIVE" : expanded;
  const dead = state === "L";
  const dim = dead ? tier("L1") : tier(level);
  const meta = {
    fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-eyebrow)", letterSpacing: ".1em",
    color: "var(--tv-context)", whiteSpace: "nowrap", opacity: dead ? tier("L1") : tier("L2")
  };
  const stateChip = {
    fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-eyebrow)", letterSpacing: ".1em",
    color: hue, whiteSpace: "nowrap", opacity: dim,
    textDecoration: state === "VOID" ? "line-through" : "none", minWidth: "38px", textAlign: "right"
  };
  const shell = {
    padding: open ? "10px 16px" : "7px 16px",
    borderBottom: "var(--tv-rule-w) solid var(--tv-rule)",
    fontFamily: "var(--font-tv-cond)", fontVariantNumeric: "tabular-nums",
    background: dead ? "var(--tv-extinguished)" : "transparent", ...style
  };

  /* A resolved or pending leg is ONE line: eyebrow, statement, price, state. Vertical budget goes to
     what is live, and a won leg does not need the same height as a leg still in play. */
  if (!open) {
    return (
      <div {...rest} style={shell}>
        <div style={{ display: "flex", alignItems: "baseline", gap: "8px" }}>
          <span style={meta}>{market}</span>
          <span style={{
            flex: 1, minWidth: 0, fontSize: "var(--tv-size-leg)", fontWeight: 700, color: hue,
            opacity: dim, textTransform: "uppercase", letterSpacing: "var(--tv-track-name)",
            whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis"
          }}>{statement}</span>
          <span style={{ ...meta, fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-leg)" }}>{price}</span>
          <span style={stateChip}>{state}</span>
        </div>
      </div>
    );
  }

  return (
    <div {...rest} style={shell}>
      <div style={{ display: "flex", alignItems: "baseline", gap: "8px" }}>
        <span style={meta}>{market}</span>
        <span style={{ ...meta, marginLeft: "auto", fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-leg)" }}>{price}</span>
        <span style={stateChip}>{state}</span>
      </div>
      <div style={{
        fontSize: "var(--tv-size-need)", fontWeight: 700, color: hue, opacity: dim, marginTop: "4px",
        textTransform: "uppercase", letterSpacing: "var(--tv-track-name)", lineHeight: 1.08
      }}>{statement}</div>
      {progress && (
        <div style={{
          fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-progress)", fontWeight: 700,
          color: "var(--tv-fact)", opacity: tier("L3"), marginTop: "4px",
          letterSpacing: ".02em", whiteSpace: "nowrap", lineHeight: 1.1
        }}>{progress}</div>
      )}
    </div>
  );
}
