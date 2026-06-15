using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    [NombreArchivo(Archivo = "TEXTO.DI")]
    public class PolizaSeccionItemTexto
    {
        /// <summary>
        /// Campo ID: 2x Sección + 9x Póliza + 6x Endoso + 3 Item de Cobertura
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
