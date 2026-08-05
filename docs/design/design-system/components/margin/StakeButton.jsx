import React from "react";

/* Two stake controls, two shapes. Quick fractions are ruled and transparent; nudge keys are raised
   chrome, because they are keys on his own machine. Both hover to biro: he chose them. */
export function StakeButton({ label, variant = "quick", onClick, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  const quick = variant === "quick";
  return (
    <button type="button" onClick={onClick} {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        width: quick ? "var(--st-quick-w)" : "var(--st-nudge-w)",
        height: quick ? "var(--st-quick-h)" : "var(--st-nudge-h)",
        background: quick ? "transparent" : "var(--ground-3)",
        border: "var(--rule-w) solid " + (hover ? "var(--biro)" : "var(--rule)"),
        borderRadius: "var(--radius)",
        color: hover ? "var(--biro)" : quick ? "var(--toner-2)" : "var(--toner)",
        fontFamily: quick ? "var(--font-data)" : "var(--font-cond)",
        fontSize: quick ? "var(--st-size-fact)" : "15px",
        letterSpacing: quick ? ".03em" : "0", cursor: "pointer",
        fontVariantNumeric: "tabular-nums", ...style
      }}>{label}</button>
  );
}
