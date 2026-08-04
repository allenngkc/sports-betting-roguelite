export interface RevealedStateProps {
  /** Literal state word. GREEN gets a wax re-inked ring; DEAD gets the house's strike. */
  state?: "PENDING" | "LIVE" | "GREEN" | "DEAD" | "VOID" | "CASHED OUT";
  inkBase?: string;
  style?: React.CSSProperties;
}
export declare function RevealedState(props: RevealedStateProps): JSX.Element;
