# Register entries — 2026-08-03, batch 8

**Transcribe into `main-2/docs/design/REGISTER.md`.** Issued at authoring time per C22. Ruled from the
tables (current through batch 7), not from batch files.

New IDs: **C25**, **C26**, **S50**, **R28**, **R29**, **R30**. Amendments: **C24**, **T47-am**, **R19(b)-am**.

---

## C24 — Constitution. **DRAFTED. Awaiting Allen.**

**State change:** Ruled (batch 7, precondition set) → **Drafted · DD 2026-08-03 · for Allen's
approval.** Precondition met: batches 5 and 7 are in the tables.

Draft: **`constitution-2026-08-03-DRAFT.md`**. Canonical home on approval:
`main-2/docs/design/constitution.md`.

**Six sections, thin by construction.** Authority · Evidence · Deviation · Inventories and gates ·
Variety · Amendment. Carries C9–C12, C14, C16–C20, C22, C22.1, C23, T18, T19 and the gate-visibility
instruction. Every clause names the register item it was promoted from, so a lead can read the case
behind the rule.

**It contains no colour, no type, no layout, no palette.** Stated in §0 as the document's own reason
for existing: `08` failed because one document tried to govern four registers, so this one governs
none of them — it governs how they are governed. C24's batch-7 finding (colour was never its content)
is the draft's opening paragraph.

**Three things the draft adds that were implicit but never written down.** Flagging them because they
are new text, not transcription, and Allen should approve them as such:

- **§1.5 — the seat's own errors are recorded as its own.** Where a ruling was wrong, the amendment
  names the *ruling* as the defect, not the lead who implemented it faithfully (S15-am, S25-am, S31-am,
  T31). A register that hides the seat's errors is not an audit trail.
- **§2.5 — measure the rendered thing, not the source.** Where a build step or a cache sits between
  source and frame, an assertion about the source is not a measurement of the surface. This is the
  generalisation of a mistake I made three times in one session before catching it.
- **§2.6 — a confounded measurement closes nothing.** Promoted from T49, so the next saturated
  instrument is returned rather than adjudicated.

**§4.2 carries the fortnight's most uncomfortable table:** four green gates — T19's signature
diversity, T47's containment epsilon, R16's collider count, S49's wallpaper `Graphic` — each found to
be measuring nothing, none caught by a suite, all four caught by captures. It is the strongest
argument in the document and it should stay visible.

**Owed and named as owed:** SureThing's and TV's owning documents (§1.1). The room's is approved
(R13); the phone is a stub by C9 and the table says so. **I am not writing the two missing owning docs
into this session** — they are surface-content documents and both surfaces are mid-conformance-wave.
Sequence them after T41/T48 and the LEDGER close-out.

---

## S50 — Markets' 44px deficit. **RULED. Panel growth REFUSED — that 34px is the OS tray.**

**State change:** markets B1 blocked → **Ruled · DD 2026-08-03.** Closes the last blocker on the B1
merge. T47's two fixes verified at `774a1c9` and accepted as built.

**The second candidate is not available, and this is the important part of the ruling.** The panel
occupies 140..670 of 704. Those are not arbitrary numbers: **34 rail + 38 tabs + 68 masthead = 140**,
and **140 + 530 = 670**. The 34px below the panel is the **OS tray** — `--st-band-tray`, the closing
term of S2's locked band arithmetic (34 + 38 + 68 + 530 + 34 = 704), and half of the NotebookChrome
that S8 is **Design-verified** on.

Growing the panel into it would delete the tray. The tray is where the laptop stops being a
sportsbook and becomes **his machine** — the single most important constraint in the system, the one
the whole two-register architecture rests on, and the thing S48 is currently folding the desktop into.
**There is no unused screen on this surface.** Every band is spoken for; that is what a locked
composition means.

Recorded as a finding in its own right: an unlabelled band read as free space. **A locked band is not
headroom** — if a band's purpose is not legible from the layout, the layout should name it.

**What is granted.**

