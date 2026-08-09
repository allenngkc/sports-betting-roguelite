A single stake control. Two variants, two sizes, both from the element kit.

```jsx
<StakeButton label="MAX" />
<StakeButton label="+ $10" variant="nudge" />
```

Quick fractions are `10% / 25% / 50% / MAX` at 68×32; nudge keys are `− $10 / + $10` at 88×32 on `--ground-3`. Stake is clamped by `BetslipModel`, never by the control. Both hover to `--biro`.
