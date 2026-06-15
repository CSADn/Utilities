using System;

namespace uniVerseSise.Helpers
{
    public class Singleton<T>
       where T : class
    {
        /// <summary>
        /// Instancia de la aplicacion
        /// </summary>
        private static volatile T instance;

        /// <summary>
        /// Clase utilzada para la sincronizacion de threads (en el caso de que sea necesario)
        /// </summary>
        protected static object _lock = new object();

        /// <summary>
        /// Constructor de la clase
        /// </summary>
        protected Singleton()
        {
        }

        /// <summary>
        /// Punto de acceso a la instancia
        /// </summary>
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (_lock)
                    {
                        if (instance == null)
                        {
                            instance = (T)Activator.CreateInstance(typeof(T), true);
                        }
                    }
                }
                return instance;
            }
        }
    }
}
