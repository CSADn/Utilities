using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class TablaFamilia
    {
        #region Properties
        public string NombreTablaPadre { get; set; }
        public string NombreColumnaPadre { get; set; }
        public string NombreTablaHijo { get; set; }
        public string NombreColumnaHijo { get; set; }
        #endregion

        #region Constructor
        public static TablaFamilia Create(DataRow dr)
        {
            return new TablaFamilia()
            {
                NombreTablaPadre = (string)dr["NombreTablaPadre"],
                NombreColumnaPadre = (string)dr["NombreColumnaPadre"],
                NombreTablaHijo = (string)dr["NombreTablaHijo"],
                NombreColumnaHijo = (string)dr["NombreColumnaHijo"]
            };
        }
        #endregion
    }
}


