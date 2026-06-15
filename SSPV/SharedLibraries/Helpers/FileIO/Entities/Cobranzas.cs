using FileHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntitiesIO
{
    [Serializable]
    [DelimitedRecord(";")]
    [IgnoreFirst(0)]
    public class Cobranzas
    {
        #region Properties

        public string Linea { get; set; }
        public string IdentificadorEmision { get; set; }
        public int NroEndoso { get; set; }
        public int Cuota { get; set; }
        public double Valor { get; set; }
        public DateTime? Vencimiento { get; set; }

        #endregion
    }
}