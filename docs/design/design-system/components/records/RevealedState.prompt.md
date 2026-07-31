The literal revealed state of a leg or ticket, with its mark. Reads only from the TV's revealed payload.

```jsx
<RevealedState state="GREEN" inkBase="../../assets/ink/" />
```

`GREEN` and `DEAD` are words first and colours second — status is never signalled by colour alone. The mark is sized from the word's own measured box, so it tracks whatever text is actually on screen. Never render a state the TV has not revealed.
