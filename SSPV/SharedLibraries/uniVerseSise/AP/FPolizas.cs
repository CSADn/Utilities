using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using uniVerseSise.DBManager;
using uniVerseSise.Entidades;
using uniVerseSise.Manager;
using uniVerseSise.Manager.Filtros;

namespace uniVerseSise.AP
{
    /// <summary>
    /// Facade de polizas
    /// </summary>
    public static class FPolizas
    {
        #region Metodos publicos

        public static List<Poliza> ObtenerEndosos(int nroPoliza, int nroEndoso = -1)
        {
            return ObtenerEndososImpl(() => 
            {
                int endosoDesde = 0;
                int endosoHasta = 100;

                if (nroEndoso >= 0)
                    endosoDesde = endosoHasta = nroEndoso;

                List<Poliza> polizas = PolizasManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP, nroPoliza, endosoDesde, endosoHasta);
                return polizas;
            });
        }

        public static List<Poliza> ObtenerEndosos(string apellidoCliente, string nombreCliente = null, int maxCantidad = 5)
        {
            return ObtenerEndososImpl(() =>
            {
                List<Poliza> polizas = PolizasManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP, apellidoCliente, nombreCliente, maxCantidad);
                return polizas;
            });
        }

        public static List<Poliza> ObtenerEndososPorProductor(int codigoProductor, int maxCantidad = 5)
        {
            return ObtenerEndososImpl(() =>
            {
                List<Poliza> polizas = PolizasManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP, codigoProductor, maxCantidad);
                return polizas;
            });
        }

        public static List<Poliza> ObtenerEndosos(FiltroPoliza filtro)
        {
            return ObtenerEndososImpl(() =>
            {
                List<Poliza> polizas = PolizasManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP, filtro);
                return polizas;
            });
        }

        //Para test
        public static async Task<List<Poliza>> ObtenerEndososAsync(int nroPoliza)
        {
            Task<List<Poliza>> retValue = new Task<List<Poliza>>(() => ObtenerEndosos(nroPoliza));
            retValue.Start();
            return await retValue;
        }

        #endregion

        #region Metodos privados

        /// <summary>
        /// Retorna los endoso de AP
        /// </summary>
        /// <param name="nroPoliza"></param>
        /// <returns></returns>
        private static List<Poliza> ObtenerEndososImpl(Func<List<Poliza>> polizasCallback)
        {
            using (ConexionUV cnx = ConexionUV.CrearConexion())
            {
                List<Poliza> polizas = polizasCallback();

                List<Cliente> clienteCache = new List<Cliente>();
                //Vale la pena hacer caches de otros elementos?

                foreach (var poliza in polizas.OrderBy(p => p.NroEndoso))
                {
                    #region Cliente

                    //Busco el cliente
                    if (!clienteCache.Any(c => c.Id == poliza.CodigoAsegurado))
                        clienteCache.Add(ClientesManager.Instance.Obtener(poliza.CodigoAsegurado));

                    poliza.Cliente = clienteCache.First(c => c.Id == poliza.CodigoAsegurado);

                    #endregion

                    #region Nomina

                    //Busco la nomina para la poliza
                    if (poliza.CantidadPersonasNomina > 0)
                    {
                        poliza.Nomina = NominaManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP,
                                                poliza.NroPoliza,
                                                poliza.NroEndoso, 0,
                                                poliza.CantidadPersonasNomina);
                    }
                    else
                        poliza.Nomina = new List<Nomina>();

                    #endregion

                    #region Coberturas

                    //Busco los detalles de la cobertura
                    poliza.PolizasCoberturas = PolizasCoberturasManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP,
                                                    poliza.NroPoliza,
                                                    poliza.NroEndoso);

                    //Si tiene coberturas, busco sus detalles
                    if (poliza.PolizasCoberturas != null)
                    {
                        for (int i = 1; i < poliza.PolizasCoberturas.Count + 1; i++)
                        {
                            PolizaSeccionItemTexto textoPolizaItem = LookupsManager.ObtenerPolizaSeccionTextoItem(Constantes.Secciones.AP, poliza.NroPoliza, poliza.NroEndoso, i);
                            if (textoPolizaItem != null)
                                poliza.PolizasCoberturas[i - 1].TextoPolizaItem = textoPolizaItem.Descripcion;
                        }
                    }

                    #endregion

                    #region Subgrupos Nomina

                    poliza.NominaSubgrupos = new List<NominaSubgrupo>();

                    if (poliza.Nomina.Count > 0)
                    {
                        for (int i = 1; i < poliza.Nomina.Count; i++)
                        {
                            poliza.NominaSubgrupos.AddRange(
                                NominaSubgruposManager.Instance.Obtener(uniVerseSise.Constantes.Secciones.AP,
                                            poliza.NroPoliza,
                                            poliza.NroEndoso,
                                            i)
                                        );
                        }
                    }

                    #endregion

                    #region Texto Nomina

                    PolizaSeccionTexto textoPoliza = LookupsManager.ObtenerPolizaSeccionTexto(Constantes.Secciones.AP, poliza.NroPoliza, poliza.NroEndoso);
                    if (textoPoliza != null)
                        poliza.TextoPoliza = textoPoliza.Descripcion;

                    #endregion
                }

                cnx.Desconectar();

                return polizas;
            }
        }
        #endregion
    }
}
