using System;
using sysoneBus.Enums;
using System.Collections.Generic;

namespace sysoneBus.Entities
{
    public class Persona
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string RazonSocial { get; set; }
        public TipoDocumento TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public TipoPersona TipoPersona { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public List<string> Dirreccion { get; set; }
        public List<string> Numero { get; set; }
        public List<string> Departament { get; set; }
        public List<string> CP { get; set; }
        public List<string> Telefono { get; set; }

    }
}
