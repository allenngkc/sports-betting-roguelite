# SBR Studio Keeper — dumb watchdog, no model, no decisions.
# Runs every 15 min via Task Scheduler ("SBR Studio Keeper").
# One check: STATUS.md heartbeat age. Stale > 45 min -> poke the orchestrator
# seat; seat missing/dead -> boot a fresh one per ORCHESTRATOR.md §3/§6c.

$orca    = "C:\Users\Allen\AppData\Local\Programs\orca\resources\bin\orca.exe"
$mainWt  = "C:\Users\Allen\orca\workspaces\sports-betting-roguelite\main-2"
$status  = Join-Path $mainWt "docs\5-orchestration\STATUS.md"
$log     = Join-Path $PSScriptRoot "keeper.log"

function Log($m) { Add-Content -Path $log -Value "$(Get-Date -Format s) $m" }

if (-not (Test-Path $status)) { Log "STATUS.md missing"; exit 0 }
$age = ((Get-Date) - (Get-Item $status).LastWriteTime).TotalMinutes
if ($age -lt 45) { exit 0 }  # heartbeat healthy — do nothing, log nothing

try { $json = (& $orca terminal list --json 2>$null) -join "`n" | ConvertFrom-Json }
catch { Log "board stale $([int]$age)m but orca CLI unreachable"; exit 0 }
if (-not $json.ok) { Log "board stale $([int]$age)m; orca returned not-ok"; exit 0 }

$terms = @($json.result.terminals | Where-Object {
    $_.connected -and $_.worktreePath -like "*sports-betting-roguelite/main-2" })
$seat = $terms | Where-Object { $_.title -match 'orchestrat|sweep|studio' } |
        Select-Object -First 1

$poke = "keeper heartbeat: STATUS.md is $([int]$age) minutes stale. If any text precedes this line, it is Allen's UNSENT draft - do not act on it without his confirm (ORCHESTRATOR.md 6c). Run one section-6 cycle now: re-arm monitors, 6a audit, dispatch, stamp the board."

if ($seat) {
    $idle = 999
    if ($seat.lastOutputAt) {
        $idle = ((Get-Date) - ([DateTimeOffset]::FromUnixTimeMilliseconds($seat.lastOutputAt).LocalDateTime)).TotalMinutes
    }
    if ($idle -lt 20) { Log "board stale $([int]$age)m but seat active $([int]$idle)m ago (long turn?) - no poke"; exit 0 }
    & $orca terminal send --terminal $seat.handle --enter --text $poke | Out-Null
    Log "poked $($seat.handle) - board $([int]$age)m stale, seat idle $([int]$idle)m"
    exit 0
}

# No seat found — reseat, with a 2-hour cooldown so a broken boot can't loop.
if (Test-Path $log) {
    $recent = Get-Content $log -Tail 40 | Where-Object { $_ -match "reseated:" } | Select-Object -Last 1
    if ($recent) {
        $ts = [datetime]::Parse(($recent -split ' ')[0])
        if (((Get-Date) - $ts).TotalMinutes -lt 120) { Log "seat missing but reseat cooldown active"; exit 0 }
    }
}
$created = (& $orca terminal create --worktree "path:$mainWt" --command "claude --model fable --dangerously-skip-permissions") -join " "
if ($created -match 'term_[0-9a-f-]+') {
    $handle = $Matches[0]
    Start-Sleep -Seconds 15
    & $orca terminal send --terminal $handle --enter --text "keeper: the previous orchestrator seat is dead (STATUS.md stale). You are the Studio Orchestrator now. Read docs/5-orchestration/STUDIO.md and ORCHESTRATOR.md; verify via 'orca terminal list' that no other orchestrator seat is active (exit the role if one is); then run one section-6 cycle and stamp STATUS.md." | Out-Null
    Log "reseated: created $handle (board was $([int]$age)m stale)"
} else {
    Log "reseat failed: create returned '$created'"
}
