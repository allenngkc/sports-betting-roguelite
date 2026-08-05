export interface TvActor {
  /** Percent of the stage width. */
  x: number;
  /** Percent of the stage height. */
  y: number;
  team?: "a" | "b";
  /** Only the backed player under PRD 7.7 carries a numbered cell. */
  number?: string;
}

export interface TvStageProps {
  actors?: TvActor[];
  /** The ball is the only object permitted L4, and only at a payoff. */
  ball?: { x: number; y: number; payoff?: boolean };
  /** The picked team attacks right. The camera is fixed: no shake, no cut, no zoom. */
  attackingRight?: boolean;
  style?: React.CSSProperties;
}
export declare function TvStage(props: TvStageProps): JSX.Element;
