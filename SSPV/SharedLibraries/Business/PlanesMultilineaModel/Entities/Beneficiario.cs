using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Beneficiario
    {
        public string Apellido { get; set; }
        public string Nombre { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public double PorcBeneficiario { get; set; }

        public Beneficiario()
        {
            //
        }
    }
}
