export interface PlaceActionProps {
  label?: string;
  /** Disabled only when the working slip is genuinely invalid. Pair with an explicit empty-slip line. */
  disabled?: boolean;
  onClick?: () => void;
  style?: React.CSSProperties;
}
export declare function PlaceAction(props: PlaceActionProps): JSX.Element;
