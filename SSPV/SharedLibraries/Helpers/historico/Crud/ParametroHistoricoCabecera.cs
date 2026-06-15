using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Crud
{
    public class ParametroHistoricoCabecera
    {
        private static ParametroHistoricoCabecera _instance;

        public static ParametroHistoricoCabecera Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ParametroHistoricoCabecera();
                return _instance;
            }
        }

        public ParametroHistoricoCabecera()
        {
            //
        }

        public List<Entities.ParametroHistoricoCabecera> GetAllParametroHistoricoCabecera()
        {
            List<Entities.ParametroHistoricoCabecera> resultado = new List<Entities.ParametroHistoricoCabecera>();
            string sSql = string.Empty;
            sSql = @"   SELECT
                        c.IdCabecera,
                        c.NombreTablaCabecera,
                        c.NombreClaveEdad,
                        c.rangoClaveEdad
                        FROM
                        PARAMETROS_HISTORICO_CABECERA c
                        ORDER BY 1 ";
            resultado = DataModel.Instance.Execute<Entities.ParametroHistoricoCabecera>(sSql);
            foreach (var itemCabecera in resultado)
            {
                if(!Char.IsNumber(itemCabecera.rangoClaveEdad,0))
                {
                    var ph = Crud.ParametroHistorico.Instance.GetParametroHistoricoByClave(itemCabecera.rangoClaveEdad).FirstOrDefault();
                    if (ph != null && !string.IsNullOrEmpty(ph.Clave))
                        itemCabecera.rangoClaveEdad = ph.Valor;
                    else
                        itemCabecera.rangoClaveEdad = "30";
                }
            }
            return resultado;
        }

        public Entities.ParametroHistoricoCabecera GetAllParametroHistoricoCabeceraDeUnaTabla(string nombreTabla)
        {
            List<Entities.ParametroHistoricoCabecera> resultado = new List<Entities.ParametroHistoricoCabecera>();
            string sSql = string.Empty;
            sSql += @"  SELECT
                        c.IdCabecera,
                        c.NombreTablaCabecera,
                        c.NombreClaveEdad, 
                        c.rangoClaveEdad 
                        FROM 
                        PARAMETROS_HISTORICO_CABECERA c
                        WHERE c.NombreTablaCabecera = '"+ nombreTabla + "'" + 
                        "ORDER BY 1 ";
            resultado = DataModel.Instance.Execute<Entities.ParametroHistoricoCabecera>(sSql);
            foreach (var itemCabecera in resultado)
            {
                if (!Char.IsNumber(itemCabecera.rangoClaveEdad, 0))
                {
                    var ph = Crud.ParametroHistorico.Instance.GetParametroHistoricoByClave(itemCabecera.rangoClaveEdad).FirstOrDefault();

                    if (ph != null && !string.IsNullOrEmpty(ph.Clave))
                        itemCabecera.rangoClaveEdad = ph.Valor;
                    else
                        itemCabecera.rangoClaveEdad = "30";
                }
            }
            return resultado.FirstOrDefault() ;
        }

        public List<int> GetAllIdTablaCabecera(string nombreTabla, string sSql_Where)
        {
            try
            {
            string sSql = string.Empty;
            List<int> result = new List<int>();
            string idPrimaryKey = string.Empty;

            List<Entities.EstructuraTabla>  listaColumnas = Crud.EstructuraTabla.Instance.GetEstructurasTablasByTabla(nombreTabla);
            var columnaPK = listaColumnas.Where(x => x.IS_PRIMARY_KEY == "YES").FirstOrDefault();

            idPrimaryKey = columnaPK.COLUMN_NAME;

            sSql = "SELECT";
            sSql += Environment.NewLine + idPrimaryKey; //Id tabla
            sSql += Environment.NewLine + " FROM " + nombreTabla;
            sSql += Environment.NewLine + " where " + sSql_Where;
            sSql += Environment.NewLine + " ORDER BY 1 ";

            var dataTable = DataModel.Instance.Execute(sSql);
            foreach (DataRow dr in dataTable.Rows)
                result.Add((int)dr[idPrimaryKey]);
            return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
