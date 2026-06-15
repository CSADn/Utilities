using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TecnoRed.TecnoRedService;

namespace TecnoRed
{
    public class TecnoRedException : Exception
    {
        public int Code { get; private set; }

        public string Message { get; private set; }

        public string Command { get; private set; }

        public string Response { get; private set; }


        public TecnoRedException(int code, string message, string command, string response)
        {
            Code = code;
            Message = message;
            Command = command;
            Response = response;
        }
    }
}
