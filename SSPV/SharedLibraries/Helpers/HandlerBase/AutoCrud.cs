using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Helpers
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoCrud<T> : IAutoCrud
        where T : class
    {
        #region Delegados

        public delegate string CrudHandler(string method);

        #endregion

        #region Campos Privados

        /// <summary>
        /// Metodos de conversion a invokar entre el handler y FacadeBase
        /// </summary>
        private Dictionary<string, Tuple<string, CrudHandler>> _methodsToInvoke = null;

        /// <summary>
        /// Metodos exceptuados de los genericos, pueden escribirse en el handler normalmente
        /// </summary>
        private List<string> _exceptedMethods = null;

        /// <summary>
        /// Metodos de invocacion del handler
        /// </summary>
        private IAutoCrudsHandler _handler = null;

        /// <summary>
        /// Indica que la baja es lógica
        /// </summary>
        private bool _logicDelete = true;

        /// <summary>
        /// Indica si debe insertarse la clave primaria
        /// </summary>
        private bool _insertPrimaryKey = false;

        /// <summary>
        /// Indica que se usan Metodos y Entidades custom para el GetById y el AddOrUpdate
        /// </summary>
        private bool _useCustom = false;

        /// <summary>
        /// Indica si se usa transaccion para Update/Insert
        /// </summary>
        private bool _useTransaction = true;

        /// <summary>
        /// Indica si se pasan a mayúsculas todas las propiedades de tipo string de la entidad al Insertar o Actualizar
        /// </summary>
        private bool _upperStrings = true;

        /// <summary>
        /// Propiedades que no se pararan a mayúscula al insertar o actualizar
        /// </summary>
        private string[] _excludedProps = null;

        /// <summary>
        /// Indica si es solo para obtener datos (all, alldt, byid)
        /// </summary>
        private bool _readOnly = false;

        /// <summary>
        /// NameSpace de las entidades DataTable (DT), por defecto es "EntitiesDT"
        /// </summary>
        private string _dtNameSpace = null;

        /// <summary>
        /// NameSpace para las entidades y custom
        /// </summary>
        private string _customNameSpace = null;

        /// <summary>
        /// Referencia a la clase FacadeBase
        /// </summary>
        private object _fCruds = null;

        /// <summary>
        /// Metodos de FCruds
        /// </summary>
        private MethodInfo[] _crudMethods = null;

        #endregion

        #region Propiedades

        public List<string> ExceptedMethods { get { return _exceptedMethods; } }

        public Dictionary<string, Tuple<string, CrudHandler>> Methods { get { return _methodsToInvoke; } }

        public object FCruds { set { _fCruds = value; } }

        public IAutoCrudsHandler Hanlder { set { _handler = value; } }

        public string DTNameSpace { set { _dtNameSpace = value; } }

        public string CustomNameSpace { set { _customNameSpace = value; } }

        public bool InsertPrimaryKey { set { _insertPrimaryKey = value; } }

        public bool UseCustom { set { _useCustom = value; } }
        
        public bool LogicDelete { set { _logicDelete = value; } }

        public bool UseTransaction { set { _useTransaction = value; } }

        public bool ReadOnly { set { _readOnly = value; } }

        public bool upperStrings { set { _upperStrings = value; } }

        public string[] ExcludedUpperProps { set { _excludedProps = value; } }

        public Type Type { get { return typeof(T); } }

        #endregion

        #region Constructor

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="fCruds">Referencia al objeto FacadeBase (FCRuds)</param>
        /// <param name="handler">Referencia al handler</param>
        /// <param name="dtNameSpace">Opcional, NameSpace para las entidades tipo DataTable</param>
        /// <param name="customNameSpace">Opcional, NameSpace para las entidades Custom</param>
        /// <param name="exceptedMethods">Opcional, Metodos excluidos de auto crud, los cuales deben codearse en el handler tradicional</param>
        /// <param name="insertPK">Opcional, indica si la clave primaria debe insertarse en la tabla, por defecto false</param>
        /// <param name="useCustom">Opcional, indica si se utilizan metodos custom para update/insert y GetbyId, por defecto false</param>
        /// <param name="logicDelete">Opcional, indica si la baja es lógica o física, por defecto true</param>
        /// <param name="useTransaction">Opcional, indica si se usa transaccion, por defecto true, no aplica a metodos custom</param>
        /// <param name="upperStrings">Opcional, indica si se hace un uppercase de las propiedades tipo string de la entidad, por defecto true</param>
        /// <param name="excludedProps">Opcional, array con las propiedades que se desean excluir de uppercase, por defecto null</param>
        public AutoCrud(object fCruds, IAutoCrudsHandler handler, string dtNameSpace = "EntitiesDT", string customNameSpace = "EntitiesCustom", string[] exceptedMethods = null, bool insertPK = false, bool useCustom = false, bool logicDelete = true, bool useTransaction = true, bool upperStrings = true, string[] excludedProps = null, bool readOnly = false)
        {
            string entityName = typeof(T).Name.Replace("_", string.Empty).ToLower();

            if (string.IsNullOrWhiteSpace(entityName))
                throw new NullReferenceException("Error al obtener el nombre de la entidad");

            _methodsToInvoke = new Dictionary<string, Tuple<string, CrudHandler>>
            {
                { $"{entityName}all", new Tuple<string, CrudHandler>("GetAll", GetAll) },
                { $"{entityName}alldt", new Tuple<string, CrudHandler>("GetAllDT", GetAllDT) },
                { $"{entityName}allcustom", new Tuple<string, CrudHandler>("GetAllCustom", GetAllCustom) }
            };

            if (!readOnly)
                _methodsToInvoke.Add($"{entityName}delete", new Tuple<string, CrudHandler>("Delete", Delete));

            if (useCustom)
            {
                _methodsToInvoke.Add($"{entityName}byid", new Tuple<string, CrudHandler>("GetCustomById", GetCustomById));

                if (!readOnly)
                    _methodsToInvoke.Add($"{entityName}addorupdate", new Tuple<string, CrudHandler>("AddOrUpdateCustom", AddOrUpdateCustom));
            }
            else
            {
                _methodsToInvoke.Add($"{entityName}byid", new Tuple<string, CrudHandler>("GetById", GetById));

                if (!readOnly)
                    _methodsToInvoke.Add($"{entityName}addorupdate", new Tuple<string, CrudHandler>("AddOrUpdate", AddOrUpdate));
            }

            if(exceptedMethods != null && exceptedMethods.Count() > 0)
            {
                _exceptedMethods = new List<string>();

                exceptedMethods.ToList().ForEach(em =>
                {
                    string key = $"{entityName}{em}";
                    _exceptedMethods.Add(key);
                    _methodsToInvoke.Remove(key);
                });
            }

            _fCruds = fCruds;
            _handler = handler;
            _dtNameSpace = dtNameSpace;
            _customNameSpace = customNameSpace;
            _logicDelete = logicDelete;
            _insertPrimaryKey = insertPK;
            _useCustom = useCustom;
            _useTransaction = useTransaction;
            _upperStrings = upperStrings;
            _excludedProps = excludedProps;
            _readOnly = readOnly;
        }

        #endregion

        #region Métodos privados

        /// <summary>
        /// Devuelve un delegado generico para invocar los GenericMethods del Handler
        /// </summary>
        /// <param name="method">Metodo a invocar en FacadeBase</param>
        /// <param name="entityType">Type de la entidad (Se utiliza el name y puede variar el namespace)</param>
        /// <param name="entityNamespace">Opcional, para especificar el namespace de la entidad que se va a devolver o pasar como parametro, EntitiesDT, EntitiesCustom etc.</param>
        /// <returns></returns>
        private dynamic GetGenericDelegate(string method, int parmsCount = 0)
        {
            //Si obtengo el type debo saber que metodo invocar
            Type crudType = _fCruds?.GetType();

            if (crudType != null)
            {
                Type targetType = typeof(T);

                if (method.ToLower().EndsWith("dt"))
                {
                    targetType = Type.GetType($"{_dtNameSpace}.{targetType.Name}, {targetType.Assembly.FullName}");

                    if (targetType == null)
                        throw new NullReferenceException($"No existe la entidad {_dtNameSpace}.{Type.Name}");
                    parmsCount = targetType.GetInterface("IEntityDT") != null ? 4 : 0;
                }

                if (method.ToLower().Contains("custom"))
                {
                    targetType = Type.GetType($"{_customNameSpace}.{targetType.Name}, {targetType.Assembly.FullName}");

                    if (targetType == null)
                        throw new NullReferenceException($"No existe la entidad {_customNameSpace}.{Type.Name}");
                }

                if(_crudMethods == null)
                    _crudMethods = crudType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                MethodInfo mi = _crudMethods.FirstOrDefault(f => f.Name == method && f.GetParameters().Count() == parmsCount);

                if (mi == null)
                    throw new NullReferenceException($"{method} no soportado por autocruds");

                MethodInfo genericMethod = mi.MakeGenericMethod(targetType);

                dynamic genericDelegate = Delegate.CreateDelegate(
                         Expression.GetFuncType
                         (
                             genericMethod.GetParameters()
                                 .Select(p => p.ParameterType)
                                 .Concat(new Type[] { genericMethod.ReturnType })
                                 .ToArray()
                         ),
                         _fCruds,
                         genericMethod
                     );

                return genericDelegate;
            }

            return null;
        }

        private string GetAll(string method)
            => _handler.GetAll(GetGenericDelegate(method, 0));

        private string GetAllDT(string method)
        {
            dynamic dele = GetGenericDelegate(method, 0);
            Type typeParm = ((Delegate)dele).Method.ReturnParameter.ParameterType.GenericTypeArguments[0];

            //Evaluo si uso el paginador standard o el nuevo, para el nuevo la Func<string, int> va null
            return typeParm.GetInterface("IEntityDT") != null ? _handler.GetAllDT(dele, null) : _handler.GetAllDT(dele);
        }

        private string GetAllCustom(string method)
            => _handler.GetAllCustom(GetGenericDelegate(method, 0));

        private string GetById(string method)
            => _handler.GetById(GetGenericDelegate(method, 1), "id");
            
        private string GetCustomById(string method)
            => _handler.GetCustomById(GetGenericDelegate(method, 1), "id");

        private string AddOrUpdate(string method)
            => _handler.AddOrUpdate(GetGenericDelegate(method, 4), _useTransaction, _insertPrimaryKey, _upperStrings, _excludedProps);
            
        private string AddOrUpdateCustom(string method)
            => _handler.AddOrUpdateCustom(GetGenericDelegate(method, 3), _upperStrings, _insertPrimaryKey, _excludedProps);

        private string Delete(string method)
            => _handler.Delete(GetGenericDelegate(method, 2), "id", _logicDelete);

        #endregion

        #region Métodos Públicos

        public bool GenericMethodAllowed(string method)
            => _methodsToInvoke != null && _methodsToInvoke.ContainsKey(method);

        public string Invoke(string method)
        {
            Tuple<string, CrudHandler> toInvoke = null;

            if (!_methodsToInvoke.TryGetValue(method, out toInvoke))
                throw new NullReferenceException($"{method} no soportado por autocruds");

            return toInvoke.Item2.Invoke(toInvoke.Item1);
        }

        #endregion
    }
}
