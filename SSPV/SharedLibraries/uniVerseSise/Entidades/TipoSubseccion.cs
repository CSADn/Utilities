using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Campo ID y Text
    /// TSUBSEC
    /// </summary>
    [NombreArchivo(Archivo = "TSUBSEC")]
    public class TipoSubseccion
    {
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        [DatoCampo(Text = true)]
        public string Descripcion { get; set; }
    }
}
