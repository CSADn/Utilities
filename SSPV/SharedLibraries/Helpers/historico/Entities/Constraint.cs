using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class Constraint
    {
        #region Properties
        public string CONSTRAINT_NAME { get; set; }
        #endregion

        #region Constructor
        public static Constraint Create(DataRow dr)
        {
            return new Constraint()
            {
                CONSTRAINT_NAME = (string)dr["CONSTRAINT_NAME"]
            };
        }
        #endregion
    }
}

