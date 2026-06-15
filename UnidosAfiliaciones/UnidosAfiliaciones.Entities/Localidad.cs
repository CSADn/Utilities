using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("localidades")]
    public partial class Localidad
    {
        [Key]
        public virtual long IdLocalidad { get; set; }
        public virtual int IdCensal { get; set; }
        public virtual int? IdDepartamento { get; set; }
        public virtual int? IdMunicipio { get; set; }
        public virtual int IdProvincia { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string NombreCensal { get; set; }
        public virtual string Categoria { get; set; }
        public virtual decimal Latitud { get; set; }
        public virtual decimal Longitud { get; set; }
    }
}
