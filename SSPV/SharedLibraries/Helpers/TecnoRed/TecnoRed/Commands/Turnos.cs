using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnoRed.Commands
{
    public class Turnos : Command
    {
        private const string _codLocalidad = "codigoLocalidad";

        public string CodLocalidad
        {
            get { return Parameters[_codLocalidad]; }
            set { SetParameter(_codLocalidad, value); }
        }


        public Turnos()
        {
            Name = "turnosPorCodigoLocalidadTrd";
        }

        public override void Validate()
        {
            return;
        }
    }
}
