using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Presupuesto
    {
        public int IdCotizacion { get; set; }
        public double SumaAsegurada { get; set; }
        public double PrimaTotal { get; set; }
        public double PremioTotal { get; set; }
        public int Cuotas { get; set; }
        public double ValorCuota { get; set; }
        public double ComisionProdPorc { get; set; }
        public double ComisionProd { get; set; }
        public double ComisionOrgPorc { get; set; }
        public double ComisionOrg { get; set; }
    }
}
