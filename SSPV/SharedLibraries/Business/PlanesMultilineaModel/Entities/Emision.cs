using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class Emision
    {
        public string CodProductor { get; set; }
        public int IdCotizacion { get; set; }
        public int IdPlan { get; set; } // Solo para WSPL
        public string NumeroInterno { get; set; }

        public Asegurado Asegurado { get; set; }
        public MedioPago MedioPago { get; set; }
        public bool DomicilioRiesgoUnico { get; set; }
    }
}
