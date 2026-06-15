using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TecnoRed.Commands
{
    public class DejarSinEfecto : Command
    {
        private const string _numeroTRD = "numeroTRD";

        public string NumeroTRD
        {
            get { return Parameters[_numeroTRD]; }
            set { SetParameter(_numeroTRD, value); }
        }


        public DejarSinEfecto()
        {
            Name = "dejarSinEfecto";
        }

        public override void Validate()
        {
            //
        }
    }
}
