Commits the round and hands the sweat to the TV. 52px, at least 280px wide, ruled in both states.

```jsx
<LockAction disabled reason="PLACE OR CLEAR THIS WORKING SLIP" />
<LockAction disabled={false} onClick={lockRound} />
```

Enabled only with at least one staged ticket and an empty working slip. Disabled states must state cause and remedy in place — a disabled LOCK with no reason is a defect.

Note: DESIGN.md specifies the disabled treatment exactly and calls LOCK a "52px ruled control"; the enabled treatment here (2px `--wax` border, toner label) is this system's inference, chosen so a second solid amber field never competes with PLACE. Flagged in readme.md.
