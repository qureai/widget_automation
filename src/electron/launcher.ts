import { ElectronApplication, Page, Browser, _electron as electron, chromium } from "playwright";
import { execSync, spawnSync, spawn, ChildProcess } from "child_process";
import * as fs from "fs";
import * as path from "path";
import { getExePath, getMsiPath, isInstalled } from "./appPaths";

/** Returns true when the current process is running with Administrator privileges. */
function isElevated(): boolean {
  try {
    execSync("net session", { stdio: "ignore" });
    return true;
  } catch {
    return false;
  }
}

/** UIAccess-enabled AutoHotkey build (created by scripts/install-uac-task.ps1). */
const UIA_AHK = "C:\\Program Files\\AutoHotkey\\v2\\AutoHotkey64_UIA.exe";

/**
 * Start the AutoHotkey UAC watcher (scripts/uac-autoyes.ahk).
 *
 * It runs alongside the install and clicks "Yes" if the UAC consent dialog
 * appears. If no dialog appears, the watcher simply times out and exits on its
 * own — so the install always proceeds whether or not UAC shows up.
 *
 * Two non-obvious requirements (set up once via scripts/install-uac-task.ps1):
 *   1. consent.exe runs at System integrity, so the watcher must be the
 *      UIAccess AutoHotkey build to be allowed to click it (plus secure desktop
 *      disabled so the dialog is on the normal desktop).
 *   2. A UIAccess exe can ONLY be launched via ShellExecute, which routes
 *      through the AppInfo service that grants the UIAccess token. spawn()/
 *      CreateProcess fail with ERROR_ELEVATION_REQUIRED (0x800702E4). We
 *      therefore launch it through PowerShell's Start-Process (ShellExecute).
 *
 * Best-effort: a watcher failure is logged and never blocks the install.
 */
function startUacWatcher(): void {
  const runWatcher = path.resolve(__dirname, "../../scripts/run-watcher.ps1");

  if (!fs.existsSync(UIA_AHK)) {
    console.warn(
      `[launcher] UIAccess AutoHotkey not found at ${UIA_AHK}.\n` +
      "Run scripts/install-uac-task.ps1 from an elevated PowerShell first. " +
      "Proceeding without the UAC watcher."
    );
    return;
  }
  if (!fs.existsSync(runWatcher)) {
    console.warn("[launcher] run-watcher.ps1 not found — proceeding without UAC watcher.");
    return;
  }

  try {
    // Launch via a dedicated -File script (run-watcher.ps1 ShellExecutes the
    // UIAccess exe — the only launch path that grants the UIAccess token).
    //
    // Do NOT use detached:true here. Detaching puts the child on a separate
    // window station, and a UIAccess app cannot be ShellExecute'd from a
    // non-interactive context — the launch silently no-ops. A plain child on
    // the interactive desktop works; we unref() so it never blocks Node exit,
    // and the watcher self-exits after it clicks Yes (or times out) regardless.
    const w = spawn(
      "powershell.exe",
      ["-NoProfile", "-ExecutionPolicy", "Bypass", "-File", runWatcher],
      { stdio: "ignore", windowsHide: true }
    );
    w.unref();
    console.log("[launcher] UAC auto-Yes watcher started (UIAccess via ShellExecute).");
  } catch (e) {
    console.warn(`[launcher] Could not start UAC watcher: ${(e as Error).message}`);
  }
}

/**
 * Install the MSI silently via an auto-elevated helper script.
 *
 * Strategy:
 *   1. If already elevated  → run msiexec /qn directly (no dialog at all).
 *   2. If not elevated      → write a tiny PS1 helper to ProgramData, request
 *      elevation via Start-Process -Verb RunAs (UAC dialog appears ONCE),
 *      run msiexec /qn inside that elevated context, write the exit code to a
 *      result file, then read it back — so the install is fully silent after
 *      the single UAC consent click.
 */
