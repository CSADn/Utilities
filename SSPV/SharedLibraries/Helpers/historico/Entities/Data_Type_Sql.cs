using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class Data_Type_Sql
    {
        #region Properties
        public string DATA_TYPE { get; set; }
        public bool CHARACTER_MAXIMUM_LENGTH { get; set; }
        public bool NUMERIC_PRECISION { get; set; }
        public bool NUMERIC_PRECISION_RADIX { get; set; }
        public string VALUE_DEFAULT { get; set; }
        #endregion

        #region Constructor
        public static Data_Type_Sql Create(string _DATA_TYPE, bool _CHARACTER_MAXIMUM_LENGTH, bool _NUMERIC_PRECISION, bool _NUMERIC_PRECISION_RADIX, string _VALUE_DEFAULT)
        {
            return new Data_Type_Sql()
            {
                DATA_TYPE = _DATA_TYPE,
                CHARACTER_MAXIMUM_LENGTH = _CHARACTER_MAXIMUM_LENGTH,
                NUMERIC_PRECISION = _NUMERIC_PRECISION,
                NUMERIC_PRECISION_RADIX = _NUMERIC_PRECISION_RADIX,
                VALUE_DEFAULT = _VALUE_DEFAULT
            };
        }
        #endregion
    }
}

