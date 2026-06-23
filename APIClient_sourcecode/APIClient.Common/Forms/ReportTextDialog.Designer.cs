namespace APIClient.Common.Forms
{
    partial class ReportTextDialog
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
            this.gbReportPlainText = new System.Windows.Forms.GroupBox();
            this.gbReportRichText = new System.Windows.Forms.GroupBox();
            this.txtReportPlainText = new System.Windows.Forms.TextBox();
            this.rtxtReportRichText = new System.Windows.Forms.RichTextBox();
            this.gbReportPlainText.SuspendLayout();
            this.gbReportRichText.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbReportPlainText
            // 
            this.gbReportPlainText.Controls.Add(this.txtReportPlainText);
            this.gbReportPlainText.Location = new System.Drawing.Point(5, 10);
            this.gbReportPlainText.Name = "gbReportPlainText";
            this.gbReportPlainText.Size = new System.Drawing.Size(380, 275);
            this.gbReportPlainText.TabIndex = 0;
            this.gbReportPlainText.TabStop = false;
            this.gbReportPlainText.Text = "Plain Text";
            // 
            // gbReportRichText
            // 
            this.gbReportRichText.Controls.Add(this.rtxtReportRichText);
            this.gbReportRichText.Location = new System.Drawing.Point(395, 10);
            this.gbReportRichText.Name = "gbReportRichText";
            this.gbReportRichText.Size = new System.Drawing.Size(380, 275);
            this.gbReportRichText.TabIndex = 1;
            this.gbReportRichText.TabStop = false;
            this.gbReportRichText.Text = "Formatted Text";
            // 
            // txtReportPlainText
            // 
            this.txtReportPlainText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReportPlainText.Location = new System.Drawing.Point(3, 16);
            this.txtReportPlainText.Multiline = true;
            this.txtReportPlainText.Name = "txtReportPlainText";
            this.txtReportPlainText.ReadOnly = true;
            this.txtReportPlainText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtReportPlainText.Size = new System.Drawing.Size(374, 256);
            this.txtReportPlainText.TabIndex = 0;
            // 
            // rtxtReportRichText
            // 
            this.rtxtReportRichText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxtReportRichText.Location = new System.Drawing.Point(3, 16);
            this.rtxtReportRichText.Name = "rtxtReportRichText";
            this.rtxtReportRichText.ReadOnly = true;
            this.rtxtReportRichText.Size = new System.Drawing.Size(374, 256);
            this.rtxtReportRichText.TabIndex = 0;
            this.rtxtReportRichText.Text = "";
            // 
            // ReportTextDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.ClientSize = new System.Drawing.Size(784, 293);
            this.Controls.Add(this.gbReportRichText);
            this.Controls.Add(this.gbReportPlainText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReportTextDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Report Text";
            this.gbReportPlainText.ResumeLayout(false);
            this.gbReportPlainText.PerformLayout();
            this.gbReportRichText.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbReportPlainText;
        private System.Windows.Forms.TextBox txtReportPlainText;
        private System.Windows.Forms.GroupBox gbReportRichText;
        private System.Windows.Forms.RichTextBox rtxtReportRichText;
    }
}