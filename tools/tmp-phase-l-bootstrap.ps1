<#
.SYNOPSIS
    Phase L's editor window, as one command. Imports TMP essential resources, generates the two
    font assets, and verifies both landed — then hands off to the test wrapper.

.DESCRIPTION
    The editor is one instance studio-wide and this window is queued behind another seat's. Every
    step here is scripted so the window is a command rather than a click-through: it can be re-run,
    reviewed, handed to someone else, and it leaves a log.

    Three Unity invocations, deliberately, not one:

      1. Import essentials  — AssetDatabase.ImportPackage completes across a domain reload, so a
                              check inside the same invocation reads the pre-import state.
      2. Generate assets    — needs the essentials present (TMP_Settings resolves the shaders the
                              generated assets reference).
      3. Verify             — settings present, both faces present, and the two faces are not the
                              same object. Exits non-zero otherwise.

    Step 3 is the point. An import that silently did nothing and a successful one look identical in
    a batchmode log tail, which is C29's shape one layer out from the test runner: a step that did
    nothing, reported as done. Nothing downstream is built on an unverified bootstrap.

    Run the suites afterwards with tools/run-unity-tests.ps1 (C29). This script does NOT run them —
    it deliberately does one job and states whether it worked.

.EXAMPLE
    ./tools/tmp-phase-l-bootstrap.ps1
    ./tools/run-unity-tests.ps1 -Platform EditMode
    ./tools/run-unity-tests.ps1 -Platform PlayMode

.NOTES
    Exit codes: 0 all three steps succeeded · 2 a step failed or Unity died · 3 verification failed
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
    @{ Name = 'import-essentials'; Method = 'SBR.EditorTools.SureThingTmpBootstrap.ImportEssentials' },
    @{ Name = 'generate-assets';   Method = 'SBR.EditorTools.SureThingTmpFontAssets.Generate' },
    @{ Name = 'verify';            Method = 'SBR.EditorTools.SureThingTmpBootstrap.Verify' }
)

foreach ($step in $steps) {
    $log = Join-Path $logDir "tmp-$($step.Name)-$stamp.log"
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
    # Echo what the step actually said, so the outcome is read from the step and not inferred from
    # the exit code alone.
    if (Test-Path $log) {
        Select-String -Path $log -Pattern '\[SureThingTmp' | ForEach-Object { Write-Host ("  " + $_.Line.Trim()) }
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
Write-Host "TMP bootstrap complete: essentials imported, two font assets generated and verified." -ForegroundColor Green
Write-Host "Next, and not run by this script (C29): ./tools/run-unity-tests.ps1 -Platform EditMode, then PlayMode."
exit 0
