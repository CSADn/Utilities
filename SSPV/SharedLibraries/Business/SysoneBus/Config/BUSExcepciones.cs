using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ClassLibrarySDK.Packages.sdk.serializer;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace sysoneBus
{
    public class BUSExcepciones : Exception, Logger
    {
        private NLog.Logger logguer = NLog.LogManager.GetCurrentClassLogger();
        public string Message { get; set; }

        public BUSExcepciones()
        {
        }
        public BUSExcepciones(Exception e)
        {
            if (e is BUSExcepciones)
            {
                this.Message = e.Message;
                return;
            }
            else if (e is WebException)
            {
                if (((WebException)e).Status == WebExceptionStatus.Timeout)
                {
                    this.Message = "Se produjo un erro de comunicacion espere unos minutos para reintentar";

                }
                else if (((WebException)e).Status == WebExceptionStatus.ProtocolError)
                {
                    switch (((HttpWebResponse)((WebException)e).Response).StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            this.Message = "Acceso no autorizado a BUS";
                            break;
                        case HttpStatusCode.InternalServerError:
                            this.Message = "Se produjo un error interno en BUS, consulte al administrador";
                            logguer.Error(e);
                            break;
                        case HttpStatusCode.BadRequest:
                            this.Message = "Error al intentar consultar los datos de BUS";
                            break;
                    }
                }
            }
            else
            {
                this.Message = "Se produjo un error interno en BUS, consulte al administrador";
                logguer.Error(e);

            }
        }

        public BUSExcepciones(string message) : base(message)
        {
        }
        public BUSExcepciones(string message, Exception innerException) : base(message, innerException)
        {
        }
        protected BUSExcepciones(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public void log(string message)
        {
        }

        public void log(Exception e)
        {
            throw e;
        }

        public void log(string message, Exception e)
        {
            throw e;
        }
    }
}
