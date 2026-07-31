# Register entries — 2026-07-31, batch 2

**Transcribe into `main-2/docs/design/REGISTER.md`.** Batch 1 (S11, S14, S15, S16, T11, T15) is
already transcribed; these are the items that were awaiting re-mount. All seven documents were
re-attached and read in full.

---

## C3 — TV canvas HDR coverage. **RULED. Gate lifted; TV 3D unblocked.**

**State change:** Coverage → Design Director → **CLOSED**.

**First, the lead's correction is accepted in full.** C3 is not an engineering blocker. The
mechanism exists, is committed (`1aa74c3`), is tested, and the canvas backgrounds were lifted to
`(0.048, 0.055, 0.068)` to match the room's emissive quad. Close it as engineering. The room
worktree was never blocked on capability.

**Second, the current coverage is wrong — but in the opposite direction to the one the lead
expected.** The shader is on three graphics: cash-out amount, payout amount, gold flood. The lead
framed narrowness as enforcing §3 by construction. It over-enforces: §3 names the L4 occupants
explicitly — *"the score at a goal, the cash-out when actionable, the payoff at its callback"* — and
§7 names the ball as *"the only object permitted L4, and only at a payoff."* **Two elements the
approved design grants L4 physically cannot reach it today.** Enforcement by construction is
correct; the construction is just missing two of its four members.

**Ruling — eligibility is exactly the set §3 and §7 already name, and nothing else. Four graphics:**

| Element | Status |
|---|---|
| Cash-out amount + gold flood — actionable, and the accepted punch | already covered |
| Payout amount, at its callback | already covered |
| **Score numerals, at a goal** | **ADD** |
| **The ball, at a payoff** | **ADD** |

**Explicitly not eligible, and why.** The **live-leg pulse** — the lead asked directly. No: `LIVE` is
L3 and is the surface's only pulse, so boosting it makes a *recurring* element the brightest thing on
the display and destroys the scarcity that lets L4 mean *now*. Also out: the event strip (L2, punches
to L3 only), risk/pays (L2 gold), won-leg gold (L3, solid, no pulse), team hues (muted, local, and the
least prominent colours by design), and the metadata row (chrome).

**Eligibility is not simultaneity.** Widening coverage means the one-L4 rule can no longer be an
artefact of who holds the shader, so it needs an explicit invariant: **one L4 owner token; a graphic's
`_HdrBoost` may exceed 1.0 only while it holds it.** Test: assert no frame has two graphics above 1.0.

**The arbitration rule, because two of the four can want it at once.** A **momentary punch preempts a
sustained state**, for the duration of the punch only. In practice the two rules already agree: at a
goal the cash-out is genuinely re-pricing, so §8's `updating` state puts it at L3 on exactly the frames
the score wants L4. No contrivance needed — and where the kit had a goal beat sitting on `actionable`,
that was a truthfulness bug, now corrected.

**Boost ceiling stays 1.8, one value, no per-element ceilings** — nothing should compete on degree.

**Bloom volume:** the lead's preference for one shared global pass until a render proves it
insufficient is endorsed. Do not build the second sweat-priority volume on speculation.

---

## C8 — Bloom legibility floor. **RULED** (arising from C3).

The lead's position — the small metadata row is system chrome, PRD §8.1 lowest priority, allowed to
degrade — is **confirmed, with one amendment.**

Protected, may never be sacrificed to bloom: **the score, the clock, each live leg's `NEED` line, the
cash-out state — and the ticket's risk/pays.** The lead's list omitted risk/pays; `VISUAL-DESIGN` §12
makes "state risk and payout" question 4 of the couch review, and failure of questions 1–5 is major.
It is load-bearing.

May degrade: round, bank, seed — reference, not action. **The floor is that a value the player is
asked to *act on* never blooms out.** Confirm on the seated capture now that the harness is self-serve.

---

## T16 — Layout B: win-probability display and momentum tape. **RULED. 3C commit unblocked.**

**Momentum tape: IN. Win-probability numeral: OUT.**

**The tape stays.** PRD §4.2 names it in the one-revealed-source-of-truth law. It is a *revealed*
channel, and dropping it would silently narrow that law — a construction call cannot do that. It is
also the only thing on the surface carrying movement over time; the ticket column carries state, the
scorebug carries the instant.

**The numeral goes.** Three reasons, any one sufficient: `VISUAL-DESIGN` §7 already bans duplicating a
win percentage in the event strip, and a standalone readout is the same duplication in a different
zone; the product's thesis is that locked odds make the player's read the game, so a live probability
is the house doing the reading for them; and the brightness ladder has no free tier for it — it would
land at L2–L3 competing with the `NEED` line, which is question 3 of the couch review.

**Where the tape goes:** at the **foot of the scorebug**, spanning the stage. It is match truth over
time, and the scorebug is where match truth lives. It is *not* an event, so it never enters the event
strip — that line is one authored explanation that punches once and settles.

**Three hard constraints on it:** **no numerals** (the moment it needs one it has become the banned
readout); **no hue** — white and grey only, per §4's "everything else is colourless"; and **never
above L2**, so it cannot compete with the score above it or the `NEED` line beside it.

