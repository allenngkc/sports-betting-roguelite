import React from "react";

/* An explicit 60x32 removal target, never a tiny unlabeled x. Mis-clicks cost money here.
   RUB OUT is an ACTION LABEL, so it is set in the condensed face — the type contract assigns the
   condensed production face to the masthead, figures, prices, team names and action labels, and the
   data face to running text and secondary labels. 60x32 is a locked element-kit value; the label is
   made to fit the control, never the control widened to fit the label. */
export function RubOutButton({ label = "RUB OUT", onClick, style, ...rest }) {
  const [hover, setHover] = React.useState(false);
  return (
    <button type="button" onClick={onClick} {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      style={{
        width: "var(--st-rub-w)", height: "var(--st-rub-h)", flex: "none", background: "transparent",
        border: "var(--rule-w) solid " + (hover ? "var(--stamp)" : "var(--rule)"),
        borderRadius: "var(--radius)", color: hover ? "var(--stamp)" : "var(--toner-3)",
        fontSize: "var(--st-size-fact)", letterSpacing: ".06em", cursor: "pointer",
        fontFamily: "var(--font-cond)", whiteSpace: "nowrap", padding: 0, ...style
      }}>{label}</button>
  );
}
