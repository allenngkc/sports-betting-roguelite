using System.Runtime.CompilerServices;

// Lets the PlayMode fixture assert against the exact same internal formatting/geometry helpers
// (LaptopUi.FitText, SportsbookApp.CompactLegLabel, SportsbookApp.InkRingGeometry, ...) that the
// render code itself calls, instead of hand-duplicating a formula the test could quietly drift
// out of sync with.
[assembly: InternalsVisibleTo("SBR.Tests.PlayMode")]
