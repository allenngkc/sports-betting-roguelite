/**
 * The core control of the SureThing form. A printed price; selecting it draws a biro ring around it.
 * @startingPoint section="SureThing" subtitle="Price cell in every state — picked, replacing, won, dead" viewport="700x150"
 */
export interface PriceCellProps {
  /** American odds, literal, exactly as the engine supplies them. Never rounded or reformatted. */
  price: string;
  /**
   * default  — printed figure, transparent ground
   * picked   — toner figure plus his biro ring
   * replace  — the other side of an already-marked matchup: selectable, biro ⇄ plus dashed underline.
   *            NEVER rendered as disabled; v0 has no limiting.
   * won      — sweat only, after TV reveal: wax figure, ring re-inked in wax
   * dead     — sweat only, after TV reveal: toner-3 figure plus the house's strike
   */
  state?: "default" | "picked" | "replace" | "won" | "dead";
  /** Keyed to the matchup index via InkMark.variantFor. Never randomised. */
  ringVariant?: "ring-a" | "ring-b" | "ring-c";
  /** "kit" = the 96x30 element-kit figure. "runtime" = the shipped 112x32 AWAY/HOME button. */
  size?: "kit" | "runtime";
  /** Path to assets/ink/ for the ring sprite. */
  inkBase?: string;
  onSelect?: () => void;
  title?: string;
  style?: React.CSSProperties;
}

export declare function PriceCell(props: PriceCellProps): JSX.Element;
