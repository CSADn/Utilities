using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class EstructuraTabla
    {
        #region Properties
        public int ORDINAL_POSITION { get; set; }
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string DATA_TYPE { get; set; }
        public int CHARACTER_MAXIMUM_LENGTH { get; set; }
        public int NUMERIC_PRECISION { get; set; }
        public int NUMERIC_PRECISION_RADIX { get; set; }
        public string IS_NULLABLE { get; set; }
        public string COLUMN_DEFAULT { get; set; }
        public string IS_PRIMARY_KEY { get; set; }
        #endregion

        #region Constructor
        public static EstructuraTabla Create(DataRow dr)
        {
            return new EstructuraTabla()
            {
                ORDINAL_POSITION            = (int)dr["ORDINAL_POSITION"],
                TABLE_NAME                  = (string)dr["TABLE_NAME"],
                COLUMN_NAME                 = (string)dr["COLUMN_NAME"],
                DATA_TYPE                   = ((string)dr["DATA_TYPE"]).ToUpper(),
                CHARACTER_MAXIMUM_LENGTH    = (int)dr["CHARACTER_MAXIMUM_LENGTH"],
                NUMERIC_PRECISION           = (int)dr["NUMERIC_PRECISION"],
                NUMERIC_PRECISION_RADIX     = (int)dr["NUMERIC_PRECISION_RADIX"],
                IS_NULLABLE                 = (string)dr["IS_NULLABLE"],
                COLUMN_DEFAULT              = (string)dr["COLUMN_DEFAULT"],
                IS_PRIMARY_KEY              = (string)dr["IS_PRIMARY_KEY"]
            };
        }
        #endregion
    }
}

