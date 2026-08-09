The form's core control — a printed American price that gets circled in biro when the player picks it.

```jsx
<PriceCell price="-260" state="picked" ringVariant={InkMark.variantFor(1)} inkBase="../../assets/ink/" onSelect={pick} />
```

States: `default` (hover lifts the figure to `--wax-lit`, no fill), `picked`, `replace`, `won`, `dead`. The figure box is 96×30 (`size="runtime"` gives the shipped 112×32 button); the button always provides a ≥32px hit area. Focus is a 2px `--wax` outline at 1px offset.

Never render an alternative price as disabled — the correct treatment is `state="replace"`, because picking it replaces the existing selection on that matchup.
