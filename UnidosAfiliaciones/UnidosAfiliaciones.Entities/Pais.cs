using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("paises")]
    public partial class Pais
    {
        [Key]
        public virtual int IdPais { get; set; }
        public virtual string Nombre { get; set; }
    }
}
