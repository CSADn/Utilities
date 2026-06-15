using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Transactions;

namespace SubdivxRipper
{
    public sealed class DataModel
    {
        private string _globalDBKey;
        private int _cmdGlobalTimeout;
        private int _cmdTimeout;
        private Logger _log = LogManager.GetCurrentClassLogger();
        private static object locking = new Object();

        public int CommandTimeout
        {
            get { return _cmdTimeout; }
            set { _cmdTimeout = (value > _cmdGlobalTimeout ? value : _cmdGlobalTimeout); }
        }

        public string GlobalConnectionString
        {
            get { return _globalDBKey; }
            set { _globalDBKey = value; }
        }


        #region Singleton

        private volatile static DataModel _instance;

        public static DataModel Instance
        {
            get
            {
                if (_instance == null)
                    lock (locking)
                        if (_instance == null)
                            _instance = new DataModel();

                return _instance;
            }
        }

        #endregion


        #region Constructor

        private DataModel()
        {
            _globalDBKey = "DB".FromAppSettings<string>(notFoundException: true);
            _cmdGlobalTimeout = "DBCommandTimeout".FromAppSettings(30);

            CommandTimeout = _cmdGlobalTimeout;

            VerifyDB();
        }

        #endregion


        #region Public Methods

        private void VerifyDB()
        {
            try
            {
                GetDate();
            }
            catch (SqlException sqlEx)
            {
                _log.Log(LogLevel.Error, sqlEx.Message);
                 throw new Exception("No fue posible establecer una conexión con la base de datos.");
            }
        }


        public DateTime GetDate()
        {
            return GetDate(_globalDBKey);
        }

        public DateTime GetDate(string dbKey)
        {
            return GetValue<DateTime>(dbKey, "SELECT GETDATE();");
        }

        public T GetValue<T>(string query, T defaultValue = default(T), bool notFoundException = false)
        {
            return GetValue<T>(_globalDBKey, query, defaultValue, notFoundException);
        }

        public T GetValue<T>(string query, T defaultValue = default(T), bool notFoundException = false, params object[] parameters)
        {
            return GetValue<T>(_globalDBKey, query, defaultValue, notFoundException, parameters);
        }

        public T GetValue<T>(string dbKey, string query, T defaultValue = default(T), bool notFoundException = false)
        {
            return GetValue<T>(dbKey, query, defaultValue, notFoundException, null);
        }

        public T GetValue<T>(string dbKey, string query, T defaultValue = default(T), bool notFoundException = false, params object[] parameters)
        {
            var dt = parameters == null ? Execute(dbKey, query) : Execute(dbKey, query, parameters);

            if (dt == null || dt.Rows.Count == 0 || dt.Rows[0].ItemArray.Length == 0)
            {
                if (notFoundException)
                {
                    var ex = new Exception("La consulta no devolvió resultados");
                    _log.Log(LogLevel.Error, ex.Message + Environment.NewLine + "(" + query + ")");

                    throw ex;
                }
                else
                    return defaultValue;
            }

            return Utilities.CastValue(dt.Rows[0][0].ToString(), defaultValue);
        }


        public DataTable Execute(string query, params object[] parameters)
        {
            return Execute(_globalDBKey, query, 0, parameters);
        }

        public DataTable Execute(string dbKey, string query, params object[] parameters)
        {
            return Execute(dbKey, query, 0, parameters);
        }

