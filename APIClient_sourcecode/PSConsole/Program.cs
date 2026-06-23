using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Commissure.PACSConnector;

namespace PSConsole
{
    class Program
    {
        // ── Windows API to hide windows by process ────────────────────────────────
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int x, int y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int SW_HIDE     = 0;
        const int SW_MINIMIZE = 6;

        // ─────────────────────────────────────────────────────────────────────────

        static RadWhereCOM _rw;
        static readonly ManualResetEventSlim _loginReady = new ManualResetEventSlim(false);
        static System.Collections.Generic.List<int> _psPids
            = new System.Collections.Generic.List<int>();

        static void Main(string[] args)
        {
            PrintHeader();

            if (args.Length == 0) { PrintHelp(); return; }

            string command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "connect":     RunConnect();                           break;
                    case "login":
                        string user = GetArg(args, "--user");
                        string pass = GetArg(args, "--password");
                        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
                        { Error("Usage: PSConsole.exe login --user <u> --password <p>"); return; }
                        RunLogin(user, pass);
                        break;
                    case "open":
                        string acc  = GetArg(args, "--accession");
                        string site = GetArg(args, "--site") ?? "";
                        if (string.IsNullOrEmpty(acc))
                        { Error("Usage: PSConsole.exe open --accession <a> [--site <s>]"); return; }
                        RunOpen(acc, site);
                        break;
                    case "close":       RunClose();                             break;
                    case "closesign":   RunCloseSign(false);                   break;
                    case "closeprelim": RunCloseSign(true);                    break;
                    case "logout":      RunLogout();                            break;
                    case "terminate":   RunTerminate();                         break;
                    case "status":      RunStatus();                            break;
                    default:
                        Error("Unknown command '" + command + "'.");
                        PrintHelp();
                        break;
                }
            }
            catch (Exception ex) { Error(ex.Message); }
            finally { _loginReady.Dispose(); }
        }

        // ── Connect ───────────────────────────────────────────────────────────────

        static void RunConnect()
        {
            Info("Connecting to PowerScribe...");

            // Snapshot PIDs before starting so we can hide the new window
            var pidsBefore = GetAllProcessPids();

            _rw = new RadWhereCOM();
            RegisterEvents();

            bool started = false;
            _rw.Start(ref started);

            if (!started) throw new Exception("PowerScribe failed to start.");

            // Hide all new PS windows that appeared during Start()
            HideNewPSWindows(pidsBefore);

            // Also use COM properties to keep hidden
            try { _rw.Minimized = true;          } catch { }
            try { _rw.Visible   = false;         } catch { }
            try { _rw.AlwaysOnTop = false;       } catch { }
            try { _rw.RestrictedWorkflow = true; } catch { }
            try { _rw.RestrictedSession  = true; } catch { }

            SaveSession("Connected", "", "", "");
            Success("PowerScribe connected and hidden.");
            Info("Next: PSConsole.exe login --user <u> --password <p>");
        }

        // ── Login ─────────────────────────────────────────────────────────────────

        static void RunLogin(string username, string password)
        {
            EnsureSession("Connected", "Run 'PSConsole.exe connect' first.");
            Info("Logging in as '" + username + "'...");

            ConnectToRunningPS();

            _loginReady.Reset();
            bool success = _rw.LoginEx(username, password);

            if (!success)
            {
                Info("Waiting for login to complete (up to 30s)...");
                if (!_loginReady.Wait(30_000))
                    throw new Exception("Login timed out. Check credentials.");
            }

            HideAllPSWindows();

            SaveSession("LoggedIn", username, "", "");
            Success("Logged in as '" + username + "'.");
            Info("Next: PSConsole.exe open --accession <accession#>");
        }

        // ── Open ──────────────────────────────────────────────────────────────────

        static void RunOpen(string accession, string site)
        {
            EnsureSession("LoggedIn|ReportOpen", "Run 'PSConsole.exe login' first.");
            string siteDisplay = string.IsNullOrEmpty(site) ? "(all sites)" : site;
            Info("Opening report → accession='" + accession + "' site='" + siteDisplay + "'");

            ConnectToRunningPS();

            bool ok = _rw.OpenReport(site, accession);
            if (!ok) throw new Exception("OpenReport failed. Accession '" + accession + "' not found.");

            // Wait for report to load then immediately hide
            Thread.Sleep(500);
            HideAllPSWindows();
            Thread.Sleep(500);
            HideAllPSWindows(); // call twice to catch any delayed popups

            var s = ReadSession();
            SaveSession("ReportOpen", s.User, accession, site);
            Success("Report opened: " + accession);
            Info("Next: PSConsole.exe close  /  closesign  /  closeprelim");
        }

        // ── Close ─────────────────────────────────────────────────────────────────

        static void RunClose()
        {
            EnsureSession("ReportOpen", "No report open. Run 'PSConsole.exe open' first.");
            Info("Closing report (no sign)...");

            ConnectToRunningPS();
            bool ok = _rw.CloseReport(false, false);
            if (!ok) throw new Exception("CloseReport failed.");

            HideAllPSWindows();

            var s = ReadSession();
            SaveSession("LoggedIn", s.User, "", "");
            Success("Report closed.");
            Info("Next: PSConsole.exe open --accession <a>   OR   PSConsole.exe logout");
        }

        // ── Close + Sign ──────────────────────────────────────────────────────────

        static void RunCloseSign(bool preliminary)
        {
            EnsureSession("ReportOpen", "No report open. Run 'PSConsole.exe open' first.");
            string mode = preliminary ? "Preliminary" : "Final";
            Info("Signing as " + mode + "...");

            ConnectToRunningPS();
            bool ok = _rw.CloseReport(true, preliminary);
            if (!ok) throw new Exception("CloseReport (sign) failed.");

            HideAllPSWindows();

            var s = ReadSession();
            SaveSession("LoggedIn", s.User, "", "");
            Success("Report signed as " + mode + ".");
            Info("Next: PSConsole.exe open --accession <a>   OR   PSConsole.exe logout");
        }

        // ── Logout ────────────────────────────────────────────────────────────────

        static void RunLogout()
        {
            EnsureSession("LoggedIn|ReportOpen", "Not logged in.");
            Info("Logging out...");

            ConnectToRunningPS();
            bool ok = _rw.LogoutEx();
            if (!ok) throw new Exception("Logout failed.");

            HideAllPSWindows();

            SaveSession("Connected", "", "", "");
            Success("Logged out.");
            Info("Next: PSConsole.exe terminate   OR   PSConsole.exe login --user <u> --password <p>");
        }

        // ── Terminate ─────────────────────────────────────────────────────────────

        static void RunTerminate()
        {
            EnsureSession("Connected|LoggedIn|ReportOpen", "Not connected.");
            Info("Terminating PowerScribe...");

            ConnectToRunningPS();
            bool ok = _rw.Terminate();
            if (!ok) throw new Exception("Terminate failed.");

            ClearSession();
            Success("PowerScribe terminated.");
        }

        // ── Status ────────────────────────────────────────────────────────────────

        static void RunStatus()
        {
            var s = ReadSession();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Session Status ──────────────────");
            Console.ResetColor();
            Console.WriteLine("  State     : " + s.Status);
            Console.WriteLine("  User      : " + (string.IsNullOrEmpty(s.User)      ? "(none)" : s.User));
            Console.WriteLine("  Accession : " + (string.IsNullOrEmpty(s.Accession) ? "(none)" : s.Accession));
            Console.WriteLine("  Site      : " + (string.IsNullOrEmpty(s.Site)      ? "(none)" : s.Site));
            Console.WriteLine();
        }

        // ── Window hiding ─────────────────────────────────────────────────────────

        /// <summary>
        /// Hides ALL windows belonging to any PowerScribe / RadWhere process.
        /// </summary>
        static void HideAllPSWindows()
        {
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    string name = proc.ProcessName.ToLower();
                    if (name.Contains("powerscribe") || name.Contains("radwhere") ||
                        name.Contains("nuance")      || name.Contains("pscribe"))
                    {
                        HideProcessWindows(proc.Id);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Snapshot PIDs before Start(), then hide all new windows that appeared.
        /// </summary>
        static void HideNewPSWindows(System.Collections.Generic.List<int> pidsBefore)
        {
            // Give PS a moment to create its window
            Thread.Sleep(800);

            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (!pidsBefore.Contains(proc.Id))
                    {
                        // New process appeared — hide its windows
                        HideProcessWindows(proc.Id);
                    }
                }
                catch { }
            }

            // Also do a full sweep
            HideAllPSWindows();
        }

        /// <summary>Hides all visible windows belonging to a specific PID.</summary>
        static void HideProcessWindows(int pid)
        {
            EnumWindows((hWnd, lParam) =>
            {
                try
                {
                    uint windowPid;
                    GetWindowThreadProcessId(hWnd, out windowPid);
                    if (windowPid == (uint)pid && IsWindowVisible(hWnd))
                    {
                        ShowWindow(hWnd, SW_HIDE);
                    }
                }
                catch { }
                return true; // continue enumeration
            }, IntPtr.Zero);
        }

        /// <summary>Returns list of all current process IDs (snapshot).</summary>
        static System.Collections.Generic.List<int> GetAllProcessPids()
        {
            var list = new System.Collections.Generic.List<int>();
            foreach (var p in Process.GetProcesses())
            {
                try { list.Add(p.Id); } catch { }
            }
            return list;
        }

        // ── Reconnect to running PS ───────────────────────────────────────────────

        static void ConnectToRunningPS()
        {
            _rw = new RadWhereCOM();
            RegisterEvents();

            bool started = false;
            _rw.Start(ref started);

            if (!started)
                throw new Exception("Could not reconnect to PowerScribe. Run 'connect' first.");
        }

        // ── Events ────────────────────────────────────────────────────────────────

        static void RegisterEvents()
        {
            _rw.Launched   += () => Event("Launched");
            _rw.Terminated += () => Event("Terminated");

            _rw.UserLoggedIn += (u) =>
            {
                Event("UserLoggedIn  : " + u);
                _loginReady.Set();
            };

            _rw.UserLoggedOut += (u) => Event("UserLoggedOut : " + u);

            _rw.ReportOpened += (site, acc, status, isAdd, plain, rich) =>
                Event("ReportOpened  | acc=" + acc + " | status=" + StatusLabel(status) + " | addendum=" + isAdd);

            _rw.ReportClosed += (site, acc, status, isAdd, plain, rich) =>
                Event("ReportClosed  | acc=" + acc + " | status=" + StatusLabel(status));

            _rw.ReportClosed3 += (site, acc, status, isAdd, plain, rich,
                                   imp, sentPrelim, signer, signerId, dictator, dictatorId) =>
                Event("ReportClosed3 | acc=" + acc +
                      " | status=" + StatusLabel(status) +
                      " | impending=" + StatusLabel(imp) +
                      " | prelim=" + sentPrelim +
                      " | signer=" + signer +
                      " | dictator=" + dictator);

            _rw.ReportChanged += (site, acc, status, isAdd, plain, rich) =>
                Event("ReportChanged | acc=" + acc + " | status=" + StatusLabel(status));

            _rw.AccessionNumbersChanged += (site, acc, status, isAdd, plain, rich) =>
                Event("AccessionNumbersChanged | acc=" + acc);

            _rw.DictationStarted += () => Event("DictationStarted");
            _rw.DictationStopped += () => Event("DictationStopped");
            _rw.ErrorOccurred    += (c, m) => Error("PS Error [" + c + "]: " + m);
            _rw.ReportFinished   += (s) => Event("ReportFinished | status=" + s);
        }

        // ── Session file ──────────────────────────────────────────────────────────

        static readonly string SessionFile = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PSConsole", "session.txt");

        static void SaveSession(string status, string user, string accession, string site)
        {
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SessionFile));
            System.IO.File.WriteAllText(SessionFile,
                status + "\n" + user + "\n" + accession + "\n" + site,
                System.Text.Encoding.UTF8);
        }

        static (string Status, string User, string Accession, string Site) ReadSession()
        {
            if (!System.IO.File.Exists(SessionFile)) return ("None", "", "", "");
            var lines = System.IO.File.ReadAllLines(SessionFile);
            return (
                lines.Length > 0 ? lines[0] : "None",
                lines.Length > 1 ? lines[1] : "",
                lines.Length > 2 ? lines[2] : "",
                lines.Length > 3 ? lines[3] : ""
            );
        }

        static void ClearSession()
        {
            if (System.IO.File.Exists(SessionFile)) System.IO.File.Delete(SessionFile);
        }

        static void EnsureSession(string required, string errorMsg)
        {
            var s = ReadSession();
            foreach (var r in required.Split('|'))
                if (s.Status.Equals(r, StringComparison.OrdinalIgnoreCase)) return;
            throw new Exception("State is '" + s.Status + "'. " + errorMsg);
        }

        // ── Console helpers ───────────────────────────────────────────────────────

        static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================");
            Console.WriteLine("  PowerScribe One - Console Automation Runner");
            Console.WriteLine("================================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  USAGE:  PSConsole.exe <command> [options]");
            Console.WriteLine();
            Console.WriteLine("  COMMANDS:");
            Console.ResetColor();
            Console.WriteLine("  connect                                   Start PowerScribe hidden");
            Console.WriteLine("  login       --user <u> --password <p>     Login");
            Console.WriteLine("  open        --accession <a> [--site <s>]  Open a report");
            Console.WriteLine("  close                                     Close without signing");
            Console.WriteLine("  closesign                                 Sign as Final");
            Console.WriteLine("  closeprelim                               Sign as Preliminary");
            Console.WriteLine("  logout                                    Logout");
            Console.WriteLine("  terminate                                 Shut down PowerScribe");
            Console.WriteLine("  status                                    Show session state");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  EXAMPLE:");
            Console.ResetColor();
            Console.WriteLine("  PSConsole.exe connect");
            Console.WriteLine("  PSConsole.exe login --user raduser --password mypass");
            Console.WriteLine("  PSConsole.exe open --accession ACC001");
            Console.WriteLine("  PSConsole.exe close");
            Console.WriteLine("  PSConsole.exe logout");
            Console.WriteLine("  PSConsole.exe terminate");
            Console.WriteLine();
        }

        static void Info(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("  [INFO]  " + msg);
            Console.ResetColor();
        }

        static void Event(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  [EVENT] " + msg);
            Console.ResetColor();
        }

        static void Success(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [OK]    " + msg);
            Console.ResetColor();
        }

        static void Error(string msg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [ERROR] " + msg);
            Console.ResetColor();
        }

        static string GetArg(string[] args, string flag)
        {
            for (int i = 0; i < args.Length - 1; i++)
                if (args[i].Equals(flag, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            return null;
        }

        static string StatusLabel(int s)
        {
            switch (s)
            {
                case -1: return "Discarded";
                case  0: return "Pending";
                case  1: return "WetRead";
                case  2: return "Draft";
                case  3: return "PendingCorrection";
                case  4: return "Corrected";
                case  5: return "CorrectionRejected";
                case  6: return "PendingSignature";
                case  7: return "SignRejected";
                case  8: return "Final";
                default: return "Unknown(" + s + ")";
            }
        }
    }
}