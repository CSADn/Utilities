using System;
using System.Collections.Generic;
using System.Linq;

namespace uniVerseSise.Manager.Helpers
{
    public static class ConverterManager
    {
        public static object Convert(string value, Type memType)
        {
            try
            {
                if (memType == typeof(string))
                    return value;

                if (string.IsNullOrEmpty(value))
                    return GetDefault(memType);

                //En el caso especial que sea DateTime
                if (memType == typeof(DateTime) || memType == typeof(DateTime?))
                    return TablaManager.BaseDate.AddDays(int.Parse(value));

                //En el caso de que sea Double
                List<Type> doubleTypeList = new List<Type>()
                {
                    typeof(double),
                    typeof(double?),
                    typeof(float),
                    typeof(float?)
                };

                //Si es double, se parsea con dos decimales
                if (doubleTypeList.Any(d => d == memType))
                    return double.Parse(value) / 100.0;

                return System.Convert.ChangeType(value, Nullable.GetUnderlyingType(memType) ?? memType);
            }
            catch (Exception ex)
            {
                string err = $"Conversión No se puede convertir el tipo {memType.Name} - valor {value}";
                throw new Exception(err, ex);
            }
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
