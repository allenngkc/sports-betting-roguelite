import React from "react";

/* 38px recessed strip. The active tab joins the document ground; inactive tabs are ruled and muted.
   Runtime mapping: Lobby -> FORM, Detail -> ENTRY, MyBets -> MY BETS, Rewards -> REWARDS. */
export function SectionTabs({
  tabs = ["FORM", "ENTRY", "MY BETS", "REWARDS"], active = "FORM", meta = "SHEET 1 OF 1",
  onSelect, style, ...rest
}) {
  return (
    <div {...rest} style={{
      height: "var(--st-band-tabs)", display: "flex", alignItems: "flex-end", gap: "2px",
      padding: "0 var(--st-pad-x)", background: "var(--ground-2)",
      borderBottom: "var(--rule-w-strong) solid var(--rule)", fontFamily: "var(--font-data)", ...style
    }}>
      {tabs.map((t) => {
        const on = t === active;
        return (
          <button key={t} type="button" onClick={onSelect && (() => onSelect(t))}
            aria-current={on ? "page" : undefined}
            style={{
              height: "var(--st-tab-h)", padding: "0 15px", display: "flex", alignItems: "center",
              background: on ? "var(--ground)" : "transparent",
              border: "var(--rule-w) solid " + (on ? "var(--rule)" : "var(--rule-soft)"),
              borderBottom: 0, borderRadius: "var(--radius)",
              color: on ? "var(--toner)" : "var(--toner-3)",
              fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-tab)",
              fontFamily: "var(--font-data)", cursor: "pointer", whiteSpace: "nowrap",
              boxShadow: on ? "0 2px 0 var(--ground)" : "none"
            }}>{t}</button>
        );
      })}
      {meta && (
        <div style={{
          marginLeft: "auto", alignSelf: "center", fontSize: "var(--st-size-fact)",
          letterSpacing: "var(--st-track-label)", color: "var(--toner-3)", whiteSpace: "nowrap"
        }}>{meta}</div>
      )}
    </div>
  );
}
