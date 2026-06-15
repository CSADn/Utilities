using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packs.Entities
{
    public class Emision
    {
        public int IdCotizacion { get; set; }
        public string NumeroInterno { get; set; }
        public string CodProductor { get; set; }                        
        public string CodSubproductor { get; set; }
        public string CodOrganizador { get; set; }

        public int idUNeg { get; set; }
        public string Subproducto { get; set; }

        public Asegurado Asegurado { get; set; }
        public MedioPago MedioPago { get; set; }
    }
}
