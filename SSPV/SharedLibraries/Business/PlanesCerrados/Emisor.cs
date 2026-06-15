using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PlanesMultilinea.Entities;
using PlanesMultilinea.Enums;

namespace PlanesMultilinea
{
    public delegate Task<string> EmisorHandler(DatosEmision datos);

    public class Emisor
    {
        private Dictionary<Sistema, EmisorHandler> _emisor;
        private EndpointManager _endpoint;

        public Emisor()
        {
            _emisor = new Dictionary<Sistema, EmisorHandler>
            {
                { Sistema.AP, EmitirPlanAP },
                { Sistema.PlusMapp, EmitirPlanPlusMapp },
                { Sistema.Hogar, EmitirPlanHogar }
            };

            _endpoint = new EndpointManager();
        }


        public async Task<CertificadoPoliza> Emitir(Sistema sistema, DatosEmision datos)
        {
            if (!_emisor.ContainsKey(sistema))
                throw new NotImplementedException();

            var certificadoPolizaJson = await _emisor[sistema](datos);

            return JsonConvert.DeserializeObject<CertificadoPoliza>(certificadoPolizaJson);
        }


        private async Task<string> EmitirPlanAP(DatosEmision datos)
        {
            var emision = new
            {
                CodProductor = datos.CodProductor,
                IdCotizacion = datos.IdCotizacion,
                IdPlan = datos.IdPlan,
                NumeroInterno = datos.NumeroInterno,
                Asegurado = datos.Asegurados.FirstOrDefault(),
                MedioPago = datos.MedioPago,
            };

            var emisionJson = JsonConvert.SerializeObject(emision);

            var emisionResponse = await SolicitarEmision(Sistema.AP, "PlanMultilinea/Emitir", emisionJson);

            if (emisionResponse.Status == ResponseStatus.Error)
                throw new Exception(emisionResponse.Body);

            return emisionResponse.Body;
        }

        private async Task<string> EmitirPlanPlusMapp(DatosEmision datos)
        {
            var emision = new
            {
                CodProductor = datos.CodProductor,
                IdPlan = datos.IdPlan,
                NumeroInterno = datos.NumeroInterno,

                Asegurado = datos.Asegurados.First(),

                MedioPago = datos.MedioPago
            };

            var emisionResponse = await SolicitarEmision(Sistema.PlusMapp, "PlanMultilinea/Emitir", JsonConvert.SerializeObject(emision));

            if (emisionResponse.Status == ResponseStatus.Error)
                throw new Exception(emisionResponse.Body);

            return emisionResponse.Body;
        }

        private async Task<string> EmitirPlanHogar(DatosEmision datos)
        {
            var emision = new
            {
                CodProductor = datos.CodProductor,
                IdCotizacion = datos.IdCotizacion,
                NumeroInterno = datos.NumeroInterno,

                Asegurado = datos.Asegurados.First(),

                MedioPago = datos.MedioPago
            };

            var emisionResponse = await SolicitarEmision(Sistema.Hogar, "PlanMultilinea/Emitir", JsonConvert.SerializeObject(emision));

            if (emisionResponse.Status == ResponseStatus.Error)
                throw new Exception(emisionResponse.Body);

            return emisionResponse.Body;
        }


        private async Task<RequestResponse> SolicitarEmision(Sistema sistema, string accion, string emisionJson)
            => await _endpoint.Post(sistema, accion, emisionJson);
    }
}
