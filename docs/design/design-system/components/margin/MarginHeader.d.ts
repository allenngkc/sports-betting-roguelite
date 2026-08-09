export interface MarginHeaderProps {
  title?: string;
  /** Literal count, e.g. "2 SELECTIONS". Never "2 legs added!" and never a bare numeral. */
  count?: string;
  style?: React.CSSProperties;
}
export declare function MarginHeader(props: MarginHeaderProps): JSX.Element;
