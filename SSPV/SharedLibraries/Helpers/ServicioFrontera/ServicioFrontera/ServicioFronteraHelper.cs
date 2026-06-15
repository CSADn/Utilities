using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace ServicioFrontera
{
    public static class ServicioFronteraHelper
    {
        const string SEND_OK = "OK";
        const string SEND_ERR = "ERR";

        public static Response SendSMS(string nrocelular, string mensaje)
        {
            if (string.IsNullOrEmpty(nrocelular) || string.IsNullOrEmpty(mensaje))
                return null;

            try
            {
                var servicio = new ServicioFrontera.servicioFrontera();

                var usuario = "ServFronteraUsuario".FromAppSettings(string.Empty, true);
                var password = "ServFronteraPassword".FromAppSettings(string.Empty, true);

                var xml =
                    @"<order>
                      <auth>
                          <usuario>[@Usuario]</usuario>
                          <password>[@Password]</password>
                      </auth>
                      <service>
                          <provision>SondeosSMS</provision>
                          <operacion>SubmitLongNumberSMS</operacion>
                      </service>
                      <options/>
                      <parameters>
                          <message>
                              <destAddress>[@NroCelular]</destAddress>
                              <shortMessage>[@Mensaje]</shortMessage>
                          </message>
                      </parameters>
                  </order>";

                xml = xml
                    .Replace("[@Usuario]", usuario)
                    .Replace("[@Password]", password)
                    .Replace("[@NroCelular]", nrocelular)
                    .Replace("[@Mensaje]", mensaje);
                
                #region Response

                XmlNode[] result = (XmlNode[])servicio.ejecutar(xml);

                var nodeOrder = result.Where(n => n.Name == "order").First();

                var codigo = nodeOrder?.Buscar("response")?.Buscar("codigo")?.Value ?? string.Empty;
                var idMensaje = nodeOrder?.Buscar("response")?.Buscar("message").Buscar("id_mensaje")?.Value ?? string.Empty;
                
                return new Response()
                {
                    Result = (codigo.Equals("000") ? SEND_OK : SEND_ERR),
                    IdMensaje = (codigo.Equals("000") ? idMensaje : string.Empty)
                };
            }
            catch
            {
                return new Response()
                {
                    Result = SEND_ERR,
                    IdMensaje = string.Empty
                };
            }

            #endregion
        }
    }
}
