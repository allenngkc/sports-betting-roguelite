import React from "react";
import { RevealedState } from "./RevealedState.jsx";

/* A leg row in the MY BETS mirror. Read-only. Live legs keep document/biro treatment; a dead entry
   dims toward the ground. No score, clock, probability, next event, or unrevealed result. */
export function RevealedLeg({ team, price, market, state = "PENDING", inkBase, style, ...rest }) {
  const dead = state === "DEAD";
  return (
    <div {...rest} style={{
      display: "flex", alignItems: "center", gap: "12px", padding: "11px 0",
      borderBottom: "var(--rule-w) solid var(--rule-soft)", opacity: dead ? .55 : 1,
      fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{ width: "15px", flex: "none", color: "var(--biro)", fontSize: "15px", lineHeight: 1 }}>✓</div>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{
          fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
          color: state === "GREEN" ? "var(--wax)" : dead ? "var(--toner-3)" : "var(--toner)",
          textTransform: "uppercase"
        }}>{team}</div>
        <div style={{
          fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-rec)", whiteSpace: "nowrap",
          color: "var(--toner-3)", marginTop: "3px"
        }}>{market}</div>
      </div>
      <div style={{
        fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)", color: "var(--toner-2)",
        fontVariantNumeric: "tabular-nums", width: "56px", textAlign: "right"
      }}>{price}</div>
      <div style={{ width: "116px", textAlign: "right" }}>
        <RevealedState state={state} inkBase={inkBase} />
      </div>
    </div>
  );
}
