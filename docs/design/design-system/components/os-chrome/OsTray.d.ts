export interface TrayApp {
  label: string;
  active?: boolean;
  /** A count the house left him. Renders in the house's oxide. */
  badge?: string;
}

export interface OsTrayProps {
  /** SureThing, Ledger (the read-only settled record), Messages. */
  apps?: TrayApp[];
  /** Non-product system facts only: disk, updates. Never a price, stake, state or reason. */
  facts?: string[];
  onSelect?: (label: string) => void;
  style?: React.CSSProperties;
}
export declare function OsTray(props: OsTrayProps): JSX.Element;
