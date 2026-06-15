using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Helpers;
using Helpers.Requests;

using PlanesMultilinea.Enums;
using PlanesMultilinea.Entities;

namespace PlanesMultilinea
{
    public delegate Task<File> ImpresionHandler(DatosImpresion datos);

    public class Impresion
    {
        private EndpointManager _endpoint;
        private Dictionary<Sistema, ImpresionHandler> _impresion;
        private Requests _rest;

        public Impresion()
        {
            _endpoint = new EndpointManager();

            _impresion = new Dictionary<Sistema, ImpresionHandler>
            {
                { Sistema.AP, ImprimirPlanAP },
                { Sistema.PlusMapp, ImprimirPlanPlusMapp },
                { Sistema.Hogar, ImprimirPlanHogar }
            };

            _rest = new Requests();
        }


        public async Task<File> Imprimir(Sistema sistema, DatosImpresion datos)
        {
            if (!_impresion.ContainsKey(sistema))
                throw new NotImplementedException();

            return await _impresion[sistema](datos);
        }


        private async Task<File> ImprimirPlanAP(DatosImpresion datos)
        {
            string accion = "Impresion/GenerarCertificadoPDF/";
            if (datos.EsPoliza)
                accion = "Impresion/GenerarPolizaPDF/";

            var binaryResponse = await _endpoint.GetBinary(Sistema.AP, accion, datos.CodProductor, datos.NroDocumento);

            if (binaryResponse.Status == ResponseStatus.Error)
                throw new Exception(binaryResponse.Error);

            return new File()
            {
                Content = binaryResponse.Body,
                Length = (int)binaryResponse.Body?.Length,
                Filename = $"{datos.CodProductor}.{datos.NroDocumento}.pdf"
            };
        }

        private async Task<File> ImprimirPlanPlusMapp(DatosImpresion datos)
        {
            string accion = "Impresion/GenerarConstanciaPDF/";
            if (datos.EsPoliza)
                accion = "Impresion/GenerarPolizaPDF/";

            var binaryResponse = await _endpoint.GetBinary(Sistema.PlusMapp, accion, 0, datos.CodProductor, datos.NroDocumento);

            if (binaryResponse.Status == ResponseStatus.Error)
                throw new Exception(binaryResponse.Error);

            return new File()
            {
                Content = binaryResponse.Body,
                Length = (int)binaryResponse.Body?.Length,
                Filename = $"{datos.CodProductor}.{datos.NroDocumento}.pdf"
            };
        }

        private async Task<File> ImprimirPlanHogar(DatosImpresion datos)
        {
            string accion = "Impresion/GenerarCertificadoPDF/";
            if (datos.EsPoliza)
                accion = "Impresion/GenerarPolizaPDF/";

            var binaryResponse = await _endpoint.GetBinary(Sistema.Hogar, accion, datos.CodProductor, datos.NroDocumento);

            if (binaryResponse.Status == ResponseStatus.Error)
                throw new Exception(binaryResponse.Error);

            return new File()
            {
                Content = binaryResponse.Body,
                Length = (int)binaryResponse.Body?.Length,
                Filename = $"{datos.CodProductor}.{datos.NroDocumento}.pdf"
            };
        }

    }
}
