namespace APIClient.IRadWhere
{
	partial class IRadWhereForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IRadWhereForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.gbClientSession = new System.Windows.Forms.GroupBox();
            this.pnlUserCreds = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.Button();
            this.imlButtons = new System.Windows.Forms.ImageList(this.components);
            this.btnTerminatePSClient = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnConnectToPSClient = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.gbProperties = new System.Windows.Forms.GroupBox();
            this.pnlProperties = new System.Windows.Forms.Panel();
            this.lblToggleRestrictedWorkflow = new System.Windows.Forms.Label();
            this.toggleRestrictedWorkflow = new APIClient.Common.CustomControls.ToggleSwitch.ToggleSwtich();
            this.lblToggleRestrictedSession = new System.Windows.Forms.Label();
            this.toggleRestrictedSession = new APIClient.Common.CustomControls.ToggleSwitch.ToggleSwtich();
            this.lblToggleAlwaysOnTop = new System.Windows.Forms.Label();
            this.toggleAlwaysOnTop = new APIClient.Common.CustomControls.ToggleSwitch.ToggleSwtich();
            this.lblToggleMinimized = new System.Windows.Forms.Label();
            this.toggleMinimized = new APIClient.Common.CustomControls.ToggleSwitch.ToggleSwtich();
            this.lblPropertyValue = new System.Windows.Forms.Label();
            this.txtApiPropertyValue = new System.Windows.Forms.TextBox();
            this.cmbApiProperty = new System.Windows.Forms.ComboBox();
            this.lblProperty = new System.Windows.Forms.Label();
            this.toolTipSettings = new System.Windows.Forms.ToolTip(this.components);
            this.ccbEventsFilter = new APIClient.Common.CustomControls.CheckedComboBox.CheckedComboBox();
            this.gbApiMethods = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExecute = new System.Windows.Forms.Button();
            this.pnlParameters = new System.Windows.Forms.Panel();
            this.lblParameters = new System.Windows.Forms.Label();
            this.cmbApiMethod = new System.Windows.Forms.ComboBox();
            this.lblMethod = new System.Windows.Forms.Label();
            this.gbEventLogging = new System.Windows.Forms.GroupBox();
            this.pnlEventLogging = new System.Windows.Forms.Panel();
            this.btnExportEventLogs = new System.Windows.Forms.Button();
            this.btnClearEventLogs = new System.Windows.Forms.Button();
            this.lblShowEventsOnly = new System.Windows.Forms.Label();
            this.toggleShowEventsOnly = new APIClient.Common.CustomControls.ToggleSwitch.ToggleSwtich();
            this.dgvEventLogging = new System.Windows.Forms.DataGridView();
            this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Type = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EventLog = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gbClientSession.SuspendLayout();
            this.pnlUserCreds.SuspendLayout();
            this.gbProperties.SuspendLayout();
            this.pnlProperties.SuspendLayout();
            this.gbApiMethods.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbEventLogging.SuspendLayout();
            this.pnlEventLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEventLogging)).BeginInit();
            this.SuspendLayout();
            // 
            // gbClientSession
            // 
            this.gbClientSession.BackColor = System.Drawing.SystemColors.Control;
            this.gbClientSession.Controls.Add(this.pnlUserCreds);
            this.gbClientSession.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbClientSession.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gbClientSession.Location = new System.Drawing.Point(14, 8);
            this.gbClientSession.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbClientSession.Name = "gbClientSession";
            this.gbClientSession.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbClientSession.Size = new System.Drawing.Size(1305, 142);
            this.gbClientSession.TabIndex = 0;
            this.gbClientSession.TabStop = false;
            this.gbClientSession.Text = "Client Session";
            // 
            // pnlUserCreds
            // 
            this.pnlUserCreds.Controls.Add(this.btnSettings);
            this.pnlUserCreds.Controls.Add(this.btnTerminatePSClient);
            this.pnlUserCreds.Controls.Add(this.btnLogout);
            this.pnlUserCreds.Controls.Add(this.btnConnectToPSClient);
            this.pnlUserCreds.Controls.Add(this.btnLogin);
            this.pnlUserCreds.Controls.Add(this.lblUsername);
            this.pnlUserCreds.Controls.Add(this.txtUsername);
            this.pnlUserCreds.Controls.Add(this.txtPassword);
            this.pnlUserCreds.Controls.Add(this.lblPassword);
            this.pnlUserCreds.Location = new System.Drawing.Point(9, 23);
            this.pnlUserCreds.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlUserCreds.Name = "pnlUserCreds";
            this.pnlUserCreds.Size = new System.Drawing.Size(1282, 111);
            this.pnlUserCreds.TabIndex = 1;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.SystemColors.Control;
            this.btnSettings.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnSettings.ImageIndex = 0;
            this.btnSettings.ImageList = this.imlButtons;
            this.btnSettings.Location = new System.Drawing.Point(1131, 28);
            this.btnSettings.Margin = new System.Windows.Forms.Padding(0);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(54, 55);
            this.btnSettings.TabIndex = 6;
            this.btnSettings.UseVisualStyleBackColor = false;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // imlButtons
            // 
            this.imlButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlButtons.ImageStream")));
            this.imlButtons.TransparentColor = System.Drawing.Color.Transparent;
            this.imlButtons.Images.SetKeyName(0, "SettingsBlue.png");
            // 
            // btnTerminatePSClient
            // 
            this.btnTerminatePSClient.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnTerminatePSClient.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnTerminatePSClient.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTerminatePSClient.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnTerminatePSClient.FlatAppearance.BorderSize = 0;
            this.btnTerminatePSClient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTerminatePSClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTerminatePSClient.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnTerminatePSClient.Location = new System.Drawing.Point(882, 11);
            this.btnTerminatePSClient.Margin = new System.Windows.Forms.Padding(0);
            this.btnTerminatePSClient.Name = "btnTerminatePSClient";
            this.btnTerminatePSClient.Size = new System.Drawing.Size(180, 35);
            this.btnTerminatePSClient.TabIndex = 5;
            this.btnTerminatePSClient.Text = "Terminate";
            this.toolTipSettings.SetToolTip(this.btnTerminatePSClient, "Terminate PowerScribe Client");
            this.btnTerminatePSClient.UseVisualStyleBackColor = false;
            this.btnTerminatePSClient.Click += new System.EventHandler(this.btnTerminatePSClient_Click);
            // 
            // btnLogout
            // 
            this.btnLogout.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnLogout.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLogout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogout.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnLogout.FlatAppearance.BorderSize = 0;
            this.btnLogout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogout.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogout.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnLogout.Location = new System.Drawing.Point(882, 58);
            this.btnLogout.Margin = new System.Windows.Forms.Padding(0);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(180, 35);
            this.btnLogout.TabIndex = 8;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = false;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnConnectToPSClient
            // 
            this.btnConnectToPSClient.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnConnectToPSClient.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnConnectToPSClient.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnectToPSClient.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnConnectToPSClient.FlatAppearance.BorderSize = 0;
            this.btnConnectToPSClient.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConnectToPSClient.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnectToPSClient.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnConnectToPSClient.Location = new System.Drawing.Point(674, 11);
            this.btnConnectToPSClient.Margin = new System.Windows.Forms.Padding(0);
            this.btnConnectToPSClient.Name = "btnConnectToPSClient";
            this.btnConnectToPSClient.Size = new System.Drawing.Size(180, 35);
            this.btnConnectToPSClient.TabIndex = 4;
            this.btnConnectToPSClient.Text = "Connect";
            this.toolTipSettings.SetToolTip(this.btnConnectToPSClient, "Start and Connect to PowerScribe Client");
            this.btnConnectToPSClient.UseVisualStyleBackColor = false;
            this.btnConnectToPSClient.Click += new System.EventHandler(this.btnConnectToPSClient_Click);
            // 
            // btnLogin
            // 
            this.btnLogin.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnLogin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLogin.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnLogin.Location = new System.Drawing.Point(674, 58);
            this.btnLogin.Margin = new System.Windows.Forms.Padding(0);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(180, 35);
            this.btnLogin.TabIndex = 7;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = false;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblUsername.Location = new System.Drawing.Point(4, 38);
            this.lblUsername.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(86, 20);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Username";
            // 
            // txtUsername
            // 
            this.txtUsername.BackColor = System.Drawing.SystemColors.HighlightText;
            this.txtUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUsername.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUsername.Location = new System.Drawing.Point(9, 63);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(299, 26);
            this.txtUsername.TabIndex = 1;
            // 
            // txtPassword
            // 
            this.txtPassword.BackColor = System.Drawing.SystemColors.HighlightText;
            this.txtPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPassword.Location = new System.Drawing.Point(344, 63);
            this.txtPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(299, 26);
            this.txtPassword.TabIndex = 2;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblPassword.Location = new System.Drawing.Point(344, 38);
            this.lblPassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(83, 20);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            // 
            // gbProperties
            // 
            this.gbProperties.Controls.Add(this.pnlProperties);
            this.gbProperties.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gbProperties.Location = new System.Drawing.Point(14, 158);
            this.gbProperties.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbProperties.Name = "gbProperties";
            this.gbProperties.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbProperties.Size = new System.Drawing.Size(1305, 137);
            this.gbProperties.TabIndex = 1;
            this.gbProperties.TabStop = false;
            this.gbProperties.Text = "Properties";
            // 
            // pnlProperties
            // 
            this.pnlProperties.Controls.Add(this.lblToggleRestrictedWorkflow);
            this.pnlProperties.Controls.Add(this.toggleRestrictedWorkflow);
            this.pnlProperties.Controls.Add(this.lblToggleRestrictedSession);
            this.pnlProperties.Controls.Add(this.toggleRestrictedSession);
            this.pnlProperties.Controls.Add(this.lblToggleAlwaysOnTop);
            this.pnlProperties.Controls.Add(this.toggleAlwaysOnTop);
            this.pnlProperties.Controls.Add(this.lblToggleMinimized);
            this.pnlProperties.Controls.Add(this.toggleMinimized);
            this.pnlProperties.Controls.Add(this.lblPropertyValue);
            this.pnlProperties.Controls.Add(this.txtApiPropertyValue);
            this.pnlProperties.Controls.Add(this.cmbApiProperty);
            this.pnlProperties.Controls.Add(this.lblProperty);
            this.pnlProperties.Location = new System.Drawing.Point(9, 23);
            this.pnlProperties.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlProperties.Name = "pnlProperties";
            this.pnlProperties.Size = new System.Drawing.Size(1282, 100);
            this.pnlProperties.TabIndex = 1;
            // 
            // lblToggleRestrictedWorkflow
            // 
            this.lblToggleRestrictedWorkflow.AutoSize = true;
            this.lblToggleRestrictedWorkflow.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblToggleRestrictedWorkflow.Location = new System.Drawing.Point(1125, 15);
            this.lblToggleRestrictedWorkflow.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToggleRestrictedWorkflow.Name = "lblToggleRestrictedWorkflow";
            this.lblToggleRestrictedWorkflow.Size = new System.Drawing.Size(151, 20);
            this.lblToggleRestrictedWorkflow.TabIndex = 17;
            this.lblToggleRestrictedWorkflow.Text = "Restricted Workflow";
            // 
            // toggleRestrictedWorkflow
            // 
            this.toggleRestrictedWorkflow.BorderColor = System.Drawing.Color.LightGray;
            this.toggleRestrictedWorkflow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleRestrictedWorkflow.ForeColor = System.Drawing.Color.White;
            this.toggleRestrictedWorkflow.IsOn = false;
            this.toggleRestrictedWorkflow.Location = new System.Drawing.Point(1155, 40);
            this.toggleRestrictedWorkflow.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.toggleRestrictedWorkflow.Name = "toggleRestrictedWorkflow";
            this.toggleRestrictedWorkflow.OffColor = System.Drawing.Color.DarkGray;
            this.toggleRestrictedWorkflow.OffText = "OFF";
            this.toggleRestrictedWorkflow.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.toggleRestrictedWorkflow.OnText = "ON";
            this.toggleRestrictedWorkflow.Size = new System.Drawing.Size(82, 43);
            this.toggleRestrictedWorkflow.TabIndex = 15;
            this.toggleRestrictedWorkflow.Text = "Restricted Workflow";
            this.toggleRestrictedWorkflow.TextEnabled = true;
            this.toggleRestrictedWorkflow.Click += new System.EventHandler(this.toggleRestrictedWorkflow_Click);
            // 
            // lblToggleRestrictedSession
            // 
            this.lblToggleRestrictedSession.AutoSize = true;
            this.lblToggleRestrictedSession.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblToggleRestrictedSession.Location = new System.Drawing.Point(975, 15);
            this.lblToggleRestrictedSession.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToggleRestrictedSession.Name = "lblToggleRestrictedSession";
            this.lblToggleRestrictedSession.Size = new System.Drawing.Size(143, 20);
            this.lblToggleRestrictedSession.TabIndex = 15;
            this.lblToggleRestrictedSession.Text = "Restricted Session";
            // 
            // toggleRestrictedSession
            // 
            this.toggleRestrictedSession.BorderColor = System.Drawing.Color.LightGray;
            this.toggleRestrictedSession.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleRestrictedSession.ForeColor = System.Drawing.Color.White;
            this.toggleRestrictedSession.IsOn = false;
            this.toggleRestrictedSession.Location = new System.Drawing.Point(1005, 40);
            this.toggleRestrictedSession.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.toggleRestrictedSession.Name = "toggleRestrictedSession";
            this.toggleRestrictedSession.OffColor = System.Drawing.Color.DarkGray;
            this.toggleRestrictedSession.OffText = "OFF";
            this.toggleRestrictedSession.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.toggleRestrictedSession.OnText = "ON";
            this.toggleRestrictedSession.Size = new System.Drawing.Size(82, 43);
            this.toggleRestrictedSession.TabIndex = 14;
            this.toggleRestrictedSession.Text = "Restricted Session";
            this.toggleRestrictedSession.TextEnabled = true;
            this.toggleRestrictedSession.Click += new System.EventHandler(this.toggleRestrictedSession_Click);
            // 
            // lblToggleAlwaysOnTop
            // 
            this.lblToggleAlwaysOnTop.AutoSize = true;
            this.lblToggleAlwaysOnTop.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblToggleAlwaysOnTop.Location = new System.Drawing.Point(840, 15);
            this.lblToggleAlwaysOnTop.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToggleAlwaysOnTop.Name = "lblToggleAlwaysOnTop";
            this.lblToggleAlwaysOnTop.Size = new System.Drawing.Size(111, 20);
            this.lblToggleAlwaysOnTop.TabIndex = 13;
            this.lblToggleAlwaysOnTop.Text = "Always on Top";
            // 
            // toggleAlwaysOnTop
            // 
            this.toggleAlwaysOnTop.BorderColor = System.Drawing.Color.LightGray;
            this.toggleAlwaysOnTop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleAlwaysOnTop.ForeColor = System.Drawing.Color.White;
            this.toggleAlwaysOnTop.IsOn = false;
            this.toggleAlwaysOnTop.Location = new System.Drawing.Point(855, 40);
            this.toggleAlwaysOnTop.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.toggleAlwaysOnTop.Name = "toggleAlwaysOnTop";
            this.toggleAlwaysOnTop.OffColor = System.Drawing.Color.DarkGray;
            this.toggleAlwaysOnTop.OffText = "OFF";
            this.toggleAlwaysOnTop.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.toggleAlwaysOnTop.OnText = "ON";
            this.toggleAlwaysOnTop.Size = new System.Drawing.Size(82, 43);
            this.toggleAlwaysOnTop.TabIndex = 13;
            this.toggleAlwaysOnTop.Text = "Always on Top";
            this.toggleAlwaysOnTop.TextEnabled = true;
            this.toggleAlwaysOnTop.Click += new System.EventHandler(this.toggleAlwaysOnTop_Click);
            // 
            // lblToggleMinimized
            // 
            this.lblToggleMinimized.AutoSize = true;
            this.lblToggleMinimized.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblToggleMinimized.Location = new System.Drawing.Point(705, 15);
            this.lblToggleMinimized.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblToggleMinimized.Name = "lblToggleMinimized";
            this.lblToggleMinimized.Size = new System.Drawing.Size(79, 20);
            this.lblToggleMinimized.TabIndex = 11;
            this.lblToggleMinimized.Text = "Minimized";
            // 
            // toggleMinimized
            // 
            this.toggleMinimized.BorderColor = System.Drawing.Color.LightGray;
            this.toggleMinimized.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleMinimized.ForeColor = System.Drawing.Color.White;
            this.toggleMinimized.IsOn = false;
            this.toggleMinimized.Location = new System.Drawing.Point(705, 40);
            this.toggleMinimized.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.toggleMinimized.Name = "toggleMinimized";
            this.toggleMinimized.OffColor = System.Drawing.Color.DarkGray;
            this.toggleMinimized.OffText = "OFF";
            this.toggleMinimized.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.toggleMinimized.OnText = "ON";
            this.toggleMinimized.Size = new System.Drawing.Size(82, 43);
            this.toggleMinimized.TabIndex = 12;
            this.toggleMinimized.Text = "Minimized";
            this.toggleMinimized.TextEnabled = true;
            this.toggleMinimized.Click += new System.EventHandler(this.toggleMinimized_Click);
            // 
            // lblPropertyValue
            // 
            this.lblPropertyValue.AutoSize = true;
            this.lblPropertyValue.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblPropertyValue.Location = new System.Drawing.Point(339, 15);
            this.lblPropertyValue.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPropertyValue.Name = "lblPropertyValue";
            this.lblPropertyValue.Size = new System.Drawing.Size(50, 20);
            this.lblPropertyValue.TabIndex = 10;
            this.lblPropertyValue.Text = "Value";
            // 
            // txtApiPropertyValue
            // 
            this.txtApiPropertyValue.BackColor = System.Drawing.SystemColors.HighlightText;
            this.txtApiPropertyValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtApiPropertyValue.Location = new System.Drawing.Point(344, 40);
            this.txtApiPropertyValue.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtApiPropertyValue.Name = "txtApiPropertyValue";
            this.txtApiPropertyValue.ReadOnly = true;
            this.txtApiPropertyValue.Size = new System.Drawing.Size(299, 26);
            this.txtApiPropertyValue.TabIndex = 11;
            // 
            // cmbApiProperty
            // 
            this.cmbApiProperty.BackColor = System.Drawing.SystemColors.HighlightText;
            this.cmbApiProperty.FormattingEnabled = true;
            this.cmbApiProperty.Location = new System.Drawing.Point(9, 40);
            this.cmbApiProperty.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbApiProperty.Name = "cmbApiProperty";
            this.cmbApiProperty.Size = new System.Drawing.Size(298, 28);
            this.cmbApiProperty.TabIndex = 10;
            this.cmbApiProperty.SelectedIndexChanged += new System.EventHandler(this.cmbApiProperty_SelectedIndexChanged);
            // 
            // lblProperty
            // 
            this.lblProperty.AutoSize = true;
            this.lblProperty.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblProperty.Location = new System.Drawing.Point(4, 15);
            this.lblProperty.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblProperty.Name = "lblProperty";
            this.lblProperty.Size = new System.Drawing.Size(68, 20);
            this.lblProperty.TabIndex = 2;
            this.lblProperty.Text = "Property";
            // 
            // ccbEventsFilter
            // 
            this.ccbEventsFilter.CheckOnClick = true;
            this.ccbEventsFilter.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.ccbEventsFilter.DropDownHeight = 1;
            this.ccbEventsFilter.FormattingEnabled = true;
            this.ccbEventsFilter.IntegralHeight = false;
            this.ccbEventsFilter.Location = new System.Drawing.Point(674, 8);
            this.ccbEventsFilter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.ccbEventsFilter.Name = "ccbEventsFilter";
            this.ccbEventsFilter.Size = new System.Drawing.Size(276, 27);
            this.ccbEventsFilter.TabIndex = 17;
            this.toolTipSettings.SetToolTip(this.ccbEventsFilter, "- [Ctrl + Shift] to select all\r\n- [Del] to unselect all\r\n- [Enter] to apply chang" +
        "es and close dropdown\r\n- [Esc] to ignore changes and close dropdown");
            this.ccbEventsFilter.DropDownClosed += new System.EventHandler(this.ccbEventsFilter_DropDownClosed);
            // 
            // gbApiMethods
            // 
            this.gbApiMethods.Controls.Add(this.panel1);
            this.gbApiMethods.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gbApiMethods.Location = new System.Drawing.Point(14, 305);
            this.gbApiMethods.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbApiMethods.Name = "gbApiMethods";
            this.gbApiMethods.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbApiMethods.Size = new System.Drawing.Size(1305, 271);
            this.gbApiMethods.TabIndex = 2;
            this.gbApiMethods.TabStop = false;
            this.gbApiMethods.Text = "API Methods";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnExecute);
            this.panel1.Controls.Add(this.pnlParameters);
            this.panel1.Controls.Add(this.lblParameters);
            this.panel1.Controls.Add(this.cmbApiMethod);
            this.panel1.Controls.Add(this.lblMethod);
            this.panel1.Location = new System.Drawing.Point(9, 23);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1282, 238);
            this.panel1.TabIndex = 1;
            // 
            // btnExecute
            // 
            this.btnExecute.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnExecute.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnExecute.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExecute.Enabled = false;
            this.btnExecute.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnExecute.FlatAppearance.BorderSize = 0;
            this.btnExecute.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExecute.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExecute.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnExecute.Location = new System.Drawing.Point(882, 40);
            this.btnExecute.Margin = new System.Windows.Forms.Padding(0);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(180, 35);
            this.btnExecute.TabIndex = 18;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = false;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // pnlParameters
            // 
            this.pnlParameters.AutoScroll = true;
            this.pnlParameters.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pnlParameters.Location = new System.Drawing.Point(344, 40);
            this.pnlParameters.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(502, 194);
            this.pnlParameters.TabIndex = 17;
            // 
            // lblParameters
            // 
            this.lblParameters.AutoSize = true;
            this.lblParameters.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblParameters.Location = new System.Drawing.Point(339, 15);
            this.lblParameters.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblParameters.Name = "lblParameters";
            this.lblParameters.Size = new System.Drawing.Size(91, 20);
            this.lblParameters.TabIndex = 10;
            this.lblParameters.Text = "Parameters";
            // 
            // cmbApiMethod
            // 
            this.cmbApiMethod.BackColor = System.Drawing.SystemColors.HighlightText;
            this.cmbApiMethod.FormattingEnabled = true;
            this.cmbApiMethod.Location = new System.Drawing.Point(9, 40);
            this.cmbApiMethod.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cmbApiMethod.Name = "cmbApiMethod";
            this.cmbApiMethod.Size = new System.Drawing.Size(298, 28);
            this.cmbApiMethod.TabIndex = 16;
            this.cmbApiMethod.SelectedIndexChanged += new System.EventHandler(this.cmbApiMethod_SelectedIndexChanged);
            // 
            // lblMethod
            // 
            this.lblMethod.AutoSize = true;
            this.lblMethod.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblMethod.Location = new System.Drawing.Point(4, 15);
            this.lblMethod.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMethod.Name = "lblMethod";
            this.lblMethod.Size = new System.Drawing.Size(63, 20);
            this.lblMethod.TabIndex = 2;
            this.lblMethod.Text = "Method";
            // 
            // gbEventLogging
            // 
            this.gbEventLogging.Controls.Add(this.pnlEventLogging);
            this.gbEventLogging.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gbEventLogging.Location = new System.Drawing.Point(14, 585);
            this.gbEventLogging.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEventLogging.Name = "gbEventLogging";
            this.gbEventLogging.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gbEventLogging.Size = new System.Drawing.Size(1305, 354);
            this.gbEventLogging.TabIndex = 3;
            this.gbEventLogging.TabStop = false;
            this.gbEventLogging.Text = "Event Logging";
            // 
            // pnlEventLogging
            // 
            this.pnlEventLogging.Controls.Add(this.btnExportEventLogs);
            this.pnlEventLogging.Controls.Add(this.ccbEventsFilter);
            this.pnlEventLogging.Controls.Add(this.btnClearEventLogs);
            this.pnlEventLogging.Controls.Add(this.lblShowEventsOnly);
            this.pnlEventLogging.Controls.Add(this.toggleShowEventsOnly);
            this.pnlEventLogging.Controls.Add(this.dgvEventLogging);
            this.pnlEventLogging.Location = new System.Drawing.Point(9, 23);
            this.pnlEventLogging.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.pnlEventLogging.Name = "pnlEventLogging";
            this.pnlEventLogging.Size = new System.Drawing.Size(1282, 323);
            this.pnlEventLogging.TabIndex = 1;
            // 
            // btnExportEventLogs
            // 
            this.btnExportEventLogs.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnExportEventLogs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnExportEventLogs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportEventLogs.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnExportEventLogs.FlatAppearance.BorderSize = 0;
            this.btnExportEventLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportEventLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExportEventLogs.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnExportEventLogs.Location = new System.Drawing.Point(1130, 5);
            this.btnExportEventLogs.Margin = new System.Windows.Forms.Padding(0);
            this.btnExportEventLogs.Name = "btnExportEventLogs";
            this.btnExportEventLogs.Size = new System.Drawing.Size(122, 35);
            this.btnExportEventLogs.TabIndex = 24;
            this.btnExportEventLogs.Text = "Export";
            this.btnExportEventLogs.UseVisualStyleBackColor = false;
            this.btnExportEventLogs.Click += new System.EventHandler(this.btnExportEventLogs_Click);
            // 
            // btnClearEventLogs
            // 
            this.btnClearEventLogs.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnClearEventLogs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClearEventLogs.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearEventLogs.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnClearEventLogs.FlatAppearance.BorderSize = 0;
            this.btnClearEventLogs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearEventLogs.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearEventLogs.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnClearEventLogs.Location = new System.Drawing.Point(980, 5);
            this.btnClearEventLogs.Margin = new System.Windows.Forms.Padding(0);
            this.btnClearEventLogs.Name = "btnClearEventLogs";
            this.btnClearEventLogs.Size = new System.Drawing.Size(122, 35);
            this.btnClearEventLogs.TabIndex = 16;
            this.btnClearEventLogs.Text = "Clear";
            this.btnClearEventLogs.UseVisualStyleBackColor = false;
            this.btnClearEventLogs.Click += new System.EventHandler(this.btnClearEventLogs_Click);
            // 
            // lblShowEventsOnly
            // 
            this.lblShowEventsOnly.AutoSize = true;
            this.lblShowEventsOnly.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblShowEventsOnly.Location = new System.Drawing.Point(394, 12);
            this.lblShowEventsOnly.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblShowEventsOnly.Name = "lblShowEventsOnly";
            this.lblShowEventsOnly.Size = new System.Drawing.Size(137, 20);
            this.lblShowEventsOnly.TabIndex = 14;
            this.lblShowEventsOnly.Text = "Show Events Only";
            // 
            // toggleShowEventsOnly
            // 
            this.toggleShowEventsOnly.BorderColor = System.Drawing.Color.LightGray;
            this.toggleShowEventsOnly.Cursor = System.Windows.Forms.Cursors.Hand;
            this.toggleShowEventsOnly.ForeColor = System.Drawing.Color.White;
            this.toggleShowEventsOnly.IsOn = false;
            this.toggleShowEventsOnly.Location = new System.Drawing.Point(534, 0);
            this.toggleShowEventsOnly.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.toggleShowEventsOnly.Name = "toggleShowEventsOnly";
            this.toggleShowEventsOnly.OffColor = System.Drawing.Color.DarkGray;
            this.toggleShowEventsOnly.OffText = "OFF";
            this.toggleShowEventsOnly.OnColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(148)))), ((int)(((byte)(255)))));
            this.toggleShowEventsOnly.OnText = "ON";
            this.toggleShowEventsOnly.Size = new System.Drawing.Size(82, 43);
            this.toggleShowEventsOnly.TabIndex = 15;
            this.toggleShowEventsOnly.Text = "Show Events Only";
            this.toggleShowEventsOnly.TextEnabled = true;
            this.toggleShowEventsOnly.Click += new System.EventHandler(this.toggleShowEventsOnly_Click);
            // 
            // dgvEventLogging
            // 
            this.dgvEventLogging.AllowUserToAddRows = false;
            this.dgvEventLogging.AllowUserToDeleteRows = false;
            this.dgvEventLogging.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvEventLogging.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Time,
            this.Type,
            this.Message,
            this.EventLog});
            this.dgvEventLogging.Location = new System.Drawing.Point(9, 49);
            this.dgvEventLogging.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.dgvEventLogging.Name = "dgvEventLogging";
            this.dgvEventLogging.ReadOnly = true;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvEventLogging.RowHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvEventLogging.RowHeadersWidth = 50;
            this.dgvEventLogging.Size = new System.Drawing.Size(1268, 265);
            this.dgvEventLogging.TabIndex = 0;
            this.dgvEventLogging.RowHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.dgvEventLogging_RowHeaderMouseClick);
            // 
            // Time
            // 
            this.Time.HeaderText = "Time";
            this.Time.MinimumWidth = 150;
            this.Time.Name = "Time";
            this.Time.ReadOnly = true;
            this.Time.Width = 150;
            // 
            // Type
            // 
            this.Type.HeaderText = "Type";
            this.Type.MinimumWidth = 150;
            this.Type.Name = "Type";
            this.Type.ReadOnly = true;
            this.Type.Width = 150;
            // 
            // Message
            // 
            this.Message.HeaderText = "Message";
            this.Message.MinimumWidth = 500;
            this.Message.Name = "Message";
            this.Message.ReadOnly = true;
            this.Message.Width = 2000;
            // 
            // EventLog
            // 
            this.EventLog.HeaderText = "";
            this.EventLog.MinimumWidth = 8;
            this.EventLog.Name = "EventLog";
            this.EventLog.ReadOnly = true;
            this.EventLog.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.EventLog.Visible = false;
            this.EventLog.Width = 8;
            // 
            // IRadWhereForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(1332, 952);
            this.Controls.Add(this.gbEventLogging);
            this.Controls.Add(this.gbApiMethods);
            this.Controls.Add(this.gbProperties);
            this.Controls.Add(this.gbClientSession);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "IRadWhereForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IRadWhere API Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IRadWhereForm_FormClosing);
            this.Load += new System.EventHandler(this.IRadWhereForm_Load);
            this.gbClientSession.ResumeLayout(false);
            this.pnlUserCreds.ResumeLayout(false);
            this.pnlUserCreds.PerformLayout();
            this.gbProperties.ResumeLayout(false);
            this.pnlProperties.ResumeLayout(false);
            this.pnlProperties.PerformLayout();
            this.gbApiMethods.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.gbEventLogging.ResumeLayout(false);
            this.pnlEventLogging.ResumeLayout(false);
            this.pnlEventLogging.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEventLogging)).EndInit();
            this.ResumeLayout(false);

		}

        #endregion

        private System.Windows.Forms.GroupBox gbClientSession;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Panel pnlUserCreds;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnTerminatePSClient;
        private System.Windows.Forms.Button btnConnectToPSClient;
        private System.Windows.Forms.ImageList imlButtons;
        private System.Windows.Forms.GroupBox gbProperties;
        private System.Windows.Forms.Panel pnlProperties;
        private System.Windows.Forms.Label lblProperty;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.ComboBox cmbApiProperty;
        private System.Windows.Forms.TextBox txtApiPropertyValue;
        private System.Windows.Forms.Label lblPropertyValue;
        private System.Windows.Forms.ToolTip toolTipSettings;
        private Common.CustomControls.ToggleSwitch.ToggleSwtich toggleMinimized;
        private System.Windows.Forms.Label lblToggleMinimized;
        private System.Windows.Forms.Label lblToggleAlwaysOnTop;
        private Common.CustomControls.ToggleSwitch.ToggleSwtich toggleAlwaysOnTop;
        private System.Windows.Forms.Label lblToggleRestrictedSession;
        private Common.CustomControls.ToggleSwitch.ToggleSwtich toggleRestrictedSession;
        private System.Windows.Forms.Label lblToggleRestrictedWorkflow;
        private Common.CustomControls.ToggleSwitch.ToggleSwtich toggleRestrictedWorkflow;
        private System.Windows.Forms.GroupBox gbApiMethods;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblParameters;
        private System.Windows.Forms.ComboBox cmbApiMethod;
        private System.Windows.Forms.Label lblMethod;
        private System.Windows.Forms.Panel pnlParameters;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.GroupBox gbEventLogging;
        private System.Windows.Forms.Panel pnlEventLogging;
        private System.Windows.Forms.DataGridView dgvEventLogging;
        private System.Windows.Forms.Label lblShowEventsOnly;
        private Common.CustomControls.ToggleSwitch.ToggleSwtich toggleShowEventsOnly;
        private System.Windows.Forms.Button btnClearEventLogs;
        private System.Windows.Forms.DataGridViewTextBoxColumn Time;
        private System.Windows.Forms.DataGridViewTextBoxColumn Type;
        private System.Windows.Forms.DataGridViewTextBoxColumn Message;
        private System.Windows.Forms.DataGridViewTextBoxColumn EventLog;
        private Common.CustomControls.CheckedComboBox.CheckedComboBox ccbEventsFilter;
		private System.Windows.Forms.Button btnExportEventLogs;
	}
}

