/**
 * One leg in the TV's full-height ticket column. Brightness is the state.
 * @startingPoint section="TV Sweat" subtitle="Leg rows — NEXT, LIVE, W, L, VOID on the brightness ladder" viewport="360x300"
 */
export interface TvLegRowProps {
  /** Market name: MONEYLINE, TOTAL GOALS, BTTS — YES, TOTAL CORNERS, ANYTIME SCORER. */
  market: string;
  price: string;
  /**
   * The headline, shown in every state and never paraphrased — VISUAL-DESIGN §6 prints these
   * verbatim: "NORTHSIDE TO WIN", "OVER 2.5 GOALS", "BOTH TEAMS TO SCORE", "MARCUS VALE TO SCORE".
   * It is also §3's active NEED statement, so it may wrap to two lines; nothing else in the rail wraps.
   */
  statement: string;
  /** NEXT=L1 structure only · LIVE=L3 with the surface's only pulse · W=L3 gold · L=L0 dark · VOID=L2 struck */
  state?: "NEXT" | "LIVE" | "W" | "L" | "VOID";
  /** Revealed progress: "LIVE • 2 GOALS • 1 MORE". Neutral until the leg resolves. */
  progress?: string;
  /**
   * Defaults to expanded while LIVE. A resolved or pending row renders as ONE line — eyebrow,
   * statement, price, state — so the vertical budget goes to what is live. A live row is never
   * truncated. Override only to force a state open for a specimen.
   */
  expanded?: boolean;
  style?: React.CSSProperties;
}
export declare function TvLegRow(props: TvLegRowProps): JSX.Element;
