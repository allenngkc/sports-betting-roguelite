import React from "react";

/* The one loud figure in the margin. 31px wax with the hand-laid highlight: a 6px amber band at
   0.26 opacity, rotated -0.5deg, behind the figure. Bank and target do not get it, because only one
   figure at a time may be the loudest thing on the surface. */
export function PayoutFigure({ label = "POTENTIAL PAYOUT", value, highlight = true, style, ...rest }) {
  return (
    <div {...rest} style={{ fontFamily: "var(--font-data)", ...style }}>
      <div style={{
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)", whiteSpace: "nowrap"
      }}>{label}</div>
      <div style={{
        position: "relative", display: "inline-block", marginTop: "2px",
        fontFamily: "var(--font-cond)", fontSize: "var(--st-size-payout)", color: "var(--wax)",
        lineHeight: "var(--st-lh-fig)", fontVariantNumeric: "tabular-nums"
      }}>
        {value}
        {highlight && (
          <span aria-hidden="true" style={{
            position: "absolute", left: "-3px", right: "-5px", bottom: "-2px",
            height: "var(--wax-highlight-h)", background: "var(--wax)",
            opacity: "var(--wax-highlight-opacity)", transform: "rotate(var(--wax-highlight-rotate))"
          }} />
        )}
      </div>
    </div>
  );
}
