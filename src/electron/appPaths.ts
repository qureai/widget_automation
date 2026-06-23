import * as path from "path";
import * as fs from "fs";
import * as dotenv from "dotenv";
import { execSync } from "child_process";

// Load .env from the project root (widget_automation/) regardless of CWD
dotenv.config({ path: path.resolve(__dirname, "../../.env") });

/**
 * Query the Windows Uninstall registry (both HKLM and HKCU, both 64-bit and 32-bit)
 * for any entry whose DisplayName matches the given pattern, and return the exe path.
 *
 * MSI installers always write their install location to the registry, so this works
 * regardless of whether the installer puts files in Program Files or LocalAppData.
 */
function findExeFromRegistry(displayNamePattern: string): string | null {
  const script = [
    `$pattern = '${displayNamePattern}'`,
    `$keys = @(`,
    `  'HKLM:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*',`,
    `  'HKLM:\\Software\\WOW6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*',`,
    `  'HKCU:\\Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\*'`,
    `)`,
    `$entry = $keys | ForEach-Object { Get-ItemProperty $_ -ErrorAction SilentlyContinue } |`,
    `  Where-Object { $_.DisplayName -like $pattern } | Select-Object -First 1`,
    `if (-not $entry) { exit 1 }`,
    // InstallLocation may or may not include trailing slash; DisplayIcon often points straight to the exe
    `$loc = $entry.InstallLocation`,
    `$icon = $entry.DisplayIcon -replace ',\\d+$',''`,  // strip icon index suffix like ",0"
    `if ($icon -and (Test-Path $icon)) { Write-Output $icon; exit 0 }`,
    `if ($loc) {`,
    `  $exe = Join-Path $loc '${displayNamePattern.replace(/\*/g, "").trim()}.exe'`,
    `  if (Test-Path $exe) { Write-Output $exe; exit 0 }`,
    `}`,
    `exit 1`,
  ].join("\n");

  try {
    const result = execSync(`powershell -NonInteractive -Command "${script.replace(/"/g, '\\"')}"`, {
      encoding: "utf8",
      timeout: 10_000,
    }).trim();
    if (result && fs.existsSync(result)) return result;
  } catch {
    // registry lookup failed or exe not found — fall through to filesystem candidates
  }
  return null;
}

export function isInstalled(): boolean {
  try {
    const p = getExePath();
    return fs.existsSync(p);
  } catch {
    return false;
  }
}

export function getExePath(): string {
  const fromEnv = process.env.APP_EXE_PATH?.trim();

  // If APP_EXE_PATH is set AND the file exists, use it directly.
  // If set but missing, ignore it and auto-detect — prevents a stale .env from
  // causing "exe missing after install" failures when the MSI installs to a
  // different directory (e.g. Program Files vs LocalAppData).
  if (fromEnv && fs.existsSync(fromEnv)) return path.resolve(fromEnv);

  // Try registry first — most reliable since MSI always writes install location there.
  const fromRegistry = findExeFromRegistry("Qure Widget");
  if (fromRegistry) return fromRegistry;

  // Filesystem fallback: known install locations for this MSI (system-wide installer).
  const candidates = [
    "C:\\Program Files\\Qure Widget\\Qure Widget.exe",
    path.join(process.env["ProgramFiles"] ?? "C:\\Program Files", "Qure Widget", "Qure Widget.exe"),
    path.join(process.env["ProgramW6432"] ?? "C:\\Program Files", "Qure Widget", "Qure Widget.exe"),
    path.join(process.env["LOCALAPPDATA"] ?? "", "Programs", "Qure Widget", "Qure Widget.exe"),
  ];

  for (const candidate of candidates) {
    if (fs.existsSync(candidate)) return candidate;
  }

  throw new Error(
    "Could not locate Qure Widget executable.\n" +
    "Set APP_EXE_PATH in .env to the full path of Qure Widget.exe, then run again."
  );
}

export function getMsiPath(): string {
  // __dirname is src/electron/ — two levels up is the project root (widget_automation/)
  const projectRoot = path.resolve(__dirname, "../../");
  const fromEnv = process.env.APP_MSI_PATH;
  if (fromEnv && fromEnv.trim()) return path.resolve(projectRoot, fromEnv.trim());
  return path.join(projectRoot, "app", "QureWidget.msi");
}
