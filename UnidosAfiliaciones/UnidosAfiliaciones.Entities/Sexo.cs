using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("sexos")]
    public partial class Sexo
    {
        [Key]
        public virtual string IdSexo { get; set; }
        public virtual string Descripcion { get; set; }
    }
}
