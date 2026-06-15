using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

using Newtonsoft.Json;

using Helpers;
using TecnoRed.Commands;
using TecnoRed.TecnoRedService;
using System.ServiceModel;

namespace TecnoRed.Client
{
    public class TecnoRedClient
    {
        private AgendaServiceClient _ws;
        private string _ticket;
        private NLog.Logger _log;


        public TecnoRedClient(string ticket)
        {
            _ticket = ticket;
            _ws = new AgendaServiceClient();
            _log = NLog.LogManager.GetCurrentClassLogger();

            ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => { return true; };
        }


        public List<Responses.ListaCentrosItem> ListaDeCentros()
        {
            var listaCentros = Invocar<Responses.ListaCentrosResponse>(new ListaCentros());

            return listaCentros.Centros;
        }

        public List<Responses.LocalidadesItem> Localidades(int codPostal)
        {
            var listaLocalidades = Invocar<Responses.LocalidadesResponse>(new Localidades { CodPostal = codPostal.ToString() });

            return listaLocalidades.Localidades;
        }

        public List<Responses.LocalidadesItem> Localidades(string codPostal)
        {
            var listaLocalidades = Invocar<Responses.LocalidadesResponse>(new Localidades { CodPostal = codPostal });

            return listaLocalidades.Localidades;
        }

        public List<Responses.TurnosItem> Turnos(int codLocalidad)
        {
            var listaTurnos = Invocar<Responses.TurnosResponse>(new Turnos { CodLocalidad = codLocalidad.ToString() });

            return listaTurnos.Turnos;
        }

        public int ReservarTurno(ReservarTurno turno)
        {
            if (turno == null)
                throw new ArgumentNullException("turno");

            var reserva = Invocar<Responses.ReservarTurnoResponse>(turno);

            return reserva.NumeroReserva;
        }

        public bool CancelarReserva(int numeroDeReserva)
        {
            var cancelar = Invocar<Responses.CancelarReservaResponse>(new CancelarReserva { NumeroDeReserva = numeroDeReserva.ToString() });

            return (cancelar.Code == 0);
        }

        public int ConfirmarReserva(ConfirmarReserva reserva)
        {
            if (reserva == null)
                throw new ArgumentNullException("reserva");

            var confirmar = Invocar<Responses.ConfirmarReservaResponse>(reserva);

            return confirmar.NumeroTRD;
        }

        public bool DejarSinEfecto(long numeroTRD)
        {
            var dejarSinEfecto = Invocar<Responses.DejarSinEfectoResponse>(new DejarSinEfecto { NumeroTRD = numeroTRD.ToString() });

            return (dejarSinEfecto.Code == 0);
        }

        public int InspeccionEnCentro(InspeccionEnCentro inspeccion)
        {
            if (inspeccion == null)
                throw new ArgumentNullException("inspeccion");

            var agendar = Invocar<Responses.InspeccionEnCentroResponse>(inspeccion);

            return agendar.NumeroTRD;
        }

        public int InspeccionEnDomicilio(InspeccionEnDomicilio inspeccion)
        {
            if (inspeccion == null)
                throw new ArgumentNullException("inspeccion");

            var agendar = Invocar<Responses.InspeccionEnDomicilioResponse>(inspeccion);

            return agendar.NumeroTRD;
        }


        private T Invocar<T>(Command command) where T : Responses.Response
        {
            try
            {
                command.Validate();
                command.Ticket = _ticket;

                var trcmd = new invocar
                {
                    request = command.ToAgendaRequest()
                };

                var response = default(invocarResponse);


                Utilities.RetryWhenFail(intento =>
                {
                    try
                    {
                        response = _ws.invocar(trcmd);

                        if (response == null)
                            throw new Exception("response == null");

                        return true;
                    }
                    catch (CommunicationException ex)
                    {
                        _log.Info("Error de comunicacion: " + ex.ToString());
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Error invocando al WebService de Tecnored: " + ex.Message);
                        return false;
                    }
                }, new Exception("Error en la comunicación con Tecnored. Intente nuevamente en unos minutos."));

                var parsedResponse = ParseResponse<T>(response);

                if (parsedResponse.Code == -1)
                {
                    trcmd
                        .request
                        .AnyAttr
                        .First(f => f.Name.Equals("ticket", StringComparison.InvariantCultureIgnoreCase))
                        .Value = "****";

                    throw new TecnoRedException(
                        parsedResponse.InnerCode.Value,
                        parsedResponse.InnerMessage,
                        JsonConvert.SerializeObject(trcmd, Newtonsoft.Json.Formatting.Indented),
                        JsonConvert.SerializeObject(response.@return, Newtonsoft.Json.Formatting.Indented)
                    );
                }
                else
                    return parsedResponse;
            }
            catch (TecnoRedException ex)
            {
                _log.Log(NLog.LogLevel.Error, string.Concat(
                        "ParseResponse Error: (", ex.Code, ") ", ex.Message, Environment.NewLine,
                        ex.Command, Environment.NewLine,
                        ex.Response, Environment.NewLine
                ));
                throw;
            }
            catch (Exception ex)
            {
                _log.Log(NLog.LogLevel.Fatal, ex.ToString());
                throw;
            }
        }

        private T ParseResponse<T>(invocarResponse invocarResponse) where T : Responses.Response
        {
            if (invocarResponse == null)
            {
                _log.Info("invocarResponse es nulo");
                throw new Exception("invocarResponse es nulo");
            }

            if (invocarResponse.@return == null)
                throw new Exception("@return es nulo");

            var ret = invocarResponse.@return;

            return (T)Activator.CreateInstance(typeof(T), ret);
        }
    }
}
