export interface TvEventStripProps {
  /**
   * One authored line explaining the latest move: "NORTHSIDE SWITCH THE PLAY — CUTBACK BLOCKED",
   * "VALE FINDS THE NET", "VAR — NO GOAL". Never duplicates the score, never two unrelated clauses,
   * never emotional copy, and never a fact the revealed payload does not support.
   */
  text: string;
  /** True for the single punch at reveal; it settles back to L2 immediately after. */
  punched?: boolean;
  style?: React.CSSProperties;
}
export declare function TvEventStrip(props: TvEventStripProps): JSX.Element;
