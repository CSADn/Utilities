using System;
using System.Collections.Generic;
using System.Linq;
using Exceptions;
using Helpers;
using DatabaseModel;
using Entities;

namespace historico
{
    public class TablaHistorico_ConsolidarEstructura
    {
        private static TablaHistorico_ConsolidarEstructura _instance;

        public static TablaHistorico_ConsolidarEstructura Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TablaHistorico_ConsolidarEstructura();

                return _instance;
            }
        }

        public TablaHistorico_ConsolidarEstructura()
        {
            //
        }


        #region proceso principal

        public static void iniciarConsolidacion(string nombreTablaCabecera, List<string> listaTablaProcesar)
        {
            try
            {
                conciliarEstructura(nombreTablaCabecera);
                foreach (var item_NombreTabla in listaTablaProcesar)
                {
                    conciliarEstructura(item_NombreTabla);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static bool conciliarEstructura(string nombreTabla)
        {
            // Obtener la lista de campos de tablaBase en base al nombreTabla
            List<Entities.EstructuraTabla> listaCamposdeTabla = new List<Entities.EstructuraTabla>();
            listaCamposdeTabla = Crud.EstructuraTabla.Instance.GetEstructurasTablasByTabla(nombreTabla);
            //--------------------------------------------------------------------------------
            string nombreTablaHistorico = Crud.EstructuraTabla.nombreTablaHistorico(nombreTabla);
            string nombreTablaHistoricoOld = Crud.EstructuraTabla.renombreTablaHistoricoToOld(Crud.EstructuraTabla.nombreTablaHistorico(nombreTabla));
            //--------------------------------------------------------------------------------
            if (listaCamposdeTabla == null)
                return false; // error no se encuentra la tabla indicada en la base
            //--------------------------------------------------------------------------------
            // Lista de campos de TablaHisto
            List<Entities.EstructuraTabla> listaCamposdeTablaHisto = new List<Entities.EstructuraTabla>();
            listaCamposdeTablaHisto = Crud.EstructuraTabla.Instance.GetEstructurasTablasByTabla(nombreTablaHistorico);
            //--------------------------------------------------------------------------------
            bool existeTablaHistorico = false;
            if (listaCamposdeTablaHisto.Count()>0)
            {
                if (compararTablaHistorico(nombreTabla, listaCamposdeTabla)) // Verificar estructura de tabla
                {
                    //si son de igual estructura sale true
                    return true;
                }
                else
                {
                    //Renombrar tabla historico                   
                    Crud.EstructuraTabla.executeRenombreTablaHistoricoToOld(nombreTablaHistorico, nombreTablaHistoricoOld);
                    existeTablaHistorico = true;
                }
            }
            //--------------------------------------------------------------------------------
            // Crear tabla historico
            
            Crud.EstructuraTabla.crearTablaHistorico(nombreTabla, listaCamposdeTabla);
                if (existeTablaHistorico)
                {
                // Migrar datos historico oll a historico actual
                TablaHistorico_MigrarDatos.Instance.iniciandoMigracionTablaHistoricoToNewHistorico(nombreTablaHistorico, nombreTablaHistoricoOld);
                }
            //--------------------------------------------------------------------------------
            return true;
        }
        /// <summary>
        ///compararTablaHistorico
        ///El siguiente metodo compara dos tablas en base a que las dos tengan la misma cantidad de registros
        /// </summary>
        /// a partir de recibir la lista de campos de una tabla base/maestro
        /// <param name="listaCamposdeTablaBase"></param>
        /// <returns></returns>
        private static bool compararTablaHistorico(string NombreTablaBase, List<Entities.EstructuraTabla> listaCamposdeTablaBase) //// Lista de campos de tablaBase
        {
            try
            {
                string sSql = string.Empty;
                //--------------------------------------------------------------------------------
                List<Entities.EstructuraTabla> listaCamposdeTablaHisto  = new List<Entities.EstructuraTabla>();
                if (string.IsNullOrEmpty(NombreTablaBase))
                    return false;
                string nombreTablaHistorica = Crud.EstructuraTabla.nombreTablaHistorico(NombreTablaBase);
                listaCamposdeTablaHisto = Crud.EstructuraTabla.Instance.GetEstructurasTablasByTabla(nombreTablaHistorica);
                //--------------------------------------------------------------------------------
                if (listaCamposdeTablaHisto == null)
                    return false;
                //--------------------------------------------------------------------------------
                if (listaCamposdeTablaBase.Count()!= listaCamposdeTablaHisto.Count())
                    return false;
                //--------------------------------------------------------------------------------
                bool resultado = true;

                foreach (var itemBase in listaCamposdeTablaBase)
                {
                    var itemHisto = listaCamposdeTablaHisto.Where(x => x.ORDINAL_POSITION == itemBase.ORDINAL_POSITION).FirstOrDefault();

                    if (
                        itemBase.ORDINAL_POSITION == itemHisto.ORDINAL_POSITION &&
                        itemBase.TABLE_NAME != itemHisto.TABLE_NAME &&
                        itemBase.COLUMN_NAME == itemHisto.COLUMN_NAME &&
                        (
                        
                        itemBase.DATA_TYPE != itemHisto.DATA_TYPE ||
                        itemBase.CHARACTER_MAXIMUM_LENGTH != itemHisto.CHARACTER_MAXIMUM_LENGTH ||
                        itemBase.NUMERIC_PRECISION != itemHisto.NUMERIC_PRECISION ||
                        itemBase.NUMERIC_PRECISION_RADIX != itemHisto.NUMERIC_PRECISION_RADIX ||
                        itemBase.IS_NULLABLE != itemHisto.IS_NULLABLE ||
                        itemBase.COLUMN_DEFAULT != itemHisto.COLUMN_DEFAULT ||
                        itemBase.IS_PRIMARY_KEY != itemHisto.IS_PRIMARY_KEY
                        )
                        )
                       resultado = false;
                }
                //--------------------------------------------------------------------------------
                return resultado;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        #endregion
    }
}
