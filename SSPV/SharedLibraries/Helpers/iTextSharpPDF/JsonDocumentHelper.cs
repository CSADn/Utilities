using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using System.Web;

using iTextSharpPDF.Utilities;

namespace iTextSharpPDF
{
    public static class JsonDocumentHelper
    {
        private delegate void RenderElementHandler(PDFDocument pdf, JsonDocument document, JsonElement element);

        private static Dictionary<ElementType, RenderElementHandler> _renderers = new Dictionary<ElementType,RenderElementHandler>
        {
            { ElementType.Text, RenderText },
            { ElementType.Textblock, RenderTextblock },
            { ElementType.Paragraph, RenderParagraph },
            { ElementType.Barcode, RenderBarcode },
            { ElementType.TextColumns, RenderTextColumns },
            { ElementType.Table, RenderTable },
            { ElementType.Image, RenderImage },
            { ElementType.Rectangle, RenderRectangle }
        };

        public static PDFDocument Create(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var jsonDocument = ParseJson(json);

            return Create(jsonDocument);
        }

        public static PDFDocument Create(JsonDocument jsonDocument)
        {
            if (jsonDocument == null)
                return null;

            var pdf = (
                jsonDocument.PageSize == null
                    ? new PDFDocument(null, new MemoryStream())
                    : new PDFDocument(jsonDocument.PageSize.Rectangle, new MemoryStream())
            );

            if (jsonDocument.Header != null)
            {
                if (jsonDocument.Margins == null)
                    jsonDocument.Margins = new iTextSharp.text.Margins(15f, 15f, 15f + jsonDocument.Header.Height, 15f);
                else
                    jsonDocument.Margins.Top += jsonDocument.Header.Height;

                pdf.OnEndingPage += (sender) => RenderHeader(pdf, jsonDocument);
            }

            if (jsonDocument.Footer != null)
            {
                if (jsonDocument.Margins == null)
                    jsonDocument.Margins = new iTextSharp.text.Margins(15f, 15f, 15f, 15f + jsonDocument.Header.Height);
                else
                    jsonDocument.Margins.Bottom += jsonDocument.Footer.Height;

                pdf.OnEndingPage += (sender) => RenderFooter(pdf, jsonDocument);
            }

            if (jsonDocument.Margins != null)
                pdf.SetMargins(jsonDocument.Margins);

            if (jsonDocument.GlobalFont != null)
                pdf.DefaultFont = jsonDocument.GlobalFont;

            pdf.OnStartingPage += (sender) => MergeMarginsEvent(pdf, jsonDocument);
            pdf.OnEndingPage += (sender) => MergeEvent(pdf, jsonDocument);

            return pdf;
        }


        public static byte[] Process(string json, bool showRulers = false)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            var jsonDocument = ParseJson(json);

            return Process(jsonDocument, showRulers);
        }

        public static byte[] Process(JsonDocument jsonDocument, bool showRulers = false)
        {
            if (jsonDocument == null || jsonDocument.Disabled)
                return null;

            var pdf = Create(jsonDocument);

            if (jsonDocument.BlankPage)
            {
                pdf.Writer.PageEmpty = false;
                pdf.Document.NewPage();
            }
            else
                RenderElements(pdf, jsonDocument);

            if (showRulers)
                pdf.DrawRulers();

            return pdf.CloseAndGetBuffer();
        }


