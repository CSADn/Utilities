using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Entities
{
    public class Beneficiario:Persona
    {
        public DateTime Fecha { get; set; }
        public TipoBeneficiario Tipo { get; set; }
    }
}
