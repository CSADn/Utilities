using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanesMultilinea.Entities
{
    public class DatosImpresion
    {
        /// <summary>
        /// Codigo del productor
        /// </summary>
        public string CodProductor { get; set; }

        /// <summary>
        /// Numero del Documento de poliza/certificado
        /// </summary>
        public int NroDocumento { get; set; }

        /// <summary>
        /// Indica si es poliza o Certificado
        /// </summary>
        public bool EsPoliza { get; set; }

        public DatosImpresion()
        {
            //
        }
    }
}