        public DataTable Execute(string dbKey, string query, int timeout, params object[] parameters)
        {
            if (query.IsNull())
                return null;

            var cs = (dbKey.IsNull()
                ? _globalDBKey.FromConnections()
                : dbKey.FromConnections()
            );

            using (var conn = new SqlConnection(cs))
            {
                using (var cmd = conn.CreateCommand())
                {
                    var paramsCount = 0;
                    cmd.CommandText = BuildQueryVariables(query, out paramsCount);
                    cmd.CommandTimeout = (_cmdTimeout > timeout ? _cmdTimeout : timeout);

                    if (paramsCount > 0 && paramsCount != parameters.Count())
                        throw new ArgumentOutOfRangeException();

                    if (parameters.Count() > 0)
                        for (int i = 0; i < parameters.Length; i++)
                            if (parameters[i] is DateTime)
                                cmd.Parameters.Add("@param" + i, SqlDbType.DateTime2).Value = (parameters[i] ?? DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@param" + i, (parameters[i] ?? DBNull.Value));

                    try
                    {
                        conn.Open();

                        using (var da = new SqlDataAdapter(cmd))
                        {
                            var dt = new DataTable();
                            da.Fill(dt);
                            return dt;
                        }
                    }
                    catch (Exception ex)
                    {
                        var values = string.Empty;
                        if (parameters != null && parameters.Count() > 0)
                            values = " (" + string.Join(",", parameters.Select(s => s != null ? s.ToString() : "null")) + ")";

                        _log.Log(LogLevel.Error, "(" + cmd.CommandText + ")" + values + Environment.NewLine + ex.ToString());

                        throw;
                    }
                    finally
                    {
                        _cmdTimeout = _cmdGlobalTimeout;
                    }
                }
            }
        }


        public List<T> Execute<T>(string query, params object[] parameters) where T: class, new()
        {
            return Execute<T>(null, query, 0, parameters);
        }

        public List<T> Execute<T>(string dbKey, string query, params object[] parameters) where T : class, new()
        {
            return Execute<T>(dbKey, query, 0, parameters);
        }

        public List<T> Execute<T>(string dbKey, string query, int timeout, params object[] parameters) where T: class, new()
        {
            var type = typeof(T);
            var create = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy); //Agregado por LEON para obtener el create de la clase EntityBase<T>

            if (create == null)
                throw new NotSupportedException("Type '" + type.ToString() + "' not supported. Static Create() method missing.");

            var list = new List<T>();

            try
            {
                var dt = Execute(dbKey, query, timeout, parameters);

                for (int i = 0; i < dt.Rows.Count; i++)
                    list.Add((T)create.Invoke(type, new object[] { dt.Rows[i] }));

                return list;
            }
            catch (Exception ex)
            {
                var values = string.Empty;
                if (parameters != null && parameters.Count() > 0)
                    values = " (" + string.Join(",", parameters.Select(s => s.ToString())) + ")";

                _log.Log(LogLevel.Error, "(" + query + ")" + values + Environment.NewLine + ex.ToString());

                throw;
            }
        }


        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            return ExecuteNonQuery(null, query, 0, parameters);
        }

        public int ExecuteNonQuery(string dbKey, string query, params object[] parameters)
        {
            return ExecuteNonQuery(dbKey, query, 0, parameters);
        }

        public int ExecuteNonQuery(string dbKey, string query, int timeout, params object[] parameters)
        {
            if (query.IsNull())
                return -1;

            var cs = (dbKey.IsNull()
                ? _globalDBKey.FromConnections()
                : dbKey.FromConnections()
            );

            using (var conn = new SqlConnection(cs))
            {
                using (var cmd = conn.CreateCommand())
                {
                    var paramsCount = 0;
                    cmd.CommandText = BuildQueryVariables(query, out paramsCount);
                    cmd.CommandTimeout = (_cmdTimeout > timeout ? _cmdTimeout : timeout);

                    if (paramsCount != parameters.Count())
                        throw new ArgumentOutOfRangeException();

                    if (parameters.Count() > 0)
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i] is byte[])
                            {
                                var arr = (byte[])parameters[i];
                                if (arr.Length == 0 || (arr.Length == 1 && arr[0] == 0))
                                    cmd.Parameters.Add("@param" + i, SqlDbType.VarBinary, -1).Value = DBNull.Value;
                                else
                                    cmd.Parameters.AddWithValue("@param" + i, parameters[i]);
                            }
                            else
                                cmd.Parameters.AddWithValue("@param" + i, parameters[i] ?? DBNull.Value);
                        }

