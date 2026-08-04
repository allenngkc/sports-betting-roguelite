import React from "react";

/* The one solid wax field on the surface. Stages a valid working slip. */
export function PlaceAction({ label = "Place Ticket", disabled = false, onClick, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  const [down, setDown] = React.useState(false);
  return (
    <button type="button" onClick={onClick} disabled={disabled} {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => { setHover(false); setDown(false); }}
      onMouseDown={() => setDown(true)} onMouseUp={() => setDown(false)}
      style={{
        height: "var(--st-place-h)", minWidth: "var(--st-place-min-w)", padding: "0 22px",
        border: 0, borderRadius: "var(--radius)", cursor: disabled ? "not-allowed" : "pointer",
        background: disabled ? "var(--ground-3)" : hover ? "var(--wax-lit)" : "var(--wax)",
        color: disabled ? "var(--toner-3)" : "var(--wax-ink)",
        fontFamily: "var(--font-cond)", fontSize: "17px",
        letterSpacing: "var(--st-track-action)", textTransform: "uppercase",
        boxShadow: disabled || down ? "none" : "0 var(--st-press-shift) 0 var(--wax-deep)",
        transform: down && !disabled ? "translateY(var(--st-press-shift))" : "none", ...style
      }}>{label}</button>
  );
}
