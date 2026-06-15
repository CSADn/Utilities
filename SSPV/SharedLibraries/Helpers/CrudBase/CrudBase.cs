using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers
{
    /// <summary>
    /// Crud base genérico 
    /// </summary>
    /// <typeparam name="T">Instancia del Crud</typeparam>
    /// <typeparam name="E">Entidad del Crud</typeparam>
    public abstract class CrudBase<T, E> : GenericSingleton<T> 
        where T : class
        where E : class, new()
    {
        #region Atributos
        
        /// <summary>
        /// DataModel
        /// </summary>
        protected DataModel _dataModel = null;

        /// <summary>
        /// Nombre de la tabla
        /// </summary>
        protected string _tableName = string.Empty;

        /// <summary>
        /// Log
        /// </summary>
        protected NLog.Logger _logger;

        #endregion

        #region Properties

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
        /// Columna por la que se filtrara en los metodos de devolucion (Ej. Estado <> 'B')
        /// </summary>
        public virtual string FilterColumnName { get; set; }

        /// <summary>
        /// Valor que filtrara
        /// </summary>
        public virtual object FilterValue { get; set; }

        /// <summary>
        /// Propiedades que se encriptan/desencriptan al obtener o guardar
        /// </summary>
        public virtual Expression<Func<E, object>>[] CryptProps { get; set;}

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        protected CrudBase()
        {
            this._dataModel = DataModel.Instance;
            this._logger = NLog.LogManager.CreateNullLogger();
            this.Init();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Obtener propiedad por nombre
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected PropertyInfo GetPropertyInfo(string name)
        {
            return typeof(E).GetProperties().FirstOrDefault(f => f.Name.ToLowerInvariant() == name.ToLowerInvariant());
        }
                
        /// <summary>
        /// Obtener la expresion para las consultas (Ej. pk => pk.Propiedad)
        /// </summary>
        /// <param name="propInfo">Propiedad (Campo de la entidad Ej. IdName)</param>
        /// <param name="parameter">Parametro (Ej. pk)</param>
        /// <returns></returns>
        protected Expression<Func<E, object>> MakeExpressionFunction(PropertyInfo propInfo, string parameter)
        {

            ParameterExpression param = Expression.Parameter(typeof(E), parameter);
            
            MemberExpression prop = Expression.Property(param, propInfo.Name);
            UnaryExpression unaryExpression = Expression.Convert(prop, typeof(object));

            return Expression.Lambda<Func<E, object>>(unaryExpression, param);
        }
        
        /// <summary>
        /// Obtener una expresion con la propiedad a utilizar.
        /// </summary>
        /// <param name="parameter">parametro que se quiere utilizar en la expresion (Ej. pk =>)</param>
        /// <returns></returns>
        protected Expression<Func<E, object>> MakeExpressionFromEntity(string parameter, string entityPropName)
        {
            PropertyInfo propInfo = this.GetPropertyInfo(entityPropName);

            if (propInfo == null)
                return null;

            return MakeExpressionFunction(propInfo, parameter);
        }
        
        #region Select

        /// <summary>
        /// Obtener todo como lista
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="filterColumnName">Columna de filtro</param>
        /// <param name="filterValue">Valor del filtro</param>
        /// <param name="decriptProps">Campos que se deben desencriptar</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns>Lista de entidades</returns>
        protected List<E> GetAll(string tableName, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            List<E> retList = null;

            string query = @"SELECT * FROM #TABLE_NAME".Replace("#TABLE_NAME", tableName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" WHERE #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);

                retList = _dataModel.Execute<E>(query, filterValue);
            }
            else
                retList = _dataModel.Execute<E>(query);

            if (retList != null && retList.Count > 0 && decriptProps != null && decriptProps.Length > 0)
                return retList.EntityCypher(Utilities.CypherAction.Decrypt, decriptProps);

            return retList;
        }

        /// <summary>
        /// Obtener todo para tablas con una Foreign key a una tabla maestra
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="fkColumnName">Columna de la clave foranea</param>
        /// <param name="idFk">id de la clave foranea</param>
        /// <param name="filterColumnName">Columna de filtro</param>
        /// <param name="filterValue">Valor de filtro</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns>Lista de entidades</returns>
        protected List<E> GetAll(string tableName, string fkColumnName, int idFk, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            List<E> retList = null;

            string query = @"SELECT * FROM #TABLE_NAME WHERE #ID_FK = ?".Replace("#TABLE_NAME", tableName).Replace("#ID_FK", fkColumnName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);

                retList = _dataModel.Execute<E>(query, idFk, filterValue);
            }
            else
                retList = _dataModel.Execute<E>(query, idFk);

            if (retList != null && retList.Count > 0 && decriptProps != null && decriptProps.Length > 0)
                return retList.EntityCypher(Utilities.CypherAction.Decrypt, decriptProps);

            return retList;
        }

        /// <summary>
        /// Obtener por id de clave primaria
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="id">id de la clave primaria</param>
        /// <param name="filterColumnName">Columna de filtro</param>
        /// <param name="filterValue">Valor de filtro</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns>Lista de entidades</returns>
        protected E GetById(string tableName, string pkColumnName, int id, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(pkColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y campo de clave primaria");

            E retValue = null;
            object[] parameters = null;

            string query = @"SELECT * FROM #TABLE_NAME WHERE #ID_NAME = ?".Replace("#TABLE_NAME", tableName)
                                                                          .Replace("#ID_NAME", pkColumnName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                parameters = new object[] { id, filterValue };
            }
            else
                parameters = new object[] { id };

            if (decriptProps != null && decriptProps.Length > 0)
                retValue = _dataModel.Execute<E>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, decriptProps).FirstOrDefault();
            else
                retValue = _dataModel.Execute<E>(query, parameters).FirstOrDefault();

            return retValue;
        }

        /// <summary>
        /// Obtener por clave primaria y clave foranea
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="idPK">Id de la clave primaria</param>
        /// <param name="fkColumnName">Nombre de la clave foranea</param>
        /// <param name="idFK">Id de la clave foranea</param>
        /// <param name="filterColumnName">Columna de filtro</param>
        /// <param name="filterValue">Valor de filtro</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns>Entidad</returns>
        protected E GetById(string tableName, string pkColumnName, int idPK, string fkColumnName, int idFK, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(pkColumnName) || string.IsNullOrWhiteSpace(fkColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla, campo de Id y campor de la FK");

            E retValue = null;
            object[] parameters = null;

            string query = @"SELECT * FROM #TABLE_NAME WHERE #ID_NAME = ? AND #ID_FK = ?".Replace("#TABLE_NAME", tableName)
                                                                                         .Replace("#ID_FK", fkColumnName)
                                                                                         .Replace("#ID_NAME", pkColumnName);
            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                parameters = new object[] { idPK, idFK, filterValue };
            }
            else
                parameters = new object[] { idPK, idFK};

            if (decriptProps != null && decriptProps.Length > 0)
                retValue = _dataModel.Execute<E>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, decriptProps).FirstOrDefault();
            else
                retValue = _dataModel.Execute<E>(query, parameters).FirstOrDefault();

            return retValue;
        }

        /// <summary>
        /// Obtener por lista de id
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="idList">Lista de valores de clave primaria</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns>Lista de entidades</returns>
        protected List<E> GetByIdList(string tableName, string pkColumnName, int[] idList, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(pkColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y campo de Id");

            List<E> retList = null;
            
            string query = @"SELECT * FROM #TABLE_NAME WHERE #ID_NAME IN ({0})".Replace("#TABLE_NAME", tableName)
                                                                               .Replace("#ID_NAME", pkColumnName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                retList = _dataModel.Execute<E>(string.Format(query, string.Join(", ", idList)), filterValue);
            }
            else
                retList = _dataModel.Execute<E>(string.Format(query, string.Join(", ", idList)));

            if(retList != null && decriptProps != null && decriptProps.Length > 0)
                return retList.EntityCypher(Utilities.CypherAction.Decrypt, decriptProps);

            return retList;
        }

        /// <summary>
        /// Obtener una entidad por codigo
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="codColumnName">Nombre de la columna de codigo</param>
        /// <param name="codValue">Valor del codigo</param>
        /// <param name="filterColumnName">Columna de filtro</param>
        /// <param name="filterValue">Valor de filtro</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns>Entidad</returns>
        protected E GetByCod(string tableName, string codColumnName, string codValue, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(codColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y campo de código");

            E retValue = null;
            object[] parameters = null;

            string query = @"SELECT * FROM #TABLE_NAME WHERE #COD_NAME = ?".Replace("#TABLE_NAME", tableName).Replace("#COD_NAME", codColumnName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                parameters = new object[] { codValue, filterValue };
            }
            else
                parameters = new object[] { codValue };

            if (decriptProps != null && decriptProps.Length > 0)
                retValue = _dataModel.Execute<E>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, decriptProps).FirstOrDefault();
            else
                retValue = _dataModel.Execute<E>(query, parameters).FirstOrDefault();

            return retValue;
        }

        /// <summary>
        /// Obtener una lista de entidades por nombre de columna
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="colName">Nombre de la columna</param>
        /// <param name="value">Valor de la columna</param>
        /// <param name="filterColumnName">Columna de filtro</param>
        /// <param name="filterValue">Valor de filtro</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns></returns>
        protected List<E> GetBy(string tableName, string colName, object value, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(colName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y la columna para obtener los datos");

            List<E> retValue = null;
            object[] parameters = null;

            string query = @"SELECT * FROM #TABLE_NAME WHERE #COL_NAME = ?".Replace("#TABLE_NAME", tableName).Replace("#COL_NAME", colName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                parameters = new object[] { value, filterValue };
            }
            else
                parameters = new object[] { value };

            if (decriptProps != null && decriptProps.Length > 0)
                retValue = _dataModel.Execute<E>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, decriptProps);
            else
                retValue = _dataModel.Execute<E>(query, parameters);

            return retValue;
        }

        /// <summary>
        /// Obtener una lista de entidades por nombre de columnas
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="colNames">Columnas por las que se desea hacer la consulta</param>
        /// <param name="values">Valores por los que se desea hacer la consulta</param>
        /// <param name="filterColumnName">Columna de filtro fijo</param>
        /// <param name="filterValue">Valor del filtro fijo</param>
        /// <param name="decriptProps">Columnas para devolver desencriptadas</param>
        /// <returns></returns>
        protected List<E> GetBy(string tableName, string[] colNames, object[] values, string filterColumnName = null, object filterValue = null, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new NullReferenceException("Debe establecer el nombre de tabla");

            if(colNames == null || colNames.Length == 0)
                throw new NullReferenceException("Debe establecer las columnas por las que desea obtener los datos");

            if(values == null || values.Length == 0)
                throw new NullReferenceException("Debe establecer los valores de las columnas por las que desea obtener los datos");

            List<E> retValue = null;
            object[] parameters = null;

            string query = @"SELECT * FROM #TABLE_NAME WHERE ".Replace("#TABLE_NAME", tableName);

            for (int i = 0; i < colNames.Length; i++)
            {
                query += colNames[i].Trim() + " = ?";

                if (i < colNames.Length - 1)
                    query += " AND ";
            }

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                parameters = values.Concat(new object[] { filterValue }).ToArray();
            }
            else
                parameters = values;

            if (decriptProps != null && decriptProps.Length > 0)
                retValue = _dataModel.Execute<E>(query, parameters).EntityCypher(Utilities.CypherAction.Decrypt, decriptProps);
            else
                retValue = _dataModel.Execute<E>(query, parameters);

            return retValue;
        }

        protected List<E> GetBy(string tableName, string where, List<object> parameters, params Expression<Func<E, object>>[] decriptProps)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new NullReferenceException("Debe establecer el nombre de tabla");

            if(string.IsNullOrWhiteSpace(where))
                throw new NullReferenceException("Debe establecer la clausula where para obtener los datos");

            List<E> retValue = null;
            string query = @"SELECT * FROM #TABLE_NAME WHERE ".Replace("#TABLE_NAME", tableName);
            query += where;

            if (decriptProps != null && decriptProps.Length > 0)
                retValue = _dataModel.Execute<E>(query, parameters.ToArray()).EntityCypher(Utilities.CypherAction.Decrypt, decriptProps);
            else
                retValue = _dataModel.Execute<E>(query, parameters.ToArray());

            return retValue;
        }

        protected int GetCountBy(string tableName, string colName, object value, string filterColumnName = null, object filterValue = null)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(colName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y la columna para obtener los datos");

            object[] parameters = null;

            string query = @"SELECT COUNT(1) FROM #TABLE_NAME WHERE #COL_NAME = ?".Replace("#TABLE_NAME", tableName).Replace("#COL_NAME", colName);

            if (!string.IsNullOrWhiteSpace(filterColumnName) && filterValue != null)
            {
                query += @" AND #FILTER_COLUMN <> ?".Replace("#FILTER_COLUMN", filterColumnName);
                parameters = new object[] { value, filterValue };
            }
            else
                parameters = new object[] { value };

            return _dataModel.GetValue<int>(query, 0, false, parameters);
        }

        #endregion

        #region CRUD

        /// <summary>
        /// Actualizar o insertar la entidad
        /// </summary>
        /// <param name="entity">Entidad</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <returns></returns>
        protected List<object> AddOrUpdate(E entity, string pkColumnName)
        {
            return _dataModel.InsertOrUpdate(entity, MakeExpressionFromEntity("Pk", pkColumnName));
        }

        /// <summary>
        /// Actualizar o insertar una entidad
        /// </summary>
        /// <param name="entity">Entidad</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="useTransaction">true = Usar transacción</param>
        /// <returns></returns>
        protected List<object> AddOrUpdate(E entity, string pkColumnName, bool useTransaction = false, bool insertKey = false, params Expression<Func<E, object>>[] encriptProps)
        {
            if (entity != null && encriptProps != null && encriptProps.Length > 0)
                entity = Utilities.EntityCypher(entity, Utilities.CypherAction.Crypt, encriptProps);

            //if (!useTransaction)
            //    return AddOrUpdate(entity, pkColumnName);

            return _dataModel.InsertOrUpdate(useTransaction, insertKey, entity, MakeExpressionFromEntity("Pk", pkColumnName));
        }

        /// <summary>
        /// Eliminar por clave primaria
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="id">Valor de la clave primaria</param>
        /// <returns></returns>
        protected bool Delete(string tableName, string pkColumnName, int id)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(pkColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y campo de Id");

            string query =  @"DELETE FROM #TABLE_NAME WHERE #ID_NAME = ?".Replace("#TABLE_NAME", tableName)
                                                                         .Replace("#ID_NAME", pkColumnName);

            return _dataModel.ExecuteNonQuery(query, id) != -1;

        }

        /// <summary>
        /// Eliminar por clave primaria
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="id">Valor de la clave primaria</param>
        /// <param name="logicDelete">True, Baja logica</param>
        /// <param name="filterColumnName">Columna de baja logica</param>
        /// <param name="filterValue">Valor de la baja logica</param>
        /// <returns></returns>
        protected bool Delete(string tableName, string pkColumnName, int id, bool logicDelete = false, string filterColumnName = null, params object[] filterValue)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(pkColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla y campo de clave primaria");

            string query = string.Empty;

            if (!logicDelete)
                return Delete(tableName, pkColumnName, id);
            else
            {
                if (string.IsNullOrWhiteSpace(filterColumnName) || filterValue == null || filterValue.Length == 0)
                    throw new NullReferenceException("Debe establecer la columna por la cual se hara la baja lógica y su valor");

                query = "UPDATE #TABLE_NAME SET #FILTER_COLUMN = ? WHERE #ID_NAME = ?".Replace("#TABLE_NAME", tableName)
                                                                                      .Replace("#FILTER_COLUMN", filterColumnName)
                                                                                      .Replace("#ID_NAME", pkColumnName);

                return _dataModel.ExecuteNonQuery(query, filterValue.Concat(new object[] { id }).ToArray()) != -1;
            }
        }

        /// <summary>
        /// Eliminar por clave primaria
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="id">Valor de la clave primaria</param>
        /// <param name="logicDelete">True, Baja logica</param>
        /// <param name="deleteParent">True, Elimina tablas relacionadas</param>
        /// <param name="parentTables">Lista de entidades a eliminar</param>
        /// <param name="filterColumnName">Columna de baja logica</param>
        /// <param name="filterValue">Valor de la baja logica</param>
        /// <returns></returns>
        protected bool Delete(string tableName, string pkColumnName, int id, bool logicDelete = false, bool deleteParent = false, List<Type> parentTables = null, string filterColumnName = null,  params object[] filterValue)
        {
            if (deleteParent && parentTables != null && parentTables.Count > 0)
            {
//                string sqlFKtables = @"SELECT   object_name(parent_object_id) FkTable,
//                                                object_name(referenced_object_id) PkTable,
//                                                name FkName
//                                         FROM sys.foreign_keys
//                                        WHERE referenced_object_id = object_id(?)";

//                List<CrudFKTable> fkList = _dataModel.Execute<CrudFKTable>(sqlFKtables, tableName);

                string sqlToDelete = @"DELETE FROM #TABLE_NAME WHERE #ID_NAME = ?";
                parentTables.ForEach(fk =>
                {
                    string query = sqlToDelete.Replace("#TABLE_NAME", fk.Name.ToUpperInvariant()).Replace("#ID_NAME", pkColumnName);
                    _dataModel.ExecuteNonQuery(query, id);
                });
            }

            return this.Delete(tableName, pkColumnName, id, logicDelete, filterColumnName, filterValue);
        }

        /// <summary>
        /// Eliminar por valor de clave primaria y clave foranea
        /// </summary>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="pkColumnName">Nombre de la clave primaria</param>
        /// <param name="idPK">Valor de la clave primaria</param>
        /// <param name="fkColumnName">Nombre de la clave foranea</param>
        /// <param name="idFK">Valor de la clave foranea</param>
        /// <returns></returns>
        protected bool Delete(string tableName, string pkColumnName, int idPK, string fkColumnName, int idFK)
        {
            if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(pkColumnName) || string.IsNullOrWhiteSpace(fkColumnName))
                throw new NullReferenceException("Debe establecer el nombre de tabla, campo de clave primaria y campor de clave foranea");

            string query = @"DELETE FROM #TABLE_NAME WHERE #ID_NAME = ? AND #ID_FK = ?".Replace("#TABLE_NAME", tableName)
                                                                                       .Replace("#ID_NAME", pkColumnName)
                                                                                       .Replace("#ID_FK", fkColumnName);

            return _dataModel.ExecuteNonQuery(query, idPK, idFK) != -1;
        }

        #endregion

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Metodo opcional para inicializar 
        /// </summary>
        protected virtual void Init() {}

        #endregion
    }
}
