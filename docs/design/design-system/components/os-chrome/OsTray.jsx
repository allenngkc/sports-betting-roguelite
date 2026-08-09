import React from "react";

/* 34px raised tray. Other apps on his machine, plus non-product facts. 12px is legal here and only
   here — nothing in this band may state a product fact. */
export function OsTray({
  apps = [{ label: "SURETHING", active: true }, { label: "LEDGER" }, { label: "MESSAGES", badge: "1" }],
  facts = ["DISK 61% FULL", "NO UPDATES AVAILABLE"], onSelect, style, ...rest
}) {
  return (
    <div {...rest} style={{
      height: "var(--st-band-tray)", background: "var(--ground-3)",
      borderTop: "var(--rule-w) solid var(--rule)", display: "flex", alignItems: "center",
      padding: "0 var(--st-rail-pad-x)", gap: "6px", fontFamily: "var(--font-data)", ...style
    }}>
      {apps.map((a) => (
        <button key={a.label} type="button" onClick={onSelect && (() => onSelect(a.label))}
          style={{
            height: "var(--st-tray-app-h)", padding: "0 10px", display: "flex", alignItems: "center",
            gap: "6px", fontSize: "var(--st-size-chrome)", letterSpacing: ".09em",
            color: a.active ? "var(--toner)" : "var(--toner-3)",
            border: "var(--rule-w) solid " + (a.active ? "var(--rule)" : "var(--rule-soft)"),
            borderRadius: "var(--radius)", background: a.active ? "var(--ground)" : "transparent",
            fontFamily: "var(--font-data)", cursor: "pointer"
          }}>
          <span style={{ width: "5px", height: "5px", background: a.active ? "var(--wax)" : "var(--toner-3)" }} />
          {a.label}
          {a.badge && (
            <span style={{ background: "var(--stamp)", color: "var(--toner)", fontSize: "var(--st-size-chrome)", padding: "0 5px" }}>{a.badge}</span>
          )}
        </button>
      ))}
      <div style={{
        marginLeft: "auto", fontSize: "var(--st-size-chrome)", letterSpacing: ".13em",
        color: "var(--toner-3)", display: "flex", gap: "14px"
      }}>{facts.map((f) => <span key={f}>{f}</span>)}</div>
    </div>
  );
}
