export interface SkipActionProps {
  label?: string;
  armedLabel?: string;
  /** Omit to let the component own the two-press state; pass it to drive from outside. */
  armed?: boolean;
  /** Fires on the second press only. */
  onSkip?: () => void;
  style?: React.CSSProperties;
}
export declare function SkipAction(props: SkipActionProps): JSX.Element;
