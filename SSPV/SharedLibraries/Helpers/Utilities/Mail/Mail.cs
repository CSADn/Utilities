using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Mail
{
    public class Message
    {
        public string From { get; set; }

        public List<string> To { get; set; }

        public  string ReplayTo { get; set; }

        public string Subject { get; set; }

        public bool IsBodyHtml { get; set; }

        public string Body { get; set; }

        public bool Async { get; set; }

        public List<Attachment> Attachments { get; set; }

        public List<Resource> LinkedResources { get; set; }


        public Message()
        {
            //
        }
    }
}
