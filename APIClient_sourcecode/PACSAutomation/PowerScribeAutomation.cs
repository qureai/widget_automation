using Commissure.PACSConnector;
using System;
using System.Threading;

namespace PACSAutomation
{
    /// <summary>
    /// Core wrapper around the Commissure.PACSConnector.RadWhereCOM COM API.
    /// Handles connection, login, report operations, and event forwarding.
    /// </summary>
    public class PowerScribeAutomation : IDisposable
    {
        // ─── Fields ───────────────────────────────────────────────────────────────

        private RadWhereCOM _rw;
        private bool _disposed;

        // Used to block on async login/launch completion
        private readonly ManualResetEventSlim _loginReady    = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _reportOpened  = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _reportClosed  = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _terminated    = new ManualResetEventSlim(false);

        // ─── Events (forwarded from COM layer) ────────────────────────────────────

        public event Action<string>                    OnLog;
        public event Action<string>                    OnUserLoggedIn;
        public event Action<string>                    OnUserLoggedOut;
        public event Action<ReportEventArgs>           OnReportOpened;
        public event Action<ReportEventArgs>           OnReportClosed;
        public event Action<ReportEventArgs>           OnReportClosed3;
        public event Action<ReportEventArgs>           OnReportChanged;
        public event Action<ReportEventArgs>           OnAccessionNumbersChanged;
        public event Action                            OnDictationStarted;
        public event Action                            OnDictationStopped;
        public event Action<int, string>               OnError;
        public event Action                            OnTerminated;

        // ─── Properties ───────────────────────────────────────────────────────────

        public bool IsConnected  => _rw != null;
        public bool IsLoggedIn   => _rw?.LoggedIn ?? false;
        public string Username   => _rw?.Username;
        public string SiteName   => _rw?.SiteName;
        public string AccessionNumbers => _rw?.AccessionNumbers;

        // ─── Connection ───────────────────────────────────────────────────────────

        /// <summary>
        /// Starts and connects to the PowerScribe client synchronously.
        /// RadWhereCOM.Start() blocks until PS is ready to accept logins.
        /// </summary>
        public void Connect()
        {
            Log("Connecting to PowerScribe client...");
            _rw = new RadWhereCOM();
            RegisterEvents();

            bool started = false;
            _rw.Start(ref started);

            if (!started)
                throw new InvalidOperationException(
                    "PowerScribe client failed to start. Check RadWhereCOM log files.");

            Log("Connected to PowerScribe client successfully.");
        }

        /// <summary>
        /// Launches PowerScribe and logs in asynchronously (Launch = Start + LoginEx in one call).
        /// Waits up to <paramref name="timeoutMs"/> for the Launched event.
        /// </summary>
        public void Launch(string username, string password, int timeoutMs = 30_000)
        {
            Log($"Launching PowerScribe for user '{username}'...");
            _rw = new RadWhereCOM();
            RegisterEvents();

            _loginReady.Reset();
            _rw.Launch(username, password, null); // server param is ignored per spec

            if (!_loginReady.Wait(timeoutMs))
                throw new TimeoutException("Timed out waiting for PowerScribe to launch.");

            Log("PowerScribe launched and ready.");
        }

        // ─── Authentication ───────────────────────────────────────────────────────

        /// <summary>
        /// Logs in using LoginEx. Returns true if login succeeded immediately.
        /// If extra steps are needed (audio training, etc.) the UserLoggedIn event
        /// fires when the user finishes those steps.
        /// </summary>
        public bool Login(string username, string password)
        {
            EnsureConnected();
            Log($"Logging in as '{username}'...");
            _loginReady.Reset();
            bool success = _rw.LoginEx(username, password);
            Log(success ? "Login succeeded." : "Login returned false (extra steps may be needed; await UserLoggedIn event).");
            return success;
        }

