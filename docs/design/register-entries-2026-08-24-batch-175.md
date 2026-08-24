# Register entries — batch 175 (2026-08-24)

**The capture is REFUSED, and not for want of trying: `K21`'s state CANNOT appear in a piped
transcript, by construction. Proven at source before a build was spent.** The route that does settle
it is cheaper than the capture — and it is a gate, not a shutter.

**Two rows.** **Destination tables:** Console (`K21-am`) · Cross-surface (`C60`).

**Four source reads. No build run, no transcript docked — and the reason is the finding.**

---

## The rows

| K21-am | `K21` IS NOT PIPE-SHOOTABLE — the state is behind a keypress and `Hold` short-circuits on redirected input | **CAPTURE REFUSED WITH PROOF · DD 2026-08-24 batch 175, on Allen's instruction to shoot it. Nothing was built and nothing was docked; the reason is below and it is a source fact rather than a failed attempt.** **THE BARRIER, and it is absolute: `SweatRenderer.cs:474` — `if (Console.IsInputRedirected) return Input.None;` — is the FIRST line of `Hold`. **`Input.FastForward` has exactly ONE producer (`:491`, inside `Hold`, past that guard) and ONE consumer (`:311`).** So a piped run can never set `fastForward`, `fastForward` stays false for the whole sweat, and the gate `K21` is about — `if (fastForward && onFinalLeg) fastForward = false;` — **has nothing to clear.** The defect is not merely unobserved in a pipe; **IT CANNOT BE THERE.*** **SO THE CAPTURE WOULD HAVE PROVED NOTHING AND WOULD HAVE READ AS A CLEAN RUN** — the worst outcome available, because a green transcript against an unreachable state is the `C55` shape this studio has already paid for twice.** **A SECOND SYMPTOM, FOUND WHILE CHECKING REACHABILITY AND NOT IN `K21`: `onFinalLeg` has a VISIBLE consumer as well as a timing one. `:492-496` prints `"  (the final leg must be sweated — no fast-forward)"` when `F` is pressed on the final leg. **Under the defect that refusal NEVER PRINTS on a same-match ticket's final telling and `F` is silently ACCEPTED** — a STRING-level consequence, which is stronger evidence than timing and is the reason the route below is cheap.** **THE ROUTE THAT SETTLES IT — A GATE, NOT A SHUTTER: `onFinalLeg` is a pure comparison. `lastLeg = ticket.Legs.Count - 1` (`:242`) is the highest leg INDEX; `evt.LegIndex` is now the telling's ANCHOR leg (`DramaEvent.cs:19-21`). **On a two-leg same-match ticket the anchor is leg 0 and `lastLeg` is 1, so `onFinalLeg` is FALSE for every telling of that ticket** — assertable with no rendering, no timing, no terminal and no seed luck, in the console's own exact-gate style (§13: *"every geometric gate here is a string-length assertion, which is exact rather than measured"*). **Mutation-test it the way `K17`'s gate was: restoring the contiguity assumption must fail it.*** **`K21`'s finding is UNCHANGED and is not weakened by this — only its evidence route is** | batch 175 |
| C60 | What a PIPED console transcript cannot carry — promoted on the second catch | **Law (register-level, `C46`/`C55`'s standing — a PRACTICE clause about an instrument, not a product one) · DD 2026-08-24 batch 175, promoted from two independent catches in four days.** **THE CONSOLE IS SELF-SHOOTABLE AND THAT REMAINS TRUE — piping stdin into the exe is the studio's cheapest evidence and it has settled real questions. **What is now measured is its BLIND SPOTS, and they are not obvious from the outside.*** **(1) COLOUR — markets, `888cc6d`: *"`ConsoleColor` does not survive a redirect — zero escape bytes."* MEASURED, not inferred. `B9` was routed out of that lane for it and needs a human at a real terminal. **(2) INPUT-GATED STATE — this batch: `Hold` returns `Input.None` on `Console.IsInputRedirected`, so ANY state reached only through a keypress is unreachable in a piped run.** `K21`'s fast-forward is the instance; cash-out-by-key (`:487`) and the save prompt sit behind the same guard.** **THE CLAUSE: BEFORE PIPING A CONSOLE CAPTURE, ASK WHETHER THE SUBJECT NEEDS COLOUR OR A KEYPRESS. If either, the pipe cannot carry it and a transcript will come back CLEAN — which is worse than coming back empty, because a clean transcript against an unreachable state reads as evidence of absence.** **`C55` IN A SECOND MEDIUM: that law says a green capture proves nothing if the subject scrolled off. **This says the same about a subject that could not have been on the screen at all** — and both fail the same way, by returning something that looks like a pass.** **AND THE CONSTRUCTIVE HALF, because two blind spots do not make the instrument bad: what the pipe DOES carry is every string, every width and every line count — which is what the console's own §13 gates assert, and it is why `K21`'s route is a GATE rather than a re-shoot. **Where the pipe is blind, the exact-assertion style is not** | batch 175 |

---

## For the orchestrator

- **No capture was commissioned and none is owed.** `K21` wants a gate assertion, which the next
  markets seating can write without a terminal, a seed or a shutter.
- **`K22` still wants nothing but a two-line source swap** and is unaffected by any of this.
- **`B9` and `K21` are now the same class** (`C60`) — both need a human at a real terminal, or an
  assertion instead. Worth pairing them if a terminal session is ever scheduled.
- **Backlog is 173–175.**

## Limits

- **I did not build or run anything**, so this rests entirely on reading four call sites —
  `Hold`'s first line, `Input.FastForward`'s single producer and consumer, and `lastLeg`'s
  definition. **All four are quoted to a line so the refusal can be checked rather than trusted.**
- **`C60` names two blind spots, not a complete list.** Anything else a redirect strips —
  cursor control, viewport, timing — is unexamined here; the two named are measured.
- **Nothing about `K21`'s substance is re-ruled.** Only its evidence route changes.
