using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers
{
    /// <summary>
    /// Clase base con metodos genericos para los cruds
    /// </summary>
    public abstract class FacadeBase
    {
        #region Atributos privados

        private static readonly string _crudDll = "Crud.dll";

        private static Assembly _crudAssembly = null;
        private static Dictionary<string, Type> _dicTypesCrud = null;
        private static Dictionary<Type, object> _dicCrudInstance = null;
        private NLog.Logger _log = null;

        #endregion

        #region Propiedades

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public FacadeBase()
        {
            _log = NLog.LogManager.GetCurrentClassLogger();
            string path = string.Empty;

            try
            {
                if (_crudAssembly == null)
                {
                     path = Path.GetDirectoryName(new Uri(Assembly.GetCallingAssembly().EscapedCodeBase).LocalPath);
                    _crudAssembly = Assembly.LoadFrom(Path.Combine(path, _crudDll));
                }

                if (_crudAssembly == null)
                {
                    throw new NullReferenceException("_crudAssembly == null");
                }

                //Cargo el diccionario de tipos
                if (_dicTypesCrud == null)
                    _dicTypesCrud = _crudAssembly
                                    .GetTypes()
                                    .Where(w => w.MemberType == MemberTypes.TypeInfo 
                                                && w.FullName.StartsWith("Crud"))
                                                    .ToDictionary(k => k.Name, v => v);

                if (_dicCrudInstance == null)
                    _dicCrudInstance = new Dictionary<Type, object>();

            }
            catch (Exception ex)
            {
                _log.Log(NLog.LogLevel.Error, string.Format("No se pudo cargar el ensamblado de Cruds. Ruta: {0}", string.IsNullOrWhiteSpace(path) ? "No establecida" : path));
                _log.Log(NLog.LogLevel.Error, ex.ToString());

                throw ex;
            }
        }

        #endregion

        #region Metodos privados

        /// <summary>
        /// Obtener la instancia del crud
        /// </summary>
        /// <param name="entityType">Tipo de la entidad a obtener el CRUD</param>
        /// <returns>Instancia de la entidad</returns>
        private KeyValuePair<Type, object> GetInstance(Type entityType)
        {
            if (entityType == null) return default(KeyValuePair<Type, object>);

            Type crudType = null;

            //Obtengo el tipo del crud segun la entidad
            if (_dicTypesCrud.TryGetValue(entityType.Name, out crudType))
            {
                if (!_dicCrudInstance.ContainsKey(crudType))
                {
                    PropertyInfo prop = crudType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    if (prop == null) return default(KeyValuePair<Type, object>);

                    _dicCrudInstance.Add(crudType, prop.GetValue(null));
                }

                return _dicCrudInstance.FirstOrDefault(f => f.Key.Equals(crudType));
            }
            else
            {
                string error = string.Format("No existe el Crud para la entidad {0}", entityType.Name);
                _log.Log(NLog.LogLevel.Error, error);

                throw new NullReferenceException(error);
            }
        }

        /// <summary>
        /// Invocar metodo
        /// </summary>
        /// <typeparam name="E">Entidad</typeparam>
        /// <param name="methodName">Nombre del metodo a invocar</param>
        /// <returns></returns>
        private List<E> GetListInvoke<E>(string methodName, params object[] parameters)
            where E : class
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return null;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            if (methods == null) return null;

            int parameterCount = 0;
            if (parameters != null && parameters.Count() > 0)
                parameterCount = parameters.Count();

            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName) && f.GetParameters().Count() == parameterCount);
            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            if (parameterCount == 0)
                return method.Invoke(instanciaCrud.Value, null) as List<E>;
            else
                return method.Invoke(instanciaCrud.Value, parameters) as List<E>;
        }

        private int GetCountInvoke<E>(string methodName, string search)
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return -1;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            if (methods == null) return -1;

            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName));
            if (method == null) return -1;

            return Convert.ToInt32(method.Invoke(instanciaCrud.Value, new object[] { search }));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private E GetInvoke<E>(string methodName, params object[] parameters)
            where E : class
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return null;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            if (methods == null) return null;

            int parameterCount = 0;
            if (parameters != null && parameters.Count() > 0)
                parameterCount = parameters.Count();

            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName) && f.GetParameters().Count() == parameterCount);
            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            if (parameterCount == 0)
                return method.Invoke(instanciaCrud.Value, null) as E;
            else
                return method.Invoke(instanciaCrud.Value, parameters) as E;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="methodName"></param>
        /// <param name="entity"></param>
        /// <param name="useTransaction"></param>
        /// <param name="insertKey"></param>
        /// <returns></returns>
        private List<object> AddOrUpdateInvoke<E>(string methodName, E entity, bool useTransaction = true, bool insertKey = false, bool allowTriggers = false)
            where E : class
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return null;

            Type crudBaseType = instanciaCrud.Key.BaseType;

            if (crudBaseType == null) return null;
            int parametersCount = 2;

            if (crudBaseType.GetGenericTypeDefinition() == typeof(CrudManager<,>) || crudBaseType.BaseType.GetGenericTypeDefinition() == typeof(CrudManager<,>))
                parametersCount += 2;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName) && f.GetParameters().Count() == parametersCount);

            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            List<object> parameters = new List<object>() { entity, useTransaction };

            //if (parametersCount == 3)
            //    parameters.Add(insertKey);

            if (parametersCount == 4)
            {
                parameters.Add(insertKey);
                parameters.Add(allowTriggers);
            }

            return method.Invoke(instanciaCrud.Value, parameters.ToArray()) as List<object>;
        }

        private List<object> AddOrUpdateCustomInvoke<E>(string methodName, E entity, bool insertKey, bool allowTrigger) where E : class
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return null;

            Type crudBaseType = instanciaCrud.Key.BaseType;

            if (crudBaseType == null) return null;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName));

            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            List<object> parameters = new List<object>() { entity, insertKey, allowTrigger };

            try
            {
                return method.Invoke(instanciaCrud.Value, parameters.ToArray()) as List<object>;
            }
            catch(TargetInvocationException tiex)
            {
                if (tiex.InnerException != null)
                    throw tiex.InnerException;
                throw;
            }
        }

        private void AddListInvoke<E>(string methodName, List<E> entities)
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return;

            Type crudBaseType = instanciaCrud.Key.BaseType;

            if (crudBaseType == null) return;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName));

            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            List<object> parameters = new List<object>() { entities };

            method.Invoke(instanciaCrud.Value, parameters.ToArray());
        }

        private void UpdateListInvoke<E>(string methodName, List<E> entities, bool insertkey = false)
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return;

            Type crudBaseType = instanciaCrud.Key.BaseType;

            if (crudBaseType == null) return;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName));

            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            List<object> parameters = new List<object>() { entities, insertkey };

            method.Invoke(instanciaCrud.Value, parameters.ToArray());
        }

        /// <summary>
        /// Eliminar por Id
        /// </summary>
        /// <typeparam name="E">Entidad</typeparam>
        /// <param name="methodName">Nombre del metodo</param>
        /// <param name="id">id</param>
        /// <param name="logicDelete">true, Eliminar Lógico</param>
        /// <returns></returns>
        private bool DeleteInvoke<E>(string methodName, object id, bool logicDelete = false) where E : class
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return false;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            //Delete para un id y baja logica
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName)
                                                         && f.GetParameters().Count() == 2
                                                         && f.GetParameters()[0].ParameterType == typeof(object)
                                                         && f.GetParameters()[1].ParameterType == typeof(bool));

            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            return Convert.ToBoolean(method.Invoke(instanciaCrud.Value, new object[] { id, logicDelete }));
        }

        private bool DeleteByListInvoke<E>(string methodName, List<int> pkValues, bool logicDelete)
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return false;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName)
                                                         && f.GetParameters().Count() == 2
                                                         && f.GetParameters()[0].ParameterType == pkValues.GetType()
                                                         && f.GetParameters()[1].ParameterType == typeof(bool));
            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            return Convert.ToBoolean(method.Invoke(instanciaCrud.Value, new object[] { pkValues, logicDelete }));
        }

        private bool DeleteByListInvoke<E>(string methodName, List<string> pkValues, bool logicDelete)
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>))) return false;

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName) 
                                                         && f.GetParameters().Count() == 2
                                                         && f.GetParameters()[0].ParameterType == pkValues.GetType()
                                                         && f.GetParameters()[1].ParameterType == typeof(bool));
            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            return Convert.ToBoolean(method.Invoke(instanciaCrud.Value, new object[] { pkValues, logicDelete }));
        }

        private bool ValidateInvoke<E>(string methodName, params object[] parameters)
        {
            KeyValuePair<Type, object> instanciaCrud = GetInstance(typeof(E));

            if (instanciaCrud.Equals(default(KeyValuePair<Type, object>)))
                throw new ArgumentNullException();

            MethodInfo[] methods = instanciaCrud.Key.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            if (methods == null)
                throw new NullReferenceException($"El crud {instanciaCrud.Key.Name} no posee métodos públicos");

            MethodInfo method = methods.FirstOrDefault(f => f.Name.Equals(methodName));
            if (method == null)
                throw new NullReferenceException($"Metodo {methodName} inexistente en el crud {instanciaCrud.Key.Name}");

            return (bool)method.Invoke(instanciaCrud.Value, parameters);
        }

        #endregion

        #region Metodos públicos

        #region Select

        #region GetAll

        /// <summary>
        /// Obtener todo
        /// </summary>
        /// <typeparam name="E">Tipado de la Entidad</typeparam>
        /// <returns>Lista de entidades</returns>
        public List<E> GetAll<E>() where E : class
        {
            return GetListInvoke<E>("GetAll");
        }

        /// <summary>
        /// Obtener todo por alguna clave foranea (Para entidades secundarias)
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <returns>Lista de entidades</returns>
        public List<E> GetAll<E>(Expression<Func<E, bool>> whereExpr) where E : class
        {
            return GetListInvoke<E>("GetAll", new object[] { whereExpr });
        }

        /// <summary>
        /// Obtener todo para DataTable
        /// </summary>
        /// <typeparam name="DT"></typeparam>
        /// <returns></returns>
        public List<DT> GetAllDT<DT>() where DT : class
        {
            return GetListInvoke<DT>("GetAllDT");
        }

        /// <summary>
        /// Obtener todo con entidades custom
        /// </summary>
        /// <typeparam name="E">Tipo de la entidad</typeparam>
        /// <returns></returns>
        public List<E> GetAllCustom<E>() where E : class
        {
            return GetListInvoke<E>("GetAllCustom");
        }

        public List<E> GetAllCustom<E, T>(Expression<Func<T, bool>> whereExpr) 
            where E : class 
            where T : class
        {
            return GetListInvoke<E>("GetAllCustom", new object[] { whereExpr });
        }

        #endregion

        #region Paginado

        /// <summary>
        /// Obtener todo paginado
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="search"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<E> GetAll<E>(int skip, int take, string search, params string[] order) where E : class
        {
            return GetListInvoke<E>("GetAll", new object[] { skip, take, search, order });
        }

        /// <summary>
        /// Obtener cantidad de registros para el GetAll paginado
        /// </summary>
        /// <typeparam name="E">Tipo de la entida</typeparam>
        /// <param name="search">Valor por el que se filtrará la consulta</param>
        /// <returns></returns>
        public int GetAllCount<E>(string search = null) where E : class
        {
            return GetCountInvoke<E>("GetAllCount", search);
        }

        public List<E> GetAllDT<E>(int pageNumber, int rowsPerPage, string search, params string[] orderFields) where E:class
        {
            return GetListInvoke<E>("GetAllDT", new object[] { pageNumber, rowsPerPage, search, orderFields });
        }

        public List<E> GetAllDTCustom<E>(int pageNumber, int rowsPerPage, string search, params string[] orderFields) where E : class
        {
            return GetListInvoke<E>("GetAllDTCustom", new object[] { pageNumber, rowsPerPage, search, orderFields });
        }

        #endregion

        #region Por Id

        /// <summary>
        /// Obtener por Id Generico
        /// </summary>
        /// <typeparam name="E">Tipado de la entidad</typeparam>
        /// <param name="id">id por el cual se obtendrá</param>
        /// <returns>Entidad</returns>
        public E GetById<E>(object id) where E : class
        {
            return GetInvoke<E>("GetById", new object[] { id });
        }

        /// <summary>
        /// Obtener Entidad Custom por Id
        /// </summary>
        /// <typeparam name="E">Tipo de la entidad custom</typeparam>
        /// <param name="id">Valor de campo id</param>
        /// <returns></returns>
        public E GetCustomById<E>(object id) where E : class
        {
            return GetInvoke<E>("GetCustomById", new object[] { id });
        }

        /// <summary>
        /// Obtener por listad de Ids
        /// </summary>
        /// <typeparam name="E">Tipo de la enitdad</typeparam>
        /// <param name="idList">Lista de Ids</param>
        /// <returns></returns>
        public List<E> GetByIdList<E>(List<int> idList) where E : class
        {
            return GetListInvoke<E>("GetByIdList", new object[] { idList });
        }

        /// <summary>
        /// Obtener mediante una expresion lambda
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="whereExpr">Expresion para obtener los datos</param>
        /// <param name="useFixedFilters">Usar los filtros fijos del crud</param>
        /// <returns></returns>
        public List<E> GetBy<E>(Expression<Func<E, bool>> whereExpr, bool useFixedFilters = true) where E : class
        {
            return GetListInvoke<E>("GetBy", new object[] { whereExpr, useFixedFilters });
        }

        #endregion

        #region Validacion

        public bool ValidateBy<E>(Expression<Func<E, object>> field, string value, Dictionary<string, string> pk, bool useFixedFilters = true) where E : class
            => ValidateInvoke<E>("ValidateBy", field, value, pk, useFixedFilters);

        #endregion

        #endregion

        #region CRUD

        /// <summary>
        /// Actualizar o Insertar Genérico
        /// </summary>
        /// <typeparam name="E">Tipado de la Entidad</typeparam>
        /// <param name="entity">Entidad</param>
        /// <param name="useTransaction">true, Usar transacción</param>
        /// <param name="insertKey">true, Inserta PK</param>
        /// <returns>Id de la enitdad creada</returns>
        public List<object> AddOrUpdate<E>(E entity, bool useTransaction = true, bool insertKey = false, bool allowTriggers = false) where E : class
        {
            return AddOrUpdateInvoke("AddOrUpdate", entity, useTransaction, insertKey, allowTriggers);
        }

        /// <summary>
        /// Actualizar o insertar una EntidadCustom
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<object> AddOrUpdateCustom<E>(E entity, bool insertKey = false, bool allowTrigger = false) where E : class
        {
            return AddOrUpdateCustomInvoke("AddOrUpdateCustom", entity, insertKey, allowTrigger);
        }

        /// <summary>
        /// Insertar una lista de entidades (BulkInsert)
        /// </summary>
        /// <typeparam name="E">Tipo de la entidad</typeparam>
        /// <param name="entities">Lista de entidades</param>
        public void AddList<E>(List<E> entities)
        {
            AddListInvoke("AddList", entities);
        }

        /// <summary>
        /// Actualizar una lista de entidades
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="entities"></param>
        /// <param name="insertkey"></param>
        public void UpdateList<E>(List<E> entities, bool insertkey = false)
        {
            UpdateListInvoke("UpdateList", entities, insertkey);
        }

        /// <summary>
        /// Delete Genérico por id (Clave primaria simple)
        /// </summary>
        /// <typeparam name="E">Tipado de la entidad</typeparam>
        /// <param name="id">Valor de la clave</param>
        /// <param name="logicDelete">true = baja lógica</param>
        /// <returns></returns>
        public bool Delete<E>(object id, bool logicDelete = true) where E: class
        {
            return DeleteInvoke<E>("Delete", id, logicDelete);
        }

        public bool DeleteByList<E>(List<int> pkValues, bool logicDelete = true)
        {
            return DeleteByListInvoke<E>("DeleteByList", pkValues, logicDelete);
        }

        public bool DeleteByList<E>(List<string> pkValues, bool logicDelete = true)
        {
            return DeleteByListInvoke<E>("DeleteByList", pkValues, logicDelete);
        }

        #endregion

        #endregion
    }
}