        /// <summary>
        /// Logs out the current user. Prompts will be shown if a report is open.
        /// </summary>
        public bool Logout()
        {
            EnsureConnected();
            Log("Logging out...");
            bool success = _rw.LogoutEx();
            Log(success ? "Logout succeeded." : "Logout failed or was cancelled by the user.");
            return success;
        }

        // ─── Report Operations ────────────────────────────────────────────────────

        /// <summary>
        /// Opens or creates a report for the given accession numbers.
        /// Must be called from the Explorer screen.
        /// </summary>
        /// <param name="siteName">Site name, or empty string for system-wide search.</param>
        /// <param name="accessionNumbers">Comma-separated accession numbers.</param>
        public bool OpenReport(string siteName, string accessionNumbers)
        {
            EnsureConnected();
            Log($"OpenReport → site='{siteName}', accessions='{accessionNumbers}'");
            _reportOpened.Reset();
            bool success = _rw.OpenReport(siteName, accessionNumbers);
            Log(success ? "OpenReport succeeded." : "OpenReport failed (accessions not found or modal dialog active).");
            return success;
        }

        /// <summary>
        /// Opens or creates a report, additionally filtering by patient MRN.
        /// </summary>
        public bool OpenReportEx(string siteName, string accessionNumbers, string mrn)
        {
            EnsureConnected();
            Log($"OpenReportEx → site='{siteName}', accessions='{accessionNumbers}', MRN='{mrn}'");
            _reportOpened.Reset();
            bool success = _rw.OpenReportEx(siteName, accessionNumbers, mrn);
            Log(success ? "OpenReportEx succeeded." : "OpenReportEx failed.");
            return success;
        }

        /// <summary>
        /// Closes the open report.
        /// </summary>
        /// <param name="sign">True to invoke the Sign action; false to just close.</param>
        /// <param name="preliminary">
        ///   True to sign as preliminary (PendingSignature). Ignored for residents.
        /// </param>
        public bool CloseReport(bool sign = false, bool preliminary = false)
        {
            EnsureConnected();
            Log($"CloseReport → sign={sign}, preliminary={preliminary}");
            _reportClosed.Reset();
            bool success = _rw.CloseReport(sign, preliminary);
            Log(success ? "CloseReport succeeded." : "CloseReport failed or was cancelled.");
            return success;
        }

        /// <summary>
        /// Cancels the open report without saving.
        /// </summary>
        /// <param name="discard">True to delete the report entirely (no confirmation).</param>
        public bool CancelReport(bool discard = false)
        {
            EnsureConnected();
            Log($"CancelReport → discard={discard}");
            bool success = _rw.CancelReport(discard);
            Log(success ? "CancelReport succeeded." : "CancelReport failed (modal dialog may be active).");
            return success;
        }

        /// <summary>Saves the current report without changing its status.</summary>
        public bool SaveReport(bool closeAfterSave = false)
        {
            EnsureConnected();
            Log($"SaveReport → closeAfterSave={closeAfterSave}");
            bool success = _rw.SaveReport(closeAfterSave);
            Log(success ? "SaveReport succeeded." : "SaveReport failed (no open report or modal dialog).");
            return success;
        }

        // ─── Order Association ────────────────────────────────────────────────────

        /// <summary>
        /// Associates additional accession numbers with the currently open report.
        /// </summary>
        public int AssociateOrders(string accessionNumbers)
        {
            EnsureConnected();
            Log($"AssociateOrders → '{accessionNumbers}'");
            int count = _rw.AssociateOrders(accessionNumbers);
            Log($"AssociateOrders: {count} order(s) associated.");
            return count;
        }

        /// <summary>
        /// Updates the full accession list for a report (from Explorer screen).
        /// Accessions not in <paramref name="newAccessionNumbers"/> will be dissociated.
        /// </summary>
        public int AssociateOrdersEx(string siteName, string currentAccessionNumber, string newAccessionNumbers)
        {
            EnsureConnected();
            Log($"AssociateOrdersEx → site='{siteName}', current='{currentAccessionNumber}', new='{newAccessionNumbers}'");
            int affected = _rw.AssociateOrdersEx(siteName, currentAccessionNumber, newAccessionNumbers);
            Log($"AssociateOrdersEx: {affected} order(s) affected.");
            return affected;
        }

