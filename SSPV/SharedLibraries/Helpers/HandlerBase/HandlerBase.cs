using Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Helpers
{
    /// <summary>
    /// Handler Base
    /// </summary>
    public abstract class HandlerBase : IHttpHandler, IRequiresSessionState
    {
        #region Delegados

        protected delegate void OutAction<T>(out T usuario);
        protected delegate string MethodHandler(HttpContext context);

        #endregion

        #region Atributos protegidos

        protected static bool _isAutoCrud = false;
        protected static object _lockObj = new object();
        protected static Dictionary<string, MethodHandler> _methods;
        protected List<IAutoCrud>_autoCruds = null;

        protected Logger _log;
        protected HttpContext _context;
        protected HttpRequest _request;
        protected HttpResponse _response;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public HandlerBase()
        {
            lock(_lockObj)
            {
                if (_methods == null || _methods.Count == 0)
                {
                    _methods = new Dictionary<string, MethodHandler>();
                    AddToHandlerDictionary();
                }
            }

            _isAutoCrud = GetType().GetInterface("IAutoCrudsHandler") != null;
            _log = LogManager.GetCurrentClassLogger();
            _context = HttpContext.Current;
            _request = HttpContext.Current.Request;
            _response = HttpContext.Current.Response;

            InitFacadeObjects();

            lock(_lockObj)
            {
                if (_isAutoCrud && _autoCruds == null)
                 {
                    _autoCruds = new List<IAutoCrud>();
                    AddToAutoCrudList(); //Este metodo es virtual, solo por compatibilidad de aplicaciones anteriores
                }
            }
        }
                

        #endregion

        #region IHttpHandler Implementation

        public virtual bool IsReusable
        {
            get { return false; }
        }

        public virtual void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            #region Autenticacion del Usuario

            if (!Utilities.IsUserLoggedIn())
            {
                Utilities.CloseSession(true);
                return;
            }

            #endregion Autenticacion del Usuario

            var method = "method".FromQueryString(string.Empty);

            if (!string.IsNullOrWhiteSpace(method))
                method = method.ToLower();

            AssignCaptcha(context, method); //Si se usa captcha hacer override en el handler del site

            var response = string.Empty;

            bool methodNotAllowed = string.IsNullOrWhiteSpace(method);

            if(!methodNotAllowed)
                methodNotAllowed = _isAutoCrud ? !_autoCruds.Any(a => a.GenericMethodAllowed(method)) && !_methods.ContainsKey(method) : !_methods.ContainsKey(method);

            if (methodNotAllowed)
            {
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                response = Utilities.ToJsonError(
                    (int)HttpStatusCode.MethodNotAllowed,
                    "Operación no permitida."
                );
            }
            else
            {
                try
                {
                    if (_isAutoCrud)
                    {
                        IAutoCrud autoCrud = _autoCruds.FirstOrDefault(f => f.GenericMethodAllowed(method));

                        if (autoCrud != null)
                            response = autoCrud.Invoke(method);
                        else //Utilizo el diccionario tradicional
                            response = _methods[method].Invoke(context);
                    }
                    else
                        response = _methods[method].Invoke(context);

                }
                catch (ThreadAbortException)
                {
                    // Nada interesante por aquí.
                }
                catch (HttpException ex)
                {
                    if ((uint)ex.ErrorCode != 0x80070040) // Remote server close connection
                    {
                        response = Utilities.HandleRequestError(ex);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch (BusinessException ex)
                {
                    response = Utilities.HandleRequestError(ex);

                    if (ex.Code == Code.UserNotFoundInSession)
                    {
                        FormsAuthentication.SignOut();
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    }
                    else
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                catch (Exception ex)
                {
                    response = Utilities.HandleRequestError(ex);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }

            context.Response.Write(response);
            context.Response.End();
        }

        #endregion

        #region Metodos privados

        /// <summary>
        /// Deserializar la entidad json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="upperStrings"></param>
        /// <returns></returns>
        protected static T DeserializeEntity<T>(HttpContext context, bool upperStrings, string[] excludedProps) where T : class
        {
            var isMultipartForm = context.Request.ContentType.ToLower().Contains("multipart/form-data");

            var entityJson = string.Empty;
            var entidad = default(T);

            if (isMultipartForm)
            {
                var form = context.Request.Form;
                var files = context.Request.Files;

                if (!form.AllKeys.Contains("json"))
                    throw new Exception("El formato del formulario multipart no es correcto. Campo 'json' inexistente.");

                entityJson = form["json"];
                var entityJObject = (JObject)JsonConvert.DeserializeObject(entityJson);

                if (form["json"].Contains("@@FileReference@") && files.Count == 0)
                {
                    //throw new Exception("El formato del formulario multipart no es correcto. Sin archivos adjuntos.");
                    var fileProperties = entityJObject
                        .Properties()
                        .Where(w => w.Value.ToString().StartsWith("@@FileReference@"));

                    foreach (var fp in fileProperties)
                        fp.Value = null;

                }
                else
                {
                    foreach (string key in files)
                    {
                        var file = files[key];

                        using (var reader = new BinaryReader(file.InputStream))
                        {
                            var buffer = reader.ReadBytes(file.ContentLength);

                            var jsonProperty = entityJObject
                                .Properties()
                                .FirstOrDefault(f => f.Value.ToString().Equals($"@@FileReference@{key}@@"));

                            if (jsonProperty != null)
                                jsonProperty.Value = buffer;
                        }
                    }
                }

                entidad = entityJObject.ToObject<T>();
            }
            else
            {
                var sr = new StreamReader(context.Request.InputStream);
                entityJson = sr.ReadToEnd();
                entidad = JsonConvert.DeserializeObject<T>(entityJson);
            }

            if (upperStrings)
            {

                PropertyInfo[] props = typeof(T).GetProperties().Where(w => w.PropertyType == typeof(string)).ToArray();

                if (excludedProps != null && excludedProps.Length > 0 && props != null)
                    props = props.Where(w => !excludedProps.Any(a => a.ToLowerInvariant().Equals(w.Name.ToLowerInvariant()))).ToArray();

                if (props != null)
                    foreach (PropertyInfo prop in props)
                    {
                        object value = prop.GetValue(entidad);
                        if(value != null)
                            prop.SetValue(entidad, (value as string).ToUpperInvariant());
                    }
            }

            return entidad;
        }

        /// <summary>
        /// Generar expresion para metodos genéricos
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static Expression<Func<T, bool>> CreateWhereExpression<T>(string[] parameters)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            Dictionary<PropertyInfo, object> dicValues = new Dictionary<PropertyInfo, object>();

            if (properties == null || properties.Count() == 0)
                throw new NullReferenceException($"No se obtuvieron las propiedades de {typeof(T).Name}");

            foreach (string param in parameters)
            {
                PropertyInfo prop = properties.FirstOrDefault(f => f.Name.ToLowerInvariant() == param.ToLowerInvariant());

                if(prop == null)
                    throw new NullReferenceException($"La propiedad {param} no existe en {typeof(T).Name}");

                var value = Convert.ChangeType(param.FromQueryString(prop.PropertyType.GetDefault()), prop.PropertyType);
                dicValues.Add(prop, value);
            }

            Expression expre = null;

            foreach (KeyValuePair<PropertyInfo, object> item in dicValues)
            {
                ParameterExpression parameter = Expression.Parameter(typeof(T), "s");
                MemberExpression me = Expression.MakeMemberAccess(parameter, item.Key);
                ConstantExpression ce = Expression.Constant(item.Value);
                BinaryExpression be = Expression.Equal(me, ce);
                
                if (expre == null)
                    expre = Expression.Lambda(be, parameter);
                else
                    expre = Expression.Lambda(Expression.AndAlso(((Expression<Func<T, bool>>)expre).Body, Expression.Lambda(be, parameter).Body), parameter);
            }

            return (Expression<Func<T, bool>>)expre;
        }

        #endregion

        #region Metodos Protegidos

        #region Genericos CRUD

        /// <summary>
        /// Obtener todo
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entities">Lista de entidades a devolver</param>
        /// <returns></returns>
        protected string GetAll<T>(HttpContext context, Func<List<T>> entities) where T : class
            => JsonConvert.SerializeObject(entities.Invoke());

        /// <summary>
        /// Obtener todo filtrando por Id
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entities">Lista de entidades a devolver</param>
        /// <param name="idParam">Id por el que se van a obtener las entidadades (Tablas hijas)</param>
        /// <returns></returns>
        protected string GetAll<T>(HttpContext context, Func<int, List<T>> entities, string idParam) where T : class
        {
            var id = idParam.FromQueryString(-1);

            var results = entities.Invoke(id);

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener mediante un filtro expression
        /// </summary>
        /// <typeparam name="T">Tipado de la entidad</typeparam>
        /// <param name="contex">Contexto Http</param>
        /// <param name="entities">metodo que devolvera los resultados</param>
        /// <param name="parameters">parametros del filtro (deben coincidir con las propiedades de la entidad)</param>
        /// <returns></returns>
        protected string GetAll<T>(HttpContext contex, Func<Expression<Func<T, bool>>, List<T>> entities, params string[] parameters) where T : class
        {
            if (parameters == null && parameters.Count() == 0)
                throw new NullReferenceException("Debe establecer los parámetros");

            var results = entities.Invoke(CreateWhereExpression<T>(parameters));

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener todo
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <typeparam name="VParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="defaultV"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetAll<TOut, UParm, VParm>(HttpContext context, Func<UParm, VParm, List<TOut>> entity, string uParm, string vParm, UParm defaultU = default(UParm), VParm defaultV = default(VParm), bool notFoundException = false) where TOut : class
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);
            VParm vParameter = vParm.FromQueryString(defaultV, notFoundException);

            List<TOut> result = entity.Invoke(uParameter, vParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener todo
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <typeparam name="VParm"></typeparam>
        /// <typeparam name="WParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="wParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="defaultV"></param>
        /// <param name="defaultW"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetAll<TOut, UParm, VParm, WParm>(HttpContext context, Func<UParm, VParm, WParm, List<TOut>> entity, string uParm, string vParm, string wParm, UParm defaultU = default(UParm), VParm defaultV = default(VParm), WParm defaultW = default(WParm), bool notFoundException = false) where TOut : class
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);
            VParm vParameter = vParm.FromQueryString(defaultV, notFoundException);
            WParm wParameter = wParm.FromAppSettings(defaultW, notFoundException);

            List<TOut> result = entity.Invoke(uParameter, vParameter, wParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener todo para tipos basicos (Built-in)
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetAllBuiltIn<TOut, UParm>(HttpContext context, Func<UParm, List<TOut>> entity, string uParm, string vParm, UParm defaultU = default(UParm),  bool notFoundException = false) where TOut : struct
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);

            List<TOut> result = entity.Invoke(uParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener todo para tipos basicos (Built-in)
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <typeparam name="VParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="defaultV"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetAllBuiltIn<TOut, UParm, VParm>(HttpContext context, Func<UParm, VParm, List<TOut>> entity, string uParm, string vParm, UParm defaultU = default(UParm), VParm defaultV = default(VParm), bool notFoundException = false) where TOut : struct
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);
            VParm vParameter = vParm.FromQueryString(defaultV, notFoundException);
            
            List<TOut> result = entity.Invoke(uParameter, vParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener todo para tipos basicos (Built-in)
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <typeparam name="VParm"></typeparam>
        /// <typeparam name="WParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="wParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="defaultV"></param>
        /// <param name="defaultW"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetAllBuiltIn<TOut, UParm, VParm, WParm>(HttpContext context, Func<UParm, VParm, WParm, List<TOut>> entity, string uParm, string vParm, string wParm, UParm defaultU = default(UParm), VParm defaultV = default(VParm), WParm defaultW = default(WParm), bool notFoundException = false) where TOut : struct
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);
            VParm vParameter = vParm.FromQueryString(defaultV, notFoundException);
            WParm wParameter = wParm.FromQueryString(defaultW, notFoundException);

            List<TOut> result = entity.Invoke(uParameter, vParameter, wParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener todo custom
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="contex"></param>
        /// <param name="entities"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected string GetAllCustom<E, T>(HttpContext contex, Func<Expression<Func<T, bool>>, List<E>> entities, params string[] parameters) where T : class where E : class
        {
            if (parameters == null && parameters.Count() == 0)
                throw new NullReferenceException("Debe establecer los parámetros");

            var results = entities.Invoke(CreateWhereExpression<T>(parameters));

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener todo desde la cache
        /// </summary>
        /// <typeparam name="T">Tipado de la entidad</typeparam>
        /// <param name="context">HttpContext</param>
        /// <param name="entities">Metodo que devuelve la lista de entidades</param>
        /// <param name="cacheKey">Clave de la cache</param>
        /// <param name="timeout">Timeout de cache</param>
        /// <returns></returns>
        protected string GetAllFromCache<T>(HttpContext context, Func<List<T>> entities, string cacheKey, string timeout) where T : class
            => JsonConvert.SerializeObject(cacheKey.FromCache(() => entities.Invoke(), timeout));

        protected string GetAllFromCache<T, V>(HttpContext context, Func<V, List<T>> entities, V param, string cacheKey, string timeout) where T : class
            => JsonConvert.SerializeObject(cacheKey.FromCache(() => entities.Invoke(param), timeout));

        /// <summary>
        /// Obtener todo con formato DataTable
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entities">Lista de entidades a devolver</param>
        /// <returns></returns>
        protected string GetAllDT<T>(HttpContext context, Func<List<T>> entities) where T : class
        {
            var dtm = new DataTables.Manager<T>(context.Request, entities);

            var results = dtm.Process();

            return JsonConvert.SerializeObject(results);
        }
        
        /// <summary>
        /// Obtener todo con formato DataTable Paginado
        /// </summary>
        /// <typeparam name="T">Tipado de la entidad</typeparam>
        /// <param name="context">contexto http</param>
        /// <param name="entities">Funcion que devolverá la lista de entidades</param>
        /// <param name="count">Funcion que devolverá el recuento de entidades a devolver</param>
        /// <returns></returns>
        protected string GetAllDT<T>(HttpContext context, Func<int, int, string, string[], List<T>> entities, Func<string, int> count)
        {
            var dtm = new DTPagination.ManagerPaging<T>(context.Request, entities, count);

            var results = dtm.Process();

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener todo con formato DataTable Paginado pasando parámetros tomados de la QueryString.
        /// </summary>
        /// <typeparam name="T">Tipado de la entidad</typeparam>
        /// <param name="context">contexto http</param>
        /// <param name="entitiesWithParameters">Función que devolverá la lista de entidades</param>
        /// <param name="count">Función que devolverá el recuento de entidades a devolver</param>
        /// <param name="parameters">Lista de parámetros a pasar a la consulta final</param>
        /// <returns></returns>
        protected string GetAllDT<T>(HttpContext context, Func<int, int, string, string[], string[], List<T>> entitiesWithParameters, Func<string, string[], int> count, params string[] parameters)
        {
            var values = default(string[]);

            if (parameters != null && parameters.Length > 0)
                values = parameters
                    .Select(s => s.FromQueryString(string.Empty))
                    .ToArray();

            var dtm = new DTPagination.ManagerPaging<T>(context.Request, entitiesWithParameters, count, values);

            var results = dtm.Process();

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener todo con formato DataTable
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entities">Lista de entidades a devolver</param>
        /// <param name="idParam">campo de id, para tablas de clave compuesta</param>
        /// <returns></returns>
        protected string GetAllDT<T>(HttpContext context, Func<int, List<T>> entities, string idParam) where T : class
        {
            var id = idParam.FromQueryString(-1);
            var dtm = new DataTables.Manager<T>(context.Request, () => entities(id));

            var results = dtm.Process();

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener entidad por id
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entity">Entidad a obtener</param>
        /// <param name="idParam">ID de la entidad a obtener</param>
        /// <returns></returns>
        protected string GetById<T>(HttpContext context, Func<object, T> entity, string idParam) where T : class
        {
            var id = idParam.FromQueryString(default(object));

            var result = entity.Invoke(id);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener entidad por alguna propiedad de tipo string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="strParam"></param>
        /// <returns></returns>
        protected string GetByStrField<T>(HttpContext context, Func<string, List<T>> entity, string strParam) where T : class
        {
            var param = strParam.FromQueryString("-1");

            var result = entity.Invoke(param);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener por un campo
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="VParam"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="param"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        protected string GetBy<TOut, VParam>(HttpContext context, Func<VParam, TOut> entity, string param, VParam defaultValue = default(VParam), bool notFoundException = false) where TOut: class
        {
            var parameter = param.FromQueryString(defaultValue, notFoundException);

            var result = entity.Invoke(parameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener por dos campos
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <typeparam name="VParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="defaultV"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetBy<TOut, UParm, VParm>(HttpContext context, Func<UParm, VParm, TOut> entity, string uParm, string vParm, UParm defaultU = default(UParm), VParm defaultV =default(VParm), bool notFoundException = false) where TOut: class
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);
            VParm vParameter = vParm.FromQueryString(defaultV, notFoundException);

            TOut result = entity.Invoke(uParameter, vParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TOut"></typeparam>
        /// <typeparam name="UParm"></typeparam>
        /// <typeparam name="VParm"></typeparam>
        /// <typeparam name="WParm"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="wParm"></param>
        /// <param name="defaultU"></param>
        /// <param name="defaultV"></param>
        /// <param name="defaultW"></param>
        /// <param name="notFoundException"></param>
        /// <returns></returns>
        protected string GetBy<TOut, UParm, VParm, WParm>(HttpContext context, Func<UParm, VParm, WParm, TOut> entity, string uParm, string vParm, string wParm, UParm defaultU = default(UParm), VParm defaultV = default(VParm), WParm defaultW = default(WParm), bool notFoundException = false) where TOut : class
        {
            UParm uParameter = uParm.FromQueryString(defaultU, notFoundException);
            VParm vParameter = vParm.FromQueryString(defaultV, notFoundException);
            WParm wParameter = wParm.FromQueryString(defaultW, notFoundException);

            TOut result = entity.Invoke(uParameter, vParameter, wParameter);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Obtener por expresion
        /// </summary>
        /// <typeparam name="T">Tipado de la entidad</typeparam>
        /// <param name="contex">Http Context</param>
        /// <param name="entities">Metodo que devolvera la lista de entidades</param>
        /// <param name="useFixedFilters">true, utiliza los filtros fijos de la entidad</param>
        /// <param name="parameters">Parámetro de busqueda en context (Deben conincidir con las propiedades del la clase)</param>
        /// <returns></returns>
        protected string GetBy<T>(HttpContext contex, Func<Expression<Func<T, bool>>, bool, List<T>> entities, bool useFixedFilters = true, params string[] parameters)
        {
            if (parameters == null && parameters.Count() == 0)
                throw new NullReferenceException("Debe establecer los parámetros");

            var results = entities.Invoke(CreateWhereExpression<T>(parameters), useFixedFilters);

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="idFirst"></param>
        /// <param name="idSecond"></param>
        /// <returns></returns>
        protected string GetBy<T>(HttpContext context, Func<int, int, T> entities, string idFirst, string idSecond)
        {
            var idF = idFirst.FromQueryString(-1);
            var idS = idSecond.FromQueryString(-1);
            var results = entities.Invoke(idF, idS);

            return JsonConvert.SerializeObject(results);
        }

        /// <summary>
        /// Obtener por Id y Filtro devolviendo un JArray (Para selec2 por ejemplo)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="output"></param>
        /// <param name="filtroParm"></param>
        /// <returns></returns>
        protected string GetBy<T>(HttpContext context, Func<string, List<T>> entities, Action<List<T>, JArray> output, string filtroParm)
        {
            var filtro = filtroParm.FromQueryString(default(string));
            var results = entities.Invoke(filtro);

            JArray jsonArray = new JArray();
            output.Invoke(results, jsonArray);

            return JsonConvert.SerializeObject(jsonArray);
        }

        /// <summary>
        /// Obtener por Id y Filtro devolviendo un JArray (Para selec2 por ejemplo)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entities"></param>
        /// <param name="output"></param>
        /// <param name="idParm"></param>
        /// <param name="filtroParm"></param>
        /// <returns></returns>
        protected string GetBy<T>(HttpContext context, Func<int, string, List<T>> entities, Action<List<T>, JArray> output, string idParm, string filtroParm)
        {
            var id = idParm.FromQueryString(-1);
            var filtro = filtroParm.FromQueryString(default(string));
            var results = entities.Invoke(id, filtro);

            JArray jsonArray = new JArray();
            output.Invoke(results, jsonArray);

            return JsonConvert.SerializeObject(jsonArray);
        }

        protected string GetBy<T>(HttpContext context, Func<string, int?, List<T>> entities, Action<List<T>, JArray> output, string filtroParm, string idParm)
        {
            var id = idParm.FromQueryString<int?>(null);
            var filtro = filtroParm.FromQueryString(default(string));
            var results = entities.Invoke(filtro, id);

            JArray jsonArray = new JArray();
            output.Invoke(results, jsonArray);

            return JsonConvert.SerializeObject(jsonArray);
        }
        

        /// <summary>
        /// Agregar una entidad a la DB
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entity">Entidad a guardar</param>
        /// <param name="upperStrings">true, realiza un UpperCase de las propiedades de tipo string</param>
        /// <returns></returns>
        protected string AddOrUpdate<T>(HttpContext context, Func<T, bool, bool, List<object>> entity, bool useTransaction = true, bool insertKey = false, bool upperStrings = true, params string[] excludedProps) where T : class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, upperStrings, excludedProps), useTransaction, insertKey);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Agregar una entidad a la DB
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entity">Entidad a guardar</param>
        /// <param name="useTransaction"></param>
        /// <param name="insertKey"></param>
        /// <param name="allowTriggers">true, permite el uso de triggers en la tabla</param>
        /// <param name="upperStrings">true, realiza un UpperCase de las propiedades de tipo string</param>
        /// <param name="excludedProps"></param>
        /// <returns></returns>
        protected string AddOrUpdate<T>(HttpContext context, Func<T, bool, bool, bool, List<object>> entity, bool useTransaction = true, bool insertKey = false, bool allowTriggers = false, bool upperStrings = true, params string[] excludedProps) where T : class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, upperStrings, excludedProps), useTransaction, insertKey, allowTriggers);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// AddOrUpdate Generico para metodos viejos de los cruds (que no utilizan CrudManager<>)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <param name="upperStrings"></param>
        /// <param name="excludedProps"></param>
        /// <returns></returns>
        protected string AddOrUpdate<T>(HttpContext context, Func<T, List<object>> entity, bool upperStrings = true, params string[] excludedProps) where T: class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, upperStrings, excludedProps));

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Agregar una entidad Custom a la DB
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad Custmo</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="entity">Entidad a guardar</param>
        /// <param name="upperStrings">true, realiza un UpperCase de las propiedades de tipo string</param>
        /// <returns></returns>
        protected string AddOrUpdateCustom<T>(HttpContext context, Func<T, List<object>> entity, bool upperStrings = true, params string[] excludedProps) where T: class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, upperStrings, excludedProps));

            return JsonConvert.SerializeObject(result);
        }

        protected string AddOrUpdateCustom<T>(HttpContext context, Func<T, bool, List<object>> entity, bool upperStrings = true, bool insertkey = false, params string[] excludedProps) where T : class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, upperStrings, excludedProps), insertkey);

            return JsonConvert.SerializeObject(result);
        }

        protected string AddOrUpdateCustom<T>(HttpContext context, Func<T, bool, bool, List<object>> entity, bool upperStrings = true, bool insertkey = false, bool allowTriggers = false, params string[] excludedProps) where T : class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, upperStrings, excludedProps), insertkey, allowTriggers);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Agregar una entidad Custom a la DB
        /// </summary>
        /// <typeparam name="T">Entidad</typeparam>
        /// <typeparam name="TOUT">Entidad devuelta en el proceso</typeparam>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected string AddOrUpdateCustom<T, TOUT>(HttpContext context, Func<T, TOUT> entity) where T : class where TOUT:class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = entity.Invoke(DeserializeEntity<T>(context, false, null));

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Eliminar entidad por Id
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="delete">Metodo del crud para eliminar la entidad</param>
        /// <param name="idParam">Id de la entidad a eliminar</param>
        /// <param name="logicDelete">Borrado lógico</param>
        /// <returns></returns>
        protected string Delete(HttpContext context, Func<object, bool, bool> delete, string idParam, bool logicDelete = true)
        {
            var id = idParam.FromQueryString(default(object));

            var result = delete.Invoke(id, logicDelete);

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Validar entidad por algún campo
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="validate">Metodo de Validacion de Crud (Para tipo string)</param>
        /// <param name="fieldName">Campo por el que se va a validar</param>
        /// <returns></returns>
        protected string ValidateBy<T>(HttpContext context, Func<string, bool> validate, string fieldName) where T : class
        {
            string campoValidacion = fieldName.FromQueryString("");

            var result = validate.Invoke(campoValidacion);

            var data = new
            {
                valid = result
            };

            return JsonConvert.SerializeObject(data);
        }

        /// <summary>
        /// Validar entidad por algún campo
        /// </summary>
        /// <typeparam name="T">Tipo de la Entidad</typeparam>
        /// <param name="context">Http context</param>
        /// <param name="validate">Metodo de Validacion de Crud (Para tipo entero)</param>
        /// <param name="fieldName">Campo por el que se va a validar</param>
        /// <returns></returns>
        protected string ValidateBy<T>(HttpContext context, Func<int, bool> validate, string fieldName) where T : class
        {
            int campoValidacion = fieldName.FromQueryString(-1);

            var result = validate.Invoke(campoValidacion);

            var data = new
            {
                valid = result
            };

            return JsonConvert.SerializeObject(data);
        }

        protected string ValidateBy<T>(
            HttpContext context,
            Func<Expression<Func<T, object>>, string, Dictionary<string, string>, bool, bool> validate,
            Expression<Func<T, object>> field,
            string fieldValue,
            params string[] pk
        ) where T : class
            => ValidateBy(context, validate, field, fieldValue, true, pk);

        protected string ValidateBy<T>(
            HttpContext context,
            Func<Expression<Func<T, object>>, string, Dictionary<string, string>, bool, bool> validate,
            Expression<Func<T, object>> field,
            string fieldValue,
            bool useFixedFilter,
            params string[] pk
        ) where T : class
        {
            var value = fieldValue.FromQueryString<string>(notFoundException: true);
            var pkValues = new Dictionary<string, string>();

            foreach (var p in pk)
            {
                var pkValue = p.FromQueryString<string>();
                if (string.IsNullOrWhiteSpace(pkValue))
                {
                    pkValues.Clear();
                    break;
                }
                pkValues.Add(p, pkValue);
            }

            var result = validate.Invoke(field, value, pkValues, useFixedFilter);

            var data = new
            {
                valid = result
            };

            return JsonConvert.SerializeObject(data);
        }

        #endregion

        #region Seguridad

        protected static string GetSeguridad<T>(HttpContext context, OutAction<T> action) where T : class
        {
            T usuario;

            action(out usuario);

            var result = usuario;

            return JsonConvert.SerializeObject(result);
        }

        protected string CerrarSesion(HttpContext context)
        {
            FormsAuthentication.SignOut();
            return string.Empty;
        }

        #endregion

        #region Serializacion

        /// <summary>
        /// Serializa 
        /// </summary>
        /// <typeparam name="TOut">Clase que será serializada</typeparam>
        /// <typeparam name="EIn">Clase que será deserializada</typeparam>
        /// <param name="context">HttpContext</param>
        /// <param name="toDeserializeEntity">Metodo que generará la clasee de salida</param>
        /// <returns></returns>
        protected string SerializeObject<TOut, EIn>(HttpContext context, Func<EIn,TOut> toDeserializeEntity) 
            where TOut : class
            where EIn : class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = toDeserializeEntity.Invoke(DeserializeEntity<EIn>(context, false, null));

            return JsonConvert.SerializeObject(result);
        }

        /// <summary>
        /// Serializar para metodos que devuelven bool
        /// </summary>
        /// <typeparam name="EIn"></typeparam>
        /// <param name="context"></param>
        /// <param name="toDeserializeEntity"></param>
        /// <returns></returns>
        protected string SerializeObject<EIn>(HttpContext context, Func<EIn, bool> toDeserializeEntity)
            where EIn : class
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var result = toDeserializeEntity.Invoke(DeserializeEntity<EIn>(context, false, null));

            return JsonConvert.SerializeObject(result);
        }

        protected string SerializeObject<TOut, VParam>(HttpContext context, Func<VParam, TOut> method, string param, VParam defaultValue = default(VParam), bool notFoundException = false)
        {
            var parameter = param.FromQueryString(defaultValue, notFoundException);

            var result = method.Invoke(parameter);

            return JsonConvert.SerializeObject(result);
        }

        protected string SerializeObject<TOut, UParam, VParam>(HttpContext context, Func<UParam, VParam, TOut> method, IDictionary<string, object> parms, bool notFoundException = false)
        {
            if (parms == null || parms.Count != 2)
                throw new Exception("La cantidad de parametros no es válida");

            var first = parms.ElementAt(0);
            var uParameter = first.Key.FromQueryString((UParam)first.Value, notFoundException);

            var second = parms.ElementAt(1);
            var vParameter = second.Key.FromQueryString((VParam)second.Value, notFoundException);

            var result = method.Invoke(uParameter, vParameter);

            return JsonConvert.SerializeObject(result);
        }

        protected string SerializeObject<TOut, UParam, VParam, WParam>(HttpContext context, Func<UParam, VParam, WParam, TOut> method, IDictionary<string, object> parms, bool notFoundException = false)
        {
            if (parms == null || parms.Count != 4)
                throw new Exception("La cantidad de parametros no es válida");

            var first = parms.ElementAt(0);
            var uParameter = first.Key.FromQueryString((UParam)first.Value, notFoundException);

            var second = parms.ElementAt(1);
            var vParameter = second.Key.FromQueryString((VParam)second.Value, notFoundException);

            var third = parms.ElementAt(2);
            var wParameter = third.Key.FromQueryString((WParam)third.Value, notFoundException);
                        
            var result = method.Invoke(uParameter, vParameter, wParameter);

            return JsonConvert.SerializeObject(result);
        }

        protected string SerializeObject<TOut, UParam, VParam, WParam, ZParam>(HttpContext context, Func<UParam, VParam, WParam, ZParam, TOut> method, IDictionary<string, object> parms, bool notFoundException = false)
        {
            if (parms == null || parms.Count != 4)
                throw new Exception("La cantidad de parametros no es válida");

            var first = parms.ElementAt(0);
            var uParameter = first.Key.FromQueryString((UParam)first.Value, notFoundException);

            var second = parms.ElementAt(1);
            var vParameter = second.Key.FromQueryString((VParam)second.Value, notFoundException);

            var third = parms.ElementAt(2);
            var wParameter = third.Key.FromQueryString((WParam)third.Value, notFoundException);

            var fourth = parms.ElementAt(3);
            var zParameter = fourth.Key.FromQueryString((ZParam)fourth.Value, notFoundException);

            var result = method.Invoke(uParameter, vParameter, wParameter, zParameter);

            return JsonConvert.SerializeObject(result);
        }

        #endregion

        #region Envio de Mails

        protected string SendEMail<T>(HttpContext context, Action<T> method, string paramId, T defaultValue = default(T))
        {
            var parameter = paramId.FromQueryString(default(T));

            if (parameter == null || parameter.Equals(default(T)) || parameter.Equals(defaultValue))
                return string.Empty;

            method.Invoke(parameter);

            return JsonConvert.SerializeObject(true);
        }

        #endregion

        #region Download

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Vparm"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="document"></param>
        /// <param name="vParm"></param>
        /// <param name="defaultValue"></param>
        /// <param name="notFoundException"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected string DownloadFile<Vparm, F>(HttpContext context, Func<Vparm, F> method, string document, string vParm, Vparm defaultValue = default(Vparm), bool notFoundException = false, string contentType = "application/pdf") where F: class
        {
            var parameter = vParm.FromQueryString(defaultValue, notFoundException);
            string fName = typeof(F).Name;

            if (parameter == null)
                return string.Empty;

            if (parameter.Equals(defaultValue))
                return string.Empty;

            var cacheKey = $"{fName}.{document}.{parameter}"; 
            var file = cacheKey.FromCache(() => method.Invoke(parameter), $"{fName}RequestTimeout", false) as IEntityFile;
            var response = context.Response;
            
            response.ContentType = contentType;
            response.AddHeader("content-disposition", "attachment;filename=\"" + file.Filename + "\"");
            response.OutputStream.Write(file.Content, 0, file.Length);
            response.Flush();
            response.End();

            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="Uparm"></typeparam>
        /// <typeparam name="Vparm"></typeparam>
        /// <typeparam name="F"></typeparam>
        /// <param name="context"></param>
        /// <param name="method"></param>
        /// <param name="document"></param>
        /// <param name="uParm"></param>
        /// <param name="vParm"></param>
        /// <param name="defaultUValue"></param>
        /// <param name="defaultVValue"></param>
        /// <param name="notFoundException"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected string DownloadFile<Uparm, Vparm, F>(HttpContext context, Func<Uparm, Vparm, F> method, string document, string uParm, string vParm, Uparm defaultUValue = default(Uparm), Vparm defaultVValue = default(Vparm), bool notFoundException = false, string contentType = "application/pdf") where F : class
        {
            var parm1 = uParm.FromQueryString(defaultUValue, notFoundException);
            var parm2 = vParm.FromQueryString(defaultVValue, notFoundException);
            
            string fName = typeof(F).Name;

            if (parm1 == null || parm2 == null)
                return string.Empty;

            if (parm1.Equals(defaultUValue))
                return string.Empty;

            var cacheKey = $"{fName}.{document}.{parm1}.{parm2}";
            var file = cacheKey.FromCache(() => method.Invoke(parm1, parm2), $"{fName}RequestTimeout", false) as IEntityFile;
            var response = context.Response;
            
            response.ContentType = contentType;
            response.AddHeader("content-disposition", "attachment;filename=\"" + file.Filename + "\"");
            response.OutputStream.Write(file.Content, 0, file.Length);
            response.Flush();
            response.End();

            return string.Empty;
        }

        #endregion

        #endregion

        #region Metodos Abstractos

        protected abstract void AddToHandlerDictionary();

        protected abstract void InitFacadeObjects();
        
        #endregion

        #region Metodos virtuales

        /// <summary>
        /// Asignar captcha (opcional si el sitio lo requiere)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="method"></param>
        protected virtual void AssignCaptcha(HttpContext context, string method) { }

        /// <summary>
        /// Agregar los types que pueden usarse con autocruds
        /// </summary>
        protected virtual void AddToAutoCrudList() { }

        #endregion
    }
}
