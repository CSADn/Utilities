using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace HtmlCF.Extensions
{
    public static class ExtensionMethods
    {
        public static void AddRange<T>(this IBindingList source, List<T> items)
        {
            foreach (var i in items)
                source.Add(i);
        }

        public static string GetDescription(this Enum source)
        {
            var type = source.GetType();
            var fields = type.GetFields();

            foreach (var f in fields)
	        {
                if (f.Name == source.ToString())
                {
                    var a = (DescriptionAttribute)Attribute.GetCustomAttribute(f, typeof(DescriptionAttribute));
                    return a.Description;
                }
	        }

            return string.Empty;
        }
    }
}
