# Plan: M5 — the phone is the bookie (bookie text thread)

_Locked via grill — by Claude (Fable, game director) + Allen. 2026-07-12. Rev 4 after Codex round 4._

## Goal

Make debt read as a CHARACTER, not a number. The desk phone becomes the bookie's voice: a
text-message thread from a person — warm degen-buddy early, colder as the favors stack — that
lands the debt lifecycle as narrative beats in real time (the phone buzzes behind you during the
TV settle card). Pure presentation: zero engine changes, zero engine RNG consumed. Prototype
fidelity throughout (code-built UI, no assets, visual-only buzz — audio comes with the sound pass).

## Approach

1. **`BookieScript`** (pure static, `SBR.Game`): the writing. Each trigger KIND has its own line
   pool; subtypes are distinct kinds (see 2), so the deterministic pick key is complete:
   variant = FNV-1a StableHash of `"{Run.Rng.RunSeed}#{round}#{kind}"` (seed via the EXISTING
   `Run.Rng.RunSeed` — no new director API; `round` per the stamping rule in 2). Tone: deadpan
   dark comedy per design/00; warm → cold.
2. **`BookieFeedModel`** (pure class, `SBR.Game`): the trigger state machine over director
   snapshots `(runGeneration, run, phase, round, debt, lastSettle)`. Contract (Codex-hardened):
   - **Trigger kinds** (subtype = its own kind): `RUN_START`, `FLOAT_WARM` (1st float of the run),
     `FLOAT_COLD` (2nd+), `DEBT_BETTING`, `NO_MORE_FAVORS` (debt betting on the final round —
     replaces `DEBT_BETTING`), `CLEARED`, `COLLECTION` (RunLost with debt — is the loss text),
     `VERDICT_WON`, `VERDICT_BUST` (clean RunLost).
   - **Dedup keys**: settle-driven triggers fire once per `(runGeneration, lastSettle.Round)`;
     betting-driven triggers once per `(runGeneration, run.Round)`. Repeated identical snapshots
     are no-ops (tested).
   - **Round stamping**: settle-driven messages stamp and hash with `lastSettle.Round` (immune to
     `ExitShop` bumping `Run.Round` before a delayed observation); betting-driven use `run.Round`.
   - **Ordered multi-emission** in one snapshot: `CLEARED` before `VERDICT_WON` (final-round
     debt-clear win emits both, in that order, each exactly once). `COLLECTION` and
     `VERDICT_BUST`/`VERDICT_WON` are mutually exclusive by definition.
   - **First snapshot rule**: the first non-null-Run snapshot IS a run start (no sentinel
     comparison), so wiring order can't eat the welcome. Null-Run snapshots are ignored.
   - **Atomic per-run reset** on generation change: messages, floatCount, unread, all dedup keys
     reset together BEFORE the new welcome appends. **`Revision` is monotone across runs, never
     reset** — it increments on append, reset, and read-state change; renderers key on it.
     **`ArrivalSequence` is a second monotone counter incremented ONLY on message append** — the
     buzz keys on it (so reads/resets never buzz, and an equal-count new-run welcome still does).
   - **Snapshot processing order** (one snapshot can carry several transitions after a delayed
     observation): run reset first, then unseen settle triggers, then the unseen betting trigger —
     so FLOAT always precedes DEBT_BETTING when both surface at once (tested).
3. **`BookieFeed`** (thin MonoBehaviour): snapshots the director each frame, feeds the model,
   exposes `Messages`, `UnreadCount`, `Revision`, **and `ArrivalSequence`** (PhoneScreen keys the
   emission pulse AND the light blink on ArrivalSequence; rendering keys on Revision). Holds a
   serialized reference to THE PHONE'S `DeskFocus` instance; unread clears only while
   `DeskFocus.Active == phoneFocus` (laptop focus must not mark the thread read).
