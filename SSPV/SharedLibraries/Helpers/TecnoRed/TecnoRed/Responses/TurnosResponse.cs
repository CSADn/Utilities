using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public class TurnosResponse : Response
    {
        public List<TurnosItem> Turnos { get; private set; }


        public TurnosResponse(nodoWsAgenda invocarReturn)
            : base(invocarReturn)
        {
            if (Turnos == null)
                Turnos = new List<TurnosItem>();
        }


        public override void Parse(List<nodoWsAgenda> items)
        {
            Turnos = items
                .Select(item => new TurnosItem(
                    codigo: item.Get<int>("codigoTurno"),
                    codLocalidad: item.Get<int>("codigoLocalidad"),
                    fecha: item.Get<DateTime>("fecha"),
                    horaDesde: item.Get<int>("horaDesde"),
                    horaHasta: item.Get<int>("horaHasta")
                ))
                .ToList();
        }
    }

    public class TurnosItem
    {
        public int Codigo { get; set; }

        public int CodLocalidad { get; set; }

        public DateTime Fecha { get; set; }

        public int HoraDesde { get; set; }

        public int HoraHasta { get; set; }


        public TurnosItem(int codigo, int codLocalidad, DateTime fecha, int horaDesde, int horaHasta)
        {
            Codigo = codigo;
            CodLocalidad = codLocalidad;
            Fecha = fecha;
            HoraDesde = horaDesde;
            HoraHasta = horaHasta;
        }
    }
}
