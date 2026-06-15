using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _1337xSearchTool
{
    public class CustomDataGridView : DataGridView
    {
        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Enter)
            {
                CurrentCell = this[CurrentCell.ColumnIndex, CurrentCell.RowIndex];
                return true;
            }

            return base.ProcessDialogKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                CurrentCell = this[CurrentCell.ColumnIndex, CurrentCell.RowIndex];
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
    }
}
