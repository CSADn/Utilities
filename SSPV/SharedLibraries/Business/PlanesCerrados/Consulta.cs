using System.Threading.Tasks;
using PlanesMultilinea.Entities;
using PlanesMultilinea.Enums;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace PlanesMultilinea
{
    public class Consulta
    {
        #region Atributos

        private EndpointManager _endpoint;

        #endregion

        #region Publicos

        public Consulta()
        {
            _endpoint = new EndpointManager();
        }

        public async Task<RequestResponse> ObtenerPlan(Sistema sistema, int idPlan)
           => await _endpoint.Get(sistema, "PlanMultilinea/Get", idPlan);

        public async Task<RequestResponse> ObtenerPlanes(Sistema sistema)
            => await _endpoint.Get(sistema, "PlanMultilinea/GetAll");

        public string ObtenerPlan(List<Sistema> sistemas, int idPlan)
        {
            return ObtenerPlanesInternal(sistemas, (s) => ObtenerPlan(s, idPlan));
        }

        public string ObtenerPlanes(List<Sistema> sistemas)
        {
            return ObtenerPlanesInternal(sistemas, (s) => ObtenerPlanes(s));
        }

        #endregion

        #region Privados

        private string ObtenerPlanesInternal(List<Sistema> sistemas, Func<Sistema, Task<RequestResponse>> getCallback)
        {
            var results = new List<object>();

            foreach (var s in sistemas)
            {
                var request = getCallback(s);
                var plan = default(object);
                var error = default(string);

                if (request.Result.Status == ResponseStatus.OK)
                    plan = JsonConvert.DeserializeObject(request.Result.Body);
                else
                    error = request.Result.Body;

                results.Add(new
                {
                    Sistema = s.ToString(),
                    Plan = plan,
                    Status = request.Result.Status.ToString(),
                    Error = error
                });
            }

            return JsonConvert.SerializeObject(results);
        }

        #endregion
    }
}
