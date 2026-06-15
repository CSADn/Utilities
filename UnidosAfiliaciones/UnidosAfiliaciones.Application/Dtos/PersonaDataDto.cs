namespace UnidosAfiliaciones.Application.Dtos
{
    public class PersonaDataDto
    {
        public long Matricula { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Celular { get; set; }
        public string Mail { get; set; }
        public string Domicilio { get; set; }
        public int IdDniAnverso { get; set; }
        public int IdDniReverso { get; set; }

        public LocalidadDataDto Localidad { get; set; }
        public AfiliacionDataDto Afiliacion { get; set; }
    }
}
