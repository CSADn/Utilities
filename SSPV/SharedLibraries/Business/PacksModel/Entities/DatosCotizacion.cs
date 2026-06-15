using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packs.Entities
{
    public class DatosCotizacion
    {
        public string GUID { get; set; }

        public string CodProductor { get; set; }
        public string CodSubproductor { get; set; }
        public string CodOrganizador { get; set; }

        public int IdUNeg { get; set; }
        public string Subproducto { get; set; }
        public int IdMedioPago { get; set; }

        public Asegurado Asegurado { get; set; }


        public DatosCotizacion()
        {
            //
        }
    }
}
