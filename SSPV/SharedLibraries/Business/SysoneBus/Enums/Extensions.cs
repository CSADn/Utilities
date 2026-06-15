using System;
using System.Reflection;


namespace sysoneBus.Enums
{
    public static class Extensions
    {
        public static T GetCustomEnumAttribute<T>(this Enum value) where T : Attribute
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
        public static int GetLinea(this Enum value)
        {
            if (value == null)
                return 0;

            return value.GetCustomEnumAttribute<LineaAttribute>().Linea;
        }

        public static string GetLineaDescription(this Enum value)
        {
            if (value == null)
                return null;

            return value.GetCustomEnumAttribute<LineaAttribute>()?.Descripcion;
        }
        public static int GetTipoDocumento(this Enum value)
        {
            if (value == null)
                return 0;

            return value.GetCustomEnumAttribute<TipoDocumentoAttribute>().TipoDocuemnto;
        }

        public static string GetTipoDocumentoDescription(this Enum value)
        {
            if (value == null)
                return null;

            return value.GetCustomEnumAttribute<TipoDocumentoAttribute>()?.Descripcion;
        }

    }
}
