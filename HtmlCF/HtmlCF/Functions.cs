using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;

using HtmlAgilityPack;
using HtmlCF.Extensions;
using HtmlCF.Utilities;
using ADn.WebHelper;
using HtmlCF.Objects;

namespace HtmlCF.Functions
{
    internal class Parameters : Attribute
    {
        public string[] Params { get; private set; }

        public Parameters(params string[] parameters)
        {
            Params = parameters;
        }

        public override string ToString()
        {
            return string.Join(", ", Params);
        }
    }
    
    public enum SelectorFunctions
    {
        /// <summary>
        /// En una colección de items devuelve el item correspondiente al índice especificado.
        /// </summary>
        [Description("Eq(index)")]
        [Parameters("index")]
        Eq,

        /// <summary>
        /// Devuelve el primer item de una colección.
        /// </summary>
        [Description("First()")]
        First,

        /// <summary>
        /// Devuelve el último item de una colección.
        /// </summary>
        [Description("Last()")]
        Last,

        /// <summary>
        /// Devuelve los items de una colección, comenzando en el item correspondiente al índice especificado.
        /// </summary>
        [Description("Skip(count)")]
        [Parameters("count")]
        Skip,

        /// <summary>
        /// Devuelve la cantidad especificada de items de una colección,
        /// comenzando desde el primero.
        /// </summary>
        [Description("Take(count)")]
        [Parameters("count")]
        Take,

        /// <summary>
        /// Devuelve la cantidad especificada de items de una colección,
        /// comenzando con el item correspondiente al índice especificado.
        /// </summary>
        [Description("SkipTake(count, count)")]
        [Parameters("count", "count")]
        SkipTake,

        /// <summary>
        /// Devuelve el anterior nodo del mismo nivel en la jerarquia
        /// </summary>
        [Description("PrevSibling()")]
        PrevSibling,

        /// <summary>
        /// Devuelve el siguiente nodo del mismo nivel en la jerarquia
        /// </summary>
        [Description("NextSibling()")]
        NextSibling
    }


    public enum CaptureFunctions
    {
        /// <summary>
        /// Devuelve la propiedad 'innerText' con Trim() aplicado
        /// </summary>
        [Description("InnerText(trim)")]
        [Parameters("trim")]
        InnerText,

        /// <summary>
        /// En una colección de items devuelve la propiedad 'innerText'
        /// del item correspondiente al índice especificado.
        /// (Trim: si es 'true' o '1' aplica Trim() a la propiedad 'innerText'
        /// </summary>
        [Description("EqText(index, trim)")]
        [Parameters("index", "trim")]
        EqText,

        /// <summary>
        /// Devuelve el anterior nodo del mismo nivel en la jerarquia
        /// </summary>
        [Description("PrevSiblingText(trim)")]
        [Parameters("trim")]
        PrevSiblingText,

        /// <summary>
        /// Devuelve el siguiente nodo del mismo nivel en la jerarquia
        /// </summary>
        [Description("NextSiblingText(trim)")]
        [Parameters("trim")]
        NextSiblingText
    }


    public enum CaptureConditions
    {
        /// <summary>
        /// Interrumpe la captura si el último valor capturado no es string.Empty
        /// </summary>
        [Description("StopIfNotEmpty")]
        StopIfNotEmpty,

        /// <summary>
        /// Interrumpe la captura si el último valor es igual al valor del parámetro
        /// </summary>
        [Description("StopIfEqual(value)")]
        [Parameters("value")]
        StopIfEqual,

        /// <summary>
        /// Interrumpe la captura si el último valor es distinto al valor del parámetro
        /// </summary>
        [Description("StopIfNotEqual(value)")]
        [Parameters("value")]
        StopIfNotEqual
    }

    public class Function
    {
        private delegate List<HtmlNode> SelectorFunctionDelegate(HtmlNode node, string tag, string parameters);
        private delegate string CaptureFunctionDelegate(HtmlNode node, string parameters);
        private delegate string CaptureConditonDelegate(HtmlNode node, string parameters, ref int index, ref CapturedElement capturedElement);