export async function installMsi(): Promise<void> {
  const msiPath = getMsiPath();
  if (!fs.existsSync(msiPath)) {
    throw new Error(
      `MSI not found at "${msiPath}".\n` +
      "Place the installer in the app/ folder and update APP_MSI_PATH in .env."
    );
  }

  const logFile   = "C:\\ProgramData\\qure-widget-install.log";
  const psScript  = "C:\\ProgramData\\qure-widget-install.ps1";
  const resultFile = "C:\\ProgramData\\qure-widget-install-result.txt";

  if (isElevated()) {
    // ── Already admin: fully silent, zero dialogs ──────────────────────────
    console.log("[launcher] Installing MSI (silent, elevated)...");
    const r = spawnSync("msiexec", ["/i", msiPath, "/qn", "/norestart", "/l*v", logFile], {
      stdio: "inherit", timeout: 180_000,
    });
    if (r.status !== 0) {
      throw new Error(`[launcher] MSI install failed (exit ${r.status}). Log: ${logFile}`);
    }
  } else {
    // ── Not elevated: ask for UAC consent once, then install silently ───────
    console.log("[launcher] Requesting elevation — approve the UAC dialog to install the widget.");

    // Write helper script: runs msiexec /qn and writes its exit code to a file.
    const escapedMsi  = msiPath.replace(/'/g, "''");
    const escapedLog  = logFile.replace(/'/g, "''");
    const escapedResult = resultFile.replace(/'/g, "''");
    fs.writeFileSync(psScript,
      `$code = (Start-Process msiexec -ArgumentList @('/i','"${escapedMsi}"','/qn','/norestart','/l*v','"${escapedLog}"') -Wait -PassThru).ExitCode\n` +
      `Set-Content -Path '${escapedResult}' -Value $code\n`
    );

    try {
      fs.unlinkSync(resultFile);
    } catch {}

    // Start the AHK watcher first so it's ready to click "Yes" the moment the
    // UAC dialog appears. If no dialog shows, it harmlessly times out.
    // (Secure desktop must be disabled and the watcher task registered manually
    //  beforehand — see scripts/install-uac-task.ps1.)
    startUacWatcher();

    // Launch the helper script elevated — this is the single UAC consent click.
    const r = spawnSync(
      "powershell.exe",
      [
        "-NonInteractive", "-ExecutionPolicy", "Bypass",
        "-Command",
        `Start-Process powershell -ArgumentList @('-NonInteractive','-ExecutionPolicy','Bypass','-File','${psScript.replace(/'/g, "''")}') -Verb RunAs -Wait`,
      ],
      { stdio: "inherit", timeout: 180_000 }
    );

    if (r.status !== 0) {
      throw new Error("[launcher] UAC elevation was denied or the install helper failed.");
    }

    // Read back the msiexec exit code written by the helper.
    let exitCode = -1;
    try {
      exitCode = parseInt(fs.readFileSync(resultFile, "utf8").trim(), 10);
    } catch {
      throw new Error(`[launcher] Install result file not found — the elevated script may not have run.\nLog: ${logFile}`);
    } finally {
      try { fs.unlinkSync(psScript); } catch {}
      try { fs.unlinkSync(resultFile); } catch {}
    }

    if (exitCode !== 0) {
      throw new Error(`[launcher] MSI install failed (exit ${exitCode}). Log: ${logFile}`);
    }
  }

  console.log("[launcher] MSI install complete.");
}

/** Run once before all tests to ensure the app is installed. */
export async function ensureInstalled(): Promise<void> {
  if (isInstalled()) {
    console.log(`[launcher] App already installed at: ${getExePath()}`);
    return;
  }

  console.log("[launcher] App not found — attempting MSI install...");
  await installMsi();

  // Re-check after install using registry + filesystem discovery
  if (!isInstalled()) {
    throw new Error(
      "[launcher] Exe not found after MSI install.\n" +
      "Check the install log or set APP_EXE_PATH in .env to the correct path."
    );
  }

  console.log(`[launcher] App installed and verified: ${getExePath()}`);
}

export async function launchApp(): Promise<{
  electronApp: ElectronApplication;
  page: Page;
}> {
  const exePath = getExePath(); // will throw with a clear message if not found

  console.log(`[launcher] Launching: ${exePath}`);
  const electronApp = await electron.launch({
    executablePath: exePath,
    args: ["--no-sandbox"],
    timeout: 30_000,
  });

  const page = await electronApp.firstWindow();
  await page.waitForLoadState("domcontentloaded");

  return { electronApp, page };
}

export async function closeApp(app: ElectronApplication | null): Promise<void> {
  if (app) {
    await app.close();
  }
}

export function launchWidget(): ChildProcess {
  const exePath = getExePath();
  console.log(`[launcher] Starting widget process: ${exePath}`);
  const proc = spawn(exePath, [], { detached: true, stdio: "ignore" });
  proc.unref();
  return proc;
}

export function closeWidget(proc: ChildProcess | null): void {
  if (proc && !proc.killed) {
    proc.kill();
  }
}

export async function launchWidgetFromSource(): Promise<{ browser: Browser; page: Page; proc: ChildProcess }> {
  const exePath = getExePath();
  const exeName = path.basename(exePath);
  const debugPort = 9222;

  // Kill any leftover instance then wait for port 9222 to be free.
  try {
    execSync(`taskkill /F /IM "${exeName}" /T`, { stdio: "ignore" });
  } catch {}
  await waitForPortFree(9222);

  // tsx sets ELECTRON_RUN_AS_NODE=1 which makes Electron run as plain Node.js
  // and reject Chromium flags. Clear it so the widget runs as a proper Electron app.
  const env = { ...process.env };
  delete env["ELECTRON_RUN_AS_NODE"];

  console.log(`[launcher] Spawning widget with CDP port ${debugPort}: ${exePath}`);
  const proc = spawn(exePath, [`--remote-debugging-port=${debugPort}`], {
    env,
    stdio: "ignore",
  });

  // Poll until DevTools endpoint is ready (up to 20 s)
  const deadline = Date.now() + 20_000;
  while (Date.now() < deadline) {
    await new Promise<void>(r => setTimeout(r, 1_000));
    try {
      const browser = await chromium.connectOverCDP(`http://localhost:${debugPort}`, { timeout: 2_000 });
      const pages = browser.contexts()[0]?.pages() ?? [];
      if (pages.length > 0) {
        console.log("[launcher] Widget source CDP connected.");
        return { browser, page: pages[0], proc };
      }
      await browser.close();
    } catch {}
  }

  proc.kill();
  throw new Error(`[launcher] Widget source did not expose DevTools on port ${debugPort}.`);
}

/** Resolves true once the given TCP port stops accepting connections (i.e. is free). */
async function waitForPortFree(port: number, timeoutMs = 10_000): Promise<void> {
  const { createConnection } = await import("net");
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const inUse = await new Promise<boolean>((resolve) => {
      const s = createConnection({ port, host: "127.0.0.1" });
      s.once("connect", () => { s.destroy(); resolve(true); });
      s.once("error",   () => resolve(false));
    });
    if (!inUse) return;
    await new Promise<void>(r => setTimeout(r, 500));
  }
  console.warn(`[launcher] Port ${port} still in use after ${timeoutMs}ms — proceeding anyway`);
}

export async function closeWidgetCDP(browser: Browser | null, proc: ChildProcess | null): Promise<void> {
  try { if (browser) await browser.close(); } catch {}
  closeWidget(proc);
  // Wait for port 9222 to be released before returning so the next scenario's
  // Before hook can safely spawn a new widget on the same port.
  await waitForPortFree(9222);
}

const psConsolePath = path.resolve(
  __dirname,
  "../../APIClient_sourcecode/PSConsole/bin/Debug/PSConsole.exe"
);

export function logoutPowerScribe(): void {
  console.log("[launcher] PSConsole logout...");
  const result = spawnSync(psConsolePath, ["logout"], { stdio: "inherit", timeout: 30_000 });
  if (result.status !== 0) {
    console.warn(`[launcher] PSConsole logout exited with code ${result.status}`);
  }
}

export function terminatePowerScribe(): void {
  console.log("[launcher] PSConsole terminate...");
  const result = spawnSync(psConsolePath, ["terminate"], { stdio: "inherit", timeout: 30_000 });
  if (result.status !== 0) {
    console.warn(`[launcher] PSConsole terminate exited with code ${result.status}`);
  }
}
