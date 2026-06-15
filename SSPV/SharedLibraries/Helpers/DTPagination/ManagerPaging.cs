using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace DTPagination
{
    public class ManagerPaging<T>
    {
        #region Atributos privados

        private HttpRequest _request = null;
        private Func<int, int, string, string[], List<T>> _entities = null;
        private Func<int, int, string, string[], string[], List<T>> _entitiesWithParameters = null;
        private Func<int, int, string, string[], Dictionary<string, dynamic>, List<T>> _entitiesWithParamClaveValor = null;
        private Func<string, int> _count;
        private Func<string, string[], int> _countWithParameters;
        private Func<string, Dictionary<string, dynamic>, int> _countWithParamClaveValor;
        private InputSettings<T> _settings = null;
        private string[] _parameters;
        private Dictionary<string, dynamic> _paramClaveValor;
        private bool _withParameters;

        #endregion

        #region Constructores

        public ManagerPaging(HttpRequest request, Func<int, int, string, string[], List<T>> entities, Func<string, int> count)
        {
            _request = request;
            _entities = entities;
            _count = count;
            _withParameters = false;

            Initialize();
        }

        public ManagerPaging(HttpRequest request, Func<int, int, string, string[], string[], List<T>> entitiesWithParameters, Func<string, string[], int> countWithParameters, params string[] parameters)
        {
            _request = request;
            _entitiesWithParameters = entitiesWithParameters;
            _countWithParameters = countWithParameters;
            _parameters = parameters;
            _withParameters = true;

            Initialize();
        }

        public ManagerPaging(HttpRequest request, Func<int, int, string, string[], Dictionary<string, dynamic>, List<T>> entitiesWithParamClaveValor, Func<string, Dictionary<string, dynamic>, int> countWithParamClaveValor, Dictionary<string, dynamic> paramClaveValor)
        {
            _request = request;
            _entitiesWithParamClaveValor = entitiesWithParamClaveValor;
            _countWithParamClaveValor = countWithParamClaveValor;
            _paramClaveValor = paramClaveValor;
            _withParameters = true;

            Initialize();
        }

        #endregion

        #region Metodos Privados

        private void Initialize()
        {
            var draw = "draw".FromQueryString<int>(notFoundException: true);
            var start = "start".FromQueryString<int>(notFoundException: true);
            var length = "length".FromQueryString<int>(notFoundException: true);
            var search = "search[value]".FromQueryString<string>(notFoundException: true);
            var orderColumnIndex = "order[0][column]".FromQueryString<int>(notFoundException: true);
            var orderColumn = ("columns[" + orderColumnIndex + "][data]").FromQueryString<string>(notFoundException: true);
            var orderDir = "order[0][dir]".FromQueryString<string>(notFoundException: true);

            _settings = new InputSettings<T>()
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                OrderColumns = new string[] { orderColumn + " " + orderDir },
                Entities = _entities
            };
        }

        private int GetPropValue(T data, string prop)
        {
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo p = props.FirstOrDefault(f => f.Name.ToLowerInvariant() == prop.ToLowerInvariant());

            if(p == null)
                throw new Exception("Uso no válido de la funcion de paginado");
            
            return Convert.ToInt32(p.GetValue(data));
        }

        #endregion

        #region Metodos publicos

        public Results<T> Process()
        {
            int total = 0;
            int filtered = 0;
            List<T> data = new List<T>();

            if (!_withParameters)
            {
                data = _entities.Invoke(_settings.Start, _settings.Length, _settings.Search, _settings.OrderColumns);

                //Paginado con cout en el query
                if (_count == null)
                {
                    if (data != null && data.Count > 0)
                    {
                        total = GetPropValue(data[0], "total_rows");
                        filtered = GetPropValue(data[0], "filtered_rows");
                    }
                }
                else
                {
                    total = _count.Invoke(null);
                    filtered = string.IsNullOrWhiteSpace(_settings.Search) ? total : _count.Invoke(_settings.Search);
                }
                
            }
            else
            {
                if (_parameters == null)
                {
                    data = _entitiesWithParamClaveValor.Invoke(_settings.Start, _settings.Length, _settings.Search, _settings.OrderColumns, _paramClaveValor);

                    if (_countWithParamClaveValor == null)
                    {
                        if (data != null && data.Count > 0)
                        {
                            total = GetPropValue(data[0], "total_rows");
                            filtered = GetPropValue(data[0], "filtered_rows");
                        }
                    }
                    else
                    {
                        total = _countWithParamClaveValor.Invoke(null, _paramClaveValor);
                        filtered = string.IsNullOrWhiteSpace(_settings.Search) ? total : _countWithParamClaveValor.Invoke(_settings.Search, _paramClaveValor);
                    }
                }
                else
                {
                    data = _entitiesWithParameters.Invoke(_settings.Start, _settings.Length, _settings.Search, _settings.OrderColumns, _parameters);
                    if (_countWithParameters == null)
                    {
                        if (data != null && data.Count > 0)
                        {
                            total = GetPropValue(data[0], "total_rows");
                            filtered = GetPropValue(data[0], "filtered_rows");
                        }
                    }
                    else
                    {
                        total = _countWithParameters.Invoke(null, _parameters);
                        filtered = string.IsNullOrWhiteSpace(_settings.Search) ? total : _countWithParameters.Invoke(_settings.Search, _parameters);
                    }
                    
                }
            }

            return new Results<T>
            {
                draw = _settings.Draw,
                recordsTotal = total,
                recordsFiltered = filtered,
                data = data
            };
        }

        #endregion
    }
}
