export interface FormTeam {
  name: string;
  /** Season record, W-L. A noisy signal by design. */
  record: string;
  /** Locked American price. Prices freeze at slate generation and never move. */
  price: string;
}

/**
 * One matchup on the house's printed form: 78px, two lines, number / matchup / price / More.
 * @startingPoint section="SureThing" subtitle="Form entry — two-line matchup row with locked prices" viewport="700x180"
 */
export interface FormEntryProps {
  /** Matchup index. Drives the deterministic ink-ring variant. */
  index?: number;
  /** Printed entry number, e.g. "02". */
  number: string;
  away: FormTeam;
  home: FormTeam;
  /** One selection per matchup. The unselected side automatically renders as "replace", never disabled. */
  selected?: "away" | "home" | null;
  /** Revealed sweat states, TV-revealed only: { away: "won" | "dead", home: ... }. */
  states?: { away?: string; home?: string };
  ringVariant?: "ring-a" | "ring-b" | "ring-c";
  onSelect?: (side: "away" | "home") => void;
  onMore?: () => void;
  inkBase?: string;
  style?: React.CSSProperties;
}

export declare function FormEntry(props: FormEntryProps): JSX.Element;
