using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("estadosusuarios")]
    public partial class EstadoUsuario
    {
        [Key]
        public virtual int IdEstadoUsuario { get; set; }
        public virtual string Descripcion { get; set; }
    }
}
