using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TecnoRed.Commands
{
    public class ListaCentros : Command
    {
        public ListaCentros()
        {
            Name = "listaCentrosInspeccion";
        }

        public override void Validate()
        {
            return;
        }
    }
}
