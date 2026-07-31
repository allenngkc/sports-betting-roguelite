export interface TvRiskPaysProps {
  /** ONE ticket-level risk figure. Never per leg — that is not how a parlay works. */
  risk: string;
  /** ONE ticket-level payout figure. */
  pays: string;
  style?: React.CSSProperties;
}
export declare function TvRiskPays(props: TvRiskPaysProps): JSX.Element;
