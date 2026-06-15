using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Entities
{
    public class Endosos
    {
        public long Id { get; set; }
        public int Numero { get; set; }
        public string Descripcion { get; set; }
        public TipoEndoso Tipo{get;set;}
        public DateTime VigenciaDesde { get; set; }
        public DateTime VigenciaHasta { get; set; }

    }
}
