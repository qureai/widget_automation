# ============================================================================
#  install-uac-task.ps1  —  One-time setup for the AHK UAC auto-Yes watcher.
#
#  Run ONCE from an ELEVATED PowerShell (Run as administrator). That admin
#  terminal is the only approval needed — there is NO separate UAC dialogue
#  for the steps below, because they inherit the terminal's elevation.
#
#     powershell -ExecutionPolicy Bypass -File scripts\install-uac-task.ps1
#
#  What it does:
#    1. Enables UIAccess on AutoHotkey64_UIA.exe (signs it + sets the uiAccess
#       manifest flag) so the watcher can drive the System-integrity
#       consent.exe window — without UIAccess, UIPI blocks every click.
#    2. Removes any stale "QureWidget-UacAutoYes" scheduled task. (We do NOT
#       use a task: a UIAccess exe can't be launched by Task Scheduler — it
#       fails with ERROR_ELEVATION_REQUIRED. The framework launches the watcher
#       via ShellExecute instead; see startUacWatcher() in launcher.ts.)
#
#  Prerequisite you set manually: secure desktop disabled, so the dialog is on
#  the normal desktop where the watcher can reach it:
#    Set-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System" `
#      -Name PromptOnSecureDesktop -Value 0
#
#  After this, the install UAC dialogue still appears at install time and the
#  watcher clicks "Yes" automatically — no per-run manual clicks.
# ============================================================================

$ErrorActionPreference = "Stop"

# Confirm we're elevated (everything below needs admin).
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    throw "Run this from an ELEVATED PowerShell (Run as administrator)."
}

$ahkDir   = "C:\Program Files\AutoHotkey\v2"
$ahkExe   = Join-Path $ahkDir "AutoHotkey64.exe"        # normal build, runs the enabler
$ahkUia   = Join-Path $ahkDir "AutoHotkey64_UIA.exe"    # UIAccess build, runs the watcher
foreach ($p in @($ahkExe, $ahkUia)) {
    if (-not (Test-Path $p)) { throw "AutoHotkey v2 not found at $p. Install AutoHotkey v2 first." }
}

# ── Step 1: enable UIAccess on the UIA exe ──────────────────────────────────
$enabler = Join-Path $PSScriptRoot "enable-uia.ahk"
if (-not (Test-Path $enabler)) { throw "Cannot find $enabler" }

$resultFile = Join-Path $env:TEMP "enable-uia-result.txt"
Remove-Item $resultFile -ErrorAction SilentlyContinue

Write-Host "Enabling UIAccess on $ahkUia ..."
Start-Process $ahkExe -ArgumentList "`"$enabler`"" -Wait
Start-Sleep -Milliseconds 500

if (Test-Path $resultFile) {
    $msg = (Get-Content $resultFile -Raw).Trim()
    Write-Host "  $msg"
    if ($msg -notmatch '^OK') { throw "UIAccess enable did not report success. See message above." }
} else {
    throw "UIAccess enabler produced no result file ($resultFile)."
}

# ── Step 2: remove any stale scheduled task (we launch via ShellExecute) ─────
$scriptPath = Join-Path $PSScriptRoot "uac-autoyes.ahk"
if (-not (Test-Path $scriptPath)) { throw "Cannot find $scriptPath" }

$taskName = "QureWidget-UacAutoYes"
try {
    schtasks /delete /tn $taskName /f *> $null
    Write-Host "Removed stale scheduled task '$taskName' (no longer used)."
} catch {}

Write-Host ""
Write-Host "Setup complete."
Write-Host "  UIAccess exe : $ahkUia  (signed, uiAccess=true)"
Write-Host "  Watcher script: $scriptPath"
Write-Host ""
Write-Host "The framework launches the watcher automatically during install via"
Write-Host "ShellExecute (startUacWatcher in launcher.ts). No scheduled task needed."
