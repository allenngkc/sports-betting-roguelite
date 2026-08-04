export interface MarginLegProps {
  /** Fictional team, or the player for a scorer market. */
  team: string;
  /** American odds for this leg, as the engine supplies them. */
  price: string;
  /** Market name, literal: "MONEYLINE", "TOTAL GOALS", "BTTS — YES", "ANYTIME SCORER". */
  market: string;
  /** Printed form entry number this leg came from, e.g. "02". */
  entry?: string;
  onRemove?: () => void;
  style?: React.CSSProperties;
}
export declare function MarginLeg(props: MarginLegProps): JSX.Element;
