using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServicioFrontera
{
    public class Response
    {
        public string IdMensaje { get; set; }

        public string Result { get; set; }

        public Response()
        {
            this.IdMensaje = string.Empty;
            this.Result = string.Empty;
        }
    }
}
