using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Texto de la sección de la poliza
    /// </summary>
    [NombreArchivo(Archivo = "TEXTO.PV")]
    public class PolizaSeccionTexto
    {
        /// <summary>
        /// Campo ID: 2x Sección + 9x Póliza + 6x Endoso
        /// </summary>
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        /// <summary>
        /// Campo 1
        /// </summary>
        [DatoCampo(Text = true)]
        public string Descripcion { get; set; }
    }
}
