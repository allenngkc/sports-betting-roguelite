export interface StakeButtonProps {
  /** "10%", "MAX", "− $10". Literal. */
  label: string;
  /** quick = 68x32 ruled fraction. nudge = 88x32 raised key. */
  variant?: "quick" | "nudge";
  onClick?: () => void;
  style?: React.CSSProperties;
}
export declare function StakeButton(props: StakeButtonProps): JSX.Element;
