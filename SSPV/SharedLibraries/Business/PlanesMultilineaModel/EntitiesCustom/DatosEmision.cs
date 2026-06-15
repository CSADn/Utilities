using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.EntitiesCustom
{
    public class DatosEmision
    {
        public int IdSolicitud { get; set; }
        public int? NroPoliza { get; set; }
        public int? NroCertificado { get; set; }
        public string GUID { get; set; }
        public List<DatosEmsionSistemaAdicional> SistemasAdicionales { get; set; }
    }

    public class DatosEmsionSistemaAdicional
    {
        public string TipoPlan { get; set; }
        public int IdPlan { get; set; }
        public int IdCotizacion { get; set; }
        public string NumeroInterno { get; set; }
    }
}
