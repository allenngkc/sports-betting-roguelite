/* @ds-bundle: {"format":4,"namespace":"SureThingDesignSystem_6e1eb3","components":[{"name":"LockAction","sourcePath":"components/actions/LockAction.jsx"},{"name":"PlaceAction","sourcePath":"components/actions/PlaceAction.jsx"},{"name":"SkipAction","sourcePath":"components/actions/SkipAction.jsx"},{"name":"StampReason","sourcePath":"components/actions/StampReason.jsx"},{"name":"PayoutFigure","sourcePath":"components/figures/PayoutFigure.jsx"},{"name":"RunFigure","sourcePath":"components/figures/RunFigure.jsx"},{"name":"ColumnHead","sourcePath":"components/form/ColumnHead.jsx"},{"name":"FormEntry","sourcePath":"components/form/FormEntry.jsx"},{"name":"InkMark","sourcePath":"components/form/InkMark.jsx"},{"name":"MarketOffer","sourcePath":"components/form/MarketOffer.jsx"},{"name":"MoreButton","sourcePath":"components/form/MoreButton.jsx"},{"name":"PriceCell","sourcePath":"components/form/PriceCell.jsx"},{"name":"MarginHeader","sourcePath":"components/margin/MarginHeader.jsx"},{"name":"MarginLeg","sourcePath":"components/margin/MarginLeg.jsx"},{"name":"MarginRow","sourcePath":"components/margin/MarginRow.jsx"},{"name":"RubOutButton","sourcePath":"components/margin/RubOutButton.jsx"},{"name":"StakeButton","sourcePath":"components/margin/StakeButton.jsx"},{"name":"StakeControls","sourcePath":"components/margin/StakeControls.jsx"},{"name":"Masthead","sourcePath":"components/os-chrome/Masthead.jsx"},{"name":"OsRail","sourcePath":"components/os-chrome/OsRail.jsx"},{"name":"OsTray","sourcePath":"components/os-chrome/OsTray.jsx"},{"name":"SectionTabs","sourcePath":"components/os-chrome/SectionTabs.jsx"},{"name":"LedgerEntry","sourcePath":"components/records/LedgerEntry.jsx"},{"name":"OfferEntry","sourcePath":"components/records/OfferEntry.jsx"},{"name":"RevealedLeg","sourcePath":"components/records/RevealedLeg.jsx"},{"name":"RevealedState","sourcePath":"components/records/RevealedState.jsx"},{"name":"TicketReceipt","sourcePath":"components/records/TicketReceipt.jsx"},{"name":"TvCashOutSlot","sourcePath":"components/tv/TvCashOutSlot.jsx"},{"name":"TvEventStrip","sourcePath":"components/tv/TvEventStrip.jsx"},{"name":"TvLegRow","sourcePath":"components/tv/TvLegRow.jsx"},{"name":"TvMomentumTape","sourcePath":"components/tv/TvMomentumTape.jsx"},{"name":"TvRiskPays","sourcePath":"components/tv/TvRiskPays.jsx"},{"name":"TvScorebug","sourcePath":"components/tv/TvScorebug.jsx"},{"name":"TvStage","sourcePath":"components/tv/TvStage.jsx"},{"name":"TvStatsPanel","sourcePath":"components/tv/TvStatsPanel.jsx"},{"name":"TvTicketCard","sourcePath":"components/tv/TvTicketCard.jsx"},{"name":"TIER","sourcePath":"components/tv/tiers.js"}],"sourceHashes":{"components/actions/LockAction.jsx":"5e075fa1a5af","components/actions/PlaceAction.jsx":"97fc19a60761","components/actions/SkipAction.jsx":"0a5de34084f4","components/actions/StampReason.jsx":"7ede80e30974","components/figures/PayoutFigure.jsx":"50dfe4f926b0","components/figures/RunFigure.jsx":"a046eb46d589","components/form/ColumnHead.jsx":"94c8b259e68e","components/form/FormEntry.jsx":"3a7b2f9f2518","components/form/InkMark.jsx":"0f1f0e228b7b","components/form/MarketOffer.jsx":"085e428412e4","components/form/MoreButton.jsx":"008c0b0f8395","components/form/PriceCell.jsx":"51922aea51a3","components/margin/MarginHeader.jsx":"7e5a3d59c266","components/margin/MarginLeg.jsx":"134c66336e21","components/margin/MarginRow.jsx":"3fb936827540","components/margin/RubOutButton.jsx":"25ef8695c6bb","components/margin/StakeButton.jsx":"54ef3402da0a","components/margin/StakeControls.jsx":"42605b10c0fa","components/os-chrome/Masthead.jsx":"92ef616b7eea","components/os-chrome/OsRail.jsx":"70cc7ae398e1","components/os-chrome/OsTray.jsx":"8d2ff53feb42","components/os-chrome/SectionTabs.jsx":"96932e93dc6d","components/records/LedgerEntry.jsx":"b9e20e69d083","components/records/OfferEntry.jsx":"9aa2e5c1b778","components/records/RevealedLeg.jsx":"93e9fd71e995","components/records/RevealedState.jsx":"4880c1c62581","components/records/TicketReceipt.jsx":"fe6a8a091093","components/tv/TvCashOutSlot.jsx":"12e4de3fd35f","components/tv/TvEventStrip.jsx":"c42dc2df4af2","components/tv/TvLegRow.jsx":"4699523f785d","components/tv/TvMomentumTape.jsx":"acf8cc09eba4","components/tv/TvRiskPays.jsx":"38cb0378ca91","components/tv/TvScorebug.jsx":"85f5822f1e0e","components/tv/TvStage.jsx":"828ea0220389","components/tv/TvStatsPanel.jsx":"30fb80505e3e","components/tv/TvTicketCard.jsx":"feca067a5a50","components/tv/tiers.js":"b10d920e9cc2","ui_kits/surething/app.jsx":"1d4e0988f382","ui_kits/surething/betmath.js":"9533653797cc","ui_kits/surething/data.js":"b0d4505fb820","ui_kits/surething/margin.jsx":"ef306e0b8af2","ui_kits/surething/screens.jsx":"23ae9567bfa4","ui_kits/tv-sweat/app.jsx":"505460838288","ui_kits/tv-sweat/data.js":"9380b372606f"},"inlinedExternals":[],"unexposedExports":[{"name":"tier","sourcePath":"components/tv/tiers.js"}]} */

(() => {

const __ds_ns = (window.SureThingDesignSystem_6e1eb3 = window.SureThingDesignSystem_6e1eb3 || {});

const __ds_scope = {};

(__ds_ns.__errors = __ds_ns.__errors || []);

// components/actions/PlaceAction.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The one solid wax field on the surface. Stages a valid working slip. */
function PlaceAction({
  label = "Place Ticket",
  disabled = false,
  onClick,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const [down, setDown] = React.useState(false);
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: onClick,
    disabled: disabled
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => {
      setHover(false);
      setDown(false);
    },
    onMouseDown: () => setDown(true),
    onMouseUp: () => setDown(false),
    style: {
      height: "var(--st-place-h)",
      minWidth: "var(--st-place-min-w)",
      padding: "0 22px",
      border: 0,
      borderRadius: "var(--radius)",
      cursor: disabled ? "not-allowed" : "pointer",
      background: disabled ? "var(--ground-3)" : hover ? "var(--wax-lit)" : "var(--wax)",
      color: disabled ? "var(--toner-3)" : "var(--wax-ink)",
      fontFamily: "var(--font-cond)",
      fontSize: "17px",
      letterSpacing: "var(--st-track-action)",
      textTransform: "uppercase",
      boxShadow: disabled || down ? "none" : "0 var(--st-press-shift) 0 var(--wax-deep)",
      transform: down && !disabled ? "translateY(var(--st-press-shift))" : "none",
      ...style
    }
  }), label);
}
Object.assign(__ds_scope, { PlaceAction });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/actions/PlaceAction.jsx", error: String((e && e.message) || e) }); }

// components/actions/SkipAction.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Separate secondary action, two presses. Never masquerades as lock. First press arms; second commits
   an empty round. Dashed unarmed, solid stamp armed. */
function SkipAction({
  label = "SKIP ROUND — PRESS TWICE",
  armedLabel = "PRESS AGAIN TO SKIP",
  armed: armedProp,
  onSkip,
  style,
  ...rest
}) {
  const [armedState, setArmed] = React.useState(false);
  const armed = armedProp == null ? armedState : armedProp;
  const [hover, setHover] = React.useState(false);
  const lit = armed || hover;
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button"
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    onClick: () => {
      if (armed) {
        setArmed(false);
        if (onSkip) onSkip();
      } else setArmed(true);
    },
    style: {
      height: "var(--st-skip-h)",
      minWidth: "var(--st-skip-min-w)",
      background: "transparent",
      border: "var(--rule-w) " + (lit ? "solid" : "dashed") + " " + (lit ? "var(--stamp)" : "var(--rule)"),
      borderRadius: "var(--radius)",
      color: lit ? "var(--stamp)" : "var(--toner-3)",
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-rec)",
      cursor: "pointer",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), armed ? armedLabel : label);
}
Object.assign(__ds_scope, { SkipAction });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/actions/SkipAction.jsx", error: String((e && e.message) || e) }); }

// components/actions/StampReason.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The house stamps a blocked action. Cause AND remedy, in place, at the 13px fact floor, inside the
   house's own oxide border. Never a tooltip, never colour alone, never below the floor. */
function StampReason({
  reason,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("small", _extends({}, rest, {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".04em",
      color: "var(--stamp)",
      border: "var(--rule-w) solid var(--stamp)",
      padding: "1px 7px",
      fontFamily: "var(--font-data)",
      whiteSpace: "nowrap",
      ...style
    }
  }), reason);
}
Object.assign(__ds_scope, { StampReason });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/actions/StampReason.jsx", error: String((e && e.message) || e) }); }

// components/actions/LockAction.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Commits the round. A 52px RULED control in both states — not a second solid wax field.
   Disabled while working marks exist or no ticket is staged, and it says why, in place. */
function LockAction({
  label = "Lock It In",
  disabled = true,
  reason,
  onClick,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: disabled ? undefined : onClick,
    disabled: disabled
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      height: "var(--st-lock-h)",
      minWidth: "var(--st-lock-min-w)",
      padding: "0 16px",
      border: disabled ? "var(--rule-w) solid var(--rule)" : "var(--rule-w-strong) solid " + (hover ? "var(--wax-lit)" : "var(--wax)"),
      borderRadius: "var(--radius)",
      background: "transparent",
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      gap: "2px",
      cursor: disabled ? "not-allowed" : "pointer",
      ...style
    }
  }), /*#__PURE__*/React.createElement("b", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "15px",
      letterSpacing: "var(--st-track-action)",
      color: disabled ? "var(--toner-3)" : "var(--toner)",
      fontWeight: 400,
      textTransform: "uppercase"
    }
  }, label), disabled && reason && /*#__PURE__*/React.createElement(__ds_scope.StampReason, {
    reason: reason
  }));
}
Object.assign(__ds_scope, { LockAction });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/actions/LockAction.jsx", error: String((e && e.message) || e) }); }

// components/figures/PayoutFigure.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The one loud figure in the margin. 31px wax with the hand-laid highlight: a 6px amber band at
   0.26 opacity, rotated -0.5deg, behind the figure. Bank and target do not get it, because only one
   figure at a time may be the loudest thing on the surface. */
function PayoutFigure({
  label = "POTENTIAL PAYOUT",
  value,
  highlight = true,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-label)",
      color: "var(--toner-3)",
      whiteSpace: "nowrap"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      position: "relative",
      display: "inline-block",
      marginTop: "2px",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-payout)",
      color: "var(--wax)",
      lineHeight: "var(--st-lh-fig)",
      fontVariantNumeric: "tabular-nums"
    }
  }, value, highlight && /*#__PURE__*/React.createElement("span", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      left: "-3px",
      right: "-5px",
      bottom: "-2px",
      height: "var(--wax-highlight-h)",
      background: "var(--wax)",
      opacity: "var(--wax-highlight-opacity)",
      transform: "rotate(var(--wax-highlight-rotate))"
    }
  })));
}
Object.assign(__ds_scope, { PayoutFigure });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/figures/PayoutFigure.jsx", error: String((e && e.message) || e) }); }

// components/figures/RunFigure.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Bank, target, relics, tickets. Persistent, literal, and legible at the 50% thumbnail check.
   Money wears the wax pencil; counts do not. */
function RunFigure({
  label,
  value,
  tone = "toner",
  size,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: "3px",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-label)",
      color: "var(--toner-3)",
      whiteSpace: "nowrap"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: size || "var(--st-size-figure)",
      letterSpacing: ".02em",
      color: tone === "wax" ? "var(--wax)" : "var(--toner)",
      lineHeight: "var(--st-lh-tight)",
      fontVariantNumeric: "tabular-nums"
    }
  }, value));
}
Object.assign(__ds_scope, { RunFigure });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/figures/RunFigure.jsx", error: String((e && e.message) || e) }); }

// components/form/ColumnHead.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* 26px recessed band above the six entries. Column heads state product facts: 13px floor. */
function ColumnHead({
  no = "NO.",
  matchup = "MATCHUP · SEASON RECORD",
  price = "MONEYLINE",
  more = "MORE",
  style,
  ...rest
}) {
  const cap = {
    fontSize: "var(--st-size-fact)",
    letterSpacing: "var(--st-track-label)",
    color: "var(--toner-3)"
  };
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "var(--st-colhead-h)",
      display: "flex",
      alignItems: "center",
      padding: "0 var(--st-pad-x)",
      background: "var(--ground-2)",
      borderBottom: "var(--rule-w) solid var(--rule)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      ...cap,
      width: "var(--st-col-no)"
    }
  }, no), /*#__PURE__*/React.createElement("div", {
    style: {
      ...cap,
      flex: 1
    }
  }, matchup), /*#__PURE__*/React.createElement("div", {
    style: {
      ...cap,
      width: "var(--st-col-price)",
      textAlign: "center"
    }
  }, price), /*#__PURE__*/React.createElement("div", {
    style: {
      ...cap,
      width: "var(--st-col-more)",
      textAlign: "right"
    }
  }, more));
}
Object.assign(__ds_scope, { ColumnHead });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/form/ColumnHead.jsx", error: String((e && e.message) || e) }); }

