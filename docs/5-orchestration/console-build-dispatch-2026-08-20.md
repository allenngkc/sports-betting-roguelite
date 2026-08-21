# Console build — dispatch to markets-pregame (2026-08-20, Allen approved)

Allen approved `docs/design/spec-console-surfaces-2026-08-19.md` (batch 121)
as written. Build it on the `game-console` betting surface (fork A — the
presentation pass on the existing surface; no new apparatus).

## Bindings the spec does not repeat

- **K16 / batch 144 — the beat-prefix defect on five market kinds is masked
  today only because they are unbettable on the console. The moment K6's
  `{matchup}#{line}` address grammar opens all fifteen kinds, it goes live.
  The fix ships IN THE SAME COMMIT that opens the grammar, or the build ships
  the defect.**
- Evidence is self-shootable: no Unity window — pipe stdin into the console
  exe. Build with `-p:SbrUnityPluginDir=<scratch>` so the tracked
  `SBR.Engine.dll` stays clean (never commit it from this lane).
- The DD's console read + evidence live under
  `docs/design/dd-import/console-read-2026-08-19/` (untracked frames/text).
- Register section: Console (K) rows K1–K16 in `docs/design/REGISTER.md`.
- Delegation contract (STUDIO.md): bundle small items into one Sonnet
  dispatch; sustained hands-on volume with zero spawns is a recorded deviation.

## Exit

Evidence per spec section (console transcripts, not claims) → DD review →
Allen. Report Done / Next / Risk / Need Allen. Update
`docs/handoffs/markets-pregame.md`.
