using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Cotizacion
    {
        public int IdPlan { get; set; }

        public string CodProductor { get; set; }
        public string CodSubproductor { get; set; }
        public string CodOrganizador { get; set; }

        public string ApellidoRazonSocialAsegurado { get; set; }
        public string NombreAsegurado { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public string CondicionIVA { get; set; }
        public string CondicionIB { get; set; }
        public int IdTipoPersona { get; set; }
        public int IdProvincia { get; set; }
        public int IdLocalidad { get; set; }
        public int CodPostal { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }
        public string EMail { get; set; }

        public int IdUNeg { get; set; }
        public string Subproducto { get; set; }
        public int IdMedioPago { get; set; }
        public bool DomicilioRiesgoUnico { get; set; }
    }
}
