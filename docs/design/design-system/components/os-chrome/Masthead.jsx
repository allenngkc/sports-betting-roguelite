import React from "react";
import { RunFigure } from "../figures/RunFigure.jsx";

/* 68px. The sheet's own masthead: brand, dateline, the run's persistent figures, and the literal
   locked-odds note. No promo rail — promotional rails are an anti-feature here. */
export function Masthead({
  title = "SureThing Form", dateline, figures = [], note, style, ...rest
}) {
  return (
    <div {...rest} style={{
      height: "var(--st-band-mast)", display: "flex", alignItems: "center",
      padding: "0 var(--st-mast-pad-x)", gap: "18px",
      borderBottom: "var(--rule-w-strong) solid var(--rule)",
      fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{
        display: "flex", flexDirection: "column", gap: "2px", paddingRight: "18px",
        borderRight: "var(--rule-w) solid var(--rule)"
      }}>
        <h1 style={{
          margin: 0, fontFamily: "var(--font-cond)", fontSize: "var(--st-size-mast)",
          letterSpacing: "var(--st-track-name)", color: "var(--toner)",
          lineHeight: "var(--st-lh-tight)", textTransform: "uppercase", fontWeight: 400
        }}>{title}</h1>
        {dateline && (
          <div style={{
            fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-tab)", color: "var(--toner-3)", whiteSpace: "nowrap"
          }}>{dateline}</div>
        )}
      </div>
      {figures.map((f) => (
        <RunFigure key={f.label} label={f.label} value={f.value} tone={f.tone} />
      ))}
      {note && (
        <div style={{
          marginLeft: "auto", maxWidth: "212px", fontSize: "var(--st-size-fact)",
          lineHeight: "var(--st-lh-copy)", letterSpacing: ".02em", color: "var(--toner-3)",
          borderLeft: "var(--rule-w-strong) solid var(--rule)", paddingLeft: "11px"
        }}>{note}</div>
      )}
    </div>
  );
}
