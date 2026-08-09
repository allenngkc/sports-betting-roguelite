import React from "react";
import { tier } from "./tiers.js";

/* The theatre stage: fixed top-down pitch, picked team attacks right, camera never moves.
   The pitch is a PLACE, not an event — markings sit at L1-L2. Actors are single lit cells in team
   hue at L3; the ball is the only object permitted L4, and only at a payoff. */
export function TvStage({ actors = [], ball, attackingRight = true, style, ...rest }) {
  const mark = { position: "absolute", border: "var(--tv-rule-w) solid var(--tv-pitch)", opacity: tier("L2") };
  return (
    <div {...rest} style={{
      position: "relative", flex: 1, minHeight: "180px", overflow: "hidden",
      background: "var(--tv-substrate)", ...style
    }}>
      <div style={{ ...mark, inset: "12px" }} />
      <div style={{ ...mark, left: "50%", top: "12px", bottom: "12px", borderWidth: "0 0 0 var(--tv-rule-w)" }} />
      <div style={{ ...mark, left: "50%", top: "50%", width: "72px", height: "72px", borderRadius: "50%", transform: "translate(-50%,-50%)" }} />
      <div style={{ ...mark, left: "12px", top: "50%", width: "58px", height: "132px", transform: "translateY(-50%)" }} />
      <div style={{ ...mark, right: "12px", top: "50%", width: "58px", height: "132px", transform: "translateY(-50%)" }} />
      {actors.map((a, i) => (
        <div key={i} style={{
          position: "absolute", left: a.x + "%", top: a.y + "%", width: a.number ? "14px" : "8px",
          height: a.number ? "14px" : "8px", transform: "translate(-50%,-50%)",
          background: a.team === "b" ? "var(--tv-team-b)" : "var(--tv-team-a)",
          opacity: tier("L3"), display: "flex", alignItems: "center", justifyContent: "center",
          fontFamily: "var(--font-tv)", fontSize: "10px", color: "var(--tv-substrate)", fontWeight: 700
        }}>{a.number}</div>
      ))}
      {ball && (
        <div style={{
          position: "absolute", left: ball.x + "%", top: ball.y + "%", width: "7px", height: "7px",
          transform: "translate(-50%,-50%)", background: "var(--tv-fact)",
          opacity: ball.payoff ? tier("L4") : tier("L3")
        }} />
      )}
      {!attackingRight && (
        <div style={{
          position: "absolute", left: "16px", bottom: "16px", fontFamily: "var(--font-tv)",
          fontSize: "var(--tv-size-eyebrow)", letterSpacing: "var(--tv-track-label)",
          color: "var(--tv-context)", opacity: tier("L1")
        }}>◄ ATTACKING</div>
      )}
    </div>
  );
}
