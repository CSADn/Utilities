using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TecnoRed.Client;
using TecnoRed.Commands;
using TecnoRed.TecnoRedService;

using Helpers;

namespace TecnoRed
{
    public static class TecnoRedHelper
    {
        private static string _ticket;
        private static TecnoRedClient _client;

        public static TecnoRedClient Client
        {
            get { return _client; }
        }


        static TecnoRedHelper()
        {
            _ticket = "TecnoRedTicket".FromAppSettings<string>(notFoundException: true);
            _client = new TecnoRedClient(_ticket);
        }
    }
}
