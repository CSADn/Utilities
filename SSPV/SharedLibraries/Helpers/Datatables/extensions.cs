using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataTables.Extensions
{
    public static class ExtensionMethods
    {
        #region Filter

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> input, string token)
        {
            return input
                .Where(w => Matched(w, token));
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> input, string token, bool recursive)
        {
            return input
                .Where(w => Matched(w, token, recursive));
        }

        private static bool Matched<T>(T target, string input)
        {
            input = input.ToLower().Trim();

            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var p in properties)
            {
                var value = p.GetValue(target, null);

                var valueStr = (value == null ? null : value.ToString().ToLower().Trim());

                if (!string.IsNullOrWhiteSpace(valueStr) && valueStr.Contains(input))
                    return true;
            }

            return false;
        }

        private static bool Matched<T>(T target, string input, bool recursive)
        {
            input = input.ToLower().Trim();

            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var p in properties)
            {
                if(recursive && p.PropertyType.IsClass && p.PropertyType.Assembly.FullName == type.Assembly.FullName)
                {
                    MethodInfo matched = typeof(ExtensionMethods).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                                                                 .FirstOrDefault(f => f.Name == "Matched" && f.GetParameters().Count() == 3);

                    if (matched == null) return false;

                    MethodInfo toInvoke = matched.MakeGenericMethod(p.PropertyType);
                    return Convert.ToBoolean(toInvoke.Invoke(null, new[] { p.GetValue(target, null), input, recursive }));
                }
                else
                { 
                    var value = p.GetValue(target, null);

                    var valueStr = (value == null ? null : value.ToString().ToLower().Trim());

                    if (!string.IsNullOrWhiteSpace(valueStr) && valueStr.Contains(input))
                        return true;
                }
            }

            return false;
        }
        
        #endregion


        #region Order

        public static IEnumerable<T> Order<T>(this IEnumerable<T> input, string column, OrderDirection? direction)
        {
            if (string.IsNullOrWhiteSpace(column))
                return input;

            var type = typeof(T);
            var property = type.GetProperty(column);

            if (property == null)
                throw new ArgumentNullException("Order: Propiedad no encontrada '" + column + "'.");

            var directionValue = direction ?? OrderDirection.Ascending;

            switch (directionValue)
            {
                case OrderDirection.Ascending:
                    return input
                        .OrderBy(o => property.GetValue(o, null));

                case OrderDirection.Descending:
                    return input
                        .OrderByDescending(o => property.GetValue(o, null));

                default:
                    return input;
            }
        }

        #endregion
    }
}
