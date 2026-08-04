export interface PayoutFigureProps {
  label?: string;
  /** Engine-backed potential payout, e.g. "$718". Never a range, never a promise. */
  value: string;
  /** The hand-laid wax band. Leave on in the margin; turn off where the figure is not the subject. */
  highlight?: boolean;
  style?: React.CSSProperties;
}
export declare function PayoutFigure(props: PayoutFigureProps): JSX.Element;
