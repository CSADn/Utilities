using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Detalle de cobertura de la poliza
    /// </summary>
    [NombreArchivo(Archivo = "DI")]
    public class PolizaCobertura
    {
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        [DatoCampo(47)]
        public double? SumaAseguradaPorPersona { get; set; }
        
        //Tasa aplicada  OJO: 3 decimales
        [DatoCampo(10)]
        public double? TasaAplicada { get; set; }

        [DatoCampo(11)]
        public double? PrimaTotal { get; set; }

        [DatoCampo(1)]
        public string UbicacionRiesgo { get; set; }

        #region Campos Extras

        public string TextoPolizaItem { get; set; }

        #endregion

    }
}