        public static JsonDocument ParseJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<JsonDocument>(json, new ITJsonConverter<JsonDocument>());
            }
            catch
            {
                NLog.LogManager.GetCurrentClassLogger().Info($"JSON: \r\n{json}\r\n");
                throw;
            }
        }

        public static void RenderElements(PDFDocument pdf, JsonDocument document)
        {
            RenderElements(pdf, document, document.Elements);
        }

        public static void RenderElements(PDFDocument pdf, JsonDocument document, List<JsonElement> elements)
        {
            if (pdf == null || document == null || elements == null || elements.Count == 0)
                return;

            foreach (var e in elements)
                if (_renderers.ContainsKey(e.Type))
                    _renderers[e.Type].Invoke(pdf, document, e);
        }

        public static void RenderNewPageElements(PDFDocument pdf, JsonDocument document)
        {
            if (pdf == null || document == null || document.NewPageElements == null || document.NewPageElements.Count == 0)
                return;

            foreach (var e in document.NewPageElements)
                if (_renderers.ContainsKey(e.Type))
                    _renderers[e.Type].Invoke(pdf, document, e);
        }


        private static void RenderText(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            var font = element.Font ?? document.GlobalFont;
            var color = element.Color ?? document.GlobalColor;
            var fill = element.Fill ?? document.GlobalFill;
            var position = element.Position ?? new System.Drawing.PointF(0f, 0f);

            pdf.Text(
                element.Text,
                position.X,
                position.Y,
                element.Align,
                font,
                color,
                element.Bold
            );
        }

        private static void RenderTextblock(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            var font = element.Font ?? document.GlobalFont;
            var color = element.Color ?? document.GlobalColor;
            var fill = element.Fill ?? document.GlobalFill;
            var position = element.Position ?? new System.Drawing.PointF(0f, 0f);

            pdf.Textblock(
                element.Text,
                position.X,
                position.Y,
                element.Align,
                font,
                element.LineSpacing,
                color,
                element.Bold
            );
        }

        private static void RenderParagraph(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            var font = element.Font ?? document.GlobalFont;
            var color = element.Color ?? document.GlobalColor;
            var fill = element.Fill ?? document.GlobalFill;
            var position = element.Position;
            var width = element.Width;

            pdf.Paragraph(
                element.Text,
                element.Align,
                font,
                element.LineSpacing,
                element.SpacingBefore,
                element.SpacingAfter,
                color,
                element.Bold,
                (position == null ? default(float?) : position.Value.X),
                (position == null ? default(float?) : position.Value.Y),
                width
            );
        }

        private static void RenderBarcode(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            if (element == null || string.IsNullOrWhiteSpace(element.Text))
                return;

            var position = element.Position ?? new System.Drawing.PointF(0f, 0f);

            using (var barcode = new BarcodeLib.Barcode())
            {
                barcode.IncludeLabel = false;

                var image = barcode.Encode(
                    element.Encode,
                    element.Text,
                    (int)Math.Truncate(element.Dimensions.Width),
                    (int)Math.Truncate(element.Dimensions.Height)
                );

                using (var ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    pdf.Image(ms.ToArray(), position.X, position.Y, 0, 0, null, null, null, element.Percent);
                }
            }

            pdf.Text(
                element.Text,
                position.X + ((element.Dimensions.Width * element.Percent / 100) / 2),
                position.Y + (element.Dimensions.Height * element.Percent / 100) + 4,
                Align.Center,
                ITFont.BPMono.Size(8f)
            );
        }

        private static void RenderTextColumns(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            var font = element.Font ?? document.GlobalFont;
            var color = element.Color ?? document.GlobalColor;

            pdf.TextColumns(
                element.Paragraphs,
                element.Align,
                font,
                element.Columns,
                element.Gutter,
                element.SpacingAfter,
                color,
                element.Bold
            );
        }

        private static void RenderTable(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            var font = element.Font ?? document.GlobalFont;
            var color = element.Color ?? document.GlobalColor;
            var position = element.Position ?? new System.Drawing.PointF(-1f, -1f);

            if (element.Border != null)
                element.Border.Color = element.Border.Color ?? document.GlobalColor;

            pdf.Table(
                element.TableColumns,
                element.TableRows,
                element.TableHeaderHeight,
                element.TableRowHeight,
                position.X,
                position.Y,
                element.Align,
                element.Border,
                font,
                color,
                element.Bold
            );
        }

        private static void RenderImage(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            if (element.Bytes == null || element.Bytes.Length == 0)
                return;

            if (element.Position == null)
                return;

            element.Dimensions = element.Dimensions ?? new Dimensions() { Width = 0f, Height = 0f };

            pdf.Image(
                element.Bytes,
                element.Position.Value.X,
                element.Position.Value.Y,
                element.Dimensions.Width,
                element.Dimensions.Height,
                element.ImageScale,
                element.VerticalAlign,
                element.Border,
                element.Percent
            );
        }

        private static void RenderRectangle(PDFDocument pdf, JsonDocument document, JsonElement element)
        {
            if (element.Position == null)
                return;

            var fill = element.Fill ?? document.GlobalFill;

            pdf.Rectangle(
                element.Position.Value.X,
                element.Position.Value.Y,
                element.Dimensions.Width,
                element.Dimensions.Height,
                element.Border,
                fill
            );
        }


        private static void MergeMarginsEvent(PDFDocument pdf, JsonDocument json)
        {
            if (!string.IsNullOrWhiteSpace(json.NewPageTemplate))
            {
                if (pdf.Writer.PageNumber > 1)
                {
                    pdf.SetMargins(json.NewPageMargins);
                    pdf.Document.NewPage();
                }
            }
        }

        private static void MergeEvent(PDFDocument pdf, JsonDocument json)
        {
            if (!string.IsNullOrWhiteSpace(json.MergeWith))
            {
                var mergePath = Utils.ResolvePath(json.MergeWith);

                if (File.Exists(mergePath))
                {
                    if (pdf.Writer.PageNumber == 1 || (json.MultiPage && string.IsNullOrWhiteSpace(json.NewPageTemplate)))
                        pdf.Merge(mergePath, 1, pdf.Writer.DirectContentUnder);
                }
            }

            if (!string.IsNullOrWhiteSpace(json.NewPageTemplate))
            {
                var newPageTemplate = Utils.ResolvePath(json.NewPageTemplate);

                if (File.Exists(newPageTemplate))
                {
                    if (pdf.Writer.PageNumber > 1)
                        pdf.Merge(newPageTemplate, 1, pdf.Writer.DirectContentUnder);
                }
            }
        }


        private static void RenderHeader(PDFDocument pdf, JsonDocument document)
        {
            if (!pdf.HeaderVisible)
                return;

            foreach (var e in document.Header.Elements)
                if (_renderers.ContainsKey(e.Type))
                {
                    var ce = e.Clone();
                    var position = ce.Position ?? new System.Drawing.PointF(0f, 0f);

                    ce.Position = new System.Drawing.PointF(
                        document.Margins.Left + position.X,
                        document.Margins.Top - document.Header.Height + position.Y
                    );

                    ce.Text = ce.Text.Replace("[@PageNumber]", pdf.Writer.CurrentPageNumber.ToString());

                    _renderers[e.Type].Invoke(pdf, document, ce);
                }
        }

        private static void RenderFooter(PDFDocument pdf, JsonDocument document)
        {
            if (!pdf.FooterVisible)
                return;

            foreach (var e in document.Footer.Elements)
                if (_renderers.ContainsKey(e.Type))
                {
                    var ce = e.Clone();
                    var position = ce.Position ?? new System.Drawing.PointF(0f, 0f);

                    ce.Position = new System.Drawing.PointF(
                        document.Margins.Left + position.X,
                        pdf.Document.PageSize.Height - document.Margins.Bottom + position.Y
                    );

                    ce.Text = ce.Text.Replace("[@PageNumber]", pdf.Writer.CurrentPageNumber.ToString());

                    _renderers[e.Type].Invoke(pdf, document, ce);
                }
        }
    }
}
