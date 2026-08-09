export interface ReceiptLeg {
  team: string;
  market: string;
  price: string;
}

/**
 * A staged or settled ticket, printed as a numbered form receipt.
 * @startingPoint section="SureThing" subtitle="Placed-ticket receipt — legs, stake, combined, pays" viewport="700x260"
 */
export interface TicketReceiptProps {
  /** Printed identity, e.g. "TICKET 01". Never invent a date. */
  number: string;
  legs?: ReceiptLeg[];
  stake: string;
  combined: string;
  payout: string;
  /** Only after the TV reveals it. Omit for a staged ticket that has not been swept. */
  state?: "PENDING" | "LIVE" | "GREEN" | "DEAD" | "VOID" | "CASHED OUT";
  inkBase?: string;
  style?: React.CSSProperties;
}
export declare function TicketReceipt(props: TicketReceiptProps): JSX.Element;
