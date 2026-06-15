using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class ParametroHistorico
    {
        #region Properties
        public string Clave { get; set; }
        public string Valor { get; set; }
        #endregion

        #region Constructor
        public static ParametroHistorico Create(DataRow dr)
        {
            return new ParametroHistorico()
            {
                Clave = (string)dr["Clave"],
                Valor = (string)dr["Valor"]
            };
        }
        #endregion
    }
}

