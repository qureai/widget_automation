using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace APIClient.Common.CustomControls.CheckedComboBox
{
    [ToolboxItem(false)]
    internal class CustomDropdownForm : Form
    {
        private readonly CheckedComboBox ccb;

        private bool[] checkedItemsStateArr;

        private bool dropdownClosed = true;

        private CustomCheckedListBox cclb;
        internal CustomCheckedListBox CCLB
        {
            get { return cclb; }
            set { cclb = value; }
        }

        private string oldCheckedItemsAsString = "";

        internal bool ValueChanged
        {
            get
            {
                var newCheckedItemsAsString = GetCheckedItemsAsString();

                return (oldCheckedItemsAsString.Length > 0) && (newCheckedItemsAsString.Length > 0) 
                    ? oldCheckedItemsAsString.CompareTo(newCheckedItemsAsString) != 0 
                    : oldCheckedItemsAsString.Length != newCheckedItemsAsString.Length;
            }
        }

        internal CustomDropdownForm(CheckedComboBox checkedComboBox)
        {
            ccb = checkedComboBox;
            InitializeComponent();
            ShowInTaskbar = false;
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            cclb = new CustomCheckedListBox
            {
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                FormattingEnabled = true,
                Location = new Point(0, 0),
                Name = "cclb",
                Size = new Size(47, 15),
                TabIndex = 0
            };
            cclb.ItemCheck += new ItemCheckEventHandler(cclb_ItemCheck);

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Menu;
            ClientSize = new Size(47, 16);
            ControlBox = false;
            Controls.Add(cclb);
            ForeColor = SystemColors.ControlText;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            MinimizeBox = false;
            Name = "ccbParent";
            StartPosition = FormStartPosition.Manual;

            ResumeLayout(false);
        }

        private void cclb_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ccb.OnItemChecked(sender, e);
        }

        private string GetCheckedItemsAsString()
        {
            var sb = new StringBuilder("");
            var seperator = ", ";
            for (int i = 0; i < cclb.CheckedItems.Count; i++)
            {
                sb.Append(cclb.GetItemText(cclb.CheckedItems[i])).Append(seperator);
            }
            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - seperator.Length, seperator.Length);
            }
            return sb.ToString();
        }

        internal void CloseDropdown(bool applyChanges)
        {
            if (dropdownClosed)
                return;

            if (applyChanges)
            {
                ccb.SelectedIndex = -1;
            }
            else
            {
                for (int i = 0; i < cclb.Items.Count; i++)
                {
                    cclb.SetItemChecked(i, checkedItemsStateArr[i]);
                }
            }
            
            dropdownClosed = true;
            ccb.Focus();
            Hide();
            ccb.OnDropDownClosed();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            dropdownClosed = false;
            oldCheckedItemsAsString = GetCheckedItemsAsString();
            checkedItemsStateArr = new bool[cclb.Items.Count];
            for (int i = 0; i < cclb.Items.Count; i++)
            {
                checkedItemsStateArr[i] = cclb.GetItemChecked(i);
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);
            CloseDropdown(true);
        }
    }
}
