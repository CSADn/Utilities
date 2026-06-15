using System.Collections.Generic;
using System.Text;
using uniVerseSise.Entidades;
using uniVerseSise.Helpers;

namespace uniVerseSise.Manager
{
    public class PolizasCoberturasManager: Singleton<PolizasCoberturasManager>
    {
        private PolizasCoberturasManager() { }

        public List<PolizaCobertura> Obtener(Constantes.Secciones seccion, int nroPoliza, int nroEndoso)
        {
            //Busco las polizas por seccion, nroPoliza y nroEndoso
            string nrosec = seccion.GetHashCode().ToString("00");
            string nropol = nroPoliza.ToString("000000000");
            string nroendoso = nroEndoso.ToString("000000");

            List<PolizaCobertura> retValue = new List<PolizaCobertura>();

            //La idea es esta, busco de a 10 items de cobertura, si hay 10 items, busco otros 10, y así
            int indexItemIni = 0;
            int indexItemFin = 10;
            bool qryFound = true;

            while (qryFound)
            {
                StringBuilder str = new StringBuilder();

                for (int i = indexItemIni; i <= indexItemFin; i++)
                    str.Append(nrosec + nropol + nroendoso + i.ToString("000")).Append(" ");

                indexItemIni = indexItemFin;
                indexItemFin += 10;

                str.Remove(str.Length - 1, 1);

                List<PolizaCobertura> polizasCoberturas = TablaManager.Instance.ExecuteCommand<PolizaCobertura>(TablaManager.Comandos.SELECT, str.ToString());
                if (polizasCoberturas.Count > 0)
                    retValue.AddRange(polizasCoberturas);

                qryFound = polizasCoberturas.Count >= 10;
            }

            return retValue;
        }
    }
}
