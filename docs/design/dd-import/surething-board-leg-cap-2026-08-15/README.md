# The board at the leg cap — where every further pick is a dead click · 2026-08-15

**Ordered by:** DD batch 84 — the dead-click treatment is authored against a frame, never blind.
**Built at:** `821d533` (the additive gesture) · **Surface:** SureThing — the laptop, FORM lobby.

**NO READ IS OFFERED, AND NO TREATMENT IS AUTHORED.** This set exists so the treatment is designed
against the state as it actually renders.

---

## The state, and why it could not exist a week ago

**Four legs, `MaxLegs` reached.** This state is NEW to the additive gesture. Before it, a second pick
on a match REPLACED the first, so the slip could always take a click — there was no such thing as a
board that refuses one. Now a pick sticks, the cap binds, and the refusal has nowhere to appear.

**The capture asserts its own premise** rather than assuming it: it fills to the cap, clicks an
unmarked offer on a fifth matchup, proves the slip does not move, then clicks a second market on an
already-marked match and proves the same. A frame of a board that could still take a pick would have
had the treatment authored against the wrong state.

---

## What is in frame — and the decomposition is the finding

The slate's six matchups carry **18 moneyline prices**. At the cap they divide into three groups
that are **pixel-identical to each other**:

| | count | what a click does |
|---|---|---|
| marked (biro ring) | **4** | **removes** the leg — live, but in the opposite direction |
| unmarked, on a marked match (`DRAW`, `HOME` on 01–04) | **8** | **nothing** |
| unmarked, on an unmarked match (05 `MALLARDS`, 06) | **6** | **nothing** |

**Fourteen of the eighteen prices on this board are dead, and not one of them says so.** `05
MALLARDS · AWAY +239` is the clearest instance in frame: an entire matchup rendering exactly as
01–04 rendered in the moment before they were picked. Every market behind each `MORE ›` is dead on
the same terms and is not in frame at all.

**And the four live prices are live BACKWARDS.** The only clicks the board still accepts are on the
prices that already carry a ring, and they un-pick. So the board's live surface and its marked
surface are now the same four cells — which is the opposite of what the ring has meant everywhere
else on this screen.

---

## Against the ruled general rule

> *A refusal knowable before the act shows BEFORE it — the act never happens.*

**This refusal is knowable before the act in the strongest sense available:** `MaxLegs` is a config
constant and `Picks.Count` is known at render time, so every one of those fourteen cells could be
answered before it is touched. Nothing about it waits on the click. The current build answers only
by doing nothing when clicked, which is the act happening and failing silently.

**S2 bars the obvious shortcut, and it is worth naming why it is barred rather than merely listed.**
The board already dims a price — `frozen ? LaptopUi.Dim(LaptopOs.Muted) : LaptopOs.White` — and that
dim already means **the round is locked**. Reusing it for the cap would put two different facts in
one channel, on the same control, on the same screen. A player who learned the dim as "the board has
closed" would read a full slip as a closed round.

---

## NOT CLAIMED

- **No treatment is proposed.** Not a dim, not a hidden control, not copy. The three dispositions are
  pre-committed on the DD's side and this seat authors none of them.
- **No read of whether the state is even legible today.** Whether a board with fourteen silent cells
  reads as broken, as full, or as nothing at all is the call this frame exists to inform.
- **`MORE ›` interiors are not photographed.** Every market body behind them is dead on the same
  terms; only the moneyline column is in frame.
- **This is not a before/after pair.** The before-state is a board that could not reach this
  condition, so there is nothing to compare it against.
- **The margin is incidental here.** It shows the cap from its own side — `4 SELECTIONS`, a live
  `PLACE TICKET` — and that half is not what is being asked about.
