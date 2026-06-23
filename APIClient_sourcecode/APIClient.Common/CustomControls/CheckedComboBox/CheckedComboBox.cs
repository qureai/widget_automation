using System;
using System.Windows.Forms;
using System.Drawing;

namespace APIClient.Common.CustomControls.CheckedComboBox
{
    public class CheckedComboBox : ComboBox 
    {
        private readonly CustomDropdownForm dropdown;

        public bool CheckOnClick 
        {
            get { return dropdown.CCLB.CheckOnClick; }
            set { dropdown.CCLB.CheckOnClick = value; }
        }

        public new CheckedListBox.ObjectCollection Items 
        {
            get { return dropdown.CCLB.Items; }
        }

        public CheckedListBox.CheckedItemCollection CheckedItems 
        {
            get { return dropdown.CCLB.CheckedItems; }
        }

        public bool ValueChanged 
        {
            get { return dropdown.ValueChanged; }
        }

        public event ItemCheckEventHandler ItemCheck;
        
        public CheckedComboBox() : base() 
        {
            DrawMode = DrawMode.OwnerDrawVariable;
            DropDownHeight = 1;            
            DropDownStyle = ComboBoxStyle.DropDown;
            dropdown = new CustomDropdownForm(this);
            CheckOnClick = true;
        }

        protected override void Dispose(bool disposing) 
        {
            base.Dispose(disposing);
        }        

        protected override void OnDropDown(EventArgs e) 
        {
            base.OnDropDown(e);
            DoDropDown();    
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
        }

        protected override void OnKeyDown(KeyEventArgs e) 
        {
            if (e.KeyCode == Keys.Down) 
            {
                OnDropDown(null);
            }

            e.Handled = !e.Alt 
                && !(e.KeyCode == Keys.Tab) 
                && !((e.KeyCode == Keys.Left) || (e.KeyCode == Keys.Right) || (e.KeyCode == Keys.Home) || (e.KeyCode == Keys.End));

            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e) 
        {
            e.Handled = true;
            base.OnKeyPress(e);
        }

        public void OnItemChecked(object sender, ItemCheckEventArgs e)
        {
            if (ItemCheck != null)
            {
                ItemCheck(sender, e);
            }
        }

        internal void OnDropDownClosed()
        {
            base.OnDropDownClosed(new EventArgs());
        }

        private void DoDropDown()
        {
            if (!dropdown.Visible)
            {
                var rect = RectangleToScreen(ClientRectangle);
                dropdown.Location = new Point(rect.X, rect.Y + Size.Height);
                var count = dropdown.CCLB.Items.Count;
                if (count > MaxDropDownItems)
                {
                    count = MaxDropDownItems;
                }
                else if (count == 0)
                {
                    count = 1;
                }
                dropdown.Size = new Size(Size.Width, dropdown.CCLB.ItemHeight * count + 2);
                dropdown.Show(this);
            }
        }
    }
}
