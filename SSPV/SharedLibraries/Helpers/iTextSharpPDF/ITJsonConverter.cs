using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Globalization;

namespace iTextSharpPDF
{
    public delegate object ITJsonPropertyHandler(JToken token);

    public class ITJsonConverter<T> : CustomCreationConverter<T>
    {
        private Dictionary<string, ITJsonPropertyHandler> _propertyConverters;


        public ITJsonConverter()
        {
            _propertyConverters = new Dictionary<string, ITJsonPropertyHandler>
            {
                { "PageSize", ITJsonConverters.PageSize },
                { "GlobalFont", ITJsonConverters.Font },
                { "Text", ITJsonConverters.Text },
                { "Font", ITJsonConverters.Font },
                { "Elements", ITJsonConverters.Elements },
                { "Margins", ITJsonConverters.Margins },
                { "NewPageMargins", ITJsonConverters.Margins },
                { "NewPageElements", ITJsonConverters.Elements },
                { "Bytes", ITJsonConverters.Bytes }
            };
        }


        public override T Create(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ITJsonConverters.Read(reader, objectType, _propertyConverters);
        }

    }

    public static class ITJsonConverters
    {
        public static object Read(JsonReader reader, Type objectType, Dictionary<string, ITJsonPropertyHandler> converters)
        {
            var json = JObject.Load(reader);

            var output = Activator.CreateInstance(objectType);

            foreach (var jp in json)
            {
                var pi = objectType.GetProperty(jp.Key);

                if (pi == null)
                    continue;

                var value = default(object);

                if (converters.ContainsKey(jp.Key))
                    value = converters[jp.Key].Invoke(jp.Value);
                else
                    value = jp.Value.ToObject(pi.PropertyType);

                pi.SetValue(output, value);
            }

            return output;
        }

        public static object Text(JToken token)
        {
            var text = string.Empty;

            if (token.Type == JTokenType.String)
                text = token.ToString();
            else if (token.Type == JTokenType.Array)
                text = string.Join("\r", token.ToList());

            return text;
        }

        public static object Font(JToken token)
        {
            if (!token.HasValues)
                return null;

            var name = token["Name"];
            var size = token["Size"];

            if (name == null || size == null)
                return null;

            var pi = typeof(ITFont).GetProperty(name.ToString());

            if (pi == null)
                return null;

            var font = (ITFont)pi.GetValue(null);

            if (font == null)
                return null;

            font.Size = size.Value<float>();

            return font;
        }

        public static object PageSize(JToken token)
        {
            if (string.IsNullOrWhiteSpace(token.ToString()))
                return null;

            if (token.Type == JTokenType.String)
            {
                var fi = typeof(PageSizeEx).GetField(token.ToString());

                if (fi == null)
                    return null;
                else
                {
                    var itPageSize = (iTextSharp.text.Rectangle)fi.GetValue(null);

                    if (itPageSize == null)
                        return null;

                    return new PageSizeEx(itPageSize);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                var width = token["Width"];
                var height = token["Height"];

                if (width == null || height == null)
                    return null;

                return new PageSizeEx(
                    float.Parse(width.ToString().Replace(",", "."), CultureInfo.InvariantCulture),
                    float.Parse(height.ToString().Replace(",", "."), CultureInfo.InvariantCulture)
                );
            }
            else
                return null;
        }

        public static object Elements(JToken token)
        {
            if (token.Type != JTokenType.Array)
                return null;

            return token
                .Select(s =>
                    JsonConvert.DeserializeObject<JsonElement>(s.ToString(), new ITJsonConverter<JsonElement>())
                )
                .ToList();
        }

        public static object Margins(JToken token)
        {
            if (!token.HasValues)
                return null;

            var left = token["Left"];
            var right = token["Right"];
            var top = token["Top"];
            var bottom = token["Bottom"];

            if (left == null || right == null || top == null || bottom == null)
                return null;

            return new Margins
            (
                float.Parse(left.ToString().Replace(",", "."), CultureInfo.InvariantCulture),
                float.Parse(right.ToString().Replace(",", "."), CultureInfo.InvariantCulture),
                float.Parse(top.ToString().Replace(",", "."), CultureInfo.InvariantCulture),
                float.Parse(bottom.ToString().Replace(",", "."), CultureInfo.InvariantCulture)
            );
        }

        public static object Bytes(JToken token)
        {
            var bytes = default(byte[]);

            if (token.Type != JTokenType.String)
                return bytes;

            try
            {
                bytes = Convert.FromBase64String(token.ToString());
            }
            catch { }

            return bytes;
        }
    }
}