        /// <summary>Removes the specified accessions from the currently open report.</summary>
        public bool DissociateOrders(string accessionNumbers)
        {
            EnsureConnected();
            Log($"DissociateOrders → '{accessionNumbers}'");
            bool success = _rw.DissociateOrders(accessionNumbers);
            Log(success ? "DissociateOrders succeeded." : "DissociateOrders failed.");
            return success;
        }

        // ─── Preview & AutoText ───────────────────────────────────────────────────

        /// <summary>Searches for and previews the specified orders.</summary>
        public bool PreviewOrders(string siteName, string accessionNumbers)
        {
            EnsureConnected();
            Log($"PreviewOrders → site='{siteName}', accessions='{accessionNumbers}'");
            bool success = _rw.PreviewOrders(siteName, accessionNumbers);
            Log(success ? "PreviewOrders succeeded." : "PreviewOrders failed.");
            return success;
        }

        /// <summary>Inserts an AutoText template (or literal text) into the open report.</summary>
        public bool InsertAutoText(string name, bool replace = false)
        {
            EnsureConnected();
            Log($"InsertAutoText → name='{name}', replace={replace}");
            bool success = _rw.InsertAutoText(name, replace);
            Log(success ? "InsertAutoText succeeded." : $"InsertAutoText failed — '{name}' not found in user or site AutoTexts.");
            return success;
        }

        // ─── Terminate ────────────────────────────────────────────────────────────

        /// <summary>
        /// Terminates the PowerScribe client application.
        /// Will prompt the user if a report is open.
        /// </summary>
        public bool Terminate()
        {
            EnsureConnected();
            Log("Terminating PowerScribe client...");
            _terminated.Reset();
            bool success = _rw.Terminate();
            Log(success ? "Terminate succeeded." : "Terminate failed or was cancelled by the user.");
            return success;
        }

        // ─── UI Helpers ───────────────────────────────────────────────────────────

        public void SetMinimized(bool minimized)          { if (_rw != null) _rw.Minimized = minimized; }
        public void SetAlwaysOnTop(bool onTop)            { if (_rw != null) _rw.AlwaysOnTop = onTop; }
        public void SetRestrictedSession(bool restrict)   { if (_rw != null) _rw.RestrictedSession = restrict; }
        public void SetRestrictedWorkflow(bool restrict)  { if (_rw != null) _rw.RestrictedWorkflow = restrict; }

        // ─── Private Helpers ──────────────────────────────────────────────────────

        private void EnsureConnected()
        {
            if (_rw == null)
                throw new InvalidOperationException(
                    "Not connected to PowerScribe. Call Connect() or Launch() first.");
        }

        private void Log(string message) =>
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] {message}");

        // ─── COM Event Wiring ─────────────────────────────────────────────────────

