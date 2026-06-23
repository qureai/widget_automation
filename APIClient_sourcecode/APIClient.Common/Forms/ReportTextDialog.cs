using APIClient.Common.Models;
using System;
using System.Windows.Forms;

namespace APIClient.Common.Forms
{
    public partial class ReportTextDialog : Form
    {
        public ReportTextDialog(EventLog eventLog)
        {
            InitializeComponent();
            Initialize(eventLog);
        }

        private void Initialize(EventLog eventLog)
        {
            if (!string.IsNullOrWhiteSpace(eventLog?.ReportParams?.PlainText))
                txtReportPlainText.Lines = eventLog?.ReportParams.PlainText.Split(new string[] { "\n" }, StringSplitOptions.None);

            rtxtReportRichText.Rtf = eventLog?.ReportParams?.RichText ?? string.Empty;
        }
    }
}
