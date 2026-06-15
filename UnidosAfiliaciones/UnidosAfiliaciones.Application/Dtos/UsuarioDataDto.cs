using System.Collections.Generic;

namespace UnidosAfiliaciones.Application.Dtos
{
    public class UsuarioDataDto
    {
        public int IdUsuario { get; set; }
        public string Mail { get; set; }
        public string Password { get; set; }
        public string RoleName { get; set; }

        public IList<UsuarioLocalidadDataDto> Localidades { get; set; }
    }
}
