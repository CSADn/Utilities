using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("provincias")]
    public partial class Provincia
    {
        [Key]
        public virtual int IdProvincia { get; set; }
        public virtual int IdPais { get; set; }
        public virtual string IdISO { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string NombreISO { get; set; }
        public virtual string NombreCompleto { get; set; }
        public virtual decimal Latitud { get; set; }
        public virtual decimal Longitud { get; set; }
    }
}
