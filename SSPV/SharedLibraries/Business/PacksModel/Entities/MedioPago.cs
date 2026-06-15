using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packs.Entities
{
    public class MedioPago
    {
        public int IdMedioPago { get; set; }
        public int IdConducto { get; set; }
        public int Cuotas { get; set; }
        public string Tarjeta { get; set; }
        public string Vencimiento { get; set; }
        public string CBU { get; set; }
        public string NroCuenta { get; set; }

        public MedioPago()
        {
            //
        }
    }
}
