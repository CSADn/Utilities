using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public class ListaCentrosResponse : Response
    {
        public List<ListaCentrosItem> Centros { get; private set; }

        public ListaCentrosResponse(nodoWsAgenda invocarReturn)
            : base(invocarReturn)
        {
            if (Centros == null)
                Centros = new List<ListaCentrosItem>();
        }

        public override void Parse(List<nodoWsAgenda> items)
        {
            Centros = items
                .Select(item => new ListaCentrosItem
                (
                    uid: item.Get<int>("uidCentro"),
                    horario: item.Get<string>("horarioCentro"),
                    direccion: item.Get<string>("direccionCentro"),
                    provincia: item.Get<string>("provincia"),
                    localidad: item.Get<string>("localidad"),
                    telefono: item.Get<string>("telefonoCentro"),
                    observaciones: item.Get<string>("observaciones")
                ))
                .ToList();
        }
    }

    public class ListaCentrosItem
    {
        public int Uid { get; private set; }

        public string Horario { get; private set; }

        public string Direccion { get; private set; }

        public string Provincia { get; private set; }

        public string Localidad { get; private set; }

        public string Telefono { get; private set; }

        public string Observaciones { get; private set; }


        public ListaCentrosItem(int uid, string horario, string direccion, string provincia, string localidad, string telefono, string observaciones)
        {
            Uid = uid;
            Horario = horario;
            Direccion = direccion;
            Provincia = provincia;
            Localidad = localidad;
            Telefono = telefono;
            Observaciones = observaciones;
        }
    }
}
