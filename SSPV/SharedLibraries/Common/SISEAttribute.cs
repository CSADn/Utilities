using System;
using System.Linq;

namespace Common
{
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class SISEAttribute: Attribute
    {
        #region Privados

        private string[] _codigos = null;

        private string _descripcion = null;

        private bool _habilitado = true;

        #endregion

        #region Read Only Properties

        public string[] Codigos { get { return _codigos; } }

        public string Descripcion { get { return _descripcion; } }

        public bool Habilitado { get { return _habilitado; } }

        #endregion

        #region Constructor

        public SISEAttribute(string[] codigos, string descripcion, bool habilitado = true)
        {
            if (codigos == null || codigos.Length == 0)
                throw new NullReferenceException("Dede Ingresar al menos un código");

            if (string.IsNullOrWhiteSpace(descripcion))
                throw new NullReferenceException("La descripción es obligatoria");

            if (habilitado && codigos.Length == 1 && string.IsNullOrWhiteSpace(codigos[0]))
                throw new Exception("Debe ingresar el código SISE para habilitar el medio de pago");

            if(habilitado && codigos.Length > 1 && codigos.Any(a => string.IsNullOrWhiteSpace(a)))
                throw new Exception("Debe ingresar todos los código SISE para habilitar el medio de pago");

            _codigos = codigos;
            _descripcion = descripcion;
            _habilitado = habilitado && ((codigos.Length == 1 && !string.IsNullOrWhiteSpace(codigos[0])) || (codigos.Length > 1 && codigos.All(a => !string.IsNullOrWhiteSpace(a))));
        }

        #endregion

    }
}
