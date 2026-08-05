export type InkMarkVariant = "ring-a" | "ring-b" | "ring-c" | "ring-wide-a" | "ring-wide-b" | "strike";

export interface InkMarkProps {
  /** Which generated sprite to draw. Rings are his mark; strike is the house's. */
  variant?: InkMarkVariant;
  /** Tint. --biro for a selection ring, --stamp for a strike, --wax for a re-inked win. Nothing else. */
  color?: string;
  /** Override size. Defaults to the sprite's own 1x size. Use InkMark.rect(cellW, cellH) for the +16px rule. */
  width?: number;
  height?: number;
  /** Path to assets/ink/. Pages not at the project root must pass this, or set window.SURETHING_INK_BASE. */
  base?: string;
  style?: React.CSSProperties;
}

export declare function InkMark(props: InkMarkProps): JSX.Element;
export declare namespace InkMark {
  function rect(cellW: number, cellH: number): { width: number; height: number; left: number; top: number };
  function variantFor(index: number, wide?: boolean): InkMarkVariant;
}
