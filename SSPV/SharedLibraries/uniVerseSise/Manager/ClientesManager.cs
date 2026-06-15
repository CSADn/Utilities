using System.Collections.Generic;
using System.Linq;
using uniVerseSise.Entidades;
using uniVerseSise.Helpers;

namespace uniVerseSise.Manager
{
    public class ClientesManager: Singleton<ClientesManager>
    {
        #region Constructor

        private ClientesManager() { }

        #endregion

        /// <summary>
        /// Retorna los asegurados por código de asegurado
        /// </summary>
        /// <param name="codigoAsegurado"></param>
        /// <returns></returns>
        public Cliente Obtener(string codigoAsegurado)
        {
            if (codigoAsegurado.Length != 8)
                throw new System.Exception("El código del asegurado es inválido");

            Cliente asegurado = TablaManager.Instance.ExecuteCommand<Cliente>(TablaManager.Comandos.SELECT, codigoAsegurado).FirstOrDefault();
            return asegurado;
        }

        /// <summary>
        /// Retorna los clientes por nombre y apellido
        /// </summary>
        /// <param name="apellido"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public List<Cliente> Obtener(string apellido, string nombre = null)
        {
            if (string.IsNullOrWhiteSpace(apellido))
                throw new System.Exception("El apellido no es válido");

            string parametros = "WITH F101 \"@PARAM\"".Replace("@PARAM", apellido.Replace("\"", string.Empty).ToUpper());
            if (!string.IsNullOrWhiteSpace(nombre))
                parametros += " AND F102 \"@PARAM\"".Replace("@PARAM", nombre.Replace("\"", string.Empty).ToUpper());

            List<Cliente> asegurados = TablaManager.Instance.ExecuteCommand<Cliente>(TablaManager.Comandos.SELECT, parametros);
            return asegurados;
        }
    }
}
