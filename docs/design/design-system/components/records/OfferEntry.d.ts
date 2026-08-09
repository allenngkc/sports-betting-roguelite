export interface OfferEntryProps {
  name: string;
  /** Literal description of what the relic or consumable does. No hype, no promise of a win. */
  description: string;
  price: string;
  affordable?: boolean;
  owned?: boolean;
  /** The real reason from the engine or director when the purchase cannot happen. Never fabricated. */
  reason?: string;
  onBuy?: () => void;
  style?: React.CSSProperties;
}
export declare function OfferEntry(props: OfferEntryProps): JSX.Element;
