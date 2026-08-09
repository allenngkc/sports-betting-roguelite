import React from "react";
import { tier } from "./tiers.js";

/* RULED IN 2026-07-31 (DD, T16). The tape stays because PRD §4.2 names it in the one-revealed-
   source-of-truth law — it is a revealed channel, and dropping it would silently narrow that law.
   The win-probability NUMERAL is ruled out separately; this tape carries no numerals, and the moment
   it needs one it has become that banned readout.

   It sits at the foot of the scorebug, spanning the stage: the scorebug is where match truth lives,
   and momentum is match truth over time. It renders at L1-L2 and is colourless — white and grey only
   — so it can never compete with the score above it or the NEED line beside it.

   Tiering, per §3: the eyebrow is a LABEL, and labels are L2 ("context that must be readable but is
   not the subject"). L1 is the dormant tier — structure and the not-yet — and a label sitting there
   composites to 1.17:1 on this substrate, which is not readable at four metres in a dark room. The
   bars are the exception that proves it: history is genuinely past, so it sits at L1 as structure,
   and only the current sample rises to L2. */
export function TvMomentumTape({ samples = [], label = "MOMENTUM", style, ...rest }) {
  const last = samples.length - 1;
  return (
    <div {...rest} style={{
      height: "28px", display: "flex", alignItems: "center", gap: "10px", padding: "0 16px",
      borderBottom: "var(--tv-rule-w) solid var(--tv-rule)", fontFamily: "var(--font-tv)", ...style
    }}>
      <span style={{
        fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
        color: "var(--tv-context)", opacity: tier("L2"), whiteSpace: "nowrap"
      }}>{label}</span>
      <div style={{ flex: 1, position: "relative", height: "16px", display: "flex", alignItems: "center", gap: "2px" }}>
        <div style={{
          position: "absolute", left: 0, right: 0, top: "50%", height: "1px",
          background: "var(--tv-structure)", opacity: tier("L1")
        }} />
        {samples.map((s, i) => {
          const v = Math.max(-1, Math.min(1, s));
          const h = Math.max(2, Math.round(Math.abs(v) * 7));
          return (
            <div key={i} style={{
              position: "relative", flex: 1, height: "16px", display: "flex",
              alignItems: v >= 0 ? "flex-start" : "flex-end"
            }}>
              <div style={{
                width: "100%", height: h + "px", marginTop: v >= 0 ? (8 - h) + "px" : 0,
                marginBottom: v < 0 ? (8 - h) + "px" : 0,
                background: i === last ? "var(--tv-fact)" : "var(--tv-context)",
                opacity: i === last ? tier("L2") : tier("L1")
              }} />
            </div>
          );
        })}
      </div>
    </div>
  );
}
