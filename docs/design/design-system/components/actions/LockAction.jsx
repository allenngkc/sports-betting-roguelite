import React from "react";
import { StampReason } from "./StampReason.jsx";

/* Commits the round. A 52px RULED control in both states — not a second solid wax field.
   Disabled while working marks exist or no ticket is staged, and it says why, in place. */
export function LockAction({ label = "Lock It In", disabled = true, reason, onClick, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  return (
    <button type="button" onClick={disabled ? undefined : onClick} disabled={disabled} {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        height: "var(--st-lock-h)", minWidth: "var(--st-lock-min-w)", padding: "0 16px",
        border: disabled
          ? "var(--rule-w) solid var(--rule)"
          : "var(--rule-w-strong) solid " + (hover ? "var(--wax-lit)" : "var(--wax)"),
        borderRadius: "var(--radius)", background: "transparent",
        display: "flex", flexDirection: "column", alignItems: "center", justifyContent: "center",
        gap: "2px", cursor: disabled ? "not-allowed" : "pointer", ...style
      }}>
      <b style={{
        fontFamily: "var(--font-cond)", fontSize: "15px", letterSpacing: "var(--st-track-action)",
        color: disabled ? "var(--toner-3)" : "var(--toner)", fontWeight: 400, textTransform: "uppercase"
      }}>{label}</b>
      {disabled && reason && <StampReason reason={reason} />}
    </button>
  );
}
