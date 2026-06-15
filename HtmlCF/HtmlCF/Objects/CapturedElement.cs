using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace HtmlCF.Objects
{
    public class CapturedElement : IEnumerable<string>
    {
        private Dictionary<string, string> _fields;

        public string this[string name]
        {
            get
            {
                if (_fields.ContainsKey(name))
                    return _fields[name];
                else
                    return null;
            }

            set
            {
                if (_fields.ContainsKey(name))
                    _fields[name] = value;
                else
                    throw new ArgumentException("La clave no existe");
            }
        }

        public Element Element { get; private set; }

        public CapturedElement Fields { get { return this; } }

        public List<string> FieldsNames { get { return _fields.Select(s => s.Key).ToList(); } }

        public CapturedElement(Element element)
        {
            _fields = new Dictionary<string, string>();
            Element = element;
        }

        public void Add(string name, string value)
        {
            if (_fields.ContainsKey(name.ToLower()))
                throw new ArgumentException("La propiedad ya existe");
            else
                _fields.Add(name.ToLower(), value);
        }

        public void Remove(string name)
        {
            if (!_fields.ContainsKey(name.ToLower()))
                throw new ArgumentException("La propiedad no existe");
            else
                _fields.Remove(name.ToLower());
        }

        public bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }

        #region IEnumerable Members

        public IEnumerator<string> GetEnumerator()
        {
            return new CapturedElementEnumerator(_fields);
        }

        #endregion

        #region IEnumerable<string> Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new CapturedElementEnumerator(_fields);
        }

        #endregion
    }

    public class CapturedElementEnumerator : IEnumerator<string>
    {
        private Dictionary<string, string> _fields;
        private int pos;

        public CapturedElementEnumerator(Dictionary<string, string> properties)
        {
            _fields = properties;
            pos = -1;
        }

        #region IEnumerator Members

        public string Current
        {
            get
            {
                if (pos < 0 || pos > _fields.Count)
                    throw new InvalidOperationException();
                else
                    return _fields.ElementAt(pos).Value;
            }
        }

        public bool MoveNext()
        {
            if (pos < _fields.Count)
                pos++;

            return !(pos == _fields.Count);
        }

        public void Reset()
        {
            pos = -1;
        }

        #endregion

        #region IEnumerator<string> Members

        object IEnumerator.Current
        {
            get
            {
                if (pos < 0 || pos > _fields.Count)
                    throw new InvalidOperationException();
                else
                    return _fields.ElementAt(pos);
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            //
        }

        #endregion
    }
}
