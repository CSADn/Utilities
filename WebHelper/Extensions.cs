using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HtmlAgilityPack;

using ADn.WebHelper;

namespace ADn.WebHelper.Extensions
{
    public static class Extensions
    {
        public static List<HtmlNode> ElementsByClass(this List<HtmlNode> input, string cssClass)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(WebHelper.ElementsByClass(cssClass, node));

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }

        public static List<HtmlNode> ElementsByTag(this List<HtmlNode> input, string htmlTag)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(WebHelper.ElementsByTag(htmlTag, node));

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }

        public static List<HtmlNode> ElementsByTagFirstLevel(this List<HtmlNode> input, string htmlTag)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(
                        node
                            .ChildNodes
                            .Where(f =>
                                f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase)
                            )
                            .ToList()
                    );

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }

        public static List<HtmlNode> ElementsById(this List<HtmlNode> input, string tagId)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(
                        node
                            .Descendants()
                            .Where(f =>
                                f.Attributes["id"] != null &&
                                f.Attributes["id"].Value == tagId
                            )
                            .ToList()
                    );

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }


        public static List<HtmlNode> ElementsByTagAndClass(this List<HtmlNode> input, string htmlTag, string cssClass)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(
                        node
                            .Descendants()
                            .Where(f =>
                                f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase) &&
                                f.Attributes.Contains("class") &&
                                f.Attributes["class"].Value.Contains(cssClass)
                            )
                            .ToList()
                    );

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }

        public static List<HtmlNode> ElementsByTagAndProperty(this List<HtmlNode> input, string htmlTag, string tagProperty, string propertyValue)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(
                        node
                            .Descendants()
                            .Where(f =>
                                f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase) &&
                                WebHelper.GetPropertyValue(f, tagProperty) == propertyValue
                            )
                            .ToList()
                    );

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }

        public static List<HtmlNode> ElementsByTagAndAttribute(this List<HtmlNode> input, string htmlTag, string tagAttribute, string attributeValue)
        {
            var nodes = new List<HtmlNode>();

            if (input == null || input.Count == 0)
                return nodes;

            try
            {
                foreach (var node in input)
                    nodes.AddRange(
                        node
                            .Descendants()
                            .Where(f =>
                                f.Name.Equals(htmlTag, StringComparison.InvariantCultureIgnoreCase) &&
                                f.Attributes.Contains(tagAttribute) &&
                                f.Attributes[tagAttribute].Value.Contains(attributeValue)
                            )
                            .ToList()
                    );

                return nodes;
            }
            catch
            {
                nodes.Clear();
                return nodes;
            }
        }
    }
}
