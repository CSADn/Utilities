using System;
using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Nomina
    /// </summary>
    [NombreArchivo(Archivo = "NOMINA")]
    public class Nomina
    {
        /// <summary>
        /// Primary key: 2X Secc + 9X Pol + 6X Endoso + 5X Item de Nomina
        /// </summary>
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        [DatoCampo(1)]
        public string NombreAsegurado { get; set; }

        [DatoCampo(2)]
        public string TipoDocumento { get; set; }

        [DatoCampo(3)]
        public string NroDocumento { get; set; }

        [DatoCampo(4)]
        public DateTime FechaNacimiento { get; set; }

        [DatoCampo(34)]
        public string CodigoActividad { get; set; }

        [DatoCampo(9)]
        public DateTime FechaAlta { get; set; }

        [DatoCampo(10)]
        public DateTime FechaBaja { get; set; }

        [DatoCampo(7)]
        public string Beneficiario { get; set; }

    }
}
