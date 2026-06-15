using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Xml.Serialization;

namespace Helpers
{
    public static class Extensions
    {
        public static string FromConnections(this string key, bool notFoundException = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException();

            var value = ConfigurationManager.ConnectionStrings[key];

            if (value == null && notFoundException)
                throw new KeyNotFoundException("'" + key.Trim() + "' not found in ConnectionStrings.");

            return value?.ConnectionString;
        }

        public static T FromAppSettings<T>(this string key, T defaultValue = default(T), bool notFoundException = false)
        {
            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            var value = ConfigurationManager.AppSettings[key];

            if (value == null && notFoundException)
                throw new KeyNotFoundException("'" + key.Trim() + "' not found in AppSettings.");

            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Utilities.CastValue(value, defaultValue);
        }

        public static bool ConnectionExists(this string key)
            => !string.IsNullOrWhiteSpace(key.FromConnections(false));

        public static T FromQueryString<T>(this string key, T defaultValue = default(T), bool notFoundException = false)
        {
            if (HttpContext.Current == null || HttpContext.Current.Request == null)
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            var value = HttpContext.Current.Request.QueryString[key] ?? HttpContext.Current.Request.Form[key];

            if (value == null && notFoundException)
                throw new KeyNotFoundException("'" + key.Trim() + "' not found in QueryString.");

            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            return Utilities.CastValue(value, defaultValue);
        }

        public static T FromSession<T>(this string key, T defaultValue = default(T), bool notFoundException = false)
        {
            if (HttpContext.Current == null)
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(key))
                return defaultValue;

            if (HttpContext.Current.Session == null || HttpContext.Current.Session.Count == 0)
                return defaultValue;

            if (HttpContext.Current.Session[key] == null && notFoundException)
                throw new KeyNotFoundException("'" + key.Trim() + "' not found in Session.");

            var obj = HttpContext.Current.Session[key];

            if (obj == null)
                return defaultValue;

            return Utilities.CastValue(obj, defaultValue);
        }


        public static string Description(this Enum value)
        {
            if (value == null)
                return string.Empty;

            try
            {
                var type = value.GetType();
                var field = type.GetField(value.ToString());
                var attr = field.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var desc = ((DescriptionAttribute)attr[0]).Description;

                return desc;
            }
            catch
            {
                return string.Empty;
            }
        }


