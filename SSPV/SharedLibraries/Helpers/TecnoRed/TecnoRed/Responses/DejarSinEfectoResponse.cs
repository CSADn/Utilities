using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public class DejarSinEfectoResponse : Response
    {
        public DejarSinEfectoResponse(nodoWsAgenda invocarReturn)
            : base(invocarReturn)
        {
            //
        }


        public override void Parse(List<nodoWsAgenda> items)
        {
            //
        }
    }
}
