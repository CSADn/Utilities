using System.Collections.Generic;
using System.Text;
using uniVerseSise.Entidades;
using uniVerseSise.Helpers;

namespace uniVerseSise.Manager
{
    public class NominaSubgruposManager: Singleton<NominaSubgruposManager>
    {
        #region Constructor

        private NominaSubgruposManager() { }
        #endregion

        #region Metodos

        public List<NominaSubgrupo> Obtener(Constantes.Secciones seccion, int nroPoliza, int nroEndoso, int subgrupo)
        {
            //Busco las polizas por seccion, nroPoliza y nroEndoso
            string nrosec = seccion.GetHashCode().ToString("00");
            string nropol = nroPoliza.ToString("000000000");
            string nroendoso = nroEndoso.ToString("000000");
            string nrosubgrupo = subgrupo.ToString("000");

            List<NominaSubgrupo> retValue = new List<NominaSubgrupo>();

            //La idea es esta, busco de a 10 items de cobertura, si hay 10 items, busco otros 10, y así
            int indexItemIni = 0;
            int indexItemFin = 10;
            bool qryFound = true;

            while (qryFound)
            {
                StringBuilder str = new StringBuilder();

                for (int i = indexItemIni; i <= indexItemFin; i++)
                    str.Append(nrosec + nropol + nroendoso + nrosubgrupo + i.ToString("0000")).Append(" ");
                indexItemIni = indexItemFin;
                indexItemFin += 10;

                str.Remove(str.Length - 1, 1);

                List<NominaSubgrupo> nominaSubgrupo = TablaManager.Instance.ExecuteCommand<NominaSubgrupo>(TablaManager.Comandos.SELECT, str.ToString());
                if (nominaSubgrupo.Count > 0)
                    retValue.AddRange(nominaSubgrupo);

                qryFound = nominaSubgrupo.Count >= 10;
            }

            return retValue;
        }

        #endregion
    }
}
