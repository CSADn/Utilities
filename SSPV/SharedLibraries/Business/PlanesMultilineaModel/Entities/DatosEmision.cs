using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class DatosEmision
    {
        public string GUID { get; set; }

        public string CodProductor { get; set; }
        public int IdCotizacion { get; set; }
        public int IdPlan { get; set; }
        public string NumeroInterno { get; set; }
        public string FechaEmisionDesde { get; set; }

        public Tomador Tomador { get; set; }
        public MedioPago MedioPago { get; set; }
        public List<Asegurado> Asegurados { get; set; }

        public bool ClausulaNoRepeticion { get; set; }
        public List<Empresa> Empresas { get; set; }

        public DatosEmision()
        {
            //
        }

    }
}
