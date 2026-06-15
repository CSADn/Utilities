namespace UnidosAfiliaciones.Application.Dtos
{
    public class UsuarioLocalidadDataDto
    {
        public int IdUsuario { get; set; }
        public long IdLocalidad { get; set; }
        public string Nombre { get; set; }
        public string NombreCensal { get; set; }
        public ProvinciaDataDto Provincia { get; set; }
    }
}
