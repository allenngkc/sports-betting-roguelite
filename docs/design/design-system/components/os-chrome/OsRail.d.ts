export interface OsRailProps {
  /** The fictional OS's identity mark. Never "SureThing" — that is the app running on it. */
  identity?: string;
  /** Something he stuck on his own machine. Biro, slightly rotated. Satire is allowed here. */
  sticker?: string;
  /** System clock. OS chrome, no product meaning, so 12px is legal. */
  clock?: string;
  batteryLow?: boolean;
  style?: React.CSSProperties;
}
export declare function OsRail(props: OsRailProps): JSX.Element;