// components/form/InkMark.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The selection ring and the dead-leg strike are real drawn assets: white RGB with all the ink in
   the alpha channel, tinted at runtime exactly as Unity tints them via Image.color. Never a CSS
   border, never a fill, never a hand-drawn approximation. Regenerate with
   `python tools/art/make-biro-rings.py` — a given seed always produces the same mark. */
const FILES = {
  "ring-a": ["ring-price-a@2x.png", 112, 46],
  "ring-b": ["ring-price-b@2x.png", 112, 46],
  "ring-c": ["ring-price-c@2x.png", 112, 46],
  "ring-wide-a": ["ring-wide-a@2x.png", 176, 46],
  "ring-wide-b": ["ring-wide-b@2x.png", 176, 46],
  "strike": ["strike-a@2x.png", 112, 46]
};
function InkMark({
  variant = "ring-a",
  color = "var(--biro)",
  width,
  height,
  base,
  style,
  ...rest
}) {
  const spec = FILES[variant] || FILES["ring-a"];
  const root = base || typeof window !== "undefined" && window.SURETHING_INK_BASE || "assets/ink/";
  const w = width == null ? spec[1] : width;
  const h = height == null ? spec[2] : height;
  const url = "url(" + root + spec[0] + ")";
  return /*#__PURE__*/React.createElement("span", _extends({
    "aria-hidden": "true"
  }, rest, {
    style: {
      display: "block",
      width: w + "px",
      height: h + "px",
      pointerEvents: "none",
      backgroundColor: color,
      WebkitMaskImage: url,
      maskImage: url,
      WebkitMaskRepeat: "no-repeat",
      maskRepeat: "no-repeat",
      WebkitMaskSize: w + "px " + h + "px",
      maskSize: w + "px " + h + "px",
      ...style
    }
  }));
}

/* ring = cell + 16px on both axes, offset -8/-8. Additive, never proportional: the pen's overshoot
   needs somewhere to go. Apply to whatever the real control is — do not assume 96x30. */
InkMark.rect = (cellW, cellH) => ({
  width: cellW + 16,
  height: cellH + 16,
  left: -8,
  top: -8
});

/* Deterministic per matchup index. Never randomise per frame or per canvas rebuild — the board
   would visibly redraw itself every time the player nudged a stake. */
InkMark.variantFor = (i, wide) => wide ? ["ring-wide-a", "ring-wide-b"][i % 2] : ["ring-a", "ring-b", "ring-c"][i % 3];
Object.assign(__ds_scope, { InkMark });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/form/InkMark.jsx", error: String((e && e.message) || e) }); }

// components/form/MarketOffer.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Event-detail market row: 160x30 offer cell, 176x46 wide ring. Reuses the lobby's price grammar,
   so a selection from any destination replaces that matchup's existing selection. */
function MarketOffer({
  line,
  price,
  state = "default",
  ringVariant = "ring-wide-a",
  onSelect,
  inkBase,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const ring = __ds_scope.InkMark.rect(160, 30);
  const figure = state === "picked" ? "var(--toner)" : state === "replace" ? hover ? "var(--toner)" : "var(--toner-2)" : hover ? "var(--wax-lit)" : "var(--toner)";
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: onSelect
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    "aria-pressed": state === "picked",
    style: {
      position: "relative",
      width: "160px",
      minHeight: "32px",
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
      gap: "12px",
      background: "transparent",
      border: 0,
      padding: 0,
      borderRadius: "var(--radius)",
      cursor: "pointer",
      fontFamily: "var(--font-data)",
      color: figure,
      ...style
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-rec)",
      textTransform: "uppercase",
      textAlign: "left",
      whiteSpace: "nowrap"
    }
  }, line), /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-price)",
      letterSpacing: "var(--st-track-name)",
      fontVariantNumeric: "tabular-nums",
      textDecoration: state === "replace" ? "underline" : "none",
      textDecorationStyle: "dashed",
      textDecorationColor: "var(--biro-deep)",
      textUnderlineOffset: "5px"
    }
  }, price), state === "picked" && /*#__PURE__*/React.createElement(__ds_scope.InkMark, {
    variant: ringVariant,
    color: "var(--biro)",
    base: inkBase,
    width: ring.width,
    height: ring.height,
    style: {
      position: "absolute",
      left: ring.left + "px",
      top: "50%",
      transform: "translateY(-50%)"
    }
  }));
}
Object.assign(__ds_scope, { MarketOffer });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/form/MarketOffer.jsx", error: String((e && e.message) || e) }); }

// components/form/MoreButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Entry to event detail — Goals, BTTS, Corners, Cards, Players. Rectangular, ruled, literal. */
function MoreButton({
  label = "MORE",
  onClick,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: onClick
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      width: "var(--st-more-w)",
      height: "var(--st-more-h)",
      background: "transparent",
      border: "var(--rule-w) solid " + (hover ? "var(--toner-3)" : "var(--rule)"),
      borderRadius: "var(--radius)",
      color: hover ? "var(--toner)" : "var(--toner-2)",
      cursor: "pointer",
      fontFamily: "var(--font-data)",
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".05em",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      gap: "4px",
      ...style
    }
  }), label, /*#__PURE__*/React.createElement("i", {
    style: {
      fontStyle: "normal",
      fontSize: "15px",
      lineHeight: 1
    }
  }, "\u203A"));
}
Object.assign(__ds_scope, { MoreButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/form/MoreButton.jsx", error: String((e && e.message) || e) }); }

// components/form/PriceCell.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* A price is a printed figure. Choosing it means CIRCLING it. Selection is never a pill or a fill.
   REPLACE, NEVER BLOCK: picking the other side of a matchup that already carries a selection
   replaces it. v0 has no limiting, no padlock, no disabled odds, no suspension. */
function PriceCell({
  price,
  state = "default",
  ringVariant = "ring-a",
  size = "kit",
  inkBase,
  onSelect,
  title,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const cellW = size === "runtime" ? 112 : 96;
  const cellH = size === "runtime" ? 32 : 30;
  const ring = __ds_scope.InkMark.rect(cellW, cellH);
  const figure = state === "won" ? "var(--wax)" : state === "dead" ? "var(--toner-3)" : state === "replace" ? hover ? "var(--toner)" : "var(--toner-2)" : hover ? "var(--wax-lit)" : "var(--toner)";
  const mark = state === "picked" ? {
    variant: ringVariant,
    color: "var(--biro)"
  } : state === "won" ? {
    variant: ringVariant,
    color: "var(--wax)"
  } : state === "dead" ? {
    variant: "strike",
    color: "var(--stamp)"
  } : null;
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: onSelect,
    title: title,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    "aria-pressed": state === "picked"
  }, rest, {
    style: {
      position: "relative",
      width: cellW + "px",
      height: Math.max(cellH, 32) + "px",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      background: "transparent",
      border: 0,
      padding: 0,
      borderRadius: "var(--radius)",
      cursor: "pointer",
      font: "inherit",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-price)",
      letterSpacing: "var(--st-track-name)",
      fontVariantNumeric: "tabular-nums",
      color: figure,
      ...style
    }
  }), state === "replace" ? /*#__PURE__*/React.createElement("span", {
    style: {
      display: "inline-flex",
      alignItems: "baseline",
      gap: "5px"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-data)",
      fontSize: "15px",
      color: "var(--biro)"
    }
  }, "\u21C4"), /*#__PURE__*/React.createElement("u", {
    style: {
      textDecoration: "underline",
      textDecorationStyle: "dashed",
      textDecorationColor: "var(--biro-deep)",
      textUnderlineOffset: "5px"
    }
  }, price)) : price, mark && /*#__PURE__*/React.createElement(__ds_scope.InkMark, {
    variant: mark.variant,
    color: mark.color,
    base: inkBase,
    width: ring.width,
    height: ring.height,
    style: {
      position: "absolute",
      left: ring.left + "px",
      top: "50%",
      transform: "translateY(-50%)"
    }
  }));
}
Object.assign(__ds_scope, { PriceCell });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/form/PriceCell.jsx", error: String((e && e.message) || e) }); }

// components/form/FormEntry.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* One 78px two-line entry on the house's document. Selecting adds a mark; it never turns the row
   into a rounded sportsbook card, and the document itself never changes. */
function FormEntry({
  index,
  number,
  away,
  home,
  selected = null,
  states = {},
  ringVariant,
  onSelect,
  onMore,
  inkBase,
  style,
  ...rest
}) {
  const variant = ringVariant || __ds_scope.InkMark.variantFor(index == null ? 0 : index);
  const sideState = side => {
    if (states[side]) return states[side];
    if (selected === side) return "picked";
    if (selected && selected !== side) return "replace";
    return "default";
  };
  const line = team => /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "9px",
      height: "30px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-price)",
      letterSpacing: "var(--st-track-name)",
      color: "var(--toner)",
      textTransform: "uppercase",
      whiteSpace: "nowrap"
    }
  }, team.name), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-rec)",
      color: "var(--toner-3)",
      whiteSpace: "nowrap",
      fontVariantNumeric: "tabular-nums"
    }
  }, team.record));
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "var(--st-entry-h)",
      display: "flex",
      alignItems: "center",
      padding: "0 var(--st-pad-x)",
      borderBottom: "var(--rule-w) solid var(--rule-soft)",
      background: selected ? "linear-gradient(90deg,var(--marked-wash),transparent 70%)" : "transparent",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "var(--st-col-no)",
      fontFamily: "var(--font-cond)",
      fontSize: "15px",
      color: "var(--toner-3)",
      fontVariantNumeric: "tabular-nums"
    }
  }, number), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      gap: "8px",
      minWidth: 0
    }
  }, line(away), line(home)), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "var(--st-col-price)",
      display: "flex",
      flexDirection: "column",
      gap: "8px",
      alignItems: "center"
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.PriceCell, {
    price: away.price,
    state: sideState("away"),
    ringVariant: variant,
    inkBase: inkBase,
    onSelect: onSelect && (() => onSelect("away")),
    title: selected === "home" ? "Marking this swaps your " + home.name + " " + home.price + " mark" : undefined
  }), /*#__PURE__*/React.createElement(__ds_scope.PriceCell, {
    price: home.price,
    state: sideState("home"),
    ringVariant: variant,
    inkBase: inkBase,
    onSelect: onSelect && (() => onSelect("home")),
    title: selected === "away" ? "Marking this swaps your " + away.name + " " + away.price + " mark" : undefined
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "var(--st-col-more)",
      display: "flex",
      justifyContent: "flex-end"
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.MoreButton, {
    onClick: onMore
  })));
}
Object.assign(__ds_scope, { FormEntry });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/form/FormEntry.jsx", error: String((e && e.message) || e) }); }

// components/margin/MarginHeader.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The margin is his. Its header is biro-ruled, and the selection count is a literal product fact. */
function MarginHeader({
  title = "My Marks",
  count,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      padding: "12px 0 9px",
      borderBottom: "var(--rule-w-strong) solid var(--biro-deep)",
      display: "flex",
      alignItems: "baseline",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("h2", {
    style: {
      margin: 0,
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      letterSpacing: "var(--st-track-head)",
      color: "var(--biro)",
      textTransform: "uppercase",
      fontWeight: 400,
      whiteSpace: "nowrap"
    }
  }, title), count != null && /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".1em",
      color: "var(--toner-2)",
      whiteSpace: "nowrap"
    }
  }, count));
}
Object.assign(__ds_scope, { MarginHeader });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/margin/MarginHeader.jsx", error: String((e && e.message) || e) }); }

// components/margin/MarginRow.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* A key/value line in the margin — COMBINED and anything like it. Not money: no wax. */
function MarginRow({
  label,
  value,
  tone = "toner",
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      alignItems: "baseline",
      padding: "9px 0",
      borderBottom: "var(--rule-w) solid var(--rule)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-label)",
      color: "var(--toner-3)",
      whiteSpace: "nowrap"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-row)",
      color: tone === "wax" ? "var(--wax)" : "var(--toner)",
      fontVariantNumeric: "tabular-nums"
    }
  }, value));
}
Object.assign(__ds_scope, { MarginRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/margin/MarginRow.jsx", error: String((e && e.message) || e) }); }

// components/margin/RubOutButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* An explicit 60x32 removal target, never a tiny unlabeled x. Mis-clicks cost money here.
   RUB OUT is an ACTION LABEL, so it is set in the condensed face — the type contract assigns the
   condensed production face to the masthead, figures, prices, team names and action labels, and the
   data face to running text and secondary labels. 60x32 is a locked element-kit value; the label is
   made to fit the control, never the control widened to fit the label. */
function RubOutButton({
  label = "RUB OUT",
  onClick,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: onClick
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      width: "var(--st-rub-w)",
      height: "var(--st-rub-h)",
      flex: "none",
      background: "transparent",
      border: "var(--rule-w) solid " + (hover ? "var(--stamp)" : "var(--rule)"),
      borderRadius: "var(--radius)",
      color: hover ? "var(--stamp)" : "var(--toner-3)",
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".06em",
      cursor: "pointer",
      fontFamily: "var(--font-cond)",
      whiteSpace: "nowrap",
      padding: 0,
      ...style
    }
  }), label);
}
Object.assign(__ds_scope, { RubOutButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/margin/RubOutButton.jsx", error: String((e && e.message) || e) }); }

// components/margin/MarginLeg.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* One explicit leg per selection: his blue check, the identity, the price, and an explicit RUB OUT. */
function MarginLeg({
  team,
  price,
  market,
  entry,
  onRemove,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "9px",
      padding: "8px 0",
      borderBottom: "var(--rule-w) dotted var(--rule)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "15px",
      flex: "none",
      color: "var(--biro)",
      fontSize: "15px",
      lineHeight: 1
    }
  }, "\u2713"), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: "7px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: "var(--toner)",
      textTransform: "uppercase"
    }
  }, team), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: "var(--toner)",
      fontVariantNumeric: "tabular-nums"
    }
  }, price)), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-rec)",
      whiteSpace: "nowrap",
      color: "var(--toner-3)",
      marginTop: "3px"
    }
  }, market, entry ? " · ENTRY " + entry : "")), /*#__PURE__*/React.createElement(__ds_scope.RubOutButton, {
    onClick: onRemove
  }));
}
Object.assign(__ds_scope, { MarginLeg });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/margin/MarginLeg.jsx", error: String((e && e.message) || e) }); }

