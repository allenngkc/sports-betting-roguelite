# Coverage Debt

Per TESTING.md: `path | why hard | escape plan`. Entries leave when the seam ladder reaches them.

| Path | Why hard | Escape plan |
|---|---|---|
| `TvSweatScreen.TryCashOut` settling-guard (displayed vs accepted price, M-T4 Sol finding) | Needs frame-precise UI-state introspection mid-odometer-tick inside the full Room flow; the guard itself is a 2-line early-return | Expose a test/debug `CashOutSettling` property if the guard ever grows logic; until then the FullRound fast-forward test covers completion and the guard is inspection-verified |
| `TvSweatScreen.TicketDeadBeat` consolation visibility above the dim overlay (M-T4 Sol finding) | Draw-order assertion requires canvas-render sampling, not state checks | Covered by the M-T4 Allen gate (a human looks at it); revisit if the settle beats gain logic beyond presentation |
