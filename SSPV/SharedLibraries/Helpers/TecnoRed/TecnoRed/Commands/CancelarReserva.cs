using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnoRed.Commands
{
    public class CancelarReserva : Command
    {
        private const string _numeroDeReserva = "numeroDeReserva";

        public string NumeroDeReserva
        {
            get { return Parameters[_numeroDeReserva]; }
            set { SetParameter(_numeroDeReserva, value); }
        }


        public CancelarReserva()
        {
            Name = "cancelarReservaPendiente";
        }

        public override void Validate()
        {
            return;
        }
    }
}
