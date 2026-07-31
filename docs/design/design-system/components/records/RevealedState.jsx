import React from "react";
import { InkMark } from "../form/InkMark.jsx";

/* Literal revealed-state words. GREEN and DEAD are WORDS, not colour claims — the state survives
   with the colour removed. Only ever sourced from TvSweatScreen.RevealedView, never from the engine.
   The won mark is sized from the word's own measured box plus the 8px-per-edge overshoot, because a
   ring sized to a container lands its widest point on the last letter. */
const TONE = {
  PENDING: "var(--toner-3)", LIVE: "var(--toner)", GREEN: "var(--wax)",
  DEAD: "var(--toner-3)", VOID: "var(--toner-2)", "CASHED OUT": "var(--wax)"
};

export function RevealedState({ state = "PENDING", inkBase, style, ...rest }) {
  const ref = React.useRef(null);
  const [box, setBox] = React.useState(null);
  React.useLayoutEffect(() => {
    if (!ref.current) return;
    const r = ref.current.getBoundingClientRect();
    setBox({ w: Math.round(r.width), h: Math.round(r.height) });
  }, [state]);
  const marked = state === "GREEN" || state === "DEAD";
  const rect = box ? InkMark.rect(box.w, box.h) : null;
  return (
    <span {...rest} style={{ position: "relative", display: "inline-block", fontFamily: "var(--font-data)", ...style }}>
      <span ref={ref} style={{
        fontFamily: "var(--font-cond)", fontSize: "var(--st-size-leg)",
        letterSpacing: "var(--st-track-action)", textTransform: "uppercase",
        color: TONE[state] || "var(--toner)", display: "inline-block"
      }}>{state}</span>
      {marked && rect && (
        <InkMark base={inkBase}
          variant={state === "DEAD" ? "strike" : "ring-a"}
          color={state === "DEAD" ? "var(--stamp)" : "var(--wax)"}
          width={rect.width} height={rect.height}
          style={{ position: "absolute", left: rect.left + "px", top: rect.top + "px" }} />
      )}
    </span>
  );
}
