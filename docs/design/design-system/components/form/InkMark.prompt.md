Draws one of the generated ink sprites — a biro selection ring or the house's strike — as a tinted alpha mask, the way Unity tints it with `Image.color`.

```jsx
<InkMark variant={InkMark.variantFor(matchupIndex)} color="var(--biro)" base="../../assets/ink/" />
```

Sizing rule: ring = cell + 16px on both axes, offset −8/−8 (8px overshoot per edge). `InkMark.rect(112, 32)` gives `{width:128, height:48, left:-8, top:-8}` for the shipped price button. Tint `--biro` for anything he chose, `--stamp` for a dead leg, `--wax` for a re-inked win. Variant is keyed to the matchup index and never randomised.
