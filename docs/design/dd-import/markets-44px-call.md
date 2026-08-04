# Markets → DD · the 44px call blocking T47

**From:** markets/sim lead (`markets-2`) · **2026-08-02** · **Blocks:** B1 merge
**State:** both fixes T47 names are implemented and verified (`774a1c9`); the suite is
red on purpose, PlayMode 45/46, the single failure being the deficit below.

T47's two named fixes are in and correct — the blocked-action reason is nested inside its
control per `LockAction.jsx:24` (it can no longer be occluded by a later sibling, and it can
no longer travel onto the payout because it has no position of its own), and the action
stack is anchored and reserved, with PLACE joining LOCK and SKIP in the kit's single
bottom-anchored group (`margin.jsx:44-52`) instead of flowing from the leg list. What
remains is arithmetic that neither fix touches. The margin panel is a fixed **530px**. The
anchored band, measured up from its bottom edge, is SKIP 8..42, LOCK 52..104, PLACE
110..154, plus 6px of separation — **160px reserved**, leaving a flow budget of **530 − 160
= 370px**. At `MaxLegs = 4` the flow measures **414px** (canvas-local; the payout figure
sits at −378..−414 against a reserved band that begins at −370), so the flow is **44px over
budget**. The reservation is doing exactly what T47 asked; the content simply does not fit
inside it, and every remaining source of 44px is a design decision rather than a layout
correction, so the lead is not spending it. Three candidates, with what each yields: the
margin's `"PRICES FINAL. NOTHING YOU DO MOVES THEM."` **restates the board header**, which
**S37** forbids outright and which the markets C14 audit already carries as invented
(**M-09**) — removing it yields **18px**; the panel occupies 140..670 of a 704px screen and
therefore leaves **34px of unused screen** below it, so growing the panel yields 34px
without touching content; or the flow region scrolls, which is the studio's established
answer for a bounded interior region (**S25**) and already has a printed-rail spec
(**S27**), at the cost of adding a behaviour the reference `marginShell` does not have.
The first two together (52px) clear the deficit with room to spare, and the first is
already ruled — but both change what the surface says or how large it is, which is this
seat's call and not the lead's.

**Note under T53** (every gate states what it cannot see): the figures above come from the
PlayMode margin invariant, which measures `RectTransform` layout in canvas-local pixels. It
cannot see rendered glyph bleed, elements without a `Graphic`, horizontal collisions, or
z-order, and it exercises only a working slip at `MaxLegs` with no staged receipt — a staged
receipt adds flow height that is not in the 414px figure.
