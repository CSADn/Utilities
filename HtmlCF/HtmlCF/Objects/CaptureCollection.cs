using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HtmlCF.Objects
{
    public class CaptureCollection : List<CapturedElement>
    {
        private List<string> _properties;

        public List<string> Properties { get { return _properties; } }

        public CaptureCollection()
        {
            _properties = new List<string>();
        }

        public new void Add(CapturedElement item)
        {
            base.Add(item);

            foreach (var f in item.FieldsNames)
            {
                if (!_properties.Contains(f))
                    _properties.Add(f);
            }
        }

        public List<string> ToList()
        {
            var list = new List<string>();

            foreach (var i in this)
            {
                //var entry = new List<string>();

                //foreach (var p in _properties)
                //    if (i.FieldsNames.Contains(p))
                //        entry.Add(i[p]);
                //    else
                //        entry.Add(string.Empty);

                //list.Add(string.Join(";", entry.ToArray()));

                foreach (var f in i.FieldsNames)
                {
                    /*
                    var encoding = Encoding.GetEncoding("iso-8859-1");
                    var utfbytes = Encoding.UTF8.GetBytes(i[f]);
                    var isobytes = Encoding.Convert(Encoding.UTF8, encoding, utfbytes);
                    list.Add(encoding.GetString(isobytes));
                    */
                    list.Add(i[f]);
                }
            }

            return list;
        }
    }
}
