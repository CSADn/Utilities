using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Tipo de endoso de Sise
    /// Archivo: TENDOSO
    /// </summary>
    [NombreArchivo(Archivo = "TENDOSO")]
    public class TipoEndoso
    {
        /// <summary>
        /// Campo ID
        /// </summary>
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        /// <summary>
        /// Campo 1
        /// </summary>
        [DatoCampo(1)]
        public string Descripcion { get; set; }
    }
}
