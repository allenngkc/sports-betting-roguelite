export interface TvCashOutSlotProps {
  /**
   * actionable  — gold at L4, inverted field with dark type punched out. The one full-brightness element.
   * updating    — gold at L3, amount visibly settling. Never L4: brightness must not promise what input refuses.
   * suspended   — L1 unlit slate, MARKET SUSPENDED, no amount.
   * pending     — as suspended; intervention controls live in their own overlay, never in this row.
   * unavailable — L1, quiet, no reflow. Copy only when the absence needs explaining.
   * accepted    — gold, brief L4 punch, then CASHED OUT $x at L3.
   */
  state?: "actionable" | "updating" | "suspended" | "pending" | "unavailable" | "accepted";
  amount?: string;
  keyHint?: string;
  style?: React.CSSProperties;
}
export declare function TvCashOutSlot(props: TvCashOutSlotProps): JSX.Element;
