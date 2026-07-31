export interface TvScorebugProps {
  /** Ticket index, e.g. "1/2". */
  ticket: string;
  /** Leg index, e.g. "2/3". */
  leg: string;
  away: string;
  home: string;
  score?: [number, number];
  /** PRE, live minutes, stoppage and FT all occupy this one rectangle. */
  clock?: string;
  /** Explicit only for moneyline. Totals, BTTS, corners, cards and scorer props show no backed marker. */
  backed?: "away" | "home" | null;
  /** Set for non-moneyline markets: the right rail says MARKET PICK instead of naming a backed team. */
  marketPick?: boolean;
  /**
   * Punches the score to L4 on its goal callback. §3 names the score at a goal as an L4 occupant.
   * A momentary punch preempts a sustained L4 — at a goal the cash-out is re-pricing and sits at L3,
   * so at most one element is ever at full brightness.
   */
  goal?: boolean;
  style?: React.CSSProperties;
}
export declare function TvScorebug(props: TvScorebugProps): JSX.Element;
