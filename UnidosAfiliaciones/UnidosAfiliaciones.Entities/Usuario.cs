using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("usuarios")]
    public partial class Usuario
    {
        [Key]
        public virtual int IdUsuario { get; set; }
        public virtual string Email { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Celular { get; set; }
        public virtual string Role { get; set; }
        public virtual string Password { get; set; }
        public virtual string PlainPassword { get; set; }
        public virtual int IdEstadoUsuario { get; set; }
    }
}
