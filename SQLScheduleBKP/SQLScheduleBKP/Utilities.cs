using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace SQLScheduleBKP
{
    public static class Extensions
    {
        public static bool IsNull(this string input)
        {
            return string.IsNullOrEmpty(input?.Trim());
        }

        public static T FromAppSettings<T>(this string key, T defaultValue = default(T), bool notFoundException = false)
        {
            if (key.IsNull())
                return defaultValue;

            var value = ConfigurationManager.AppSettings[key];

            if (value == null && notFoundException)
                throw new KeyNotFoundException("'" + key.Trim() + "' not found in AppSettings.");

            if (value.IsNull())
                return defaultValue;

            return Utilities.CastValue(value, defaultValue);
        }
    }

    public static class Utilities
    {
        public static T CastValue<T>(object value, T defaultValue)
        {
            T output;
            var stringValue = (value is string ? (string)value : null);

            var t = typeof(T);

            if (t.IsEnum)
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), stringValue);
                }
                catch
                {
                    return defaultValue;
                }
            }
            else if (t == typeof(TimeSpan))
            {
                try
                {
                    var ts = TimeSpan.Parse(stringValue);
                    return (T)Convert.ChangeType(ts, t);
                }
                catch
                {
                    return defaultValue;
                }
            }

            var tc = Type.GetTypeCode(t);

            switch (tc)
            {
                case TypeCode.Boolean:

                    if (stringValue.IsNull())
                        throw new NotSupportedException();

                    var val = 0;

                    if (stringValue.Trim().ToLower() == "true" ||
                        (int.TryParse(stringValue.Trim(), out val) && val >= 1))
                        output = (T)Convert.ChangeType(true, tc);
                    else
                        output = (T)Convert.ChangeType(false, tc);

                    return output;

                case TypeCode.Byte:
                case TypeCode.DateTime:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.String:

                    try
                    {
                        output = (T)Convert.ChangeType(value, tc);
                    }
                    catch
                    {
                        output = defaultValue;
                    }

                    return output;

                case TypeCode.Object:

                    if (typeof(T) == typeof(Dictionary<string, string>))
                    {
                        if (stringValue.IsNull())
                            throw new NotSupportedException();

                        var d = new Dictionary<string, string>();

                        var entries = stringValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (entries == null || entries.Length == 0)
                            return defaultValue;

                        foreach (var e in entries)
                        {
                            if (e.Contains(":"))
                            {
                                var entry = e.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                                if (entry == null || entry.Length != 2 ||
                                    entry[0].IsNull() || entry[1].IsNull())
                                    throw new FormatException();

                                d.Add(entry[0], entry[1]);
                            }
                            else
                                d.Add(e, "");
                        }

                        return (T)Convert.ChangeType(d, typeof(T));
                    }
                    else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
                    {
                        if (stringValue.IsNull())
                            throw new NotSupportedException();

                        var entries = stringValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (entries == null || entries.Length == 0)
                            return defaultValue;

                        var argumentType = typeof(T).GetGenericArguments().First();
                        var convertedList = (System.Collections.IList)Activator.CreateInstance(typeof(T));

                        entries
                            .ToList()
                            .ForEach(f => convertedList.Add(Convert.ChangeType(f, argumentType)));

                        return (T)convertedList;
                    }
                    else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var ut = Nullable.GetUnderlyingType(typeof(T));

                        if (!ut.IsPrimitive)
                            throw new NotSupportedException();

                        if (stringValue.IsNull())
                            return default(T);

                        return (T)Convert.ChangeType(stringValue, ut);
                    }
                    else
                    {
                        try
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch (Exception)
                        {
                            throw new NotSupportedException();
                        }
                    }

                default:
                    throw new NotSupportedException();
            }
        }
    }
}
