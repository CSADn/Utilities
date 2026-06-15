using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using TecnoRed.TecnoRedService;

namespace TecnoRed.Commands
{
    public abstract class Command
    {
        private const string _ticket = "ticket";
        private const string _name = "command";

        public string Ticket
        {
            get { return Parameters[_ticket]; }
            set { SetParameter(_ticket, value); }
        }

        protected string Name
        {
            get { return Parameters[_name]; }
            set { SetParameter(_name, value); }
        }

        protected Dictionary<string, string> Parameters { get; private set; }

        protected List<nodoWsAgenda> Items { get; private set; }


        public Command()
        {
            //
        }


        protected void SetParameter(string name, string value)
        {
            if (Parameters == null)
                Parameters = new Dictionary<string, string>();

            if (Parameters.ContainsKey(name))
                Parameters[name] = value;
            else
                Parameters.Add(name, value);
        }

        protected void AddItem(string name, Dictionary<string, string> attributes = null, List<nodoWsAgenda> items = null)
        {
            if (Items == null)
                Items = new List<nodoWsAgenda>();

            var xml = new XmlDocument();

            var item = new nodoWsAgenda
            {
                Any = xml.CreateElement(name),
                AnyAttr = ToXmlAttributes(attributes)
            };

            if (items != null)
                item.item = items.ToArray();

            Items.Add(item);
        }


        public nodoWsAgenda ToAgendaRequest()
        {
            var request = new nodoWsAgenda
            {
                AnyAttr = ToXmlAttributes(Parameters)
            };

            if (Items != null)
                request.item = Items.ToArray();

            return request;
        }

        public abstract void Validate();


        private XmlAttribute[] ToXmlAttributes(Dictionary<string, string> input)
        {
            return input
                .Select(p =>
                {
                    var attr = (new XmlDocument()).CreateAttribute(p.Key);
                    attr.Value = p.Value;

                    return attr;
                })
                .ToArray();
        }
    }
}