4. **`PhoneScreen`** (MonoBehaviour, mirrors LaptopScreen's construction): world-space canvas on
   the face-up phone quad (canvas +Z pointing INTO the phone), thread bottom-anchored (bookie-grey
   bubbles, ROUND-n stamps from the message's stamped round), unread badge, buzz on arrival
   (screen emission pulse + small point-light blink; glows while unread). Renders when
   `feed.Revision` changes — not message count (count can be unchanged across a run reset).
5. **`DeskFocus` upgrades** (Codex rounds 2–3): (a) serialized `prompt` field (default
   "Use laptop"; the phone sets "Check phone") replacing the hard-coded string; (b) **focus
   ownership is claimed BEFORE the glide starts** and released only when focus-out completes —
   the claim rejects `OnInteract` ONLY on Idle-entry by a non-owning instance; the owning
   instance's own Back path is always permitted; (c) **`OnDisable` unwinds before releasing**:
   stop glides, restore cursor + controller (camera snapped home, `SetCursorFree(false)`,
   `ExitSeated` resync), then clear the claim — a mid-transition disable can't strand the camera.
6. **GrayboxRoomBuilder**: `BuildPhone` replaces the ScreenStub with DeskFocus #2 (anchor ~0.30 m
   above the phone looking straight down, up = the quad's screen-up so text reads from the
   player's side; `focusFov` ≈ 30°) + `PhoneScreen` + `BookieFeed` wiring (director + phone
   focus). `ScreenStub.cs` DELETED (no users remain). EventSystem already exists (M4).
7. **Tests.** EditMode (`BookieFeedModelTests`, synthetic `SettleReport`s — public ctor):
   - every trigger path (welcome; float warm; float→clear; float→collect; debt-betting;
     no-more-favors; verdicts), asserting ordered messages, counts, rounds, tiers;
   - **repeated-snapshot idempotence** (every snapshot fed 3×, no double-fire);
   - **Shop→Betting persistence** (LastSettle unchanged across ExitShop must not re-fire; delayed
     observation after ExitShop stamps the settled round, not the new one);
   - **final-round clear+win** emits CLEARED then VERDICT_WON exactly once each;
   - **atomic reset**: warm float tier restored after a new run; dedup keys don't leak; Revision
     strictly increases across the reset;
   - **variant determinism**: same (seed, round, kind) ⇒ same line, across model instances;
   - **ArrivalSequence semantics**: every append increments it; read-state changes and the reset
     alone do NOT; a reset followed by the equal-count welcome increments it exactly once;
   - **RNG purity, the strong form**: two same-seed engine runs; one is observed through a full
     feed lifecycle; then BOTH are driven through LockRound/outcomes/shop generation and every
     engine output compared equal (the existing purity-test pattern, extended).
   PlayMode: RoomSmokeTests → 3 Interactables (1 SitSpot, 2 DeskFocus, 0 ScreenStub), PhoneScreen +
   BookieFeed present, welcome rendered after load; NEW: phone-vs-laptop focus identity (laptop
   focus does NOT clear unread, phone focus does), prompt text per instance, and the ownership
   race windows Codex named: a second instance's interact is rejected DURING focus-in and DURING
   focus-out (not just while engaged), and disabling a focus mid-glide restores camera/cursor.
   **Adapter integration (Codex r3 #6, flow fixed per r4 #3)**: `RunDirector.StartNewRun(string)`
   becomes public WITH the same normalization as first-run entry (trim; null/whitespace ⇒ fresh
   random seed). The test starts a pinned seed whose scripted round-1 bet deterministically loses
   (found at implementation time; engine determinism guarantees it forever), locks via the
   director, **drains each `Run.Sweats` session with `MoveNext` (leaving Phase == Sweat — NOT
   `FastForwardRound`, which finishes internally and would make `FinishAndSettle` early-return)**,
   then calls `director.FinishAndSettle()` once — and asserts the REAL BookieFeed adapter emitted
   FLOAT_WARM with the correct round stamp and DebtAfter amount. One flow, not a model re-suite.
8. **Docs + protocol**: DECISIONS.md M5 entry (incl. the one-round-loan correction), PLAYTESTS
   gate note, README status. Scene rebuilt headless; both suites green; commit.

## Key decisions & tradeoffs (from the grill + review)

- **Phone = the bookie's VOICE, not an info hub.** Game-state dashboards stay on laptop/TV. The
  FLOAT text DOES name the dollar amount — a bookie telling you what you're into him for is
  voice, not a dashboard — and that amount is pinned to `SettleReport.DebtAfter` (what you owe:
  principal + juice), invariant-formatted like every money string.
- **Person, not app persona.** Warm → cold degen-buddy arc, deadpan (design/00). VIP-host satire
  parked.
- **Reuse DeskFocus top-down; pick-up-into-hand deferred to the juice pass.** The two DeskFocus
  hardenings (prompt field, ownership claim) are M5 scope.
- **All seven beats, 2–3 variants per kind** (~20 lines), deterministic per (seed, round, kind).
  Immediate delivery — the buzz lands during the TV settle card.
- **Visual-only buzz**; first audio arrives with the sound pass.
- **CORRECTION from planning:** "escalating reminders each settle in debt" is impossible — debt
  is a one-round loan (Settle clears it or ends the run; Run.cs). Escalation ships as:
  FLOAT_COLD on repeat floats, DEBT_BETTING inside the indebted round, NO_MORE_FAVORS on the
  final round.

## Risks / open questions

- **Readability of the flat phone at 0.065×0.135 m**: mitigated by the tight-FOV top-down focus;
  font sizing needs Allen's eyes at review (same risk class as the laptop, which passed).
- **Buzz salience vs. settle card**: the pulse must register without stealing the TV beat —
  intensity/duration are dials.
- **Line variants are first-draft writing** — all in `BookieScript`, punch-ups trivial at review.

## Out of scope

- Audio (sound pass), pick-up-into-hand gesture (juice pass), player replies/choices, VIP-host
  angle (parked), any engine change, non-bookie phone content, meta-progression hooks.
