using System;


namespace sysoneBus.Enums
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class TipoDocumentoAttribute:Attribute
    {
        #region Propiedades

        public int TipoDocuemnto { get; private set; }
        public string Descripcion { get; private set; }

        #endregion

        #region Constructor

        public TipoDocumentoAttribute(int tipodocumento, string descripcion)
        {
            if (tipodocumento > 0)
                throw new ArgumentNullException("El numro de tipodocumento no puede ser igual a cero");

            TipoDocuemnto = tipodocumento;
            Descripcion = descripcion;
        }

        #endregion
    }
}
