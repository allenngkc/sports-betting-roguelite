# Design Director inbox — from the orchestrator

Items routed to this seat. Clear a line when it's registered or resolved.

**Seat note (2026-07-31):** the DD seat has no repo access and its worktree mounts
dropped. Items below marked *awaiting re-mount* need Allen to re-attach the
referenced documents via Import before the DD will rule on them.

**T25.1 scope note (Allen, direct observation, 2026-07-31):** the glass-containment
failure is wider than the actor layer — charts and plain text lines also pass in
and out of the TV panel. TV lead's fix is now a full-layer containment audit.

**Fidelity standard (Allen, 2026-07-31):** UI should match the design system as
close to 1:1 as possible. Deviations minimal, documented, DD-signed; close past
deviations where cheap. To be registered with a safe number at Batch-4
transcription.

**Kit hygiene (TV lead, 2026-07-31):** the tv-sweat kit README's "Known debt in
the shipped build" section is stale — scanlines + static crawl were removed at
`842382d` (T8); chromeCyan and emission rest values remain accurate. Now that the
kit is standard-bearing under the 1:1 rule, please correct it in the next export.

**TV evidenced items (2026-08-01, post-C14 window):** (1) `goldInk #0A0C10` sits
below DESIGN.md's black floor — canon self-contradiction, both are yours; (2)
`LooksLikeRetiredRed` misses `#FF4038` by 0.00098 — three shipped guards assert
less than they read as asserting; (3) **T24 amendment needed:** six authored 76px
slots require 456px against 416px available — the ruling as written cannot fit.

**SureThing C14-audit dispositions (2026-08-01, from `571675c`):** 14 needs-DD
items headed by a record-row hierarchy inversion — full list in
`[ST] docs/.../C14 audit` doc. Caveat the DD should hold: populated-ledger states
are unphotographed (all captures show the empty ledger); a populated capture
precedes any record-row rebuild.

**TV C14 audit (2026-08-01, committed gap-list):** 38 gaps, 4 falsified prior
claims; ~20 fix-now in flight; capture window = your exact evidence list; TV-12/
TV-13 unjudged until those frames exist.

**Markets C14-audit calls (2026-08-01, from `50e19ae`):** (a) ladder letter-spacing —
accept a documented deviation or move the element to TMP; (b) the scroll position
indicator's form for the PLAYERS tab (S25 requires one if scroll ships); (c) an
S24/S25 interaction conflict the lead wants confirmed before reversing a shipped
decision — details in the audit doc. Plus the two-column→single-column ladder
rebuild is large; DD sequencing preference welcome.

## Pending (next DD batch)

1. **T17 presentation confirm:** reserve-don't-spend means a scorer leg's backed
   side reads one goal short until the final sequence — player-visible; confirm
   intended. (Fix implemented as ruled; not blocking.)
2. **"LEAVE — NEXT ROUND" in saturated biro** (SureThing S9 audit): primary action
   in the player's ink — neither his mark nor optional. Detail in the S6–S8 bundle.
3. **Baked-only-light design read** (room R10): does a baked-only bounce source sit
   inside the direction's lighting language? Room proceeded on its interpretation.
4. **T6 visual half:** rendered captures now exist (49, held out of git pending the
   evidence-storage ruling) — attach via import when storage is settled.
5. **Review backlog:** S6/S7/S8 (bundle committed at
   `dd-import/surething-s6-s8-design-review-evidence.md`, ships after the Archivo
   capture refresh); room re-review after R10 lands.
6. **Studio constitution** — thin top layer per the approved two-tier authority;
   room's owning doc is done (R13), laptop/TV owning docs eventually consolidate
   from DESIGN.md + the design system.
7. **R12 amendment proposal (room, from R10's measurements):** "surface detail is
   gated by lighting" sharpens to — bounce fills shadow but does not reveal
   surface; only direct light at a grazing angle raises relief. As written, R12
   would have predicted the failed baked-light route works.
8. **Room re-review** — now due (R9/R10 landed per your batch-2 sequencing);
   package in `dd-import/`, includes the couch-headroom design question.
9a. **T20 live-row deviation (TV):** canon's three-line live row costs ~73px
   against a 69px slot — knife-edge glyph clipping in the real font. Built with
   no market/price/state meta line on the live row (state survives via the word,
   price via compact form), documented in the struct's doc comment. Needs your
   nod alongside item 9.
9. **TV row model: canon vs Layout B.** `TvLegRow.jsx` says live rows expand in
   place and NEED may wrap to two lines; Layout B's no-reflow law (pinned by two
   3C tests) fixes 70px slots, and a wrapped NEED needs ~98px. Orchestrator
   selected the lead's recommended interim: T20's ruled px values adopted within
   the no-reflow law, NEED capped at one line on Unity, deviation documented.
   Rule: fixed rows with one-line NEED as the TV-surface constraint, or amend the
   no-reflow law to canon's expanding rows.

10. **§8.8 stats panel — two unsourceable rows (TV):** the spec names two rows the
    sim cannot source; panel blocked. Rule: drop, re-source, or respec.
11. **§8.10 held cash-out preview — confirm gesture (TV):** preview built and
    tested (struck-and-dimmed, never extinguished, exact revert) but unbound
    pending your gesture ruling.
12. **§7.7 backed-player locator — treatment (TV):** binding half wired and
    tested; visually nothing until you rule the treatment. Concrete finding
    attached from the lead: the numeral's stated justification in §7.7 no longer
    holds. **Items 10–12 together gate the rest of Phase 3** — one batch answers
    most of what remains.
13. **Markets D-01…D-05:** five design questions from the F_0.4.0 reconciliation
    (label vocabulary, RIDING absent from the DS enum, a one-sided scorer market
    in a paired-price grammar, an S17-class overflow, one more). Full text in the
    committed gap-list in the `markets-2` worktree — Allen attaches on import.

## Cleared 2026-07-31, batch 2 (transcribed to REGISTER.md by the orchestrator)

- C3 → ruled, 3D unblocked (coverage over-enforced; add score-at-goal +
  ball-at-payoff; one-token invariant; boost 1.8).
- C8 → bloom floor: risk/pays joins the protected set.
- Layout B → **T16 ruled**, 3C commit unblocked once the tape is restored
  (scorebug foot, no numerals, no hue, ≤L2); win-prob numeral out.
- R5, R6 → **Design-verified** — the studio's first. R5's finding promoted to
  standing law R12.
- R9 → approved bounded; R10 → approved, bounce-first route.
- Room package questions → soot dropped; decals not-yet (frusta re-place first);
  the direction's read is the bar.
- Art authority → **two-tier approved by Allen** (C9); phone stays a stub.

## Cleared 2026-07-31, batch 1 (transcribed to REGISTER.md by the orchestrator)

- Form-guide identity → **S14 spec issued** (three build gaps; reference
  implementation held in DD seat, pending hand-over).
- Typeface → **S11 closed**: Archivo + Archivo Narrow, OFL 1.1.
- Lost-ticket oxide → **S15 ruled violation** (LOST struck in oxide, row
  `--toner-3`, `$0` in toner).
- Naming → **S16 closed**: LEDGER only; S9 unblocked.
- Slip-strip raw-hex → **T15 ruled violation, T8 class**; scan extended to markup.
  FYI: the markup-aware scan then found the same class in `[ST] SportsbookApp.cs` —
  routed to the SureThing lead for remediation under the T15 class ruling.

