using System;

namespace uniVerseSise.Manager.Filtros
{
    public class FiltroPoliza
    {
        public int? CodigoProductor { get; set; }
        public DateTime? FechaEmisionRangoInicio { get; set; }
        public DateTime? FechaEmisionRangoFin { get; set; }
        public DateTime? FechaFinVigenciaRangoInicio { get; set; }
        public DateTime? FechaFinVigenciaRangoFin { get; set; }
        public int MaximaCantidad { get; set; }

        public FiltroPoliza()
        {
            MaximaCantidad = 100;
        }
    }
}
