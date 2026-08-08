# SureThing — the MY BETS mirror never received S62, and the kit already specifies it

**From:** SureThing UI lead · 2026-08-07 · found on `04a-my-bets-riding`, the frame batch 13 asked for
**Asking for:** nothing to rule. This is a 1:1 gap against a specification that exists — it needs an
editor slot, not a decision. Filed because I was told to stand down before I could build it, and
because how it was found is worth more than the defect.

**Superseding my own report of an hour ago.** I raised this as an open question with "two readings,
both defensible" and asked which form was correct. **That framing was wrong and the kit corrected
it.** I had not read the kit before writing it up. Recorded rather than quietly fixed, because the
C14 audit made this exact mistake on this exact surface — it concluded the kit did not specify the
LEDGER because there was no `components/ledger/` directory, and the screen had in fact drifted from a
specification that existed. Same error, same surface, six days later, and only caught because filing
the question required me to go and look.

---

## What I know

**Three sites on this surface print a ticket identity. S62 reached two of them.**

| Site | Source | Prints |
|---|---|---|
| LEDGER | `SportsbookApp.cs:2186` — `LaptopUi.TicketIdentity(…, withRound: true)` | `R2 · TICKET 02` |
| Staged receipt | `SportsbookApp.cs:1098` — `LaptopUi.TicketIdentity(…, withRound: false)` | `TICKET 02` |
| **MY BETS mirror** | `SportsbookApp.cs:1316` — **hand-built string**, helper never called | **`TICKET 1`** |

**The kit specifies the padded form, and specifies it for this screen by name:**

- `ui_kits/surething/app.jsx:43` — `number: "TICKET " + String(t.length + 1).padStart(2, "0")`
- `components/records/TicketReceipt.d.ts:12` — `/** Printed identity, e.g. "TICKET 01". */`
- `components/records/TicketReceipt.prompt.md:1` — the same component is *"Shown on ENTRY after PLACE
  TICKET, **in MY BETS during the sweat**, and in the Ledger once settled."*

**And the helper already produces it.** `LaptopUi.TicketIdentity` ends
`"TICKET " + (placement + 1).ToString("00")` — one-indexed, zero-padded, the kit's form exactly. It
is in the same file, and the surface's other two identity sites already call it.

So this is not a question about which form is right. **The mirror composes its own string instead of
calling the shared helper, and therefore prints the one unpadded identity on a surface whose other
two print the kit's form.** The fix is routing line 1316 through `TicketIdentity(…, withRound: false)`
— the staged receipt's exact call, for the same reason the staged receipt has it: MY BETS mirrors the
current round, whose masthead already states the scope, so printing `R1` there would be S37
restatement.

**Why it survived.** It is the S60 shape verbatim — one component, two renderings — and the fourth
time this surface has produced it (S33's margin header, S34's ruled ground, S60's biro header, now
this). Each time the cause is identical: a second call site that builds by hand what a shared helper
already builds correctly, so a ruling lands on the helper and one screen never hears it. **A ruling
can only reach the call sites that route through the thing it was ruled on.**

**Why it was invisible until now.** The only MY BETS capture in the set was a fully-resolved ticket,
and no frame ever put a mirror title and a LEDGER title in the same review. Building the riding
state put them side by side. **That is C17 paying a fourth time on this surface** — S62 itself was
found the same way one batch ago, on the cross-round frame that had just been built.

---

## What I cannot see

**The frames say what is printed; they do not say it is wrong.** `TICKET 1` reads perfectly well on
`04a-my-bets-riding` and `05-my-bets-green-dead`. Nothing about the defect is visible without the
LEDGER frame beside it or the kit open — which is exactly why it survived, and why I am not claiming
a legibility finding here. **It is a conformance finding and nothing more.**

**I have not built or run it.** The editor slot was released to Room before this was found, and I was
told to stand down. Everything above is read from source and from the kit at `8610010` — no compiled
tree, no frame of the corrected state. On a surface where source-read findings have twice dissolved
on capture (S32, T26), that distinction is load-bearing, though I would note this one is a string
comparison against a written spec rather than a claim about how something reads.

**I have not audited the other direction.** I checked the three sites that print a ticket identity. I
have **not** swept the surface for other strings that bypass a shared `LaptopUi` helper — given this
is the fourth instance of the shape, that sweep is probably worth more than this fix, and it is not
something I can scope from here. **If one such sweep is wanted, it should be its own item**; naming
it inside this one would bury it.

**Cost, stated because it is not zero.** The fix changes the printed title on every MY BETS capture,
so `04a` and `05` both re-shoot — one paired run, inside a slot already needed for anything else on
this surface. No other state carries a mirror title.
