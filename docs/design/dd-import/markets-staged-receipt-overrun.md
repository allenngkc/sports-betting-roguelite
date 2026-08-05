# Markets → DD · the staged-receipt overrun (the new item S50 predicted)

**From:** markets/sim lead (`markets-2`) · **2026-08-03** · **Blocks:** B1 merge
**State:** S50's two grants are implemented and compile clean (`b979877`). PlayMode 45/46;
the single failure is this. Suite held red as the merge guard, per instruction.

S50's two grants worked and are not in question — the deleted status line returned its 18px
and the leg-row collapse returned 7px × 4 legs = 28px, 46px against the 44px deficit, and
the bare four-leg case now fits. The named consequence is what does not. **With a staged
receipt present, the payout figure measures top −446, bottom −482, against a flow budget
that ends at −370: the flow overruns by 112px.** The staged receipt therefore costs roughly
**114px** of flow — it is not a 2px-headroom problem, it is most of a second leg list. The
arithmetic S50 authorised cannot reach it, and per S50's own instruction I have not tried:
no further spacing was closed, nothing that states a product fact was deleted, the band was
not shrunk, RUB OUT stayed 60×32, and the panel was not grown into `--st-band-tray` (R30).
The shape of the problem is different from T47's, which is why it is a new item rather than
a continuation: T47's deficit was a fixed 44px that a fixed 46px could close, whereas this
one **scales with staged tickets** — up to `MaxTicketsPerRound = 3` receipts can stack in
the same flow above the same anchored band, so any constant reclaim is defeated by placing
one more ticket. That points at structure rather than pixels, and the candidate answers are
all this seat's: the receipts move out of the margin entirely (the kit puts them in the
700px sheet under `PLACED THIS ROUND`, `screens.jsx:50-57` — the markets C14 audit already
carries this as **E-07**, receipts rendered in the wrong region), or the receipt list
becomes its own bounded region, or the margin's flow scrolls after all. **E-07 is already a
ruled-adjacent open item and would close this by construction**, which is the option I would
raise first if asked.

**Note under C25** (instrument scope reported with the measurement): the figures come from
the PlayMode margin invariant, measuring `RectTransform` layout in canvas-local pixels. It
cannot see rendered glyph bleed, `Graphic`-less elements, horizontal collisions or z-order.
It now exercises a full slip on top of **one** staged receipt at `MaxLegs`; it does **not**
exercise two or three receipts, which by the scaling argument above are worse, nor the
board-frozen state, nor the REWARDS/MY BETS passive margins. **Owed and not yet supplied:** a
capture of this state — the existing `09-margin-max-legs` frame stages no receipt, so no
photograph of the overrun exists yet. Cheap to add on request; not added unprompted because
the number is what decides B1 and the frame would only illustrate it.
