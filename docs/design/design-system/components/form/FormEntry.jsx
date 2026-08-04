import React from "react";
import { PriceCell } from "./PriceCell.jsx";
import { MoreButton } from "./MoreButton.jsx";
import { InkMark } from "./InkMark.jsx";

/* One 78px two-line entry on the house's document. Selecting adds a mark; it never turns the row
   into a rounded sportsbook card, and the document itself never changes. */
export function FormEntry({
  index, number, away, home, selected = null, states = {}, ringVariant,
  onSelect, onMore, inkBase, style, ...rest
}) {
  const variant = ringVariant || InkMark.variantFor((index == null ? 0 : index));
  const sideState = (side) => {
    if (states[side]) return states[side];
    if (selected === side) return "picked";
    if (selected && selected !== side) return "replace";
    return "default";
  };
  const line = (team) => (
    <div style={{ display: "flex", alignItems: "center", gap: "9px", height: "30px" }}>
      <div style={{
        fontFamily: "var(--font-cond)", fontSize: "var(--st-size-price)",
        letterSpacing: "var(--st-track-name)", color: "var(--toner)",
        textTransform: "uppercase", whiteSpace: "nowrap"
      }}>{team.name}</div>
      <div style={{
        fontSize: "var(--st-size-fact)", letterSpacing: "var(--st-track-rec)", color: "var(--toner-3)", whiteSpace: "nowrap",
        fontVariantNumeric: "tabular-nums"
      }}>{team.record}</div>
    </div>
  );
  return (
    <div {...rest} style={{
      height: "var(--st-entry-h)", display: "flex", alignItems: "center",
      padding: "0 var(--st-pad-x)", borderBottom: "var(--rule-w) solid var(--rule-soft)",
      background: selected ? "linear-gradient(90deg,var(--marked-wash),transparent 70%)" : "transparent",
      fontFamily: "var(--font-data)", ...style
    }}>
      <div style={{
        width: "var(--st-col-no)", fontFamily: "var(--font-cond)", fontSize: "15px",
        color: "var(--toner-3)", fontVariantNumeric: "tabular-nums"
      }}>{number}</div>
      <div style={{ flex: 1, display: "flex", flexDirection: "column", gap: "8px", minWidth: 0 }}>
        {line(away)}{line(home)}
      </div>
      <div style={{
        width: "var(--st-col-price)", display: "flex", flexDirection: "column",
        gap: "8px", alignItems: "center"
      }}>
        <PriceCell price={away.price} state={sideState("away")} ringVariant={variant} inkBase={inkBase}
          onSelect={onSelect && (() => onSelect("away"))}
          title={selected === "home" ? "Marking this swaps your " + home.name + " " + home.price + " mark" : undefined} />
        <PriceCell price={home.price} state={sideState("home")} ringVariant={variant} inkBase={inkBase}
          onSelect={onSelect && (() => onSelect("home"))}
          title={selected === "away" ? "Marking this swaps your " + away.name + " " + away.price + " mark" : undefined} />
      </div>
      <div style={{ width: "var(--st-col-more)", display: "flex", justifyContent: "flex-end" }}>
        <MoreButton onClick={onMore} />
      </div>
    </div>
  );
}
