export interface MarketOfferProps {
  /** The market line, literal: "OVER 2.5 GOALS", "BOTH TEAMS TO SCORE", "VALE ANYTIME". */
  line: string;
  /** Locked American price. */
  price: string;
  state?: "default" | "picked" | "replace";
  ringVariant?: "ring-wide-a" | "ring-wide-b";
  onSelect?: () => void;
  inkBase?: string;
  style?: React.CSSProperties;
}
export declare function MarketOffer(props: MarketOfferProps): JSX.Element;
