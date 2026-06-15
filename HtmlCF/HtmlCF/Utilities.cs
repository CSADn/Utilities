using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Net;
using System.IO;

namespace HtmlCF.Utilities
{
    public class Dictionary
    {
        public static Dictionary<T, string> Of<T>()
        {
            var type = typeof(T);

            if (!type.IsEnum)
                throw new Exception("Source type not allowed.");

            var fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);

            var result = new Dictionary<T, string>();

            foreach (var f in fields)
            {
                var value = f.GetValue(null);
                var desc = Tools.GetCustomAttribute<DescriptionAttribute>("Description", f);

                if (string.IsNullOrEmpty(desc))
                    desc = f.GetValue(null).ToString();

                result.Add((T)value, desc);
            }

            return result;
        }

        /// <summary>
        /// Devuelve un diccionario a partir de una enumeración y permite especificar el tipo de dato para la clave.
        /// </summary>
        /// <typeparam name="TKey">Tipo de dato de la clave del diccionario.</typeparam>
        /// <typeparam name="T">Tipo de dato de la enumeración.</typeparam>
        /// <returns></returns>
        public static Dictionary<TKey, string> Of<TKey, T>()
        {
            var dic = Of<T>();

            var a = new List<string>();

            return dic.ToDictionary(
                k => (TKey)Convert.ChangeType(k.Key, typeof(TKey)),
                v => v.Value
            );
        }
    }

    public class Tools
    {
        public static string GetCustomAttribute<T>(FieldInfo fi)
        {
            return GetCustomAttribute<T>(null, fi);
        }

        public static string GetCustomAttribute<T>(string propertyName, FieldInfo fi)
        {
            var desc = fi.GetValue(null).ToString();
            var attr = fi.GetCustomAttributes(typeof(T), false).FirstOrDefault();

            if (attr != null)
            {
                var castAttr = (T)attr;

                if (!string.IsNullOrEmpty(propertyName))
                {
                    var pi = castAttr.GetType().GetProperty(propertyName);

                    if (pi != null)
                        return pi.GetValue(castAttr, null).ToString();
                }
                else
                    return castAttr.ToString();
            }

            return string.Empty;
        }

        public static bool BoolTryParse(string input, out bool output)
        {
            output = false;

            if (string.IsNullOrEmpty(input))
                return false;

            input = input.Trim().ToLower();

            if (input.Equals("true"))
            {
                output = true;
                return true;
            }

            if (input.Equals("false"))
                return true;

            if (input.Equals("verdadero"))
            {
                output = true;
                return true;
            }

            if (input.Equals("falso"))
                return true;

            if (input.Equals("1"))
            {
                output = true;
                return true;
            }

            if (input.Equals("0"))
                return true;

            if (input.Equals("v"))
            {
                output = true;
                return true;
            }

            if (input.Equals("f"))
                return true;

            if (input.Equals("s"))
            {
                output = true;
                return true;
            }

            if (input.Equals("n"))
                return true;

            return false;
        }

        public static List<Cookie> ParseCookieValues(string domain, string cookie)
        {
            var values = new List<Cookie>();

            if (string.IsNullOrEmpty(cookie))
                return values;

            var tokens = cookie.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var t in tokens)
            {
                var keyvalue = t.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (keyvalue.Length == 2)
                    values.Add(new Cookie
                    {
                        Domain = domain,
                        Name = keyvalue[0].Trim(),
                        Value = keyvalue[1].Trim()
                    });
            }

            return values;
        }

        public static List<Cookie> ParseCookieFile(string path)
        {
            var lines = File.ReadAllLines(path);

            return lines
                .Where(w => !w.Trim().StartsWith("#"))
                .Select(line =>
                {
                    var values = line.Split('\t');
                    var expires = (
                        values[4].Trim() == "0"
                            ? DateTime.MinValue
                            : DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(values[4].Trim())).DateTime
                    );

                    return new Cookie
                    {
                        Domain = values[0].Trim(),
                        Path = values[2].Trim(),
                        Expires = expires,
                        Name = values[5].Trim(),
                        Value = values[6].Trim()
                    };
                })
                .ToList();
        }
    }
}
