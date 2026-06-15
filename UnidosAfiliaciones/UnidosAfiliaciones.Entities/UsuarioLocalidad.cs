using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("usuarioslocalidades")]
    public partial class UsuarioLocalidad
    {
        [Key]
        public virtual int IdUsuario { get; set; }
        public virtual long IdLocalidad { get; set; }
    }
}
