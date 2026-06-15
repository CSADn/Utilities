namespace UnidosAfiliaciones.Application.Dtos
{
    public class AfiliacionDataDto
    {
        public int IdAfiliacion { get; set; }
        public int IdEstadoAfiliacion { get; set; }
        public string FechaSolicitud { get; set; }

        public UsuarioDataDto Usuario { get; set; }
    }
}
