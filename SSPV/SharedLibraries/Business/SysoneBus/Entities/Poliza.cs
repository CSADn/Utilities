using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Entities
{
    public class Poliza
    {

        public long id { get; set; }
        public string Numero { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaVigenciaDesde { get; set; }
        public DateTime FechaVigenciaHasta { get; set; }
        public DateTime FechaEmision { get; set; }
        public EstadoPoliza Estado { get; set;}
        public Tomador Tomador { set; get; }
        public List<Beneficiario> Benificiario {set;get;}
        public List<Endosos> Endosos { get; set; }
        public List<Asegurado> Asegurado { get; set; }
        public List<Cobertura> Cobertura { get; set; }
        public Double Prima { get; set; }
        public Double Premio { get; set; }

    }
}
