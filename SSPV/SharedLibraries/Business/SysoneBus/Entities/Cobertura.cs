using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Entities
{
    public class Cobertura
    {
        public int id { get; set; }
        public int Numero { get; set; }
        public string Descripcion { get; set; }
        public TipoCobertura Tipo { get; set; }
        
    }
}
