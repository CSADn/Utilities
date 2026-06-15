using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packs.Entities
{
    public class CertificadoPoliza
    {
        public int IdSolicitud { get; set; }
        public int IdCotizacion { get; set; }
        public int Nro { get; set; }
        public int Endoso { get; set; }
        public double PrimaTotal { get; set; }
        public double PremioTotal { get; set; }
        public int CanCuotas { get; set; }
        public double ValorCuota { get; set; }
        public DateTime VigenciaDesde { get; set; }
        public DateTime VigenciaHasta { get; set; }
        public double PorcComision { get; set; }
        public double PorcComisionSubproductor { get; set; }
        public double PorcOrganizador { get; set; }
        public double ComisionProd { get; set; }
        public double ComisionSubprod { get; set; }
        public double ComisionOrg { get; set; }

        public bool EsPoliza { get; set; }


        public Impuestos Impuestos;
    }
}
