using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;

namespace iTextSharpPDFToolGUI.ajax
{
    public class handler : IHttpHandler
    {
        protected delegate string MethodHandler(HttpContext context);

        protected static Dictionary<string, MethodHandler> _methods;


        public handler()
        {
            _methods = new Dictionary<string, MethodHandler>
            {

            };

        }


        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            var method = "method".FromQueryString(string.Empty);
            var response = string.Empty;

            if (!method.IsNull())
                method = method.ToLower();

            if (!_methods.ContainsKey(method))
            {
                context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;

                response = Utilities.ToJsonError(
                    (int)HttpStatusCode.MethodNotAllowed,
                    "Operación no permitida."
                );
            }
            else
            {
                try
                {
                    response = _methods[method].Invoke(context);
                }
                catch (ThreadAbortException)
                {
                    //
                }
                catch (HttpException ex)
                {
                    if ((uint)ex.ErrorCode != 0x80070040) // Remote server close connection
                    {
                        response = Utilities.HandleRequestError(ex);
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    }
                }
                catch (Exception ex)
                {
                    response = Utilities.HandleRequestError(ex);
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }

            context.Response.Write(response);
            context.Response.End();
        }



        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}