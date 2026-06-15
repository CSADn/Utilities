using System.Collections.Generic;
using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades.Test
{
    [NombreArchivo(Archivo = "ACTS.F")]
    public class Acto
    {
        [DatoCampo(true)]
        public ulong Id { get; set; }

        [DatoCampo(true)]
        public int? NroActo { get; set; }

        [DatoCampo(1)]
        public string Descripcion { get; set; }

        [DatoCampo(2)]
        public int? Duracion { get; set; }

        [DatoCampo(3, new [] {1, 2, 3, 4, 5})]
        public List<int> Operadores { get; set; }

        //[DatoCampo(3, 1)]
        public int? Operador1 { get; set; }

        //[DatoCampo(3, 2)]
        public int? Operador2 { get; set; }

        //[DatoCampo(3, 4)]
        public int? Operador4 { get; set; }
    }
}
