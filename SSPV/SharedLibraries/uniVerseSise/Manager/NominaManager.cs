using System.Collections.Generic;
using System.Text;
using uniVerseSise.Entidades;
using uniVerseSise.Helpers;

namespace uniVerseSise.Manager
{
    public class NominaManager : Singleton<NominaManager>
    {
        #region Constructor

        private NominaManager() { }

        #endregion

        #region Metodos

        public List<Nomina> Obtener(Constantes.Secciones seccion, int nroPoliza, int nroEndoso, int itemDesde, int itemHasta)
        {
            //Busco las polizas por seccion, nroPoliza y nroEndoso
            string nrosec = seccion.GetHashCode().ToString("00");
            string nropol = nroPoliza.ToString("000000000");
            string nroend = nroEndoso.ToString("000000");

            if (itemDesde > itemHasta)
                throw new System.Exception("Rango de items inválidos");

            if (itemHasta - itemDesde > 200)
                throw new System.Exception("La cantidad de items a buscar debe ser mayor a 0 y menor a 200");

            StringBuilder str = new StringBuilder();

            for (int i = itemDesde; i <= itemHasta; i++)
            {
                str.Append(nrosec + nropol + nroend + i.ToString("00000")).Append(" ");
            }

            str.Remove(str.Length - 1, 1);

            List<Nomina> nomina = TablaManager.Instance.ExecuteCommand<Nomina>(TablaManager.Comandos.SELECT, str.ToString());
            return nomina;
        }

        #endregion
    }
}
