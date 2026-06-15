using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Responses
{
    public abstract class Response
    {
        protected nodoWsAgenda _node;


        public int Code { get; private set; }

        public ResponseStatus Status { get; private set; }

        public int? InnerCode { get; private set; }

        public string InnerMessage { get; private set; }


        public Response(nodoWsAgenda invocarReturn)
        {
            _node = invocarReturn;

            Code = invocarReturn.Get<int>("retCode");
            Status = invocarReturn.Get<string>("message").ToStatus();
            var items = (invocarReturn.item ?? new nodoWsAgenda[0]).ToList();

            if (Code == -1)
            {
                var attributes = invocarReturn.Any.Attributes;
                InnerCode = attributes.Cast<XmlAttribute>().ToArray().Get<int>("retCode");
                InnerMessage = attributes.Cast<XmlAttribute>().ToArray().Get<string>("message");
            }
            else
            {
                if (items != null && invocarReturn.Any != null)
                {
                    items.Insert(0, new nodoWsAgenda
                    {
                        AnyAttr = invocarReturn.Any.Attributes.Cast<XmlAttribute>().ToArray()
                    });

                    if (items.Count > 0)
                        Parse(items);
                }
            }
        }

        public abstract void Parse(List<nodoWsAgenda> items);
    }
}