// components/margin/StakeButton.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Two stake controls, two shapes. Quick fractions are ruled and transparent; nudge keys are raised
   chrome, because they are keys on his own machine. Both hover to biro: he chose them. */
function StakeButton({
  label,
  variant = "quick",
  onClick,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  const quick = variant === "quick";
  return /*#__PURE__*/React.createElement("button", _extends({
    type: "button",
    onClick: onClick
  }, rest, {
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      width: quick ? "var(--st-quick-w)" : "var(--st-nudge-w)",
      height: quick ? "var(--st-quick-h)" : "var(--st-nudge-h)",
      background: quick ? "transparent" : "var(--ground-3)",
      border: "var(--rule-w) solid " + (hover ? "var(--biro)" : "var(--rule)"),
      borderRadius: "var(--radius)",
      color: hover ? "var(--biro)" : quick ? "var(--toner-2)" : "var(--toner)",
      fontFamily: quick ? "var(--font-data)" : "var(--font-cond)",
      fontSize: quick ? "var(--st-size-fact)" : "15px",
      letterSpacing: quick ? ".03em" : "0",
      cursor: "pointer",
      fontVariantNumeric: "tabular-nums",
      ...style
    }
  }), label);
}
Object.assign(__ds_scope, { StakeButton });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/margin/StakeButton.jsx", error: String((e && e.message) || e) }); }

// components/margin/StakeControls.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Stake figure plus its controls. Display stake and payout together after every input. */
function StakeControls({
  label = "STAKE",
  stake,
  fractions = ["10%", "25%", "50%", "MAX"],
  nudges = ["− $10", "+ $10"],
  onFraction,
  onNudge,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      padding: "10px 0 0",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-label)",
      color: "var(--toner-3)"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-stake)",
      color: "var(--toner)",
      lineHeight: "var(--st-lh-tight)",
      fontVariantNumeric: "tabular-nums"
    }
  }, stake)), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: "4px",
      marginTop: "8px"
    }
  }, fractions.map(f => /*#__PURE__*/React.createElement(__ds_scope.StakeButton, {
    key: f,
    label: f,
    onClick: onFraction && (() => onFraction(f)),
    style: {
      flex: 1,
      width: "auto"
    }
  }))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: "4px",
      marginTop: "4px"
    }
  }, nudges.map(n => /*#__PURE__*/React.createElement(__ds_scope.StakeButton, {
    key: n,
    label: n,
    variant: "nudge",
    onClick: onNudge && (() => onNudge(n)),
    style: {
      flex: 1,
      width: "auto"
    }
  }))));
}
Object.assign(__ds_scope, { StakeControls });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/margin/StakeControls.jsx", error: String((e && e.message) || e) }); }

// components/os-chrome/Masthead.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* 68px. The sheet's own masthead: brand, dateline, the run's persistent figures, and the literal
   locked-odds note. No promo rail — promotional rails are an anti-feature here. */
function Masthead({
  title = "SureThing Form",
  dateline,
  figures = [],
  note,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "var(--st-band-mast)",
      display: "flex",
      alignItems: "center",
      padding: "0 var(--st-mast-pad-x)",
      gap: "18px",
      borderBottom: "var(--rule-w-strong) solid var(--rule)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      flexDirection: "column",
      gap: "2px",
      paddingRight: "18px",
      borderRight: "var(--rule-w) solid var(--rule)"
    }
  }, /*#__PURE__*/React.createElement("h1", {
    style: {
      margin: 0,
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-mast)",
      letterSpacing: "var(--st-track-name)",
      color: "var(--toner)",
      lineHeight: "var(--st-lh-tight)",
      textTransform: "uppercase",
      fontWeight: 400
    }
  }, title), dateline && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-tab)",
      color: "var(--toner-3)",
      whiteSpace: "nowrap"
    }
  }, dateline)), figures.map(f => /*#__PURE__*/React.createElement(__ds_scope.RunFigure, {
    key: f.label,
    label: f.label,
    value: f.value,
    tone: f.tone
  })), note && /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      maxWidth: "212px",
      fontSize: "var(--st-size-fact)",
      lineHeight: "var(--st-lh-copy)",
      letterSpacing: ".02em",
      color: "var(--toner-3)",
      borderLeft: "var(--rule-w-strong) solid var(--rule)",
      paddingLeft: "11px"
    }
  }, note));
}
Object.assign(__ds_scope, { Masthead });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/os-chrome/Masthead.jsx", error: String((e && e.message) || e) }); }

// components/os-chrome/OsRail.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* His machine, not an institution's. 34px raised chrome: identity mark, a sticker he put there,
   clock, battery. 12px type only — the rail carries no product meaning. */
function OsRail({
  identity = "NOTEBOOK",
  sticker = "property of nobody",
  clock = "02:47",
  batteryLow = true,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "var(--st-band-rail)",
      background: "var(--ground-3)",
      borderBottom: "var(--rule-w) solid var(--rule)",
      display: "flex",
      alignItems: "center",
      padding: "0 var(--st-rail-pad-x)",
      gap: "11px",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "7px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: "11px",
      height: "11px",
      background: "var(--toner-3)"
    }
  }), /*#__PURE__*/React.createElement("b", {
    style: {
      fontSize: "var(--st-size-chrome)",
      letterSpacing: ".13em",
      fontWeight: 600,
      color: "var(--toner-2)"
    }
  }, identity)), sticker && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-chrome)",
      letterSpacing: ".09em",
      padding: "2px 6px",
      color: "var(--biro)",
      border: "var(--rule-w) solid var(--biro-deep)",
      transform: "rotate(-.6deg)",
      whiteSpace: "nowrap"
    }
  }, sticker), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      display: "flex",
      alignItems: "center",
      gap: "13px",
      fontSize: "var(--st-size-chrome)",
      letterSpacing: ".1em",
      color: "var(--toner-2)",
      fontVariantNumeric: "tabular-nums"
    }
  }, /*#__PURE__*/React.createElement("span", null, clock), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "20px",
      height: "9px",
      border: "var(--rule-w) solid var(--toner-3)",
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      top: "1.5px",
      bottom: "1.5px",
      left: "1.5px",
      width: "5px",
      background: batteryLow ? "var(--stamp)" : "var(--toner-3)"
    }
  }))));
}
Object.assign(__ds_scope, { OsRail });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/os-chrome/OsRail.jsx", error: String((e && e.message) || e) }); }

// components/os-chrome/OsTray.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* 34px raised tray. Other apps on his machine, plus non-product facts. 12px is legal here and only
   here — nothing in this band may state a product fact. */
function OsTray({
  apps = [{
    label: "SURETHING",
    active: true
  }, {
    label: "LEDGER"
  }, {
    label: "MESSAGES",
    badge: "1"
  }],
  facts = ["DISK 61% FULL", "NO UPDATES AVAILABLE"],
  onSelect,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "var(--st-band-tray)",
      background: "var(--ground-3)",
      borderTop: "var(--rule-w) solid var(--rule)",
      display: "flex",
      alignItems: "center",
      padding: "0 var(--st-rail-pad-x)",
      gap: "6px",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), apps.map(a => /*#__PURE__*/React.createElement("button", {
    key: a.label,
    type: "button",
    onClick: onSelect && (() => onSelect(a.label)),
    style: {
      height: "var(--st-tray-app-h)",
      padding: "0 10px",
      display: "flex",
      alignItems: "center",
      gap: "6px",
      fontSize: "var(--st-size-chrome)",
      letterSpacing: ".09em",
      color: a.active ? "var(--toner)" : "var(--toner-3)",
      border: "var(--rule-w) solid " + (a.active ? "var(--rule)" : "var(--rule-soft)"),
      borderRadius: "var(--radius)",
      background: a.active ? "var(--ground)" : "transparent",
      fontFamily: "var(--font-data)",
      cursor: "pointer"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      width: "5px",
      height: "5px",
      background: a.active ? "var(--wax)" : "var(--toner-3)"
    }
  }), a.label, a.badge && /*#__PURE__*/React.createElement("span", {
    style: {
      background: "var(--stamp)",
      color: "var(--toner)",
      fontSize: "var(--st-size-chrome)",
      padding: "0 5px"
    }
  }, a.badge))), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      fontSize: "var(--st-size-chrome)",
      letterSpacing: ".13em",
      color: "var(--toner-3)",
      display: "flex",
      gap: "14px"
    }
  }, facts.map(f => /*#__PURE__*/React.createElement("span", {
    key: f
  }, f))));
}
Object.assign(__ds_scope, { OsTray });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/os-chrome/OsTray.jsx", error: String((e && e.message) || e) }); }

// components/os-chrome/SectionTabs.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* 38px recessed strip. The active tab joins the document ground; inactive tabs are ruled and muted.
   Runtime mapping: Lobby -> FORM, Detail -> ENTRY, MyBets -> MY BETS, Rewards -> REWARDS. */
function SectionTabs({
  tabs = ["FORM", "ENTRY", "MY BETS", "REWARDS"],
  active = "FORM",
  meta = "SHEET 1 OF 1",
  onSelect,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "var(--st-band-tabs)",
      display: "flex",
      alignItems: "flex-end",
      gap: "2px",
      padding: "0 var(--st-pad-x)",
      background: "var(--ground-2)",
      borderBottom: "var(--rule-w-strong) solid var(--rule)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), tabs.map(t => {
    const on = t === active;
    return /*#__PURE__*/React.createElement("button", {
      key: t,
      type: "button",
      onClick: onSelect && (() => onSelect(t)),
      "aria-current": on ? "page" : undefined,
      style: {
        height: "var(--st-tab-h)",
        padding: "0 15px",
        display: "flex",
        alignItems: "center",
        background: on ? "var(--ground)" : "transparent",
        border: "var(--rule-w) solid " + (on ? "var(--rule)" : "var(--rule-soft)"),
        borderBottom: 0,
        borderRadius: "var(--radius)",
        color: on ? "var(--toner)" : "var(--toner-3)",
        fontSize: "var(--st-size-fact)",
        letterSpacing: "var(--st-track-tab)",
        fontFamily: "var(--font-data)",
        cursor: "pointer",
        whiteSpace: "nowrap",
        boxShadow: on ? "0 2px 0 var(--ground)" : "none"
      }
    }, t);
  }), meta && /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      alignSelf: "center",
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-label)",
      color: "var(--toner-3)",
      whiteSpace: "nowrap"
    }
  }, meta));
}
Object.assign(__ds_scope, { SectionTabs });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/os-chrome/SectionTabs.jsx", error: String((e && e.message) || e) }); }

// components/records/LedgerEntry.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Old Slips / Ledger: the read-only settled record, in the same form grammar as MY BETS but with no
   live styling, no action buttons, and no watch-the-TV instruction. Terminal states are literal words. */
function LedgerEntry({
  number,
  legs,
  terminal,
  stake,
  payout,
  style,
  ...rest
}) {
  const won = terminal === "WON" || terminal === "CASHED OUT";
  const key = {
    fontSize: "var(--st-size-fact)",
    letterSpacing: "var(--st-track-label)",
    color: "var(--toner-3)",
    whiteSpace: "nowrap"
  };
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "16px",
      padding: "11px var(--st-pad-x)",
      borderBottom: "var(--rule-w) solid var(--rule-soft)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "112px",
      flex: "none",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: "var(--toner-2)",
      textTransform: "uppercase",
      whiteSpace: "nowrap",
      fontVariantNumeric: "tabular-nums"
    }
  }, number), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0,
      fontSize: "var(--st-size-fact)",
      color: "var(--toner-2)",
      letterSpacing: ".02em"
    }
  }, legs), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "96px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: key
  }, "STAKE"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: "var(--toner)",
      fontVariantNumeric: "tabular-nums"
    }
  }, stake)), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "104px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: key
  }, "RETURNED"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: won ? "var(--wax)" : "var(--toner-3)",
      fontVariantNumeric: "tabular-nums"
    }
  }, payout)), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "104px",
      flex: "none",
      textAlign: "right",
      whiteSpace: "nowrap",
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-action)",
      color: won ? "var(--wax)" : "var(--toner-3)",
      textDecoration: won ? "none" : "line-through",
      textDecorationColor: "var(--stamp)"
    }
  }, terminal));
}
Object.assign(__ds_scope, { LedgerEntry });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/records/LedgerEntry.jsx", error: String((e && e.message) || e) }); }

// components/records/OfferEntry.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* A REWARDS offer as a ruled form entry: name, literal description, price, affordability, buy.
   Affordability is never colour-only, and an unavailable purchase states the real engine reason. */
function OfferEntry({
  name,
  description,
  price,
  affordable = true,
  owned = false,
  reason,
  onBuy,
  style,
  ...rest
}) {
  const [hover, setHover] = React.useState(false);
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "14px",
      padding: "13px var(--st-pad-x)",
      borderBottom: "var(--rule-w) solid var(--rule-soft)",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: "9px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-price)",
      letterSpacing: "var(--st-track-name)",
      color: "var(--toner)",
      textTransform: "uppercase",
      whiteSpace: "nowrap"
    }
  }, name), owned && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".08em",
      color: "var(--biro)",
      border: "var(--rule-w) solid var(--biro-deep)",
      padding: "0 5px"
    }
  }, "HELD")), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      lineHeight: "var(--st-lh-copy)",
      color: "var(--toner-2)",
      marginTop: "3px",
      maxWidth: "44ch"
    }
  }, description)), /*#__PURE__*/React.createElement("div", {
    style: {
      textAlign: "right",
      width: "92px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-label)",
      color: "var(--toner-3)"
    }
  }, "PRICE"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-price)",
      color: "var(--wax)",
      fontVariantNumeric: "tabular-nums"
    }
  }, price)), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "132px",
      display: "flex",
      flexDirection: "column",
      alignItems: "flex-end",
      gap: "4px"
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: affordable && !owned ? onBuy : undefined,
    disabled: !affordable || owned,
    onMouseEnter: () => setHover(true),
    onMouseLeave: () => setHover(false),
    style: {
      height: "var(--target-min-h)",
      minWidth: "var(--target-min-w)",
      padding: "0 14px",
      background: "transparent",
      border: "var(--rule-w) solid " + (affordable && !owned && hover ? "var(--wax)" : "var(--rule)"),
      borderRadius: "var(--radius)",
      color: !affordable || owned ? "var(--toner-3)" : hover ? "var(--wax-lit)" : "var(--toner)",
      fontFamily: "var(--font-data)",
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".08em",
      cursor: !affordable || owned ? "not-allowed" : "pointer"
    }
  }, owned ? "HELD" : "BUY"), !affordable && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: ".04em",
      color: "var(--stamp)",
      whiteSpace: "nowrap",
      border: "var(--rule-w) solid var(--stamp)",
      padding: "0 5px"
    }
  }, reason || "BANK TOO LOW")));
}
Object.assign(__ds_scope, { OfferEntry });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/records/OfferEntry.jsx", error: String((e && e.message) || e) }); }

