using System;
using System.Collections.Generic;
using uniVerseSise.Configuracion;

namespace uniVerseSise.Entidades
{
    /// <summary>
    /// Poliza
    /// Almacenamiento: PV
    /// </summary>
    [NombreArchivo(Archivo = "PV")]
    public class Poliza
    {
        #region Campos del Archivo PV

        /// <summary>
        /// Identificador de la poliza
        /// </summary>
        [DatoCampo(true)]
        public string Id { get; set; }

        /// <summary>
        /// La seccion son los primeros 2 numeros del ID
        /// </summary>
        public int Seccion
        {
            get
            {
                return Convert.ToInt32(Convert.ToUInt64(Id) / 1000000000000000);
            }
        }

        /// <summary>
        /// La poliza son los 9 numeros siguientes al ID
        /// </summary>
        public int NroPoliza
        {
            get
            {
                return Convert.ToInt32((Convert.ToUInt64(Id) % 1000000000000000) / 1000000);
            }
        }

        /// <summary>
        /// El endoso son los ultimos 6 numeros del ID
        /// </summary>
        public int NroEndoso
        {
            get
            {
                return Convert.ToInt32(Convert.ToUInt64(Id) % 1000000);
            }
        }

        [DatoCampo(71)]
        public int? TipoEndoso { get; set; }

        [DatoCampo(3)]
        public DateTime? FechaEmision { get; set; }

        [DatoCampo(4)]
        public DateTime? VigenciaDesde { get; set; }

        [DatoCampo(5)]
        public DateTime? VigenciaHasta { get; set; }

        [DatoCampo(66)]
        public int? TipoSubseccion { get; set; }

        [DatoCampo(87)]
        public int? Agencia { get; set; }

        [DatoCampo(1)]
        public string CodigoAsegurado { get; set; }

        [DatoCampo(47, 1)]
        public string CodigoProductor { get; set; }

        [DatoCampo(47, new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 })]
        public List<string> Organizadores { get; set; }

        [DatoCampo(49, 1)]
        public double? ComisionProductor { get; set; }

        [DatoCampo(49, new int[] { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 })]
        public List<double> ComisionOrganizadores { get; set; }

        [DatoCampo(37)]
        public double? PremioEmitido { get; set; }

        [DatoCampo(116)]
        public double? PremioEmitidoMonedaExtranjera { get; set; }

        [DatoCampo(23)]
        public double? PrimaEmitida { get; set; }

        [DatoCampo(103)]
        public double? PrimaEmitidaMonedaExtranjera { get; set; }

        //II.BB PV<228, x>	dos decimales. Se adjunta comentario
        public double? ImporteIIBB { get; set; }

        //II.BB Moneda Extranjera PV<232, x>	dos decimales        
        public double? ImporteIIBBMonedaExtranjera { get; set; }

        [DatoCampo(170)]
        public double? ImporteIVA { get; set; }

        [DatoCampo(172)]
        public double? ImporteIVAMonedaExtranjera { get; set; }

        [DatoCampo(177)]
        public double? ImporteRG_2408_08 { get; set; }

        [DatoCampo(178)]
        public double? ImporteRG_2408_08_MonedaExtranjera { get; set; }

        [DatoCampo(27)]
        public double? ImporteDerechoEmision { get; set; }

        [DatoCampo(106)]
        public double? ImporteDerechoEmisionMonedaExtranjera { get; set; }

        [DatoCampo(28)]
        public double? ImporteRecargoFinanciero { get; set; }

        [DatoCampo(108)]
        public double? ImporteRecargoFinancieroMonedaExtranjera { get; set; }

        [DatoCampo(30)]
        public double? ImporteImpuestosInternos { get; set; }

        [DatoCampo(109)]
        public double? ImporteImpuestosInternosMonedaExtranjera { get; set; }

        [DatoCampo(32)]
        public double? ImporteOOSEG { get; set; }

        [DatoCampo(111)]
        public double? ImporteOOSEGMonedaExtranjera { get; set; }

        [DatoCampo(31)]
        public double? ImporteSSN { get; set; }

        [DatoCampo(110)]
        public double? ImporteSSNMonedaExtranjera { get; set; }

        [DatoCampo(33)]
        public double? ImporteSellados { get; set; }

        [DatoCampo(112)]
        public double? ImporteSelladosExtranjera { get; set; }

        [DatoCampo(8)]
        public string TipoMoneda { get; set; }

        //Código del conducto de pago PV<194,1>	FK s/CONDUCTOS
        [DatoCampo(194, 1)]
        public string CodigoConductoPago { get; set; }

        [DatoCampo(212)]
        public int CantidadPersonasNomina { get; set; }

        #endregion

        #region Campos Extras

        public Cliente Cliente { get; set; }

        public List<Nomina> Nomina { get; set; }

        public List<PolizaCobertura> PolizasCoberturas { get; set; }

        public List<NominaSubgrupo> NominaSubgrupos { get; set; }

        public string TextoPoliza { get; set; }

        #endregion

    }
}
