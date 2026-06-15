using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnoRed.Commands
{
    public class ReservarTurno : Command
    {
        private const string _codTurno = "codigoTurno";
        private const string _codLocalidad = "codigoLocalidad";
        private const string _fecha = "fecha";
        private const string _horaDesde = "horaDesde";

        public string CodTurno
        {
            get { return Parameters[_codTurno]; }
            set { SetParameter(_codTurno, value); }
        }

        public string CodLocalidad
        {
            get { return Parameters[_codLocalidad]; }
            set { SetParameter(_codLocalidad, value); }
        }

        public DateTime Fecha
        {
            get { return DateTime.ParseExact(Parameters[_fecha], "yyyy-MM-dd", null); }
            set { SetParameter(_fecha, value.ToString("yyyy-MM-dd")); }
        }

        public string HoraDesde
        {
            get { return Parameters[_horaDesde]; }
            set { SetParameter(_horaDesde, value); }
        }


        public ReservarTurno()
        {
            Name = "reservarTurno";
        }

        public override void Validate()
        {
            return;
        }
    }
}
