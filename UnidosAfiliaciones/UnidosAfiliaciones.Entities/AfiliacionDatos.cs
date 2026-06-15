using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UnidosAfiliaciones.Entities
{
    [Table("afiliacionesdatos")]
    public partial class AfiliacionDatos
    {
        [Key]
        public virtual int IdAfiliacion { get; set; }
        public virtual long Dni { get; set; }
        public virtual string Nombres { get; set; }
        public virtual string Apellidos { get; set; }
        public virtual DateTime FechaNacimiento { get; set; }
        public virtual string DomicilioDni { get; set; }
        public virtual string DomicilioReal { get; set; }
        public virtual string Celular { get; set; }
        public virtual string Email { get; set; }
        public virtual string Profesion { get; set; }
        public virtual string LugarNacimiento { get; set; }
        public virtual string IdSexo { get; set; }
        public virtual int? IdEstadoCivil { get; set; }
        public virtual long? IdDniAnverso { get; set; }
        public virtual long? IdDniReverso { get; set; }
        public virtual string NombrePadre { get; set; }
        public virtual string NombreMadre { get; set; }
    }
}
