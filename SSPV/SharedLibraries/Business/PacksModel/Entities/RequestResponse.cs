using Helpers;
using Packs.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packs.Entities
{
    public delegate void CotizarHandler();
    public class RequestResponse
    {
        public string Body { get; private set; }
        public ResponseStatus Status { get; private set; }

        public RequestResponse(string body, ResponseStatus status)
        {
            Body = body;
            Status = status;
        }
    }
}
