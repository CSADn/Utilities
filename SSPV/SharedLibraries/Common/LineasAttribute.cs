using System;

namespace Common
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class LineaAttribute : Attribute
    {
        #region Propiedades

        public string Linea { get; private set; }

        public string Descripcion { get; private set; }

        #endregion

        #region Constructor

        public LineaAttribute(string linea, string descripcion)
        {
            if (string.IsNullOrWhiteSpace(linea))
                throw new ArgumentNullException("Debe especificar la linea");

            if (linea.Length != 2)
                throw new Exception("Linea debe ser un string de longitud 2");

            Linea = linea;
            Descripcion = descripcion;
        }

        #endregion
    }
}
