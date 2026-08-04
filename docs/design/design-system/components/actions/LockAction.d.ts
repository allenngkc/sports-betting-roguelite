export interface LockActionProps {
  label?: string;
  /** True while working marks exist or no ticket is staged. */
  disabled?: boolean;
  /** Required when disabled. Cause and remedy: "PLACE OR CLEAR THIS WORKING SLIP". */
  reason?: string;
  onClick?: () => void;
  style?: React.CSSProperties;
}
export declare function LockAction(props: LockActionProps): JSX.Element;
