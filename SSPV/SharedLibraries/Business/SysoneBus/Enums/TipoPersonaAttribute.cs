using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sysoneBus.Enums
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TipoPersonaAttribute:Attribute
    {
        #region Propiedades

        public int TipoPersona { get; private set; }
        public string Descripcion { get; private set; }

        #endregion

        #region Constructor

        public TipoPersonaAttribute(int tipodopersona, string descripcion)
        {
            if (tipodopersona > 0)
                throw new ArgumentNullException("El numro de tipopersona no puede ser igual a cero");

            TipoPersona = tipodopersona;
            Descripcion = descripcion;
        }

        #endregion
    }
}