                    try
                    {
                        conn.Open();
                        return cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        var values = string.Empty;
                        if (parameters != null && parameters.Count() > 0)
                            values = " (" + string.Join(",", parameters.Select(s => s.ToString())) + ")";

                        _log.Log(LogLevel.Error, "(" + cmd.CommandText + ")" + values + Environment.NewLine + ex.ToString());

                        throw;
                    }
                    finally
                    {
                        _cmdTimeout = _cmdGlobalTimeout;
                    }
                }
            }
        }


        public T ExecuteScalar<T>(string query, T defaultReturn, params object[] parameters)
        {
            return ExecuteScalar(null, query, 0, defaultReturn, parameters);
        }

        public T ExecuteScalar<T>(string query, int timeout, T defaultReturn, params object[] parameters)
        {
            return ExecuteScalar(null, query, timeout, defaultReturn, parameters);
        }

        public T ExecuteScalar<T>(string dbKey, string query, T defaultReturn, params object[] parameters)
        {
            return ExecuteScalar<T>(dbKey, query, 0, defaultReturn, parameters);
        }

        public T ExecuteScalar<T>(string dbKey, string query, int timeout, T defaultReturn, params object[] parameters)
        {
            if (query.IsNull())
                return defaultReturn;

            try
            {
                var dt = Execute(dbKey, query, timeout, parameters);

                if (dt.Rows.Count == 0)
                    return defaultReturn;
                else
                    return (T)dt.Rows[0][0];
            }
            catch (Exception ex)
            {
                var values = string.Empty;
                if (parameters != null && parameters.Count() > 0)
                    values = " (" + string.Join(",", parameters.Select(s => s.ToString())) + ")";

                _log.Log(LogLevel.Error, "(" + query + ")" + values + Environment.NewLine + ex.ToString());

                throw;
            }
        }


        public bool Transaction(Action<TransactionScope> actions, bool rethrowException = false)
        {
            return Transaction(_globalDBKey, actions, rethrowException);
        }

        public bool Transaction(string dbKey, Action<TransactionScope> actions, bool rethrowException = false)
        {
            try
            {
                var tsoptions = new TransactionOptions
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                    Timeout = TransactionManager.MaximumTimeout
                };

                using (var ts = new TransactionScope(TransactionScopeOption.Required, tsoptions))
                {
                    actions(ts);
                    ts.Complete();
                    return true;
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Error, ex.ToString());

                if (rethrowException)
                    throw;
                else
                    return false;
            }
        }

        public DataTable ExecuteSP(string name, out List<object> output, params object[] inputs)
        {
            return ExecuteSP(null, name, 0, out output, inputs);
        }

        public DataTable ExecuteSP(string name, int timeout, out List<object> output, params object[] inputs)
        {
            return ExecuteSP(null, name, timeout, out output, inputs);
        }

        public DataTable ExecuteSP(string dbKey, string name, out List<object> output, params object[] inputs)
        {
            return ExecuteSP(dbKey, name, 0, out output, inputs);
        }

        public DataTable ExecuteSP(string dbKey, string name, int timeout, out List<object> output, params object[] inputs)
        {
            if (name.IsNull())
                throw new ArgumentNullException();

            var cs = (dbKey.IsNull()
                ? _globalDBKey.FromConnections()
                : dbKey.FromConnections()
            );

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = name;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandTimeout = (_cmdTimeout > timeout ? _cmdTimeout : timeout);

                    SqlCommandBuilder.DeriveParameters(cmd);

                    var parameters = cmd.Parameters
                        .Cast<SqlParameter>();

                    var inputParameters = parameters
                        .Where(w => w.Direction == ParameterDirection.Input)
                        .ToList();

                    var outputParameters = parameters
                        .Where(w => w.Direction == ParameterDirection.InputOutput || w.Direction == ParameterDirection.Output)
                        .Select(s =>
                        {
                            s.Direction = ParameterDirection.Output;
                            return s;
                        })
                        .ToList();

                    var returnParameter = parameters
                        .Where(f => f.Direction == ParameterDirection.ReturnValue)
                        .ToList();

                    if (inputParameters.Count() > 0)
                        if (inputs == null)
                            throw new ArgumentNullException();
                        else if (inputParameters.Count() != inputs.Length)
                            throw new IndexOutOfRangeException();

                    for (int i = 0; i < inputParameters.Count(); i++)
                        inputParameters.ElementAt(i).Value = inputs[i];

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        da.Fill(dt);

                        output = returnParameter
                            .Select(s => s.Value)
                            .ToList();

                        output.AddRange(
                            outputParameters
                                .Select(s => s.Value)
                                .ToList()
                        );

                        _cmdTimeout = _cmdGlobalTimeout;
                        return dt;
                    }
                }
            }
        }


        /// <summary>
        /// Actualiza o Inserta una instancia de entidad, en transacción y permitiendo insertar el campo especificado en matchKey.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la entidad. (Implícito. No es necesario especificarlo).</typeparam>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns>Devuelve True si pudo insertarse o actualizarse correctamente la entidad. Excepción y rollback ante cualquier error.</returns>
        public List<object> InsertOrUpdate<T>(T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate<T>(_globalDBKey, true, true, entity, matchKey);
        }

        /// <summary>
        /// Actualiza o Inserta una instancia de entidad, en transacción y permitiendo insertar el campo especificado en matchKey.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la entidad. (Implícito. No es necesario especificarlo).</typeparam>
        /// <param name="dbKey">Especifica una key para obtener el connection string.</param>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns>Devuelve True si pudo insertarse o actualizarse correctamente la entidad. Excepción y rollback ante cualquier error.</returns>
        public List<object> InsertOrUpdate<T>(string dbKey, T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate<T>(dbKey, true, true, entity, matchKey);
        }

        /// <summary>
        /// Actualiza o Inserta una instancia de entidad en transacción.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la entidad. (Implícito. No es necesario especificarlo).</typeparam>
        /// <param name="insertKey">Activa la inserción del campo especificado en matchKey.</param>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns>Devuelve True si pudo insertarse o actualizarse correctamente la entidad. Excepción y rollback ante cualquier error.</returns>
        public List<object> InsertOrUpdate<T>(bool insertKey, T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate<T>(_globalDBKey, true, insertKey, entity, matchKey);
        }

        /// <summary>
        /// Actualiza o Inserta una instancia de entidad en transacción.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la entidad. (Implícito. No es necesario especificarlo).</typeparam>
        /// <param name="dbKey">Especifica una key para obtener el connection string.</param>
        /// <param name="insertKey">Activa la inserción del campo especificado en matchKey.</param>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns>Devuelve True si pudo insertarse o actualizarse correctamente la entidad. Excepción y rollback ante cualquier error.</returns>
        public List<object> InsertOrUpdate<T>(string dbKey, bool insertKey, T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate<T>(dbKey, true, insertKey, entity, matchKey);
        }

        /// <summary>
        /// Actualiza o Inserta una instancia de entidad.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la entidad. (Implícito. No es necesario especificarlo).</typeparam>
        /// <param name="transaction">Ejecuta la operación dentro de una transacción.</param>
        /// <param name="insertKey">Activa la inserción del campo especificado en matchKey.</param>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns>Devuelve True si pudo insertarse o actualizarse correctamente la entidad. Excepción y rollback ante cualquier error.</returns>
        /// <returns></returns>
        public List<object> InsertOrUpdate<T>(bool transaction, bool insertKey, T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate<T>(_globalDBKey, transaction, insertKey, entity, matchKey);
        }

        /// <summary>
        /// Actualiza o Inserta una instancia de entidad
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="transaction">Ejecuta la operación dentro de una transacción.</param>
        /// <param name="insertKey">Activa la inserción del campo especificado en matchKey.</param>
        /// <param name="allowTrigger">true, permite triggers en la tabla</param>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns></returns>
        public List<object> InsertOrUpdate<T>(bool transaction, bool insertKey, bool allowTrigger, T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate<T>(_globalDBKey, transaction, insertKey, allowTrigger, entity, matchKey);
        }
        
        /// <summary>
        /// Actualiza o Inserta una instancia de entidad.
        /// </summary>
        /// <typeparam name="T">Tipo de dato de la entidad. (Implícito. No es necesario especificarlo).</typeparam>
        /// <param name="dbKey">Especifica una key para obtener el connection string.</param>
        /// <param name="transaction">Ejecuta la operación dentro de una transacción.</param>
        /// <param name="insertKey">Activa la inserción del campo especificado en matchKey.</param>
        /// <param name="entity">Entidad a actualizar o insertar. (Con todas sus propiedades cargadas).</param>
        /// <param name="matchKey">Propiedad por la que se va a filtrar para actualizar la entidad.</param>
        /// <returns>Devuelve True si pudo insertarse o actualizarse correctamente la entidad. Excepción y rollback ante cualquier error.</returns>
        public List<object> InsertOrUpdate<T>(string dbKey, bool transaction, bool insertKey, T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            return InsertOrUpdate(dbKey, transaction, insertKey, false, entity, matchKey);
            //if (string.IsNullOrWhiteSpace(dbKey))
            //    throw new ArgumentNullException("Invalid ConnectionString");

            //if (entity == null)
            //    throw new ArgumentNullException("Entity can't be null");

            //if (matchKey == null)
            //    throw new ArgumentNullException("MatchKey can't be null");

            //var tran = default(SqlTransaction);

            //using (var conn = new SqlConnection(dbKey.FromConnections()))
            //{
            //    conn.Open();

            //    var cmd = conn.CreateCommand();

            //    if (transaction)
            //    {
            //        tran = conn.BeginTransaction();
            //        cmd.Transaction = tran;
            //    }

            //    try
            //    {
            //        var dt = new DataTable();
            //        var table = entity.GetType().Name;
            //        var exists = false;
            //        var properties = GetEntityProperties(entity);
            //        var key = GetEntityPrimaryKey(entity, matchKey);
            //        var pk = key
            //            .SelectMany(s => new List<object>
            //            {
            //                s.Value
            //            })
            //            .ToList();

            //        cmd.CommandText = BuildUpdateQuery(table, properties, key);
            //        cmd.CommandTimeout = _cmdTimeout;

            //        if (string.IsNullOrWhiteSpace(cmd.CommandText))
            //        {
            //            cmd.CommandText = BuildSelectQuery(table, key);
            //            exists = ((int)cmd.ExecuteScalar() > 0);
            //        }
            //        else
            //        {
            //            using (var da = new SqlDataAdapter(cmd))
            //            {
            //                dt = new DataTable();
            //                da.Fill(dt);
            //            }
            //        }

            //        if (!exists && dt.Rows.Count == 0)
            //        {
            //            cmd.CommandText = BuildInsertQuery(insertKey, entity.GetType().Name, properties, key);

            //            if (string.IsNullOrWhiteSpace(cmd.CommandText))
            //                throw new ArgumentNullException("Invalid query. Nothing to be inserted.");

            //            using (var da = new SqlDataAdapter(cmd))
            //            {
            //                dt = new DataTable();
            //                da.Fill(dt);
            //            }

            //            pk.Clear();

            //            for (int i = 0; i < dt.Columns.Count; i++)
            //                pk.Add(dt.Rows[0][i]);
            //        }

            //        if (transaction)
            //            tran.Commit();

            //        return pk;
            //    }
            //    catch (Exception ex)
            //    {
            //        _log.Log(LogLevel.Error, "(" + cmd.CommandText + ")" + Environment.NewLine + ex.ToString());

            //        if (transaction)
            //        {
            //            try
            //            {
            //                tran.Rollback();
            //                return new List<object> { -1 };
            //            }
            //            catch (Exception fatalEx)
            //            {
            //                _log.Log(LogLevel.Fatal, fatalEx.ToString());
            //                throw;
            //            }
            //        }
            //        else
            //            throw;
            //    }
            //    finally
            //    {
            //        _cmdTimeout = _cmdGlobalTimeout;
            //    }
            //}
        }

        public List<object> InsertOrUpdate<T>(string dbKey, bool transaction, bool insertKey, bool allowTrigger, T entity, params Expression<Func<T, object>>[] matchKey)
            where T: class, new()
        {
            if (string.IsNullOrWhiteSpace(dbKey))
                throw new ArgumentNullException("Invalid ConnectionString");

            if (entity == null)
                throw new ArgumentNullException("Entity can't be null");

            if (matchKey == null)
                throw new ArgumentNullException("MatchKey can't be null");

            var tran = default(SqlTransaction);

            using (var conn = new SqlConnection(dbKey.FromConnections()))
            {
                conn.Open();

                var cmd = conn.CreateCommand();

                if (transaction)
                {
                    tran = conn.BeginTransaction();
                    cmd.Transaction = tran;
                }

                try
                {
                    var dt = new DataTable();
                    var table = entity.GetType().Name;
                    var exists = false;
                    var properties = GetEntityProperties(entity);
                    var key = GetEntityPrimaryKey(entity, matchKey);
                    var pk = key
                        .SelectMany(s => new List<object>
                        {
                            s.Value
                        })
                        .ToList();

                    cmd.CommandText = BuildUpdateQuery(table, properties, key, allowTrigger);
                    cmd.CommandTimeout = _cmdTimeout;

                    if (string.IsNullOrWhiteSpace(cmd.CommandText))
                    {
                        cmd.CommandText = BuildSelectQuery(table, key);
                        exists = ((int)cmd.ExecuteScalar() > 0);
                    }
                    else
                    {
                        using (var da = new SqlDataAdapter(cmd))
                        {
                            dt = new DataTable();
                            da.Fill(dt);
                        }
                    }

                    if (!exists && dt.Rows.Count == 0)
                    {
                        cmd.CommandText = BuildInsertQuery(insertKey, entity.GetType().Name, properties, key, allowTrigger);

                        if (string.IsNullOrWhiteSpace(cmd.CommandText))
                            throw new ArgumentNullException("Invalid query. Nothing to be inserted.");

                        using (var da = new SqlDataAdapter(cmd))
                        {
                            dt = new DataTable();
                            da.Fill(dt);
                        }

                        pk.Clear();

                        for (int i = 0; i < dt.Columns.Count; i++)
                            pk.Add(dt.Rows[0][i]);
                    }

                    if (transaction)
                        tran.Commit();

                    return pk;
                }
                catch (Exception ex)
                {
                    _log.Log(LogLevel.Error, "(" + cmd.CommandText + ")" + Environment.NewLine + ex.ToString());

                    if (transaction)
                    {
                        try
                        {
                            tran.Rollback();
                            return new List<object> { -1 };
                        }
                        catch (Exception fatalEx)
                        {
                            _log.Log(LogLevel.Fatal, fatalEx.ToString());
                            throw;
                        }
                    }
                    else
                        throw;
                }
                finally
                {
                    _cmdTimeout = _cmdGlobalTimeout;
                }
            }
        }

        public void BulkInsert<T>(List<T> entities, string targetTableName, List<string> excludedProperties = null) where T : class
        {
            BulkInsert(_globalDBKey, entities, targetTableName, excludedProperties);
        }

        public void BulkInsert<T>(string dbKey, List<T> entities, string targetTableName, List<string> excludedProperties = null) where T : class
        {
            var type = typeof(T);

            var entityProps = type
                .GetProperties()
                .Where(w =>
                    excludedProperties == null ||
                    (
                        excludedProperties != null &&
                        !excludedProperties.Any(a => a.Equals(w.Name, StringComparison.InvariantCultureIgnoreCase))
                    )
                )
                .ToArray();

            var dt = new DataTable();

            foreach (var p in entityProps)
                dt.Columns.Add(p.Name, p.PropertyType);

            foreach (var e in entities)
            {
                var rowsData = new object[entityProps.Count()];

                for (var i = 0; i < entityProps.Count(); i++)
                    rowsData[i] = entityProps[i].GetValue(e, null);

                dt.Rows.Add(rowsData);
            }

            var targetColumns = new List<string>();

            try
            {
                using (var conn = new SqlConnection(dbKey.FromConnections()))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_columns";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = _cmdTimeout;
                        cmd.Parameters.AddWithValue("@table_name", targetTableName);

                        var dr = cmd.ExecuteReader();

                        while (dr.Read())
                            targetColumns.Add((string)dr["column_name"]);

                        dr.Close();
                    }

                    using (var sbk = new SqlBulkCopy(conn))
                    {
                        foreach (var p in entityProps)
                            sbk.ColumnMappings.Add(
                                p.Name,
                                targetColumns.First(f => f.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase))
                            );

                        sbk.DestinationTableName = targetTableName;
                        sbk.WriteToServer(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Log(LogLevel.Fatal, string.Concat("(BulkInsert '", type.Name, "' into '", targetTableName, "')", Environment.NewLine, ex.ToString()));
                throw;
            }
            finally
            {
                _cmdTimeout = _cmdGlobalTimeout;
            }
        }

        #endregion


        #region Private Methods

        private List<Objects.EntityProperty> GetEntityProperties<T>(T entity) where T : class, new()
        {
            if (entity == null)
                return null;

            var type = typeof(T);

            return type
                .GetProperties()
                .Select(s => new Objects.EntityProperty
                {
                    Name = s.Name,
                    Value = s.GetValue(entity, null),
                    SqlValue = s.GetValue(entity, null).ToSqlValue()
                })
                .ToList();
        }

        private List<Objects.EntityPrimaryKey> GetEntityPrimaryKey<T>(T entity, params Expression<Func<T, object>>[] matchKey) where T : class, new()
        {
            if (entity == null)
                return null;

            if (matchKey == null || matchKey.Count() == 0)
                return new List<Objects.EntityPrimaryKey>();

            var type = typeof(T);
            var key = new List<Objects.EntityPrimaryKey>();

            foreach (var mk in matchKey)
            {
                var me = default(MemberExpression);
                var ue = (mk.Body as UnaryExpression);

                me = (ue == null
                    ? (mk.Body as MemberExpression)
                    : (ue.Operand as MemberExpression)
                );

                if (me == null)
                    throw new InvalidExpressionException();

                var pi = (me.Member as PropertyInfo);
                if (pi == null)
                    throw new InvalidExpressionException();

                key.Add(new Objects.EntityPrimaryKey
                {
                    Name = pi.Name,
                    Value = pi.GetValue(entity, null)
                });
            }

            return (
                key.Count == 0
                    ? default(List<Objects.EntityPrimaryKey>)
                    : key
            );
        }

        private string BuildQueryVariables(string query, out int count)
        {
            var n = 0;

            var build = Regex.Replace(
                query,
                @"\?(?=([^""]*""[^""]*"")*[^""]*$)",
                (m) =>
                {
                    return "@param" + n++;
                }
            );

            count = n;

            return build;
        }

        private string BuildInsertQuery(bool insertKey, string table, List<Objects.EntityProperty> properties, List<Objects.EntityPrimaryKey> key, bool allowTrigger)
        {
            var columns = string.Empty;
            var output = string.Empty;
            var values = string.Empty;

            //Para permitir el uso de triggers
            var tableVariable = string.Empty;
            var into = string.Empty;
            var sql = string.Empty;
            var selectOutput = string.Empty;

            var props = properties
                .Where(prop =>
                {
                    if (insertKey)
                        return true;
                    else
                        return !key.Any(pk => pk.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));
                })
                .ToList();

            if (props.Count == 0)
                return null;

            output = string.Join(
                ", ",
                key
                    .Select(pk => "INSERTED.[" + pk.Name + "]")
            );

            columns = string.Join(
                ", ",
                props
                    .Select(s => "[" + s.Name + "]")
            );

            values = string.Join(
                ", ",
                props
                    .Select(s => s.SqlValue)
            );

            if (allowTrigger)
            {
                //Generar tabla
                tableVariable = GetTableVariable(table, key);
                into = "INTO @OutputTable";
                selectOutput = "SELECT * FROM @OutputTable";
            }

            sql = "[DeclareTable] INSERT INTO [[Table]] ( [Columns] ) OUTPUT [Output] [Into] VALUES ( [Values] ); [SelectOutput]";

            return sql.Replace("[DeclareTable]", tableVariable)
                      .Replace("[Table]", table)
                      .Replace("[Columns]", columns)
                      .Replace("[Output]", output)
                      .Replace("[Into]", into)
                      .Replace("[Values]", values)
                      .Replace("[SelectOutput]", selectOutput);
        }

        private string BuildUpdateQuery(string table, List<Objects.EntityProperty> properties, List<Objects.EntityPrimaryKey> key, bool allowTrigger)
        {
            var columnsValues = string.Empty;
            var output = string.Empty;
            var filter = string.Empty;

            //Para permitir el uso de triggers
            var tableVariable = string.Empty;
            var into = string.Empty;
            var sql = string.Empty;
            var selectOutput = string.Empty;

            var props = properties
                .Where(prop => !key.Any(pk => pk.Name.Equals(prop.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (props.Count == 0)
                return null;

            columnsValues = string.Join(
                ", ",
                props
                    .Select(prop => "[" + prop.Name + "]=" + prop.SqlValue)
            );

            output = string.Join(
                ", ",
                key
                    .Select(pk => "INSERTED.[" + pk.Name + "]")
            );

            filter = string.Join(
                " AND ",
                key
                    .Select(pk => "[" + pk.Name + "]=" + pk.Value.ToSqlValue())
            );

            if (allowTrigger)
            {
                //Generar tabla
                tableVariable = GetTableVariable(table, key);
                into = "INTO @OutputTable";
                selectOutput = "SELECT * FROM @OutputTable";
            }

            return "[DeclareTable] UPDATE [[Table]] SET [ColumnsValues] OUTPUT [Output] [Into] WHERE [Filter]; [SelectOutput]"
                .Replace("[DeclareTable]", tableVariable)
                .Replace("[Table]", table)
                .Replace("[ColumnsValues]", columnsValues)
                .Replace("[Output]", output)
                .Replace("[Into]", into)
                .Replace("[Filter]", filter)
                .Replace("[SelectOutput]", selectOutput);
        }

        private string BuildSelectQuery(string table, List<Objects.EntityPrimaryKey> key)
        {
            var filter = string.Empty;

            filter = string.Join(
                " AND ",
                key
                    .Select(pk => "[" + pk.Name + "]=" + pk.Value.ToSqlValue())
            );

            return "SELECT COUNT(1) FROM [[Table]] WHERE [Filter];"
                .Replace("[Table]", table)
                .Replace("[Filter]", filter);
        }

        private string BuildColumnDeclaration(List<Objects.EntityPrimaryKeyOutput> pkOutput, List<Objects.EntityPrimaryKey> keys)
        {
            List<string> columns = new List<string>();

            keys.ForEach(key =>
            {
                string cDef = string.Empty;

                Objects.EntityPrimaryKeyOutput column = pkOutput.FirstOrDefault(f => f.ColumnName.ToLowerInvariant() == key.Name.ToLowerInvariant());
                if (column == null)
                    throw new Exception($"La columna {key.Name} no está definida como parte de la clave primaria");

                cDef = $"{column.ColumnName}  {column.DataType}";

                if (column.DataType.EndsWith("CHAR", StringComparison.InvariantCultureIgnoreCase) || column.DataType.EndsWith("BINARY", StringComparison.InvariantCultureIgnoreCase))
                    cDef += $"({(column.CharMaxLength == -1 ? "MAX": column.CharMaxLength.ToString())})";
                else if(column.DataType.Equals("DECIMAL", StringComparison.InvariantCultureIgnoreCase))
                    cDef += $"({column.NumericPrecision}, {column.NumericPresicionRadix})";

                columns.Add(cDef);
            });

            return string.Join(",\n", columns);
        }

        private string GetTableVariable(string table, List<Objects.EntityPrimaryKey> keys)
        {
            string pkSql = @" SELECT    c.ORDINAL_POSITION,
                                        c.COLUMN_NAME,
                                        c.DATA_TYPE,
                                        CAST(ISNULL(c.CHARACTER_MAXIMUM_LENGTH,0) AS INT) CHARACTER_MAXIMUM_LENGTH,
                                        CAST(ISNULL(c.NUMERIC_PRECISION,0)AS INT) NUMERIC_PRECISION,
                                        CAST(ISNULL(c.NUMERIC_PRECISION_RADIX,0) AS INT) NUMERIC_PRECISION_RADIX
                                FROM INFORMATION_SCHEMA.columns C
                                WHERE c.TABLE_NAME = ? 
	                              AND EXISTS 
	                                (    
                                        SELECT 1
                                          FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu 
                                         WHERE cu.TABLE_NAME = c.TABLE_NAME
			                               AND cu.TABLE_SCHEMA = c.TABLE_SCHEMA
			                               AND c.COLUMN_NAME = cu.COLUMN_NAME 
			                               AND (OBJECTPROPERTY(OBJECT_ID(cu.CONSTRAINT_SCHEMA + '.' + QUOTENAME(cu.CONSTRAINT_NAME)), 'IsPrimaryKey') = 1) 
	                                )
                                ORDER BY 1";

            List<Objects.EntityPrimaryKeyOutput> pkColumnsInfo = Execute<Objects.EntityPrimaryKeyOutput>(pkSql, new object[] { table });

            if (pkColumnsInfo == null || pkColumnsInfo.Count == 0)
                throw new Exception("No se obtuvieron las columnas de clave primaria");

            if (keys.Count != pkColumnsInfo.Count)
                throw new Exception("La definición de clave primaria de la tabla no coincide con proporcionada por el usuario");

            string tableVariable = @"   
                                    SET NOCOUNT ON
                                    DECLARE @OutputTable TABLE
                                    (
                                        [ColumnInfo]
                                    );".Replace("[ColumnInfo]", BuildColumnDeclaration(pkColumnsInfo, keys));
            return tableVariable;
        }

        #endregion
    }
}
