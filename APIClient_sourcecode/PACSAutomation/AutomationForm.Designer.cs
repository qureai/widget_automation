using System.Drawing;
using System.Windows.Forms;

namespace PACSAutomation
{
    partial class AutomationForm
    {
        private System.ComponentModel.IContainer components = null;

        // ── Controls ─────────────────────────────────────────────────────────────
        private SplitContainer     splitMain;
        private SplitContainer     splitLeft;
        private GroupBox           grpConnection;
        private Label              lblUsername;
        private TextBox            txtUsername;
        private Label              lblPassword;
        private TextBox            txtPassword;
        private Label              lblSiteName;
        private TextBox            txtSiteName;
        private Button             btnConnect;
        private Button             btnLogin;
        private Button             btnLogout;
        private Button             btnTerminate;
        private GroupBox           grpScenarios;
        private DataGridView       dgvScenarios;
        private DataGridViewCheckBoxColumn colRun;
        private DataGridViewTextBoxColumn  colId;
        private DataGridViewTextBoxColumn  colName;
        private DataGridViewTextBoxColumn  colAccession;
        private DataGridViewTextBoxColumn  colStatus;
        private DataGridViewTextBoxColumn  colDetail;
        private FlowLayoutPanel    pnlScenarioButtons;
        private Button             btnRunSelected;
        private Button             btnRunAll;
        private Button             btnRunSelected2;
        private Button             btnStop;
        private Button             btnEditConfig;
        private GroupBox           grpLog;
        private ListBox            lstLog;
        private Button             btnClearLog;
        private StatusStrip        statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ProgressBar        progressBar;
        private Label              lblHeader;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Header ────────────────────────────────────────────────────────────
            lblHeader = new Label
            {
                Text      = "PowerScribe One  ·  PACS Automation Runner",
                Dock      = DockStyle.Top,
                Height    = 42,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 80, 140),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(12, 0, 0, 0)
            };

            // ── Split layout ──────────────────────────────────────────────────────
            splitMain = new SplitContainer
            {
                Dock            = DockStyle.Fill,
                Orientation     = Orientation.Horizontal,
                SplitterDistance = 340,
                Panel1MinSize   = 240,
                Panel2MinSize   = 120
            };