// components/records/RevealedState.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Literal revealed-state words. GREEN and DEAD are WORDS, not colour claims — the state survives
   with the colour removed. Only ever sourced from TvSweatScreen.RevealedView, never from the engine.
   The won mark is sized from the word's own measured box plus the 8px-per-edge overshoot, because a
   ring sized to a container lands its widest point on the last letter. */
const TONE = {
  PENDING: "var(--toner-3)",
  LIVE: "var(--toner)",
  GREEN: "var(--wax)",
  DEAD: "var(--toner-3)",
  VOID: "var(--toner-2)",
  "CASHED OUT": "var(--wax)"
};
function RevealedState({
  state = "PENDING",
  inkBase,
  style,
  ...rest
}) {
  const ref = React.useRef(null);
  const [box, setBox] = React.useState(null);
  React.useLayoutEffect(() => {
    if (!ref.current) return;
    const r = ref.current.getBoundingClientRect();
    setBox({
      w: Math.round(r.width),
      h: Math.round(r.height)
    });
  }, [state]);
  const marked = state === "GREEN" || state === "DEAD";
  const rect = box ? __ds_scope.InkMark.rect(box.w, box.h) : null;
  return /*#__PURE__*/React.createElement("span", _extends({}, rest, {
    style: {
      position: "relative",
      display: "inline-block",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("span", {
    ref: ref,
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      letterSpacing: "var(--st-track-action)",
      textTransform: "uppercase",
      color: TONE[state] || "var(--toner)",
      display: "inline-block"
    }
  }, state), marked && rect && /*#__PURE__*/React.createElement(__ds_scope.InkMark, {
    base: inkBase,
    variant: state === "DEAD" ? "strike" : "ring-a",
    color: state === "DEAD" ? "var(--stamp)" : "var(--wax)",
    width: rect.width,
    height: rect.height,
    style: {
      position: "absolute",
      left: rect.left + "px",
      top: rect.top + "px"
    }
  }));
}
Object.assign(__ds_scope, { RevealedState });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/records/RevealedState.jsx", error: String((e && e.message) || e) }); }

// components/records/RevealedLeg.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* A leg row in the MY BETS mirror. Read-only. Live legs keep document/biro treatment; a dead entry
   dims toward the ground. No score, clock, probability, next event, or unrevealed result. */
function RevealedLeg({
  team,
  price,
  market,
  state = "PENDING",
  inkBase,
  style,
  ...rest
}) {
  const dead = state === "DEAD";
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "12px",
      padding: "11px 0",
      borderBottom: "var(--rule-w) solid var(--rule-soft)",
      opacity: dead ? .55 : 1,
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "15px",
      flex: "none",
      color: "var(--biro)",
      fontSize: "15px",
      lineHeight: 1
    }
  }, "\u2713"), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: state === "GREEN" ? "var(--wax)" : dead ? "var(--toner-3)" : "var(--toner)",
      textTransform: "uppercase"
    }
  }, team), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--st-size-fact)",
      letterSpacing: "var(--st-track-rec)",
      whiteSpace: "nowrap",
      color: "var(--toner-3)",
      marginTop: "3px"
    }
  }, market)), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: "var(--toner-2)",
      fontVariantNumeric: "tabular-nums",
      width: "56px",
      textAlign: "right"
    }
  }, price), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "116px",
      textAlign: "right"
    }
  }, /*#__PURE__*/React.createElement(__ds_scope.RevealedState, {
    state: state,
    inkBase: inkBase
  })));
}
Object.assign(__ds_scope, { RevealedLeg });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/records/RevealedLeg.jsx", error: String((e && e.message) || e) }); }

// components/records/TicketReceipt.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* A placed ticket, printed as a numbered form receipt. It renders Run.Tickets and invents nothing —
   no date, no settlement, no outcome that the source did not supply. */
function TicketReceipt({
  number,
  legs = [],
  stake,
  combined,
  payout,
  state,
  inkBase,
  style,
  ...rest
}) {
  const key = {
    fontSize: "var(--st-size-fact)",
    letterSpacing: "var(--st-track-label)",
    color: "var(--toner-3)",
    whiteSpace: "nowrap"
  };
  const val = {
    fontFamily: "var(--font-cond)",
    fontSize: "var(--st-size-leg)",
    color: "var(--toner)",
    fontVariantNumeric: "tabular-nums"
  };
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      border: "var(--rule-w) solid var(--rule)",
      background: "var(--ground-2)",
      padding: "11px 13px",
      fontFamily: "var(--font-data)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: "10px",
      paddingBottom: "8px",
      borderBottom: "var(--rule-w) solid var(--rule)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      letterSpacing: "var(--st-track-action)",
      color: "var(--toner)",
      textTransform: "uppercase"
    }
  }, number), /*#__PURE__*/React.createElement("div", {
    style: {
      ...key,
      marginLeft: "auto"
    }
  }, legs.length, " ", legs.length === 1 ? "LEG" : "LEGS"), state && /*#__PURE__*/React.createElement(__ds_scope.RevealedState, {
    state: state,
    inkBase: inkBase,
    style: {
      marginLeft: "6px"
    }
  })), legs.map((l, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: "8px",
      padding: "6px 0",
      borderBottom: i === legs.length - 1 ? "0" : "var(--rule-w) dotted var(--rule-soft)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: "var(--st-size-leg)",
      color: "var(--toner)",
      textTransform: "uppercase"
    }
  }, l.team), /*#__PURE__*/React.createElement("div", {
    style: {
      ...key,
      letterSpacing: "var(--st-track-rec)"
    }
  }, l.market), /*#__PURE__*/React.createElement("div", {
    style: {
      ...val,
      marginLeft: "auto"
    }
  }, l.price))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: "18px",
      marginTop: "9px",
      paddingTop: "9px",
      borderTop: "var(--rule-w) solid var(--rule)"
    }
  }, /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: key
  }, "STAKE"), /*#__PURE__*/React.createElement("div", {
    style: val
  }, stake)), /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: key
  }, "COMBINED"), /*#__PURE__*/React.createElement("div", {
    style: val
  }, combined)), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      textAlign: "right"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: key
  }, "PAYS"), /*#__PURE__*/React.createElement("div", {
    style: {
      ...val,
      color: "var(--wax)"
    }
  }, payout))));
}
Object.assign(__ds_scope, { TicketReceipt });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/records/TicketReceipt.jsx", error: String((e && e.message) || e) }); }

// components/tv/tiers.js
try { (() => {
/* The TV's one law: brightness is the primary semantic channel, hue is secondary.
   At most ONE L4 element exists on the surface at any instant. If two things want full brightness,
   the design has not decided what matters. */
const TIER = {
  L4: 1,
  L3: 0.7,
  L2: 0.4,
  L1: 0.15,
  L0: 0
};
const tier = level => TIER[level] == null ? 1 : TIER[level];
Object.assign(__ds_scope, { TIER, tier });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/tiers.js", error: String((e && e.message) || e) }); }

// components/tv/TvCashOutSlot.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* One fixed rectangle owning all six states. It never reflows: reserved space stays reserved and
   simply goes dark. THE BRIGHTNESS OF THIS SLOT IS A PROMISE ABOUT INPUT — L4 means the key works
   right now. If the slot is bright and the press does nothing, the surface has lied. */
function TvCashOutSlot({
  state = "actionable",
  amount,
  keyHint = "[E]",
  style,
  ...rest
}) {
  const inverted = state === "actionable";
  const money = state === "actionable" ? "CASH OUT " + amount : state === "updating" ? "CASH OUT " + amount : state === "accepted" ? "CASHED OUT " + amount : null;
  /* The status word rides at label scale beside the figure, never at money scale — the rectangle is
     fixed and its copy is ONE line. Suspended and unavailable carry no amount, so their whole copy
     sits at label scale: L1, quiet, no reflow, explaining an absence. */
  const status = state === "actionable" ? keyHint : state === "updating" ? "UPDATING" : state === "suspended" || state === "pending" ? "MARKET SUSPENDED" : state === "accepted" ? null : "CASH OUT UNAVAILABLE";
  const level = inverted ? "L4" : state === "updating" || state === "accepted" ? "L3" : "L1";
  const hue = inverted ? "var(--tv-gold-ink)" : state === "updating" || state === "accepted" ? "var(--tv-gold)" : "var(--tv-context)";
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "58px",
      flex: "none",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      gap: "10px",
      overflow: "hidden",
      padding: "0 12px",
      background: inverted ? "var(--tv-gold)" : "transparent",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)",
      opacity: inverted ? 1 : __ds_scope.tier(level) + 0.3,
      fontFamily: "var(--font-tv-cond)",
      fontVariantNumeric: "tabular-nums",
      ...style
    }
  }), money && /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--tv-size-cashout)",
      fontWeight: 700,
      letterSpacing: "var(--tv-track-name)",
      color: hue,
      textTransform: "uppercase",
      whiteSpace: "nowrap",
      lineHeight: 1
    }
  }, money), status && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-eyebrow)",
      fontWeight: 500,
      letterSpacing: "var(--tv-track-label)",
      color: hue,
      textTransform: "uppercase",
      whiteSpace: "nowrap",
      lineHeight: 1
    }
  }, status));
}
Object.assign(__ds_scope, { TvCashOutSlot });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvCashOutSlot.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvEventStrip.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* One line, white, L2 at rest, punching to L3 at its reveal callback and settling back.
   Explanation, not commentary theatre. It never uses money hues and never covers the pitch. */
function TvEventStrip({
  text,
  punched = false,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      padding: "10px 16px",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)",
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-event)",
      fontWeight: 600,
      color: "var(--tv-fact)",
      opacity: punched ? __ds_scope.tier("L3") : __ds_scope.tier("L2"),
      letterSpacing: "var(--tv-track-name)",
      textTransform: "uppercase",
      transition: "opacity var(--tv-dur-event) var(--tv-step)",
      whiteSpace: "nowrap",
      overflow: "hidden",
      textOverflow: "ellipsis",
      ...style
    }
  }), text);
}
Object.assign(__ds_scope, { TvEventStrip });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvEventStrip.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvLegRow.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* One row per leg, in ticket order, brightness carrying the state. Multiple legs can be live at once:
   L3 is a tier, not a slot. Live rows expand in place; rows below are pushed, never reordered.
   Resolved and pending rows compress to a single line.

   The headline is ALWAYS the market statement, in every state — VISUAL-DESIGN §6 prints it verbatim
   ("NORTHSIDE TO WIN", "OVER 2.5 GOALS", "MARCUS VALE TO SCORE") and §3 calls that line the active
   NEED statement, so the two are one slot, not two. It may wrap to two lines; nothing else in the
   rail wraps. Never paraphrase it: it carries the team name, which §4 names as the primary carrier
   of team identity, and the market's own line value. */
