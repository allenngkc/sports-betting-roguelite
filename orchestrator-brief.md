# Orchestrator brief — 2026-07-31

**From:** Studio Orchestrator (`main-2`). Supplements your `handoff.md`; does not
replace it. Board: `main-2/docs/5-orchestration/STATUS.md`.

## Ruling

- Your uncommitted `ProjectSettings/*` (EditorBuildSettings, ProjectSettings,
  ShaderGraphSettings) and URP global settings changes: **approved by Allen,
  2026-07-31**, as deliberate. Keep them. Call them out explicitly in your
  integration handoff and the commit that carries them.

## Directives

1. Commit `handoff.md` — still untracked at your root.
2. Clean the stray test XML/log files at `unity/SBR/` root (`diag-run.xml`,
   `diag2-run.xml`, `final-*.xml`, `fix-*.xml`, `*.log`, dated capture XMLs) before
   your next commit. Keep evidence in a dedicated evidence folder, not the repo root.
3. Continue implementing approved specs in register order: S7 ink sprites (partial —
   `SureThingInkImporter.cs` is untracked, commit it with its meta), S8 OS chrome,
   S9 event detail / staged ticket / MY BETS / rewards / old slips.
4. Spec gaps or ambiguity → Design Director, not Allen. Critical/strategy →
   orchestrator.
5. Announce Unity batch/test runs in your updates — one editor instance studio-wide;
   the orchestrator sequences runs.

## Watch items

- S11: Bell Centennial licence is unresolved — do not bake the typeface into
  production assets until it clears.
- S10 (laptop "loud register" sweat) is Candidate only — no built spec; do not
  implement ahead of the Design Director.

Report result-first, telegraphic, ending `Done / Next / Risk / Need Allen`.
