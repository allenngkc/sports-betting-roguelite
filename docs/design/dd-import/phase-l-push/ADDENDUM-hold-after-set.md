# ADDENDUM (2026-08-08, orchestrator) — REVISED: the corrected set has arrived

**Verify on `final/`.** It supersedes `after/` entirely (that set was shot at
SemiBold roman before Allen's Regular-400 ruling; keep it only as
migration-fidelity evidence). Since `after/` was shot, on Allen's rulings:

- **S20 closed** — roman is Regular 400, confirmed by prediction: every roman
  region dropped 3.6–4.6pp of ink, the condensed control moved 0.25pp, the
  rail identity 0.00pp (it is now 600 by choice, the only deliberate weight).
- **S29 closed** — masthead run figures moved to Condensed per §4.1's own
  assignment; the TMP Regular face has no tnum, so roman figures would jitter
  width as the bank changes. Note: the old "spread 0" was measured against
  the wrong face (the accidental SemiBold is near-tabular at 0.1875; true
  Regular is proportional at 4.7656).
- **S52 held** through type-stack, weight and face changes (rail band and
  tray byte-zero diffs).

Known limits carried honestly: the digit jitter is measured-not-photographed
(no capture state varies the bank); the 13px fact floor is still
source-checked, not frame-checked.

Original hold note follows for the record.

---

Read before reviewing `after/`.

Since the pair was pushed, Allen ruled the roman voice is **Regular 400** (S20
family) and the weight-600 tier is being wired. The `after/` set was shot at
SemiBold roman — it documents **migration fidelity** (the 1:1 TMP swap:
pixel-identical chrome, held brightness laws) but no longer represents the
build's type voices.

- **Valid now:** `before/` (unchanged), and `after/` as migration-fidelity
  evidence only.
- **Hold:** item-by-item re-verification and deviation expiry until the
  corrected after-set arrives — same pinned flows, shot at Regular 400 with
  tnum and the wired 600 tier. It follows within the day.

Also queued for you in that push: three kit inconsistencies the lead left
untracked rather than inventing tokens (LedgerEntry.jsx `.02em`,
OsRail.jsx:17 `.13em` on the identity mark, and the two untracked slots named
in the close report).
