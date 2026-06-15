using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("estadosciviles")]
    public partial class EstadoCivil
    {
        [Key]
        public virtual int IdEstadoCivil { get; set; }
        public virtual string Descripcion { get; set; }
    }
}
