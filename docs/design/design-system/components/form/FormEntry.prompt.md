A single 78px entry on the house's form — the lobby's repeating unit. Six of them fill the 530px work area.

```jsx
<FormEntry index={1} number="02"
  away={{ name: "Mallards", record: "3-6", price: "+210" }}
  home={{ name: "Bricklayers", record: "8-1", price: "-260" }}
  selected="home" onSelect={pick} onMore={openDetail} inkBase="../../assets/ink/" />
```

Pass `selected` and the component handles the rest: the picked price gets its biro ring, the other side becomes a `replace` control, and the row picks up a faint biro wash. Fictional teams only. Prices are locked — never animate or update them.
