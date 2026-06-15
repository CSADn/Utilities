using System;

namespace uniVerseSise.Configuracion
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class DatoCampoAttribute: Attribute
    {
        /// <summary>
        /// Indica que el atributo es ID
        /// </summary>
        public bool Id { get; set; }

        /// <summary>
        /// Indica que el atributo es texto
        /// </summary>
        public bool Text { get; set; }

        /// <summary>
        /// Indica que es la posicion, si no es multivaluado
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Posicion de campo multivaluado
        /// </summary>
        public int Subposition { get; set; }

        /// <summary>
        /// Para generar un vector a partir de una posicion de un campo
        /// </summary>
        public int[] SubpositionItems { get; set; }

        public DatoCampoAttribute()
        {
            Id = false;
            Text = false;
            Position = 0;
            Subposition = 0;
        }

        public DatoCampoAttribute(bool isId) : this()
        {
            Id = isId;
        }

        public DatoCampoAttribute(int pos): this()
        {
            Position = pos;
        }

        public DatoCampoAttribute(int pos, int subpos) : this()
        {
            Position = pos;
            Subposition = subpos;
        }

        public DatoCampoAttribute(int pos, int[] subpositems) : this()
        {
            Position = pos;
            SubpositionItems = subpositems;
        }
    }
}
