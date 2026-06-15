using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uniVerseSise.Entidades;
using uniVerseSise.Helpers;
using uniVerseSise.Manager.Filtros;

namespace uniVerseSise.Manager
{
    public class PolizasManager: Singleton<PolizasManager>
    {
        #region Constructor

        private PolizasManager() { }

        #endregion

        #region Metodos

        /// <summary>
        /// Retorna las polizas por nro de poliza, y endosos
        /// </summary>
        /// <param name="seccion"></param>
        /// <param name="nroPoliza"></param>
        /// <param name="nroEndosoMin"></param>
        /// <param name="nroEndosoMax"></param>
        /// <returns></returns>
        public List<Poliza> Obtener(Constantes.Secciones seccion, int nroPoliza, int nroEndosoMin, int nroEndosoMax)
        {
            //Busco las polizas por seccion, nroPoliza y nroEndoso
            string nrosec = seccion.GetHashCode().ToString("00");
            string nropol = (nroPoliza >= 0) ? nroPoliza.ToString("000000000") : "000000000";

            if (nroEndosoMin < 0 || nroEndosoMin > nroEndosoMax)
                throw new System.Exception("Rango de endosos inválidos");

            if (nroEndosoMax - nroEndosoMin > 200)
                throw new System.Exception("La cantidad de endosos a buscar debe ser mayor a 0");

            StringBuilder str = new StringBuilder();

            for (int i = nroEndosoMin; i <= nroEndosoMax; i++)
            {
                str.Append(nrosec + nropol + i.ToString("000000")).Append(" ");
            }

            str.Remove(str.Length - 1, 1);

            List<Poliza> polizas = TablaManager.Instance.ExecuteCommand<Poliza>(TablaManager.Comandos.SELECT, str.ToString());

            return polizas;
        }

        /// <summary>
        /// Retorna las polizas por codigo de productor
        /// </summary>
        /// <returns></returns>
        public List<Poliza> Obtener(Constantes.Secciones seccion, int codigoProductor, int maxCantidad = 100)
        {
            if (codigoProductor < 0 || codigoProductor > 99999)
                throw new Exception("El código de productor es inválido");

            List<Poliza> polizas = TablaManager.Instance.ExecuteCommandBulk<Poliza>(
                $"QSELECT GPROD {codigoProductor.ToString("00000")}" + Environment.NewLine +
                 "QSELECT GPROD" + Environment.NewLine +
                $"SELECT PV WITH @ID > {seccion.GetHashCode().ToString("00")}000000000000000 AND @ID < {(seccion.GetHashCode() + 1).ToString("00")}000000000000000" + (maxCantidad > 0 ? (" SAMPLE " + maxCantidad.ToString()) : string.Empty));

            //Filtro las polizas por seccion
            return polizas;
        }

        /// <summary>
        /// Retorna las polizas por nombre y apellido del cliente
        /// </summary>
        /// <returns></returns>
        public List<Poliza> Obtener(Constantes.Secciones seccion, string apellidoCliente, string nombreCliente = null, int maxCantidad = 100)
        {
            if (string.IsNullOrWhiteSpace(apellidoCliente))
                throw new System.Exception("El apellido no es válido");

            string nc = apellidoCliente.Replace("\"", string.Empty).ToUpper();

            if (!string.IsNullOrWhiteSpace(nombreCliente))
                nc += " " + nombreCliente.Replace("\"", string.Empty).ToUpper();

            List<Poliza> polizas = TablaManager.Instance.ExecuteCommandBulk<Poliza>(
                $"SELECT MASEG WITH NOMBRE.NDX = \"[{nc}]\"" + Environment.NewLine +
                "QSELECT GASEG" + Environment.NewLine +
                "QSELECT GASEG" + Environment.NewLine +
                $"SELECT PV WITH @ID > {seccion.GetHashCode().ToString("00")}000000000000000 AND @ID < {(seccion.GetHashCode() + 1).ToString("00")}000000000000000" + (maxCantidad > 0 ? (" SAMPLE " + maxCantidad.ToString()) : string.Empty));

            //Filtro las polizas por seccion
            return polizas;
        }

