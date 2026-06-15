using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileHelpers;

namespace EntitiesIO
{
    [DelimitedRecord(";")]
    [IgnoreFirst(0)]
    public class Progresos_Valores
    {
        public DateTime Fecha;

        public int IdCampaña;

        public int IdSucursal;

        public int IdVendedor;

        public int IdObjetivo;

        public int Valor;

        public int ValorSugerido;
    }
}
