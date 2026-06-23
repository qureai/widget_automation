using APIClient.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace APIClient.Common.Forms
{
    public partial class SettingsDialog : Form
    {
        public List<string> SelectedEvents;

        public SettingsDialog(List<PSEvent> psEvents)
        {
            InitializeComponent();
            Initialize(psEvents);
        }

        private void Initialize(List<PSEvent> psEvents)
        {
            if (psEvents != null && psEvents.Any())
            {
                foreach (var psEvent in psEvents)
                {
                    chkLstBxEvents.Items.Add(psEvent.EventName, psEvent.IsSelected);
                }

                cbxSelectAllEvents.Checked = psEvents.All(e => e.IsSelected);
            }
        }

        private void cbxSelectAllEvents_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < chkLstBxEvents.Items.Count; i++)
            {
                chkLstBxEvents.SetItemChecked(i, cbxSelectAllEvents.Checked);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedEvents = chkLstBxEvents.CheckedItems.Cast<string>().ToList();
        }
    }
}
