# SureThing — the run-verdict screen has never been ruled or photographed

**From:** SureThing UI lead · 2026-08-03 · found during S46, raised after S44/S45
**Asking for:** a ruling, or a decision that it does not need one yet. Nothing is blocked on it.

**What I know.** `LaptopOs.RenderVerdict` draws the screen a player reaches when a run ends
(`Phase.RunWon` / `RunLost`), and it is in no register entry, no kit file and no capture state. Three
things on it look wrong against laws already ruled elsewhere on this surface. Its ground is
`rgba(0.03, 0.02, 0.06, 1)` — effectively black and blue-tinted, which is the *exact* pair of
violations already found and fixed on the desktop taskbar (`rgba(.025, .02, .05, .94)`), with the
reasoning recorded in a comment twenty lines above this one: nothing on this surface may be pure
black, and the room physically cannot return a saturated cool colour, so a cool-cast element reads
as composited into the scene rather than photographed in it. Its losing headline, `THE BOOKIE
COLLECTS`, is drawn in oxide — the house's mark is for blocked actions and the strike on a dead leg
or lost ticket, and a run's verdict is a generic "bad" tint, which is the use the law names. And its
`NEW RUN` control is a **biro-filled field** with toner type: biro is only ever what the player
chose (Law Two, and the rule S44 has just applied to the desktop wordmark and the app's own icon
glyph), while S18 says a primary action is a wax field, wax-ink type and a 2px `--wax-deep` edge.
The winning headline in wax may well be right; I am not confident either way, because on a screen
that is entirely about money it is not obvious whether wax is carrying money or carrying mood.

**What I cannot see.** No capture exists of this screen, in any state — the twelve-state set is
betting, sweat, shop, ledger and desktop, and every finding above is read from source at HEAD
`916d4f4`, not from a frame. That matters more than usual here: this surface has twice produced a
source-read finding that a capture then dissolved (S32, T26), and the one absolute colour check ever
made against a hand-computed token on this surface was wrong because the project renders in linear
space. This canvas also sits inside the room's URP grade with bloom, so I cannot say what a
near-black blue ground actually measures on screen, only what the token says. I have not driven a
run to a verdict to photograph it, and I have not proposed a fix — S46 corrected only the app's name
on this screen and I stopped there rather than restyle an unruled surface. If you want it ruled, the
cheapest evidence is a capture state that forces `RunWon` and `RunLost`; say the word and it goes in
front of the next drag rather than into this one.
