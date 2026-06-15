using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("departamentos")]
    public partial class Departamento
    {
        [Key]
        public virtual int IdDepartamento { get; set; }
        public virtual int IdProvincia { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string NombreCompleto { get; set; }
        public virtual decimal ProvinciaInterseccion { get; set; }
        public virtual string Categoria { get; set; }
        public virtual decimal Latitud { get; set; }
        public virtual decimal Longitud { get; set; }
    }
}
