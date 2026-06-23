namespace APIClient.Common.Forms
{
    partial class SettingsDialog
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
            this.chkLstBxEvents = new System.Windows.Forms.CheckedListBox();
            this.lblSubscribedEvents = new System.Windows.Forms.Label();
            this.cbxSelectAllEvents = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // chkLstBxEvents
            // 
            this.chkLstBxEvents.FormattingEnabled = true;
            this.chkLstBxEvents.Location = new System.Drawing.Point(12, 28);
            this.chkLstBxEvents.Name = "chkLstBxEvents";
            this.chkLstBxEvents.Size = new System.Drawing.Size(184, 184);
            this.chkLstBxEvents.TabIndex = 0;
            // 
            // lblSubscribedEvents
            // 
            this.lblSubscribedEvents.AutoSize = true;
            this.lblSubscribedEvents.Location = new System.Drawing.Point(10, 10);
            this.lblSubscribedEvents.Name = "lblSubscribedEvents";
            this.lblSubscribedEvents.Size = new System.Drawing.Size(96, 13);
            this.lblSubscribedEvents.TabIndex = 1;
            this.lblSubscribedEvents.Text = "Subscribed Events";
            // 
            // cbxSelectAllEvents
            // 
            this.cbxSelectAllEvents.AutoSize = true;
            this.cbxSelectAllEvents.Location = new System.Drawing.Point(12, 218);
            this.cbxSelectAllEvents.Name = "cbxSelectAllEvents";
            this.cbxSelectAllEvents.Size = new System.Drawing.Size(106, 17);
            this.cbxSelectAllEvents.TabIndex = 2;
            this.cbxSelectAllEvents.Text = "Select All Events";
            this.cbxSelectAllEvents.UseVisualStyleBackColor = true;
            this.cbxSelectAllEvents.CheckedChanged += new System.EventHandler(this.cbxSelectAllEvents_CheckedChanged);
            // 
            // btnOk
            // 
            this.btnOk.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnOk.FlatAppearance.BorderSize = 0;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOk.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnOk.Location = new System.Drawing.Point(22, 245);
            this.btnOk.Margin = new System.Windows.Forms.Padding(0);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 8;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.SystemColors.MenuHighlight;
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatAppearance.BorderColor = System.Drawing.SystemColors.Highlight;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.ForeColor = System.Drawing.SystemColors.HighlightText;
            this.btnCancel.Location = new System.Drawing.Point(112, 245);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // SettingsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.ClientSize = new System.Drawing.Size(208, 277);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.cbxSelectAllEvents);
            this.Controls.Add(this.lblSubscribedEvents);
            this.Controls.Add(this.chkLstBxEvents);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox chkLstBxEvents;
        private System.Windows.Forms.Label lblSubscribedEvents;
        private System.Windows.Forms.CheckBox cbxSelectAllEvents;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
    }
}