        public static SqlCommand Query(this SqlCommand cmd, string query)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(query))
                throw new ArgumentNullException();

            var q = query; // Fortify. No remover esta línea.
            cmd.CommandText = q;

            return cmd;
        }

        public static SqlCommand AddParam(this SqlCommand cmd, string name, object value)
        {
            if (cmd == null || string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException();

            cmd.Parameters.AddWithValue(name, value);

            return cmd;
        }

        public static SqlCommand ResetParams(this SqlCommand cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException();

            cmd.Parameters.Clear();

            return cmd;
        }


        public static string ToSqlValue(this object value)
        {
            if (value == null)
                return "null";

            var t = value.GetType();
            var tc = Type.GetTypeCode(t);

            switch (tc)
            {
                case TypeCode.String:
                case TypeCode.Char:
                    return "'" + ((string)value).Replace("'", "''") + "'";

                case TypeCode.Boolean:
                    return ((bool)value ? "'1'" : "'0'");

                case TypeCode.DBNull:
                    return "null";

                case TypeCode.DateTime:
                    return "'" + ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss") + "'";

                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return value.ToString().Replace(",", ".");

                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return value.ToString();

                case TypeCode.Object:

                    if (t == typeof(byte[]))
                        return "0x" + BitConverter.ToString((byte[])value).Replace("-", string.Empty);
                    else if (t == typeof(TimeSpan))
                        return "'" + ((TimeSpan)value).ToString() + "'";
                    else
                        throw new NotSupportedException();

                default:
                    throw new NotSupportedException();
            }
        }


        public static string ToJson(this Exception ex)
        {
            if (ex == null)
                return string.Empty;

            var type = ex.GetType();

            var toJsonMethod = type.GetMethod("ToJson");
            if (toJsonMethod?.ReturnType == typeof(string))
                return toJsonMethod.Invoke(ex, null).ToString();

            var codePi = type.GetProperty("Code");
            var descPi = type.GetProperty("ExtendedDescription");
            var argPi = type.GetProperty("Argument");

            var code = (
                codePi == null
                    ? 0
                    : (int)codePi.GetValue(ex, null)
            );

            var desc = (
                descPi == null
                    ? ex.Message + (
                        ex.InnerException == null
                            ? string.Empty
                            : "\n" + ex.InnerException.Message
                    )
                    : (string)descPi.GetValue(ex, null)
            );

            var arg = (
                argPi == null
                ? string.Empty
                : (string)argPi.GetValue(ex, null)
            );

            return Utilities.ToJsonError(code, desc, arg);
        }

        public static string ToDoubleSise(this double value, int decimales = 2)
        {
            var mult = int.Parse(string.Concat("1", "0".PadLeft(decimales, '0')));
            return (Math.Truncate(Math.Round(value, decimales) * mult)).ToString();
        }

        public static string Right(this string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return (value.Length <= length
                   ? value
                   : value.Substring(value.Length - length)
                   );
        }

        public static string Left(this string value, int length)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return (value.Length <= length
                   ? value
                   : value.Substring(0, length)
                   );

        }

        public static string GetIP(this HttpRequest request, bool CheckForward = false)
        {
            //
            // http://stackoverflow.com/a/13249280/52277
            //

            string ip = null;
            if (CheckForward)
            {
                ip = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            }

            if (string.IsNullOrEmpty(ip))
            {
                ip = request.ServerVariables["REMOTE_ADDR"];
            }
            else
            { // Using X-Forwarded-For last address
                ip = ip.Split(',')
                       .Last()
                       .Trim();
            }

            return ip;
        }


        public static T NewInstanceOf<T>(T input) where T : class
        {
            if (input == null)
                return null;

            var s = new XmlSerializer(input.GetType());
            var w = new StringWriter();
            T o = null;

            s.Serialize(w, input);
            var r = new StringReader(w.ToString());
            o = (T)s.Deserialize(r);

            w.Close();
            r.Close();

            return o;
        }

        public static string RegexReplace(this string input, string match, string replacement)
        {
            return Regex.Replace(input, match, replacement, RegexOptions.IgnoreCase);
        }

        public static string ToHexString(this byte[] input)
        {
            var sb = new StringBuilder();

            foreach (var b in input)
                sb.Append(string.Format("{0:x2}", b));

            return sb.ToString();
        }


        public static T GetValue<T>(this DataRow row, string column, T defaultValue = default(T), bool notFoundException = true)
        {
            if (row == null)
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(column))
                throw new ArgumentNullException();

            try
            {
                var value = row[column];

                if (value == DBNull.Value)
                    return defaultValue;
                else if (typeof(T).IsArray)
                    return (T)Convert.ChangeType(value, typeof(T));
                else
                {
                    var tc = Type.GetTypeCode(typeof(T));
                    return (T)Convert.ChangeType(value, tc);
                }
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        public static DateTime FromUnixTime(this int units, bool inMilliseconds = false)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            if (inMilliseconds)
                return epoch.AddMilliseconds(units);
            else
                return epoch.AddSeconds(units);
        }

        public static void RetryWhenFail(Func<int, bool> action, bool throwWhenOut = false, int retries = 3)
        {
            if (action == null)
                return;

            var @try = 1;
            var @continue = true;

            while ((@try < retries) && @continue)
            {
                if (action.Invoke(@try))
                    @continue = false;

                @try++;
            }

            if (throwWhenOut && (@try > retries))
                throw new IndexOutOfRangeException();
        }


        public static Dictionary<TKey, TValue> AddRange<TKey, TValue>(this Dictionary<TKey, TValue> input, Dictionary<TKey, TValue> range)
        {
            if (input == null)
                throw new ArgumentNullException();

            if (range == null || range.Count == 0)
                return input;

            foreach (var v in range)
            {
                if (input.ContainsKey(v.Key))
                    throw new Exception("La clave ya existe '" + v.Key + "'");
                else
                    input.Add(v.Key, v.Value);
            }

            return input;
        }


        public static string GetContentString(this HttpContext context)
        {
            if (context.Request.InputStream.Length == 0)
                return string.Empty;

            var content = string.Empty;

            using (var sr = new StreamReader(context.Request.InputStream))
            {
                content = sr.ReadToEnd();
            }

            return content;
        }

        public static byte[] GetContentBytes(this HttpContext context)
        {
            if (context.Request.ContentLength == 0)
                return new byte[0];

            using (var br = new BinaryReader(context.Request.InputStream))
            {
                return br.ReadBytes(context.Request.ContentLength);
            }
        }

        public static string UTF8ToBase64(this string utf8String)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(utf8String);
            var text = Convert.ToBase64String(bytes);
            return text;
        }

        public static string Base64ToUTF8(this string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            var text = System.Text.Encoding.UTF8.GetString(bytes);
            return text;
        }


        public static T FromCache<T>(this string key, Func<T> getItemMethod)
        {
            return FromCache(key, getItemMethod, "CacheItemTimeout", null, true);
        }

        public static T FromCache<T>(this string key, Func<T> getItemMethod, int? timeout)
        {
            return FromCache(key, getItemMethod, string.Empty, timeout, true);
        }

        public static T FromCache<T>(this string key, Func<T> getItemMethod, bool infiniteUpdate)
        {
            return FromCache(key, getItemMethod, "CacheItemTimeout", null,  infiniteUpdate);
        }

        public static T FromCache<T>(this string key, Func<T> getItemMethod, int? timeout, bool infiniteUpdate)
        {
            return FromCache(key, getItemMethod, string.Empty, timeout, infiniteUpdate);
        }

        public static T FromCache<T>(this string key, Func<T> getItemMethod, string cacheTimeoutKey)
        {
            return FromCache(key, getItemMethod, cacheTimeoutKey, null, true);
        }

        public static T FromCache<T>(this string key, Func<T> getItemMethod, string cacheTimeoutKey, bool infiniteUpdate)
        {
            return FromCache(key, getItemMethod, cacheTimeoutKey, null, infiniteUpdate);
        }

        public static T FromCache<T>(this string key, Func<T> getItemMethod, string cacheTimeoutKey = "CacheItemTimeout", int? timeout = null, bool infiniteUpdate = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException();

            if (getItemMethod == null)
                throw new ArgumentNullException();

            if (string.IsNullOrWhiteSpace(cacheTimeoutKey) && !timeout.HasValue)
                throw new ArgumentException();

            var cacheTimeout = timeout ?? cacheTimeoutKey.FromAppSettings(5.00);

            var context = HttpContext.Current;

            if (context == null || cacheTimeout <= 0.00)
            {
                var item = getItemMethod.Invoke();

                if (item == null)
                    return default(T);

                return item;
            }

            if (context.Cache[key] == null)
            {
                var item = getItemMethod.Invoke();

                if (item == null)
                    return default(T);

                context
                    .Cache
                    .Insert(
                        key,
                        item,
                        null,
                        DateTime.Now.AddMinutes(cacheTimeout),
                        Cache.NoSlidingExpiration,
                        (string k, CacheItemUpdateReason r, out object o, out CacheDependency d, out DateTime e, out TimeSpan s) =>
                        {
                            d = null;
                            e = (infiniteUpdate ? DateTime.Now.AddMinutes(cacheTimeout) : DateTime.Now);
                            s = Cache.NoSlidingExpiration;

                            o = (infiniteUpdate ? getItemMethod.Invoke() : default(T));
                        }
                    );

                return item;
            }
            else
                return (T)context.Cache[key];
        }

        public static bool UpdateCache(this string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException();

            var context = HttpContext.Current;

            if (context == null)
                return true;

            if (context.Cache[key] != null)
            {
                context.Cache.Remove(key);
                return true;
            }
            else
                return false;
        }

        public static string ToCurrency(this double input, bool withSymbol = true, bool withDecimals = true, string symbol = "$ ", int decimals = 2)
        {
            input = (withDecimals ? input : Math.Truncate(input));

            return input
                .ToString((withDecimals ? $"C{decimals}": "C0"), System.Globalization.CultureInfo.CreateSpecificCulture("es-AR"))
                .Replace("$", (withSymbol ? symbol : string.Empty));
        }

        public static string ToPercent(this double input)
        {
            return (input / 100)
                .ToString("P1", System.Globalization.CultureInfo.CreateSpecificCulture("es-AR"));
        }

        public static bool Between(this double value, double start, double end)
        {
            return value >= start && value <= end;
        }

        public static bool Between(this int value, int start, int end, bool excludeLimits = false)
        {
            if (excludeLimits)
                return value > start && value < end;

            return value >= start && value <= end;
        }

        public static bool Between(this DateTime value, DateTime start, DateTime end, bool excludeLimits = false)
        {
            if (excludeLimits)
                return value > start && value < end;

            return value >= start && value <= end;
        }

        public static bool TryParseFecha(this string value, string format)
        {
            DateTime Fecha;
            return DateTime.TryParseExact(value, format,
                           CultureInfo.InvariantCulture,
                           DateTimeStyles.None,
                           out Fecha);
        }

        public static bool IsNull(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static string ValueOrEmpty(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;
            return input;
        }

        /// <summary>
        /// Evalua si la lista es nulla o vacia
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="exceptionMessage">Si no es vacío, se lanza excepción con el mensaje</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this List<T> list, string exceptionMessage) where T: class
        {
            bool result = false;

            if (list == null || list.Count == 0)
                result = true;

            if (result && !string.IsNullOrWhiteSpace(exceptionMessage))
                throw new NullReferenceException(exceptionMessage);

            return result;
        }

        /// <summary>
        /// Copia todas las propiedades y atributos de un objeto en otro, siempre que tengan el mismo nombre y tipo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public static T ConvertTo<S, T>(this S source) 
            where T: class, new()
            where S: class, new()
        {
            var sourceType = typeof(S);
            var targetType = typeof(T);

            T target = new T();

            foreach (var sourceProperty in sourceType.GetProperties())
            {
                var targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty != null && targetProperty.PropertyType == sourceProperty.PropertyType)
                    targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
            }

            foreach (var sourceField in sourceType.GetFields())
            {
                var targetField = targetType.GetField(sourceField.Name);
                if (targetField != null && targetField.FieldType == sourceField.FieldType)
                    targetField.SetValue(target, sourceField.GetValue(source));
            }

            return target;
        }

        /// <summary>
        /// Si str es nulo, entonces retorna el string vacio, sino retorna str
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetNotNull(this string str)
        {
            if (str.IsNull())
                return "";
            return str;
        }
    }
}
