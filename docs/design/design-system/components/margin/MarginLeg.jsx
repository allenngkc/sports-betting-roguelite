import React from "react";
import { RubOutButton } from "./RubOutButton.jsx";

/* One explicit leg per selection: his blue check, the identity, the price, and an explicit RUB OUT. */
export function MarginLeg({ team, price, market, entry, onRemove, style, ...rest }) {
  return (
    <div {...rest} style={{
      display: "flex", alignItems: "center", gap: "9px", padding: "8px 0",
      borderBottom: "var(--rule-w) dotted var(--rule)", fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{ width: "15px", flex: "none", color: "var(--biro)", fontSize: "15px", lineHeight: 1 }}>✓</div>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: "flex", alignItems: "baseline", gap: "7px" }}>
          <div style={{
            fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
            color: "var(--toner)", textTransform: "uppercase"
          }}>{team}</div>
          <div style={{
            marginLeft: "auto", fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
            color: "var(--toner)", fontVariantNumeric: "tabular-nums"
          }}>{price}</div>
        </div>
        <div style={{
          fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-rec)", whiteSpace: "nowrap",
          color: "var(--toner-3)", marginTop: "3px"
        }}>{market}{entry ? " · ENTRY " + entry : ""}</div>
      </div>
      <RubOutButton onClick={onRemove} />
    </div>
  );
}
