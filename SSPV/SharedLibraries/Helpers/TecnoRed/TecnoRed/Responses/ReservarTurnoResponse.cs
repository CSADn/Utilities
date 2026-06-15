using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public class ReservarTurnoResponse : Response
    {
        public int NumeroReserva { get; private set; }


        public ReservarTurnoResponse(nodoWsAgenda invocarReturn)
            : base(invocarReturn)
        {
            NumeroReserva = (Code == -1 ? -1 : _node.Get<int>("numeroDeReserva"));
        }


        public override void Parse(List<nodoWsAgenda> items)
        {
            //
        }
    }
}
