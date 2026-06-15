using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using PlanesMultilinea.Entities;
using PlanesMultilinea.Enums;

namespace PlanesMultilinea
{
    public delegate Task<string> CotizadorHandler(DatosCotizacion datos, string planJson);

    public class Cotizador
    {
        private Dictionary<Sistema, CotizadorHandler> _cotizador;
        private EndpointManager _endpoint;
        private Consulta _consulta;

        public Cotizador()
        {
            _cotizador = new Dictionary<Sistema, CotizadorHandler>
            {
                { Sistema.AP, CotizarPlanAP },
                { Sistema.PlusMapp, CotizarPlanPlusMapp },
                { Sistema.Hogar, CotizarPlanHogar }
            };

            _endpoint = new EndpointManager();
            _consulta = new Consulta();
        }


        public async Task<Presupuesto> Cotizar(Sistema sistema, int idPlanMultilinea, DatosCotizacion datos)
        {
            if (!_cotizador.ContainsKey(sistema))
                throw new NotImplementedException();

            var planResponse = await _consulta.ObtenerPlan(sistema, idPlanMultilinea);

            if (planResponse.Status == ResponseStatus.Error)
                throw new Exception(planResponse.Body);

            var presupuestoJson = await _cotizador[sistema](datos, planResponse.Body);

            return JsonConvert.DeserializeObject<Presupuesto>(presupuestoJson);
        }


        private async Task<string> CotizarPlanAP(DatosCotizacion datos, string planJson)
        {
            var plan = (dynamic)JsonConvert.DeserializeObject(planJson);

            var cotizacion = new
            {
                IdPlan = plan.IdPlan,

                CodProductor = datos.CodProductor,
                CodSubproductor = datos.CodSubproductor,
                CodOrganizador = datos.CodOrganizador,

                ApellidoRazonSocialAsegurado = datos.Asegurado.ApellidoRazonSocial,
                NombreAsegurado = datos.Asegurado.Nombre,
                TipoDocumento = datos.Asegurado.TipoDocumento,
                NroDocumento = datos.Asegurado.NroDocumento,
                CondicionIVA = datos.Asegurado.CondicionIVA,
                CondicionIB = datos.Asegurado.CondicionIB,
                IdTipoPersona = datos.Asegurado.IdTipoPersona,
                IdProvincia = datos.Asegurado.IdProvincia,
                Telefono = datos.Asegurado.Telefono,
                Celular = datos.Asegurado.Celular,
                EMail = datos.Asegurado.Email,

                IdMedioPago = datos.IdMedioPago
            };

            var cotizacionResponse = await SolicitarCotizacion(Sistema.AP, "PlanMultilinea/Cotizar", JsonConvert.SerializeObject(cotizacion));

            if (cotizacionResponse.Status == ResponseStatus.Error)
                throw new Exception(cotizacionResponse.Body);

            return cotizacionResponse.Body;
        }

        private async Task<string> CotizarPlanPlusMapp(DatosCotizacion datos, string planJson)
        {
            //var plan = (dynamic)JsonConvert.DeserializeObject(planJson);

            //var cotizacion = new
            //{
            //    IdPlan = plan.IdPlan,

            //    CodProductor = datos.CodProductor,
            //    CodSubproductor = datos.CodSubproductor,
            //    CodOrganizador = datos.CodOrganizador,

            //    ApellidoRazonSocialAsegurado = datos.Asegurado.ApellidoRazonSocial,
            //    NombreAsegurado = datos.Asegurado.Nombre,
            //    TipoDocumento = datos.Asegurado.TipoDocumento,
            //    NroDocumento = datos.Asegurado.NroDocumento,
            //    CondicionIVA = datos.Asegurado.CondicionIVA,
            //    CondicionIB = datos.Asegurado.CondicionIB,
            //    IdTipoPersona = datos.Asegurado.IdTipoPersona,
            //    IdProvincia = datos.Asegurado.IdProvincia,
            //    IdLocalidad = datos.Asegurado.IdLocalidad,
            //    CodPostal = datos.Asegurado.CodPostal,
            //    Telefono = datos.Asegurado.Telefono,
            //    Celular = datos.Asegurado.Celular,
            //    EMail = datos.Asegurado.Email,

            //    IdMedioPago = datos.IdMedioPago
            //};

            //var cotizacionResponse = await SolicitarCotizacion(Sistema.PlusMapp, "PlanMultilinea/Cotizar", JsonConvert.SerializeObject(cotizacion));

            //if (cotizacionResponse.Status == ResponseStatus.Error)
            //    throw new Exception(cotizacionResponse.Body);

            //return cotizacionResponse.Body;
            return JsonConvert.SerializeObject(
                new RequestResponse(
                    JsonConvert.SerializeObject(
                        new Entities.Presupuesto
                        {
                            IdCotizacion = 0,
                            SumaAsegurada = 0,
                            PrimaTotal = 0,
                            PremioTotal = 0,
                            Cuotas = 0,
                            ValorCuota = 0,
                            ComisionProdPorc = 0.0,
                            ComisionProd = 0.0,
                            ComisionOrgPorc = 0.0,
                            ComisionOrg = 0.0
                        }
                    ),
                    ResponseStatus.OK
                )
            );
        }

        private async Task<string> CotizarPlanHogar(DatosCotizacion datos, string planJson)
        {
            var plan = (dynamic)JsonConvert.DeserializeObject(planJson);

            var cotizacion = new
            {
                IdPlan = plan.IdPlan,

                CodProductor = datos.CodProductor,
                CodSubproductor = datos.CodSubproductor,
                CodOrganizador = datos.CodOrganizador,

                ApellidoRazonSocialAsegurado = datos.Asegurado.ApellidoRazonSocial,
                NombreAsegurado = datos.Asegurado.Nombre,
                TipoDocumento = datos.Asegurado.TipoDocumento,
                NroDocumento = datos.Asegurado.NroDocumento,
                CondicionIVA = datos.Asegurado.CondicionIVA,
                CondicionIB = datos.Asegurado.CondicionIB,
                IdTipoPersona = datos.Asegurado.IdTipoPersona,
                IdProvincia = datos.Asegurado.IdProvincia,
                IdLocalidad = datos.Asegurado.IdLocalidad,
                CodPostal = datos.Asegurado.CodPostal,
                Telefono = datos.Asegurado.Telefono,
                Celular = datos.Asegurado.Celular,
                EMail = datos.Asegurado.Email,

                IdMedioPago = datos.IdMedioPago
            };

            var cotizacionResponse = await SolicitarCotizacion(Sistema.Hogar, "PlanMultilinea/Cotizar", JsonConvert.SerializeObject(cotizacion));

            if (cotizacionResponse.Status == ResponseStatus.Error)
                throw new Exception(cotizacionResponse.Body);

            return cotizacionResponse.Body;
        }


        private async Task<RequestResponse> SolicitarCotizacion(Sistema sistema, string accion, string cotizacionJson)
            => await _endpoint.Post(sistema, accion, cotizacionJson);
    }
}
