
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("localidades")]
    public class LocalidadFull
    {
        [Key]
        public long IdLocalidad { get; set; }
        public string Localidad { get; set; }
        public string Departamento { get; set; }
        public string Provincia { get; set; }
    }
}
