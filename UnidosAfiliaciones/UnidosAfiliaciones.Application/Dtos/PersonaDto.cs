using UnidosAfiliaciones.Entities;

namespace UnidosAfiliaciones.Application.Dtos
{
    public class PersonaDto
    {
        public Afiliacion Afiliacion { get; set; }
        public AfiliacionDatos AfiliacionDatos { get; set; }
    }
}
