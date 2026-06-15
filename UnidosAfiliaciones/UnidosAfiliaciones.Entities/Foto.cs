using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("fotos")]
    public partial class Foto
    {
        [Key]
        public virtual long IdFoto { get; set; }
        public virtual byte[] Binario { get; set; }
    }
}
