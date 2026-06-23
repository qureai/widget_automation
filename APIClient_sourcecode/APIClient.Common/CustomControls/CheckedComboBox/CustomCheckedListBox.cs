using System.ComponentModel;
using System.Windows.Forms;

namespace APIClient.Common.CustomControls.CheckedComboBox
{
    [ToolboxItem(false)]
    internal class CustomCheckedListBox : CheckedListBox
    {
        private int curSelIndex = -1;

        public CustomCheckedListBox() : base()
        {
            SelectionMode = SelectionMode.One;
            HorizontalScrollbar = true;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Apply selection.
                ((CustomDropdownForm)Parent).CloseDropdown(true);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // Cancel selection.
                ((CustomDropdownForm)Parent).CloseDropdown(false);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // Delete unckecks all.
                for (var i = 0; i < Items.Count; i++)
                {
                    SetItemChecked(i, false);
                }
                e.Handled = true;
            }
            else if (e.Control && e.Shift)
            {
                // [Ctrl + Shift] ckecks all.
                for (var i = 0; i < Items.Count; i++)
                {
                    SetItemChecked(i, true);
                }
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var index = IndexFromPoint(e.Location);
            if ((index >= 0) && (index != curSelIndex))
            {
                curSelIndex = index;
                SetSelected(index, true);
            }
        }
    }
}
