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
    /// Manager para entidades Lookups
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="E"></typeparam>
    public abstract class LookupsManager<T, E> : GenericSingleton<T>
        where T:class
        where E:class, new()
    {

        #region Constantes

        protected static readonly string SELECT_STATEMENT = "SELECT * FROM #TABLE_NAME";

        #endregion

        #region Atributos

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
        };

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
                    _tableName = typeof(E).Name.ToUpperInvariant();

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
        /// Filtros fijos para las consultas
        /// </summary>
        protected virtual Expression<Func<E, bool>> FixedFilters { get; set; } = null;

        /// <summary>
        /// Lista de claves primarias
        /// </summary>
        protected virtual List<Expression<Func<E, object>>> PrimaryKeys { get; set; } = new List<Expression<Func<E, object>>>();

        #endregion

        #region Constructor

        protected LookupsManager()
        {
            InitializeInternal();
        }

        #endregion

        #region Metodos privados

        #region Inicializacion

        private void InitializeInternal()
        {
            _dataModel = DataModel.Instance;
            _logger = NLog.LogManager.GetCurrentClassLogger();

            Initialize();
            SetPrimaryKeys();
            SetFixedFilters();
        }

        #endregion

        #region Reflection

        private string GetMemberPropertyName(Expression<Func<E, object>> expression)
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
        /// Obtener el nombre de la clave primaria
        /// </summary>
        /// <returns></returns>
        private string GetPrimaryKeyName() => GetMemberPropertyName(PrimaryKeys[0]);

        /// <summary>
        /// Obtener condicion de orden para el paginado
        /// </summary>
        /// <param name="orderFields"></param>
        /// <returns></returns>
        protected string GetOrderingForPaging(string[] orderFields)
        {
            return string.Join(",", orderFields);
        }

        /// <summary>
        /// Obtener condicion de busqueda para el paginado
        /// </summary>
        /// <param name="search"></param>
        /// <param name="fixedFilters"></param>
        /// <returns></returns>
        protected SearchCondition GetSearchForPaging(string search, string fixedFilters, Type entityType = null)
        {
            SearchCondition searchCond = new SearchCondition();

            searchCond.Search = search;

            PropertyInfo[] properties = null;
            Type type = entityType ?? typeof(E);

            if (string.IsNullOrWhiteSpace(fixedFilters))
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            else
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                      .Where(w => !fixedFilters.ToLowerInvariant().Contains(w.Name.ToLowerInvariant()))
                                      .ToArray();

            foreach (PropertyInfo prop in properties)
            {
                Type t = prop.PropertyType;

                if (t.IsValueType || t == typeof(string))
                {
                    string field = null;

                    if (_dicTypeToLike.TryGetValue(t, out field))
                        field = field.Replace("#FIELD", prop.Name.ToLowerInvariant());
                    else
                        field = "CAST(#FIELD AS VARCHAR) LIKE ?".Replace("#FIELD", prop.Name.ToLowerInvariant());

                    searchCond.Fields.Add(field);

                    var searchValue = search;

                    if (field.Contains("LIKE"))
                        searchValue = "%" + search + "%";

                    searchCond.Parameters.Add(searchValue);
                }
            }

            return searchCond;
        }
        #endregion

        #region Validaciones

        private object ValidatePkType(object id)
        {
            string pk = GetPrimaryKeyName();

            PropertyInfo pi = typeof(E).GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(f => f.Name.ToLowerInvariant() == pk);

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

        private void ValidateTable()
        {
            if (string.IsNullOrWhiteSpace(TableName))
                throw new InvalidTableNameException("Debe establecer el nombre de la tabla");
        }

        private void ValidatePrimaryKey()
        {
            ValidateNullPrimaryKeys();
        }

        private void ValidateNullPrimaryKeys()
        {
            if (PrimaryKeys == null)
                throw new NullPrimaryKeysException("La clave primaria no fue establecida");
        }

        private void ValidateSinglePrimaryKey()
        {
            if (PrimaryKeys.Count > 1)
                throw new InvalidPrimaryKeysException("El metodo GetByPk es para entidades con clave primaria simple");

        }

        private void ValidateGet()
        {
            ValidateTable();
            ValidatePrimaryKey();
        }

        #endregion

        #region Datamodel

        private List<E> Execute(string query, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(_connection))
                return _dataModel.Execute<E>(query, parameters);

            return _dataModel.Execute<E>(_connection, query, parameters);
        }

        #endregion

        #region Select

        /// <summary>
        /// Obtener todo interno
        /// </summary>
        /// <returns>Lista de entidades</returns>
        private List<E> GetAllInternal()
        {
            List<E> retList = null;

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" WHERE ";
                query += where.Where;

                retList = Execute(query, where.Parameters.ToArray()); //_dataModel.Execute<C>(query, where.Parameters.ToArray());
            }
            else
                retList = Execute(query); // _dataModel.Execute<C>(query);

            return retList;
        }

        /// <summary>
        /// Obtener por Id Interno
        /// </summary>
        /// <param name="id">Valor de id de PK</param>
        /// <returns>Entidad</returns>
        private E GetByPkInternal(object id)
        {
            E retValue = null;
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

            retValue = Execute(query, parameters).FirstOrDefault();

            return retValue;
        }

        /// <summary>
        /// Obtener por expresion lambda
        /// </summary>
        /// <param name="whereExpr">Expresion lambda para filtrar los datos</param>
        /// <param name="useFixedFilters">true, utiliza los filtros asignados al manager</param>
        /// <returns></returns>
        private List<E> GetByInternal(Expression<Func<E, bool>> whereExpr, bool useFixedFilters)
        {
            List<E> retList = null;

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

                retList = Execute(query, parametros.ToArray());
            }

            return retList;
        }

        /// <summary>
        /// Obtener por lista de codigos
        /// </summary>
        /// <param name="codList">Lista con los valores de los códigos</param>
        /// <returns></returns>
        private List<E> GetByCodListInternal(List<string> codList)
            => GetByListInternal(string.Join(", ", codList.Select(s => "'" + s + "'")));

        /// <summary>
        /// Obtener por lista de items
        /// </summary>
        /// <param name="listToString">Lista de items separados por ","</param>
        /// <returns></returns>
        private List<E> GetByListInternal(string listToString)
        {
            List<E> retList = null;

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetPrimaryKeyName() + @" IN ({0})";


            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;

                retList = Execute(string.Format(query, listToString), where.Parameters.ToArray());
            }
            else
                retList = Execute(string.Format(query, listToString));
            
            return retList;
        }

        private List<E> GetByCodListInternal(string codList, Expression<Func<E, object>> fieldSelect, Expression<Func<E, bool>> filter)
        {
            List<E> retList = null;
            List<object> parameter = new List<object>();

            string query = SELECT_STATEMENT.Replace("#TABLE_NAME", TableName);

            query += @" WHERE " + GetMemberPropertyName(fieldSelect) + @" IN ({0})";

            if(filter != null && filter.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(filter);

                query += @" AND ";
                query += where.Where;
                parameter.AddRange(where.Parameters);
            }

            if (FixedFilters != null && FixedFilters.Parameters.Count == 1)
            {
                WhereClause where = ExpressionConverter.Instance.GenerarClausulaWhere(FixedFilters);

                query += @" AND ";
                query += where.Where;
                parameter.AddRange(where.Parameters);
            }
            //string.Join(", ", codList.Select(s => "'" + s + "'")
            if (parameter.Count > 0)
                retList = Execute(string.Format(query, codList), parameter.ToArray());
            else
                retList = Execute(string.Format(query, codList));
                        
            return retList;
        }

        #endregion

        #endregion

        #region Metodos publicos

        /// <summary>
        /// Obtener todos los registros
        /// </summary>
        /// <returns></returns>
        public virtual List<E> GetAll()
        {
            ValidateGet();
            return GetAllInternal();
        }

        /// <summary>
        /// Obtener por lista de códigos
        /// </summary>
        /// <param name="codList">Lista de códigos para filtrar</param>
        /// <returns></returns>
        public virtual List<E> GetByCodList(List<string> codList, Expression<Func<E, object>> fieldSelect, Expression<Func<E, bool>> filter = null)
            => GetByCodListInternal(string.Join(", ", codList.Select(s => "'" + s + "'")), fieldSelect, filter);

        public virtual List<E> GetByIdList(List<int> idList, Expression<Func<E, object>> fieldSelect, Expression<Func<E, bool>> filter = null)
            => GetByCodListInternal(string.Join(", ", idList.Select(s => s.ToString())), fieldSelect, filter);

        public virtual E GetByPk(object pkValue)
        {
            ValidateGet();
            ValidateSinglePrimaryKey();

            return GetByPkInternal(ValidatePkType(pkValue));
        }

        /// <summary>
        /// Obtener
        /// </summary>
        /// <param name="whereExpr">Expresion de filtro para obtener los datos</param>
        /// <returns></returns>
        public virtual List<E> GetBy(Expression<Func<E, bool>> whereExpr, bool useFixedFilters = true) 
            => GetByInternal(whereExpr, useFixedFilters);

        #endregion

        #region Metodos Virtuales

        protected virtual void Initialize() { /* TODO:Opcional si hay que cambiar algo en la inicializacion */ }

        #endregion

        #region Metodos abstractos

        protected abstract void SetPrimaryKeys();

        protected abstract void SetFixedFilters();

        #endregion
    }
}
