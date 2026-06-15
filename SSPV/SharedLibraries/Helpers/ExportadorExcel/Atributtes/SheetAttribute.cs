using System;

namespace Helpers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SheetAttribute : Attribute
    {
        #region Propiedades

        public string SheetName { get; set; }

        public int ExcludedRows { get; set; }

        #endregion

        #region Constructor

        public SheetAttribute(string sheetName, int excludedRows = 1)
        {
            SheetName = sheetName;
            ExcludedRows = excludedRows;
        }

        #endregion
    }
}
