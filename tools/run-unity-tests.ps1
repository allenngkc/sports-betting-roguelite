<#
.SYNOPSIS
    Runs a Unity test suite and refuses to report success for a run that executed nothing.

.DESCRIPTION
    C29 (LAW, DD 2026-08-05): "Every test invocation reports its executed case count, and a run
    with zero executed cases exits non-zero. No verdict, gate, grant or Design-verified claim rests
    on a run that did not state how many tests it ran."

    The defect this exists to stop: a mistyped `-testFilter` matches no tests, and Unity exits
    GREEN with `testcasecount="0"`. A run that did nothing, reported as a pass — and unlike the
    four vacuous gates before it, this one is the runner itself, so it can green ANY suite from ANY
    seat with one typo. That is the fifth vacuous green in a fortnight.

    Wrap every invocation in this. It always prints the executed case count, and it exits non-zero
    when that count is zero, when the results file is missing, or when any case failed.

.PARAMETER ProjectPath
    The Unity project. Defaults to <repo>/unity/SBR relative to this script, so any worktree works
    with no argument.

.PARAMETER Platform
    EditMode or PlayMode.

.PARAMETER TestFilter
    Optional NUnit filter. THIS IS THE DANGEROUS ARGUMENT — it is what silently matches nothing.
    When one is supplied and nothing matches, the failure message names the filter.

.EXAMPLE
    ./tools/run-unity-tests.ps1 -Platform PlayMode
    ./tools/run-unity-tests.ps1 -Platform EditMode -TestFilter SBR.Tests.EditMode.SureThingNameTests

.NOTES
    Deliberately does NOT pass -nographics. It fails the four SureThing capture tests on
    `RenderTexture.Create`, which reads as four regressions and is not one.

    Exit codes: 0 pass · 1 test failures · 2 no results (compile error / crash / timeout)
                3 ZERO EXECUTED CASES (C29)
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidateSet('EditMode', 'PlayMode')][string]$Platform,
    [string]$ProjectPath,
    [string]$ResultsPath,
    [string]$LogPath,
    [string]$TestFilter,
    [string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.5.3f1\Editor\Unity.exe',
    [int]$TimeoutSeconds = 900
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path $PSScriptRoot -Parent
if (-not $ProjectPath) { $ProjectPath = Join-Path $repoRoot 'unity\SBR' }
$stamp = (Get-Date -Format 'yyyyMMdd-HHmmss')
if (-not $ResultsPath) { $ResultsPath = Join-Path $repoRoot "evidence\test-results\$Platform-$stamp.xml" }
if (-not $LogPath) { $LogPath = Join-Path $repoRoot "evidence\logs\$Platform-$stamp.log" }

foreach ($dir in @((Split-Path $ResultsPath -Parent), (Split-Path $LogPath -Parent))) {
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Force $dir | Out-Null }
}
if (-not (Test-Path $UnityPath)) { Write-Host "FAIL: Unity not found at $UnityPath" -ForegroundColor Red; exit 2 }
# A stale results file from a previous run would be read as this run's evidence if Unity dies
# before writing one. That is the same class of lie C29 is about, so it goes first.
if (Test-Path $ResultsPath) { Remove-Item $ResultsPath -Force }

$unityArgs = @(
    '-batchmode', '-runTests',
    '-projectPath', $ProjectPath,
    '-testPlatform', $Platform,
    '-testResults', $ResultsPath,
    '-logFile', $LogPath
)
if ($TestFilter) { $unityArgs += @('-testFilter', $TestFilter) }

Write-Host "[$Platform] running$(if ($TestFilter) { " (filter: $TestFilter)" })..."
$proc = Start-Process -FilePath $UnityPath -PassThru -ArgumentList $unityArgs
try { Wait-Process -Id $proc.Id -Timeout $TimeoutSeconds -ErrorAction Stop }
catch {
    Write-Host "[$Platform] TIMED OUT after ${TimeoutSeconds}s — executed case count UNKNOWN" -ForegroundColor Red
    exit 2
}

if (-not (Test-Path $ResultsPath)) {
    Write-Host "[$Platform] NO RESULTS FILE — executed case count UNKNOWN (0 assumed)" -ForegroundColor Red
    if (Test-Path $LogPath) {
        Select-String -Path $LogPath -Pattern 'error CS' | Select-Object -First 12 |
            ForEach-Object { Write-Host ("  " + $_.Line.Trim()) }
    }
    exit 2
}

[xml]$xml = Get-Content $ResultsPath
$run = $xml.'test-run'
$discovered = [int]$run.testcasecount
$executed = [int]$run.total
$passed = [int]$run.passed
$failed = [int]$run.failed
$skipped = [int]$run.skipped

# C29: the count is reported on every path, pass or fail, before any verdict is drawn from it.
Write-Host "[$Platform] executed $executed of $discovered discovered · passed $passed · failed $failed · skipped $skipped · $($run.result)"

if ($discovered -eq 0 -or $executed -eq 0) {
    Write-Host ""
    Write-Host "C29 VIOLATION: this run executed ZERO test cases and Unity reported it green." -ForegroundColor Red
    if ($TestFilter) {
        Write-Host "  The filter matched nothing: '$TestFilter'" -ForegroundColor Red
        Write-Host "  Check the fully-qualified name, including namespace." -ForegroundColor Red
    } else {
        Write-Host "  No filter was supplied, so the suite itself came back empty — check assembly compilation." -ForegroundColor Red
    }
    Write-Host "  No verdict, gate, grant or Design-verified claim may rest on this run." -ForegroundColor Red
    exit 3
}

if ($failed -gt 0) {
    Write-Host ""
    $xml.SelectNodes("//test-case[@result='Failed']") | ForEach-Object {
        Write-Host ("FAIL: " + $_.fullname) -ForegroundColor Red
        if ($_.failure.message.InnerText) { Write-Host ("  " + $_.failure.message.InnerText.Trim()) }
    }
    exit 1
}

Write-Host "[$Platform] PASS" -ForegroundColor Green
exit 0
