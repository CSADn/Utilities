using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers
{
    public abstract class CrudManagerCustomDT<T, C, D, DT> : CrudManagerCustom<T, C, D>
        where T : class
        where C : class, new()
        where D : class, new()
        where DT: class, new()
    {

        #region Elementos Privados

        /// <summary>
        /// Propiedades a desencriptar
        /// </summary>
        private List<Expression<Func<DT, object>>> _cryptProps = null;
        private List<Expression<Func<C, object>>> _customCryptProps = null;

        #endregion

        #region Metodos Protegidos

        protected List<Expression<Func<E, object>>> GetCryptProps<E>()
        {
            List<Expression<Func<E, object>>> retList = null;

            if (CryptProps == null || CryptProps.Count() == 0)
                return retList;
            else
            {
                PropertyInfo[] propsDT = typeof(E).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (propsDT == null || propsDT.Count() == 0)
                    throw new Exception($"No se han obtenido propiedades para {typeof(E).Name}");

                CryptProps.ForEach(cp =>
                {
                    Expression expre = null;
                    PropertyInfo prop = propsDT.FirstOrDefault(f => f.Name.ToLowerInvariant() == GetMemberPropertyName(cp));
                    if (prop != null)
                    {
                        ParameterExpression parameter = Expression.Parameter(typeof(T), "s");
                        MemberExpression me = Expression.MakeMemberAccess(parameter, prop);
                        expre = Expression.Lambda(me, parameter);

                        retList.Add((Expression<Func<E, object>>)expre);
                    }
                });

                return retList;
            }
        }

        #endregion

        #region Paginado

        /// <summary>
        /// Obtener todo paginado para las datatables
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="search"></param>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        public virtual List<DT> GetAllDT(int pageNumber, int rowsPerPage, string search, params string[] orderFields)
        {
            List<object> parametros = new List<object>() { pageNumber, pageNumber, rowsPerPage };
            List<DT> retList = null;

            string sortFields = GetOrderingForPaging(orderFields); //Orden
            string fields = GetFieldForPaging<DT>("Total_Rows", "Filtered_Rows"); //Campos
            string searhCond = string.Empty; //Condiciones de Busqueda
            string fixedFilter = string.Empty; //Filtros fijos

            _cryptProps = GetCryptProps<DT>(); //Obtengo si hay campos para desencriptar

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                fixedFilter = @" WHERE " + where.Where;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging(search, where.Where, new string[] { "Total_Rows", "Filtered_Rows" }, _cryptProps);

                    searhCond = @" AND " + sc.Where;
                    parametros = where.Parameters.Concat(sc.Parameters.Concat(where.Parameters).Concat(parametros)).ToList();
                }
                else
                    parametros = where.Parameters.Concat(where.Parameters.Concat(parametros)).ToList();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging(search, null, new string[] { "Total_Rows", "Filtered_Rows" }, _cryptProps);

                    searhCond = @" WHERE " + sc.Where;
                    parametros = sc.Parameters.Concat(parametros).ToList();
                }
            }

            //Traerá el count de todos los registros (unicamente filtrará por los fixed filters
            string countSQL = @"SELECT COUNT(1) AS [TOTAL_ROWS] FROM #TABLE_NAME# #FIXED_FILTER#".Replace("#TABLE_NAME#", TableName)
                                                                                                 .Replace("#FIXED_FILTER#", fixedFilter);

            //Query Principal
            string mainSQL = @"SELECT
                                    COUNT(1) OVER() AS [FILTERED_ROWS],
                                    ROW_NUMBER() OVER(ORDER BY #SORT_FIELDS#) AS [R_NUMBER],
                                    #FIELDS#
								 FROM #TABLE_NAME#
                       #FIXED_FILTER##SEARCH_COND#".Replace("#TABLE_NAME#", TableName)
                                                   .Replace("#SORT_FIELDS#", sortFields)
                                                   .Replace("#FIELDS#", fields)
                                                   .Replace("#FIXED_FILTER#", fixedFilter)
                                                   .Replace("#SEARCH_COND#", searhCond);

            //Query final haciendo crossjoin entre el total y el query principal
            string resultSQL = @"SELECT [TOTAL_ROWS],
                                        [FILTERED_ROWS],
                                        #FIELDS#
                                   FROM (#MAINQUERY#) AS R2 CROSS JOIN (#COUNTQUERY#) AS TOTAL
                                  WHERE R2.R_NUMBER > ? AND R2.R_NUMBER <= (? + ?)
                                  ORDER BY #SORT_FIELDS#".Replace("#MAINQUERY#", mainSQL)
                                                         .Replace("#COUNTQUERY#", countSQL)
                                                         .Replace("#FIELDS#", fields)
                                                         .Replace("#SORT_FIELDS#", sortFields);

            retList = Execute<DT>(resultSQL, parametros.ToArray());

            if (retList != null && retList.Count > 0 && _cryptProps != null && _cryptProps.Count > 0)
                return retList.EntityCypher(Utilities.CypherAction.Decrypt, _cryptProps.ToArray());

            return retList;
        }

        public virtual List<DT> GetAllDTCustom(int pageNumber, int rowsPerPage, string search, params string[] orderFields)
        {
            List<object> parametros = new List<object>() { pageNumber, pageNumber, rowsPerPage };
            List<DT> retList = null;

            string sortFields = GetOrderingForPaging(orderFields); //Orden
            string fields = GetFieldForPaging<C>(); //Campos
            string searhCond = string.Empty; //Condiciones de Busqueda
            string fixedFilter = string.Empty; //Filtros fijos

            _customCryptProps = GetCryptProps<C>(); //Obtengo si hay campos para desencriptar

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                fixedFilter = @" WHERE " + where.Where;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging(search, where.Where, null, _customCryptProps);

                    searhCond = @" AND " + sc.Where;
                    parametros = where.Parameters.Concat(sc.Parameters.Concat(where.Parameters).Concat(parametros)).ToList();
                }
                else
                    parametros = where.Parameters.Concat(where.Parameters.Concat(parametros)).ToList();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging(search, null, null, _customCryptProps);

                    searhCond = @" WHERE " + sc.Where;
                    parametros = sc.Parameters.Concat(parametros).ToList();
                }
            }

            //Traerá el count de todos los registros (unicamente filtrará por los fixed filters
            string countSQL = @"SELECT COUNT(1) AS [TOTAL_ROWS] FROM #TABLE_NAME# #FIXED_FILTER#".Replace("#TABLE_NAME#", TableName)
                                                                                                 .Replace("#FIXED_FILTER#", fixedFilter);

            //Query Principal
            string mainSQL = @"SELECT
                                    COUNT(1) OVER() AS [FILTERED_ROWS],
                                    ROW_NUMBER() OVER(ORDER BY #SORT_FIELDS#) AS [R_NUMBER],
                                    #FIELDS#
								 FROM #TABLE_NAME#
                       #FIXED_FILTER##SEARCH_COND#".Replace("#TABLE_NAME#", TableName)
                                                   .Replace("#SORT_FIELDS#", sortFields)
                                                   .Replace("#FIELDS#", fields)
                                                   .Replace("#FIXED_FILTER#", fixedFilter)
                                                   .Replace("#SEARCH_COND#", searhCond);

            //Query final haciendo crossjoin entre el total y el query principal
            string resultSQL = @"SELECT [TOTAL_ROWS],
                                        [FILTERED_ROWS],
                                        #FIELDS#
                                   FROM (#MAINQUERY#) AS R2 CROSS JOIN (#COUNTQUERY#) AS TOTAL
                                  WHERE R2.R_NUMBER > ? AND R2.R_NUMBER <= (? + ?)
                                  ORDER BY #SORT_FIELDS#".Replace("#MAINQUERY#", mainSQL)
                                                         .Replace("#COUNTQUERY#", countSQL)
                                                         .Replace("#FIELDS#", fields)
                                                         .Replace("#SORT_FIELDS#", sortFields);

            retList = Execute<DT>(resultSQL, parametros.ToArray());

            //if (retList != null && retList.Count > 0 && _customCryptProps != null && _customCryptProps.Count > 0)
            //    return retList.EntityCypher<DT>(Utilities.CypherAction.Decrypt, _customCryptProps.ToArray());

            if (retList != null && retList.Count > 0)
                retList.ForEach(f => FillCustomProperties(f));

            return retList;
        }

        #endregion

        #region Metodos Abstractos

        protected virtual void FillCustomProperties(DT entityCustom) { }

        #endregion
    }
}
