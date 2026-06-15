using System;
using System.Collections.Generic;
using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    [NombreArchivo(Archivo = "PVC.H")]
    public class NominaSubgrupo
    {
        /// <summary>
        /// 2X Secc + 9X Pol + 6X End + 3X Subrupo + 4X Nro.Correlativo
        /// </summary>
        [DatoCampo(Id = true)]
        public string Id { get; set; }

        [DatoCampo(1)]
        public string Descripcion { get; set; }

        [DatoCampo(3, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 })]
        public List<int> CodigosCoberturas { get; set; }

        [DatoCampo(4, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 })]
        public List<double> SumasCoberturas { get; set; }

    }
}
