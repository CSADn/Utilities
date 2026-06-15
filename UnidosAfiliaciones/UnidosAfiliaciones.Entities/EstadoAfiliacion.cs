using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("estadosafiliaciones")]
    public partial class EstadoAfiliacion
    {
        [Key]
        public virtual int IdEstadoAfiliacion { get; set; }
        public virtual string Descripcion { get; set; }
    }
}
