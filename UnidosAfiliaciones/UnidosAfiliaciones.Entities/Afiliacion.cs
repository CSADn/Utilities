using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("afiliaciones")]
    public partial class Afiliacion
    {
        [Key]
        public virtual int IdAfilacion { get; set; }
        public virtual int IdEstadoAfiliacion { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual long? IdLocalidadDni { get; set; }
        public virtual long? IdLocalidadReal { get; set; }
        public virtual DateTime FechaSolicitud { get; set; }
    }
}
