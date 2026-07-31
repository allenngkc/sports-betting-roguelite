import React from "react";

/* Old Slips / Ledger: the read-only settled record, in the same form grammar as MY BETS but with no
   live styling, no action buttons, and no watch-the-TV instruction. Terminal states are literal words. */
export function LedgerEntry({ number, legs, terminal, stake, payout, style, ...rest }) {
  const won = terminal === "WON" || terminal === "CASHED OUT";
  const key = { fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)", whiteSpace: "nowrap" };
  return (
    <div {...rest} style={{
      display: "flex", alignItems: "center", gap: "16px", padding: "11px var(--st-pad-x)",
      borderBottom: "var(--rule-w) solid var(--rule-soft)", fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{
        width: "112px", flex: "none", fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
        color: "var(--toner-2)", textTransform: "uppercase", whiteSpace: "nowrap", fontVariantNumeric: "tabular-nums"
      }}>{number}</div>
      <div style={{ flex: 1, minWidth: 0, fontSize: "var(--st-size-fact)", color: "var(--toner-2)", letterSpacing: ".02em" }}>{legs}</div>
      <div style={{ width: "96px" }}>
        <div style={key}>STAKE</div>
        <div style={{ fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)", color: "var(--toner)", fontVariantNumeric: "tabular-nums" }}>{stake}</div>
      </div>
      <div style={{ width: "104px" }}>
        <div style={key}>RETURNED</div>
        <div style={{
          fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
          color: won ? "var(--wax)" : "var(--toner-3)", fontVariantNumeric: "tabular-nums"
        }}>{payout}</div>
      </div>
      <div style={{
        width: "104px", flex: "none", textAlign: "right", whiteSpace: "nowrap", fontFamily: "var(--font-cond)",
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-action)",
        color: won ? "var(--wax)" : "var(--toner-3)",
        textDecoration: won ? "none" : "line-through",
        textDecorationColor: "var(--stamp)"
      }}>{terminal}</div>
    </div>
  );
}
