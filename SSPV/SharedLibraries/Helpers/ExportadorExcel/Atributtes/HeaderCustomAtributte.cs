using System;

namespace Helpers
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class HeaderCustomAttribute : Attribute
    {
        #region Propiedades

        public int Index { get; set; }

        public string Header { get; set; }

        public Enums.CellType CellType { get; set; }

        public string Format { get; set; }

        public string Formula { get; set; }

        public bool AllowImportFormula { get; set; }

        #endregion

        #region Constructor

        public HeaderCustomAttribute(int index)
        {
            Index = index;
        }
        
        public HeaderCustomAttribute(int index, string header, Enums.CellType type)
            :this(index)
        {
            Header = header;
            CellType = type;
            
        }

        public HeaderCustomAttribute(int index, string header, Enums.CellType type, string format)
            :this(index, header, type)
        {
            Format = format;
        }

        public HeaderCustomAttribute(int index, string header, Enums.CellType type, string format, string formula, bool allowImportFormula = false)
            : this(index, header, type, format)
        {
            Formula = formula;
            AllowImportFormula = allowImportFormula;
        }

        #endregion
    }
}
