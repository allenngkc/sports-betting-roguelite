import React from "react";
import { StakeButton } from "./StakeButton.jsx";

/* Stake figure plus its controls. Display stake and payout together after every input. */
export function StakeControls({
  label = "STAKE", stake, fractions = ["10%", "25%", "50%", "MAX"],
  nudges = ["− $10", "+ $10"], onFraction, onNudge, style, ...rest
}) {
  return (
    <div {...rest} style={{ padding: "10px 0 0", fontFamily: "var(--font-data)", ...style }}>
      <div style={{ display: "flex", alignItems: "baseline" }}>
        <div style={{
          fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)"
        }}>{label}</div>
        <div style={{
          marginLeft: "auto", fontFamily: "var(--font-cond)", fontSize: "var(--st-size-stake)",
          color: "var(--toner)", lineHeight: "var(--st-lh-tight)", fontVariantNumeric: "tabular-nums"
        }}>{stake}</div>
      </div>
      <div style={{ display: "flex", gap: "4px", marginTop: "8px" }}>
        {fractions.map((f) => (
          <StakeButton key={f} label={f} onClick={onFraction && (() => onFraction(f))} style={{ flex: 1, width: "auto" }} />
        ))}
      </div>
      <div style={{ display: "flex", gap: "4px", marginTop: "4px" }}>
        {nudges.map((n) => (
          <StakeButton key={n} label={n} variant="nudge" onClick={onNudge && (() => onNudge(n))} style={{ flex: 1, width: "auto" }} />
        ))}
      </div>
    </div>
  );
}
