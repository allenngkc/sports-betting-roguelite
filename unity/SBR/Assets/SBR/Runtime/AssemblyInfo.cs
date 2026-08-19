using System.Runtime.CompilerServices;

// Lets the PlayMode fixture assert against the exact same internal formatting/geometry helpers
// (LaptopUi.FitText, SportsbookApp.CompactLegLabel, SportsbookApp.InkRingGeometry, ...) that the
// render code itself calls, instead of hand-duplicating a formula the test could quietly drift
// out of sync with.
[assembly: InternalsVisibleTo("SBR.Tests.PlayMode")]

// EditMode gets the same grant for the same reason. Some ruled logic is PURE — S102's contents
// suppression predicate is string equality on two printed forms — and belongs in the fast suite
// rather than in a scene. Without this the only ways to gate it were to make a UI-internal rule
// `public` on SportsbookApp, which states something false about who the rule is for, or to move a
// pure-logic gate into PlayMode and pay a scene for it. Widening our OWN test assembly's view is
// the smaller claim than widening the game's API.
[assembly: InternalsVisibleTo("SBR.Tests.EditMode")]
