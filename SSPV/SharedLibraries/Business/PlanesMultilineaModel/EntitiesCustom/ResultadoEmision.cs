using PlanesMultilinea.Entities;

namespace PlanesMultilinea.EntitiesCustom
{
    public class ResultadoEmision
    {
        public bool EmisionCorrecta { get; set; }
        public string Error { get; set; }
        public DatosEmsionSistemaAdicional Sistema { get; set; }
        public CertificadoPoliza CertificadoPoliza { get; set; }

    }
}