            splitLeft = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                Orientation      = Orientation.Vertical,
                SplitterDistance = 220,
                Panel1MinSize    = 200
            };

            // ── Connection Panel ─────────────────────────────────────────────────
            grpConnection = new GroupBox
            {
                Text    = "Connection",
                Dock    = DockStyle.Fill,
                Padding = new Padding(8),
                Font    = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            int y = 22;
            lblUsername = MakeLabel("Username:", 10, y);
            txtUsername = MakeTextBox(90, y, 110); y += 44;
            lblPassword = MakeLabel("Password:", 10, y);
            txtPassword = MakeTextBox(90, y, 110); txtPassword.PasswordChar = '●'; y += 44;
            lblSiteName = MakeLabel("Site Name:", 10, y);
            txtSiteName = MakeTextBox(90, y, 110); y += 48;

            btnConnect   = MakeButton("Connect",   10, y, 92, Color.FromArgb(30, 100, 170));
            btnLogin     = MakeButton("Login",    108, y, 92, Color.FromArgb(50, 140, 80)); y += 36;
            btnLogout    = MakeButton("Logout",    10, y, 92, Color.FromArgb(190, 110, 30));
            btnTerminate = MakeButton("Terminate",108, y, 92, Color.FromArgb(180, 40, 40));

            grpConnection.Controls.AddRange(new Control[]
            {
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                lblSiteName, txtSiteName,
                btnConnect, btnLogin,
                btnLogout, btnTerminate
            });

            splitLeft.Panel1.Controls.Add(grpConnection);

            // ── Scenario Grid ─────────────────────────────────────────────────────
            grpScenarios = new GroupBox
            {
                Text    = "Test Scenarios",
                Dock    = DockStyle.Fill,
                Padding = new Padding(6),
                Font    = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            colRun       = new DataGridViewCheckBoxColumn { HeaderText = "Run",      Name = "colRun",       Width = 38 };
            colId        = new DataGridViewTextBoxColumn  { HeaderText = "ID",       Name = "colId",        Width = 42, ReadOnly = true };
            colName      = new DataGridViewTextBoxColumn  { HeaderText = "Scenario", Name = "colName",      AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, ReadOnly = true };
            colAccession = new DataGridViewTextBoxColumn  { HeaderText = "Accession",Name = "colAccession", Width = 110, ReadOnly = true };
            colStatus    = new DataGridViewTextBoxColumn  { HeaderText = "Status",   Name = "colStatus",    Width = 85,  ReadOnly = true };
            colDetail    = new DataGridViewTextBoxColumn  { HeaderText = "Detail",   Name = "colDetail",    Width = 200, ReadOnly = true };

            dgvScenarios = new DataGridView
            {
                Dock                  = DockStyle.Fill,
                RowHeadersVisible     = false,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect           = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                Font                  = new Font("Segoe UI", 8.5f),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(45, 65, 100),
                    ForeColor = Color.White,
                    Font      = new Font("Segoe UI", 9f, FontStyle.Bold)
                }
            };
            dgvScenarios.Columns.AddRange(colRun, colId, colName, colAccession, colStatus, colDetail);
            dgvScenarios.EnableHeadersVisualStyles = false;
            dgvScenarios.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 255);

            pnlScenarioButtons = new FlowLayoutPanel
            {
                Dock     = DockStyle.Bottom,
                Height   = 38,
                Padding  = new Padding(0, 4, 4, 0),
                FlowDirection = FlowDirection.RightToLeft
            };

            btnEditConfig  = MakeSmallButton("⚙ Config",   Color.FromArgb(80, 80, 80));
            btnStop        = MakeSmallButton("⬛ Stop",     Color.FromArgb(160, 50, 50));  btnStop.Enabled = false;
            btnRunAll      = MakeSmallButton("▶▶ Run All",  Color.FromArgb(30, 130, 60));
            btnRunSelected = MakeSmallButton("▶ Run Checked", Color.FromArgb(30, 100, 170));
            btnRunSelected2= MakeSmallButton("▶ Run Row",  Color.FromArgb(90, 90, 170));

            pnlScenarioButtons.Controls.AddRange(new Control[]
                { btnEditConfig, btnStop, btnRunAll, btnRunSelected, btnRunSelected2 });

            grpScenarios.Controls.Add(dgvScenarios);
            grpScenarios.Controls.Add(pnlScenarioButtons);
            splitLeft.Panel2.Controls.Add(grpScenarios);
            splitMain.Panel1.Controls.Add(splitLeft);

            // ── Event Log ─────────────────────────────────────────────────────────
            grpLog = new GroupBox
            {
                Text    = "Event Log",
                Dock    = DockStyle.Fill,
                Padding = new Padding(6),
                Font    = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            lstLog = new ListBox
            {
                Dock          = DockStyle.Fill,
                DrawMode      = DrawMode.OwnerDrawFixed,
                ItemHeight    = 18,
                Font          = new Font("Consolas", 8.5f),
                BackColor     = Color.FromArgb(18, 20, 28),
                ForeColor     = Color.LightGray,
                BorderStyle   = BorderStyle.None,
                IntegralHeight = false,
                HorizontalScrollbar = true
            };
            lstLog.DrawItem += lstLog_DrawItem;

            btnClearLog = new Button
            {
                Text     = "Clear Log",
                Dock     = DockStyle.Bottom,
                Height   = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Font     = new Font("Segoe UI", 8.5f)
            };
            btnClearLog.Click += btnClearLog_Click;

            grpLog.Controls.Add(lstLog);
            grpLog.Controls.Add(btnClearLog);
            splitMain.Panel2.Controls.Add(grpLog);

            // ── Status Bar ────────────────────────────────────────────────────────
            statusStrip = new StatusStrip { BackColor = Color.FromArgb(30, 30, 40) };
            lblStatus   = new ToolStripStatusLabel("Not connected.")
            {
                ForeColor = Color.LightGray,
                Font      = new Font("Segoe UI", 9f)
            };
            progressBar = new ProgressBar { Width = 160, Height = 14, Visible = false };
            var tsProgress = new ToolStripControlHost(progressBar);
            statusStrip.Items.Add(lblStatus);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(tsProgress);

            // ── Wire Buttons ──────────────────────────────────────────────────────
            btnConnect.Click     += btnConnect_Click;
            btnLogin.Click       += btnLogin_Click;
            btnLogout.Click      += btnLogout_Click;
            btnTerminate.Click   += btnTerminate_Click;
            btnRunAll.Click      += btnRunAll_Click;
            btnRunSelected.Click += btnRunSelected_Click;
            btnRunSelected2.Click+= btnRunSelected2_Click;
            btnStop.Click        += btnStop_Click;
            btnEditConfig.Click  += btnEditConfig_Click;

            // ── Form ──────────────────────────────────────────────────────────────
            Text            = "PowerScribe One — PACS Automation";
            Size            = new Size(1180, 720);
            MinimumSize     = new Size(900, 600);
            Font            = new Font("Segoe UI", 9f);
            BackColor       = Color.FromArgb(240, 243, 250);
            StartPosition   = FormStartPosition.CenterScreen;

            Controls.Add(splitMain);
            Controls.Add(lblHeader);
            Controls.Add(statusStrip);
        }

        // ── Layout Helpers ────────────────────────────────────────────────────────

        private static Label MakeLabel(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y), AutoSize = true,
                        Font = new Font("Segoe UI", 8.8f), ForeColor = Color.DimGray };

        private static TextBox MakeTextBox(int x, int y, int width) =>
            new TextBox { Location = new Point(x, y), Width = width,
                          Font = new Font("Segoe UI", 9f), BorderStyle = BorderStyle.FixedSingle };

        private static Button MakeButton(string text, int x, int y, int width, Color back) =>
            new Button
            {
                Text      = text,
                Location  = new Point(x, y),
                Size      = new Size(width, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };

        private static Button MakeSmallButton(string text, Color back) =>
            new Button
            {
                Text      = text,
                Width     = 105,
                Height    = 28,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                Margin    = new Padding(2, 0, 2, 0)
            };
    }
}
