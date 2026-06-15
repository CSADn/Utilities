using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Common
{
    public static class Extensions
    {
        #region Extensiones
        private static SISEAttribute GetSISEAttribute(this Enum value)
        {
            try
            {
                Type type = value.GetType();
                FieldInfo fInfo = type.GetField(value.ToString());
                SISEAttribute attr = fInfo.GetCustomAttribute<SISEAttribute>(false);

                return attr;
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public static T GetCustomEnumAttribute<T>(this Enum value) where T: Attribute
        {
            try
            {

                Type type = value.GetType();
                FieldInfo fInfo = type.GetField(value.ToString());
                T attr = fInfo.GetCustomAttribute<T>(false);

                return attr;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string CodigoSISE(this Enum value)
        {
            if (value == null)
                return null;

            return value.CodigosSISE()?.FirstOrDefault();
        }

        public static string[] CodigosSISE(this Enum value)
        {
            if (value == null)
                return null;

            try
            {
                return value.GetSISEAttribute()?.Codigos;
            }
            catch
            {
                return null;
            }
        }

        public static string DescripcionSISE(this Enum value)
        {
            if (value == null)
                return string.Empty;

            try
            {
                return value.GetSISEAttribute()?.Descripcion;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string DigitsOnly(this string value)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(value, "");
        }

        public static string GetLinea(this Enum value)
        {
            if (value == null)
                return null;

            return value.GetCustomEnumAttribute<LineaAttribute>()?.Linea;
        }

        public static string GetLineaDescription(this Enum value)
        {
            if (value == null)
                return null;

            return value.GetCustomEnumAttribute<LineaAttribute>()?.Descripcion;
        }

        #endregion

    }
}
