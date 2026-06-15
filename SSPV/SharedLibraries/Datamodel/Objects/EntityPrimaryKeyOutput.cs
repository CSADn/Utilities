using System.Data;

namespace DatabaseModel.Objects
{
    public class EntityPrimaryKeyOutput
    {
        #region Propiedades

        public int OrdinalPosition { get; set; }
                
        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public int CharMaxLength { get; set; }
        
        public int NumericPrecision { get; set; }

        public int NumericPresicionRadix { get; set; }

        #endregion

        #region Factory Method

        public static EntityPrimaryKeyOutput Create(DataRow dr)
        {
            return new EntityPrimaryKeyOutput()
            {
                OrdinalPosition = (int)dr["ORDINAL_POSITION"],
                ColumnName = (string)dr["COLUMN_NAME"],
                DataType = ((string)dr["DATA_TYPE"]).ToUpper(),
                CharMaxLength = (int)dr["CHARACTER_MAXIMUM_LENGTH"],
                NumericPrecision = (int)dr["NUMERIC_PRECISION"],
                NumericPresicionRadix = (int)dr["NUMERIC_PRECISION_RADIX"]
            };
        }

        #endregion
    }
}
