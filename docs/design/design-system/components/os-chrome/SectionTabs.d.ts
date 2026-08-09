export interface SectionTabsProps {
  tabs?: string[];
  active?: string;
  /** Sheet counter or equivalent. States a product fact, so 13px floor applies. */
  meta?: string;
  onSelect?: (tab: string) => void;
  style?: React.CSSProperties;
}
export declare function SectionTabs(props: SectionTabsProps): JSX.Element;