        private void RegisterEvents()
        {
            _rw.Launched                += () => { Log("[EVENT] Launched"); _loginReady.Set(); };
            _rw.UserLoggedIn            += (u)  => { Log($"[EVENT] UserLoggedIn: {u}"); _loginReady.Set(); OnUserLoggedIn?.Invoke(u); };
            _rw.UserLoggedOut           += (u)  => { Log($"[EVENT] UserLoggedOut: {u}"); OnUserLoggedOut?.Invoke(u); };
            _rw.Terminated              += ()   => { Log("[EVENT] Terminated"); _terminated.Set(); OnTerminated?.Invoke(); };
            _rw.DictationStarted        += ()   => { Log("[EVENT] DictationStarted"); OnDictationStarted?.Invoke(); };
            _rw.DictationStopped        += ()   => { Log("[EVENT] DictationStopped"); OnDictationStopped?.Invoke(); };
            _rw.ErrorOccurred           += (c, m) => { Log($"[EVENT] ErrorOccurred: code={c}, msg={m}"); OnError?.Invoke(c, m); };

            _rw.ReportOpened += (site, acc, status, isAdd, plain, rich) =>
            {
                var args = new ReportEventArgs(site, acc, status, isAdd, plain, rich);
                Log($"[EVENT] ReportOpened: {args}");
                _reportOpened.Set();
                OnReportOpened?.Invoke(args);
            };

            _rw.ReportClosed += (site, acc, status, isAdd, plain, rich) =>
            {
                var args = new ReportEventArgs(site, acc, status, isAdd, plain, rich);
                Log($"[EVENT] ReportClosed: {args}");
                _reportClosed.Set();
                OnReportClosed?.Invoke(args);
            };

            _rw.ReportClosed3 += (site, acc, status, isAdd, plain, rich,
                                   impending, sentPrelim, signer, signerId, dictator, dictatorId) =>
            {
                var args = new ReportEventArgs(site, acc, status, isAdd, plain, rich)
                {
                    ImpendingStatus = impending,
                    SentAsPreliminary = sentPrelim,
                    Signer = signer,
                    SignerId = signerId,
                    Dictator = dictator,
                    DictatorId = dictatorId
                };
                Log($"[EVENT] ReportClosed3: {args}");
                OnReportClosed3?.Invoke(args);
            };

            _rw.ReportChanged += (site, acc, status, isAdd, plain, rich) =>
            {
                var args = new ReportEventArgs(site, acc, status, isAdd, plain, rich);
                Log($"[EVENT] ReportChanged: {args}");
                OnReportChanged?.Invoke(args);
            };

            _rw.AccessionNumbersChanged += (site, acc, status, isAdd, plain, rich) =>
            {
                var args = new ReportEventArgs(site, acc, status, isAdd, plain, rich);
                Log($"[EVENT] AccessionNumbersChanged: {args}");
                OnAccessionNumbersChanged?.Invoke(args);
            };

            _rw.ReportFinished += (s) => Log($"[EVENT] ReportFinished: status={s}");
            _rw.AudioTranscribed += (b) => Log($"[EVENT] AudioTranscribed: textRecognized={b}");
        }

        // ─── IDisposable ──────────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _loginReady.Dispose();
            _reportOpened.Dispose();
            _reportClosed.Dispose();
            _terminated.Dispose();
            _rw = null;
        }
    }

    // ─── Helper DTO ───────────────────────────────────────────────────────────────

    /// <summary>Carries report event data from a COM callback.</summary>
    public class ReportEventArgs
    {
        public string SiteName          { get; }
        public string AccessionNumbers  { get; }
        public int    Status            { get; }
        public bool   IsAddendum        { get; }
        public string PlainText         { get; }
        public string RichText          { get; }

        // ReportClosed3 extras
        public int    ImpendingStatus   { get; set; }
        public bool   SentAsPreliminary { get; set; }
        public string Signer            { get; set; }
        public string SignerId          { get; set; }
        public string Dictator          { get; set; }
        public string DictatorId        { get; set; }

        public ReportEventArgs(string site, string accessions, int status,
                               bool isAddendum, string plainText, string richText)
        {
            SiteName         = site;
            AccessionNumbers = accessions;
            Status           = status;
            IsAddendum       = isAddendum;
            PlainText        = plainText;
            RichText         = richText;
        }

        public string StatusLabel => status_label(Status);
        private static string status_label(int s) => s switch
        {
            -1 => "Discarded",
             0 => "Pending",
             1 => "WetRead",
             2 => "Draft",
             3 => "PendingCorrection",
             4 => "Corrected",
             5 => "CorrectionRejected",
             6 => "PendingSignature",
             7 => "SignRejected",
             8 => "Final",
             _ => $"Unknown({s})"
        };

        public override string ToString() =>
            $"site='{SiteName}' acc='{AccessionNumbers}' status={StatusLabel} addendum={IsAddendum}";
    }
}
