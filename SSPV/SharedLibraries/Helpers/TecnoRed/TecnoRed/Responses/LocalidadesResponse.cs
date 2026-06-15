using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public class LocalidadesResponse : Response
    {
        public List<LocalidadesItem> Localidades { get; private set; }


        public LocalidadesResponse(nodoWsAgenda invocarReturn)
            : base(invocarReturn)
        {
            if (Localidades == null)
                Localidades = new List<LocalidadesItem>();
        }


        public override void Parse(List<nodoWsAgenda> items)
        {
            Localidades = items
                .Select(item => new LocalidadesItem
                (
                    codigo: item.Get<int>("codigoLocalidad"),
                    provincia: item.Get<string>("provincia"),
                    localidad: item.Get<string>("localidad")
                ))
                .ToList();
        }
    }

    public class LocalidadesItem
    {
        public int Codigo { get; set; }

        public string Provincia { get; set; }

        public string Localidad { get; set; }


        public LocalidadesItem(int codigo, string provincia, string localidad)
        {
            Codigo = codigo;
            Provincia = provincia;
            Localidad = localidad;
        }
    }
}
