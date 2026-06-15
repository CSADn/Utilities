using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers
{
    /// <summary>
    /// Manager Generico para los CRUDs
    /// </summary>
    /// <typeparam name="T">Instancia del Manager</typeparam>
    /// <typeparam name="C">Entidad</typeparam>
    public abstract class CrudManager<T, C> : GenericSingleton<T>
        where T : class
        where C : class, new()
    {
        #region Delegados 

        protected delegate bool CheckValueConvertHandler(string value);

        #endregion

        #region Constantes

        protected static readonly string SELECT_STATEMENT = "SELECT * FROM #TABLE_NAME";

        protected static readonly string DELETE_STATEMENT = "DELETE FROM #TABLE_NAME";

        protected static readonly string UPDATE_STATEMENT = "UPDATE #TABLE_NAME SET";

        protected static readonly string COUNT_STATEMENT = "SELECT COUNT(1) FROM #TABLE_NAME";

        #endregion

        #region Atributos
        private Dictionary<Type, CheckValueConvertHandler> _dicTypeCheckValue = null;

        private Dictionary<Type, string> _dicTypeToLike = new Dictionary<Type, string>()
        {
            { typeof(byte), "#FIELD = ?"},
            { typeof(short), "CAST(#FIELD AS VARCHAR) LIKE ?"},
            { typeof(int), "CAST(#FIELD AS VARCHAR) LIKE ?"},
            { typeof(long), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            { typeof(float), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            { typeof(double), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            { typeof(decimal), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            { typeof(bool), "#FIELD = ?"},
            { typeof(string), "#FIELD LIKE ?" },
            { typeof(DateTime), "CONVERT(VARCHAR, #FIELD, 103) LIKE ?"}

            //{ typeof(byte?), "#FIELD = ?"},
            //{ typeof(short?), "CAST(#FIELD AS VARCHAR) LIKE ?"},
            //{ typeof(int?), "CAST(#FIELD AS VARCHAR) LIKE ?"},
            //{ typeof(long?), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            //{ typeof(float?), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            //{ typeof(double?), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            //{ typeof(decimal?), "CAST(#FIELD AS VARCHAR) LIKE ?" },
            //{ typeof(bool?), "#FIELD = ?"},
            //{ typeof(DateTime?), "CONVERT(VARCHAR, #FIELD, 103) LIKE ?"}
        };

        /// <summary>
        /// DataModel
        /// </summary>
        protected DataModel _dataModel = null;


        /// <summary>
        /// Nombre de la tabla
        /// </summary>
        private string _tableName = string.Empty;

        private string _connection = string.Empty;

        /// <summary>
        /// Log
        /// </summary>
        protected NLog.Logger _logger;

        #endregion

        #region Propiedades

        /// <summary>
        /// Tabla en la DB de la entidad
        /// </summary>
        protected virtual string TableName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_tableName))
                    _tableName = typeof(C).Name.ToUpperInvariant();

                return _tableName;
            }
            set
            {
                _tableName = value;
            }
        }

        /// <summary>
        /// Coneccion a la base de datos
        /// </summary>
        protected virtual string Connection
        {
            get
            {
                return _connection;
            }
            set
            {
                _connection = value;
            }
        }

        /// <summary>
        /// Lista de claves primarias
        /// </summary>
        protected virtual List<Expression<Func<C, object>>> PrimaryKeys { get; set; }

        /// <summary>
        /// Filtros fijos para las consultas
        /// </summary>
        protected virtual Expression<Func<C, bool>> FixedFilters { get; set; }

        /// <summary>
        /// Campo por el cual se aplicara el borrado lógico y su valor
        /// </summary>
        protected virtual Expression<Func<C, bool>> DeleteLogicFields { get; set; }

        /// <summary>
        /// Propiedades que se encriptan/desencriptan al obtener o guardar
        /// </summary>
        protected virtual List<Expression<Func<C, object>>> CryptProps { get; set; }

        /// <summary>
        /// Lista de Entidades de las cuales se eliminara en caso de elegir DeleteChild en true
        /// </summary>
        protected virtual List<Type> ChildTables { get; set; }

        /// <summary>
        /// Eliminar tablas hijas
        /// </summary>
        protected virtual bool DeleteChild { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected CrudManager()
        {
            InitializeInternal();
        }

        #endregion

        #region Metodos privados

        #region Inicializacion
                
        private void InitializeInternal()
        {
            _dataModel = DataModel.Instance;
            _logger = NLog.LogManager.CreateNullLogger();

            _dicTypeCheckValue = new Dictionary<Type, CheckValueConvertHandler>
            {
                { typeof(byte), CheckForByte},
                { typeof(short), CheckForShort},
                { typeof(int), CheckForInt},
                { typeof(long), CheckForLong },
                { typeof(float), CheckForFloat},
                { typeof(double), CheckForDouble },
                { typeof(decimal), CheckForDecimal},
                { typeof(bool), CheckForBool},
                { typeof(string), CheckForString },
                { typeof(DateTime), CheckForDateTime }
            };

            Initialize();
            SetPrimaryKeysInternal();
            SetFixedFiltersInternal();
            SetCryptPropsInternal();
            SetDeleteLogicFieldsInternal();
            SetChildTablesInternal();
        }

        private void SetChildTablesInternal()
        {
            DeleteChild = false;
            ChildTables = new List<Type>();
            SetChildTables();
        }

        private void SetDeleteLogicFieldsInternal()
        {
            DeleteLogicFields = null;
            SetDeleteLogicFields();
        }

        private void SetPrimaryKeysInternal()
        {
            PrimaryKeys = new List<Expression<Func<C, object>>>();
            SetPrimaryKeys();
        }

        private void SetCryptPropsInternal()
        {
            CryptProps = new List<Expression<Func<C, object>>>();
            SetCryptProps();
        }

        private void SetFixedFiltersInternal()
        {
            FixedFilters = null;
            SetFixedFilters();
        }

        #endregion

        #region Checks para paginado

        private bool CheckForByte(string value)
        {
            byte result;

            return Byte.TryParse(value, out result);
        }

        private bool CheckForFloat(string value)
        {
            float result;
            
            return float.TryParse(value, out result);
        }
        
        private bool CheckForDouble(string value)
        {
            double result;

            return Double.TryParse(value, out result);
        }

        private bool CheckForDecimal(string value)
        {
            decimal result;

            return Decimal.TryParse(value, out result);
        }

        private bool CheckForLong(string value)
        {
            long result;

            return Int64.TryParse(value, out result);
        }

        private bool CheckForInt(string value)
        {
            int result;

            return Int32.TryParse(value, out result);
        }

        private bool CheckForShort(string value)
        {
            short result;

            return Int16.TryParse(value, out result);
        }

        private bool CheckForBool(string value)
        {
            bool result;

            return Boolean.TryParse(value, out result);
        }

        private bool CheckForDateTime(string value)
        {
            DateTime result;

            if (value.Length < 2 || value.Length > 10)
                return false;
            else if (value.Length == 2)
                return DateTime.TryParseExact(value, new string[] { "dd", "mm", "yy" }, null, System.Globalization.DateTimeStyles.AssumeLocal, out result);
            else if (value.Length > 2 && value.Length < 6 && (value.ToLower().Contains("/") || value.ToLower().Contains("-")))
                return DateTime.TryParseExact(value, new string[] { "dd/MM" }, null, System.Globalization.DateTimeStyles.AssumeLocal, out result);
            else if (value.Length >= 8 && (value.ToLower().Contains("/") || value.ToLower().Contains("-")))
                return DateTime.TryParseExact(value, new string[] { "dd/MM/yy", "dd/MM/yyyy" }, null, System.Globalization.DateTimeStyles.AssumeLocal, out result);
            else
                return false;
        }

        private Boolean CheckForString(string value)
            => true;

        #endregion

        #region Reflection

        /// <summary>
        /// Obtener el nombre de la clave primaria
        /// </summary>
        /// <returns></returns>
        protected string GetPrimaryKeyName(int index = 0)
        {
            return GetMemberPropertyName(PrimaryKeys[index]);
        }
                
        protected string GetMemberPropertyName<P>(Expression<Func<P, object>> expression)  where P:class
        {
            MemberExpression me = default(MemberExpression);
            UnaryExpression ue = (expression.Body as UnaryExpression);

            me = (ue == null ? (expression.Body as MemberExpression) : (ue.Operand as MemberExpression));

            if (me == null)
                throw new InvalidExpressionException($"La propiedad no es correcta. Expresion no válida ({expression.ToString()})");

            PropertyInfo pi = (me.Member as PropertyInfo);

            if (pi == null)
                throw new InvalidExpressionException($"La propiedad no es correcta. Expresion no válida ({expression.ToString()})");

            return pi.Name.ToLowerInvariant();
        }

        /// <summary>
        /// Obtengo los nombres de las propiedades que se encriptan/desencriptan
        /// </summary>
        /// <returns></returns>
        protected List<string> CryptPropsNames()
            => CryptPropsNames(CryptProps);

        /// <summary>
        /// Obtengo los nombres de las propiedades que se encriptan/desencriptan
        /// </summary>
        /// <typeparam name="P"></typeparam>
        /// <param name="cryptProps"></param>
        /// <returns></returns>
        protected List<string> CryptPropsNames<P>(List<Expression<Func<P, object>>> cryptProps) where P: class
        {
            if (cryptProps != null && cryptProps.Count > 0)
            {
                List<string> retList = new List<string>();

                foreach (var cProp in cryptProps)
                    retList.Add(GetMemberPropertyName(cProp));
                
                return retList;
            }
            else
                return null;
        }

        private object GetMemberPropertyValue(Expression<Func<C, object>> expression)
        {
            //MemberExpression me = default(MemberExpression);
            UnaryExpression ue = (expression.Body as UnaryExpression);

            if(ue == null)
                throw new InvalidExpressionException("La expresión debe estar formada con una propiedad y un valor (x => x.Id == 2)");

            
            return null;
        }

        /// <summary>
        /// Obtener los campos de ordenamiento
        /// </summary>
        /// <param name="orderExpr"></param>
        /// <returns></returns>
        protected string GetOrderingForPaging(Expression<Func<C, object>>[] orderExpr)
        {
            if (orderExpr == null || orderExpr.Length == 0)
                throw new InvalidOrderException("Los campos de orden no son correctos");

            List<string> order = new List<string>();

            foreach (Expression<Func<C, object>> oExpr in orderExpr)
                order.Add(GetMemberPropertyName(oExpr));

            if (order != null && order.Count > 0)
                return string.Join(",", order);
            else
                throw new InvalidOrderException("Los campos de orden no son correctos");
        }

        /// <summary>
        /// Obtener los nombres de las propiedades
        /// </summary>
        /// <returns></returns>
        protected virtual string GetFieldForPaging<P>(params string[] excludedFields) where P : class
        {
            List<string> fields = new List<string>();

            PropertyInfo[] properties = null;
            if(excludedFields == null || excludedFields.Count() == 0)
                properties = typeof(P).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            else
                properties = typeof(P).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(w => !excludedFields.Any(a => a.ToLowerInvariant() == w.Name.ToLowerInvariant()))
                                      .ToArray();

            foreach (PropertyInfo prop in properties)
                fields.Add(prop.Name.ToLowerInvariant());

            return string.Join(", ", fields);
        }

        protected string GetFieldForPaging()
            => GetFieldForPaging<C>();

        /// <summary>
        /// Obtener condicion de orden para el paginado
        /// </summary>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        protected virtual string GetOrderingForPaging(string[] orderFields)
        {
            return string.Join(", ", orderFields);
        }

        /// <summary>
        /// Por compatibilidad
        /// </summary>
        /// <param name="search"></param>
        /// <param name="fixedFilters"></param>
        /// <returns></returns>
        protected SearchCondition GetSearchForPaging(string search, string fixedFilters)
            => GetSearchForPaging(search, fixedFilters, cryptProps:CryptProps);

        /// <summary>
        /// Obtener condicion de busqueda para el paginado
        /// </summary>
        /// <param name="search"></param>
        /// <param name="fixedFilters"></param>
        /// <returns></returns>
        protected virtual SearchCondition GetSearchForPaging<P>(string search, string fixedFilters, string[] excludedProps = null, List<Expression<Func<P, object>>> cryptProps = null) where P: class
        {
            SearchCondition searchCond = new SearchCondition();

            searchCond.Search = search;
            
            PropertyInfo[] properties = null;
            List<string> toCypher = null;

            if (string.IsNullOrWhiteSpace(fixedFilters))
                properties = typeof(P).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            else
                properties = typeof(P).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(w => !fixedFilters.ToLowerInvariant().Contains(w.Name.ToLowerInvariant()))
                                      .ToArray();

            if (excludedProps != null && excludedProps.Count() > 0)
                properties = properties.Where(w => !excludedProps.Any(a => a.ToLowerInvariant() == w.Name.ToLowerInvariant())).ToArray();

            if (cryptProps != null && cryptProps.Count > 0)
                toCypher = CryptPropsNames(cryptProps);

            foreach (PropertyInfo prop in properties)
            {
                Type t = prop.PropertyType;

                if (IsTypeForSeach(t)) //t.IsValueType || t == typeof(string))
                {
                    CheckValueConvertHandler dCheck = null;
                    
                    if (_dicTypeCheckValue.TryGetValue(t, out dCheck) || (t.IsNullableType() && _dicTypeCheckValue.TryGetValue(Nullable.GetUnderlyingType(t), out dCheck)))
                    {
                        string searchNew = search;
                        if (dCheck.Invoke(searchNew))
                        {
                            string field = null;

                            if (_dicTypeToLike.TryGetValue(t, out field))
                                field = field.Replace("#FIELD", prop.Name.ToLowerInvariant());
                            else if(t.IsNullableType() && _dicTypeToLike.TryGetValue(Nullable.GetUnderlyingType(t), out field) )
                                field = field.Replace("#FIELD", prop.Name.ToLowerInvariant());
                            else
                                field = "CAST(#FIELD AS VARCHAR) LIKE ?".Replace("#FIELD", prop.Name.ToLowerInvariant());

                            searchCond.Fields.Add(field);

                            string searchValue;
                            //Verifico si se encripta la propiedad
                            if(toCypher != null && toCypher.Count > 0)
                            {
                                if (toCypher.Any(a => prop.Name.ToLowerInvariant() == a))
                                    searchNew = Utilities.Encrypt(search);
                            }

                            if (field.Contains("LIKE"))
                                searchValue = "%" + searchNew + "%";
                            else
                                searchValue = searchNew;

                            searchCond.Parameters.Add(searchValue);
                        }
                    }
                }
            }

            return searchCond;
        }

        #endregion

        #region Datamodel

        protected virtual List<E> Execute<E>(string query, params object[] parameters) where E: class, new()
        {
            if (string.IsNullOrWhiteSpace(_connection))
                return _dataModel.Execute<E>(query, parameters);

            return _dataModel.Execute<E>(_connection, query, parameters);
        }

        protected virtual E GetValue<E>(string query, E defaultValue = default(E), bool notFoundException = false)
        {
            if (string.IsNullOrWhiteSpace(_connection))
                return _dataModel.GetValue(query, defaultValue, notFoundException);

            return _dataModel.GetValue(_connection, query, defaultValue, notFoundException);
        }

        protected virtual E GetValue<E>(string query, E defaultValue = default(E), bool notFoundException = false, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(_connection))
                return _dataModel.GetValue<E>(query, defaultValue, notFoundException, parameters);

            return _dataModel.GetValue<E>(_connection, query, defaultValue, notFoundException, parameters);
        }

        protected virtual List<object> InsertOrUpdate(bool useTransaction, bool insertKey, bool allowTriggers, C entity, params Expression<Func<C, object>>[] matchKey)
        {
            if (string.IsNullOrWhiteSpace(_connection))
                return _dataModel.InsertOrUpdate(useTransaction, insertKey, allowTriggers, entity, PrimaryKeys.ToArray());

            return _dataModel.InsertOrUpdate(_connection, useTransaction, insertKey, allowTriggers, entity, PrimaryKeys.ToArray());

        }

        protected int ExecuteNonQuery(string query, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(_connection))
                return _dataModel.ExecuteNonQuery(query, parameters);

            return _dataModel.ExecuteNonQuery(_connection, query, parameters);
        }

        #endregion

        #region Select

        /// <summary>
        /// Obtener todo interno
        /// </summary>
        /// <returns>Lista de entidades</returns>
        private List<C> GetAllInternal()
        {
            List<C> retList = null;

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" WHERE ";
                query += where.Where;

                retList = Execute<C>(query, where.Parameters.ToArray()); //_dataModel.Execute<C>(query, where.Parameters.ToArray());
            }
            else
                retList = Execute<C>(query); // _dataModel.Execute<C>(query);

            if (retList != null && retList.Count > 0 && CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted;
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetAllInternal();
                }
            }

            return retList;
        }

        /// <summary>
        /// Obtener todo interno
        /// </summary>
        /// <param name="whereExpr">Expresion lambda de filtro</param>
        /// <returns>Lista de entidades</returns>
        private List<C> GetAllInternal(Expression<Func<C, bool>> whereExpr)
        {
            ValidateGelAllPrimaryKeys();

            List<C> retList = null;

            //StringBuilder whereStr = new StringBuilder();
            //IList<object> listadoParametros = new List<object>();

            if (whereExpr != null && whereExpr.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(whereExpr);

                string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);
                query += @" WHERE ";
                query += where.Where;
                List<object> parametros = where.Parameters;

                if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
                {
                    where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                    query += @" AND ";
                    query += where.Where;

                    parametros = parametros.Concat(where.Parameters).ToList();
                }

                retList = Execute<C>(query, parametros.ToArray()); // _dataModel.Execute<C>(query, parametros.ToArray());

                if (retList != null && retList.Count > 0 && CryptProps != null && CryptProps.Count > 0)
                {
                    List<C> unCypher = null;
                    List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                    if (unCypher == null || unCypher.Count == 0)
                        return retDecrypted;
                    else
                    {
                        unCypher.ForEach(f => AddOrUpdate(f));

                        return GetAllInternal();
                    }
                }
                //return retList.EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());
            }

            return retList;
        }

        /// <summary>
        /// Obtener paginado interno
        /// </summary>
        /// <param name="orderExpr"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        private List<C> GetAllInternal(int pageNumber, int rowsPerPage, params Expression<Func<C, object>>[] orderExpr)
        {
            return GetAllInternal(pageNumber, rowsPerPage, null, GetOrderingForPaging(orderExpr));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        private List<C> GetAllInternal(int pageNumber, int rowsPerPage, string search, string[] orderFields)
        {
            return GetAllInternal(pageNumber, rowsPerPage, search, GetOrderingForPaging(orderFields));
        }
        
        /// <summary>
        /// Obtener paginado
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        private List<C> GetAllInternal(int pageNumber, int rowsPerPage, string search, string orderFields, string mainQuery = null, bool returnCount = false)
        {
            //Version con numero de pagina WHERE R_NUMBER BETWEEN ((? - 1) * ? + 1) AND (? * ?)

            List<object> parametros = new List<object>() { pageNumber, pageNumber, rowsPerPage };
            List<C> retList = null;
            string mainSQL = null;

            if (string.IsNullOrWhiteSpace(mainQuery))
                mainSQL = @"* FROM #TABLE_NAME#FIXED_FILTER#SEARCH_COND";
            else
                mainSQL = mainQuery;

            string query = @"SELECT #FIELDS 
                               FROM (
                                    SELECT #RETURNCOUNT ROW_NUMBER() OVER(ORDER BY #SORT_FIELDS) AS [R_NUMBER], #MAINQUERY
                                ) AS RESULT
                              WHERE R_NUMBER > ? AND R_NUMBER <= (? + ?)
                              ORDER BY #SORT_FIELDS".Replace("#MAINQUERY", mainSQL)
                                                    .Replace("#RETURNCOUNT", returnCount ? "COUNT(1) OVER() AS [ROW_COUNT]," : string.Empty)
                                                    .Replace("#TABLE_NAME", TableName)
                                                    .Replace("#FIELDS", GetFieldForPaging())
                                                    .Replace("#SORT_FIELDS", orderFields);

            if (string.IsNullOrWhiteSpace(search))
                query = query.Replace("#SEARCH_COND", string.Empty);

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query = query.Replace("#FIXED_FILTER", @" WHERE " + where.Where);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging(search, where.Where);

                    query = query.Replace("#SEARCH_COND", @" AND " + sc.Where);
                    parametros = sc.Parameters.Concat(parametros).ToList();
                }

                parametros = where.Parameters.Concat(parametros).ToList();
            }
            else
            {
                query = query.Replace("#FIXED_FILTER", string.Empty);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging<C>(search, null);

                    query = query.Replace("#SEARCH_COND", @" WHERE " + sc.Where);
                    parametros = sc.Parameters.Concat(parametros).ToList();
                }
            }

            retList = Execute<C>(query, parametros.ToArray());

            if (retList != null && retList.Count > 0 && CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted;
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetAllInternal();
                }
            }
            //return retList.EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());

            return retList;
        }

        /// <summary>
        /// Cantidad de registros para el obtener todo
        /// </summary>
        /// <returns></returns>
        private int GetAllCountInternal(string search)
        {
            string query = COUNT_STATEMENT.Replace("#TABLE_NAME", TableName);

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" WHERE ";
                query += where.Where;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging<C>(search, null);

                    query += @" AND ";
                    query += sc.Where;
                    where.Parameters = where.Parameters.Concat(sc.Parameters).ToList();
                }

                return GetValue(query, 0, false, where.Parameters.ToArray());
            }
            else
                if (!string.IsNullOrWhiteSpace(search))
                {
                    SearchCondition sc = GetSearchForPaging<C>(search, null);
    
                    query += @" WHERE ";
                    query += sc.Where;

                    return GetValue(query, 0, false, sc.Parameters.ToArray());
                }
                else
                    return GetValue(query, 0, false);
        }

        /// <summary>
        /// Obtener por Id Interno
        /// </summary>
        /// <param name="id">Valor de id de PK</param>
        /// <returns>Entidad</returns>
        private C GetByIdInternal(object id)
        {
            C retValue = null;
            object[] parameters = new object[] { id };

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetPrimaryKeyName() + @" = ?";

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;

                parameters = parameters.Concat(where.Parameters).ToArray();
            }

            if (CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = Execute<C>(query, parameters).EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted.FirstOrDefault();
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetByIdInternal(id);
                }
                //retValue = Execute<C>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray()).FirstOrDefault();
            }
            else
                retValue = Execute<C>(query, parameters).FirstOrDefault();

            return retValue;
        }

        /// <summary>
        /// Obtener por clave primaria compuesta
        /// </summary>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        private List<C> GetByPkInternal(object[] pkValues)
        {
            List<C> retList = null;
            object[] parameters = pkValues;
            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE ";

            for (int i = 0; i < pkValues.Count(); i++)
            {
                query += GetPrimaryKeyName(i) + @" = ? ";

                if (i < pkValues.Count() - 1)
                    query += @"AND ";
            }

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;

                parameters = parameters.Concat(where.Parameters).ToArray();
            }

            if (CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = Execute<C>(query, parameters).EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted;
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetByPkInternal(pkValues);
                }
                //retList = Execute<C>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());
            }
            else
                retList = Execute<C>(query, parameters);

            return retList;
        }

        /// <summary>
        /// Obtener por lista de items
        /// </summary>
        /// <param name="listToString">Lista de items separados por ","</param>
        /// <returns></returns>
        private List<C> GetByListInternal(string listToString)
        {
            List<C> retList = null;

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetPrimaryKeyName() + @" IN ({0})";


            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;

                retList = Execute<C>(string.Format(query, listToString), where.Parameters.ToArray());
            }
            else
                retList = Execute<C>(string.Format(query, listToString));

            if (retList != null && CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted;
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetByListInternal(listToString);
                }
                //return retList.EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());
            }

            return retList;
        }

        /// <summary>
        /// Obtener por lista de IDs
        /// </summary>
        /// <param name="idList">Lista con los valores de los IDs</param>
        /// <returns></returns>
        private List<C> GetByIdListInternal(List<int> idList)
        {
            return GetByListInternal(string.Join(", ", idList));
        }

        /// <summary>
        /// Obtener por una lista de Ids especificando la columna
        /// </summary>
        /// <param name="idList">Lista con los id</param>
        /// <param name="fieldSelect">Expression de selección</param>
        /// <returns></returns>
        private List<C> GetByIdListInternal(List<int> idList, Expression<Func<C, object>> fieldSelect)
        {
            List<C> retList = null;

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetMemberPropertyName(fieldSelect) + @" IN ({0})";

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;

                retList = Execute<C>(string.Format(query, string.Join(", ", idList)), where.Parameters.ToArray());
            }
            else
                retList = Execute<C>(string.Format(query, string.Join(", ", idList)));

            if (retList != null && CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted;
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetByIdListInternal(idList, fieldSelect);
                }
                //return retList.EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());
            }
            return retList;
        }

        /// <summary>
        /// Obtener por lista de codigos
        /// </summary>
        /// <param name="codList">Lista con los valores de los códigos</param>
        /// <returns></returns>
        private List<C> GetByCodListInternal(List<string> codList)
        {
            return GetByListInternal(string.Join(", ", codList.Select(s => "'" + s + "'")));
        }

        /// <summary>
        /// Obtener por Lista de códigos
        /// </summary>
        /// <param name="codList">Lista de códigos a seleccionar</param>
        /// <param name="fieldSelect">Campo por el que se filtrará la consulta</param>
        /// <returns></returns>
        private List<C> GetByCodListInternal(List<string> codList, Expression<Func<C, object>> fieldSelect)
        {
            if (codList == null || codList.Count == 0)
                throw new NullReferenceException("Debe suministrar la lista de codigos a filtrar");

            List<C> retList = null;

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetMemberPropertyName(fieldSelect) + @" IN ({0})";

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;

                retList = Execute<C>(string.Format(query, string.Join(", ", codList.Select(s => "'" + s + "'"))), where.Parameters.ToArray());
            }
            else
                retList = Execute<C>(string.Format(query, string.Join(", ", codList.Select(s => "'" + s + "'"))));

            if (retList != null && CryptProps != null && CryptProps.Count > 0)
            {
                List<C> unCypher = null;
                List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                if (unCypher == null || unCypher.Count == 0)
                    return retDecrypted;
                else
                {
                    unCypher.ForEach(f => AddOrUpdate(f));

                    return GetByCodListInternal(codList, fieldSelect);
                }
                //return retList.EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());
            }

            return retList;
        }

        /// <summary>
        /// Obtener por expresion lambda
        /// </summary>
        /// <param name="whereExpr">Expresion lambda para filtrar los datos</param>
        /// <param name="useFixedFilters">true, utiliza los filtros asignados al manager</param>
        /// <returns></returns>
        private List<C> GetByInternal(Expression<Func<C, bool>> whereExpr, bool useFixedFilters)
        {
            List<C> retList = null;

            if (whereExpr != null && whereExpr.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(whereExpr);
                string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

                query += @" WHERE ";
                query += where.Where;
                List<object> parametros = where.Parameters;

                if (useFixedFilters && FixedFilters != null && FixedFilters.Parameters.Count == 1)
                {
                    where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                    query += @" AND ";
                    query += where.Where;
                    parametros = parametros.Concat(where.Parameters).ToList();
                }

                retList = Execute<C>(query, parametros.ToArray());

                if (retList != null && retList.Count > 0 && CryptProps != null && CryptProps.Count > 0)
                {
                    List<C> unCypher = null;
                    List<C> retDecrypted = retList.EntityCypher(ref unCypher, CryptProps.ToArray());

                    if (unCypher == null || unCypher.Count == 0)
                        return retDecrypted;
                    else
                    {
                        unCypher.ForEach(f => AddOrUpdate(f));

                        return GetByInternal(whereExpr, useFixedFilters);
                    }
                    //return retList.EntityCypher(Utilities.CypherAction.Decrypt, CryptProps.ToArray());
                }
                
            }
            return retList;
        }

        private List<C> GetByInternal(string whereStr, object[] values)
        {
            throw new NotImplementedException("Método aún no implementado");
        }

        private bool ValidateByInternal(Expression<Func<C, object>> field, string value, Dictionary<string, string> pk, bool useFixedFilters)
        {
            if (field == null)
                throw new ArgumentNullException();

            var me = default(MemberExpression);
            var ue = (field.Body as UnaryExpression);

            me = (ue == null
                ? (field.Body as MemberExpression)
                : (ue.Operand as MemberExpression)
            );

            if (me == null)
                throw new InvalidExpressionException();

            var pi = (me.Member as PropertyInfo);
            if (pi == null)
                throw new InvalidExpressionException();

            var entityType = typeof(C);
            var fieldName = pi.Name;
            var parametros = new List<object>();
            var query = COUNT_STATEMENT.Replace("#TABLE_NAME", TableName);
            query += " WHERE ";

            if (pk.Count > 0)
            {
                if (pk.Count != PrimaryKeys.Count)
                    throw new InvalidPrimaryKeysException();

                var pkNames = PrimaryKeys
                    .Select(s => GetMemberPropertyName(s));

                if (pkNames.Any(a => !pk.Any(b => a == b.Key.ToLower())))
                    throw new InvalidPrimaryKeysException();

                var pkValues = pkNames
                    .Select(p => new
                    {
                        Name = p,
                        Value = pk.First(f => f.Key.ToLower() == p).Value
                    });

                var pkValuesWhere = string.Join(" AND ", pkValues.Select(s => $"{s.Name} <> ?"));

                query += $"( {pkValuesWhere} AND LOWER({fieldName}) = ?)";

                parametros.AddRange(pkValues.Select(s => s.Value));
                parametros.Add(value.ToLower());
            }
            else
            {
                query += $"LOWER({fieldName}) = ?";
                parametros.Add(value.ToLower());
            }

            if (useFixedFilters && FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                var where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;
                parametros = parametros.Concat(where.Parameters).ToList();
            }

            var count = GetValue(query, 0, false, parametros.ToArray());

            return (count == 0);
        }

        #endregion

        #region InsertUpdate

        private List<object> AddOrUpdateInternal(C entity, bool useTransaction, bool insertKey, bool allowTriggers)
        {
            if (entity != null && CryptProps != null && CryptProps.Count > 0)
                entity = Utilities.EntityCypher(entity, Utilities.CypherAction.Crypt, CryptProps.ToArray());

            return InsertOrUpdate(useTransaction, insertKey, allowTriggers, entity, PrimaryKeys.ToArray());
        }

        private List<object> AddOrUpdateInternal(C entity, bool useTransaction, bool insertKey)
        {
            if (entity != null && CryptProps != null && CryptProps.Count > 0)
                entity = Utilities.EntityCypher(entity, Utilities.CypherAction.Crypt, CryptProps.ToArray());

            return InsertOrUpdate(useTransaction, insertKey, false, entity, PrimaryKeys.ToArray());
        }

        private void BulkInsertInternal(List<C> entities)
        {
            if (string.IsNullOrWhiteSpace(_connection))
                _dataModel.BulkInsert(entities, TableName);
            else
                _dataModel.BulkInsert(_connection, entities, TableName);
        }

        private bool UpdateListInternal(List<C> entities, bool insertkey)
        {
            bool commitOK = _dataModel.Transaction(scope =>
            {
                entities.ForEach(f => AddOrUpdate(f, false, insertkey));

            }, true);

            return commitOK;
        }

        #endregion

        #region Delete

        private bool DeleteLogicInternal(object id)
        {
            if (DeleteLogicFields != null && DeleteLogicFields.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(DeleteLogicFields, true);

                string query = UPDATE_STATEMENT.Replace("#TABLE_NAME", TableName);

                query += " " + where.Where.Replace("(", string.Empty).Replace(")", string.Empty).ToString();
                query += @" WHERE " + GetPrimaryKeyName() + " = ?";

                return ExecuteNonQuery(query, where.Parameters.Concat(new object[] { id }).ToArray()) > 0;
            }
            else
                throw new InvalidLogicDeleteException("No se han especificado los campos y sus valores para le borrado lógico");
        }

        private bool DeletePhysicalInternal(object id)
        {
            string query = DELETE_STATEMENT.Replace("#TABLE_NAME", TableName);
            query += @" WHERE " + GetPrimaryKeyName() + " = ?";

            return ExecuteNonQuery(query, id) > 0;
        }

        private void DeleteChildInternal(object id)
        {
            //Eliminar datos de las tablas relacionadas
            if (DeleteChild && ChildTables != null && ChildTables.Count > 0)
            {
                string sqlToDelete = DELETE_STATEMENT + @" WHERE #ID_NAME = ?";

                ChildTables.ForEach(fk =>
                {
                    if (fk.BaseType.GetGenericTypeDefinition() != typeof(CrudManager<,>) &&
                        fk.BaseType.GetGenericTypeDefinition() != typeof(CrudManagerCustom<,,>) &&
                        fk.BaseType.GetGenericTypeDefinition() != typeof(CrudManagerCustomDT<,,,>) && 
                        fk.BaseType.GetGenericTypeDefinition() != typeof(CrudManagerDT<,,>))
                        throw new InvalidTableNameException("La tabla secundaria es incorrecta, el crud no hereda de CrudManager");

                    string query = sqlToDelete.Replace("#TABLE_NAME", fk.Name.ToUpperInvariant()).Replace("#ID_NAME", GetPrimaryKeyName());
                    ExecuteNonQuery(query, id);
                });
            }
        }

        private bool DeleteInternalTran(object id, bool logicDelete)
        {
            try
            {
                bool deleteOK = _dataModel.Transaction(scope =>
                {
                    DeleteChildInternal(id);
                    DeleteReferencedData(id);

                    if (!logicDelete)
                        DeletePhysicalInternal(id);
                    else
                        DeleteLogicInternal(id);
                });
                return deleteOK;
            }
            catch
            {
                return false;
            }
        }

        private bool DeleteInternal(object id, bool logicDelete)
        {
            ValidateDeleteSingle();

            if(DeleteChild)
                return DeleteInternalTran(id, logicDelete);
            else
                if(!logicDelete)
                    return DeletePhysicalInternal(id);
                else
                    return DeleteLogicInternal(id);
        }
        
        private bool DeleteInternal(object[] pkValues, bool logicDelete)
        {
            List<object> parameters = null;
            string query = string.Empty;
            string sWhere = @" WHERE ";

            //Genero Where
            for (int i = 0; i < pkValues.Count(); i++)
            {
                sWhere += GetPrimaryKeyName(i) + @" = ? ";

                if (i < pkValues.Count() - 1)
                    sWhere += @"AND ";
            }

            if (logicDelete)
            {
                if (DeleteLogicFields != null && DeleteLogicFields.Parameters.Count == 1)
                {
                    WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(DeleteLogicFields, true);
                    query = UPDATE_STATEMENT.Replace("#TABLE_NAME", TableName);

                    query += " " + where.Where.Replace("(", string.Empty).Replace(")", string.Empty).ToString();
                    query += sWhere;
                    parameters = where.Parameters.Concat(pkValues).ToList();
                }
            }
            else
            {
                query = DELETE_STATEMENT.Replace("#TABLE_NAME", TableName) + sWhere;
                parameters = pkValues.ToList();
            }

            return ExecuteNonQuery(query, parameters.ToArray()) > 0;
        }

        private bool DeleteByListInternal(List<string> pkValues, bool logicDelete)
        {
            if(!logicDelete)
                return DeleteByListInternal(string.Join(",", pkValues.Select(s => "'" + s + "'")));
            else
                return DeleteByListLogicInternal(string.Join(",", pkValues.Select(s => "'" + s + "'")));
        }

        private bool DeleteByListInternal(List<int> pkValues, bool logicDelete)
        {
            if (!logicDelete)
                return DeleteByListInternal(string.Join(",", pkValues));
            else
                return DeleteByListLogicInternal(string.Join(",", pkValues));
        }
        
        private bool DeleteByListLogicInternal(string values)
        {
            if (DeleteLogicFields != null && DeleteLogicFields.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(DeleteLogicFields, true);

                string query = UPDATE_STATEMENT.Replace("#TABLE_NAME", TableName);

                query += " " + where.Where.Replace("(", string.Empty).Replace(")", string.Empty).ToString();
                query += @" WHERE " + GetPrimaryKeyName() + @" IN ({0})";

                return ExecuteNonQuery(string.Format(query, values), where.Parameters.ToArray()) > 0;
            }
            else
                return false;
        }

        private bool DeleteByListInternal(string values)
        {
            string query = DELETE_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetPrimaryKeyName() + @" IN ({0})";

            return ExecuteNonQuery(string.Format(query, values)) > 0;
        }
        
        private bool DeleteByInternal(Expression<Func<C, bool>> deleteWhere)
        {
            if(deleteWhere != null && deleteWhere.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(deleteWhere);
                string query = DELETE_STATEMENT.Replace("#TABLE_NAME", TableName);

                query += @" WHERE ";
                query += where.Where;

                return ExecuteNonQuery(query, where.Parameters.ToArray()) > 0;
            }
            return false;
        }

        #endregion

        #endregion

        #region Metodos publicos

        public bool IsTypeForSeach(Type type)
        {
            bool retValue = type.IsValueType || type == typeof(string);

            if (type.IsNullableType())
            {
                Type underType = Nullable.GetUnderlyingType(type);

                retValue |= underType.IsValueType || underType == typeof(string);
            }

            return retValue;
        }

        #region Select

        /// <summary>
        /// Obtener todo
        /// </summary>
        /// <returns></returns>
        public virtual List<C> GetAll()
        {
            ValidateGet();

            return GetAllInternal();
        }

        /// <summary>
        /// Obtener todo filtrando por alguno de los campos de clave primaria
        /// </summary>
        /// <param name="whereExpr">Expresion de filtro</param>
        /// <returns>Lista de entidades</returns>
        public virtual List<C> GetAll(Expression<Func<C, bool>> whereExpr)
        { 
            return GetAllInternal(whereExpr);
        }

        /// <summary>
        /// Obtner paginado
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="rowsPerPage">Registros por página</param>
        /// <param name="orderExpr">Expresión de orden de la consulta</param>
        /// <returns></returns>
        public virtual List<C> GetAll(int pageNumber, int rowsPerPage, params Expression<Func<C, object>>[] orderExpr)
        {
            return GetAllInternal(pageNumber, rowsPerPage, orderExpr);
        }

        /// <summary>
        /// Obtener paginado
        /// </summary>
        /// <param name="pageNumber">Número de página</param>
        /// <param name="rowsPerPage">Registros por página</param>
        /// <param name="search">Valor a buscar</param>
        /// <param name="orderFields">Ordenamiento</param>
        /// <returns></returns>
        public virtual List<C> GetAll(int pageNumber, int rowsPerPage, string search, params string[] orderFields)
        {
            return GetAllInternal(pageNumber, rowsPerPage, search, orderFields);
        }
                
        /// <summary>
        /// Obtener la cantidad de registros para el obtener todo
        /// </summary>
        /// <returns></returns>
        public virtual int GetAllCount(string search = null)
        {
            return GetAllCountInternal(search);
        }
        
        /// <summary>
        /// Obtener por id
        /// </summary>
        /// <param name="id">Id de la entidad a devolver</param>
        /// <returns></returns>
        public virtual C GetById(object id)
        {
            ValidateGet();

            return GetByIdInternal(ValidatePkType(id));
        }

        /// <summary>
        /// Obetener por clave primaria compuesta
        /// </summary>
        /// <param name="pkValues">Valores de la Pk</param>
        /// <returns></returns>
        public virtual List<C> GetByPk(params object[] pkValues)
        {
            //Validar Types
            ValidatePkValues(pkValues);

            return GetByPkInternal(pkValues);
        }
        
        /// <summary>
        /// Obtener por lista de id
        /// </summary>
        /// <param name="idList">lista con los id de las entidades a devolver</param>
        /// <returns></returns>
        public virtual List<C> GetByIdList(List<int> idList)
        {
            return GetByIdListInternal(idList);
        }

        /// <summary>
        /// Obtener por lista de id
        /// </summary>
        /// <param name="idList">Lista con los id de las entidades a devolver</param>
        /// <param name="fieldSelect">Propiedad por la que se va a obtener la lista de entidades</param>
        /// <returns></returns>
        public virtual List<C> GetByIdList(List<int> idList, Expression<Func<C, object>> fieldSelect)
        {
            ValidateIdType(fieldSelect);
            return GetByIdListInternal(idList, fieldSelect);
        }

        /// <summary>
        /// Obtener por lista de códigos
        /// </summary>
        /// <param name="codList">Lista de códigos para filtrar</param>
        /// <returns></returns>
        public virtual List<C> GetByCodList(List<string> codList)
        {
            return GetByCodListInternal(codList);
        }

        public virtual List<C> GetByCodList(List<string> codList, Expression<Func<C, object>> fieldSelect)
        {

            return GetByCodListInternal(codList, fieldSelect);
        }

        /// <summary>
        /// Obtener
        /// </summary>
        /// <param name="whereExpr">Expresion de filtro para obtener los datos</param>
        /// <returns></returns>
        public virtual List<C> GetBy(Expression<Func<C, bool>> whereExpr, bool useFixedFilters = true)
        {
            return GetByInternal(whereExpr, useFixedFilters);
        }

        public virtual C GetByFirst(Expression<Func<C, bool>> whereExpr, bool useFixedFilters = true)
        {
            return GetByInternal(whereExpr, useFixedFilters).FirstOrDefault();
        }

        public virtual bool ValidateBy(Expression<Func<C, object>> field, string value, Dictionary<string, string> pk, bool useFixedFilters = true)
        {
            return ValidateByInternal(field, value, pk, useFixedFilters);
        }

        /// <summary>
        /// Obtener 
        /// </summary>
        /// <param name="whereStr">where tradicional de SQL</param>
        /// <param name="values">parametros</param>
        /// <returns></returns>
        public virtual List<C> GetBy(string whereStr, params object[] values)
        {
            return GetByInternal(whereStr, values);
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Actualizar o insertar una entidad
        /// </summary>
        /// <param name="entity">Entidad</param>
        /// <param name="useTransaction">Usar Transacción</param>
        /// <param name="insertKey">Insertar clave primaria</param>
        /// <returns></returns>
        public virtual List<object> AddOrUpdate(C entity, bool useTransaction = false, bool insertKey = false, bool allowTrigger = false)
        {
            return AddOrUpdateInternal(entity, useTransaction, insertKey, allowTrigger);
        }

        /// <summary>
        /// Actualizar o insertar entidades de una tabla hija
        /// </summary>
        /// <param name="entities">Lista de entiddades</param>
        /// <param name="idProp">Porpiedad de id de la tabla (la misma que la padre)</param>
        /// <param name="id">Valor del id</param>
        /// <param name="useTransaction">Usar transacción, false por defecto</param>
        /// <param name="insertKey">Insertar clave primaria, false por defecto</param>
        public virtual void AddOrUpdate(List<C> entities, Expression<Func<C, object>> idProp, object id, bool useTransaction = false, bool insertKey = false, bool useBulk = false)
        {
            //Busco la propiedad a actualizar el id
            string property = GetMemberPropertyName(idProp);
            PropertyInfo pClass = typeof(C).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                           .FirstOrDefault(f => f.Name.ToLowerInvariant() == property);
            if (!useBulk)
                entities.ForEach(e => 
                {
                    pClass.SetValue(e, id);
                    AddOrUpdate(e, useTransaction, insertKey);
                });
            else
            {
                entities.ForEach(e => pClass.SetValue(e, id));
                BulkInsertInternal(entities);
            }

        }

        /// <summary>
        /// Insertar una lista de entidades
        /// </summary>
        /// <param name="entities">Entidades a insertar</param>
        public virtual void AddList(List<C> entities)
        {
            BulkInsertInternal(entities);
        }

        /// <summary>
        /// Actualizar una lista de entidades
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="insertkey"></param>
        /// <returns></returns>
        public virtual bool UpdateList(List<C> entities, bool insertkey = false)
        {
            return UpdateListInternal(entities, insertkey);
        }

        /// <summary>
        /// Eliminar por id
        /// </summary>
        /// <param name="id">id del registro a eliminar</param>
        /// <param name="logicDelete">true, marca el registro en la DB con estado según la expresión DeleteLogicFields</param>
        /// <returns></returns>
        public virtual bool Delete(object id, bool logicDelete = true)
        {
            ValidateDelete();
            ValidataRelationships(id);
            return DeleteInternal(ValidatePkType(id), logicDelete);
        }

        /// <summary>
        /// Eliminar por clave primaria compuesta
        /// </summary>
        /// <param name="pkValues"></param>
        /// <returns></returns>
        public virtual bool DeleteByPk(object[] pkValues, bool logicDelete = false)
            => DeleteInternal(pkValues, logicDelete);

        public virtual bool DeleteByList(List<int> Values, bool logicDelete = true)
        {
            return DeleteByListInternal(Values, logicDelete);
        }

        public virtual bool DeleteByList(List<string> Values, bool logicDelete = true)
        {
            return DeleteByListInternal(Values, logicDelete);
        }

        public virtual bool DeleteBy(Expression<Func<C, bool>> deleteWhere)
        {
            return DeleteByInternal(deleteWhere);
        }

        /// <summary>
        /// Eliminar los datos de las tablas relacionadas
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        protected virtual void DeleteChildData(object id)
        {
            ValidateDeleteChild();
            DeleteChildInternal(id);
        }

        /// <summary>
        /// Obtener todo paginado pasando el query
        /// </summary>
        /// <param name="mainQuery"></param>
        /// <param name="returnCount"></param>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="search"></param>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        protected virtual List<C> GetAll(string mainQuery, bool returnCount, int pageNumber, int rowsPerPage, string search, params string[] orderFields)
            => GetAllInternal(pageNumber, rowsPerPage, search, GetOrderingForPaging(orderFields), mainQuery, returnCount);

        #endregion

        #region Historico

        /// <summary>
        /// Archivar en el historico los datos de la tabla y sus tablas relaciondas
        /// </summary>
        /// <param name="id">Valor de la clave primaria</param>
        public virtual void BackupById(object id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Archivar en el historico los datos de la tabla y sus tablas relacionadas
        /// </summary>
        /// <param name="backWhere">Clausula para filtrar los datos a archivar</param>
        public virtual void BackupBy(Expression<Func<C, bool>> backWhere)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtener los datos históricos por ID
        /// </summary>
        /// <param name="id">Valor de la clave primaria</param>
        /// <returns></returns>
        public virtual C RestoreById(object id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Obtener los datos historicos segun la clausula where
        /// </summary>
        /// <param name="restoreWhere">Claúsula de selección</param>
        /// <returns></returns>
        public virtual List<C> RestoreBy(Expression<Func<C, bool>> restoreWhere)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Validacion

        private void ValidateType(Expression<Func<C, object>> fieldSelect, List<Type> validTypes)
        {
            string property = GetMemberPropertyName(fieldSelect);
            PropertyInfo pi = typeof(C).GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(f => f.Name.ToLowerInvariant() == property);

            if (pi == null)
                throw new InvalidPrimaryKeysException("La clave primaria no es correcta");

            if (!validTypes.Any(a => a == pi.PropertyType))
                throw new InvalidCastException($"La tipo de la propiedad {pi.Name} no es compatible con los tipos Int");
        }

        private void ValidateIdType(Expression<Func<C, object>> fieldSelect)
        {
            List<Type> validTypes = new List<Type>
            {
                typeof(int),
                typeof(short),
                typeof(long)
            };

            ValidateType(fieldSelect, validTypes);
        }

        private void ValidateCodType(Expression<Func<C, object>> fieldSelect)
        {
            List<Type> validTypes = new List<Type>
            {
                typeof(string),
                typeof(char)
            };

            ValidateType(fieldSelect, validTypes);
        }

        private object ValidatePkType(object id, int index = 0)
        {
            string pk = GetPrimaryKeyName(index);

            PropertyInfo pi = typeof(C).GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(f => f.Name.ToLowerInvariant() == pk);

            if (pi == null)
                throw new InvalidPrimaryKeysException("La clave primaria no es correcta");

            try
            {
                return Convert.ChangeType(id, pi.PropertyType);
            }
            catch (Exception)
            {
                throw new InvalidCastException("El tipo del valor por el que se desea obtener la entidad no coincide con el tipo de la clave primaria");
            }
        }

        private void ValidateGelAllPrimaryKeys()
        {
            ValidateNullPrimaryKeys();

            if (PrimaryKeys != null && PrimaryKeys.Count == 1)
                throw new InvalidPrimaryKeysException("El metodo es utilizable para entidades con clave primaria compuesta");
        }
        
        private void ValidatePkValues(object[] pkValues)
        {
            if(pkValues == null || pkValues.Count() == 0)
                throw new InvalidPrimaryKeysException("Debe especificar los valores de clave primaria");

            if(pkValues.Count() != PrimaryKeys.Count)
                throw new InvalidPrimaryKeysException($"La cantidad({pkValues.Count()}) de valores para la clave primaria no coincide con la cantidad({PrimaryKeys.Count}) especificada en el CRUD");

            int i = 0;
            foreach (object pk in pkValues)
                ValidatePkType(pk, i++);
        }

        private void ValidateTable()
        {
            if (string.IsNullOrWhiteSpace(TableName))
                throw new InvalidTableNameException("Debe establecer el nombre de la tabla");
        }
        
        private void ValidatePrimaryKey()
        {
            ValidateNullPrimaryKeys();

            //if (PrimaryKeys.Count > 1)
            //    throw new InvalidPrimaryKeysException("La clave primaria debe ser única");
        }

        private void ValidateNullPrimaryKeys()
        {
            if (PrimaryKeys == null)
                throw new NullPrimaryKeysException("La clave primaria no fue establecida");
        }
        
        private void ValidateExistsPrimaryKeys()
        {

        }

        private void ValidateGet()
        {
            ValidateTable();
            ValidatePrimaryKey();
        }

        private void ValidateDeleteSingle()
        {
            ValidateTable();
            ValidateNullPrimaryKeys();
            ValidatePrimaryKey();
        }

        private void ValidateDeleteChild()
        {
            if (!DeleteChild)
                throw new InvalidOperationException("Debe habilitar el borrado de datos de las tablas relacionadas");

            if(ChildTables == null || ChildTables.Count == 0)
                throw new InvalidOperationException("Debe dar de alta los tipos de las tablas relacionadas");
        }

        #endregion

        #region Metodos Abstractos

        protected abstract void SetPrimaryKeys();

        protected abstract void SetCryptProps();

        protected abstract void SetFixedFilters();

        protected abstract void SetDeleteLogicFields();

        #endregion

        #region Metodos virtuales opcionales

        protected virtual void DeleteReferencedData(object id)
        {

        }

        protected virtual void SetChildTables()
        {
            //TODO: En la clase heredada si fuese necesario establecer la propiedad DeleteChild en true 
            //      y agregar los tipos necesarios a la lista ChildTables
        }

        protected virtual void Initialize()
        {
            //TODO:Opcional si hay que cambiar algo en la inicializacion
        }

        protected virtual void ValidataRelationships(object id)
        {

        }

        protected virtual void ValidateDelete()
        {
            ValidateTable();
            ValidateNullPrimaryKeys();

            if (PrimaryKeys != null && PrimaryKeys.Count > 1)
                throw new InvalidPrimaryKeysException("El metodo no es utilizable para entidades con clave primaria compuesta");
        }

        #endregion

        #endregion
    }
}
