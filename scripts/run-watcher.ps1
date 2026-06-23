# ============================================================================
#  run-watcher.ps1  —  Launches the UIAccess AutoHotkey UAC watcher.
#
#  Invoked by startUacWatcher() in launcher.ts as:
#     powershell -NoProfile -ExecutionPolicy Bypass -File scripts\run-watcher.ps1
#
#  Using a dedicated -File script (instead of an inline -Command string) avoids
#  Windows/Node argument-quoting problems: the watcher's nested quotes never
#  reach Node's command-line builder.
#
#  A UIAccess exe can ONLY be launched via ShellExecute (Start-Process), which
#  routes through the AppInfo service that grants the UIAccess token. Plain
#  CreateProcess (Node spawn / Task Scheduler) fails with ERROR_ELEVATION_REQUIRED.
# ============================================================================

$ErrorActionPreference = 'Stop'

$uia     = 'C:\Program Files\AutoHotkey\v2\AutoHotkey64_UIA.exe'
$watcher = Join-Path $PSScriptRoot 'uac-autoyes.ahk'

if (-not (Test-Path $uia))     { Write-Error "UIAccess AutoHotkey not found: $uia"; exit 2 }
if (-not (Test-Path $watcher)) { Write-Error "Watcher script not found: $watcher"; exit 3 }

Start-Process -FilePath $uia -ArgumentList ('"' + $watcher + '"')
