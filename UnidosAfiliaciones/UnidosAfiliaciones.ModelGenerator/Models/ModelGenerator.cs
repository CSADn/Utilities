using Dapper;
using System;

namespace UnidosAfiliaciones.Entities
{
    [Table("afiliaciones")]
    public partial class Afiliaciones
    {
        public virtual int IdAfilacion { get; set; }
        public virtual int IdEstadoAfiliacion { get; set; }
        public virtual int? IdUsuario { get; set; }
        public virtual long IdLocalidadDni { get; set; }
        public virtual long? IdLocalidadReal { get; set; }
        public virtual DateTime FechaSolicitud { get; set; }
    }

    [Table("usuarios")]
    public partial class Usuarios
    {
        public virtual int IdUsuario { get; set; }
        public virtual string Email { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string Celular { get; set; }
        public virtual string Role { get; set; }
        public virtual string Password { get; set; }
        public virtual int IdEstadoUsuario { get; set; }
    }

    [Table("estadosusuarios")]
    public partial class Estadosusuarios
    {
        public virtual int IdEstadoUsuario { get; set; }
        public virtual string Descripcion { get; set; }
    }

    [Table("sexos")]
    public partial class Sexos
    {
        public virtual string IdSexo { get; set; }
        public virtual string Descripcion { get; set; }
    }

    [Table("afiliacionesdatos")]
    public partial class Afiliacionesdatos
    {
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

    [Table("departamentos")]
    public partial class Departamentos
    {
        public virtual int IdDepartamento { get; set; }
        public virtual int IdProvincia { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string NombreCompleto { get; set; }
        public virtual decimal ProvinciaInterseccion { get; set; }
        public virtual string Categoria { get; set; }
        public virtual decimal Latitud { get; set; }
        public virtual decimal Longitud { get; set; }
    }

    [Table("localidades")]
    public partial class Localidades
    {
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

    [Table("municipios")]
    public partial class Municipios
    {
        public virtual int IdMunicipio { get; set; }
        public virtual int IdProvincia { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string NombreCompleto { get; set; }
        public virtual decimal ProvinciaInterseccion { get; set; }
        public virtual string Categoria { get; set; }
        public virtual decimal Latitud { get; set; }
        public virtual decimal Longitud { get; set; }
    }

    [Table("paises")]
    public partial class Paises
    {
        public virtual int IdPais { get; set; }
        public virtual string Nombre { get; set; }
    }

    [Table("provincias")]
    public partial class Provincias
    {
        public virtual int IdProvincia { get; set; }
        public virtual int IdPais { get; set; }
        public virtual string IdISO { get; set; }
        public virtual string Nombre { get; set; }
        public virtual string NombreISO { get; set; }
        public virtual string NombreCompleto { get; set; }
        public virtual decimal Latitud { get; set; }
        public virtual decimal Longitud { get; set; }
    }

    [Table("estadosafiliaciones")]
    public partial class Estadosafiliaciones
    {
        public virtual int IdEstadoAfiliacion { get; set; }
        public virtual string Descripcion { get; set; }
    }

    [Table("fotos")]
    public partial class Fotos
    {
        public virtual long IdFoto { get; set; }
        public virtual byte[] Binario { get; set; }
    }

    [Table("estadosciviles")]
    public partial class Estadosciviles
    {
        public virtual int IdEstadoCivil { get; set; }
        public virtual string Descripcion { get; set; }
    }

    [Table("usuarioslocalidades")]
    public partial class Usuarioslocalidades
    {
        public virtual int IdUsuario { get; set; }
        public virtual long IdLocalidad { get; set; }
    }

}
