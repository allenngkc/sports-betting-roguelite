export interface TvTicketCardProps {
  /** "TICKET 2 OF 2". */
  heading: string;
  /** One-line leg summaries: ["NORTHSIDE ML +135", "OVER 2.5 -110", "VALE ANYTIME +210"]. */
  legs?: string[];
  risk: string;
  pays: string;
  style?: React.CSSProperties;
}
export declare function TvTicketCard(props: TvTicketCardProps): JSX.Element;
