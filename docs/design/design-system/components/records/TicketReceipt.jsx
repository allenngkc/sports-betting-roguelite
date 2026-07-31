import React from "react";
import { RevealedState } from "./RevealedState.jsx";

/* A placed ticket, printed as a numbered form receipt. It renders Run.Tickets and invents nothing —
   no date, no settlement, no outcome that the source did not supply. */
export function TicketReceipt({
  number, legs = [], stake, combined, payout, state, inkBase, style, ...rest
}) {
  const key = { fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)", whiteSpace: "nowrap" };
  const val = { fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)", color: "var(--toner)", fontVariantNumeric: "tabular-nums" };
  return (
    <div {...rest} style={{
      border: "var(--rule-w) solid var(--rule)", background: "var(--ground-2)",
      padding: "11px 13px", fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{
        display: "flex", alignItems: "baseline", gap: "10px", paddingBottom: "8px",
        borderBottom: "var(--rule-w) solid var(--rule)"
      }}>
        <div style={{
          fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
          letterSpacing: "var(--st-track-action)", color: "var(--toner)", textTransform: "uppercase"
        }}>{number}</div>
        <div style={{ ...key, marginLeft: "auto" }}>{legs.length} {legs.length === 1 ? "LEG" : "LEGS"}</div>
        {state && <RevealedState state={state} inkBase={inkBase} style={{ marginLeft: "6px" }} />}
      </div>
      {legs.map((l, i) => (
        <div key={i} style={{
          display: "flex", alignItems: "baseline", gap: "8px", padding: "6px 0",
          borderBottom: i === legs.length - 1 ? "0" : "var(--rule-w) dotted var(--rule-soft)"
        }}>
          <div style={{ fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)", color: "var(--toner)", textTransform: "uppercase" }}>{l.team}</div>
          <div style={{ ...key, letterSpacing: "var(--st-track-rec)" }}>{l.market}</div>
          <div style={{ ...val, marginLeft: "auto" }}>{l.price}</div>
        </div>
      ))}
      <div style={{ display: "flex", gap: "18px", marginTop: "9px", paddingTop: "9px", borderTop: "var(--rule-w) solid var(--rule)" }}>
        <div><div style={key}>STAKE</div><div style={val}>{stake}</div></div>
        <div><div style={key}>COMBINED</div><div style={val}>{combined}</div></div>
        <div style={{ marginLeft: "auto", textAlign: "right" }}>
          <div style={key}>PAYS</div>
          <div style={{ ...val, color: "var(--wax)" }}>{payout}</div>
        </div>
      </div>
    </div>
  );
}
