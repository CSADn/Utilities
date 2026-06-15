using PlanesMultilinea.Enums;
using System;
using System.Collections.Generic;

namespace PlanesMultilinea.EntitiesCustom
{
    public class WSPlanMultilinea
    {
        #region Propiedades

        public string Sistema { get; set; }
        public string PlanJson { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }

        #endregion

        #region Metodos

        public Sistema GetSistema()
        {
            Dictionary<string, Sistema> _dicResponseSistema = new Dictionary<string, Sistema>()
            {
                { Enums.Sistema.AP.ToString(), Enums.Sistema.AP },
                { Enums.Sistema.Hogar.ToString(), Enums.Sistema.Hogar },
                { Enums.Sistema.PlusMapp.ToString(), Enums.Sistema.PlusMapp }
            };

            if (_dicResponseSistema.ContainsKey(Sistema))
                return _dicResponseSistema[Sistema];

            throw new Exception("Sistema no encontrado");

        }

        public ResponseStatus GetStatus()
        {
            Dictionary<string, ResponseStatus> _dicResponseStatus = new Dictionary<string, ResponseStatus>()
            {
                { ResponseStatus.OK.ToString(), ResponseStatus.OK },
                { ResponseStatus.Error.ToString(), ResponseStatus.Error }
            };

            return _dicResponseStatus.ContainsKey(Status) ? _dicResponseStatus[Status] : ResponseStatus.Error;
        }

        #endregion
    }
}