        private static Dictionary<SelectorFunctions, SelectorFunctionDelegate> _dicSelectorFunctions = new Dictionary<SelectorFunctions, SelectorFunctionDelegate>
        {
            { SelectorFunctions.Eq, SelectorEq },
            { SelectorFunctions.First, SelectorFirst },
            { SelectorFunctions.Last, SelectorLast },
            { SelectorFunctions.Skip, SelectorSkip },
            { SelectorFunctions.Take, SelectorTake },
            { SelectorFunctions.SkipTake, SelectorSkipTake },
            { SelectorFunctions.PrevSibling, SelectorPrevSibling },
            { SelectorFunctions.NextSibling, SelectorNextSibling }
        };

        private static Dictionary<CaptureFunctions, CaptureFunctionDelegate> _dicCaptureFunctions = new Dictionary<CaptureFunctions, CaptureFunctionDelegate>
        {
            { CaptureFunctions.InnerText, CaptureInnerText },
            { CaptureFunctions.EqText, CaptureEqText },
            { CaptureFunctions.PrevSiblingText, CapturePrevSiblingText },
            { CaptureFunctions.NextSiblingText, CaptureNextSiblingText }
        };

        private static Dictionary<CaptureConditions, CaptureConditonDelegate> _dicCaptureConditions = new Dictionary<CaptureConditions, CaptureConditonDelegate>
        {
            { CaptureConditions.StopIfNotEmpty, CaptureStopIfNotEmpty },
            { CaptureConditions.StopIfEqual, CaptureStopIfEqual },
            { CaptureConditions.StopIfNotEqual, CaptureStopIfNotEqual }
        };

        public static bool HasParameter(SelectorFunctions function)
        {
            var fi = function.GetType().GetField(function.ToString());
            return !string.IsNullOrEmpty(Utilities.Tools.GetCustomAttribute<Parameters>(fi));
        }

        public static bool HasParameter(CaptureFunctions function)
        {
            var fi = function.GetType().GetField(function.ToString());
            return !string.IsNullOrEmpty(Utilities.Tools.GetCustomAttribute<Parameters>(fi));
        }

        public static bool HasParameter(CaptureConditions condition)
        {
            var fi = condition.GetType().GetField(condition.ToString());
            return !string.IsNullOrEmpty(Utilities.Tools.GetCustomAttribute<Parameters>(fi));
        }


        public static List<HtmlNode> Execute(SelectorFunctions func, HtmlNode node, string tag, string parameters)
        {
            if (_dicSelectorFunctions.ContainsKey(func))
                return _dicSelectorFunctions[func].Invoke(node, tag, parameters);
            else
                throw new ArgumentException(func.GetDescription() + ": Función no implementada");
        }

        public static string Execute(CaptureFunctions func, HtmlNode node, string parameters)
        {
            if (_dicCaptureFunctions.ContainsKey(func))
                return _dicCaptureFunctions[func].Invoke(node, parameters);
            else
                throw new ArgumentException(func.GetDescription() + ": Función no implementada");
        }

        public static string Execute(CaptureConditions func, HtmlNode node, string parameters, ref int index, ref CapturedElement capturedElement)
        {
            if (_dicCaptureConditions.ContainsKey(func))
                return _dicCaptureConditions[func].Invoke(node, parameters, ref index, ref capturedElement);
            else
                throw new ArgumentException(func.GetDescription() + ": Condición no implementada");
        }


        private static List<HtmlNode> SelectorEq(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var index = -1;

            if (!int.TryParse(v[0], out index))
                return result;

            if (index < 0)
                return result;

            var list = WebHelper
                .ElementsByTagFirstLevel(
                    tag,
                    (
                        node.ChildNodes != null  && node.ChildNodes.Count == 1
                            ? node.FirstChild
                            : node
                    )
                );

            if (list != null && list.Count > 0 && index <= list.Count - 1)
                result.Add(list[index]);

            return result;
        }

        private static List<HtmlNode> SelectorFirst(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();

            var list = WebHelper.ElementsByTag(tag, node);

            if (list != null && list.Count > 0)
                result.Add(list.FirstOrDefault());

            return result;
        }

        private static List<HtmlNode> SelectorLast(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();

            var list = WebHelper.ElementsByTag(tag, node);

            if (list != null && list.Count > 0)
                result.Add(list.LastOrDefault());

            return result;
        }

