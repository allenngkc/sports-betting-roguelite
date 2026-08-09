export interface StakeControlsProps {
  label?: string;
  /** Formatted stake, e.g. "$200". Minimum stake is $10; stakes are uncapped to bank. */
  stake: string;
  fractions?: string[];
  nudges?: string[];
  onFraction?: (fraction: string) => void;
  onNudge?: (nudge: string) => void;
  style?: React.CSSProperties;
}
export declare function StakeControls(props: StakeControlsProps): JSX.Element;