const LEVEL = {
  NEXT: "L1",
  LIVE: "L3",
  W: "L3",
  L: "L0",
  VOID: "L2"
};
function TvLegRow({
  market,
  price,
  statement,
  state = "NEXT",
  progress,
  expanded,
  style,
  ...rest
}) {
  const level = LEVEL[state] || "L2";
  const hue = state === "W" ? "var(--tv-gold)" : state === "VOID" ? "var(--tv-void)" : "var(--tv-fact)";
  const open = expanded == null ? state === "LIVE" : expanded;
  const dead = state === "L";
  const dim = dead ? __ds_scope.tier("L1") : __ds_scope.tier(level);
  const meta = {
    fontFamily: "var(--font-tv)",
    fontSize: "var(--tv-size-eyebrow)",
    letterSpacing: ".1em",
    color: "var(--tv-context)",
    whiteSpace: "nowrap",
    opacity: dead ? __ds_scope.tier("L1") : __ds_scope.tier("L2")
  };
  const stateChip = {
    fontFamily: "var(--font-tv)",
    fontSize: "var(--tv-size-eyebrow)",
    letterSpacing: ".1em",
    color: hue,
    whiteSpace: "nowrap",
    opacity: dim,
    textDecoration: state === "VOID" ? "line-through" : "none",
    minWidth: "38px",
    textAlign: "right"
  };
  const shell = {
    padding: open ? "10px 16px" : "7px 16px",
    borderBottom: "var(--tv-rule-w) solid var(--tv-rule)",
    fontFamily: "var(--font-tv-cond)",
    fontVariantNumeric: "tabular-nums",
    background: dead ? "var(--tv-extinguished)" : "transparent",
    ...style
  };

  /* A resolved or pending leg is ONE line: eyebrow, statement, price, state. Vertical budget goes to
     what is live, and a won leg does not need the same height as a leg still in play. */
  if (!open) {
    return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
      style: shell
    }), /*#__PURE__*/React.createElement("div", {
      style: {
        display: "flex",
        alignItems: "baseline",
        gap: "8px"
      }
    }, /*#__PURE__*/React.createElement("span", {
      style: meta
    }, market), /*#__PURE__*/React.createElement("span", {
      style: {
        flex: 1,
        minWidth: 0,
        fontSize: "var(--tv-size-leg)",
        fontWeight: 700,
        color: hue,
        opacity: dim,
        textTransform: "uppercase",
        letterSpacing: "var(--tv-track-name)",
        whiteSpace: "nowrap",
        overflow: "hidden",
        textOverflow: "ellipsis"
      }
    }, statement), /*#__PURE__*/React.createElement("span", {
      style: {
        ...meta,
        fontFamily: "var(--font-tv-cond)",
        fontSize: "var(--tv-size-leg)"
      }
    }, price), /*#__PURE__*/React.createElement("span", {
      style: stateChip
    }, state)));
  }
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: shell
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: "8px"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: meta
  }, market), /*#__PURE__*/React.createElement("span", {
    style: {
      ...meta,
      marginLeft: "auto",
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-leg)"
    }
  }, price), /*#__PURE__*/React.createElement("span", {
    style: stateChip
  }, state)), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--tv-size-need)",
      fontWeight: 700,
      color: hue,
      opacity: dim,
      marginTop: "4px",
      textTransform: "uppercase",
      letterSpacing: "var(--tv-track-name)",
      lineHeight: 1.08
    }
  }, statement), progress && /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-progress)",
      fontWeight: 700,
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L3"),
      marginTop: "4px",
      letterSpacing: ".02em",
      whiteSpace: "nowrap",
      lineHeight: 1.1
    }
  }, progress));
}
Object.assign(__ds_scope, { TvLegRow });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvLegRow.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvMomentumTape.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* RULED IN 2026-07-31 (DD, T16). The tape stays because PRD §4.2 names it in the one-revealed-
   source-of-truth law — it is a revealed channel, and dropping it would silently narrow that law.
   The win-probability NUMERAL is ruled out separately; this tape carries no numerals, and the moment
   it needs one it has become that banned readout.

   It sits at the foot of the scorebug, spanning the stage: the scorebug is where match truth lives,
   and momentum is match truth over time. It renders at L1-L2 and is colourless — white and grey only
   — so it can never compete with the score above it or the NEED line beside it.

   Tiering, per §3: the eyebrow is a LABEL, and labels are L2 ("context that must be readable but is
   not the subject"). L1 is the dormant tier — structure and the not-yet — and a label sitting there
   composites to 1.17:1 on this substrate, which is not readable at four metres in a dark room. The
   bars are the exception that proves it: history is genuinely past, so it sits at L1 as structure,
   and only the current sample rises to L2. */
function TvMomentumTape({
  samples = [],
  label = "MOMENTUM",
  style,
  ...rest
}) {
  const last = samples.length - 1;
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      height: "28px",
      display: "flex",
      alignItems: "center",
      gap: "10px",
      padding: "0 16px",
      borderBottom: "var(--tv-rule-w) solid var(--tv-rule)",
      fontFamily: "var(--font-tv)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2"),
      whiteSpace: "nowrap"
    }
  }, label), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      position: "relative",
      height: "16px",
      display: "flex",
      alignItems: "center",
      gap: "2px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      left: 0,
      right: 0,
      top: "50%",
      height: "1px",
      background: "var(--tv-structure)",
      opacity: __ds_scope.tier("L1")
    }
  }), samples.map((s, i) => {
    const v = Math.max(-1, Math.min(1, s));
    const h = Math.max(2, Math.round(Math.abs(v) * 7));
    return /*#__PURE__*/React.createElement("div", {
      key: i,
      style: {
        position: "relative",
        flex: 1,
        height: "16px",
        display: "flex",
        alignItems: v >= 0 ? "flex-start" : "flex-end"
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        width: "100%",
        height: h + "px",
        marginTop: v >= 0 ? 8 - h + "px" : 0,
        marginBottom: v < 0 ? 8 - h + "px" : 0,
        background: i === last ? "var(--tv-fact)" : "var(--tv-context)",
        opacity: i === last ? __ds_scope.tier("L2") : __ds_scope.tier("L1")
      }
    }));
  })));
}
Object.assign(__ds_scope, { TvMomentumTape });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvMomentumTape.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvRiskPays.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* One risk figure and one payout figure, at the foot of the ticket column, in gold at L2.
   PRD 8.4: ticket-level, never per leg — the approved concept render got this wrong. */
function TvRiskPays({
  risk,
  pays,
  style,
  ...rest
}) {
  const cell = (k, v) => /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2")
    }
  }, k), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-risk)",
      fontWeight: 700,
      color: "var(--tv-gold)",
      opacity: __ds_scope.tier("L2"),
      fontVariantNumeric: "tabular-nums"
    }
  }, v));
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      justifyContent: "space-between",
      padding: "12px 16px",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)",
      ...style
    }
  }), cell("RISK", risk), cell("PAYS", pays));
}
Object.assign(__ds_scope, { TvRiskPays });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvRiskPays.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvScorebug.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Compact scorebug at the top of the right region. Team names in their hues either side of white
   tabular figures, clock at the far right, ticket/leg index at L1. Records do not appear live.
   The score is the largest element on the surface at all times. Nothing outgrows it.

   §3 names the score at a goal as one of the surface's L4 occupants, so `goal` punches it to full
   brightness for that callback. Per the C3 coverage ruling (2026-07-31) a momentary punch transiently
   preempts a sustained L4: at a goal the cash-out is legitimately re-pricing and sits at L3, so the
   one-L4-at-any-instant invariant holds without contrivance. */
function TvScorebug({
  ticket,
  leg,
  away,
  home,
  score = [0, 0],
  clock = "PRE",
  backed = null,
  marketPick = false,
  goal = false,
  style,
  ...rest
}) {
  const name = (t, hue, isBacked) => /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "8px",
      minWidth: 0
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-team)",
      color: hue,
      opacity: __ds_scope.tier("L3"),
      textTransform: "uppercase",
      letterSpacing: "var(--tv-track-name)",
      whiteSpace: "nowrap"
    }
  }, t), isBacked && /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-eyebrow)",
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L2"),
      letterSpacing: "var(--tv-track-label)",
      border: "var(--tv-rule-w) solid currentColor",
      padding: "0 5px"
    }
  }, "BACKED"));
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "var(--tv-gutter)",
      padding: "10px 16px",
      borderBottom: "var(--tv-rule-w) solid var(--tv-rule)",
      fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-structure)",
      opacity: __ds_scope.tier("L1"),
      whiteSpace: "nowrap"
    }
  }, "TICKET ", ticket, " \u2022 LEG ", leg), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      display: "flex",
      alignItems: "center",
      gap: "18px"
    }
  }, name(away, "var(--tv-team-a)", backed === "away"), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-score)",
      fontWeight: 700,
      color: "var(--tv-fact)",
      opacity: goal ? __ds_scope.tier("L4") : __ds_scope.tier("L3"),
      transition: "opacity var(--tv-dur-punch) var(--tv-step)",
      letterSpacing: ".02em",
      whiteSpace: "nowrap"
    }
  }, score[0], " \u2014 ", score[1]), name(home, "var(--tv-team-b)", backed === "home")), /*#__PURE__*/React.createElement("div", {
    style: {
      marginLeft: "auto",
      fontSize: "var(--tv-size-clock)",
      fontWeight: 700,
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L3")
    }
  }, clock), marketPick && /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2"),
      whiteSpace: "nowrap"
    }
  }, "MARKET PICK"));
}
Object.assign(__ds_scope, { TvScorebug });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvScorebug.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvStage.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The theatre stage: fixed top-down pitch, picked team attacks right, camera never moves.
   The pitch is a PLACE, not an event — markings sit at L1-L2. Actors are single lit cells in team
   hue at L3; the ball is the only object permitted L4, and only at a payoff. */
function TvStage({
  actors = [],
  ball,
  attackingRight = true,
  style,
  ...rest
}) {
  const mark = {
    position: "absolute",
    border: "var(--tv-rule-w) solid var(--tv-pitch)",
    opacity: __ds_scope.tier("L2")
  };
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      position: "relative",
      flex: 1,
      minHeight: "180px",
      overflow: "hidden",
      background: "var(--tv-substrate)",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      ...mark,
      inset: "12px"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      ...mark,
      left: "50%",
      top: "12px",
      bottom: "12px",
      borderWidth: "0 0 0 var(--tv-rule-w)"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      ...mark,
      left: "50%",
      top: "50%",
      width: "72px",
      height: "72px",
      borderRadius: "50%",
      transform: "translate(-50%,-50%)"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      ...mark,
      left: "12px",
      top: "50%",
      width: "58px",
      height: "132px",
      transform: "translateY(-50%)"
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      ...mark,
      right: "12px",
      top: "50%",
      width: "58px",
      height: "132px",
      transform: "translateY(-50%)"
    }
  }), actors.map((a, i) => /*#__PURE__*/React.createElement("div", {
    key: i,
    style: {
      position: "absolute",
      left: a.x + "%",
      top: a.y + "%",
      width: a.number ? "14px" : "8px",
      height: a.number ? "14px" : "8px",
      transform: "translate(-50%,-50%)",
      background: a.team === "b" ? "var(--tv-team-b)" : "var(--tv-team-a)",
      opacity: __ds_scope.tier("L3"),
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
      fontFamily: "var(--font-tv)",
      fontSize: "10px",
      color: "var(--tv-substrate)",
      fontWeight: 700
    }
  }, a.number)), ball && /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      left: ball.x + "%",
      top: ball.y + "%",
      width: "7px",
      height: "7px",
      transform: "translate(-50%,-50%)",
      background: "var(--tv-fact)",
      opacity: ball.payoff ? __ds_scope.tier("L4") : __ds_scope.tier("L3")
    }
  }), !attackingRight && /*#__PURE__*/React.createElement("div", {
    style: {
      position: "absolute",
      left: "16px",
      bottom: "16px",
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L1")
    }
  }, "\u25C4 ATTACKING"));
}
Object.assign(__ds_scope, { TvStage });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvStage.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvStatsPanel.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* Opens from the head of the ticket column and FREEZES PLAYBACK. It expands over the ticket column
   and stage without moving either — when it closes, everything beneath is exactly where it was.
   All values are revealed-ledger values only. */
function TvStatsPanel({
  title = "MATCH STATS",
  away,
  home,
  rows = [],
  onClose,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      position: "absolute",
      inset: 0,
      background: "var(--tv-panel)",
      display: "flex",
      flexDirection: "column",
      fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: "12px",
      padding: "12px 16px",
      borderBottom: "var(--tv-rule-w) solid var(--tv-rule)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L3")
    }
  }, title), /*#__PURE__*/React.createElement("span", {
    style: {
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2")
    }
  }, "PLAYBACK FROZEN"), onClose && /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onClose,
    style: {
      marginLeft: "auto",
      background: "transparent",
      border: "var(--tv-rule-w) solid var(--tv-rule)",
      color: "var(--tv-context)",
      fontFamily: "var(--font-tv)",
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      padding: "3px 9px",
      cursor: "pointer"
    }
  }, "CLOSE")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      padding: "8px 16px",
      gap: "12px"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-team-a)",
      opacity: __ds_scope.tier("L3")
    }
  }, away), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "160px",
      textAlign: "center",
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2")
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      textAlign: "right",
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-team-b)",
      opacity: __ds_scope.tier("L3")
    }
  }, home)), rows.map(r => /*#__PURE__*/React.createElement("div", {
    key: r.label,
    style: {
      display: "flex",
      alignItems: "center",
      padding: "9px 16px",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-leg)",
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L3")
    }
  }, r.away), /*#__PURE__*/React.createElement("div", {
    style: {
      width: "160px",
      textAlign: "center",
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2")
    }
  }, r.label), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      textAlign: "right",
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-leg)",
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L3")
    }
  }, r.home))));
}
Object.assign(__ds_scope, { TvStatsPanel });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvStatsPanel.jsx", error: String((e && e.message) || e) }); }

// components/tv/TvTicketCard.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
/* The ticket interstitial. The stage and active-leg card clear before it appears, and no score,
   clock, tape, event line, suspended label or prior offer remains. */
function TvTicketCard({
  heading,
  legs = [],
  risk,
  pays,
  style,
  ...rest
}) {
  return /*#__PURE__*/React.createElement("div", _extends({}, rest, {
    style: {
      position: "absolute",
      inset: 0,
      background: "var(--tv-substrate)",
      display: "flex",
      flexDirection: "column",
      alignItems: "center",
      justifyContent: "center",
      gap: "26px",
      padding: "0 48px",
      textAlign: "center",
      fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums",
      ...style
    }
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-score)",
      fontWeight: 700,
      letterSpacing: "var(--tv-track-name)",
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L3"),
      textTransform: "uppercase"
    }
  }, heading), /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--tv-size-leg)",
      letterSpacing: "var(--tv-track-name)",
      color: "var(--tv-fact)",
      opacity: __ds_scope.tier("L2"),
      textTransform: "uppercase",
      lineHeight: 1.6
    }
  }, legs.join("  •  ")), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: "72px",
      borderTop: "var(--tv-rule-w) solid var(--tv-rule)",
      paddingTop: "18px"
    }
  }, [["RISK", risk], ["PAYS", pays]].map(([k, v]) => /*#__PURE__*/React.createElement("div", {
    key: k
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: "var(--tv-size-eyebrow)",
      letterSpacing: "var(--tv-track-label)",
      color: "var(--tv-context)",
      opacity: __ds_scope.tier("L2")
    }
  }, k), /*#__PURE__*/React.createElement("div", {
    style: {
      fontFamily: "var(--font-tv-cond)",
      fontSize: "var(--tv-size-risk)",
      fontWeight: 700,
      color: "var(--tv-gold)",
      opacity: __ds_scope.tier("L3")
    }
  }, v)))));
}
Object.assign(__ds_scope, { TvTicketCard });
})(); } catch (e) { __ds_ns.__errors.push({ path: "components/tv/TvTicketCard.jsx", error: String((e && e.message) || e) }); }

