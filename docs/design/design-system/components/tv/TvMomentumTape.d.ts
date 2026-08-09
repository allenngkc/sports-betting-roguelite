export interface TvMomentumTapeProps {
  /**
   * Revealed momentum samples over time, each -1..1. Above the centre line is the home side, below
   * is the away side. Values come from the revealed ledger only — never from engine state, and never
   * ahead of the broadcast.
   */
  samples?: number[];
  /**
   * Tracked eyebrow at L2 so the channel is nameable in under three seconds. §3 puts labels at L2;
   * L1 is the dormant tier and is unreadable at couch distance. Only the ticket/leg index sits at
   * L1, and only because §7 names that exemption explicitly.
   */
  label?: string;
  style?: React.CSSProperties;
}
export declare function TvMomentumTape(props: TvMomentumTapeProps): JSX.Element;
