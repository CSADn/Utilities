using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public class ConfirmarReservaResponse : Response
    {
        public int NumeroTRD { get; private set; }


        public ConfirmarReservaResponse(nodoWsAgenda invocarReturn)
            : base(invocarReturn)
        {
            NumeroTRD = (Code == -1 ? -1 : _node.Get<int>("numeroTRD"));
        }

        public override void Parse(List<nodoWsAgenda> items)
        {
            //
        }
    }
}
