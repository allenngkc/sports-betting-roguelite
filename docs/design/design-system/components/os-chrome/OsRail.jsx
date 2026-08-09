import React from "react";

/* His machine, not an institution's. 34px raised chrome: identity mark, a sticker he put there,
   clock, battery. 12px type only — the rail carries no product meaning. */
export function OsRail({
  identity = "NOTEBOOK", sticker = "property of nobody", clock = "02:47",
  batteryLow = true, style, ...rest
}) {
  return (
    <div {...rest} style={{
      height: "var(--st-band-rail)", background: "var(--ground-3)",
      borderBottom: "var(--rule-w) solid var(--rule)", display: "flex", alignItems: "center",
      padding: "0 var(--st-rail-pad-x)", gap: "11px", fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{ display: "flex", alignItems: "center", gap: "7px" }}>
        <div style={{ width: "11px", height: "11px", background: "var(--toner-3)" }} />
        <b style={{ fontSize: "var(--st-size-chrome)", letterSpacing: ".13em", fontWeight: 600, color: "var(--toner-2)" }}>{identity}</b>
      </div>
      {sticker && (
        <div style={{
          fontSize: "var(--st-size-chrome)", letterSpacing: ".09em", padding: "2px 6px",
          color: "var(--biro)", border: "var(--rule-w) solid var(--biro-deep)", transform: "rotate(-.6deg)", whiteSpace: "nowrap"
        }}>{sticker}</div>
      )}
      <div style={{
        marginLeft: "auto", display: "flex", alignItems: "center", gap: "13px",
        fontSize: "var(--st-size-chrome)", letterSpacing: ".1em", color: "var(--toner-2)",
        fontVariantNumeric: "tabular-nums"
      }}>
        <span>{clock}</span>
        <div style={{ width: "20px", height: "9px", border: "var(--rule-w) solid var(--toner-3)", position: "relative" }}>
          <div style={{
            position: "absolute", top: "1.5px", bottom: "1.5px", left: "1.5px", width: "5px",
            background: batteryLow ? "var(--stamp)" : "var(--toner-3)"
          }} />
        </div>
      </div>
    </div>
  );
}
