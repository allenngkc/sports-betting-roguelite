Commits an empty round. 34px, dashed and quiet until armed, then solid `--stamp`.

```jsx
<SkipAction onSkip={skipRound} />
```

Two presses, always: the first arms and swaps the label to `PRESS AGAIN TO SKIP`, the second commits. It sits apart from LOCK IT IN and must never be styled to look like it.
