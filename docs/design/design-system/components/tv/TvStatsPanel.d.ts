export interface TvStatsRow {
  label: string;
  away: string;
  home: string;
}

export interface TvStatsPanelProps {
  title?: string;
  away: string;
  home: string;
  /** Revealed-ledger values only. Never engine truth, never an unrevealed number. */
  rows?: TvStatsRow[];
  onClose?: () => void;
  style?: React.CSSProperties;
}
export declare function TvStatsPanel(props: TvStatsPanelProps): JSX.Element;
