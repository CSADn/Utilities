using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using HtmlCF.Extensions;
using HtmlCF.Functions;

namespace HtmlCF.Objects
{
    public class Element : INotifyPropertyChanged
    {
        private string _tag;
        private ElementType _type;
        private List<Element> _childs;

        [XmlAttribute]
        public int Id { get; set; }

        [XmlAttribute]
        public string Tag { get { return _tag; } set { _tag = value; RiseChangedEvent("Tag"); } }

        [XmlAttribute]
        public ElementType Type { get { return _type; } set { _type = value; RiseChangedEvent("Type"); } }

        [XmlAttribute]
        public bool HasSelector { get { return (Selector == null || Selector.Count == 0 ? false : true); } }

        [XmlAttribute]
        public bool HasCapture { get { return (Capture == null || Capture.Count == 0 ? false : true); } }

        [XmlAttribute]
        public bool HasChilds { get { return (Childs == null || Childs.Count == 0 ? false : true); } }

        [XmlAttribute]
        public bool HasBreak { get { return (Type == ElementType.CaptureBreak); } }


        public List<Selector> Selector { get; set; }

        public List<Capture> Capture { get; set; }

        public List<Element> Childs { get { return _childs; } set { _childs = value; RiseChangedEvent("Childs"); } }


        public Element(): this(-1, string.Empty) { /**/ }

        public Element(int id, string tag)
        {
            Id = id;
            Tag = tag;
            Type = ElementType.Normal;

            Selector = null;
            Capture = null;
            _childs = null;
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RiseChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }

    public class Selector : INotifyPropertyChanged
    {
        private SelectorType _type;
        private string _name;
        private string _value;

        [XmlAttribute]
        public SelectorType Type { get { return _type; } set { _type = value; RiseChangedEvent("Type"); } }

        [XmlAttribute]
        public string Name { get { return _name; } set { _name = value; RiseChangedEvent("Name"); } }

        [XmlAttribute]
        public string Value { get { return _value; } set { _value = value; RiseChangedEvent("Value"); } }

        
        public Selector(): this(SelectorType.Attribute, string.Empty, string.Empty) { /**/ }

        public Selector(SelectorType type, string name, string value)
        {
            _type = type;
            _name = name;
            _value = value;
        }

        public override string ToString()
        {
            var result = string.Empty;

            if (Type == SelectorType.Function)
            {
                if (string.IsNullOrEmpty(Value))
                    return "(" + Type.GetDescription() + ") = " + Name + "()";
                else
                    return "(" + Type.GetDescription() + ") = " + Name + "(" + Value + ")";
            }
            else
            {
                if (string.IsNullOrEmpty(Name))
                    return "(" + Type.GetDescription() + ") -> \"" + Value + "\"";
                else if (string.IsNullOrEmpty(Value))
                    return "(" + Type.GetDescription() + ") -> \"" + Name + "\"";
                else
                    return "(" + Type.GetDescription() + ") \"" + Name + "\" -> \"" + Value + "\"";
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RiseChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }

    public class Capture : INotifyPropertyChanged
    {
        private string _description;
        private CaptureType _type;
        private string _name;
        private string _value;

        [XmlAttribute]
        public string Description { get { return _description; } set { _description = value; RiseChangedEvent("Description"); } }

        [XmlAttribute]
        public CaptureType Type { get { return _type; } set { _type = value; RiseChangedEvent("Type"); } }

        [XmlAttribute]
        public string Name { get { return _name; } set { _name = value; RiseChangedEvent("Name"); } }

        [XmlAttribute]
        public string Value { get { return _value; } set { _value = value; RiseChangedEvent("Value"); } }


        public Capture() : this(string.Empty, CaptureType.Attribute, string.Empty, string.Empty) { /**/ }

        public Capture(string description, CaptureType type, string name, string value)
        {
            Description = description;
            Type = type;
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            var result = string.Empty;

            //if (string.IsNullOrEmpty(Name))
            //    return "(" + Type.GetDescription() + ") = \"" + Value + "\"";
            //else if (string.IsNullOrEmpty(Value))
            //    return "(" + Type.GetDescription() + ") = \"" + Name + "\"";
            //else
            //    return "(" + Type.GetDescription() + ") \"" + Name + "\" -> \"" + Value + "\"";
            if (Type == CaptureType.Function)
            {
                if (string.IsNullOrEmpty(Value))
                    return "(" + Type.GetDescription() + ") = " + Name + "()";
                else
                    return "(" + Type.GetDescription() + ") = " + Name + "(" + Value + ")";
            }
            else if (Type == CaptureType.Condition)
            {
                if (string.IsNullOrEmpty(Value))
                    return "(" + Type.GetDescription() + ") -> [" + Name + "]";
                else
                    return "(" + Type.GetDescription() + ") -> [" + Name + " = \"" + Value + "\"]";
            }
            else
            {
                if (string.IsNullOrEmpty(Name))
                    return "(" + Type.GetDescription() + ") -> \"" + Value + "\"";
                else if (string.IsNullOrEmpty(Value))
                    return "(" + Type.GetDescription() + ") -> \"" + Name + "\"";
                else
                    return "(" + Type.GetDescription() + ") \"" + Name + "\" -> \"" + Value + "\"";
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RiseChangedEvent(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
