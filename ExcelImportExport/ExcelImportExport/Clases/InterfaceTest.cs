using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelImportExport.Clases
{
    [Sheet("interface")]
    public class InterfaceTest
    {
        [HeaderCustom(0, "Sc", Enums.CellType.String)]
        public string SC { get; set; }
        [HeaderCustom(1, "Siniestro", Enums.CellType.Numeric)]
        public int Siniestro { get; set; }
        [HeaderCustom(2, "Item", Enums.CellType.Numeric)]
        public int Item { get; set; }
        [HeaderCustom(3, "SST", Enums.CellType.Numeric)]
        public int SST { get; set; }
        [HeaderCustom(4, "Poliza...", Enums.CellType.Numeric)]
        public int Poliza { get; set; }
        [HeaderCustom(5, "Endoso", Enums.CellType.Numeric)]
        public int Endoso { get; set; }
        [HeaderCustom(6, "MANAGER.CODE", Enums.CellType.String)]
        public string ManagerCode { get; set; }
        [HeaderCustom(7, "Nombre del Asegurado", Enums.CellType.String)]
        public string NombreAsegurado { get; set; }
        [HeaderCustom(8, "Nombre Productor........................", Enums.CellType.String)]
        public string NombreProductor { get; set; }
        [HeaderCustom(9, "PROPIETARIO ESTAD.................................", Enums.CellType.String)]
        public string PropietarioEstado { get; set; }
        [HeaderCustom(10, "MARCA UNIDAD............................", Enums.CellType.String)]
        public string MarcaUnidad { get; set; }
        [HeaderCustom(11, "PATENTE...", Enums.CellType.String)]
        public string Patente { get; set; }
        [HeaderCustom(12, "MOTOR.............................................", Enums.CellType.String)]
        public string Notor { get; set; }
        [HeaderCustom(13, "GNC", Enums.CellType.String)]
        public string Gnc { get; set; }
        [HeaderCustom(14, "CHASSIS.............", Enums.CellType.String)]
        public string Chasis { get; set; }
        [HeaderCustom(15, "COLOR...............", Enums.CellType.String)]
        public string Color { get; set; }
        [HeaderCustom(16, "MODELO", Enums.CellType.Numeric)]
        public string Modelo { get; set; }
        [HeaderCustom(17, "Ocurrido..", Enums.CellType.Numeric, "m/d/yy")]
        public DateTime Ocurrido { get; set; }
        [HeaderCustom(18, "HORA..", Enums.CellType.Numeric, "h:mm:ss")]
        public DateTime Hora { get; set; }
        [HeaderCustom(19, "ENTRADA...", Enums.CellType.Numeric, "m/d/yy")]
        public DateTime Entrada { get; set; }
        [HeaderCustom(20, "Primera Estimación", Enums.CellType.Numeric, "m/d/yy")]
        public DateTime PrimeraEstimacion { get; set; }
        [HeaderCustom(21, "SOLICITUD", Enums.CellType.String)]
        public long Solicitud { get; set; }
        [HeaderCustom(22, "Fec.Ing.Solic.", Enums.CellType.Numeric, "m/d/yy")]
        public DateTime FechaIngSolicitud { get; set; }
        [HeaderCustom(23, "LOCALIDAD.........................................", Enums.CellType.String)]
        public string Localida { get; set; }
        [HeaderCustom(24, "COD.POSTAL", Enums.CellType.Numeric)]
        public int CodPostal { get; set; }
        [HeaderCustom(25, "PROVINCIA.....................", Enums.CellType.String)]
        public string Provincia { get; set; }
        [HeaderCustom(26, "COMISARIA.........................................", Enums.CellType.String)]
        public string Comisaria { get; set; }
        [HeaderCustom(27, "JUZGADO.................................", Enums.CellType.String)]
        public string Juzgado { get; set; }
        [HeaderCustom(28, "codigo liq interno", Enums.CellType.Numeric)]
        public int CodigoLiqIntero { get; set; }
        [HeaderCustom(29, "LIQUIDADOR INTERNO", Enums.CellType.String)]
        public string LiquidadorInterno { get; set; }
        [HeaderCustom(30, "LIQ. EXTERNO......................................", Enums.CellType.String)]
        public string LiquidadorExterno { get; set; }
        [HeaderCustom(31, "ALARMA...........", Enums.CellType.String)]
        public string Alarma { get; set; }
        [HeaderCustom(32, "ULT.ESTIM $...", Enums.CellType.Numeric, "#,##0.00")]
        public double UltimaEstimacion { get; set; }
        [HeaderCustom(33, "PAGOS BRUTOS ML", Enums.CellType.Numeric, "#,##0.00")]
        public double PagosBrutosML { get; set; }
        [HeaderCustom(34, "Moneda", Enums.CellType.Numeric)]
        public int Moneda { get; set; }
        [HeaderCustom(35, "ULT.ESTIM ME..", Enums.CellType.Numeric)]
        public int UltimaEstimacionME { get; set; }
        [HeaderCustom(36, "PAGOS BRUTOS ME", Enums.CellType.Numeric)]
        public int PagosBrutosME { get; set; }
        [HeaderCustom(37, "Cobertura", Enums.CellType.Numeric)]
        public int Cobertura { get; set; }
        [HeaderCustom(38, "DESCRIP. COBERTURA EMIS", Enums.CellType.String)]
        public string DescripcionCobertura { get; set; }
        [HeaderCustom(39, "Estado", Enums.CellType.Numeric)]
        public int Estado { get; set; }
        [HeaderCustom(40, "SUMA ITEM $...", Enums.CellType.Numeric, "#,##0.00")]
        public double SumaItem { get; set; }
        [HeaderCustom(41, "SUMA ITEM ME..", Enums.CellType.Numeric, "#,##0.00")]
        public double SumaItemME { get; set; }
        [HeaderCustom(42, "Codigo Productor", Enums.CellType.Numeric)]
        public int CodigoProductor { get; set; }
        [HeaderCustom(43, "Nombre Productor", Enums.CellType.String)]
        public string NombreProductor2 { get; set; }
        [HeaderCustom(44, "Codigo Organizador", Enums.CellType.Numeric)]
        public int CodigoOrganizador { get; set; }
        [HeaderCustom(45, "Nombre Organizador", Enums.CellType.String)]
        public string NombreOrganizador { get; set; }
        [HeaderCustom(46, " Clave stro ", Enums.CellType.String,"", "", true)]
        public string ClaveSiniestro { get; set; }
    }                 
}                     
