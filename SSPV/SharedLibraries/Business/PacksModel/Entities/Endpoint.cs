using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packs.Entities
{
    public class Endpoint
    {
        public string Url { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }

        public Endpoint(string url, string user, string password)
        {
            Url = url;
            User = user;
            Password = password;
        }
    }
}
