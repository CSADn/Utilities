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
    public class Juicios
    {
        public int NroJuicio;

        public int NroActor;

        public string Nombre;

        public DateTime Fecha;

        public string Email;

        public string Lugar;

        public string Telefono;
        
        [FieldConverter(ConverterKind.Decimal, ",")]
        public decimal Importe;
        
        public bool AdmiteContraoferta;
    }
}
