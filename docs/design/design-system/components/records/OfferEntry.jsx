import React from "react";

/* A REWARDS offer as a ruled form entry: name, literal description, price, affordability, buy.
   Affordability is never colour-only, and an unavailable purchase states the real engine reason. */
export function OfferEntry({
  name, description, price, affordable = true, owned = false, reason, onBuy, style, ...rest
}) {
  const [hover, setHover] = React.useState(false);
  return (
    <div {...rest} style={{
      display: "flex", alignItems: "center", gap: "14px", padding: "13px var(--st-pad-x)",
      borderBottom: "var(--rule-w) solid var(--rule-soft)", fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ display: "flex", alignItems: "baseline", gap: "9px" }}>
          <div style={{
            fontFamily: "var(--font-cond)", fontSize: "var(--st-size-price)",
            letterSpacing: "var(--st-track-name)", color: "var(--toner)", textTransform: "uppercase", whiteSpace: "nowrap"
          }}>{name}</div>
          {owned && (
            <div style={{
              fontSize: "var(--st-size-fact)", letterSpacing: ".08em", color: "var(--biro)",
              border: "var(--rule-w) solid var(--biro-deep)", padding: "0 5px"
            }}>HELD</div>
          )}
        </div>
        <div style={{
          fontSize: "var(--st-size-fact)", lineHeight: "var(--st-lh-copy)",
          color: "var(--toner-2)", marginTop: "3px", maxWidth: "44ch"
        }}>{description}</div>
      </div>
      <div style={{ textAlign: "right", width: "92px" }}>
        <div style={{ fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-label)", color: "var(--toner-3)" }}>PRICE</div>
        <div style={{
          fontFamily: "var(--font-cond)", fontSize: "var(--st-size-price)", color: "var(--wax)",
          fontVariantNumeric: "tabular-nums"
        }}>{price}</div>
      </div>
      <div style={{ width: "132px", display: "flex", flexDirection: "column", alignItems: "flex-end", gap: "4px" }}>
        <button type="button" onClick={affordable && !owned ? onBuy : undefined}
          disabled={!affordable || owned}
          onMouseEnter={() => setHover(true)} onMouseLeave={() => setHover(false)}
          style={{
            height: "var(--target-min-h)", minWidth: "var(--target-min-w)", padding: "0 14px",
            background: "transparent",
            border: "var(--rule-w) solid " + (affordable && !owned && hover ? "var(--wax)" : "var(--rule)"),
            borderRadius: "var(--radius)",
            color: !affordable || owned ? "var(--toner-3)" : hover ? "var(--wax-lit)" : "var(--toner)",
            fontFamily: "var(--font-data)", fontSize: "var(--st-size-fact)",
            letterSpacing: ".08em", cursor: !affordable || owned ? "not-allowed" : "pointer"
          }}>{owned ? "HELD" : "BUY"}</button>
        {!affordable && (
          <div style={{
            fontSize: "var(--st-size-fact)", letterSpacing: ".04em", color: "var(--stamp)", whiteSpace: "nowrap",
            border: "var(--rule-w) solid var(--stamp)", padding: "0 5px"
          }}>{reason || "BANK TOO LOW"}</div>
        )}
      </div>
    </div>
  );
}
