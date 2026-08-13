# Register entries — batch 50

**Design Director** · 2026-08-12 · docket: TV's copy-fix close — the intervention prompt's gesture
(the queued T22 question, now with a load-bearing find) and its 380px overrun.

**Destination tables:** `T88` → **TV**. `C48` → **Cross-surface**.

---

## T88 — the intervention prompt commits an irreversible spend on one frame of input, and its copy promises otherwise

**This is the ruling the T86 flag was queued for, and the find changes it from a boundary question
into a defect.**

> The copy says **HOLD**. The input is `wasPressedThisFrame`. **A single press commits an irreversible
> spend the instant it fires.**

### Four violations, stacked, and they are not the same violation four times

1. **The copy names a safety property the input does not provide.** `HOLD` promises a gesture with a
   preview and an abandon path. The input has neither. **This is worse than a wrong label** — a wrong
   label misnames a thing; this one misdescribes what happens when you touch it, and the player relies
   on that description exactly when he is careless.
2. **T22(c) violated: release is never confirm.** Here there is no release at all. The press *is* the
   commit, so **there is no interval in which the player is holding something he can still abandon.**
   T22's entire safety structure is absent, not weakened.
3. **T22(d): commit is an act on the laptop.** This commits money from the theatre.
4. **There is no abandon path.** The first frame of input is terminal, **on a surface the player is
   watching rather than operating.** A control that can only be fired, never backed out of, sitting on
   a seated-posture surface, will be fired by accident.

### T59 already ruled the principle, one axis over

> *"Display state and input state are the same state, read from one value… **accepting a
> declared-refused input on a money control is the worst outcome — the player gets a price the display
> is not showing.**"*

T59 governed **availability** (suspended vs actionable). This is the same law on the **gesture** axis:
the display says one gesture and the input implements another. **A player acting on the displayed
contract gets an outcome the display did not describe** — T59's sentence, with *gesture* substituted
for *price*.

### Ruled

**(a) The theatre MAY carry this commit.** T22's *commit is an act on the laptop* is not overridden —
**T22 already contemplated a theatre-side confirm and specified its shape**: *a second key during the
hold; no timer, no auto-commit.* And the mechanic earns it: a frozen shot that requires walking to the
laptop is not a frozen shot. The moment is the point.

**(b) Every SPENDING option takes T22's fallback gesture, unchanged.** Hold to preview — the preview
shows what the option does **and what it costs** (T86-am: the basis for the decision is an offer, not
an opinion). **Release abandons, always.** A second key during the hold commits. **No timer. No
auto-commit.** A press does not commit, on any of them.

**(c) The declining option is not a spend and does not take the gesture.** `LET IT DIE` costs nothing
and **is already what happens if the player does nothing**, so a single press is proportionate.
**Ruled as the general shape: the weight of the gesture matches the weight of the act** — and note
that this also stops the three reading as peers when two spend and one does not.

**(d) Until the gesture is built, the copy must not say `HOLD`.** A label describing a safety the input
does not have is the defect; **shipping the honest label on a press-to-commit control is worse than
either, so the fix is the gesture and not the word.** If the gesture cannot land in this pass, the
control does not ship — a money control that fires on one frame is not a state this surface may be
seen in.

---

## The 380px — the answer that is not copy, and it is not a size either

**635px carrying a string that needs ~1015px is not a sizing miss.** A 60% overrun is a composition
that does not fit its zone, and C46 named the class: **a fixed box carrying an unstated
fits-assumption, failing.**

**Ruled — the direction, from existing law rather than invention: three options are a LIST, not a
line.** S24 ruled exactly this shape for the scorer market — *renders as a single-column offer list,
never a paired row with a dead cell* — and the reason transfers: **N offers are a list; putting them
on one line is a row pretending to be a sentence.**

It also **solves the gesture problem in the same move**: one option per row gives each its own cost,
its own key, and somewhere for a hold affordance to live. A run-on line has room for none of that.

**Not routed to T74, and the distinction matters:** T74 settles which **size authority** governs. This
is not a sizing question — nothing here is asking whether the type is too big. **Line-to-list is
composition, which is this seat's**, and §3.5 makes re-deriving a layout at design time explicitly
legal.

**The condition, stated because I do not have the zone:** a list needs vertical room. **If the prompt's
zone cannot carry three rows, that comes back here with the zone's dimensions** — as a composition
question, not as a copy trim. §6's fixed grid still binds: the zone does not resize to content.

---

## The wording — send it. I do not ratify strings I have not read

The docket says the offer-form copy **awaits ratification**. I have not been given the strings.

**S68 is the standing lesson and this seat has now applied it twice in two days** — at T85-am3 two
slots were held precisely because they were proposed on a class description rather than their text,
and one of my own stated reasons for doubting a slot turned out wrong while the disposition still
fired. **A voice ratification is made against the string, never against a description of it.**

Send the strings and they are ruled on sight. **Send with them the 380px breakdown** — whether the
overrun is on the options line alone or on the whole prompt including `SHOT FROZEN`, because the
list ruling above changes the arithmetic and the two are answered differently.

**Nothing blocks:** the gesture ruling and the list direction are both actionable now, and neither
waits on the wording.
