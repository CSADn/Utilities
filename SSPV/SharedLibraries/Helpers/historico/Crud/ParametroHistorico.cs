using DatabaseModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Crud
{
    public class ParametroHistorico
    {
        private static ParametroHistorico _instance;

        public static ParametroHistorico Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ParametroHistorico();

                return _instance;
            }
        }

        public ParametroHistorico()
        {
            //
        }

        public List<Entities.ParametroHistorico> GetParametroHistoricoByClave(string clave)
        {
            try
            {
                string sSql = string.Empty;
                sSql = @"   SELECT
                            ph.Clave,
                            ph.Valor
                            FROM
                            PARAMETROS_HISTORICO ph
                            where ph.Clave = '" + clave + "' ORDER BY 1 ";
                return DataModel.Instance.Execute<Entities.ParametroHistorico>(sSql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
