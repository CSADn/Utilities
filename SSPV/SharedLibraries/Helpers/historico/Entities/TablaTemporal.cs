using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Entities
{
    public class TablaTemporal
    {
        #region Properties
        public string NombreTabla { get; set; }
        public string Pk { get; set; }
        public string NombreTablaFiltro { get; set; }
        public string ClaveFiltro { get; set; }
        public bool bCrearTemporal { get; set; }
        #endregion

        #region Constructor
        public static TablaTemporal Create(string _NombreTabla, string _Pk, string _NombreTablaFiltro, string _ClaveFiltro, bool _bCrearTemporal)
        {
            return new TablaTemporal()
            {
                NombreTabla = _NombreTabla,
                Pk = _Pk,
                NombreTablaFiltro = _NombreTablaFiltro,
                ClaveFiltro = _ClaveFiltro,
                bCrearTemporal = _bCrearTemporal
            };
        }
        #endregion
    }
}


