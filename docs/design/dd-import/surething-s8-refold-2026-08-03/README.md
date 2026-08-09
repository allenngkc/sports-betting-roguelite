# S8 re-verification — the desktop after the chrome fold (S48)

**From:** SureThing UI lead · 2026-08-03 · HEAD `3a85f23`
**For:** the DD's re-review of S8, which S48 returns to review by its own terms.

## What changed on the surface S8 was verified on

The desktop's own 54px taskbar is gone. It now carries the shared `NotebookChrome` — the 34px rail
and the 34px tray — and the wallpaper is the remainder rather than the whole screen. HOME landed on
the rail's identity band, the centre `SURETHING · LEDGER` label on the tray's real app slots, and
`02:47 · 12%` on the rail's own clock and battery. Nothing was left over.

Three earlier items in the same block also changed this frame, and are in the register rather than
here: the wallpaper's wordmark and tagline are deleted (S44/S45), the icon glyph left the player's
ink (S44 via S47's wording), and the icons now speak a two-state installed/not-installed vocabulary
(S47). `(soon)` is gone.

## Frames

- `*-11-desktop-*` — the state to re-verify against. Flat 1024×704 and through the room camera.
- `*-01-form-lobby-*` — the in-app screen, same run, included only so the claim below is checkable.

## What I measured, and what it does not cover (C25)

Comparing the two frames: the **rail band (y 0–33) is 100% pixel-identical** across the desktop and
the in-app screen — 17408 samples, every other column, zero differing. The **tray band past the app
slots** (y 670–703, x ≥ 420, which is where the two legitimately differ, since in-app the sportsbook
is running and reads pressed-in) is likewise **100% identical** over 10268 samples. The rail's own
border-bottom draws at 61,61,42 against the `--rule` token's 60,60,44 on both.

That is the fold's whole claim — one chrome consumed twice, not two copies that resemble each
other — and it is a comparison, which on this surface is the only kind of colour check that has
never misled anyone.

**It does not answer any composition question**, and those are the ones I would expect the review to
turn on:

1. The rail sits 86px above the first icon. That gap was the wordmark's; nothing occupies it now.
   Whether it reads as intentional space or as an unfinished screen is a judgement I cannot make
   from a measurement, and I deliberately did not build S44's optional dead-manufacturer wordmark
   into it — the rail already carries the machine's own marks, so a second one seemed like exactly
   the duplication the fold was removing.
2. The tray brings the **MESSAGES slot and its unread badge** onto the desktop for the first time.
   It is shared furniture and it followed the fold, but nobody has ruled that a desktop should carry
   it.
3. The icon chips and the chrome are both `--ground-3`, so an installed app's tile is the same
   material as the rail and tray around it. Correct by token; unexamined as composition.

Nothing is blocked on the answer. Suites at this commit: EditMode 76/76, PlayMode 46/46.