        private static List<HtmlNode> SelectorSkip(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var count = -1;

            if (!int.TryParse(v[0], out count))
                return result;

            var list = WebHelper.ElementsByTag(tag, node);

            if (list != null && list.Count > 0)
                result = list.Skip(count).ToList();

            return result;
        }

        private static List<HtmlNode> SelectorTake(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var count = -1;

            if (!int.TryParse(v[0], out count))
                return result;

            var list = WebHelper.ElementsByTag(tag, node);

            if (list != null && list.Count > 0)
                result = list.Take(count).ToList();

            return result;
        }

        private static List<HtmlNode> SelectorSkipTake(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var count1 = -1;
            var count2 = -1;

            if (v.Length < 2)
                return result;

            if (!int.TryParse(v[0], out count1))
                return result;

            if (!int.TryParse(v[1], out count2))
                return result;

            var list = WebHelper.ElementsByTag(tag, node);

            if (list != null && list.Count > 0)
                result = list.Skip(count1).Take(count2).ToList();

            return result;
        }

        private static List<HtmlNode> SelectorPrevSibling(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();

            if (node.FirstChild.PreviousSibling != null)
                result.Add(node.FirstChild.PreviousSibling);

            return result;
        }

        private static List<HtmlNode> SelectorNextSibling(HtmlNode node, string tag, string parameters)
        {
            var result = new List<HtmlNode>();

            if (node.FirstChild.NextSibling != null)
                result.Add(node.FirstChild.NextSibling);

            return result;
        }


        private static string CaptureInnerText(HtmlNode node, string parameters)
        {
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var trim = false;

            if (v.Length < 1)
                return null;

            if (!Tools.BoolTryParse(v[0], out trim))
                return string.Empty;

            var text = node.InnerText;

            if (trim)
                text = text.Trim();

            return text;
        }

        private static string CaptureEqText(HtmlNode node, string parameters)
        {
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var index = -1;
            var trim = false;

            if (v.Length < 2)
                return null;

            if (!int.TryParse(v[0], out index))
                return null;

            if (index < 0)
                return null;

            if (!Tools.BoolTryParse(v[1], out trim))
                return null;
            
            if (index > (node.ChildNodes.Count - 1))
                return null;

            var text = node.ChildNodes[index].InnerText;

            if (trim)
                text = text.Trim();

            return text;
        }

        private static string CapturePrevSiblingText(HtmlNode node, string parameters)
        {
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var trim = false;

            if (v.Length < 1)
                return null;

            if (!Tools.BoolTryParse(v[0], out trim))
                return string.Empty;

            if (node.PreviousSibling == null)
                return string.Empty;

            var text = node.PreviousSibling.InnerText;

            if (trim)
                text = text.Trim();

            return text;
        }

        private static string CaptureNextSiblingText(HtmlNode node, string parameters)
        {
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var trim = false;

            if (v.Length < 1)
                return null;

            if (!Tools.BoolTryParse(v[0], out trim))
                return string.Empty;

            if (node.NextSibling == null)
                return string.Empty;

            var text = node.NextSibling.InnerText;

            if (trim)
                text = text.Trim();

            return text;
        }

        private static string CaptureStopIfNotEmpty(HtmlNode node, string parameters, ref int index, ref CapturedElement capturedElement)
        {
            if ((index - 1) < 0)
                return null;

            var lastValue = capturedElement.Last();
            
            if (!string.IsNullOrEmpty(lastValue))
                index = capturedElement.Element.Capture.Count;

            return null;
        }

        private static string CaptureStopIfEqual(HtmlNode node, string parameters, ref int index, ref CapturedElement capturedElement)
        {
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var value = default(string);

            if (v.Length < 1)
                return null;

            value = v[0];

            if ((index - 1) < 0)
                return null;

            var lastValue = capturedElement.Last();

            if (lastValue.Equals(value))
                index = capturedElement.Element.Capture.Count;

            return null;
        }

        private static string CaptureStopIfNotEqual(HtmlNode node, string parameters, ref int index, ref CapturedElement capturedElement)
        {
            var v = parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var value = default(string);

            if (v.Length < 1)
                return null;

            value = v[0];

            if ((index - 1) < 0)
                return null;

            var lastValue = capturedElement.Last();

            if (!lastValue.Equals(value))
                index = capturedElement.Element.Capture.Count;

            return null;
        }
    }
}
