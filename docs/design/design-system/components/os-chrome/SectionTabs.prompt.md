The app's section navigation — 27px tabs on a 38px recessed strip.

```jsx
<SectionTabs active="FORM" onSelect={go} meta="SHEET 1 OF 1" />
```

FORM, ENTRY, MY BETS, REWARDS. The active tab drops its bottom border and joins `--ground` so the sheet reads as continuous. Persistent chrome: this strip is present on every surface and never rebuilds when a destination changes.
