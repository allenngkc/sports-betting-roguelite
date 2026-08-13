<#
.SYNOPSIS
    Phase T's step T-1b, as one command: generate the TV's TMP font assets, then verify the faces
    are the named instances they claim.

.DESCRIPTION
    Same shape and the same reasoning as tools/tmp-phase-l-bootstrap.ps1 — the editor is one
    instance studio-wide, so a window is a command rather than a click-through: re-runnable,
    reviewable, and it leaves a log.

    Two invocations, not one. Generation and verification are separate processes so the verify
    reads the assets back from DISK across a domain reload, rather than reading the objects
    generation just left in memory. Verifying in-process would prove the copy succeeded, not that
    the asset persisted — which is exactly the §1 restore rule this lane already paid for once:
    *verify by USING the artefact, never by hashing it against what you just wrote.*

    TMP essential resources are NOT imported here. Phase L already imported them and
    `Assets/TextMesh Pro/Resources/TMP Settings.asset` is committed; if that ever stops being true
    the generate step fails inside TMP on a null TMP_Settings, which is loud.

.EXAMPLE
    ./tools/tv-phase-t-bootstrap.ps1
    ./tools/run-unity-tests.ps1 -Platform EditMode
    ./tools/run-unity-tests.ps1 -Platform PlayMode

.NOTES
    Exit codes: 0 both steps succeeded · 2 a step failed or Unity died · 3 verification failed
#>
[CmdletBinding()]
param(
    [string]$ProjectPath,
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.3f1\Editor\Unity.exe',
    [int]$TimeoutSeconds = 900
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
if (-not $ProjectPath) { $ProjectPath = Join-Path $repoRoot 'unity\SBR' }
$stamp = (Get-Date -Format 'yyyyMMdd-HHmmss')
$logDir = Join-Path $repoRoot 'evidence\logs'
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Force $logDir | Out-Null }

if (-not (Test-Path $UnityPath)) {
    Write-Host "FAIL: Unity not found at $UnityPath" -ForegroundColor Red; exit 2
}

$steps = @(
    @{ Name = 'generate'; Method = 'SBR.EditorTools.TvTmpFontAssets.Generate' },
    @{ Name = 'verify';   Method = 'SBR.EditorTools.TvTmpFontAssets.Verify' }
)

foreach ($step in $steps) {
    $log = Join-Path $logDir "tv-tmp-$($step.Name)-$stamp.log"
    Write-Host "[$($step.Name)] running..."
    $unityArgs = @(
        '-batchmode', '-quit',
        '-projectPath', $ProjectPath,
        '-executeMethod', $step.Method,
        '-logFile', $log
    )
    $proc = Start-Process -FilePath $UnityPath -PassThru -ArgumentList $unityArgs
    try { Wait-Process -Id $proc.Id -Timeout $TimeoutSeconds -ErrorAction Stop }
    catch {
        Write-Host "[$($step.Name)] TIMED OUT after ${TimeoutSeconds}s — outcome UNKNOWN" -ForegroundColor Red
        exit 2
    }

    $code = $proc.ExitCode
    # Read the outcome from what the step SAID, not from the exit code alone.
    if (Test-Path $log) {
        Select-String -Path $log -Pattern '\[TvTmpFontAssets' | ForEach-Object { Write-Host ("  " + $_.Line.Trim()) }
        $errors = Select-String -Path $log -Pattern 'error CS|Exception:' | Select-Object -First 8
        if ($errors) { $errors | ForEach-Object { Write-Host ("  " + $_.Line.Trim()) -ForegroundColor Red } }
    } else {
        Write-Host "  (no log written — Unity died before opening it)" -ForegroundColor Red
    }

    if ($code -ne 0) {
        Write-Host "[$($step.Name)] FAILED (exit $code) — log: $log" -ForegroundColor Red
        exit $(if ($code -eq 3) { 3 } else { 2 })
    }
    Write-Host "[$($step.Name)] ok" -ForegroundColor Green
}

Write-Host ""
Write-Host "T-1b complete: four TV font assets generated and verified against their named instances." -ForegroundColor Green
Write-Host "Next, and not run by this script (C29): ./tools/run-unity-tests.ps1 -Platform EditMode, then PlayMode."
exit 0
