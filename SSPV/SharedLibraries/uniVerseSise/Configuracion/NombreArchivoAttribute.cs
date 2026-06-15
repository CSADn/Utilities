using System;

namespace uniVerseSise.Configuracion
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NombreArchivoAttribute: Attribute
    {
        public string Archivo { get; set; }
    }
}
