export interface MarginRowProps {
  /** Tracked uppercase field key at the 13px fact floor. */
  label: string;
  value: string;
  /** "wax" only if the value genuinely is money. Odds are not money. */
  tone?: "toner" | "wax";
  style?: React.CSSProperties;
}
export declare function MarginRow(props: MarginRowProps): JSX.Element;