Built and running: `components/tv/TvMomentumTape.jsx`, wired into `ui_kits/tv-sweat/`.

---

## R5 — Full PBR surface maps. **DESIGN-VERIFIED.**

**State change:** Implemented · review pending → **Design-verified · DD 2026-07-31.**

The maps are correct and the deliverable is met. The relief not reading was **not** a texture failure:
the sd-versus-visibility inversion (ceiling weakest at ~9 yet most visible; fabric strongest at ~80 and
invisible) rules out map strength, and the sin θ measurement explains it. Accepting the lead's finding
as a standing design law: **surface detail is gated by lighting, not by texture authoring.** Asking for
"more surface detail" will not produce it; asking for light that varies across a surface will. Any
future art request that reaches for texture where the real deficit is grazing light should be refused
with this line.

---

## R6 — Indirect light, Adaptive Probe Volumes. **DESIGN-VERIFIED.**

**State change:** Implemented · review pending → **Design-verified · DD 2026-07-31.**

×6.3 relief contrast on the right wall, ×1.67 whole-frame, with **mean luminance held** (33.0→32.0,
38.6→38.4, 29.0→28.5). The signed-off value structure is intact — the room is not brighter, the
texture became visible, which is precisely the approved direction. The two flat gate views
(seated-TV ×1.00, focused-laptop ×1.04) are narrow-FOV close-ups framed on emissive screens with
almost no GI-lit surface in frame; that is expected and is not a miss.

These are the studio's **first two Design-verified items.**

---

## R9 — Ambient rebalance. **APPROVED as a bounded experiment.**

**State change:** Candidate → **Approved · DD 2026-07-31, gate re-run attached.**

The design argument is stronger than the lead put it. Flat ambient does not merely dilute R6 — it
works against the direction's own premise. *The machines are nicer than the life*: lowering the room's
floor raises the screen-to-room contrast the whole thesis rests on, and the three light sources become
more distinguishable, not less.

**Bounds, which are the ruling:** the bounded first step of ~30–40%, the full 8/8 gate re-run, and two
numeric acceptances — **bunk-2 mattress luminance holds at 43.9 ±1** (the ratified *legible as
occupied, never legible as empty* requirement), and **region means stay within 10% of the R6 values**.
Acceptance is **value separation, not more relief**; if relief rises while the room crosses into the
horror-cell failure state that §10 names, the experiment failed. If the ladder fails, revert — R6
stands on its own.

---

## R10 — Couch-corner grazing source. **APPROVED with a route change.**

**State change:** Candidate → **Approved · DD 2026-07-31, bounce-first.**

The corner must gain **directional variation** so the room's strongest normal map finally reads. That
is the design requirement. *How* is where I am amending the memo.

Four attempts at this class of fix have been reverted, all for the same reason: a grazing light needs
an offset to be bright, the offset puts it into the room, and a bunk is in front of every wall worth
grazing. A fifth attempt at the same manoeuvre is the expensive route. **R6 just proved that
directional bounce is the lever that works on this room** — so try bounce first: raise what reaches
that corner through the probe volume rather than adding a fifth direct source.

If bounce genuinely cannot rescue a corner that dark — which is the lead's stated expectation and may
well be right — then the grazing source is approved as the fallback, **with `y < 1.50` written into the
spec and bunk-2 luminance at 43.9 as the gate**. Lower confidence is accepted and recorded; I would
rather spend one bake than a fifth revert.

---

## R7 review — the three open questions. **RULED.**

**5.1 Ceiling soot — DROP IT.** The ceiling is the one surface where relief already reads, precisely
because the fluorescent rakes it at θ ≈ 87°. A soot halo would flatten the read that works, on the
room's most visible surface, and it currently renders as a hard-edged rectangle for reasons nobody has
explained. Decay is already carried by the walls, the conduit and the construction. **A clean ceiling
under strong raking light is the better read.** R7 stays parked as-is; do not spend an integration slot
on this.

**5.2 URP Decal Renderer Feature — NOT JUSTIFIED YET.** R7's own diagnosis is that the failure was
**placement versus camera** — a planning error, not a technique ceiling. Until wear is placed where the
three cameras actually look, we do not know whether the technique is the constraint, and a
shared-renderer change across three worktrees is too expensive to spend on an untested hypothesis.
**Re-place the existing wear against the three camera frusta first** — cheap, no renderer change. If
wear that is genuinely in frame still under-reads, that is the evidence that justifies the shared
change, and I will approve it on that evidence.

**5.3 How far from the concept — NEITHER the current state nor the concept is the bar.** The concept is
a style reference, not lighting truth; it shows light arriving from several grazing directions at once,
which a room with one overhead tube and three local sources cannot honestly produce, and chasing it
would mean inventing light the construction does not have. **The bar is the direction's stated read:**
painterly semi-realistic, olive/khaki/rust under a warm dim fluorescent, three distinguishable
sources, the machines nicer than the life. R6 got the walls there. The remaining gap is the couch
corner and overall value separation — which is exactly R9 and R10. **Sign off R5 and R6 now; re-review
after R9/R10 land.** R8 may proceed on that basis.

---

## Still open — the art-authority gap (inbox item 2)

Proposal for Allen below. Not a ruling; it needs his approval on shape before anything is authored.
