using System;
using System.Collections.Generic;
using System.Configuration;

namespace SubdivxRipper
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

        public static bool IsNull(this string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public static string Numeric(this int input)
        {
            return input
                .ToString("C0")
                .Replace("$", string.Empty);
        }
    }
}
