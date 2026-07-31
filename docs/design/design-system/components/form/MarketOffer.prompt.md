A market row inside ENTRY (event detail) — Goals, BTTS, Corners, Cards or Players. 160×30 cell, ringed with the wider `ring-wide-*` sprite when picked.

```jsx
<MarketOffer line="OVER 2.5 GOALS" price="-110" state="picked" inkBase="../../assets/ink/" />
```

Switching destination changes only the market body; the matchup header and the working margin persist. A market on an already-marked matchup is `replace`, never unavailable.
