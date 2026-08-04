The stats panel (PRD §8.8), opened from the head of the ticket column. It freezes playback while open.

```jsx
<TvStatsPanel away="Nighthawks" home="Foundry" onClose={close}
  rows={[{label:"SHOTS",away:"7",home:"11"},{label:"CORNERS",away:"3",home:"5"}]} />
```

It expands over the ticket column and stage without moving either, so everything is exactly where it was when it closes. Per-team rows use the muted team hues; every value comes from the revealed ledger.
