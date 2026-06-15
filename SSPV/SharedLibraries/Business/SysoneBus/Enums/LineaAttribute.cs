using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Enums
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class LineaAttribute : Attribute
    {
        #region Propiedades

        public int Linea { get; private set; }

        public string Descripcion { get; private set; }

        #endregion

        #region Constructor

        public LineaAttribute(int linea, string descripcion)
        {
            if (linea > 0)
                throw new ArgumentNullException("El numro de linea no puede ser igual a cero");

            Linea = linea;
            Descripcion = descripcion;
        }

        #endregion
    }
}
