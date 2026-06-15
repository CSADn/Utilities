using System.Collections.Generic;
using System.Linq;
using uniVerseSise.DBManager;
using uniVerseSise.Entidades;

namespace uniVerseSise.Manager
{
    public static class LookupsManager
    {
        /// <summary>
        /// Retorna todas las sucursales
        /// </summary>
        /// <returns></returns>
        public static List<Sucursal> ObtenerSucursales()
        {
            using (var cnx = ConexionUV.CrearConexion())
            {
                return ObtenerListado<Sucursal>();
            }
        }

        /// <summary>
        /// Retorna todos los tipos de endosos
        /// </summary>
        /// <returns></returns>
        public static List<TipoEndoso> ObtenerTiposEndosos()
        {
            using (var cnx = ConexionUV.CrearConexion())
            {
                return ObtenerListado<TipoEndoso>();
            }
        }

        /// <summary>
        /// Retorna todas las sucursales
        /// </summary>
        /// <returns></returns>
        public static List<TipoSubseccion> ObtenerSubsecciones()
        {
            using (var cnx = ConexionUV.CrearConexion())
            {
                return ObtenerListado<TipoSubseccion>();
            }
        }

        /// <summary>
        /// Retorna todas las actividades
        /// </summary>
        /// <returns></returns>
        public static List<Actividad> ObtenerActividades()
        {
            using (var cnx = ConexionUV.CrearConexion())
            {
                return ObtenerListado<Actividad>();
            }
        }

        /// <summary>
        /// Retorna el texto asociado al item de la poliza
        /// </summary>
        /// <param name="id">Formada por: 2x Sección + 9x Póliza + 6x Endoso + 3 Item de Cobertura</param>
        /// <returns></returns>
        public static PolizaSeccionTexto ObtenerPolizaSeccionTexto(Constantes.Secciones seccion, int nroPoliza, int nroEndoso)
        {
            return ObtenerPorId<PolizaSeccionTexto>(seccion.GetHashCode().ToString("00") + nroPoliza.ToString("000000000") + nroEndoso.ToString("000000"));
        }

        /// <summary>
        /// Retorna el texto asociado al item de la poliza
        /// </summary>
        /// <param name="id">Formada por: 2x Sección + 9x Póliza + 6x Endoso + 3 Item de Cobertura</param>
        /// <returns></returns>
        public static PolizaSeccionItemTexto ObtenerPolizaSeccionTextoItem(Constantes.Secciones seccion, int nroPoliza, int nroEndoso, int itemCobertura)
        {
            return ObtenerPorId<PolizaSeccionItemTexto>(seccion.GetHashCode().ToString("00") + nroPoliza.ToString("000000000") + nroEndoso.ToString("000000") + itemCobertura.ToString("000"));
        }

        /// <summary>
        /// Retorna el listado de entidad generica
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> ObtenerListado<T>()
            where T : class, new()
        {
            List<T> listado = TablaManager.Instance.ExecuteCommand<T>(TablaManager.Comandos.SELECT);
            return listado;
        }

        /// <summary>
        /// Retorna un item por identificador
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T ObtenerPorId<T>(string id)
            where T : class, new()
        {
            T listado = TablaManager.Instance.ExecuteCommand<T>(TablaManager.Comandos.SELECT, id).FirstOrDefault();
            return listado;
        }
    }
}
