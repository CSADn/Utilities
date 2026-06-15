using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Tipo Sucursal
    /// </summary>
    [NombreArchivo(Archivo = "TSUC")]
    public class Sucursal
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
