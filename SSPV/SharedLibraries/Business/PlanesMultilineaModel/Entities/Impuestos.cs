using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Impuestos
    {
        public double AlicuotaSellados { get; set; }
        public double Sellados { get; set; }
        public double AlicuotaIva { get; set; }
        public double Iva { get; set; }
        public double AlicuotaSsn { get; set; }
        public double Ssn { get; set; }
        public double AlicuotaOsseg { get; set; }
        public double Osseg { get; set; }
        public double AlicuotaIb { get; set; }
        public double Ib { get; set; }
        public double AlicuotaPercepcion { get; set; }
        public double Percepcion { get; set; }
        public double AlicuotaImpuestosInternos { get; set; }
        public double ImpuestosInternos { get; set; }
        public double PorcRecargoFinanciero { get; set; }
        public double RecargoFinanciero { get; set; }

        public Impuestos()
        {
            //
        }
    }
}
