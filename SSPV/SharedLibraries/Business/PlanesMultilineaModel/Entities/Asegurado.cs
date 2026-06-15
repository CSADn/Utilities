using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Asegurado
    {
        public string ApellidoRazonSocial { get; set; }
        public string Nombre { get; set; }
        public int IdTipoPersona { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public string Sexo { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public int IdProvincia { get; set; }
        public int IdLocalidad { get; set; }
        public int CodPostal { get; set; }
        public string Calle { get; set; }
        public string CalleNro { get; set; }
        public string Piso { get; set; }
        public string Depto { get; set; }
        public string CondicionIVA { get; set; }
        public string CondicionIB { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }

        public int IdActividad { get; set; }
        public int IdTipoBeneficiario { get; set; }

        public List<Beneficiario> Beneficiarios { get; set; }

        public Asegurado()
        {
            //
        }
    }
}
