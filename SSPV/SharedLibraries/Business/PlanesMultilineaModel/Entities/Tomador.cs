using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Tomador
    {
        public string ApellidoRazonSocial { get; set; }
        public string Nombre { get; set; }
        public string IdTipoPersona { get; set; }
        public string Genero { get; set; }
        public string TipoDocumento { get; set; }
        public string NroDocumento { get; set; }
        public string IdProvincia { get; set; }
        public string IdLocalidad { get; set; }
        public string CodPostal { get; set; }
        public string CondicionIVA { get; set; }
        public string CondicionIB { get; set; }
        public string Calle { get; set; }
        public string CalleNro { get; set; }
        public string Piso { get; set; }
        public string Depto { get; set; }
        public string Telefono { get; set; }
        public string Celular { get; set; }
        public string Email { get; set; }
        public string UbicacionRiesgo { get; set; }

        public Tomador()
        {
            //
        }
    }
}
