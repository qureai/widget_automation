using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PACSAutomation
{
    /// <summary>
    /// Main automation form — mirrors the Demo PACS workflow:
    /// Connect → Login → Run Scenarios → Logout → Terminate
    /// </summary>
    public partial class AutomationForm : Form
    {
        // ─── Fields ───────────────────────────────────────────────────────────────

        private PowerScribeAutomation _ps;
        private ScenarioConfig        _cfg = new ScenarioConfig();
        private List<AutomationScenario> _scenarios;
        private CancellationTokenSource  _cts;
        private readonly object          _logLock = new object();

        // ─── Constructor ──────────────────────────────────────────────────────────

        public AutomationForm()
        {
            InitializeComponent();
            BuildScenarioList();
            ApplyCredentialsToUI();
        }

        // ─── Scenario Setup ───────────────────────────────────────────────────────

        private void BuildScenarioList()
        {
            _scenarios = ScenarioRunner.BuildScenarios(_cfg);
            dgvScenarios.Rows.Clear();

            foreach (var s in _scenarios)
            {
                int row = dgvScenarios.Rows.Add(
                    true,           // chkRun
                    s.Id,           // ID
                    s.Name,         // Name
                    s.AccessionNumbers, // Accession
                    "—",            // Status
                    "");            // Detail
                dgvScenarios.Rows[row].Tag = s;
            }
        }

        private void ApplyCredentialsToUI()
        {
            txtUsername.Text = _cfg.Username;
            txtPassword.Text = _cfg.Password;
            txtSiteName.Text = _cfg.SiteName;
        }

        // ─── Button Handlers ──────────────────────────────────────────────────────

        private void btnConnect_Click(object sender, EventArgs e)
        {
            SetUIBusy(true);
            Task.Run(() =>
            {
                try
                {
                    _ps = new PowerScribeAutomation();
                    WireAutomationEvents();
                    _ps.Connect();                          // Synchronous Start()
                    UpdateStatus("Connected to PowerScribe.", Color.ForestGreen);
                }
                catch (Exception ex)
                {
                    _ps?.Dispose();
                    _ps = null;
                    Log($"[ERROR] Connect: {ex.Message}");
                    UpdateStatus($"Connect failed: {ex.Message}", Color.Crimson);
                }
                finally
                {
                    BeginInvoke((Action)(() => SetUIBusy(false)));
                }
            });
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (_ps == null) { ShowError("Not connected. Click Connect first."); return; }
            SetUIBusy(true);
            Task.Run(() =>
            {
                try
                {
                    bool ok = _ps.Login(txtUsername.Text.Trim(), txtPassword.Text);
                    UpdateStatus(ok ? "Logged in." : "Login pending (check PS client).", ok ? Color.ForestGreen : Color.DarkOrange);
                }
                catch (Exception ex) { Log($"[ERROR] Login: {ex.Message}"); }
                finally { BeginInvoke((Action)(() => SetUIBusy(false))); }
            });
        }

        private void btnRunSelected_Click(object sender, EventArgs e)
        {
            var selected = GetSelectedScenarios();
            if (!selected.Any()) { ShowError("Select at least one scenario."); return; }
            RunScenarios(selected);
        }

        private void btnRunAll_Click(object sender, EventArgs e) =>
            RunScenarios(_scenarios);

        private void btnRunSelected2_Click(object sender, EventArgs e)
        {
            if (dgvScenarios.CurrentRow?.Tag is AutomationScenario s)
                RunScenarios(new[] { s });
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (_ps == null) { ShowError("Not connected."); return; }
            Task.Run(() =>
            {
                try   { bool ok = _ps.Logout(); UpdateStatus(ok ? "Logged out." : "Logout failed.", ok ? Color.ForestGreen : Color.Crimson); }
                catch (Exception ex) { Log($"[ERROR] Logout: {ex.Message}"); }
            });
        }

        private void btnTerminate_Click(object sender, EventArgs e)
        {
            if (_ps == null) { ShowError("Not connected."); return; }
            Task.Run(() =>
            {
                try
                {
                    bool ok = _ps.Terminate();
                    if (ok) BeginInvoke((Action)(() => { _ps.Dispose(); _ps = null; UpdateStatus("PowerScribe terminated.", Color.Gray); }));
                    else Log("[WARN] Terminate was cancelled by the user.");
                }
                catch (Exception ex) { Log($"[ERROR] Terminate: {ex.Message}"); }
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _cts?.Cancel();
            Log("[INFO] Run cancelled by user.");
        }

        private void btnClearLog_Click(object sender, EventArgs e) =>
            Invoke((Action)(() => lstLog.Items.Clear()));

        private void btnEditConfig_Click(object sender, EventArgs e)
        {
            using var dlg = new ConfigDialog(_cfg);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _cfg = dlg.Config;
                BuildScenarioList();
                ApplyCredentialsToUI();
                Log("[INFO] Configuration updated.");
            }
        }

        // ─── Scenario Execution ───────────────────────────────────────────────────

        private void RunScenarios(IEnumerable<AutomationScenario> scenarios)
        {
            if (_ps == null || !_ps.IsLoggedIn)
            {
                ShowError("Must be connected and logged in before running scenarios.");
                return;
            }

            _cts = new CancellationTokenSource();
            var list = scenarios.ToList();
            SetUIBusy(true);
            btnStop.Enabled = true;

            Task.Run(() =>
            {
                foreach (var s in list)
                {
                    if (_cts.Token.IsCancellationRequested) break;

                    Log($"─── Running [{s.Id}] {s.Name} ───");
                    SetScenarioStatus(s.Id, ScenarioStatus.Running, "Running…");

                    var result = ScenarioRunner.Run(s, _ps);

                    Log($"    Result: {result.Status} — {result.Detail} ({result.Duration.TotalSeconds:F1}s)");
                    SetScenarioStatus(s.Id, result.Status, result.Detail);

                    // Brief pause between scenarios
                    if (!_cts.Token.IsCancellationRequested)
                        Thread.Sleep(800);
                }

                BeginInvoke((Action)(() =>
                {
                    SetUIBusy(false);
                    btnStop.Enabled = false;
                    Log("─── All selected scenarios finished. ───");
                }));
            }, _cts.Token);
        }

        // ─── Automation Event Wiring ──────────────────────────────────────────────

        private void WireAutomationEvents()
        {
            _ps.OnLog               += msg  => AppendLog(msg, Color.DimGray);
            _ps.OnUserLoggedIn      += user => AppendLog($"✔ UserLoggedIn: {user}", Color.ForestGreen);
            _ps.OnUserLoggedOut     += user => AppendLog($"✔ UserLoggedOut: {user}", Color.DarkOrange);
            _ps.OnReportOpened      += args => AppendLog($"📂 ReportOpened: {args}", Color.SteelBlue);
            _ps.OnReportClosed      += args => AppendLog($"✖ ReportClosed: {args}", Color.SlateGray);
            _ps.OnReportClosed3     += args => AppendLog(
                $"✖ ReportClosed3: {args} | prelim={args.SentAsPreliminary} signer={args.Signer} dictator={args.Dictator}", Color.SlateGray);
            _ps.OnReportChanged     += args => AppendLog($"✎ ReportChanged: {args}", Color.DarkCyan);
            _ps.OnAccessionNumbersChanged += args => AppendLog($"🔗 AccessionNumbersChanged: {args}", Color.Purple);
            _ps.OnDictationStarted  += ()   => AppendLog("🎙 DictationStarted", Color.DarkGreen);
            _ps.OnDictationStopped  += ()   => AppendLog("⬛ DictationStopped", Color.DarkGreen);
            _ps.OnError             += (c, m) => AppendLog($"⚠ Error [{c}]: {m}", Color.Crimson);
            _ps.OnTerminated        += ()   => AppendLog("⏹ Terminated", Color.Gray);
        }

        // ─── UI Helpers ───────────────────────────────────────────────────────────

        private void SetUIBusy(bool busy)
        {
            if (InvokeRequired) { BeginInvoke((Action<bool>)SetUIBusy, busy); return; }
            btnConnect.Enabled      = !busy && _ps == null;
            btnLogin.Enabled        = !busy && _ps != null;
            btnRunAll.Enabled       = !busy;
            btnRunSelected.Enabled  = !busy;
            btnLogout.Enabled       = !busy && _ps != null;
            btnTerminate.Enabled    = !busy && _ps != null;
            btnEditConfig.Enabled   = !busy;
            progressBar.Visible     = busy;
            progressBar.Style       = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        }

        private void SetScenarioStatus(string id, ScenarioStatus status, string detail)
        {
            if (InvokeRequired) { BeginInvoke((Action<string, ScenarioStatus, string>)SetScenarioStatus, id, status, detail); return; }

            foreach (DataGridViewRow row in dgvScenarios.Rows)
            {
                if (row.Tag is AutomationScenario s && s.Id == id)
                {
                    row.Cells["colStatus"].Value = status.ToString();
                    row.Cells["colDetail"].Value = detail;
                    row.DefaultCellStyle.ForeColor = StatusColor(status);
                    return;
                }
            }
        }

        private static Color StatusColor(ScenarioStatus s) => s switch
        {
            ScenarioStatus.Passed  => Color.ForestGreen,
            ScenarioStatus.Failed  => Color.Crimson,
            ScenarioStatus.Skipped => Color.DarkOrange,
            ScenarioStatus.Running => Color.SteelBlue,
            _                      => SystemColors.WindowText
        };

        private void Log(string message)
        {
            AppendLog(message, SystemColors.WindowText);
        }

        private void AppendLog(string message, Color color)
        {
            if (InvokeRequired) { BeginInvoke((Action<string, Color>)AppendLog, message, color); return; }
            lock (_logLock)
            {
                lstLog.Items.Add(new LogItem { Message = $"[{DateTime.Now:HH:mm:ss}] {message}", Color = color });
                lstLog.SelectedIndex = lstLog.Items.Count - 1;
            }
        }

        private void lstLog_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            var item = (LogItem)lstLog.Items[e.Index];
            using var brush = new SolidBrush(item.Color);
            e.Graphics.DrawString(item.Message, e.Font, brush, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void UpdateStatus(string text, Color color)
        {
            if (InvokeRequired) { BeginInvoke((Action<string, Color>)UpdateStatus, text, color); return; }
            lblStatus.Text      = text;
            lblStatus.ForeColor = color;
        }

        private List<AutomationScenario> GetSelectedScenarios() =>
            dgvScenarios.Rows.Cast<DataGridViewRow>()
                .Where(r => Convert.ToBoolean(r.Cells["colRun"].Value) && r.Tag is AutomationScenario)
                .Select(r => (AutomationScenario)r.Tag)
                .ToList();

        private static void ShowError(string msg) =>
            MessageBox.Show(msg, "PACS Automation", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        // ─── Form Close ───────────────────────────────────────────────────────────

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            _ps?.Dispose();
            base.OnFormClosing(e);
        }

        // ─── Nested Types ─────────────────────────────────────────────────────────

        private class LogItem
        {
            public string Message { get; set; }
            public Color  Color   { get; set; }
            public override string ToString() => Message;
        }
    }
}
