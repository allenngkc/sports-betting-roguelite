import React from "react";
import { tier } from "./tiers.js";

/* One line, white, L2 at rest, punching to L3 at its reveal callback and settling back.
   Explanation, not commentary theatre. It never uses money hues and never covers the pitch. */
export function TvEventStrip({ text, punched = false, style, ...rest }) {
  return (
    <div {...rest} style={{
      padding: "10px 16px", borderTop: "var(--tv-rule-w) solid var(--tv-rule)",
      fontFamily: "var(--font-tv)", fontSize: "var(--tv-size-event)", fontWeight: 600,
      color: "var(--tv-fact)", opacity: punched ? tier("L3") : tier("L2"),
      letterSpacing: "var(--tv-track-name)", textTransform: "uppercase",
      transition: "opacity var(--tv-dur-event) var(--tv-step)",
      whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis", ...style
    }}>{text}</div>
  );
}