1. **Delete `PRICES FINAL. NOTHING YOU DO MOVES THEM.` — 18px.** This is not a new decision and the
   lead should not have had to ask: **S37 already forbids it** (nothing restates the masthead's or the
   board header's scope) and the markets C14 audit already carries it as invented (M-09). An
   unexecuted ruling is not an open question. Execute it.
2. **The remaining 26px comes out of the leg row, by applying S39's discipline where it already
   belongs.** S39 collapsed the LEDGER record to **one baseline** — identity left, figures under their
   heads, terminal word right — and returned ~19px per record. **A margin leg is the same kind of
   object as a settled record**: an identity, a price, a market, a state. It has no business carrying
   a different vertical grammar. Put the identity and the price on one baseline with the market line
   beneath and RUB OUT vertically centred against the pair; at four legs that is the deficit.
3. **The margin does not scroll, and the reserved band does not shrink.** SKIP 34, LOCK 52, PLACE 44
   are exact element-kit sizes and the anchored stack is T47's whole point. The 60×32 RUB OUT stays at
   size — it exists at that size precisely because a mis-click here costs money.

**Yield order, standing for any future margin deficit: spacing, then repetition, then nothing.**
Nothing that states a product fact is deleted to make a layout fit, and no hierarchy is reordered for
a shortfall (T51). If a deficit ever survives that list, it comes back to this seat, not to a delete
key.

**Named consequence.** Once the 18px is executed and the leg row collapses, a **staged receipt** still
adds flow the 414px figure never contained — the lead flagged this himself and it is the right flag.
Re-measure with a staged receipt present before B1 is called clean, and if *that* deficit is real it
is a new item, because the fix will not be arithmetic.

### T47-am — the "never needs to scroll" clause is withdrawn

**Amended · DD 2026-08-03, my own error.** T47 asserted the margin's leg list "at a hard cap of 4
never needs to scroll." That was an assertion, not a measurement, and the measurement says the flow
is 44px over at exactly that cap. The conclusion holds — the margin still does not scroll — but it
holds because the row collapses, **not because I was right about the arithmetic.** The reasoning is
corrected on the record so the next deficit is not argued from my sentence.

---

## C25 — Instrument scope is part of a measurement. **LAW.**

**Ruled · DD 2026-08-03.** Promoted from the markets lead's own note, which is the best-formed
instance of C18 §4.2 the studio has produced: the PlayMode margin invariant reports its figures **and**
states that it cannot see rendered glyph bleed, `Graphic`-less elements, horizontal collisions or
z-order, and that it exercises only a working slip at `MaxLegs` with no staged receipt.

**A measurement is reported with its scope attached, in the same breath, unprompted.** Not on request,
not in a footnote, not when challenged. A number without its scope invites exactly the four vacuous
gates C18 §4.2 tabulates.

The lead reached the standard before it was written. Recorded as the pattern to copy.

---

## R19(b)-am — Metal-colder-than-room. **AMENDED: the channel yields, not the albedo and not the lighting.**

**State change:** R19(b) ruled (batch 5) → **Amended · DD 2026-08-03.**

**"Colder" is struck as a requirement. The ≥2-channel rule stands, and the two channels are value and
finish.**

**Why this is rulable now, before surface-pure numbers.** The finding is not about magnitude, it is
about direction, and the direction is settled physics this studio has already ruled on. **T48 turned on
exactly this**: under one warm source, on warm dirty plaster, the room cannot return saturated cool
colour — that is Law 1.1's mechanism, and it does not stop applying because the surface in question is
steel instead of plaster. **A requirement that the metal read colder than the room is a requirement
that the room break its own top law in one region.** The first-pass boxes affect how much, not whether;
the lead's caveat is correct and does not gate this ruling. Cut the harness regions anyway — the
numbers belong in the handoff — but do not hold R19(a) for them.

**What was actually being asked for, and where it lives instead.** The intent of R19(b) is that the
institution's metal must not read as *his*. Hue temperature was the wrong carrier for that. Metal reads
institutional through **value** (harder, darker diffuse) and **finish** (tight bright specular against
the laptop's dull plastic, hard-edged chipped paint against worn smooth plastic). Both survive a warm
key; both are already in R19(a)'s ≥2-channel formulation. **The requirement loses nothing except the
one channel that could not have delivered it.**

**No lighting instrument for this. Refused explicitly.** R12's grazing class reveals **relief**; it is
not a colour-temperature tool. Adding a cool light to make metal read cold would invent a light source
to satisfy a document — that is T48's rejected Option D wearing new clothes, and it would break the
three-distinguishable-sources rule the room's lighting design is signed off on. **The room has one warm
key and a short-reach cool window, and neither is available as a metal-tinting device.**

**Endorsed:** the lead held the albedo line under pressure from his own measurement, which is exactly
what R19(b)'s guard was written for ("too-dark-after is an R12 lighting finding, never an albedo
licence"), and stated the first-pass caveat unprompted rather than letting indicative numbers pass as
harness numbers. Both are the standard.

**Unblocked:** R19(a) proceeds on value + finish. The laptop/TV/phone material split is still the
highest-ranked room item and still outranks all room polish.

---

## R28 — PhoneScreen ownership. **RULED: the room owns the object; no one owns the content yet.**

**Ruled · DD 2026-08-03.** Ruled from principle — the question text did not travel with the bundle, so
if the lead's question was narrower than this, re-ask and I will answer the narrow form.

**The object is the room's.** Geometry, material, placement and its named interaction `MeshCollider`
(R16 keeps it, deliberately, as one of the two named interaction colliders) are all room-owned. R19(a)
already fixes its register: **the phone is his, so it sits near the laptop's material register and not
the TV housing's.** It is the second personal machine in the room, not a third institutional one.

**The content is owned by nobody, and that is the correct state.** C9 keeps the phone a **stub** — no
owning document, therefore no authority over what is on the screen. Until a phone surface exists,
**the phone screen is dark**, and the room may treat it as an unlit surface with the same discipline as
any other: no invented UI, no placeholder app, no lorem content. A dark phone is honest; a mocked-up
phone is a surface nobody has authority to approve.

One useful consequence: a dark phone screen **cannot become a C13 instance.** The laptop and TV both
spent a cycle rendering superseded content inside room captures; the phone is structurally immune while
it stays a stub.

## R29 — Gate 2 active-state. **RULED: a gate that ran against one state certifies that state only.**

**Ruled · DD 2026-08-03.** Same caveat — ruled from principle, question text absent; re-ask the narrow
form if this misses.

**C18 decides it without needing the specifics.** A gate certifies the configuration it ran against.
If Gate 2 was read against an inactive or idle state, then **the active state is unproven** — not
failed, unproven, which is exactly R22's distinction and the reason R22 is void rather than red.

Three requirements, in order:

1. **Name which state the existing reading covers.** Amend the gate's own line to say so. A gate that
   does not state its configuration is C18's bare-count defect in a different costume.
2. **State what it cannot see** (C18 §4.2, now C25's reporting form) — including whether it can
   distinguish the two states at all. If the harness cannot tell active from inactive, that is the
   finding, and it is a bigger one than the gate's result.
3. **Re-run against the active state, or record it as uncovered.** Both are acceptable. Silently
   reporting a pass that covers one of two states is not.

**Do not report Gate 2 as a pass in an N/N total until this is settled** — same instruction R22 carries
about 8/8, same reason.

## R30 — Unlabelled bands read as free space. **LAW.**

**Ruled · DD 2026-08-03.** Promoted from S50. **A locked band is not headroom.** Where a composition's
bands are ratified (S2's 34/38/68/530/34), no band is available as spare space regardless of what it
currently draws — the OS tray draws little and carries the laptop's entire personal-machine register.

Standing instruction: **a locked composition names every band, and a lead proposing to grow into one
cites the band by name.** The markets lead's reading was reasonable given what the layout showed, which
means the layout under-documented itself. Applies to all three surfaces' fixed canvases.

---

## C26 — Owning documents owed. **OPEN, sequenced.**

**Ruled · DD 2026-08-03.** Recorded so it does not go missing behind the constitution's approval.
Under C9, two owning documents do not exist: **SureThing's** and **TV's**. The room's is approved
(R13); the phone is a stub by design.

They are surface-content documents, and both surfaces are mid-conformance-wave with six colour and
layout rulings open between them. **Writing an owning document against an unsettled surface produces
another `08`** — a document asserting values the build is about to change. Sequence: **SureThing's
after the LEDGER close-out and the desktop fold (S48); TV's after T41, T48 and T49 land.** Neither is
blocking; both are owed before either surface is called finished.

---

## Ordering for the orchestrator

**Unblocked now:** markets B1 (S50 — two changes, then re-measure with a staged receipt), room R19(a)
(R19(b)-am — value + finish, no wait for surface-pure numbers).

**Unchanged from batch 6:** T41 → T48 (T45 subsumed) → T43 → T46 → R19(a) → R16/T57 → R22 → T42 → R19(c)
→ T44 → T49/T50.

**Awaiting Allen:** the constitution draft (C24). Nothing is blocked behind it — every clause is
already in force as a ruled item; approval makes it readable in one place rather than newly binding.

**Not addressed this session:** SureThing's and TV's owning documents (C26, sequenced above).
