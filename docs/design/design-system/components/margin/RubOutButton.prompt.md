Removes one leg from the working slip. 60×32, ruled, labelled in words.

```jsx
<RubOutButton onClick={() => removeLeg(leg.matchupId)} />
```

Removal clears that matchup and immediately recalculates combined odds, stake and payout through `BetslipModel`. Hover borders in `--stamp` because the house's mark is what an erasure leaves — never use `--stamp` as a generic "danger" colour elsewhere.
