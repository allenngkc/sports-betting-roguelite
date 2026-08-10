# The C14 audit, swept against the build — bounded inventory

**From:** SureThing UI lead · 2026-08-08 · at `5d545f3`
**Asking for:** dispositions. **Inventory only — nothing here is fixed**, per S67's shape.

**Why it exists:** S71 was **M-11 in this audit**, found again from a frame months after the audit
named it. That is the recurring-rediscovery cost S67 was opened to stop, so the same treatment
applies here — convert an open-ended document into a bounded list.

Swept: `docs/1-plans/C14-audit-market-presentation.md` §3's deviation register (M/B/L/P rows), each
checked against the current build by source, not by memory.

---

## 1. STILL LIVE — verified in source today

| # | Element | Kit | Build now |
|---|---|---|---|
| **M-04** | Stake figure | cond **26px** `--toner`, right-flushed, label separate | `"STAKE $35"` **fused, 16px, roman, left-aligned** (`SportsbookApp.cs:989-991`) |
| **M-05** | Stake block order | figure → fractions → nudges | **fractions → nudges → figure** (`:979-991`) |
| **M-14** | Quick-fraction chips | transparent + 1px `--rule`, `--toner-2` | `SurfaceRaised` fill, `White`, **no border** (`MakeChip`) |
| **M-15** | RUB OUT | transparent + 1px border | `Ink` fill, **no border** (`:932-934`) |

**M-04 and M-05 are one item in practice** — the kit's stake block leads with the figure at 26px
because the figure is the fact and the controls are how you change it; the build leads with the
controls and states the figure last, small, in the wrong voice. **Of everything in this sweep, that is
the one I would rank first**: it is the only row where the *information hierarchy* differs, rather
than the chrome.

## 2. PARTLY RESOLVED — the ruled half landed, the rest did not

| # | Landed | Still open |
|---|---|---|
| **M-12** | LOCK's 1px `--rule` edge and transparent disabled fill (**S69**); PLACE's 2px wax-deep edge (**S18**) and `--ground-3` disabled fill (**S69**) | **SKIP is still an opaque `Ink` fill with no border** (`:1144`), and the kit's **dashed→solid border on arm** exists in no state |
| **M-13** | the reason sits **inside** the LOCK button (**T47**) | the kit's **1px `--stamp` bordered box** around it is not built — the reason is bare text |

Both are worth knowing precisely because they *look* closed: S69 and T47 each resolved the half that
was ruled, and the audit row is what records the remainder.

## 3. RESOLVED — with the ruling that did it

`M-01` S34 (closed on measurement, batch 10) · `M-02` B1 rebuild + S50 · `M-03` S33's shared
`MarginRow` · `M-06` label restored, cited in source · `M-08` S33/S60 · `M-09` S50 §1 (deleted) ·
`M-11` **S71** · `B-02` S35c · `L-01`/`L-04` S38/S39/S40 · `L-03` S15-am · `L-05` S62 · `L-06` S30 ·
`L-07` S36/S41 · `L-08` S35a · `P-01` S33.

## 4. SUPERSEDED — the build was ratified over the audit

- **M-07** wax highlight z-order — **batch 17 ratified the build**: the band marks the slot, not the
  amount. The audit row is answered, against the kit.
- **L-02** *half* — the audit wants `VOIDED`; **S23 amended the DS vocabulary to `VOID`**, which the
  build emits. That half is closed by ruling.

## 5. UNRESOLVED AND UNRULED — I could not disposition these

- **L-02's other half: the ledger emits `OPEN`** (`SportsbookApp.cs:2399`), which appears in neither
  the kit's terminal list nor S23's amended enum. It is the word a non-terminal ticket gets in a list
  of *settled* records. **Either it cannot occur and is dead, or it can and the vocabulary is short a
  member.** I did not determine which; it needs the engine's settlement contract, not the surface.
- **B-01 ticket axis.** The audit says the kit stacks tickets vertically at full width; the build lays
  them out in equal horizontal columns (`BuildMirrorTicket`'s `columnWidth` split). **MY BETS has been
  ruled on repeatedly since (S58, S60, S61, and this batch's grants) without the axis being raised**,
  so it may be ratified by use — but ratified-by-silence is not a ruling, and I am not treating it as
  one.
- **M-10 locked state.** The kit *replaces* the action stack with `ROUND LOCKED` + a StampReason; the
  build keeps the buttons and inerts them, and carries the copy elsewhere (`:421`). I verified the
  copy exists and the buttons persist, but **not** whether the current arrangement is what T47/S50
  intended when they rebuilt that stack. Flagged rather than judged.

---

## 6. What this sweep cannot see (C18 §4.2)

1. **It reads the audit's rows, not the surface.** A divergence the audit never noticed is invisible
   here — this bounds a known list, it does not re-audit the build. **The audit was written against
   the pre-TMP surface**, so anything the migration changed is outside its frame entirely.
2. **Source, not frames.** Every "still live" above is a code reading. M-14 and M-15 are colour and
   border claims that a frame would settle harder, and §2's "looks closed" rows are exactly where a
   frame beats a grep.
3. **§3's tables only.** The audit's §1 (S24/S25), §2 (physically impossible) and §4 (confirmed 1:1)
   are not swept; §1 and §2 were dispositioned by batch 5 and §4 asserts no defect.
4. **I did not check whether any row was resolved and later regressed.** Section 3 trusts the ruling
   that closed each row; it does not re-verify the build against them. A row can be closed by ruling
   and broken again by a later commit, and nothing here would show it.
5. **Priority in §1 is mine, not ruled.** M-04/M-05 leading is my judgement that a hierarchy defect
   outranks chrome defects. The seat may order them differently.