// ui_kits/surething/app.jsx
try { (() => {
const {
  OsRail,
  SectionTabs,
  Masthead,
  OsTray
} = window.SureThingDesignSystem_6e1eb3;
const {
  WorkingMargin,
  PassiveMargin
} = window;
const {
  FormScreen,
  EntryScreen,
  MyBetsScreen,
  RewardsScreen,
  LedgerScreen
} = window;
const TABS = ["FORM", "ENTRY", "MY BETS", "REWARDS"];
const TARGET = 1900;
function App() {
  const [tab, setTab] = React.useState("FORM");
  const [tray, setTray] = React.useState("SURETHING");
  const [selections, setSelections] = React.useState({
    2: {
      side: "home",
      market: "MONEYLINE",
      team: "Bricklayers",
      price: "-260",
      entry: "02",
      id: 2
    },
    4: {
      side: "away",
      market: "MONEYLINE",
      team: "Longhaulers",
      price: "+180",
      entry: "04",
      id: 4
    }
  });
  const [stake, setStake] = React.useState(200);
  const [tickets, setTickets] = React.useState([]);
  const [detail, setDetail] = React.useState({
    matchup: window.STData.slate[1],
    destination: "GOALS"
  });
  const [locked, setLocked] = React.useState(false);
  const [revealed, setRevealed] = React.useState({});
  const [shopResult, setShopResult] = React.useState(null);
  const bank = 1340;
  const legs = Object.values(selections);
  const select = (m, side) => {
    const t = side === "away" ? m.away : m.home;
    setSelections(s => ({
      ...s,
      [m.id]: {
        id: m.id,
        side,
        market: "MONEYLINE",
        team: t.name,
        price: t.price,
        entry: m.no
      }
    }));
  };
  const selectMarket = mk => {
    const m = detail.matchup;
    setSelections(s => ({
      ...s,
      [m.id]: {
        id: m.id,
        side: null,
        market: detail.destination,
        line: mk.line,
        team: mk.line,
        price: mk.price,
        entry: m.no
      }
    }));
  };
  const remove = id => setSelections(s => {
    const n = {
      ...s
    };
    delete n[id];
    return n;
  });
  const fraction = f => {
    const pct = f === "MAX" ? 1 : parseInt(f, 10) / 100;
    setStake(Math.max(10, Math.round(bank * pct)));
  };
  const nudge = n => setStake(s => Math.max(10, Math.min(bank, s + (n.indexOf("+") >= 0 ? 10 : -10))));
  const place = () => {
    if (!legs.length) return;
    setTickets(t => t.concat([{
      number: "TICKET " + String(t.length + 1).padStart(2, "0"),
      legs: legs.map(l => ({
        team: l.team,
        market: l.market === "MONEYLINE" ? "MONEYLINE" : l.market,
        price: l.price
      })),
      stake: window.STMath.money(stake),
      combined: window.STMath.combinedAmerican(legs),
      payout: window.STMath.payout(legs, stake)
    }]));
    setSelections({});
  };
  const lock = () => {
    setLocked(true);
    setTab("MY BETS");
    const seq = [];
    tickets.forEach(t => t.legs.forEach((_, i) => seq.push(t.number + "/" + i)));
    seq.forEach((k, i) => {
      window.setTimeout(() => setRevealed(r => ({
        ...r,
        [k]: "LIVE"
      })), 700 + i * 900);
      window.setTimeout(() => setRevealed(r => ({
        ...r,
        [k]: i % 3 === 2 ? "DEAD" : "GREEN"
      })), 1500 + i * 900);
    });
  };
  const reset = () => {
    setLocked(false);
    setRevealed({});
    setTickets([]);
    setTab("FORM");
    setSelections({
      2: {
        side: "home",
        market: "MONEYLINE",
        team: "Bricklayers",
        price: "-260",
        entry: "02",
        id: 2
      },
      4: {
        side: "away",
        market: "MONEYLINE",
        team: "Longhaulers",
        price: "+180",
        entry: "04",
        id: 4
      }
    });
    setStake(200);
  };
  const figures = [{
    label: "BANK",
    value: window.STMath.money(bank)
  }, {
    label: "TARGET",
    value: window.STMath.money(TARGET),
    tone: "wax"
  }, {
    label: "RELICS",
    value: "2/5"
  }, {
    label: "TICKETS",
    value: tickets.length + "/3"
  }];
  const activeTab = tray === "LEDGER" ? "LEDGER" : tab;
  const body = () => {
    if (tray === "LEDGER") return /*#__PURE__*/React.createElement(LedgerScreen, null);
    if (tab === "ENTRY") return /*#__PURE__*/React.createElement(EntryScreen, {
      matchup: detail.matchup,
      destination: detail.destination,
      onDestination: d => setDetail(x => ({
        ...x,
        destination: d
      })),
      selection: selections[detail.matchup.id],
      onSelectMarket: selectMarket,
      onBack: () => setTab("FORM"),
      tickets: tickets
    });
    if (tab === "MY BETS") return /*#__PURE__*/React.createElement(MyBetsScreen, {
      tickets: tickets,
      revealed: revealed
    });
    if (tab === "REWARDS") return /*#__PURE__*/React.createElement(RewardsScreen, {
      result: shopResult,
      onBuy: o => setShopResult(o.name.toUpperCase() + " BOUGHT — BANK REDUCED BY " + o.price)
    });
    return /*#__PURE__*/React.createElement(FormScreen, {
      selections: selections,
      onSelect: select,
      onMore: m => {
        setDetail({
          matchup: m,
          destination: "GOALS"
        });
        setTab("ENTRY");
      }
    });
  };
  const margin = () => {
    if (tray === "LEDGER") return /*#__PURE__*/React.createElement(PassiveMargin, {
      title: "Record",
      rows: [{
        label: "TICKETS SETTLED",
        value: "3"
      }, {
        label: "STAKED",
        value: "$400"
      }, {
        label: "RETURNED",
        value: "$560",
        tone: "wax"
      }],
      note: "Read-only. The ledger copies settled tickets and derives nothing."
    });
    if (tab === "MY BETS") return /*#__PURE__*/React.createElement(PassiveMargin, {
      title: "This round",
      rows: [{
        label: "TICKETS",
        value: tickets.length + "/3"
      }, {
        label: "AT RISK",
        value: tickets.length ? tickets[0].stake : "$0"
      }, {
        label: "IF EVERYTHING LANDS",
        value: tickets.length ? tickets[0].payout : "$0",
        tone: "wax"
      }],
      note: "No stake, selection or lock controls during the sweat. The board is frozen."
    });
    if (tab === "REWARDS") return /*#__PURE__*/React.createElement(PassiveMargin, {
      title: "Run context",
      rows: [{
        label: "BANK",
        value: window.STMath.money(bank)
      }, {
        label: "RELICS HELD",
        value: "2/5"
      }, {
        label: "ROUND",
        value: "3 OF 8"
      }],
      note: "Buying updates the existing tally. Errors are literal and are not given a remedy the engine did not supply."
    });
    return /*#__PURE__*/React.createElement(WorkingMargin, {
      legs: legs,
      stake: stake,
      tickets: tickets,
      locked: locked,
      onRemove: remove,
      onFraction: fraction,
      onNudge: nudge,
      onPlace: place,
      onLock: lock,
      onSkip: () => setTickets([])
    });
  };
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 1024,
      height: 704,
      position: "relative",
      overflow: "hidden",
      background: "var(--ground)",
      color: "var(--toner)",
      boxShadow: "0 24px 64px rgba(0,0,0,.75)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    "aria-hidden": "true",
    style: {
      position: "absolute",
      inset: 0,
      pointerEvents: "none",
      zIndex: 9,
      opacity: "var(--toner-grain-opacity)",
      backgroundImage: "url(\"data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='140' height='140'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='3'/%3E%3C/filter%3E%3Crect width='140' height='140' filter='url(%23n)'/%3E%3C/svg%3E\")"
    }
  }), /*#__PURE__*/React.createElement(OsRail, {
    clock: "02:47"
  }), /*#__PURE__*/React.createElement(SectionTabs, {
    tabs: TABS,
    active: activeTab,
    onSelect: t => {
      setTray("SURETHING");
      setTab(t);
    },
    meta: tray === "LEDGER" ? "READ ONLY" : "SHEET 1 OF 1"
  }), /*#__PURE__*/React.createElement(Masthead, {
    title: tray === "LEDGER" ? "Ledger" : "SureThing Form",
    dateline: "ROUND 3 OF 8 · SEED 8F3K-22 · " + (tray === "LEDGER" ? "SETTLED RECORD" : locked ? "ROUND LOCKED" : "PRICES FINAL"),
    figures: figures,
    note: tray === "LEDGER" ? "Every ticket this run, after it settled. Nothing here can be changed." : locked ? "The board is frozen. The TV owns the rest." : "Prices are final. Nothing you do moves them."
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      height: 530,
      display: "flex",
      position: "relative",
      zIndex: 2
    }
  }, body(), margin()), /*#__PURE__*/React.createElement(OsTray, {
    apps: [{
      label: "SURETHING",
      active: tray === "SURETHING"
    }, {
      label: "LEDGER",
      active: tray === "LEDGER"
    }, {
      label: "MESSAGES",
      badge: "1"
    }],
    onSelect: l => {
      if (l === "LEDGER" || l === "SURETHING") setTray(l);
    }
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 1024,
      display: "flex",
      alignItems: "center",
      gap: 14,
      marginTop: 12,
      fontFamily: "var(--font-data)",
      fontSize: 12,
      letterSpacing: ".1em",
      color: "var(--toner-3)"
    }
  }, /*#__PURE__*/React.createElement("span", null, "1024 \xD7 704 \xB7 UNITY UGUI CANVAS ON A WORLD-SPACE LAPTOP \xB7 READ AT AN ANGLE"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: reset,
    style: {
      marginLeft: "auto",
      height: 26,
      padding: "0 12px",
      background: "transparent",
      border: "1px solid var(--rule)",
      color: "var(--toner-3)",
      fontFamily: "var(--font-data)",
      fontSize: 12,
      letterSpacing: ".1em",
      cursor: "pointer"
    }
  }, "RESET KIT")));
}
ReactDOM.createRoot(document.getElementById("root")).render(/*#__PURE__*/React.createElement(App, null));
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/surething/app.jsx", error: String((e && e.message) || e) }); }

