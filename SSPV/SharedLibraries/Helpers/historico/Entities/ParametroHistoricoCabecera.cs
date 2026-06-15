using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class ParametroHistoricoCabecera
    {
        #region Properties
        public int idCabecera { get; set; }
        public string nombreTablaCabecera { get; set; }
        public string nombreClaveEdad { get; set; }
        public string rangoClaveEdad { get; set; }
        #endregion

        #region Constructor
        public static ParametroHistoricoCabecera Create(DataRow dr)
        {
            return new ParametroHistoricoCabecera()
            {
                idCabecera = (int)dr["idCabecera"],
                nombreTablaCabecera = (string)dr["nombreTablaCabecera"],
                nombreClaveEdad = (string)dr["nombreClaveEdad"],
                rangoClaveEdad = (string)dr["rangoClaveEdad"]
            };
        }
        #endregion
    }
}
