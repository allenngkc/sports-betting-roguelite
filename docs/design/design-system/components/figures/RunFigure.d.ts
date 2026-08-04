export interface RunFigureProps {
  /** Tracked uppercase key: BANK, TARGET, RELICS, TICKETS, DEBT. */
  label: string;
  /** Engine-backed value, formatted literally: "$1,340", "2/5", "0/3". */
  value: string;
  /** "wax" for the target and other money figures; "toner" for counts. */
  tone?: "toner" | "wax";
  /** Override the 21px default. */
  size?: string;
  style?: React.CSSProperties;
}
export declare function RunFigure(props: RunFigureProps): JSX.Element;
