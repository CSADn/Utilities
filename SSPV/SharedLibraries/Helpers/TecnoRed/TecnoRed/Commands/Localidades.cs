using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnoRed.Commands
{
    public class Localidades : Command
    {
        private const string _codPostal = "codigoPostal";

        public string CodPostal
        {
            get { return Parameters[_codPostal]; }
            set { SetParameter(_codPostal, value); }
        }


        public Localidades()
        {
            Name = "localidadesPorCodigoPostal";
        }

        public override void Validate()
        {
            return;
        }
    }
}
