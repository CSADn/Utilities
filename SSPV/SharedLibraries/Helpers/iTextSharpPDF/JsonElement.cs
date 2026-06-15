using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

using iTextSharp.text;
using iTextSharp.text.pdf;

namespace iTextSharpPDF
{
    public class JsonElement
    {
        public ElementType Type { get; set; }

        public string Name { get; set; }

        public string Text { get; set; }

        public PointF? Position { get; set; }

        public Dimensions Dimensions { get; set; }

        public Align Align { get; set; }

        public VerticalAlign VerticalAlign { get; set; }

        public ITFont Font { get; set; }

        public Color? Color { get; set; }

        public Color? Fill { get; set; }

        public bool Bold { get; set; }

        public float LineSpacing { get; set; }

        public float SpacingBefore { get; set; }

        public float SpacingAfter { get; set; }

        public BarcodeLib.TYPE Encode { get; set; }

        public float Percent { get; set; }

        public int Columns { get; set; }

        public float Gutter { get; set; }

        public List<string> Paragraphs { get; set; }

        public Border Border { get; set; }

        public List<TableColumn> TableColumns { get; set; }

        public float TableHeaderHeight { get; set; }

        public float TableRowHeight { get; set; }

        public List<List<string>> TableRows { get; set; }

        public ImageScale ImageScale { get; set; }

        public byte[] Bytes { get; set; }

        public float? Width { get; set; }


        public JsonElement()
        {
            Type = ElementType.Text;
            Name = string.Empty;
            Text = string.Empty;
            Align = Align.Left;
            LineSpacing = 1.0f;
            SpacingBefore = 0.0f;
            SpacingAfter = 0.0f;
            Encode = BarcodeLib.TYPE.CODE39;
            Percent = 0f;
            Columns = 1;
            Gutter = 24.0f;
            TableHeaderHeight = 0f;
            TableRowHeight = 0f;
            ImageScale = ImageScale.Auto;
            VerticalAlign = VerticalAlign.Top;
            Bold = false;
            Width = null;
        }

        public JsonElement Clone()
        {
            return new JsonElement
            {
                Type = this.Type,
                Name = this.Name,
                Text = this.Text,
                Position = this.Position,
                Dimensions = this.Dimensions,
                Align = this.Align,
                VerticalAlign = this.VerticalAlign,
                Font = this.Font,
                Color = this.Color,
                Fill = this.Fill,
                LineSpacing = this.LineSpacing,
                SpacingBefore = this.SpacingBefore,
                SpacingAfter = this.SpacingAfter,
                Encode = this.Encode,
                Percent = this.Percent,
                Columns = this.Columns,
                Gutter = this.Gutter,
                Paragraphs = this.Paragraphs,
                Border = this.Border,
                TableColumns = this.TableColumns,
                TableHeaderHeight = this.TableHeaderHeight,
                TableRowHeight = this.TableRowHeight,
                TableRows = this.TableRows,
                Bytes = this.Bytes,
                Bold = this.Bold,
                Width = null,
                ImageScale = this.ImageScale
            };
        }
    }
}