        /// <summary>
        /// Retorna las polizas por filtros genericos
        /// </summary>
        /// <returns></returns>
        public List<Poliza> Obtener(Constantes.Secciones seccion, FiltroPoliza filtro)
        {
            #region Validaciones

            if (filtro.CodigoProductor.HasValue)
            {
                if (filtro.CodigoProductor.Value < 0 || filtro.CodigoProductor.Value > 99999)
                    throw new Exception("El código de productor es inválido");
            }

            if (filtro.FechaEmisionRangoInicio.HasValue)
            {
                if (filtro.FechaEmisionRangoInicio.Value < TablaManager.BaseDate)
                    filtro.FechaEmisionRangoInicio = TablaManager.BaseDate;
            }

            if (filtro.FechaEmisionRangoFin.HasValue)
            {
                if (filtro.FechaEmisionRangoFin.Value < TablaManager.BaseDate)
                    filtro.FechaEmisionRangoFin = TablaManager.BaseDate;

                if (filtro.FechaEmisionRangoInicio.HasValue && filtro.FechaEmisionRangoInicio.Value > filtro.FechaEmisionRangoFin.Value)
                    throw new Exception("El rango de fecha de emisión es inválido");
            }

            if (filtro.FechaFinVigenciaRangoInicio.HasValue)
            {
                if (filtro.FechaFinVigenciaRangoInicio.Value < TablaManager.BaseDate)
                    filtro.FechaFinVigenciaRangoInicio = TablaManager.BaseDate;
            }

            if (filtro.FechaFinVigenciaRangoFin.HasValue)
            {
                if (filtro.FechaFinVigenciaRangoFin.Value < TablaManager.BaseDate)
                    filtro.FechaFinVigenciaRangoFin = TablaManager.BaseDate;

                if (filtro.FechaFinVigenciaRangoInicio.HasValue && filtro.FechaFinVigenciaRangoInicio.Value > filtro.FechaFinVigenciaRangoFin.Value)
                    throw new Exception("El rango de fecha de fin de vigencia es inválido");
            }

            #endregion

            #region Constructor del query
            StringBuilder strQuery = new StringBuilder();

            if (filtro.CodigoProductor.HasValue)
            {
                strQuery.AppendLine($"QSELECT GPROD {filtro.CodigoProductor.Value.ToString("00000")}");
                strQuery.AppendLine("QSELECT GPROD");
            }

            strQuery.Append($"SELECT PV WITH @ID > {seccion.GetHashCode().ToString("00")}000000000000000 AND @ID < {(seccion.GetHashCode() + 1).ToString("00")}000000000000000");

            //Filtros de Rangos
            if (filtro.FechaEmisionRangoInicio.HasValue)
                strQuery.Append($" AND FEC.EMI >= {filtro.FechaEmisionRangoInicio.Value.ToString("dd/MM/yyyy") }");

            if (filtro.FechaEmisionRangoFin.HasValue)
                strQuery.Append($" AND FEC.EMI <= {filtro.FechaEmisionRangoInicio.Value.ToString("dd/MM/yyyy") }");

            if (filtro.FechaFinVigenciaRangoInicio.HasValue)
                strQuery.Append($" AND VIG.HAS >= {filtro.FechaFinVigenciaRangoInicio.Value.ToString("dd/MM/yyyy") }");

            if (filtro.FechaFinVigenciaRangoFin.HasValue)
                strQuery.Append($" AND VIG.HAS <= {filtro.FechaFinVigenciaRangoFin.Value.ToString("dd/MM/yyyy") }");

            if (filtro.MaximaCantidad > 0)
                strQuery.Append(" SAMPLE " + filtro.MaximaCantidad.ToString());

            #endregion

            List<Poliza> polizas = TablaManager.Instance.ExecuteCommandBulk<Poliza>(strQuery.ToString());

            return polizas;
        }

        #endregion
    }
}
