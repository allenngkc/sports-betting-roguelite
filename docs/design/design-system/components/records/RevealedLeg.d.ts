export interface RevealedLegProps {
  team: string;
  price: string;
  market: string;
  state?: "PENDING" | "LIVE" | "GREEN" | "DEAD" | "VOID";
  inkBase?: string;
  style?: React.CSSProperties;
}
export declare function RevealedLeg(props: RevealedLegProps): JSX.Element;
