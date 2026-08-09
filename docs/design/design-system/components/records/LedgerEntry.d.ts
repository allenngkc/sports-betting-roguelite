export interface LedgerEntryProps {
  /** Ticket identity, e.g. "R2 · TICKET 02". */
  number: string;
  /** One-line leg summary: "Bricklayers ML −260 · Over 2.5 −110". */
  legs: string;
  /** Literal terminal state: WON, LOST, CASHED OUT, VOIDED. */
  terminal: "WON" | "LOST" | "CASHED OUT" | "VOIDED";
  stake: string;
  payout: string;
  style?: React.CSSProperties;
}
export declare function LedgerEntry(props: LedgerEntryProps): JSX.Element;
