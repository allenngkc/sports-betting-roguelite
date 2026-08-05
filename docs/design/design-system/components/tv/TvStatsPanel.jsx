import React from "react";
import { tier } from "./tiers.js";

/* Opens from the head of the ticket column and FREEZES PLAYBACK. It expands over the ticket column
   and stage without moving either — when it closes, everything beneath is exactly where it was.
   All values are revealed-ledger values only. */
export function TvStatsPanel({ title = "MATCH STATS", away, home, rows = [], onClose, style, ...rest }) {
  return (
    <div {...rest} style={{
      position: "absolute", inset: 0, background: "var(--tv-panel)",
      display: "flex", flexDirection: "column", fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums", ...style
    }}>
      <div style={{
        display: "flex", alignItems: "center", gap: "12px", padding: "12px 16px",
        borderBottom: "var(--tv-rule-w) solid var(--tv-rule)"
      }}>
        <span style={{
          fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
          color: "var(--tv-fact)", opacity: tier("L3")
        }}>{title}</span>
        <span style={{
          fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
          color: "var(--tv-context)", opacity: tier("L2")
        }}>PLAYBACK FROZEN</span>
        {onClose && (
          <button type="button" onClick={onClose} style={{
            marginLeft: "auto", background: "transparent", border: "var(--tv-rule-w) solid var(--tv-rule)",
            color: "var(--tv-context)", fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-eyebrow)",
            letterSpacing: "var(--tv-track-label)", padding: "3px 9px", cursor: "pointer"
          }}>CLOSE</button>
        )}
      </div>
      <div style={{ display: "flex", padding: "8px 16px", gap: "12px" }}>
        <div style={{ flex: 1, fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)", color: "var(--tv-team-a)", opacity: tier("L3") }}>{away}</div>
        <div style={{ width: "160px", textAlign: "center", fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)", color: "var(--tv-context)", opacity: tier("L2") }} />
        <div style={{ flex: 1, textAlign: "right", fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)", color: "var(--tv-team-b)", opacity: tier("L3") }}>{home}</div>
      </div>
      {rows.map((r) => (
        <div key={r.label} style={{
          display: "flex", alignItems: "center", padding: "9px 16px",
          borderTop: "var(--tv-rule-w) solid var(--tv-rule)"
        }}>
          <div style={{ flex: 1, fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-leg)", color: "var(--tv-fact)", opacity: tier("L3") }}>{r.away}</div>
          <div style={{ width: "160px", textAlign: "center", fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)", color: "var(--tv-context)", opacity: tier("L2") }}>{r.label}</div>
          <div style={{ flex: 1, textAlign: "right", fontFamily: "var(--font-tv-cond)", fontSize: "var(--tv-size-leg)", color: "var(--tv-fact)", opacity: tier("L3") }}>{r.home}</div>
        </div>
      ))}
    </div>
  );
}
