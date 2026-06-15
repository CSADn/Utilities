using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsappSimulator
{
    public class Message
    {
        public string Endpoint { get; set; }

        public MessageType Type { get; set; }

        public string PhoneFrom { get; set; }

        public string PhoneTo { get; set; }

        public string Text { get; set; }
    }
}
