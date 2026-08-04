import React from "react";

/* Separate secondary action, two presses. Never masquerades as lock. First press arms; second commits
   an empty round. Dashed unarmed, solid stamp armed. */
export function SkipAction({
  label = "SKIP ROUND — PRESS TWICE", armedLabel = "PRESS AGAIN TO SKIP",
  armed: armedProp, onSkip, style, ...rest
}) {
  const [armedState, setArmed] = React.useState(false);
  const armed = armedProp == null ? armedState : armedProp;
  const [hover, setHover] = React.useState(false);
  const lit = armed || hover;
  return (
    <button type="button" {...rest}
      onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
      onClick={() => { if (armed) { setArmed(false); if (onSkip) onSkip(); } else setArmed(true); }}
      style={{
        height: "var(--st-skip-h)", minWidth: "var(--st-skip-min-w)", background: "transparent",
        border: "var(--rule-w) " + (lit ? "solid" : "dashed") + " " + (lit ? "var(--stamp)" : "var(--rule)"),
        borderRadius: "var(--radius)", color: lit ? "var(--stamp)" : "var(--toner-3)",
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-rec)",
        cursor: "pointer", fontFamily: "var(--font-data)", ...style
      }}>{armed ? armedLabel : label}</button>
  );
}
