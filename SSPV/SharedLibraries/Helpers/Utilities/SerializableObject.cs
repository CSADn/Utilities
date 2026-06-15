using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public abstract class SerializableObject
    {
        public SerializableObject()
        {

        }

        public SerializableObject(string serializedInput)
        {
            Deserialize(serializedInput);
        }


        public override string ToString()
        {
            return Serialize();
        }

        private string Serialize()
        {
            var type = this.GetType();
            var properties = type.GetProperties();

            var output = new List<string>();

            foreach (var pi in properties)
            {
                if (pi.PropertyType != typeof(string))
                    throw new NotSupportedException("Todas las propiedades deben ser de tipo String");

                var key = pi.Name.ToLower();
                var value = (string)pi.GetValue(this);

                value = (string.IsNullOrWhiteSpace(value)
                    ? string.Empty
                    : Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
                );

                output.Add($"{key}:{value}");
            }

            return string.Join(";", output);
        }

        private void Deserialize(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException();

            var type = this.GetType();

            var properties = type.GetProperties();
            var values = Parse(input);

            foreach (var pi in properties)
            {
                if (pi.PropertyType != typeof(string))
                    throw new NotSupportedException("Todas las propiedades deben ser de tipo String");

                var key = pi.Name.ToLower();

                if (values.ContainsKey(key))
                    pi.SetValue(this, values[key]);
            }
        }

        private Dictionary<string, string> Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentNullException();

            var output = new Dictionary<string, string>();

            var pairs = input.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in pairs)
            {
                var keyValue = p.Split(':');
                var key = keyValue.First().ToLower();
                var value = Encoding.UTF8.GetString(Convert.FromBase64String(keyValue.Last()));
                output.Add(key, value);
            }

            return output;
        }
    }
}