// ui_kits/surething/betmath.js
try { (() => {
/* American odds helpers. In the real product /engine owns all of this and the UI never re-derives it;
   these exist only so the kit's figures move truthfully when you click. */
(function () {
  const dec = american => {
    const n = parseInt(String(american).replace("+", ""), 10);
    return n > 0 ? 1 + n / 100 : 1 + 100 / Math.abs(n);
  };
  const toAmerican = d => {
    if (d <= 1) return "—";
    const n = d >= 2 ? Math.round((d - 1) * 100) : -Math.round(100 / (d - 1));
    return (n > 0 ? "+" : "") + n;
  };
  const money = n => "$" + Math.round(n).toLocaleString("en-US");
  const combined = legs => legs.reduce((a, l) => a * dec(l.price), 1);
  window.STMath = {
    dec,
    toAmerican,
    money,
    combined,
    combinedAmerican: legs => legs.length ? toAmerican(combined(legs)) : "—",
    payout: (legs, stake) => legs.length ? money(combined(legs) * stake) : "$0"
  };
})();
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/surething/betmath.js", error: String((e && e.message) || e) }); }

// ui_kits/surething/data.js
try { (() => {
/* The Round 3 slate from SHARED-SPEC.md, plus the event-detail markets the engine already prices:
   Moneyline, TotalGoals, BothTeamsToScore, TotalCorners, TotalCards, AnytimeScorer.
   Fictional leagues, teams and players only — IP safety and the comedy both require it. */
window.STData = {
  slate: [{
    id: 1,
    no: "01",
    away: {
      name: "Yams",
      record: "7-2",
      price: "-145"
    },
    home: {
      name: "Startups",
      record: "4-5",
      price: "+125"
    }
  }, {
    id: 2,
    no: "02",
    away: {
      name: "Mallards",
      record: "3-6",
      price: "+210"
    },
    home: {
      name: "Bricklayers",
      record: "8-1",
      price: "-260"
    }
  }, {
    id: 3,
    no: "03",
    away: {
      name: "Nighthawks",
      record: "5-4",
      price: "+135"
    },
    home: {
      name: "Foundry",
      record: "6-3",
      price: "-155"
    }
  }, {
    id: 4,
    no: "04",
    away: {
      name: "Longhaulers",
      record: "6-3",
      price: "+180"
    },
    home: {
      name: "Tidewater",
      record: "2-7",
      price: "-215"
    }
  }, {
    id: 5,
    no: "05",
    away: {
      name: "Saltmen",
      record: "4-5",
      price: "-110"
    },
    home: {
      name: "Junction",
      record: "5-4",
      price: "-110"
    }
  }, {
    id: 6,
    no: "06",
    away: {
      name: "Kestrels",
      record: "8-1",
      price: "-300"
    },
    home: {
      name: "Pressmen",
      record: "3-6",
      price: "+240"
    }
  }],
  destinations: ["GOALS", "BTTS", "CORNERS", "CARDS", "PLAYERS"],
  markets: {
    GOALS: [{
      line: "Over 1.5 goals",
      price: "-190"
    }, {
      line: "Under 1.5 goals",
      price: "+155"
    }, {
      line: "Over 2.5 goals",
      price: "-110"
    }, {
      line: "Under 2.5 goals",
      price: "-110"
    }, {
      line: "Over 3.5 goals",
      price: "+185"
    }, {
      line: "Under 3.5 goals",
      price: "-225"
    }],
    BTTS: [{
      line: "Both teams to score — Yes",
      price: "-105"
    }, {
      line: "Both teams to score — No",
      price: "-115"
    }],
    CORNERS: [{
      line: "Over 8.5 corners",
      price: "+100"
    }, {
      line: "Under 8.5 corners",
      price: "-125"
    }, {
      line: "Over 10.5 corners",
      price: "+195"
    }, {
      line: "Under 10.5 corners",
      price: "-240"
    }],
    CARDS: [{
      line: "Over 3.5 cards",
      price: "-130"
    }, {
      line: "Under 3.5 cards",
      price: "+105"
    }, {
      line: "Over 4.5 cards",
      price: "+165"
    }, {
      line: "Under 4.5 cards",
      price: "-200"
    }],
    PLAYERS: [{
      line: "Marcus Vale anytime",
      price: "+210"
    }, {
      line: "Osric Kean anytime",
      price: "+265"
    }, {
      line: "Dennis Prole anytime",
      price: "+320"
    }, {
      line: "Ivo Tanager anytime",
      price: "+410"
    }]
  },
  offers: [{
    name: "Vig Rebate",
    price: "$140",
    description: "Returns 4% of every losing stake to the bank at round settlement.",
    affordable: true
  }, {
    name: "The Shill's Notebook",
    price: "$260",
    description: "Reveals one guru's pick per round. The guru is wrong more often than not.",
    affordable: true
  }, {
    name: "Late Line",
    price: "$420",
    description: "One selection per round may be swapped after the round is locked.",
    affordable: false,
    reason: "BANK TOO LOW"
  }, {
    name: "Accounting Error",
    price: "$180",
    description: "A missed target books the shortfall at 1.35x instead of 1.5x.",
    affordable: true,
    owned: true
  }],
  ledger: [{
    number: "R2 · TICKET 02",
    legs: "Kestrels ML −300 · Under 2.5 +105",
    terminal: "LOST",
    stake: "$180",
    payout: "$0"
  }, {
    number: "R2 · TICKET 01",
    legs: "Foundry ML −155 · BTTS Yes −105",
    terminal: "WON",
    stake: "$120",
    payout: "$396"
  }, {
    number: "R1 · TICKET 01",
    legs: "Junction ML −110 · Over 8.5 corners +100",
    terminal: "CASHED OUT",
    stake: "$100",
    payout: "$164"
  }]
};
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/surething/data.js", error: String((e && e.message) || e) }); }

// ui_kits/surething/margin.jsx
try { (() => {
/* The player margin — the right 324px, present on every surface. Its vertical order is fixed:
   header, legs, combined, stake, payout, actions. On MY BETS it becomes passive; on REWARDS it
   carries run context. It never turns into a floating drawer. */
const {
  MarginHeader,
  MarginLeg,
  MarginRow,
  StakeControls,
  PayoutFigure,
  PlaceAction,
  LockAction,
  SkipAction,
  RunFigure,
  StampReason
} = window.SureThingDesignSystem_6e1eb3;
const marginShell = {
  width: 324,
  display: "flex",
  flexDirection: "column",
  padding: "0 14px",
  background: "repeating-linear-gradient(180deg,transparent 0 25px,var(--rule-soft) 25px 26px)"
};
function WorkingMargin({
  legs,
  stake,
  tickets,
  onRemove,
  onFraction,
  onNudge,
  onPlace,
  onLock,
  onSkip,
  locked
}) {
  const combined = window.STMath.combinedAmerican(legs);
  const payout = window.STMath.payout(legs, stake);
  const lockReason = legs.length ? "PLACE OR CLEAR THIS WORKING SLIP" : tickets.length === 0 ? "PLACE AT LEAST ONE TICKET" : null;
  return /*#__PURE__*/React.createElement("div", {
    style: marginShell
  }, /*#__PURE__*/React.createElement(MarginHeader, {
    count: legs.length + (legs.length === 1 ? " SELECTION" : " SELECTIONS")
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      paddingTop: 4
    }
  }, legs.length === 0 && /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "14px 0",
      fontSize: 13,
      lineHeight: 1.5,
      color: "var(--toner-3)",
      letterSpacing: ".02em"
    }
  }, "No marks on this sheet. Circle a price to start a ticket."), legs.map(l => /*#__PURE__*/React.createElement(MarginLeg, {
    key: l.id,
    team: l.team,
    price: l.price,
    market: l.market,
    entry: l.entry,
    onRemove: () => onRemove(l.id)
  }))), /*#__PURE__*/React.createElement(MarginRow, {
    label: "COMBINED",
    value: combined
  }), /*#__PURE__*/React.createElement(StakeControls, {
    stake: window.STMath.money(stake),
    onFraction: onFraction,
    onNudge: onNudge
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: 10,
      paddingTop: 9,
      borderTop: "1px solid var(--rule)"
    }
  }, /*#__PURE__*/React.createElement(PayoutFigure, {
    value: payout,
    highlight: legs.length > 0
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: "auto",
      paddingBottom: 13,
      display: "flex",
      flexDirection: "column",
      gap: 6
    }
  }, locked ? /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "10px 0",
      display: "flex",
      flexDirection: "column",
      gap: 7
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      fontSize: 13,
      letterSpacing: ".12em",
      color: "var(--toner-3)"
    }
  }, "ROUND LOCKED"), /*#__PURE__*/React.createElement(StampReason, {
    reason: "BOARD FROZEN \u2014 WATCH THE TV"
  })) : /*#__PURE__*/React.createElement(React.Fragment, null, /*#__PURE__*/React.createElement(PlaceAction, {
    onClick: onPlace,
    disabled: legs.length === 0
  }), /*#__PURE__*/React.createElement(LockAction, {
    disabled: !!lockReason,
    reason: lockReason || undefined,
    onClick: onLock
  }), /*#__PURE__*/React.createElement(SkipAction, {
    onSkip: onSkip
  }))));
}
function PassiveMargin({
  title,
  rows,
  note,
  children
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: marginShell
  }, /*#__PURE__*/React.createElement(MarginHeader, {
    title: title
  }), /*#__PURE__*/React.createElement("div", {
    style: {
      paddingTop: 4
    }
  }, (rows || []).map(r => /*#__PURE__*/React.createElement(MarginRow, {
    key: r.label,
    label: r.label,
    value: r.value,
    tone: r.tone
  }))), children, note && /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: "auto",
      paddingBottom: 15,
      fontSize: 13,
      lineHeight: 1.5,
      color: "var(--toner-3)"
    }
  }, note));
}
Object.assign(window, {
  WorkingMargin,
  PassiveMargin,
  marginShell
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/surething/margin.jsx", error: String((e && e.message) || e) }); }

// ui_kits/surething/screens.jsx
try { (() => {
function _extends() { return _extends = Object.assign ? Object.assign.bind() : function (n) { for (var e = 1; e < arguments.length; e++) { var t = arguments[e]; for (var r in t) ({}).hasOwnProperty.call(t, r) && (n[r] = t[r]); } return n; }, _extends.apply(null, arguments); }
const {
  ColumnHead,
  FormEntry,
  MarketOffer,
  MoreButton,
  TicketReceipt,
  RevealedLeg,
  OfferEntry,
  LedgerEntry,
  RunFigure,
  StampReason,
  InkMark
} = window.SureThingDesignSystem_6e1eb3;
const sheet = {
  width: 700,
  borderRight: "2px solid var(--rule)",
  display: "flex",
  flexDirection: "column"
};
const key = {
  fontSize: 13,
  letterSpacing: ".12em",
  color: "var(--toner-3)",
  whiteSpace: "nowrap"
};

/* FORM — the house's document. Six ruled two-line entries, moneyline and MORE only. */
function FormScreen({
  selections,
  onSelect,
  onMore
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: sheet
  }, /*#__PURE__*/React.createElement(ColumnHead, null), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column"
    }
  }, window.STData.slate.map((m, i) => /*#__PURE__*/React.createElement(FormEntry, {
    key: m.id,
    index: i,
    number: m.no,
    away: m.away,
    home: m.home,
    selected: selections[m.id] && selections[m.id].market === "MONEYLINE" ? selections[m.id].side : null,
    onSelect: side => onSelect(m, side),
    onMore: () => onMore(m)
  }))));
}

/* ENTRY — event detail. Only the market body changes when a destination is switched; the header and
   the working margin persist. A staged ticket is shown as a receipt before the action area. */
function EntryScreen({
  matchup,
  destination,
  onDestination,
  selection,
  onSelectMarket,
  onBack,
  tickets
}) {
  const markets = window.STData.markets[destination] || [];
  return /*#__PURE__*/React.createElement("div", {
    style: sheet
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 14,
      padding: "0 14px",
      height: 44,
      borderBottom: "1px solid var(--rule)",
      background: "var(--ground-2)"
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: onBack,
    style: {
      height: 32,
      minWidth: 96,
      background: "transparent",
      border: "1px solid var(--rule)",
      color: "var(--toner-2)",
      fontFamily: "var(--font-data)",
      fontSize: 13,
      letterSpacing: ".06em",
      cursor: "pointer"
    }
  }, "\u2039 BACK TO FORM"), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: 10
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: 19,
      letterSpacing: ".03em",
      color: "var(--toner)",
      textTransform: "uppercase"
    }
  }, matchup.away.name, " at ", matchup.home.name), /*#__PURE__*/React.createElement("span", {
    style: {
      ...key,
      letterSpacing: ".08em"
    }
  }, matchup.away.record, " \xB7 ", matchup.home.record, " \xB7 ENTRY ", matchup.no))), /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      gap: 2,
      padding: "8px 14px 0",
      borderBottom: "1px solid var(--rule)"
    }
  }, window.STData.destinations.map(d => /*#__PURE__*/React.createElement("button", {
    key: d,
    type: "button",
    onClick: () => onDestination(d),
    style: {
      height: 27,
      padding: "0 13px",
      background: d === destination ? "var(--ground)" : "transparent",
      border: "1px solid " + (d === destination ? "var(--rule)" : "var(--rule-soft)"),
      borderBottom: 0,
      color: d === destination ? "var(--toner)" : "var(--toner-3)",
      fontFamily: "var(--font-data)",
      fontSize: 13,
      letterSpacing: ".11em",
      cursor: "pointer"
    }
  }, d))), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      overflowY: "auto"
    }
  }, tickets.length > 0 && /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "12px 14px",
      borderBottom: "1px solid var(--rule)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      ...key,
      marginBottom: 7
    }
  }, "PLACED THIS ROUND"), tickets.map(t => /*#__PURE__*/React.createElement(TicketReceipt, {
    key: t.number,
    number: t.number,
    legs: t.legs,
    stake: t.stake,
    combined: t.combined,
    payout: t.payout,
    style: {
      marginBottom: 8
    }
  }))), markets.map(mk => {
    const picked = selection && selection.line === mk.line;
    const other = selection && selection.line !== mk.line;
    return /*#__PURE__*/React.createElement("div", {
      key: mk.line,
      style: {
        display: "flex",
        alignItems: "center",
        padding: "0 14px",
        height: 54,
        borderBottom: "1px solid var(--rule-soft)",
        background: picked ? "linear-gradient(90deg,var(--marked-wash),transparent 70%)" : "transparent"
      }
    }, /*#__PURE__*/React.createElement("div", {
      style: {
        flex: 1,
        fontFamily: "var(--font-cond)",
        fontSize: 19,
        letterSpacing: ".03em",
        color: "var(--toner)",
        textTransform: "uppercase"
      }
    }, mk.line), /*#__PURE__*/React.createElement(MarketOffer, {
      line: "",
      price: mk.price,
      state: picked ? "picked" : other ? "replace" : "default",
      onSelect: () => onSelectMarket(mk),
      style: {
        width: 176,
        justifyContent: "flex-end"
      }
    }));
  })));
}

/* MY BETS — the revealed mirror. Read-only, TV-owned, and never ahead of the broadcast. */
function MyBetsScreen({
  tickets,
  revealed
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: sheet
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "0 14px",
      height: 44,
      borderBottom: "1px solid var(--rule)",
      background: "var(--ground-2)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: key
  }, "READ-ONLY MIRROR"), /*#__PURE__*/React.createElement(StampReason, {
    reason: "THE TV OWNS THE REVEAL \u2014 THIS SHEET ONLY COPIES IT"
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      overflowY: "auto",
      padding: "0 14px"
    }
  }, tickets.length === 0 && /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "18px 0",
      fontSize: 13,
      color: "var(--toner-3)",
      lineHeight: 1.5
    }
  }, "No tickets locked this round."), tickets.map(t => /*#__PURE__*/React.createElement("div", {
    key: t.number,
    style: {
      padding: "14px 0",
      borderBottom: "1px solid var(--rule)"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "baseline",
      gap: 12,
      marginBottom: 4
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-cond)",
      fontSize: 19,
      letterSpacing: ".14em",
      color: "var(--toner)",
      textTransform: "uppercase"
    }
  }, t.number), /*#__PURE__*/React.createElement("span", {
    style: key
  }, "STAKE ", t.stake, " \xB7 PAYS ", t.payout)), t.legs.map((l, i) => /*#__PURE__*/React.createElement(RevealedLeg, {
    key: i,
    team: l.team,
    price: l.price,
    market: l.market,
    state: revealed[t.number + "/" + i] || "PENDING"
  }))))));
}

/* REWARDS — the shop. Ruled offer entries, never a promotional rail. */
function RewardsScreen({
  result,
  onBuy
}) {
  return /*#__PURE__*/React.createElement("div", {
    style: sheet
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "0 14px",
      height: 44,
      borderBottom: "1px solid var(--rule)",
      background: "var(--ground-2)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: key
  }, "OFFERS THIS ROUND"), /*#__PURE__*/React.createElement("span", {
    style: {
      ...key,
      marginLeft: "auto"
    }
  }, "RELICS HELD 2 / 5")), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      overflowY: "auto"
    }
  }, window.STData.offers.map(o => /*#__PURE__*/React.createElement(OfferEntry, _extends({
    key: o.name
  }, o, {
    onBuy: () => onBuy(o)
  }))), result && /*#__PURE__*/React.createElement("div", {
    style: {
      padding: "13px 14px"
    }
  }, /*#__PURE__*/React.createElement(StampReason, {
    reason: result
  }))));
}

/* LEDGER — Old Slips. The settled record, read-only, same document grammar, no live styling. */
function LedgerScreen() {
  return /*#__PURE__*/React.createElement("div", {
    style: sheet
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      display: "flex",
      alignItems: "center",
      gap: 12,
      padding: "0 14px",
      height: 44,
      borderBottom: "1px solid var(--rule)",
      background: "var(--ground-2)"
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: key
  }, "SETTLED TICKETS \xB7 THIS RUN"), /*#__PURE__*/React.createElement("span", {
    style: {
      ...key,
      marginLeft: "auto"
    }
  }, "3 RECORDS")), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      overflowY: "auto"
    }
  }, window.STData.ledger.map(l => /*#__PURE__*/React.createElement(LedgerEntry, _extends({
    key: l.number
  }, l)))));
}
Object.assign(window, {
  FormScreen,
  EntryScreen,
  MyBetsScreen,
  RewardsScreen,
  LedgerScreen
});
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/surething/screens.jsx", error: String((e && e.message) || e) }); }

