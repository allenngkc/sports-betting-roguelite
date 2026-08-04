export interface MastheadFigure {
  label: string;
  value: string;
  tone?: "toner" | "wax";
}

export interface MastheadProps {
  title?: string;
  /** Round, seed, and the literal prices-final context: "ROUND 3 OF 8 · SEED 8F3K-22 · PRICES FINAL". */
  dateline?: string;
  /** Persistent run context. Bank, target, relics held, tickets placed — and debt when there is any. */
  figures?: MastheadFigure[];
  /** The literal locked-odds note. Satire is permitted here; a fact is not. */
  note?: string;
  style?: React.CSSProperties;
}
export declare function Masthead(props: MastheadProps): JSX.Element;
