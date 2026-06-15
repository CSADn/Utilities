using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubtitleReport
{
    public static class Extensions
    {
        public static List<DataGridViewColumn> AddColumn(this List<DataGridViewColumn> columns,
            string propertyName,
            string header = null,
            bool visible = true,
            int width = 50,
            DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft,
            CellType cellType = CellType.TextBox,
            Type type = null
        )
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException();

            if (columns == null)
                throw new ArgumentNullException();

            var column = default(DataGridViewColumn);

            switch (cellType)
            {
                case CellType.TextBox:
                    column = new DataGridViewTextBoxColumn();
                    break;

                case CellType.Button:
                    column = new DataGridViewButtonColumn();
                    break;

                default:
                    throw new NotSupportedException();
            }

            column.Name = propertyName;
            column.HeaderText = header ?? propertyName;
            column.DataPropertyName = propertyName;
            column.ValueType = type ?? typeof(string);
            column.DefaultCellStyle.Alignment = align;
            column.SortMode = DataGridViewColumnSortMode.Automatic;
            column.Visible = visible;

            if (width == 0)
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            else if (width == -1)
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            else
                column.Width = width;


            columns.Add(column);

            return columns;
        }

        public static void Invoke(this Control control, Action action)
        {
            control.Invoke(action);
        }
    }
}