// ui_kits/tv-sweat/app.jsx
try { (() => {
const {
  TvScorebug,
  TvLegRow,
  TvRiskPays,
  TvCashOutSlot,
  TvEventStrip,
  TvStage,
  TvStatsPanel,
  TvTicketCard,
  TvMomentumTape
} = window.SureThingDesignSystem_6e1eb3;
const D = window.TVData;
const ctl = {
  height: 28,
  padding: "0 13px",
  background: "transparent",
  border: "1px solid var(--tv-rule)",
  color: "var(--tv-context)",
  fontFamily: "var(--font-tv)",
  fontSize: 12,
  letterSpacing: ".12em",
  cursor: "pointer",
  textTransform: "uppercase"
};
function App() {
  const [i, setI] = React.useState(1);
  const [stats, setStats] = React.useState(false);
  const [card, setCard] = React.useState(false);
  const [cashedOut, setCashedOut] = React.useState(null);
  const [punch, setPunch] = React.useState(true);
  const b = D.beats[i];
  /* DESIGN.md §7: if concurrency exceeds the column's height, resolved rows collapse first, then
     pending. A live row is never truncated. With this three-leg ticket the column never runs out of
     room — the one beat with three concurrent live legs has no resolved rows left to collapse — so
     this is a defensive capability, wired and ready for a taller ticket rather than exercised here. */
  const liveCount = b.states.filter(s => s === "LIVE").length;
  /* VISUAL-DESIGN §5: a backed-team marker is honest for moneyline only. For totals, BTTS, corners,
     cards and scorer props the scorebug shows both identities and the rail says MARKET PICK. */
  const activeLeg = D.legs[Math.max(0, parseInt(b.legIndex, 10) - 1)] || D.legs[0];
  const underPressure = liveCount >= 3 || D.legs.length > 3;
  React.useEffect(() => {
    setPunch(true);
    const t = window.setTimeout(() => setPunch(false), 900);
    return () => window.clearTimeout(t);
  }, [i]);
  const cash = cashedOut ? {
    state: "accepted",
    amount: cashedOut
  } : b.cash;
  const advance = () => {
    setCard(false);
    setI(n => Math.min(n + 1, D.beats.length - 1));
  };
  const back = () => {
    setCard(false);
    setI(n => Math.max(n - 1, 0));
  };
  return /*#__PURE__*/React.createElement("div", null, /*#__PURE__*/React.createElement("div", {
    style: {
      width: 980,
      height: 550,
      position: "relative",
      overflow: "hidden",
      background: "var(--tv-substrate)",
      fontFamily: "var(--font-tv)",
      fontVariantNumeric: "tabular-nums",
      display: "flex"
    }
  }, /*#__PURE__*/React.createElement("div", {
    style: {
      width: "var(--tv-ticket-col-w)",
      display: "flex",
      flexDirection: "column",
      borderRight: "1px solid var(--tv-rule)",
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement("button", {
    type: "button",
    onClick: () => setStats(s => !s),
    style: {
      ...ctl,
      height: 34,
      border: 0,
      borderBottom: "1px solid var(--tv-rule)",
      textAlign: "left",
      fontSize: 15,
      letterSpacing: ".16em",
      color: "var(--tv-structure)"
    }
  }, "TICKET ", D.ticket.index, " \xB7 STATS"), D.legs.map((l, n) => /*#__PURE__*/React.createElement(TvLegRow, {
    key: n,
    market: l.market,
    price: l.price,
    statement: l.statement,
    state: b.states[n],
    progress: b.progress[n]
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      marginTop: "auto"
    }
  }, /*#__PURE__*/React.createElement(TvRiskPays, {
    risk: D.ticket.risk,
    pays: D.ticket.pays
  }), /*#__PURE__*/React.createElement("div", {
    onClick: () => {
      if (cash.state === "actionable") setCashedOut(cash.amount);
    },
    style: {
      cursor: cash.state === "actionable" ? "pointer" : "default"
    }
  }, /*#__PURE__*/React.createElement(TvCashOutSlot, {
    state: cash.state,
    amount: cash.amount
  }))), stats && /*#__PURE__*/React.createElement(TvStatsPanel, {
    away: D.away,
    home: D.home,
    rows: D.stats,
    onClose: () => setStats(false)
  })), /*#__PURE__*/React.createElement("div", {
    style: {
      flex: 1,
      display: "flex",
      flexDirection: "column",
      position: "relative"
    }
  }, /*#__PURE__*/React.createElement(TvScorebug, {
    ticket: D.ticket.index,
    leg: b.legIndex,
    away: D.away,
    home: D.home,
    score: b.score,
    clock: b.clock,
    backed: activeLeg.backed || null,
    marketPick: !activeLeg.backed,
    goal: b.goal ? true : undefined
  }), /*#__PURE__*/React.createElement(TvMomentumTape, {
    samples: b.momentum
  }), /*#__PURE__*/React.createElement(TvStage, {
    actors: b.actors,
    ball: b.ball
  }), /*#__PURE__*/React.createElement(TvEventStrip, {
    text: b.event,
    punched: punch
  }), card && /*#__PURE__*/React.createElement(TvTicketCard, D.nextTicket))), /*#__PURE__*/React.createElement("div", {
    style: {
      width: 980,
      display: "flex",
      alignItems: "center",
      gap: 8,
      marginTop: 12
    }
  }, /*#__PURE__*/React.createElement("span", {
    style: {
      fontFamily: "var(--font-tv)",
      fontSize: 12,
      letterSpacing: ".1em",
      color: "var(--tv-structure)",
      marginRight: "auto"
    }
  }, "980 \xD7 550 REFERENCE CANVAS \xB7 THE IN-ROOM RENDER AT THE SEATED CAMERA IS THE ONLY VALID ACCEPTANCE VIEW"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    style: ctl,
    onClick: back
  }, "\u25C4 Beat"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    style: ctl,
    onClick: advance
  }, "Beat \u25BA"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    style: ctl,
    onClick: () => setStats(s => !s)
  }, "Stats"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    style: ctl,
    onClick: () => setCard(c => !c)
  }, "Ticket card"), /*#__PURE__*/React.createElement("button", {
    type: "button",
    style: ctl,
    onClick: () => {
      setI(1);
      setCashedOut(null);
      setCard(false);
      setStats(false);
    }
  }, "Reset")));
}
ReactDOM.createRoot(document.getElementById("root")).render(/*#__PURE__*/React.createElement(App, null));
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/tv-sweat/app.jsx", error: String((e && e.message) || e) }); }

// ui_kits/tv-sweat/data.js
try { (() => {
/* One ticket swept beat by beat. Every figure here would come from the engine's revealed payload in
   the real product; the TV is the only surface allowed to show score, clock and probability at all.
   Fictional teams and players only. */
window.TVData = {
  away: "Pressmen",
  home: "Foundry",
  ticket: {
    index: "1/2",
    risk: "$50",
    pays: "$462"
  },
  /* Statements are VISUAL-DESIGN §6's lines verbatim. The scorer line wraps to two, which §3 allows. */
  legs: [{
    market: "MONEYLINE",
    price: "-155",
    statement: "Foundry to win",
    backed: "home"
  }, {
    market: "TOTAL GOALS",
    price: "-110",
    statement: "Over 2.5 goals"
  }, {
    market: "ANYTIME SCORER",
    price: "+210",
    statement: "Marcus Vale to score"
  }],
  beats: [{
    clock: "PRE",
    score: [0, 0],
    legIndex: "1/3",
    event: "Teams out — kick-off shortly",
    momentum: [0, 0, 0, 0, 0, 0, 0, 0],
    states: ["NEXT", "NEXT", "NEXT"],
    progress: [null, null, null],
    cash: {
      state: "unavailable"
    },
    actors: [{
      x: 30,
      y: 46,
      team: "a"
    }, {
      x: 44,
      y: 50,
      team: "b",
      number: "9"
    }, {
      x: 62,
      y: 44,
      team: "b"
    }],
    ball: {
      x: 50,
      y: 50
    }
  }, {
    clock: "12'",
    score: [0, 0],
    legIndex: "1/3",
    event: "Foundry build through the middle",
    momentum: [0, .1, .2, .1, .3, .4, .3, .5],
    states: ["LIVE", "LIVE", "LIVE"],
    progress: ["LIVE • LEVEL 0–0", "LIVE • 0 GOALS • 3 MORE", "LIVE • WAITING FOR VALE"],
    cash: {
      state: "actionable",
      amount: "$148"
    },
    actors: [{
      x: 34,
      y: 40,
      team: "a"
    }, {
      x: 48,
      y: 52,
      team: "b",
      number: "9"
    }, {
      x: 58,
      y: 38,
      team: "b"
    }, {
      x: 70,
      y: 56,
      team: "a"
    }],
    ball: {
      x: 49,
      y: 51
    }
  }, {
    clock: "34'",
    score: [0, 1],
    legIndex: "2/3",
    event: "Vale finds the net",
    goal: true,
    momentum: [.2, .1, .3, .4, .3, .5, .7, .9],
    states: ["LIVE", "LIVE", "W"],
    progress: ["LIVE • LEADING 1–0", "LIVE • 1 GOAL • 2 MORE", null],
    cash: {
      state: "updating",
      amount: "$212"
    },
    actors: [{
      x: 62,
      y: 44,
      team: "b",
      number: "9"
    }, {
      x: 74,
      y: 38,
      team: "b"
    }, {
      x: 80,
      y: 52,
      team: "a"
    }],
    ball: {
      x: 88,
      y: 47,
      payoff: true
    }
  }, {
    clock: "58'",
    score: [1, 1],
    legIndex: "2/3",
    event: "Pressmen equalise from the spot",
    goal: true,
    momentum: [.5, .3, 0, -.3, -.5, -.7, -.8, -.6],
    states: ["LIVE", "LIVE", "W"],
    progress: ["LIVE • LEVEL 1–1", "LIVE • 2 GOALS • 1 MORE", null],
    cash: {
      state: "updating",
      amount: "$176"
    },
    actors: [{
      x: 24,
      y: 48,
      team: "a"
    }, {
      x: 40,
      y: 42,
      team: "a"
    }, {
      x: 56,
      y: 54,
      team: "b",
      number: "9"
    }],
    ball: {
      x: 18,
      y: 48
    }
  }, {
    clock: "71'",
    score: [1, 1],
    legIndex: "2/3",
    event: "VAR — checking the second goal",
    momentum: [.3, 0, -.3, -.5, -.7, -.6, -.4, -.2],
    states: ["LIVE", "LIVE", "W"],
    progress: ["LIVE • LEVEL 1–1", "LIVE • 2 GOALS • 1 MORE", null],
    cash: {
      state: "suspended"
    },
    actors: [{
      x: 40,
      y: 44,
      team: "a"
    }, {
      x: 52,
      y: 50,
      team: "b",
      number: "9"
    }],
    ball: {
      x: 46,
      y: 47
    }
  }, {
    clock: "78'",
    score: [1, 2],
    legIndex: "3/3",
    event: "Foundry back in front — cutback finished",
    goal: true,
    momentum: [-.5, -.3, 0, .2, .5, .7, .8, 1],
    states: ["LIVE", "W", "W"],
    progress: ["LIVE • LEADING 2–1", null, null],
    cash: {
      state: "updating",
      amount: "$340"
    },
    actors: [{
      x: 66,
      y: 40,
      team: "b",
      number: "9"
    }, {
      x: 78,
      y: 52,
      team: "b"
    }, {
      x: 84,
      y: 44,
      team: "a"
    }],
    ball: {
      x: 90,
      y: 50,
      payoff: true
    }
  }, {
    clock: "FT",
    score: [1, 2],
    legIndex: "3/3",
    event: "Full time",
    momentum: [0, .2, .5, .7, .8, 1, .6, .3],
    states: ["W", "W", "W"],
    progress: [null, null, null],
    cash: {
      state: "unavailable"
    },
    actors: [{
      x: 48,
      y: 46,
      team: "b",
      number: "9"
    }, {
      x: 56,
      y: 52,
      team: "a"
    }],
    ball: {
      x: 50,
      y: 50
    }
  }],
  stats: [{
    label: "SHOTS",
    away: "7",
    home: "14"
  }, {
    label: "SHOTS ON TARGET",
    away: "2",
    home: "6"
  }, {
    label: "CORNERS",
    away: "3",
    home: "8"
  }, {
    label: "CARDS",
    away: "2",
    home: "1"
  }],
  nextTicket: {
    heading: "TICKET 2 OF 2",
    risk: "$50",
    pays: "$462",
    legs: ["JUNCTION ML -110", "BTTS YES -105", "OVER 8.5 CORNERS +100"]
  }
};
})(); } catch (e) { __ds_ns.__errors.push({ path: "ui_kits/tv-sweat/data.js", error: String((e && e.message) || e) }); }

__ds_ns.LockAction = __ds_scope.LockAction;

__ds_ns.PlaceAction = __ds_scope.PlaceAction;

__ds_ns.SkipAction = __ds_scope.SkipAction;

__ds_ns.StampReason = __ds_scope.StampReason;

__ds_ns.PayoutFigure = __ds_scope.PayoutFigure;

__ds_ns.RunFigure = __ds_scope.RunFigure;

__ds_ns.ColumnHead = __ds_scope.ColumnHead;

__ds_ns.FormEntry = __ds_scope.FormEntry;

__ds_ns.InkMark = __ds_scope.InkMark;

__ds_ns.MarketOffer = __ds_scope.MarketOffer;

__ds_ns.MoreButton = __ds_scope.MoreButton;

__ds_ns.PriceCell = __ds_scope.PriceCell;

__ds_ns.MarginHeader = __ds_scope.MarginHeader;

__ds_ns.MarginLeg = __ds_scope.MarginLeg;

__ds_ns.MarginRow = __ds_scope.MarginRow;

__ds_ns.RubOutButton = __ds_scope.RubOutButton;

__ds_ns.StakeButton = __ds_scope.StakeButton;

__ds_ns.StakeControls = __ds_scope.StakeControls;

__ds_ns.Masthead = __ds_scope.Masthead;

__ds_ns.OsRail = __ds_scope.OsRail;

__ds_ns.OsTray = __ds_scope.OsTray;

__ds_ns.SectionTabs = __ds_scope.SectionTabs;

__ds_ns.LedgerEntry = __ds_scope.LedgerEntry;

__ds_ns.OfferEntry = __ds_scope.OfferEntry;

__ds_ns.RevealedLeg = __ds_scope.RevealedLeg;

__ds_ns.RevealedState = __ds_scope.RevealedState;

__ds_ns.TicketReceipt = __ds_scope.TicketReceipt;

__ds_ns.TvCashOutSlot = __ds_scope.TvCashOutSlot;

__ds_ns.TvEventStrip = __ds_scope.TvEventStrip;

__ds_ns.TvLegRow = __ds_scope.TvLegRow;

__ds_ns.TvMomentumTape = __ds_scope.TvMomentumTape;

__ds_ns.TvRiskPays = __ds_scope.TvRiskPays;

__ds_ns.TvScorebug = __ds_scope.TvScorebug;

__ds_ns.TvStage = __ds_scope.TvStage;

__ds_ns.TvStatsPanel = __ds_scope.TvStatsPanel;

__ds_ns.TvTicketCard = __ds_scope.TvTicketCard;

__ds_ns.TIER = __ds_scope.TIER;

})();
